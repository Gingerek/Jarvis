local LrApplication = import 'LrApplication'
local LrApplicationView = import 'LrApplicationView'
local LrDevelopController = import 'LrDevelopController'
local LrFileUtils = import 'LrFileUtils'
local LrFunctionContext = import 'LrFunctionContext'
local LrPathUtils = import 'LrPathUtils'
local LrSocket = import 'LrSocket'
local LrTasks = import 'LrTasks'


local sender = nil
local senderConnected = false
local receiverConnected = false

local function bridgeDataPath()
    local home = LrPathUtils.getStandardFilePath('home')
    local localAppData = LrPathUtils.child(home, 'AppData')
    localAppData = LrPathUtils.child(localAppData, 'Local')
    local base = LrPathUtils.child(localAppData, 'Jarvis')
    base = LrPathUtils.child(base, 'lightroom')
    if not LrFileUtils.exists(base) then
        LrFileUtils.createAllDirectories(base)
    end
    return base
end
local function portPath(index)
    return LrPathUtils.child(bridgeDataPath(), 'bridge.port' .. tostring(index))
end

local function writePort(port, index)
    local file = io.open(portPath(index), 'w+')
    if file then
        file:write(tostring(port))
        file:close()
    end
end

local function sanitize(value)
    local text = tostring(value or '')
    text = string.gsub(text, '[\r\n]+', ' ')
    text = string.gsub(text, '|', '/')
    return text
end

local function split(message)
    local result = {}
    message = string.gsub(message or '', '[\r\n]+$', '') .. '|'
    for value in string.gmatch(message, '(.-)|') do
        table.insert(result, value)
    end
    return result
end
local function reply(id, status, payload)
    if not senderConnected or sender == nil then return end
    local line = sanitize(id) .. '|' .. sanitize(status) .. '|' .. sanitize(payload) .. '\n'
    LrTasks.startAsyncTaskWithoutErrorHandler(function()
        sender:send(line)
    end, 'JarvisLightroomReply')
end

local function ensureDevelop()
    if LrApplicationView.getCurrentModuleName() ~= 'develop' then
        LrApplicationView.switchToModule('develop')
        LrTasks.sleep(0.20)
    end
end

local function safeDevelopGet(param)
    ensureDevelop()
    local ok, value = pcall(function()
        return LrDevelopController.getValue(param)
    end)
    if not ok then return false, value end
    if value == nil then return false, 'parameter unavailable or no active photo' end
    return true, value
end
local function safeDevelopRange(param)
    ensureDevelop()
    local ok, minimum, maximum = pcall(function()
        return LrDevelopController.getRange(param)
    end)
    if not ok then return false, minimum end
    if minimum == nil or maximum == nil then return false, 'range unavailable' end
    return true, minimum, maximum
end

local function safeDevelopSet(param, rawValue)
    local number = tonumber(rawValue)
    if number == nil then return false, 'numeric value required' end
    local rangeOk, minimum, maximum = safeDevelopRange(param)
    if rangeOk then
        number = math.max(minimum, math.min(maximum, number))
    end
    local ok, err = pcall(function()
        LrDevelopController.setValue(param, number)
    end)
    if not ok then return false, err end
    local readOk, value = safeDevelopGet(param)
    return readOk, value
end
local function handleMessage(message)
    local parts = split(message)
    local id = parts[1] or '0'
    local command = parts[2] or ''
    local arg1 = parts[3] or ''
    local arg2 = parts[4] or ''

    if command == 'ping' then
        reply(id, 'ok', 'pong')
    elseif command == 'version' then
        reply(id, 'ok', LrApplication.versionString())
    elseif command == 'module' then
        reply(id, 'ok', LrApplicationView.getCurrentModuleName())
    elseif command == 'switch_module' then
        local ok, err = pcall(function()
            LrApplicationView.switchToModule(arg1)
        end)
        reply(id, ok and 'ok' or 'error', ok and arg1 or err)
    elseif command == 'develop_get' then
        local ok, value = safeDevelopGet(arg1)
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'develop_range' then
        local ok, minimum, maximum = safeDevelopRange(arg1)
        reply(id, ok and 'ok' or 'error', ok and (minimum .. ',' .. maximum) or minimum)
    elseif command == 'develop_set' then
        local ok, value = safeDevelopSet(arg1, arg2)
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'auto_tone' then
        ensureDevelop()
        local ok, err = pcall(function()
            LrDevelopController.setAutoTone()
        end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    elseif command == 'auto_wb' then
        ensureDevelop()
        local ok, err = pcall(function()
            LrDevelopController.setAutoWhiteBalance()
        end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    else
        reply(id, 'error', 'unknown command')
    end
end

local function createSender(context)
    sender = LrSocket.bind {
        functionContext = context,
        port = 0,
        mode = 'send',
        plugin = _PLUGIN,
        onConnecting = function(socket, port)
            writePort(port, 2)
        end,
        onConnected = function(socket, port)
            senderConnected = true
        end,
        onClosed = function(socket)
            senderConnected = false
        end,
        onError = function(socket, err)
            senderConnected = false
            socket:reconnect()
        end,
    }
end

local function createReceiver(context)
    return LrSocket.bind {
        functionContext = context,
        port = 0,
        mode = 'receive',
        plugin = _PLUGIN,
        onConnecting = function(socket, port)
            writePort(port, 1)
        end,
        onConnected = function(socket, port)
            receiverConnected = true
            if sender == nil then createSender(context) end
        end,
        onClosed = function(socket)
            receiverConnected = false
            if sender ~= nil then sender:close() end
            sender = nil
            senderConnected = false
            socket:reconnect()
        end,
        onError = function(socket, err)
            receiverConnected = false
            socket:reconnect()
        end,
        onMessage = function(socket, message)
            LrTasks.startAsyncTaskWithoutErrorHandler(function()
                handleMessage(message)
            end, 'JarvisLightroomMessage')
        end,
    }
end

LrTasks.startAsyncTask(function()
    LrFunctionContext.callWithContext('JarvisLightroomBridge', function(context)
        local receiver = createReceiver(context)
        while true do
            LrTasks.sleep(1.0)
        end
        receiver:close()
    end)
end)

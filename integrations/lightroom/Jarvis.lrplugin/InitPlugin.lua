local LrApplication = import 'LrApplication'
local LrApplicationView = import 'LrApplicationView'
local LrDevelopController = import 'LrDevelopController'
local LrFileUtils = import 'LrFileUtils'
local LrFunctionContext = import 'LrFunctionContext'
local LrPathUtils = import 'LrPathUtils'
local LrSelection = import 'LrSelection'
local LrSocket = import 'LrSocket'
local LrTasks = import 'LrTasks'
local LrUndo = import 'LrUndo'


local sender = nil
local copiedDevelopSettings = nil
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

local function ensureMasking()
    ensureDevelop()
    local ok, err = pcall(function() LrDevelopController.goToMasking() end)
    if not ok then return false, err end
    LrTasks.sleep(0.10)
    return true
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
    elseif command == 'active_photo_uuid' then
        local photo = LrApplication.activeCatalog():getTargetPhoto()
        if photo == nil then
            reply(id, 'error', 'no active photo')
        else
            reply(id, 'ok', photo:getRawMetadata('uuid'))
        end
    elseif command == 'switch_module' then
        local ok, err = pcall(function()
            LrApplicationView.switchToModule(arg1)
        end)
        reply(id, ok and 'ok' or 'error', ok and arg1 or err)
    elseif command == 'crop_get_angle' then
        local ok, value = safeDevelopGet('straightenAngle')
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'crop_set_angle' then
        local ok, value = safeDevelopSet('straightenAngle', arg1)
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'crop_reset' then
        ensureDevelop()
        local ok, err = pcall(function() LrDevelopController.resetCrop() end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    elseif command == 'mask_count' then
        ensureDevelop()
        local ok, masks = pcall(function() return LrDevelopController.getAllMasks() end)
        if not ok then reply(id, 'error', masks) else
            local count = 0
            if type(masks) == 'table' then for _ in pairs(masks) do count = count + 1 end end
            reply(id, 'ok', tostring(count))
        end
    elseif command == 'mask_create_ai' then
        ensureDevelop()
        local allowed = { subject=true, sky=true, background=true, objects=true, people=true, landscape=true }
        if not allowed[arg1] then reply(id, 'error', 'unsupported ai mask subtype') else
            local ok, err = pcall(function() LrDevelopController.createNewMask('aiSelection', arg1) end)
            reply(id, ok and 'ok' or 'error', ok and arg1 or err)
        end
    elseif command == 'mask_create_component' or command == 'mask_add_component' or command == 'mask_subtract_component' or command == 'mask_intersect_component' then
        local ready, readyErr = ensureMasking()
        if not ready then reply(id, 'error', readyErr); return end
        local subtype = arg2 ~= '' and arg2 or nil
        local ok, err = pcall(function()
            if command == 'mask_create_component' then LrDevelopController.createNewMask(arg1, subtype)
            elseif command == 'mask_add_component' then LrDevelopController.addToCurrentMask(arg1, subtype)
            elseif command == 'mask_subtract_component' then LrDevelopController.subtractFromCurrentMask(arg1, subtype)
            else LrDevelopController.intersectWithCurrentMask(arg1, subtype) end
        end)
        reply(id, ok and 'ok' or 'error', ok and (arg1 .. ':' .. tostring(subtype or '')) or err)
    elseif command == 'mask_overlay' then
        local ready, readyErr = ensureMasking()
        if not ready then reply(id, 'error', readyErr); return end
        local ok, err = pcall(function() LrDevelopController.toggleOverlay() end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    elseif command == 'mask_reset' then
        local ready, readyErr = ensureMasking()
        if not ready then reply(id, 'error', readyErr); return end
        local ok, err = pcall(function() LrDevelopController.resetMasking() end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    elseif command == 'develop_get_tool' then
        ensureDevelop(); local ok, value = pcall(function() return LrDevelopController.getSelectedTool() end); reply(id, ok and 'ok' or 'error', value)
    elseif command == 'develop_select_tool' then
        ensureDevelop(); local allowed={loupe=true,crop=true,dust=true,redeye=true,masking=true,upright=true,point_color=true,local_point_color=true,depth_refinement=true}
        if not allowed[arg1] then reply(id,'error','unsupported develop tool') else local ok, value = pcall(function() return LrDevelopController.selectTool(arg1) end); reply(id, ok and 'ok' or 'error', ok and arg1 or value) end
    elseif command == 'grading_get_view' then
        ensureDevelop(); local ok, value = pcall(function() return LrDevelopController.getActiveColorGradingView() end); reply(id, ok and 'ok' or 'error', value)
    elseif command == 'grading_set_view' then
        ensureDevelop(); local allowed={['3-way']=true,shadow=true,midtone=true,highlight=true,global=true}; if not allowed[arg1] then reply(id,'error','unsupported grading view') else local ok,err=pcall(function() LrDevelopController.setActiveColorGradingView(arg1) end); reply(id,ok and 'ok' or 'error',ok and arg1 or err) end
    elseif command == 'lensblur_get_bokeh' then
        ensureDevelop(); local ok,value=pcall(function() return LrDevelopController.getSelectedLensBlurBokeh() end); reply(id,ok and 'ok' or 'error',value)
    elseif command == 'lensblur_set_bokeh' then
        ensureDevelop(); local allowed={Circle=true,SoapBubble=true,Blade=true,Ring=true,Anamorphic=true}; if not allowed[arg1] then reply(id,'error','unsupported bokeh') else local ok,err=pcall(function() LrDevelopController.setLensBlurBokeh(arg1) end); reply(id,ok and 'ok' or 'error',ok and arg1 or err) end
    elseif command == 'remove_open' then
        ensureDevelop(); local allowed={heal_patchmatch=true,heal=true,clone=true}; if not allowed[arg1] then reply(id,'error','unsupported remove type') else local ok,err=pcall(function() LrDevelopController.goToRemove(arg1) end); reply(id,ok and 'ok' or 'error',ok and arg1 or err) end
    elseif command == 'remove_reset' then
        ensureDevelop(); local ok,err=pcall(function() LrDevelopController.resetHealing() end); reply(id,ok and 'ok' or 'error',ok and 'done' or err)
    elseif command == 'develop_get' then
        local ok, value = safeDevelopGet(arg1)
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'develop_range' then
        local ok, minimum, maximum = safeDevelopRange(arg1)
        reply(id, ok and 'ok' or 'error', ok and (minimum .. ',' .. maximum) or minimum)
    elseif command == 'develop_set' then
        local ok, value = safeDevelopSet(arg1, arg2)
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'develop_reset_param' then
        ensureDevelop()
        local ok, err = pcall(function() LrDevelopController.resetToDefault(arg1) end)
        if not ok then reply(id, 'error', err) else local readOk, value = safeDevelopGet(arg1); reply(id, readOk and 'ok' or 'error', value) end
    elseif command == 'develop_reset_all' then
        ensureDevelop()
        local ok, err = pcall(function() LrDevelopController.resetAllDevelopAdjustments() end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    elseif command == 'develop_reset_transforms' then
        ensureDevelop()
        local ok, err = pcall(function() LrDevelopController.resetTransforms() end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
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
    elseif command == 'selection_next' then
        local ok, err = pcall(function() LrSelection.nextPhoto() end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    elseif command == 'selection_previous' then
        local ok, err = pcall(function() LrSelection.previousPhoto() end)
        reply(id, ok and 'ok' or 'error', ok and 'done' or err)
    elseif command == 'rating_get' then
        local ok, value = pcall(function() return LrSelection.getRating() end)
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'rating_set' then
        local rating = tonumber(arg1)
        if rating == nil or rating < 0 or rating > 5 then
            reply(id, 'error', 'rating must be 0..5')
        else
            local ok, err = pcall(function() LrSelection.setRating(rating) end)
            reply(id, ok and 'ok' or 'error', ok and tostring(rating) or err)
        end
    elseif command == 'rating_up' then
        local ok, err = pcall(function() LrSelection.increaseRating() end)
        if ok then LrTasks.sleep(0.10) end
        local readOk, value = pcall(function() return LrSelection.getRating() end)
        reply(id, ok and readOk and 'ok' or 'error', ok and readOk and value or err)
    elseif command == 'rating_down' then
        local ok, err = pcall(function() LrSelection.decreaseRating() end)
        if ok then LrTasks.sleep(0.10) end
        local readOk, value = pcall(function() return LrSelection.getRating() end)
        reply(id, ok and readOk and 'ok' or 'error', ok and readOk and value or err)
    elseif command == 'flag_get' then
        local ok, value = pcall(function() return LrSelection.getFlag() end)
        reply(id, ok and 'ok' or 'error', value)
    elseif command == 'flag_pick' then
        local ok, err = pcall(function() LrSelection.flagAsPick() end)
        reply(id, ok and 'ok' or 'error', ok and '1' or err)
    elseif command == 'flag_reject' then
        local ok, err = pcall(function() LrSelection.flagAsReject() end)
        reply(id, ok and 'ok' or 'error', ok and '-1' or err)
    elseif command == 'flag_clear' then
        local ok, err = pcall(function() LrSelection.removeFlag() end)
        reply(id, ok and 'ok' or 'error', ok and '0' or err)
    elseif command == 'develop_copy_settings' then
        local photo = LrApplication.activeCatalog():getTargetPhoto()
        if photo == nil then reply(id, 'error', 'no active photo') else
            local ok, value = LrTasks.pcall(function() return photo:getDevelopSettings() end)
            if ok and value ~= nil then copiedDevelopSettings = value end
            reply(id, ok and value ~= nil and 'ok' or 'error', ok and value ~= nil and 'copied' or tostring(value))
        end
    elseif command == 'develop_paste_settings' then
        local catalog = LrApplication.activeCatalog(); local photo = catalog:getTargetPhoto()
        if copiedDevelopSettings == nil then reply(id, 'error', 'no copied settings')
        elseif photo == nil then reply(id, 'error', 'no active photo') else
            local ok, err = LrTasks.pcall(function()
                catalog:withWriteAccessDo('Jarvis: Paste Develop Settings', function() photo:applyDevelopSettings(copiedDevelopSettings) end)
            end)
            if ok then LrTasks.sleep(0.20) end
            reply(id, ok and 'ok' or 'error', ok and 'pasted' or tostring(err))
        end
    elseif command == 'undo' then
        if not LrUndo.canUndo() then reply(id, 'error', 'nothing to undo') else
            local ok, err = LrTasks.pcall(function() LrUndo.undo() end); if ok then LrTasks.sleep(0.15) end
            reply(id, ok and 'ok' or 'error', ok and 'done' or tostring(err))
        end
    elseif command == 'redo' then
        if not LrUndo.canRedo() then reply(id, 'error', 'nothing to redo') else
            local ok, err = LrTasks.pcall(function() LrUndo.redo() end); if ok then LrTasks.sleep(0.15) end
            reply(id, ok and 'ok' or 'error', ok and 'done' or tostring(err))
        end
    else
        reply(id, 'error', 'unknown command')
    end
end

local function runSocketSession(context)
    local running = true
    local receiver = nil
    local sessionSender = nil

    local function stopSession()
        running = false
    end

    local function createSender()
        sessionSender = LrSocket.bind {
            functionContext = context,
            port = 0,
            mode = 'send',
            plugin = _PLUGIN,
            onConnecting = function(socket, port)
                writePort(port, 2)
            end,            onConnected = function(socket, port)
                senderConnected = true
            end,
            onClosed = function(socket)
                senderConnected = false
                stopSession()
            end,
            onError = function(socket, err)
                senderConnected = false
                if tostring(err) == 'timeout' and running then
                    socket:reconnect()
                else
                    stopSession()
                end
            end,
        }
        sender = sessionSender
    end

    receiver = LrSocket.bind {
        functionContext = context,
        port = 0,
        mode = 'receive',
        plugin = _PLUGIN,
        onConnecting = function(socket, port)
            writePort(port, 1)
        end,        onConnected = function(socket, port)
            receiverConnected = true
            if sender == nil then createSender() end
        end,
        onClosed = function(socket)
            receiverConnected = false
            stopSession()
        end,
        onError = function(socket, err)
            receiverConnected = false
            if tostring(err) == 'timeout' and running then
                socket:reconnect()
            else
                stopSession()
            end
        end,
        onMessage = function(socket, message)
            LrTasks.startAsyncTaskWithoutErrorHandler(function()
                handleMessage(message)
            end, 'JarvisLightroomMessage')
        end,
    }

    while running do LrTasks.sleep(0.10) end
    receiverConnected = false
    senderConnected = false
    local oldSender = sender
    sender = nil
    if oldSender ~= nil then pcall(function() oldSender:close() end) end
    if receiver ~= nil then pcall(function() receiver:close() end) end
end

LrTasks.startAsyncTask(function()
    while true do
        LrFunctionContext.callWithContext('JarvisLightroomBridgeSession', function(context)
            runSocketSession(context)
        end)
        LrTasks.sleep(0.10)
    end
end)

local LrFileUtils = import 'LrFileUtils'
local LrPathUtils = import 'LrPathUtils'

local home = LrPathUtils.getStandardFilePath('home')
local base = LrPathUtils.child(home, 'AppData')
base = LrPathUtils.child(base, 'Local')
base = LrPathUtils.child(base, 'Jarvis')
base = LrPathUtils.child(base, 'lightroom')

for _, name in ipairs({ 'bridge.port1', 'bridge.port2' }) do
    local path = LrPathUtils.child(base, name)
    if LrFileUtils.exists(path) then
        pcall(function() LrFileUtils.delete(path) end)
    end
end

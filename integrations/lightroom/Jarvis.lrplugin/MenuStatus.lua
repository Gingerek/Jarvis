local LrDialogs = import 'LrDialogs'
local LrFileUtils = import 'LrFileUtils'
local LrPathUtils = import 'LrPathUtils'

local home = LrPathUtils.getStandardFilePath('home')
local base = LrPathUtils.child(home, 'AppData')
base = LrPathUtils.child(base, 'Local')
base = LrPathUtils.child(base, 'Jarvis')
base = LrPathUtils.child(base, 'lightroom')

local port1 = LrPathUtils.child(base, 'bridge.port1')
local port2 = LrPathUtils.child(base, 'bridge.port2')
local running = LrFileUtils.exists(port1) and LrFileUtils.exists(port2)

LrDialogs.message(
    'Jarvis Lightroom Bridge',
    running and 'Bridge is active.' or 'Bridge is not active yet.',
    'info')

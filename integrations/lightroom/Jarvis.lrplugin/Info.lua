return {
    LrSdkVersion = 6.0,
    LrSdkMinimumVersion = 6.0,
    LrToolkitIdentifier = 'com.gingerek.jarvis.lightroom',
    LrPluginName = 'Jarvis Lightroom Bridge',
    LrInitPlugin = 'InitPlugin.lua',
    LrShutdownPlugin = 'ShutdownPlugin.lua',
    LrForceInitPlugin = true,
    LrHelpMenuItems = {
        { title = 'Jarvis Lightroom Bridge Status', file = 'MenuStatus.lua' },
    },
    VERSION = { major = 0, minor = 1, revision = 0, build = 3 }
}

const HOST_NAME = "com.jarvis.browser";
let nativePort = null;
let reconnectTimer = null;

function scheduleReconnect() {
  if (reconnectTimer) return;
  reconnectTimer = setTimeout(() => {
    reconnectTimer = null;
    connectNative();
  }, 1000);
}

function connectNative() {
  if (nativePort) return;
  try {
    const port = chrome.runtime.connectNative(HOST_NAME);
    nativePort = port;
    port.onMessage.addListener(handleNativeMessage);
    port.onDisconnect.addListener(() => {
      nativePort = null;
      scheduleReconnect();
    });
    port.postMessage({ kind: "hello", version: "0.1.0" });
  } catch {
    nativePort = null;
    scheduleReconnect();
  }
}
async function currentTab() {
  const [tab] = await chrome.tabs.query({ active: true, lastFocusedWindow: true });
  if (!tab?.id) throw new Error("No active tab");
  return tab;
}

async function switchTab(delta) {
  const active = await currentTab();
  const tabs = await chrome.tabs.query({ windowId: active.windowId });
  if (!tabs.length) throw new Error("No tabs in current window");
  const nextIndex = (active.index + delta + tabs.length) % tabs.length;
  const target = tabs.find(t => t.index === nextIndex);
  if (!target?.id) throw new Error("Target tab not found");
  await chrome.tabs.update(target.id, { active: true });
  return { tabId: target.id, index: target.index };
}

async function executeCommand(action, argument) {
  switch (action) {
    case "ping": return { ok: true };
    case "new_tab": {
      const tab = await chrome.tabs.create(argument ? { url: argument } : {});
      return { tabId: tab.id };
    }
    case "close_tab": {
      const tab = await currentTab();
      await chrome.tabs.remove(tab.id);
      return { tabId: tab.id };
    }
    case "next_tab": return await switchTab(1);
    case "previous_tab": return await switchTab(-1);
    case "reload": {
      const tab = await currentTab();
      await chrome.tabs.reload(tab.id);
      return { tabId: tab.id };
    }
    case "back": {
      const tab = await currentTab();
      await chrome.tabs.goBack(tab.id);
      return { tabId: tab.id };
    }
    case "forward": {
      const tab = await currentTab();
      await chrome.tabs.goForward(tab.id);
      return { tabId: tab.id };
    }
    case "duplicate_tab": {
      const tab = await currentTab();
      const duplicate = await chrome.tabs.duplicate(tab.id);
      return { tabId: duplicate?.id };
    }
    case "mute_tab": {
      const tab = await currentTab();
      await chrome.tabs.update(tab.id, { muted: true });
      return { tabId: tab.id };
    }
    case "unmute_tab": {
      const tab = await currentTab();
      await chrome.tabs.update(tab.id, { muted: false });
      return { tabId: tab.id };
    }
    case "navigate": {
      const tab = await currentTab();
      await chrome.tabs.update(tab.id, { url: argument });
      return { tabId: tab.id };
    }
    case "scroll_down": {
      const tab = await currentTab();
      const [r] = await chrome.scripting.executeScript({ target: { tabId: tab.id }, func: () => { window.scrollBy({ top: Math.max(600, window.innerHeight * 0.8), behavior: "smooth" }); return { y: window.scrollY }; } });
      return r?.result ?? {};
    }
    case "scroll_up": {
      const tab = await currentTab();
      const [r] = await chrome.scripting.executeScript({ target: { tabId: tab.id }, func: () => { window.scrollBy({ top: -Math.max(600, window.innerHeight * 0.8), behavior: "smooth" }); return { y: window.scrollY }; } });
      return r?.result ?? {};
    }
    case "scroll_top": {
      const tab = await currentTab();
      const [r] = await chrome.scripting.executeScript({ target: { tabId: tab.id }, func: () => { window.scrollTo({ top: 0, behavior: "smooth" }); return { y: window.scrollY }; } });
      return r?.result ?? {};
    }
    case "scroll_bottom": {
      const tab = await currentTab();
      const [r] = await chrome.scripting.executeScript({ target: { tabId: tab.id }, func: () => { window.scrollTo({ top: document.documentElement.scrollHeight, behavior: "smooth" }); return { y: window.scrollY }; } });
      return r?.result ?? {};
    }
    case "toggle_media": {
      const tab = await currentTab();
      const [r] = await chrome.scripting.executeScript({ target: { tabId: tab.id }, func: async () => { const list = [...document.querySelectorAll("video,audio")]; const media = list.find(x => !x.paused) ?? list[0]; if (!media) return { found: false }; if (media.paused) { await media.play(); return { found: true, paused: false }; } media.pause(); return { found: true, paused: true }; } });
      return r?.result ?? { found: false };
    }    case "context": {
      const tab = await currentTab();
      return {
        tabId: tab.id,
        windowId: tab.windowId,
        index: tab.index,
        title: tab.title ?? null,
        url: tab.url ?? null,
        audible: tab.audible ?? false,
        muted: tab.mutedInfo?.muted ?? false
      };
    }
    default:
      throw new Error(`Unknown browser action: ${action}`);
  }
}

async function handleNativeMessage(message) {
  if (message?.kind !== "command" || !message.id) return;
  try {
    const data = await executeCommand(message.action, message.argument);
    nativePort?.postMessage({ kind: "response", id: message.id, ok: true, data });
  } catch (error) {
    nativePort?.postMessage({
      kind: "response",
      id: message.id,
      ok: false,
      error: String(error?.message ?? error)
    });
  }
}

chrome.runtime.onInstalled.addListener(connectNative);
chrome.runtime.onStartup.addListener(connectNative);
connectNative();

mergeInto(LibraryManager.library, {
  PushUslugiSetOrientationMode: function(modePtr) {
    var mode = UTF8ToString(modePtr);

    if (typeof window === "undefined") {
      return;
    }

    window.__pushUslugiOrientationMode = mode;

    if (
      window.PushUslugiViewport &&
      typeof window.PushUslugiViewport.setOrientationMode === "function"
    ) {
      window.PushUslugiViewport.setOrientationMode(mode);
      return;
    }

    window.dispatchEvent(new CustomEvent("pushuslugi-orientation-mode", {
      detail: { mode: mode }
    }));
  }
});

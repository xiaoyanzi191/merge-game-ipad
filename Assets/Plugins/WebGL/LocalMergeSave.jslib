mergeInto(LibraryManager.library, {
  LocalMergeReadSave: function(keyPointer) {
    try {
      var value = localStorage.getItem('merge-sandbox/v1/' + UTF8ToString(keyPointer));
      if (value === null) return 0;
      var size = lengthBytesUTF8(value) + 1;
      var result = _malloc(size);
      stringToUTF8(value, result, size);
      return result;
    } catch (error) {
      window.dispatchEvent(new CustomEvent('merge-save-status', {detail: {ok: false}}));
      return 0;
    }
  },
  LocalMergeWriteSave: function(keyPointer, valuePointer) {
    try {
      localStorage.setItem('merge-sandbox/v1/' + UTF8ToString(keyPointer), UTF8ToString(valuePointer));
      window.dispatchEvent(new CustomEvent('merge-save-status', {detail: {ok: true}}));
    } catch (error) {
      window.dispatchEvent(new CustomEvent('merge-save-status', {detail: {ok: false}}));
    }
    // Unity 2022.3.62f3 autoSyncPersistentDataPath owns the secondary IDBFS flush.
  }
});

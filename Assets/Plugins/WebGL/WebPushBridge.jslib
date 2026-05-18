mergeInto(LibraryManager.library, {
  RegisterWebPushFromUnity: function (publicKeyPtr, jwtPtr) {
    const publicKey = UTF8ToString(publicKeyPtr);
    const jwt = UTF8ToString(jwtPtr);

    window.RegisterWebPush(publicKey, jwt)
      .then(result => {
        console.log("WebPush register result:", result);
      })
      .catch(error => {
        console.error("WebPush register error:", error);
      });
  }
});
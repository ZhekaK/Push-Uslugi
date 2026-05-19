var PushUslugiWebPushBridge = {
  $PushUslugiWebPushBridge: {
    subscribePath: "/api/push/subscribe",
    serviceWorkerFileName: "service-worker.js",

    sendUnityMessage: function (gameObjectName, methodName, value) {
      if (!gameObjectName || !methodName || typeof SendMessage !== "function") {
        return;
      }

      SendMessage(gameObjectName, methodName, String(value));
    },

    urlBase64ToUint8Array: function (base64String) {
      const padding = "=".repeat((4 - base64String.length % 4) % 4);
      const base64 = (base64String + padding)
        .replace(/-/g, "+")
        .replace(/_/g, "/");
      const rawData = window.atob(base64);
      const outputArray = new Uint8Array(rawData.length);

      for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
      }

      return outputArray;
    },

    subscriptionToServerPayload: function (subscription) {
      const json = subscription.toJSON();
      const keys = json.keys || {};

      return {
        Endpoint: subscription.endpoint,
        P256dh: keys.p256dh || "",
        Auth: keys.auth || "",
        Platform: "WebGL"
      };
    },

    buildApiUrl: function (apiBaseUrl, path) {
      if (!apiBaseUrl) {
        return path;
      }

      return apiBaseUrl.replace(/\/+$/, "") + path;
    },

    getServiceWorkerUrl: function () {
      return new URL(PushUslugiWebPushBridge.serviceWorkerFileName, window.location.href).toString();
    },

    getRegistrationForStatus: async function () {
      if (!("serviceWorker" in navigator) || !("PushManager" in window)) {
        return null;
      }

      return await navigator.serviceWorker.getRegistration();
    },

    getOrCreateRegistration: async function () {
      if (!("serviceWorker" in navigator) || !("PushManager" in window)) {
        throw new Error("Web Push is not supported in this browser.");
      }

      let registration = await navigator.serviceWorker.getRegistration();

      if (!registration) {
        registration = await navigator.serviceWorker.register(PushUslugiWebPushBridge.getServiceWorkerUrl());
      }

      return registration;
    },

    getSubscription: async function () {
      const registration = await PushUslugiWebPushBridge.getRegistrationForStatus();

      if (!registration) {
        return null;
      }

      return await registration.pushManager.getSubscription();
    },

    register: async function (publicKey, jwt, apiBaseUrl) {
      if (!("Notification" in window)) {
        throw new Error("Notifications are not supported in this browser.");
      }

      const permission = await Notification.requestPermission();

      if (permission !== "granted") {
        throw new Error("Notification permission was not granted.");
      }

      const registration = await PushUslugiWebPushBridge.getOrCreateRegistration();
      let subscription = await registration.pushManager.getSubscription();

      if (!subscription) {
        subscription = await registration.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey: PushUslugiWebPushBridge.urlBase64ToUint8Array(publicKey)
        });
      }

      const response = await fetch(
        PushUslugiWebPushBridge.buildApiUrl(apiBaseUrl, PushUslugiWebPushBridge.subscribePath),
        {
        method: "POST",
        headers: {
          "Authorization": "Bearer " + jwt,
          "Content-Type": "application/json"
        },
        body: JSON.stringify(PushUslugiWebPushBridge.subscriptionToServerPayload(subscription))
        });

      if (!response.ok) {
        const responseBody = await response.text();
        throw new Error("Push subscribe failed: " + response.status + " " + responseBody);
      }

      return subscription;
    }
  },

  RegisterWebPushFromUnity: function (publicKeyPtr, jwtPtr, apiBaseUrlPtr, gameObjectNamePtr, stateCallbackMethodNamePtr) {
    const publicKey = UTF8ToString(publicKeyPtr);
    const jwt = UTF8ToString(jwtPtr);
    const apiBaseUrl = apiBaseUrlPtr ? UTF8ToString(apiBaseUrlPtr) : "";
    const gameObjectName = gameObjectNamePtr ? UTF8ToString(gameObjectNamePtr) : "";
    const stateCallbackMethodName = stateCallbackMethodNamePtr ? UTF8ToString(stateCallbackMethodNamePtr) : "";

    PushUslugiWebPushBridge.register(publicKey, jwt, apiBaseUrl)
      .then(subscription => {
        console.log("WebPush register result:", subscription.endpoint);
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, "1");
      })
      .catch(error => {
        console.error("WebPush register error:", error);
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, "0");
      });
  },

  GetWebPushSubscriptionStateFromUnity: function (gameObjectNamePtr, stateCallbackMethodNamePtr) {
    const gameObjectName = gameObjectNamePtr ? UTF8ToString(gameObjectNamePtr) : "";
    const stateCallbackMethodName = stateCallbackMethodNamePtr ? UTF8ToString(stateCallbackMethodNamePtr) : "";

    PushUslugiWebPushBridge.getSubscription()
      .then(subscription => {
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, subscription ? "1" : "0");
      })
      .catch(error => {
        console.error("WebPush state error:", error);
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, "0");
      });
  },

  GetWebPushSubscriptionEndpointFromUnity: function (gameObjectNamePtr, endpointCallbackMethodNamePtr, stateCallbackMethodNamePtr) {
    const gameObjectName = gameObjectNamePtr ? UTF8ToString(gameObjectNamePtr) : "";
    const endpointCallbackMethodName = endpointCallbackMethodNamePtr ? UTF8ToString(endpointCallbackMethodNamePtr) : "";
    const stateCallbackMethodName = stateCallbackMethodNamePtr ? UTF8ToString(stateCallbackMethodNamePtr) : "";

    PushUslugiWebPushBridge.getSubscription()
      .then(subscription => {
        const endpoint = subscription ? subscription.endpoint : "";
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, endpointCallbackMethodName, endpoint);
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, subscription ? "1" : "0");
      })
      .catch(error => {
        console.error("WebPush endpoint error:", error);
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, endpointCallbackMethodName, "");
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, "0");
      });
  },

  UnsubscribeWebPushLocallyFromUnity: function (gameObjectNamePtr, stateCallbackMethodNamePtr) {
    const gameObjectName = gameObjectNamePtr ? UTF8ToString(gameObjectNamePtr) : "";
    const stateCallbackMethodName = stateCallbackMethodNamePtr ? UTF8ToString(stateCallbackMethodNamePtr) : "";

    PushUslugiWebPushBridge.getSubscription()
      .then(subscription => {
        if (!subscription) {
          PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, "0");
          return true;
        }

        return subscription.unsubscribe().then(() => {
          PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, "0");
          return true;
        });
      })
      .catch(error => {
        console.error("WebPush local unsubscribe error:", error);
        PushUslugiWebPushBridge.sendUnityMessage(gameObjectName, stateCallbackMethodName, "1");
      });
  }
};

autoAddDeps(PushUslugiWebPushBridge, "$PushUslugiWebPushBridge");
mergeInto(LibraryManager.library, PushUslugiWebPushBridge);

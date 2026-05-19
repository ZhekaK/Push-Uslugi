self.addEventListener("install", event => {
  self.skipWaiting();
});

self.addEventListener("activate", event => {
  event.waitUntil(self.clients.claim());
});

self.addEventListener("push", event => {
  const defaultUrl = new URL(".", self.location.href).toString();
  const defaultIcon = new URL("favicon.png", self.location.href).toString();
  let data = {
    title: "\u041f\u044b\u0448-\u0423\u0441\u043b\u0443\u0433\u0438",
    body: "\u0423 \u0432\u0430\u0441 \u043d\u043e\u0432\u043e\u0435 \u0443\u0432\u0435\u0434\u043e\u043c\u043b\u0435\u043d\u0438\u0435",
    url: defaultUrl,
    icon: defaultIcon
  };

  if (event.data) {
    try {
      data = Object.assign(data, event.data.json());
    } catch {
      data.body = event.data.text();
    }
  }

  event.waitUntil(
    self.registration.showNotification(data.title, {
      body: data.body,
      icon: data.icon || defaultIcon,
      badge: data.icon || defaultIcon,
      data: {
        url: data.url || defaultUrl
      }
    })
  );
});

self.addEventListener("notificationclick", event => {
  event.notification.close();

  const targetUrl = event.notification.data?.url || new URL(".", self.location.href).toString();

  event.waitUntil(
    self.clients.matchAll({ type: "window", includeUncontrolled: true })
      .then(clients => {
        for (const client of clients) {
          if (client.url.includes(self.location.origin) && "focus" in client) {
            return client.focus();
          }
        }

        if (self.clients.openWindow) {
          return self.clients.openWindow(targetUrl);
        }
      })
  );
});

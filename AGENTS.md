# Контекст проекта Push-Uslugi

Этот файл предназначен для агентов, работающих с репозиторием из новых чатов. Он описывает состояние **клиентского Unity-проекта**, проверенное по исходникам и настройкам репозитория. Перед изменением конкретной функции всё равно читайте соответствующие скрипты и сериализованные ссылки в сцене: детали могли измениться.

## Что это за проект

- `Push-Uslugi` — приложение с главным меню и набором сервисов, мини-игр и модулей, преимущественно с русским интерфейсом. Код на C#, сцены и UI — Unity.
- Версия редактора: **Unity 6000.3.15f1** (`ProjectSettings/ProjectVersion.txt`). В настройках продукта указаны `Push-Uslugi`, `Kartofan Studio`, версия приложения `1.1.0`.
- Используются Universal Render Pipeline 2D, uGUI (`UnityEngine.UI`, включая legacy `Text` и `InputField`), Input System. Зависимости пакетов закреплены в `Packages/manifest.json` и `Packages/packages-lock.json`.
- В `Assets/Plugins/` находятся сторонние исходники Zenject и DOTween, а также собственные WebGL-мосты. Xoxi вызывает DOTween. Не редактируйте сторонние плагины для исправлений логики приложения без причины.
- Репозиторий содержит **Unity-клиент**, но не исходники серверного API. Для работы сетевых экранов нужен доступный сервер, соответствующий используемым маршрутам.

## Старт и навигация

Список сцен и порядок сборки определяет `ProjectSettings/EditorBuildSettings.asset`:

1. `Assets/_MainApp/Scenes/_StartupScene.unity` — `StartupScreen` проверяет сохранённый токен через `/api/user/profile` и открывает `MainMenuScene` либо `LoginScene`.
2. `Assets/_MainApp/Scenes/LoginScene.unity` — вход гостем или по серии и номеру ключа.
3. `Assets/_MainApp/Scenes/MainMenuScene.unity` — профиль, выход, Web Push, индикаторы непрочитанных уведомлений и переходы в модули.
4. `Assets/_Xoxi/MenuXoxi.unity`, `Assets/_Xoxi/GameXoxi.unity` — меню и игра «крестики-нолики».
5. `Assets/_Alcometr/Scenes/Alcometr.unity` — алкометр.
6. `Assets/_GovCalculator/Scenes/GovCalculator.unity` — калькулятор деления счёта с льготой для одного человека.
7. `Assets/_CalendarEvents/CalendarScene.unity` — календарь событий.
8. `Assets/_RewardModule/Scenes/RewardScene.unity` — награды и чемпионаты.
9. `Assets/_CinemaModule/Scenes/CinemaScene.unity` — каталог фильмов.
10. `Assets/_VoteModule/Scenes/VoteScene.unity` — голосования.
11. `Assets/_Zong/Scenes/ZongScene.unity` — сетевая игра «Зонг».
12. `Assets/_Durak/Scenes/DurakScene.unity` — сетевая карточная игра «Дурак».
13. `Assets/_Messenger/Scenes/MessengerScene.unity` — общий, личные и групповые чаты пользователей с ID; Canvas, панели, кнопки, поля и шаблоны строк сохранены в сцене и редактируются в Unity. `MessengerScreen` при запуске заполняет списки данными.

`MainMenuScreen.ServiceModule[]` в `MainMenuScene.unity` связывает кнопку, `SceneName`, `ModuleKey`, значок уведомления и `GuestAvailable`. При добавлении модуля нужны сцена в Build Settings и элемент этого массива. Сейчас гостю доступны Xoxi, GovCalculator, Calendar, Cinema, Reward, Zong и Durak; кнопки Alcometr и Vote для гостя выключены. Это **видимость кнопок клиента**, а фактические права на действия определяет сервер.

Кнопка модуля вызывает `SceneManager.LoadScene(SceneName)`. Экраны возвращаются в `MainMenuScene`. `ScreenOrientationPolicy` в `Assets/_MainApp/Scripts/App/MainMenuScreen.cs` разрешает любое положение в основном приложении и фиксирует ландшафт для Xoxi. В WebGL он передаёт режим через `Assets/Plugins/WebGL/PushUslugiOrientation.jslib` в шаблон страницы.

## Где искать код

| Каталог | Содержание |
| --- | --- |
| `Assets/_MainApp/Scripts/App/` | Запуск, авторизация, профиль, меню, API-клиент, уведомления, Web Push. |
| `Assets/_CalendarEvents/Scripts/` | Календарь, загрузка месяца, добавление/удаление событий, адаптивная раскладка. |
| `Assets/_CinemaModule/Scripts/` | Список фильмов, открытие ссылок, добавление фильма. |
| `Assets/_RewardModule/Scripts/` | Просмотр чемпионатов и государственных наград. |
| `Assets/_VoteModule/Scripts/` | Список и детали голосований, создание опроса, выбор и отправка ответов. Поля вариантов создаются из `OptionInputRowTemplate` в `VoteScene`, а экран прокручивает список к добавленной строке. |
| `Assets/_Zong/Scripts/` | Комнаты, броски костей, банк очков, подсчёт и анимация. |
| `Assets/_Durak/Scripts/` | Комнаты, карты и ходы, отображение игроков и карточных скинов. |
| `Assets/_Messenger/Scripts/` | API и контроллер сцены мессенджера; список пользователей и чатов, сообщения с периодическим обновлением, отметки непрочитанного и переключатель уведомлений каждого чата. Список пользователей выводит `displayName` из API в формате «фамилия имя отчество»; заголовок сообщения получает `senderName` в формате «имя отчество». `MessengerMessageRowView` хранит ссылки на тексты в редактируемом шаблоне сообщения. |
| `Assets/_Xoxi/Scripts/` | Автономная игра, боты, локальные настройки и сохранения. |
| `Assets/_Alcometr/script/main.cs` | Сценарий алкометра и его UI. |
| `Assets/_GovCalculator/Scripts/GovCalculatorScreen.cs` | Расчёт суммы на человека; формула `S / (N - 0,5)` и половина этой суммы для льготника. |
| `Assets/Scripts/Responsive/` | Масштабирование UI при смене ориентации. |
| `Assets/*/Editor/*UiGenerator.cs` | Команды редактора `Tools/Push Uslugi/...` для генерации UI модулей. |
| `Assets/Editor/CanvasUiScaleRestorer.cs` | Инструмент восстановления масштаба Canvas. |
| `Assets/Resources/` | Общие ресурсы и префабы, включая шрифты и настройки DOTween. |
| `Assets/WebGLTemplates/Push-Uslugi/` | Собственный WebGL-шаблон, включая `service-worker.js`. |

Для большинства сетевых модулей действует схема `*Screen.cs` (состояние и кнопки) → `*Api.cs` (запросы) → `*Models.cs` (DTO) плюс небольшие `*View.cs` для строк/элементов списка. Сначала ищите изменение в соответствующем модуле, затем проверяйте общие сервисы.

## Авторизация и сетевой слой

- `Assets/_MainApp/Scripts/App/Api/ApiConfig.cs`: в Unity Editor базовый URL `http://localhost:5028`, вне редактора `https://push-pelmesh.ru`. Это задано через `#if UNITY_EDITOR`; перед сменой адреса учитывайте обе среды.
- `ApiClient.cs` отправляет GET/POST/PUT/DELETE через `UnityWebRequest`. Вызовы с `withAuth: true` добавляют `Authorization: Bearer <token>`. Неуспех преобразуется в `ApiException` со статусом и телом ответа.
- `AuthService.cs` реализует `/api/auth/guest`, `/api/auth/login-by-key`, `/api/auth/login-by-name`, `/api/user/profile`, `/api/user/role`, `/api/user/profile/weight`. DTO находятся в `Models/AuthModels.cs`.
- JWT действует 7 дней. `TokenStorage.cs` хранит JWT в `PlayerPrefs` под ключом `jwt_token`, а выданный при входе по ключу секрет устройства — под `remember_login_token`. После истечения JWT `LoginScreen` показывает добавленную форму входа по имени; сервер принимает её только вместе с секретом устройства и после 3 ошибок требует полный вход по серии и номеру. При 401 во время автовхода очищается JWT, но секрет устройства сохраняется. При явном выходе очищаются оба.
- Сериализация запросов и ответов в этих модулях сделана через `JsonUtility`: при изменении API проверяйте имена и типы **полей** DTO и форму обёрток JSON (например, `events`, `movies`, `poll`, `room`).
- У разных модулей есть проверки ролей в UI. Calendar: `Minister`/`President` для создания, `President` для удаления любых событий, `Minister` для своих; системные события не удаляются. Cinema показывает добавление фильма для `President`. Vote разрешает выбор групп аудитории для `President` или `Governor`. Сервер должен отдельно проверять полномочия.

### Маршруты модулей

| Модуль | Клиентский API |
| --- | --- |
| Calendar | `/api/calendar/events` — выборка по `from`/`to`, создание, удаление по ID. |
| Reward | `/api/rewards/championships`, `/api/rewards/government-awards` — чтение. |
| Cinema | `/api/cinema/movies` — чтение и создание; `CinemaRowView` открывает URL фильма через `Application.OpenURL`. |
| Vote | `/api/votes/polls` — список, детали, создание, отправка ответа через `/{id}/vote`. |
| Zong | `/api/zong/rooms` — список, комната, создание, вход, `start`, `roll`, `bank`. |
| Durak | `/api/durak/rooms` — список, комната, создание, вход, `start`, `attack`, `defend`, `transfer`, `take`, `pass`. |
| Уведомления меню | `/api/module-notifications/unread`, `/api/module-notifications/{moduleKey}/read`. |
| Web Push | `/api/push/subscribe` из JS-моста; `/api/push/unsubscribe` из C#. |
| Messenger | `/api/messenger/users`, `/chats`, `/chats/direct`, `/chats/group`, `/chats/{id}/messages`; ID `0` означает общий чат. Доступ только у негостевых пользователей с серией и номером ID. |

Zong и Durak получают состояние комнаты от сервера и выполняют игровые действия через API; клиентские экраны отображают полученные права и состояние. Для изменений правил игры проверяйте серверный контракт, а также локальный `ZongScoring.cs`, если работа касается отображения/подсчёта «Зонга». `ModuleKey` для индикаторов берётся из поля в сцене, а при пустом поле — из `SceneName`; в текущем меню особые значения: `CalendarScene` и `VoteModule`. Меню обновляет индикаторы примерно раз в 60 секунд и помечает модуль прочитанным при открытии.

## WebGL и локальные особенности

- В `ProjectSettings/ProjectSettings.asset` выбран WebGL-шаблон `PROJECT:Push-Uslugi`. `WebPushService.cs` вызывает `Assets/Plugins/WebGL/WebPushBridge.jslib`; JS работает с browser Notification/Push API и service worker из `Assets/WebGLTemplates/Push-Uslugi/service-worker.js`. В Editor Web Push только пишет сообщения в лог; проверяйте подписку в WebGL-сборке в браузере.
- У сообщений мессенджера есть Web Push для подписанных браузеров. Меню опрашивает список чатов для индикатора непрочитанного каждые 10 секунд, экран мессенджера — каждые 5 секунд; открытый чат отмечает загруженные сообщения прочитанными. Для кнопки мессенджера в `serviceModules` используйте `SceneName: MessengerScene`, `ModuleKey: MessengerScene` и назначенный `NotificationIcon`; пустой `ModuleKey` также берётся из `SceneName`. Кнопка «Без звука» / «Включить» сохраняет настройку на сервере для конкретного пользователя и чата. При уже существующей push-подписке меню повторно связывает её с текущим аккаунтом.
- Собственный шаблон и JS-мост обеспечивают поведение страницы/ориентации. Если меняете ориентацию, проверяйте и C# `ScreenOrientationPolicy`, и мост, и шаблон.
- Xoxi имеет собственные `MenuXoxi`/`GameXoxi`, сохранения в `Assets/_Xoxi/Scripts/SaveSystem/` и инициализацию DOTween в `RuntimeBootstrap.cs`; она устроена отдельно от серверных модулей.
- Алкометр использует данные профиля, в том числе вес; GovCalculator вычисляет локально и не обращается к API.
- `Assets/_*/Editor/*UiGenerator.cs` создают UI и могут менять сцену. Не запускайте генерацию поверх доработанной сцены без проверки ожидаемого результата и diff.
- Существующий UI сохраняйте: при новых функциях добавляйте элементы, не пересоздавайте пользовательские сцены. Кнопка мессенджера уже создана пользователем в `MainMenuScene` и привязана к `serviceModules`; не создавайте её в коде. Галка «Экстра» добавляется в существующую форму кинотопа при запуске.

## Порядок работы с репозиторием

1. Перед правкой смотрите `git status --short` и сохраняйте уже существующие незакоммиченные изменения пользователя. Сцены и префабы — сериализованный YAML: проверяйте diff на лишние изменения.
2. При добавлении или перемещении Unity-ассетов сохраняйте `.meta` и GUID; не удаляйте их случайно. `Library/`, `Temp/`, `Logs/`, `UserSettings/`, сборки и генерируемые `.csproj`/`.slnx` игнорируются и не являются исходниками проекта.
3. Для работы с UI проверяйте `[SerializeField]` в скрипте **и назначения в сцене**. Изменения одного C# файла часто недостаточно. Для нового экрана синхронизируйте сцену, Build Settings, кнопку меню и возврат в меню.
4. Для изменения API синхронизируйте `*Api.cs`, `*Models.cs`, экран и серверный контракт. Не делайте вывод о поведении backend только по клиентскому DTO.
5. Проверка после C#-правок: открыть проект в указанной версии Unity, дождаться импорта и компиляции, проверить Console и соответствующую сцену в Play Mode. Для сетевых модулей нужен работающий API; Web Push и поведение браузера дополнительно проверяются в WebGL. В репозитории нет собственных тестовых C#-файлов вне сторонних плагинов, поэтому не заявляйте об автоматических тестах, если они не запускались.
6. Не меняйте адрес production API, ключи Web Push, настройки сборки и сторонние плагины как побочный эффект работы над отдельным экраном. Не записывайте реальные токены или данные пользователей в репозиторий и вывод команд.

Документ — карта для быстрого входа в проект, а не замена чтению актуального кода, сцен и настроек перед конкретной задачей.

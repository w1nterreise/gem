# Gem

Ультралегковесный десктопный ассистент автоматизации для мгновенного взаимодействия с веб-интерфейсом ИИ (Gemini) напрямую через буфер обмена Windows.

Приложение представляет собой компактный виджет, закрепленный у края экрана, который полностью берет на себя рутинную работу по форматированию, лимитированию и отправке ваших текстов в нейросеть.

---

## ⚡ Управление и Горячие Клавиши

Взаимодействие с ассистентом построено на перехвате правого клика мыши (`ПКМ`) внутри окна приложения с использованием клавиш-модификаторов:

*   **Простой клик ПКМ** — **Режим Терминала (Linux-style)**. Мгновенно забирает текущий текст из буфера обмена, подготавливает его и отправляет в ИИ без вывода каких-либо меню.
*   **Ctrl + ПКМ** — Открывает контекстное меню с вашими кастомными промптами-шаблонами из секции `[CTRL]`.
*   **Shift + ПКМ** — Открывает контекстное меню с шаблонами из секции `[SHIFT]`.
*   **Alt + ПКМ** — Открывает контекстное меню с шаблонами из секции `[ALT]`.
*   **Ctrl + Shift + ПКМ** — **Хакорд разработчика**. Пропускает перехват интерфейса и открывает нативное инженерное меню Chromium (WebView2).

---

## ⚙️ Конфигурация (`prompts.ini`)

При первом запуске приложение автоматически создает рядом со своим исполняемым файлом конфигурационный файл **`prompts.ini`** на русском языке с подробной инструкцией. 

### Правила синтаксиса файла:
1.  Строки, начинающиеся с символа `#`, считаются комментариями и полностью игнорируются парсером.
2.  Разделение промптов внутри секций `[CTRL]`, `[SHIFT]` и `[ALT]` происходит строго с помощью **двух переносов строки (одной пустой строки)**.
3.  Используйте плейсхолдер **`{clip}`** в любом месте вашего шаблона. При вызове команды Gem автоматически заменит его на актуальное содержимое буфера обмена. Если плейсхолдер отсутствует, команда выполнится как статичная, а буфер будет проигнорирован.

Приложение поддерживает **Hot Reload**: кэш промптов в оперативной памяти обновляется автоматически «на лету» при любом сохранении файла `prompts.ini` (контролируется по времени изменения и размеру файла).

---

## 🛠 Устранение неполадок (Troubleshooting)

### Бесконечная загрузка при авторизации Google
Встроенный контейнер Microsoft WebView2 из-за жестких правил изоляции кук иногда приостанавливает каскад внутренних редиректов безопасности Google (на этапе `CheckCookie`) сразу после успешного ввода пароля.
*   **Решение:** Если после ввода пароля страница авторизации зависла в бесконечной загрузке — просто нажмите кнопку **«Домой» (Home)** на верхней панели виджета Gem. Ваша сессия уже успешно сохранилась в контейнере, и приложение мгновенно перенаправит вас на рабочую страницу.




# Gem

An ultra-lightweight desktop automation assistant for instant interaction with the AI web interface (Gemini) directly via the Windows clipboard.

The application is a compact widget docked to the side of the screen that completely automates the routine work of formatting, limiting, and sending your texts to the neural network.

---

## ⚡ Controls and Hotkeys

Interaction with the assistant is based on intercepting the right mouse click (`RMB`) inside the application window using modifier keys:

*   **Plain RMB Click** — **Terminal Mode (Linux-style)**. Instantly grabs the current text from the clipboard, prepares it, and sends it to the AI without displaying any menus.
*   **Ctrl + RMB** — Opens a context menu with your custom prompt templates from the `[CTRL]` section.
*   **Shift + RMB** — Opens a context menu with templates from the `[SHIFT]` section.
*   **Alt + RMB** — Opens a context menu with templates from the `[ALT]` section.
*   **Ctrl + Shift + RMB** — **Developer Cheat Code**. Bypasses interface interception and opens the native Chromium engineering menu (WebView2).

---

## ⚙️ Configuration (`prompts.ini`)

Upon its first launch, the application automatically creates a configuration file named **`prompts.ini`** next to its executable file with detailed instructions.

### File Syntax Rules:
1.  Lines starting with the `#` character are considered comments and are completely ignored by the parser.
2.  Prompts within the `[CTRL]`, `[SHIFT]`, and `[ALT]` sections are strictly separated using **two line breaks (one empty line)**.
3.  Use the **`{clip}`** placeholder anywhere within your template. When a command is triggered, Gem will automatically replace it with the current contents of the clipboard. If the placeholder is omitted, the command will execute as a static prompt, and the clipboard content will be completely ignored.

The application supports **Hot Reload**: the prompt cache in RAM updates automatically "on the fly" whenever the `prompts.ini` file is saved (monitored by file modification time and file size).

---

## 🛠 Troubleshooting

### Infinite Loading During Google Authentication
Due to strict cookie isolation rules, the built-in Microsoft WebView2 container can sometimes pause the cascade of internal Google security redirects (at the `CheckCookie` stage) immediately after a password is successfully entered.
*   **Solution:** If the login page hangs on infinite loading after you enter your password — simply click the **"Home"** button on the top panel of the Gem widget. Your session has already been successfully saved in the container, and the application will instantly redirect you to the working page.

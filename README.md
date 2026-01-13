# FileGet — Прием файлов через веб-форму

**FileGet** — приложение для быстрого приема файлов на компьютер через веб-форму. Генерируется публичная ссылка туннеля, по которой можно загружать файлы с любого устройства в локальную сеть или интернет.

## Основные возможности

- **Веб-форма** — загрузка файлов: документов, изображений, архивов
- **Публичная ссылка** — туннель с автогенерацией URL
- **Локальное хранилище** — файлы сохраняются в `C:\FileGet\Files`
- **Manager GUI** — управление сервером из WPF приложения
- **ASP.NET Core API** — RESTful backend для обработки

## Требования

- **ОС**: Windows 10/11 (64-bit)
- **.NET Runtime**: .NET 10.0 или выше
- **Память**: 256 MB минимум
- **Интернет**: Для работы туннеля

## Быстрый старт

### 1. Клонирование репозитория
```bash
git clone <repository-url> FileGet
cd FileGet
```

### 2. Запуск LocalServer (Backend)

**Production режим** (без Hot Reload):
```bash
cd LocalServer
dotnet run --urls "http://localhost:5233" --environment Production
```

**Или через Debug** (с Hot Reload):
```bash
dotnet watch run
```

Сервер запущен → `http://localhost:5233`

### 3. Запуск Manager (GUI)

**Visual Studio**:
1. Откройте `FileGet.sln`
2. Right-click `Manager` → Set as Startup Project
3. F5 (Debug) или Ctrl+F5 (Release)

**Или из командной строки**:
```bash
cd Manager
dotnet run
```

### 4. Генерирование Туннеля

В Manager → Кнопка **Start** → автоматический запуск туннеля

Получите публичную ссылку (например: `https://fileget-test.loca.lt`)

### 5. Загрузка файлов

1. Откройте ссылку туннеля в браузере
2. Загрузите файл через веб-форму
3. Файл сохранится в `C:\FileGet\Files`

## Структура проекта

```
FileGet/
├── LocalServer/                   # ASP.NET Core Web API
│   ├── wwwroot/
│   │   └──  index.html            # Веб-форма
│   ├── Program.cs                 # Конфигурация сервера, обработка загрузок
│   └── LocalServer.csproj
│
├── Manager/                       # WPF Desktop приложение
│   ├── MainWindow.xaml            # Интерфейс
│   ├── MainWindow.xaml.cs         # Логика туннеля
│   └── Manager.csproj
│
└── FileGet.sln
```

## Как работает туннель?

**Туннель** (tunnel) — это публичная ссылка, которая перенаправляет трафик из интернета на ваш локальный сервер (`localhost:5233`).

### Поток данных:
```
Мобильный телефон
    ↓ (HTTPS запрос)
https://fileget-test.loca.lt
    ↓ (перенаправление)
Ваш компьютер (localhost:5233)
    ↓ (сохранение файла)
C:\FileGet\Files
```

## Туннель

### ⚠️ Текущий статус: В разработке

На данный момент возникают трудности с выбором оптимального решения для туннеля:

| Решение | Статус | Проблема |
|---------|--------|----------|
| **Ngrok** | Недоступен | Заблокирован в России (санкции 2025) |
| **cloudflared** | Недоступен | Заблокирован в России |
| **Pinggy** | False Positive | Антивирус блокирует как HackTool |
| **Bore** | False Positive | Антивирус ругается |
| **Zrok** | Тестирование | **Планируемое решение** |

До этого я использовал туннели **cloudflared** и **localtunnel**.

**cloudflared** постоянно выдавал ошибку 1033, скорее всего он заблокирован провайдерами.
В **localtunnel** я пока не смог исправить ситуацию, когда страница постоянно автоматически обновляется после получения публичной ссылки.

### Временное решение: Локальный режим

Пока туннель разрабатывается, используйте **локальный доступ**:

```bash
# Запуск сервера локально
cd LocalServer
dotnet run --urls "http://localhost:5233"

# Доступ с того же компьютера
http://localhost:5233
```

**Доступ с других устройств в локальной сети**:
```
http://<IP-вашего-ПК>:5233

# Пример:
http://192.168.1.100:5233
```

### Будущее: Zrok

**Zrok** — open-source альтернатива Ngrok, работает в России.
Его особенности:
- No-code регистрация
- Работает с российскими IP
- Open-source (NetFoundry)
- HTTPS по умолчанию

**Планируемая интеграция**: Q1 2026

## API Endpoints

### Загрузка файла
```http
POST /api/upload HTTP/1.1
Host: localhost:5233
Content-Type: multipart/form-data

[file data]
```

**Ответ (200 OK)**:
```json
{
  "success": true,
  "fileName": "document.pdf",
  "filePath": "C:\\FileGet\\Files\\document.pdf",
  "timestamp": "2026-01-13T12:30:00Z"
}
```

### Проверка статуса
```http
GET /api/status HTTP/1.1
Host: localhost:5233
```

**Ответ**:
```json
{
  "status": "online",
  "uploadPath": "C:\\FileGet\\Files",
  "filesCount": 5
}
```

### Manager приложение
- Кнопка **Start** — запуск сервера + туннеля
- Отображение публичной ссылки
- Статус соединения

### Веб-форма
- Форма загрузки файлов
- QR-код для мобильного доступа
- Статус загрузки

## Troubleshooting

### Ошибка: "Port 5233 is already in use"
```bash
# Найти процесс на порту 5233
netstat -ano | findstr :5233

# Завершить процесс (замените PID)
taskkill /PID <PID> /F
```

### Ошибка: "Cannot find dotnet"
Установить .NET 10.0 Runtime: https://dotnet.microsoft.com/download

### Файлы не сохраняются
```bash
# Проверить путь и права доступа
# По умолчанию: C:\FileGet\Files
# Создать директорию, если её нет

mkdir C:\FileGet\Files
```

## Использование

### Сценарий 1: Одноразовая загрузка
1. Запустить Manager → Start
2. Получить ссылку туннеля (когда будет готов)
3. Отправить ссылку коллеге
4. Коллега загружает файлы через веб-форму
5. Остановить Manager → Stop

### Сценарий 2: Постоянное хранилище
1. Запустить LocalServer в Production
2. Настроить туннель (Zrok)
3. Использовать как постоянный файлообменник
4. Периодически архивировать старые файлы

## Разработка

### Требования для разработчиков
- Visual Studio 2022 (Community или выше)
- .NET 10.0 SDK
- Node.js (для фронтенда, если требуется)

### Запуск в режиме разработки
```bash
# LocalServer с Hot Reload
cd LocalServer
dotnet watch run

# Manager (в отдельном терминале)
cd Manager
dotnet run
```

### Структура кода
- **Controllers** — API endpoints
- **wwwroot** — статические файлы (HTML, CSS, JS)
- **Models** — структуры данных
- **Services** — бизнес-логика

## Вклад в проект

Для добавления функций или исправления ошибок:

1. Fork репозитория
2. Создайте branch (`git checkout -b feature/new-feature`)
3. Commit изменений (`git commit -m 'Add new feature'`)
4. Push в branch (`git push origin feature/new-feature`)
5. Откройте Pull Request

---

**FileGet v0.1.0** | Автор: [Ваше имя] | Обновлено: 13 января 2026

![Logo](./docs/logo.svg)

____

![Demo](https://img.shields.io/badge/⚠️%20Demo%20Project-Non--functional%20(ToS%20limitations)-orange?style=for-the-badge)
![Tests](https://github.com/Miclell/GorzdravBooking/actions/workflows/dotnet.yml/badge.svg) ![License](https://img.shields.io/badge/license-MIT-green) ![DotNet](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet) ![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)

____

# ⚠️Дисклеймер⚠️

#### Проект выкладывается в нерабочей версии, поскольку Terms of Service (ToS) сайта, с которым он работает, не существует (для имитации используется фейковый ApiService). Данный репозиторий представляет собой демонстрацию кода и архитектурных решений.

____

## Как это работает?

GorzdravBooking автоматизирует процесс записи к врачу через систему Gorzdrav:

### Основной workflow:

1. Поиск талонов - система периодически проверяет доступные записи
2. Фильтрация по предпочтениям - отбор по врачу, специальности, времени
3. Автоматическое бронирование - мгновенное занятие подходящего талона

### Ключевые возможности:

- Умный поиск по временным предпочтениям (утро/день/вечер, конкретные дни)
- Приоритизация - выбор наиболее подходящих талонов
- Авто-ретраи - повторные попытки при занятии талона
- Логирование - детальный трекинг процесса

## Текущее состояние

- ✅ Реализован консольный вариант
- ✅ React фронтенд
- ❌ Telegram bot

## Установка и запуск

Требования: .NET 10 SDK (версия закреплена в `global.json`), Node.js 24 для React. Открывать `GorzdravBooking.slnx`.
Docker для Aspire не нужен: SQLite работает в процессе API.

Из корня репозитория:

```bash
dotnet tool restore
dotnet nuke --target Check
dotnet run --project src/AspireHost
```

Aspire запускает API и React, показывает ссылки и логи в dashboard. При первом запуске восстанавливаются компоненты
Aspire CLI bundle. Адрес dashboard — `http://localhost:15280`; ссылка с токеном выводится в терминал. Frontend получает
адрес API автоматически, Vite проксирует `/api`.

Отдельный запуск без Aspire:

```bash
dotnet run --project src/Presentation/CLI
dotnet run --project src/Presentation/Server --launch-profile http
# В другом терминале:
npm --prefix src/Presentation/Web/gorzdrab-booking ci
npm --prefix src/Presentation/Web/gorzdrab-booking run dev
```

Зарегистрирован `FakeApiService` в `src/Infrastructure/DependencyInjection.cs`. Настоящий клиент остаётся
закомментированным; проверки не обращаются к Gorzdrav. Cookie-аутентификация по HTTP разрешена только явной настройкой
`Authentication:AllowInsecureLocalhost` в Development/Aspire. В production требуется HTTPS.

### NUKE и публикация CLI

Локальный инструмент не требует глобальной установки:

```bash
dotnet nuke --target Build
dotnet nuke --target CLI
dotnet nuke --target CLI --runtime linux-x64
dotnet nuke --target CLI --runtime linux-arm64
```

Без `--runtime` CLI публикуется под текущую ОС и архитектуру. При кросс-сборке укажите переносимый .NET RID явно:
`linux-x64` для обычного Arch Linux на x86_64, `linux-arm64` для ARM64, `linux-musl-x64` для Alpine.
Если NUKE установлен глобально, эквивалент: `nuke --target CLI --runtime linux-x64`. Без установки инструмента:
`./build.ps1 --target CLI --runtime linux-x64` или `bash build.sh --target CLI --runtime linux-x64`.

Результат: `artifacts/cli/linux-x64/CLI` или `artifacts/cli/win-x64/CLI.exe`. Это self-contained single-file:
SDK/runtime на целевой машине не нужен. Native SQLite распаковывается .NET во временный каталог. Trimming/AOT не
включены, чтобы сохранить совместимость EF Core и меню. Для Linux нужен обычный glibc-дистрибутив с системными
зависимостями .NET. `--target` выбирает задачу NUKE, а `--runtime` — платформу публикуемого CLI.
Нативный Termux использует Android `bionic`, поэтому `linux-arm64` для него не подходит. Но Ubuntu внутри
`proot-distro` — отдельное окружение с glibc: на ARM64-устройстве `dotnet nuke --target CLI`, запущенный внутри
Ubuntu с установленным .NET 10 SDK, должен выбрать `linux-arm64`. Можно также собрать этот RID на другом компьютере
через `--runtime linux-arm64` и перенести результат внутрь Ubuntu. Работу .NET под proot нужно проверить на самом
устройстве: совместимость такого окружения не равна обычному Ubuntu-хосту.

Цели: `Restore`, `Build`, `Test` (TRX + Cobertura в `artifacts/test-results`), `Frontend`, `Slopwatch`, `Check`
(сборка + тесты + frontend + Slopwatch), `Format` (проверка форматирования), `CLI`, `Server`, `Docker`.
`--configuration Debug` меняет Release по умолчанию. `Server` публикует API в `artifacts/server`; `Docker` собирает
контейнер API и требует запущенный Docker.

GitHub Actions запускает только сборку и тесты .NET с проверкой Slopwatch. Он не публикует CLI, не собирает frontend-бандл
и не загружает исполняемые файлы или другие artifacts. Локальная цель `Check` по-прежнему дополнительно собирает frontend.

### SQLite и миграции

Server и CLI автоматически применяют единственную `InitialCreate` до запуска запросов и фонового worker. Повторный
запуск сохраняет данные. В исходниках используется общая `data/GorzdravBooking.db`, при запуске опубликованной версии
вне репозитория — база рядом с исполняемым файлом. Текущий каталог не влияет на путь.

Для явного выбора базы задайте `ConnectionStrings__GorzdravBooking` (абсолютный путь). Например, PowerShell:

```powershell
$env:ConnectionStrings__GorzdravBooking = 'Data Source=C:/data/GorzdravBooking.db'
dotnet run --project src/Presentation/CLI -- --migrate-only
```

Linux:

```bash
ConnectionStrings__GorzdravBooking='Data Source=/var/lib/gorzdrav/booking.db' ./CLI --migrate-only
```

Режим `--migrate-only` проверяет/создаёт схему и завершает CLI без меню и worker. Он также удобен для проверки
опубликованного файла.

Команды разработки миграций (design-time factory использует тот же путь и переменную окружения):

```bash
dotnet ef migrations list --project src/Infrastructure
dotnet ef migrations has-pending-model-changes --project src/Infrastructure
dotnet ef migrations add ChangeName --project src/Infrastructure --output-dir Persistence/Migrations
dotnet ef database update --project src/Infrastructure
dotnet ef migrations remove --project src/Infrastructure
```

Перед обновлением существующей базы остановите Server и CLI и сохраните backup базы, включая `-wal`/`-shm`, если они
существуют. Не копируйте активную SQLite-базу обычным файловым копированием. Неизвестная история миграций вызывает
понятную ошибку: автоматического удаления/сброса данных нет. Для старой экспериментальной схемы выберите новый путь или
перенесите данные отдельно. Откат через `dotnet ef database update <PreviousMigration>` возможен после backup;
`database update 0` удаляет таблицы и данные.

### Структура

`src/Core`, `src/Application`, `src/Infrastructure` и `src/Presentation/{CLI,Server,Web}` сохраняют существующие слои.
По примеру Moviezator/Procurator добавлены `src/AspireHost`, `src/ServiceDefaults`, общие `Directory.Build.props`/
`Directory.Packages.props` и `.slnx`. Оркестрация передаёт обычные настройки окружения; бизнес-код не зависит от Aspire.

Актуальный объём подготовки — [TODO.md](TODO.md), исходные предложения о будущем
рефакторинге — [docs/Backlog.md](docs/Backlog.md).

____

- #### [Руководство пользователя](./docs/UserManual.md)
- #### [Архитектура](./docs/Architecture.md)

____

## Лицензия

MIT — см. [LICENSE](./LICENSE).

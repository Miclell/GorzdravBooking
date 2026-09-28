![Logo](./docs/logo.svg)

____

![Demo](https://img.shields.io/badge/⚠️%20Demo%20Project-Non--functional%20(ToS%20limitations)-orange?style=for-the-badge)
![Tests](https://github.com/Miclell/GorzdravBooking/actions/workflows/dotnet.yml/badge.svg) ![License](https://img.shields.io/badge/license-MIT-green) ![DotNet](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet) ![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)

____

# Дисклеймер

Публичная версия проекта демонстрационная и не подключается к Gorzdrav из-за ограничений использования сервиса.

____

## Как это работает?

GorzdravBooking автоматизирует процесс записи к врачу через систему Gorzdrav:

Основной сценарий:

1. Поиск талонов – система периодически проверяет доступные записи
2. Фильтрация по предпочтениям – отбор по врачу, специальности, времени
3. Автоматическое бронирование – занятие подходящего талона

## Установка и запуск

Требования: .NET 10 SDK и Node.js 24. Для работы в IDE откройте `GorzdravBooking.slnx`.

Из корня репозитория:

```bash
dotnet tool restore
dotnet nuke --target Check
dotnet run --project src/AspireHost
```

Aspire запустит API и веб-интерфейс; ссылку на dashboard покажет терминал.

Отдельный запуск без Aspire:

```bash
dotnet run --project src/Presentation/CLI
dotnet run --project src/Presentation/Server --launch-profile http
# В другом терминале:
npm --prefix src/Presentation/Web/gorzdrab-booking ci
npm --prefix src/Presentation/Web/gorzdrab-booking run dev
```

CLI сохраняет служебные логи в файл рядом с приложением, не перекрывая меню.

### Сборка с NUKE

`--target` выбирает задачу: `Check` проверяет проект, `CLI` публикует консольное приложение.
`--runtime` задаёт платформу публикации; без него выбирается текущая система.

```bash
dotnet nuke --target CLI
dotnet nuke --target CLI --runtime linux-x64
```

Для ARM64 укажите `--runtime linux-arm64`. Результат находится в `artifacts/cli/<runtime>/` и запускается
без установленного .NET runtime на целевой машине.

### SQLite и миграции

Server и CLI применяют миграции при запуске. При запуске из исходников база находится в `data/GorzdravBooking.db`,
у опубликованного приложения – рядом с исполняемым файлом. Путь можно изменить через
`ConnectionStrings__GorzdravBooking`:

```powershell
$env:ConnectionStrings__GorzdravBooking = 'Data Source=C:/data/GorzdravBooking.db'
dotnet run --project src/Presentation/CLI -- --migrate-only
```

`--migrate-only` подготовит базу и завершит CLI без запуска меню.

____

- #### [Руководство пользователя](./docs/UserManual.md)
- #### [Архитектура](./docs/Architecture.md)

____

## Лицензия

MIT – см. [LICENSE](./LICENSE).

# Архитектура системы

## Обзор

Система построена на принципах **Clean Architecture** с четким разделением ответственности между слоями. Архитектура обеспечивает независимость бизнес-логики от деталей реализации, что делает систему легко тестируемой, расширяемой и поддерживаемой.

## Архитектурные слои

Система организована в четыре основных слоя:

```mermaid
graph TB
    subgraph "Presentation Layer"
        CLI[CLI Interface]
    end
    
    subgraph "Application Layer"
        UC[Use Cases]
        SVC[Services]
        COORD[Coordinators]
        WORK[Workers]
    end
    
    subgraph "Core Layer"
        ENT[Entities]
        INT[Interfaces]
        EVT[Events]
        MOD[Models]
    end
    
    subgraph "Infrastructure Layer"
        REPO[Repositories]
        EXT[External Services]
        DB[(Database)]
        EB[Event Bus]
    end
    
    CLI --> UC
    CLI --> SVC
    UC --> COORD
    COORD --> SVC
    WORK --> UC
    SVC --> INT
    COORD --> INT
    REPO --> ENT
    EXT --> MOD
    REPO --> DB
    SVC --> REPO
    COORD --> EXT
    SVC --> EB
    WORK --> EB
```

### Core Layer (Ядро)

**Назначение:** Содержит бизнес-логику и доменные сущности, не зависящие от внешних библиотек и фреймворков.

**Компоненты:**
- **Entities** — доменные сущности (User, PatientProfile, Appointment, AppointmentSearchRequest, TimePreferences)
- **Interfaces** — контракты для репозиториев и сервисов
- **Events** — доменные события для асинхронной коммуникации
- **Models** — модели данных для внешних API
- **Enums** — перечисления для статусов и типов

**Принципы:**
- Не зависит от других слоев
- Содержит только бизнес-логику
- Определяет интерфейсы, реализуемые в Infrastructure

### Application Layer (Приложение)

**Назначение:** Содержит бизнес-логику приложения, use cases и координацию между сервисами.

**Компоненты:**
- **Use Cases** — конкретные сценарии использования (CheckAppointmentSearchRequestsUseCase)
- **Services** — сервисы приложения (AppointmentService, PatientService, TimePreferencesService)
- **Coordinators** — координаторы для сложных операций (AppointmentCoordinator)
- **Workers** — фоновые сервисы (AppointmentSchedulerWorker)
- **DTOs** — объекты передачи данных между слоями

**Принципы:**
- Зависит только от Core
- Содержит бизнес-правила и валидацию
- Использует интерфейсы из Core, не зная об их реализации

### Infrastructure Layer (Инфраструктура)

**Назначение:** Реализует технические детали: доступ к данным, внешние API, события.

**Компоненты:**
- **Repositories** — реализация паттерна Repository для работы с БД
- **Persistence** — конфигурация Entity Framework, миграции
- **Services** — реализация внешних сервисов (ExternalAppointmentService, ExternalPatientService)
- **ApiClient** — HTTP клиент для работы с внешним API
- **Events** — реализация Event Bus (InMemoryEventBus)
- **Security** — хеширование паролей и другие функции безопасности

**Принципы:**
- Реализует интерфейсы из Core
- Содержит технические детали реализации
- Может быть заменен без изменения бизнес-логики

### Presentation Layer (Представление)

**Назначение:** Пользовательский интерфейс и точка входа в приложение.

**Компоненты:**
- **CLI** — консольный интерфейс с меню и командами

**Принципы:**
- Зависит от Application и Infrastructure
- Отвечает только за представление данных
- Не содержит бизнес-логики

## Основные паттерны

### 1. Repository Pattern

Инкапсулирует логику доступа к данным, предоставляя более объектно-ориентированный интерфейс.

```mermaid
classDiagram
    class IRepository~T~ {
        <<interface>>
        +AddAsync(T)
        +GetByIdAsync(Guid)
        +UpdateAsync(T)
        +DeleteAsync(Guid)
    }
    
    class IUserRepository {
        <<interface>>
        +GetByUsernameAsync(string)
    }
    
    class UserRepository {
        -DbContext context
        +AddAsync(User)
        +GetByIdAsync(Guid)
    }
    
    IRepository~T~ <|.. IUserRepository
    IUserRepository <|.. UserRepository
```

### 2. Service Layer Pattern

Сервисы инкапсулируют бизнес-логику и координируют работу между репозиториями.

```mermaid
classDiagram
    class IAppService {
        <<interface>>
    }
    
    class IAppointmentService {
        <<interface>>
        +CreateAsync(CreateAppointmentDto)
        +GetByPatientIdAsync(Guid)
    }
    
    class AppointmentService {
        -IAppointmentRepository repository
        +CreateAsync(CreateAppointmentDto)
    }
    
    IAppService <|.. IAppointmentService
    IAppointmentService <|.. AppointmentService
    AppointmentService --> IAppointmentRepository
```

### 3. Coordinator Pattern

Координаторы управляют сложными бизнес-процессами, объединяя несколько сервисов.

```mermaid
classDiagram
    class IAppointmentCoordinator {
        <<interface>>
        +CreateCompleteAppointmentAsync(AppointmentSearchRequest)
    }
    
    class AppointmentCoordinator {
        -IExternalAppointmentService externalService
        -ITimePreferencesService timePreferencesService
        -IAppointmentService appointmentService
        +CreateCompleteAppointmentAsync(AppointmentSearchRequest)
        -TryFindAndBookAppointmentWithRetryAsync()
        -TryGetPreferAppointment()
    }
    
    IAppointmentCoordinator <|.. AppointmentCoordinator
    AppointmentCoordinator --> IExternalAppointmentService
    AppointmentCoordinator --> ITimePreferencesService
    AppointmentCoordinator --> IAppointmentService
```

### 4. Event Bus Pattern (Pub/Sub)

Асинхронная коммуникация между компонентами через события.

```mermaid
sequenceDiagram
    participant Worker as AppointmentSchedulerWorker
    participant EB as EventBus
    participant UC as UseCase
    participant Handler as EventHandler
    
    Worker->>EB: PublishAsync(SearchRequestStarted)
    EB->>Handler: Handle(SearchRequestStarted)
    UC->>EB: PublishAsync(SearchRequestCompleted)
    EB->>Handler: Handle(SearchRequestCompleted)
```

### 5. Use Case Pattern

Каждый use case представляет отдельный бизнес-сценарий.

```mermaid
classDiagram
    class IAppUseCase {
        <<interface>>
        +ExecuteAsync(CancellationToken)
    }
    
    class CheckAppointmentSearchRequestsUseCase {
        -IAppointmentSearchRequestRepository repository
        -IAppointmentCoordinator coordinator
        -IEventBus eventBus
        +ExecuteAsync(CancellationToken)
        -IsTimeToCheck(AppointmentSearchRequest, DateTime)
    }
    
    IAppUseCase <|.. CheckAppointmentSearchRequestsUseCase
    CheckAppointmentSearchRequestsUseCase --> IAppointmentSearchRequestRepository
    CheckAppointmentSearchRequestsUseCase --> IAppointmentCoordinator
    CheckAppointmentSearchRequestsUseCase --> IEventBus
```

## Основной workflow

### Процесс автоматического поиска и бронирования

```mermaid
sequenceDiagram
    participant Worker as AppointmentSchedulerWorker
    participant UC as CheckAppointmentSearchRequestsUseCase
    participant Repo as AppointmentSearchRequestRepository
    participant Coord as AppointmentCoordinator
    participant ExtSvc as ExternalAppointmentService
    participant TimeSvc as TimePreferencesService
    participant AppSvc as AppointmentService
    participant DB as Database
    
    loop Каждую минуту
        Worker->>UC: ExecuteAsync()
        UC->>Repo: GetActiveAsync()
        Repo-->>UC: List<AppointmentSearchRequest>
        
        loop Для каждого активного запроса
            UC->>Coord: CreateCompleteAppointmentAsync(request)
            Coord->>TimeSvc: GetByPresetAsync()
            TimeSvc-->>Coord: TimePreferencesPresetDto
            
            Coord->>ExtSvc: GetByDoctorAsync(lpuId, doctorId)
            ExtSvc-->>Coord: List<Appointment>
            
            Coord->>Coord: TryGetPreferAppointment(appointments, preferences)
            
            alt Найден подходящий талон
                Coord->>ExtSvc: CreateAppointmentAsync(request)
                ExtSvc-->>Coord: (success, errorCode)
                
                alt Бронирование успешно
                    Coord->>AppSvc: CreateAsync(dto)
                    AppSvc->>DB: Save Appointment
                    Coord-->>UC: Success(true)
                else Талон уже занят
                    Coord->>Coord: Retry (до 3 попыток)
                end
            else Талон не найден
                Coord-->>UC: Success(false)
            end
            
            UC->>Repo: UpdateAsync(request)
        end
    end
```

### Жизненный цикл запроса на поиск

```mermaid
stateDiagram-v2
    [*] --> Pending: Создание запроса
    Pending --> InProgress: Начало обработки
    InProgress --> InProgress: Поиск талонов
    InProgress --> Completed: Талон успешно забронирован
    InProgress --> Cancelled: Запрос отменен пользователем
    InProgress --> Failed: Превышено количество попыток
    Completed --> [*]
    Cancelled --> [*]
    Failed --> [*]
```

## Структура данных

### Основные сущности

```mermaid
erDiagram
    User ||--o{ PatientProfile : has
    User ||--o{ TimePreferences : has
    User ||--o{ AppointmentSearchRequest : creates
    PatientProfile ||--o{ Appointment : has
    PatientProfile ||--o{ AppointmentSearchRequest : uses
    TimePreferences ||--o{ AppointmentSearchRequest : references
    
    User {
        Guid Id
        string Username
        string PasswordHash
    }
    
    PatientProfile {
        Guid Id
        Guid UserId
        string PatientId
        string LpuId
        string LpuName
    }
    
    TimePreferences {
        Guid Id
        Guid UserId
        string PresetName
    }
    
    AppointmentSearchRequest {
        Guid Id
        Guid PatientProfileId
        string DoctorId
        string Speciality
        TimeSpan SearchInterval
        SearchRequestStatus Status
    }
    
    Appointment {
        Guid Id
        Guid PatientProfileId
        string ExternalAppointmentId
        DateTime VisitStart
        DateTime VisitEnd
    }
```

## Система событий

События используются для асинхронной коммуникации и обновления UI в реальном времени.

### Типы событий

- **SearchRequestStarted** — начало обработки запроса на поиск
- **SearchRequestCompleted** — завершение обработки запроса
- **NextSearchScheduled** — запланирована следующая проверка
- **SearchServiceStatusChanged** — изменение статуса сервиса поиска

### Поток событий

```mermaid
graph LR
    A[Worker] -->|Publish| B[EventBus]
    C[UseCase] -->|Publish| B
    D[Coordinator] -->|Publish| B
    B -->|Notify| E[EventHandlers]
    E -->|Update| F[UI Components]
```

## Dependency Injection

Система использует встроенный DI контейнер .NET для управления зависимостями.

### Регистрация сервисов

```mermaid
graph TB
    A[Program.cs] --> B[AddInfrastructure]
    A --> C[AddApplication]
    B --> D[Repositories]
    B --> E[External Services]
    B --> F[Event Bus]
    B --> G[Database Context]
    C --> H[Application Services]
    C --> I[Use Cases]
    C --> J[Coordinators]
```

**Принципы регистрации:**
- Репозитории и сервисы — `Scoped`
- Event Bus — `Singleton`
- Workers — `HostedService`

## Обработка ошибок

Система использует паттерн Result для обработки ошибок без исключений.

```mermaid
classDiagram
    class Result~T~ {
        +bool IsSuccess
        +bool IsFailure
        +T Value
        +Error Error
    }
    
    class Error {
        +string Code
        +string Message
    }
    
    Result~T~ --> Error : contains
```

**Типы ошибок:**
- `NotFound` — ресурс не найден
- `Conflict` — конфликт при создании/обновлении
- `Failure` — общая ошибка выполнения
- `Validation` — ошибка валидации

## Тестирование

Архитектура поддерживает легкое тестирование благодаря:

1. **Инверсии зависимостей** — все зависимости через интерфейсы
2. **Разделению слоев** — каждый слой тестируется независимо
3. **Использованию моков** — внешние сервисы легко мокируются

### Структура тестов

```
tests/
├── UnitTests/
│   ├── Application.Tests/
│   └── Infrastructure.Tests/
```

## Масштабируемость

Архитектура позволяет легко:

- **Добавлять новые use cases** — просто реализовать `IAppUseCase`
- **Добавлять новые сервисы** — реализовать интерфейс из Core
- **Менять реализацию** — заменить Infrastructure без изменения Application
- **Добавлять новые UI** — создать новый Presentation слой (например, Web API или Telegram Bot)

## Заключение

Архитектура системы обеспечивает:

✅ **Чистоту кода** — четкое разделение ответственности  
✅ **Тестируемость** — легкость написания unit-тестов  
✅ **Расширяемость** — простое добавление новых функций  
✅ **Поддерживаемость** — понятная структура и паттерны  
✅ **Независимость** — бизнес-логика не зависит от деталей реализации

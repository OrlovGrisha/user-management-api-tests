# UserManagement API

Тестовое задание SDET C#: REST API для управления пользователями на ASP.NET Core и набор автотестов к нему.

Данные хранятся в памяти (in-memory), сервис потокобезопасен.

**Автор:** Орлов Григорий

**GitHub:** https://github.com/OrlovGrisha/user-management-api-tests

---

## Возможности

| Метод | URL | Описание | Ответ |
|---|---|---|---|
| `POST` | `/api/users` | Создание пользователя. `userId` и `dateCreated` генерирует сервер | `201 Created` + заголовок `Location` |
| `GET` | `/api/users/{userId}` | Получение пользователя по ID | `200 OK` / `404 Not Found` |
| `PUT` | `/api/users/{userId}` | Обновление `countryIsoCode` и `locale`, выставляет `dateModified` | `200 OK` / `404 Not Found` |
| `GET` | `/api/users?page=&pageSize=` | Список пользователей, пагинация необязательна | `200 OK` |

Пример тела запроса для `POST` и `PUT`:

```json
{ "countryIsoCode": "RU", "locale": "ru-RU" }
```

**Валидация** (при нарушении — `400 Bad Request` в формате `ValidationProblemDetails` с текстом ошибки по каждому полю):
- `userId` в URL — корректный GUID, не `00000000-0000-0000-0000-000000000000`;
- `countryIsoCode` — ровно 2 латинские буквы в любом регистре, сохраняется в верхнем регистре;
- `locale` — строго формат `xx-XX` (например, `ru-RU`, `en-US`);
- `page` — от 1 до 1 000 000, `pageSize` — от 1 до 10.

**Непредвиденные ошибки** возвращаются как `500 Internal Server Error` с JSON в формате `ProblemDetails`, без stack trace и внутренних деталей.

---

## Технологии

**API** (`UserManagement.API`):
- .NET 8, ASP.NET Core Web API
- DataAnnotations — валидация входных данных
- [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore) — логирование, в том числе логи HTTP-запросов
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) — генерация OpenAPI-спецификации
- [Scalar.AspNetCore](https://github.com/scalar/scalar) — интерактивная документация API

**Тесты** (`UserManagement.Tests`):
- xUnit
- Microsoft.AspNetCore.Mvc.Testing (`WebApplicationFactory`) — запуск API в памяти
- Microsoft.Extensions.TimeProvider.Testing (`FakeTimeProvider`) — управляемое время в тестах

---

## Запуск

Требуется [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

### API

```bash
dotnet run --project UserManagement.API --launch-profile http
```

После запуска:
- **Scalar (документация и отправка запросов из браузера):** http://localhost:5242/scalar/v1
- OpenAPI-спецификация: http://localhost:5242/swagger/v1/swagger.json
- API: http://localhost:5242/api/users

Scalar и OpenAPI доступны только в окружении `Development` (задано в профиле запуска).

### Тесты

Из корня решения:

```bash
dotnet test
```

Запуск отдельных групп тестов:

```bash
dotnet test --filter "FullyQualifiedName~Tests.Unit"
dotnet test --filter "FullyQualifiedName~Tests.Integration"
dotnet test --filter "FullyQualifiedName~Tests.E2E"
```

Запуск тестов по ID кейса из [test-cases.md](test-cases.md):

```bash
dotnet test --filter "TestCase=TC-E2E-01"
dotnet test --filter "TestCase~TC-PAGE"
```

Чтобы увидеть логи API в выводе всех тестов, а не только упавших:

```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## Структура

```
UserManagement.API/
  Controllers/        UsersController — эндпоинты
  DTOs/               UserDto — тело запроса с правилами валидации
  ExceptionHandling/  GlobalExceptionHandler — обработка непредвиденных ошибок (500)
  Interfaces/         IUserRepository
  Models/             User — неизменяемая модель пользователя
  Repositories/       InMemoryUserRepository — потокобезопасное хранилище
  Validation/         NotEmptyGuidAttribute — запрет Guid.Empty в URL

UserManagement.Tests/
  Infrastructure/     WebApplicationFactory, базовый класс тестов, логгер для ITestOutputHelper,
                      проверка JSON-схемы, репозиторий-заглушка для теста на 500
  Models/             UserResponse — модель ответа API для десериализации
  Tests/
    Unit/             модель User, атрибут валидации, репозиторий (включая многопоточность)
    Integration/      эндпоинты, валидация, пагинация, обработка ошибок, параллельные запросы, изоляция
    E2E/              сценарии полного жизненного цикла пользователя

test-cases.md         чек-лист тест-кейсов (тест-дизайн)
```

---

## Принятые решения

**Потокобезопасность.** Хранилище построено на `ConcurrentDictionary`. Модель `User` неизменяемая: обновление создаёт новый объект, а репозиторий атомарно заменяет запись через `TryUpdate`. Поэтому параллельные запросы никогда не видят «наполовину обновлённого» пользователя.

**Ожидаемые ошибки — не исключения.** «Пользователь не найден» — штатная ситуация: репозиторий возвращает `null` / `false`, контроллер превращает это в `404`. Глобальный обработчик ловит только действительно непредвиденные исключения.

**Обработка ошибок через `IExceptionHandler`.** Это рекомендуемый в .NET 8 способ глобальной обработки исключений поверх встроенного middleware. Ответы `400`, `404` и `500` возвращаются в едином формате `ProblemDetails`.

**Валидация формата, а не существования.** По ТЗ проверяется формат кодов (`xx-XX`, 2 буквы), а не то, что код существует в ISO 3166-1 / BCP 47. Например, `XX` формально валиден. Проверку по справочнику можно добавить как улучшение.

**Время через `TimeProvider`.** Сервис берёт текущее время из `TimeProvider`, а тесты подменяют его на `FakeTimeProvider`. Даты проверяются точным равенством, без задержек и нестабильных проверок «в пределах интервала».

**Изоляция тестов.** xUnit создаёт новый экземпляр тестового класса на каждый тест, а базовый класс в конструкторе поднимает новый экземпляр API. Каждый тест получает пустое хранилище, сбрасывать состояние не нужно.

**Проверка схемы ответа.** Помимо десериализации, «сырой» JSON проверяется на наличие всех полей и их типы: десериализация не отличает отсутствующее поле `dateModified` от `"dateModified": null`.

**Пагинация.** Без параметров возвращаются все пользователи — как требует базовое описание эндпоинта. Сортировка стабильная: сначала новые, при равных датах — по `userId`.

**Логи в тестах.** Логи приложения выводятся в `ITestOutputHelper` и видны в выводе упавшего теста. У каждого тестового экземпляра API свой логгер, поэтому при параллельном запуске логи разных тестов не смешиваются.

**Трассируемость.** Каждый интеграционный и E2E-тест помечен `[Trait("TestCase", "TC-...")]` и связан с кейсом из [test-cases.md](test-cases.md).

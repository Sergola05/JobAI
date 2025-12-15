# Тестирование сервера JobAI

## Шаг 1: Применение миграций базы данных

Перед первым запуском нужно создать базу данных:

```powershell
cd JobAI.Server

# Если dotnet-ef не установлен, установите его:
dotnet tool install --global dotnet-ef

# Примените миграции
dotnet ef database update
```

Если возникнет ошибка, убедитесь, что:
- SQL Server LocalDB установлен и запущен
- Строка подключения в `appsettings.json` правильная

## Шаг 2: Запуск сервера

### Вариант 1: Через Visual Studio
1. Откройте решение `JobAI.sln`
2. Установите `JobAI.Server` как стартовый проект
3. Нажмите **F5** или выберите профиль **"http"**
4. Сервер запустится на `http://localhost:5143`
5. Автоматически откроется Swagger UI

### Вариант 2: Через командную строку
```powershell
cd JobAI.Server
dotnet run
```

Сервер будет доступен по адресу: **http://localhost:5143**

## Шаг 3: Тестирование через Swagger UI

1. Откройте браузер и перейдите на: **http://localhost:5143/swagger**
2. Вы увидите все доступные API endpoints

### Тест 1: Создание вакансии

**Endpoint:** `POST /api/Vacancies`

1. Найдите `POST /api/Vacancies` в Swagger
2. Нажмите **"Try it out"**
3. Вставьте JSON:
```json
{
  "title": "Разработчик .NET",
  "company": "Технологии будущего",
  "location": "Москва",
  "sourceUrl": "https://example.com/vacancy",
  "description": "Требуется опытный разработчик .NET с опытом работы с ASP.NET Core, Entity Framework, SQL Server. Опыт работы от 3 лет."
}
```
4. Нажмите **"Execute"**
5. Сохраните `id` из ответа (например, `1`)

**Ожидаемый результат:** 
- Status: `201 Created`
- В ответе будет созданная вакансия с `id`, `createdAt`

### Тест 2: Получение списка вакансий

**Endpoint:** `GET /api/Vacancies`

1. Найдите `GET /api/Vacancies`
2. Нажмите **"Try it out"** → **"Execute"**
3. Проверьте, что ваша вакансия в списке

**Ожидаемый результат:**
- Status: `200 OK`
- Массив вакансий, включая созданную ранее

### Тест 3: Получение вакансии по ID

**Endpoint:** `GET /api/Vacancies/{id}`

1. Найдите `GET /api/Vacancies/{id}`
2. Введите `id` из шага 1 (например, `1`)
3. Нажмите **"Execute"**

**Ожидаемый результат:**
- Status: `200 OK`
- Данные вакансии

### Тест 4: Генерация сопроводительного письма

**Endpoint:** `POST /api/CoverLetters/generate`

⚠️ **Важно:** Этот тест требует интернет-соединения и валидный API ключ Perplexity.

1. Найдите `POST /api/CoverLetters/generate`
2. Нажмите **"Try it out"**
3. Вставьте JSON (используйте `id` вакансии из шага 1):
```json
{
  "vacancyId": 1,
  "candidateName": "Иван Иванов",
  "candidateContacts": "ivan@example.com, +7-999-123-45-67",
  "candidateSkills": "C#, ASP.NET Core, Entity Framework, SQL Server, REST API",
  "candidateExperience": "5 лет опыта разработки на .NET. Работал над крупными проектами, включая веб-приложения и API. Опыт работы с микросервисной архитектурой."
}
```
4. Нажмите **"Execute"**
5. **Дождитесь ответа** (генерация может занять 10-30 секунд)

**Ожидаемый результат:**
- Status: `200 OK`
- Сгенерированное письмо с полями:
  - `id` - ID письма
  - `vacancyId` - ID вакансии
  - `vacancyTitle` - Название вакансии
  - `candidateName` - Имя кандидата
  - `candidateContacts` - Контакты
  - `letterText` - Текст письма (200-300 слов)
  - `createdAt` - Дата создания

**Возможные ошибки:**
- `400 Bad Request` - проверьте, что вакансия существует
- `500 Internal Server Error` - проверьте API ключ Perplexity и интернет-соединение

### Тест 5: Получение писем по вакансии

**Endpoint:** `GET /api/CoverLetters/by-vacancy/{vacancyId}`

1. Найдите `GET /api/CoverLetters/by-vacancy/{vacancyId}`
2. Введите `vacancyId` из шага 1
3. Нажмите **"Execute"**

**Ожидаемый результат:**
- Status: `200 OK`
- Массив писем для этой вакансии

### Тест 6: Обновление письма

**Endpoint:** `PUT /api/CoverLetters/{id}`

1. Найдите `PUT /api/CoverLetters/{id}`
2. Введите `id` письма из шага 4
3. Вставьте JSON с обновленными данными:
```json
{
  "id": 1,
  "vacancyId": 1,
  "vacancyTitle": "Разработчик .NET",
  "candidateName": "Иван Иванов",
  "candidateContacts": "ivan@example.com",
  "letterText": "Обновленный текст письма...",
  "createdAt": "2024-12-10T12:00:00Z",
  "updatedAt": null
}
```
4. Нажмите **"Execute"**

**Ожидаемый результат:**
- Status: `204 No Content`

### Тест 7: Удаление вакансии

**Endpoint:** `DELETE /api/Vacancies/{id}`

1. Найдите `DELETE /api/Vacancies/{id}`
2. Введите `id` вакансии
3. Нажмите **"Execute"**

**Ожидаемый результат:**
- Status: `204 No Content`

⚠️ **Внимание:** При удалении вакансии также удалятся все связанные письма (каскадное удаление).

## Шаг 4: Тестирование через HTTP файл (Visual Studio Code)

В файле `JobAI.Server.http` есть примеры запросов. Вы можете использовать расширение REST Client для VS Code.

## Проверка логов

Сервер выводит логи в консоль. Обратите внимание на:
- ✅ Сообщения о подключении к базе данных
- ⚠️ Предупреждения о валидации
- ❌ Ошибки при вызове Perplexity API
- ❌ Ошибки базы данных

## Типичные проблемы

### Проблема: "Cannot connect to database"
**Решение:**
```powershell
# Проверьте, что LocalDB запущен
sqllocaldb info MSSQLLocalDB
sqllocaldb start MSSQLLocalDB

# Примените миграции
cd JobAI.Server
dotnet ef database update
```

### Проблема: "Perplexity API error: 401 Unauthorized"
**Решение:**
- Проверьте API ключ в `appsettings.json`
- Убедитесь, что ключ активен и имеет баланс

### Проблема: "404 Not Found" для endpoints
**Решение:**
- Убедитесь, что контроллеры в папке `Controllers` (не `Constrollers`)
- Проверьте регистр в URL: `api/CoverLetters` (с заглавной C и L)

### Проблема: Swagger не открывается
**Решение:**
- Убедитесь, что переменная окружения `ASPNETCORE_ENVIRONMENT=Development`
- Проверьте, что сервер запущен на правильном порту (5143)


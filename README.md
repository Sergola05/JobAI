# JobAI - Система генерации сопроводительных писем

## Быстрый старт

### 1. Установка зависимостей

- **.NET 8.0 SDK** - [Скачать](https://dotnet.microsoft.com/download)
- **SQL Server LocalDB** (входит в Visual Studio) или SQL Server Express
- **.NET Framework 4.8** (для клиентского приложения)
- **API ключ Perplexity AI** - [Получить](https://www.perplexity.ai/)


### 2. Настройка базы данных

```powershell
cd "Исходные файлы программного продукта\Серверное приложение\JobAI.Server"
dotnet tool install --global dotnet-ef
dotnet ef database update
```

### 3. Настройка API ключа

Откройте `Исходные файлы программного продукта\Серверное приложение\JobAI.Server\appsettings.json` и добавьте ваш API ключ:

```json
{
  "Perplexity": {
    "ApiKey": "ваш-api-ключ",
    "Model": "sonar-pro"
  }
}
```

### 4. Запуск сервера

```powershell
cd "Исходные файлы программного продукта\Серверное приложение\JobAI.Server"
dotnet run
```

Сервер будет доступен на: http://localhost:5143

### 5. Запуск клиента

**Через Visual Studio:**
1. Откройте `JobAI.sln`
2. Установите `JobAI.Client.WPF` как стартовый проект
3. Нажмите F5

**Или запустите .exe:**
```
Исходные файлы программного продукта\Клиентское приложение\JobAI.Client.WPF\bin\Debug\JobAI.Client.WPF.exe
```

## Структура проекта

```
JobAI/
├── Исходные файлы программного продукта/
│   ├── Серверное приложение/
│   │   └── JobAI.Server/          # ASP.NET Core API сервер
│   ├── Клиентское приложение/
│   │   └── JobAI.Client.WPF/      # WPF клиентское приложение
│   └── Вспомогательные пользовательские динамические библиотеки/
│       └── JobAI.Shared/          # Общие модели данных
└── JobAI.sln
```

## Скриншоты
> - Скрин 1 — главное окно приложения
> - <img width="1461" height="812" alt="image" src="https://github.com/user-attachments/assets/6e6f9f99-0718-4af5-b9b2-8c938cc61725" />

> - Скрин 2 — Форма заполнения вакансии
> -  <img width="717" height="528" alt="image" src="https://github.com/user-attachments/assets/3185f1a3-3856-4638-99bd-382aae432ba7" />

> - Скрин 3 — Форма заполнения резюме
> - <img width="599" height="610" alt="image" src="https://github.com/user-attachments/assets/d55f5a17-9195-4ac5-a2c7-a5044e4c31ac" />


> - Скрин 4 — сгенерированное сопроводительное письмо
> - <img width="871" height="767" alt="image" src="https://github.com/user-attachments/assets/853c7778-8ef2-46d5-80a8-0f590be66845" />



## Основные функции

- ✅ Управление вакансиями (создание, просмотр, удаление)
- ✅ Автоматическая генерация сопроводительных писем через AI
- ✅ Редактирование и сохранение писем
- ✅ История писем по каждой вакансии

## Технологии

- **Backend:** ASP.NET Core 8.0, Entity Framework Core, SQL Server
- **Frontend:** WPF (.NET Framework 4.8)
- **AI:** Perplexity AI API
- **API Documentation:** Swagger/OpenAPI

## Порты

- **Сервер:** http://localhost:5143
- **Swagger UI:** http://localhost:5143/swagger




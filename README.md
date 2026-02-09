# TestTaskSMS

Это тестовое задание для вакансии.

**Архитектура**
Проект разделен на слои: абстракции + реализации клиента + демо + тесты.
Ключевая идея — общий контракт `IApiClient`, который имеет разные реализации (HTTP и gRPC).

**Состав решения**
- `FoodServerClient.Abstractions` — контракт `IApiClient` и доменные модели `MenuItem`, `Order`, `OrderItem`.
- `FoodServerClient.Http` — HTTP‑клиент `ApiClient`, DTO/транспортные модели и мапперы.
- `FoodServerClient.Grpc` — gRPC‑клиент `ApiClient`, мапперы и proto‑контракт.
- `FoodServerClient.ConsoleApp` — консольное приложение: получает меню, сохраняет в PostgreSQL и отправляет заказ.
- `FoodServerClient.WpfApp` — WPF‑утилита для редактирования набора переменных окружения.
- `FoodServerClient.StubServer` — простой заглушечный сервер для локальных проверок.
- `Tests/FoodServerClient.Tests` — юнит‑тесты для HTTP и gRPC клиентов.

**Как запустить**
1. Запустить `FoodServerClient.StubServer` (локальный HTTP API по `http://localhost:5057/api`).
2. Запустить `FoodServerClient.ConsoleApp` (конфиги в `FoodServerClient.ConsoleApp/appsettings.json`).
3. Для редактирования переменных окружения запустить `FoodServerClient.WpfApp`.
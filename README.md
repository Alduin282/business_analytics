# BusinessAnalytics

Система для аналитики бизнес-данных. Построена на стеке .NET 10 (Backend) и React + Vite (Frontend).

## Структура проекта

- `BusinessAnalytics.API` — Backend на ASP.NET Core 10.
- `business-analytics-ui` — Frontend на React.

## Как запустить проект

### 1. Запуск Backend (API)
Перейдите в папку с API и запустите в режиме отладки:

```bash
cd OrderAnalytics.API
dotnet watch run
```

- **Swagger UI**: [http://localhost:5014/swagger](http://localhost:5014/swagger) — здесь можно тестировать API вручную.
- **База данных**: Используется SQLite. Файл БД — `business.db`.

### 2. Запуск Frontend (UI)
Перейдите в папку фронтенда, установите зависимости (если еще не сделано) и запустите dev-сервер:

```bash
cd business-analytics-ui
npm install
npm run dev
```

- **URL**: По умолчанию фронтенд доступен по адресу [http://localhost:5173](http://localhost:5173).

## Решение проблем

### Ошибка Rollup на Windows (ERR_DLOPEN_FAILED)
Если при запуске фронтенда возникает ошибка `Cannot find module @rollup/rollup-win32-x64-msvc`, выполните:

```bash
rm -rf node_modules package-lock.json
npm install
```

### Доверие SSL-сертификату
Если браузер блокирует запросы к API по HTTPS, выполните в терминале:
```bash
dotnet dev-certs https --trust
```

## Технологии
- **Backend**: .NET 10, EF Core, SQLite, Identity + JWT, Swagger, Repository + Unit of Work patterns.
- **Frontend**: React (Vite), Axios, Vanilla CSS (Glassmorphism design).

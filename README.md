# BusinessAnalytics

Система для аналитики бизнес-данных. Построена на стеке .NET 10 (Backend) и React + Vite (Frontend).

## Основные возможности
- **Аналитика продаж**: Группировка данных по дням, неделям и месяцам.
- **Гибкие стратегии**: Использование паттерна "Стратегия" для расчета различных метрик (выручка, количество заказов и т.д.).
- **Современный UI**: Интерактивные графики с возможностью выбора периода.
- **Архитектура**: Применение паттернов Repository, Unit of Work и Strategy для чистоты и расширяемости кода.

## Структура проекта

- `BusinessAnalytics.API` — Backend на ASP.NET Core 10.
- `business-analytics-ui` — Frontend на React.

## Как запустить проект

### 1. Запуск Backend (API)
Перейдите в папку с API и запустите:

```bash
cd BusinessAnalytics.API
dotnet watch run
```

- **Swagger UI**: [http://localhost:5014/swagger](http://localhost:5014/swagger) — здесь можно тестировать API вручную.
- **База данных**: Используется SQLite. Файл БД — `business.db`.

### 2. Запуск Frontend (UI)
Перейдите в папку фронтенда, установите зависимости и запустите dev-сервер:

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
- **Backend**: .NET 10, EF Core, SQLite, Identity + JWT, Swagger, Repository + Unit of Work patterns, Strategy pattern.
- **Frontend**: React (Vite), Recharts (для графиков), Axios, Vanilla CSS (Glassmorphism design).

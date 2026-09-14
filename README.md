# Telegram News Digest Bot

Telegram-бот для создания новостных дайджестов из каналов.

## Архитектура проекта

### Проекты

- **NewsDigest.App** — точка входа, DI, конфигурация, hosted services
- **NewsDigest.Core** — домен, интерфейсы, модели, бизнес-логика
- **NewsDigest.Infrastructure** — WTelegram, Telegram.Bot, OllamaSharp, EF Core, файловое хранилище

## Первая настройка

1. Запустите Ollama, скачайте модель: `ollama pull <model>`
2. Заполните User Secrets:
   ```
   dotnet user-secrets init
   dotnet user-secrets set "Telegram:ApiId" "..."
   dotnet user-secrets set "Telegram:ApiHash" "..."
   dotnet user-secrets set "Telegram:PhoneNumber" "..."
   dotnet user-secrets set "Telegram:SessionPath" "..."
   dotnet user-secrets set "Telegram:BotToken" "..."
   dotnet user-secrets set "Ollama:Url" "http://localhost:11434"
   dotnet user-secrets set "Ollama:Model" "llama3"
   dotnet user-secrets set "Storage:DatabasePath" "news-digest.db"
   dotnet user-secrets set "Storage:BatchDirectory" "batches"
   ```
3. Запустите приложение. В консоли введите код из Telegram и пароль 2FA (если есть).
4. После успешного логина сессия сохранится, при следующих запусках код не потребуется.
5. Напишите боту /start, добавьте каналы через /add.

## Команды бота

- `/start` — краткая инструкция
- `/add <@username|t.me/...|id>` — добавить канал пользователю
- `/remove <@username|id>` — удалить канал
- `/list` — список каналов пользователя
- `/news` — запустить сбор, сформировать дайджест, создать очередь
- `/next` — прислать следующую новость из очереди
- `/status` — статус очереди

## Требования

- .NET 10 SDK
- Ollama установлен и запущен
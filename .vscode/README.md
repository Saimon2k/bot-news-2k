# 🛠️ VSCode Configuration для NewsDigest

## 📋 Обзор

Этот каталог содержит настройки VSCode для удобной разработки, отладки и запуска проекта NewsDigest.

---

## 🚀 Конфигурации запуска (launch.json)

### 1. **🚀 Launch NewsDigest**
Основной режим запуска приложения с интегрированной консолью.

**Использование:**
- Нажмите `F5` или выберите в панели отладки
- Консоль откроется в интегрированном терминале VSCode
- Логи будут отображаться в реальном времени

**Когда использовать:** Для обычного запуска бота без глубокой отладки.

---

### 2. **🐛 Debug NewsDigest**
Режим отладки с расширенными возможностями.

**Использование:**
- Установите точки останова (breakpoints)
- Запустите через панель отладки
- Используйте Step Into (F11), Step Over (F10) для пошагового выполнения

**Особенности:**
- `justMyCode: true` — пропускает код фреймворков
- `suppressJITOptimizations: true` — отключает оптимизации для точной отладки
- `enableStepFiltering: false` — позволяет заходить в любой код

**Когда использовать:** Для поиска багов и анализа логики работы.

---

### 3. **🔧 Attach to NewsDigest**
Подключение к уже запущенному процессу.

**Использование:**
1. Запустите приложение вручную: `dotnet run --project NewsDigest.App`
2. Выберите конфигурацию "Attach to NewsDigest"
3. VSCode подключится к работающему процессу

**Когда использовать:** Для отладки уже запущенного приложения без перезапуска.

---

## 🔨 Задачи (tasks.json)

### Основные задачи

#### **build** (По умолчанию)
```
Ctrl+Shift+B
```
Сборка проекта NewsDigest.App. Используется как preLaunchTask.

#### **run**
Быстрый запуск приложения без отладчика:
```bash
dotnet run --project NewsDigest.App
```

#### **watch**
Hot-reload режим — автоматический перезапуск при изменении кода:
```bash
dotnet watch run --project NewsDigest.App
```

#### **test**
Запуск всех тестов в проекте NewsDigest.Tests:
```
Ctrl+Shift+T (по умолчанию)
```

---

### Дополнительные задачи

| Задача | Описание |
|--------|----------|
| `clean` | Очистка артефактов сборки (bin/obj) |
| `rebuild` | Полная пересборка (clean + build) |
| `restore` | Восстановление NuGet-пакетов |
| `clean-build` | Последовательное выполнение clean → build |

**Запуск задачи:**
1. `Ctrl+Shift+P` → "Tasks: Run Task"
2. Выберите нужную задачу из списка

---

## ⚙️ Настройки (settings.json)

### Форматирование
- **Автоформатирование при сохранении** (`Ctrl+S`)
- **Автоорганизация using-директив**
- **Отступы:** 4 пробела (стандарт C#)
- **Линейка:** 120 символов

### Исключения из поиска
Папки `bin/`, `obj/`, `.vs/` скрыты из проводника и поиска для ускорения работы.

### Автосохранение
Файлы сохраняются автоматически через 1 секунду после изменения.

### OmniSharp (C# Language Server)
- Включены Roslyn-анализаторы
- Поддержка `.editorconfig`
- IntelliSense для импортов

---

## 🎯 Быстрые команды

| Действие | Комбинация |
|----------|-----------|
| Запустить с отладкой | `F5` |
| Запустить без отладки | `Ctrl+F5` |
| Сборка проекта | `Ctrl+Shift+B` |
| Открыть панель задач | `Ctrl+Shift+P` → "Tasks: Run Task" |
| Остановить отладку | `Shift+F5` |
| Перезапустить отладку | `Ctrl+Shift+F5` |
| Установить breakpoint | `F9` |
| Step Over | `F10` |
| Step Into | `F11` |
| Step Out | `Shift+F11` |

---

## 🐛 Отладка типичных проблем

### Проблема: "Cannot find project file"
**Решение:** Убедитесь, что путь к `.csproj` корректен:
```json
"program": "${workspaceFolder}/NewsDigest.App/bin/Debug/net10.0/NewsDigest.App.dll"
```

### Проблема: "Breakpoint не срабатывает"
**Решение:** 
1. Проверьте, что используете конфигурацию "Debug NewsDigest"
2. Убедитесь, что код не оптимизирован (Debug mode)
3. Пересоберите проект: `Ctrl+Shift+B`

### Проблема: "Приложение не запускается"
**Решение:**
1. Проверьте наличие секретов: `dotnet user-secrets list --project NewsDigest.App`
2. Убедитесь, что установлен .NET 10.0: `dotnet --version`
3. Восстановите пакеты: выполните задачу `restore`

---

## 📦 Расширения VSCode (рекомендуемые)

Для полной поддержки добавьте:
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit",
    "formulahendry.dotnet-test-explorer",
    "jchannon.csharpextensions",
    "kreativ-software.csharpextensions"
  ]
}
```

Сохраните в `.vscode/extensions.json` для автоматических рекомендаций команде.

---

## 🔒 Безопасность

**Секреты и конфиденциальные данные:**
- Используйте `dotnet user-secrets` для хранения токенов
- Файл `.secrets.toml` добавлен в `.gitignore`
- Не храните пароли в `appsettings.json`

**Проверка секретов:**
```bash
dotnet user-secrets list --project NewsDigest.App
```

---

## 📚 Дополнительные ресурсы

- [VSCode C# Debugging](https://code.visualstudio.com/docs/languages/csharp)
- [Tasks in VSCode](https://code.visualstudio.com/docs/editor/tasks)
- [Launch Configurations](https://code.visualstudio.com/docs/editor/debugging)

---

**Версия:** 1.0  
**Последнее обновление:** 2026-10-13

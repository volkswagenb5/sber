# Перенос проекта sberbank на новый компьютер

## Проверенная совместимость

Проект `sberbank` - классический WPF-проект на C# для `.NET Framework 4.8`.

Подходит для нового компьютера:

- Visual Studio Community 2022 17.7.6.
- .NET Framework 4.8.09037.
- SQL Server Express `WS105305\SQLEXPRESS`.
- SSMS 18.9.1.
- SQL Server 15.0.2000.

В проекте не найдено Entity Framework, EDMX-модели, `packages.config` или `PackageReference`. Доступ к базе сделан через ADO.NET: `System.Data.SqlClient`, `SqlConnection`, `SqlCommand`, `SqlDataAdapter`.

## Что копировать

Скопируйте всю папку проекта:

```text
sberbank
```

Минимально важные файлы и папки:

- `sberbank.csproj`
- `App.config`
- `App.xaml`, `App.xaml.cs`
- `Model`
- `Pages`
- `Services`
- `Themes`
- `View`
- `Properties`
- `Sql`
- `README.txt`
- `TRANSFER_TO_NEW_PC.md`

Папки `bin` и `obj` можно не переносить: Visual Studio создаст их заново при сборке.

## Подключение к базе

`App.config` подключает отдельный файл `ConnectionStrings.config`. Основной вариант под новый компьютер уже прописан в `ConnectionStrings.config`:

```xml
connectionString="Data Source=WS105305\SQLEXPRESS;Initial Catalog=SberbankDB;User ID=SA;Password=PUT_SA_PASSWORD_HERE;Encrypt=False;TrustServerCertificate=True"
```

Перед запуском замените `PUT_SA_PASSWORD_HERE` в `ConnectionStrings.config` на пароль пользователя `SA`.

Если пароль содержит специальные XML-символы, экранируйте их:

- `&` -> `&amp;`
- `<` -> `&lt;`
- `>` -> `&gt;`
- `"` -> `&quot;`

Альтернативный вариант через Windows Authentication:

```xml
connectionString="Data Source=WS105305\SQLEXPRESS;Initial Catalog=SberbankDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True"
```

Windows Authentication обычно безопаснее, потому что пароль не хранится в `App.config`. Но для вашей целевой машины основной вариант оставлен через SQL Server Authentication и `SA`.

## Создание базы в SSMS 18.9.1

1. Откройте SQL Server Management Studio.
2. Подключитесь к серверу `WS105305\SQLEXPRESS`.
3. Выберите `SQL Server Authentication`.
4. Login: `SA`.
5. Password: введите вручную.
6. Откройте файл `Sql\SberbankDB_SchemaAndData.sql`.
7. Нажмите `Execute`.

Скрипт полностью создает базу `SberbankDB`:

- таблицы;
- primary keys;
- foreign keys;
- default constraints;
- индексы;
- тестовые данные.

Важно: в начале скрипта есть блок, который удаляет существующую базу `SberbankDB`, если она уже есть. Если удалять базу нельзя, уберите из скрипта блок `IF DB_ID ... DROP DATABASE`.

## Проверка базы после выполнения скрипта

В SSMS выполните:

```sql
USE SberbankDB;
GO

SELECT ProductId, Name, Rate, ServiceCost
FROM BankProducts
ORDER BY ProductId;
GO

SELECT COUNT(*) AS ZeroProductValues
FROM BankProducts
WHERE Name IN
(
    N'Дебетовая карта СберКарта',
    N'Кредитная карта 120 дней',
    N'Потребительский кредит',
    N'Вклад СберВклад',
    N'Расчетный счет для бизнеса'
)
AND (Rate = 0 OR ServiceCost = 0);
GO
```

`ZeroProductValues` должен быть `0`.

Если база уже создана и нужно только обновить тестовые значения услуг, выполните `Sql\UpdateBankProductsTestValues.sql`.

## Открытие проекта в Visual Studio

В папке проекта сейчас нет файла `.sln`, поэтому открывайте проект так:

1. Запустите Visual Studio 2022.
2. Выберите `Open a project or solution`.
3. Откройте `sberbank.csproj`.
4. Убедитесь, что выбран target framework `.NET Framework 4.8`.
5. Выполните `Build -> Build sberbank`.
6. Запустите проект через `Start`.

NuGet-пакеты восстанавливать не нужно: проект использует только стандартные сборки .NET Framework и WPF.

## Проверка приложения

1. Войдите тестовым пользователем:
   - `Admin` / `Admin1!`
   - `ivanov` / `User1!`
   - `petrova` / `Client2!`
2. Откройте вкладку `Услуги`.
3. Проверьте, что услуги загружаются из базы.
4. Нажмите `Оставить заявку` для любой услуги.
5. Подтвердите создание заявки.
6. Проверьте вкладку заявок пользователя или админ-панель.

## Entity Framework / EDMX

В текущем проекте Entity Framework и EDMX не используются.

Обновлять `.edmx` не нужно, потому что его нет. Строка подключения хранится в `ConnectionStrings.config`, который подключен из `App.config`, а запросы находятся в `Services\DatabaseService.cs`.

Если позже добавите EDMX-модель, после смены сервера нужно будет:

1. Открыть `.edmx`.
2. Правой кнопкой по дизайнеру выбрать `Update Model from Database`.
3. Создать или выбрать подключение к `WS105305\SQLEXPRESS`.
4. Проверить EntityClient connection string в `App.config`.
5. Пересобрать проект.

## Частые проблемы

### Не подключается к SQL Server

Проверьте:

- имя сервера ровно `WS105305\SQLEXPRESS`;
- SQL Server service запущен;
- SQL Server Browser запущен, если instance не находится;
- включен SQL Server Authentication или Mixed Mode;
- пользователь `SA` включен;
- пароль `SA` верный;
- в `ConnectionStrings.config` заменен `PUT_SA_PASSWORD_HERE`.

### Ошибка SSL / certificate / Encrypt

Для этого проекта в строке подключения указано:

```text
Encrypt=False;TrustServerCertificate=True
```

Это самый простой вариант для учебного локального SQL Server Express. Для промышленной среды лучше настроить нормальный сертификат и включить шифрование.

### Ошибка .NET Framework

Нужен `.NET Framework 4.8` и желательно `.NET Framework 4.8 Developer Pack`. Если проект не открывается или не собирается, установите workload Visual Studio:

- `.NET desktop development`

### Ошибка версии Visual Studio

Проект старого формата `ToolsVersion="15.0"`, но Visual Studio 2022 17.7.6 его поддерживает. Если нет `.sln`, открывайте напрямую `sberbank.csproj`.

### Ошибка версии SSMS

SSMS 18.9.1 подходит для выполнения скрипта. Скрипт не использует функций, требующих более новой версии SSMS.

### База уже существует

Скрипт `SberbankDB_SchemaAndData.sql` пересоздает базу. Если нужно сохранить данные, не запускайте полный скрипт. Используйте backup или отдельные `UPDATE`/`ALTER` скрипты.

## Что нужно вручную

Не хватает только пароля от `SA`. Его нельзя и не нужно хранить в репозитории.

Если на новом компьютере будет другое имя SQL Server instance, замените `Data Source` в `ConnectionStrings.config`.

Проект: sberbank
Тема: Проектирование и разработка WPF .NET приложения ООО "Сбербанк"

Среда:
- Visual Studio Community 2022
- WPF .NET Framework
- Целевая версия проекта: .NET Framework 4.8
- База данных: SQL Server / SQL Server Management Studio

Что реализовано:
- Окна: AuthorizationWindow, RegistrationWindow, MainWindow, AdminWindow.
- Страницы: ProfilePage, ServicesPage, ApplicationsPage, SelectedServicesPage.
- Папки: View, Pages, Model, Services, Themes, Resources, Sql.
- Тема Themes/SberbankTheme.xaml в зеленом стиле Сбербанка.
- Авторизация по Users.Login и Users.Password.
- Разделение администратора и клиента по Roles.
- Регистрация клиента с проверкой пароля.
- Просмотр и изменение профиля.
- Просмотр банковских продуктов, фильтрация по типу, сортировка по стоимости.
- Создание заявки на банковскую услугу.
- Просмотр заявок текущего пользователя.
- Админ-панель: пользователи, клиенты, услуги, заявки, поиск, сохранение изменений, удаление, CSV-экспорт, смена статуса заявки.

Как создать базу в SSMS:
1. Откройте SQL Server Management Studio.
2. Подключитесь к Database Engine.
3. Откройте файл Sql/SberbankDB_SchemaAndData.sql.
4. Нажмите Execute / Выполнить.
5. Скрипт создаст базу SberbankDB, таблицы, связи и тестовые данные.

Важно:
- Скрипт удаляет существующую базу SberbankDB, если она уже есть.
- Если удалять базу нельзя, уберите из начала скрипта блок IF DB_ID ... DROP DATABASE.

Тестовые пользователи:
- Admin / Admin1! - администратор.
- ivanov / User1! - клиент.
- petrova / Client2! - клиент.

Строка подключения:
Файл ConnectionStrings.config содержит строку:
Data Source=WS105305\SQLEXPRESS;Initial Catalog=SberbankDB;User ID=SA;Password=PUT_SA_PASSWORD_HERE;Encrypt=False;TrustServerCertificate=True

Если в SSMS имя сервера другое:
1. Откройте ConnectionStrings.config.
2. В connectionStrings измените Data Source.
3. Примеры:
   Data Source=WS105305\SQLEXPRESS;Initial Catalog=SberbankDB;User ID=SA;Password=PUT_SA_PASSWORD_HERE;Encrypt=False;TrustServerCertificate=True
   Data Source=.\SQLEXPRESS;Initial Catalog=SberbankDB;Integrated Security=True;TrustServerCertificate=True
   Data Source=localhost;Initial Catalog=SberbankDB;Integrated Security=True;TrustServerCertificate=True
   Data Source=YOUR_SERVER;Initial Catalog=SberbankDB;User ID=SA;Password=your_password;Encrypt=False;TrustServerCertificate=True

Если используете ADO.NET Entity Data Model:
1. В Visual Studio: Add -> New Item -> ADO.NET Entity Data Model.
2. Выберите EF Designer from database.
3. Создайте подключение к SberbankDB.
4. Выберите таблицы Roles, Users, Clients, Employees, Departments, BankProducts, Applications, ApplicationStatuses, Payments.
5. Если модель лежит в папке Model, проверьте namespace и строку metadata в App.config.
В этом проекте выбран более простой способ: ADO.NET SqlConnection/SqlCommand без EDMX.

Excel-таблицы для импорта:
- Users.xlsx: Login, Password, RoleId, IsActive.
- Clients.xlsx: UserId, FullName, PassportNumber, Phone, Email, Address.
- BankProducts.xlsx: Name, ProductType, Description, Rate, ServiceCost, IsActive.
- Applications.xlsx: ClientId, ProductId, StatusId, EmployeeId, CreatedAt, Comment.

Импорт Excel через SSMS:
1. Закройте Excel-файл перед импортом.
2. В SSMS нажмите правой кнопкой по SberbankDB.
3. Tasks -> Import Data.
4. Источник: Microsoft Excel.
5. Выберите файл .xlsx.
6. Назначение: SQL Server / SQL Server Native Client или доступный SQL Server provider.
7. Выберите базу SberbankDB.
8. Сопоставьте колонки с нужными таблицами.
9. Выполните импорт и обновите базу в Object Explorer.

Частые ошибки импорта Excel:
- Excel-файл открыт: закройте файл.
- Ошибка формата: пересохраните как Excel 97-2003 или обычный .xlsx.
- Не совпадают типы колонок: проверьте даты, числа, пустые значения и длину текста.
- Нарушение внешних ключей: сначала импортируйте справочники Roles, ApplicationStatuses, BankProducts, затем Users, Clients, Applications.
- Дубли логинов: Users.Login должен быть уникальным.

Генерация SQL-скрипта базы в SSMS:
1. Правой кнопкой по базе SberbankDB.
2. Tasks -> Generate Scripts.
3. Выберите объекты или всю базу.
4. Advanced -> Types of data to script -> Schema and Data.
5. Save as script file.
6. На другом компьютере откройте .sql файл в SSMS и нажмите Execute.
7. Если база с таким именем уже существует, удалите ее или измените имя базы в скрипте.

Запуск проекта:
1. Сначала выполните Sql/SberbankDB_SchemaAndData.sql в SSMS.
2. Проверьте строку подключения в ConnectionStrings.config.
3. Откройте sberbank.csproj или решение в Visual Studio.
4. Убедитесь, что установлен .NET Framework 4.8 Developer Pack.
5. Запустите проект.

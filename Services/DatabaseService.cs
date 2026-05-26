using sberbank.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace sberbank.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SberbankDb"].ConnectionString;
        }

        public User GetUserByLogin(string login)
        {
            const string sql = @"
SELECT TOP 1 u.UserId, u.Login, u.Password, u.RoleId, r.Name AS RoleName,
       c.ClientId, c.FullName, c.Phone, c.Email
FROM Users u
INNER JOIN Roles r ON r.RoleId = u.RoleId
LEFT JOIN Clients c ON c.UserId = u.UserId
WHERE u.Login = @Login";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Login", login);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return MapUser(reader);
                }
            }
        }

        public User GetUserById(int userId)
        {
            const string sql = @"
SELECT TOP 1 u.UserId, u.Login, u.Password, u.RoleId, r.Name AS RoleName,
       c.ClientId, c.FullName, c.Phone, c.Email
FROM Users u
INNER JOIN Roles r ON r.RoleId = u.RoleId
LEFT JOIN Clients c ON c.UserId = u.UserId
WHERE u.UserId = @UserId";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() ? MapUser(reader) : null;
                }
            }
        }

        public bool LoginExists(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Login = @Login", connection))
            {
                command.Parameters.AddWithValue("@Login", login);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public void RegisterUser(string login, string password, string fullName, string phone, string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int roleId;
                        using (var roleCommand = new SqlCommand("SELECT RoleId FROM Roles WHERE Name = N'Client'", connection, transaction))
                        {
                            roleId = Convert.ToInt32(roleCommand.ExecuteScalar());
                        }

                        int userId;
                        using (var userCommand = new SqlCommand(
                            "INSERT INTO Users(Login, Password, RoleId, IsActive) OUTPUT INSERTED.UserId VALUES(@Login, @Password, @RoleId, 1)",
                            connection,
                            transaction))
                        {
                            userCommand.Parameters.AddWithValue("@Login", login);
                            userCommand.Parameters.AddWithValue("@Password", password);
                            userCommand.Parameters.AddWithValue("@RoleId", roleId);
                            userId = Convert.ToInt32(userCommand.ExecuteScalar());
                        }

                        using (var clientCommand = new SqlCommand(
                            "INSERT INTO Clients(UserId, FullName, Phone, Email) VALUES(@UserId, @FullName, @Phone, @Email)",
                            connection,
                            transaction))
                        {
                            clientCommand.Parameters.AddWithValue("@UserId", userId);
                            clientCommand.Parameters.AddWithValue("@FullName", string.IsNullOrWhiteSpace(fullName) ? (object)DBNull.Value : fullName);
                            clientCommand.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                            clientCommand.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                            clientCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateProfile(int userId, string login, string password, string fullName, string phone, string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var userCommand = new SqlCommand("UPDATE Users SET Login = @Login, Password = @Password WHERE UserId = @UserId", connection, transaction))
                    {
                        userCommand.Parameters.AddWithValue("@UserId", userId);
                        userCommand.Parameters.AddWithValue("@Login", login);
                        userCommand.Parameters.AddWithValue("@Password", password);
                        userCommand.ExecuteNonQuery();
                    }

                    using (var clientCommand = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM Clients WHERE UserId = @UserId)
    UPDATE Clients SET FullName = @FullName, Phone = @Phone, Email = @Email WHERE UserId = @UserId
ELSE
    INSERT INTO Clients(UserId, FullName, Phone, Email) VALUES(@UserId, @FullName, @Phone, @Email)", connection, transaction))
                    {
                        clientCommand.Parameters.AddWithValue("@UserId", userId);
                        clientCommand.Parameters.AddWithValue("@FullName", string.IsNullOrWhiteSpace(fullName) ? (object)DBNull.Value : fullName);
                        clientCommand.Parameters.AddWithValue("@Phone", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                        clientCommand.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                        clientCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public List<BankProduct> GetBankProducts(string productType, bool sortByCost)
        {
            var result = new List<BankProduct>();
            var sql = @"
SELECT ProductId, Name, ProductType, Description, Rate, ServiceCost
FROM BankProducts
WHERE IsActive = 1 AND (@ProductType IS NULL OR ProductType = @ProductType)
ORDER BY " + (sortByCost ? "ServiceCost, Name" : "Name");

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ProductType", string.IsNullOrWhiteSpace(productType) ? (object)DBNull.Value : productType);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new BankProduct
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            Name = Convert.ToString(reader["Name"]),
                            ProductType = Convert.ToString(reader["ProductType"]),
                            Description = Convert.ToString(reader["Description"]),
                            Rate = Convert.ToDecimal(reader["Rate"]),
                            ServiceCost = Convert.ToDecimal(reader["ServiceCost"])
                        });
                    }
                }
            }

            return result;
        }

        public List<string> GetProductTypes()
        {
            var result = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SELECT DISTINCT ProductType FROM BankProducts WHERE IsActive = 1 ORDER BY ProductType", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(Convert.ToString(reader[0]));
                    }
                }
            }
            return result;
        }

        public void CreateApplication(int userId, int productId, string comment)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
DECLARE @ClientId INT = (SELECT ClientId FROM Clients WHERE UserId = @UserId);
DECLARE @StatusId INT = (SELECT StatusId FROM ApplicationStatuses WHERE Name = N'Новая');
INSERT INTO Applications(ClientId, ProductId, StatusId, CreatedAt, Comment)
VALUES(@ClientId, @ProductId, @StatusId, GETDATE(), @Comment)", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@ProductId", productId);
                command.Parameters.AddWithValue("@Comment", string.IsNullOrWhiteSpace(comment) ? (object)DBNull.Value : comment);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<ApplicationInfo> GetApplicationsForUser(int userId)
        {
            var result = new List<ApplicationInfo>();
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(@"
SELECT a.ApplicationId, bp.Name AS ProductName, bp.ProductType, s.Name AS StatusName, a.CreatedAt, a.Comment
FROM Applications a
INNER JOIN Clients c ON c.ClientId = a.ClientId
INNER JOIN BankProducts bp ON bp.ProductId = a.ProductId
INNER JOIN ApplicationStatuses s ON s.StatusId = a.StatusId
WHERE c.UserId = @UserId
ORDER BY a.CreatedAt DESC", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ApplicationInfo
                        {
                            ApplicationId = Convert.ToInt32(reader["ApplicationId"]),
                            ProductName = Convert.ToString(reader["ProductName"]),
                            ProductType = Convert.ToString(reader["ProductType"]),
                            StatusName = Convert.ToString(reader["StatusName"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            Comment = Convert.ToString(reader["Comment"])
                        });
                    }
                }
            }
            return result;
        }

        public DataTable GetTable(string tableName, string searchText)
        {
            string sql;
            if (tableName == "Users")
            {
                sql = @"
SELECT u.UserId, u.Login, u.Password, u.RoleId, r.Name AS RoleName, u.IsActive
FROM Users u
INNER JOIN Roles r ON r.RoleId = u.RoleId
WHERE @Search = '' OR u.Login LIKE @LikeSearch OR r.Name LIKE @LikeSearch";
            }
            else if (tableName == "BankProducts")
            {
                sql = @"
SELECT ProductId, Name, ProductType, Description, Rate, ServiceCost, IsActive
FROM BankProducts
WHERE @Search = '' OR Name LIKE @LikeSearch OR ProductType LIKE @LikeSearch OR Description LIKE @LikeSearch";
            }
            else if (tableName == "Clients")
            {
                sql = @"
SELECT ClientId, UserId, FullName, PassportNumber, Phone, Email, Address, CreatedAt
FROM Clients
WHERE @Search = '' OR FullName LIKE @LikeSearch OR Phone LIKE @LikeSearch OR Email LIKE @LikeSearch OR PassportNumber LIKE @LikeSearch";
            }
            else if (tableName == "Applications")
            {
                sql = @"
SELECT a.ApplicationId, c.FullName, bp.Name AS ProductName, bp.ProductType, s.Name AS StatusName,
       a.StatusId, a.CreatedAt, a.Comment
FROM Applications a
INNER JOIN Clients c ON c.ClientId = a.ClientId
INNER JOIN BankProducts bp ON bp.ProductId = a.ProductId
INNER JOIN ApplicationStatuses s ON s.StatusId = a.StatusId
WHERE @Search = '' OR c.FullName LIKE @LikeSearch OR bp.Name LIKE @LikeSearch OR s.Name LIKE @LikeSearch";
            }
            else
            {
                throw new ArgumentException("Неизвестная таблица.");
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter(sql, connection))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@Search", searchText ?? string.Empty);
                adapter.SelectCommand.Parameters.AddWithValue("@LikeSearch", "%" + (searchText ?? string.Empty) + "%");
                var table = new DataTable(tableName);
                adapter.Fill(table);
                return table;
            }
        }

        public void SaveUsers(DataTable table)
        {
            SaveEditableTable(table, "SELECT UserId, Login, Password, RoleId, IsActive FROM Users");
        }

        public void SaveProducts(DataTable table)
        {
            SaveEditableTable(table, "SELECT ProductId, Name, ProductType, Description, Rate, ServiceCost, IsActive FROM BankProducts");
        }

        public void SaveClients(DataTable table)
        {
            SaveEditableTable(table, "SELECT ClientId, UserId, FullName, PassportNumber, Phone, Email, Address, CreatedAt FROM Clients");
        }

        public DataTable GetStatuses()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter("SELECT StatusId, Name FROM ApplicationStatuses ORDER BY StatusId", connection))
            {
                var table = new DataTable("ApplicationStatuses");
                adapter.Fill(table);
                return table;
            }
        }

        public void UpdateApplicationStatus(int applicationId, int statusId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("UPDATE Applications SET StatusId = @StatusId WHERE ApplicationId = @ApplicationId", connection))
            {
                command.Parameters.AddWithValue("@ApplicationId", applicationId);
                command.Parameters.AddWithValue("@StatusId", statusId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void SaveEditableTable(DataTable table, string selectSql)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var adapter = new SqlDataAdapter(selectSql, connection))
            using (var builder = new SqlCommandBuilder(adapter))
            {
                adapter.InsertCommand = builder.GetInsertCommand();
                adapter.UpdateCommand = builder.GetUpdateCommand();
                adapter.DeleteCommand = builder.GetDeleteCommand();
                adapter.Update(table);
            }
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                Login = Convert.ToString(reader["Login"]),
                Password = Convert.ToString(reader["Password"]),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                RoleName = Convert.ToString(reader["RoleName"]),
                ClientId = reader["ClientId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ClientId"]),
                FullName = Convert.ToString(reader["FullName"]),
                Phone = Convert.ToString(reader["Phone"]),
                Email = Convert.ToString(reader["Email"])
            };
        }
    }
}

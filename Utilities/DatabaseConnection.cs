using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace ContactListWeb.Utilities
{
    public class DatabaseConnection
    {
        private static readonly string _connectionString;

        // Static constructor to load connection string from appsettings.json
        static DatabaseConnection()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            _connectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("DefaultConnection");
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        // Returns a new SQL Server connection
        public static SqlConnection GetConnection() // Changed from IDbConnection to SqlConnection
        {
            return new SqlConnection(_connectionString);
        }

        // Initializes the database by creating Users and Contacts tables if they don't exist
        public static async Task InitializeDatabaseAsync()
        {
            using var connection = GetConnection(); // Use SqlConnection directly
            await connection.OpenAsync(); // OpenAsync is available on SqlConnection

            // Create Users table with IDENTITY for auto-incrementing ID
            await connection.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                CREATE TABLE Users (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Username NVARCHAR(255) UNIQUE NOT NULL,
                    Password NVARCHAR(255) NOT NULL
                )");

            // Create Contacts table with a foreign key to Users
            await connection.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contacts')
                CREATE TABLE Contacts (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    UserId INT NOT NULL,
                    Name NVARCHAR(255) NOT NULL,
                    PhoneNo NVARCHAR(50),
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                )");
        }
    }
}
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

        static DatabaseConnection()
        {
            _connectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("DefaultConnection");
            Console.WriteLine($"Connection String: {_connectionString}"); // Debug output
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // Optional: Initialize database if tables don’t exist
        public static async Task InitializeDatabaseAsync()
        {
           try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                Console.WriteLine("Connected to database successfully.");

                // Ensure the KAMAAA schema exists
                await connection.ExecuteAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'KAMAAA')
                    EXEC('CREATE SCHEMA KAMAAA')");

                // Check and create Users1 table
                var usersTableExists = await connection.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) 
                    FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = 'Users1' AND s.name = 'KAMAAA'");

                if (usersTableExists == 0)
                {
                    await connection.ExecuteAsync(@"
                        CREATE TABLE KAMAAA.Users1 (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Username NVARCHAR(255) UNIQUE NOT NULL,
                            Password NVARCHAR(255) NOT NULL
                        )");
                    Console.WriteLine("Users1 table created successfully.");
                }
                else
                {
                    Console.WriteLine("Users1 table already exists, skipping creation.");
                }

                // Check and create Contacts table
                var contactsTableExists = await connection.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) 
                    FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE t.name = 'Contacts' AND s.name = 'KAMAAA'");

                if (contactsTableExists == 0)
                {
                    await connection.ExecuteAsync(@"
                        CREATE TABLE KAMAAA.Contacts (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            UserId INT NOT NULL,
                            Name NVARCHAR(255) NOT NULL,
                            PhoneNo NVARCHAR(50),
                            FOREIGN KEY (UserId) REFERENCES KAMAAA.Users1(Id)
                        )");
                    Console.WriteLine("Contacts table created successfully.");
                }
                else
                {
                    Console.WriteLine("Contacts table already exists, skipping creation.");
                }

                Console.WriteLine("Database tables verified/created successfully.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                throw; // Re-throw for debugging
            }
        }
    }
}
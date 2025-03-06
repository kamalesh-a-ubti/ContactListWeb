using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactListWeb.Models;
using Microsoft.Data.SqlClient; 
using Dapper;
using System.Threading.Tasks;

namespace ContactListWeb.Services
{
    public class AuthService
    {
        private readonly SqlConnection _connection; // Changed from IDbConnection to SqlConnection

        // Constructor with dependency injection for configuration
        public AuthService(IConfiguration configuration)
        {
            _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // Stores the currently logged-in user
        public User? CurrentUser { get; private set; }

        // Registers a new user asynchronously
        public async Task<bool> RegisterAsync(string username, string password)
        {
            try
            {
                // Insert user into Users table and retrieve the new ID
                var result = await _connection.ExecuteScalarAsync<int>(
                    "INSERT INTO KAMAAA.Users1 (Username, Password) VALUES (@Username, @Password); SELECT SCOPE_IDENTITY();",
                    new { Username = username, Password = password });
                return result > 0; // Returns true if insertion was successful
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Registration failed: {ex.Message}");
                return false;
            }
        }

        // Logs in a user asynchronously
        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                // Query for user with matching credentials
                CurrentUser = await _connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM KAMAAA.Users1 WHERE Username = @Username AND Password = @Password",
                    new { Username = username, Password = password });
                return CurrentUser != null; // Returns true if user is found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                return false;
            }
        }
    }
}
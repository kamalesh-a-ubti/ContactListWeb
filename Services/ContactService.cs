using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactListWeb.Models;
using Microsoft.Data.SqlClient; // Ensure this is included
using Dapper;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContactListWeb.Services
{
    public class ContactService
    {
        private readonly int _currentUserId;
        private readonly SqlConnection _connection; // Changed from IDbConnection to SqlConnection

        // Constructor with user ID and configuration for database connection
        public ContactService(int userId, IConfiguration configuration)
        {
            _currentUserId = userId;
            _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // Adds a new contact asynchronously
        public async Task<bool> AddContactAsync(Contact contact)
        {
            try
            {
                var result = await _connection.ExecuteAsync(
                    "INSERT INTO KAMAAA.Contacts (UserId, Name, PhoneNo) VALUES (@UserId, @Name, @PhoneNo)",
                    new { UserId = _currentUserId, contact.Name, contact.PhoneNo });
                return result > 0; // Returns true if contact was added
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding contact: {ex.Message}");
                return false;
            }
        }

        // Retrieves all contacts for the current user asynchronously
        public async Task<List<Contact>> GetAllContactsAsync()
        {
            try
            {
                var contacts = await _connection.QueryAsync<Contact>(
                    "SELECT * FROM KAMAAA.Contacts WHERE UserId = @UserId",
                    new { UserId = _currentUserId });
                return contacts.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving contacts: {ex.Message}");
                return new List<Contact>();
            }
        }

        // Updates an existing contact asynchronously
        public async Task<bool> UpdateContactAsync(int contactId, Contact updatedContact)
        {
            try
            {
                var affectedRows = await _connection.ExecuteAsync(
                    @"UPDATE KAMAAA.Contacts SET
                        Name = @Name,
                        PhoneNo = @PhoneNo
                    WHERE Id = @ContactId AND UserId = @UserId",
                    new
                    {
                        ContactId = contactId,
                        UserId = _currentUserId,
                        updatedContact.Name,
                        updatedContact.PhoneNo
                    });
                return affectedRows > 0; // Returns true if update was successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating contact: {ex.Message}");
                return false;
            }
        }

        // Deletes a contact asynchronously
        public async Task<bool> DeleteContactAsync(int contactId)
        {
            try
            {
                var affectedRows = await _connection.ExecuteAsync(
                    "DELETE FROM KAMAAA.Contacts WHERE Id = @Id AND UserId = @UserId",
                    new { Id = contactId, UserId = _currentUserId });
                return affectedRows > 0; // Returns true if deletion was successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting contact: {ex.Message}");
                return false;
            }
        }

        // Searches for contacts by name or phone number asynchronously
        public async Task<List<Contact>> SearchContactsAsync(string searchTerm)
        {
            try
            {
                var results = await _connection.QueryAsync<Contact>(
                    @"SELECT * FROM KAMAAA.Contacts WHERE UserId = @UserId AND 
                    (Name LIKE @SearchTerm OR PhoneNo LIKE @SearchTerm)",
                    new { UserId = _currentUserId, SearchTerm = $"%{searchTerm}%" });
                return results.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex.Message}");
                return new List<Contact>();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactListWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactListWeb.Data
{
    public class TodoContext : DbContext 
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

        public DbSet<Todo> Todos { get; set; }   
    }
}
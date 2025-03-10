using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ContactListWeb.Data;
using ContactListWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactListWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")] // Force JSON output
    public class TodosController : ControllerBase
    {
        // ... (rest of the controller code remains the same)
        private readonly TodoContext _context;

        public TodosController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/todos
        [HttpGet]
        public ActionResult<IEnumerable<Todo>> GetTodos()
        {
            try
            {
                var userId = GetUserId();
                var todos = _context.Todos.Where(t => t.UserId == userId).ToList();
                return Ok(todos);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User not authenticated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/todos/{id}
        [HttpGet("{id}")]
        public ActionResult<Todo> GetTodo(int id)
        {
            try
            {
                var userId = GetUserId();
                var todo = _context.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                {
                    return NotFound($"Todo with ID {id} not found or not owned by user.");
                }

                return Ok(todo);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User not authenticated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/todos
        [HttpPost]
        public ActionResult<Todo> CreateTodo([FromBody] Todo todo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserId();
                todo.UserId = userId;
                todo.Id = 0; // Ensure ID is auto-assigned by the in-memory DB

                _context.Todos.Add(todo);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User not authenticated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/todos/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateTodo(int id, [FromBody] Todo updatedTodo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserId();
                var todo = _context.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                {
                    return NotFound($"Todo with ID {id} not found or not owned by user.");
                }

                todo.Title = updatedTodo.Title;
                todo.IsCompleted = updatedTodo.IsCompleted;
                _context.SaveChanges();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User not authenticated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/todos/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTodo(int id)
        {
            try
            {
                var userId = GetUserId();
                var todo = _context.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                {
                    return NotFound($"Todo with ID {id} not found or not owned by user.");
                }

                _context.Todos.Remove(todo);
                _context.SaveChanges();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User not authenticated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to get authenticated user ID
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException();
            }
            return userId;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactListWeb.Models;
using ContactListWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContactListWeb.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ContactService _contactService;

        // Constructor with dependency injection for ContactService and session access
        public ContactsController(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            var userId = httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) throw new UnauthorizedAccessException("User not logged in.");
            _contactService = new ContactService(userId.Value, configuration);
        }

        // Displays all contacts for the current user
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return View(contacts);
        }

        // Displays the form to create a new contact
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Handles the submission of a new contact
        [HttpPost]
        public async Task<IActionResult> Create(Contact contact)
        {
            if (ModelState.IsValid)
            {
                if (await _contactService.AddContactAsync(contact))
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Failed to add contact.");
            }
            return View(contact);
        }

        // Displays the form to edit an existing contact
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var contact = (await _contactService.GetAllContactsAsync()).Find(c => c.Id == id);
            if (contact == null) return NotFound();
            return View(contact);
        }

        // Handles the submission of an updated contact
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Contact contact)
        {
            if (ModelState.IsValid)
            {
                if (await _contactService.UpdateContactAsync(id, contact))
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Failed to update contact.");
            }
            return View(contact);
        }

        // Displays the confirmation page for deleting a contact
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var contact = (await _contactService.GetAllContactsAsync()).Find(c => c.Id == id);
            if (contact == null) return NotFound();
            return View(contact);
        }

        // Handles the deletion of a contact
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _contactService.DeleteContactAsync(id))
                return RedirectToAction("Index");
            return RedirectToAction("Index"); // Redirect even if deletion fails for simplicity
        }

        // Displays the search results based on a search term
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return RedirectToAction("Index");
            var results = await _contactService.SearchContactsAsync(searchTerm);
            return View("Index", results); // Reuse Index view for search results
        }
    }
}
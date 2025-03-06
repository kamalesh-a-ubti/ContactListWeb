using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactListWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ContactListWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        // Constructor with dependency injection for AuthService
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Displays the login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Handles login form submission
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (await _authService.LoginAsync(username, password))
            {
                // Store user ID in session upon successful login
                HttpContext.Session.SetInt32("UserId", _authService.CurrentUser.Id);
                return RedirectToAction("Index", "Contacts");
            }
            ViewBag.Error = "Invalid credentials!";
            return View();
        }

        // Displays the registration page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Handles registration form submission
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (await _authService.RegisterAsync(username, password))
            {
                return RedirectToAction("Login");
            }
            ViewBag.Error = "Registration failed! Username might be taken.";
            return View();
        }

        // Logs out the user
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("Login");
        }
    }
}
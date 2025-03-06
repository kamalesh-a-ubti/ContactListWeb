using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactListWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace ContactListWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (await _authService.LoginAsync(username, password))
            {
                // Create claims for the authenticated user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, _authService.CurrentUser.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Keep the user logged in across sessions
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // Optional: Set expiration
                };

                // Sign in the user with cookie authentication
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Store user ID in session (optional, if still needed)
                HttpContext.Session.SetInt32("UserId", _authService.CurrentUser.Id);

                return RedirectToAction("Index", "Contacts");
            }
            ViewBag.Error = "Invalid credentials!";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user from cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("Login");
        }
    }
}
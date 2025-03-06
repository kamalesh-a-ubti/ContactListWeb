using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ContactListWeb.Models;

namespace ContactListWeb.Controllers;

public class HomeController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }
}

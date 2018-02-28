using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RETrasactionManager.Models;
using System.Net.Http;

namespace RETrasactionManager.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index(String message)
        {
            ViewData["Message"] = message;
            return View();
        }
        
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

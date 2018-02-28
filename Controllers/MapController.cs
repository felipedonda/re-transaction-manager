using Microsoft.AspNetCore.Mvc;
using RETrasactionManager.Models;
using System;

namespace RETrasactionManager.Controllers
{
    public class MapController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
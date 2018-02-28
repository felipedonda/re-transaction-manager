using Microsoft.AspNetCore.Mvc;
using RETrasactionManager.Models;
using System;

namespace RETrasactionManager.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index() => new ObjectResult("Hello world!");
    }
}
using RETrasactionManager.Lib.KmlFunctions;
using RETrasactionManager.Lib.GeojsonFunctions;
using RETrasactionManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Generic;

namespace RETrasactionManager.Controllers
{
    public class ConvertController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    public IActionResult Geojson()
        {
            return View();
        }
        public IActionResult Shape()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ShapeToKmlConvert(List<IFormFile> files)
        {
            string FileName = "";
            XmlDocument Output = new XmlDocument();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    Kml kml = new Kml();
                    using (Stream stream = formFile.OpenReadStream())
                    {
                        await kml.LoadAsync(stream);
                    }
                    Output = kml.ToXml();
                    FileName = formFile.FileName;
                }
            }
            
            return File(System.Text.Encoding.UTF8.GetBytes(Output.OuterXml),"text/plain", String.Format("{0}.kml",FileName.Split(".")[0]) );
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace MCCA.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Centers()
        {
            ViewData["Message"] = "Centers page";

            return View();
        }

        public IActionResult VisionMissionValues()
        {
            ViewData["Message"] = "VisionMissionValues page";

            return View();
        }

        public IActionResult MESASchoolsProgram()
        {
            ViewData["Message"] = "MESA Schools Program page";

            return View();
        }

        public IActionResult MESACommunityCollegeProgram()
        {
            ViewData["Message"] = "MESA Community College Program page";

            return View();
        }

        public IActionResult MESAEngineeringProgram()
        {
            ViewData["Message"] = "MESA Engineering Program page";

            return View();
        }

        public IActionResult News()
        {
            ViewData["Message"] = "News page";

            return View();
        }

        public IActionResult Donate()
        {
            ViewData["Message"] = "Donate page";

            return View();
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}

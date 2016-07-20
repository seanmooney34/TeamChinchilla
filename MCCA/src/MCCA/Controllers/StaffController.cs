using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;

namespace MCCA.Controllers
{ 
    public class StaffController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ManageSite()
        {
            return View();
        }
        public IActionResult ManagePersonalAccount()
        {
            return View();
        }
    }
}

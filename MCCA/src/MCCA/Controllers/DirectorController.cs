﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MCCA.Controllers
{
    public class DirectorController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ManageAccounts()
        {
            return View();
        }
        public IActionResult AddAccount()
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

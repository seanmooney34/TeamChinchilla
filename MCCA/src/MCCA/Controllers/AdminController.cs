﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;

namespace MCCA.Controllers
{
    public class AdminController : Controller
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
        public IActionResult SelectCenter()
        {
            return View();
        }
        public IActionResult ManageCenters()
        {
            return View();
        }
        public IActionResult AddCenter()
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

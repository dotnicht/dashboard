﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace InvestorDashboard.Web.Server.RestAPI
{
    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
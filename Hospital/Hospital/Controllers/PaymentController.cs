﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hospital.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        public ActionResult choosepayment()
        {
            return View();
        }
    }
}
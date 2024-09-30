﻿using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.CalculationRunNameIndex);
        }
    }
}

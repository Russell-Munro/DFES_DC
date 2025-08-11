using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using DataConnectorUI.Models;
using DataConnectorUI.Controllers.Filters;

namespace DataConnectorUI.Controllers
{
    [Produces("text/html")]
    public class HomeController : UIController
    {
        [ServiceFilter(typeof(AdminAuthFilter))]
        public IActionResult Index()
        {
            return View();
        }
        [ServiceFilter(typeof(AdminAuthFilter))]
        public IActionResult ConnectionEditor()
        {
            ViewData["Message"] = "This View will Host the Connection Editor UI...";
            return View();
        }
        [ServiceFilter(typeof(AdminAuthFilter))]
        public IActionResult RuleManager()
        {
            ViewData["Message"] = "This View will Host the Rule Manager UI...";
            return View();
        }
        [ServiceFilter(typeof(AdminAuthFilter))]
        public IActionResult LogViewer()
        {
            ViewData["Message"] = "This View will Host the Log Viewer UI...";
            return View();
        }

        // Fix for CS0114: Add 'new' keyword to explicitly hide the inherited member
        public new IActionResult Unauthorized()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BakeHub.Controllers
{
    public class OrderController : Controller
    {
        // GET: Order
        public ActionResult Orders()
        {
            return View();
        }

        // GET: editOrder

        public ActionResult EditOrder()
        {
            return View();
        }

        // GET: addOrder

        public ActionResult AddOrder()
        {
            return View();
        }
    }
}
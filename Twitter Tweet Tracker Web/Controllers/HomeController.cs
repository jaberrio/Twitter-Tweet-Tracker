using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twitter_Tweet_Tracker_Web.Models;

namespace Twitter_Tweet_Tracker_Web.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            var user = (TTTUser) TempData["user"];
            if (!string.IsNullOrWhiteSpace(user?.oAuthSecret))
            {
                ViewBag.user = user;
                HttpCookie oauth_token_cookie = new HttpCookie("oauth_token");
                HttpCookie oauth_token_secret_cookie = new HttpCookie("oauth_token_secret");
                oauth_token_cookie.Value = user.oAuthToken;
                oauth_token_secret_cookie.Value = user.oAuthSecret;
                oauth_token_cookie.Expires = DateTime.Now.AddHours(1);
                oauth_token_secret_cookie.Expires = DateTime.Now.AddHours(1);

                Response.Cookies.Add(oauth_token_cookie);
                Response.Cookies.Add(oauth_token_secret_cookie);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Dashboard(TTTUser user)
        {
            ViewBag.user = user;
            Console.Write(ViewBag.user);
            return View();
        }
    }

}

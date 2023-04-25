using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Sciencetopia.Models;
using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Sciencetopia.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDriver _neo4jDriver;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(IDriver neo4jDriver, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _neo4jDriver = neo4jDriver;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
                if (result.Succeeded)
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.ErrorMessage = "用户名或密码错误。";
            return View("Login");
        }
    }
}

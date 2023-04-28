using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using Sciencetopia.Models;
using System.Text;

namespace Sciencetopia.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDriver _driver;

        public AccountController(IDriver driver)
        {
            _driver = driver;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string salt = Guid.NewGuid().ToString();
                string hashedPassword = HashPassword(model.Password, salt);

                var newUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.UserName,
                    Email = model.Email,
                    Password = hashedPassword, // 使用哈希密码而不是明文密码
                    Salt = salt // 存储盐值以便将来验证用户
                };

                if (await CreateUserAsync(newUser))
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError("UserName", "用户名已存在。");
                }
            }
            // 如果我们进行到这一步，表示出现了某个错误，请重新显示表单
            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await ValidateUserAsync(model.UserName, model.Password);
                if (user != null)
                {
                    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = model.RememberMe });

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "无效的登录尝试。");
                }
            }
            // 如果我们进行到这一步，表示出现了某个错误，请重新显示表单
            ModelState.AddModelError(string.Empty, "无效的登录尝试。");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        // 注册用户的逻辑
        public async Task<bool> CreateUserAsync(ApplicationUser user)
        {
            using (var session = _driver.AsyncSession())
            {
                var result = await session.WriteTransactionAsync(async tx =>
                {
                    var exists = await tx.RunAsync("MATCH (u:User {UserName: $UserName}) RETURN u", new { UserName = user.UserName });
                    if (await exists.PeekAsync() != null)
                    {
                        return false;
                    }

                    await tx.RunAsync("CREATE (u:User {Id: $Id, UserName: $UserName, Email: $Email, Password: $Password})",
                        new { Id = user.Id, UserName = user.UserName, Email = user.Email, Password = user.Password });

                    return true;
                });

                return result;
            }
        }

        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] hashedBytes = KeyDerivation.Pbkdf2(password, saltBytes, KeyDerivationPrf.HMACSHA1, 10000, 256 / 8);
            return Convert.ToBase64String(hashedBytes);
        }

        // 验证用户的逻辑
        public async Task<ApplicationUser> ValidateUserAsync(string userName, string password)
        {
            using (var session = _driver.AsyncSession())
            {
                var user = await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync("MATCH (u:User {UserName: $UserName}) RETURN u",
                        new { UserName = userName });

                    return await result.PeekAsync();
                });

                if (user != null)
                {
                    string salt = user["Salt"].As<string>();
                    string hashedPassword = HashPassword(password, salt);

                    if (hashedPassword == user["Password"].As<string>())
                    {
                        return new ApplicationUser
                        {
                            Id = user["Id"].As<string>(),
                            UserName = user["UserName"].As<string>(),
                            Email = user["Email"].As<string>(),
                            Password = user["Password"].As<string>(),
                            Salt = user["Salt"].As<string>()
                        };
                    }
                }

                return null;
            }
        }

    }
}

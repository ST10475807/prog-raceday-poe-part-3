using Microsoft.AspNetCore.Mvc;
namespace RaceDay.Mvc.Controllers;
public abstract class BaseController:Controller
{
 protected bool LoggedIn=>!string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
 protected bool IsRole(string role)=>HttpContext.Session.GetString("Role")==role;
 protected IActionResult? Require(string role){if(!LoggedIn)return RedirectToAction("Login","Account");if(!IsRole(role))return RedirectToAction("Index","Home");return null;}
}

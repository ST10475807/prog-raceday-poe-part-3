using System.Net.Http.Json;using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Models; using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class AccountController(ApiClient api):BaseController
{
 public IActionResult Login()=>View();
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginVm vm){if(!ModelState.IsValid)return View(vm);var r=await api.Send(HttpMethod.Post,"api/auth/login",new{vm.Email,vm.Password});if(!r.IsSuccessStatusCode){ModelState.AddModelError("","Login failed. Check your details and ensure the API is running.");return View(vm);}var d=await r.Content.ReadFromJsonAsync<LoginResponse>();if(d==null){ModelState.AddModelError("","The API returned an invalid login response.");return View(vm);}HttpContext.Session.SetString("Token",d.accessToken);HttpContext.Session.SetString("Role",d.profile.role);HttpContext.Session.SetString("Name",d.profile.firstName);HttpContext.Session.SetString("UserId",d.profile.userId.ToString());return RedirectToAction("Index","Home");}
 public IActionResult Register()=>View();
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterVm vm){if(!ModelState.IsValid)return View(vm);if(vm.Role is not ("Organizer" or "Participant")){ModelState.AddModelError(nameof(vm.Role),"Choose a valid account role.");return View(vm);}var r=await api.Send(HttpMethod.Post,"api/auth/register",vm);if(!r.IsSuccessStatusCode){ModelState.AddModelError("","Registration failed. The email may already be registered or the API may be unavailable.");return View(vm);}TempData["Message"]="Account created. Please log in.";return RedirectToAction(nameof(Login));}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Logout(){HttpContext.Session.Clear();return RedirectToAction("Index","Home");}
 private record LoginResponse(string accessToken,Profile profile); private record Profile(int userId,string firstName,string role);
}

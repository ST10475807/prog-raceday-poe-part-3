using System.Net.Http.Json;using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Services;using RaceDay.Mvc.Models;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController(ApiClient api):BaseController
{
 public async Task<IActionResult> MyEvents(){var g=Require("Participant");if(g!=null)return g;return View(await api.Get<List<EnrolmentVm>>("api/enrolments/me")??[]);}
 public async Task<IActionResult> Results(){var g=Require("Participant");if(g!=null)return g;return View(await api.Get<List<ResultVm>>("api/results/me")??[]);}
 public async Task<IActionResult> Profile(){var g=Require("Participant");if(g!=null)return g;var p=await api.Get<ProfileVm>("api/users/me");if(p==null)return View(new ProfileVm());ViewBag.ProfileImage=HttpContext.Session.GetString("ProfileImage");return View(p);}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Profile(ProfileVm vm){var g=Require("Participant");if(g!=null)return g;var r=await api.Send(HttpMethod.Put,"api/users/me",new{vm.FirstName,vm.LastName,vm.Phone});TempData["Message"]=r.IsSuccessStatusCode?"Profile updated.":"Profile could not be updated.";return RedirectToAction(nameof(Profile));}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> UploadPicture(IFormFile? picture){var g=Require("Participant");if(g!=null)return g;if(picture==null||picture.Length==0||!picture.ContentType.StartsWith("image/")){TempData["Message"]="Choose a valid image file.";return RedirectToAction(nameof(Profile));}var r=await api.Upload("api/files/profile-picture",picture);if(r.IsSuccessStatusCode){var body=await r.Content.ReadFromJsonAsync<PictureResponse>();if(body?.url!=null)HttpContext.Session.SetString("ProfileImage",body.url);TempData["Message"]="Profile picture uploaded.";}else TempData["Message"]="Profile picture upload failed.";return RedirectToAction(nameof(Profile));}
 private record PictureResponse(string url);
}

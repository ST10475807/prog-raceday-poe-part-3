using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Models;using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class EventsController(ApiClient api):BaseController
{
 public async Task<IActionResult> Details(int id){var e=await api.Get<EventDetailsVm>($"api/events/{id}");return e is null?NotFound():View(e);}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Enrol(int eventId,int categoryId){var guard=Require("Participant");if(guard!=null)return guard;var res=await api.Send(HttpMethod.Post,$"api/events/{eventId}/categories/{categoryId}/enrolments");TempData["Message"]=res.IsSuccessStatusCode?"Your enrolment has been recorded.":$"Enrolment could not be completed ({(int)res.StatusCode}). Check availability and try again.";return RedirectToAction(nameof(Details),new{id=eventId});}
}

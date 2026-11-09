using Microsoft.AspNetCore.Mvc; using RaceDay.Mvc.Services; using RaceDay.Mvc.Models;
namespace RaceDay.Mvc.Controllers;
public class HomeController(ApiClient api):BaseController
{
 public async Task<IActionResult> Index(string? eventType,DateTime? fromDate,DateTime? toDate){var all=await api.Get<List<EventVm>>("api/events?status=Open")??[];var events=all.Where(e=>e.EventDate.Date>=DateTime.Today);if(!string.IsNullOrWhiteSpace(eventType))events=events.Where(e=>e.EventType==eventType);if(fromDate.HasValue)events=events.Where(e=>e.EventDate.Date>=fromDate.Value.Date);if(toDate.HasValue)events=events.Where(e=>e.EventDate.Date<=toDate.Value.Date);ViewBag.EventType=eventType;ViewBag.FromDate=fromDate?.ToString("yyyy-MM-dd");ViewBag.ToDate=toDate?.ToString("yyyy-MM-dd");return View(events.OrderBy(e=>e.EventDate).ToList());}
 public IActionResult Error()=>View();
}

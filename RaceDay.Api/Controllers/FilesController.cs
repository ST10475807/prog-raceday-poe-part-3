using System.Security.Claims; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using RaceDay.Api.Data; using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/files"),Authorize]
public class FilesController(RaceDayContext db, IBlobStorageService blobs):ControllerBase {
 int UserId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 [HttpPost("events/{eventId:int}/banner"),Authorize(Roles="Organizer")]
 public async Task<IActionResult> Banner(int eventId,IFormFile file){var e=await db.Events.FindAsync(eventId); if(e is null)return NotFound(); if(e.OrganizerId!=UserId)return Forbid(); if(file is null||file.Length==0||file.Length>5*1024*1024||!file.ContentType.StartsWith("image/"))return BadRequest(new{message="Please upload an image no larger than 5 MB."}); e.BannerImageUrl=await blobs.UploadAsync(file,"events"); await db.SaveChangesAsync(); return Ok(new{url=e.BannerImageUrl});}
 [HttpPost("profile-picture")]
 public async Task<IActionResult> Profile(IFormFile file){var u=await db.Users.FindAsync(UserId); if(u is null)return NotFound(); if(file is null||file.Length==0||file.Length>5*1024*1024||!file.ContentType.StartsWith("image/"))return BadRequest(new{message="Please upload an image no larger than 5 MB."}); u.ProfileImageUrl=await blobs.UploadAsync(file,"profiles"); await db.SaveChangesAsync(); return Ok(new{url=u.ProfileImageUrl});}
}

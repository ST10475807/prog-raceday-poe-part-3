using Azure.Storage.Blobs;
namespace RaceDay.Api.Services;
public interface IBlobStorageService { Task<string> UploadAsync(IFormFile file,string folder); }
public class BlobStorageService(IConfiguration config, IWebHostEnvironment env) : IBlobStorageService {
 public async Task<string> UploadAsync(IFormFile file,string folder){
  if(file.Length==0) throw new InvalidOperationException("Empty file.");
  var name=$"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
  var cs=config["AzureBlob:ConnectionString"]; var container=config["AzureBlob:Container"] ?? "raceday-images";
  if(!string.IsNullOrWhiteSpace(cs)) { var client=new BlobContainerClient(cs,container); await client.CreateIfNotExistsAsync(); var blob=client.GetBlobClient(name); await blob.UploadAsync(file.OpenReadStream(), overwrite:true); return blob.Uri.ToString(); }
  var root=Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath,"wwwroot"),"uploads",folder); Directory.CreateDirectory(root); var local=Path.Combine(root,Path.GetFileName(name)); await using var fs=File.Create(local); await file.CopyToAsync(fs); return $"/uploads/{folder}/{Path.GetFileName(name)}";
 }
}

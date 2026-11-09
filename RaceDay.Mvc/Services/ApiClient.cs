using System.Net.Http.Headers; using System.Net.Http.Json;
namespace RaceDay.Mvc.Services;
public class ApiClient(HttpClient http,IHttpContextAccessor accessor)
{
    private void AddAuth(HttpRequestMessage? request=null) { var token=accessor.HttpContext?.Session.GetString("Token"); if(!string.IsNullOrWhiteSpace(token)) { if(request!=null) request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token); else http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token); } }
    public async Task<T?> Get<T>(string url) { try { using var req=new HttpRequestMessage(HttpMethod.Get,url); AddAuth(req); using var res=await http.SendAsync(req); if(!res.IsSuccessStatusCode)return default; return await res.Content.ReadFromJsonAsync<T>(); } catch(HttpRequestException){return default;} catch(TaskCanceledException){return default;} catch(System.Text.Json.JsonException){return default;} }
    public async Task<HttpResponseMessage> Send(HttpMethod method,string url,object? body=null) { var req=new HttpRequestMessage(method,url); AddAuth(req); if(body!=null)req.Content=JsonContent.Create(body); try{return await http.SendAsync(req);}catch(HttpRequestException){return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable){ReasonPhrase="RaceDay API is unavailable"};} }
    public async Task<HttpResponseMessage> Upload(string url,IFormFile file) { using var req=new HttpRequestMessage(HttpMethod.Post,url); AddAuth(req); var form=new MultipartFormDataContent(); form.Add(new StreamContent(file.OpenReadStream()),"file",file.FileName); req.Content=form; try{return await http.SendAsync(req);}catch(HttpRequestException){return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);} }
}

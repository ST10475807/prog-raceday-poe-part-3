using RaceDay.Mvc.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options=>{options.IdleTimeout=TimeSpan.FromHours(2);options.Cookie.HttpOnly=true;options.Cookie.IsEssential=true;options.Cookie.SameSite=SameSiteMode.Lax;});
builder.Services.AddHttpClient<ApiClient>(client=>{var apiUrl=builder.Configuration["ApiBaseUrl"]??"http://localhost:5080/";client.BaseAddress=new Uri(apiUrl.EndsWith('/')?apiUrl:apiUrl+"/");client.Timeout=TimeSpan.FromSeconds(30);});
var app=builder.Build();
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseSession();app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");app.Run();

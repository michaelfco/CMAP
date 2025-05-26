using CMAPTask.Application.Interfaces;
using CMAPTask.Application.UseCases;
using CMAPTask.Infrastructure;
using CMAPTask.Infrastructure.Extensions;
using CMAPTask.Infrastructure.Services;
using CMAPTask.web;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


//==add new created dbcontext
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllersWithViews();


//builder.Configuration.GetSection("OB").Get<OBSettings>();

var obSettings = builder.Configuration.GetSection("OB").Get<OBSettings>();

builder.Services.Configure<OBSettings>(builder.Configuration.GetSection("OB"));

builder.Services.AddHttpClient<OBTokenService>(client =>
{
    client.BaseAddress = new Uri(obSettings.BaseURL);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<OpenBankingService>(client =>
{
    client.BaseAddress = new Uri(obSettings.BaseURL);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();

using CantineBack.Models;
using CantineFront.ServiceFactory;
using CantineFront.Static;
using DPWorldMobile.ServiceFactory;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using CantineFront.Services;
using CantineFront.Hubs;
using CantineFront.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CantineFront.Identity;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
ApiUrlGeneric.AppSettings = builder.Configuration.GetRequiredSection("AppSettings").Get<AppSettings>();

builder.Services.AddTransient<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddHttpClient("ClearanceApi", HttpClient =>
{
    HttpClient.BaseAddress = new Uri(builder.Configuration["AppSettings:ApiUrl"]);

    try
    {
        
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpClient.DefaultRequestHeaders.Add("Cookie", "AspxAutoDetectCookieSupport=1");
    }
    catch (Exception ex)
    {

   
    }

})
        .AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

//.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(3))
//.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
;
//builder.Services.AddAuthentication("YourSchemeName") // Sets the default scheme to cookies
//.AddCookie("YourSchemeName", options =>
//{
//    options.LoginPath = "/Login/Index";
//});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,options =>
{
    options.LoginPath = "/Login/Index";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.Cookie.MaxAge = options.ExpireTimeSpan; // optional
    options.SlidingExpiration = true;
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(IdentityData.AdminPolicy, policy => policy.RequireClaim(ClaimTypes.Role, "ADMIN"));
    options.AddPolicy(IdentityData.UserPolicy, policy => policy.RequireClaim(ClaimTypes.Role, "USER"));
    options.AddPolicy(IdentityData.GerantPolicy, policy => policy.RequireClaim(ClaimTypes.Role, "GERANT"));
});

builder.Services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureMyCookie>();

// Add services to the container.
builder.Services.AddControllersWithViews();

//Auto Mapper 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<FormOptions>(options =>
{
    // Set the limit to 20 MB
    options.MultipartBodyLengthLimit = 20971520;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});


builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxResponseBufferSize = null;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Menu}/{id?}");
app.MapHub<ChatCommandHub>("/chatHub");

app.Run();

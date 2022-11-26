using Dynsec;
using DynsecAdmin.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using MQTTnet;
using MQTTnet.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<MqttFactory>();
builder.Services.AddSingleton<MqttCredentialProvider>();
builder.Services.AddSingleton(s => new MqttClientOptionsBuilder()
    .WithTcpServer(builder.Configuration["MqttServer"])
    .WithCredentials(s.GetRequiredService<MqttCredentialProvider>())
    .Build());
builder.Services.AddScoped<IMqttClient, ConnectedMqttClient>();
builder.Services.AddScoped<DynsecClient>();

var mvcBuilder = builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/");
    o.Conventions.AllowAnonymousToPage("/Login");
    o.Conventions.AllowAnonymousToPage("/Error");
});
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, x =>
    {
        x.LoginPath = "/login";
        x.LogoutPath = "/logout";
        x.ReturnUrlParameter = "returnurl";
        x.SlidingExpiration = true;
        x.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

using Microsoft.AspNetCore.HttpOverrides;
using MultiMillionaire.Database;
using MultiMillionaire.Hubs;
using MultiMillionaire.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment()) mvcBuilder.AddRazorRuntimeCompilation();
builder.Services.AddProgressiveWebApp();

builder.Services.AddSignalR(hubOptions => { hubOptions.MaximumParallelInvocationsPerClient = 5; });

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton<IMultiplayerHubStorage, MultiplayerHubStorage>();
builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

builder.Services.AddDetection();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseDetection();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.MapHub<MultiplayerGameHub>("/multiplayerHub");

app.Run();

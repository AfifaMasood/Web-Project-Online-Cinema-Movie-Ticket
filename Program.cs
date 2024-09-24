using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using practice.Data;
using System.Security.Claims;
using Domain.Interface;
using Domain.Entities;
using Application;
using Infrastructure;
using practice.Hubs;
using Microsoft.Extensions.Logging;
using Project_Task.Controllers;

var builder = WebApplication.CreateBuilder(args);
string conn = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=MyNewDB;Integrated Security=True";


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddSignalR();

builder.Services.AddScoped<IRepository<SeatReserved>>(provider =>
    new Genericrepo<SeatReserved>(conn)
);
builder.Services.AddScoped<IRepository<Contact>>(provider =>
    new Genericrepo<Contact>(conn)
);
builder.Services.AddScoped<IMovie, MovieRepo>();
builder.Services.AddScoped<SeatRepoDecorator>();
//builder.Services.AddScoped<ISeat, SeatRepo>();

builder.Services.AddMemoryCache();

builder.Services.AddDefaultIdentity<MyUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(3);
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LoggedInPolicy", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("PakistaniPolicy", policy =>
      policy.RequireClaim(ClaimTypes.Country, "Pakistan"));

    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim(ClaimTypes.Email, "admin@gmail.com");
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
});
app.MapRazorPages();

app.Run();

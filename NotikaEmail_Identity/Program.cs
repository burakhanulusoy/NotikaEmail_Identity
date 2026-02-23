using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Context;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));

});


builder.Services.AddValidatorsFromAssembly(typeof(RegisterValidator).Assembly);

builder.Services.AddIdentity<AppUser, AppRole>(config =>
{
    config.User.RequireUniqueEmail = true;
    config.Password.RequireDigit = true;
    config.Password.RequireLowercase = true;
    config.Password.RequireUppercase = true;

    config.Lockout.MaxFailedAccessAttempts = 5;    // 5 hatalý giriþ denemesinden sonra hesabý kilitle
    config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Hesap kilitlendiðinde 15 dakika boyunca giriþ yapýlamasýn
    config.Lockout.AllowedForNewUsers = true;      // Yeni oluþturulan kullanýcýlar için de bu kilitleme mekanizmasý geçerli olsun


}).AddEntityFrameworkStores<AppDbContext>();





builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

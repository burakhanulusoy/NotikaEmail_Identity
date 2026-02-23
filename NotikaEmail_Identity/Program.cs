using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Context;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"));

});





//ekleme sebeim dependcy ýnjection idednyiy usermanager , signinmanager .... kullanmak için lazým
builder.Services.AddIdentity<AppUser, AppRole>(config =>
{
    config.User.RequireUniqueEmail = true;
    config.Password.RequireDigit = true;
    config.Password.RequireLowercase = true;
    config.Password.RequireUppercase = true;

    config.Lockout.MaxFailedAccessAttempts = 5;    // 5 hatalý giriþ denemesinden sonra hesabý kilitle
    config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10); // Hesap kilitlendiðinde 15 dakika boyunca giriþ yapýlamasýn
    config.Lockout.AllowedForNewUsers = true;      // Yeni oluþturulan kullanýcýlar için de bu kilitleme mekanizmasý geçerli olsun


}).AddEntityFrameworkStores<AppDbContext>()
  .AddErrorDescriber<CustomErrorDescriber>();
//fromstores idedntiy ile veritabaný iliþki haberleþmesýný saðlýyor...




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



//ilk defa yaptým amacý su:
// identity ile ilgiis yok direk .net security 
//
app.UseStatusCodePagesWithReExecute("/PageNotFound/Index", "?code={0}");
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

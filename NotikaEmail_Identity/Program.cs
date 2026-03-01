using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.Context;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Mappings;
using NotikaEmail_Identity.Repositories.CategoryRepositories;
using NotikaEmail_Identity.Repositories.MessageRepositories;
using NotikaEmail_Identity.Repositories.UserRepositories;
using NotikaEmail_Identity.Services.CategoryServices;
using NotikaEmail_Identity.Services.MessageServices;
using NotikaEmail_Identity.Services.SendEmailServices;
using NotikaEmail_Identity.Services.UserServices;
using NotikaEmail_Identity.Validations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



// --- SERÝLOG VE SEQ AYARLARI BURAYA ---
// --- SERÝLOG VE SEQ AYARLARI BURAYA ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // BÜTÜN ÇÖPÜ SUSTURAN SÝHÝR 1
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)    // BÜTÜN ÇÖPÜ SUSTURAN SÝHÝR 2
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog(); // ASP.NET Core'a "Loglama iþi artýk Serilog'da" diyoruz.


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

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Login/SignIn";

});


builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            // DEÐÝÞEN KISIM: "login" veya "select_account" yerine "consent" yazdýk.
            // Bu sayede o aradýðýn "Devam Et" ve onay sayfalarý her seferinde karþýna gelir.
            context.Response.Redirect(context.RedirectUri + "&prompt=consent");
            return Task.CompletedTask;
        };
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];

    });













builder.Services.AddAutoMapper(typeof(CategoryMappings).Assembly);

builder.Services.AddFluentValidationAutoValidation().
                 AddFluentValidationClientsideAdapters()
                 .AddValidatorsFromAssembly(typeof(ChangePasswordValidator).Assembly);

builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
builder.Services.AddScoped<ICategoryService,CategoryService>();


builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ISendEmail, SendEmail>();

builder.Services.AddScoped<SeqLogService>();

builder.Services.AddHttpClient();

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

// Bunu ekle: Gelen HTTP isteklerini Seq'e çok þýk ve tek satýrda loglar!
app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
          );


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

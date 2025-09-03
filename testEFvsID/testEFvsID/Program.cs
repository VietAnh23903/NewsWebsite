using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using testEFvsID.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using testEFvsID.Services;
using testEFvsID.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var Configuration = builder.Configuration;
var connectionString = builder.Configuration.GetConnectionString("identityContextConnection") ?? throw new InvalidOperationException("Connection string 'identityContextConnection' not found.");
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddOptions();
var mailsetting = Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddSingleton<IEmailSender,SendMailService>();
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();
builder.Services.AddIdentity<AppUser,IdentityRole>().AddEntityFrameworkStores<AppDBContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // cau hinh password
    options.Password.RequireDigit = false; //ko bat co so
    options.Password.RequireLowercase = false;// ko bat co chu thuong
    options.Password.RequireNonAlphanumeric = false; // ko bat kt db
    options.Password.RequireUppercase = false; // ko bat chu hoa
    options.Password.RequiredLength = 3;// so ki tu thoi thieu
    options.Password.RequiredUniqueChars = 1;//so ki tu rieng biet
    //cau hinh lockout-khoa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
    //cau hinh ve user
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-._@+";//cac ki tu dat ten
    options.User.RequireUniqueEmail = true;
    //cau hinh dang nhap
    options.SignIn.RequireConfirmedEmail = true; //cau hinh xac thu email
    options.SignIn.RequireConfirmedPhoneNumber = false; //xac thu sdt
    options.SignIn.RequireConfirmedAccount = true;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login/";
    options.LogoutPath= "/logout";
    options.AccessDeniedPath = "/khongduoctruycap.html";
});
builder.Services.AddAuthentication().AddGoogle(options => 
{
    var gconfig = Configuration.GetSection("Authentication:Google");
    options.ClientId= gconfig["ClientId"];
    options.ClientSecret = gconfig["ClientSecret"];
    options.CallbackPath = "/dang-nhap-tu-google";
}).AddFacebook(options =>
{
    var fconfig = Configuration.GetSection("Authentication:Facebook");
    options.AppId = fconfig["AppId"];
    options.AppSecret = fconfig["AppSecret"];
    options.CallbackPath = "/dang-nhap-tu-facebook";
});

builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", builder =>
    {
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
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
app.UseAuthentication();// xac dinh danh tinh
app.UseAuthorization();// xac thuc quyen truy cap
app.MapControllerRoute(
           name: "areas",
           pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
         );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

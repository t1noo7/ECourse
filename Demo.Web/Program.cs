using Demo.Web.Startup;
using Demo.Web.Helpers;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.AddMongoDatabase();
builder.Services.RegisterServices(builder.Configuration);
builder.Services.AddMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15); // Thời gian timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.Configure<FormOptions>(options => {
    options.MultipartBodyLengthLimit = 524288000; // 500MB
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

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "admin/{controller=Home}/{action=Index}"
);

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
name: "danhsachkhoahoc",
pattern: FriendlyUrl.CoursesListFrUrl,
defaults: new { controller = "Course", action = "List" });

app.MapControllerRoute(
    name: "thongtinkhoahoc",
    pattern: FriendlyUrl.CourseDetailFrUrl,
    defaults: new { controller = "Course", action = "Detail" });

app.MapControllerRoute(
    name: "baihoccuatoi",
    pattern: FriendlyUrl.MyLessonFrUrl,
    defaults: new { controller = "Lesson", action = "MyLessons" });

app.MapControllerRoute(
    name: "khoahoccuatoi",
    pattern: FriendlyUrl.MyCoursesFrUrl,
    defaults: new { controller = "Order", action = "MyCourses" });

app.MapControllerRoute(
    name: "donhangcuatoi",
    pattern: FriendlyUrl.MyOrdersFrUrl,
    defaults: new { controller = "Order", action = "MyOrders" });

app.MapControllerRoute(
    name: "thongtincuatoi",
    pattern: FriendlyUrl.MyProfileFrUrl,
    defaults: new { controller = "Account", action = "MyProfile" });

app.MapControllerRoute(
    name: "lienhe",
    pattern: FriendlyUrl.ContactFrUrl,
    defaults: new { controller = "Home", action = "Contact" });

app.MapControllerRoute(
    name: "muakhoahoc",
    pattern: FriendlyUrl.CourseShopFrUrl,
    defaults: new { controller = "Orders", action = "Checkout" });

app.MapControllerRoute(
    name: "danhsachtin",
    pattern: FriendlyUrl.NewListFrUrl,
    defaults: new { controller = "New", action = "List" });

app.MapControllerRoute(
    name: "tintuc",
    pattern: FriendlyUrl.NewDetailsFrUrl,
    defaults: new { controller = "New", action = "Details" });

app.MapControllerRoute(
    name: "vechungtoi",
    pattern: FriendlyUrl.AboutUsFrUrl,
    defaults: new { controller = "Home", action = "AboutUs" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

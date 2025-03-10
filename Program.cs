using ContactListWeb.Data;
using ContactListWeb.Models;
using ContactListWeb.Services;
using ContactListWeb.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container
// builder.Services.AddControllersWithViews();
// builder.Services.AddSession();
// builder.Services.AddHttpContextAccessor();
// builder.Services.AddScoped<AuthService>();
// builder.Services.AddScoped<ContactService>(sp =>
// {
//     var userId = sp.GetService<IHttpContextAccessor>().HttpContext.Session.GetInt32("UserId");
//     return userId.HasValue ? new ContactService(userId.Value, sp.GetService<IConfiguration>()) : null;
// });

// // Add authentication with cookies (simple session-based approach)
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options =>
//     {
//         options.LoginPath = "/Auth/Login";
//         options.LogoutPath = "/Auth/Logout";
//     });

// var app = builder.Build();

// // Configure the HTTP request pipeline
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseStaticFiles();
// app.UseRouting();
// app.UseSession();
// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// // Initialize the database
// await DatabaseConnection.InitializeDatabaseAsync();

// app.Run();
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Optional: keep property names as-is
    });
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ContactService>(sp =>
{
    var userId = sp.GetService<IHttpContextAccessor>().HttpContext.Session.GetInt32("UserId");
    return userId.HasValue ? new ContactService(userId.Value, sp.GetService<IConfiguration>()) : null;
});
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseInMemoryDatabase("TodoList"));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.Events.OnRedirectToLogin = context => // Customize redirect for API
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new { error = "Unauthorized" });
                return context.Response.WriteAsync(result);
            }
            context.Response.Redirect(context.RedirectUri); // Default redirect for non-API
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Skip HTTPS redirection in development unless explicitly needed
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

// Seed data (unchanged)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    context.Todos.AddRange(
        new Todo { Id = 1, Title = "Buy groceries", IsCompleted = false, UserId = 1 },
        new Todo { Id = 2, Title = "Finish project", IsCompleted = true, UserId = 1 },
        new Todo { Id = 3, Title = "Call mom", IsCompleted = false, UserId = 2 }
    );
    context.SaveChanges();
}

await DatabaseConnection.InitializeDatabaseAsync();

app.Run();
using ContactListWeb.Data;
using ContactListWeb.Models;
using ContactListWeb.Services;
using ContactListWeb.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ContactService>(sp =>
{
    var userId = sp.GetService<IHttpContextAccessor>().HttpContext?.Session.GetInt32("UserId");
    return userId.HasValue ? new ContactService(userId.Value, sp.GetService<IConfiguration>()) : null;
});
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseInMemoryDatabase("TodoList"));

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere1234567890123456");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "YourIssuer",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "YourAudience",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data
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
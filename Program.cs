using Microsoft.EntityFrameworkCore;
using Past2Future.Data;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register EF Core DbContext (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register MVC + Razor views
builder.Services.AddControllersWithViews();
// Session is used for lightweight user state (e.g., role, preferences)
builder.Services.AddSession();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Routing + auth middlewares
app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static assets from wwwroot (css, js, images)
app.UseRouting();

app.UseAuthorization();
app.UseSession();

// Default route: /Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
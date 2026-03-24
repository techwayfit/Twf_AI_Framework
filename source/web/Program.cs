using Microsoft.EntityFrameworkCore;
using TwfAiFramework.Web.Data;
using TwfAiFramework.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure workflow storage - choose between JSON file or SQLite
var useDatabase = builder.Configuration.GetValue<bool>("UseDatabase", false);

if (useDatabase)
{
    // SQLite Database option
    var connectionString = builder.Configuration.GetConnectionString("WorkflowDb") 
        ?? "Data Source=workflows.db";
    
    builder.Services.AddDbContext<WorkflowDbContext>(options =>
   options.UseSqlite(connectionString));
    
    builder.Services.AddScoped<IWorkflowRepository, SqliteWorkflowRepository>();
}
else
{
    // JSON File option (default)
    var dataDirectory = builder.Configuration.GetValue<string>("WorkflowDataDirectory") 
        ?? Path.Combine(Directory.GetCurrentDirectory(), "workflows");
    
    builder.Services.AddSingleton<IWorkflowRepository>(
        sp => new JsonFileWorkflowRepository(dataDirectory));
}

var app = builder.Build();

// Initialize database if using SQLite
if (useDatabase)
{
    using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
   db.Database.EnsureCreated();
    }
}

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
    pattern: "{controller=Workflow}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

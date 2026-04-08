using Microsoft.EntityFrameworkCore;
using TwfAiFramework.Web.Data;
using TwfAiFramework.Web.Repositories;
using TwfAiFramework.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddScoped<WorkflowDefinitionRunner>();

// SQLite is the only supported storage backend.
var connectionString = builder.Configuration.GetConnectionString("WorkflowDb")
    ?? "Data Source=workflows.db";

builder.Services.AddDbContext<WorkflowDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IWorkflowRepository, SqliteWorkflowRepository>();
builder.Services.AddScoped<INodeTypeRepository, SqliteNodeTypeRepository>();

var app = builder.Build();

// Ensure database schema is created and seed data on first run.
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    db.Database.EnsureCreated();

    // Seed built-in node type definitions (runs once when table is empty)
    var nodeRepo = scope.ServiceProvider.GetRequiredService<INodeTypeRepository>();
    await NodeTypeSeeder.SeedAsync(nodeRepo);

    // Import any workflow JSON files that have not yet been migrated to SQLite
    var workflowDir = builder.Configuration.GetValue<string>("WorkflowDataDirectory")
        ?? Path.Combine(Directory.GetCurrentDirectory(), "workflows");
    await WorkflowSeeder.SeedFromDirectoryAsync(workflowDir, db, logger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "workflow_subflow_designer",
    pattern: "{id:guid}/{subWorkflowId:guid}",
    defaults: new { controller = "Workflow", action = "Designer" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Workflow}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

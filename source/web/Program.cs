using Microsoft.EntityFrameworkCore;
using TwfAiFramework.Web.Data;
using TwfAiFramework.Web.Repositories;
using TwfAiFramework.Web.Services;
using TwfAiFramework.Web.Services.NodeFactory;
using TwfAiFramework.Web.Services.VariableResolution;
using TwfAiFramework.Web.Services.Execution;
using TwfAiFramework.Web.Services.GraphWalker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Register workflow execution services (NEW - Refactored architecture)
builder.Services.AddSingleton<IVariableResolver, TemplateVariableResolver>();
builder.Services.AddSingleton<INodeFactory, ReflectionNodeFactory>();
builder.Services.AddScoped<INodeExecutor, RetryableNodeExecutor>();
builder.Services.AddScoped<IWorkflowGraphWalker, WorkflowGraphWalker>();
builder.Services.AddScoped<WorkflowDefinitionRunner>();

// SQLite is the only supported storage backend.
var connectionString = builder.Configuration.GetConnectionString("WorkflowDb")
    ?? "Data Source=workflows.db";

builder.Services.AddDbContext<WorkflowDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IWorkflowRepository, SqliteWorkflowRepository>();
builder.Services.AddScoped<INodeTypeRepository, SqliteNodeTypeRepository>();
builder.Services.AddScoped<IWorkflowInstanceRepository, SqliteWorkflowInstanceRepository>();

var app = builder.Build();

// Ensure database schema is created and seed data on first run.
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    db.Database.EnsureCreated();

    // EnsureCreated won't add tables to an existing DB — apply additive DDL manually.
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS WorkflowInstances (
            Id                   TEXT    NOT NULL PRIMARY KEY,
            WorkflowDefinitionId TEXT    NOT NULL,
            WorkflowName         TEXT    NOT NULL,
            Status               TEXT    NOT NULL,
            StartedAt            TEXT    NOT NULL,
            CompletedAt          TEXT    NULL,
            JsonData             TEXT    NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_WorkflowInstances_WorkflowDefinitionId
            ON WorkflowInstances (WorkflowDefinitionId);
        CREATE INDEX IF NOT EXISTS IX_WorkflowInstances_StartedAt
            ON WorkflowInstances (StartedAt);
        CREATE INDEX IF NOT EXISTS IX_WorkflowInstances_Status
            ON WorkflowInstances (Status);
        """);

    // Additive DDL: add new columns to NodeTypes if they don't exist yet.
    // SQLite does not support IF NOT EXISTS for ALTER TABLE, so we catch duplicates.
    try { db.Database.ExecuteSqlRaw("ALTER TABLE NodeTypes ADD COLUMN IdPrefix TEXT NOT NULL DEFAULT 'node'"); }
    catch { /* column already exists */ }
    try { db.Database.ExecuteSqlRaw("ALTER TABLE NodeTypes ADD COLUMN FullTypeName TEXT"); }
    catch { /* column already exists */ }

    // Seed built-in node type definitions; re-seeds if IdPrefix is missing.
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

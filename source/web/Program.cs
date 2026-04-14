using Microsoft.EntityFrameworkCore;
using TwfAiFramework.Web.Data;
using TwfAiFramework.Web.Repositories;
using TwfAiFramework.Web.Services;
using TwfAiFramework.Web.Services.NodeFactory;
using TwfAiFramework.Web.Services.VariableResolution;
using TwfAiFramework.Web.Services.Execution;
using TwfAiFramework.Web.Services.GraphWalker;
using TwfAiFramework.Web.Middleware;
using TwfAiFramework.Web.Services.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// TODO: Enable FluentValidation once package compatibility is confirmed for .NET 10
// Register FluentValidation validators
// builder.Services.AddScoped<IValidator<WorkflowDefinition>, WorkflowDefinitionValidator>();
// builder.Services.AddScoped<IValidator<WorkflowRunRequest>, WorkflowRunRequestValidator>();
// builder.Services.AddScoped<IValidator<NodeTypeEntity>, NodeTypeEntityValidator>();

// Register global exception handler and ProblemDetails support
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Register workflow execution services (Refactored architecture)
builder.Services.AddSingleton<IVariableResolver, TemplateVariableResolver>();
builder.Services.AddSingleton<INodeFactory, ReflectionNodeFactory>();
builder.Services.AddScoped<INodeExecutor, RetryableNodeExecutor>();
builder.Services.AddScoped<IWorkflowGraphWalker, WorkflowGraphWalker>();
builder.Services.AddScoped<WorkflowDefinitionRunner>();

// Register database migration service
builder.Services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();

// SQLite is the only supported storage backend.
var connectionString = builder.Configuration.GetConnectionString("WorkflowDb")
    ?? "Data Source=workflows.db";

builder.Services.AddDbContext<WorkflowDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IWorkflowRepository, SqliteWorkflowRepository>();
builder.Services.AddScoped<INodeTypeRepository, SqliteNodeTypeRepository>();
builder.Services.AddScoped<IWorkflowInstanceRepository, SqliteWorkflowInstanceRepository>();

var app = builder.Build();

// Run database migrations and seeding on startup
using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        await migrationService.MigrateSchemaAsync();
        await migrationService.SeedDataAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database migration or seeding failed - application cannot start");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Add correlation ID middleware (before exception handler)
app.UseMiddleware<CorrelationIdMiddleware>();

// Add global exception handler
app.UseExceptionHandler();

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

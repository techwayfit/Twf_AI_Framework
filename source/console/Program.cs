// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using TwfAiFramework.Core;
using TwfAiFramework.Nodes.Control;
using TwfAiFramework.Nodes.Data;

using var logFactory = LoggerFactory.Create(b =>
       b.AddConsole().SetMinimumLevel(LogLevel.Debug));
var logger = logFactory.CreateLogger("Demo");

Console.WriteLine("📌 Concept 1: WorkflowData fluent API");
var data = WorkflowData.From("user_message", "Hello from FlowForge!")
    .Set("user_id", 42)
    .Set("language", "en")
    .Set("tags", new List<string> { "demo", "test" });

Console.WriteLine($"  data.GetString('user_message') = '{data.GetString("user_message")}'");
Console.WriteLine($"  data.Get<int>('user_id')       = {data.Get<int>("user_id")}");
Console.WriteLine($"  data.Has('language')           = {data.Has("language")}");
Console.WriteLine($"  data.Keys                      = [{string.Join(", ", data.Keys)}]");
Console.WriteLine($"  data.ToJson()                  = {data.ToJson()}");



Console.WriteLine("\n📌 Concept 2: Node chaining & branching");

var workflow = Workflow.Create("ConceptDemo")
    .UseLogger(logger)

    // Validate
    .AddNode(new FilterNode("Validate")
        .RequireNonEmpty("text")
        .MaxLength("text", 100))

    // Transform
    .AddNode(new TransformNode("Uppercase",
        d => d.Clone().Set("text_upper", d.GetString("text")?.ToUpper())))

    // Condition
    .AddNode(new ConditionNode("CheckLength",
        ("is_long", d => (d.GetString("text")?.Length ?? 0) > 20)))

    // Branch based on condition
    .Branch(
        condition: d => d.Get<bool>("is_long"),
        trueBranch: b => b.AddNode(new TransformNode("LongTextHandler",
            d => d.Clone().Set("result", $"LONG: {d.GetString("text_upper")}"))),
        falseBranch: b => b.AddNode(new TransformNode("ShortTextHandler",
            d => d.Clone().Set("result", $"SHORT: {d.GetString("text_upper")}")))
    )

    // Log result
    .AddNode(LogNode.Keys("Final", "result", "is_long"))

    .OnComplete(r =>
    {
        Console.WriteLine($"  ✅ Pipeline completed: result = '{r.Data.GetString("result")}'");
        Console.WriteLine(r.Summary());
    });

await workflow.RunAsync(WorkflowData.From("text", "Hello FlowForge!"));
await workflow.RunAsync(WorkflowData.From("text", "Manas!"));
await workflow.RunAsync(WorkflowData.From("text", "This is a longer text that exceeds 20 chars"));




Console.ReadLine();
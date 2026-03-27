using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TwfAiFramework.Core;
using TwfAiFramework.Nodes.AI;
using TwfAiFramework.Nodes.Control;
using TwfAiFramework.Nodes.Data;

namespace _001_CustomerSupportChatbot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatApiController : ControllerBase
{
    private readonly ILogger<ChatApiController> _logger;
    private readonly IConfiguration _configuration;
    private static readonly Dictionary<string, WorkflowContext> _sessions = new();

    public ChatApiController(ILogger<ChatApiController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = Constants.Messages.EmptyMessage });
            }

            // Get or create session context
            var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
            if (!_sessions.ContainsKey(sessionId))
            {
                var context = new WorkflowContext("CustomerSupportBot", _logger);
                context.SetState("company_name", Constants.CompanyName);
                context.SetState("support_tier", Constants.DefaultResponseType);
                context.SetState("bot_type", request.BotType ?? "support");
                _sessions[sessionId] = context;
            }
            var botType = request.BotType;
            var sessionContext = _sessions[sessionId];

            // Get API key from configuration
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, new { error = Constants.Messages.OpenApiKeyNotConfigured });
            }

            // Build the customer support workflow
            var workflow = botType == "basic" ?
                BuildSimpleWorkFlow(apiKey) :
                botType == "safetycheck" ? BuildWorkFlowWithSafetyCheck(apiKey) :
                botType == "sentiment" ? BuildWorkFlowWithSentimentAnalyzer(apiKey) :
                BuildCustomerSupportWorkflow(apiKey);

            // Prepare input data
            var input = WorkflowData.From("user_message", request.Message)
                         .Set("company_name", Constants.CompanyName);

            // Run the workflow
            var result = await workflow.RunAsync(input, sessionContext);

            if (result.IsSuccess)
            {
                var response = result.Data.GetString("llm_response") ?? Constants.Messages.RequestCouldNotProcessed;
                var responseType = result.Data.GetString("response_type") ?? Constants.DefaultResponseType;
                var sentiment = result.Data.GetString("sentiment");

                return Ok(new ChatResponse
                {
                    SessionId = sessionId,
                    Message = response,
                    ResponseType = responseType,
                    Sentiment = sentiment,
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogError("Workflow failed: {Error}", result.ErrorMessage);
                //return StatusCode(500, new { error = Constants.Messages.FailedToProcessRequest });
                return StatusCode(500, new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpDelete("session/{sessionId}")]
    public IActionResult ClearSession(string sessionId)
    {
        if (_sessions.ContainsKey(sessionId))
        {
            _sessions.Remove(sessionId);
            return Ok(new { message = Constants.Messages.SessionCleared });
        }
        return NotFound(new { error = Constants.Messages.SessionNotFound });
    }
    private Workflow BuildWorkflow(string apiKey, out LlmConfig llmConfig)
    {
        llmConfig = LlmConfig.OpenAI(apiKey);
        var workflow = Workflow.Create("BasicAIResponse").UseLogger(_logger);
        var inputFilter = new FilterNode("ValidateInput").RequireNonEmpty("user_message").MaxLength("user_message", 500);
        workflow.AddNode(inputFilter);
        return workflow;
    }

    private Workflow BuildSimpleWorkFlow(string apiKey)
    {
        LlmConfig llmConfig;
        Workflow workflow = BuildWorkflow(apiKey, out llmConfig);
        var promptNode = new PromptBuilderNode(
            name: "BasicPrompt",
            promptTemplate: Constants.Prompts.BasicSupportPrompt,
            systemTemplate: Constants.Prompts.BasicSupportSystemTemplate
        );
        workflow.AddNode(promptNode);
        var llmNode = new LlmNode("BasicResponseLLMNode", llmConfig with { MaxTokens = 300 });
        workflow.AddNode(llmNode, NodeOptions.WithRetry(2));


        return workflow;
    }

    private Workflow BuildWorkFlowWithSafetyCheck(string apiKey)
    {
        LlmConfig llmConfig;
        Workflow workflow = BuildWorkflow(apiKey, out llmConfig);
        var safetyCheckPromptNode = new PromptBuilderNode(
            name: "SafetyCheckPrompt",
            promptTemplate: Constants.Prompts.SafetyCheckPrompt,
            systemTemplate: Constants.Prompts.SafetyCheckSystemPrompt);
        var safetyCheckLlmNode = new LlmNode("SafetyChecker", llmConfig with { MaxTokens = 100 });
        workflow.AddNode(safetyCheckPromptNode)
            .AddNode(safetyCheckLlmNode, NodeOptions.WithRetry(2))
            .AddNode(OutputParserNode.WithMapping("SafetyResponseParser", ("is_safe", "isSafe"), ("reason", "safetyReason")));

        Action<Workflow> safeFlow = flow =>
        {
            var promptNode = new PromptBuilderNode(
                name: "BasicPrompt",
                promptTemplate: Constants.Prompts.BasicSupportPrompt,
                systemTemplate: Constants.Prompts.BasicSupportSystemTemplate
                );
            workflow.AddNode(promptNode);
            var llmNode = new LlmNode("BasicResponseLLMNode", llmConfig with { MaxTokens = 300 });
            workflow.AddNode(llmNode, NodeOptions.WithRetry(2));
        };
        Action<Workflow> unSafeFlow = flow =>
        {
            flow.AddStep("RejectUnsafe", (data, _) =>
            Task.FromResult(data.Set("llm_response", Constants.Prompts.UnSafeResponse).Set("response_type", "rejected")));
        };

        workflow.Branch(data => data.Get<bool>("isSafe"), safeFlow, unSafeFlow);


        return workflow;
    }

    private Workflow BuildWorkFlowWithSentimentAnalyzer(string apiKey)
    {
        LlmConfig llmConfig;
        Workflow workflow = BuildWorkflow(apiKey, out llmConfig);
        var safetyCheckPromptNode = new PromptBuilderNode(
            name: "SafetyCheckPrompt",
            promptTemplate: Constants.Prompts.SafetyCheckPrompt,
            systemTemplate: Constants.Prompts.SafetyCheckSystemPrompt);
        var safetyCheckLlmNode = new LlmNode("SafetyChecker", llmConfig with { MaxTokens = 100 });
        workflow.AddNode(safetyCheckPromptNode)
            .AddNode(safetyCheckLlmNode, NodeOptions.WithRetry(2))
            .AddNode(OutputParserNode.WithMapping("SafetyResponseParser", ("is_safe", "isSafe"), ("reason", "safetyReason")));

        Action<Workflow> safeFlow = flow =>
        {
            var sentimentPromptNode = new PromptBuilderNode(
            name: "SentimentAnalyzer",
            promptTemplate: Constants.Prompts.SentimentPrompt);
            var sentimentCheckLlmNode = new LlmNode("SentimentAnalyzer", llmConfig with { MaxTokens = 100 });
            workflow.AddNode(sentimentPromptNode)
                .AddNode(sentimentCheckLlmNode, NodeOptions.WithRetry(2))
                .AddNode(OutputParserNode.WithMapping("SentimentParser", ("sentiment", "sentiment"), ("score", "anger_score")));
            Action<Workflow> angrySentimentFlow = wflow =>
            {
                wflow.AddNode(new PromptBuilderNode(
                    name: "EscalationPrompt",
                    promptTemplate: Constants.Prompts.SentimentEscalationPrompt,
                    systemTemplate: Constants.Prompts.SentimentEscalationSystemPrompt
                    ))
                .AddNode(new LlmNode("EscalationResponder", llmConfig with
                {
                    MaintainHistory = true,
                    MaxTokens = 500
                }))
                .AddStep("TagEscalation", (data, _) =>
                Task.FromResult(data.Set("response_type", "escalation")));
            };
            Action<Workflow> normalSentimentFlow = wflow =>
            {
                wflow.AddNode(new PromptBuilderNode(
                    name: "StandardResponsePrompt",
                    promptTemplate: Constants.Prompts.SentimentNormalPrompt,
                    systemTemplate: Constants.Prompts.SentimentNormalSystemPrompt
                    ))
                .AddNode(new LlmNode("StandardResponder", llmConfig with
                {
                    MaintainHistory = true,
                    MaxTokens = 500
                }))
                .AddStep("TagStandard", (data, _) =>
                Task.FromResult(data.Set("response_type", "standard")));
            };

            flow.Branch(data => data.GetString("sentiment") == "angry" || data.Get<int>("anger_score") >= 7,
                angrySentimentFlow, normalSentimentFlow);

        };
        Action<Workflow> unSafeFlow = flow =>
        {
            flow.AddStep("RejectUnsafe", (data, _) =>
            Task.FromResult(data.Set("llm_response", Constants.Prompts.UnSafeResponse).Set("response_type", "rejected")));
        };

        workflow.Branch(data => data.Get<bool>("isSafe"), safeFlow, unSafeFlow);




        return workflow;
    }

    private Workflow BuildCustomerSupportWorkflow(string apiKey)
    {
        var llm = LlmConfig.OpenAI(apiKey);

        return Workflow.Create("CustomerSupportBot")
                  .UseLogger(_logger)
           // 1. Validate input
           .AddNode(new FilterNode("ValidateInput")
          .RequireNonEmpty("user_message")
            .MaxLength("user_message", 2000))

          // 2. Safety check
          .AddNode(new PromptBuilderNode(
                      name: "SafetyCheckPrompt",
         promptTemplate: """Classify if this customer message is safe to respond to. Message: "{{user_message}}" Respond ONLY with JSON: {"is_safe": true/false, "reason": "brief reason"}""",
            systemTemplate: "You are a content safety classifier. Be concise."
                  ))
                  .AddNode(new LlmNode("SafetyChecker", llm with { MaxTokens = 100 }),
                 NodeOptions.WithRetry(2))
      .AddNode(OutputParserNode.WithMapping("SafetyParser",
            ("is_safe", "is_safe"),
                      ("reason", "safety_reason")))

                  // 3. Branch: safe vs unsafe
                  .Branch(data => data.Get<bool>("is_safe"),
              trueBranch: safe => safe
                 // Sentiment analysis
                 .AddNode(new PromptBuilderNode(
            name: "SentimentAnalyzer",
           promptTemplate: "Analyze the sentiment: \"{{user_message}}\". " +
          "JSON: {\"sentiment\": \"positive|neutral|negative|angry\", \"score\": 1-10}"))
          .AddNode(new LlmNode("SentimentAnalyzer", llm with { MaxTokens = 100 }))
                    .AddNode(OutputParserNode.WithMapping("SentimentParser",
                ("sentiment", "sentiment"),
                    ("score", "anger_score")))

              // Branch by sentiment
              .Branch(data => data.GetString("sentiment") == "angry" || data.Get<int>("anger_score") >= 7,
           trueBranch: angry => angry
                 .AddNode(new PromptBuilderNode(
              name: "EscalationPrompt",
               promptTemplate: @"
Customer is angry. Be empathetic and offer concrete help.
Company: {{company_name}}
Message: {{user_message}}

Provide a warm, empathetic response. Use formatting:
- Use **bold** for important points
- Use bullet points (-) to list action items or options
- Be conversational and understanding
- Offer to escalate if needed

Format your response in a clear, well-structured way.",
               systemTemplate: "You are an empathetic senior support agent. Format responses clearly with markdown-like syntax."
              ))
             .AddNode(new LlmNode("EscalationResponder", llm with
             {
                 MaintainHistory = true,
                 MaxTokens = 500
             }))
           .AddStep("TagEscalation", (data, _) =>
           Task.FromResult(data.Set("response_type", "escalation"))),

                   falseBranch: normal => normal
                    .AddNode(new PromptBuilderNode(
       name: "StandardResponsePrompt",
            promptTemplate: @"
Help this customer professionally.
Company: {{company_name}}
Message: {{user_message}}

Provide a helpful, professional response. Use formatting:
- Use **bold** for key information or product names
- Use bullet points (-) to list features, steps, or options
- Use numbered lists (1., 2., 3.) for sequential instructions
- Keep it conversational and friendly

Format your response in a clear, well-structured way.",
            systemTemplate: "You are a helpful support agent for {{company_name}}. Format responses clearly with markdown-like syntax."
             ))
           .AddNode(new LlmNode("StandardResponder", llm with
           {
               MaintainHistory = true,
               MaxTokens = 500
           }))
           .AddStep("TagStandard", (data, _) =>
                 Task.FromResult(data.Set("response_type", "standard")))
          ),

        falseBranch: unsafeBranch => unsafeBranch
           .AddStep("RejectUnsafe", (data, _) =>
        Task.FromResult(
         data.Set("llm_response",
            "I'm unable to process that request. Please contact **support@techflow.com** for assistance.")
       .Set("response_type", "rejected")))
                )
                .AddNode(MemoryNode.Write("last_response_type"));
    }
}

public class ChatRequest
{
    public string? SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? BotType { get; set; }
}

public class ChatResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string? Sentiment { get; set; }
    public DateTime Timestamp { get; set; }
}

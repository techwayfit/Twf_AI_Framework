using TwfAiFramework.Web.Models;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Provides parameter schemas for all node types
/// </summary>
public static class NodeSchemaProvider
{
    public static Dictionary<string, NodeParameterSchema> GetAllSchemas()
    {
        return new Dictionary<string, NodeParameterSchema>
        {
            // AI Nodes
            ["LlmNode"] = new()
            {
                NodeType = "LlmNode",
                Description = "Calls a Large Language Model API to generate responses",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Input prompt or message" }
                },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data, Description = "LLM response" }
                },
                Capabilities = new()
                {
                    SupportsRetry = true,
                    SupportsTimeout = true
                },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 0, MinValue = 0, MaxValue = 10, Description = "Number of retry attempts on failure" },
                    new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 1000, MinValue = 100, MaxValue = 60000, Description = "Delay between retries" },
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 30000, MinValue = 1000, MaxValue = 300000, Description = "Maximum execution time" },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false, Description = "Continue workflow if this node fails" }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "openai",
                        Options = new() {
                            new() { Value = "openai", Label = "OpenAI" },
                            new() { Value = "azure", Label = "Azure OpenAI" },
                            new() { Value = "anthropic", Label = "Anthropic" },
                            new() { Value = "ollama", Label = "Ollama (Local)" }
                        }
                    },
                    new() { Name = "model", Label = "Model", Type = ParameterType.Text, Required = true, DefaultValue = "gpt-4o", Placeholder = "e.g., gpt-4o, claude-3-opus" },
                    new() { Name = "apiKey", Label = "API Key", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty to use environment variable" },
                    new() { Name = "apiUrl", Label = "API URL", Type = ParameterType.Text, Required = false, Placeholder = "Custom API endpoint (optional)" },
                    new() { Name = "systemPrompt", Label = "System Prompt", Type = ParameterType.TextArea, Required = false, Placeholder = "You are a helpful assistant..." },
                    new() { Name = "temperature", Label = "Temperature", Type = ParameterType.Number, Required = false, DefaultValue = 0.7, MinValue = 0, MaxValue = 2 },
                    new() { Name = "maxTokens", Label = "Max Tokens", Type = ParameterType.Number, Required = false, DefaultValue = 1000, MinValue = 1, MaxValue = 128000 },
                    new() { Name = "maintainHistory", Label = "Maintain Chat History", Type = ParameterType.Boolean, Required = false, DefaultValue = false }
                }
            },

            ["PromptBuilderNode"] = new()
            {
                NodeType = "PromptBuilderNode",
                Description = "Builds prompts from templates with variable interpolation",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
                },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data }
                },
                Capabilities = new(),
                ExecutionOptions = new()
                {
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "promptTemplate", Label = "Prompt Template", Type = ParameterType.TextArea, Required = true,
                        Placeholder = "Use {{variable}} syntax, e.g., 'Summarize: {{content}}'" },
                    new() { Name = "systemTemplate", Label = "System Template", Type = ParameterType.TextArea, Required = false,
                        Placeholder = "Optional system prompt template" }
                }
            },

            ["EmbeddingNode"] = new()
            {
                NodeType = "EmbeddingNode",
                Description = "Generates embeddings for text using various embedding models",
                InputPorts = new()
                {
    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
              },
                OutputPorts = new()
            {
                  new() { Id = "output", Label = "Output", Type = PortType.Data }
         },
                Capabilities = new()
                {
                    SupportsRetry = true,
                    SupportsTimeout = true
                },
                ExecutionOptions = new()
        {
         new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 0, MinValue = 0, MaxValue = 10 },
               new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 1000, MinValue = 100, MaxValue = 60000 },
new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 30000, MinValue = 1000, MaxValue = 300000 },
       new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
            {
new() { Name = "model", Label = "Embedding Model", Type = ParameterType.Select, Required = true, DefaultValue = "text-embedding-3-small",
            Options = new() {
       new() { Value = "text-embedding-3-small", Label = "OpenAI Embedding Small" },
      new() { Value = "text-embedding-3-large", Label = "OpenAI Embedding Large" },
       new() { Value = "text-embedding-ada-002", Label = "OpenAI Ada 002 (Legacy)" }
            }
   },
    new() { Name = "apiKey", Label = "API Key", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty to use environment variable" },
            new() { Name = "apiUrl", Label = "API URL", Type = ParameterType.Text, Required = false, DefaultValue = "https://api.openai.com/v1/embeddings" }
     }
            },

            ["OutputParserNode"] = new()
            {
                NodeType = "OutputParserNode",
                Description = "Parses and extracts structured data from LLM outputs",
                InputPorts = new()
           {
     new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
                },
                OutputPorts = new()
           {
       new() { Id = "output", Label = "Output", Type = PortType.Data }
   },
                Capabilities = new(),
                ExecutionOptions = new()
        {
           new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
  },
                Parameters = new()
 {
     new() { Name = "fieldMapping", Label = "Field Mapping (JSON)", Type = ParameterType.Json, Required = false,
   Placeholder = "{\"sentiment\": \"sentiment\", \"score\": \"confidence\"}" },
  new() { Name = "strictMode", Label = "Strict Mode", Type = ParameterType.Boolean, Required = false, DefaultValue = false }
      }
            },

            // Control Nodes
            ["ConditionNode"] = new()
            {
                NodeType = "ConditionNode",
                Description = "Routes workflow based on conditional expressions",
                InputPorts = new()
         {
             new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
                },
                OutputPorts = new()
                {
           new() { Id = "success", Label = "Success", Type = PortType.Conditional, Description = "Condition evaluates to true" },
        new() { Id = "failure", Label = "Failure", Type = PortType.Conditional, Description = "Condition evaluates to false or error" }
  },
                Capabilities = new()
                {
                    SupportsConditionalRouting = true,
                    SupportsMultipleOutputs = true
                },
                ExecutionOptions = new()
       {
        new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
         },
                Parameters = new()
       {
   new() { Name = "condition", Label = "Condition Expression", Type = ParameterType.Text, Required = true,
    Placeholder = "e.g., score > 5 && status == 'active'",
     Description = "Expression that evaluates to true or false" }
  }
            },

            ["DelayNode"] = new()
            {
                NodeType = "DelayNode",
                Description = "Introduces a delay in workflow execution",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
                },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data }
                },
                Capabilities = new(),
                ExecutionOptions = new()
                {
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "delayMs", Label = "Delay (milliseconds)", Type = ParameterType.Number, Required = true, DefaultValue = 1000, MinValue = 0, MaxValue = 60000 },
                    new() { Name = "reason", Label = "Reason", Type = ParameterType.Text, Required = false, Placeholder = "e.g., Rate limit" }
                }
            },

            ["MergeNode"] = new()
            {
                NodeType = "MergeNode",
                Description = "Merges multiple data values into a single output",
                InputPorts = new()
    {
       new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
       },
                OutputPorts = new()
       {
        new() { Id = "output", Label = "Output", Type = PortType.Data }
      },
                Capabilities = new(),
                ExecutionOptions = new()
  {
       new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
      },
                Parameters = new()
   {
    new() { Name = "sourceKeys", Label = "Source Keys (comma-separated)", Type = ParameterType.Text, Required = true, Placeholder = "key1, key2, key3" },
    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, Placeholder = "merged_output" },
         new() { Name = "separator", Label = "Separator", Type = ParameterType.Text, Required = false, DefaultValue = "\n" }
    }
            },

            ["LogNode"] = new()
            {
                NodeType = "LogNode",
                Description = "Logs workflow data at a specific checkpoint",
                InputPorts = new()
          {
    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
 },
                OutputPorts = new()
          {
       new() { Id = "output", Label = "Output", Type = PortType.Data }
         },
                Capabilities = new(),
                ExecutionOptions = new()
     {
        new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
   },
                Parameters = new()
        {
      new() { Name = "label", Label = "Checkpoint Label", Type = ParameterType.Text, Required = true, Placeholder = "Checkpoint name" },
     new() { Name = "keysToLog", Label = "Keys to Log (comma-separated)", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty for all keys" },
    new() { Name = "logLevel", Label = "Log Level", Type = ParameterType.Select, Required = false, DefaultValue = "Information",
        Options = new() {
   new() { Value = "Trace", Label = "Trace" },
      new() { Value = "Debug", Label = "Debug" },
      new() { Value = "Information", Label = "Information" },
      new() { Value = "Warning", Label = "Warning" },
       new() { Value = "Error", Label = "Error" }
     }
   }
  }
            },

            // Data Nodes
            ["TransformNode"] = new()
            {
                NodeType = "TransformNode",
                Description = "Transforms data using various transformation strategies",
                InputPorts = new()
         {
     new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
      },
                OutputPorts = new()
  {
       new() { Id = "output", Label = "Output", Type = PortType.Data }
       },
                Capabilities = new(),
                ExecutionOptions = new()
             {
   new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
   },
                Parameters = new()
            {
       new() { Name = "transformType", Label = "Transform Type", Type = ParameterType.Select, Required = true, DefaultValue = "custom",
   Options = new() {
    new() { Value = "rename", Label = "Rename Key" },
        new() { Value = "select", Label = "Select Key" },
           new() { Value = "concat", Label = "Concatenate Strings" },
   new() { Value = "custom", Label = "Custom (requires code)" }
    }
        },
        new() { Name = "fromKey", Label = "From Key", Type = ParameterType.Text, Required = false, Placeholder = "Source key name" },
  new() { Name = "toKey", Label = "To Key", Type = ParameterType.Text, Required = false, Placeholder = "Target key name" },
    new() { Name = "keys", Label = "Keys (comma-separated)", Type = ParameterType.Text, Required = false, Placeholder = "For concat: key1, key2, key3" },
new() { Name = "separator", Label = "Separator", Type = ParameterType.Text, Required = false, DefaultValue = " " }
        }
            },

            ["FilterNode"] = new()
            {
                NodeType = "FilterNode",
                Description = "Validates and filters workflow data",
                InputPorts = new()
 {
   new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
 },
                OutputPorts = new()
  {
       new() { Id = "output", Label = "Output", Type = PortType.Data }
       },
                Capabilities = new(),
                ExecutionOptions = new()
     {
 new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
       },
                Parameters = new()
        {
 new() { Name = "throwOnFail", Label = "Throw on Validation Failure", Type = ParameterType.Boolean, Required = false, DefaultValue = true },
     new() { Name = "requiredKeys", Label = "Required Keys (comma-separated)", Type = ParameterType.Text, Required = false, Placeholder = "email, name, phone" },
   new() { Name = "maxLengths", Label = "Max Lengths (JSON)", Type = ParameterType.Json, Required = false, Placeholder = "{\"name\": 100, \"bio\": 500}" }
        }
            },

            ["ChunkTextNode"] = new()
            {
                NodeType = "ChunkTextNode",
                Description = "Splits text into smaller chunks for processing",
                InputPorts = new()
       {
       new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
        },
                OutputPorts = new()
  {
       new() { Id = "output", Label = "Output", Type = PortType.Data }
      },
                Capabilities = new(),
                ExecutionOptions = new()
  {
    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
              },
                Parameters = new()
      {
 new() { Name = "chunkSize", Label = "Chunk Size", Type = ParameterType.Number, Required = false, DefaultValue = 500, MinValue = 50, MaxValue = 10000 },
   new() { Name = "overlap", Label = "Overlap", Type = ParameterType.Number, Required = false, DefaultValue = 50, MinValue = 0, MaxValue = 1000 },
  new() { Name = "strategy", Label = "Chunking Strategy", Type = ParameterType.Select, Required = false, DefaultValue = "Character",
    Options = new() {
    new() { Value = "Character", Label = "By Character" },
         new() { Value = "Word", Label = "By Word" },
new() { Value = "Sentence", Label = "By Sentence" }
      }
     }
     }
            },

            ["MemoryNode"] = new()
            {
                NodeType = "MemoryNode",
                Description = "Reads from and writes to workflow memory storage",
                InputPorts = new()
      {
     new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
    },
                OutputPorts = new()
        {
     new() { Id = "output", Label = "Output", Type = PortType.Data }
    },
                Capabilities = new(),
                ExecutionOptions = new()
     {
      new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
    },
                Parameters = new()
    {
    new() { Name = "mode", Label = "Mode", Type = ParameterType.Select, Required = true, DefaultValue = "Read",
     Options = new() {
       new() { Value = "Read", Label = "Read from Memory" },
       new() { Value = "Write", Label = "Write to Memory" }
       }
        },
           new() { Name = "keys", Label = "Keys (comma-separated)", Type = ParameterType.Text, Required = true, Placeholder = "user_id, session_state" }
        }
            },

            // IO Nodes
            ["HttpRequestNode"] = new()
            {
                NodeType = "HttpRequestNode",
                Description = "Makes HTTP requests to external APIs",
                InputPorts = new()
    {
  new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
   },
                OutputPorts = new()
          {
 new() { Id = "output", Label = "Output", Type = PortType.Data, Description = "HTTP response" },
    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error", Description = "HTTP error response" }
          },
                Capabilities = new()
                {
                    SupportsTimeout = true,
                    SupportsRetry = true,
                    SupportsConditionalRouting = true
                },
                ExecutionOptions = new()
       {
         new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 0, MinValue = 0, MaxValue = 10 },
 new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 1000, MinValue = 100, MaxValue = 60000 },
            new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 30000, MinValue = 1000, MaxValue = 300000 },
        new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
         },
                Parameters = new()
      {
        new() { Name = "method", Label = "HTTP Method", Type = ParameterType.Select, Required = true, DefaultValue = "GET",
     Options = new() {
  new() { Value = "GET", Label = "GET" },
  new() { Value = "POST", Label = "POST" },
    new() { Value = "PUT", Label = "PUT" },
      new() { Value = "PATCH", Label = "PATCH" },
  new() { Value = "DELETE", Label = "DELETE" }
            }
 },
 new() { Name = "urlTemplate", Label = "URL Template", Type = ParameterType.Text, Required = true,
 Placeholder = "https://api.example.com/users/{{user_id}}" },
        new() { Name = "headers", Label = "Headers (JSON)", Type = ParameterType.Json, Required = false,
    Placeholder = "{\"Authorization\": \"Bearer token\", \"Content-Type\": \"application/json\"}" },
 new() { Name = "body", Label = "Request Body Template", Type = ParameterType.TextArea, Required = false,
      Placeholder = "Use {{variable}} syntax" },
     new() { Name = "timeout", Label = "Timeout (seconds)", Type = ParameterType.Number, Required = false, DefaultValue = 30, MinValue = 1, MaxValue = 300 },
new() { Name = "throwOnError", Label = "Throw on HTTP Error", Type = ParameterType.Boolean, Required = false, DefaultValue = true }
       }
            },

            // Special Control Nodes (Phase 5)
            ["StartNode"] = new()
            {
                NodeType = "StartNode",
                Description = "Entry point for workflow execution. Every workflow should start here.",
                InputPorts = new(),  // No input ports
                OutputPorts = new()
           {
   new() { Id = "output", Label = "Start", Type = PortType.Control, Description = "Workflow begins here" }
 },
          Capabilities = new(),
          ExecutionOptions = new(),
     Parameters = new()
    {
    new() { Name = "description", Label = "Description", Type = ParameterType.Text, Required = false,
  Placeholder = "e.g., Process customer feedback workflow",
     Description = "Optional description of what this workflow does" }
    }
            },
   
            ["EndNode"] = new()
  {
         NodeType = "EndNode",
          Description = "Exit point for workflow execution. Marks successful completion of the workflow.",
                InputPorts = new()
  {
      new() { Id = "input", Label = "End", Type = PortType.Control, Required = true, Description = "Workflow ends here" }
                },
                OutputPorts = new(),  // No output ports
        Capabilities = new(),
     ExecutionOptions = new(),
        Parameters = new()
  {
    new() { Name = "status", Label = "Completion Status", Type = ParameterType.Select, Required = false,
         DefaultValue = "success",
       Options = new() {
      new() { Value = "success", Label = "Success" },
           new() { Value = "completed", Label = "Completed" },
new() { Value = "finished", Label = "Finished" }
 },
               Description = "Status to mark when this endpoint is reached" },
       new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false,
     Placeholder = "e.g., final_result",
           Description = "WorkflowData key to return as the final result" }
    }
            },

        ["LoopNode"] = new()
    {
    NodeType = "LoopNode",
          Description = "Iterates over a collection and processes each item through a sub-workflow",
     InputPorts = new()
  {
       new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Collection to iterate over" }
         },
        OutputPorts = new()
   {
   new() { Id = "iteration", Label = "For Each Item", Type = PortType.Control, Description = "Executes for each item in the collection (sub-workflow)" },
         new() { Id = "completed", Label = "After Loop", Type = PortType.Data, Description = "Executes after all iterations complete (processed results)" }
   },
         Capabilities = new()
 {
          SupportsSubWorkflow = true,
      SupportsMultipleOutputs = true
     },
       ExecutionOptions = new()
  {
  new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false, 
     Description = "Continue processing remaining items if one fails" }
                },
           Parameters = new()
      {
 new() { Name = "itemsKey", Label = "Items Key", Type = ParameterType.Text, Required = true,
            Placeholder = "e.g., documents, users, products",
      Description = "WorkflowData key containing the collection to iterate over" },
  new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true,
        Placeholder = "e.g., processed_items, summaries, results",
    Description = "Where to store the array of processed results" },
   new() { Name = "loopItemKey", Label = "Loop Item Variable", Type = ParameterType.Text, Required = false,
         DefaultValue = "__loop_item__",
      Placeholder = "__loop_item__",
  Description = "Variable name for current item in sub-workflow (default: __loop_item__)" },
    new() { Name = "maxIterations", Label = "Max Iterations", Type = ParameterType.Number, Required = false,
          DefaultValue = 0, MinValue = 0, MaxValue = 10000,
  Description = "Maximum number of iterations (0 = unlimited)" }
     }
      },

            ["ParallelNode"] = new()
            {
                NodeType = "ParallelNode",
         Description = "Executes multiple branches simultaneously and merges results",
       InputPorts = new()
    {
              new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Input data for all parallel branches" }
 },
          OutputPorts = new()
       {
  new() { Id = "branch1", Label = "Branch 1", Type = PortType.Control, Description = "First parallel execution branch (sub-workflow)" },
        new() { Id = "branch2", Label = "Branch 2", Type = PortType.Control, Description = "Second parallel execution branch (sub-workflow)" },
       new() { Id = "branch3", Label = "Branch 3", Type = PortType.Control, Description = "Third parallel execution branch (sub-workflow)" },
new() { Id = "completed", Label = "After All", Type = PortType.Data, Description = "Executes after all branches complete (merged results)" }
   },
         Capabilities = new()
           {
         SupportsSubWorkflow = true,
     SupportsMultipleOutputs = true
 },
   ExecutionOptions = new()
      {
   new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false,
       Description = "Continue if one branch fails" }
     },
       Parameters = new()
     {
       new() { Name = "branchCount", Label = "Number of Branches", Type = ParameterType.Number, Required = false,
    DefaultValue = 3, MinValue = 2, MaxValue = 5,
    Description = "Number of parallel execution branches (2-5)" },
    new() { Name = "mergeStrategy", Label = "Merge Strategy", Type = ParameterType.Select, Required = false,
        DefaultValue = "overwrite",
       Options = new() {
     new() { Value = "overwrite", Label = "Overwrite (later values win)" },
 new() { Value = "preserve", Label = "Preserve (earlier values win)" },
      new() { Value = "array", Label = "Collect as Array" }
  },
        Description = "How to merge results from parallel branches" }
      }
},

       ["BranchNode"] = new()
            {
       NodeType = "BranchNode",
   Description = "Routes to different paths based on value matching (switch/case)",
     InputPorts = new()
         {
    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Input value to evaluate" }
       },
   OutputPorts = new()
        {
       new() { Id = "case1", Label = "Case 1", Type = PortType.Conditional, Condition = "case1", Description = "Matches first case value" },
     new() { Id = "case2", Label = "Case 2", Type = PortType.Conditional, Condition = "case2", Description = "Matches second case value" },
    new() { Id = "case3", Label = "Case 3", Type = PortType.Conditional, Condition = "case3", Description = "Matches third case value" },
      new() { Id = "default", Label = "Default", Type = PortType.Conditional, Condition = "default", Description = "No case matches (fallback)" }
   },
                Capabilities = new()
             {
      SupportsConditionalRouting = true,
      SupportsMultipleOutputs = true
       },
    ExecutionOptions = new()
     {
     new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
     },
     Parameters = new()
              {
 new() { Name = "valueKey", Label = "Value Key", Type = ParameterType.Text, Required = true,
     Placeholder = "e.g., status, type, category",
          Description = "WorkflowData key containing the value to match" },
      new() { Name = "case1Value", Label = "Case 1 Value", Type = ParameterType.Text, Required = false,
        Placeholder = "e.g., approved, success, high",
  Description = "Value that matches Case 1" },
new() { Name = "case2Value", Label = "Case 2 Value", Type = ParameterType.Text, Required = false,
    Placeholder = "e.g., pending, processing, medium",
    Description = "Value that matches Case 2" },
        new() { Name = "case3Value", Label = "Case 3 Value", Type = ParameterType.Text, Required = false,
 Placeholder = "e.g., rejected, failed, low",
     Description = "Value that matches Case 3" },
       new() { Name = "caseSensitive", Label = "Case Sensitive", Type = ParameterType.Boolean, Required = false,
  DefaultValue = false,
           Description = "Whether value matching is case-sensitive" }
     }
        }
        };
    }
}

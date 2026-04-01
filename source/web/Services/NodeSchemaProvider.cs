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

            ["ErrorRouteNode"] = new()
            {
                NodeType = "ErrorRouteNode",
                Description = "Routes to success/error outputs based on error indicators",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true }
                },
                OutputPorts = new()
                {
                    new() { Id = "success", Label = "Success", Type = PortType.Conditional, Condition = "success", Description = "No error detected" },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error", Description = "Error detected" }
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
                    new() { Name = "errorMessageKey", Label = "Error Message Key", Type = ParameterType.Text, Required = false, DefaultValue = "error_message",
                        Placeholder = "e.g., error_message, last_error",
                        Description = "WorkflowData key containing an error message" },
                    new() { Name = "statusCodeKey", Label = "Status Code Key", Type = ParameterType.Text, Required = false, DefaultValue = "http_status_code",
                        Placeholder = "e.g., http_status_code",
                        Description = "WorkflowData key containing a status code" },
                    new() { Name = "errorStatusThreshold", Label = "Error Status Threshold", Type = ParameterType.Number, Required = false, DefaultValue = 400, MinValue = 100, MaxValue = 599,
                        Description = "Status codes >= threshold are treated as error" }
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

            ["DataMapperNode"] = new()
            {
                NodeType = "DataMapperNode",
                Description = "Explicitly maps source fields/paths to target input keys",
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
                    new() { Name = "mappings", Label = "Mappings (JSON)", Type = ParameterType.Json, Required = true,
                        Placeholder = "{\"prompt\": \"llm_response\", \"customer_id\": \"http_response.data.id\"}",
                        Description = "Map target keys to source paths. Example: target_key -> source.path" },
                    new() { Name = "defaultValues", Label = "Default Values (JSON)", Type = ParameterType.Json, Required = false,
                        Placeholder = "{\"system_prompt\": \"You are a helpful assistant\"}",
                        Description = "Optional fallback values applied when a source path is missing" },
                    new() { Name = "throwOnMissing", Label = "Throw on Missing Mapping", Type = ParameterType.Boolean, Required = false, DefaultValue = false,
                        Description = "If enabled, node fails when a mapped source path is missing and no default exists" },
                    new() { Name = "removeUnmapped", Label = "Output Only Mapped Keys", Type = ParameterType.Boolean, Required = false, DefaultValue = false,
                        Description = "If enabled, starts with empty data and writes only mapped/default keys" }
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

            ["ErrorNode"] = new()
            {
                NodeType = "ErrorNode",
                Description = "Workflow-level error entry point. One allowed per workflow.",
                InputPorts = new(), // No input ports
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "On Error", Type = PortType.Control, Description = "Runs when workflow error handling starts" }
                },
                Capabilities = new()
                {
                    SupportsMultipleOutputs = false
                },
                ExecutionOptions = new(),
                Parameters = new()
                {
                    new() { Name = "description", Label = "Description", Type = ParameterType.Text, Required = false,
                        Placeholder = "Optional notes about this error handler",
                        Description = "Use this branch to define error handling behavior" }
                }
            },

            ["SubWorkflowNode"] = new()
            {
                NodeType = "SubWorkflowNode",
                Description = "Calls a reusable child workflow and routes success/error outcomes",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Input data for child workflow" }
                },
                OutputPorts = new()
                {
                    new() { Id = "success", Label = "Success", Type = PortType.Conditional, Condition = "success", Description = "Child workflow completed successfully" },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error", Description = "Child workflow failed" }
                },
                Capabilities = new()
                {
                    SupportsSubWorkflow = true,
                    SupportsConditionalRouting = true,
                    SupportsMultipleOutputs = true
                },
                ExecutionOptions = new()
                {
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "subWorkflowId", Label = "Sub Workflow", Type = ParameterType.Text, Required = true,
                        Placeholder = "Select from node properties",
                        Description = "Identifier of the child workflow to execute" }
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
        },

            ["HttpResponseNode"] = new()
            {
                NodeType = "HttpResponseNode",
                Description = "Returns an HTTP response from the workflow with a configurable status code, headers, and body",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Data to include in the response body" }
                },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data, Description = "Passes through workflow data after the response is sent" }
                },
                Capabilities = new(),
                ExecutionOptions = new()
                {
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "statusCode", Label = "HTTP Status Code", Type = ParameterType.Number, Required = true,
                        DefaultValue = 200, MinValue = 100, MaxValue = 599,
                        Description = "HTTP status code to return (e.g. 200, 201, 400, 404, 500)" },
                    new() { Name = "showStatusCode", Label = "Show Status Code in Body", Type = ParameterType.Boolean,
                        Required = false, DefaultValue = false,
                        Description = "Append the HTTP status code to the response body for debugging" },
                    new() { Name = "contentType", Label = "Content-Type", Type = ParameterType.Select, Required = false, DefaultValue = "application/json",
                        Options = new() {
                            new() { Value = "application/json", Label = "JSON (application/json)" },
                            new() { Value = "text/plain", Label = "Plain Text (text/plain)" },
                            new() { Value = "text/html", Label = "HTML (text/html)" },
                            new() { Value = "application/xml", Label = "XML (application/xml)" }
                        }
                    },
                    new() { Name = "bodyKey", Label = "Body Data Key", Type = ParameterType.Text, Required = false,
                        Placeholder = "e.g., final_result, llm_response",
                        Description = "WorkflowData key whose value becomes the response body (leave empty to send all data)" },
                    new() { Name = "headers", Label = "Additional Headers (JSON)", Type = ParameterType.Json, Required = false,
                        Placeholder = "{\"X-Request-Id\": \"{{request_id}}\"}" }
                }
            },

            // Logic Nodes
            ["FunctionNode"] = new()
            {
                NodeType = "FunctionNode",
                Description = "Invokes a named function or method by name with optional parameters",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Input data" }
                },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data, Description = "Function result" },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error", Description = "Function threw an exception" }
                },
                Capabilities = new()
                {
                    SupportsRetry = true,
                    SupportsTimeout = true,
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
                    new() { Name = "functionName", Label = "Function Name", Type = ParameterType.Text, Required = true,
                        Placeholder = "e.g., CalculateScore, ValidateEmail, FormatResponse",
                        Description = "Name of the function or method to invoke" },
                    new() { Name = "parameters", Label = "Parameters (JSON)", Type = ParameterType.Json, Required = false,
                        Placeholder = "{\"param1\": \"{{input_key}}\", \"param2\": 42}",
                        Description = "Parameters to pass to the function (supports {{variable}} interpolation)" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false,
                        Placeholder = "e.g., function_result",
                        Description = "WorkflowData key to store the function return value" },
                    new() { Name = "async", Label = "Async Execution", Type = ParameterType.Boolean, Required = false,
                        DefaultValue = false,
                        Description = "Whether the function runs asynchronously" }
                }
            },

            ["DbQueryNode"] = new()
            {
                NodeType = "DbQueryNode",
                Description = "Executes a SQL query against a configured database connection",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Input data (supports {{variable}} in query)" }
                },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data, Description = "Query result set or affected row count" },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error", Description = "Query failed (connection error, SQL error)" }
                },
                Capabilities = new()
                {
                    SupportsRetry = true,
                    SupportsTimeout = true,
                    SupportsConditionalRouting = true
                },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 0, MinValue = 0, MaxValue = 5 },
                    new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 2000, MinValue = 100, MaxValue = 60000 },
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 30000, MinValue = 1000, MaxValue = 300000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "connectionString", Label = "Connection String", Type = ParameterType.Text, Required = true,
                        Placeholder = "Server=localhost;Database=mydb;User Id=sa;Password=...",
                        Description = "Database connection string (or reference to a secret key)" },
                    new() { Name = "queryType", Label = "Query Type", Type = ParameterType.Select, Required = true, DefaultValue = "SELECT",
                        Options = new() {
                            new() { Value = "SELECT", Label = "SELECT (returns rows)" },
                            new() { Value = "INSERT", Label = "INSERT (returns new id)" },
                            new() { Value = "UPDATE", Label = "UPDATE (returns affected rows)" },
                            new() { Value = "DELETE", Label = "DELETE (returns affected rows)" },
                            new() { Value = "EXEC", Label = "EXEC / Stored Procedure" }
                        }
                    },
                    new() { Name = "query", Label = "SQL Query", Type = ParameterType.TextArea, Required = true,
                        Placeholder = "SELECT * FROM users WHERE id = {{user_id}}",
                        Description = "SQL query with optional {{variable}} placeholders" },
                    new() { Name = "parameters", Label = "Query Parameters (JSON)", Type = ParameterType.Json, Required = false,
                        Placeholder = "{\"user_id\": \"{{user_id}}\"}",
                        Description = "Named parameters for parameterised queries (recommended over inline values)" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false,
                        DefaultValue = "db_result",
                        Placeholder = "e.g., db_result, user_rows",
                        Description = "WorkflowData key to store the result" },
                    new() { Name = "singleRow", Label = "Single Row Result", Type = ParameterType.Boolean, Required = false,
                        DefaultValue = false,
                        Description = "Return only the first row instead of an array (SELECT queries)" }
                }
            },

            ["ProcessNode"] = new()
            {
                NodeType = "ProcessNode",
                Description = "Executes an external process, shell command, or script and captures its output",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Input data (variables available in arguments)" }
                },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data, Description = "Process stdout and exit code" },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error", Description = "Non-zero exit code or execution failure" }
                },
                Capabilities = new()
                {
                    SupportsRetry = true,
                    SupportsTimeout = true,
                    SupportsConditionalRouting = true
                },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 0, MinValue = 0, MaxValue = 5 },
                    new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 1000, MinValue = 100, MaxValue = 60000 },
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 60000, MinValue = 1000, MaxValue = 600000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "processType", Label = "Process Type", Type = ParameterType.Select, Required = true, DefaultValue = "Command",
                        Options = new() {
                            new() { Value = "Command", Label = "Shell Command" },
                            new() { Value = "Script", Label = "Script File" },
                            new() { Value = "Python", Label = "Python Script" },
                            new() { Value = "Node", Label = "Node.js Script" },
                            new() { Value = "PowerShell", Label = "PowerShell" }
                        }
                    },
                    new() { Name = "command", Label = "Command / Script Path", Type = ParameterType.Text, Required = true,
                        Placeholder = "e.g., python, /scripts/process.sh, node",
                        Description = "Executable or path to the script to run" },
                    new() { Name = "arguments", Label = "Arguments", Type = ParameterType.Text, Required = false,
                        Placeholder = "--input {{input_file}} --output {{output_path}}",
                        Description = "Command-line arguments (supports {{variable}} interpolation)" },
                    new() { Name = "workingDirectory", Label = "Working Directory", Type = ParameterType.Text, Required = false,
                        Placeholder = "/app/scripts",
                        Description = "Working directory for the process (leave empty for default)" },
                    new() { Name = "environmentVariables", Label = "Environment Variables (JSON)", Type = ParameterType.Json, Required = false,
                        Placeholder = "{\"API_KEY\": \"{{api_key}}\", \"ENV\": \"production\"}",
                        Description = "Additional environment variables to inject" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false,
                        DefaultValue = "process_output",
                        Placeholder = "e.g., process_output",
                        Description = "WorkflowData key to store stdout" },
                    new() { Name = "captureStderr", Label = "Capture stderr", Type = ParameterType.Boolean, Required = false,
                        DefaultValue = false,
                        Description = "Also capture stderr into the error output" }
                }
            },

            ["StepNode"] = new()
            {
                NodeType = "StepNode",
                Description = "Represents a discrete, named action step in a workflow with configurable inputs and outputs",
                InputPorts = new()
                {
                    new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true, Description = "Data flowing into this step" }
                },
                OutputPorts = new()
                {
                    new() { Id = "success", Label = "Success", Type = PortType.Conditional, Condition = "success", Description = "Step completed successfully" },
                    new() { Id = "failure", Label = "Failure", Type = PortType.Conditional, Condition = "failure", Description = "Step failed or validation did not pass" }
                },
                Capabilities = new()
                {
                    SupportsRetry = true,
                    SupportsConditionalRouting = true,
                    SupportsMultipleOutputs = true
                },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 0, MinValue = 0, MaxValue = 10 },
                    new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 1000, MinValue = 100, MaxValue = 60000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "stepType", Label = "Step Type", Type = ParameterType.Select, Required = true, DefaultValue = "Action",
                        Options = new() {
                            new() { Value = "Action", Label = "Action" },
                            new() { Value = "Validation", Label = "Validation" },
                            new() { Value = "Transformation", Label = "Transformation" },
                            new() { Value = "Notification", Label = "Notification" },
                            new() { Value = "Approval", Label = "Approval / Gate" }
                        }
                    },
                    new() { Name = "description", Label = "Step Description", Type = ParameterType.TextArea, Required = false,
                        Placeholder = "Describe what this step does…",
                        Description = "Human-readable description of what this workflow step performs" },
                    new() { Name = "actionKey", Label = "Action Handler Key", Type = ParameterType.Text, Required = false,
                        Placeholder = "e.g., SendEmail, ValidateSchema, ApproveOrder",
                        Description = "Identifier of the handler registered to execute this step" },
                    new() { Name = "inputKeys", Label = "Input Keys (comma-separated)", Type = ParameterType.Text, Required = false,
                        Placeholder = "e.g., user_id, order_total, email",
                        Description = "WorkflowData keys this step reads as inputs" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false,
                        Placeholder = "e.g., step_result",
                        Description = "WorkflowData key to write the step result to" },
                    new() { Name = "metadata", Label = "Metadata (JSON)", Type = ParameterType.Json, Required = false,
                        Placeholder = "{\"priority\": \"high\", \"owner\": \"team-a\"}",
                        Description = "Arbitrary metadata to attach to this step for documentation or tooling" }
                }
            },

            // Additional Control Nodes
            ["WaitNode"] = new()
            {
                NodeType = "WaitNode",
                Description = "Pauses workflow execution until a duration elapses, a named event fires, or a webhook is called",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Resumed", Type = PortType.Data },
                    new() { Id = "timeout", Label = "Timeout", Type = PortType.Conditional, Condition = "timeout" }
                },
                Capabilities = new() { SupportsConditionalRouting = true, SupportsMultipleOutputs = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "waitType", Label = "Wait Type", Type = ParameterType.Select, Required = true, DefaultValue = "Duration",
                        Options = new() {
                            new() { Value = "Duration", Label = "Fixed Duration" },
                            new() { Value = "Event", Label = "Named Event" },
                            new() { Value = "Webhook", Label = "Webhook Callback" },
                            new() { Value = "Approval", Label = "Human Approval" }
                        }
                    },
                    new() { Name = "durationMs", Label = "Duration (ms)", Type = ParameterType.Number, Required = false, DefaultValue = 5000, MinValue = 100, MaxValue = 86400000 },
                    new() { Name = "eventName", Label = "Event Name", Type = ParameterType.Text, Required = false, Placeholder = "e.g., order.approved" },
                    new() { Name = "webhookPath", Label = "Webhook Path", Type = ParameterType.Text, Required = false, Placeholder = "/resume/{{workflow_id}}" },
                    new() { Name = "timeoutMs", Label = "Max Wait Timeout (ms)", Type = ParameterType.Number, Required = false, DefaultValue = 0, MinValue = 0, MaxValue = 604800000, Description = "0 = no timeout" },
                    new() { Name = "resumeDataKey", Label = "Resume Data Key", Type = ParameterType.Text, Required = false, Placeholder = "e.g., approval_payload" }
                }
            },

            ["RetryNode"] = new()
            {
                NodeType = "RetryNode",
                Description = "Wraps a sub-path in a configurable retry loop with optional exponential backoff",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "attempt", Label = "Attempt", Type = PortType.Control, Description = "Execute on each attempt" },
                    new() { Id = "success", Label = "Success", Type = PortType.Conditional, Condition = "success" },
                    new() { Id = "failed", Label = "All Failed", Type = PortType.Conditional, Condition = "failed" }
                },
                Capabilities = new() { SupportsSubWorkflow = true, SupportsMultipleOutputs = true, SupportsConditionalRouting = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, Required = true, DefaultValue = 3, MinValue = 1, MaxValue = 20 },
                    new() { Name = "retryDelayMs", Label = "Initial Retry Delay (ms)", Type = ParameterType.Number, Required = false, DefaultValue = 1000, MinValue = 100, MaxValue = 60000 },
                    new() { Name = "backoffMultiplier", Label = "Backoff Multiplier", Type = ParameterType.Number, Required = false, DefaultValue = 2.0, MinValue = 1.0, MaxValue = 10.0, Description = "Multiply delay on each retry (1.0 = no backoff)" },
                    new() { Name = "retryOn", Label = "Retry On", Type = ParameterType.Select, Required = false, DefaultValue = "Error",
                        Options = new() {
                            new() { Value = "Error", Label = "Any Error" },
                            new() { Value = "Condition", Label = "Condition Expression" },
                            new() { Value = "All", Label = "Always Retry" }
                        }
                    },
                    new() { Name = "retryCondition", Label = "Retry Condition", Type = ParameterType.Text, Required = false, Placeholder = "e.g., status_code == 429 || status_code >= 500" }
                }
            },

            ["TimeoutNode"] = new()
            {
                NodeType = "TimeoutNode",
                Description = "Aborts a sub-path if it exceeds the configured wall-clock time limit",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "execute", Label = "Execute", Type = PortType.Control, Description = "Path to run within the limit" },
                    new() { Id = "success", Label = "Success", Type = PortType.Conditional, Condition = "success" },
                    new() { Id = "timeout", Label = "Timeout", Type = PortType.Conditional, Condition = "timeout" }
                },
                Capabilities = new() { SupportsSubWorkflow = true, SupportsTimeout = true, SupportsConditionalRouting = true, SupportsMultipleOutputs = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, Required = true, DefaultValue = 30000, MinValue = 100, MaxValue = 3600000 },
                    new() { Name = "outputKey", Label = "Timeout Message Key", Type = ParameterType.Text, Required = false, DefaultValue = "timeout_message" }
                }
            },

            ["EventTriggerNode"] = new()
            {
                NodeType = "EventTriggerNode",
                Description = "Emits or listens for a named workflow event to synchronise branches",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = false } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "mode", Label = "Mode", Type = ParameterType.Select, Required = true, DefaultValue = "Emit",
                        Options = new() {
                            new() { Value = "Emit", Label = "Emit (publish event)" },
                            new() { Value = "Listen", Label = "Listen (subscribe & wait)" }
                        }
                    },
                    new() { Name = "eventName", Label = "Event Name", Type = ParameterType.Text, Required = true, Placeholder = "e.g., order.processed" },
                    new() { Name = "payloadKey", Label = "Payload Key", Type = ParameterType.Text, Required = false, Placeholder = "e.g., event_payload" }
                }
            },

            ["SwitchNode"] = new()
            {
                NodeType = "SwitchNode",
                Description = "Evaluates up to 4 boolean conditions in order and routes to the first matching output",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "case1", Label = "Case 1", Type = PortType.Conditional, Condition = "case1" },
                    new() { Id = "case2", Label = "Case 2", Type = PortType.Conditional, Condition = "case2" },
                    new() { Id = "case3", Label = "Case 3", Type = PortType.Conditional, Condition = "case3" },
                    new() { Id = "case4", Label = "Case 4", Type = PortType.Conditional, Condition = "case4" },
                    new() { Id = "default", Label = "Default", Type = PortType.Conditional, Condition = "default" }
                },
                Capabilities = new() { SupportsConditionalRouting = true, SupportsMultipleOutputs = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "case1Expression", Label = "Case 1 Expression", Type = ParameterType.Text, Required = false, Placeholder = "e.g., score >= 90" },
                    new() { Name = "case1Label", Label = "Case 1 Label", Type = ParameterType.Text, Required = false, DefaultValue = "Case 1" },
                    new() { Name = "case2Expression", Label = "Case 2 Expression", Type = ParameterType.Text, Required = false, Placeholder = "e.g., score >= 70" },
                    new() { Name = "case2Label", Label = "Case 2 Label", Type = ParameterType.Text, Required = false, DefaultValue = "Case 2" },
                    new() { Name = "case3Expression", Label = "Case 3 Expression", Type = ParameterType.Text, Required = false, Placeholder = "e.g., score >= 50" },
                    new() { Name = "case3Label", Label = "Case 3 Label", Type = ParameterType.Text, Required = false, DefaultValue = "Case 3" },
                    new() { Name = "case4Expression", Label = "Case 4 Expression", Type = ParameterType.Text, Required = false, Placeholder = "e.g., status == 'pending'" },
                    new() { Name = "case4Label", Label = "Case 4 Label", Type = ParameterType.Text, Required = false, DefaultValue = "Case 4" },
                    new() { Name = "stopOnFirstMatch", Label = "Stop on First Match", Type = ParameterType.Boolean, Required = false, DefaultValue = true }
                }
            },

            // Additional Data Nodes
            ["SetVariableNode"] = new()
            {
                NodeType = "SetVariableNode",
                Description = "Writes one or more literal or interpolated values into WorkflowData keys",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "assignments", Label = "Assignments (JSON)", Type = ParameterType.Json, Required = true,
                        Placeholder = "{\"greeting\": \"Hello {{name}}\", \"count\": 0, \"active\": true}",
                        Description = "Key/value pairs to write. Values support {{variable}} interpolation." },
                    new() { Name = "mergeMode", Label = "Merge Mode", Type = ParameterType.Select, Required = false, DefaultValue = "Merge",
                        Options = new() {
                            new() { Value = "Merge", Label = "Merge (keep existing keys)" },
                            new() { Value = "Replace", Label = "Replace (clear all first)" }
                        }
                    }
                }
            },

            ["ParseJsonNode"] = new()
            {
                NodeType = "ParseJsonNode",
                Description = "Parses a raw JSON string in WorkflowData into a structured object",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsConditionalRouting = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "sourceKey", Label = "Source Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., raw_response" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, Placeholder = "e.g., parsed_data (empty = merge into root)" },
                    new() { Name = "strict", Label = "Strict Mode", Type = ParameterType.Boolean, Required = false, DefaultValue = true, Description = "Fail on invalid JSON" }
                }
            },

            ["AggregateNode"] = new()
            {
                NodeType = "AggregateNode",
                Description = "Computes sum, count, avg, min, or max over a collection in WorkflowData",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "itemsKey", Label = "Items Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., orders" },
                    new() { Name = "field", Label = "Field", Type = ParameterType.Text, Required = false, Placeholder = "e.g., amount (empty for primitive arrays)" },
                    new() { Name = "operation", Label = "Operation", Type = ParameterType.Select, Required = true, DefaultValue = "Count",
                        Options = new() {
                            new() { Value = "Count", Label = "Count" },
                            new() { Value = "Sum", Label = "Sum" },
                            new() { Value = "Avg", Label = "Average" },
                            new() { Value = "Min", Label = "Minimum" },
                            new() { Value = "Max", Label = "Maximum" },
                            new() { Value = "First", Label = "First" },
                            new() { Value = "Last", Label = "Last" }
                        }
                    },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "aggregate_result" }
                }
            },

            ["SortNode"] = new()
            {
                NodeType = "SortNode",
                Description = "Sorts a collection in WorkflowData by a specified field",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "itemsKey", Label = "Items Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., products" },
                    new() { Name = "sortBy", Label = "Sort By Field", Type = ParameterType.Text, Required = false, Placeholder = "e.g., price (empty for primitive arrays)" },
                    new() { Name = "direction", Label = "Direction", Type = ParameterType.Select, Required = false, DefaultValue = "Asc",
                        Options = new() {
                            new() { Value = "Asc", Label = "Ascending" },
                            new() { Value = "Desc", Label = "Descending" }
                        }
                    },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty to sort in-place" }
                }
            },

            ["JoinNode"] = new()
            {
                NodeType = "JoinNode",
                Description = "Joins two collections in WorkflowData on a shared key field (in-memory SQL-like join)",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "leftKey", Label = "Left Collection Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., orders" },
                    new() { Name = "rightKey", Label = "Right Collection Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., users" },
                    new() { Name = "joinField", Label = "Join Field", Type = ParameterType.Text, Required = true, Placeholder = "e.g., user_id", Description = "Field present in both collections" },
                    new() { Name = "joinType", Label = "Join Type", Type = ParameterType.Select, Required = false, DefaultValue = "Inner",
                        Options = new() {
                            new() { Value = "Inner", Label = "Inner Join" },
                            new() { Value = "Left", Label = "Left Join" },
                            new() { Value = "Right", Label = "Right Join" }
                        }
                    },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "joined_result" }
                }
            },

            ["SchemaValidateNode"] = new()
            {
                NodeType = "SchemaValidateNode",
                Description = "Validates WorkflowData against a JSON Schema and routes valid/invalid",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "success", Label = "Valid", Type = PortType.Conditional, Condition = "success" },
                    new() { Id = "failure", Label = "Invalid", Type = PortType.Conditional, Condition = "failure" }
                },
                Capabilities = new() { SupportsConditionalRouting = true, SupportsMultipleOutputs = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "schema", Label = "JSON Schema", Type = ParameterType.Json, Required = true,
                        Placeholder = "{\"type\":\"object\",\"required\":[\"name\",\"email\"],\"properties\":{\"name\":{\"type\":\"string\"},\"email\":{\"type\":\"string\"}}}" },
                    new() { Name = "dataKey", Label = "Data Key", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty to validate all workflow data" },
                    new() { Name = "errorsKey", Label = "Errors Output Key", Type = ParameterType.Text, Required = false, DefaultValue = "validation_errors" }
                }
            },

            ["TemplateNode"] = new()
            {
                NodeType = "TemplateNode",
                Description = "Renders a Handlebars / Mustache template string using WorkflowData variables",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "engine", Label = "Template Engine", Type = ParameterType.Select, Required = false, DefaultValue = "Handlebars",
                        Options = new() {
                            new() { Value = "Handlebars", Label = "Handlebars ({{var}}, {{#if}}, {{#each}})" },
                            new() { Value = "Mustache", Label = "Mustache ({{var}})" },
                            new() { Value = "Liquid", Label = "Liquid ({{ var }}, {% if %})" }
                        }
                    },
                    new() { Name = "template", Label = "Template", Type = ParameterType.TextArea, Required = true, Placeholder = "Hello {{name}},\n\nYour order #{{order_id}} status: {{status}}." },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "rendered_template" }
                }
            },

            ["CsvParseNode"] = new()
            {
                NodeType = "CsvParseNode",
                Description = "Parses a CSV string into an array of objects",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsConditionalRouting = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "sourceKey", Label = "Source Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., csv_content" },
                    new() { Name = "delimiter", Label = "Delimiter", Type = ParameterType.Text, Required = false, DefaultValue = ",", Placeholder = "e.g., , or ; or \\t" },
                    new() { Name = "hasHeaders", Label = "First Row is Headers", Type = ParameterType.Boolean, Required = false, DefaultValue = true },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "csv_rows" }
                }
            },

            ["XmlParseNode"] = new()
            {
                NodeType = "XmlParseNode",
                Description = "Parses an XML string into a structured JSON object",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsConditionalRouting = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "sourceKey", Label = "Source Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., xml_content" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "xml_data" },
                    new() { Name = "preserveAttributes", Label = "Preserve XML Attributes", Type = ParameterType.Boolean, Required = false, DefaultValue = true }
                }
            },

            ["Base64Node"] = new()
            {
                NodeType = "Base64Node",
                Description = "Encodes or decodes a value using Base64",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "operation", Label = "Operation", Type = ParameterType.Select, Required = true, DefaultValue = "Encode",
                        Options = new() {
                            new() { Value = "Encode", Label = "Encode to Base64" },
                            new() { Value = "Decode", Label = "Decode from Base64" }
                        }
                    },
                    new() { Name = "sourceKey", Label = "Source Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., file_content" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "base64_result" }
                }
            },

            ["HashNode"] = new()
            {
                NodeType = "HashNode",
                Description = "Computes a cryptographic hash of a value (MD5, SHA-256, SHA-512, HMAC)",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "algorithm", Label = "Algorithm", Type = ParameterType.Select, Required = true, DefaultValue = "SHA256",
                        Options = new() {
                            new() { Value = "MD5", Label = "MD5" },
                            new() { Value = "SHA1", Label = "SHA-1" },
                            new() { Value = "SHA256", Label = "SHA-256" },
                            new() { Value = "SHA512", Label = "SHA-512" },
                            new() { Value = "HMACSHA256", Label = "HMAC-SHA256" }
                        }
                    },
                    new() { Name = "sourceKey", Label = "Source Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., password" },
                    new() { Name = "secretKey", Label = "HMAC Secret Key", Type = ParameterType.Text, Required = false, Placeholder = "Required for HMAC algorithms" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "hash_result" },
                    new() { Name = "encoding", Label = "Output Encoding", Type = ParameterType.Select, Required = false, DefaultValue = "Hex",
                        Options = new() {
                            new() { Value = "Hex", Label = "Hexadecimal" },
                            new() { Value = "Base64", Label = "Base64" }
                        }
                    }
                }
            },

            ["DateTimeNode"] = new()
            {
                NodeType = "DateTimeNode",
                Description = "Parses, formats, or performs arithmetic on date/time values",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "operation", Label = "Operation", Type = ParameterType.Select, Required = true, DefaultValue = "Now",
                        Options = new() {
                            new() { Value = "Now", Label = "Get Current DateTime" },
                            new() { Value = "Format", Label = "Format DateTime" },
                            new() { Value = "Parse", Label = "Parse String to DateTime" },
                            new() { Value = "Add", Label = "Add Duration" },
                            new() { Value = "Subtract", Label = "Subtract Duration" },
                            new() { Value = "Diff", Label = "Difference Between Two Dates" }
                        }
                    },
                    new() { Name = "sourceKey", Label = "Source Key", Type = ParameterType.Text, Required = false, Placeholder = "e.g., created_at (required for Format/Parse/Add/Subtract)" },
                    new() { Name = "inputFormat", Label = "Input Format", Type = ParameterType.Text, Required = false, Placeholder = "e.g., yyyy-MM-dd" },
                    new() { Name = "outputFormat", Label = "Output Format", Type = ParameterType.Text, Required = false, DefaultValue = "yyyy-MM-ddTHH:mm:ssZ" },
                    new() { Name = "amount", Label = "Amount", Type = ParameterType.Number, Required = false, DefaultValue = 1 },
                    new() { Name = "unit", Label = "Unit", Type = ParameterType.Select, Required = false, DefaultValue = "Days",
                        Options = new() {
                            new() { Value = "Milliseconds", Label = "Milliseconds" },
                            new() { Value = "Seconds", Label = "Seconds" },
                            new() { Value = "Minutes", Label = "Minutes" },
                            new() { Value = "Hours", Label = "Hours" },
                            new() { Value = "Days", Label = "Days" },
                            new() { Value = "Months", Label = "Months" },
                            new() { Value = "Years", Label = "Years" }
                        }
                    },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "datetime_result" }
                }
            },

            ["RandomNode"] = new()
            {
                NodeType = "RandomNode",
                Description = "Generates a random UUID, number, or picks/shuffles a list",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = false } },
                OutputPorts = new() { new() { Id = "output", Label = "Output", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "type", Label = "Random Type", Type = ParameterType.Select, Required = true, DefaultValue = "UUID",
                        Options = new() {
                            new() { Value = "UUID", Label = "UUID v4" },
                            new() { Value = "Number", Label = "Random Number" },
                            new() { Value = "Pick", Label = "Random Pick from List" },
                            new() { Value = "Shuffle", Label = "Shuffle List" }
                        }
                    },
                    new() { Name = "min", Label = "Min (Number)", Type = ParameterType.Number, Required = false, DefaultValue = 0 },
                    new() { Name = "max", Label = "Max (Number)", Type = ParameterType.Number, Required = false, DefaultValue = 100 },
                    new() { Name = "listKey", Label = "List Key (Pick/Shuffle)", Type = ParameterType.Text, Required = false, Placeholder = "e.g., options" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "random_result" }
                }
            },

            // Additional IO Nodes
            ["FileReadNode"] = new()
            {
                NodeType = "FileReadNode",
                Description = "Reads a file from the local file system or cloud storage",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsConditionalRouting = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "filePath", Label = "File Path", Type = ParameterType.Text, Required = true, Placeholder = "/data/input/{{filename}}" },
                    new() { Name = "format", Label = "Read As", Type = ParameterType.Select, Required = false, DefaultValue = "Text",
                        Options = new() {
                            new() { Value = "Text", Label = "Text (UTF-8)" },
                            new() { Value = "Binary", Label = "Binary (Base64)" },
                            new() { Value = "Json", Label = "Parse as JSON" },
                            new() { Value = "Lines", Label = "Array of Lines" }
                        }
                    },
                    new() { Name = "encoding", Label = "Encoding", Type = ParameterType.Text, Required = false, DefaultValue = "utf-8" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = true, DefaultValue = "file_content" }
                }
            },

            ["FileWriteNode"] = new()
            {
                NodeType = "FileWriteNode",
                Description = "Writes workflow data to a file on the local file system",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsConditionalRouting = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "filePath", Label = "File Path", Type = ParameterType.Text, Required = true, Placeholder = "/data/output/{{request_id}}.json" },
                    new() { Name = "contentKey", Label = "Content Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., final_result" },
                    new() { Name = "writeMode", Label = "Write Mode", Type = ParameterType.Select, Required = false, DefaultValue = "Overwrite",
                        Options = new() {
                            new() { Value = "Overwrite", Label = "Overwrite" },
                            new() { Value = "Append", Label = "Append" },
                            new() { Value = "CreateNew", Label = "Create New (fail if exists)" }
                        }
                    },
                    new() { Name = "createDirectories", Label = "Create Directories", Type = ParameterType.Boolean, Required = false, DefaultValue = true },
                    new() { Name = "encoding", Label = "Encoding", Type = ParameterType.Text, Required = false, DefaultValue = "utf-8" }
                }
            },

            ["EmailSendNode"] = new()
            {
                NodeType = "EmailSendNode",
                Description = "Sends an email via SMTP or a transactional email service (SendGrid, Mailgun, SES)",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Sent", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsRetry = true, SupportsTimeout = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 2, MinValue = 0, MaxValue = 5 },
                    new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 2000, MinValue = 500, MaxValue = 30000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "SMTP",
                        Options = new() {
                            new() { Value = "SMTP", Label = "SMTP" },
                            new() { Value = "SendGrid", Label = "SendGrid" },
                            new() { Value = "Mailgun", Label = "Mailgun" },
                            new() { Value = "SES", Label = "AWS SES" }
                        }
                    },
                    new() { Name = "smtpHost", Label = "SMTP Host", Type = ParameterType.Text, Required = false, Placeholder = "smtp.gmail.com" },
                    new() { Name = "smtpPort", Label = "SMTP Port", Type = ParameterType.Number, Required = false, DefaultValue = 587, MinValue = 1, MaxValue = 65535 },
                    new() { Name = "apiKey", Label = "API Key", Type = ParameterType.Text, Required = false, Placeholder = "For SendGrid / Mailgun / SES" },
                    new() { Name = "from", Label = "From", Type = ParameterType.Text, Required = true, Placeholder = "no-reply@example.com" },
                    new() { Name = "to", Label = "To", Type = ParameterType.Text, Required = true, Placeholder = "{{recipient_email}}" },
                    new() { Name = "cc", Label = "CC", Type = ParameterType.Text, Required = false },
                    new() { Name = "subject", Label = "Subject", Type = ParameterType.Text, Required = true, Placeholder = "Order #{{order_id}} confirmed" },
                    new() { Name = "body", Label = "Body", Type = ParameterType.TextArea, Required = true, Placeholder = "Hi {{name}},\n\nYour order has been confirmed." },
                    new() { Name = "isHtml", Label = "HTML Body", Type = ParameterType.Boolean, Required = false, DefaultValue = false },
                    new() { Name = "useSSL", Label = "Use SSL/TLS", Type = ParameterType.Boolean, Required = false, DefaultValue = true }
                }
            },

            ["WebhookNode"] = new()
            {
                NodeType = "WebhookNode",
                Description = "Exposes an HTTP endpoint that triggers this workflow when called",
                InputPorts = new(),
                OutputPorts = new() { new() { Id = "output", Label = "Triggered", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new(),
                Parameters = new()
                {
                    new() { Name = "path", Label = "Webhook Path", Type = ParameterType.Text, Required = true, Placeholder = "/webhooks/my-event" },
                    new() { Name = "method", Label = "HTTP Method", Type = ParameterType.Select, Required = false, DefaultValue = "POST",
                        Options = new() {
                            new() { Value = "POST", Label = "POST" },
                            new() { Value = "GET", Label = "GET" },
                            new() { Value = "PUT", Label = "PUT" }
                        }
                    },
                    new() { Name = "authType", Label = "Authentication", Type = ParameterType.Select, Required = false, DefaultValue = "None",
                        Options = new() {
                            new() { Value = "None", Label = "None" },
                            new() { Value = "ApiKey", Label = "API Key Header" },
                            new() { Value = "BasicAuth", Label = "Basic Auth" },
                            new() { Value = "HmacSignature", Label = "HMAC Signature" }
                        }
                    },
                    new() { Name = "secretKey", Label = "Secret / API Key", Type = ParameterType.Text, Required = false, Placeholder = "Shared secret for authentication" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, DefaultValue = "webhook_payload" }
                }
            },

            ["QueueNode"] = new()
            {
                NodeType = "QueueNode",
                Description = "Publishes or consumes messages from a queue (RabbitMQ, Azure Service Bus, SQS)",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsRetry = true, SupportsTimeout = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 2, MinValue = 0, MaxValue = 10 },
                    new() { Name = "retryDelayMs", Label = "Retry Delay (ms)", Type = ParameterType.Number, DefaultValue = 2000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "RabbitMQ",
                        Options = new() {
                            new() { Value = "RabbitMQ", Label = "RabbitMQ" },
                            new() { Value = "AzureServiceBus", Label = "Azure Service Bus" },
                            new() { Value = "SQS", Label = "Amazon SQS" },
                            new() { Value = "Redis", Label = "Redis Pub/Sub" }
                        }
                    },
                    new() { Name = "connectionString", Label = "Connection String", Type = ParameterType.Text, Required = true, Placeholder = "amqp://user:pass@localhost:5672" },
                    new() { Name = "queueName", Label = "Queue / Topic Name", Type = ParameterType.Text, Required = true, Placeholder = "e.g., orders.created" },
                    new() { Name = "operation", Label = "Operation", Type = ParameterType.Select, Required = true, DefaultValue = "Publish",
                        Options = new() {
                            new() { Value = "Publish", Label = "Publish Message" },
                            new() { Value = "Consume", Label = "Consume Next Message" }
                        }
                    },
                    new() { Name = "messageKey", Label = "Message Key (Publish)", Type = ParameterType.Text, Required = false, Placeholder = "Leave empty to send all data" },
                    new() { Name = "outputKey", Label = "Output Key (Consume)", Type = ParameterType.Text, Required = false, DefaultValue = "queue_message" }
                }
            },

            ["CacheNode"] = new()
            {
                NodeType = "CacheNode",
                Description = "Reads from or writes to a cache (Redis or in-memory) with configurable TTL",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "hit", Label = "Cache Hit", Type = PortType.Conditional, Condition = "hit" },
                    new() { Id = "miss", Label = "Cache Miss", Type = PortType.Conditional, Condition = "miss" }
                },
                Capabilities = new() { SupportsConditionalRouting = true, SupportsMultipleOutputs = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = false, DefaultValue = "InMemory",
                        Options = new() {
                            new() { Value = "InMemory", Label = "In-Memory (per run)" },
                            new() { Value = "Redis", Label = "Redis" }
                        }
                    },
                    new() { Name = "connectionString", Label = "Redis Connection String", Type = ParameterType.Text, Required = false, Placeholder = "localhost:6379" },
                    new() { Name = "operation", Label = "Operation", Type = ParameterType.Select, Required = true, DefaultValue = "Get",
                        Options = new() {
                            new() { Value = "Get", Label = "Get" },
                            new() { Value = "Set", Label = "Set" },
                            new() { Value = "Delete", Label = "Delete" },
                            new() { Value = "Exists", Label = "Exists" }
                        }
                    },
                    new() { Name = "cacheKey", Label = "Cache Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., user:{{user_id}}:profile" },
                    new() { Name = "valueKey", Label = "Value Key (Set)", Type = ParameterType.Text, Required = false, Placeholder = "WorkflowData key to cache" },
                    new() { Name = "outputKey", Label = "Output Key (Get)", Type = ParameterType.Text, Required = false, DefaultValue = "cached_value" },
                    new() { Name = "ttlSeconds", Label = "TTL (seconds)", Type = ParameterType.Number, Required = false, DefaultValue = 300, MinValue = 1, MaxValue = 86400 }
                }
            },

            ["NotificationNode"] = new()
            {
                NodeType = "NotificationNode",
                Description = "Sends a message to Slack, Microsoft Teams, or Discord via webhook",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Sent", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsRetry = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 2, MinValue = 0, MaxValue = 5 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "Slack",
                        Options = new() {
                            new() { Value = "Slack", Label = "Slack" },
                            new() { Value = "Teams", Label = "Microsoft Teams" },
                            new() { Value = "Discord", Label = "Discord" }
                        }
                    },
                    new() { Name = "webhookUrl", Label = "Webhook URL", Type = ParameterType.Text, Required = true, Placeholder = "https://hooks.slack.com/services/..." },
                    new() { Name = "message", Label = "Message", Type = ParameterType.TextArea, Required = true, Placeholder = "{{workflow_name}} completed: {{summary}}" },
                    new() { Name = "channel", Label = "Channel (Slack)", Type = ParameterType.Text, Required = false, Placeholder = "#alerts" },
                    new() { Name = "title", Label = "Title", Type = ParameterType.Text, Required = false, Placeholder = "Workflow Alert" }
                }
            },

            ["StorageNode"] = new()
            {
                NodeType = "StorageNode",
                Description = "Reads from, writes to, lists, or deletes objects in cloud storage (S3, Azure Blob, GCS)",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsRetry = true, SupportsTimeout = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 2, MinValue = 0, MaxValue = 5 },
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 60000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "S3",
                        Options = new() {
                            new() { Value = "S3", Label = "Amazon S3" },
                            new() { Value = "AzureBlob", Label = "Azure Blob Storage" },
                            new() { Value = "GCS", Label = "Google Cloud Storage" }
                        }
                    },
                    new() { Name = "connectionString", Label = "Connection String / Credentials", Type = ParameterType.Text, Required = true, Placeholder = "Connection string or JSON credentials" },
                    new() { Name = "operation", Label = "Operation", Type = ParameterType.Select, Required = true, DefaultValue = "Read",
                        Options = new() {
                            new() { Value = "Read", Label = "Read / Download" },
                            new() { Value = "Write", Label = "Write / Upload" },
                            new() { Value = "Delete", Label = "Delete" },
                            new() { Value = "List", Label = "List Objects" },
                            new() { Value = "Exists", Label = "Check Exists" }
                        }
                    },
                    new() { Name = "bucket", Label = "Bucket / Container Name", Type = ParameterType.Text, Required = true, Placeholder = "e.g., my-data-bucket" },
                    new() { Name = "objectKey", Label = "Object Key / Path", Type = ParameterType.Text, Required = false, Placeholder = "e.g., exports/{{date}}/report.json" },
                    new() { Name = "contentKey", Label = "Content Key (Write)", Type = ParameterType.Text, Required = false, Placeholder = "WorkflowData key containing content to upload" },
                    new() { Name = "outputKey", Label = "Output Key (Read/List)", Type = ParameterType.Text, Required = false, DefaultValue = "storage_result" }
                }
            },

            // Additional Logic Nodes
            ["ScriptNode"] = new()
            {
                NodeType = "ScriptNode",
                Description = "Executes an inline JavaScript or Python snippet in a sandboxed environment",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Output", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsTimeout = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 10000, MinValue = 500, MaxValue = 120000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "language", Label = "Language", Type = ParameterType.Select, Required = true, DefaultValue = "JavaScript",
                        Options = new() {
                            new() { Value = "JavaScript", Label = "JavaScript" },
                            new() { Value = "Python", Label = "Python" }
                        }
                    },
                    new() { Name = "script", Label = "Script", Type = ParameterType.TextArea, Required = true,
                        Placeholder = "// Access data via: data.myKey\n// Return output: return { result: data.value * 2 };" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, DefaultValue = "",
                        Placeholder = "Leave empty to merge return value into workflow data" }
                }
            },

            ["RateLimiterNode"] = new()
            {
                NodeType = "RateLimiterNode",
                Description = "Throttles workflow execution using a token-bucket or fixed-window rate limit",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Allowed", Type = PortType.Data },
                    new() { Id = "blocked", Label = "Rate Limited", Type = PortType.Conditional, Condition = "blocked" }
                },
                Capabilities = new() { SupportsConditionalRouting = true },
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "strategy", Label = "Strategy", Type = ParameterType.Select, Required = true, DefaultValue = "FixedWindow",
                        Options = new() {
                            new() { Value = "FixedWindow", Label = "Fixed Window" },
                            new() { Value = "SlidingWindow", Label = "Sliding Window" },
                            new() { Value = "TokenBucket", Label = "Token Bucket" }
                        }
                    },
                    new() { Name = "maxRequests", Label = "Max Requests", Type = ParameterType.Number, Required = true, DefaultValue = 10, MinValue = 1, MaxValue = 100000 },
                    new() { Name = "windowMs", Label = "Window (ms)", Type = ParameterType.Number, Required = true, DefaultValue = 60000, MinValue = 100, MaxValue = 3600000 },
                    new() { Name = "key", Label = "Rate Limit Key", Type = ParameterType.Text, Required = false, Placeholder = "e.g., {{user_id}} (empty = global)" },
                    new() { Name = "blockAction", Label = "When Blocked", Type = ParameterType.Select, Required = false, DefaultValue = "Route",
                        Options = new() {
                            new() { Value = "Route", Label = "Route to Rate Limited output" },
                            new() { Value = "Wait", Label = "Wait until allowed" },
                            new() { Value = "Throw", Label = "Throw error" }
                        }
                    }
                }
            },

            // Additional AI Nodes
            ["VectorSearchNode"] = new()
            {
                NodeType = "VectorSearchNode",
                Description = "Performs a semantic similarity search over a vector store (Qdrant, Pinecone, pgvector, Weaviate)",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Results", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsRetry = true, SupportsTimeout = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 1, MinValue = 0, MaxValue = 5 },
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 30000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Vector Store Provider", Type = ParameterType.Select, Required = true, DefaultValue = "Qdrant",
                        Options = new() {
                            new() { Value = "Qdrant", Label = "Qdrant" },
                            new() { Value = "Pinecone", Label = "Pinecone" },
                            new() { Value = "PgVector", Label = "pgvector (PostgreSQL)" },
                            new() { Value = "Weaviate", Label = "Weaviate" },
                            new() { Value = "Chroma", Label = "ChromaDB" }
                        }
                    },
                    new() { Name = "connectionString", Label = "Connection String / API Key", Type = ParameterType.Text, Required = true, Placeholder = "https://my-cluster.qdrant.io or API key" },
                    new() { Name = "indexName", Label = "Index / Collection Name", Type = ParameterType.Text, Required = true, Placeholder = "e.g., documents" },
                    new() { Name = "queryKey", Label = "Query Embedding Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., embedding (output of EmbeddingNode)" },
                    new() { Name = "topK", Label = "Top K Results", Type = ParameterType.Number, Required = false, DefaultValue = 5, MinValue = 1, MaxValue = 100 },
                    new() { Name = "minScore", Label = "Minimum Score (0–1)", Type = ParameterType.Number, Required = false, DefaultValue = 0.7, MinValue = 0, MaxValue = 1 },
                    new() { Name = "filter", Label = "Metadata Filter (JSON)", Type = ParameterType.Json, Required = false, Placeholder = "{\"category\": \"legal\"}" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, DefaultValue = "search_results" }
                }
            },

            ["AgentNode"] = new()
            {
                NodeType = "AgentNode",
                Description = "Runs a ReAct-style AI agent that autonomously calls tools until the task is complete",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Result", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsRetry = true, SupportsTimeout = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 0, MinValue = 0, MaxValue = 3 },
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 120000, MinValue = 5000, MaxValue = 600000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "openai",
                        Options = new() {
                            new() { Value = "openai", Label = "OpenAI" },
                            new() { Value = "azure", Label = "Azure OpenAI" },
                            new() { Value = "anthropic", Label = "Anthropic" }
                        }
                    },
                    new() { Name = "model", Label = "Model", Type = ParameterType.Text, Required = true, DefaultValue = "gpt-4o" },
                    new() { Name = "apiKey", Label = "API Key", Type = ParameterType.Text, Required = false },
                    new() { Name = "systemPrompt", Label = "System Prompt", Type = ParameterType.TextArea, Required = false, Placeholder = "You are a helpful assistant. Use tools to complete the task." },
                    new() { Name = "goalKey", Label = "Goal / Task Key", Type = ParameterType.Text, Required = false, Placeholder = "WorkflowData key containing the task description" },
                    new() { Name = "tools", Label = "Available Tools (JSON)", Type = ParameterType.Json, Required = false, Placeholder = "[{\"name\":\"search\",\"description\":\"Search the web\"}]" },
                    new() { Name = "maxIterations", Label = "Max Iterations", Type = ParameterType.Number, Required = false, DefaultValue = 10, MinValue = 1, MaxValue = 50 },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, DefaultValue = "agent_result" }
                }
            },

            ["TextSplitterNode"] = new()
            {
                NodeType = "TextSplitterNode",
                Description = "Splits text into chunks using token-aware, markdown, code-aware, or recursive strategies",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new() { new() { Id = "output", Label = "Chunks", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new() { new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false } },
                Parameters = new()
                {
                    new() { Name = "strategy", Label = "Split Strategy", Type = ParameterType.Select, Required = true, DefaultValue = "Recursive",
                        Options = new() {
                            new() { Value = "Recursive", Label = "Recursive (paragraph → sentence → word)" },
                            new() { Value = "Token", Label = "Token-aware (tiktoken)" },
                            new() { Value = "Markdown", Label = "Markdown headers" },
                            new() { Value = "Code", Label = "Code blocks (by function/class)" },
                            new() { Value = "Character", Label = "Fixed Character Count" }
                        }
                    },
                    new() { Name = "textKey", Label = "Text Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., document_content" },
                    new() { Name = "chunkSize", Label = "Chunk Size", Type = ParameterType.Number, Required = false, DefaultValue = 1000, MinValue = 50, MaxValue = 32000 },
                    new() { Name = "chunkOverlap", Label = "Chunk Overlap", Type = ParameterType.Number, Required = false, DefaultValue = 200, MinValue = 0, MaxValue = 5000 },
                    new() { Name = "codeLanguage", Label = "Code Language (Code strategy)", Type = ParameterType.Text, Required = false, Placeholder = "e.g., python, csharp" },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, DefaultValue = "text_chunks" }
                }
            },

            ["SummariseNode"] = new()
            {
                NodeType = "SummariseNode",
                Description = "Summarises long documents using stuff, map-reduce, or refine strategies",
                InputPorts = new() { new() { Id = "input", Label = "Input", Type = PortType.Data, Required = true } },
                OutputPorts = new()
                {
                    new() { Id = "output", Label = "Summary", Type = PortType.Data },
                    new() { Id = "error", Label = "Error", Type = PortType.Conditional, Condition = "error" }
                },
                Capabilities = new() { SupportsRetry = true, SupportsTimeout = true, SupportsConditionalRouting = true },
                ExecutionOptions = new()
                {
                    new() { Name = "maxRetries", Label = "Max Retries", Type = ParameterType.Number, DefaultValue = 1, MinValue = 0, MaxValue = 5 },
                    new() { Name = "timeoutMs", Label = "Timeout (ms)", Type = ParameterType.Number, DefaultValue = 120000, MinValue = 5000, MaxValue = 600000 },
                    new() { Name = "continueOnError", Label = "Continue on Error", Type = ParameterType.Boolean, DefaultValue = false }
                },
                Parameters = new()
                {
                    new() { Name = "provider", Label = "Provider", Type = ParameterType.Select, Required = true, DefaultValue = "openai",
                        Options = new() {
                            new() { Value = "openai", Label = "OpenAI" },
                            new() { Value = "azure", Label = "Azure OpenAI" },
                            new() { Value = "anthropic", Label = "Anthropic" }
                        }
                    },
                    new() { Name = "model", Label = "Model", Type = ParameterType.Text, Required = true, DefaultValue = "gpt-4o" },
                    new() { Name = "apiKey", Label = "API Key", Type = ParameterType.Text, Required = false },
                    new() { Name = "textKey", Label = "Text Key", Type = ParameterType.Text, Required = true, Placeholder = "e.g., document_content" },
                    new() { Name = "strategy", Label = "Strategy", Type = ParameterType.Select, Required = false, DefaultValue = "Stuff",
                        Options = new() {
                            new() { Value = "Stuff", Label = "Stuff (single prompt — short docs)" },
                            new() { Value = "MapReduce", Label = "Map-Reduce (chunk then combine)" },
                            new() { Value = "Refine", Label = "Refine (iterative refinement)" }
                        }
                    },
                    new() { Name = "systemPrompt", Label = "Summarisation Instructions", Type = ParameterType.TextArea, Required = false, Placeholder = "Provide a concise 3-paragraph summary." },
                    new() { Name = "maxLength", Label = "Max Summary Length (words)", Type = ParameterType.Number, Required = false, DefaultValue = 300, MinValue = 50, MaxValue = 2000 },
                    new() { Name = "outputKey", Label = "Output Key", Type = ParameterType.Text, Required = false, DefaultValue = "summary" }
                }
            },

            // Visual Nodes
            ["NoteNode"] = new()
            {
                NodeType = "NoteNode",
                Description = "Sticky note annotation on the canvas. No execution effect.",
                InputPorts = new(),
                OutputPorts = new(),
                Capabilities = new(),
                ExecutionOptions = new(),
                Parameters = new()
                {
                    new() { Name = "text", Label = "Note Text", Type = ParameterType.TextArea, Required = false, Placeholder = "Add a comment or annotation…" },
                    new() { Name = "color", Label = "Color", Type = ParameterType.Select, Required = false, DefaultValue = "yellow",
                        Options = new() {
                            new() { Value = "yellow", Label = "Yellow" },
                            new() { Value = "blue", Label = "Blue" },
                            new() { Value = "green", Label = "Green" },
                            new() { Value = "red", Label = "Red" },
                            new() { Value = "purple", Label = "Purple" }
                        }
                    }
                }
            },

            ["AnchorNode"] = new()
            {
                NodeType = "AnchorNode",
                Description = "Named jump point to reduce edge spaghetti in large workflows",
                InputPorts = new() { new() { Id = "input", Label = "From", Type = PortType.Data, Required = false } },
                OutputPorts = new() { new() { Id = "output", Label = "To", Type = PortType.Data } },
                Capabilities = new(),
                ExecutionOptions = new(),
                Parameters = new()
                {
                    new() { Name = "anchorName", Label = "Anchor Name", Type = ParameterType.Text, Required = true,
                        Placeholder = "e.g., After-Validation, Pre-Notification" }
                }
            },

            ["ContainerNode"] = new()
            {
                NodeType = "ContainerNode",
                Description = "Visually groups nodes on the canvas. No execution effect.",
                InputPorts = new(),
                OutputPorts = new(),
                Capabilities = new(),
                ExecutionOptions = new(),
                Parameters = new()
                {
                    new() { Name = "backgroundColor", Label = "Background Color", Type = ParameterType.Color,
                        Required = false, DefaultValue = "#6366f1",
                        Description = "Background fill colour for the group" },
                    new() { Name = "opacity", Label = "Opacity (0 – 1)", Type = ParameterType.Number,
                        Required = false, DefaultValue = 0.12, MinValue = 0, MaxValue = 1,
                        Description = "Transparency of the background fill" },
                }
            },
        };
    }
}

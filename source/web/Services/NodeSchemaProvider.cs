using TwfAiFramework.Web.Models;

namespace TwfAiFramework.Web.Services;

/// <summary>
/// Provides the parameters schema (form field definitions) for each node type.
/// Routing handles live in nodeConfig.js. Data port metadata comes from NodePortMetadataProvider.
/// </summary>
public static class NodeSchemaProvider
{
    public static Dictionary<string, NodeParameterSchema> GetAllSchemas()
    {
        return new Dictionary<string, NodeParameterSchema>
        {
            // ── AI ──────────────────────────────────────────────────────────
            ["LlmNode"] = new()
            {
                NodeType = "LlmNode",
                Description = "Calls a Large Language Model API to generate responses",
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
                Parameters = new()
            {
     new() { Name = "fieldMapping", Label = "Field Mapping (JSON)", Type = ParameterType.Json, Required = false,
   Placeholder = "{\"sentiment\": \"sentiment\", \"score\": \"confidence\"}" },
  new() { Name = "strictMode", Label = "Strict Mode", Type = ParameterType.Boolean, Required = false, DefaultValue = false }
      }
            },

            // ── Control ─────────────────────────────────────────────────────
            ["StartNode"] = new()
            {
                NodeType = "StartNode",
                Description = "Entry point for workflow execution. Every workflow should start here.",
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
                Parameters = new()
            {
                    new() { Name = "description", Label = "Description", Type = ParameterType.Text, Required = false,
                        Placeholder = "Optional notes about this error handler",
                        Description = "Use this branch to define error handling behavior" }
                }
            },

            ["ErrorRouteNode"] = new()
            {
                NodeType = "ErrorRouteNode",
                Description = "Routes to success/error outputs based on error indicators",
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

            ["ConditionNode"] = new()
            {
                NodeType = "ConditionNode",
                Description = "Routes workflow based on conditional expressions",
                Parameters = new()
            {
   new() { Name = "condition", Label = "Condition Expression", Type = ParameterType.Text, Required = true,
    Placeholder = "e.g., score > 5 && status == 'active'",
     Description = "Expression that evaluates to true or false" }
  }
            },

            ["BranchNode"] = new()
            {
                NodeType = "BranchNode",
                Description = "Routes to different paths based on value matching (switch/case)",
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

            ["SubWorkflowNode"] = new()
            {
                NodeType = "SubWorkflowNode",
                Description = "Calls a reusable child workflow and routes success/error outcomes",
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

            ["DelayNode"] = new()
            {
                NodeType = "DelayNode",
                Description = "Introduces a delay in workflow execution",
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

            // ── Data ────────────────────────────────────────────────────────
            ["SetVariableNode"] = new()
            {
                NodeType = "SetVariableNode",
                Description = "Writes one or more literal or interpolated values into WorkflowData keys",
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

            ["TransformNode"] = new()
            {
                NodeType = "TransformNode",
                Description = "Transforms data using various transformation strategies",
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

            // ── IO ──────────────────────────────────────────────────────────
            ["HttpRequestNode"] = new()
            {
                NodeType = "HttpRequestNode",
                Description = "Makes HTTP requests to external APIs",
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

            ["FileReadNode"] = new()
            {
                NodeType = "FileReadNode",
                Description = "Reads a file from the local file system or cloud storage",
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

            // ── Visual ──────────────────────────────────────────────────────
            ["ContainerNode"] = new()
            {
                NodeType = "ContainerNode",
                Description = "Visually groups nodes on the canvas. No execution effect.",
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

            ["NoteNode"] = new()
            {
                NodeType = "NoteNode",
                Description = "Sticky note annotation on the canvas. No execution effect.",
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

        };
    }
}
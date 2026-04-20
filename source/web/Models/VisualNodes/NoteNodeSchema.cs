using TwfAiFramework.Core;

namespace TwfAiFramework.Web.Models.VisualNodes;

/// <summary>
/// Schema definition for NoteNode - a sticky note annotation with multiple connection points.
/// This is a UI-only node that doesn't execute in workflows.
/// Defined in the web layer because it has no business logic.
/// </summary>
public static class NoteNodeSchema
{
    public const string NodeType = "NoteNode";

    public static NodeParameterSchema GetSchema() => new()
    {
        NodeType = NodeType,
        Description = "Add sticky note annotations with arrows to document your workflow",
        Parameters =
   [
   new()
       {
         Name = "text",
     Label = "Note Text",
     Type = ParameterType.TextArea,
        Required = false,
           DefaultValue = "",
     Placeholder = "Enter your note or comment here...",
          Description = "Free-form text content for the note"
      },
            new()
  {
  Name = "color",
       Label = "Color Theme",
         Type = ParameterType.Select,
    Required = false,
          DefaultValue = "yellow",
    Options =
       [
       new() { Value = "yellow", Label = "Yellow (Classic)" },
         new() { Value = "blue", Label = "Blue" },
           new() { Value = "green", Label = "Green" },
         new() { Value = "red", Label = "Red" },
     new() { Value = "purple", Label = "Purple" }
  ],
    Description = "Choose a color theme for the sticky note"
   }
  ],
        // No input ports, only output handles for visual arrows
        DataInputs = [],
        DataOutputs =
        [
     new() { Key = "ref-top", Required = false, Description = "Connection from top" },
new() { Key = "ref-right", Required = false, Description = "Connection from right" },
       new() { Key = "ref-bottom", Required = false, Description = "Connection from bottom" },
new() { Key = "ref-left", Required = false, Description = "Connection from left" }
        ]
    };
}

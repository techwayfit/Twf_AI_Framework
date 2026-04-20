using TwfAiFramework.Core;

namespace TwfAiFramework.Web.Models.VisualNodes;

/// <summary>
/// Schema definition for ContainerNode - a visual grouping element.
/// This is a UI-only node that doesn't execute in workflows.
/// Defined in the web layer because it has no business logic.
/// </summary>
public static class ContainerNodeSchema
{
    public const string NodeType = "ContainerNode";
    
    public static NodeParameterSchema GetSchema() => new()
    {
   NodeType = NodeType,
   Description = "Visual container for grouping nodes with customizable background color",
        Parameters =
        [
          new() 
          { 
         Name = "backgroundColor", 
      Label = "Background Color", 
    Type = ParameterType.Color, 
             Required = false, 
                DefaultValue = "#6366f1",
                Description = "Choose a color to theme your container group"
     },
            new() 
{ 
        Name = "opacity", 
         Label = "Opacity", 
  Type = ParameterType.Number, 
      Required = false, 
         DefaultValue = 0.12, 
         MinValue = 0, 
     MaxValue = 1,
                Description = "Background transparency (0=invisible, 1=solid)"
    },
            new() 
    { 
            Name = "width", 
       Label = "Width", 
      Type = ParameterType.Number, 
                Required = false, 
                DefaultValue = 400,
        MinValue = 120,
       Description = "Container width in pixels"
    },
            new() 
        { 
             Name = "height", 
     Label = "Height", 
       Type = ParameterType.Number, 
       Required = false, 
     DefaultValue = 300,
       MinValue = 80,
            Description = "Container height in pixels"
       }
        ],
        // No data ports - purely visual
        DataInputs = [],
    DataOutputs = []
    };
}

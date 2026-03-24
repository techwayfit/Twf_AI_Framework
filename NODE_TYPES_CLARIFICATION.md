# Node Types vs Workflow Features - Important Clarification

## Overview

The TWF AI Framework has a **unique architecture** that differs from traditional visual workflow tools like n8n. Understanding this distinction is crucial for using the workflow designer effectively.

## Two Types of Flow Control

### 1. **Workflow-Level Features** (Code-based, not visual nodes)

These are **NOT individual nodes** you can drag onto the canvas. They are **C# workflow builder methods**:

```csharp
// These are builder methods, not drag-and-drop nodes!
Workflow.Create("MyWorkflow")
    .AddNode(new LlmNode())
    .Branch(       // ? NOT a node - it's a method
  condition: data => data.Get<bool>("is_positive"),
        trueBranch: b => b.AddNode(new PositiveNode()),
    falseBranch: b => b.AddNode(new NegativeNode())
    )
    .Parallel(// ? NOT a node - it's a method
        new SentimentNode(),
    new KeywordNode()
    )
    .ForEach(         // ? NOT a node - it's a method
        itemsKey: "items",
        outputKey: "results",
        bodyBuilder: b => b.AddNode(new ProcessNode())
    )
```

**These features exist in code but cannot be represented as draggable nodes in the visual designer** (yet).

### 2. **Actual Node Types** (Can be used in designer)

These ARE real `INode` implementations that you can drag, drop, and configure:

#### AI Nodes
- `LlmNode` - LLM API calls
- `PromptBuilderNode` - Template-based prompt generation
- `EmbeddingNode` - Vector embeddings for RAG
- `OutputParserNode` - Parse JSON from LLM output

#### Control Nodes
- `ConditionNode` - Evaluate predicates, write boolean flags to WorkflowData
- `DelayNode` - Insert delays for rate limiting
- `MergeNode` - Combine multiple data keys
- `LogNode` - Debug logging checkpoints

#### Data Nodes
- `TransformNode` - Custom data transformations
- `FilterNode` - Data validation
- `ChunkTextNode` - Text chunking for RAG
- `MemoryNode` - Global state read/write

#### IO Nodes
- `HttpRequestNode` - REST API calls

## How to Achieve Branching/Looping

### Option 1: Use `ConditionNode` + Workflow.Branch() in Code

**In the visual designer:**
```
[Input] ? [ConditionNode] ? [Output]
```

The `ConditionNode` evaluates conditions and writes flags like:
- `is_positive = true`
- `needs_escalation = false`

**In your C# code when executing:**
```csharp
var workflow = Workflow.Create("ChatBot")
    .AddNode(new ConditionNode("CheckSentiment",
        ("is_positive", data => data.GetString("sentiment") == "positive")
    ))
    .Branch(
        condition: data => data.Get<bool>("is_positive"),
        trueBranch: b => b.AddNode(new PositiveResponseNode()),
        falseBranch: b => b.AddNode(new EscalationNode())
    )
    .RunAsync(data);
```

### Option 2: Future Enhancement - Visual Branch Nodes

To truly support n8n-style branching in the designer, we would need to implement:

1. **Visual Branch Node** that can be dragged onto canvas
2. **Multiple output ports** (true/false or case1/case2/case3)
3. **Connection routing** based on conditions
4. **Sub-workflow containers** for each branch

**This is a planned enhancement but not yet implemented.**

## Current Limitations of the Visual Designer

The visual designer currently supports:

? **Sequential workflows** (A ? B ? C ? D)  
? **All actual node types** listed above  
? **Node configuration** via properties panel  
? **JSON export/import** of workflow definitions  

? **Conditional branching** (requires code-based Branch method)  
? **Parallel execution** (requires code-based Parallel method)  
? **Loops/ForEach** (requires code-based ForEach method)  
? **Sub-workflows** (not yet implemented)  

## Roadmap for Full Visual Support

To make the designer feature-complete:

### Phase 1: Node Palette Accuracy ?
- [x] Update node palette to show only actual node types
- [x] Remove non-existent If/Switch/Loop nodes
- [x] Add accurate descriptions

### Phase 2: Advanced Visual Elements (Future)
- [ ] Visual branch node with multiple outputs
- [ ] Visual loop node with iteration logic
- [ ] Visual parallel execution node
- [ ] Subworkflow containers
- [ ] Connection labels (true/false, case names)

### Phase 3: Execution Engine Integration (Future)
- [ ] Execute workflows directly from designer
- [ ] Real-time execution visualization
- [ ] Debug mode with step-through
- [ ] Variable inspection

## Workaround for Now

If you need branching/looping in your workflows:

1. **Design the main flow** in the visual designer
2. **Export to JSON**
3. **Load and enhance in code**:

```csharp
// Load the visual workflow
var workflowDef = await repository.GetByIdAsync(workflowId);

// Build it with code-based enhancements
var workflow = Workflow.Create(workflowDef.Name)
    .AddNode(new HttpRequestNode(...))  // From visual design
    .Branch(  // Added in code
        condition: data => data.Get<int>("status") == 200,
  trueBranch: b => b.AddNode(new ProcessNode()),
        falseBranch: b => b.AddNode(new ErrorNode())
    )
    .AddNode(new TransformNode(...));   // From visual design

await workflow.RunAsync(data);
```

## Summary

| Feature | Available in Designer? | How to Use |
|---------|----------------------|------------|
| LLM, Prompt, Embedding nodes | ? Yes | Drag & drop |
| Transform, Filter, Memory nodes | ? Yes | Drag & drop |
| HTTP Request node | ? Yes | Drag & drop |
| Condition, Delay, Log nodes | ? Yes | Drag & drop |
| **Branching (If/Else)** | ? No | Use `Workflow.Branch()` in code |
| **Looping (ForEach)** | ? No | Use `Workflow.ForEach()` in code |
| **Parallel execution** | ? No | Use `Workflow.Parallel()` in code |

---

**The visual designer is best for designing linear/sequential workflows. For complex control flow, use the fluent C# API.**

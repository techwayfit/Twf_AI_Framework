# TwfAiFramework — Complete Documentation Index

Welcome to the comprehensive documentation for **TwfAiFramework** — a lightweight, node-based AI workflow automation library for .NET 10.

---

## ?? Documentation Overview

This documentation suite covers everything from architecture to implementation details:

| Document | Purpose | Audience |
|----------|---------|----------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System design, patterns, and technical architecture | Developers, Architects |
| [USE_CASES.md](USE_CASES.md) | Real-world applications with ROI metrics | Product Managers, Business Analysts |
| [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) | Step-by-step tutorials for common tasks | Developers, Engineers |
| [NODE_REFERENCE.md](NODE_REFERENCE.md) | Complete API reference for all built-in nodes | Developers |
| [creating-a-new-node.md](creating-a-new-node.md) | Guide to extending the framework with custom nodes | Advanced Developers |
| [guide.md](guide.md) | Workflow Designer user guide | End Users, Designers |
| [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md) | Code style and naming standards | Contributors |

---

## ?? Quick Start

### Installation

```bash
dotnet add package TwfAiFramework
```

### Your First Workflow

```csharp
using TwfAiFramework.Core;
using TwfAiFramework.Nodes.AI;

var workflow = Workflow.Create("HelloAI")
    .UseLogger(logger)
    .AddNode(new PromptBuilderNode(
        name: "BuildPrompt",
        promptTemplate: "Answer this question: {{question}}"
    ))
    .AddNode(new LlmNode("ChatGPT", LlmConfig.OpenAI(apiKey, "gpt-4o")));

var result = await workflow.RunAsync(
    WorkflowData.From("question", "What is AI?")
);

Console.WriteLine(result.Data.GetString("llm_response"));
```

**Next Steps:**
1. Read [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) for detailed tutorials
2. Explore [USE_CASES.md](USE_CASES.md) for production examples
3. Check [NODE_REFERENCE.md](NODE_REFERENCE.md) for complete API documentation

---

## ?? Learning Path

### For Developers New to the Framework

**Week 1: Fundamentals**
1. ? Read [README.md](../source/core/README.md) — Core concepts
2. ? Complete [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Section 1-2 — Getting started
3. ? Run `source/console/examples/` — See real workflows
4. ? Review [ARCHITECTURE.md](ARCHITECTURE.md) — Understanding the design

**Week 2: Building Workflows**
1. ? [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Section 3-5 — LLMs, data, control flow
2. ? [NODE_REFERENCE.md](NODE_REFERENCE.md) — Study built-in nodes
3. ? Build your first workflow
4. ? [USE_CASES.md](USE_CASES.md) — Find patterns matching your needs

**Week 3: Advanced Topics**
1. ? [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Section 8-9 — RAG, multi-turn conversations
2. ? [creating-a-new-node.md](creating-a-new-node.md) — Extend the framework
3. ? [ARCHITECTURE.md](ARCHITECTURE.md) Extension Points — Custom providers
4. ? Review test suite in `tests/` — Learn best practices

### For Product Managers / Business Analysts

**Understanding the Framework:**
1. ? [USE_CASES.md](USE_CASES.md) — See what's possible
2. ? [README.md](../source/core/README.md) Quick Start — Basic concepts
3. ? [guide.md](guide.md) — Workflow designer interface

**Defining Requirements:**
1. ? [USE_CASES.md](USE_CASES.md) Section 14 — Common patterns
2. ? Review ROI metrics in each use case
3. ? Map your use case to framework capabilities

### For Architects / Tech Leads

**Technical Evaluation:**
1. ? [ARCHITECTURE.md](ARCHITECTURE.md) — System design and patterns
2. ? Review [twf_ai_framework.csproj](../source/core/twf_ai_framework.csproj) — Dependencies
3. ? [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Section 11-12 — Performance, deployment
4. ? Study test coverage in `tests/`

**Integration Planning:**
1. ? [ARCHITECTURE.md](ARCHITECTURE.md) Infrastructure Layer — Understand extension points
2. ? [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Section 7 — External API integration
3. ? [USE_CASES.md](USE_CASES.md) Section 4 — Data enrichment patterns

---

## ?? Quick Reference

### Core Concepts

| Concept | Description | Learn More |
|---------|-------------|------------|
| **WorkflowData** | Data packet flowing between nodes | [ARCHITECTURE.md](ARCHITECTURE.md#data-flow-architecture) |
| **WorkflowContext** | Execution environment (logger, state, history) | [NODE_REFERENCE.md](NODE_REFERENCE.md#workflowcontext) |
| **INode** | Contract for workflow operations | [ARCHITECTURE.md](ARCHITECTURE.md#node-layer) |
| **BaseNode** | Template for custom nodes | [creating-a-new-node.md](creating-a-new-node.md) |
| **NodeOptions** | Per-node retry, timeout, conditions | [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md#error-handling) |

### Built-In Nodes (27 Total)

| Category | Count | Key Nodes | Reference |
|----------|-------|-----------|-----------|
| **AI** | 4 | LLM, PromptBuilder, OutputParser, Embedding | [NODE_REFERENCE.md](NODE_REFERENCE.md#ai-nodes) |
| **Data** | 6 | Transform, Mapper, Filter, Chunking, Memory | [NODE_REFERENCE.md](NODE_REFERENCE.md#data-nodes) |
| **Control** | 8 | Condition, Branch, TryCatch, Loop, Delay | [NODE_REFERENCE.md](NODE_REFERENCE.md#control-nodes) |
| **IO** | 4 | HTTP, FileReader, FileWriter, GoogleSearch | [NODE_REFERENCE.md](NODE_REFERENCE.md#io-nodes) |
| **Base** | 3 | BaseNode, SimpleTransformNode, LambdaNode | [NODE_REFERENCE.md](NODE_REFERENCE.md#base-classes) |

### Supported LLM Providers

| Provider | Configuration | Streaming | Reference |
|----------|---------------|-----------|-----------|
| **OpenAI** | `LlmConfig.OpenAI(apiKey, model)` | ? Yes | [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md#openai-configuration) |
| **Anthropic** | `LlmConfig.Anthropic(apiKey, model)` | ? Yes | [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md#anthropic-claude-configuration) |
| **Azure OpenAI** | `LlmConfig.AzureOpenAI(key, model, endpoint)` | ? Yes | [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md#azure-openai-configuration) |
| **Ollama** | `LlmConfig.Ollama(model, host)` | ? Yes | [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md#ollama-local-configuration) |
| **Custom** | `LlmConfig.Custom(model, key, endpoint)` | ? Yes | [NODE_REFERENCE.md](NODE_REFERENCE.md#llmnode) |

---

## ?? Find What You Need

### By Task

| I want to... | Go to... |
|--------------|----------|
| **Understand the architecture** | [ARCHITECTURE.md](ARCHITECTURE.md) |
| **See real-world examples** | [USE_CASES.md](USE_CASES.md) or `source/console/examples/` |
| **Learn how to build workflows** | [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) |
| **Find a specific node's API** | [NODE_REFERENCE.md](NODE_REFERENCE.md) |
| **Create a custom node** | [creating-a-new-node.md](creating-a-new-node.md) |
| **Use the visual designer** | [guide.md](guide.md) |
| **Understand naming conventions** | [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md) |

### By Role

**Developers:**
- Start: [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md)
- Reference: [NODE_REFERENCE.md](NODE_REFERENCE.md)
- Advanced: [creating-a-new-node.md](creating-a-new-node.md)

**Architects:**
- Architecture: [ARCHITECTURE.md](ARCHITECTURE.md)
- Deployment: [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md#deployment)
- Integration: [USE_CASES.md](USE_CASES.md)

**Product Managers:**
- Use Cases: [USE_CASES.md](USE_CASES.md)
- ROI Analysis: [USE_CASES.md](USE_CASES.md#roi-summary)
- Capabilities: [NODE_REFERENCE.md](NODE_REFERENCE.md)

**End Users:**
- Designer Guide: [guide.md](guide.md)
- Workflow Patterns: [guide.md](guide.md#common-workflow-patterns)

---

## ??? Project Structure

```
Twf_AI_Framework/
??? docs/        # Documentation (you are here)
?   ??? ARCHITECTURE.md          # System architecture
?   ??? USE_CASES.md        # Real-world applications
?   ??? HOW_TO_GUIDE.md      # Step-by-step tutorials
?   ??? NODE_REFERENCE.md      # Complete API reference
?   ??? creating-a-new-node.md   # Custom node guide
?   ??? guide.md # Designer user guide
?   ??? NAMING_CONVENTIONS.md     # Code standards
?
??? source/
?   ??? core/    # TwfAiFramework (NuGet package)
? ?   ??? Core/      # Workflow engine
?   ?   ??? Nodes/    # Built-in nodes
??   ?   ??? AI/         # LLM, Prompt, Embedding, OutputParser
?   ?   ?   ??? Data/         # Transform, Filter, Chunking, Memory
?   ?   ?   ??? Control/         # Branch, Loop, TryCatch, Condition
?   ?   ?   ??? IO/  # HTTP, File, GoogleSearch
?   ?   ??? Tracking/         # Execution tracking
?   ?   ??? README.md         # Core library README
?   ?
?   ??? console/            # Console examples
?   ?   ??? examples/       # Production workflows
?   ?   ?   ??? CustomerSupportChatbot.cs
?   ?   ?   ??? RagDocumentQA.cs
?   ?   ?   ??? ContentGenerationPipeline.cs
?   ?   ??? concepts/           # Framework fundamentals
?   ?   ??? README.md           # Console app guide
?   ?
?   ??? web/          # ASP.NET Core + React designer
?     ??? Services/# Workflow execution engine
?  ??? designer-react/             # Visual workflow designer
?
??? tests/     # Comprehensive test suite
    ??? Core/    # Core framework tests
  ??? Nodes/         # Node-specific tests
    ??? Integration/         # Integration tests
    ??? README.md   # Testing guide
```

---

## ?? Key Statistics

### Framework Capabilities

- **Built-in Nodes:** 27
- **Node Categories:** 4 (AI, Data, Control, IO)
- **LLM Providers:** 5 (OpenAI, Anthropic, Azure, Ollama, Custom)
- **Control Flow:** Sequential, Parallel, Branching, Looping, Try-Catch
- **Target Framework:** .NET 10
- **Test Coverage:** 90%+ core framework

### Documentation Stats

- **Total Pages:** 7
- **Code Examples:** 200+
- **Use Cases:** 10 detailed scenarios
- **Node Examples:** 27 complete references
- **Tutorial Steps:** 50+ how-to guides

---

## ?? Contributing

### Code Contributions

1. Review [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md)
2. Study [ARCHITECTURE.md](ARCHITECTURE.md) to understand design patterns
3. Add tests for new features (see `tests/` for examples)
4. Follow [creating-a-new-node.md](creating-a-new-node.md) for node contributions

### Documentation Contributions

- Found an error? Open an issue or PR
- Missing use case? Add to [USE_CASES.md](USE_CASES.md)
- Need clarification? Update [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md)
- New pattern? Document in appropriate guide

---

## ?? External Resources

### Official Links

- **GitHub Repository:** https://github.com/techwayfit/Twf_AI_Framework
- **NuGet Package:** https://www.nuget.org/packages/TwfAiFramework
- **License:** MIT

### Learning Resources

- **Design Inspiration:** [n8n.io](https://n8n.io/) — Visual workflow automation
- **Patterns:** [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/)
- **.NET Documentation:** [learn.microsoft.com/dotnet](https://learn.microsoft.com/dotnet)

### LLM Provider Documentation

- **OpenAI API:** [platform.openai.com/docs](https://platform.openai.com/docs)
- **Anthropic Claude:** [docs.anthropic.com](https://docs.anthropic.com)
- **Azure OpenAI:** [learn.microsoft.com/azure/ai-services/openai](https://learn.microsoft.com/azure/ai-services/openai)
- **Ollama:** [ollama.ai/docs](https://ollama.ai/docs)

---

## ?? Version History

### v1.0.1 (Current)
- ? LLM streaming support (SSE)
- ? `OnChunk` callback for real-time responses
- ? Token usage tracking for streamed responses
- ? Prompt sanitization

### v1.0.0
- ? Initial release
- ? 27 built-in nodes
- ? 5 LLM providers
- ? Visual workflow designer
- ? Comprehensive test suite

---

## ?? Getting Help

### Documentation Issues

| Problem | Solution |
|---------|----------|
| **Concept unclear** | Check [ARCHITECTURE.md](ARCHITECTURE.md) for theory |
| **How to implement X** | Search [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) |
| **Node parameter unclear** | See [NODE_REFERENCE.md](NODE_REFERENCE.md) |
| **Use case needed** | Browse [USE_CASES.md](USE_CASES.md) |

### Code Issues

1. Check the [test suite](../tests/) for examples
2. Review [console examples](../source/console/examples/)
3. Search [GitHub issues](https://github.com/techwayfit/Twf_AI_Framework/issues)
4. Open a new issue with:
   - Framework version
   - Minimal reproduction code
   - Expected vs actual behavior

### Feature Requests

1. Check if it exists in [USE_CASES.md](USE_CASES.md)
2. Review [ARCHITECTURE.md](ARCHITECTURE.md) extension points
3. Open a GitHub issue with:
   - Use case description
   - Proposed API
   - Why existing nodes don't solve it

---

## ?? Training Materials

### Workshops

**Half-Day Workshop: Building AI Workflows**
1. Hour 1: Framework fundamentals ([ARCHITECTURE.md](ARCHITECTURE.md))
2. Hour 2: Hands-on with [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Sections 1-5
3. Hour 3: Build a customer support chatbot
4. Hour 4: Extend with custom nodes

**Full-Day Workshop: Production AI Applications**
1. Morning: Complete half-day workshop
2. Afternoon Session 1: RAG pipeline implementation
3. Afternoon Session 2: Multi-agent systems
4. Afternoon Session 3: Deployment and monitoring

### Self-Paced Learning

**Track 1: Developer Fundamentals (8 hours)**
- [README.md](../source/core/README.md) (1 hour)
- [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Sections 1-6 (3 hours)
- Build 3 workflows from scratch (2 hours)
- [NODE_REFERENCE.md](NODE_REFERENCE.md) deep dive (2 hours)

**Track 2: Advanced Development (8 hours)**
- [ARCHITECTURE.md](ARCHITECTURE.md) complete (2 hours)
- [creating-a-new-node.md](creating-a-new-node.md) (1 hour)
- Create 2 custom nodes (3 hours)
- Review test patterns in `tests/` (2 hours)

**Track 3: Production Deployment (4 hours)**
- [USE_CASES.md](USE_CASES.md) relevant sections (1 hour)
- [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) Sections 11-12 (2 hours)
- Deploy sample application (1 hour)

---

## ?? Roadmap

### Planned Features

**Version 1.1 (Q2 2025)**
- Workflow versioning
- Enhanced caching layer
- Plugin system for dynamic node loading
- Workflow marketplace

**Version 1.2 (Q3 2025)**
- Distributed execution
- Advanced monitoring and observability
- Built-in A/B testing
- Workflow templates library

**Version 2.0 (Q4 2025)**
- GraphQL API
- Real-time collaboration in designer
- Workflow as Code DSL
- Cloud-native deployment templates

---

## ?? Documentation Standards

### When to Update Documentation

- **New Node:** Update [NODE_REFERENCE.md](NODE_REFERENCE.md) + [creating-a-new-node.md](creating-a-new-node.md)
- **New Pattern:** Add to [USE_CASES.md](USE_CASES.md)
- **API Change:** Update [HOW_TO_GUIDE.md](HOW_TO_GUIDE.md) + [NODE_REFERENCE.md](NODE_REFERENCE.md)
- **Architecture Change:** Update [ARCHITECTURE.md](ARCHITECTURE.md)

### Documentation Review Checklist

- [ ] Code examples compile and run
- [ ] Cross-references work (links to other docs)
- [ ] Version-specific features noted
- [ ] Examples follow [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md)
- [ ] Table of contents updated

---

## ?? Acknowledgments

### Inspiration

- **n8n** — Visual workflow automation that inspired the node-based approach
- **Apache Airflow** — DAG orchestration patterns
- **LangChain** — LLM chaining concepts

### Community

- All contributors who provided feedback
- Early adopters who tested the framework
- Open source projects that made this possible

---

## ?? Contact

- **Issues:** [GitHub Issues](https://github.com/techwayfit/Twf_AI_Framework/issues)
- **Discussions:** [GitHub Discussions](https://github.com/techwayfit/Twf_AI_Framework/discussions)
- **Repository:** [github.com/techwayfit/Twf_AI_Framework](https://github.com/techwayfit/Twf_AI_Framework)

---

**Last Updated:** January 2025  
**Documentation Version:** 1.0.1  
**Framework Version:** 1.0.1

---

Made with ?? by the TwfAiFramework team

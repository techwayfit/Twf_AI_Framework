using twf_ai_framework.Core.Models;

namespace TwfAiFramework.Tests.Core;

/// <summary>
/// Tests for WorkflowContext - the execution context shared across all nodes.
/// Covers logging, tracking, state management, services, and chat history.
/// </summary>
public class WorkflowContextTests
{
    [Fact]
    public void Constructor_Should_Initialize_With_Unique_RunId()
    {
     // Act
        var context1 = new WorkflowContext("Workflow1", NullLogger.Instance);
        var context2 = new WorkflowContext("Workflow1", NullLogger.Instance);

        // Assert
        context1.RunId.Should().NotBeNullOrEmpty();
        context2.RunId.Should().NotBeNullOrEmpty();
 context1.RunId.Should().NotBe(context2.RunId);
    }

    [Fact]
    public void Constructor_Should_Set_WorkflowName()
    {
        // Act
        var context = new WorkflowContext("MyWorkflow", NullLogger.Instance);

        // Assert
        context.WorkflowName.Should().Be("MyWorkflow");
    }

    [Fact]
    public void Constructor_Should_Set_StartedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var context = new WorkflowContext("Test", NullLogger.Instance);

        // Assert
        context.StartedAt.Should().BeOnOrAfter(before);
        context.StartedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void SetState_And_GetState_Should_Store_And_Retrieve_Values()
    {
        // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);

        // Act
context.SetState("key1", "value1");
        context.SetState("key2", 42);

        // Assert
        context.GetState<string>("key1").Should().Be("value1");
        context.GetState<int>("key2").Should().Be(42);
    }

    [Fact]
    public void GetState_Should_Return_Default_When_Key_Not_Found()
    {
    // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);

        // Assert
  context.GetState<string>("nonexistent").Should().BeNull();
        context.GetState<int>("nonexistent").Should().Be(0);
    }

  [Fact]
    public void HasState_Should_Return_True_When_Key_Exists()
    {
        // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);
  context.SetState("existing", "value");

        // Assert
        context.HasState("existing").Should().BeTrue();
        context.HasState("nonexistent").Should().BeFalse();
    }

[Fact]
    public void AppendMessage_Should_Add_To_Chat_History()
    {
        // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);

     // Act
        context.AppendMessage(ChatMessage.User("Hello"));
  context.AppendMessage(ChatMessage.Assistant("Hi there!"));

        // Assert
        var history = context.GetChatHistory();
        history.Should().HaveCount(2);
        history[0].Role.Should().Be("user");
        history[0].Content.Should().Be("Hello");
      history[1].Role.Should().Be("assistant");
     history[1].Content.Should().Be("Hi there!");
    }

    [Fact]
    public void GetChatHistory_Should_Return_Empty_List_Initially()
    {
        // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);

        // Act
        var history = context.GetChatHistory();

        // Assert
        history.Should().BeEmpty();
  }

    [Fact]
    public void ClearChatHistory_Should_Remove_All_Messages()
  {
    // Arrange
    var context = new WorkflowContext("Test", NullLogger.Instance);
     context.AppendMessage(ChatMessage.User("Message 1"));
        context.AppendMessage(ChatMessage.User("Message 2"));

// Act
        context.ClearChatHistory();

        // Assert
        context.GetChatHistory().Should().BeEmpty();
    }

    [Fact]
    public void ChatMessage_System_Should_Create_System_Message()
    {
        // Act
        var message = ChatMessage.System("System instruction");

        // Assert
        message.Role.Should().Be("system");
        message.Content.Should().Be("System instruction");
     message.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
public void ChatMessage_User_Should_Create_User_Message()
    {
        // Act
        var message = ChatMessage.User("User question");

        // Assert
    message.Role.Should().Be("user");
        message.Content.Should().Be("User question");
    }

    [Fact]
    public void ChatMessage_Assistant_Should_Create_Assistant_Message()
    {
// Act
    var message = ChatMessage.Assistant("Assistant response");

        // Assert
        message.Role.Should().Be("assistant");
        message.Content.Should().Be("Assistant response");
    }

    [Fact]
    public void RegisterService_And_GetService_Should_Work()
    {
     // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);
        var service = new TestService { Value = "test" };

        // Act
        context.RegisterService(service);
        var retrieved = context.GetService<TestService>();

        // Assert
        retrieved.Should().BeSameAs(service);
        retrieved.Value.Should().Be("test");
    }

    [Fact]
    public void GetService_Should_Throw_When_Service_Not_Registered()
    {
   // Arrange
     var context = new WorkflowContext("Test", NullLogger.Instance);

     // Act & Assert
      var act = () => context.GetService<TestService>();
        act.Should().Throw<InvalidOperationException>()
        .WithMessage("*Service 'TestService' not registered*");
    }

    [Fact]
    public void HasService_Should_Return_True_When_Service_Registered()
    {
        // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);
      context.RegisterService(new TestService());

        // Assert
        context.HasService<TestService>().Should().BeTrue();
        context.HasService<AnotherService>().Should().BeFalse();
    }

    [Fact]
    public void WithService_Should_Register_And_Return_Context()
  {
        // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);
        var service = new TestService();

    // Act
var returned = context.WithService(service);

        // Assert
      returned.Should().BeSameAs(context);
        context.GetService<TestService>().Should().BeSameAs(service);
    }

    [Fact]
    public void WithState_Should_Set_State_And_Return_Context()
    {
   // Arrange
        var context = new WorkflowContext("Test", NullLogger.Instance);

     // Act
        var returned = context.WithState("key", "value");

        // Assert
        returned.Should().BeSameAs(context);
        context.GetState<string>("key").Should().Be("value");
    }

    [Fact]
    public void Fluent_Builder_Should_Chain()
    {
        // Arrange & Act
     var context = new WorkflowContext("Test", NullLogger.Instance)
     .WithService(new TestService { Value = "test" })
            .WithState("key1", "value1")
  .WithState("key2", 42);

        // Assert
        context.GetService<TestService>().Value.Should().Be("test");
        context.GetState<string>("key1").Should().Be("value1");
      context.GetState<int>("key2").Should().Be(42);
    }

    [Fact]
    public void CancellationToken_Should_Default_To_None()
    {
 // Arrange & Act
   var context = new WorkflowContext("Test", NullLogger.Instance);

        // Assert
     context.CancellationToken.Should().Be(CancellationToken.None);
    }

    [Fact]
    public void CancellationToken_Should_Be_Passed_Through()
    {
      // Arrange
        var cts = new CancellationTokenSource();

        // Act
   var context = new WorkflowContext("Test", NullLogger.Instance, cancellationToken: cts.Token);

    // Assert
        context.CancellationToken.Should().Be(cts.Token);
    }

    [Fact]
    public void Tracker_Should_Be_Initialized()
    {
  // Arrange & Act
        var context = new WorkflowContext("Test", NullLogger.Instance);

        // Assert
      context.Tracker.Should().NotBeNull();
    }

    // Helper test services
    private class TestService
    {
        public string Value { get; set; } = string.Empty;
    }

    private class AnotherService { }
}

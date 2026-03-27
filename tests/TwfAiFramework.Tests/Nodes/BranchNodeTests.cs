using TwfAiFramework.Nodes.Control;

namespace TwfAiFramework.Tests.Nodes;

public class BranchNodeTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Select_Case1_With_CaseInsensitive_Matching_By_Default()
    {
        var node = new BranchNode(
            "BranchByStatus",
            "status",
            case1Value: "approved",
            case2Value: "pending",
            case3Value: "rejected");

        var input = new WorkflowData().Set("status", "APPROVED");
        var context = new WorkflowContext("Test", NullLogger.Instance);
        var result = await node.ExecuteAsync(input, context);

        result.IsSuccess.Should().BeTrue();
        result.Data.Get<string>("branch_selected_port").Should().Be("case1");
        result.Data.Get<bool>("branch_case1").Should().BeTrue();
        result.Data.Get<bool>("branch_case2").Should().BeFalse();
        result.Data.Get<bool>("branch_case3").Should().BeFalse();
        result.Data.Get<bool>("branch_default").Should().BeFalse();
        result.Metadata["selected_port"].Should().Be("case1");
    }

    [Fact]
    public async Task ExecuteAsync_Should_Select_Default_When_CaseSensitive_And_Value_Differs_By_Casing()
    {
        var node = new BranchNode(
            "BranchCaseSensitive",
            "status",
            case1Value: "approved",
            caseSensitive: true);

        var input = new WorkflowData().Set("status", "APPROVED");
        var context = new WorkflowContext("Test", NullLogger.Instance);
        var result = await node.ExecuteAsync(input, context);

        result.IsSuccess.Should().BeTrue();
        result.Data.Get<string>("branch_selected_port").Should().Be("default");
        result.Data.Get<bool>("branch_default").Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Select_Default_When_No_Cases_Match()
    {
        var node = new BranchNode(
            "BranchNoMatch",
            "priority",
            case1Value: "high",
            case2Value: "medium",
            case3Value: "low");

        var input = new WorkflowData().Set("priority", "urgent");
        var context = new WorkflowContext("Test", NullLogger.Instance);
        var result = await node.ExecuteAsync(input, context);

        result.IsSuccess.Should().BeTrue();
        result.Data.Get<string>("branch_selected_port").Should().Be("default");
        result.Data.Get<string?>("branch_selected_value").Should().BeNull();
        result.Data.Get<bool>("branch_default").Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Select_First_Matching_Case_When_Multiple_Cases_Have_Same_Value()
    {
        var node = new BranchNode(
            "BranchFirstMatchWins",
            "status",
            case1Value: "same",
            case2Value: "same",
            case3Value: "same");

        var input = new WorkflowData().Set("status", "same");
        var context = new WorkflowContext("Test", NullLogger.Instance);
        var result = await node.ExecuteAsync(input, context);

        result.IsSuccess.Should().BeTrue();
        result.Data.Get<string>("branch_selected_port").Should().Be("case1");
        result.Data.Get<bool>("branch_case1").Should().BeTrue();
        result.Data.Get<bool>("branch_case2").Should().BeFalse();
        result.Data.Get<bool>("branch_case3").Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Select_Default_When_ValueKey_Is_Missing()
    {
        var node = new BranchNode(
            "BranchMissingValue",
            "missing_key",
            case1Value: "x");

        var input = new WorkflowData();
        var context = new WorkflowContext("Test", NullLogger.Instance);
        var result = await node.ExecuteAsync(input, context);

        result.IsSuccess.Should().BeTrue();
        result.Data.Get<string>("branch_selected_port").Should().Be("default");
        result.Data.Get<bool>("branch_default").Should().BeTrue();
    }
}

using TwfAiFramework.Web.Models;

namespace TwfAiFramework.Web.Repositories;

public interface IWorkflowInstanceRepository
{
    Task<WorkflowInstance> CreateAsync(WorkflowInstance instance);
    Task<WorkflowInstance> UpdateAsync(WorkflowInstance instance);
    Task<WorkflowInstance?> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkflowInstance>> GetByWorkflowIdAsync(Guid workflowDefinitionId);
}

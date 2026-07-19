using ExpenseTracker.Api.DTOs.Momo;

namespace ExpenseTracker.Api.Services.Momo;

public interface IMomoAssistantService
{
    Task<MomoChatResponse> ChatAsync(
        MomoChatRequest request,
        CancellationToken cancellationToken);
}

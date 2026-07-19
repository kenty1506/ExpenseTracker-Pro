using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.AuditTrails;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Tests.AuditTrails;

public sealed class AuditTrailServiceTests
{
    [Fact]
    public async Task GetModuleAsync_UsesCurrentUserAndModuleFilter()
    {
        var repository = new RecordingAuditTrailRepository();
        var currentUser = new TestCurrentUserService("user-123");
        var service = new AuditTrailService(
            repository,
            currentUser);

        var result = await service.GetModuleAsync(
            " Transfers ",
            new AuditTrailQueryDto());

        Assert.Equal("user-123", repository.RequestedUserId);
        Assert.Equal("Transfers", repository.RequestedQuery?.Module);
        Assert.Single(result.Items);
        Assert.Equal("Transfers", result.Items[0].Module);
        Assert.Equal("7", result.Items[0].EntityId);
    }

    [Fact]
    public async Task GetConsolidatedAsync_RejectsInvalidDateRange()
    {
        var service = new AuditTrailService(
            new RecordingAuditTrailRepository(),
            new TestCurrentUserService("user-123"));

        var query = new AuditTrailQueryDto
        {
            FromUtc = new DateTime(2026, 7, 18, 12, 0, 0,
                DateTimeKind.Utc),
            ToUtc = new DateTime(2026, 7, 17, 12, 0, 0,
                DateTimeKind.Utc)
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetConsolidatedAsync(query));
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public TestCurrentUserService(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }

        public bool IsAuthenticated => true;
    }

    private sealed class RecordingAuditTrailRepository
        : IAuditTrailRepository
    {
        public string? RequestedUserId { get; private set; }

        public AuditTrailQueryDto? RequestedQuery { get; private set; }

        public Task<PagedResult<AuditLog>> GetPagedAsync(
            string userId,
            AuditTrailQueryDto query)
        {
            RequestedUserId = userId;
            RequestedQuery = query;

            return Task.FromResult(new PagedResult<AuditLog>
            {
                Page = 1,
                PageSize = 20,
                TotalRecords = 1,
                TotalPages = 1,
                Items =
                [
                    new AuditLog
                    {
                        Id = 1,
                        UserId = userId,
                        Module = query.Module ?? "Accounts",
                        Operation = "Create",
                        EntityId = "7",
                        Method = "POST",
                        Route = "/api/v1/transfers",
                        Action = "Transfers.Create",
                        StatusCode = 201,
                        Succeeded = true,
                        ElapsedMilliseconds = 12,
                        TraceId = "test-trace",
                        CreatedAtUtc = DateTime.UtcNow
                    }
                ]
            });
        }

        public Task<AuditLog?> GetByIdAsync(
            long id,
            string userId)
        {
            return Task.FromResult<AuditLog?>(null);
        }

        public Task<IReadOnlyList<AuditModuleSummaryDto>>
            GetModuleSummaryAsync(
                string userId,
                DateTime? fromUtc,
                DateTime? toUtc)
        {
            IReadOnlyList<AuditModuleSummaryDto> result =
                Array.Empty<AuditModuleSummaryDto>();

            return Task.FromResult(result);
        }
    }
}

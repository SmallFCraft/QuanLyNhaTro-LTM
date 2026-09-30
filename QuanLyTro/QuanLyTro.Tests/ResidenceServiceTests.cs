using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class ResidenceServiceTests
{
    [TestMethod]
    [ExpectedException(typeof(BusinessRuleException))]
    public async Task ExportHistory_EndDateBeforeStartDate_ThrowsException()
    {
        var service = new ResidenceService(repo: null!);
        var req = new ExportHistoryRequest(
            FromDate: new DateTime(2026, 10, 1),
            ToDate: new DateTime(2026, 9, 1),
            Format: "CSV",
            RoomNumber: null,
            EventType: null);

        await service.ExportHistoryAsync(req, CancellationToken.None);
    }

    [TestMethod]
    [ExpectedException(typeof(BusinessRuleException))]
    public async Task GetHistory_EndDateBeforeStartDate_ThrowsException()
    {
        var service = new ResidenceService(repo: null!);
        await service.GetHistoryAsync(
            from: new DateTime(2026, 10, 1),
            to: new DateTime(2026, 9, 1),
            room: null,
            CancellationToken.None);
    }
}

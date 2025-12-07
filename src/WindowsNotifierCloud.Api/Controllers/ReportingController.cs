using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Api.Models.Reporting;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportingController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReportingController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<SummaryReportDto>> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var (start, end) = NormalizeRange(from, to);

        var events = _db.TelemetryEvents.AsQueryable()
            .Where(e => e.OccurredAtUtc >= start && e.OccurredAtUtc <= end);

        var counts = await events
            .GroupBy(e => 1)
            .Select(g => new
            {
                Total = g.Count(),
                ToastShown = g.Count(x => x.EventType == TelemetryEventType.ToastShown),
                ButtonOk = g.Count(x => x.EventType == TelemetryEventType.ButtonOk),
                ButtonMoreInfo = g.Count(x => x.EventType == TelemetryEventType.ButtonMoreInfo),
                Dismissed = g.Count(x => x.EventType == TelemetryEventType.Dismissed),
                TimedOut = g.Count(x => x.EventType == TelemetryEventType.TimedOut),
                ScriptError = g.Count(x => x.EventType == TelemetryEventType.ScriptError),
                ConditionCheck = g.Count(x => x.EventType == TelemetryEventType.ConditionCheck),
                Completed = g.Count(x => x.EventType == TelemetryEventType.Completed)
            })
            .FirstOrDefaultAsync(ct) ?? new
            {
                Total = 0,
                ToastShown = 0,
                ButtonOk = 0,
                ButtonMoreInfo = 0,
                Dismissed = 0,
                TimedOut = 0,
                ScriptError = 0,
                ConditionCheck = 0,
                Completed = 0
            };

        return new SummaryReportDto(
            counts.Total,
            counts.ToastShown,
            counts.ButtonOk,
            counts.ButtonMoreInfo,
            counts.Dismissed,
            counts.TimedOut,
            counts.ScriptError,
            counts.ConditionCheck,
            counts.Completed,
            start,
            end);
    }

    [HttpGet("modules")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<IEnumerable<ModuleReportDto>>> GetModules([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var (start, end) = NormalizeRange(from, to);

        var telemetry = _db.TelemetryEvents
            .Where(e => e.OccurredAtUtc >= start && e.OccurredAtUtc <= end);

        var modules = await _db.ModuleDefinitions
            .Include(m => m.Campaign)
            .ToListAsync(ct);

        var grouped = await telemetry
            .GroupBy(e => e.ModuleId)
            .Select(g => new
            {
                ModuleId = g.Key,
                ToastShown = g.Count(x => x.EventType == TelemetryEventType.ToastShown),
                ButtonOk = g.Count(x => x.EventType == TelemetryEventType.ButtonOk),
                ButtonMoreInfo = g.Count(x => x.EventType == TelemetryEventType.ButtonMoreInfo),
                Dismissed = g.Count(x => x.EventType == TelemetryEventType.Dismissed),
                TimedOut = g.Count(x => x.EventType == TelemetryEventType.TimedOut),
                ScriptError = g.Count(x => x.EventType == TelemetryEventType.ScriptError),
                ConditionCheck = g.Count(x => x.EventType == TelemetryEventType.ConditionCheck),
                Completed = g.Count(x => x.EventType == TelemetryEventType.Completed),
                First = g.Min(x => (DateTime?)x.OccurredAtUtc),
                Last = g.Max(x => (DateTime?)x.OccurredAtUtc)
            })
            .ToListAsync(ct);

        var dto = grouped.Select(g =>
        {
            var module = modules.FirstOrDefault(m => m.ModuleId == g.ModuleId);
            return new ModuleReportDto(
                g.ModuleId,
                module?.DisplayName,
                module?.Type.ToString(),
                module?.Category.ToString(),
                module?.Campaign?.Name,
                g.ToastShown,
                g.ButtonOk,
                g.ButtonMoreInfo,
                g.Dismissed,
                g.TimedOut,
                g.ScriptError,
                g.ConditionCheck,
                g.Completed,
                g.First,
                g.Last);
        });

        return Ok(dto);
    }

    [HttpGet("modules/{moduleId}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<ModuleReportDto>> GetModule(string moduleId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var (start, end) = NormalizeRange(from, to);

        var module = await _db.ModuleDefinitions.Include(m => m.Campaign).FirstOrDefaultAsync(m => m.ModuleId == moduleId, ct);
        if (module == null) return NotFound();

        var g = await _db.TelemetryEvents
            .Where(e => e.ModuleId == moduleId && e.OccurredAtUtc >= start && e.OccurredAtUtc <= end)
            .GroupBy(e => e.ModuleId)
            .Select(g => new
            {
                ToastShown = g.Count(x => x.EventType == TelemetryEventType.ToastShown),
                ButtonOk = g.Count(x => x.EventType == TelemetryEventType.ButtonOk),
                ButtonMoreInfo = g.Count(x => x.EventType == TelemetryEventType.ButtonMoreInfo),
                Dismissed = g.Count(x => x.EventType == TelemetryEventType.Dismissed),
                TimedOut = g.Count(x => x.EventType == TelemetryEventType.TimedOut),
                ScriptError = g.Count(x => x.EventType == TelemetryEventType.ScriptError),
                ConditionCheck = g.Count(x => x.EventType == TelemetryEventType.ConditionCheck),
                Completed = g.Count(x => x.EventType == TelemetryEventType.Completed),
                First = g.Min(x => (DateTime?)x.OccurredAtUtc),
                Last = g.Max(x => (DateTime?)x.OccurredAtUtc)
            })
            .FirstOrDefaultAsync(ct);

        g ??= new
        {
            ToastShown = 0,
            ButtonOk = 0,
            ButtonMoreInfo = 0,
            Dismissed = 0,
            TimedOut = 0,
            ScriptError = 0,
            ConditionCheck = 0,
            Completed = 0,
            First = (DateTime?)null,
            Last = (DateTime?)null
        };

        var dto = new ModuleReportDto(
            module.ModuleId,
            module.DisplayName,
            module.Type.ToString(),
            module.Category.ToString(),
            module.Campaign?.Name,
            g.ToastShown,
            g.ButtonOk,
            g.ButtonMoreInfo,
            g.Dismissed,
            g.TimedOut,
            g.ScriptError,
            g.ConditionCheck,
            g.Completed,
            g.First,
            g.Last);

        return dto;
    }

    [HttpGet("campaigns/{campaignId:guid}")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public async Task<ActionResult<CampaignReportDto>> GetCampaign(Guid campaignId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var (start, end) = NormalizeRange(from, to);

        var campaign = await _db.Campaigns.Include(c => c.Modules).FirstOrDefaultAsync(c => c.Id == campaignId, ct);
        if (campaign == null) return NotFound();

        var moduleIds = campaign.Modules.Select(m => m.ModuleId).ToList();
        var telemetry = _db.TelemetryEvents
            .Where(e => moduleIds.Contains(e.ModuleId) && e.OccurredAtUtc >= start && e.OccurredAtUtc <= end);

        var agg = await telemetry.GroupBy(e => 1).Select(g => new
        {
            ToastShown = g.Count(x => x.EventType == TelemetryEventType.ToastShown),
            ButtonOk = g.Count(x => x.EventType == TelemetryEventType.ButtonOk),
            ButtonMoreInfo = g.Count(x => x.EventType == TelemetryEventType.ButtonMoreInfo),
            Dismissed = g.Count(x => x.EventType == TelemetryEventType.Dismissed),
            TimedOut = g.Count(x => x.EventType == TelemetryEventType.TimedOut),
            Completed = g.Count(x => x.EventType == TelemetryEventType.Completed)
        }).FirstOrDefaultAsync(ct) ?? new
        {
            ToastShown = 0,
            ButtonOk = 0,
            ButtonMoreInfo = 0,
            Dismissed = 0,
            TimedOut = 0,
            Completed = 0
        };

        return new CampaignReportDto(
            campaign.Id,
            campaign.Name,
            campaign.Modules.Count,
            agg.ToastShown,
            agg.ButtonOk,
            agg.ButtonMoreInfo,
            agg.Dismissed,
            agg.TimedOut,
            agg.Completed);
    }

    private static (DateTime start, DateTime end) NormalizeRange(DateTime? from, DateTime? to)
    {
        var start = from ?? DateTime.UtcNow.AddDays(-30);
        var end = to ?? DateTime.UtcNow;
        if (end < start)
        {
            (start, end) = (end, start);
        }
        return (start, end);
    }
}

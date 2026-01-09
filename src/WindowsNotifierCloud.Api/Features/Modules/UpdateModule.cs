using MediatR;
using WindowsNotifierCloud.Api.Models.Modules;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.Linq;

namespace WindowsNotifierCloud.Api.Features.Modules;

public class UpdateModule
{
    public class Command : IRequest<ModuleDefinition>
    {
        public Guid Id { get; set; }
        public ModuleUpsertRequest Request { get; set; }
        public ClaimsPrincipal User { get; set; }

        public Command(Guid id, ModuleUpsertRequest request, ClaimsPrincipal user)
        {
            Id = id;
            Request = request;
            User = user;
        }
    }

    public class Handler : IRequestHandler<Command, ModuleDefinition>
    {
        private readonly IModuleRepository _modules;
        private readonly ApplicationDbContext _db;

        public Handler(IModuleRepository modules, ApplicationDbContext db)
        {
            _modules = modules;
            _db = db;
        }

        public async Task<ModuleDefinition> Handle(Command command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var request = command.Request;
            var user = command.User;

            var entity = await _modules.GetAsync(id, cancellationToken);
            if (entity == null) return null;

            var role = user.FindFirst("role")?.Value;
            var disallowed = (role == "Basic") && entity.Type != ModuleType.Standard;
            if (disallowed)
            {
                throw new UnauthorizedAccessException("Basic users cannot edit this module type.");
            }

            if (request.Type == ModuleType.Hero || entity.Type == ModuleType.Hero)
            {
                if (string.IsNullOrWhiteSpace(request.Title ?? entity.Title))
                {
                    throw new ArgumentException("Hero notifications require a title.");
                }
                request.Message = null;
                request.IconFileName = null;
                request.IconOriginalName = null;
            }

            var modifiedBy = ResolveUserId(user);

            entity.UpdateDetails(request.DisplayName, request.Description, request.Category, modifiedBy);
            entity.UpdateContent(request.Title, request.Message, request.LinkUrl, modifiedBy);
            entity.UpdateScripts(request.ConditionalScriptBody, request.ConditionalIntervalMinutes, request.DynamicScriptBody, modifiedBy);
            entity.UpdateSchedule(request.ScheduleUtc, request.ExpiresUtc, request.ReminderHours, modifiedBy);
            entity.UpdateMedia(request.IconFileName, request.IconOriginalName, request.HeroFileName, request.HeroOriginalName, modifiedBy);
            entity.UpdateDynamicOptions(request.DynamicMaxLength, request.DynamicTrimWhitespace, request.DynamicFailIfEmpty, request.DynamicFallbackMessage, modifiedBy);
            entity.UpdateCoreSettings(request.CoreSettings, modifiedBy);

            await _modules.SaveChangesAsync(cancellationToken);
            return entity;
        }

        private Guid ResolveUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? user.FindFirst("sub")?.Value;
                      
            if (Guid.TryParse(sub, out var parsed))
            {
                return parsed;
            }
            
            // fallback
            var fallback = _db.PortalUsers.Select(u => u.Id).FirstOrDefault();
            return fallback != Guid.Empty ? fallback : Guid.NewGuid();
        }
    }
}

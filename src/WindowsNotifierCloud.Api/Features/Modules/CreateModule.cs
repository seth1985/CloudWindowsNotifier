using MediatR;
using WindowsNotifierCloud.Api.Models.Modules;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using WindowsNotifierCloud.Infrastructure.Persistence;
using FluentValidation;

namespace WindowsNotifierCloud.Api.Features.Modules;

public class CreateModule
{
    public class Command : IRequest<ModuleDefinition>
    {
        public ModuleUpsertRequest Request { get; set; }
        public ClaimsPrincipal User { get; set; }

        public Command(ModuleUpsertRequest request, ClaimsPrincipal user)
        {
            Request = request;
            User = user;
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.DisplayName).NotEmpty().WithMessage("DisplayName is required.");
            RuleFor(x => x.Request.ModuleId).NotEmpty().WithMessage("ModuleId is required.");
            
            When(x => x.Request.Type == ModuleType.Hero, () => {
                RuleFor(x => x.Request.Title).NotEmpty().WithMessage("Hero notifications require a title.");
            });
        }
    }

    public class Handler : IRequestHandler<Command, ModuleDefinition>
    {
        private readonly IModuleRepository _modules;
        private readonly ApplicationDbContext _db;
        private readonly IValidator<Command> _validator;

        public Handler(IModuleRepository modules, ApplicationDbContext db, IValidator<Command> validator)
        {
            _modules = modules;
            _db = db;
            _validator = validator;
        }

        public async Task<ModuleDefinition> Handle(Command command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var request = command.Request;
            var user = command.User;

            if (request.Type == ModuleType.Hero)
            {
               // Hero specific logic
               request.IconFileName = null;
               request.IconOriginalName = null;
            }

            // Simple role check (logic moved from controller)
            var role = user.FindFirst("role")?.Value;
            var disallowed = (role == "Basic") && request.Type != ModuleType.Standard;
            if (disallowed)
            {
                throw new UnauthorizedAccessException("Basic users can only create Standard modules.");
            }

            var userId = ResolveUserId(user);

            var entity = ModuleDefinition.Create(
                request.DisplayName,
                request.ModuleId,
                request.Type,
                request.Category,
                userId
            );

            entity.UpdateDetails(request.DisplayName, request.Description, request.Category, userId);
            entity.UpdateContent(request.Title, request.Message, request.LinkUrl, userId);
            entity.UpdateScripts(request.ConditionalScriptBody, request.ConditionalIntervalMinutes, request.DynamicScriptBody, userId);
            entity.UpdateSchedule(request.ScheduleUtc, request.ExpiresUtc, request.ReminderHours, userId);
            entity.UpdateMedia(request.IconFileName, request.IconOriginalName, request.HeroFileName, request.HeroOriginalName, userId);
            entity.UpdateDynamicOptions(request.DynamicMaxLength, request.DynamicTrimWhitespace, request.DynamicFailIfEmpty, request.DynamicFallbackMessage, userId);
            entity.UpdateCoreSettings(request.CoreSettings, userId);

            await _modules.AddAsync(entity, cancellationToken);
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

            // Fallback logic
            return Guid.Empty; 
        }
    }
}

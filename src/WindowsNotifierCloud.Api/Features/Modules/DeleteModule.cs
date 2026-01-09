using MediatR;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;
using WindowsNotifierCloud.Api.Services;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Security.Claims;

namespace WindowsNotifierCloud.Api.Features.Modules;

public class DeleteModule
{
    public class Command : IRequest<bool>
    {
        public Guid Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public Command(Guid id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IModuleRepository _modules;
        private readonly StorageCleanupService _cleanup;

        public Handler(IModuleRepository modules, StorageCleanupService cleanup)
        {
            _modules = modules;
            _cleanup = cleanup;
        }

        public async Task<bool> Handle(Command command, CancellationToken cancellationToken)
        {
            var id = command.Id;
            var user = command.User;

            var entity = await _modules.GetAsync(id, cancellationToken);
            if (entity == null) return false;

            var role = user.FindFirst("role")?.Value;
            var disallowed = (role == "Basic") && entity.Type != ModuleType.Standard;
            if (disallowed)
            {
                throw new UnauthorizedAccessException("Basic users cannot delete this module type.");
            }

            _modules.Delete(entity);
            await _modules.SaveChangesAsync(cancellationToken);

            _cleanup.RemoveModuleArtifacts(entity.Id, entity.ModuleId);
            return true;
        }
    }
}

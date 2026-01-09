using MediatR;
using WindowsNotifierCloud.Api.Models.Campaigns;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Persistence;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Linq;

namespace WindowsNotifierCloud.Api.Features.Campaigns;

public class CreateCampaign
{
    public class Command : IRequest<Campaign>
    {
        public CampaignCreateRequest Request { get; set; }
        public ClaimsPrincipal User { get; set; }

        public Command(CampaignCreateRequest request, ClaimsPrincipal user)
        {
            Request = request;
            User = user;
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.");
        }
    }

    public class Handler : IRequestHandler<Command, Campaign>
    {
        private readonly ICampaignRepository _campaigns;
        private readonly ApplicationDbContext _db;
        private readonly IValidator<Command> _validator;

        public Handler(ICampaignRepository campaigns, ApplicationDbContext db, IValidator<Command> validator)
        {
            _campaigns = campaigns;
            _db = db;
            _validator = validator;
        }

        public async Task<Campaign> Handle(Command command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var request = command.Request;
            var user = command.User;
            var userId = ResolveUserId(user);

            var campaign = new Campaign
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            await _campaigns.AddAsync(campaign, cancellationToken);
            await _campaigns.SaveChangesAsync(cancellationToken);

            return campaign;
        }

        private Guid ResolveUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? user.FindFirst("sub")?.Value;
            if (Guid.TryParse(sub, out var parsed)) return parsed;
            
            // Fallback
            var fallback = _db.PortalUsers.Select(u => u.Id).FirstOrDefault();
            return fallback != Guid.Empty ? fallback : Guid.NewGuid();
        }
    }
}

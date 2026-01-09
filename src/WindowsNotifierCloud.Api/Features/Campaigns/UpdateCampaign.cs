using MediatR;
using WindowsNotifierCloud.Api.Models.Campaigns;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace WindowsNotifierCloud.Api.Features.Campaigns;

public class UpdateCampaign
{
    public class Command : IRequest<Campaign?>
    {
        public Guid Id { get; set; }
        public CampaignUpdateRequest Request { get; set; }

        public Command(Guid id, CampaignUpdateRequest request)
        {
            Id = id;
            Request = request;
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // Name is optional in UpdateRequest (if null, ignored), 
            // but if provided it should not be empty.
            RuleFor(x => x.Request.Name)
                .Must(name => name == null || !string.IsNullOrWhiteSpace(name))
                .WithMessage("Name cannot be empty if provided.");
        }
    }

    public class Handler : IRequestHandler<Command, Campaign?>
    {
        private readonly ICampaignRepository _campaigns;
        private readonly IValidator<Command> _validator;

        public Handler(ICampaignRepository campaigns, IValidator<Command> validator)
        {
            _campaigns = campaigns;
            _validator = validator;
        }

        public async Task<Campaign?> Handle(Command command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);
            
            var entity = await _campaigns.GetAsync(command.Id, cancellationToken);
            if (entity == null) return null;

            var request = command.Request;

            entity.Name = request.Name?.Trim() ?? entity.Name;
            entity.Description = request.Description?.Trim();

            await _campaigns.SaveChangesAsync(cancellationToken);
            return entity;
        }
    }
}

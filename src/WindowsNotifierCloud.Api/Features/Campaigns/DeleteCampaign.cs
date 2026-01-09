using MediatR;
using WindowsNotifierCloud.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace WindowsNotifierCloud.Api.Features.Campaigns;

public class DeleteCampaign
{
    public class Command : IRequest<bool>
    {
        public Guid Id { get; set; }

        public Command(Guid id)
        {
            Id = id;
        }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ICampaignRepository _campaigns;

        public Handler(ICampaignRepository campaigns)
        {
            _campaigns = campaigns;
        }

        public async Task<bool> Handle(Command command, CancellationToken cancellationToken)
        {
            var entity = await _campaigns.GetAsync(command.Id, cancellationToken);
            if (entity == null) return false;

            _campaigns.Delete(entity);
            await _campaigns.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

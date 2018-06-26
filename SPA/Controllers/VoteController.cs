namespace SPA.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Bson;
    using SPA.Messaging;

    [ApiController]
    public class VoteController : ControllerBase
    {
        private readonly IMediator mediator;

        public VoteController(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        public async Task<ActionResult<ObjectId>> Post([FromBody]AddVoteRequest request, CancellationToken cancellationToken)
        {
            return await this.mediator.Send(request, cancellationToken);
        }
    }
}

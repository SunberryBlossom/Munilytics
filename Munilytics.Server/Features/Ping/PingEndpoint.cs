using FastEndpoints;

namespace Munilytics.Server.Features.Ping
{
    public class PingEndpoint : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/ping");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            await Send.OkAsync("Pong!");
        }
    }
}

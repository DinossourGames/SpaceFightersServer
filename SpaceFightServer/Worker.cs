using LiteNetLib;
using LiteNetLib.Utils;

namespace SpaceFightServer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly NetManager _server;
    private readonly EventBasedNetListener _listener;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Server");
        _server.Start(9050);
        _logger.LogInformation("Server Started!");

        _listener.ConnectionRequestEvent += request => request.AcceptIfKey("SomeConnectionKey");

        _listener.PeerConnectedEvent += peer =>
        {
            _logger.LogInformation("Income connection: {peerAddress}", peer.EndPoint);
            var writer = new NetDataWriter();
            writer.Put("Hello client!");
            peer.Send(writer, DeliveryMethod.Sequenced);
        };

        _listener.NetworkReceiveEvent += (peer, reader, channel, method) =>
        {
            _logger.LogInformation("Peer: {peer} -- response: {response}", peer.EndPoint.Address,
                reader.GetString(100));
        };
        
        _logger.LogInformation("Starting Pooling Events");

        while (!stoppingToken.IsCancellationRequested)
        {
            _server.PollEvents();
            Thread.Sleep(15);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _server.Stop();
        return base.StopAsync(cancellationToken);
    }
}
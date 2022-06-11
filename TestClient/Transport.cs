using LiteNetLib;
namespace TestClient;

public class TransportTests
{
    [Test]
    public void CanConnectWithSocketUsingRightKey()
    {
        var listener = new EventBasedNetListener();
        var client = new NetManager(listener);
        
        client.Start();
        client.Connect("localhost", 9050, "SomeConnectionKey");

        listener.NetworkReceiveEvent += (peer, reader, channel, method) =>
        {
            var content = reader.GetString(100);
            Assert.That(content, Is.EqualTo("Hello client!"));
        };

        Thread.Sleep(200);
        
        client.PollEvents();
        client.Stop();
    }
    
    [Test]
    public void CantConnectWithSocketWithoutKey()
    {
        var listener = new EventBasedNetListener();
        var client = new NetManager(listener);
        
        client.Start();
        client.Connect("localhost", 9050, "SomeConnectionKey!");

        listener.NetworkReceiveEvent += (peer, reader, channel, method) =>
        {
            var content = reader.GetString(100);
            Assert.That(content, Is.Not.EqualTo("Hello client!"));
        };

        Thread.Sleep(200);
        
        client.PollEvents();
        client.Stop();
    }
}
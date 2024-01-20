using Azure.Core;
using Azure.Core.Pipeline;

namespace tests.Utility;

public static class FakeClientOptions
{
    public static T GetClientOptions<T>(T options) where T : ClientOptions
    {
        var clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        options.Transport = new HttpClientTransport(clientHandler);

        return options;
    }
}
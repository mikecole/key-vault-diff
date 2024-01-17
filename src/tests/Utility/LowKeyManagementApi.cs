using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace tests.Utility;

public static class LowKeyManagementApi
{
    public static async Task CreateVault(string vaultUri)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        
        var client = new HttpClient(handler);
        client.BaseAddress = new Uri("https://localhost:8443/");

        var response = await client.PostAsync("management/vault", JsonContent.Create(new
        {
            BaseUri = vaultUri,
            RecoveryLevel = "Recoverable",
            RecoverableDays = 90,
        }));

        response.EnsureSuccessStatusCode();
    }
    
    public static async Task DeleteVault(string vaultUri)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:8443/")
        };
        
        client.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"management/vault?baseUri={HttpUtility.UrlEncode(vaultUri)}");
        deleteRequest.Content = JsonContent.Create("");

        var response = await client.SendAsync(deleteRequest);
        
        response.EnsureSuccessStatusCode();
    }
    
    public static async Task PurgeDeletedVault(string vaultUri)
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:8443/")
        };
        
        client.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"management/vault/purge?baseUri={HttpUtility.UrlEncode(vaultUri)}");
        deleteRequest.Content = JsonContent.Create("");

        var response = await client.SendAsync(deleteRequest);
        
        response.EnsureSuccessStatusCode();
    }
}
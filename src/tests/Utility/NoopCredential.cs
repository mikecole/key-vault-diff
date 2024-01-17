using Azure.Core;

namespace tests.Utility;

public class NoopCredentials: TokenCredential
{
    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(GetToken(requestContext, cancellationToken));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new AccessToken("noop", DateTimeOffset.MaxValue);
    }
}
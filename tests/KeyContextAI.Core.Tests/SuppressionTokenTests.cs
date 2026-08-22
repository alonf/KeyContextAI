using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Tests;

public sealed class SuppressionTokenTests
{
    [Fact]
    public void Create_ProducesNonEmptyToken()
    {
        var token = SuppressionToken.Create();

        Assert.NotEqual(Guid.Empty, token.Value);
    }

    [Fact]
    public void Default_TokenCarriesTheEmptyGuid()
    {
        Assert.Equal(Guid.Empty, default(SuppressionToken).Value);
    }
}

using RaceDay.Api.Services;
using Xunit;
namespace RaceDay.Api.Tests;
public class PasswordServiceTests
{
    [Fact]
    public void HashDoesNotContainPlainTextAndVerifiesCorrectPassword()
    {
        var service = new PasswordService();
        const string password = "SafePass!123";
        var hash = service.Hash(password);
        Assert.DoesNotContain(password, hash);
        Assert.True(service.Verify(password, hash));
        Assert.False(service.Verify("wrong-password", hash));
    }
}

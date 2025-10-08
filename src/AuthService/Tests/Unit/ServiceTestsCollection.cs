#if DEBUG
using Xunit;

namespace AuthService.Tests.UnitTests
{
    [CollectionDefinition("ServiceTests")]
    public class ServiceTestsCollection : ICollectionFixture<AccessTokenServiceTests>
    {
        // This class has no code, and is never created. 
        // Its purpose is to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
    }
}
#endif

using AutoMapper;
using UserService.Interfaces.Data;
using UserService.Interfaces;
using UserService.Interfaces.Infrastructure;
using Moq;
using UserService.Services;

namespace UserService.Tests.Unit
{
    public class UserProfileServiceTests
    {
        protected readonly Mock<IProfilesRepository> ProfilesRepositoryMock;
        protected readonly Mock<IFollowChecker> FollowCheckerMock;
        protected readonly Mock<IRedisService> RedisServiceMock;
        protected readonly Mock<IMapper> MapperMock;
        protected readonly UserProfileService UserProfileService;

        public UserProfileServiceTests()
        {
            ProfilesRepositoryMock = new Mock<IProfilesRepository>();
            FollowCheckerMock = new Mock<IFollowChecker>();
            RedisServiceMock = new Mock<IRedisService>();
            MapperMock = new Mock<IMapper>();
            var logger = new Mock<ILogger<UserProfileService>>();

            //UserProfileService = new UserProfileService(
            //    ProfilesRepositoryMock.Object,
            //    FollowCheckerMock.Object,
            //    MapperMock.Object,
            //    RedisServiceMock.Object,
            //    logger.Object);
        }
    }
}

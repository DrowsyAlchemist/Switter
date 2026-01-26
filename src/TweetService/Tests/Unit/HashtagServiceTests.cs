using FluentAssertions;
using Moq;
using TweetService.Exceptions;
using TweetService.Interfaces.Data.Repositories;
using TweetService.Models;
using TweetService.Services;
using Xunit;

namespace TweetService.Tests.Unit
{
    public class HashtagServiceTests
    {
        private readonly Mock<IHashtagRepository> _hashtagRepositoryMock;
        private readonly Mock<ITweetRepository> _tweetRepositoryMock;
        private readonly Mock<ITweetHashtagRepository> _tweetHashtagRepositoryMock;
        private readonly HashtagService _hashtagService;

        public HashtagServiceTests()
        {
            _hashtagRepositoryMock = new Mock<IHashtagRepository>();
            _tweetRepositoryMock = new Mock<ITweetRepository>();
            _tweetHashtagRepositoryMock = new Mock<ITweetHashtagRepository>();

            _hashtagService = new HashtagService(
                _hashtagRepositoryMock.Object,
                _tweetRepositoryMock.Object,
                _tweetHashtagRepositoryMock.Object
            );
        }

        [Fact]
        public async Task ProcessHashtagsAsync_NullContent_ThrowsArgumentNullException()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            string content = null;

            // Act
            Func<Task> act = async () => await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task ProcessHashtagsAsync_EmptyContent_DoesNothing()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "";

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task ProcessHashtagsAsync_ContentWithoutHashtags_DoesNothing()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "This is a tweet without hashtags";

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task ProcessHashtagsAsync_HashtagLengthZero_ThrowsInvalidHashtagException()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "#";

            // Act
            Func<Task> act = async () => await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            await act.Should().ThrowAsync<InvalidHashtagException>()
                .WithMessage($"Invalid hashtag length (0).");
            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task ProcessHashtagsAsync_HashtagLengthExceeds50_ThrowsInvalidHashtagException()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var longHashtag = new string('a', 51);
            var content = $"#{longHashtag}";

            // Act
            Func<Task> act = async () => await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            await act.Should().ThrowAsync<InvalidHashtagException>()
                .WithMessage($"Invalid hashtag length (51).");
            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task ProcessHashtagsAsync_HashtagLengthExactly50_ProcessesSuccessfully()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var longHashtag = new string('a', 50);
            var content = $"#{longHashtag}";
            var hashtags = new List<string> { longHashtag };
            var hashtagIds = new Dictionary<string, Guid> { { longHashtag, Guid.NewGuid() } };

            SetupRepositoriesForHashtags(hashtags, hashtags, new List<string>(), hashtagIds);

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyAllRepositoriesCalled(hashtags, new List<string>(), hashtags, hashtagIds, tweetId);
        }

        [Fact]
        public async Task ProcessHashtagsAsync_MultipleValidHashtags_ProcessesCorrectly()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "Learning #csharp and #dotnet #programming";
            var hashtags = new List<string> { "csharp", "dotnet", "programming" };
            var existingHashtags = new List<string> { "csharp", "dotnet" };
            var newHashtags = new List<string> { "programming" };
            var hashtagIds = new Dictionary<string, Guid>
        {
            { "csharp", Guid.NewGuid() },
            { "dotnet", Guid.NewGuid() },
            { "programming", Guid.NewGuid() }
        };

            SetupRepositoriesForHashtags(hashtags, existingHashtags, newHashtags, hashtagIds);

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyAllRepositoriesCalled(hashtags, newHashtags, existingHashtags, hashtagIds, tweetId);
        }

        [Fact]
        public async Task ProcessHashtagsAsync_OnlyNewHashtags_AddsAllAndCreatesTweetHashtags()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "#new1 #new2 #new3";
            var hashtags = new List<string> { "new1", "new2", "new3" };
            var existingHashtags = new List<string>();
            var newHashtags = new List<string> { "new1", "new2", "new3" };
            var hashtagIds = new Dictionary<string, Guid>
        {
            { "new1", Guid.NewGuid() },
            { "new2", Guid.NewGuid() },
            { "new3", Guid.NewGuid() }
        };

            SetupRepositoriesForHashtags(hashtags, existingHashtags, newHashtags, hashtagIds);

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyAllRepositoriesCalled(hashtags, newHashtags, existingHashtags, hashtagIds, tweetId);
        }

        [Fact]
        public async Task ProcessHashtagsAsync_OnlyExistingHashtags_IncrementsAndCreatesTweetHashtags()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "#existing1 #existing2";
            var hashtags = new List<string> { "existing1", "existing2" };
            var existingHashtags = new List<string> { "existing1", "existing2" };
            var newHashtags = new List<string>();
            var hashtagIds = new Dictionary<string, Guid>
        {
            { "existing1", Guid.NewGuid() },
            { "existing2", Guid.NewGuid() }
        };

            SetupRepositoriesForHashtags(hashtags, existingHashtags, newHashtags, hashtagIds);

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyAllRepositoriesCalled(hashtags, newHashtags, existingHashtags, hashtagIds, tweetId);
        }

        [Fact]
        public async Task ProcessHashtagsAsync_DuplicateHashtagsInContent_ProcessesOnceEach()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "#csharp #dotnet #csharp #dotnet";
            var hashtags = new List<string> { "csharp", "dotnet" };
            var existingHashtags = new List<string> { "csharp" };
            var newHashtags = new List<string> { "dotnet" };
            var hashtagIds = new Dictionary<string, Guid>
        {
            { "csharp", Guid.NewGuid() },
            { "dotnet", Guid.NewGuid() }
        };

            SetupRepositoriesForHashtags(hashtags, existingHashtags, newHashtags, hashtagIds);

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyAllRepositoriesCalled(hashtags, newHashtags, existingHashtags, hashtagIds, tweetId);
        }

        [Fact]
        public async Task ProcessHashtagsAsync_MixedValidAndInvalidHashtags_ThrowsOnFirstInvalid()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var longHashtag = new string('a', 51);
            var content = $"#valid #{longHashtag}";

            // Act
            Func<Task> act = async () => await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            await act.Should().ThrowAsync<InvalidHashtagException>()
                .WithMessage($"Invalid hashtag length (51).");
            VerifyNoRepositoryCalls();
        }

        [Fact]
        public async Task ProcessHashtagsAsync_ContentWithExtraSpaces_ProcessesCorrectly()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var content = "  #first    #second   #third  ";
            var hashtags = new List<string> { "first", "second", "third" };
            var existingHashtags = new List<string> { "first", "third" };
            var newHashtags = new List<string> { "second" };
            var hashtagIds = new Dictionary<string, Guid>
        {
            { "first", Guid.NewGuid() },
            { "second", Guid.NewGuid() },
            { "third", Guid.NewGuid() }
        };

            SetupRepositoriesForHashtags(hashtags, existingHashtags, newHashtags, hashtagIds);

            // Act
            await _hashtagService.ProcessHashtagsAsync(tweetId, content);

            // Assert
            VerifyAllRepositoriesCalled(hashtags, newHashtags, existingHashtags, hashtagIds, tweetId);
        }

        private void SetupRepositoriesForHashtags(
            List<string> allHashtags,
            List<string> existingHashtags,
            List<string> newHashtags,
            Dictionary<string, Guid> hashtagIds)
        {
            _hashtagRepositoryMock
                .Setup(r => r.GetExists(allHashtags))
                .ReturnsAsync(existingHashtags);

            _hashtagRepositoryMock
                .Setup(r => r.GetIdByTag(allHashtags))
                .ReturnsAsync(hashtagIds.Select(h => h.Value).ToList());
        }

        private void VerifyAllRepositoriesCalled(
            List<string> allHashtags,
            List<string> newHashtags,
            List<string> existingHashtags,
            Dictionary<string, Guid> hashtagIds,
            Guid tweetId)
        {
            _hashtagRepositoryMock.Verify(r => r.GetExists(allHashtags), Times.Once);

            if (newHashtags.Any())
                _hashtagRepositoryMock.Verify(r => r.AddRangeAsync(newHashtags), Times.Once);

            _hashtagRepositoryMock.Verify(r => r.IncrementUsageCounterAsync(existingHashtags), Times.Once);
            _hashtagRepositoryMock.Verify(r => r.GetIdByTag(allHashtags), Times.Once);

            _tweetHashtagRepositoryMock.Verify(r => r.AddRangeAsync(
                It.Is<List<TweetHashtag>>(list =>
                    list.Count == hashtagIds.Count &&
                    list.All(th => th.TweetId == tweetId) &&
                    list.All(th => hashtagIds.ContainsValue(th.HashtagId))
                )), Times.Once);
        }

        private void VerifyNoRepositoryCalls()
        {
            _hashtagRepositoryMock.Verify(r => r.GetExists(It.IsAny<List<string>>()), Times.Never);
            _hashtagRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<string>>()), Times.Never);
            _hashtagRepositoryMock.Verify(r => r.IncrementUsageCounterAsync(It.IsAny<List<string>>()), Times.Never);
            _hashtagRepositoryMock.Verify(r => r.GetIdByTag(It.IsAny<List<string>>()), Times.Never);
            _tweetHashtagRepositoryMock.Verify(r => r.AddRangeAsync(It.IsAny<List<TweetHashtag>>()), Times.Never);
        }
    }
}
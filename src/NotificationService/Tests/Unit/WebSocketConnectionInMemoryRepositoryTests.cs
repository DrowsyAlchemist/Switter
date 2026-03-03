using FluentAssertions;
using Moq;
using NotificationService.Interfaces;
using NotificationService.Services;
using Xunit;

namespace NotificationService.Tests.Unit
{
    public class WebSocketConnectionInMemoryRepositoryTests
    {
        private readonly Mock<ILogger<WebSocketConnectionInMemoryRepository>> _loggerMock;
        private readonly IWebSocketConnectionRepository _repository;

        public WebSocketConnectionInMemoryRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<WebSocketConnectionInMemoryRepository>>();
            _repository = new WebSocketConnectionInMemoryRepository(_loggerMock.Object);
        }

        [Fact]
        public void AddConnection_WithValidData_AddsConnectionSuccessfully()
        {
            // Arrange
            var connectionId = "conn-123";
            var userId = Guid.NewGuid();
            var userAgent = "Mozilla/5.0";
            var ipAddress = "192.168.1.1";

            // Act
            _repository.AddConnection(connectionId, userId, userAgent, ipAddress);

            // Assert
            var connection = _repository.GetConnection(connectionId);
            connection.Should().NotBeNull();
            connection!.ConnectionId.Should().Be(connectionId);
            connection.UserId.Should().Be(userId);
            connection.UserAgent.Should().Be(userAgent);
            connection.IpAddress.Should().Be(ipAddress);
            connection.ConnectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void AddConnection_WithNullOrEmptyConnectionId_ThrowsArgumentNullException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var act = () => _repository.AddConnection(null!, userId, null, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddConnection_WithMultipleConnectionsForSameUser_StoresAllConnections()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var connectionId1 = "conn-1";
            var connectionId2 = "conn-2";

            // Act
            _repository.AddConnection(connectionId1, userId, null, null);
            _repository.AddConnection(connectionId2, userId, null, null);

            // Assert
            var userConnections = _repository.GetUserConnections(userId);
            userConnections.Should().HaveCount(2);
            userConnections.Should().Contain(connectionId1);
            userConnections.Should().Contain(connectionId2);
        }

        [Fact]
        public void RemoveConnection_WithExistingConnection_RemovesSuccessfully()
        {
            // Arrange
            var connectionId = "conn-123";
            var userId = Guid.NewGuid();
            _repository.AddConnection(connectionId, userId, null, null);

            // Act
            _repository.RemoveConnection(connectionId);

            // Assert
            var connection = _repository.GetConnection(connectionId);
            connection.Should().BeNull();

            var userConnections = _repository.GetUserConnections(userId);
            userConnections.Should().BeEmpty();
        }

        [Fact]
        public void RemoveConnection_WithNullOrEmptyConnectionId_ThrowsArgumentNullException()
        {
            // Act
            var act = () => _repository.RemoveConnection(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void RemoveConnection_WithNonExistentConnection_DoesNothing()
        {
            // Arrange
            var initialCount = _repository.GetConnectionCount();
            var nonExistentId = "non-existent";

            // Act
            _repository.RemoveConnection(nonExistentId);

            // Assert
            _repository.GetConnectionCount().Should().Be(initialCount);
        }

        [Fact]
        public void RemoveConnection_WithLastUserConnection_RemovesUserFromConnectedUsers()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var connectionId = "conn-123";
            _repository.AddConnection(connectionId, userId, null, null);

            // Act
            _repository.RemoveConnection(connectionId);

            // Assert
            var connectedUsers = _repository.GetConnectedUsers();
            connectedUsers.Should().NotContain(userId);
        }

        [Fact]
        public void GetUserConnections_WithExistingUser_ReturnsAllConnections()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var connectionId1 = "conn-1";
            var connectionId2 = "conn-2";
            _repository.AddConnection(connectionId1, userId, null, null);
            _repository.AddConnection(connectionId2, userId, null, null);

            // Act
            var connections = _repository.GetUserConnections(userId);

            // Assert
            connections.Should().HaveCount(2);
            connections.Should().BeEquivalentTo([connectionId1, connectionId2]);
        }

        [Fact]
        public void GetUserConnections_WithNonExistentUser_ReturnsEmptyList()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var connections = _repository.GetUserConnections(nonExistentUserId);

            // Assert
            connections.Should().BeEmpty();
        }

        [Fact]
        public void GetConnectedUsers_ReturnsAllUsersWithActiveConnections()
        {
            // Arrange
            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            _repository.AddConnection("conn-1", user1, null, null);
            _repository.AddConnection("conn-2", user1, null, null);
            _repository.AddConnection("conn-3", user2, null, null);

            // Act
            var connectedUsers = _repository.GetConnectedUsers();

            // Assert
            connectedUsers.Should().HaveCount(2);
            connectedUsers.Should().Contain(user1);
            connectedUsers.Should().Contain(user2);
        }

        [Fact]
        public void GetConnectionCount_ReturnsTotalNumberOfConnections()
        {
            // Arrange
            _repository.AddConnection("conn-1", Guid.NewGuid(), null, null);
            _repository.AddConnection("conn-2", Guid.NewGuid(), null, null);
            _repository.AddConnection("conn-3", Guid.NewGuid(), null, null);

            // Act
            var count = _repository.GetConnectionCount();

            // Assert
            count.Should().Be(3);
        }

        [Fact]
        public void GetConnection_WithExistingId_ReturnsConnection()
        {
            // Arrange
            var connectionId = "conn-123";
            var userId = Guid.NewGuid();
            _repository.AddConnection(connectionId, userId, "test-agent", "127.0.0.1");

            // Act
            var connection = _repository.GetConnection(connectionId);

            // Assert
            connection.Should().NotBeNull();
            connection!.ConnectionId.Should().Be(connectionId);
            connection.UserId.Should().Be(userId);
        }

        [Fact]
        public void GetConnection_WithNonExistentId_ReturnsNull()
        {
            // Act
            var connection = _repository.GetConnection("non-existent");

            // Assert
            connection.Should().BeNull();
        }

        [Fact]
        public void CleanupOldConnections_RemovesConnectionsOlderThanMaxAge()
        {
            // Arrange
            var oldConnectionId = "old-conn";
            var newConnectionId = "new-conn";
            var userId = Guid.NewGuid();

            _repository.AddConnection(oldConnectionId, userId, null, null);
            Thread.Sleep(10);
            _repository.AddConnection(newConnectionId, userId, null, null);

            var maxAge = TimeSpan.FromMilliseconds(5);

            // Act
            _repository.CleanupOldConnections(maxAge);

            // Assert
            var oldConnection = _repository.GetConnection(oldConnectionId);
            var newConnection = _repository.GetConnection(newConnectionId);

            oldConnection.Should().BeNull();
            newConnection.Should().NotBeNull();
        }

        [Fact]
        public void CleanupOldConnections_WithNoOldConnections_RemovesNothing()
        {
            // Arrange
            var connectionId = "conn-123";
            var userId = Guid.NewGuid();
            _repository.AddConnection(connectionId, userId, null, null);

            var initialCount = _repository.GetConnectionCount();
            var maxAge = TimeSpan.FromHours(1);

            // Act
            _repository.CleanupOldConnections(maxAge);

            // Assert
            _repository.GetConnectionCount().Should().Be(initialCount);
            var connection = _repository.GetConnection(connectionId);
            connection.Should().NotBeNull();
        }

        [Fact]
        public void CleanupOldConnections_RemovesUserFromConnectedUsers_WhenLastConnectionRemoved()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldConnectionId = "old-conn";

            _repository.AddConnection(oldConnectionId, userId, null, null);

            Thread.Sleep(10);

            var maxAge = TimeSpan.FromMilliseconds(5);

            // Act
            _repository.CleanupOldConnections(maxAge);

            // Assert
            var connectedUsers = _repository.GetConnectedUsers();
            connectedUsers.Should().NotContain(userId);
        }

        [Fact]
        public void MultipleOperations_WithSameData_MaintainConsistency()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var connectionId1 = "conn-1";
            var connectionId2 = "conn-2";

            // Act & Assert 
            _repository.AddConnection(connectionId1, userId, null, null);
            _repository.AddConnection(connectionId2, userId, null, null);

            _repository.GetConnectionCount().Should().Be(2);
            _repository.GetUserConnections(userId).Should().HaveCount(2);
            _repository.GetConnectedUsers().Should().ContainSingle();

            // Remove one
            _repository.RemoveConnection(connectionId1);

            _repository.GetConnectionCount().Should().Be(1);
            _repository.GetUserConnections(userId).Should().ContainSingle(connectionId2);
            _repository.GetConnectedUsers().Should().ContainSingle();

            // Remove last
            _repository.RemoveConnection(connectionId2);

            _repository.GetConnectionCount().Should().Be(0);
            _repository.GetUserConnections(userId).Should().BeEmpty();
            _repository.GetConnectedUsers().Should().BeEmpty();
        }
    }
}

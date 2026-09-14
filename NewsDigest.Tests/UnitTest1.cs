using FluentAssertions;
using NewsDigest.Core.Models;
using Xunit;

namespace NewsDigest.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test_MessageType_Values()
        {
            // Testing enum values for MessageType
            var values = Enum.GetValues(typeof(MessageType));
            values.Should().Contain(MessageType.Text);
            values.Should().Contain(MessageType.MediaWithCaption);
            values.Should().Contain(MessageType.MediaWithoutText);
        }

        [Fact]
        public void Test_UserModel_Creation()
        {
            // Testing User model creation
            var user = new User
            {
                Id = 1,
                TelegramUserId = 123456789L,
                CreatedAt = DateTime.Now
            };
            
            user.Id.Should().Be(1);
            user.TelegramUserId.Should().Be(123456789L);
        }

        [Fact]
        public void Test_UserChannelModel_Creation()
        {
            // Testing UserChannel model creation
            var channel = new UserChannel
            {
                Id = 1,
                UserId = 1,
                ChannelId = 987654321L,
                ChannelUsername = "test_channel",
                Title = "Test Channel",
                CreatedAt = DateTime.Now
            };
            
            channel.Id.Should().Be(1);
            channel.UserId.Should().Be(1);
            channel.ChannelId.Should().Be(987654321L);
        }
    }
}
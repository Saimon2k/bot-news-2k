using FluentAssertions;

namespace NewsDigest.Tests;

public class BotCommandParserTests
{
    [Fact]
    public void Parse_news_without_argument_preserves_default_behavior()
    {
        var command = BotCommandParser.Parse("/news", 123);

        command.Type.Should().Be(BotCommandType.News);
        command.Argument.Should().BeEmpty();
        command.TelegramUserId.Should().Be(123);
    }

    [Theory]
    [InlineData("/news 24", "24")]
    [InlineData("/news   24", "24")]
    public void Parse_news_preserves_lookback_hours(string text, string expectedHours)
    {
        var command = BotCommandParser.Parse(text, 123);

        command.Type.Should().Be(BotCommandType.News);
        command.Argument.Should().Be(expectedHours);
    }
}

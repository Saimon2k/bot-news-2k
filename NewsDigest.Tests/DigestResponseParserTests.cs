using FluentAssertions;

namespace NewsDigest.Tests;

public class DigestResponseParserTests
{
    [Fact]
    public void Parses_valid_json_with_string_ids()
    {
        const string json = """
            {"selected":[{"id":"1","reason":"важная"}],"ads":["2"],"duplicates":["3"]}
            """;

        var result = DigestResponseParser.TryParse(json);

        result.Should().NotBeNull();
        result!.Selected.Should().ContainSingle();
        result.Selected[0].Id.Should().Be(1);
        result.Selected[0].Reason.Should().Be("важная");
        result.Ads.Should().Contain(2);
        result.Duplicates.Should().Contain(3);
    }

    [Fact]
    public void Parses_numeric_ids_and_missing_reason()
    {
        const string json = """{"selected":[{"id":5}],"ads":[1],"duplicates":[]}""";

        var result = DigestResponseParser.TryParse(json);

        result.Should().NotBeNull();
        result!.Selected.Should().ContainSingle().Which.Id.Should().Be(5);
        result.Selected[0].Reason.Should().BeEmpty();
        result.Ads.Should().Equal(1);
        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public void Returns_null_for_invalid_json()
    {
        DigestResponseParser.TryParse("this is not json").Should().BeNull();
        DigestResponseParser.TryParse(null).Should().BeNull();
    }

    [Fact]
    public void Skips_unknown_ids_in_selected()
    {
        const string json = """{"selected":[{"id":"abc","reason":"x"},{"id":"7","reason":"ok"}],"ads":[],"duplicates":[]}""";

        var result = DigestResponseParser.TryParse(json);

        result.Should().NotBeNull();
        result!.Selected.Should().ContainSingle().Which.Id.Should().Be(7);
    }
}
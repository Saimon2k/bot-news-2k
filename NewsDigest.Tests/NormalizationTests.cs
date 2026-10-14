using FluentAssertions;

namespace NewsDigest.Tests;

public class NormalizationTests
{
    [Fact]
    public void Normalize_lowercases_and_trims()
    {
        NewsNormalizer.Normalize("  Hello WORLD  ").Should().Be("hello world");
    }

    [Fact]
    public void Normalize_removes_urls_hashtags_and_mentions()
    {
        var result = NewsNormalizer.Normalize("Смотри https://example.com/abc #news @channel");

        result.Should().NotContain("https:").And.NotContain("#news").And.NotContain("@channel").And.NotBeEmpty();
    }

    [Fact]
    public void Same_semantic_text_gives_same_hash()
    {
        var first = NewsNormalizer.ComputeHash(NewsNormalizer.Normalize("  Foo  bar "));
        var second = NewsNormalizer.ComputeHash(NewsNormalizer.Normalize("foo bar"));

        first.Should().Be(second);
    }

    [Fact]
    public void Empty_text_normalizes_to_empty_string()
    {
        NewsNormalizer.Normalize(null).Should().BeEmpty();
        NewsNormalizer.Normalize("   ").Should().BeEmpty();
    }

    [Fact]
    public void Canonical_selection_marks_later_duplicate()
    {
        var first = new CollectedMessage { Id = 1, Date = new DateTime(2026, 1, 1, 10, 0, 0), NormalizedText = "news", ContentHash = "h1" };
        var second = new CollectedMessage { Id = 2, Date = new DateTime(2026, 1, 1, 12, 0, 0), NormalizedText = "news", ContentHash = "h1" };
        var third = new CollectedMessage { Id = 3, Date = new DateTime(2026, 1, 2, 10, 0, 0), NormalizedText = "other", ContentHash = "h2" };

        NewsNormalizer.SelectCanonicals([first, second, third]);

        first.IsCanonical.Should().BeTrue();
        first.DuplicateOfMessageId.Should().BeNull();
        second.IsCanonical.Should().BeFalse();
        second.DuplicateOfMessageId.Should().Be(first.Id);
        third.IsCanonical.Should().BeTrue();
    }
}
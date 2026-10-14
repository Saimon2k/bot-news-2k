using FluentAssertions;

namespace NewsDigest.Tests;

public class ChannelStateRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = TestDb.CreateContext();
    private readonly ChannelStateRepository _repository;

    public ChannelStateRepositoryTests() => _repository = new ChannelStateRepository(_context);

    [Fact]
    public async Task GetOrCreates_state_and_returns_default()
    {
        var state = await _repository.GetOrCreateAsync(1, 987654321);

        state.Id.Should().BeGreaterThan(0);
        state.LastDigestDate.Should().Be(default);
    }

    [Fact]
    public async Task Update_last_digest_persists()
    {
        var created = await _repository.GetOrCreateAsync(1, 987654321);
        var date = new DateTime(2026, 1, 1, 10, 30, 0);

        await _repository.UpdateLastDigestAsync(created.UserId, created.ChannelId, date, 42);

        var loaded = await _repository.GetAsync(1, 987654321);
        loaded.Should().NotBeNull();
        loaded!.LastDigestDate.Should().Be(date);
        loaded.LastMessageId.Should().Be(42);
    }

    [Fact]
    public async Task States_are_independent_per_user()
    {
        await _repository.UpdateLastDigestAsync(1, 1, new DateTime(2026, 1, 1), 1);

        var other = await _repository.GetAsync(2, 1);

        other.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
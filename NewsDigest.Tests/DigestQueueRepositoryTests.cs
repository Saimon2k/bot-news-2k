using FluentAssertions;
using NewsDigest.Core.Models;
using NewsDigest.Infrastructure.Data;
using NewsDigest.Infrastructure.Repositories;

namespace NewsDigest.Tests;

public class DigestQueueRepositoryTests : IDisposable
{
    private readonly AppDbContext _context = TestDb.CreateContext();
    private readonly DigestQueueRepository _repository;

    public DigestQueueRepositoryTests() => _repository = new DigestQueueRepository(_context);

    [Fact]
    public async Task Next_item_returns_unsent_in_order()
    {
        var queue = await _repository.CreateQueueAsync(new DigestQueue { UserId = 1 });
        await _repository.AddItemsAsync(
        [
            new DigestItem { DigestQueueId = queue.Id, OrderIndex = 0, MessageUrl = "a" },
            new DigestItem { DigestQueueId = queue.Id, OrderIndex = 1, MessageUrl = "b" },
        ]);

        var first = await _repository.GetNextItemAsync(1);
        var second = await _repository.GetNextItemAsync(1);

        first.Should().NotBeNull();
        first!.MessageUrl.Should().Be("a");
        second.Should().NotBeNull();
        second!.MessageUrl.Should().Be("b");
    }

    [Fact]
    public async Task Mark_as_sent_hides_item_from_next()
    {
        var queue = await _repository.CreateQueueAsync(new DigestQueue { UserId = 1 });
        var item = new DigestItem { DigestQueueId = queue.Id, OrderIndex = 0, MessageUrl = "a" };
        await _repository.AddItemsAsync([item]);

        await _repository.MarkAsSentAsync(item.Id);

        var next = await _repository.GetNextItemAsync(1);
        next.Should().BeNull();
    }

    [Fact]
    public async Task Creating_new_queue_finishes_previous()
    {
        var first = await _repository.CreateQueueAsync(new DigestQueue { UserId = 1 });

        var second = await _repository.CreateQueueAsync(new DigestQueue { UserId = 1 });

        first.Should().NotBeNull();
        var finished = await _context.DigestQueues.FindAsync([first.Id]);
        finished!.Status.Should().Be(DigestQueueStatus.Finished);
        second.Id.Should().BeGreaterThan(first.Id);
    }

    [Fact]
    public async Task Empty_queue_is_finished_after_last_item()
    {
        var queue = await _repository.CreateQueueAsync(new DigestQueue { UserId = 1 });
        await _repository.AddItemsAsync([new DigestItem { DigestQueueId = queue.Id, OrderIndex = 0, MessageUrl = "a" }]);

        await _repository.MarkAsSentAsync((await _repository.GetNextItemAsync(1))!.Id);
        var afterEmpty = await _repository.GetNextItemAsync(1);

        afterEmpty.Should().BeNull();
        var reloaded = await _context.DigestQueues.FindAsync([queue.Id]);
        reloaded!.Status.Should().Be(DigestQueueStatus.Finished);
    }

    [Fact]
    public async Task Count_unsent_returns_remaining()
    {
        var queue = await _repository.CreateQueueAsync(new DigestQueue { UserId = 1 });
        await _repository.AddItemsAsync(
        [
            new DigestItem { DigestQueueId = queue.Id, OrderIndex = 0, MessageUrl = "a" },
            new DigestItem { DigestQueueId = queue.Id, OrderIndex = 1, MessageUrl = "b" },
        ]);

        (await _repository.CountUnsentAsync(1)).Should().Be(2);
        await _repository.MarkAsSentAsync((await _repository.GetNextItemAsync(1))!.Id);
        (await _repository.CountUnsentAsync(1)).Should().Be(1);
    }

    public void Dispose() => _context.Dispose();
}
namespace NewsDigest.Core.Models;

public sealed record DigestSelection(int Id, string Reason);

public sealed record DigestResult(
    IReadOnlyList<DigestSelection> Selected,
    IReadOnlyList<int> Ads,
    IReadOnlyList<int> Duplicates);
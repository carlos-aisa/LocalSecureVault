namespace Vault.Application.Import;

public sealed record ImportOptions(
    bool SkipDuplicates = true,
    bool TagIncompleteAsNeedsReview = true
);

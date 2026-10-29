using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Drafting;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class DraftingReason {
    public static readonly DraftingReason Stated = new("rejected input");
    public static readonly DraftingReason ScaleRatio = new("no finite positive ratio");
    public static readonly DraftingReason RevisionLetters = new("letters outside the revision alphabet or its letter count");
    public static readonly DraftingReason RevisionOrdinal = new("ordinal outside the revision alphabet");
    public static readonly DraftingReason IssueDate = new("no ISO 8601 date");
    public static readonly DraftingReason TitleMissing = new("title-block field without an entry");
    public static readonly DraftingReason TitleInvalid = new("title-block field with an invalid entry");
    public static readonly DraftingReason NameBroken = new("name outside its naming standard");
    public static readonly DraftingReason NumberTaken = new("number another sheet holds");

    public string Message { get; }
}

public sealed record DraftingFault(string Subject, DraftingReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject} at index {at}"), () => $"{Reason.Message}: {Subject}"), 0, Inner),
    IValidationError<DraftingFault> {
    public static DraftingFault Create(string message) => new(message, DraftingReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, DraftingReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new DraftingFault(string.Create(CultureInfo.InvariantCulture, $"{names} = {rejected}"), reason, index);
}

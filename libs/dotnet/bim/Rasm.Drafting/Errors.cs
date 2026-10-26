using System.Globalization;
using System.Runtime.CompilerServices;

namespace Rasm.Drafting;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class DraftingReason {
    public static readonly DraftingReason Stated = new("rule its factory states");
    public static readonly DraftingReason ScaleRatio = new("ratio not finite and positive");
    public static readonly DraftingReason RevisionLetters = new("letters outside revision alphabet or letter count");
    public static readonly DraftingReason RevisionOrdinal = new("ordinal outside revision alphabet");
    public static readonly DraftingReason IssueDate = new("no ISO 8601 date");
    public static readonly DraftingReason TitleMissing = new("title-block field without entry");
    public static readonly DraftingReason TitleInvalid = new("invalid title-block entry");
    public static readonly DraftingReason NameBroken = new("name outside naming standard");
    public static readonly DraftingReason NumberTaken = new("sheet number another sheet holds");

    public string Message { get; }
}

public sealed record DraftingFault(string Subject, DraftingReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Subject} fails: {Reason.Message} at index {at}"), () => $"{Subject} fails: {Reason.Message}"), 0, Inner), IValidationError<DraftingFault> {
    public static DraftingFault Create(string message) => new(message, DraftingReason.Stated);

    public static Fin<Unit> Unless(bool flag, DraftingReason reason, Option<int> index = default, [CallerArgumentExpression(nameof(flag))] string subject = "") => flag ? unit : new DraftingFault(subject, reason, index);
}

using System.Globalization;

namespace Rasm.Drafting;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class DraftingReason {
    public static readonly DraftingReason Stated = new("{0}");
    public static readonly DraftingReason ScaleRatio = new("{0} is no finite positive ratio");
    public static readonly DraftingReason RevisionLetters = new("{0} lie outside the revision alphabet or its letter count");
    public static readonly DraftingReason RevisionOrdinal = new("{0} lies outside its revision alphabet");
    public static readonly DraftingReason IssueDate = new("{0} is no ISO 8601 date");
    public static readonly DraftingReason TitleMissing = new("title-block field {0} has no entry");
    public static readonly DraftingReason TitleInvalid = new("title-block field {0} has an invalid entry");
    public static readonly DraftingReason NameBroken = new("{0} breaks its naming standard");
    public static readonly DraftingReason NumberTaken = new("{0} takes the number of another sheet");

    public string Template { get; }
}

public sealed record DraftingFault(string Subject, DraftingReason Reason, Option<Error> Inner = default)
    : Expected(string.Format(CultureInfo.InvariantCulture, Reason.Template, Subject), 0, Inner), IValidationError<DraftingFault> {
    public static DraftingFault Create(string message) => new(message, DraftingReason.Stated);
}

namespace Rasm.Drafting;

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class DraftingReason : IFaultReason {
    public static readonly DraftingReason ScaleRatio = new("no finite positive ratio");
    public static readonly DraftingReason RevisionLetters = new("letters outside the revision alphabet or its letter count");
    public static readonly DraftingReason RevisionOrdinal = new("ordinal outside the revision alphabet");
    public static readonly DraftingReason IssueDate = new("no ISO 8601 date");
    public static readonly DraftingReason TitleMissing = new("empty title-block field");
    public static readonly DraftingReason TitleInvalid = new("invalid title-block field");
    public static readonly DraftingReason TitleUnknown = new("entry for a field outside the title block");
    public static readonly DraftingReason NameBroken = new("name outside its naming standard");
    public static readonly DraftingReason NumberTaken = new("duplicate sheet number");
    public static readonly DraftingReason Blank = new("blank name");
    public static readonly DraftingReason Measure = new("length negative or not finite");
    public static readonly DraftingReason Repeated = new("empty or repeated entries");

    public string Message { get; }
}

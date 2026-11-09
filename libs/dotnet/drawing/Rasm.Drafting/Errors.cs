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

public sealed record DraftingFault(object? Subject, DraftingReason Reason, Option<int> Index = default, Option<Error> Inner = default, Option<string> Expression = default)
    : Expected(Reason.Message, 0, Inner),
    IValidationError<DraftingFault> {
    public object? Subject { get; } = Subject;
    public DraftingReason Reason { get; } = Reason;
    public Option<string> Expression { get; } = Expression;

    public override string Message =>
        string.Concat([
            Reason.Message, ": ", .. Expression.Map(static named => $"{named} = ").ToSeq(), string.Create(CultureInfo.InvariantCulture, $"{Subject}"),
            .. Index.Map(static at => string.Create(CultureInfo.InvariantCulture, $" at index {at}")).ToSeq(),
        ]);

    public static DraftingFault Create(string message) => new(message, DraftingReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, DraftingReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string? names = null) =>
        flag ? unit : new DraftingFault(rejected, reason, index, Expression: Optional(names));
}

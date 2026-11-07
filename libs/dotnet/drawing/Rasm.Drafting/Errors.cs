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
    public static readonly DraftingReason NameBroken = new("name outside its naming standard");
    public static readonly DraftingReason NumberTaken = new("duplicate sheet number");

    public string Message { get; }
}

public sealed record DraftingFault(object? Subject, DraftingReason Reason, Option<int> Index = default, Option<Error> Inner = default, string Expression = "")
    : Expected(Reason.Message, 0, Inner),
    IValidationError<DraftingFault> {
    public object? Subject { get; } = Subject;
    public DraftingReason Reason { get; } = Reason;
    public string Expression { get; } = Expression;

    public override string Message {
        get {
            string subject = Expression.Length is 0 ? string.Create(CultureInfo.InvariantCulture, $"{Subject}") : string.Create(CultureInfo.InvariantCulture, $"{Expression} = {Subject}");
            return Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {subject} at index {at}"), () => $"{Reason.Message}: {subject}");
        }
    }

    public static DraftingFault Create(string message) => new(message, DraftingReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, DraftingReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new DraftingFault(rejected, reason, index, Expression: names);
}

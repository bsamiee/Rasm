using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Rhino.Commands;

namespace Rasm.RhinoCommon;

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class RhinoReason {
    public static readonly RhinoReason Stated = new("{0}");
    public static readonly RhinoReason Declined = new("{0} returned failure");
    public static readonly RhinoReason Absent = new("{0} returned nothing");
    public static readonly RhinoReason Invalid = new("{0} returned an invalid value");
    public static readonly RhinoReason Outside = new("{0} is outside its range");
    public static readonly RhinoReason Repeated = new("{0} repeats");
    public static readonly RhinoReason Missing = new("{0} matches nothing");
    public static readonly RhinoReason Mismatched = new("{0} holds another type");
    public static readonly RhinoReason Disabled = new("{0} is disabled");
    public static readonly RhinoReason Loaded = new("{0} is loaded");
    public static readonly RhinoReason Unheld = new("host cannot hold {0}");
    public static readonly RhinoReason Raised = new("script raised {0}");
    public static readonly RhinoReason Nothing = new("{0} ended with nothing") { Ending = Some(Result.Nothing) };
    public static readonly RhinoReason Exited = new("{0} ended with Rhino exiting") { Ending = Some(Result.ExitRhino) };
    public static readonly RhinoReason Unknown = new("{0} named an unknown command") { Ending = Some(Result.UnknownCommand) };
    public static readonly RhinoReason Taken = new("{0} is taken");
    public static readonly RhinoReason Unwritable = new("{0} is unwritable");
    public static readonly RhinoReason Unreadable = new("{0} is unreadable");
    public static readonly RhinoReason Unqualified = new("{0} is unqualified");

    public string Template { get; }

    [IgnoreMember]
    public Option<Result> Ending { get; private init; }
}

public sealed record RhinoFault(string Subject, RhinoReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(
        Index.Match(
            at => string.Create(CultureInfo.InvariantCulture, $"{string.Format(CultureInfo.InvariantCulture, Reason.Template, Subject)} at index {at}"),
            () => string.Format(CultureInfo.InvariantCulture, Reason.Template, Subject)),
        ErrorCodes.ValidationFailed,
        Inner),
    IValidationError<RhinoFault> {
    public static RhinoFault Create(string message) => new(message, RhinoReason.Stated);

    public static Fin<Unit> Unless(bool accepted, Option<int> index = default, [CallerArgumentExpression(nameof(accepted))] string call = "") =>
        accepted ? unit : new RhinoFault(call, RhinoReason.Declined, index);

    public static Fin<T> Found<T>(T? value, Option<int> index = default, [CallerArgumentExpression(nameof(value))] string call = "") where T : class =>
        value is { } held ? held : new RhinoFault(call, RhinoReason.Absent, index);

    public static Fin<T> Found<T>(T? value, Option<int> index = default, [CallerArgumentExpression(nameof(value))] string call = "") where T : struct =>
        value is { } held ? held : new RhinoFault(call, RhinoReason.Absent, index);

    public static Fin<T> Found<T>(Option<T> value, Option<int> index = default, [CallerArgumentExpression(nameof(value))] string call = "") =>
        value.ToFin(new RhinoFault(call, RhinoReason.Absent, index));

    public static Fin<Unit> Unreported(string? report, [CallerArgumentExpression(nameof(report))] string call = "") =>
        string.IsNullOrEmpty(report) ? unit : new RhinoFault(call, RhinoReason.Declined, Inner: Error.New(report));

    public static Fin<Seq<TRow>> Unique<TRow, TKey>(Seq<TRow> rows, Func<TRow, TKey> key, IEqualityComparer<TKey>? comparer = null, [CallerArgumentExpression(nameof(rows))] string call = "")
        where TKey : notnull =>
        toSeq(rows.CountBy(key, comparer))
            .Filter(static counted => counted.Value > 1)
            .Traverse(counted => Fin.Fail<Unit>(new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{counted.Key} in {call}"), RhinoReason.Repeated)))
            .As()
            .Map(_ => rows);
}

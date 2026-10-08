using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInSink {
    public void Report(Error error, Type owner, string member);

    public static IPlugInSink Of(object constructed) => (IPlugInSink)PlugIn.Find(constructed.GetType().Assembly);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RowText {
    public static CultureInfo Culture => CultureInfo.GetCultureInfo(Localization.CurrentLanguageId);

    public static LocalizeStringPair Library(string english, params ReadOnlySpan<object?> arguments) {
        LocalizeStringPair pair = Localization.LocalizeCommandOptionName(english, typeof(RowText).Assembly, 0);
        return arguments.IsEmpty ? pair : new(string.Format(Culture, pair.English, arguments), string.Format(Culture, pair.Local, arguments));
    }
}

public static partial class ErrorOps {
    public static string Localize(Error error, bool includeInner = true) =>
        string.Join(Environment.NewLine, Causes(error, includeInner).Map(static leaf =>
            leaf is Expected expected
                ? Token.Replace(
                    Localization.LocalizeString(expected.Message, expected, expected.Code),
                    match => Optional(expected.GetType().GetProperty(match.Groups["name"].Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        .Map(property => string.Create(RowText.Culture, $"{property.GetValue(expected)}"))
                        .IfNone(match.Value))
                : leaf.Message));

    public static IO<Unit> Report(Error error, Type owner, string member) =>
        from causes in IO.lift(() => Causes(error))
        from _ in IO.lift(() => causes.Iter(cause => RhinoApp.WriteLine($"{owner.Name}.{member}: {Localize(cause, includeInner: false)}")))
        from __ in IO.lift(() => causes.Choose(static cause => cause.Exception).Iter(exception => HostUtils.ExceptionReport($"{owner.FullName}.{member}", exception)))
        select unit;

    private static Seq<Error> Causes(Error error, bool includeInner = true) =>
        error.FoldM<Seq, Error>(leaf => leaf.Cons(includeInner ? leaf.Inner.ToSeq().Bind(static inner => Causes(inner)) : [])).As();

    [GeneratedRegex(@"\{(?<name>\w+)\}", RegexOptions.ExplicitCapture, Timeout.Infinite)]
    private static partial Regex Token { get; }
}

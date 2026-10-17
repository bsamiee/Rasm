using System.Globalization;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInSink {
    public void Report(Error error, Type owner, string member);

    public static IPlugInSink Of(object constructed) => constructed as IPlugInSink ?? (IPlugInSink)PlugIn.Find(constructed.GetType().Assembly);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RowText {
    public static CultureInfo Culture => CultureInfo.GetCultureInfo(Localization.CurrentLanguageId);

    public static LocalizeStringPair Localize(string english, params ReadOnlySpan<object?> arguments) =>
        Localize(english, typeof(RowText).Assembly, arguments);

    public static LocalizeStringPair Localize(string english, object table, params ReadOnlySpan<object?> arguments) {
        LocalizeStringPair pair = Localization.LocalizeCommandOptionName(english, table, 0);
        return arguments.IsEmpty ? pair : new(string.Format(Culture, pair.English, arguments), string.Format(Culture, pair.Local, arguments));
    }
}

public static class ErrorOps {
    public static string Localize(Error error) => string.Join(Environment.NewLine, Causes(error).Map(static cause => cause.Message));

    public static IO<Unit> Report(Error error, Type owner, string member) =>
        (from causes in IO.lift(() => Causes(error))
         from _ in IO.lift(() => causes.Iter(cause => RhinoApp.WriteLine($"{owner.Name}.{member}: {cause.Message}")))
         from __ in IO.lift(() => causes.Choose(static cause => cause.Exception).Iter(exception => HostUtils.ExceptionReport($"{owner.FullName}.{member}", exception)))
         select unit)
        .Catch(fault => IO.lift(() => HostUtils.ExceptionReport($"{owner.FullName}.{member}", fault.ToException()))).As();

    private static Seq<Error> Causes(Error error) =>
        error.FoldM(static leaf => leaf.Cons(leaf.Inner.ToSeq().Bind(Causes))).As();
}

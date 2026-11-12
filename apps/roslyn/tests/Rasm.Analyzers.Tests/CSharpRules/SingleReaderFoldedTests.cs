using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class SingleReaderFoldedTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {
    [Fact]
    public Task CustomConversions() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using LanguageExt.Common;

        internal static class CustomConversions {
            internal static Fin<int> Preserve(Fin<int> value) => ToFin(ToValidation(value));

            internal static Validation<Error, int> {|#0:ToValidation|}(Fin<int> value) => value.Map(static held => held + 1).ToValidation();

            internal static Fin<int> ToFin(Validation<Error, int> value) => value.ToFin().Map(static held => held * 2);
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("CustomConversions.ToValidation", "CustomConversions.Preserve").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task FailureChecks() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using LanguageExt.Common;
        using LanguageExt.Traits;

        internal static class FailureChecks {
            internal static Func<Error, bool> {|#0:Predicate|}() => static _ => false;
            internal static Func<Error, K<IO, int>> {|#1:Recovery|}() => static _ => IO.pure(0);
            internal static IO<int> Failed(Error failure) => IO.fail<int>(failure).Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Lifted(Fin<int> result) => IO.lift(result).Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Either(Either<Error, int> result) => IO.lift(result).Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Mapped(Fin<int> result) => IO.lift(result).Map(static value => value + 1).Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Named(Error failure) => IO.fail<int>(failure).Catch(Fail: static _ => IO.pure(0), Predicate: static _ => false);
            internal static IO<int> Chained(Error failure) => IO.fail<int>(failure).Catch(static _ => false, static _ => IO.pure(0)).Map(static value => value + 1);
            internal static IO<int> Branch(bool failed, Error failure) => (failed ? IO.fail<int>(failure) : IO.pure(1)).Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Switched(bool failed, Error failure) => (failed switch { true => IO.fail<int>(failure), false => IO.pure(1) }).Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Unknown(IO<int> supplied) => supplied.Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Ninth(int one, int two, int three, int four, int five, int six, int seven, int eight, K<IO, int> supplied) => supplied.As().Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> Extended(this K<IO, int> supplied, int ignored) => supplied.As().Catch(static _ => false, static _ => IO.pure(0));
            internal static IO<int> MethodGroup(IO<int> supplied, Func<Error, bool> predicate, Func<Error, K<IO, int>> recovery) => supplied.Catch(predicate, recovery);
            internal static IO<int> EvaluatedArguments(Error failure) => IO.fail<int>(failure).Catch(Fail: Recovery(), Predicate: Predicate());
            internal static IO<int> Mutated(Error failure) {
                IO<int> supplied = IO.pure(1);
                supplied = IO.fail<int>(failure);
                return supplied.Catch(static _ => false, static _ => IO.pure(0));
            }
            internal static IO<int> Nested(Error failure) => IO.fail<int>(failure).Catch(static _ => false, static error => IO.fail<int>(error).Catch(static _ => false, static _ => IO.pure(0)));
            internal static IO<int> Called(Error failure) => Ninth(0, 0, 0, 0, 0, 0, 0, 0, IO.fail<int>(failure));
            internal static IO<int> Reordered(Error failure) => Ninth(supplied: IO.fail<int>(failure), eight: 0, seven: 0, six: 0, five: 0, four: 0, three: 0, two: 0, one: 0);
            internal static IO<int> Reduced(Error failure) => IO.fail<int>(failure).Extended(0);
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("FailureChecks.Predicate", "FailureChecks.EvaluatedArguments").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(1).WithArguments("FailureChecks.Recovery", "FailureChecks.EvaluatedArguments").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task Channels() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum<string>]
        internal sealed partial class Channel {
            public static readonly Channel Red = new("red");
            public static readonly Channel Green = new("green");
        }

        [SmartEnum<string>]
        internal sealed partial class Parsed {
            public static readonly Parsed First = new("first");
            public static readonly Parsed Second = new("second");
        }

        internal static class Channels {
            internal static Channel {|#0:Get|}(this Channel channel) => channel;

            internal static Channel Checked(Channel channel) => channel.Get();

            internal static Parsed Read() => Parsed.Parse("first", null);
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("Channels.Get", "Channels.Checked").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task ReportedTwice() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using LanguageExt;
        using LanguageExt.Common;
        using Microsoft.Extensions.Logging;
        using Rhino.Runtime;
        using static LanguageExt.Prelude;

        internal sealed class Sink(ILogger logger) {
            internal Fin<Unit> Report(Error error) {
                Failed(logger, error.Message);
                return unit;
            }

            [LoggerMessage(Level = LogLevel.Error, Message = "{Failure}")]
            private static void Failed(ILogger logger, string failure) { }
        }

        internal static class ReportedTwice {
            internal static Unit Reported(Sink sink, Error error) => sink.Report(error).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Unit Forwarded(Sink sink, Error error) => Forward(sink, error).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> {|#0:Forward|}(Sink sink, Error error) => Relay(sink, error);

            internal static Fin<Unit> {|#1:Relay|}(Sink sink, Error error) => sink.Report(error);

            internal static Unit Delegate(ILogger logger, Error error) => Invoked(logger, error).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> {|#2:Invoked|}(ILogger logger, Error error) {
                Action report = () => ReportedOnce.Failed(logger, error.Message);
                Action invoke = report;
                invoke();
                return unit;
            }

            internal static Unit Supplied(ILogger logger, Error error) => Invoke(() => ReportedOnce.Failed(logger, error.Message)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> Invoke(Action callback) {
                callback();
                return unit;
            }

            internal static Unit Lifted(ILogger logger, Error error) => IO.lift(() => {
                ReportedOnce.Failed(logger, error.Message);
                return unit;
            }).RunSafe().IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Unit ForwardedCallback(ILogger logger, Error error) => ForwardCallback(() => ReportedOnce.Failed(logger, error.Message)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            private static Fin<Unit> ForwardCallback(Action callback) => Invoke(callback);

            internal static Unit WrappedCallback(ILogger logger, Error error) => Wrap(() => ReportedOnce.Failed(logger, error.Message)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            private static Fin<Unit> Wrap(Action callback) => Invoke(() => callback());
        }

        internal static class ReportedOnce {
            private static readonly List<Action> callbacks = [];

            internal static Unit Reported(Sink sink, Error error) => sink.Report(error).IfFail(static _ => unit);

            internal static Unit Crashed(Fin<Unit> result) => result.IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Unit Recursive(Error error) => First(error).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> First(Error error) => Second(error);

            internal static Fin<Unit> {|#3:Second|}(Error error) => First(error);

            internal static Unit Enabled(ILogger logger, Error error) => Checked(logger).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> {|#4:Checked|}(ILogger logger) => logger.IsEnabled(LogLevel.Error) ? unit : Error.New("disabled");

            internal static Unit Deferred(ILogger logger, Error error) => Unused(logger, error).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> {|#5:Unused|}(ILogger logger, Error error) {
                Action callback = () => Failed(logger, error.Message);
                return unit;
            }

            internal static Unit Local(ILogger logger, Error error) => Uncalled(logger, error).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            internal static Fin<Unit> {|#6:Uncalled|}(ILogger logger, Error error) {
                void {|CS8321:Later|}() => Failed(logger, error.Message);
                return unit;
            }

            internal static Unit Ignored(ILogger logger, Error error) => Ignore(() => Failed(logger, error.Message)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            private static Fin<Unit> Ignore(Action callback) => unit;

            internal static Unit Stored(ILogger logger, Error error) => Store(() => Failed(logger, error.Message)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            private static Fin<Unit> Store(Action callback) {
                callbacks.Add(callback);
                return unit;
            }

            internal static Unit Reassigned(ILogger logger, Error error) => Replaced(() => Failed(logger, error.Message)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            private static Fin<Unit> Replaced(Action callback) {
                callback = static () => { };
                return ReportedTwice.Invoke(callback);
            }

            internal static Unit Cyclic(ILogger logger, Error error) => First(() => Failed(logger, error.Message)).IfFail(fault => HostUtils.ExceptionReport(fault.ToException()));

            private static Fin<Unit> First(Action callback) => Second(callback);

            private static Fin<Unit> Second(Action callback) => First(callback);

            [LoggerMessage(Level = LogLevel.Error, Message = "{Failure}")]
            internal static void Failed(ILogger logger, string failure) { }
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("ReportedTwice.Forward", "ReportedTwice.Forwarded").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(1).WithArguments("ReportedTwice.Relay", "ReportedTwice.Forward").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(2).WithArguments("ReportedTwice.Invoked", "ReportedTwice.Delegate").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(3).WithArguments("ReportedOnce.Second", "ReportedOnce.First").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(4).WithArguments("ReportedOnce.Checked", "ReportedOnce.Enabled").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(5).WithArguments("ReportedOnce.Unused", "ReportedOnce.Deferred").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(6).WithArguments("ReportedOnce.Uncalled", "ReportedOnce.Local").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task ChainedPage() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using Rhino.UI;

        namespace Rasm.Rhino.UI {
            public sealed class ViewBody : IDisposable {
                public Control Content { get; } = new Panel();

                public static IO<ViewBody> Open() => IO.lift(static () => new ViewBody());

                public void Dispose() => Content.Dispose();
            }
        }

        public abstract class ChainedPage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Chained";

            public override object PageControl => First;

            internal object {|#0:First|} => Next();

            internal object {|#1:Next|}() => Last;

            internal object {|#2:Last|} => new Panel();
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("ChainedPage.First", "ChainedPage.PageControl").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(1).WithArguments("ChainedPage.Next", "ChainedPage.First").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(2).WithArguments("ChainedPage.Last", "ChainedPage.Next").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task RecursivePage() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using Rhino.UI;

        namespace Rasm.Rhino.UI {
            public sealed class ViewBody : IDisposable {
                public Control Content { get; } = new Panel();

                public static IO<ViewBody> Open() => IO.lift(static () => new ViewBody());

                public void Dispose() => Content.Dispose();
            }
        }

        public abstract class RecursivePage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Recursive";

            public override object PageControl => First;

            internal object First => Second;

            internal object {|#0:Second|} => First;
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("RecursivePage.Second", "RecursivePage.First").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task SetterPage() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using Rhino.UI;

        namespace Rasm.Rhino.UI {
            public sealed class ViewBody : IDisposable {
                public Control Content { get; } = new Panel();

                public static IO<ViewBody> Open() => IO.lift(static () => new ViewBody());

                public void Dispose() => Content.Dispose();
            }
        }

        public abstract class SetterPage : ObjectPropertiesPage {
            private object? body;

            public override string EnglishPageTitle => "Setter";

            public override object? PageControl => Body;

            internal object? {|#0:Body|} {
                get => body;
                set => body = new Panel();
            }
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("SetterPage.Body", "SetterPage.PageControl").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task Factor() => VerifyAnalyzerAsync(
        """
        internal sealed record Factor(float Value) {
            public static Factor Unit { get; } = new(1f);

            public static Factor Create(float value) => new(value);

            public static Factor {|#0:Increased|}(float value) => new(value + 1f);
        }

        internal sealed record Grow(Factor Factor) {
            public static readonly Grow Fill = new(Factor.Unit);
        }

        internal static class Sizes {
            internal static Grow Whole => new Grow(Factor.Create(1f));

            internal static Factor One => Factor.Create(1f);

            internal static Factor Half => Factor.Create(0.5f);

            internal static Grow Held => Grow.Fill;

            internal static Factor Increased => Factor.Increased(1f);

            internal static Factor Converted => new Factor(1);
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("Factor.Increased", "Sizes.Increased").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task Overloaded() => VerifyAnalyzerAsync(
        """
        internal sealed record Overloaded {
            internal Overloaded(int value) => Value = value;

            internal Overloaded(object value) => Value = value;

            internal object Value { get; }

            internal static Overloaded One { get; } = new(1);

            internal static Overloaded {|#0:Create|}(int value) => new((object)value);
        }

        internal static class DistinctOverloads {
            internal static Overloaded Boxed => new((object)1);

            internal static Overloaded Created => Overloaded.Create(1);
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("Overloaded.Create", "DistinctOverloads.Created").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task InitializedFactor() => VerifyAnalyzerAsync(
        """
        internal sealed class MutableFactor(int value) {
            internal static readonly MutableFactor Unit = new(1);

            public int Value { get; set; } = value;
        }

        internal sealed record CountedFactor {
            private static int count;

            internal static readonly CountedFactor Unit = new(1);

            internal CountedFactor(int value) {
                Value = value;
                ++count;
            }

            internal int Value { get; }
        }

        internal sealed record InitializedFactor(int Value) {
            private static int count;

            internal static readonly InitializedFactor Unit = new(1);

            internal int Ordinal { get; } = Next();

            internal static int {|#0:Next|}() => ++count;
        }

        internal readonly struct ReadonlyFactor {
            internal static readonly ReadonlyFactor Unit = new(1);

            internal ReadonlyFactor(int value) => Value = value;

            internal int Value { get; }

            internal static ReadonlyFactor {|#1:Create|}(int value) => new(value);
        }

        internal record BaseFactor(int Value);

        internal sealed record DerivedFactor(int Input) : BaseFactor(Count(Input)) {
            private static int calls;

            internal static readonly DerivedFactor Unit = new(1);

            private static int Count(int value) {
                ++calls;
                return value;
            }
        }

        internal static class ConstructionEffects {
            internal static MutableFactor Mutable => new(1);

            internal static CountedFactor Counted => new(1);

            internal static InitializedFactor Initialized => new(1);

            internal static DerivedFactor Derived => new(1);

            internal static ReadonlyFactor Readonly => new(1);

            internal static ReadonlyFactor Factory => ReadonlyFactor.Create(1);
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("InitializedFactor.Next", "InitializedFactor.Ordinal").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        Diagnostic("RASM3015").WithLocation(1).WithArguments("ReadonlyFactor.Create", "ConstructionEffects.Factory").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));

    [Fact]
    public Task DisplayType() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Reflection;
        using LanguageExt.Common;
        using Alias = System.Type;

        internal sealed record Label(string? Text);

        internal readonly record struct DisplayText(string Text) {
            public static implicit operator DisplayText(string text) => new("display");
        }

        internal sealed record DisplayedLabel(DisplayText Text);

        internal static class Labels {
            internal static Label Direct(Alias type) => new(type.Name);
            internal static Label Qualified(Alias type) => new(type.AssemblyQualifiedName);
            internal static Label Transformed(Alias type) => new(type.FullName?.Trim());
            internal static Label Generic<T>() => new(nameof(T));
            internal static Label Interpolated(Alias type) => new($"name: {type.Name}");
            internal static Label Replaced(Label label, Alias type) => label with { Text = type.Name };
            internal static Label Initialized(Alias type) => new(null) { Text = type.Name };
            internal static Label Relayed(Alias type) => Build(1, 2, 3, 4, 5, 6, 7, 8, type.Name);
            internal static Label Named(Alias type) => Build(text: type.Name, first: 1, second: 2, third: 3, fourth: 4, fifth: 5, sixth: 6, seventh: 7, eighth: 8);
            internal static Label Extension(Alias type) => type.Name.Label();
            internal static Label Changed(Alias type) => Build(type.Name);
            internal static Label Display(Alias type) => new(type.Name.Length.ToString());
            internal static string Render(Alias type) => type.Name.Trim();
            internal static Label MethodName(MethodInfo method) => new(method.Name);
            internal static Exception Thrown(Alias type) => new InvalidOperationException(type.Name);
            internal static Error Failure(Alias type) => new Expected(type.Name, 0, default);
            internal static Grasshopper2.UI.Nomen DisplayName(Alias type) => new(type.Name);
            internal static Label Unstored(Alias type) => Ignore(type.Name);
            internal static Label Cyclic(Alias type) => Cycle(type.Name);
            internal static Label AliasValue(Alias type) {
                string name = type.Name;
                return new(name.Length > 0 ? name : "display");
            }
            internal static Label Reassigned(Alias type) {
                string name = type.Name;
                name = "display";
                return new(name.Length > 0 ? name : "display");
            }
            internal static Label StoredBefore(Alias type) {
                string name = type.Name;
                Label held = new(name);
                name = "display";
                return held;
            }
            internal static Label Disjoint(Alias type, bool replaced) {
                string name = type.Name;
                if (replaced) {
                    name = "display";
                    return new(name);
                }
                return new(name);
            }
            internal static Label SourceFactory(Alias type) => LanguageExt.SourceFactory.Fail(type.Name);
            internal static Label Hidden(DisplayType type) => new(type.ToString());
            internal static Label Overridden(RenderedType type) => new(type.ToString());
            internal static DisplayedLabel Converted(Alias type) => new(type.Name);
            internal static Label Finalized(Alias type, bool returned) {
                string held = "display";
                string selected;
                try {
                    if (returned) {
                        held = type.Name;
                        return new("display");
                    }
                }
                finally {
                    selected = held;
                }
                return new(selected);
            }
            internal static void OverwrittenFinally<T>(bool stop) {
                string name = typeof(T).Name;
                string next = "display";
                try {
                    if (stop) return;
                    name = "display";
                }
                finally {
                    string alias = name;
                    next = alias;
                }
                _ = new Label(next);
            }
            internal static Label StoredFinally<T>() {
                string name = typeof(T).Name;
                string next;
                try { }
                finally {
                    string alias = name;
                    next = alias;
                }
                return new(next);
            }
            private static Label Build(int first, int second, int third, int fourth, int fifth, int sixth, int seventh, int eighth, string text) => new(text);
            private static Label Build(string text) => Build(1, 2, 3, 4, 5, 6, 7, 8, text);
            private static Label Ignore(string text) => new("display");
            private static Label Cycle(string text) => Cycle(text);
            internal static Label Label(this string text) => new(text);
        }

        internal sealed class DisplayType(Alias type) : TypeDelegator(type) {
            public new string {|#0:ToString|}() => "display";
        }

        internal sealed class RenderedType(Alias type) : TypeDelegator(type) {
            public override string ToString() => base.ToString();
        }

        namespace LanguageExt {
            internal static class SourceFactory {
                internal static Label Fail(string text) => new(text);
            }
        }

        namespace Grasshopper2.UI {
            internal sealed class Nomen(string text) {
                internal string Text { get; } = text;
            }
        }
        """,
        Diagnostic("RASM3015").WithLocation(0).WithArguments("DisplayType.ToString", "Labels.Hidden").WithOptions(DiagnosticOptions.IgnoreAdditionalLocations));
}

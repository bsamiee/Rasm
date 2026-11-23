using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Eto;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class NativeCallbackTests : CSharpCodeFixVerifier<NativeCallbackAnalyzer, EmptyCodeFixProvider> {

    // --- [NATIVE]

    [Fact]
    public Task ReportsNativeCallbackBodyOtherThanOneSessionCallOrConstantReturn() => new CSharpCodeFixTest<NativeCallbackAnalyzer, EmptyCodeFixProvider> {
        TestState = {
            Sources = {
                Host,
                """
                using System;
                using System.Runtime.InteropServices;
                using AppKit;
                using Foundation;
                using LanguageExt;
                using Rasm.Rhino;
                using static LanguageExt.Prelude;

                internal static class Callbacks {
                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private delegate void Frame(nint link);

                    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
                    private delegate nint HitTest(nint point);

                    private static readonly Frame Routed = static link => Session.Run(IO.pure(link));
                    private static readonly HitTest Constant = static _ => 0;
                    private static readonly Frame Direct = static link => {|#0:Ticked(link)|};
                    private static readonly Frame Continued = static link => {|#1:Ticked(link).Iter(static held => Session.Run(IO.pure(held)))|};
                    private static readonly Frame Twice = static link => {|#4:{ Session.Run(IO.pure(link)); Session.Run(IO.pure(link)); }|};

                    internal static NSObject Observed(NSNotificationCenter center, NSString name, Func<NSNotification, IO<Unit>> deliver) =>
                        center.AddObserver(name, notification => Session.Run(deliver(notification)));

                    internal static NSObject Forwarded(NSNotificationCenter center, NSString name, Action<NSObject?> changed) =>
                        center.AddObserver(name, notification => {|#2:changed(notification.Object)|});

                    [Export("frame:")]
                    private static void Exported(nint link) => {|#3:Ticked(link)|};

                    [UnmanagedCallersOnly]
                    private static nint Monitored(nint link) => Session.Run(IO.pure(link), 0);

                    private static Option<int> Ticked(nint link) => link == 0 ? None : Some(1);

                    private sealed class Clicked : NSView {
                        public override void MouseDown(NSEvent theEvent) => {|#5:Ticked(0)|};

                        public override void MouseUp(NSEvent theEvent) => Session.Run(IO.pure(theEvent));
                    }
                }
                """,
            },
            ExpectedDiagnostics = {
                Diagnostic().WithLocation(0).WithArguments("Frame", "Callbacks.Ticked"),
                Diagnostic().WithLocation(1).WithArguments("Frame", "Option.Iter"),
                Diagnostic().WithLocation(2).WithArguments("NSNotificationCenter.AddObserver", "changed"),
                Diagnostic().WithLocation(3).WithArguments("Exported", "Callbacks.Ticked"),
                Diagnostic().WithLocation(4).WithArguments("Frame", "{ Session.Run(IO.pure(link)); Session.Run(IO.pure(link)); }"),
                Diagnostic().WithLocation(5).WithArguments("MouseDown", "Callbacks.Ticked"),
            },
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [ETO]

    [Fact]
    public Task ReportsEtoOverrideReadingHostInSessionArgumentsAndIgnoresReadDeferredInsideSession() => new CSharpCodeFixTest<NativeCallbackAnalyzer, EmptyCodeFixProvider> {
        TestState = {
            Sources = {
                Host,
                """
                using System;
                using Eto.Drawing;
                using Eto.Forms;
                using LanguageExt;
                using Rasm.Rhino;
                using static LanguageExt.Prelude;

                internal sealed class Moved : Drawable {
                    private static IO<Unit> Move(SizeF size, MouseEventArgs args) => IO.pure(unit);

                    private static void Wrap(SizeF size, MouseEventArgs args) => Session.Run(Move(size, args));

                    protected override void OnMouseMove(MouseEventArgs e) => {|#0:Session.Run(Move(Size, e))|};

                    protected override void OnMouseUp(MouseEventArgs e) => {|#1:Wrap(Size, e)|};

                    protected override void OnMouseDown(MouseEventArgs e) => Session.Run(IO.lift(() => Move(Size, e)).Bind(static moved => moved));

                    protected override void OnLostFocus(EventArgs e) => Session.Run(IO.lift(Invalidate));
                }
                """,
            },
            ExpectedDiagnostics = {
                Diagnostic().WithLocation(0).WithArguments("OnMouseMove", "Size"),
                Diagnostic().WithLocation(1).WithArguments("OnMouseUp", "Size"),
            },
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [ENTRY_POINTS]

    [Fact]
    public Task IgnoresCallbackOfTheEntryPointTypeAndReportsTheSameCallbackElsewhere() => new CSharpCodeFixTest<NativeCallbackAnalyzer, EmptyCodeFixProvider> {
        TestState = {
            OutputKind = OutputKind.ConsoleApplication,
            Sources = {
                Host,
                """
                using System.Runtime.InteropServices;

                internal static class Program {
                    private static int started;

                    private static void Main() { }

                    [UnmanagedCallersOnly]
                    private static void Started() => started = 1 - started;
                }

                internal static class Elsewhere {
                    private static int started;

                    [UnmanagedCallersOnly]
                    private static void Started() => {|#0:started = 1 - started|};
                }
                """,
            },
            ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("Started", "started = 1 - started") },
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task ReportsCallbackOfAMainDeclaringTypeInALibrary() => new CSharpCodeFixTest<NativeCallbackAnalyzer, EmptyCodeFixProvider> {
        TestState = {
            Sources = {
                Host,
                """
                using System.Runtime.InteropServices;

                internal static class Program {
                    private static int started;

                    private static void Main() { }

                    [UnmanagedCallersOnly]
                    private static void Started() => {|#0:started = 1 - started|};
                }
                """,
            },
            ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("Started", "started = 1 - started") },
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [THREADS]

    [Fact]
    public Task ReportsDocumentReadInsideScheduledWorkAndIgnoresReadBeforeSchedulingOrInOrdinaryLambda() => new CSharpCodeFixTest<OffUiThreadAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using System.Linq;
            using System.Threading.Tasks;
            using Rhino;

            namespace Rasm.Rhino {
                public static class Session;
            }

            internal static class Scheduled {
                internal static Task<int> Inside(RhinoDoc doc) => Task.Run(() => {|#0:doc|}.Objects.Count);

                internal static Task<int> Before(RhinoDoc doc) {
                    int count = doc.Objects.Count;
                    return Task.Run(() => count + 1);
                }

                internal static int Ordinary(RhinoDoc doc) => Enumerable.Range(0, 1).Select(_ => doc.Objects.Count).Sum();
            }
            """,
        ExpectedDiagnostics = { CSharpCodeFixVerifier<OffUiThreadAnalyzer, EmptyCodeFixProvider>.Diagnostic().WithLocation(0).WithArguments("doc", "Task.Run") },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [SOURCES]

    private const string Host = """
        using LanguageExt;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public static class Session {
                public static T Run<T>(IO<T> effect, T failed) => effect.RunSafe().IfFail(failed);
                public static Unit Run<T>(IO<T> effect) => effect.RunSafe().Map(static _ => unit).IfFail(unit);
            }
        }
        """;
}

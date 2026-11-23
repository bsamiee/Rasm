using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PageControlTests : CSharpCodeFixVerifier<PageControlAnalyzer, EmptyCodeFixProvider> {

    // --- [SOURCES]

    private const string Views = """
        using System;
        using System.Runtime.CompilerServices;
        using Eto.Forms;
        using LanguageExt;

        namespace Rasm.Rhino {
            public static class Products {
                public static IO<T> Of<T>(Func<T?> call, [CallerArgumentExpression(nameof(call))] string member = "") where T : class =>
                    IO.lift(() => call() ?? throw new InvalidOperationException(member));
            }
        }

        namespace Rasm.Rhino.UI {
            public sealed class ViewBody {
                public Control Content { get; } = new Panel();

                public static IO<ViewBody> Open() => IO.lift(static () => new ViewBody());

                public static IO<ViewBody> Realize() => IO.lift(static () => new ViewBody());
            }
        }
        """;

    // --- [MEMBERS]

    [Fact]
    public Task GetterReachingAControlThroughMembersOrASetterItWritesIsReportedWhileUnwrittenSettersActivationRecursionAndOtherPagesAreSilent() => VerifyAnalyzerAsync(
        """
        using Eto.Forms;
        using Rhino.UI;

        public abstract class ChainedPage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Chained";

            public override object {|#0:PageControl|} => First;

            internal object First => Next();

            internal object Next() => Last;

            internal object Last => new Panel();
        }

        public abstract class RecursivePage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Recursive";

            public override object PageControl => First;

            internal object First => Second;

            internal object Second => First;
        }

        public abstract class SetterPage : ObjectPropertiesPage {
            private object? body;

            public override string EnglishPageTitle => "Setter";

            public override object? PageControl => Body;

            internal object? Body {
                get => body;
                set => body = new Panel();
            }
        }

        public abstract class WritingPage : ObjectPropertiesPage {
            private object? body;

            public override string EnglishPageTitle => "Writing";

            public override object? {|#1:PageControl|} {
                get {
                    Body = null;
                    return body;
                }
            }

            internal object? Body {
                get => body;
                set => body = new Panel();
            }
        }

        public abstract class ActivatedPage : ObjectPropertiesPage {
            private Control? held;

            public override string EnglishPageTitle => "Activated";

            public override object? PageControl => held;

            public override bool OnActivate(bool active) {
                held ??= new Panel();
                return true;
            }
        }

        public abstract class OptionsPage() : OptionsDialogPage("Options") {
            public override object PageControl => new Panel();
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("ChainedPage", "new Panel"),
        Diagnostic().WithLocation(1).WithArguments("WritingPage", "new Panel"));

    // --- [CALLBACKS]

    [Fact]
    public Task GetterInvokingACallbackThatBuildsAControlIsReportedWhileUncalledOrStoredCallbacksAreSilent() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
        using Eto.Forms;
        using Rhino.UI;

        public abstract class LocalPage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Local";

            public override object {|#0:PageControl|} {
                get {
                    object Create() => new Panel();
                    return Create();
                }
            }
        }

        public abstract class DelegatePage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Delegate";

            public override object {|#1:PageControl|} {
                get {
                    Func<object> create = static () => new Panel();
                    Func<object> invoke = create;
                    return invoke();
                }
            }
        }

        public abstract class SuppliedPage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Supplied";

            public override object {|#2:PageControl|} => Invoke(static () => new Panel());

            private static object Invoke(Func<object> callback) => callback();
        }

        public abstract class UncalledPage : ObjectPropertiesPage {
            private readonly object body = new();

            public override string EnglishPageTitle => "Uncalled";

            public override object PageControl {
                get {
                    object {|CS8321:Unused|}() => new Panel();
                    Func<object> unused = static () => new Panel();
                    return body;
                }
            }
        }

        public abstract class StoredPage : ObjectPropertiesPage {
            private static readonly List<Func<object>> factories = [];

            public override string EnglishPageTitle => "Stored";

            public override object PageControl => Store(static () => new Panel());

            private static object Store(Func<object> callback) {
                factories.Add(callback);
                return new object();
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("LocalPage", "new Panel"),
        Diagnostic().WithLocation(1).WithArguments("DelegatePage", "new Panel"),
        Diagnostic().WithLocation(2).WithArguments("SuppliedPage", "new Panel"));

    // --- [REFERENCES]

    [Fact]
    public async Task ViewOpensAcquiresAndKnownEffectCallbacksAreReportedWhileAnOpaqueReferencedHelperIsSilent() {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<PageControlAnalyzer, EmptyCodeFixProvider> test = new() {
            TestState = {
                Sources = {
                    Views,
                    """
                    using Eto.Forms;
                    using LanguageExt;
                    using Rasm.Rhino;
                    using Rasm.Rhino.UI;
                    using Rhino.UI;

                    public abstract class OpenedPage : ObjectPropertiesPage {
                        public override string EnglishPageTitle => "Opened";

                        public override object {|#0:PageControl|} => ViewBody.Open().Run().Content;
                    }

                    public abstract class RealizedPage : ObjectPropertiesPage {
                        public override string EnglishPageTitle => "Realized";

                        public override object {|#2:PageControl|} => ViewBody.Realize().Run().Content;
                    }

                    public abstract class AcquiredPage : ObjectPropertiesPage {
                        public override string EnglishPageTitle => "Acquired";

                        public override object {|#3:PageControl|} => Products.Of<Panel>(static () => null).Run();
                    }

                    public abstract class LiftedPage : ObjectPropertiesPage {
                        public override string EnglishPageTitle => "Lifted";

                        public override object {|#1:PageControl|} => IO.lift(static () => new Panel()).Run();
                    }

                    public abstract class ForeignPage : ObjectPropertiesPage {
                        public override string EnglishPageTitle => "Foreign";

                        public override object PageControl => External.Calls.Value(static () => new Button());
                    }
                    """,
                },
                ExpectedDiagnostics = {
                    Diagnostic().WithLocation(0).WithArguments("OpenedPage", "ViewBody.Open"),
                    Diagnostic().WithLocation(1).WithArguments("LiftedPage", "new Panel"),
                    Diagnostic().WithLocation(2).WithArguments("RealizedPage", "ViewBody.Realize"),
                    Diagnostic().WithLocation(3).WithArguments("AcquiredPage", "Products.Of"),
                },
            },
        };
        test.TestState.AdditionalReferences.Add((await AdditionalMetadataReferences.CompiledAsync($"{test.TestState.Name}.Callbacks", """
            using System;

            namespace External;

            public static class Calls {
                public static object Value(Func<object> callback) => callback();
            }
            """, token)).ToMetadataReference());
        await test.RunAsync(token);
    }
}

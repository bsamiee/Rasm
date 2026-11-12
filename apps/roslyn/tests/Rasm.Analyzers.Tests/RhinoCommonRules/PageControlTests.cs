using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PageControlTests : CSharpCodeFixVerifier<PageControlAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM2010]

    [Fact]
    public Task RealizingPage() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using LanguageExt.UnsafeValueAccess;
        using Rasm.Rhino.UI;
        using Rhino.UI;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino.UI {
            public sealed class ViewBody : IDisposable {
                public Control Content { get; } = new Panel();

                public static IO<ViewBody> Open() => IO.lift(static () => new ViewBody());

                public void Dispose() => Content.Dispose();
            }
        }

        public abstract class RealizingPage : ObjectPropertiesPage {
            private readonly Atom<Option<(ViewBody Body, Control Host)>> page = Atom(Option<(ViewBody Body, Control Host)>.None);

            public override string EnglishPageTitle => "Realizing";

            public override object? {|#0:PageControl|} => Realized.Map(static object (held) => held.Host).ValueUnsafe();

            private Option<(ViewBody Body, Control Host)> Realized =>
                page.Value || (from host in IO.lift(static () => new Panel())
                               from body in ViewBody.Open()
                               from configured in IO.lift(() => host.Content = body.Content)
                               from published in page.SwapIO(_ => Some((Body: body, Host: (Control)host)))
                               select published).Run();
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("RealizingPage", "new Panel"));

    [Fact]
    public Task DirectPage() => VerifyAnalyzerAsync(
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

        public abstract class DirectPage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Direct";

            public override object {|#0:PageControl|} => new Panel();
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("DirectPage", "new Panel"));

    [Fact]
    public Task HeldPage() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using LanguageExt.UnsafeValueAccess;
        using Rasm.Rhino.UI;
        using Rhino.UI;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino.UI {
            public sealed class ViewBody : IDisposable {
                public Control Content { get; } = new Panel();

                public static IO<ViewBody> Open() => IO.lift(static () => new ViewBody());

                public void Dispose() => Content.Dispose();
            }
        }

        public abstract class HeldPage : ObjectPropertiesPage {
            private readonly Atom<Option<(ViewBody Body, Control Host)>> page = Atom(Option<(ViewBody Body, Control Host)>.None);

            public override string EnglishPageTitle => "Held";

            public override object? PageControl => page.Value.Map(static object (held) => held.Host).ValueUnsafe();

            public override bool OnActivate(bool active) => active ? Realized.IsSome : page.Value.IsSome;

            private Option<(ViewBody Body, Control Host)> Realized =>
                page.Value || (from host in IO.lift(static () => new Panel())
                               from body in ViewBody.Open()
                               from published in page.SwapIO(_ => Some((Body: body, Host: (Control)host)))
                               select published).Run();
        }
        """);

    [Fact]
    public Task OptionsPage() => VerifyAnalyzerAsync(
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

        public abstract class OptionsPage() : OptionsDialogPage("Options") {
            public override object PageControl => new Panel();
        }
        """);

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

            public override object {|#0:PageControl|} => First;

            internal object First => Next();

            internal object Next() => Last;

            internal object Last => new Panel();
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("ChainedPage", "new Panel"));

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

            internal object Second => First;
        }
        """);

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

            internal object? Body {
                get => body;
                set => body = new Panel();
            }
        }
        """);

    [Fact]
    public Task UnusedPage() => VerifyAnalyzerAsync(
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

        public abstract class UnusedPage : ObjectPropertiesPage {
            private readonly object body = new();

            public override string EnglishPageTitle => "Unused";

            public override object PageControl {
                get {
                    object {|CS8321:Unused|}() => new Panel();
                    Func<object> unusedLambda = static () => new Panel();
                    return body;
                }
            }
        }
        """);

    [Fact]
    public Task InvokedLocalPage() => VerifyAnalyzerAsync(
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

        public abstract class InvokedLocalPage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Invoked local";

            public override object {|#0:PageControl|} => First;

            private object First {
                get {
                    object Create() => new Panel();
                    return Create();
                }
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("InvokedLocalPage", "new Panel"));

    [Fact]
    public Task InvokedDelegatePage() => VerifyAnalyzerAsync(
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

        public abstract class InvokedDelegatePage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Invoked delegate";

            public override object {|#0:PageControl|} => First;

            private object First {
                get {
                    Func<object> create = static () => new Panel();
                    Func<object> invoke = create;
                    return invoke();
                }
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("InvokedDelegatePage", "new Panel"));

    [Fact]
    public Task SuppliedDelegatePage() => VerifyAnalyzerAsync(
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

        public abstract class SuppliedDelegatePage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Supplied delegate";

            public override object {|#0:PageControl|} => Invoke(static () => new Panel());

            private static object Invoke(Func<object> callback) => callback();
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("SuppliedDelegatePage", "new Panel"));

    [Fact]
    public Task IgnoredDelegatePage() => VerifyAnalyzerAsync(
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

        public abstract class IgnoredDelegatePage : ObjectPropertiesPage {
            public override string EnglishPageTitle => "Ignored delegate";

            public override object PageControl => Ignore(static () => new Panel());

            private static object Ignore(Func<object> callback) => new object();
        }
        """);

    [Fact]
    public Task StoredDelegatePage() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections.Generic;
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

        public abstract class StoredDelegatePage : ObjectPropertiesPage {
            private static readonly List<Func<object>> factories = [];

            public override string EnglishPageTitle => "Stored delegate";

            public override object PageControl => Store(static () => new Panel());

            private static object Store(Func<object> callback) {
                factories.Add(callback);
                return new object();
            }
        }
        """);

    // --- [REFERENCES]

    [Fact]
    public async Task ForeignCallbacks() {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<PageControlAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                using Eto.Forms;
                using Rhino.UI;

                public abstract class ForeignPage : ObjectPropertiesPage {
                    public override string EnglishPageTitle => "Foreign";

                    public override object PageControl => External.Calls.Value(static () => new Button());
                }
                """,
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

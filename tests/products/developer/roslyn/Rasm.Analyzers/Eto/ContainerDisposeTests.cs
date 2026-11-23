using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Eto;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ContainerDisposeTests : CSharpCodeFixVerifier<ContainerDisposeAnalyzer, EmptyCodeFixProvider> {

    // --- [TIMELINE]

    [Fact]
    public Task ReportsContainerAcquiredAfterItsChildAndIgnoresChildRealizedAfterTheContainer() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class Timeline {
            internal static IO<Control> ChildFirst(Func<IO<Control>> realize) =>
                from body in realize()
                from split in {|#0:use(static () => new Splitter())|}
                from set in IO.lift(() => { split.Panel1 = body; })
                select (Control)split;

            internal static IO<Control> ChildAfter(Func<IO<Control>> realize) =>
                from split in use(static () => new Splitter())
                from body in realize()
                from set in IO.lift(() => { split.Panel1 = body; })
                select (Control)split;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Prelude.use", "Splitter"));

    [Fact]
    public Task ReportsStackLayoutWithChildrenWhateverTheirOrderAndIgnoresChildlessStackLayout() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal static class Stacked {
            internal static IO<Control> ChildAfter(Func<IO<Control>> realize) =>
                from stack in {|#0:use(static () => new StackLayout())|}
                from body in realize()
                from set in IO.lift(() => { stack.Items.Add(new StackLayoutItem(body, true)); })
                select (Control)stack;

            internal static IO<Control> Childless() =>
                from stack in use(static () => new StackLayout())
                from set in IO.lift(() => { stack.Spacing = 4; })
                select (Control)stack;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Prelude.use", "StackLayout"));

    // --- [ACQUIRES]

    [Fact]
    public Task ReportsEveryPlainAcquireHoldingAnEarlierChildAndIgnoresDetachingReleaseAndChildlessContainer() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Forms;
        using LanguageExt;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public static class Products {
                public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to) where T : class, IDisposable => IO.lift(() => make()!).Bind(to);
            }
        }

        internal static class Acquires {
            internal static IO<int> Transferred(Control body) =>
                {|#0:Products.Transfer(static () => new Panel(), panel => IO.lift(() => { panel.Content = body; return panel.Width; }))|};

            internal static Control Retained(Action<IDisposable> retain, Control body) {
                Scrollable scroll = new() { Content = body };
                {|#1:retain(scroll)|};
                return scroll;
            }

            internal static IO<int> Detached(Control body) =>
                use(static () => new Panel(), static panel => IO.lift(() => { panel.Content = null; panel.Dispose(); }))
                    .Map(panel => { panel.Content = body; return panel.Width; });

            internal static void Childless(Action<IDisposable> retain) => retain(new Panel { Width = 10 });
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Products.Transfer", "Panel"),
        Diagnostic().WithLocation(1).WithArguments("retain", "Scrollable"));

    // --- [HOLDINGS]

    [Fact]
    public Task ReportsChildHeldThroughGenericCollectionsAndIgnoresRecursiveTypeHoldingNoControl() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;
        using Eto.Forms;
        using LanguageExt;
        using static LanguageExt.Prelude;

        internal sealed class Slot<T> {
            internal List<T> Items { get; } = [];
        }

        internal abstract class BaseSlot<T> {
            internal List<T> Items { get; } = [];
        }

        internal sealed class InheritedSlot<T> : BaseSlot<Slot<T>>;

        internal sealed class Node<T> {
            internal List<Node<List<T>>> Next { get; } = [];
        }

        internal sealed class InheritedFrame : Panel {
            internal List<InheritedSlot<Control>> Rows { get; } = [];
        }

        internal sealed class ExpandingFrame : Panel {
            internal List<Node<int>> Rows { get; } = [];
        }

        internal static class Holdings {
            internal static IO<Control> Inherited(Control body) =>
                from frame in {|#0:use(static () => new InheritedFrame())|}
                from added in IO.lift(() => { frame.Rows.Add(new InheritedSlot<Control> { Items = { new Slot<Control> { Items = { body } } } }); })
                select (Control)frame;

            internal static IO<Control> Expanding(Node<int> row) =>
                from frame in use(static () => new ExpandingFrame())
                from added in IO.lift(() => { frame.Rows.Add(row); })
                select (Control)frame;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Prelude.use", "InheritedFrame"));
}

using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Construction;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TypeNameTextTests : CSharpCodeFixVerifier<TypeNameTextAnalyzer, EmptyCodeFixProvider> {

    // --- [STORAGE]

    [Fact]
    public Task ReportsTypeNameStoredAsDataButNotTextRenderedForAPerson() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Reflection;
        using LanguageExt.Common;

        internal sealed record Label(string? Text);

        internal readonly record struct DisplayText(string Text) {
            public static implicit operator DisplayText(string text) => new("display");
        }

        internal sealed record DisplayedLabel(DisplayText Text);

        internal sealed class Caption(string text) {
            internal string Text { get; } = text;

            public static implicit operator Caption(string text) => new(text);
        }

        internal sealed class Tag(string text) {
            internal string Text { get; } = text;
        }

        internal static class Labels {
            internal static Label Direct(Type type) => new({|#0:type.Name|});
            internal static Label Generic<T>() => new({|#1:nameof(T)|});
            internal static Label Named() => new(nameof(Labels));
            internal static Label Interpolated(Type type) => new($"name: {{|#2:type.FullName|}}");
            internal static Tag Tagged(Type type) => new({|#3:type.Name|});
            internal static Label Overridden(RenderedType type) => new({|#4:type.ToString()|});
            internal static Label Hidden(DisplayType type) => new(type.ToString());
            internal static Label MethodName(MethodInfo method) => new(method.Name);
            internal static Label Counted(Type type) => new(type.Name.Length.ToString());
            internal static Exception Thrown(Type type) => new InvalidOperationException(type.Name);
            internal static Error Failure(Type type) => new Expected(type.Name, 0, default);
            internal static Caption Captioned(Type type) => new(type.Name);
            internal static DisplayedLabel Converted(Type type) => new(type.Name);
        }

        internal sealed class DisplayType(Type type) : TypeDelegator(type) {
            public new string ToString() => "display";
        }

        internal sealed class RenderedType(Type type) : TypeDelegator(type) {
            public override string ToString() => base.ToString();
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("type.Name", "Label"),
        Diagnostic().WithLocation(1).WithArguments("nameof(T)", "Label"),
        Diagnostic().WithLocation(2).WithArguments("type.FullName", "Label"),
        Diagnostic().WithLocation(3).WithArguments("type.Name", "Tag"),
        Diagnostic().WithLocation(4).WithArguments("type.ToString()", "Label"));

    // --- [FLOW]

    [Fact]
    public Task ReportsTypeNameWhoseValueReachesStorageButNotOneOverwrittenFirst() => VerifyAnalyzerAsync(
        """
        using System;

        internal sealed record Label(string Text);

        internal static class Flows {
            internal static Label Kept(Type type) {
                string name = {|#0:type.Name|};
                return new(name.Length > 0 ? name : "display");
            }
            internal static Label Reassigned(Type type) {
                string name = type.Name;
                name = "display";
                return new(name);
            }
            internal static Label StoredBefore(Type type) {
                string name = {|#1:type.Name|};
                Label held = new(name);
                name = "display";
                return held;
            }
            internal static Label Disjoint(Type type, bool replaced) {
                string name = {|#2:type.Name|};
                if (replaced) {
                    name = "display";
                    return new(name);
                }
                return new(name);
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
                string name = {|#3:typeof(T).Name|};
                string next;
                try { }
                finally {
                    string alias = name;
                    next = alias;
                }
                return new(next);
            }
            internal static Label Relayed(Type type) => Build({|#4:type.Name|});
            internal static Label Ignored(Type type) => Ignore(type.Name);
            internal static Label Cyclic(Type type) => Cycle(type.Name);
            private static Label Build(string text) => new(text);
            private static Label Ignore(string text) => new("display");
            private static Label Cycle(string text) => Cycle(text);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("type.Name", "Label"),
        Diagnostic().WithLocation(1).WithArguments("type.Name", "Label"),
        Diagnostic().WithLocation(2).WithArguments("type.Name", "Label"),
        Diagnostic().WithLocation(3).WithArguments("typeof(T).Name", "Label"),
        Diagnostic().WithLocation(4).WithArguments("type.Name", "Label"));

    // --- [FILES]

    [Fact]
    public Task ReportsTypeNamePassedToARecordBuiltInAnotherFile() => new CSharpCodeFixTest<TypeNameTextAnalyzer, EmptyCodeFixProvider> {
        TestState = { Sources = {
            ("/Models.cs", """
            using System;

            internal sealed partial record Entry(Guid Key, string Text);

            internal static partial class Flow {
                internal static Entry Build(string text, Guid key) => new(key, text);
            }
            """),
            ("/Calls.cs", """
            using System;

            internal static partial class Flow {
                internal static Entry Typed(Type type, Guid key) => Build(key: key, text: {|#0:type.Name|});
            }
            """),
        }, },
        ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("type.Name", "Entry") },
    }.RunAsync(TestContext.Current.CancellationToken);
}

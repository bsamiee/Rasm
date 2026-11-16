using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TypeNameTextTests : CSharpCodeFixVerifier<TypeNameTextAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3101]

    [Fact]
    public Task Labels() => new CSharpCodeFixTest<TypeNameTextAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
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
                internal static Label Direct(Alias type) => new({|#0:type.Name|});
                internal static Label Qualified(Alias type) => new({|#1:type.AssemblyQualifiedName|});
                internal static Label Transformed(Alias type) => new({|#2:type.FullName|}?.Trim());
                internal static Label Generic<T>() => new({|#3:nameof(T)|});
                internal static Label Interpolated(Alias type) => new($"name: {{|#4:type.Name|}}");
                internal static Label Replaced(Label label, Alias type) => label with { Text = {|#5:type.Name|} };
                internal static Label Initialized(Alias type) => new(null) { Text = {|#6:type.Name|} };
                internal static Label Relayed(Alias type) => Build(1, 2, 3, 4, 5, 6, 7, 8, {|#7:type.Name|});
                internal static Label Named(Alias type) => Build(text: {|#8:type.Name|}, first: 1, second: 2, third: 3, fourth: 4, fifth: 5, sixth: 6, seventh: 7, eighth: 8);
                internal static Label Extension(Alias type) => {|#9:type.Name|}.Label();
                internal static Label Changed(Alias type) => Build({|#10:type.Name|});
                internal static Label Display(Alias type) => new(type.Name.Length.ToString());
                internal static string Render(Alias type) => type.Name.Trim();
                internal static Label MethodName(MethodInfo method) => new(method.Name);
                internal static Exception Thrown(Alias type) => new InvalidOperationException(type.Name);
                internal static Error Failure(Alias type) => new Expected(type.Name, 0, default);
                internal static Grasshopper2.UI.Nomen DisplayName(Alias type) => new(type.Name);
                internal static Label Unstored(Alias type) => Ignore(type.Name);
                internal static Label Cyclic(Alias type) => Cycle(type.Name);
                internal static Label AliasValue(Alias type) {
                    string name = {|#11:type.Name|};
                    return new(name.Length > 0 ? name : "display");
                }
                internal static Label Reassigned(Alias type) {
                    string name = type.Name;
                    name = "display";
                    return new(name.Length > 0 ? name : "display");
                }
                internal static Label StoredBefore(Alias type) {
                    string name = {|#12:type.Name|};
                    Label held = new(name);
                    name = "display";
                    return held;
                }
                internal static Label Disjoint(Alias type, bool replaced) {
                    string name = {|#13:type.Name|};
                    if (replaced) {
                        name = "display";
                        return new(name);
                    }
                    return new(name);
                }
                internal static Label SourceFactory(Alias type) => LanguageExt.SourceFactory.Fail({|#14:type.Name|});
                internal static Label Hidden(DisplayType type) => new(type.ToString());
                internal static Label Overridden(RenderedType type) => new({|#15:type.ToString()|});
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
                    string name = {|#16:typeof(T).Name|};
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
                public new string ToString() => "display";
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
        TestState = { AnalyzerConfigFiles = { ("/.globalconfig", """
            is_global = true
            RASM3101.exempt = T:Grasshopper2.UI.Nomen
            """), }, },
        ExpectedDiagnostics = {
            Diagnostic().WithLocation(0).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(1).WithArguments("type.AssemblyQualifiedName", "Label"),
            Diagnostic().WithLocation(2).WithArguments("type.FullName", "Label"),
            Diagnostic().WithLocation(3).WithArguments("nameof(T)", "Label"),
            Diagnostic().WithLocation(4).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(5).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(6).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(7).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(8).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(9).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(10).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(11).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(12).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(13).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(14).WithArguments("type.Name", "Label"),
            Diagnostic().WithLocation(15).WithArguments("type.ToString()", "Label"),
            Diagnostic().WithLocation(16).WithArguments("typeof(T).Name", "Label"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    // --- [FILES]

    [Fact]
    public Task CrossFileStorage() => new CSharpCodeFixTest<TypeNameTextAnalyzer, EmptyCodeFixProvider> {
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

            internal sealed partial record Entry {
                internal bool Missing => Key == Guid.Empty;
            }

            internal static partial class Flow {
                internal static Entry Typed(Type type, Guid key) => Build(key: key, text: {|RASM3101:type.Name|});
                internal static Entry Empty() => new Entry(default, "display");
            }
            """),
        }, },
    }.RunAsync(TestContext.Current.CancellationToken);
}

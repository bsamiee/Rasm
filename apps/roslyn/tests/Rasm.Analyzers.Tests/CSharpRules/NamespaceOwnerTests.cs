using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class NamespaceOwnerTests : CSharpCodeFixVerifier<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3005]

    [Fact]
    public Task BorrowedNamespaces() => VerifyAnalyzerAsync(
        """
        namespace {|#0:Thinktecture|} {
            internal static class Squatted;
        }

        namespace {|#1:System.Text.Json.Serialization|} {
            internal static class Converted;
        }

        namespace Rasm.Core {
            internal static class Owned;
        }
        """,
        Diagnostic("RASM3005").WithLocation(0).WithArguments("Thinktecture", "Thinktecture.Runtime.Extensions"),
        Diagnostic("RASM3005").WithLocation(1).WithArguments("System.Text.Json.Serialization", "System.Text.Json"));

    [Fact]
    public Task BorrowedPackageNamespace() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Reflection;
        using LanguageExt.Common;
        using Alias = System.Type;

        internal sealed record Label(string? Text);

        namespace {|#0:LanguageExt|} {
            internal static class SourceFactory {
                internal static Label Fail(string text) => new(text);
            }
        }
        """,
        Diagnostic("RASM3005").WithLocation(0).WithArguments("LanguageExt", "LanguageExt.Core"));

    // --- [RASM3006]

    [Fact]
    public Task GenericClash() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.IO;
        using System.Runtime.CompilerServices;
        using LanguageExt;
        using Calendar = System.DateTime;

        namespace UserDefined {
            public sealed class {|#0:Lazy|}<T>(T value) {
                public T Value { get; } = value;
            }

            internal sealed class Read(Lazy<List<int>> value) {
                public Lazy<List<int>> Source { get; } = value;

                internal int Count => Source.Value.Count;
            }
        }
        """,
        Diagnostic("RASM3006").WithLocation(0).WithArguments("Lazy<T>", "System.Lazy<T>"));

    [Fact]
    public Task ImportedNames() => new CSharpCodeFixTest<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> {
        TestState = { Sources = {
            ("/Imports.cs", """
            global using global::System.Text;

            namespace Rasm.Texts {
                internal static class Builders {
                    internal static StringBuilder Fresh() => new();
                }

                internal static class Assembly;

                internal static class Calendar;

                internal static class Vector2;
            }

            namespace Rasm.Cultures {
                using System.Globalization;

                internal static class {|#0:Calendar|};
            }

            namespace Rasm.Partials {
                internal static partial class {|#1:Vector2|};
            }
            """),
            ("/Modeling.cs", """
            using System.Reflection;
            using LanguageExt.Traits;

            namespace Rasm.Modeling {
                internal static class {|#2:Assembly|};

                internal sealed record Proximity(double Distance);

                internal sealed record Proximity<T>(T Value, double Distance);

                internal abstract class ValueProvider<T>;

                internal abstract class ValueProvider<TValue, TKey, TError>;

                internal sealed record {|#3:Encoder|}(int Width);

                internal abstract class Edit;

                internal sealed record Edit<T>(T Value) : K<Edit, T>;

                internal static class Solids;
            }

            namespace Rasm.Partials {
                using System.Numerics;

                internal static partial class Vector2;
            }
            """),
        }, },
        ExpectedDiagnostics = {
            Diagnostic("RASM3006").WithLocation(0).WithArguments("Calendar", "System.Globalization.Calendar"),
            Diagnostic("RASM3006").WithLocation(1).WithArguments("Vector2", "System.Numerics.Vector2"),
            Diagnostic("RASM3006").WithLocation(2).WithArguments("Assembly", "System.Reflection.Assembly"),
            Diagnostic("RASM3006").WithLocation(3).WithArguments("Encoder", "System.Text.Encoder"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProjectReferences(bool listed) {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                namespace Consumer;

                internal sealed class {|#0:Entry|};
                """,
        };
        test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Catalog", """
            namespace Models;

            public sealed class Entry;
            """, token), token));
        test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", $"is_global = true\n{(listed ? "build_property.ProjectReferenceAssemblies = Catalog\n" : "")}"));
        test.ExpectedDiagnostics.AddRange(listed ? [Diagnostic("RASM3006").WithLocation(0).WithArguments("Entry", "Models.Entry")] : []);
        await test.RunAsync(token);
    }

    [Fact]
    public async Task ImportedReferences() {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                using Models;

                namespace Consumer;

                internal sealed class {|#0:Entry|};
                """,
            ExpectedDiagnostics = { Diagnostic("RASM3006").WithLocation(0).WithArguments("Entry", "Models.Entry") },
        };
        test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync($"{test.TestState.Name}.Catalog", """
            namespace Models;

            public sealed class Entry;
            """, token), token));
        await test.RunAsync(token);
    }
}

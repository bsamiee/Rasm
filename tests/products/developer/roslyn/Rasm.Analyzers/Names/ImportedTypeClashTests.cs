using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Names;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ImportedTypeClashTests : CSharpCodeFixVerifier<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> {

    // --- [CLASHES]

    [Fact]
    public Task ReportsATypeClashingWithAnImportedTypeOfEqualArityAndIgnoresOneOfAnotherArity() => VerifyAnalyzerAsync(
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
            public sealed class Lazy;

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
    public Task ReportsTypeClashingOnlyWithTypesImportedInItsScope() => new CSharpCodeFixTest<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> {
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

    [Fact]
    public async Task ReportsAClashWithAPublicTypeOfAListedProjectReferenceAndIgnoresUnlistedReferencesAndInternalTypes() {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<NamespaceOwnerAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                namespace Consumer;

                internal sealed class {|#0:Entry|};

                internal sealed class Shelf;

                internal sealed class Ledger;
                """,
            TestState = { AnalyzerConfigFiles = { ("/.globalconfig", "is_global = true\nbuild_property.ProjectReferenceAssemblies = Catalog\n") }, },
            ExpectedDiagnostics = { Diagnostic("RASM3006").WithLocation(0).WithArguments("Entry", "Models.Entry") },
        };
        test.TestState.AdditionalReferences.AddRange([
            AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Catalog", """
                namespace Models;

                public sealed class Entry;

                internal sealed class Shelf;
                """, token), token),
            AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Ledgers", """
                namespace Books;

                public sealed class Ledger;
                """, token), token),
        ]);
        await test.RunAsync(token);
    }

    // --- [QUALIFIED_NAMES]

    [Fact]
    public Task ReportsAQualifiedNameWhoseImportClashesWithNothingAndIgnoresOneWhoseImportWouldClash() => new CSharpCodeFixTest<QualifiedNameAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using System.Timers;

            internal sealed record Color(int Value);

            internal static class Qualified {
                internal static {|#0:System.Collections.Immutable.ImmutableArray<int>|} Empty => [];
                internal static System.Drawing.Bitmap Blank => new(1, 1);
                internal static Timer Ticker => new();
                internal static System.Threading.Thread Current => System.Threading.Thread.CurrentThread;
            }
            """,
        ExpectedDiagnostics = {
            CSharpCodeFixVerifier<QualifiedNameAnalyzer, EmptyCodeFixProvider>.Diagnostic("RASM3010").WithLocation(0)
                .WithArguments("System.Collections.Immutable.ImmutableArray<int>", "System.Collections.Immutable", "ImmutableArray<int>"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task ReportsAQualifiedNameAroundAShadowingParameterAndIgnoresOneAroundAMethodOfTheSameName() => new CSharpCodeFixTest<QualifiedNameAnalyzer, EmptyCodeFixProvider> {
        TestCode = """
            using LanguageExt;
            using static LanguageExt.Prelude;

            internal static class Painter {
                internal static int Bitmap(int size) => size;
                internal static System.Drawing.Bitmap Made(int size) => new(size, size);
                internal static Unit Run(int unit) => {|#0:Prelude.unit|};
            }
            """,
        ExpectedDiagnostics = {
            CSharpCodeFixVerifier<QualifiedNameAnalyzer, EmptyCodeFixProvider>.Diagnostic("RASM3009").WithLocation(0).WithArguments("Prelude.unit", "int unit", "LanguageExt.Prelude"),
        },
    }.RunAsync(TestContext.Current.CancellationToken);
}

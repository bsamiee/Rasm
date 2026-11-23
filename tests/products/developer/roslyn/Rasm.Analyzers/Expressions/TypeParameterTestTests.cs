using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Expressions;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TypeParameterTestTests : CSharpCodeFixVerifier<TypeParameterTestAnalyzer, EmptyCodeFixProvider> {

    // --- [PATTERNS]

    [Fact]
    public Task ReportsConcreteTestOfTypeParameterAndIgnoresSameTestOfDeclaredTypeOrAbstractTarget() => VerifyAnalyzerAsync(
        """
        internal abstract record Stage;

        internal sealed record Dither(int Levels) : Stage;

        internal static class Tests {
            internal static bool Is<TState>(TState state) where TState : Stage => {|#0:state is Dither|};

            internal static int Matched<TState>(TState state) where TState : Stage => state switch { {|#1:Dither dither|} => dither.Levels, _ => 0 };

            internal static Dither? Cast<TState>(TState state) where TState : Stage => {|#2:state as Dither|};

            internal static bool Declared(Stage stage) => stage is Dither;

            internal static bool Abstract<TState>(TState state) => state is Stage;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("state is Dither", "TState", "Dither"),
        Diagnostic().WithLocation(1).WithArguments("Dither dither", "TState", "Dither"),
        Diagnostic().WithLocation(2).WithArguments("state as Dither", "TState", "Dither"));

    // --- [TYPEOF]

    [Fact]
    public Task ReportsTypeEqualityReachingTypeParameterAndIgnoresReassignedOrRuntimeType() => VerifyAnalyzerAsync(
        """
        using System;

        internal abstract record Stage;

        internal sealed record Dither(int Levels) : Stage;

        internal static class Kinds {
            internal static bool Direct<TState>() where TState : Stage => {|#0:typeof(Dither) != typeof(TState)|};

            internal static bool Aliased<TState>() where TState : Stage {
                Type kind = typeof(TState);
                return {|#1:kind == typeof(Dither)|};
            }

            internal static bool Reassigned<TState>() where TState : Stage {
                Type kind = typeof(TState);
                kind = typeof(Stage);
                return kind == typeof(Dither);
            }

            internal static bool Runtime(Type state) => state == typeof(Dither);

            internal static bool Dispatched<TState>(TState state) where TState : Stage => {|#2:state.GetType() == typeof(Dither)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("typeof(Dither) != typeof(TState)", "TState", "Dither"),
        Diagnostic().WithLocation(1).WithArguments("kind == typeof(Dither)", "TState", "Dither"),
        Diagnostic().WithLocation(2).WithArguments("state.GetType() == typeof(Dither)", "TState", "Dither"));

    [Fact]
    public Task IgnoresTypeofEqualityInsideExternalOverrideAndReportsItInOverrideOfProjectMember() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Text.Json;
        using System.Text.Json.Serialization;

        internal abstract record Stage;

        internal sealed record Dither(int Levels) : Stage;

        internal sealed class StageConverter<TState> : JsonConverter<TState> where TState : Stage {
            public override bool CanConvert(Type typeToConvert) => typeof(TState) == typeof(Dither);

            public override TState? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null;

            public override void Write(Utf8JsonWriter writer, TState value, JsonSerializerOptions options) { }
        }

        internal abstract class Accepting<TState> where TState : Stage {
            internal abstract bool Accepts();
        }

        internal sealed class Owned<TState> : Accepting<TState> where TState : Stage {
            internal override bool Accepts() => {|#0:typeof(TState) == typeof(Dither)|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("typeof(TState) == typeof(Dither)", "TState", "Dither"));

    // --- [REFERENCES]

    [Fact]
    public async Task ReportsTypeOfListedProjectReferenceAndIgnoresTypeOfUnlistedReference() {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<TypeParameterTestAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                using Models;
                using Vendor;

                namespace Consumer;

                internal static class Usage {
                    internal static bool Listed<TState>(TState value) where TState : Stage => {|#0:value is Dither|};

                    internal static bool Unlisted<TPart>(TPart value) where TPart : Part => value is Bolt;
                }
                """,
            TestState = { AnalyzerConfigFiles = { ("/.globalconfig", "is_global = true\nbuild_property.ProjectReferenceAssemblies = Catalog\n") } },
            ExpectedDiagnostics = { Diagnostic().WithLocation(0).WithArguments("value is Dither", "TState", "Dither") },
        };
        test.TestState.AdditionalReferences.AddRange([
            AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Catalog", """
                namespace Models;

                public abstract record Stage;

                public sealed record Dither(int Value) : Stage;
                """, token), token),
            AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Foreign", """
                namespace Vendor;

                public abstract record Part;

                public sealed record Bolt(int Value) : Part;
                """, token), token),
        ]);
        await test.RunAsync(token);
    }
}

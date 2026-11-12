using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TypeParameterTestTests : CSharpCodeFixVerifier<TypeParameterTestAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3007]

    [Fact]
    public Task ConcreteTests() => VerifyAnalyzerAsync(
        """
        using System;

        internal abstract record Stage;

        internal sealed record Dither(int Levels) : Stage;

        internal sealed record Exposure(double Stops) : Stage;

        internal sealed record Recorded<TState>(TState Record) where TState : Stage;

        internal static class ConcreteTests {
            internal static bool Dithered<TState>(Recorded<TState> value) where TState : Stage => {|#0:value.Record is Dither|};

            internal static int Levels<TState>(TState state) where TState : Stage => state switch { {|#1:Dither dither|} => dither.Levels, _ => 0 };

            internal static Exposure? Exposed<TState>(TState state) where TState : Stage => {|#2:state as Exposure|};

            internal static bool Kind<TState>() where TState : Stage => {|#3:typeof(TState) == typeof(Dither)|};

            internal static bool Reversed<TState>() where TState : Stage => {|#4:typeof(Dither) != typeof(TState)|};

            internal static bool Aliased<TState>() where TState : Stage {
                Type kind = typeof(TState);
                return {|#5:kind == typeof(Dither)|};
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("value.Record is Dither", "TState", "Dither"),
        Diagnostic().WithLocation(1).WithArguments("Dither dither", "TState", "Dither"),
        Diagnostic().WithLocation(2).WithArguments("state as Exposure", "TState", "Exposure"),
        Diagnostic().WithLocation(3).WithArguments("typeof(TState) == typeof(Dither)", "TState", "Dither"),
        Diagnostic().WithLocation(4).WithArguments("typeof(Dither) != typeof(TState)", "TState", "Dither"),
        Diagnostic().WithLocation(5).WithArguments("kind == typeof(Dither)", "TState", "Dither"));

    [Fact]
    public Task StatedBehavior() => VerifyAnalyzerAsync(
        """
        using System;

        internal abstract record Stage;

        internal sealed record Dither(int Levels) : Stage;

        internal interface IReplayed {
            static abstract bool Replayed { get; }
        }

        internal sealed record Recorded<TState>(TState Record) where TState : Stage;

        internal static class StatedBehavior {
            internal static bool Replayed<TState>() where TState : IReplayed => TState.Replayed;

            internal static bool Staged(Stage stage) => stage is Dither;

            internal static bool Abstract<TState>(TState state) => state is Stage;

            internal static bool Runtime(Type state) => state != typeof(Dither);

            internal static bool Closed() => typeof(Stage) == typeof(Dither);

            internal static bool Reassigned<TState>() where TState : Stage {
                Type kind = typeof(TState);
                kind = typeof(Stage);
                return kind == typeof(Dither);
            }

            internal static int Held<TState>(Recorded<TState> value) where TState : Stage => value switch { var held => held.GetHashCode() };
        }
        """);

    [Fact]
    public Task Branch() => VerifyAnalyzerAsync(
        """
        internal abstract record Branch<T> {
            internal sealed record Label(T Value) : Branch<T>;

            internal sealed record Check(bool Value) : Branch<T>;

            internal bool Labeled => this is Label;

            internal Label? AsLabel() => this as Label;

            internal string Kind() => this switch { Label => nameof(Label), Check => nameof(Check), _ => nameof(Branch<T>) };
        }
        """);

    [Fact]
    public Task StageConverter() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Text.Json;
        using System.Text.Json.Serialization;

        internal abstract record Stage;

        internal sealed record Dither(int Levels) : Stage;

        internal sealed class StageConverter : JsonConverter<Stage> {
            public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Dither);

            public override Stage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => null;

            public override void Write(Utf8JsonWriter writer, Stage value, JsonSerializerOptions options) { }
        }
        """);

    // --- [REFERENCES]

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProjectReferences(bool listed) {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<TypeParameterTestAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                using Models;

                namespace Consumer;

                internal static class Usage {
                    internal static bool Is<TState>(TState value) where TState : Stage => {|#0:value is Dither|};
                }
                """,
        };
        test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Catalog", """
            namespace Models;

            public abstract record Stage;

            public sealed record Dither(int Value) : Stage;
            """, token), token));
        test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", $"is_global = true\n{(listed ? "build_property.ProjectReferenceAssemblies = Catalog\n" : "")}"));
        test.ExpectedDiagnostics.AddRange(listed ? [Diagnostic().WithLocation(0).WithArguments("value is Dither", "TState", "Dither")] : []);
        await test.RunAsync(token);
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Rasm.Csp.Generators;

namespace Rasm.Csp.Tests;

// --- [CONSTANTS] -------------------------------------------------------------------------

internal static class UnionSpecimens {
    public const string Attribute = """
        namespace Rasm.Domain;

        [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false)]
        public sealed class GenerateUnionOpsAttribute : System.Attribute;
        """;

    // Decoy members attack the case filter: non-sealed records, grandchildren, classes, and
    // structs must never receive a SelfOp anchor; accessibility is not part of the filter.
    public const string Unions = """
        namespace Rasm.Specimen;

        [Rasm.Domain.GenerateUnionOps]
        public abstract partial record Fault {
            public sealed partial record MissingCase : Fault;
            public sealed partial record RejectedCase(string Detail) : Fault;
            internal sealed partial record HiddenCase : Fault;
            public partial record OpenCase : Fault;
            public sealed partial record StrayCase : OpenCase;
            public sealed partial class Bystander;
            public readonly partial struct Bare;
        }

        [Rasm.Domain.GenerateUnionOps]
        public partial record Toggle {
            public sealed partial record OnCase : Toggle;
            public sealed partial record OffCase : Toggle;
        }
        """;
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public sealed class UnionOpsGeneratorSpec {
    public static TheoryData<string> SilentShapes => new(
        """
        namespace Rasm.Specimen;
        [Rasm.Domain.GenerateUnionOps]
        public abstract partial record Empty;
        """,
        """
        namespace Rasm.Specimen;
        [Rasm.Domain.GenerateUnionOps]
        public partial record Outer {
            public partial record OpenCase : Outer;
            public sealed partial class Bystander;
        }
        """,
        """
        namespace Rasm.Specimen;
        public abstract partial record Unannotated {
            public sealed partial record ACase : Unannotated;
        }
        """);

    [Fact]
    public async Task EmittedSourceMatchesContract() {
        GeneratorDriverRunResult run = Run(UnionSpecimens.Attribute, UnionSpecimens.Unions);
        Assert.Empty(run.Diagnostics);
        _ = await Verifier.Verify(Render(run));
    }

    [Theory]
    [MemberData(nameof(SilentShapes))]
    public void NonQualifyingShapesEmitNothing(string source) =>
        Assert.Empty(Run(UnionSpecimens.Attribute, source).GeneratedTrees);

    [Fact]
    public void UnrelatedTreeChangeReusesCachedOutputs() {
        CancellationToken cancel = TestContext.Current.CancellationToken;
        CSharpCompilation compilation = Compile(UnionSpecimens.Attribute, UnionSpecimens.Unions);
        GeneratorDriver driver = Driver().RunGenerators(compilation, cancellationToken: cancel);
        GeneratorDriverRunResult cold = driver.GetRunResult();
        GeneratorDriverRunResult warm = driver
            .RunGenerators(
                compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("namespace Rasm.Specimen.Unrelated { public sealed class Noise; }", cancellationToken: cancel)),
                cancellationToken: cancel)
            .GetRunResult();
        Assert.Equal(Render(cold), Render(warm));
        Assert.All(
            warm.Results[0].TrackedOutputSteps.SelectMany(static steps => steps.Value).SelectMany(static step => step.Outputs),
            static output => Assert.True(
                output.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                $"output step re-ran with reason {output.Reason}"));
    }

    private static CSharpGeneratorDriver Driver() => CSharpGeneratorDriver.Create(
        generators: [new UnionOpsGenerator().AsSourceGenerator()],
        driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    private static CSharpCompilation Compile(params string[] sources) => CSharpCompilation.Create(
        assemblyName: "UnionOpsSpecimen",
        syntaxTrees: sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
        references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static GeneratorDriverRunResult Run(params string[] sources) =>
        Driver().RunGenerators(Compile(sources)).GetRunResult();

    private static string Render(GeneratorDriverRunResult run) => string.Join(
        '\n',
        run.Results.SelectMany(static result => result.GeneratedSources)
            .Select(static generated => "// --- hint: " + generated.HintName + "\n" + generated.SourceText));
}

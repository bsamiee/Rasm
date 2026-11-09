using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Rasm.Analyzers.CodeFixes;
using Rasm.Rhino.Analyzers;
using Thinktecture.CodeAnalysis.SmartEnums;

namespace Rasm.Analyzers.Tests;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class Analysis : CSharpCodeFixTest<EmptyDiagnosticAnalyzer, EmptyCodeFixProvider, DefaultVerifier> {
    private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers = [
        .. Exported<DiagnosticAnalyzer, DiagnosticAnalyzerAttribute>(typeof(ResultLiftAnalyzer).Assembly),
        .. Exported<DiagnosticAnalyzer, DiagnosticAnalyzerAttribute>(typeof(PlugInShells).Assembly),
    ];

    private static readonly ImmutableArray<CodeFixProvider> Fixes = Exported<CodeFixProvider, ExportCodeFixProviderAttribute>(typeof(RasmCodeFix).Assembly);

    private static readonly ImmutableArray<Type> Generators = [.. new[] { typeof(SmartEnumSourceGenerator), typeof(PlugInShells) }
        .SelectMany(static held => held.Assembly.GetExportedTypes()).Where(static type => type.IsDefined(typeof(GeneratorAttribute), inherit: false))];

    internal static readonly FrozenDictionary<string, (DiagnosticDescriptor Descriptor, bool Fixable)> Rules = Analyzers.SelectMany(static analyzer => analyzer.SupportedDiagnostics)
        .ToFrozenDictionary(static descriptor => descriptor.Id, static descriptor => (descriptor, Fixes.Any(fix => fix.FixableDiagnosticIds.Contains(descriptor.Id, StringComparer.Ordinal))), StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, string> References = toSeq(typeof(Analysis).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        .Choose(static held => Optional(held.Value).Map(value => (held.Key, Value: value)))
        .ToFrozenDictionary(static held => held.Key, static held => held.Value, StringComparer.Ordinal);

    private static readonly ReferenceAssemblies Framework = References["TargetingPack"].Split('|') switch {
        [var id, var version, var framework] => new(framework, new PackageIdentity(id, version), Path.Combine("ref", framework)),
        _ => throw new InvalidOperationException(nameof(Framework)),
    };

    private static readonly ImmutableArray<PortableExecutableReference> AdditionalReferences = [.. References
        .Where(static reference => reference.Key.StartsWith("AdditionalReference.", StringComparison.Ordinal))
        .Select(static reference => MetadataReference.CreateFromFile(reference.Value))];

    internal Analysis(DiagnosticDescriptor? rule = null) {
        ReferenceAssemblies = Framework;
        DisabledDiagnostics.AddRange(["CS1701", "CS1702"]);
        TestState.AdditionalReferences.AddRange(AdditionalReferences);
        TestState.DocumentationMode = DocumentationMode.Parse;
        CompilerDiagnostics = CompilerDiagnostics.Warnings;
        CodeActionValidationMode = CodeActionValidationMode.Full;
        CodeFixTestBehaviors = rule?.CustomTags.Contains(WellKnownDiagnosticTags.CompilationEnd, StringComparer.Ordinal) is true ? CodeFixTestBehaviors.SkipLocalDiagnosticCheck : CodeFixTestBehaviors.None;
        TestBehaviors = TestBehaviors.SkipGeneratedSourcesCheck;
    }

    protected override CompilationOptions CreateCompilationOptions() =>
        ((CSharpCompilationOptions)base.CreateCompilationOptions()).WithNullableContextOptions(NullableContextOptions.Enable);

    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() => Analyzers;

    protected override IEnumerable<CodeFixProvider> GetCodeFixProviders() => Fixes;

    protected override IEnumerable<Type> GetSourceGenerators() => Generators;

    private static ImmutableArray<T> Exported<T, TAttribute>(Assembly assembly) where TAttribute : Attribute =>
        [.. assembly.GetExportedTypes().Where(static type => type.IsDefined(typeof(TAttribute), inherit: false)).Select(static type => Activator.CreateInstance(type)).OfType<T>()];
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class Samples {
    private static readonly string Root = Path.Combine(AttributeReader.GetProjectDirectory(typeof(Samples).Assembly), nameof(Samples));

    public static TheoryData<string> Rules => [.. Directory.EnumerateFileSystemEntries(Root).Where(static path => !path.EndsWith(".globalconfig", StringComparison.Ordinal))
        .Select(static path => Path.GetFileNameWithoutExtension(path)).Order(StringComparer.Ordinal)];

    [Theory]
    [MemberData(nameof(Rules))]
    public async Task Verified(string rule) {
        CancellationToken token = TestContext.Current.CancellationToken;
        string folder = Path.Combine(Root, rule);
        IEnumerable<(string Path, string File)> files = Directory.Exists(folder)
            ? Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories).Order(StringComparer.Ordinal).Select(file => ($"/{Path.ChangeExtension(Path.GetRelativePath(folder, file), "cs").Replace('\\', '/')}", file))
            : [($"/{rule}.cs", Path.Combine(Root, $"{rule}.txt"))];
        (string Path, SourceText Source, ImmutableDictionary<string, ImmutableArray<TextSpan>> Markers)[] samples =
            await Task.WhenAll(files.Select(async file => {
                TestFileMarkupParser.GetSpans(await File.ReadAllTextAsync(file.File, token).ConfigureAwait(false), out string source, out ImmutableDictionary<string, ImmutableArray<TextSpan>> markers);
                return (file.Path, SourceText.From(source), markers);
            }));
        (DiagnosticDescriptor descriptor, bool fixable) = Analysis.Rules[rule];
        FrozenDictionary<(string Path, TextSpan Span, string Id), (string Name, DiagnosticResult Expected)> markers = (
            from sample in samples
            from marker in sample.Markers
            from span in marker.Value
            let expected = (marker.Key.StartsWith("CS", StringComparison.Ordinal) ? DiagnosticResult.CompilerWarning(marker.Key)
                : new DiagnosticResult(Analysis.Rules.TryGetValue(marker.Key, out (DiagnosticDescriptor Descriptor, bool Fixable) named) ? named.Descriptor : descriptor))
                .WithSpan(new FileLinePositionSpan(sample.Path, sample.Source.Lines.GetLinePositionSpan(span)))
                .WithMessage(null)
                .WithOptions(DiagnosticOptions.IgnoreAdditionalLocations)
            select KeyValuePair.Create((sample.Path, span, expected.Id), (marker.Key, expected))).ToFrozenDictionary();
        Dictionary<(string Path, TextSpan Span, string Id), string> reported = [];
        Analysis analysis = new(descriptor) {
            DiagnosticVerifier = (diagnostic, _, _) => {
                if (!Analysis.Rules.ContainsKey(diagnostic.Id)) return;
                (string Path, TextSpan Span, string Id) site = (diagnostic.Location.GetLineSpan().Path, diagnostic.Location.SourceSpan, diagnostic.Id);
                reported[site] = $"{markers[site].Name} {diagnostic.GetMessage(CultureInfo.InvariantCulture)}\n";
            },
        };
        analysis.TestState.Sources.AddRange(samples.Select(static sample => (sample.Path, sample.Source)));
        string options = Path.Combine(Root, $"{rule}.globalconfig");
        if (File.Exists(options)) analysis.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", await File.ReadAllTextAsync(options, token)));
        if (fixable) analysis.FixedState.Sources.AddRange(
            await Task.WhenAll(samples.Select(async sample => (
                sample.Path,
                SourceText.From(await File.ReadAllTextAsync(Path.Combine(Root, "..", "Fixes", samples.Length is 1 ? $"{rule}.txt" : Path.Combine(rule, Path.ChangeExtension(sample.Path.TrimStart('/'), "txt"))), token).ConfigureAwait(false))))));
        analysis.ExpectedDiagnostics.AddRange(markers.Values.Select(static marker => marker.Expected));
        await analysis.RunAsync(token);
        _ = await Verify(string.Concat(reported.OrderBy(static pair => pair.Key.Path, StringComparer.Ordinal)
            .ThenBy(static pair => pair.Key.Span.Start).ThenBy(static pair => pair.Key.Span.End).ThenBy(static pair => pair.Key.Id, StringComparer.Ordinal)
            .Select(static pair => pair.Value))).UseFileName(rule);
    }

    [Fact]
    public Task SelectedFixAll() {
        const string source = """
            using LanguageExt;

            internal static class Sample {
                internal static Fin<int> Lift(int value) => {|#0:{|RASM1001:Fin.Succ(value)|}|};
                internal static Fin<int> Narrow(Fin<int> value) => value.{|RASM1007:As()|};
            }

            """;
        return new Analysis(Analysis.Rules["RASM1001"].Descriptor) {
            TestCode = source,
            FixedCode = source.Replace("value.{|RASM1007:As()|}", "value", StringComparison.Ordinal),
            FixedState = { ExpectedDiagnostics = {
                new DiagnosticResult(Analysis.Rules["RASM1001"].Descriptor).WithLocation(0).WithArguments("Fin.Succ(value)", "value", "Fin<int>"),
            }, },
            CodeActionEquivalenceKey = "RASM1007",
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public Task TopLevelConversion() => new Analysis(Analysis.Rules["RASM1004"].Descriptor) {
        TestState = { OutputKind = OutputKind.ConsoleApplication },
        TestCode = """
            LanguageExt.ValidationExtensions.{|RASM1004:ToFin(LanguageExt.Fin.Succ(1).ToValidation())|};

            """,
        FixedCode = """
            LanguageExt.Fin.Succ(1);

            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task CoveredArms() => new Analysis {
        ExpectedDiagnostics = { DiagnosticResult.CompilerError("CS8510").WithLocation(0) },
        FixedState = { ExpectedDiagnostics = { DiagnosticResult.CompilerWarning("CS8524").WithLocation(1) } },
        TestCode = """
            internal enum Mode { First, Second }

            internal static class Sample {
                internal static int Of(Mode value) => value switch {
                    Mode.First => 1,
                    Mode.Second => 2,
                    {|RASM3002:not Mode.First => 3|},
                    {|RASM3002:{|#0:_|} => 4|},
                };
            }

            """,
        FixedCode = """
            internal enum Mode { First, Second }

            internal static class Sample {
                internal static int Of(Mode value) => value {|#1:switch|} {
                    Mode.First => 1,
                    Mode.Second => 2,
                };
            }

            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task DependentMembers() {
        string[] names = ["ReadRead", "Read", "ReadReadRead"];
        return new Analysis(Analysis.Rules["RASM3001"].Descriptor) {
            TestCode = $$"""
                internal static class Sample {
                    private const int ReadValue = 1;

                    internal static int Value => {{names[0]}}();

                {{string.Join("\n\n", names.Select((name, index) => $"    private static int {{|RASM3001:{name}|}}() => {(index + 1 == names.Length ? "ReadValue" : $"{names[index + 1]}()")};"))}}
                }

                """,
            FixedCode = """
                internal static class Sample {
                    private const int ReadValue = 1;

                    internal static int Value => ReadValue;
                }

                """,
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public Task CrossFileStorage() => new Analysis {
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
                internal static Entry Empty() => {|RASM3102:new Entry(default, "display")|};
            }

            """),
        }, },
    }.RunAsync(TestContext.Current.CancellationToken);

    [Fact]
    public Task FixAllProjects() {
        Analysis analysis = new(Analysis.Rules["RASM3001"].Descriptor);
        ProjectState[] states = [analysis.TestState, analysis.TestState.AdditionalProjects["Other"]];
        foreach (ProjectState state in states) {
            ProjectState fixedState = state == analysis.TestState ? analysis.FixedState : analysis.FixedState.AdditionalProjects[state.Name];
            foreach (string name in new[] { "First", "Second" }) {
                state.Sources.Add(($"/{state.Name}/{name}.cs", $$"""
                    internal static class {{name}} {
                        internal static int Value => Read(1);

                        private static int {|RASM3001:Read|}(int read) => read;
                    }

                    """));
                fixedState.Sources.Add(($"/{state.Name}/{name}.cs", $$"""
                    internal static class {{name}} {
                        internal static int Value => 1;
                    }

                    """));
            }
        }
        analysis.NumberOfFixAllInDocumentIterations = states.Sum(static state => state.Sources.Count);
        analysis.NumberOfFixAllInProjectIterations = states.Length;
        return analysis.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public Task RelatedLocations() => new Analysis {
        ExpectedDiagnostics = {
            new DiagnosticResult(Analysis.Rules["RASM3001"].Descriptor).WithLocation(0).WithLocation(1).WithArguments("Read", "ReadValue"),
            new DiagnosticResult(Analysis.Rules["RASM1004"].Descriptor).WithLocation(2).WithLocation(3).WithLocation(4).WithArguments("ToValidation", "ToValidation", "ToFin"),
            new DiagnosticResult(Analysis.Rules["RASM2103"].Descriptor).WithLocation(5).WithLocation(6).WithArguments("Cast", "curve.Duplicate()", "Curve"),
        },
        TestCode = """
            using System;
            using LanguageExt;
            using Rhino.Geometry;

            namespace Rasm.Rhino {
                public static class Products {
                    public static T Copied<T>(Func<T> call) where T : class => call();
                }
            }

            internal static class Sample {
                private const int {|#0:Read|} = 1;

                internal static readonly int ReadValue = {|#1:Read|};

                internal static Fin<int> Round(Fin<int> value) => {|#4:{|#3:value.ToValidation()|}.{|#2:ToFin()|}|};

                internal static Curve Narrow(Curve curve) => Cast({|#5:curve.Duplicate()|});

                internal static Curve Cast(GeometryBase copy) => {|#6:(Curve)copy|};
            }

            """,
    }.RunAsync(TestContext.Current.CancellationToken);

    private const string ReferencedCode = """
        using Stage = Models.Stage;
        using Dither = Models.Dither;

        namespace Consumer;

        internal sealed class {|#0:Entry|};

        internal static class Usage {
            internal static bool Is<TState>(TState value) where TState : Stage => {|#1:value is Dither|};
        }

        """;

    private const string ReferencedModels = """
        namespace Models;

        public sealed class Entry;

        public abstract record Stage;

        public sealed record Dither(int Value) : Stage;
        """;

    private static async Task<CSharpCompilation> ReferencedAsync(Analysis analysis, string name, string source, CancellationToken token) =>
        CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, cancellationToken: token)], await analysis.ReferenceAssemblies.ResolveAsync(LanguageNames.CSharp, token).ConfigureAwait(false), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static PortableExecutableReference Metadata(CSharpCompilation compilation, CancellationToken token) {
        using MemoryStream image = new();
        EmitResult emitted = compilation.Emit(image, cancellationToken: token);
        Assert.True(emitted.Success, string.Join('\n', emitted.Diagnostics));
        image.Position = 0;
        return MetadataReference.CreateFromStream(image);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProjectReferences(bool listed) {
        CancellationToken token = TestContext.Current.CancellationToken;
        Analysis analysis = new() { TestCode = ReferencedCode };
        analysis.TestState.AdditionalReferences.Add(Metadata(await ReferencedAsync(analysis, "Catalog", ReferencedModels, token), token));
        analysis.TestState.AnalyzerConfigFiles.Add(("/Build.globalconfig", $"is_global = true\n{(listed ? "build_property.ProjectReferenceAssemblies = Catalog\n" : "")}"));
        analysis.ExpectedDiagnostics.AddRange(listed ? [
            new DiagnosticResult(Analysis.Rules["RASM3006"].Descriptor).WithLocation(0).WithArguments("Entry", "Models.Entry"),
            new DiagnosticResult(Analysis.Rules["RASM3007"].Descriptor).WithLocation(1).WithArguments("value is Dither", "TState", "Dither"),
        ] : []);
        await analysis.RunAsync(token);
    }

    [Fact]
    public async Task ImportedReferences() {
        CancellationToken token = TestContext.Current.CancellationToken;
        Analysis analysis = new() {
            TestCode = $"using Models;\n{ReferencedCode}",
            ExpectedDiagnostics = { new DiagnosticResult(Analysis.Rules["RASM3006"].Descriptor).WithLocation(0).WithArguments("Entry", "Models.Entry") },
        };
        analysis.TestState.AdditionalReferences.Add(Metadata(await ReferencedAsync(analysis, $"{analysis.TestState.Name}.Catalog", ReferencedModels, token), token));
        await analysis.RunAsync(token);
    }

    [Fact]
    public async Task ForeignCallbacks() {
        CancellationToken token = TestContext.Current.CancellationToken;
        Analysis analysis = new() {
            TestCode = """
                using Eto.Forms;
                using LanguageExt;
                using LanguageExt.Common;
                using Microsoft.Extensions.Logging;
                using Rhino.Runtime;
                using Rhino.UI;
                using static LanguageExt.Prelude;

                internal static class Reports {
                    internal static Unit Translated(ILogger logger) => Result(logger).IfFail(error => HostUtils.ExceptionReport(error.ToException()));

                    private static Fin<Unit> Result(ILogger logger) {
                        External.Calls.Invoke(() => Failed(logger, "failure"));
                        return unit;
                    }

                    [LoggerMessage(Level = LogLevel.Error, Message = "{Failure}")]
                    private static void Failed(ILogger logger, string failure) { }
                }

                public abstract class ForeignPage : ObjectPropertiesPage {
                    public override string EnglishPageTitle => "Foreign";

                    public override object PageControl => External.Calls.Value(static () => new Button());
                }

                """,
        };
        analysis.TestState.AdditionalReferences.Add((await ReferencedAsync(analysis, $"{analysis.TestState.Name}.Callbacks", """
            using System;

            namespace External;

            public static class Calls {
                public static void Invoke(Action callback) => callback();

                public static object Value(Func<object> callback) => callback();
            }
            """, token)).ToMetadataReference());
        await analysis.RunAsync(token);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

internal static class Snapshots {
    [ModuleInitializer]
    internal static void Initialize() {
        VerifierSettings.UseUtf8NoBom();
        UseProjectRelativeDirectory("Snapshots");
    }
}

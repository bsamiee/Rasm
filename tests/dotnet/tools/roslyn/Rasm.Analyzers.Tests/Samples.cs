using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Rasm.Analyzers.CodeFixes;
using Rhino;
using Thinktecture.CodeAnalysis.SmartEnums;

namespace Rasm.Analyzers.Tests;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Analysis : CSharpCodeFixTest<EmptyDiagnosticAnalyzer, EmptyCodeFixProvider, DefaultVerifier> {
    private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers = Exported<DiagnosticAnalyzer, DiagnosticAnalyzerAttribute>(typeof(RasmAnalyzer).Assembly);

    private static readonly ImmutableArray<CodeFixProvider> Fixes = Exported<CodeFixProvider, ExportCodeFixProviderAttribute>(typeof(RasmCodeFix).Assembly);

    private static readonly ImmutableArray<Type> Generators = [.. typeof(SmartEnumSourceGenerator).Assembly.GetExportedTypes().Where(static type => type.IsDefined(typeof(GeneratorAttribute), inherit: false))];

    internal static readonly FrozenDictionary<string, (DiagnosticDescriptor Descriptor, bool Fixable)> Rules = Analyzers.SelectMany(static analyzer => analyzer.SupportedDiagnostics)
        .ToFrozenDictionary(static descriptor => descriptor.Id, static descriptor => (descriptor, Fixes.Any(fix => fix.FixableDiagnosticIds.Contains(descriptor.Id, StringComparer.Ordinal))), StringComparer.Ordinal);

    private static readonly string EditorConfig = File.ReadAllText(Path.Combine(AttributeReader.GetSolutionDirectory(typeof(Analysis).Assembly), ".editorconfig"));

    private static readonly ReferenceAssemblies Framework =
        typeof(Analysis).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToFrozenDictionary(static held => held.Key, static held => held.Value!, StringComparer.Ordinal) switch {
            var build => new(build["TargetingPackFramework"], new PackageIdentity(build["TargetingPackId"], build["TargetingPackVersion"]), Path.Combine("ref", build["TargetingPackFramework"])),
        };

    internal Analysis(DiagnosticDescriptor? rule = null) {
        ReferenceAssemblies = Framework;
        DisabledDiagnostics.AddRange(["CS1701", "CS1702"]);
        foreach (Assembly assembly in new[] { typeof(Fin<>).Assembly, typeof(UnionAttribute).Assembly, typeof(RhinoDoc).Assembly, typeof(Bitmap).Assembly, typeof(Application).Assembly, typeof(ILogger).Assembly })
            TestState.AdditionalReferences.Add(assembly);
        TestState.AnalyzerConfigFiles.Add(("/.editorconfig", EditorConfig));
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
        [.. assembly.GetExportedTypes().Where(static type => type.IsDefined(typeof(TAttribute), inherit: false)).Select(static type => (T)Activator.CreateInstance(type)!)];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Samples {
    private enum LiftCase { Success, Failure, Message }

    private static readonly string Root = Path.Combine(AttributeReader.GetProjectDirectory(typeof(Samples).Assembly), nameof(Samples));

    public static TheoryData<string> Rules => [.. Directory.EnumerateFileSystemEntries(Root).Select(static path => Path.GetFileNameWithoutExtension(path)).Order(StringComparer.Ordinal)];

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
                (string Path, TextSpan Span, string Id) site = (diagnostic.Location.SourceTree!.FilePath, diagnostic.Location.SourceSpan, diagnostic.Id);
                reported[site] = $"{markers[site].Name} {diagnostic.GetMessage(CultureInfo.InvariantCulture)}\n";
            },
        };
        analysis.TestState.Sources.AddRange(samples.Select(static sample => (sample.Path, sample.Source)));
        if (fixable) analysis.FixedState.Sources.AddRange(
            await Task.WhenAll(samples.Select(async sample => (
                sample.Path,
                SourceText.From(await File.ReadAllTextAsync(Path.Combine(Root, "..", "Fixes", samples.Length is 1 ? $"{rule}.txt" : Path.Combine(rule, Path.ChangeExtension(sample.Path.TrimStart('/'), "txt"))), token).ConfigureAwait(false))))));
        analysis.ExpectedDiagnostics.AddRange(markers.Values.Select(static marker => marker.Expected));
        await analysis.RunAsync(token);
        _ = await Verify(string.Concat(reported.OrderBy(static pair => pair.Key.Path, StringComparer.Ordinal)
            .ThenBy(static pair => pair.Key.Span.Start).ThenBy(static pair => pair.Key.Span.End).ThenBy(static pair => pair.Key.Id, StringComparer.Ordinal)
            .Select(static pair => pair.Value))).UseDirectory("Snapshots").UseFileName(rule);
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
        return new Analysis(LanguageExtAnalyzer.Lift) {
            TestCode = source,
            FixedCode = source.Replace("value.{|RASM1007:As()|}", "value", StringComparison.Ordinal),
            FixedState = { ExpectedDiagnostics = {
                new DiagnosticResult(LanguageExtAnalyzer.Lift).WithLocation(0).WithArguments("Fin.Succ(value)", "value", "Fin<int>"),
            }, },
            CodeActionEquivalenceKey = LanguageExtAnalyzer.Narrowed.Id,
        }.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public Task TopLevelConversion() => new Analysis(LanguageExtAnalyzer.RoundTrip) {
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
        return new Analysis(LanguageAnalyzer.Reader) {
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
        Analysis analysis = new(LanguageAnalyzer.Reader);
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
            using LanguageExt;
            using Rhino.Geometry;

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

    [Fact]
    public async Task SourceReferences() {
        CancellationToken token = TestContext.Current.CancellationToken;
        Analysis analysis = new() {
            TestCode = ReferencedCode,
            ExpectedDiagnostics = {
                new DiagnosticResult(Analysis.Rules["RASM3006"].Descriptor).WithLocation(0).WithArguments("Entry", "Models.Entry"),
                new DiagnosticResult(Analysis.Rules["RASM3007"].Descriptor).WithLocation(1).WithArguments("value is Dither", "TState", "Dither"),
            },
        };
        analysis.TestState.AdditionalReferences.Add((await ReferencedAsync(analysis, "Catalog", ReferencedModels, token)).ToMetadataReference());
        await analysis.RunAsync(token);
    }

    [Fact]
    public async Task MetadataReferences() {
        CancellationToken token = TestContext.Current.CancellationToken;
        Analysis analysis = new() { TestCode = ReferencedCode };
        analysis.TestState.AdditionalReferences.Add(Metadata(await ReferencedAsync(analysis, $"{analysis.TestState.Name}.Catalog", ReferencedModels, token), token));
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

    [Theory]
    [InlineData(null, ".dll", false)]
    [InlineData(null, ".exe", false)]
    [InlineData("library", ".dll", true)]
    [InlineData(null, ".rhp", true)]
    public async Task RedrawApplicability(string? role, string extension, bool expected) {
        CancellationToken token = TestContext.Current.CancellationToken;
        Analysis analysis = new() {
            TestCode = """
                using System;
                using Rhino;

                internal static class Redraws {
                    internal static void Redrawn(RhinoDoc doc) => {|#0:doc.Views.Redraw()|};

                    internal static Action Deferred(RhinoDoc doc) => {|#1:doc.Views.Redraw|};
                }

                """,
        };
        analysis.TestState.AnalyzerConfigFiles.Add(("/Build.globalconfig", $"is_global = true\n{(role is null ? "" : $"build_property.ProjectRole = {role}\n")}build_property.TargetExt = {extension}\n"));
        analysis.TestState.AdditionalReferences.Add(Metadata(await ReferencedAsync(analysis, $"{analysis.TestState.Name}.Policies", """
            namespace Rasm.Rhino.Document;

            public static class Commits;
            """, token), token));
        analysis.ExpectedDiagnostics.AddRange(expected ? [
            new DiagnosticResult(RhinoAnalyzer.Redraw).WithLocation(0).WithArguments("doc.Views.Redraw()"),
            new DiagnosticResult(RhinoAnalyzer.Redraw).WithLocation(1).WithArguments("doc.Views.Redraw"),
        ] : []);
        await analysis.RunAsync(token);
    }

    [Fact]
    public Task Lifts() {
        static string Fin(string type) => $"Fin<{type}>";
        static string Validation(string type) => $"Validation<Error, {type}>";
        static string Sample(IEnumerable<string> members) => $$"""
            using System;
            using System.Threading.Tasks;
            using LanguageExt;
            using LanguageExt.Common;
            using static LanguageExt.Prelude;

            internal static class Sample {
                internal static R Same<R>(R value) => value;
            {{string.Join('\n', members.Select(static member => $"    {member}"))}}
            }

            """;
        return Gen.OneOf<(string Type, string Text, bool IsError)>(
                ("int", "1", false),
                ("string", "string.Empty", false),
                ("Unit", "unit", false),
                ("Seq<int>", "Seq(1, 2)", false),
                ("Option<int>", "Some(1)", false),
                ("object", "new object()", false),
                ("Error", """Error.New("held")""", true),
                ("Expected", """new Expected("held", 1, None)""", false))
            .Select(
                Gen.OneOf<(Func<string, string> Target, Func<string, string, string> Text, LiftCase Case)>(
                    (Fin, static (_, value) => $"Fin.Succ({value})", LiftCase.Success),
                    (Fin, static (_, value) => $"Pure({value})", LiftCase.Success),
                    (Fin, static (type, value) => $"new Fin<{type}>.Succ({value})", LiftCase.Success),
                    (Fin, static (type, error) => $"Fin.Fail<{type}>({error})", LiftCase.Failure),
                    (Fin, static (_, error) => $"Fail({error})", LiftCase.Failure),
                    (Fin, static (type, error) => $"new Fin<{type}>.Fail({error})", LiftCase.Failure),
                    (Fin, static (type, _) => $"""Fin.Fail<{type}>("refused")""", LiftCase.Message),
                    (Validation, static (type, value) => $"Success<Error, {type}>({value})", LiftCase.Success),
                    (Validation, static (type, value) => $"Validation.Success<Error, {type}>({value})", LiftCase.Success),
                    (Validation, static (type, error) => $"Fail<Error, {type}>({error})", LiftCase.Failure),
                    (Validation, static (type, error) => $"Validation.Fail<Error, {type}>({error})", LiftCase.Failure)),
                Gen.OneOf<(bool FixesTarget, Func<string, string, string, string> Member)>(
                    (true, static (target, lift, name) => $"internal static {target} {name}() => {lift};"),
                    (true, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { return {{lift}}; }"""),
                    (true, static (target, lift, name) => $"internal static readonly {target} {name} = {lift};"),
                    (true, static (target, lift, name) => $$"""internal static {{target}} {{name}} { get; } = {{lift}};"""),
                    (true, static (target, lift, name) => $"internal static {target} {name}(bool held) => held ? {lift} : {name}(false);"),
                    (true, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { return Take({{lift}}); static {{target}} Take({{target}} value) => value; }"""),
                    (true, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { {{target}} held; held = {{lift}}; return held; }"""),
                    (true, static (target, lift, name) => $"internal static Func<{target}> {name}() => () => {lift};"),
                    (true, static (target, lift, name) => $$"""internal static Func<Task<{{target}}>> {{name}}() => async () => { await Task.Yield(); return {{lift}}; };"""),
                    (false, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { var held = {{lift}}; return held; }"""),
                    (false, static (target, lift, name) => $"internal static {target} {name}() => Same({lift});"),
                    (false, static (target, lift, name) => $"internal static {target}? {name}(bool held) => held ? {lift} : default;")))
            .Array[1, 6]
            .Select(static slots => {
                (string Source, string Fixed)[] members = [
                    .. slots.Select(static (slot, index) => {
                        ((string Type, string Text, bool IsError) value,
                            (Func<string, string> Target, Func<string, string, string> Text, LiftCase Case) form,
                            (bool FixesTarget, Func<string, string, string, string> Member) position) = slot;
                        (string target, string name) = (form.Target(value.Type), string.Create(CultureInfo.InvariantCulture, $"M{index}"));
                        string argument = form.Case is LiftCase.Success ? value.Text : """Error.New("refused")""";
                        string lift = form.Text(value.Type, argument);
                        return position.FixesTarget && form.Case is not LiftCase.Message && !value.IsError
                            ? (position.Member(target, $"{{|RASM1001:{lift}|}}", name), position.Member(target, argument, name))
                            : (position.Member(target, lift, name), position.Member(target, lift, name));
                    }),
                ];
                return (Source: Sample(members.Select(static member => member.Source)), Fixed: Sample(members.Select(static member => member.Fixed)));
            })
            .SampleAsync(static sample => new Analysis(LanguageExtAnalyzer.Lift) { TestCode = sample.Source, FixedCode = sample.Fixed }.RunAsync(TestContext.Current.CancellationToken));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Snapshots {
    [ModuleInitializer]
    internal static void Initialize() => VerifierSettings.UseUtf8NoBom();
}

using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Rasm.Analyzers.CodeFixes;
using Rhino;

namespace Rasm.Analyzers.Tests;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum LiftCase {
    Success,
    Failure,
    Message,
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record LiftValue(string Type, string Text, bool IsError);

internal sealed record LiftForm(Func<string, string> Target, Func<string, string, string> Text, LiftCase Case);

internal sealed record LiftPosition(bool FixesTarget, Func<string, string, string, string> Member);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Analysis : CSharpCodeFixTest<EmptyDiagnosticAnalyzer, EmptyCodeFixProvider, DefaultVerifier> {
    internal static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers = Exported<DiagnosticAnalyzer, DiagnosticAnalyzerAttribute>(typeof(RasmAnalyzer).Assembly);

    private static readonly ImmutableArray<CodeFixProvider> Fixes = Exported<CodeFixProvider, ExportCodeFixProviderAttribute>(typeof(RasmCodeFix).Assembly);

    private static readonly string EditorConfig = File.ReadAllText(Path.Combine(AttributeReader.GetSolutionDirectory(typeof(Analysis).Assembly), ".editorconfig"));

    internal Analysis(IEnumerable<(string Path, string Source)> sources, IEnumerable<(string Path, string Source)> fixes, IEnumerable<DiagnosticResult> expected) {
        TestState.Sources.AddRange(sources.Select(static source => (source.Path, SourceText.From(source.Source))));
        FixedState.Sources.AddRange(fixes.Select(static source => (source.Path, SourceText.From(source.Source))));
        ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
        TestState.AdditionalReferences.Add(typeof(Fin<>).Assembly);
        TestState.AdditionalReferences.Add(typeof(UnionAttribute).Assembly);
        TestState.AdditionalReferences.Add(typeof(RhinoDoc).Assembly);
        TestState.AdditionalReferences.Add(typeof(Bitmap).Assembly);
        TestState.AdditionalReferences.Add(typeof(Application).Assembly);
        TestState.AdditionalReferences.Add(typeof(ILogger).Assembly);
        TestState.AnalyzerConfigFiles.Add(("/.editorconfig", EditorConfig));
        ExpectedDiagnostics.AddRange(expected);
        CodeActionValidationMode = CodeActionValidationMode.Full;
    }

    protected override CompilationOptions CreateCompilationOptions() =>
        ((CSharpCompilationOptions)base.CreateCompilationOptions()).WithNullableContextOptions(NullableContextOptions.Enable);

    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() => Analyzers;

    protected override IEnumerable<CodeFixProvider> GetCodeFixProviders() => Fixes;

    private static ImmutableArray<T> Exported<T, TAttribute>(Assembly assembly) where TAttribute : Attribute =>
        [.. assembly.GetExportedTypes().Where(static type => type.IsDefined(typeof(TAttribute), inherit: false)).Select(static type => (T)Activator.CreateInstance(type)!)];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Samples {
    private static readonly string Root = Path.Combine(AttributeReader.GetProjectDirectory(typeof(Samples).Assembly), nameof(Samples));

    public static TheoryData<string> Rules => [.. Directory.EnumerateFileSystemEntries(Root).Select(static path => Path.GetFileNameWithoutExtension(path)).Order(StringComparer.Ordinal)];

    [Theory]
    [MemberData(nameof(Rules))]
    public async Task Verified(string rule) {
        CancellationToken token = TestContext.Current.CancellationToken;
        string folder = Path.Combine(Root, rule);
        ImmutableArray<(string Path, string File)> files = Directory.Exists(folder)
            ? [.. Directory.EnumerateFiles(folder, "*.txt", SearchOption.AllDirectories).Order(StringComparer.Ordinal).Select(file => ($"/{Path.ChangeExtension(Path.GetRelativePath(folder, file), "cs").Replace('\\', '/')}", file))]
            : [($"/{rule}.cs", Path.Combine(Root, $"{rule}.txt"))];
        string[] texts = await Task.WhenAll(files.Select(file => File.ReadAllTextAsync(file.File, token)));
        ImmutableArray<(string Path, string Source, ImmutableDictionary<string, ImmutableArray<TextSpan>> Markers)> samples = [
            .. files.Zip(texts).Select(static file => {
                TestFileMarkupParser.GetSpans(file.Second, out string source, out ImmutableDictionary<string, ImmutableArray<TextSpan>> markers);
                return (file.First.Path, source, markers);
            }),
        ];
        ImmutableArray<(string Path, string Source)> sources = [.. samples.Select(static sample => (sample.Path, sample.Source))];
        DiagnosticDescriptor descriptor = Analysis.Analyzers.SelectMany(static analyzer => analyzer.SupportedDiagnostics).Single(held => string.Equals(held.Id, rule, StringComparison.Ordinal));
        DiagnosticResult[] expected = [
            .. from sample in samples
            from spans in sample.Markers.Values
            select new DiagnosticResult(descriptor)
                .WithSpan(new FileLinePositionSpan(sample.Path, SourceText.From(sample.Source).Lines.GetLinePositionSpan(spans.Single())))
                .WithMessage(null)
                .WithOptions(DiagnosticOptions.IgnoreAdditionalLocations),
        ];
        CodeAction? applied = null;
        _ = await Record.ExceptionAsync(() => new Analysis(sources, sources.Select(static source => (source.Path, "")), expected) {
            CodeFixTestBehaviors = CodeFixTestBehaviors.SkipFixAllCheck,
            CodeActionVerifier = (action, _) => applied = action,
        }.RunAsync(token));
        ImmutableArray<Document> documents = applied is null ? [] : [.. (await applied.GetOperationsAsync(token)).OfType<ApplyChangesOperation>().Single().ChangedSolution.Projects.Single().Documents];
        SourceText[] changed = await Task.WhenAll(documents.Select(document => document.GetTextAsync(token)));
        Option<ImmutableArray<(string Path, string Source)>> repaired = applied is null ? None : ImmutableArray.CreateRange(documents.Zip(changed, static (document, text) => (document.FilePath ?? document.Name, text.ToString())));
        string Marked(Location location) =>
            samples.Single(sample => string.Equals(sample.Path, location.SourceTree?.FilePath, StringComparison.Ordinal)).Markers.Single(marker => marker.Value.Single() == location.SourceSpan).Key;
        Atom<ImmutableSortedDictionary<(string Path, int Start), string>> reported = Atom(ImmutableSortedDictionary.Create<(string Path, int Start), string>(Comparer<(string Path, int Start)>.Create(
            static (left, right) => string.CompareOrdinal(left.Path, right.Path) is var order and not 0 ? order : left.Start.CompareTo(right.Start)), StringComparer.Ordinal));
        await new Analysis(sources, repaired.IfNone(sources), expected) {
            DiagnosticVerifier = (diagnostic, _, _) => _ = reported.Swap(held => held.SetItem(
                (diagnostic.Location.SourceTree?.FilePath ?? "", diagnostic.Location.SourceSpan.Start),
                $"{Marked(diagnostic.Location)} {diagnostic.GetMessage(CultureInfo.InvariantCulture)}\n")),
        }.RunAsync(token);
        Seq<Target> repairs = from fixes in repaired.ToSeq()
                              from fix in toSeq(fixes)
                              select new Target("txt", fix.Source, fixes.Length is 1 ? "fixed" : $"fixed-{Path.GetFileNameWithoutExtension(fix.Path)}");
        _ = await Verify(string.Concat(reported.Value.Values), repairs).UseDirectory("Snapshots").UseFileName(rule);
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
        return Gen.OneOf(
                new LiftValue("int", "1", IsError: false),
                new LiftValue("string", "string.Empty", IsError: false),
                new LiftValue("Unit", "unit", IsError: false),
                new LiftValue("Seq<int>", "Seq(1, 2)", IsError: false),
                new LiftValue("Option<int>", "Some(1)", IsError: false),
                new LiftValue("object", "new object()", IsError: false),
                new LiftValue("Error", """Error.New("held")""", IsError: true),
                new LiftValue("Expected", """new Expected("held", 1, None)""", IsError: false))
            .Select(
                Gen.OneOf(
                    new LiftForm(Fin, static (_, value) => $"Fin.Succ({value})", LiftCase.Success),
                    new LiftForm(Fin, static (_, value) => $"FinSucc({value})", LiftCase.Success),
                    new LiftForm(Fin, static (_, value) => $"Pure({value})", LiftCase.Success),
                    new LiftForm(Fin, static (type, value) => $"new Fin<{type}>.Succ({value})", LiftCase.Success),
                    new LiftForm(Fin, static (type, error) => $"Fin.Fail<{type}>({error})", LiftCase.Failure),
                    new LiftForm(Fin, static (type, error) => $"FinFail<{type}>({error})", LiftCase.Failure),
                    new LiftForm(Fin, static (_, error) => $"Fail({error})", LiftCase.Failure),
                    new LiftForm(Fin, static (type, error) => $"new Fin<{type}>.Fail({error})", LiftCase.Failure),
                    new LiftForm(Fin, static (type, _) => $"""Fin.Fail<{type}>("refused")""", LiftCase.Message),
                    new LiftForm(Validation, static (type, value) => $"Success<Error, {type}>({value})", LiftCase.Success),
                    new LiftForm(Validation, static (type, value) => $"Validation.Success<Error, {type}>({value})", LiftCase.Success),
                    new LiftForm(Validation, static (type, error) => $"Fail<Error, {type}>({error})", LiftCase.Failure),
                    new LiftForm(Validation, static (type, error) => $"Validation.Fail<Error, {type}>({error})", LiftCase.Failure)),
                Gen.OneOf(
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $"internal static {target} {name}() => {lift};"),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { return {{lift}}; }"""),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $"internal static readonly {target} {name} = {lift};"),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $$"""internal static {{target}} {{name}} { get; } = {{lift}};"""),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $"internal static {target} {name}(bool held) => held ? {lift} : {name}(false);"),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { return Take({{lift}}); static {{target}} Take({{target}} value) => value; }"""),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { {{target}} held; held = {{lift}}; return held; }"""),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $"internal static Func<{target}> {name}() => () => {lift};"),
                    new LiftPosition(FixesTarget: true, static (target, lift, name) => $"internal static Func<Task<{target}>> {name}() => async () => {lift};"),
                    new LiftPosition(FixesTarget: false, static (target, lift, name) => $$"""internal static {{target}} {{name}}() { var held = {{lift}}; return held; }"""),
                    new LiftPosition(FixesTarget: false, static (target, lift, name) => $"internal static {target} {name}() => Same({lift});"),
                    new LiftPosition(FixesTarget: false, static (target, lift, name) => $"internal static {target} {name}(bool held) => held ? {lift} : default;")))
            .Array[1, 6]
            .Select(static slots => {
                (string Source, string Fixed)[] members = [
                    .. slots.Select(static (slot, index) => {
                        (LiftValue value, LiftForm form, LiftPosition position) = slot;
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
            .SampleAsync(static sample => new Analysis([], [], []) { TestCode = sample.Source, FixedCode = sample.Fixed }.RunAsync(TestContext.Current.CancellationToken));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Snapshots {
    [ModuleInitializer]
    internal static void Initialize() => VerifierSettings.UseUtf8NoBom();
}

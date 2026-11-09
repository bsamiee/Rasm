using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.Execution;

namespace MsbuildAnalyzer;

// --- [MODELS] ---------------------------------------------------------------------------

/// <summary>A project reference, with the file whose evaluation declared it.</summary>
/// <param name="Root">Workspace-relative folder of the referenced project.</param>
/// <param name="Declared">Workspace-relative file that declares the reference.</param>
internal sealed record Reference(string Root, string Declared);

/// <summary>Facts one project's evaluations state, every path workspace-relative.</summary>
/// <param name="File">Project file.</param>
/// <param name="Name">MSBuild project name.</param>
/// <param name="Language">MSBuild language of the project.</param>
/// <param name="Test">Whether the project is a test project or a testing platform application.</param>
/// <param name="Packable">Whether the project packs to a NuGet package.</param>
/// <param name="Frameworks">Every target framework the project builds.</param>
/// <param name="Configurations">Configurations the project declares.</param>
/// <param name="Runtimes">Runtime identifiers the project states.</param>
/// <param name="References">Project references across every evaluation.</param>
/// <param name="Inputs">Files outside the project folder that its evaluation reads.</param>
/// <param name="Outputs">Output and intermediate folders across every configuration, runtime, and framework.</param>
/// <param name="Bases">Base output and intermediate folders.</param>
/// <param name="Restored">Restore-written import the evaluation reads when present.</param>
/// <param name="Results">Test results folder under the artifacts layout, where the dotnet test command writes it.</param>
internal sealed record Node(
    string File,
    string Name,
    string Language,
    bool Test,
    bool Packable,
    ImmutableArray<string> Frameworks,
    ImmutableArray<string> Configurations,
    ImmutableArray<string> Runtimes,
    ImmutableArray<Reference> References,
    ImmutableArray<string> Inputs,
    ImmutableArray<string> Outputs,
    ImmutableArray<string> Bases,
    string Restored,
    string? Results);

/// <summary>A project file whose evaluation failed.</summary>
/// <param name="File">Workspace-relative project file.</param>
/// <param name="Message">MSBuild's message for the failure.</param>
internal sealed record Failure(string File, string Message);

/// <summary>Every project of one workspace evaluation.</summary>
/// <param name="Nodes">Projects that evaluated.</param>
/// <param name="Failures">Projects that failed to evaluate.</param>
/// <param name="Evaluated">Workspace-relative files any evaluation read.</param>
internal sealed record Report(ImmutableArray<Node> Nodes, ImmutableArray<Failure> Failures, ImmutableArray<string> Evaluated);

// --- [OPERATIONS] -----------------------------------------------------------------------

internal static class Workspace {
    private static readonly ImmutableDictionary<string, string> Defaults = Seq("EnableDefaultEmbeddedResourceItems", "EnableDefaultNoneItems", "EnableDefaultContentItems")
        .Fold(ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase), static (held, name) => held.SetItem(name, "false"));

    private static readonly FrozenSet<string> Projects = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, ".csproj", ".fsproj", ".vbproj");
    private static readonly Seq<string> Linked = Seq("Compile", "AdditionalFiles", "EmbeddedResource", "Content", "GlobalAnalyzerConfigFiles");
    private static readonly Seq<string> Nearest = Seq("global.json", "Directory.Build.rsp");
    private static readonly Seq<string> Cascading = Seq("nuget.config", ".editorconfig");

    public static Report Analyze(string root, Seq<string> files) {
        EvaluationContext context = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared);
        using ProjectCollection collection = new();
        Seq<(string File, Fin<(Node Node, Seq<string> Evaluated)> Result)> read = files
            .Filter(static file => Projects.Contains(Path.GetExtension(file)))
            .Map(file => (file, Try.lift(() => Evaluate(root, file, context, collection)).Run()));
        return new Report(
            [.. read.Choose(static held => held.Result.ToOption().Map(static evaluated => evaluated.Node))],
            [.. read.Choose(static held => held.Result.Match(Succ: static _ => None, Fail: error => Some(new Failure(held.File, error.Message))))],
            [.. read.Bind(static held => held.Result.Match(Succ: static evaluated => evaluated.Evaluated, Fail: static _ => Seq<string>())).Distinct().Order(StringComparer.Ordinal)]);
    }

    private static (Node Node, Seq<string> Evaluated) Evaluate(string root, string file, EvaluationContext context, ProjectCollection collection) {
        string path = Path.GetFullPath(Path.Combine(root, file));
        string folder = Path.GetDirectoryName(path)!;
        ProjectInstance outer = Load(path, Defaults, context, collection);
        Seq<string> frameworks = Listed(outer, "TargetFrameworks");
        Seq<ProjectInstance> modes =
            from configuration in Listed(outer, "Configurations")
            from runtime in Seq(Option<string>.None) + Listed(outer, "RuntimeIdentifiers").Map(static runtime => Some(runtime))
            from framework in frameworks.IsEmpty ? Seq(Option<string>.None) : frameworks.Map(static framework => Some(framework))
            select Load(path, Mode(configuration, runtime, framework), context, collection);
        Seq<ProjectInstance> instances = Seq(outer) + modes;
        Seq<string> imports = instances
            .Bind(static instance => toSeq(instance.ImportPaths))
            .Filter(import => !Under(import, Path.GetFullPath(Path.Combine(folder, outer.GetPropertyValue("MSBuildProjectExtensionsPath")))));
        Seq<string> walked = Walk(root, folder);
        Seq<string> bases = Seq(outer.GetPropertyValue("BaseOutputPath"), outer.GetPropertyValue("BaseIntermediateOutputPath")).Map(at => Path.GetFullPath(Path.Combine(folder, at)));
        Seq<string> inside = imports.Map(import => Relative(root, import)).Filter(static relative => !relative.StartsWith("..", StringComparison.Ordinal));
        return (
            new Node(
                file.Replace('\\', '/'),
                outer.GetPropertyValue("MSBuildProjectName"),
                outer.GetPropertyValue("Language"),
                instances.Exists(static instance => Flag(instance, "IsTestProject") || Flag(instance, "IsTestingPlatformApplication")),
                Flag(outer, "IsPackable"),
                [.. frameworks.IsEmpty ? Seq(outer.GetPropertyValue("TargetFramework")) : frameworks],
                [.. Listed(outer, "Configurations")],
                [.. Listed(outer, "RuntimeIdentifiers")],
                [.. instances
                    .Bind(static instance => toSeq(instance.GetItems("ProjectReference")))
                    .Map(reference => new Reference(Relative(root, Path.GetDirectoryName(reference.GetMetadataValue("FullPath"))!), Relative(root, reference.GetMetadataValue("DefiningProjectFullPath"))))
                    .Distinct()],
                [.. imports
                    .Append(instances.Bind(static instance => Linked.Bind(type => toSeq(instance.GetItems(type)))).Map(static item => item.GetMetadataValue("FullPath")))
                    .Filter(input => File.Exists(input) && !Under(input, folder) && !bases.Exists(at => Under(input, at)))
                    .Map(input => Relative(root, input))
                    .Filter(static relative => !relative.StartsWith("..", StringComparison.Ordinal))
                    .Append(walked)
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                [.. modes
                    .Bind(static instance => Seq(instance.GetPropertyValue("OutputPath"), instance.GetPropertyValue("IntermediateOutputPath")))
                    .Map(output => Relative(root, Path.GetFullPath(Path.Combine(folder, output))))
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                [.. bases.Map(at => Relative(root, at)).Distinct()],
                Relative(root, Path.GetFullPath(Path.Combine(folder, outer.GetPropertyValue("MSBuildProjectExtensionsPath"), $"{outer.GetPropertyValue("MSBuildProjectFile")}.nuget.g.props"))),
                Flag(outer, "UseArtifactsOutput") ? Relative(root, Path.Combine(outer.GetPropertyValue("ArtifactsPath"), "test", outer.GetPropertyValue("ArtifactsProjectName"))) : null),
            Seq(file.Replace('\\', '/')) + inside + walked);
    }

    private static ProjectInstance Load(string path, ImmutableDictionary<string, string> properties, EvaluationContext context, ProjectCollection collection) =>
        ProjectInstance.FromFile(path, new ProjectOptions { GlobalProperties = properties, ProjectCollection = collection, EvaluationContext = context, LoadSettings = ProjectLoadSettings.DoNotEvaluateElementsWithFalseCondition });

    private static ImmutableDictionary<string, string> Mode(string configuration, Option<string> runtime, Option<string> framework) =>
        (Seq<(string Name, string Value)>(("Configuration", configuration)) + runtime.Map(static value => (Name: "RuntimeIdentifier", Value: value)).ToSeq() + framework.Map(static value => (Name: "TargetFramework", Value: value)).ToSeq())
            .Fold(Defaults, static (held, pair) => held.SetItem(pair.Name, pair.Value));

    private static Seq<string> Walk(string root, string folder) {
        Seq<string> entries = Ancestors(root, folder).Bind(static at => toSeq(Directory.EnumerateFiles(at)));
        Seq<string> Named(string name) => entries.Filter(entry => Path.GetFileName(entry).Equals(name, StringComparison.OrdinalIgnoreCase));
        return Nearest.Choose(name => Named(name).Head).Append(Cascading.Bind(Named)).Map(found => Relative(root, found));
    }

    private static Seq<string> Ancestors(string root, string folder) =>
        Path.GetRelativePath(root, folder) is "." ? Seq(folder) : Seq(folder) + Ancestors(root, Path.GetDirectoryName(folder)!);

    private static Seq<string> Listed(ProjectInstance instance, string property) =>
        toSeq(instance.GetPropertyValue(property).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static bool Flag(ProjectInstance instance, string property) =>
        bool.TryParse(instance.GetPropertyValue(property), out bool value) && value;

    private static bool Under(string path, string folder) =>
        Path.GetFullPath(path).StartsWith(Path.TrimEndingDirectorySeparator(folder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/').TrimEnd('/');
}

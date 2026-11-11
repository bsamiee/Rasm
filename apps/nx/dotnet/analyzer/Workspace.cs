using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.Execution;
using NuGet.ProjectModel;

namespace MsbuildAnalyzer;

// --- [MODELS] ---------------------------------------------------------------------------

internal sealed record Reference(string Root, string Declared);

internal sealed record Package(string Id, string Version, string Hash, bool Direct, ImmutableArray<string> Dependencies);

internal sealed record Tracking(string Shipped, string Unshipped);

internal sealed record Mode(string Configuration, string? Runtime, ImmutableArray<string> Outputs, ImmutableArray<string> Published, ImmutableArray<string> Reports);

internal sealed record Node(
    string File,
    string Name,
    string Language,
    bool Test,
    bool Packable,
    bool Publishable,
    string Configuration,
    ImmutableArray<Mode> Modes,
    ImmutableArray<Reference> References,
    ImmutableArray<string> Inputs,
    ImmutableArray<string> Bases,
    ImmutableArray<Package> Packages,
    ImmutableArray<string> Locks,
    ImmutableArray<string> Restored,
    ImmutableArray<string> Results,
    ImmutableArray<string> Platforms,
    ImmutableArray<string> Implicit,
    string Packed,
    string? Central,
    Tracking? Releases);

internal sealed record Failure(string File, string Message);

internal sealed record Report(ImmutableArray<Node> Nodes, ImmutableArray<Failure> Failures, ImmutableArray<string> Evaluated);

// --- [OPERATIONS] -----------------------------------------------------------------------

internal static class Workspace {
    public static readonly ImmutableDictionary<string, string> Defaults = Seq("EnableDefaultEmbeddedResourceItems", "EnableDefaultNoneItems", "EnableDefaultContentItems")
        .Fold(ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase), static (held, name) => held.SetItem(name, "false"));

    private static readonly FrozenSet<string> Projects = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, ".csproj", ".fsproj", ".vbproj");

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

    private static Fin<(Node Node, Seq<string> Evaluated)> Evaluate(string root, string file, EvaluationContext context, ProjectCollection collection) {
        string path = Path.GetFullPath(Path.Combine(root, file));
        if (Path.GetDirectoryName(path) is not { } folder) return Error.New($"{path} names no folder");
        ProjectInstance outer = Load(path, Defaults, context, collection);
        Seq<string> frameworks = Listed(outer, "TargetFrameworks");
        Seq<string> configurations = Listed(outer, "Configurations");
        Seq<(string Configuration, Option<string> Runtime, Seq<ProjectInstance> Instances)> modes =
            from configuration in configurations
            from runtime in Seq(Option<string>.None) + Listed(outer, "RuntimeIdentifiers").Map(static runtime => Some(runtime))
            select (configuration, runtime, (frameworks.IsEmpty ? Seq(Option<string>.None) : frameworks.Map(static framework => Some(framework))).Map(framework => Load(path, Properties(configuration, runtime, framework), context, collection)));
        Seq<ProjectInstance> instances = Seq(outer) + modes.Bind(static mode => mode.Instances);
        string Placed(string output) => Relative(root, Path.GetFullPath(Path.Combine(folder, output)));
        string Extensions(ProjectInstance instance) => Path.GetFullPath(Path.Combine(folder, instance.GetPropertyValue("MSBuildProjectExtensionsPath")));
        Seq<string> imports =
            from instance in instances
            from import in toSeq(instance.ImportPaths)
            where !Under(import, Extensions(instance))
            select import;
        Seq<string> walked = Walk(root, folder);
        Seq<string> bases = Seq(outer.GetPropertyValue("BaseOutputPath"), outer.GetPropertyValue("BaseIntermediateOutputPath")).Map(at => Path.GetFullPath(Path.Combine(folder, at)));
        Seq<string> inside = imports.Map(import => Relative(root, import)).Filter(static relative => !relative.StartsWith("..", StringComparison.Ordinal));
        Seq<string> locks = instances
            .Map(static instance => Path.GetFullPath(Path.Combine(instance.Directory, instance.GetPropertyValue("NuGetLockFilePath") is { Length: > 0 } named ? named : PackagesLockFileFormat.LockFileName)))
            .Distinct()
            .Filter(File.Exists);
        return (
            new Node(
                file.Replace('\\', '/'),
                outer.GetPropertyValue(Flag(outer, "IsPackable") ? "PackageId" : "MSBuildProjectName"),
                outer.GetPropertyValue("Language"),
                instances.Exists(static instance => Flag(instance, "IsTestProject") || Flag(instance, "IsTestingPlatformApplication")),
                Flag(outer, "IsPackable"),
                Flag(outer, "IsPublishable"),
                configurations.Find(configuration => configuration.Equals(outer.GetPropertyValue("Configuration"), StringComparison.OrdinalIgnoreCase)).IfNone(() => configurations.Head.IfNone(outer.GetPropertyValue("Configuration"))),
                [.. modes.Map(mode => new Mode(
                    mode.Configuration,
                    mode.Runtime.ValueUnsafe(),
                    [.. mode.Instances.Map(static instance => instance.GetPropertyValue("OutputPath")).Map(Placed).Distinct().Order(StringComparer.Ordinal)],
                    [.. mode.Instances.Filter(static instance => Flag(instance, "IsPublishable")).Map(static instance => instance.GetPropertyValue("PublishDir")).Map(Placed).Distinct().Order(StringComparer.Ordinal)],
                    [.. mode.Instances.Choose(static instance => toSeq(instance.GetPropertyValue("ErrorLog").Split([';', ','], StringSplitOptions.RemoveEmptyEntries)).Head).Map(Placed).Distinct().Order(StringComparer.Ordinal)]))],
                [.. instances
                    .Bind(static instance => toSeq(instance.GetItems("ProjectReference")))
                    .Choose(reference => Optional(Path.GetDirectoryName(reference.GetMetadataValue("FullPath"))).Map(referenced => new Reference(Relative(root, referenced), Relative(root, reference.GetMetadataValue("DefiningProjectFullPath")))))
                    .Distinct()],
                [.. imports
                    .Append(from instance in instances from type in Seq("Compile", "AdditionalFiles", "EmbeddedResource", "Content", "GlobalAnalyzerConfigFiles") from item in toSeq(instance.GetItems(type)) select item.GetMetadataValue("FullPath"))
                    .Filter(input => File.Exists(input) && !Under(input, folder) && !bases.Exists(at => Under(input, at)))
                    .Map(input => Relative(root, input))
                    .Filter(static relative => !relative.StartsWith("..", StringComparison.Ordinal))
                    .Append(walked)
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                [.. bases.Map(at => Relative(root, at)).Distinct()],
                [.. locks
                    .Bind(static locked => toSeq(PackagesLockFileFormat.Read(locked).Targets))
                    .Bind(static target => toSeq(target.Dependencies))
                    .Filter(static dependency => dependency.Type is not PackageDependencyType.Project)
                    .AsEnumerable()
                    .DistinctBy(static dependency => (dependency.Id, dependency.ResolvedVersion))
                    .Select(static dependency => new Package(dependency.Id, dependency.ResolvedVersion.ToNormalizedString(), dependency.ContentHash, dependency.Type is PackageDependencyType.Direct, [.. dependency.Dependencies.Select(static held => held.Id)]))],
                [.. locks.Map(locked => Relative(root, locked))],
                [.. instances.Map(Extensions).Distinct().Map(at => Relative(root, at))],
                [.. (Flag(outer, "UseArtifactsOutput")
                    ? Seq(Path.Combine(outer.GetPropertyValue("ArtifactsPath"), "test", outer.GetPropertyValue("ArtifactsProjectName")))
                    : instances.Exists(static instance => Flag(instance, "IsTestingPlatformApplication"))
                        ? instances.Map(static instance => instance.GetPropertyValue("TargetDir")).Filter(static target => target.Length > 0).Map(static target => Path.Combine(target, "TestResults"))
                        : Seq(Path.Combine(folder, "TestResults")))
                    .Map(results => Relative(root, results))
                    .Distinct()
                    .Order(StringComparer.Ordinal)],
                [.. toSeq(outer.GetItems("SupportedPlatform")).Map(static item => item.EvaluatedInclude).Distinct().Order(StringComparer.Ordinal)],
                [.. toSeq(outer.GetItems("NxImplicitDependency")).Map(static item => item.EvaluatedInclude).Distinct().Order(StringComparer.Ordinal)],
                Placed(modes.Find(static mode => mode.Runtime.IsNone && mode.Configuration.Equals("Release", StringComparison.OrdinalIgnoreCase)).Bind(static mode => mode.Instances.Head).IfNone(outer).GetPropertyValue("PackageOutputPath")),
                Flag(outer, "ManagePackageVersionsCentrally") ? Relative(root, outer.GetPropertyValue("DirectoryPackagesPropsPath")) : null,
                Tracked(root, instances)),
            Seq(file.Replace('\\', '/')) + inside + walked);
    }

    public static ProjectInstance Load(string path, ImmutableDictionary<string, string> properties, EvaluationContext context, ProjectCollection collection) =>
        ProjectInstance.FromFile(path, new ProjectOptions { GlobalProperties = properties, ProjectCollection = collection, EvaluationContext = context, LoadSettings = ProjectLoadSettings.DoNotEvaluateElementsWithFalseCondition });

    private static ImmutableDictionary<string, string> Properties(string configuration, Option<string> runtime, Option<string> framework) =>
        (Seq<(string Name, string Value)>(("Configuration", configuration)) + runtime.Map(static value => (Name: "RuntimeIdentifier", Value: value)).ToSeq() + framework.Map(static value => (Name: "TargetFramework", Value: value)).ToSeq())
            .Fold(Defaults, static (held, pair) => held.SetItem(pair.Name, pair.Value));

    private static Tracking? Tracked(string root, Seq<ProjectInstance> instances) {
        Seq<string> additional = instances.Bind(static instance => toSeq(instance.GetItems("AdditionalFiles"))).Map(static item => item.GetMetadataValue("FullPath")).Distinct();
        Option<string> Named(string name) => additional.Find(file => Path.GetFileName(file).Equals(name, StringComparison.OrdinalIgnoreCase)).Map(file => Relative(root, file));
        return (from shipped in Named("AnalyzerReleases.Shipped.md") from unshipped in Named("AnalyzerReleases.Unshipped.md") select new Tracking(shipped, unshipped)).ValueUnsafe();
    }

    private static Seq<string> Walk(string root, string folder) {
        Seq<string> entries = Ancestors(root, folder).Bind(static at => toSeq(Directory.EnumerateFiles(at)));
        Seq<string> Named(string name) => entries.Filter(entry => Path.GetFileName(entry).Equals(name, StringComparison.OrdinalIgnoreCase));
        return Seq("global.json", "Directory.Build.rsp").Choose(name => Named(name).Head).Append(Seq("nuget.config", ".editorconfig", ".globalconfig").Bind(Named)).Map(found => Relative(root, found));
    }

    private static Seq<string> Ancestors(string root, string folder) =>
        Path.GetRelativePath(root, folder) is "." ? Seq(folder) : Seq(folder) + Optional(Path.GetDirectoryName(folder)).ToSeq().Bind(parent => Ancestors(root, parent));

    private static Seq<string> Listed(ProjectInstance instance, string property) =>
        toSeq(instance.GetPropertyValue(property).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static bool Flag(ProjectInstance instance, string property) =>
        bool.TryParse(instance.GetPropertyValue(property), out bool value) && value;

    private static bool Under(string path, string folder) =>
        Path.GetFullPath(path).StartsWith(Path.TrimEndingDirectorySeparator(folder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/').TrimEnd('/');
}

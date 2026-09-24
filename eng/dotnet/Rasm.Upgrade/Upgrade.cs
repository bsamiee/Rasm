using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Rasm.Upgrade;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Evaluation(EvaluatedProperties Properties, EvaluatedItems Items);

internal sealed record EvaluatedProperties(string TargetFramework);

internal sealed record EvaluatedItems(ImmutableArray<ReferenceItem> PackageReference, ImmutableArray<VersionItem> PackageVersion);

internal sealed record ReferenceItem(string Identity);

internal sealed record VersionItem(string Identity, string Version, string DefiningProjectFullPath);

internal sealed record Project(NuGetFramework Framework, Seq<Row> Rows);

internal sealed record Row(string Id, NuGetVersion Version, string File);

internal sealed record Move(Row Row, NuGetVersion Version);

internal sealed record Feed(string Source, PackageMetadataResource Metadata);

[JsonSourceGenerationOptions(RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(Evaluation))]
internal sealed partial class EvaluationJson : JsonSerializerContext;

// --- [ERRORS] --------------------------------------------------------------------------
internal sealed record Usage() : Expected("Pass one argument, the project whose PackageReference items name the central rows to move", 1);

internal sealed record EvaluationFailed(string Project, string Output) : Expected($"dotnet msbuild could not evaluate {Project}: {string.Join(' ', Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))}", 2);

internal sealed record UnsupportedFramework(string TargetFramework) : Expected($"Target framework '{TargetFramework}' names no framework NuGet knows, set a known TargetFramework on the project", 3);

internal sealed record MalformedVersion(string Id, string Text, string File) : Expected($"{File} gives {Id} the Version '{Text}', which is neither a version nor a version range, correct the row", 4);

internal sealed record MissingRow(string Id, string File) : Expected($"{File} holds no PackageVersion element with a Version attribute for {Id}", 5);

internal sealed record MissingMetadata(string Source) : Expected($"Package source {Source} serves no package metadata resource, correct its URL in NuGet.config", 6);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Projects {
    // --- [EVALUATION]
    public static IO<Project> Evaluate(string path) =>
        (from process in use(() => new Process {
            StartInfo = new ProcessStartInfo("dotnet", ["msbuild", path, "-getProperty:TargetFramework", "-getItem:PackageReference", "-getItem:PackageVersion"]) {
                RedirectStandardOutput = true,
                UseShellExecute = false,
            },
        })
         from output in IO.liftAsync(async env => {
             _ = process.Start();
             string text = await process.StandardOutput.ReadToEndAsync(env.Token).ConfigureAwait(false);
             await process.WaitForExitAsync(env.Token).ConfigureAwait(false);
             return (process.ExitCode, Text: text);
         })
         from evaluation in IO.lift(() => (output.ExitCode == 0 ? Optional(JsonSerializer.Deserialize(output.Text, EvaluationJson.Default.Evaluation)) : None).ToFin(new EvaluationFailed(path, output.Text)))
         from project in IO.lift(Validated(evaluation).ToFin())
         select project).Bracket();

    // --- [VALIDATION]
    private static Validation<Error, Project> Validated(Evaluation evaluation) =>
        (Framework(evaluation.Properties.TargetFramework),
            toSeq(evaluation.Items.PackageVersion.IntersectBy(evaluation.Items.PackageReference.Select(static reference => reference.Identity), static item => item.Identity, StringComparer.OrdinalIgnoreCase))
                .Traverse(Parsed)
                .As())
            .Apply(static (framework, rows) => new Project(framework, rows.Somes()))
            .As();

    private static Validation<Error, NuGetFramework> Framework(string targetFramework) =>
        NuGetFramework.Parse(targetFramework) switch {
            { IsUnsupported: true } => new UnsupportedFramework(targetFramework),
            var framework => framework,
        };

    private static Validation<Error, Option<Row>> Parsed(VersionItem item) =>
        NuGetVersion.TryParse(item.Version, out NuGetVersion? version)
            ? Some(new Row(item.Identity, version, item.DefiningProjectFullPath))
            : VersionRange.TryParse(item.Version, allowFloating: true, out VersionRange? _)
                ? Option<Row>.None
                : new MalformedVersion(item.Identity, item.Version, item.DefiningProjectFullPath);
}

internal static class Feeds {
    // --- [SOURCES]
    public static IO<Seq<Feed>> Sources(ISettings settings) =>
        toSeq(SettingsUtility.GetEnabledSources(settings))
            .Traverse(static source =>
                from resource in IO.liftAsync(env => Repository.Factory.GetCoreV3(source).GetResourceAsync<PackageMetadataResource>(env.Token))
                from metadata in IO.lift(Optional(resource).ToFin(new MissingMetadata(source.Name)))
                select new Feed(source.Name, metadata))
            .As();

    // --- [METADATA]
    public static IO<Seq<IPackageSearchMetadata>> Published(SourceCacheContext cache, PackageSourceMapping mapping, Seq<Feed> feeds, string id) =>
        feeds
            .Filter(feed => !mapping.IsEnabled || mapping.GetConfiguredPackageSources(id).Contains(feed.Source, StringComparer.OrdinalIgnoreCase))
            .Traverse(feed => IO.liftAsync(env => feed.Metadata.GetMetadataAsync(id, includePrerelease: true, includeUnlisted: false, cache, NullLogger.Instance, env.Token)).Map(static published => toSeq(published)))
            .As()
            .Map(static published => published.Flatten());
}

internal static partial class Releases {
    // --- [SELECTION]
    public static Option<Move> Newest(NuGetFramework framework, Row row, Seq<IPackageSearchMetadata> published) =>
        Optional(published.Filter(metadata => Compatible(framework, metadata)).MaxBy(static metadata => (metadata.Identity.Version.Version, !metadata.Identity.Version.IsPrerelease, metadata.Published, metadata.Identity.Version)))
            .Map(static metadata => metadata.Identity.Version)
            .Filter(version => !VersionComparer.Default.Equals(version, row.Version))
            .Map(version => new Move(row, version));

    private static bool Compatible(NuGetFramework framework, IPackageSearchMetadata metadata) =>
        toSeq(metadata.DependencySets).Map(static set => set.TargetFramework) switch {
            { IsEmpty: true } => true,
            var frameworks => new FrameworkReducer().GetNearest(framework, frameworks) is not null,
        };

    // --- [REWRITE]
    public static Validation<Error, string> Rewrite(string file, string text, Seq<Move> moves) {
        Lst<string> lines = toList(text.Split('\n'));
        HashMap<string, (int Index, string Line, Match Attribute)> rows = toHashMap(
            toSeq(XDocument.Parse(text, LoadOptions.SetLineInfo).Descendants("PackageVersion")).Choose(element =>
                from include in Optional(element.Attribute("Include"))
                from version in Optional(element.Attribute("Version"))
                let at = (IXmlLineInfo)version
                from line in lines.At(at.LineNumber - 1)
                let attribute = VersionAttribute.Match(line, at.LinePosition - 1)
                where attribute.Success
                select (include.Value, (at.LineNumber - 1, line, attribute))));
        Option<int> column = toSeq(
                toSeq(rows.Values)
                    .Map(static row => row.Attribute.Groups["comment"])
                    .Filter(static comment => comment.Success)
                    .CountBy(static comment => comment.Index)
                    .OrderBy(static count => (count.Value, count.Key)))
            .Last
            .Map(static count => count.Key);
        return moves
            .Traverse(move => rows.Find(move.Row.Id).Map(row => (row.Index, Line: Spliced(row.Line, row.Attribute, move.Version, column))).ToValidation<Error>(new MissingRow(move.Row.Id, file)))
            .As()
            .Map(edits => string.Join('\n', edits.Fold(lines, static (current, edit) => current.SetItem(edit.Index, edit.Line))));
    }

    private static string Spliced(string line, Match attribute, NuGetVersion version, Option<int> column) =>
        Aligned(string.Concat(line.AsSpan(0, attribute.Groups["value"].Index), version.ToNormalizedString(), attribute.Groups["quote"].Value, attribute.Groups["close"].Value), attribute.Groups["comment"], column);

    private static string Aligned(string head, Group comment, Option<int> column) =>
        comment.Success ? string.Concat(head.PadRight(column.Filter(at => at > head.Length).IfNone(head.Length + 1)), comment.Value) : head;

    [GeneratedRegex("""\GVersion\s*=\s*(?<quote>["'])(?<value>[^"']*)\k<quote>(?<close>[^<]*?)\s*(?<comment><!--.*)?$""", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex VersionAttribute { get; }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    private const int MaximumConcurrentPackages = 32;

    public static int Main(string[] args) =>
        Upgrade(args).RunSafe().Match(
            Succ: Report,
            Fail: Report);

    private static IO<Seq<Move>> Upgrade(string[] args) =>
        from path in IO.lift(() => ProjectPath(args))
        from settings in IO.lift(() => Settings.LoadDefaultSettings(Path.GetDirectoryName(path)))
        from inputs in (Projects.Evaluate(path), Feeds.Sources(settings)).Apply(static (project, feeds) => (Project: project, Feeds: feeds)).As()
        from cache in use(static () => new SourceCacheContext { NoCache = true })
        let mapping = PackageSourceMapping.GetPackageSourceMapping(settings)
        from moves in toSeq(inputs.Project.Rows.Chunk(MaximumConcurrentPackages))
            .TraverseM(chunk => toSeq(chunk).Traverse(row => Feeds.Published(cache, mapping, inputs.Feeds, row.Id).Map(published => Releases.Newest(inputs.Project.Framework, row, published))).As())
            .As()
            .Map(static chunks => chunks.Flatten().Somes())
        from written in Write(moves)
        select moves;

    private static Fin<string> ProjectPath(string[] args) =>
        args is [var path] ? Path.GetFullPath(path) : new Usage();

    private static IO<Unit> Write(Seq<Move> moves) =>
        from documents in toSeq(moves.GroupBy(static move => move.Row.File, StringComparer.Ordinal))
            .Traverse(static file => IO.lift(() => (Path: file.Key, Text: File.ReadAllText(file.Key), Moves: toSeq(file))))
            .As()
        from rewritten in IO.lift(documents.Traverse(static document => Releases.Rewrite(document.Path, document.Text, document.Moves).Map(text => (document.Path, Text: text))).As().ToFin())
        from written in rewritten.TraverseM(static document => IO.lift(() => File.WriteAllText(document.Path, document.Text))).As()
        select unit;

    private static int Report(Seq<Move> moves) {
        _ = moves.Iter(static move => Console.WriteLine($"{move.Row.Id} {move.Row.Version.OriginalVersion} -> {move.Version.ToNormalizedString()}"));
        return 0;
    }

    private static int Report(Error error) {
        Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
        return 1;
    }
}

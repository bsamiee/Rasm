using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Darkroom.Chain;
using Darkroom.Output;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class WorkerPlatform {
    public static readonly WorkerPlatform MacOs = new("darwin", static script => ["-nosplash", $"-runscript={script}"]);
    public static readonly WorkerPlatform Windows = new("windows", static script => ["/nosplash", $"/runscript={script}"]);

    public static WorkerPlatform Current() => OperatingSystem.IsWindows() ? Windows : MacOs;

    [UseDelegateFromConstructor]
    public partial string[] Arguments(string script);
}

public sealed record FlamencoSettings(Uri Manager, WorkerPlatform Platform, int Priority, Option<string> Scripts, Option<Guid> WorkerTag);

internal sealed record FlamencoJob(string Name, string Type, string TypeEtag, int Priority, string SubmitterPlatform, FlamencoJob.Parameters Settings, Guid? WorkerTag) {
    internal sealed record Parameters(string Copy, string[] Arguments);
}

internal sealed record FlamencoReceipt(Guid Id);

internal sealed record FlamencoTypes(FlamencoTypes.JobType[] JobTypes) {
    internal sealed record JobType(string Name, string Etag);
}

[JsonSerializable(typeof(FlamencoJob))]
[JsonSerializable(typeof(FlamencoReceipt))]
[JsonSerializable(typeof(FlamencoTypes))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, RespectRequiredConstructorParameters = true, RespectNullableAnnotations = true)]
internal sealed partial class FlamencoContext : JsonSerializerContext;

internal sealed record QueueJob(int RhinoVersion, string Copy, Seq<QueueJob.Entry> Entries) {
    internal static readonly JsonTypeInfo<QueueJob> Info = ArchivableDictionaries.Info<QueueJob>();

    internal sealed record Entry(QueuedEntry Queued, string Folder, NamePart Stem, bool Overwrite, Seq<Output> Outputs);

    internal sealed record Output(SequenceNumber Frame, OutputTarget Target, Seq<NamePart> Scope, Option<OutputVersion> Version, Seq<NamePart> Parts);

    // --- [OUTBOUND]
    internal static QueueJob Of(RenderBatch batch, string copy) =>
        new(RhinoApp.ExeVersion, copy, [.. batch.Entries.Map(static entry => Entried(entry, entry.Outputs.Map(static output => new Output(
            output.Name.Number.IfNone(SequenceNumber.MinValue), output.Target, output.Name.Scope,
            output.Name.Version is OutputVersioning.Pinned pinned ? Some(pinned.Version) : None, output.Name.Parts))))]);

    internal static Entry Identity<TPath>(QueueEntry<TPath> entry) =>
        Entried(entry, entry.Outputs.Map(static output => (output.Target, output.Name.Scope, output.Name.Parts)).Distinct()
            .Map(static output => new Output(SequenceNumber.MinValue, output.Target, output.Scope, None, output.Parts)));

    private static Entry Entried<TPath>(QueueEntry<TPath> entry, Seq<Output> outputs) => new(entry.Queued, entry.Folder, entry.Stem, entry.Overwrite, [.. outputs]);

    // --- [INBOUND]
    internal IO<RenderBatch> Batch(RhinoDoc doc) =>
        from settings in RenderOverride.Current(doc)
        from entries in IO.lift(Entries.Map(static (entry, index) => (Entry: entry, Index: index))
            .Traverse(held => Planned(held.Entry, settings).MapFail(error => new DarkroomFault("render queue job entry", DarkroomReason.Invalid, held.Index, error)))
            .As()
            .Bind(static entries => Conversions.Unique(entries, static entry => entry.Key)))
        from batch in RenderBatch.Placed(doc, entries, overwrite: true)
        select batch;

    private static Fin<QueueEntry<Unit>> Planned(Entry entry, RenderSettings settings) =>
        from frames in FrameSpan.Of(entry.Queued.Sun)
        from outputs in toSeq(entry.Outputs).Map(static (output, index) => (Output: output, Index: index)).Traverse(held =>
                DarkroomFault.Unless((int)held.Output.Frame < frames.Numbers.Count, DarkroomReason.Beyond, (held.Output.Frame, frames.Numbers.Count), held.Index)
                    .Map(_ => new QueueOutput<Unit>(held.Output.Target, new OutputName(
                        held.Output.Scope,
                        held.Output.Version.Match<OutputVersioning>(Some: static pinned => new OutputVersioning.Pinned(pinned), None: static () => new OutputVersioning.Unversioned()),
                        held.Output.Parts, frames.Named(held.Output.Frame), held.Output.Target.FileFormat.Extension), unit)))
            .As()
        select new QueueEntry<Unit>(entry.Queued, new RenderOverride(settings, entry.Queued.Clay), entry.Folder, entry.Stem, entry.Overwrite, frames, outputs);

    internal static IO<QueueJob> Read(string path) =>
        from found in FilePublication.Read(path, Info, static job => Fin.Succ(job))
        from job in IO.lift(found.ToFin(new DarkroomFault(path, DarkroomReason.Missing)))
        select job;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class PackageFile {
    private static readonly JsonTypeInfo<JsonNode> Node = ArchivableDictionaries.Info<JsonNode>();

    public static readonly PackageFile Copy = new(FileExtension.Create(".3dm"), None, static (doc, _, path, _) =>
        Exchange.WriteFile(doc, path, static options => (options.IncludeBitmapTable, options.IncludeRenderMeshes) = (true, true)));
    public static readonly PackageFile Job = new(FileExtension.Create(".json"), None, static (_, batch, path, copy) =>
        FilePublication.Write(path, QueueJob.Of(batch, copy), QueueJob.Info));
    public static readonly PackageFile Schema = new(FileExtension.Create(".json"), Some(NamePart.Create(nameof(Schema))), static (_, _, path, _) =>
        FilePublication.Write(path, QueueJob.Info.GetJsonSchemaAsNode(), Node));

    public FileExtension Extension { get; }

    private readonly Option<NamePart> part;

    [UseDelegateFromConstructor]
    public partial IO<Unit> Write(RhinoDoc doc, RenderBatch batch, OutputPath path, OutputPath copy);

    public OutputName Name(NamePart stamp) => new(Seq(stamp), new OutputVersioning.Unversioned(), part.ToSeq(), None, Extension);
}

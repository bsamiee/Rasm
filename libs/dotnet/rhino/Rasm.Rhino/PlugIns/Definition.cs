using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns.Licenses;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;

namespace Rasm.Rhino.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComputeEndpoint(string Path, Type Type);

public sealed record PlugInDefinition {
    public Option<PlugInLoadTime> LoadTime { get; init; }
    public Option<LicenseDefinition> License { get; init; }
    public Seq<Guid> Dependencies { get; init; }
    public Seq<PlugInSetting> Settings { get; init; }
    public Func<ISettingReader, IO<HistoryLimits>> History { get; init; } = static _ => IO.pure(HistoryLimits.Default);
    public required TimeProvider Clock { get; init; }
    public Seq<Func<IPlugInSink, Command>> Commands { get; init; }
    public Seq<View> Views { get; init; }
    public Seq<Func<IPlugInViews, Action<IDisposable>, IO<Unit>>> Acquisitions { get; init; }
    public Seq<ComputeEndpoint> Endpoints { get; init; }
    public Seq<DocumentFrame> DocumentFrames { get; init; }
    public Option<HelpTopic> Help { get; init; }
    public Option<object> Published { get; init; }
}

public sealed record FileFormat<TOptions>(
    string Description,
    IterableNE<FileExtension> Extensions,
    Option<View.Dialog> Options,
    Func<string, RhinoDoc, TOptions, IO<Unit>> Run) where TOptions : class;

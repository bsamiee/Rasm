using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Plugin.Licenses;
using Rasm.Rhino.Render.Content;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;

namespace Rasm.Rhino.Plugin;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComputeEndpoint(string Path, Type Type);

public sealed record PlugInDefinition {
    public Option<PlugInLoadTime> LoadTime { get; init; }
    public Option<LicenseDefinition> License { get; init; }
    public Seq<Guid> Dependencies { get; init; }
    public Seq<PlugInSetting> Settings { get; init; }
    public Option<(PlugInSetting<HistoryCapacity, int, InvalidRhinoValue> Capacity, PlugInSetting<MergeWindow, int, InvalidRhinoValue> Window)> History { get; init; }
    public TimeProvider Clock { get; init; } = TimeProvider.System;
    public Seq<Func<IPlugInSink, Command>> Commands { get; init; }
    public Seq<View> Views { get; init; }
    public Seq<EffectKind> Effects { get; init; }
    public Seq<Func<PlugIn, IPlugInSink, IO<IDisposable>>> Acquisitions { get; init; }
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

public sealed record RendererDefinition {
    // --- [ROWS]
    public required RenderEngine Engine { get; init; }
    public LanguageExt.HashSet<RenderPlugIn.RenderFeature> Features { get; init; }
    public Seq<(string Description, OutputTarget Target)> SaveTypes { get; init; }
    public Seq<View.Tab> Tabs { get; init; }
    public Seq<View.Pane> Panes { get; init; }
    public Seq<View.Section> SettingsSections { get; init; }
    public Seq<View.Section> SunSections { get; init; }
    public Option<View.DocumentPage> RenderPage { get; init; }
    public Option<ContentPreviewRow> Preview { get; init; }

    // --- [DERIVED]
    public Seq<View> Views => [.. Tabs, .. Panes, .. SettingsSections, .. SunSections];
}

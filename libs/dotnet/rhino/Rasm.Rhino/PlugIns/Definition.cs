using Rasm.Rhino.Document.Files;
using Rasm.Rhino.PlugIns.Licenses;
using Rasm.Rhino.Render.Content;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComputeEndpoint(string Path, Type Type);

public sealed record PlugInDefinition {
    public Option<PlugInLoadTime> LoadTime { get; init; }
    public Option<LicenseDefinition> License { get; init; }
    public Seq<Guid> Dependencies { get; init; }
    public required TimeProvider Clock { get; init; }
    public Seq<Action<ILoggingBuilder>> Logging { get; init; }
    public Seq<Func<Command>> Commands { get; init; }
    public Seq<View> Views { get; init; }
    public Seq<Func<IPlugInViews, IO<Unit>>> Acquisitions { get; init; }
    public Seq<ComputeEndpoint> Endpoints { get; init; }
    public Seq<DocumentFrame> DocumentFrames { get; init; }
    public Option<HelpTopic> Help { get; init; }
    public Option<object> Published { get; init; }
}

public sealed record SaveType(string Extension, string Description, Func<RenderWindow, string, bool, IO<Unit>> Write);

public sealed record RendererDefinition {
    // --- [ROWS]
    public required RenderEngine Engine { get; init; }
    public HashSet<RenderPlugIn.RenderFeature> Features { get; init; }
    public Seq<SaveType> SaveTypes { get; init; }
    public Seq<View.Tab> Tabs { get; init; }
    public Seq<View.Pane> Panes { get; init; }
    public Seq<View.Section> SettingsSections { get; init; }
    public Seq<View.Section> SunSections { get; init; }
    public Option<View.DocumentPage> RenderPage { get; init; }
    public Option<ContentPreviewRow> Preview { get; init; }

    // --- [DERIVED]
    public Seq<View> Views => [.. Tabs, .. Panes, .. SettingsSections, .. SunSections];
}

public sealed record FileFormat<TOptions>(
    string Description,
    IterableNE<FileExtension> Extensions,
    Option<View.Dialog> Options,
    Func<string, RhinoDoc, TOptions, IO<Unit>> Run) where TOptions : class;

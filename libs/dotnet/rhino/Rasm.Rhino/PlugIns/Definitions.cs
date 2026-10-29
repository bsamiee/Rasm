using Eto.Forms;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.PlugIns.Licenses;
using Rasm.Rhino.Render.Content;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ComputeEndpoint(string Path, Type Type);

public sealed record PlugInDefinition {
    public required TimeProvider Clock { get; init; }
    public Option<PlugInLoadTime> LoadTime { get; init; }
    public Option<LicenseDefinition> License { get; init; }
    public Seq<Guid> Dependencies { get; init; }
    public Seq<Action<ILoggingBuilder>> Logging { get; init; }
    public Seq<PlugInCommand> Commands { get; init; }
    public Seq<View> Views { get; init; }
    public Seq<IO<Unit>> Load { get; init; }
    public Seq<ComputeEndpoint> Endpoints { get; init; }
    public Seq<DocumentFrame> Frames { get; init; }
    public Option<HelpTopic> Help { get; init; }
    public Option<object> Published { get; init; }
}

public sealed record FileFormat<TOptions>(
    string Description,
    IterableNE<FileExtension> Extensions,
    Option<View.Dialog> Options,
    Func<string, RhinoDoc, TOptions, IO<Unit>> Run) where TOptions : class;

public sealed record FileDefinition<TOptions> where TOptions : class {
    public required PlugInDefinition PlugIn { get; init; }
    public required IterableNE<FileFormat<TOptions>> Formats { get; init; }
}

public sealed record SaveType(string Extension, string Description, Func<RenderWindow, string, bool, IO<Unit>> Write);

public sealed record RendererDefinition {
    public required PlugInDefinition PlugIn { get; init; }
    public required RenderEngine Engine { get; init; }
    public HashSet<RenderPlugIn.RenderFeature> Features { get; init; }
    public Seq<SaveType> SaveTypes { get; init; }
    public Seq<View.Tab> Tabs { get; init; }
    public Seq<View.Pane> Panes { get; init; }
    public Seq<View.Section> SettingsSections { get; init; }
    public Seq<View.Section> SunSections { get; init; }
    public Option<View.Page> RenderPage { get; init; }
    public Option<ContentPreviewRow> Preview { get; init; }
}

public sealed record SkinDefinition {
    public Seq<Action<ILoggingBuilder>> Logging { get; init; }
    public Option<string> ApplicationName { get; init; }
    public Option<IO<Control>> Splash { get; init; }
    public Option<IO<Control>> About { get; init; }
    public Option<string> TemplateFolder { get; init; }
    public Option<IO<Seq<string>>> Recent { get; init; }
    public Option<IO<Unit>> MainFrameCreated { get; init; }
    public Option<IO<Unit>> LicenseChecked { get; init; }
    public Option<IO<Unit>> CommandsRegistered { get; init; }
}

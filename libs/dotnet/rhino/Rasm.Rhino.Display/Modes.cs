using Rasm.Rhino.Document;
using Rhino.Display;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ModeState(
    Guid Id,
    Option<string> EnglishName,
    Option<string> LocalName,
    bool InMenu,
    bool SupportsShadeCommand,
    bool SupportsShading,
    bool AllowObjectAssignment,
    bool ShadedPipelineRequired,
    bool WireframePipelineRequired,
    bool PipelineLocked);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayModes {
    // --- [TABLE]
    public static IO<Seq<ModeState>> GetDisplayModes() =>
        TableOps.ReadRows(DisplayModeDescription.GetDisplayModes, static mode => IO.lift(() => ModeMapper.ToState(mode)));

    public static IO<Option<Guid>> Named(string name) =>
        Identified(IO.lift(() => Optional(DisplayModeDescription.FindByName(name))));

    public static IO<Unit> Edit(Guid id, Func<DisplayModeDescription, IO<Unit>> edit) =>
        Viewports.WithMode(id, copy =>
            from edited in edit(copy)
            from updated in IO.lift(() => Refused.Unless(DisplayModeDescription.UpdateDisplayMode(copy), nameof(DisplayModeDescription.UpdateDisplayMode)))
            select updated);

    public static IO<Guid> Add(string name) =>
        IO.lift(() => Answers.NonEmpty(DisplayModeDescription.AddDisplayMode(name), nameof(DisplayModeDescription.AddDisplayMode)));

    public static IO<Guid> Copy(Guid source, string name) =>
        IO.lift(() => Answers.NonEmpty(DisplayModeDescription.CopyDisplayMode(source, name), nameof(DisplayModeDescription.CopyDisplayMode)));

    public static IO<Guid> Import(string path, bool interactive) =>
        IO.lift(() => Answers.NonEmpty(DisplayModeDescription.ImportFromFile(path, interactive), nameof(DisplayModeDescription.ImportFromFile)));

    public static IO<Unit> Delete(Guid id) =>
        IO.lift(() => Refused.Unless(DisplayModeDescription.DeleteDisplayMode(id), nameof(DisplayModeDescription.DeleteDisplayMode)));

    public static IO<Unit> Export(Guid id, string path) =>
        Viewports.WithMode(id, mode => IO.lift(() => Refused.Unless(DisplayModeDescription.ExportToFile(mode, path), nameof(DisplayModeDescription.ExportToFile))));

    private static IO<Option<Guid>> Identified(IO<Option<DisplayModeDescription>> found) =>
        found.Bind(static copy => copy.Traverse(static mode => Disposal.Using(() => mode, static staged => IO.pure(staged.Id))).As());

    // --- [VIEWPORT]
    public static IO<Unit> Assign(RhinoViewport viewport, Guid modeId) =>
        Viewports.WithMode(modeId, mode => IO.lift(() => { viewport.DisplayMode = mode; }));

    public static IO<Option<Guid>> Current(RhinoViewport viewport) =>
        Identified(IO.lift(() => Optional(viewport.DisplayMode)));
}

[Mapper]
internal static partial class ModeMapper {
    internal static partial ModeState ToState(DisplayModeDescription mode);
}

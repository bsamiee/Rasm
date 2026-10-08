using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Riok.Mapperly.Abstractions;

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
[Mapper]
internal static partial class ModeMapper {
    internal static partial ModeState ToState(DisplayModeDescription mode);
}

public static class DisplayModes {
    // --- [IDENTITY]
    public static Guid ConceptId { get; } = new("881f20dd-a78e-4930-a7c3-690e5e6b0927");

    // --- [TABLE]
    public static IO<Seq<ModeState>> GetDisplayModes() =>
        IO.lift(static () => Conversions.Rows(DisplayModeDescription.GetDisplayModes()))
            .Bracket(Use: static modes => IO.lift(() => modes.Map(ModeMapper.ToState).Strict()), Fin: DisposalOps.Release);

    public static IO<Guid> FindByName(string englishName) =>
        use(IO.lift(() => Missing.Unless(DisplayModeDescription.FindByName(englishName), nameof(DisplayModeDescription.FindByName))))
            .Map(static mode => mode.Id)
            .Bracket();

    public static IO<Guid> AddDisplayMode(string name) =>
        Saved(IO.lift(() => Conversions.Required(DisplayModeDescription.AddDisplayMode(name), nameof(DisplayModeDescription.AddDisplayMode))));

    public static IO<Guid> CopyDisplayMode(Guid source, string name) =>
        Saved(IO.lift(() => Conversions.Required(DisplayModeDescription.CopyDisplayMode(source, name), nameof(DisplayModeDescription.CopyDisplayMode))));

    public static IO<TValue> UpdateDisplayMode<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> edit) =>
        Viewports.WithMode(id, copy =>
            from value in edit(copy)
            from updated in IO.lift(() => Refused.Unless(DisplayModeDescription.UpdateDisplayMode(copy), nameof(DisplayModeDescription.UpdateDisplayMode)))
            select value);

    public static IO<Unit> DeleteDisplayMode(Guid id) =>
        Saved(
            from removed in Removed(id)
            from forgotten in SettingRoots.DeleteChild(SettingsNode.Options.Child("DisplayAttributesManager"), id.ToString())
            select forgotten);

    public static IO<Guid> ImportFromFile(string path, bool interactive) =>
        Saved(IO.lift(() => Exchange.ExistingPath(path)).Bind(existing => Imported(existing, interactive)));

    public static IO<Guid> Install(string path) =>
        Saved(
            from existing in IO.lift(() => Exchange.ExistingPath(path))
            from staged in Imported(existing, interactive: false)
            from removed in Removed(staged)
            from installed in Imported(existing, interactive: false)
            select installed);

    public static IO<Unit> ExportToFile(Guid id, string path) =>
        from target in IO.lift(Exchange.QualifiedPath(path))
        from exported in Viewports.WithMode(id, mode => IO.lift(() => Refused.Unless(DisplayModeDescription.ExportToFile(mode, target), nameof(DisplayModeDescription.ExportToFile))))
        select exported;

    private static IO<Guid> Imported(string path, bool interactive) =>
        IO.lift(() => Conversions.Required(DisplayModeDescription.ImportFromFile(path, interactive), nameof(DisplayModeDescription.ImportFromFile)));

    private static IO<Unit> Removed(Guid id) =>
        IO.lift(() => Refused.Unless(DisplayModeDescription.DeleteDisplayMode(id), nameof(DisplayModeDescription.DeleteDisplayMode)));

    private static IO<T> Saved<T>(IO<T> change) =>
        from value in change
        from saved in IO.lift(DisplayModeDescription.SaveDisplayModes)
        select value;

    // --- [VIEWPORT]
    public static IO<Guid> DisplayMode(RhinoDoc doc, ViewportTarget target) =>
        use(Viewports.ResolveViewport(doc, target))
            .Bind(static row => use(IO.lift(() => Missing.Unless(row.Viewport.DisplayMode, nameof(RhinoViewport.DisplayMode)))).Map(static mode => mode.Id).Bracket())
            .Bracket();

    public static IO<Seq<bool>> SetDisplayMode(RhinoDoc doc, ViewportSet viewports, Guid id, RedrawPolicy redraw) =>
        Viewports.WithMode(id, mode => Navigation.ApplyToViewports(doc, viewports, viewport => {
            viewport.DisplayMode = mode;
            return unit;
        }, redraw));

    // --- [VARIANTS]
    public static IO<Bitmap> CaptureToBitmap(RhinoView view, Guid id, Option<Size> pixels, Func<DisplayPipelineAttributes, IO<Unit>> edit) =>
        Viewports.WithMode(id, mode =>
            from edited in edit(mode.DisplayAttributes)
            from raster in IO.lift(() => Missing.Unless(
                pixels.Match(Some: size => view.CaptureToBitmap(size, mode.DisplayAttributes), None: () => view.CaptureToBitmap(mode.DisplayAttributes)),
                nameof(RhinoView.CaptureToBitmap)))
            select raster);
}

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
    public static IO<Seq<ModeState>> GetDisplayModes() => ReadModes(static modes => Fin.Succ(modes.Map(ModeMapper.ToState).Strict()));

    public static IO<Guid> FindByName(string englishName) =>
        ReadModes(modes => modes.Find(mode => string.Equals(mode.EnglishName, englishName, StringComparison.OrdinalIgnoreCase))
            .Map(static mode => mode.Id).ToFin(new Missing(nameof(DisplayModeDescription.FindByName))));

    public static IO<Guid> AddDisplayMode(string name, Option<Guid> source = default) =>
        Saved(IO.lift(() => source.Match(
            Some: id => Conversions.Required(DisplayModeDescription.CopyDisplayMode(id, name), nameof(DisplayModeDescription.CopyDisplayMode)),
            None: () => Conversions.Required(DisplayModeDescription.AddDisplayMode(name), nameof(DisplayModeDescription.AddDisplayMode)))));

    public static IO<TValue> UpdateDisplayMode<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> edit) =>
        Viewports.WithMode(id, copy => from value in edit(copy)
                                       from updated in IO.lift(() => Refused.Unless(DisplayModeDescription.UpdateDisplayMode(copy), nameof(DisplayModeDescription.UpdateDisplayMode)))
                                       select value);

    public static IO<Unit> DeleteDisplayMode(Guid id) =>
        Saved(
            from removed in Removed(id)
            from forgotten in SettingRoots.DeleteChild(SettingsNode.DisplayModes, id.ToString())
            select forgotten);

    public static IO<Guid> ImportFromFile(string path, bool interactive, bool replace = false) =>
        Saved(
            from existing in IO.lift(() => Exchange.ExistingPath(path))
            let import = IO.lift(() => Conversions.Required(DisplayModeDescription.ImportFromFile(existing, interactive), nameof(DisplayModeDescription.ImportFromFile)))
            from staged in import
            from installed in replace ? from removed in Removed(staged) from fresh in import select fresh : IO.pure(staged)
            select installed);

    public static IO<Unit> ExportToFile(Guid id, string path) =>
        from target in IO.lift(Exchange.QualifiedPath(path))
        from exported in Viewports.WithMode(id, mode => IO.lift(() => Refused.Unless(DisplayModeDescription.ExportToFile(mode, target), nameof(DisplayModeDescription.ExportToFile))))
        select exported;

    private static IO<T> ReadModes<T>(Func<Seq<DisplayModeDescription>, Fin<T>> read) =>
        IO.lift(static () => Conversions.Rows(DisplayModeDescription.GetDisplayModes()))
            .Bracket(Use: modes => IO.lift(() => read(modes)), Fin: DisposalOps.Release);

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
        Viewports.WithMode(id, mode => Navigation.ApplyToViewports(doc, viewports, viewport => IO.lift(() => { viewport.DisplayMode = mode; }), redraw));

    // --- [VARIANTS]
    public static IO<Bitmap> CaptureToBitmap(RhinoView view, Guid id, Option<Size> pixels, Func<DisplayPipelineAttributes, IO<Unit>> edit) =>
        Viewports.WithMode(id, mode => from edited in edit(mode.DisplayAttributes)
                                       let size = pixels.IfNone(() => view.ClientRectangle.Size)
                                       from raster in IO.lift(() => Missing.Unless(view.CaptureToBitmap(size, mode.DisplayAttributes), nameof(RhinoView.CaptureToBitmap)))
                                       select raster);
}

using System.Drawing;
using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;

namespace Rasm.Rhino.Viewport.Printing;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CaptureRequest(double Density, Seq<Step<ViewCaptureSettings>> Steps) {
    public Option<Size> Media { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewCaptures {
    // --- [SETTINGS]
    private static readonly Stage<ViewCaptureSettings> Scratch = new(
        Copy: IO.pure,
        Unchanged: static (_, _) => false,
        Commit: static (_, staged) => Products.Accepted(() => staged.IsValid).Map(_ => staged));

    public static IO<ViewCaptureSettings> Settings(ViewportRef row, CaptureRequest request) =>
        from settings in Products.Of(() => row.View is RhinoPageView page && row.Viewport.ViewportType is ViewportType.PageViewMainViewport
            ? new ViewCaptureSettings(page, request.Density)
            : new ViewCaptureSettings(row.View, request.Media.IfNone(() => row.Viewport.Size), request.Density))
        from bound in IO.lift(() => {
            if (row.Viewport.ViewportType is ViewportType.DetailViewport || row.Viewport.ParentView is null)
                settings.SetViewport(row.Viewport);
            if (row.Viewport.ParentView is null)
                settings.Document = row.View.Document;
        })
        from applied in Edits.Apply(Scratch, settings, request.Steps)
        select applied;

    // --- [STEPS]
    public static Step<ViewCaptureSettings> Window(Point2d first, Point2d second) =>
        Steps.Of<ViewCaptureSettings>(settings => {
            settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
            settings.SetWindowRect(first, second);
            return true;
        });

    public static Step<ViewCaptureSettings> Window(Point3d first, Point3d second) =>
        Steps.Of<ViewCaptureSettings>(settings => {
            settings.ViewArea = ViewCaptureSettings.ViewAreaMapping.Window;
            settings.SetWindowRect(first, second);
            return true;
        });

    public static Step<ViewCaptureSettings> Margins(PrinterMargins inset) =>
        Steps.Of<ViewCaptureSettings>(settings => settings.SetMargins(UnitSystem.Millimeters, Millimeters(inset.Left), Millimeters(inset.Top), Millimeters(inset.Right), Millimeters(inset.Bottom)));

    public static Step<ViewCaptureSettings> Offset(ViewCaptureSettings.AnchorLocation anchor, bool fromMargin, Length x, Length y) =>
        Steps.Of<ViewCaptureSettings>(settings => {
            settings.OffsetAnchor = anchor;
            settings.SetOffset(UnitSystem.Millimeters, fromMargin, Millimeters(x), Millimeters(y));
            return true;
        });

    public static Step<ViewCaptureSettings> Scaled(DrawingScale scale) =>
        Steps.Of<ViewCaptureSettings>(settings => {
            settings.SetModelScaleToValue(Millimeters(scale.ToModel(Length.FromMillimeters(1))));
            return true;
        });

    private static double Millimeters(Length length) => length.Millimeters.ToDouble();

    // --- [SCOPE]
    public static IO<T> Clean<T>(RhinoDoc doc, ViewportSet views, IO<T> body) =>
        (from selected in ObjectTarget.Selection(doc)
         from rows in Viewports.Resolve(doc, views)
         from overlays in IO.lift(() => rows.Bind(Overlays).Strict())
         from pages in IO.lift(() => toSeq(rows.Choose(static row => Optional(row.View as RhinoPageView)).DistinctBy(static page => page.RuntimeSerialNumber))
             .Map(static page => (Page: page, Active: Conversions.Present(page.ActiveDetailId))).Strict())
         let restores = overlays.Map(static overlay => overlay.Restore)
             + pages.Map(static held => held.Active.Match(Some: id => Products.Accepted(() => held.Page.SetActiveDetail(id)), None: () => IO.lift(held.Page.SetPageAsActive)))
             + Seq(ObjectTarget.Reselected(selected))
         from value in Products.Each(overlays.Map(static overlay => overlay.Clear) + pages.Map(static held => IO.lift(held.Page.SetPageAsActive)) + Seq(IO.lift(() => ignore(doc.Objects.UnselectAll()))))
             .Bind(_ => body)
             .Finally(Products.Each(restores).Map(unit))
         select value).Bracket();

    private static Seq<(IO<Unit> Clear, IO<Unit> Restore)> Overlays(ViewportRef row) =>
        Seq(
            Overlay(() => row.Viewport.ConstructionGridVisible, value => row.Viewport.ConstructionGridVisible = value),
            Overlay(() => row.Viewport.ConstructionAxesVisible, value => row.Viewport.ConstructionAxesVisible = value),
            Overlay(() => row.Viewport.WorldAxesVisible, value => row.Viewport.WorldAxesVisible = value));

    private static (IO<Unit> Clear, IO<Unit> Restore) Overlay(Func<bool> read, Action<bool> write) =>
        read() switch {
            var held => (IO.lift(() => write(false)), IO.lift(() => write(held))),
        };
}

using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Rasm.Rhino.Commands;
using DetailViewGeometry = Rhino.Geometry.DetailView;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FileSheetSize(UnitSystem Units, Option<double> Width = default, Option<double> Height = default) {
    internal Fin<Option<(double Width, double Height)>> Create(RhinoDoc document, Op op) {
        FileSheetSize self = this;
        return (self.Width.Case, self.Height.Case) switch {
            (double w, double h) => Resolve(value: (Width: w, Height: h), units: self.Units, document: document, op: op).Map(value => Some(value: value)),
            (double, _) or (_, double) => Fin.Fail<Option<(double Width, double Height)>>(error: op.InvalidInput()),
            _ => Fin.Succ(value: Option<(double Width, double Height)>.None),
        };
    }

    internal Fin<(Option<double> Width, Option<double> Height)> Resize(RhinoDoc document, Op op) {
        FileSheetSize self = this;
        return from resolvedWidth in self.Width.Case switch {
            double value => Resolve(value: value, units: self.Units, document: document, op: op).Map(value => Some(value: value)),
            _ => Fin.Succ(value: Option<double>.None),
        }
               from resolvedHeight in self.Height.Case switch {
                   double value => Resolve(value: value, units: self.Units, document: document, op: op).Map(value => Some(value: value)),
                   _ => Fin.Succ(value: Option<double>.None),
               }
               select (Width: resolvedWidth, Height: resolvedHeight);
    }

    private static Fin<(double Width, double Height)> Resolve((double Width, double Height) value, UnitSystem units, RhinoDoc document, Op op) =>
        from resolvedWidth in Resolve(value: value.Width, units: units, document: document, op: op)
        from resolvedHeight in Resolve(value: value.Height, units: units, document: document, op: op)
        select (Width: resolvedWidth, Height: resolvedHeight);

    private static Fin<double> Resolve(double value, UnitSystem units, RhinoDoc document, Op op) =>
        from active in Optional(document).ToFin(Fail: op.InvalidInput())
        from valid in guard(
            units is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits
            && active.PageUnitSystem is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits
            && RhinoMath.IsValidDouble(x: value) && value > 0.0,
            op.InvalidInput())
        let scale = RhinoMath.UnitScale(units, active.PageUnitSystem)
        let resolved = value * scale
        from validResult in guard(
            RhinoMath.IsValidDouble(x: scale) && scale > 0.0 && RhinoMath.IsValidDouble(x: resolved) && resolved > 0.0,
            op.InvalidResult())
        select resolved;
}

// Polymorphic detail-scale algebra. One `SetScale` rail absorbs unitless ratio, explicit length pair, and
// natively-parsed named scales ("1:100", "1/4\"=1'-0\"") — a second scale mode is a discriminator, not a knob.
// Native `Rhino.ScaleValue`/`LengthValue` own the parsing; `LengthUnit` is a readonly struct, not an enum.
[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FileScale {
    private FileScale() { }
    public sealed record Ratio(double Model, double Page) : FileScale;
    public sealed record Lengths(double ModelLength, LengthUnit ModelUnit, double PageLength, LengthUnit PageUnit) : FileScale;
    public sealed record Named(string Value) : FileScale;

    // One SetScale rail for ratio/length/named scales. `SetScale`'s NonConstPointer() promotes the detail geometry into
    // RhinoObject.m_edited_geometry; the detail's `CommitChanges()` (routed to CRhinoDetailObject_InternalCommitChanges)
    // then persists it so a re-queried DetailGeometry.PageToModelRatio / GetFormattedScale round-trips — this is the
    // geometry-commit rail, NOT CommitViewportChanges (which flushes only the camera snapshot) and NOT Objects.Replace
    // (which rejects detail geometry). SetScale requires a parallel projection, so coerce a perspective detail first.
    internal Fin<DetailViewObject> Apply(DetailViewObject detail, RhinoDoc document, Op op) =>
        from spec in Resolve(document: document, op: op)
        from parallel in EnsureParallel(detail: detail, op: op)
        from _ in op.Confirm(success:
            parallel.DetailGeometry.SetScale(modelLength: spec.ModelLength, modelUnits: spec.ModelUnit, pageLength: spec.PageLength, pageUnits: spec.PageUnit)
            && parallel.CommitChanges())
        select parallel;

    // SetScale rejects perspective details; coerce the viewport to parallel + persist it via CommitViewportChanges,
    // then re-query the live object (the commit rebinds the object serial, leaving the pre-coercion handle stale).
    private static Fin<DetailViewObject> EnsureParallel(DetailViewObject detail, Op op) =>
        detail.DetailGeometry is { IsParallelProjection: true }
            ? Fin.Succ(value: detail)
            : op.Catch(() => {
                ViewportInfo info = new(detail.Viewport);
                _ = info.ChangeToParallelProjection(symmetricFrustum: true);
                _ = detail.Viewport.SetViewProjection(projection: info, updateTargetLocation: false);
                _ = detail.CommitViewportChanges();
                return Optional(detail.ParentPageView)
                    .Bind(page => toSeq(page.GetDetailViews()).Find(found => found.Id == detail.Id))
                    .ToFin(Fail: op.InvalidResult());
            });

    internal Fin<(double ModelLength, LengthUnit ModelUnit, double PageLength, LengthUnit PageUnit)> Resolve(RhinoDoc document, Op op) =>
        Switch(
            (Doc: document, Op: op),
            ratio: static (ctx, r) =>
                from m in Positive(value: r.Model, op: ctx.Op)
                from p in Positive(value: r.Page, op: ctx.Op)
                let units = LengthUnit.FromKnownUnitSystem(knownUnitSystem: ctx.Doc.ModelUnitSystem)
                select (ModelLength: m, ModelUnit: units, PageLength: p, PageUnit: units),
            lengths: static (ctx, l) =>
                from m in Positive(value: l.ModelLength, op: ctx.Op)
                from p in Positive(value: l.PageLength, op: ctx.Op)
                from _ in guard(Defined(unit: l.ModelUnit) && Defined(unit: l.PageUnit), ctx.Op.InvalidInput())
                select (ModelLength: m, l.ModelUnit, PageLength: p, l.PageUnit),
            named: static (ctx, n) => ParseNamed(value: n.Value, fallback: LengthUnit.FromKnownUnitSystem(knownUnitSystem: ctx.Doc.ModelUnitSystem), op: ctx.Op));

    // `ScaleValue.LeftLengthValue()` is the drawing/page side, `RightLengthValue()` the real/model side; native
    // wrappers are IDisposable, so the tuple is materialized (LengthUnit is a value) before the `using` disposes.
    // Unitless ratios ("1:100", "1:50") parse to LengthUnit.None on both sides — substitute the doc model unit (units
    // cancel for a pure ratio) so SetScale accepts them; united scales ("1/4\"=1'-0\"") keep their parsed units.
    private static Fin<(double, LengthUnit, double, LengthUnit)> ParseNamed(string value, LengthUnit fallback, Op op) =>
        from text in FileEndpoint.NonBlank(value: value, op: op)
        from tuple in op.Catch(() => {
            // DefaultParseSettings (a shared const, Dispose inert) enables Rhino's standard rational/feet-inch/scale
            // parsing — `new StringParserSettings()` (ON_ParseSettings_New) is a separate, ambiguous native seed.
            using ScaleValue scale = ScaleValue.Create(s: text, ps: StringParserSettings.DefaultParseSettings);
            return scale is { } sv && !sv.IsUnset()
                ? op.Catch(() => {
                    using LengthValue page = sv.LeftLengthValue();
                    using LengthValue model = sv.RightLengthValue();
                    return Fin.Succ(value: (model.Length(units: model.Units), UnitOr(unit: model.Units, fallback: fallback), page.Length(units: page.Units), UnitOr(unit: page.Units, fallback: fallback)));
                })
                : Fin.Fail<(double, LengthUnit, double, LengthUnit)>(error: op.InvalidInput());
        })
        from valid in guard(RhinoMath.IsValidDouble(x: tuple.Item1) && tuple.Item1 > 0.0 && RhinoMath.IsValidDouble(x: tuple.Item3) && tuple.Item3 > 0.0, op.InvalidResult())
        select tuple;

    private static LengthUnit UnitOr(LengthUnit unit, LengthUnit fallback) =>
        LengthUnit.IsNone(unit: unit) || LengthUnit.IsUnset(unit: unit) ? fallback : unit;

    internal static Option<string> Format(DetailViewObject detail) =>
        detail.GetFormattedScale(format: DetailViewObject.ScaleFormat.OneToModelLength, value: out string formatted)
            ? FileArchiveOps.TextOption(value: formatted) : Option<string>.None;

    private static bool Defined(LengthUnit unit) => !LengthUnit.IsNone(unit: unit) && !LengthUnit.IsUnset(unit: unit);

    private static Fin<double> Positive(double value, Op op) =>
        RhinoMath.IsValidDouble(x: value) && value > 0.0 ? Fin.Succ(value: value) : Fin.Fail<double>(error: op.InvalidInput());
}

public readonly record struct FileSheetSpec(
    string Name,
    Option<FileSheetSize> Size = default,
    Option<string> Group = default,
    Option<string> Description = default);

public readonly record struct FileDetailSpec(
    string Name,
    Point2d Corner,
    Point2d Opposite,
    DefinedViewportProjection Projection = DefinedViewportProjection.Top,
    bool ProjectionLocked = true,
    Option<Guid> DisplayMode = default,
    Option<FileScale> Scale = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FileLayerOverrideSpec(
    FileOverride<DrawingColor> Color = default,
    FileOverride<bool> Visible = default,
    FileOverride<bool> PersistentVisible = default,
    FileOverride<DrawingColor> PlotColor = default,
    FileOverride<double> PlotWeight = default) {
    public static FileLayerOverrideSpec operator |(FileLayerOverrideSpec left, FileLayerOverrideSpec right) =>
        new(
            Color: left.Color | right.Color,
            Visible: left.Visible | right.Visible,
            PersistentVisible: left.PersistentVisible | right.PersistentVisible,
            PlotColor: left.PlotColor | right.PlotColor,
            PlotWeight: left.PlotWeight | right.PlotWeight);

    // Nothing to set ⇒ the only meaningful action is a bulk per-viewport reset (Layer.DeletePerViewportSettings).
    internal bool IsReset =>
        Color.Value.IsNone && Visible.Value.IsNone && PersistentVisible.Value.IsNone && PlotColor.Value.IsNone && PlotWeight.Value.IsNone;
}

public readonly record struct FileLayerOverride(string LayerPath, FileLayerOverrideSpec Spec);

// Conjunctive page filter. One resolution rail consumed by SheetOps batch verbs and Publish.FileViewSource.Pages.
public readonly record struct SheetQuery(Option<Guid> Id = default, Option<string> Name = default, Option<string> Group = default, Option<Func<RhinoPageView, bool>> Where = default) {
    public static SheetQuery All => default;

    internal Fin<Seq<RhinoPageView>> Resolve(RhinoDoc document, Op op) {
        SheetQuery self = this;
        return op.Catch(() => {
            Option<int> group = self.Group.Bind(name => Optional(document.PageViewGroups.FindName(name: name)).Map(static found => found.Index));
            return (self.Group.Case, group.Case) switch {
                (string, not int) => Fin.Succ(value: Seq<RhinoPageView>()),
                _ => Fin.Succ(value: toSeq(document.Views.GetPageViews()).Filter(page =>
                        self.Id.Map(id => page.MainViewport.Id == id).IfNone(noneValue: true)
                        && self.Name.Map(name => string.Equals(a: page.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(noneValue: true)
                        && group.Map(index => page.IsInPageViewGroup(pageViewGroupIndex: index)).IfNone(noneValue: true)
                        && self.Where.Map(predicate => predicate(arg: page)).IfNone(noneValue: true))),
            };
        });
    }
}

public readonly record struct FileSheetConfig(
    Option<FileSheetSize> Size = default,
    Option<string> Group = default,
    Option<string> Description = default,
    Option<FileScale> DetailScale = default,
    Option<UnitSystem> PageUnits = default,
    Seq<FileUserString> UserStrings = default,
    Option<string> GroupDescription = default);

public readonly record struct FileNumbering(string SheetPattern, int Start = 1, Option<string> DetailPattern = default);

public readonly record struct FileDetailGrid(int Columns, double SpacingX, double SpacingY, Point2d Origin = default, Option<Func<DetailViewObject, bool>> Where = default);

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record FileSheetEdit {
    private FileSheetEdit() { }
    public sealed record Create(FileSheetSpec Spec) : FileSheetEdit;
    public sealed record Remove(string SheetName) : FileSheetEdit;
    public sealed record Duplicate(string SheetName, bool WithGeometry = true) : FileSheetEdit;
    public sealed record Rename(string SheetName, string NewName) : FileSheetEdit;
    public sealed record Reorder(Seq<string> SheetNames) : FileSheetEdit;
    public sealed record AddDetail(string SheetName, FileDetailSpec Spec) : FileSheetEdit;
    public sealed record Resize(string SheetName, Option<FileSheetSize> Size = default, Option<string> Description = default) : FileSheetEdit;
    public sealed record ScaleDetail(string SheetName, Option<string> DetailName, FileScale Scale) : FileSheetEdit;
    public sealed record Import(FileEndpoint Source, Guid SourceViewportId, string Name) : FileSheetEdit;
    public sealed record RemoveDetail(string SheetName, string DetailName) : FileSheetEdit;
    public sealed record ActivateDetail(string SheetName, Option<string> DetailName) : FileSheetEdit;
    public sealed record LayerOverride(string SheetName, Option<string> DetailName, Seq<FileLayerOverride> Overrides) : FileSheetEdit;
    public sealed record ClippingOverride(string SheetName, string DetailName, BoundingBox Box) : FileSheetEdit;
    public sealed record Configure(SheetQuery Query, FileSheetConfig Config) : FileSheetEdit;
    public sealed record Number(SheetQuery Query, FileNumbering Numbering) : FileSheetEdit;
    public sealed record ArrangeDetails(SheetQuery Query, FileDetailGrid Grid) : FileSheetEdit;
}

public readonly record struct FileDetailReport(string Name, Option<string> Scale, DefinedViewportProjection Projection, Guid ViewportId, Seq<string> OverriddenLayers);

public readonly record struct FileSheetReport(
    string Name,
    int Number,
    Option<(double Width, double Height)> Size,
    Seq<string> Groups,
    Seq<FileDetailReport> Details,
    Seq<(string Group, Seq<FileUserString> Strings)> GroupMetadata);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static partial class SheetOps {
    // Best-effort standard-view classification: DefinedViewportProjection has no native readback, so the parallel
    // axis-aligned look direction is matched against Rhino's world-view convention; non-axis parallel ⇒ None.
    private static readonly Seq<(Vector3d Direction, DefinedViewportProjection Projection)> AxisTable = Seq(
        (new Vector3d(0.0, 0.0, -1.0), DefinedViewportProjection.Top),
        (new Vector3d(0.0, 0.0, 1.0), DefinedViewportProjection.Bottom),
        (new Vector3d(0.0, 1.0, 0.0), DefinedViewportProjection.Front),
        (new Vector3d(0.0, -1.0, 0.0), DefinedViewportProjection.Back),
        (new Vector3d(-1.0, 0.0, 0.0), DefinedViewportProjection.Right),
        (new Vector3d(1.0, 0.0, 0.0), DefinedViewportProjection.Left));

    internal static Fin<DocumentReceipt> Apply(RhinoDoc document, FileSheetEdit edit, Op op) =>
        edit.Switch(
            (Document: document, Op: op),
            create: static (ctx, e) => CreateSheet(document: ctx.Document, spec: e.Spec, op: ctx.Op),
            remove: static (ctx, e) => RemoveSheet(document: ctx.Document, sheetName: e.SheetName, op: ctx.Op),
            duplicate: static (ctx, e) => DuplicateSheet(document: ctx.Document, sheetName: e.SheetName, withGeometry: e.WithGeometry, op: ctx.Op),
            rename: static (ctx, e) => RenameSheet(document: ctx.Document, sheetName: e.SheetName, newName: e.NewName, op: ctx.Op),
            reorder: static (ctx, e) => ReorderSheets(document: ctx.Document, sheetNames: e.SheetNames, op: ctx.Op),
            addDetail: static (ctx, e) => AddDetail(document: ctx.Document, sheetName: e.SheetName, spec: e.Spec, op: ctx.Op),
            resize: static (ctx, e) => ResizeSheet(document: ctx.Document, sheetName: e.SheetName, size: e.Size, description: e.Description, op: ctx.Op),
            scaleDetail: static (ctx, e) => ScaleDetail(document: ctx.Document, sheetName: e.SheetName, detailName: e.DetailName, scale: e.Scale, op: ctx.Op),
            import: static (ctx, e) => ImportSheet(document: ctx.Document, source: e.Source, sourceViewportId: e.SourceViewportId, name: e.Name, op: ctx.Op),
            removeDetail: static (ctx, e) => RemoveDetail(document: ctx.Document, sheetName: e.SheetName, detailName: e.DetailName, op: ctx.Op),
            activateDetail: static (ctx, e) => ActivateDetail(document: ctx.Document, sheetName: e.SheetName, detailName: e.DetailName, op: ctx.Op),
            layerOverride: static (ctx, e) => ApplyLayerOverride(document: ctx.Document, sheetName: e.SheetName, detailName: e.DetailName, overrides: e.Overrides, op: ctx.Op),
            clippingOverride: static (ctx, e) => ApplyClippingOverride(document: ctx.Document, sheetName: e.SheetName, detailName: e.DetailName, box: e.Box, op: ctx.Op),
            configure: static (ctx, e) => Configure(document: ctx.Document, query: e.Query, config: e.Config, op: ctx.Op),
            number: static (ctx, e) => Number(document: ctx.Document, query: e.Query, numbering: e.Numbering, op: ctx.Op),
            arrangeDetails: static (ctx, e) => ArrangeDetails(document: ctx.Document, query: e.Query, grid: e.Grid, op: ctx.Op));

    internal static Fin<Seq<FileSheetReport>> Inspect(RhinoDoc document, SheetQuery query, Op op) =>
        from pages in query.Resolve(document: document, op: op)
        from reports in pages.TraverseM(page => ReportOf(document: document, page: page, op: op)).As()
        select reports;

    private static Fin<DocumentReceipt> CreateSheet(RhinoDoc document, FileSheetSpec spec, Op op) =>
        from name in FileEndpoint.NonBlank(value: spec.Name, op: op)
        from unique in guard(!toSeq(document.Views.GetPageViews()).Exists(view => string.Equals(a: view.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)), op.InvalidInput())
        from size in spec.Size.Map(value => value.Create(document: document, op: op)).IfNone(Fin.Succ(value: Option<(double Width, double Height)>.None))
        from page in op.Catch(() => {
            RhinoPageView? created = size.Case switch {
                (double width, double height) => document.Views.AddPageView(title: name, pageWidth: width, pageHeight: height),
                _ => document.Views.AddPageView(title: name),
            };
            return Optional(created).ToFin(Fail: op.InvalidResult());
        })
        from grouped in spec.Group.Case switch {
            string rawGroup =>
                from groupName in FileEndpoint.NonBlank(value: rawGroup, op: op)
                from joined in op.Catch(() => document.PageViewGroups.FindName(name: groupName) switch {
                    PageViewGroup existing => Fin.Succ(value: Op.Side(() => page.AddToPageViewGroup(pageViewGroupIndex: existing.Index))),
                    _ => document.PageViewGroups.Add(new PageViewGroup { Name = groupName }, Seq(page).AsIterable()) switch {
                        int index when index >= 0 => Fin.Succ(value: unit),
                        _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                    },
                })
                select joined,
            _ => Fin.Succ(value: unit),
        }
        from described in op.Catch(() => {
            _ = spec.Description.Iter(value => page.Description = value);
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            Created = Seq(page.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    private static Fin<DocumentReceipt> RemoveSheet(RhinoDoc document, string sheetName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        let pageId = page.MainViewport.Id
        from _ in op.Confirm(success: page.Close())
        select DocumentReceipt.Empty with {
            Deleted = Seq(pageId),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: sheetName)),
        };

    private static Fin<DocumentReceipt> DuplicateSheet(RhinoDoc document, string sheetName, bool withGeometry, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from copy in Optional(page.Duplicate(duplicatePageGeometry: withGeometry)).ToFin(Fail: op.InvalidResult())
        select DocumentReceipt.Empty with {
            Created = Seq(copy.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: copy.PageName)),
        };

    private static Fin<DocumentReceipt> RenameSheet(RhinoDoc document, string sheetName, string newName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: newName, op: op)
        from _ in op.Catch(() => { page.PageName = name; return Fin.Succ(value: unit); })
        select DocumentReceipt.Empty with {
            AttributeChanged = Seq(page.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    // BOUNDARY ADAPTER — RhinoCommon exposes no ViewTable.Reorder/MoveTo; rebinding page numbers
    // is the only public surface that preserves identity. PageNumber is a writable property.
    private static Fin<DocumentReceipt> ReorderSheets(RhinoDoc document, Seq<string> sheetNames, Op op) =>
        from names in sheetNames.TraverseM(name => FileEndpoint.NonBlank(value: name, op: op)).As()
        from unique in RequireUniqueSheetNames(names: names, op: op)
        from pages in names.TraverseM(name => Sheet(document: document, name: name, op: op)).As()
        from _ in op.Catch(() => {
            _ = pages.AsIterable().Select((page, index) => (Page: page, Index: index)).Iter(static item => item.Page.PageNumber = item.Index);
            document.Views.Redraw();
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            AttributeChanged = pages.Map(static page => page.MainViewport.Id),
            ResourceChanged = pages.Map(static page => new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: page.PageName)),
        };

    private static Fin<Unit> RequireUniqueSheetNames(Seq<string> names, Op op) =>
        toSeq(names.GroupBy(keySelector: static name => name, comparer: StringComparer.OrdinalIgnoreCase).Where(static row => row.Skip(1).Any())).IsEmpty
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: op.InvalidInput());

    private static Fin<DocumentReceipt> AddDetail(RhinoDoc document, string sheetName, FileDetailSpec spec, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: spec.Name, op: op)
        from displayMode in spec.DisplayMode
            .Map(id => Optional(DisplayModeDescription.GetDisplayMode(id: id)).ToFin(Fail: op.InvalidInput()).Map(Some))
            .IfNone(Fin.Succ(value: Option<DisplayModeDescription>.None))
        from receipt in op.Catch(() => {
            // BOUNDARY ADAPTER — native CRhinoPageView::AddDetailView allocates the detail against the page's
            // realized window and returns null unless the page is the active/displayed view. Activate + redraw to
            // realize the window, run the full detail lifecycle (including scale) while active, then restore.
            RhinoView? prior = document.Views.ActiveView;
            try {
                document.Views.ActiveView = page;
                page.SetPageAsActive();
                document.Views.Redraw();
                // DisplayMode is a viewport edit (CommitViewportChanges); scale + IsProjectionLocked are geometry edits.
                // scale.Apply re-queries the detail after coercion rebinds the object serial, so bind that fresh handle
                // (scaledDetail) before setting IsProjectionLocked + the final geometry CommitChanges — setting the lock
                // on the pre-coercion handle would be discarded by Apply's re-query.
                return from detail in Optional(page.AddDetailView(title: name, corner0: spec.Corner, corner1: spec.Opposite, initialProjection: spec.Projection)).ToFin(Fail: op.InvalidResult())
                       from named in op.Catch(() => {
                           ObjectAttributes attributes = detail.Attributes.Duplicate();
                           attributes.Name = name;
                           return op.Confirm(success: document.Objects.ModifyAttributes(objectId: detail.Id, newAttributes: attributes, quiet: true));
                       })
                       from display in displayMode.Map(mode =>
                           from applied in op.Catch(() => {
                               detail.Viewport.DisplayMode = mode;
                               _ = detail.CommitViewportChanges();
                               return Fin.Succ(value: unit);
                           })
                           select applied).IfNone(Fin.Succ(value: unit))
                       from scaledDetail in spec.Scale.Map(scale => scale.Apply(detail: detail, document: document, op: op))
                           .IfNone(Fin.Succ(value: detail))
                       from locked in op.Catch(() => { scaledDetail.DetailGeometry.IsProjectionLocked = spec.ProjectionLocked; return Fin.Succ(value: unit); })
                       from committed in op.Catch(() => op.Confirm(success: scaledDetail.CommitChanges()))
                       select DocumentReceipt.Empty with {
                           Created = Seq(scaledDetail.Id),
                           ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
                       };
            } finally {
                _ = prior is RhinoView view ? Op.Side(() => document.Views.ActiveView = view) : unit;
            }
        })
        select receipt;

    private static Fin<DocumentReceipt> ResizeSheet(RhinoDoc document, string sheetName, Option<FileSheetSize> size, Option<string> description, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from resolved in size.Map(value => value.Resize(document: document, op: op)).IfNone(Fin.Succ(value: (Width: Option<double>.None, Height: Option<double>.None)))
        from requested in resolved.Width.IsSome || resolved.Height.IsSome || description.IsSome ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: op.InvalidInput())
        from resized in op.Catch(() => {
            _ = resolved.Width.Iter(value => page.PageWidth = value);
            _ = resolved.Height.Iter(value => page.PageHeight = value);
            _ = description.Iter(value => page.Description = value);
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Empty with {
            AttributeChanged = Seq(page.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: page.PageName)),
        };

    // DetailName None ⇒ scale every detail on the sheet (batch).
    private static Fin<DocumentReceipt> ScaleDetail(RhinoDoc document, string sheetName, Option<string> detailName, FileScale scale, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from details in ResolveDetailsOrAll(page: page, name: detailName, op: op)
        from changed in details.TraverseM(detail =>
            from scaled in scale.Apply(detail: detail, document: document, op: op)
            select scaled.Id).As()
        select DocumentReceipt.Empty with {
            AttributeChanged = changed,
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: sheetName)),
        };

    private static Fin<DocumentReceipt> ImportSheet(RhinoDoc document, FileEndpoint source, Guid sourceViewportId, string name, Op op) =>
        from endpoint in source.Input(op: op)
        from format in (endpoint.Format | FileFormat.Detect(path: endpoint.Path)).Filter(format => format == FileFormat.ThreeDm).ToFin(Fail: op.InvalidInput())
        from pageName in FileEndpoint.NonBlank(value: name, op: op)
        from unique in guard(!toSeq(document.Views.GetPageViews()).Exists(view => string.Equals(a: view.PageName, b: pageName, comparisonType: StringComparison.OrdinalIgnoreCase)), op.InvalidInput())
        from id in sourceViewportId != Guid.Empty ? Fin.Succ(value: sourceViewportId) : Fin.Fail<Guid>(error: op.InvalidInput())
        let before = toSeq(document.Views.GetPageViews()).Map(static page => page.MainViewport.Id)
        from _ in op.Confirm(success: document.Views.ImportPageView(filename: endpoint.Path, mainViewportId: id, pageName: pageName))
        from page in toSeq(document.Views.GetPageViews())
            .Find(page => !before.Exists(viewportId => viewportId == page.MainViewport.Id)
                && string.Equals(a: page.PageName, b: pageName, comparisonType: StringComparison.OrdinalIgnoreCase))
            .ToFin(Fail: op.InvalidResult())
        select DocumentReceipt.Empty with {
            Created = Seq(page.MainViewport.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: pageName)),
        };

    private static Fin<DocumentReceipt> RemoveDetail(RhinoDoc document, string sheetName, string detailName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: detailName, op: op)
        from detail in Detail(page: page, name: name, op: op)
        let detailId = detail.Id
        from _ in op.Confirm(success: document.Objects.Delete(obj: detail, quiet: true))
        select DocumentReceipt.Empty with {
            Deleted = Seq(detailId),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    private static Fin<DocumentReceipt> ActivateDetail(RhinoDoc document, string sheetName, Option<string> detailName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from target in detailName.Case switch {
            string name => Detail(page: page, name: name, op: op).Bind(detail =>
                op.Catch(() => { detail.IsActive = true; return Fin.Succ(value: Some(detail.Id)); })),
            _ => op.Catch(() => { page.SetPageAsActive(); return Fin.Succ(value: Option<Guid>.None); }),
        }
        select DocumentReceipt.Empty with {
            AttributeChanged = target.Map(Seq).IfNone(Seq(page.MainViewport.Id)),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: detailName.IfNone(sheetName))),
        };

    // N layers per detail; DetailName None ⇒ every detail. Empty/all-inherit spec ⇒ bulk per-viewport reset.
    private static Fin<DocumentReceipt> ApplyLayerOverride(RhinoDoc document, string sheetName, Option<string> detailName, Seq<FileLayerOverride> overrides, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from _valid in guard(!overrides.IsEmpty, op.InvalidInput())
        from details in ResolveDetailsOrAll(page: page, name: detailName, op: op)
        from applied in details.TraverseM(detail =>
            overrides.TraverseM(entry =>
                from path in FileEndpoint.NonBlank(value: entry.LayerPath, op: op)
                from layer in Layer(document: document, path: path, op: op)
                from _ in entry.Spec.IsReset
                    ? op.Catch(() => Fin.Succ(value: Op.Side(() => layer.DeletePerViewportSettings(viewportId: detail.Viewport.Id))))
                    : op.Catch(() => ApplySpec(layer: layer, viewportId: detail.Viewport.Id, spec: entry.Spec))
                select path).As().Map(paths => (DetailId: detail.Id, Paths: paths))).As()
        select DocumentReceipt.Empty with {
            AttributeChanged = applied.Map(static item => item.DetailId).Distinct(),
            ResourceChanged = applied.Bind(static item => item.Paths).Distinct().Map(static path => new DocumentResourceChange(Kind: DocumentResourceKind.Layer, Name: path))
                + Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: sheetName)),
        };

    private static Fin<DocumentReceipt> ApplyClippingOverride(RhinoDoc document, string sheetName, string detailName, BoundingBox box, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: detailName, op: op)
        from validBox in op.AcceptValue(value: box)
        from detail in Detail(page: page, name: name, op: op)
        from applied in op.Catch(() => {
            detail.Viewport.SetClippingPlanes(box: validBox);
            return Commit(detail: detail, op: op);
        })
        select DocumentReceipt.Empty with {
            AttributeChanged = Seq(detail.Id),
            ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: name)),
        };

    // Page-wide propagation: size/description/detail-scale per matched page; group + user-strings + page-units once.
    private static Fin<DocumentReceipt> Configure(RhinoDoc document, SheetQuery query, FileSheetConfig config, Op op) =>
        from pages in query.Resolve(document: document, op: op)
        from _valid in guard(!pages.IsEmpty, op.InvalidInput())
        from _groupMeta in guard(config.Group.IsSome || (config.GroupDescription.IsNone && config.UserStrings.IsEmpty), op.InvalidInput())
        from groupSlot in config.Group.Case switch {
            string groupName => ResolveOrCreateGroup(document: document, groupName: groupName, pages: pages, op: op).Map(Some),
            _ => Fin.Succ(value: Option<PageViewGroup>.None),
        }
        from _units in config.PageUnits.Map(units => op.Catch(() => Fin.Succ(value: Op.Side(() => document.AdjustPageUnitSystem(newUnitSystem: units, scale: true))))).IfNone(Fin.Succ(value: unit))
        from perPage in pages.TraverseM(page => ConfigurePage(document: document, page: page, config: config, op: op)).As()
        from _meta in groupSlot.Map(active => ApplyGroupMeta(pageGroup: active, config: config, op: op)).IfNone(Fin.Succ(value: unit))
        select DocumentReceipt.Empty with {
            AttributeChanged = perPage,
            ResourceChanged = pages.Map(static page => new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: page.PageName))
                + config.Group.Map(name => Seq(new DocumentResourceChange(Kind: DocumentResourceKind.PageViewGroup, Name: name))).IfNone(Seq<DocumentResourceChange>()),
        };

    // Pattern-driven auto-numbering across the ordered set; optional DetailPattern renames details by grid position.
    private static Fin<DocumentReceipt> Number(RhinoDoc document, SheetQuery query, FileNumbering numbering, Op op) =>
        from pattern in FileEndpoint.NonBlank(value: numbering.SheetPattern, op: op)
        from pages in query.Resolve(document: document, op: op)
        from _valid in guard(!pages.IsEmpty, op.InvalidInput())
        let ordered = toSeq(pages.OrderBy(static page => page.PageNumber))
        from numbered in ordered.Map((page, index) => (Page: page, Value: numbering.Start + index)).TraverseM(item =>
            from label in FileEndpoint.NonBlank(value: Substitute(pattern: pattern, value: item.Value), op: op)
            from _ in op.Catch(() => {
                item.Page.PageName = label;
                return Fin.Succ(value: unit);
            })
            from details in numbering.DetailPattern.Map(dp => NumberDetails(document: document, page: item.Page, pattern: dp, op: op)).IfNone(Fin.Succ(value: Seq<Guid>()))
            select (Page: item.Page.MainViewport.Id, Label: label, Details: details)).As()
        select DocumentReceipt.Empty with {
            AttributeChanged = numbered.Map(static n => n.Page) + numbered.Bind(static n => n.Details),
            ResourceChanged = numbered.Map(static n => new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: n.Label)),
        };

    // GATED — verify (bridge) that ObjectTable.Transform moves the detail's page window, not its model contents,
    // before relying on this. Row-major grid placement by page-space translation of each matched detail.
    private static Fin<DocumentReceipt> ArrangeDetails(RhinoDoc document, SheetQuery query, FileDetailGrid grid, Op op) =>
        from _cols in guard(grid.Columns > 0, op.InvalidInput())
        from pages in query.Resolve(document: document, op: op)
        from _valid in guard(!pages.IsEmpty, op.InvalidInput())
        from arranged in pages.TraverseM(page => ArrangePage(document: document, page: page, grid: grid, op: op)).As()
        let moved = arranged.Bind(static item => item)
        from _any in guard(!moved.IsEmpty, op.InvalidInput())
        select DocumentReceipt.Empty with {
            Created = moved.Map(static m => m.NewId),
            Deleted = moved.Map(static m => m.OldId),
            Transformed = moved.Map(static m => m.NewId),
            ResourceChanged = pages.Map(static page => new DocumentResourceChange(Kind: DocumentResourceKind.Layout, Name: page.PageName)),
        };

    private static Fin<Seq<(Guid OldId, Guid NewId)>> ArrangePage(RhinoDoc document, RhinoPageView page, FileDetailGrid grid, Op op) {
        Seq<DetailViewObject> ordered = toSeq(toSeq(page.GetDetailViews())
            .Filter(detail => grid.Where.Map(predicate => predicate(arg: detail)).IfNone(noneValue: true))
            .OrderByDescending(static detail => MinCorner(detail: detail).Y)
            .ThenBy(static detail => MinCorner(detail: detail).X));
        return ordered.Map((detail, index) => (Detail: detail, Index: index)).TraverseM(item => op.Catch(() => {
            (double currentX, double currentY) = MinCorner(detail: item.Detail);
            int column = item.Index % grid.Columns;
            int row = item.Index / grid.Columns;
            Transform translation = Transform.Translation(new Vector3d(x: grid.Origin.X + (column * grid.SpacingX) - currentX, y: grid.Origin.Y + (row * grid.SpacingY) - currentY, z: 0.0));
            return DocumentTarget.IdResult(id: document.Objects.Transform(obj: item.Detail, xform: translation, deleteOriginal: true), op: op).Map(newId => (OldId: item.Detail.Id, NewId: newId));
        })).As();
    }

    private static Fin<Guid> ConfigurePage(RhinoDoc document, RhinoPageView page, FileSheetConfig config, Op op) =>
        from resized in config.Size.Map(value => value.Resize(document: document, op: op)).IfNone(Fin.Succ(value: (Width: Option<double>.None, Height: Option<double>.None)))
        from _applied in op.Catch(() => {
            _ = resized.Width.Iter(value => page.PageWidth = value);
            _ = resized.Height.Iter(value => page.PageHeight = value);
            _ = config.Description.Iter(value => page.Description = value);
            return Fin.Succ(value: unit);
        })
        from _scaled in config.DetailScale.Map(scale => ScaleAllDetails(document: document, page: page, scale: scale, op: op)).IfNone(Fin.Succ(value: unit))
        select page.MainViewport.Id;

    // Mirror ScaleDetail: a configure that requests a detail scale on a page with no details is a no-op the caller
    // did not intend, so fail explicitly rather than letting TraverseM succeed vacuously on the empty sequence.
    private static Fin<Unit> ScaleAllDetails(RhinoDoc document, RhinoPageView page, FileScale scale, Op op) =>
        toSeq(page.GetDetailViews()) switch {
            Seq<DetailViewObject> details when !details.IsEmpty =>
                details.TraverseM(detail => scale.Apply(detail: detail, document: document, op: op)).As().Map(static _ => unit),
            _ => Fin.Fail<Unit>(error: op.InvalidResult()),
        };

    private static Fin<Unit> ApplyGroupMeta(PageViewGroup pageGroup, FileSheetConfig config, Op op) =>
        op.Catch(() => {
            _ = config.GroupDescription.Iter(value => pageGroup.Description = value);
            _ = config.UserStrings.Iter(entry => ApplyUserString(pageGroup: pageGroup, entry: entry));
            return Fin.Succ(value: unit);
        });

    private static Unit ApplyUserString(PageViewGroup pageGroup, FileUserString entry) =>
        entry.Value.Case switch {
            string value => pageGroup.SetUserString(key: entry.Key, value: value).Ignore(),
            _ => pageGroup.DeleteUserString(key: entry.Key).Ignore(),
        };

    private static Fin<PageViewGroup> ResolveOrCreateGroup(RhinoDoc document, string groupName, Seq<RhinoPageView> pages, Op op) =>
        from name in FileEndpoint.NonBlank(value: groupName, op: op)
        from resolved in op.Catch(() => document.PageViewGroups.FindName(name: name) switch {
            PageViewGroup existing => Fin.Succ(value: pages.Iter(page => page.AddToPageViewGroup(pageViewGroupIndex: existing.Index))).Map(_ => existing),
            _ => document.PageViewGroups.Add(new PageViewGroup { Name = name }, pages.AsIterable()) switch {
                int index when index >= 0 => Optional(document.PageViewGroups.FindIndex(index: index)).ToFin(Fail: op.InvalidResult()),
                _ => Fin.Fail<PageViewGroup>(error: op.InvalidResult()),
            },
        })
        select resolved;

    private static Fin<Seq<Guid>> NumberDetails(RhinoDoc document, RhinoPageView page, string pattern, Op op) {
        Seq<DetailViewObject> ordered = toSeq(toSeq(page.GetDetailViews())
            .OrderByDescending(static detail => MinCorner(detail: detail).Y)
            .ThenBy(static detail => MinCorner(detail: detail).X));
        return ordered.Map((detail, index) => (Detail: detail, Value: index + 1)).TraverseM(item =>
            from label in FileEndpoint.NonBlank(value: Substitute(pattern: pattern, value: item.Value), op: op)
            from attributes in Optional(item.Detail.Attributes?.Duplicate()).ToFin(Fail: op.InvalidResult())
            from _ in op.Catch(() => {
                attributes.Name = label;
                return op.Confirm(success: document.Objects.ModifyAttributes(objectId: item.Detail.Id, newAttributes: attributes, quiet: true));
            })
            select item.Detail.Id).As();
    }

    private static Fin<Unit> ApplySpec(Layer layer, Guid viewportId, FileLayerOverrideSpec spec) {
        _ = spec.Color.Apply(set: value => layer.SetPerViewportColor(viewportId: viewportId, color: value), inherit: () => layer.DeletePerViewportColor(viewportId: viewportId));
        _ = spec.Visible.Apply(set: value => layer.SetPerViewportVisible(viewportId: viewportId, visible: value), inherit: () => layer.DeletePerViewportVisible(viewportId: viewportId));
        _ = spec.PersistentVisible.Apply(set: value => layer.SetPerViewportPersistentVisibility(viewportId: viewportId, persistentVisibility: value), inherit: () => layer.UnsetPerViewportPersistentVisibility(viewportId: viewportId));
        _ = spec.PlotColor.Apply(set: value => layer.SetPerViewportPlotColor(viewportId: viewportId, color: value), inherit: () => layer.DeletePerViewportPlotColor(viewportId: viewportId));
        _ = spec.PlotWeight.Apply(set: value => layer.SetPerViewportPlotWeight(viewportId: viewportId, plotWeight: value), inherit: () => layer.DeletePerViewportPlotWeight(viewportId: viewportId));
        return Fin.Succ(value: unit);
    }

    private static Fin<FileSheetReport> ReportOf(RhinoDoc document, RhinoPageView page, Op op) =>
        op.Catch(() => {
            Seq<PageViewGroup> groups = toSeq(page.GetPageViewGroupList()).Map(index => Optional(document.PageViewGroups.FindIndex(index: index))).Somes();
            Seq<FileDetailReport> details = toSeq(page.GetDetailViews()).Map(detail => DetailReportOf(document: document, detail: detail));
            return Fin.Succ(value: new FileSheetReport(
                Name: page.PageName,
                Number: page.PageNumber,
                Size: Some((Width: page.PageWidth, Height: page.PageHeight)),
                Groups: groups.Map(static item => item.Name),
                Details: details,
                GroupMetadata: groups.Map(static item => (Group: item.Name, Strings: UserStringsOf(pageGroup: item)))));
        });

    private static FileDetailReport DetailReportOf(RhinoDoc document, DetailViewObject detail) {
        Guid viewportId = detail.Viewport.Id;
        Seq<string> overridden = toSeq(document.Layers)
            .Filter(layer => !layer.IsDeleted && layer.HasPerViewportSettings(viewportId: viewportId))
            .Map(static layer => layer.FullPath);
        return new FileDetailReport(
            Name: GetDetailName(detail: detail).IfNone(string.Empty),
            Scale: FileScale.Format(detail: detail),
            Projection: ProjectionOf(detail: detail),
            ViewportId: viewportId,
            OverriddenLayers: overridden);
    }

    private static Seq<FileUserString> UserStringsOf(PageViewGroup pageGroup) {
        System.Collections.Specialized.NameValueCollection strings = pageGroup.GetUserStrings();
        return toSeq(strings.AllKeys).Choose(key => Optional(key).Map(active => new FileUserString(Key: active, Section: Option<string>.None, Value: FileArchiveOps.TextOption(value: strings[active]))));
    }

    private static DefinedViewportProjection ProjectionOf(DetailViewObject detail) {
        RhinoViewport viewport = detail.Viewport;
        return viewport.IsParallelProjection
            ? AxisProjection(direction: viewport.CameraDirection)
            : viewport.IsTwoPointPerspectiveProjection ? DefinedViewportProjection.TwoPointPerspective : DefinedViewportProjection.Perspective;
    }

    private static DefinedViewportProjection AxisProjection(Vector3d direction) {
        Vector3d look = direction;
        return look.IsValid && look.Unitize()
            ? AxisTable.Find(entry => entry.Direction * look > 0.9995).Map(static entry => entry.Projection).IfNone(DefinedViewportProjection.None)
            : DefinedViewportProjection.None;
    }

    private static (double X, double Y) MinCorner(DetailViewObject detail) {
        BoundingBox box = detail.DetailGeometry switch {
            DetailViewGeometry geometry => geometry.GetBoundingBox(accurate: true),
            _ => BoundingBox.Empty,
        };
        return (box.Min.X, box.Min.Y);
    }

    private static string Substitute(string pattern, int value) =>
        NumberPattern().Replace(input: pattern, evaluator: match =>
            match.Groups["fmt"].Success
                ? value.ToString(format: match.Groups["fmt"].Value, provider: CultureInfo.InvariantCulture)
                : value.ToString(provider: CultureInfo.InvariantCulture));

    [GeneratedRegex(pattern: "\\{n(?::(?<fmt>[^{}]+))?\\}", options: RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    private static partial Regex NumberPattern();

    private static Fin<T> Resolve<T>(IEnumerable<T> source, Func<T, bool> match, Op op) =>
        toSeq(source).Find(match).ToFin(Fail: op.InvalidInput());

    private static Fin<RhinoPageView> Sheet(RhinoDoc document, string name, Op op) =>
        from valid in FileEndpoint.NonBlank(value: name, op: op)
        from page in Resolve(source: document.Views.GetPageViews(), match: view => string.Equals(a: view.PageName, b: valid, comparisonType: StringComparison.OrdinalIgnoreCase), op: op)
        select page;

    // Display projection only (the detail report's Name): prefer the Rasm-stable Attributes.Name that AddDetail
    // authors, fall back to the viewport title. NOT a match key — see Detail.
    private static Option<string> GetDetailName(DetailViewObject detail) =>
        Optional(detail.Attributes?.Name).Filter(static value => !string.IsNullOrEmpty(value: value))
        | Optional(detail.Viewport?.Name).Filter(static value => !string.IsNullOrEmpty(value: value));

    // A detail carries two INDEPENDENT name strings: Attributes.Name (the object label AddDetail mirrors) and
    // Viewport.Name (the projection title AddDetailView/native _Detail write, and the field Rhino's own
    // RhinoPageView.SetActiveDetail matches on). Foreign 3dm layouts can populate either or both, divergently — so
    // match EITHER. A prefer-one projection (GetDetailName) would silently miss details identified by the other field.
    private static Fin<DetailViewObject> Detail(RhinoPageView page, string name, Op op) =>
        Resolve(source: page.GetDetailViews(), match: detail =>
            string.Equals(a: detail.Attributes.Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)
            || string.Equals(a: detail.Viewport.Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase), op: op);

    private static Fin<Seq<DetailViewObject>> ResolveDetailsOrAll(RhinoPageView page, Option<string> name, Op op) =>
        name.Case switch {
            string detailName => Detail(page: page, name: detailName, op: op).Map(Seq),
            _ => toSeq(page.GetDetailViews()) switch {
                Seq<DetailViewObject> details when !details.IsEmpty => Fin.Succ(value: details),
                _ => Fin.Fail<Seq<DetailViewObject>>(error: op.InvalidResult()),
            },
        };

    private static Fin<Layer> Layer(RhinoDoc document, string path, Op op) =>
        document.Layers.FindByFullPath(layerPath: path, notFoundReturnValue: -1) switch {
            int index when index >= 0 => Optional(document.Layers[index]).ToFin(Fail: op.InvalidInput()),
            _ => Fin.Fail<Layer>(error: op.InvalidInput()),
        };

    // BOUNDARY ADAPTER — CommitViewportChanges flushes the detail's viewport snapshot (CRhinoDetailViewObject_
    // CommitViewportChanges over the lazily-built m_viewport) and returns false when nothing is pending: a freshly
    // created detail, or a geometry-only SetScale that lives on the detail geometry rather than the viewport. That
    // is not an operation failure — the detail exists and renders — so the commit is best-effort, not a gate.
    private static Fin<Unit> Commit(DetailViewObject detail, Op op) =>
        op.Catch(() => { _ = detail.CommitViewportChanges(); return Fin.Succ(value: unit); });
}

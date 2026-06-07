using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using DetailViewGeometry = Rhino.Geometry.DetailView;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
public enum FileDetailLayoutMode { Grid, Align, DistributeHorizontal, DistributeVertical, FitPage }

[SmartEnum<int>]
public sealed partial class FileDetailAnchor {
    public static readonly FileDetailAnchor TopLeft = new(key: 0, xFactor: 0.0, yFactor: 1.0);
    public static readonly FileDetailAnchor TopCenter = new(key: 1, xFactor: 0.5, yFactor: 1.0);
    public static readonly FileDetailAnchor TopRight = new(key: 2, xFactor: 1.0, yFactor: 1.0);
    public static readonly FileDetailAnchor MiddleLeft = new(key: 3, xFactor: 0.0, yFactor: 0.5);
    public static readonly FileDetailAnchor Center = new(key: 4, xFactor: 0.5, yFactor: 0.5);
    public static readonly FileDetailAnchor MiddleRight = new(key: 5, xFactor: 1.0, yFactor: 0.5);
    public static readonly FileDetailAnchor BottomLeft = new(key: 6, xFactor: 0.0, yFactor: 0.0);
    public static readonly FileDetailAnchor BottomCenter = new(key: 7, xFactor: 0.5, yFactor: 0.0);
    public static readonly FileDetailAnchor BottomRight = new(key: 8, xFactor: 1.0, yFactor: 0.0);

    public double XFactor { get; }
    public double YFactor { get; }
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FileScale {
    private FileScale() { }

    public sealed record Ratio(double Model, double Page) : FileScale;

    public sealed record Lengths(double ModelLength, LengthUnit ModelUnit, double PageLength, LengthUnit PageUnit) : FileScale;

    public sealed record Named(string Value) : FileScale;

    internal Fin<(double ModelLength, LengthUnit ModelUnit, double PageLength, LengthUnit PageUnit)> Resolve(RhinoDoc document, Op op) =>
        Switch(
            (Doc: document, Op: op),
            ratio: static (ctx, r) =>
                from m in ctx.Op.Positive(value: r.Model)
                from p in ctx.Op.Positive(value: r.Page)
                let modelUnit = LengthUnit.FromKnownUnitSystem(knownUnitSystem: ctx.Doc.ModelUnitSystem)
                let pageUnit = LengthUnit.FromKnownUnitSystem(knownUnitSystem: ctx.Doc.PageUnitSystem)
                select (ModelLength: m, ModelUnit: modelUnit, PageLength: p, PageUnit: pageUnit),
            lengths: static (ctx, l) =>
                from m in ctx.Op.Positive(value: l.ModelLength)
                from p in ctx.Op.Positive(value: l.PageLength)
                from _ in guard(Defined(unit: l.ModelUnit) && Defined(unit: l.PageUnit), ctx.Op.InvalidInput())
                select (ModelLength: m, l.ModelUnit, PageLength: p, l.PageUnit),
            named: static (ctx, n) => ParseNamed(
                value: n.Value,
                modelFallback: LengthUnit.FromKnownUnitSystem(knownUnitSystem: ctx.Doc.ModelUnitSystem),
                pageFallback: LengthUnit.FromKnownUnitSystem(knownUnitSystem: ctx.Doc.PageUnitSystem),
                op: ctx.Op));

    internal Fin<DetailViewObject> Apply(DetailViewObject detail, RhinoDoc document, Op op) =>
        from spec in Resolve(document: document, op: op)
        from parallel in Optional(detail).ToFin(Fail: op.InvalidInput())
        from _projection in guard(parallel.DetailGeometry is { IsParallelProjection: true }, op.InvalidInput())
        from _ in op.Confirm(success:
            parallel.DetailGeometry.SetScale(modelLength: spec.ModelLength, modelUnits: spec.ModelUnit, pageLength: spec.PageLength, pageUnits: spec.PageUnit)
            && parallel.CommitChanges())
        select parallel;

    internal static Option<string> Format(DetailViewObject detail) =>
        detail.GetFormattedScale(format: DetailViewObject.ScaleFormat.OneToModelLength, value: out string formatted)
            ? FileArchiveOps.TextOption(value: formatted) : Option<string>.None;

    private static bool Defined(LengthUnit unit) => !LengthUnit.IsNone(unit: unit) && !LengthUnit.IsUnset(unit: unit);

    private static LengthUnit UnitOr(LengthUnit unit, LengthUnit fallback) =>
        LengthUnit.IsNone(unit: unit) || LengthUnit.IsUnset(unit: unit) ? fallback : unit;

    private static Fin<(double, LengthUnit, double, LengthUnit)> ParseNamed(string value, LengthUnit modelFallback, LengthUnit pageFallback, Op op) =>
        from text in FileEndpoint.NonBlank(value: value, op: op)
        from tuple in op.Catch(() => {
            using ScaleValue scale = ScaleValue.Create(s: text, ps: StringParserSettings.DefaultParseSettings);
            return scale is { } sv && !sv.IsUnset()
                ? op.Catch(() => {
                    using LengthValue page = sv.LeftLengthValue();
                    using LengthValue model = sv.RightLengthValue();
                    return Fin.Succ(value: (model.Length(units: model.Units), UnitOr(unit: model.Units, fallback: modelFallback), page.Length(units: page.Units), UnitOr(unit: page.Units, fallback: pageFallback)));
                })
                : Fin.Fail<(double, LengthUnit, double, LengthUnit)>(error: op.InvalidInput());
        })
        from valid in guard(RhinoMath.IsValidDouble(x: tuple.Item1) && tuple.Item1 > 0.0 && RhinoMath.IsValidDouble(x: tuple.Item3) && tuple.Item3 > 0.0, op.InvalidResult())
        select tuple;
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FileClipOp {
    private FileClipOp() { }
    public sealed record AddPlane(Plane Plane, double U, double V, Option<ObjectAttributes> Attributes = default) : FileClipOp;
    public sealed record Toggle(Guid PlaneId, bool Active) : FileClipOp;
    public sealed record Scope(Guid PlaneId, Seq<Guid> ObjectIds, Seq<int> LayerIndices, bool Exclusive) : FileClipOp;
}

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
    public sealed record ScaleDetail(string SheetName, DetailQuery Detail, FileScale Scale) : FileSheetEdit;
    public sealed record Import(FileEndpoint Source, Guid SourceViewportId, string Name) : FileSheetEdit;
    public sealed record RemoveDetail(string SheetName, DetailQuery Detail) : FileSheetEdit;
    public sealed record ActivateDetail(string SheetName, Option<DetailQuery> Detail = default) : FileSheetEdit;
    public sealed record LayerOverride(string SheetName, DetailQuery Detail, Seq<FileLayerOverride> Overrides) : FileSheetEdit;
    public sealed record ClipDetail(string SheetName, DetailQuery Detail, FileClipOp Op) : FileSheetEdit;
    public sealed record SaveDetailView(string SheetName, DetailQuery Detail, string ViewName) : FileSheetEdit;
    public sealed record RestoreDetailView(string SheetName, DetailQuery Detail, string ViewName) : FileSheetEdit;
    public sealed record FrameDetail(string SheetName, DetailQuery Detail, Seq<CameraEdit> Edits, Option<FileScale> Scale = default, Option<BoundingBox> Clipping = default) : FileSheetEdit;
    public sealed record ComposeDetail(string SheetName, DetailQuery Detail, CameraShot Shot, Option<FileScale> Scale = default) : FileSheetEdit;
    public sealed record Configure(SheetQuery Query, FileSheetConfig Config) : FileSheetEdit;
    public sealed record Number(SheetQuery Query, FileNumbering Numbering) : FileSheetEdit;
    public sealed record ArrangeDetails(SheetQuery Query, FileDetailLayout Layout) : FileSheetEdit;
    public sealed record GroupAssign(SheetQuery Query, string Group, bool RemoveFromOthers = false) : FileSheetEdit;
}

// --- [MODELS] -----------------------------------------------------------------------------
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

public readonly record struct DetailQuery(Option<Guid> Id = default, Option<string> Name = default, Option<Func<DetailViewObject, bool>> Where = default) {
    public static DetailQuery All => default;
    public static DetailQuery Named(string name) => new(Name: Some(name));
    public static DetailQuery Parallel => new(Where: Some<Func<DetailViewObject, bool>>(value: static detail => detail.DetailGeometry is { IsParallelProjection: true }));
    public static DetailQuery Perspective => new(Where: Some<Func<DetailViewObject, bool>>(value: static detail => detail.DetailGeometry is not { IsParallelProjection: true }));
    internal bool IsAll => Id.IsNone && Name.IsNone && Where.IsNone;

    internal static Option<string> NameOf(DetailViewObject detail) =>
        Optional(detail.Attributes?.Name).Filter(static value => !string.IsNullOrEmpty(value: value)).Case switch {
            string name => Some(name),
            _ => Optional(detail.Viewport?.Name).Filter(static value => !string.IsNullOrEmpty(value: value)),
        };

    internal Fin<Seq<DetailViewObject>> Resolve(RhinoPageView page, Op op) {
        DetailQuery self = this;
        return from name in self.Name.Map(value => FileEndpoint.NonBlank(value: value, op: op).Map(Some)).IfNone(Fin.Succ(value: Option<string>.None))
               from details in op.Catch(() => Fin.Succ(value: toSeq(page.GetDetailViews()).Filter(detail =>
                   self.Id.Map(id => detail.Id == id || detail.Viewport.Id == id).IfNone(noneValue: true)
                   && name.Map(value => NameOf(detail: detail).Map(found => string.Equals(a: found, b: value, comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(noneValue: false)).IfNone(noneValue: true)
                   && self.Where.Map(predicate => predicate(arg: detail)).IfNone(noneValue: true))))
               from _any in guard(!details.IsEmpty, op.InvalidResult())
               select details;
    }

    internal Fin<DetailViewObject> Single(RhinoPageView page, Op op) =>
        from details in Resolve(page: page, op: op)
        from _one in guard(details.Count == 1, op.InvalidInput())
        select details[0];
}

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

    private static Fin<(double Width, double Height)> Resolve((double Width, double Height) value, UnitSystem units, RhinoDoc document, Op op) =>
        from resolvedWidth in Resolve(value: value.Width, units: units, document: document, op: op)
        from resolvedHeight in Resolve(value: value.Height, units: units, document: document, op: op)
        select (Width: resolvedWidth, Height: resolvedHeight);
}

public readonly record struct FileSheetSpec(
    string Name,
    Option<FileSheetSize> Size = default,
    Option<string> Group = default,
    Option<string> Description = default);

public readonly record struct FileSheetConfig(
    Option<FileSheetSize> Size = default,
    Option<string> Group = default,
    Option<string> Description = default,
    Option<FileScale> DetailScale = default,
    Option<UnitSystem> DocumentPageUnits = default,
    Seq<FileUserString> UserStrings = default,
    Option<string> GroupDescription = default);

public readonly record struct FileNumbering(string SheetPattern, int Start = 1, Option<string> DetailPattern = default);

public readonly record struct FileDetailSpec(
    string Name,
    Point2d Corner,
    Point2d Opposite,
    DefinedViewportProjection Projection = DefinedViewportProjection.Top,
    bool ProjectionLocked = true,
    Option<Guid> DisplayMode = default,
    Option<FileScale> Scale = default,
    Seq<CameraEdit> View = default);

public readonly record struct FileDetailLayout(
    int Columns = 1,
    double SpacingX = 0.0,
    double SpacingY = 0.0,
    Point2d Origin = default,
    DetailQuery Detail = default,
    Option<Point2d> Size = default,
    FileDetailLayoutMode Mode = FileDetailLayoutMode.Grid,
    FileDetailAnchor? Anchor = null,
    double Margin = 0.0,
    bool Duplicate = false);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FileLayerOverrideSpec(
    FileOverride<DrawingColor> Color = default,
    FileOverride<bool> Visible = default,
    FileOverride<bool> PersistentVisible = default,
    FileOverride<DrawingColor> PlotColor = default,
    FileOverride<double> PlotWeight = default,
    bool ResetAll = false) {
    public static FileLayerOverrideSpec Reset => new(ResetAll: true);

    internal bool HasFieldOperation => Color.IsActive || Visible.IsActive || PersistentVisible.IsActive || PlotColor.IsActive || PlotWeight.IsActive;

    internal bool Applies => ResetAll || HasFieldOperation;

    public static FileLayerOverrideSpec operator |(FileLayerOverrideSpec left, FileLayerOverrideSpec right) =>
        right.ResetAll ? right : right.HasFieldOperation ? new(
            Color: left.Color | right.Color,
            Visible: left.Visible | right.Visible,
            PersistentVisible: left.PersistentVisible | right.PersistentVisible,
            PlotColor: left.PlotColor | right.PlotColor,
            PlotWeight: left.PlotWeight | right.PlotWeight) : left;
}

public readonly record struct FileLayerOverride(string LayerPath, FileLayerOverrideSpec Spec);

public readonly record struct FileClipReport(
    Guid Id,
    Plane Plane,
    Seq<Guid> ObjectIds,
    Seq<int> LayerIndices,
    bool Exclusive);

public readonly record struct FileDetailReport(
    string Name,
    string DescriptiveTitle,
    Option<string> Scale,
    DefinedViewportProjection Projection,
    Guid ViewportId,
    Option<Guid> ParentViewportId,
    bool Active,
    bool ProjectionLocked,
    double PageToModelRatio,
    Transform WorldToPage,
    Transform PageToWorld,
    Option<double> PaperLengthPerModelUnit,
    Option<double> ModelLengthPerPaperUnit,
    Seq<FileClipReport> ClippingPlanes,
    Seq<string> OverriddenLayers,
    Option<(double Near, double Far)> ClipRange);

public readonly record struct FileSheetReport(
    string Name,
    int Number,
    Option<string> PrinterName,
    Option<string> PaperName,
    bool Active,
    Option<Guid> ActiveDetailId,
    (double Width, double Height) Size,
    Seq<string> Groups,
    Seq<FileDetailReport> Details,
    Seq<(string Group, Seq<FileUserString> Strings)> GroupMetadata);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static partial class SheetOps {
    [GeneratedRegex(pattern: "\\{n(?::(?<fmt>[^{}]+))?\\}", options: RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    private static partial Regex NumberPattern { get; }

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
            scaleDetail: static (ctx, e) => FrameDetail(
                document: ctx.Document,
                sheetName: e.SheetName,
                detail: e.Detail,
                edits: Seq<CameraEdit>(),
                scale: Some(e.Scale),
                clipping: Option<BoundingBox>.None,
                op: ctx.Op),
            import: static (ctx, e) => ImportSheet(document: ctx.Document, source: e.Source, sourceViewportId: e.SourceViewportId, name: e.Name, op: ctx.Op),
            removeDetail: static (ctx, e) => RemoveDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, op: ctx.Op),
            activateDetail: static (ctx, e) => ActivateDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, op: ctx.Op),
            layerOverride: static (ctx, e) => ApplyLayerOverride(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, overrides: e.Overrides, op: ctx.Op),
            clipDetail: static (ctx, e) => ApplyClipDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, clipOp: e.Op, op: ctx.Op),
            saveDetailView: static (ctx, e) => ApplySaveDetailView(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, viewName: e.ViewName, op: ctx.Op),
            restoreDetailView: static (ctx, e) => ApplyRestoreDetailView(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, viewName: e.ViewName, op: ctx.Op),
            frameDetail: static (ctx, e) => FrameDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, edits: e.Edits, scale: e.Scale, clipping: e.Clipping, op: ctx.Op),
            composeDetail: static (ctx, e) => ComposeDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, shot: e.Shot, scale: e.Scale, op: ctx.Op),
            configure: static (ctx, e) => Configure(document: ctx.Document, query: e.Query, config: e.Config, op: ctx.Op),
            number: static (ctx, e) => Number(document: ctx.Document, query: e.Query, numbering: e.Numbering, op: ctx.Op),
            arrangeDetails: static (ctx, e) => ArrangeDetails(document: ctx.Document, query: e.Query, layout: e.Layout, op: ctx.Op),
            groupAssign: static (ctx, e) => GroupAssign(document: ctx.Document, query: e.Query, label: e.Group, removeFromOthers: e.RemoveFromOthers, op: ctx.Op));

    internal static Fin<Seq<FileSheetReport>> Inspect(RhinoDoc document, SheetQuery query, Op op, DetailQuery detail = default) =>
        from pages in query.Resolve(document: document, op: op)
        from reports in pages.TraverseM(page => ReportOf(document: document, page: page, detail: detail, op: op)).As()
        select reports;

    private static Fin<RhinoPageView> Sheet(RhinoDoc document, string name, Op op) =>
        from valid in FileEndpoint.NonBlank(value: name, op: op)
        from page in toSeq(document.Views.GetPageViews())
            .Find(view => string.Equals(a: view.PageName, b: valid, comparisonType: StringComparison.OrdinalIgnoreCase))
            .ToFin(Fail: op.InvalidInput())
        select page;

    private static Fin<DocumentReceipt> MutateDetails(
        RhinoDoc document,
        string sheetName,
        DetailQuery detail,
        Op op,
        Func<RhinoPageView, DetailViewObject, Fin<(Guid Id, Seq<DocumentResourceChange> Resources)>> mutate) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from validMutate in Optional(mutate).ToFin(Fail: op.InvalidInput())
        from details in detail.Resolve(page: page, op: op)
        from changed in details.TraverseM(detail => validMutate(arg1: page, arg2: detail)).As()
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: changed.Map(static item => item.Id).Distinct(), resources: Seq(DocumentResourceKind.Layout.Change(name: sheetName)) + changed.Bind(static item => item.Resources).Distinct());

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
                    _ => op.Confirm(success: document.PageViewGroups.Add(new PageViewGroup { Name = groupName }, Seq(page).AsIterable()) >= 0),
                })
                select joined,
            _ => Fin.Succ(value: unit),
        }
        from described in op.Catch(() => {
            _ = spec.Description.Iter(value => page.Description = value);
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(page.MainViewport.Id), kind: DocumentResourceKind.Layout, name: name);

    private static Fin<DocumentReceipt> RemoveSheet(RhinoDoc document, string sheetName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        let pageId = page.MainViewport.Id
        from _ in op.Confirm(success: page.Close())
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Deleted, ids: Seq(pageId), kind: DocumentResourceKind.Layout, name: sheetName);

    private static Fin<DocumentReceipt> DuplicateSheet(RhinoDoc document, string sheetName, bool withGeometry, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from copy in Optional(page.Duplicate(duplicatePageGeometry: withGeometry)).ToFin(Fail: op.InvalidResult())
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(copy.MainViewport.Id), kind: DocumentResourceKind.Layout, name: copy.PageName);

    private static Fin<DocumentReceipt> RenameSheet(RhinoDoc document, string sheetName, string newName, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: newName, op: op)
        from _ in op.Catch(() => { page.PageName = name; return Fin.Succ(value: unit); })
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: Seq(page.MainViewport.Id), kind: DocumentResourceKind.Layout, name: name);

    private static Fin<DocumentReceipt> ReorderSheets(RhinoDoc document, Seq<string> sheetNames, Op op) =>
        from names in sheetNames.TraverseM(name => FileEndpoint.NonBlank(value: name, op: op)).As()
        from unique in guard(
            toSeq(names.GroupBy(keySelector: static name => name, comparer: StringComparer.OrdinalIgnoreCase)
                .Where(static row => row.Skip(1).Any())).IsEmpty,
            op.InvalidInput()).ToFin()
        from pages in names.TraverseM(name => Sheet(document: document, name: name, op: op)).As()
        let current = toSeq(document.Views.GetPageViews().OrderBy(static page => page.PageNumber))
        let requested = new System.Collections.Generic.HashSet<Guid>(pages.Map(static page => page.MainViewport.Id).AsIterable())
        let ordered = pages + current.Filter(page => !requested.Contains(page.MainViewport.Id))
        from _ in op.Catch(() => {
            _ = ordered.AsIterable().Select((page, index) => (Page: page, Index: index)).Iter(static item => item.Page.PageNumber = item.Index);
            document.Views.Redraw();
            return Fin.Succ(value: unit);
        })
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: ordered.Map(static page => page.MainViewport.Id), resources: ordered.Map(static page => DocumentResourceKind.Layout.Change(name: page.PageName)));

    private static Fin<DocumentReceipt> AddDetail(RhinoDoc document, string sheetName, FileDetailSpec spec, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from name in FileEndpoint.NonBlank(value: spec.Name, op: op)
        from displayMode in spec.DisplayMode
            .Map(id => Optional(DisplayModeDescription.GetDisplayMode(id: id)).ToFin(Fail: op.InvalidInput()).Map(Some))
            .IfNone(Fin.Succ(value: Option<DisplayModeDescription>.None))
        from receipt in op.Catch(() => {
            // BOUNDARY ADAPTER — AddDetailView requires the page be the active view; the finally restores the prior ActiveView even on failure.
            RhinoView? prior = document.Views.ActiveView;
            try {
                document.Views.ActiveView = page;
                page.SetPageAsActive();
                return from detail in Optional(page.AddDetailView(title: name, corner0: spec.Corner, corner1: spec.Opposite, initialProjection: spec.Projection)).ToFin(Fail: op.InvalidResult())
                       from named in op.Catch(() => {
                           ObjectAttributes attributes = detail.Attributes.Duplicate();
                           attributes.Name = name;
                           return op.Confirm(success: document.Objects.ModifyAttributes(objectId: detail.Id, newAttributes: attributes, quiet: true));
                       })
                       from display in displayMode.Map(mode =>
                           op.Catch(() => {
                               detail.Viewport.DisplayMode = mode;
                               return op.Confirm(success: detail.CommitViewportChanges());
                           })).IfNone(Fin.Succ(value: unit))
                       from scaledDetail in spec.Scale.Map(scale => scale.Apply(detail: detail, document: document, op: op))
                           .IfNone(Fin.Succ(value: detail))
                       from locked in op.Catch(() => { scaledDetail.DetailGeometry.IsProjectionLocked = spec.ProjectionLocked; return Fin.Succ(value: unit); })
                       from committed in op.Catch(() => op.Confirm(success: scaledDetail.CommitChanges()))
                       from framed in ApplyDetailView(document: document, page: page, detail: scaledDetail, edits: spec.View)
                       from _redraw in op.Catch(() => { document.Views.Redraw(); return Fin.Succ(value: unit); })
                       select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(framed.Id), kind: DocumentResourceKind.Layout, name: name);
            } finally {
                _ = prior is RhinoView view ? Op.Side(() => document.Views.ActiveView = view) : unit;
            }
        })
        select receipt;

    private static Fin<DocumentReceipt> ResizeSheet(RhinoDoc document, string sheetName, Option<FileSheetSize> size, Option<string> description, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from resolved in size.Map(value => value.Resize(document: document, op: op)).IfNone(Fin.Succ(value: (Width: Option<double>.None, Height: Option<double>.None)))
        from requested in guard(resolved.Width.IsSome || resolved.Height.IsSome || description.IsSome, op.InvalidInput())
        from resized in ApplyPageConfig(page: page, size: resolved, description: description, op: op)
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: Seq(page.MainViewport.Id), kind: DocumentResourceKind.Layout, name: page.PageName);

    private static Fin<DocumentReceipt> ImportSheet(RhinoDoc document, FileEndpoint source, Guid sourceViewportId, string name, Op op) =>
        from endpoint in source.Input(op: op)
        from format in (endpoint.Format | FileFormat.Detect(path: endpoint.Path)).Filter(format => format.Is(key: "3dm")).ToFin(Fail: op.InvalidInput())
        from pageName in FileEndpoint.NonBlank(value: name, op: op)
        from unique in guard(!toSeq(document.Views.GetPageViews()).Exists(view => string.Equals(a: view.PageName, b: pageName, comparisonType: StringComparison.OrdinalIgnoreCase)), op.InvalidInput())
        from id in guard(sourceViewportId != Guid.Empty, op.InvalidInput()).ToFin().Map(_ => sourceViewportId)
        let before = toSeq(document.Views.GetPageViews()).Map(static page => page.MainViewport.Id)
        from _ in op.Confirm(success: document.Views.ImportPageView(filename: endpoint.Path, mainViewportId: id, pageName: pageName))
        from page in toSeq(document.Views.GetPageViews())
            .Find(page => !before.Exists(viewportId => viewportId == page.MainViewport.Id)
                && string.Equals(a: page.PageName, b: pageName, comparisonType: StringComparison.OrdinalIgnoreCase))
            .ToFin(Fail: op.InvalidResult())
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: Seq(page.MainViewport.Id), kind: DocumentResourceKind.Layout, name: pageName);

    private static Fin<DocumentReceipt> RemoveDetail(RhinoDoc document, string sheetName, DetailQuery detail, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from details in detail.Resolve(page: page, op: op)
        let names = details.Map(DetailQuery.NameOf).Somes()
        from _ in details.TraverseM(item => op.Confirm(success: document.Objects.Delete(obj: item, quiet: true))).As()
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Deleted, ids: details.Map(static item => item.Id), resources: (names.IsEmpty ? Seq(DocumentResourceKind.Layout.Change(name: sheetName))
                    : names.Map(static name => DocumentResourceKind.Layout.Change(name: name)))
                .Distinct());

    private static Fin<DocumentReceipt> ActivateDetail(RhinoDoc document, string sheetName, Option<DetailQuery> detail, Op op) =>
        from page in Sheet(document: document, name: sheetName, op: op)
        from target in detail.Case switch {
            DetailQuery query => query.Single(page: page, op: op).Bind(match =>
                op.Catch(() =>
                    op.Confirm(success: page.SetActiveDetail(detailId: match.Id))
                        .Map(_ => Some(match)))),
            _ => op.Catch(() => { page.SetPageAsActive(); return Fin.Succ(value: Option<DetailViewObject>.None); }),
        }
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: target.Map(item => Seq(item.Id)).IfNone(Seq(page.MainViewport.Id)), kind: DocumentResourceKind.Layout, name: target.Bind(DetailQuery.NameOf).IfNone(sheetName));

    private static Fin<DocumentReceipt> ApplyLayerOverride(RhinoDoc document, string sheetName, DetailQuery detail, Seq<FileLayerOverride> overrides, Op op) =>
        from _valid in guard(!overrides.IsEmpty && overrides.ForAll(static entry => entry.Spec.Applies), op.InvalidInput())
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (page, detail) =>
            overrides.TraverseM(entry =>
                from path in FileEndpoint.NonBlank(value: entry.LayerPath, op: op)
                from layer in document.Layers.FindByFullPath(layerPath: path, notFoundReturnValue: -1) switch {
                    int index when index >= 0 => Optional(document.Layers[index]).ToFin(Fail: op.InvalidInput()),
                    _ => Fin.Fail<Layer>(error: op.InvalidInput()),
                }
                from _reset in entry.Spec.ResetAll
                    ? op.Catch(() => Fin.Succ(value: Op.Side(() => layer.DeletePerViewportSettings(viewportId: detail.Viewport.Id))))
                    : Fin.Succ(value: unit)
                from _fields in entry.Spec.HasFieldOperation
                    ? op.Catch(() => {
                        _ = entry.Spec.Color.Apply(set: value => layer.SetPerViewportColor(viewportId: detail.Viewport.Id, color: value), inherit: () => layer.DeletePerViewportColor(viewportId: detail.Viewport.Id));
                        _ = entry.Spec.Visible.Apply(set: value => layer.SetPerViewportVisible(viewportId: detail.Viewport.Id, visible: value), inherit: () => layer.DeletePerViewportVisible(viewportId: detail.Viewport.Id));
                        _ = entry.Spec.PersistentVisible.Apply(set: value => layer.SetPerViewportPersistentVisibility(viewportId: detail.Viewport.Id, persistentVisibility: value), inherit: () => layer.UnsetPerViewportPersistentVisibility(viewportId: detail.Viewport.Id));
                        _ = entry.Spec.PlotColor.Apply(set: value => layer.SetPerViewportPlotColor(viewportId: detail.Viewport.Id, color: value), inherit: () => layer.DeletePerViewportPlotColor(viewportId: detail.Viewport.Id));
                        _ = entry.Spec.PlotWeight.Apply(set: value => layer.SetPerViewportPlotWeight(viewportId: detail.Viewport.Id, plotWeight: value), inherit: () => layer.DeletePerViewportPlotWeight(viewportId: detail.Viewport.Id));
                        return Fin.Succ(value: unit);
                    })
                    : Fin.Succ(value: unit)
                select path).As()
                .Map(paths => (detail.Id, Resources: paths.Distinct().Map(static path => DocumentResourceKind.Layer.Change(name: path)))))
        select receipt;

    private static Fin<DocumentReceipt> ApplyClipDetail(RhinoDoc document, string sheetName, DetailQuery detail, FileClipOp clipOp, Op op) =>
        clipOp.Switch(
            (Document: document, SheetName: sheetName, Detail: detail, Op: op),
            addPlane: static (c, cmd) =>
                MutateDetails(document: c.Document, sheetName: c.SheetName, detail: c.Detail, op: c.Op, mutate: (_, view) =>
                    from attrs in cmd.Attributes.Case switch {
                        ObjectAttributes existing => Fin.Succ(value: existing),
                        _ => c.Op.Catch(() => Fin.Succ(value: new ObjectAttributes())),
                    }
                    from id in c.Op.Catch(() => c.Op.AcceptValue(value: c.Document.Objects.AddClippingPlane(
                        plane: cmd.Plane, uMagnitude: cmd.U, vMagnitude: cmd.V,
                        clippedViewportIds: Seq(view.Viewport.Id).AsIterable(), attributes: attrs)))
                    select (view.Id, Resources: Seq(DocumentResourceKind.Object.Change(name: id.ToString())))),
            toggle: static (c, cmd) =>
                MutateDetails(document: c.Document, sheetName: c.SheetName, detail: c.Detail, op: c.Op, mutate: (_, view) =>
                    from clip in ClipObject(document: c.Document, planeId: cmd.PlaneId, op: c.Op)
                    from _toggled in c.Op.Confirm(success: cmd.Active
                        ? clip.AddClipViewport(viewport: view.Viewport, commit: true)
                        : clip.RemoveClipViewport(viewport: view.Viewport, commit: true))
                    select (view.Id, Resources: Seq(DocumentResourceKind.Object.Change(name: cmd.PlaneId.ToString())))),
            scope: static (c, cmd) =>
                MutateDetails(document: c.Document, sheetName: c.SheetName, detail: c.Detail, op: c.Op, mutate: (_, view) =>
                    from clip in ClipObject(document: c.Document, planeId: cmd.PlaneId, op: c.Op)
                    from geom in Optional(clip.ClippingPlaneGeometry).ToFin(Fail: c.Op.InvalidInput())
                    from _scoped in c.Op.Catch(() => {
                        geom.SetClipParticipation(
                            objectIds: cmd.ObjectIds.AsIterable(),
                            layerIndices: cmd.LayerIndices.AsIterable(),
                            isExclusionList: cmd.Exclusive);
                        return c.Op.Confirm(success: clip.CommitChanges());
                    })
                    select (view.Id, Resources: Seq(DocumentResourceKind.Object.Change(name: cmd.PlaneId.ToString())))));

    private static Fin<DocumentReceipt> ApplySaveDetailView(RhinoDoc document, string sheetName, DetailQuery detail, string viewName, Op op) =>
        from name in FileEndpoint.NonBlank(value: viewName, op: op)
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (_, view) =>
            from _index in op.Catch(() => document.NamedViews.Add(name: name, viewportId: view.Viewport.Id) switch {
                int created when created >= 0 => Fin.Succ(value: created),
                _ => Fin.Fail<int>(error: op.InvalidResult()),
            })
            select (view.Id, Resources: Seq(DocumentResourceKind.NamedView.Change(name: name))))
        select receipt;

    private static Fin<DocumentReceipt> ApplyRestoreDetailView(RhinoDoc document, string sheetName, DetailQuery detail, string viewName, Op op) =>
        from name in FileEndpoint.NonBlank(value: viewName, op: op)
        from index in document.NamedViews.FindByName(name: name) switch {
            int found when found >= 0 => Fin.Succ(value: found),
            _ => Fin.Fail<int>(error: op.MissingContext()),
        }
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (_, view) =>
            from _restored in op.Confirm(success: document.NamedViews.Restore(index: index, viewport: view.Viewport))
            from _committed in op.Confirm(success: view.CommitViewportChanges())
            select (view.Id, Resources: Seq(DocumentResourceKind.NamedView.Change(name: name))))
        select receipt;

    private static Fin<DocumentReceipt> FrameDetail(RhinoDoc document, string sheetName, DetailQuery detail, Seq<CameraEdit> edits, Option<FileScale> scale, Option<BoundingBox> clipping, Op op) =>
        from admitted in Optional(edits).ToFin(Fail: op.InvalidInput()).Map(static source => toSeq(source))
        from requested in guard(!admitted.IsEmpty || scale.IsSome || clipping.IsSome, op.InvalidInput())
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (page, view) =>
            from scaled in scale.Map(value => value.Apply(detail: view, document: document, op: op)).IfNone(Fin.Succ(value: view))
            from clipped in clipping.Map(box => ApplyClipping(detail: scaled, box: box, op: op)).IfNone(Fin.Succ(value: scaled))
            from viewed in ApplyDetailView(document: document, page: page, detail: clipped, edits: admitted)
            select (viewed.Id, Resources: Seq<DocumentResourceChange>()))
        select receipt;

    private static Fin<DocumentReceipt> ComposeDetail(RhinoDoc document, string sheetName, DetailQuery detail, CameraShot shot, Option<FileScale> scale, Op op) =>
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (page, view) =>
            from scaled in scale.Map(value => value.Apply(detail: view, document: document, op: op)).IfNone(Fin.Succ(value: view))
            from changed in ApplyDetailShot(document: document, page: page, detail: scaled, shot: shot)
            select (scaled.Id, changed.Resources))
        select receipt;

    private static Fin<DocumentReceipt> Configure(RhinoDoc document, SheetQuery query, FileSheetConfig config, Op op) =>
        from pages in query.Resolve(document: document, op: op)
        from _valid in guard(!pages.IsEmpty, op.InvalidInput())
        from _groupMeta in guard(config.Group.IsSome || (config.GroupDescription.IsNone && config.UserStrings.IsEmpty), op.InvalidInput())
        from groupSlot in config.Group.Case switch {
            string groupName => ResolveOrCreateGroup(document: document, groupName: groupName, pages: pages, op: op).Map(Some),
            _ => Fin.Succ(value: Option<PageViewGroup>.None),
        }
        from _units in config.DocumentPageUnits.Map(units => op.Catch(() => Fin.Succ(value: Op.Side(() => document.AdjustPageUnitSystem(newUnitSystem: units, scale: true))))).IfNone(Fin.Succ(value: unit))
        from perPage in pages.TraverseM(page => ConfigurePage(document: document, page: page, config: config, op: op)).As()
        from _meta in groupSlot.Map(active => ApplyGroupMeta(pageGroup: active, config: config, op: op)).IfNone(Fin.Succ(value: unit))
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: perPage, resources: pages.Map(static page => DocumentResourceKind.Layout.Change(name: page.PageName))
                + config.Group.Map(name => Seq(DocumentResourceKind.PageViewGroup.Change(name: name))).IfNone(Seq<DocumentResourceChange>()));

    private static Fin<Guid> ConfigurePage(RhinoDoc document, RhinoPageView page, FileSheetConfig config, Op op) =>
        from resized in config.Size.Map(value => value.Resize(document: document, op: op)).IfNone(Fin.Succ(value: (Width: Option<double>.None, Height: Option<double>.None)))
        from _applied in ApplyPageConfig(page: page, size: resized, description: config.Description, op: op)
        from _scaled in config.DetailScale.Map(scale => ScaleAllDetails(document: document, page: page, scale: scale, op: op)).IfNone(Fin.Succ(value: unit))
        select page.MainViewport.Id;

    private static Fin<Unit> ApplyPageConfig(RhinoPageView page, (Option<double> Width, Option<double> Height) size, Option<string> description, Op op) =>
        op.Catch(() => {
            _ = size.Width.Iter(value => page.PageWidth = value);
            _ = size.Height.Iter(value => page.PageHeight = value);
            _ = description.Iter(value => page.Description = value);
            return Fin.Succ(value: unit);
        });

    private static Fin<Unit> ScaleAllDetails(RhinoDoc document, RhinoPageView page, FileScale scale, Op op) =>
        from details in Fin.Succ(value: toSeq(page.GetDetailViews()))
        from _ in guard(!details.IsEmpty, op.InvalidResult()).ToFin()
        from __ in details.TraverseM(detail => scale.Apply(detail: detail, document: document, op: op)).As()
        select unit;

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

    private static Fin<Unit> ApplyGroupMeta(PageViewGroup pageGroup, FileSheetConfig config, Op op) =>
        op.Catch(() => {
            _ = config.GroupDescription.Iter(value => pageGroup.Description = value);
            _ = config.UserStrings.Iter(entry => _ = entry.Value.Case switch {
                string value => pageGroup.SetUserString(key: entry.Key, value: value).Ignore(),
                _ => pageGroup.DeleteUserString(key: entry.Key).Ignore(),
            });
            return Fin.Succ(value: unit);
        });

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
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: numbered.Map(static n => n.Page) + numbered.Bind(static n => n.Details), resources: numbered.Map(static n => DocumentResourceKind.Layout.Change(name: n.Label)));

    private static Fin<Seq<Guid>> NumberDetails(RhinoDoc document, RhinoPageView page, string pattern, Op op) {
        Seq<DetailViewObject> ordered = OrderedDetails(toSeq(page.GetDetailViews()));
        return ordered.Map((detail, index) => (Detail: detail, Value: index + 1)).TraverseM(item =>
            from label in FileEndpoint.NonBlank(value: Substitute(pattern: pattern, value: item.Value), op: op)
            from attributes in Optional(item.Detail.Attributes?.Duplicate()).ToFin(Fail: op.InvalidResult())
            from _ in op.Catch(() => {
                attributes.Name = label;
                return op.Confirm(success: document.Objects.ModifyAttributes(objectId: item.Detail.Id, newAttributes: attributes, quiet: true));
            })
            select item.Detail.Id).As();
    }

    private static string Substitute(string pattern, int value) =>
        NumberPattern.Replace(input: pattern, evaluator: match =>
            match.Groups["fmt"].Success
                ? value.ToString(format: match.Groups["fmt"].Value, provider: CultureInfo.InvariantCulture)
                : value.ToString(provider: CultureInfo.InvariantCulture));

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct DetailMove(Guid SourceId, Guid TargetId, bool Duplicate);

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct FitGrid(int Columns, int Rows, double Width, double Height);

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct DetailFrame(double X, double Y, double Width, double Height) {
        internal bool IsValid =>
            RhinoMath.IsValidDouble(x: X) && RhinoMath.IsValidDouble(x: Y)
            && RhinoMath.IsValidDouble(x: Width) && RhinoMath.IsValidDouble(x: Height)
            && Width > RhinoMath.ZeroTolerance && Height > RhinoMath.ZeroTolerance;
    }

    private static Fin<DocumentReceipt> ArrangeDetails(RhinoDoc document, SheetQuery query, FileDetailLayout layout, Op op) =>
        from _cols in guard(layout.Columns > 0 && layout.SpacingX >= 0.0 && layout.SpacingY >= 0.0 && layout.Margin >= 0.0, op.InvalidInput())
        from pages in query.Resolve(document: document, op: op)
        from _valid in guard(!pages.IsEmpty, op.InvalidInput())
        from arranged in pages.TraverseM(page => ArrangePage(document: document, page: page, layout: layout, op: op)).As()
        let moved = arranged.Bind(static item => item)
        from _any in guard(!moved.IsEmpty, op.InvalidInput())
        select DocumentReceipt.Objects(
            groups: Seq(
                (Slot: DocumentReceiptSlot.Created, Ids: moved.Filter(static m => m.Duplicate).Map(static m => m.TargetId)),
                (Slot: DocumentReceiptSlot.Attributes, Ids: moved.Map(static m => m.TargetId)),
                (Slot: DocumentReceiptSlot.Transformed, Ids: moved.Map(static m => m.TargetId))),
            resources: pages.Map(static page => DocumentResourceKind.Layout.Change(name: page.PageName)));

    private static Fin<DocumentReceipt> GroupAssign(RhinoDoc document, SheetQuery query, string label, bool removeFromOthers, Op op) =>
        from groupName in FileEndpoint.NonBlank(value: label, op: op)
        from pages in query.Resolve(document: document, op: op)
        from _valid in guard(!pages.IsEmpty, op.InvalidInput())
        from target in ResolveOrCreateGroup(document: document, groupName: groupName, pages: pages, op: op)
        from _assigned in removeFromOthers
            ? op.Catch(() => Fin.Succ(value: pages.Iter(page =>
                toSeq(page.GetPageViewGroupList())
                    .Filter(index => index != target.Index)
                    .Iter(index => page.RemoveFromPageViewGroup(pageViewGroupIndex: index)))))
            : Fin.Succ(value: unit)
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: pages.Map(static page => page.MainViewport.Id), resources: pages.Map(static page => DocumentResourceKind.Layout.Change(name: page.PageName))
                + Seq(DocumentResourceKind.PageViewGroup.Change(name: groupName)));

    // FindId returns RhinoObject; clip viewport/participation edits persist through the owning ClippingPlaneObject.
    private static Fin<ClippingPlaneObject> ClipObject(RhinoDoc document, Guid planeId, Op op) =>
        Optional(document.Objects.FindId(id: planeId) as ClippingPlaneObject).ToFin(Fail: op.InvalidInput());

    private static Fin<DetailViewObject> ApplyClipping(DetailViewObject detail, BoundingBox box, Op op) =>
        op.Catch(() => {
            detail.Viewport.SetClippingPlanes(box: box);
            return op.Confirm(success: detail.CommitViewportChanges()).Map(_ => detail);
        });

    private static Fin<DetailViewObject> ApplyDetailView(RhinoDoc document, RhinoPageView page, DetailViewObject detail, Seq<CameraEdit> edits) {
        CameraScope scope = new(Document: document, View: page, Viewport: detail.Viewport, Detail: Some(detail));
        return edits.IsEmpty
            ? Fin.Succ(value: detail)
            : from changes in CameraOps.Change(edits: edits.AsIterable(), redrawEach: false).Run(arg: scope)
              from redraw in changes.Redraw.ApplyTo(scope: scope)
              select detail;
    }

    private static Fin<CameraChangeReceipt> ApplyDetailShot(RhinoDoc document, RhinoPageView page, DetailViewObject detail, CameraShot shot) {
        CameraScope scope = new(Document: document, View: page, Viewport: detail.Viewport, Detail: Some(detail));
        return from changes in CameraOps.Shot(shot: shot, redrawEach: false).Run(arg: scope)
               from redraw in changes.Redraw.ApplyTo(scope: scope)
               select changes.Value;
    }

    private static Fin<Seq<DetailMove>> ArrangePage(RhinoDoc document, RhinoPageView page, FileDetailLayout layout, Op op) =>
        from matched in layout.Detail.Resolve(page: page, op: op)
        let ordered = OrderedDetails(details: matched)
        from frames in ordered.Map((detail, index) => (Detail: detail, Index: index)).TraverseM(item =>
            from current in FrameOf(detail: item.Detail, op: op)
            from target in TargetFrame(page: page, current: current, layout: layout, index: item.Index, count: ordered.Count, op: op)
            select (item.Detail, current, target)).As()
        from moved in frames.TraverseM(item => MoveDetailFrame(document: document, detail: item.Detail, xform: FrameTransform(current: item.current, target: item.target), duplicate: layout.Duplicate, op: op)).As()
        select moved;

    private static Seq<DetailViewObject> OrderedDetails(Seq<DetailViewObject> details, Option<Func<DetailViewObject, bool>> filter = default) {
        Op op = Op.Of(name: nameof(OrderedDetails));
        return toSeq(
            details
                .Filter(d => filter.Map(p => p(arg: d)).IfNone(noneValue: true))
                .Select(d => (Detail: d, Frame: FrameOf(detail: d, op: op)))
                .OrderByDescending(static pair => pair.Frame.Map(static f => f.Y).IfFail(0.0))
                .ThenBy(static pair => pair.Frame.Map(static f => f.X).IfFail(0.0))
                .Select(static pair => pair.Detail));
    }

    private static Fin<DetailFrame> FrameOf(DetailViewObject detail, Op op) {
        BoundingBox box = detail.DetailGeometry switch {
            DetailViewGeometry geometry => geometry.GetBoundingBox(accurate: true),
            _ => BoundingBox.Empty,
        };
        DetailFrame frame = new(X: box.Min.X, Y: box.Min.Y, Width: box.Max.X - box.Min.X, Height: box.Max.Y - box.Min.Y);
        return frame.IsValid ? Fin.Succ(value: frame) : Fin.Fail<DetailFrame>(error: op.InvalidResult());
    }

    private static Fin<DetailFrame> TargetFrame(RhinoPageView page, DetailFrame current, FileDetailLayout layout, int index, int count, Op op) =>
        from fitGrid in layout.Mode == FileDetailLayoutMode.FitPage
            ? ComputeFitGrid(page: page, layout: layout, count: count, op: op)
            : Fin.Succ(value: default(FitGrid))
        from size in TargetSize(layout: layout, current: current, fitGrid: fitGrid, op: op)
        let column = index % layout.Columns
        let row = index / layout.Columns
        let grid = new DetailFrame(
            X: layout.Origin.X + (column * layout.SpacingX),
            Y: layout.Origin.Y + (row * layout.SpacingY),
            Width: size.Width,
            Height: size.Height)
        from target in layout.Mode switch {
            FileDetailLayoutMode.Grid => Fin.Succ(value: grid),
            FileDetailLayoutMode.FitPage => Fin.Succ(value: new DetailFrame(
                X: layout.Margin + (index % fitGrid.Columns * (size.Width + layout.SpacingX)),
                Y: layout.Margin + (index / fitGrid.Columns * (size.Height + layout.SpacingY)),
                Width: size.Width,
                Height: size.Height)),
            FileDetailLayoutMode.Align => Fin.Succ(value: AnchorFrame(origin: layout.Origin, anchor: layout.Anchor, size: size)),
            FileDetailLayoutMode.DistributeHorizontal => Fin.Succ(value: new DetailFrame(
                X: count <= 1 ? layout.Margin : layout.Margin + (index * (Math.Max(0.0, page.PageWidth - (2.0 * layout.Margin) - size.Width) / (count - 1))),
                Y: current.Y,
                Width: size.Width,
                Height: size.Height)),
            FileDetailLayoutMode.DistributeVertical => Fin.Succ(value: new DetailFrame(
                X: current.X,
                Y: count <= 1 ? layout.Margin : layout.Margin + (index * (Math.Max(0.0, page.PageHeight - (2.0 * layout.Margin) - size.Height) / (count - 1))),
                Width: size.Width,
                Height: size.Height)),
            _ => Fin.Fail<DetailFrame>(error: op.InvalidResult()),
        }
        from valid in target.IsValid ? Fin.Succ(value: target) : Fin.Fail<DetailFrame>(error: op.InvalidResult())
        select valid;

    private static Fin<(double Width, double Height)> TargetSize(FileDetailLayout layout, DetailFrame current, FitGrid fitGrid, Op op) =>
        layout.Mode == FileDetailLayoutMode.FitPage
            ? Fin.Succ(value: (fitGrid.Width, fitGrid.Height))
            : layout.Size.Case switch {
                Point2d size when size.X > RhinoMath.ZeroTolerance && size.Y > RhinoMath.ZeroTolerance => Fin.Succ(value: (Width: size.X, Height: size.Y)),
                Point2d => Fin.Fail<(double Width, double Height)>(error: op.InvalidInput()),
                _ => Fin.Succ(value: (current.Width, current.Height)),
            };

    private static Fin<FitGrid> ComputeFitGrid(RhinoPageView page, FileDetailLayout layout, int count, Op op) {
        int cols = Math.Min(layout.Columns, Math.Max(count, 1));
        int rows = (int)Math.Ceiling(count / (double)cols);
        double w = (page.PageWidth - (2.0 * layout.Margin) - ((cols - 1) * layout.SpacingX)) / cols;
        double h = (page.PageHeight - (2.0 * layout.Margin) - ((rows - 1) * layout.SpacingY)) / rows;
        return guard(w > RhinoMath.ZeroTolerance && h > RhinoMath.ZeroTolerance, op.InvalidInput())
            .ToFin().Map(_ => new FitGrid(Columns: cols, Rows: rows, Width: w, Height: h));
    }

    private static DetailFrame AnchorFrame(Point2d origin, FileDetailAnchor? anchor, (double Width, double Height) size) {
        FileDetailAnchor a = anchor ?? FileDetailAnchor.TopLeft;
        return new DetailFrame(
            X: origin.X - (a.XFactor * size.Width),
            Y: origin.Y - ((1.0 - a.YFactor) * size.Height),
            Width: size.Width,
            Height: size.Height);
    }

    private static Fin<DetailMove> MoveDetailFrame(RhinoDoc document, DetailViewObject detail, Transform xform, bool duplicate, Op op) =>
        duplicate
            ? DocumentTarget.IdResult(id: document.Objects.Transform(obj: detail, xform: xform, deleteOriginal: false), op: op)
                .Map(id => new DetailMove(SourceId: detail.Id, TargetId: id, Duplicate: true))
            : op.Catch(() =>
                detail.DetailGeometry.Transform(xform)
                && detail.CommitChanges()
                    ? Fin.Succ(value: new DetailMove(SourceId: detail.Id, TargetId: detail.Id, Duplicate: false))
                    : Fin.Fail<DetailMove>(error: op.InvalidResult()));

    private static Transform FrameTransform(DetailFrame current, DetailFrame target) {
        double scaleX = target.Width / current.Width;
        double scaleY = target.Height / current.Height;
        Plane origin = new(new Point3d(x: current.X, y: current.Y, z: 0.0), Vector3d.ZAxis);
        Transform scale = Transform.Scale(plane: origin, xScaleFactor: scaleX, yScaleFactor: scaleY, zScaleFactor: 1.0);
        // Scale about the current lower-left corner lands it at (current.X * scaleX, current.Y * scaleY);
        // translate the residual so it lands at (target.X, target.Y).
        Transform translate = Transform.Translation(
            motion: new Vector3d(
                x: target.X - (current.X * scaleX),
                y: target.Y - (current.Y * scaleY),
                z: 0.0));
        return translate * scale;
    }

    // BOUNDARY ADAPTER: binds positionally to both TryGetPaperLength (first param "paper") and TryGetModelLength (first param "model"); "input" names the shared model-domain source neutrally.
    private delegate bool TryLength(double input, out double result);

    private static Option<double> Length(double value, TryLength convert) =>
        convert(input: value, result: out double result) && RhinoMath.IsValidDouble(x: result) && result > 0.0
            ? Some(result)
            : Option<double>.None;

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

    private static FileDetailReport DetailReportOf(RhinoDoc document, DetailViewObject detail) {
        Guid viewportId = detail.Viewport.Id;
        Seq<string> overridden = toSeq(document.Layers)
            .Filter(layer => !layer.IsDeleted && layer.HasPerViewportSettings(viewportId: viewportId))
            .Map(static layer => layer.FullPath);
        bool hasRange = detail.Viewport.GetFrustum(out _, out _, out _, out _, out double near, out double far);
        return new FileDetailReport(
            Name: DetailQuery.NameOf(detail: detail).IfNone(string.Empty),
            DescriptiveTitle: detail.DescriptiveTitle,
            Scale: FileScale.Format(detail: detail),
            Projection: ProjectionOf(detail: detail),
            ViewportId: viewportId,
            ParentViewportId: Optional(detail.ParentPageView).Map(static page => page.MainViewport.Id),
            Active: detail.IsActive,
            ProjectionLocked: detail.DetailGeometry.IsProjectionLocked,
            PageToModelRatio: detail.DetailGeometry.PageToModelRatio,
            WorldToPage: detail.WorldToPageTransform,
            PageToWorld: detail.PageToWorldTransform,
            PaperLengthPerModelUnit: Length(value: 1.0, convert: detail.TryGetPaperLength),
            ModelLengthPerPaperUnit: Length(value: 1.0, convert: detail.TryGetModelLength),
            ClippingPlanes: toSeq(document.Objects.FindClippingPlanesForViewport(viewport: detail.Viewport))
                .Choose(static plane => Optional(plane.ClippingPlaneGeometry).Map(geom => {
                    // BOUNDARY ADAPTER — GetClipParticipation projects participation through out-params; null lists default to empty.
                    geom.GetClipParticipation(objectIds: out IEnumerable<Guid> objIds, layerIndices: out IEnumerable<int> layerIdxs, isExclusionList: out bool exclusive);
                    return new FileClipReport(
                        Id: plane.Id,
                        Plane: geom.Plane,
                        ObjectIds: toSeq(objIds ?? []),
                        LayerIndices: toSeq(layerIdxs ?? []),
                        Exclusive: exclusive);
                })),
            OverriddenLayers: overridden,
            ClipRange: hasRange && RhinoMath.IsValidDouble(x: near) && RhinoMath.IsValidDouble(x: far)
                ? Some((Near: near, Far: far))
                : Option<(double, double)>.None);
    }

    private static Fin<FileSheetReport> ReportOf(RhinoDoc document, RhinoPageView page, DetailQuery detail, Op op) =>
        op.Catch(() => {
            Seq<PageViewGroup> groups = toSeq(page.GetPageViewGroupList()).Map(index => Optional(document.PageViewGroups.FindIndex(index: index))).Somes();
            Guid activeDetailId = page.ActiveDetailId;
            Fin<Seq<DetailViewObject>> resolved = detail.IsAll ? Fin.Succ(value: toSeq(page.GetDetailViews())) : detail.Resolve(page: page, op: op);
            return resolved.Map(detailViews => new FileSheetReport(
                Name: page.PageName,
                Number: page.PageNumber,
                PrinterName: FileArchiveOps.TextOption(value: page.PrinterName),
                PaperName: FileArchiveOps.TextOption(value: page.PaperName),
                Active: page.MainViewport.Id == document.Views.ActiveView?.MainViewport.Id,
                ActiveDetailId: activeDetailId == Guid.Empty ? Option<Guid>.None : Some(activeDetailId),
                Size: (Width: page.PageWidth, Height: page.PageHeight),
                Groups: groups.Map(static item => item.Name),
                Details: detailViews.Map(detail => DetailReportOf(document: document, detail: detail)),
                GroupMetadata: groups.Map(static item => (Group: item.Name, Strings: UserStringsOf(pageGroup: item)))));
        });

}

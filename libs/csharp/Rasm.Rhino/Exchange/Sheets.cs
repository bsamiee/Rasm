using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FileDetailLayoutMode {
    public static readonly FileDetailLayoutMode Grid = new(key: 0, frame: static c =>
        Fin.Succ(value: SheetOps.GridCell(content: c.Content, layout: c.Layout, size: c.Size, index: c.Index, columns: c.Layout.Columns, spacingX: c.Layout.SpacingX, spacingY: c.Layout.SpacingY, fromOrigin: true)));
    public static readonly FileDetailLayoutMode FitPage = new(key: 1, frame: static c =>
        Fin.Succ(value: SheetOps.GridCell(content: c.Content, layout: c.Layout, size: c.Size, index: c.Index, columns: c.FitGrid.Columns, spacingX: c.Layout.SpacingX, spacingY: c.Layout.SpacingY, fromOrigin: false)));
    public static readonly FileDetailLayoutMode Align = new(key: 2, frame: static c =>
        Fin.Succ(value: SheetOps.AnchorFrame(content: c.Content, layout: c.Layout, size: c.Size)));
    public static readonly FileDetailLayoutMode DistributeHorizontal = new(key: 3, frame: static c =>
        Fin.Succ(value: new FileDetailFrame(
            X: c.Count <= 1 ? c.Content.X : c.Content.X + (c.Index * (Math.Max(val1: 0.0, val2: c.Content.Width - c.Size.Width) / (c.Count - 1))),
            Y: c.Current.Y, Width: c.Size.Width, Height: c.Size.Height)));
    public static readonly FileDetailLayoutMode DistributeVertical = new(key: 4, frame: static c =>
        Fin.Succ(value: new FileDetailFrame(
            X: c.Current.X,
            Y: c.Count <= 1 ? c.Content.Y : c.Content.Y + (c.Index * (Math.Max(val1: 0.0, val2: c.Content.Height - c.Size.Height) / (c.Count - 1))),
            Width: c.Size.Width, Height: c.Size.Height)));

    [UseDelegateFromConstructor] internal partial Fin<FileDetailFrame> Frame(LayoutContext c);

    internal bool NeedsFitGrid => this == FitPage;
}

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

[SmartEnum<int>]
public sealed partial class NamedViewOp {
    public static readonly NamedViewOp Save = new(key: 0, apply: static (doc, detail, name, op) =>
        op.Catch(() => doc.NamedViews.Add(name: name, viewportId: detail.Viewport.Id) switch {
            int created when created >= 0 => Fin.Succ(value: unit),
            _ => Fin.Fail<Unit>(error: op.InvalidResult()),
        }));
    public static readonly NamedViewOp Restore = new(key: 1, apply: static (doc, detail, name, op) =>
        op.Catch(() =>
            from index in doc.NamedViews.FindByName(name: name) switch {
                int found when found >= 0 => Fin.Succ(value: found),
                _ => Fin.Fail<int>(error: op.MissingContext()),
            }
            from _r in op.Confirm(success: doc.NamedViews.RestoreWithAspectRatio(index: index, viewport: detail.Viewport))
            from _c in new DetailCommit(Viewport: true).Apply(detail: detail, op: op)
            select unit));

    [UseDelegateFromConstructor] internal partial Fin<Unit> Apply(RhinoDoc doc, DetailViewObject detail, string name, Op op);
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record LayerOverrideField {
    private LayerOverrideField() { }
    public sealed record Color(FileOverride<DrawingColor> Op) : LayerOverrideField;
    public sealed record Visible(FileOverride<bool> Op) : LayerOverrideField;
    public sealed record PersistentVisible(FileOverride<bool> Op) : LayerOverrideField;
    public sealed record PlotColor(FileOverride<DrawingColor> Op) : LayerOverrideField;
    public sealed record PlotWeight(FileOverride<double> Op) : LayerOverrideField;

    internal int Key => Switch(color: static _ => 0, visible: static _ => 1, persistentVisible: static _ => 2, plotColor: static _ => 3, plotWeight: static _ => 4);

    internal bool IsActive => Switch(
        color: static f => f.Op.IsActive,
        visible: static f => f.Op.IsActive,
        persistentVisible: static f => f.Op.IsActive,
        plotColor: static f => f.Op.IsActive,
        plotWeight: static f => f.Op.IsActive);

    internal LayerOverrideField Merge(LayerOverrideField right) => (this, right) switch {
        (Color l, Color r) => new Color(Op: l.Op | r.Op),
        (Visible l, Visible r) => new Visible(Op: l.Op | r.Op),
        (PersistentVisible l, PersistentVisible r) => new PersistentVisible(Op: l.Op | r.Op),
        (PlotColor l, PlotColor r) => new PlotColor(Op: l.Op | r.Op),
        (PlotWeight l, PlotWeight r) => new PlotWeight(Op: l.Op | r.Op),
        _ => right,
    };

    internal Unit Apply(Layer layer, Guid viewport) => Switch(
        (Layer: layer, Viewport: viewport),
        color: static (ctx, f) => f.Op.Apply(
            set: value => ctx.Layer.SetPerViewportColor(viewportId: ctx.Viewport, color: value),
            inherit: () => ctx.Layer.DeletePerViewportColor(viewportId: ctx.Viewport)),
        visible: static (ctx, f) => f.Op.Apply(
            set: value => ctx.Layer.SetPerViewportVisible(viewportId: ctx.Viewport, visible: value),
            inherit: () => ctx.Layer.DeletePerViewportVisible(viewportId: ctx.Viewport)),
        persistentVisible: static (ctx, f) => f.Op.Apply(
            set: value => ctx.Layer.SetPerViewportPersistentVisibility(viewportId: ctx.Viewport, persistentVisibility: value),
            inherit: () => ctx.Layer.UnsetPerViewportPersistentVisibility(viewportId: ctx.Viewport)),
        plotColor: static (ctx, f) => f.Op.Apply(
            set: value => ctx.Layer.SetPerViewportPlotColor(viewportId: ctx.Viewport, color: value),
            inherit: () => ctx.Layer.DeletePerViewportPlotColor(viewportId: ctx.Viewport)),
        plotWeight: static (ctx, f) => f.Op.Apply(
            set: value => ctx.Layer.SetPerViewportPlotWeight(viewportId: ctx.Viewport, plotWeight: value),
            inherit: () => ctx.Layer.DeletePerViewportPlotWeight(viewportId: ctx.Viewport)));
}

[SmartEnum<int>]
public sealed partial class CommitTarget {
    public static readonly CommitTarget Geometry = new(key: 0, commit: static d => d.CommitChanges());
    public static readonly CommitTarget Viewport = new(key: 1, commit: static d => d.CommitViewportChanges());
    [UseDelegateFromConstructor] internal partial bool Commit(DetailViewObject detail);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct DetailCommit(bool Geometry = false, bool Viewport = false) {
    public static DetailCommit Of(CommitTarget target) => new(Geometry: target == CommitTarget.Geometry, Viewport: target == CommitTarget.Viewport);

    public static DetailCommit operator |(DetailCommit left, DetailCommit right) => new(Geometry: left.Geometry || right.Geometry, Viewport: left.Viewport || right.Viewport);

    // Geometry commits before viewport (CommitTarget key order), each at most once.
    internal Fin<Unit> Apply(DetailViewObject detail, Op op) =>
        ((Geometry ? Seq(CommitTarget.Geometry) : Seq<CommitTarget>()) + (Viewport ? Seq(CommitTarget.Viewport) : Seq<CommitTarget>())) switch {
            { IsEmpty: true } => Fin.Succ(value: unit),
            Seq<CommitTarget> targets => targets.TraverseM(target => op.Confirm(success: target.Commit(detail: detail))).As().Map(static _ => unit),
        };
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record DetailConfigOp {
    private DetailConfigOp() { }
    public sealed record LockProjection(bool Value) : DetailConfigOp;
    public sealed record LockCamera(bool Value) : DetailConfigOp;
    public sealed record SetDisplay(Guid Mode) : DetailConfigOp;
    public sealed record SetProjection(DefinedViewportProjection Projection) : DetailConfigOp;
    public sealed record Rename(string Name) : DetailConfigOp;

    internal Fin<Option<CommitTarget>> Apply(RhinoDoc document, DetailViewObject view, Op op) => Switch(
        (Doc: document, View: view, Op: op),
        lockProjection: static (ctx, c) => ctx.Op.Catch(() => { ctx.View.DetailGeometry.IsProjectionLocked = c.Value; return Fin.Succ(value: Some(CommitTarget.Geometry)); }),
        lockCamera: static (ctx, c) => ctx.Op.Catch(() => { ctx.View.Viewport.LockedProjection = c.Value; return Fin.Succ(value: Some(CommitTarget.Viewport)); }),
        setDisplay: static (ctx, c) =>
            from mode in Optional(DisplayModeDescription.GetDisplayMode(id: c.Mode)).ToFin(Fail: ctx.Op.InvalidInput())
            from _set in ctx.Op.Catch(() => { ctx.View.Viewport.DisplayMode = mode; return Fin.Succ(value: unit); })
            select Some(CommitTarget.Viewport),
        setProjection: static (ctx, c) => ctx.Op.Catch(() =>
            ctx.Op.Confirm(success: ctx.View.Viewport.SetProjection(projection: c.Projection, viewName: ctx.View.Viewport.Name, updateConstructionPlane: false))
                .Map(_ => Some(CommitTarget.Viewport))),
        rename: static (ctx, c) =>
            from name in FileEndpoint.NonBlank(value: c.Name, op: ctx.Op)
            from _named in ctx.Op.Catch(() => {
                ObjectAttributes attributes = ctx.View.Attributes.Duplicate();
                attributes.Name = name;
                return ctx.Op.Confirm(success: ctx.Doc.Objects.ModifyAttributes(objectId: ctx.View.Id, newAttributes: attributes, quiet: true));
            })
            select Option<CommitTarget>.None);
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
    public sealed record Manage(FileSheetPlan Plan) : FileSheetEdit;
    public sealed record Remove(string SheetName) : FileSheetEdit;
    public sealed record Duplicate(string SheetName, bool WithGeometry = true) : FileSheetEdit;
    public sealed record Rename(string SheetName, string NewName) : FileSheetEdit;
    public sealed record Reorder(Seq<string> SheetNames) : FileSheetEdit;
    public sealed record AddDetail(string SheetName, FileDetailSpec Spec) : FileSheetEdit;
    public sealed record Resize(string SheetName, Option<FileSheetSize> Size = default, Option<string> Description = default) : FileSheetEdit;
    public sealed record PageUnits(UnitSystem Units, bool Scale = true) : FileSheetEdit;
    public sealed record Import(FileEndpoint Source, Guid SourceViewportId, string Name) : FileSheetEdit;
    public sealed record RemoveDetail(string SheetName, DetailQuery Detail) : FileSheetEdit;
    public sealed record ActivateDetail(string SheetName, Option<DetailQuery> Detail = default) : FileSheetEdit;
    public sealed record LayerOverride(string SheetName, DetailQuery Detail, Seq<FileLayerOverride> Overrides) : FileSheetEdit;
    public sealed record ClipDetail(string SheetName, DetailQuery Detail, FileClipOp Op) : FileSheetEdit;
    public sealed record NamedDetailView(string SheetName, DetailQuery Detail, string ViewName, NamedViewOp Op) : FileSheetEdit;
    public sealed record FrameDetail(string SheetName, DetailQuery Detail, Seq<CameraEdit> Edits, Option<FileScale> Scale = default, Option<BoundingBox> Clipping = default) : FileSheetEdit;
    public sealed record ComposeDetail(string SheetName, DetailQuery Detail, CameraShot Shot, Option<FileScale> Scale = default) : FileSheetEdit;
    public sealed record Configure(SheetQuery Query, FileSheetConfig Config) : FileSheetEdit;
    public sealed record ConfigureDetail(string SheetName, DetailQuery Detail, Seq<DetailConfigOp> Ops) : FileSheetEdit;
    public sealed record Number(SheetQuery Query, FileNumbering Numbering) : FileSheetEdit;
    public sealed record ArrangeDetails(SheetQuery Query, FileDetailLayout Layout) : FileSheetEdit;
    public sealed record GroupAssign(SheetQuery Query, string Group, bool RemoveFromOthers = false) : FileSheetEdit;
}

[Union]
public abstract partial record ScaleConflict {
    private ScaleConflict() { }
    public sealed record RatioMismatch(string Sheet, string Detail, string Formatted, double LiveRatio, double DeclaredRatio) : ScaleConflict;
    public sealed record PerspectiveScale(string Sheet, string Detail) : ScaleConflict;
    public sealed record PageUnitDrift(string Sheet, string Detail, UnitSystem PageUnits) : ScaleConflict;
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
    Seq<FileUserString> UserStrings = default,
    Option<string> GroupDescription = default) {
    internal bool Applies =>
        Size.IsSome || Group.IsSome || Description.IsSome || DetailScale.IsSome
        || !UserStrings.IsEmpty || GroupDescription.IsSome;
}

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

public readonly record struct FileDetailPlan(
    FileDetailSpec Spec,
    Seq<CameraEdit> Edits = default,
    Option<FileScale> Scale = default,
    Option<BoundingBox> Clipping = default,
    Seq<DetailConfigOp> Config = default);

public readonly record struct FileSheetPlan(
    FileSheetSpec Sheet,
    FileSheetConfig Config = default,
    Seq<FileDetailPlan> Details = default,
    Option<FileDetailLayout> Layout = default,
    Option<FileNumbering> Numbering = default,
    bool ReplaceExisting = false);

public readonly record struct FileDetailLayout(
    int Columns = 1,
    double SpacingX = 0.0,
    double SpacingY = 0.0,
    Point2d Origin = default,
    DetailQuery Detail = default,
    Option<Point2d> Size = default,
    FileDetailLayoutMode? Mode = null,
    FileDetailAnchor? Anchor = null,
    double Margin = 0.0,
    Option<CaptureMargins> Margins = default,
    bool Duplicate = false);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FileDetailFrame(double X, double Y, double Width, double Height) {
    internal bool IsValid =>
        RhinoMath.IsValidDouble(x: X) && RhinoMath.IsValidDouble(x: Y)
        && RhinoMath.IsValidDouble(x: Width) && RhinoMath.IsValidDouble(x: Height)
        && Width > RhinoMath.ZeroTolerance && Height > RhinoMath.ZeroTolerance;

    internal Point2d AnchorPoint(FileDetailAnchor anchor, Point2d offset) =>
        new(x: X + (anchor.XFactor * Width) + offset.X, y: Y + (anchor.YFactor * Height) + offset.Y);

    internal static Fin<FileDetailFrame> Of(DetailViewObject detail, Op op) =>
        Optional(detail)
            .Bind(active => Optional(active.DetailGeometry))
            .Map(static geometry => geometry.GetBoundingBox(accurate: true))
            .Filter(static box => box.IsValid && box.Max.X > box.Min.X && box.Max.Y > box.Min.Y)
            .Map(static box => new FileDetailFrame(X: box.Min.X, Y: box.Min.Y, Width: box.Max.X - box.Min.X, Height: box.Max.Y - box.Min.Y))
            .ToFin(Fail: op.InvalidResult());
}

[StructLayout(LayoutKind.Auto)]
internal readonly record struct FitGrid(int Columns, int Rows, double Width, double Height);

[StructLayout(LayoutKind.Auto)]
internal readonly record struct LayoutContext(FileDetailFrame Content, FileDetailFrame Current, FileDetailLayout Layout, FitGrid FitGrid, (double Width, double Height) Size, int Index, int Count, Op Op);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FileLayerOverride(string LayerPath, Seq<LayerOverrideField> Fields = default, bool ResetAll = false) {
    public static FileLayerOverride Reset(string path) => new(LayerPath: path, ResetAll: true);

    internal bool HasFieldOperation => Fields.Exists(static entry => entry.IsActive);

    internal bool Applies => ResetAll || HasFieldOperation;

    public static FileLayerOverride operator |(FileLayerOverride left, FileLayerOverride right) =>
        right.ResetAll
            ? right
            : right.HasFieldOperation
                ? left with {
                    Fields = right.Fields.Fold(initialState: left.Fields, f: static (merged, field) =>
                        merged.Exists(existing => existing.Key == field.Key)
                            ? merged.Map(slot => slot.Key == field.Key ? slot.Merge(right: field) : slot)
                            : merged.Add(field)),
                }
                : left;
}

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
    FileDetailFrame Frame,
    Option<double> PageToModelRatio,
    Transform WorldToPage,
    Transform PageToWorld,
    Option<double> PaperLengthPerModelUnit,
    Option<double> ModelLengthPerPaperUnit,
    Seq<FileClipReport> ClippingPlanes,
    Seq<string> OverriddenLayers,
    Option<(double Left, double Right, double Bottom, double Top)> Frustum,
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
    [GeneratedRegex(pattern: "\\{(?<tok>[ng])(?::(?<fmt>[^{}]+))?\\}", options: RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    private static partial Regex TokenPattern { get; }

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
            manage: static (ctx, e) => Manage(document: ctx.Document, plan: e.Plan, op: ctx.Op),
            remove: static (ctx, e) => RemoveSheet(document: ctx.Document, sheetName: e.SheetName, op: ctx.Op),
            duplicate: static (ctx, e) => DuplicateSheet(document: ctx.Document, sheetName: e.SheetName, withGeometry: e.WithGeometry, op: ctx.Op),
            rename: static (ctx, e) => RenameSheet(document: ctx.Document, sheetName: e.SheetName, newName: e.NewName, op: ctx.Op),
            reorder: static (ctx, e) => ReorderSheets(document: ctx.Document, sheetNames: e.SheetNames, op: ctx.Op),
            addDetail: static (ctx, e) => AddDetail(document: ctx.Document, sheetName: e.SheetName, spec: e.Spec, op: ctx.Op),
            resize: static (ctx, e) => ResizeSheet(document: ctx.Document, sheetName: e.SheetName, size: e.Size, description: e.Description, op: ctx.Op),
            pageUnits: static (ctx, e) =>
                from valid in guard(e.Units is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits, ctx.Op.InvalidInput()).ToFin()
                from _set in ctx.Op.Catch(() => { ctx.Document.AdjustPageUnitSystem(newUnitSystem: e.Units, scale: e.Scale); return Fin.Succ(value: unit); })
                select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes,
                    ids: toSeq(ctx.Document.Views.GetPageViews()).Map(static page => page.MainViewport.Id),
                    resources: toSeq(ctx.Document.Views.GetPageViews()).Map(static page => DocumentResourceKind.Layout.Change(name: page.PageName))),
            import: static (ctx, e) => ImportSheet(document: ctx.Document, source: e.Source, sourceViewportId: e.SourceViewportId, name: e.Name, op: ctx.Op),
            removeDetail: static (ctx, e) => RemoveDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, op: ctx.Op),
            activateDetail: static (ctx, e) => ActivateDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, op: ctx.Op),
            layerOverride: static (ctx, e) => ApplyLayerOverride(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, overrides: e.Overrides, op: ctx.Op),
            clipDetail: static (ctx, e) => ApplyClipDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, clipOp: e.Op, op: ctx.Op),
            namedDetailView: static (ctx, e) => NamedDetailView(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, viewName: e.ViewName, mode: e.Op, op: ctx.Op),
            frameDetail: static (ctx, e) => FrameDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, edits: e.Edits, scale: e.Scale, clipping: e.Clipping, op: ctx.Op),
            composeDetail: static (ctx, e) => ComposeDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, shot: e.Shot, scale: e.Scale, op: ctx.Op),
            configure: static (ctx, e) => Configure(document: ctx.Document, query: e.Query, config: e.Config, op: ctx.Op),
            configureDetail: static (ctx, e) => ConfigureDetail(document: ctx.Document, sheetName: e.SheetName, detail: e.Detail, ops: e.Ops, op: ctx.Op),
            number: static (ctx, e) => Number(document: ctx.Document, query: e.Query, numbering: e.Numbering, op: ctx.Op),
            arrangeDetails: static (ctx, e) => ArrangeDetails(document: ctx.Document, query: e.Query, layout: e.Layout, op: ctx.Op),
            groupAssign: static (ctx, e) => GroupAssign(document: ctx.Document, query: e.Query, label: e.Group, removeFromOthers: e.RemoveFromOthers, op: ctx.Op));

    internal static Fin<Seq<FileSheetReport>> Inspect(RhinoDoc document, SheetQuery query, Op op, DetailQuery detail = default) =>
        from pages in query.Resolve(document: document, op: op)
        from reports in pages.TraverseM(page => ReportOf(document: document, page: page, detail: detail, op: op)).As()
        select reports;

    // Pure correlation over already-materialized reports: the three scale axes (declared formatted scale,
    // live parallel PageToModelRatio, document page-unit space) only conflict when read together, so the
    // audit folds the conflict family out of the report projection without re-reading the host.
    internal static Seq<ScaleConflict> ScaleAudit(Seq<FileSheetReport> reports, UnitSystem pageUnits, double tolerance = 1e-6) =>
        reports.Bind(sheet => sheet.Details.Bind(detail => ConflictsOf(sheet: sheet.Name, detail: detail, pageUnits: pageUnits, tolerance: tolerance)));

    private static Seq<ScaleConflict> ConflictsOf(string sheet, FileDetailReport detail, UnitSystem pageUnits, double tolerance) =>
        (detail.Scale.Case, detail.PageToModelRatio.Case) switch {
            (string formatted, double live) =>
                (DeclaredRatio(formatted: formatted).Filter(declared => Math.Abs(value: live - declared) > tolerance).Case switch {
                    double declared => Seq<ScaleConflict>(new ScaleConflict.RatioMismatch(Sheet: sheet, Detail: detail.Name, Formatted: formatted, LiveRatio: live, DeclaredRatio: declared)),
                    _ => Seq<ScaleConflict>(),
                }) + DriftOf(sheet: sheet, detail: detail.Name, pageUnits: pageUnits),
            (string, _) => Seq<ScaleConflict>(new ScaleConflict.PerspectiveScale(Sheet: sheet, Detail: detail.Name)),
            _ => Seq<ScaleConflict>(),
        };

    private static Seq<ScaleConflict> DriftOf(string sheet, string detail, UnitSystem pageUnits) =>
        pageUnits is UnitSystem.None or UnitSystem.Unset
            ? Seq<ScaleConflict>(new ScaleConflict.PageUnitDrift(Sheet: sheet, Detail: detail, PageUnits: pageUnits))
            : Seq<ScaleConflict>();

    private static Option<double> DeclaredRatio(string formatted) {
        using ScaleValue scale = ScaleValue.Create(s: formatted, ps: StringParserSettings.DefaultParseSettings);
        return scale is { } value && !value.IsUnset()
            ? Op.Of().Catch(() => {
                using LengthValue page = value.LeftLengthValue();
                using LengthValue model = value.RightLengthValue();
                double pageLen = page.Length(units: page.Units);
                double modelLen = model.Length(units: model.Units);
                return RhinoMath.IsValidDouble(x: pageLen) && RhinoMath.IsValidDouble(x: modelLen) && modelLen > 0.0
                    ? Fin.Succ(value: pageLen / modelLen)
                    : Fin.Fail<double>(error: Op.Of().InvalidResult());
            }).ToOption()
            : Option<double>.None;
    }

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

    private static Fin<DocumentReceipt> Manage(RhinoDoc document, FileSheetPlan plan, Op op) =>
        from name in FileEndpoint.NonBlank(value: plan.Sheet.Name, op: op)
        let query = new SheetQuery(Name: Some(name))
        from removed in plan.ReplaceExisting switch {
            true => toSeq(document.Views.GetPageViews())
                .Find(view => string.Equals(a: view.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase))
                .Case switch {
                    RhinoPageView => RemoveSheet(document: document, sheetName: name, op: op),
                    _ => Fin.Succ(value: DocumentReceipt.Empty),
                },
            false => Fin.Succ(value: DocumentReceipt.Empty),
        }
        from created in CreateSheet(document: document, spec: plan.Sheet with { Name = name }, op: op)
        from configured in plan.Config.Applies
            ? Configure(document: document, query: query, config: plan.Config, op: op)
            : Fin.Succ(value: DocumentReceipt.Empty)
        from createdDetails in plan.Details.TraverseM(detail => AddDetail(document: document, sheetName: name, spec: detail.Spec, op: op)).As()
        from framedDetails in plan.Details.TraverseM(detail =>
            !detail.Edits.IsEmpty || detail.Scale.IsSome || detail.Clipping.IsSome
                ? FrameDetail(document: document, sheetName: name, detail: DetailQuery.Named(name: detail.Spec.Name), edits: detail.Edits, scale: detail.Scale, clipping: detail.Clipping, op: op)
                : Fin.Succ(value: DocumentReceipt.Empty)).As()
        from configuredDetails in plan.Details.TraverseM(detail =>
            !detail.Config.IsEmpty
                ? ConfigureDetail(document: document, sheetName: name, detail: DetailQuery.Named(name: detail.Spec.Name), ops: detail.Config, op: op)
                : Fin.Succ(value: DocumentReceipt.Empty)).As()
        from arranged in plan.Layout.Map(layout => ArrangeDetails(document: document, query: query, layout: layout, op: op)).IfNone(Fin.Succ(value: DocumentReceipt.Empty))
        from numbered in plan.Numbering.Map(numbering => Number(document: document, query: query, numbering: numbering, op: op)).IfNone(Fin.Succ(value: DocumentReceipt.Empty))
        select (Seq(removed, created, configured) + createdDetails + framedDetails + configuredDetails + Seq(arranged, numbered))
            .Fold(initialState: DocumentReceipt.Empty, f: static (state, receipt) => state + receipt);

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
                       from committed in new DetailCommit(Geometry: true).Apply(detail: scaledDetail, op: op)
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
        from pruned in details.TraverseM(item => PruneClips(document: document, detail: item, op: op)).As()
        from _ in details.TraverseM(item => op.Confirm(success: document.Objects.Delete(obj: item, quiet: true))).As()
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Deleted, ids: details.Map(static item => item.Id) + pruned.Bind(static p => p), resources: (names.IsEmpty ? Seq(DocumentResourceKind.Layout.Change(name: sheetName))
                    : names.Map(static name => DocumentResourceKind.Layout.Change(name: name)))
                .Distinct());

    private static Fin<Seq<Guid>> PruneClips(RhinoDoc document, DetailViewObject detail, Op op) =>
        op.Catch(() => Fin.Succ(value: toSeq(document.Objects.FindClippingPlanesForViewport(viewport: detail.Viewport))
            .Choose(plane => Optional(plane.ClippingPlaneGeometry).Map(geom => {
                Guid[] viewports = geom.ViewportIds();
                // Sole binding to the removed detail's viewport -> delete the orphan; a shared plane only detaches.
                _ = viewports is [Guid only] && only == detail.Viewport.Id
                    ? Op.Side(() => document.Objects.Delete(objectId: plane.Id, quiet: true))
                    : Op.Side(() => plane.RemoveClipViewport(viewport: detail.Viewport, commit: true));
                return plane.Id;
            }))));

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
        from _valid in guard(!overrides.IsEmpty && overrides.ForAll(static entry => entry.Applies), op.InvalidInput())
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (page, detail) =>
            overrides.TraverseM(entry =>
                from path in FileEndpoint.NonBlank(value: entry.LayerPath, op: op)
                from layer in document.Layers.FindByFullPath(layerPath: path, notFoundReturnValue: -1) switch {
                    int index when index >= 0 => Optional(document.Layers[index]).ToFin(Fail: op.InvalidInput()),
                    _ => Fin.Fail<Layer>(error: op.InvalidInput()),
                }
                from _reset in entry.ResetAll
                    ? op.Catch(() => Fin.Succ(value: Op.Side(() => layer.DeletePerViewportSettings(viewportId: detail.Viewport.Id))))
                    : Fin.Succ(value: unit)
                from _fields in op.Catch(() => Fin.Succ(value: entry.Fields.Iter(field =>
                    field.Apply(layer: layer, viewport: detail.Viewport.Id))))
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
                    let attached = toSeq(c.Document.Objects.FindClippingPlanesForViewport(viewport: view.Viewport)).Exists(plane => plane.Id == cmd.PlaneId)
                    from _toggled in (cmd.Active, attached) switch {
                        (true, true) or (false, false) => Fin.Succ(value: unit),
                        (true, false) => c.Op.Confirm(success: clip.AddClipViewport(viewport: view.Viewport, commit: true)),
                        (false, true) => c.Op.Confirm(success: clip.RemoveClipViewport(viewport: view.Viewport, commit: true)),
                    }
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

    private static Fin<DocumentReceipt> NamedDetailView(RhinoDoc document, string sheetName, DetailQuery detail, string viewName, NamedViewOp mode, Op op) =>
        from name in FileEndpoint.NonBlank(value: viewName, op: op)
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (_, view) =>
            from _applied in mode.Apply(doc: document, detail: view, name: name, op: op)
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

    private static Fin<DocumentReceipt> ConfigureDetail(RhinoDoc document, string sheetName, DetailQuery detail, Seq<DetailConfigOp> ops, Op op) =>
        from _requested in guard(!ops.IsEmpty, op.InvalidInput()).ToFin()
        from receipt in MutateDetails(document: document, sheetName: sheetName, detail: detail, op: op, mutate: (_, view) =>
            from targets in ops.TraverseM(o => o.Apply(document: document, view: view, op: op)).As()
            from _committed in targets.Somes().Fold(default(DetailCommit), static (acc, target) => acc | DetailCommit.Of(target: target)).Apply(detail: view, op: op)
            from name in ops.Fold(Option<DetailConfigOp.Rename>.None, static (found, o) => found.IsSome ? found : o is DetailConfigOp.Rename r ? Some(r) : found).Case switch {
                DetailConfigOp.Rename rename => FileEndpoint.NonBlank(value: rename.Name, op: op).Map(Some),
                _ => Fin.Succ(value: Option<string>.None),
            }
            select (view.Id, Resources: name.Map(value => Seq(DocumentResourceKind.Layout.Change(name: value))).IfNone(Seq<DocumentResourceChange>())))
        select receipt;

    private static Fin<DocumentReceipt> Configure(RhinoDoc document, SheetQuery query, FileSheetConfig config, Op op) =>
        from pages in query.Resolve(document: document, op: op)
        from _valid in guard(!pages.IsEmpty, op.InvalidInput())
        from _groupMeta in guard(config.Group.IsSome || (config.GroupDescription.IsNone && config.UserStrings.IsEmpty), op.InvalidInput())
        from groupSlot in config.Group.Case switch {
            string groupName => ResolveOrCreateGroup(document: document, groupName: groupName, pages: pages, op: op).Map(Some),
            _ => Fin.Succ(value: Option<PageViewGroup>.None),
        }
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
        let grouped = ordered.Map((page, index) => (
            Page: page,
            Sheet: numbering.Start + index,
            Group: toSeq(page.GetPageViewGroupList()).Find(static _ => true).Map(g =>
                numbering.Start + ordered.Take(index + 1).Filter(p => toSeq(p.GetPageViewGroupList()).Exists(x => x == g)).Count - 1)))
        from numbered in grouped.TraverseM(item =>
            from label in FileEndpoint.NonBlank(value: Substitute(pattern: pattern, sheet: item.Sheet, groupOrdinal: item.Group), op: op)
            from _ in op.Catch(() => {
                item.Page.PageName = label;
                return Fin.Succ(value: unit);
            })
            from details in numbering.DetailPattern.Map(dp => NumberDetails(document: document, page: item.Page, pattern: dp, op: op)).IfNone(Fin.Succ(value: Seq<Guid>()))
            select (Page: item.Page.MainViewport.Id, Label: label, Details: details)).As()
        select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Attributes, ids: numbered.Map(static n => n.Page) + numbered.Bind(static n => n.Details), resources: numbered.Map(static n => DocumentResourceKind.Layout.Change(name: n.Label)));

    private static Fin<Seq<Guid>> NumberDetails(RhinoDoc document, RhinoPageView page, string pattern, Op op) {
        Seq<DetailViewObject> ordered = OrderedDetails(details: toSeq(page.GetDetailViews()), op: op);
        return ordered.Map((detail, index) => (Detail: detail, Value: index + 1)).TraverseM(item =>
            from label in FileEndpoint.NonBlank(value: Substitute(pattern: pattern, sheet: item.Value, groupOrdinal: Option<int>.None), op: op)
            from attributes in Optional(item.Detail.Attributes?.Duplicate()).ToFin(Fail: op.InvalidResult())
            from _ in op.Catch(() => {
                attributes.Name = label;
                return op.Confirm(success: document.Objects.ModifyAttributes(objectId: item.Detail.Id, newAttributes: attributes, quiet: true));
            })
            select item.Detail.Id).As();
    }

    private static string Substitute(string pattern, int sheet, Option<int> groupOrdinal) =>
        TokenPattern.Replace(input: pattern, evaluator: match => {
            int value = match.Groups["tok"].Value switch { "g" => groupOrdinal.IfNone(noneValue: sheet), _ => sheet };
            return match.Groups["fmt"].Success
                ? value.ToString(format: match.Groups["fmt"].Value, provider: CultureInfo.InvariantCulture)
                : value.ToString(provider: CultureInfo.InvariantCulture);
        });

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct DetailMove(Guid SourceId, Guid TargetId, bool Duplicate);

    private static Fin<DocumentReceipt> ArrangeDetails(RhinoDoc document, SheetQuery query, FileDetailLayout layout, Op op) =>
        from _cols in guard(layout.Columns > 0 && layout.SpacingX >= 0.0 && layout.SpacingY >= 0.0 && (layout.Margins.IsSome || layout.Margin >= 0.0), op.InvalidInput())
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
        from content in ContentFrame(document: document, page: page, layout: layout, op: op)
        let ordered = OrderedDetails(details: matched, op: op)
        from frames in ordered.Map((detail, index) => (Detail: detail, Index: index)).TraverseM(item =>
            from current in FrameOf(detail: item.Detail, op: op)
            from target in TargetFrame(content: content, current: current, layout: layout, index: item.Index, count: ordered.Count, op: op)
            select (item.Detail, current, target)).As()
        from moved in frames.TraverseM(item => MoveDetailFrame(document: document, detail: item.Detail, xform: FrameTransform(current: item.current, target: item.target), duplicate: layout.Duplicate, op: op)).As()
        select moved;

    private static Seq<DetailViewObject> OrderedDetails(Seq<DetailViewObject> details, Op op, Option<Func<DetailViewObject, bool>> filter = default) =>
        toSeq(
            details
                .Filter(d => filter.Map(p => p(arg: d)).IfNone(noneValue: true))
                .Select(d => (Detail: d, Frame: FrameOf(detail: d, op: op)))
                .OrderByDescending(static pair => pair.Frame.Map(static f => f.Y).IfFail(0.0))
                .ThenBy(static pair => pair.Frame.Map(static f => f.X).IfFail(0.0))
                .Select(static pair => pair.Detail));

    private static Fin<FileDetailFrame> FrameOf(DetailViewObject detail, Op op) =>
        FileDetailFrame.Of(detail: detail, op: op);

    private static Fin<FileDetailFrame> ContentFrame(RhinoDoc document, RhinoPageView page, FileDetailLayout layout, Op op) =>
        layout.Margins.Case switch {
            CaptureMargins margins =>
                from _valid in guard(
                    margins.Units is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits
                    && document.PageUnitSystem is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits
                    && RhinoMath.IsValidDouble(x: margins.Left) && margins.Left >= 0.0
                    && RhinoMath.IsValidDouble(x: margins.Top) && margins.Top >= 0.0
                    && RhinoMath.IsValidDouble(x: margins.Right) && margins.Right >= 0.0
                    && RhinoMath.IsValidDouble(x: margins.Bottom) && margins.Bottom >= 0.0,
                    op.InvalidInput()).ToFin()
                let scale = RhinoMath.UnitScale(margins.Units, document.PageUnitSystem)
                let frame = new FileDetailFrame(
                    X: margins.Left * scale,
                    Y: margins.Bottom * scale,
                    Width: page.PageWidth - ((margins.Left + margins.Right) * scale),
                    Height: page.PageHeight - ((margins.Top + margins.Bottom) * scale))
                from _frame in guard(RhinoMath.IsValidDouble(x: scale) && scale > 0.0 && frame.IsValid, op.InvalidInput()).ToFin()
                select frame,
            _ => new FileDetailFrame(
                X: layout.Margin,
                Y: layout.Margin,
                Width: page.PageWidth - (2.0 * layout.Margin),
                Height: page.PageHeight - (2.0 * layout.Margin)) switch {
                    FileDetailFrame frame when frame.IsValid => Fin.Succ(value: frame),
                    _ => Fin.Fail<FileDetailFrame>(error: op.InvalidInput()),
                },
        };

    private static Fin<FileDetailFrame> TargetFrame(FileDetailFrame content, FileDetailFrame current, FileDetailLayout layout, int index, int count, Op op) {
        FileDetailLayoutMode mode = layout.Mode ?? FileDetailLayoutMode.Grid;
        return from fitGrid in mode.NeedsFitGrid ? ComputeFitGrid(content: content, layout: layout, count: count, op: op) : Fin.Succ(value: default(FitGrid))
               from size in TargetSize(mode: mode, layout: layout, current: current, fitGrid: fitGrid, op: op)
               from target in mode.Frame(c: new LayoutContext(Content: content, Current: current, Layout: layout, FitGrid: fitGrid, Size: size, Index: index, Count: count, Op: op))
               from valid in target.IsValid ? Fin.Succ(value: target) : Fin.Fail<FileDetailFrame>(error: op.InvalidResult())
               select valid;
    }

    private static Fin<(double Width, double Height)> TargetSize(FileDetailLayoutMode mode, FileDetailLayout layout, FileDetailFrame current, FitGrid fitGrid, Op op) =>
        mode.NeedsFitGrid
            ? Fin.Succ(value: (fitGrid.Width, fitGrid.Height))
            : layout.Size.Case switch {
                Point2d size when size.X > RhinoMath.ZeroTolerance && size.Y > RhinoMath.ZeroTolerance => Fin.Succ(value: (Width: size.X, Height: size.Y)),
                Point2d => Fin.Fail<(double Width, double Height)>(error: op.InvalidInput()),
                _ => Fin.Succ(value: (current.Width, current.Height)),
            };

    internal static FileDetailFrame GridCell(FileDetailFrame content, FileDetailLayout layout, (double Width, double Height) size, int index, int columns, double spacingX, double spacingY, bool fromOrigin) {
        int column = index % columns;
        int row = index / columns;
        return fromOrigin
            ? new FileDetailFrame(X: layout.Origin.X + (column * spacingX), Y: layout.Origin.Y + (row * spacingY), Width: size.Width, Height: size.Height)
            : new FileDetailFrame(X: content.X + (column * (size.Width + spacingX)), Y: content.Y + (row * (size.Height + spacingY)), Width: size.Width, Height: size.Height);
    }

    private static Fin<FitGrid> ComputeFitGrid(FileDetailFrame content, FileDetailLayout layout, int count, Op op) {
        int cols = Math.Min(layout.Columns, Math.Max(count, 1));
        int rows = (int)Math.Ceiling(count / (double)cols);
        double w = (content.Width - ((cols - 1) * layout.SpacingX)) / cols;
        double h = (content.Height - ((rows - 1) * layout.SpacingY)) / rows;
        return guard(w > RhinoMath.ZeroTolerance && h > RhinoMath.ZeroTolerance, op.InvalidInput())
            .ToFin().Map(_ => new FitGrid(Columns: cols, Rows: rows, Width: w, Height: h));
    }

    internal static FileDetailFrame AnchorFrame(FileDetailFrame content, FileDetailLayout layout, (double Width, double Height) size) {
        FileDetailAnchor anchor = layout.Anchor ?? FileDetailAnchor.TopLeft;
        Point2d origin = content.AnchorPoint(anchor: anchor, offset: layout.Origin);
        return new FileDetailFrame(
            X: origin.X - (anchor.XFactor * size.Width),
            Y: origin.Y - (anchor.YFactor * size.Height),
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

    private static Transform FrameTransform(FileDetailFrame current, FileDetailFrame target) {
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

    private static FileDetailReport DetailReportOf(RhinoDoc document, DetailViewObject detail, Op op) {
        Guid viewportId = detail.Viewport.Id;
        Option<DetailView> geometry = Optional(detail.DetailGeometry);
        bool parallel = geometry.Map(static g => g.IsParallelProjection).IfNone(noneValue: false);
        bool hasFrustum = detail.Viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far);
        Seq<string> overridden = toSeq(document.Layers)
            .Filter(layer => !layer.IsDeleted && layer.HasPerViewportSettings(viewportId: viewportId))
            .Map(static layer => layer.FullPath);
        return new FileDetailReport(
            Name: DetailQuery.NameOf(detail: detail).IfNone(string.Empty),
            DescriptiveTitle: detail.DescriptiveTitle,
            Scale: FileScale.Format(detail: detail),
            Projection: ProjectionOf(detail: detail),
            ViewportId: viewportId,
            ParentViewportId: Optional(detail.ParentPageView).Map(static page => page.MainViewport.Id),
            Active: detail.IsActive,
            ProjectionLocked: geometry.Map(static g => g.IsProjectionLocked).IfNone(noneValue: false),
            Frame: FrameOf(detail: detail, op: op).IfFail(default(FileDetailFrame)),
            PageToModelRatio: parallel ? geometry.Map(static g => g.PageToModelRatio).Filter(RhinoMath.IsValidDouble) : Option<double>.None,
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
            Frustum: hasFrustum && RhinoMath.IsValidDouble(x: left) && RhinoMath.IsValidDouble(x: right) && RhinoMath.IsValidDouble(x: bottom) && RhinoMath.IsValidDouble(x: top)
                ? Some((Left: left, Right: right, Bottom: bottom, Top: top))
                : Option<(double, double, double, double)>.None,
            ClipRange: hasFrustum && RhinoMath.IsValidDouble(x: near) && RhinoMath.IsValidDouble(x: far)
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
                Details: detailViews.Map(detail => DetailReportOf(document: document, detail: detail, op: op)),
                GroupMetadata: groups.Map(static item => (Group: item.Name, Strings: UserStringsOf(pageGroup: item)))));
        });

}

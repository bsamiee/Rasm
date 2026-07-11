# [RASM_RHINO_SHEETS]

The sheet transaction family (`Rasm.Rhino.Exchange`). ONE request union carries page selection, detail selection, desired state, mutation policy, conflict evidence, and receipts: pages resolve through one `SheetSelect`, details through one `DetailSelect`, and every per-detail mutation travels as ONE declarative `DetailState` whose field rows carry their own host writer and commit consequence — the geometry-versus-viewport commit is DERIVED from which fields are present and applied at most once each, so the census's twenty-one `FileSheetEdit` verb cases and their private mutator family collapse into a state correspondence. Page-view groups, per-viewport layer veils, clipping-plane participation, named detail views, deterministic detail arrangement, pattern numbering, and the scale audit all ride the same rail; camera framing composes the viewport camera rail against a detail-addressed target before a sheet commit, and page-unit regime changes are the Document session's regime surface — neither owns a seat here. Frame, grid, anchor, and numbering arithmetic composes the kernel numeric owners: anchors are `UnitInterval` factor pairs, counts and ordinals are `Dimension` values, and unit conversion is the host's own `RhinoMath.UnitScale` read at the boundary.

## [01]-[INDEX]

- [02]-[SELECTORS]: `SheetSelect` and `DetailSelect` — page and detail resolution as data.
- [03]-[SCALE_AND_VEILS]: `SheetScale` the format/parse/apply scale owner, `FieldOverride<T>`/`VeilField`/`LayerVeil` the per-viewport layer overrides, `ClipRule` the clipping participation family.
- [04]-[DETAIL_STATE]: `DetailSpec` creation, `DetailAnchor`/`DetailArrangement` the layout algebra, and `DetailState` — the desired-state record with the derived commit correspondence.
- [05]-[TRANSACTION_RAIL]: `SheetSize`/`SheetSpec`, `NumberRule`, `ScaleConflict`, `SheetOp`/`SheetFact`/`SheetReceipt`, and `Sheets.Commit`.

## [02]-[SELECTORS]

- Owner: `SheetSelect` — page addressing as one value: id, name, group membership, and an open predicate compose conjunctively, and the empty selector is the whole page roster in `PageNumber` order. `DetailSelect` — detail addressing with the same grammar plus the projection presets (`Parallel`, `Perspective`); `Single` proves exactly one match for operations whose host member admits one detail.
- Law: selection is read-only — a selector never activates, mutates, or redraws; it resolves live host objects inside the demand window that consumes them and hands them onward within that window.
- Law: name matching is ordinal-case-insensitive to match the host's page-name semantics; the predicate slot is the open escape for structural conditions and never a mutation channel.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct SheetSelect(
    Option<Guid> Id = default,
    Option<string> Name = default,
    Option<string> Group = default,
    Option<Func<RhinoPageView, bool>> Where = default) {
    public static SheetSelect All => default;
    public static SheetSelect Named(string name) => new(Name: Some(name));

    internal Fin<Seq<RhinoPageView>> Resolve(RhinoDoc document, Op op) {
        SheetSelect self = this;
        return op.Catch(() => {
            Option<int> group = self.Group.Bind(name =>
                Optional(document.PageViewGroups.FindName(name: name)).Map(static found => found.Index));
            Seq<RhinoPageView> pages = toSeq(document.Views.GetPageViews())
                .Filter(page =>
                    self.Id.Map(id => page.MainViewport.Id == id).IfNone(noneValue: true)
                    && self.Name.Map(name => string.Equals(a: page.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(noneValue: true)
                    && group.Map(index => page.IsInPageViewGroup(pageViewGroupIndex: index)).IfNone(noneValue: true)
                    && self.Where.Map(where => where(arg: page)).IfNone(noneValue: true))
                .OrderBy(static page => page.PageNumber)
                .AsIterable()
                .ToSeq();
            return Fin.Succ(value: pages);
        });
    }

    internal Fin<RhinoPageView> Single(RhinoDoc document, Op op) =>
        Resolve(document: document, op: op).Bind(pages => pages switch {
            [var only] => Fin.Succ(value: only),
            _ => Fin.Fail<RhinoPageView>(error: op.InvalidInput()),
        });
}

public readonly record struct DetailSelect(
    Option<Guid> Id = default,
    Option<string> Name = default,
    Option<Func<DetailViewObject, bool>> Where = default) {
    public static DetailSelect All => default;
    public static DetailSelect Named(string name) => new(Name: Some(name));
    public static DetailSelect Parallel => new(Where: Some<Func<DetailViewObject, bool>>(value: static detail => detail.DetailGeometry is { IsParallelProjection: true }));
    public static DetailSelect Perspective => new(Where: Some<Func<DetailViewObject, bool>>(value: static detail => detail.DetailGeometry is not { IsParallelProjection: true }));

    internal static Option<string> NameOf(DetailViewObject detail) =>
        Optional(detail.Attributes.Name).Filter(static text => !string.IsNullOrWhiteSpace(value: text))
        | Optional(detail.Viewport.Name).Filter(static text => !string.IsNullOrWhiteSpace(value: text));

    internal Fin<Seq<DetailViewObject>> Resolve(RhinoPageView page, Op op) {
        DetailSelect self = this;
        return op.Catch(() => Fin.Succ(value: toSeq(page.GetDetailViews())
            .Filter(detail =>
                self.Id.Map(id => detail.Id == id || detail.Viewport.Id == id).IfNone(noneValue: true)
                && self.Name.Map(name => NameOf(detail: detail).Map(found =>
                    string.Equals(a: found, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(noneValue: false)).IfNone(noneValue: true)
                && self.Where.Map(where => where(arg: detail)).IfNone(noneValue: true))));
    }

    internal Fin<DetailViewObject> Single(RhinoPageView page, Op op) =>
        Resolve(page: page, op: op).Bind(details => details switch {
            [var only] => Fin.Succ(value: only),
            _ => Fin.Fail<DetailViewObject>(error: op.InvalidInput()),
        });
}
```

## [03]-[SCALE_AND_VEILS]

- Owner: `SheetScale` `[Union]` — the detail scale on ONE owner carrying both directions: `RatioCase(model, page)` and `LengthsCase(modelLength, modelUnit, pageLength, pageUnit)` construct, `NamedCase(string)` parses the human spelling, `Format` renders the live ratio back to that spelling — parse and format are inverse operations of one surface, and `Apply` resolves any case to the host `SetScale` write over a parallel-projection detail. `FieldOverride<T>` — the keep/set/clear tri-state whose `|` merge is right-biased; `VeilField` `[Union]` — the per-viewport layer override rows (color, visibility, persistent visibility, plot color, plot weight), each dispatching to its host setter/deleter pair; `LayerVeil` — one layer path plus its field set with `ResetAll` as the whole-slate clear. `ClipRule` `[Union]` — clipping participation: `AddCase` mints a plane clipping the detail, `ToggleCase` attaches or detaches an existing plane idempotently, `ScopeCase` sets object/layer participation through `SetClipParticipation`, and `PruneCase` detaches or deletes planes bound to a retiring detail — a plane solely bound to that detail deletes, a shared plane only detaches.
- Law: a scale applies only to a parallel projection — the perspective refusal is typed and precedes the host write, and the same predicate feeds the audit's `PerspectiveScale` conflict row.
- Law: `NamedCase` parsing is unit-aware — a bare `1:100` reads both sides in the document's page and model units, an annotated `1cm:1m` carries its own; the format leg emits the canonical `model:page` spelling so round-tripping a formatted scale reconstructs the ratio exactly.
- Law: veil merging is per-field — two veils on one layer path merge field-wise before any host write, so the last writer wins per field, never per layer.
- Law: `SheetScale` also carries the paper↔model length correspondence as two operations of the one scale owner over the host's `TryGetPaperLength`/`TryGetModelLength` pair — the same owner answers both directions, and a false host return is a typed refusal, never a zero length.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
public readonly record struct FieldOverride<T>(Option<T> Value = default, bool Inherit = false) {
    public static FieldOverride<T> Keep() => default;
    public static FieldOverride<T> Set(T value) => new(Value: Some(value));
    public static FieldOverride<T> Clear() => new(Inherit: true);

    internal bool IsActive => Value.IsSome || Inherit;

    public static FieldOverride<T> operator |(FieldOverride<T> left, FieldOverride<T> right) =>
        right.IsActive ? right : left;

    internal Unit Apply(Action<T> set, Action inherit) {
        _ = Value.Iter(value => set(obj: value));
        return Op.SideWhen(Inherit && Value.IsNone, () => inherit());
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VeilField {
    private VeilField() { }
    public sealed record ColorCase(FieldOverride<System.Drawing.Color> Write) : VeilField;
    public sealed record VisibleCase(FieldOverride<bool> Write) : VeilField;
    public sealed record PersistentVisibleCase(FieldOverride<bool> Write) : VeilField;
    public sealed record PlotColorCase(FieldOverride<System.Drawing.Color> Write) : VeilField;
    public sealed record PlotWeightCase(FieldOverride<double> Write) : VeilField;

    internal bool IsActive => Switch(
        colorCase: static field => field.Write.IsActive,
        visibleCase: static field => field.Write.IsActive,
        persistentVisibleCase: static field => field.Write.IsActive,
        plotColorCase: static field => field.Write.IsActive,
        plotWeightCase: static field => field.Write.IsActive);

    internal Unit Apply(Layer layer, Guid viewport) => Switch(
        state: (Layer: layer, Viewport: viewport),
        colorCase: static (ctx, field) => field.Write.Apply(
            set: value => ctx.Layer.SetPerViewportColor(viewportId: ctx.Viewport, color: value),
            inherit: () => ctx.Layer.DeletePerViewportColor(viewportId: ctx.Viewport)),
        visibleCase: static (ctx, field) => field.Write.Apply(
            set: value => ctx.Layer.SetPerViewportVisible(viewportId: ctx.Viewport, visible: value),
            inherit: () => ctx.Layer.DeletePerViewportVisible(viewportId: ctx.Viewport)),
        persistentVisibleCase: static (ctx, field) => field.Write.Apply(
            set: value => ctx.Layer.SetPerViewportPersistentVisibility(viewportId: ctx.Viewport, persistentVisibility: value),
            inherit: () => ctx.Layer.UnsetPerViewportPersistentVisibility(viewportId: ctx.Viewport)),
        plotColorCase: static (ctx, field) => field.Write.Apply(
            set: value => ctx.Layer.SetPerViewportPlotColor(viewportId: ctx.Viewport, color: value),
            inherit: () => ctx.Layer.DeletePerViewportPlotColor(viewportId: ctx.Viewport)),
        plotWeightCase: static (ctx, field) => field.Write.Apply(
            set: value => ctx.Layer.SetPerViewportPlotWeight(viewportId: ctx.Viewport, plotWeight: value),
            inherit: () => ctx.Layer.DeletePerViewportPlotWeight(viewportId: ctx.Viewport)));
}

public readonly record struct LayerVeil(string LayerPath, Seq<VeilField> Fields, bool ResetAll) {
    public static LayerVeil Reset(string path) => new(LayerPath: path, Fields: Seq<VeilField>(), ResetAll: true);
    public static LayerVeil Of(string path, params ReadOnlySpan<VeilField> fields) =>
        new(LayerPath: path, Fields: toSeq(fields.ToArray()), ResetAll: false);

    internal bool Applies => ResetAll || Fields.Exists(static field => field.IsActive);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClipRule {
    private ClipRule() { }
    public sealed record AddCase(Plane Plane, double U, double V) : ClipRule;
    public sealed record ToggleCase(Guid PlaneId, bool Active) : ClipRule;
    public sealed record ScopeCase(Guid PlaneId, Seq<Guid> ObjectIds, Seq<int> LayerIndices, bool Exclusive) : ClipRule;
    public sealed record PruneCase : ClipRule;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SheetScale {
    private SheetScale() { }
    public sealed record RatioCase(double Model, double Page) : SheetScale;
    public sealed record LengthsCase(double ModelLength, UnitSystem ModelUnit, double PageLength, UnitSystem PageUnit) : SheetScale;
    public sealed record NamedCase(string Spelling) : SheetScale;

    public static Fin<SheetScale> Ratio(double model, double page, Op? key = null) {
        Op op = key.OrDefault();
        return from _model in op.Positive(value: model)
               from _page in op.Positive(value: page)
               select (SheetScale)new RatioCase(Model: model, Page: page);
    }

    internal static Option<string> Format(DetailViewObject detail) =>
        detail.DetailGeometry is { IsParallelProjection: true } geometry && geometry.PageToModelRatio > 0.0
            ? Some(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"1:{1.0 / geometry.PageToModelRatio:0.######}"))
            : Option<string>.None;

    internal Fin<(double ModelLength, UnitSystem ModelUnit, double PageLength, UnitSystem PageUnit)> Resolve(RhinoDoc document, Op op) => Switch(
        state: (Document: document, Op: op),
        ratioCase: static (ctx, scale) =>
            Fin.Succ(value: (scale.Model, ctx.Document.ModelUnitSystem, scale.Page, ctx.Document.PageUnitSystem)),
        lengthsCase: static (ctx, scale) =>
            from _model in ctx.Op.Positive(value: scale.ModelLength)
            from _page in ctx.Op.Positive(value: scale.PageLength)
            select (scale.ModelLength, scale.ModelUnit, scale.PageLength, scale.PageUnit),
        namedCase: static (ctx, scale) => Parse(spelling: scale.Spelling, document: ctx.Document, op: ctx.Op));

    internal Fin<Unit> Apply(DetailViewObject detail, RhinoDoc document, Op op) =>
        from _parallel in guard(detail.DetailGeometry is { IsParallelProjection: true }, op.InvalidInput()).ToFin()
        from resolved in Resolve(document: document, op: op)
        from _scaled in op.Confirm(success:
            detail.DetailGeometry.SetScale(
                modelLength: resolved.ModelLength, modelUnits: LengthUnit.FromKnownUnitSystem(knownUnitSystem: resolved.ModelUnit),
                pageLength: resolved.PageLength, pageUnits: LengthUnit.FromKnownUnitSystem(knownUnitSystem: resolved.PageUnit)))
        select unit;

    private static Fin<(double, UnitSystem, double, UnitSystem)> Parse(string spelling, RhinoDoc document, Op op) =>
        from text in op.AcceptText(value: spelling)
        from split in text.Split(':') is [var left, var right]
            ? Fin.Succ(value: (Left: left.Trim(), Right: right.Trim()))
            : Fin.Fail<(string Left, string Right)>(error: op.InvalidInput())
        from model in ParseSide(text: split.Left, fallback: document.ModelUnitSystem, op: op)
        from page in ParseSide(text: split.Right, fallback: document.PageUnitSystem, op: op)
        select (model.Value, model.Unit, page.Value, page.Unit);

    public static Fin<double> PaperLength(DetailViewObject detail, double modelLength, Op? key = null) {
        Op op = key.OrDefault();
        return from _length in op.Positive(value: modelLength)
               from paper in op.Catch(() => detail.TryGetPaperLength(modelLength, out double paperLength)
                   ? Fin.Succ(value: paperLength)
                   : Fin.Fail<double>(error: op.InvalidResult()))
               select paper;
    }

    public static Fin<double> ModelLength(DetailViewObject detail, double paperLength, Op? key = null) {
        Op op = key.OrDefault();
        return from _length in op.Positive(value: paperLength)
               from model in op.Catch(() => detail.TryGetModelLength(paperLength, out double modelLength)
                   ? Fin.Succ(value: modelLength)
                   : Fin.Fail<double>(error: op.InvalidResult()))
               select model;
    }

    private static Fin<(double Value, UnitSystem Unit)> ParseSide(string text, UnitSystem fallback, Op op) {
        (string Suffix, UnitSystem Unit)[] suffixes = [("mm", UnitSystem.Millimeters), ("cm", UnitSystem.Centimeters), ("m", UnitSystem.Meters), ("in", UnitSystem.Inches), ("ft", UnitSystem.Feet)];
        (string Number, UnitSystem Unit) parsed = toSeq(suffixes)
            .Find(row => text.EndsWith(row.Suffix, StringComparison.OrdinalIgnoreCase))
            .Map(row => (text[..^row.Suffix.Length].Trim(), row.Unit))
            .IfNone(() => (text, fallback));
        return double.TryParse(s: parsed.Number, provider: System.Globalization.CultureInfo.InvariantCulture, result: out double value) && value > 0.0
            ? Fin.Succ(value: (value, parsed.Unit))
            : Fin.Fail<(double, UnitSystem)>(error: op.InvalidInput());
    }
}
```

## [04]-[DETAIL_STATE]

- Owner: `DetailSpec` — detail creation: name, page-space corners, initial projection, optional display mode, optional scale, projection lock; realized through the host's active-view bracket because `AddDetailView` requires its page active, with the prior active view restored on every exit. `DetailAnchor` `[SmartEnum<int>]` — the nine placement anchors as `UnitInterval` factor pairs. `DetailArrangement` `[SmartEnum<int>]` — the deterministic layout rows (`Grid`, `FitPage`, `AlignAnchor`, `DistributeHorizontal`, `DistributeVertical`), each carrying its frame delegate over one `LayoutContext`; arrangement is a pure frame computation folded over the matched details, then one transform per detail moves it on the page. `DetailState` — the desired-state record: every field is an `Option` whose presence means "make it so", and the CORRESPONDENCE table is the one primary map from field to host writer and commit consequence — `Apply` folds present rows, unions their `DetailCommit` flags, and commits geometry then viewport at most once each. Activation orders last because `SetActiveDetail` reads committed state.
- Law: the correspondence is the only mutation path — a new detail property is one row (presence probe, writer, commit flag) and every consumer inherits it through the same fold; a sibling mutator method is the deleted census form.
- Law: layout arithmetic is kernel-composed — anchor factors are `UnitInterval` values, grid columns derive from `Dimension` counts via ceiling division, and page-space frames stay `double` page units validated finite and positive before any transform mints.
- Boundary: camera pose inside a detail is the viewport camera rail addressed at `ViewportTarget.DetailCase`; `DetailState` owns scale, locks, naming, display mode, veils, and clips — the split keeps one camera algebra in the package.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DetailAnchor {
    public static readonly DetailAnchor TopLeft = new(key: 0, x: UnitInterval.Create(value: 0.0), y: UnitInterval.Create(value: 1.0));
    public static readonly DetailAnchor TopCenter = new(key: 1, x: UnitInterval.Create(value: 0.5), y: UnitInterval.Create(value: 1.0));
    public static readonly DetailAnchor TopRight = new(key: 2, x: UnitInterval.Create(value: 1.0), y: UnitInterval.Create(value: 1.0));
    public static readonly DetailAnchor MiddleLeft = new(key: 3, x: UnitInterval.Create(value: 0.0), y: UnitInterval.Create(value: 0.5));
    public static readonly DetailAnchor Center = new(key: 4, x: UnitInterval.Create(value: 0.5), y: UnitInterval.Create(value: 0.5));
    public static readonly DetailAnchor MiddleRight = new(key: 5, x: UnitInterval.Create(value: 1.0), y: UnitInterval.Create(value: 0.5));
    public static readonly DetailAnchor BottomLeft = new(key: 6, x: UnitInterval.Create(value: 0.0), y: UnitInterval.Create(value: 0.0));
    public static readonly DetailAnchor BottomCenter = new(key: 7, x: UnitInterval.Create(value: 0.5), y: UnitInterval.Create(value: 0.0));
    public static readonly DetailAnchor BottomRight = new(key: 8, x: UnitInterval.Create(value: 1.0), y: UnitInterval.Create(value: 0.0));

    public UnitInterval X { get; }
    public UnitInterval Y { get; }
}

public readonly record struct DetailFrame(double X, double Y, double Width, double Height) {
    internal bool IsValid =>
        double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Width) && double.IsFinite(Height) && Width > 0.0 && Height > 0.0;

    internal Point2d Anchored(DetailAnchor anchor, Point2d offset) =>
        new(x: X + (Width * (double)anchor.X) + offset.X, y: Y + (Height * (double)anchor.Y) + offset.Y);
}

internal readonly record struct LayoutContext(
    DetailFrame Current, (double Width, double Height) Page,
    DetailAnchor Anchor, Point2d Offset, double Gutter, Dimension Columns, int Index, int Count, Op Key);

[SmartEnum<int>]
public sealed partial class DetailArrangement {
    public static readonly DetailArrangement Grid = new(key: 0, frame: static ctx => {
        int columns = ctx.Columns.Value;
        int rows = (ctx.Count + columns - 1) / columns;
        double cellWidth = (ctx.Page.Width - (ctx.Gutter * (columns + 1))) / columns;
        double cellHeight = (ctx.Page.Height - (ctx.Gutter * (rows + 1))) / rows;
        int column = ctx.Index % columns;
        int row = ctx.Index / columns;
        DetailFrame cell = new(
            X: ctx.Gutter + (column * (cellWidth + ctx.Gutter)),
            Y: ctx.Page.Height - ctx.Gutter - ((row + 1) * cellHeight) - (row * ctx.Gutter),
            Width: cellWidth, Height: cellHeight);
        return cell.IsValid ? Fin.Succ(value: cell) : Fin.Fail<DetailFrame>(error: ctx.Key.InvalidResult());
    });
    public static readonly DetailArrangement FitPage = new(key: 1, frame: static ctx => {
        DetailFrame fit = new(X: ctx.Gutter, Y: ctx.Gutter, Width: ctx.Page.Width - (2 * ctx.Gutter), Height: ctx.Page.Height - (2 * ctx.Gutter));
        return fit.IsValid ? Fin.Succ(value: fit) : Fin.Fail<DetailFrame>(error: ctx.Key.InvalidResult());
    });
    public static readonly DetailArrangement AlignAnchor = new(key: 2, frame: static ctx => {
        Point2d seat = new DetailFrame(X: 0.0, Y: 0.0, Width: ctx.Page.Width, Height: ctx.Page.Height).Anchored(anchor: ctx.Anchor, offset: ctx.Offset);
        DetailFrame aligned = ctx.Current with { X = seat.X - (ctx.Current.Width * (double)ctx.Anchor.X), Y = seat.Y - (ctx.Current.Height * (double)ctx.Anchor.Y) };
        return aligned.IsValid ? Fin.Succ(value: aligned) : Fin.Fail<DetailFrame>(error: ctx.Key.InvalidResult());
    });
    public static readonly DetailArrangement DistributeHorizontal = new(key: 3, frame: static ctx => {
        double step = (ctx.Page.Width - (2 * ctx.Gutter)) / ctx.Count;
        DetailFrame spaced = ctx.Current with { X = ctx.Gutter + (ctx.Index * step) + ((step - ctx.Current.Width) / 2.0) };
        return spaced.IsValid ? Fin.Succ(value: spaced) : Fin.Fail<DetailFrame>(error: ctx.Key.InvalidResult());
    });
    public static readonly DetailArrangement DistributeVertical = new(key: 4, frame: static ctx => {
        double step = (ctx.Page.Height - (2 * ctx.Gutter)) / ctx.Count;
        DetailFrame spaced = ctx.Current with { Y = ctx.Gutter + (ctx.Index * step) + ((step - ctx.Current.Height) / 2.0) };
        return spaced.IsValid ? Fin.Succ(value: spaced) : Fin.Fail<DetailFrame>(error: ctx.Key.InvalidResult());
    });

    [UseDelegateFromConstructor]
    internal partial Fin<DetailFrame> Frame(LayoutContext context);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record DetailSpec(
    string Name,
    Point2d Corner,
    Point2d Opposite,
    Rhino.Display.DefinedViewportProjection Projection,
    Option<Guid> DisplayMode,
    Option<SheetScale> Scale,
    bool ProjectionLocked);

[SmartEnum<int>]
public sealed partial class NamedDetailMode {
    public static readonly NamedDetailMode Save = new(key: 0, apply: static (document, detail, name, op) =>
        op.Confirm(success: document.NamedViews.Add(name: name, viewportId: detail.Viewport.Id) >= 0));
    public static readonly NamedDetailMode Restore = new(key: 1, apply: static (document, detail, name, op) =>
        document.NamedViews.FindByName(name) is var index && index >= 0
            ? op.Confirm(success: document.NamedViews.RestoreWithAspectRatio(index: index, viewport: detail.Viewport))
            : Fin.Fail<Unit>(error: op.InvalidInput()));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Apply(RhinoDoc document, DetailViewObject detail, string name, Op key);
}

public readonly record struct DetailCommit(bool Geometry, bool Viewport) {
    public static DetailCommit Neither => default;
    public static DetailCommit OfGeometry => new(Geometry: true, Viewport: false);
    public static DetailCommit OfViewport => new(Geometry: false, Viewport: true);

    public static DetailCommit operator |(DetailCommit left, DetailCommit right) =>
        new(Geometry: left.Geometry || right.Geometry, Viewport: left.Viewport || right.Viewport);

    internal Fin<Unit> Apply(DetailViewObject detail, Op op) =>
        from _geometry in Geometry ? op.Confirm(success: detail.CommitChanges()) : Fin.Succ(value: unit)
        from _viewport in Viewport ? op.Confirm(success: detail.CommitViewportChanges()) : Fin.Succ(value: unit)
        select unit;
}

public sealed record DetailState(
    Option<string> Name = default,
    Option<bool> LockProjection = default,
    Option<bool> LockCamera = default,
    Option<Guid> DisplayMode = default,
    Option<Rhino.Display.DefinedViewportProjection> Projection = default,
    Option<SheetScale> Scale = default,
    Option<DetailFrame> Frame = default,
    Option<(string Name, NamedDetailMode Mode)> NamedView = default,
    Seq<LayerVeil> Veils = default,
    Option<ClipRule> Clip = default,
    Option<bool> Active = default) {
    internal bool Touches =>
        Name.IsSome || LockProjection.IsSome || LockCamera.IsSome || DisplayMode.IsSome || Projection.IsSome
        || Scale.IsSome || Frame.IsSome || NamedView.IsSome || !Veils.IsEmpty || Clip.IsSome || Active.IsSome;

    private static readonly Seq<(Func<DetailState, bool> Present, Func<DetailState, RhinoDoc, RhinoPageView, DetailViewObject, Op, Fin<Unit>> Write, DetailCommit Commit)> Correspondence = Seq<(Func<DetailState, bool>, Func<DetailState, RhinoDoc, RhinoPageView, DetailViewObject, Op, Fin<Unit>>, DetailCommit)>(
        (static state => state.Name.IsSome,
         static (state, document, _, detail, op) => op.Catch(() => {
             ObjectAttributes attributes = detail.Attributes.Duplicate();
             attributes.Name = state.Name.IfNone(noneValue: string.Empty);
             return op.Confirm(success: document.Objects.ModifyAttributes(objectId: detail.Id, newAttributes: attributes, quiet: true));
         }),
         DetailCommit.Neither),
        (static state => state.LockProjection.IsSome,
         static (state, _, _, detail, op) => op.Catch(() => {
             detail.DetailGeometry.IsProjectionLocked = state.LockProjection.IfNone(noneValue: false);
             return Fin.Succ(value: unit);
         }),
         DetailCommit.OfGeometry),
        (static state => state.LockCamera.IsSome,
         static (state, _, _, detail, op) => op.Catch(() => {
             detail.Viewport.LockedProjection = state.LockCamera.IfNone(noneValue: false);
             return Fin.Succ(value: unit);
         }),
         DetailCommit.OfViewport),
        (static state => state.DisplayMode.IsSome,
         static (state, _, _, detail, op) =>
             state.DisplayMode
                 .Bind(static id => Optional(Rhino.Display.DisplayModeDescription.GetDisplayMode(id: id)))
                 .ToFin(Fail: op.InvalidInput())
                 .Bind(mode => op.Catch(() => {
                     detail.Viewport.DisplayMode = mode;
                     return Fin.Succ(value: unit);
                 })),
         DetailCommit.OfViewport),
        (static state => state.Projection.IsSome,
         static (state, _, _, detail, op) => op.Confirm(success: detail.Viewport.SetProjection(
             projection: state.Projection.IfNone(noneValue: Rhino.Display.DefinedViewportProjection.Top),
             viewName: detail.Viewport.Name, updateConstructionPlane: false)),
         DetailCommit.OfViewport),
        (static state => state.Scale.IsSome,
         static (state, document, _, detail, op) =>
             state.Scale.ToFin(Fail: op.InvalidInput()).Bind(scale => scale.Apply(detail: detail, document: document, op: op)),
         DetailCommit.OfGeometry),
        (static state => state.Frame.IsSome,
         static (state, document, _, detail, op) =>
             from frame in state.Frame.ToFin(Fail: op.InvalidInput())
             from _valid in guard(frame.IsValid, op.InvalidInput()).ToFin()
             from current in DetailFrameOf(detail: detail, op: op)
             from _moved in op.Catch(() => {
                 Transform move = Transform.Translation(motion: new Vector3d(x: frame.X - current.X, y: frame.Y - current.Y, z: 0.0));
                 Transform size = Transform.Scale(
                     plane: new Plane(new Point3d(x: frame.X, y: frame.Y, z: 0.0), Vector3d.ZAxis),
                     xScaleFactor: frame.Width / current.Width, yScaleFactor: frame.Height / current.Height, zScaleFactor: 1.0);
                 return op.Confirm(success: document.Objects.Transform(objectId: detail.Id, xform: size * move, deleteOriginal: true) != Guid.Empty);
             })
             select unit,
         DetailCommit.Neither),
        (static state => state.NamedView.IsSome,
         static (state, document, _, detail, op) =>
             state.NamedView.ToFin(Fail: op.InvalidInput()).Bind(named =>
                 op.AcceptText(value: named.Name).Bind(name =>
                     named.Mode.Apply(document: document, detail: detail, name: name, key: op))),
         DetailCommit.Neither),
        (static state => !state.Veils.IsEmpty,
         static (state, document, _, detail, op) =>
             state.Veils
                 .Filter(static veil => veil.Applies)
                 .Fold(Seq<LayerVeil>(), static (merged, veil) =>
                     merged.Exists(row => string.Equals(a: row.LayerPath, b: veil.LayerPath, comparisonType: StringComparison.OrdinalIgnoreCase))
                         ? merged.Map(row => string.Equals(a: row.LayerPath, b: veil.LayerPath, comparisonType: StringComparison.OrdinalIgnoreCase)
                             ? row with { Fields = row.Fields + veil.Fields, ResetAll = row.ResetAll || veil.ResetAll }
                             : row)
                         : merged.Add(veil))
                 .TraverseM(veil =>
                     from path in op.AcceptText(value: veil.LayerPath)
                     from layer in document.Layers.FindByFullPath(layerPath: path, notFoundReturnValue: -1) switch {
                         int index when index >= 0 => Optional(document.Layers[index]).ToFin(Fail: op.InvalidInput()),
                         _ => Fin.Fail<Layer>(error: op.InvalidInput()),
                     }
                     from _reset in veil.ResetAll
                         ? op.Catch(() => {
                             layer.DeletePerViewportSettings(viewportId: detail.Viewport.Id);
                             return Fin.Succ(value: unit);
                         })
                         : Fin.Succ(value: unit)
                     from _fields in op.Catch(() => {
                         _ = veil.Fields.Iter(field => field.Apply(layer: layer, viewport: detail.Viewport.Id));
                         return Fin.Succ(value: unit);
                     })
                     select unit)
                 .As()
                 .Map(static _ => unit),
         DetailCommit.Neither),
        (static state => state.Clip.IsSome,
         static (state, document, _, detail, op) =>
             state.Clip.ToFin(Fail: op.InvalidInput()).Bind(rule => Clips.Apply(rule: rule, document: document, detail: detail, op: op)),
         DetailCommit.Neither));

    internal Fin<DetailCommit> Apply(RhinoDoc document, RhinoPageView page, DetailViewObject detail, Op op) {
        DetailState self = this;
        return Correspondence
            .Filter(row => row.Present(arg: self))
            .TraverseM(row => row.Write(self, document, page, detail, op).Map(_ => row.Commit))
            .As()
            .Map(static commits => commits.Fold(DetailCommit.Neither, static (folded, commit) => folded | commit))
            .Bind(folded => folded.Apply(detail: detail, op: op).Map(_ => folded))
            .Bind(folded => self.Active.Case switch {
                true => op.Confirm(success: page.SetActiveDetail(detailId: detail.Id)).Map(_ => folded),
                false => op.Catch(() => {
                    page.SetPageAsActive();
                    return Fin.Succ(value: folded);
                }),
                _ => Fin.Succ(value: folded),
            });
    }

    internal static Fin<DetailFrame> DetailFrameOf(DetailViewObject detail, Op op) =>
        op.Catch(() => {
            BoundingBox bounds = detail.Geometry.GetBoundingBox(accurate: true);
            DetailFrame frame = new(X: bounds.Min.X, Y: bounds.Min.Y, Width: bounds.Max.X - bounds.Min.X, Height: bounds.Max.Y - bounds.Min.Y);
            return frame.IsValid ? Fin.Succ(value: frame) : Fin.Fail<DetailFrame>(error: op.InvalidResult());
        });
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class Clips {
    internal static Fin<Unit> Apply(ClipRule rule, RhinoDoc document, DetailViewObject detail, Op op) => rule.Switch(
        state: (Document: document, Detail: detail, Op: op),
        addCase: static (ctx, clip) =>
            from id in ctx.Op.Catch(() => ctx.Op.AcceptValue(value: ctx.Document.Objects.AddClippingPlane(
                plane: clip.Plane, uMagnitude: clip.U, vMagnitude: clip.V,
                clippedViewportIds: Seq(ctx.Detail.Viewport.Id).AsIterable(), attributes: new ObjectAttributes())))
            from _minted in guard(id != Guid.Empty, ctx.Op.InvalidResult()).ToFin()
            select unit,
        toggleCase: static (ctx, clip) =>
            from plane in Optional(ctx.Document.Objects.FindId(objectId: clip.PlaneId) as ClippingPlaneObject).ToFin(Fail: ctx.Op.InvalidInput())
            let attached = toSeq(ctx.Document.Objects.FindClippingPlanesForViewport(viewport: ctx.Detail.Viewport)).Exists(row => row.Id == clip.PlaneId)
            from _toggled in (clip.Active, attached) switch {
                (true, true) or (false, false) => Fin.Succ(value: unit),
                (true, false) => ctx.Op.Confirm(success: plane.AddClipViewport(viewport: ctx.Detail.Viewport, commit: true)),
                (false, true) => ctx.Op.Confirm(success: plane.RemoveClipViewport(viewport: ctx.Detail.Viewport, commit: true)),
            }
            select unit,
        scopeCase: static (ctx, clip) =>
            from plane in Optional(ctx.Document.Objects.FindId(objectId: clip.PlaneId) as ClippingPlaneObject).ToFin(Fail: ctx.Op.InvalidInput())
            from geometry in Optional(plane.ClippingPlaneGeometry).ToFin(Fail: ctx.Op.InvalidInput())
            from _scoped in ctx.Op.Catch(() => {
                geometry.SetClipParticipation(
                    objectIds: clip.ObjectIds.AsIterable(), layerIndices: clip.LayerIndices.AsIterable(), isExclusionList: clip.Exclusive);
                return ctx.Op.Confirm(success: plane.CommitChanges());
            })
            select unit,
        pruneCase: static (ctx, _) => ctx.Op.Catch(() => {
            _ = toSeq(ctx.Document.Objects.FindClippingPlanesForViewport(viewport: ctx.Detail.Viewport))
                .Choose(plane => Optional(plane.ClippingPlaneGeometry).Map(geometry => {
                    _ = geometry.ViewportIds() is [Guid only] && only == ctx.Detail.Viewport.Id
                        ? Op.Side(() => ctx.Document.Objects.Delete(objectId: plane.Id, quiet: true))
                        : Op.Side(() => plane.RemoveClipViewport(viewport: ctx.Detail.Viewport, commit: true));
                    return plane.Id;
                }));
            return Fin.Succ(value: unit);
        }));
}
```

## [05]-[TRANSACTION_RAIL]

- Owner: `SheetSize` — page dimensions in an explicit unit resolved to page units through `RhinoMath.UnitScale`; `SheetSpec` — page desired state: name, size, group seat, ordinal seat. `NumberRule` — pattern numbering: a template over the stamp token grammar (`%pagenumber%`, `%page%`) plus a `Dimension` start ordinal, renaming and renumbering a page set in one fold. `ScaleConflict` `[Union]` — the audit evidence: `RatioMismatchCase` between a declared scale and the live `PageToModelRatio`, `PerspectiveScaleCase` for a scale expectation on a perspective detail, `PageUnitDriftCase` when the page unit regime undermines a formatted scale. `SheetOp` `[Union]` — the transaction family: `EnsureCase` creates-or-updates a page to its spec, `CloneCase` duplicates with or without geometry, `RetireCase` deletes, `AdoptCase` imports a page view from an archive by delegating the Document table rail's import row, `OrderCase` reorders and renumbers, `GroupCase` seats pages in a page-view group with optional exclusivity, `SpawnCase` creates a detail from `DetailSpec`, `StateCase` applies one `DetailState` across the matched details, `ArrangeCase` lays matched details out through the arrangement rows, `NumberCase` applies a `NumberRule`, `AuditCase` produces conflict evidence, `BatchCase` sequences a program. `SheetFact`/`SheetSlot`/`SheetReceipt` — the one receipt stream, `IDetachedDocumentResult` for the session demand.
- Entry: `Sheets.Commit(DocumentSession, SheetOp, Op?) : Fin<SheetReceipt>` — mutating cases demand `Mutate`+`Undo`+`Redraw` and run inside one host undo record bracketed on every exit path with one redraw on the bracket's success, the audit demands `Read`; `AdoptCase` routes through `Tables.Commit` as a recorded transaction so page import stays the Document rail's single undo-recorded row while its receipt folds into the sheet fact stream, and a `BatchCase` carrying an `AdoptCase` refuses at admission because adoption is a session-rail operation that never runs inside the demand-window program.
- Law: `EnsureCase` is idempotent by construction — an existing page updates toward the spec, an absent one is created then updated by the same field fold, so create-versus-configure is not a caller decision; the census `Create`/`Rename`/`Resize`/`Configure` quartet is this one case.
- Law: `AddDetailView` runs inside the active-view bracket — prior active view captured, page activated, and the prior view restored on every exit including failure.
- Law: ordering is total — `OrderCase` seats the named pages first in given order, retains every unnamed page in current order behind them, and hands the whole roster to the host's own `ReorderPageViews` renumbering; duplicate names refuse at admission.
- Law: the audit never mutates — conflicts are evidence rows computed from live host state (`PageToModelRatio`, projection class, `PageUnitSystem`), and an expected scale is compared through the same `SheetScale.Resolve` the writer uses, so audit and write share one arithmetic.
- Boundary: a page-unit regime change is the Document session's regime surface; a sheet operation that needs different page units composes that surface first, and this rail reads `PageUnitSystem` as found.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SheetSlot {
    public static readonly SheetSlot Created = new(key: 0);
    public static readonly SheetSlot Updated = new(key: 1);
    public static readonly SheetSlot Removed = new(key: 2);
    public static readonly SheetSlot Cloned = new(key: 3);
    public static readonly SheetSlot Adopted = new(key: 4);
    public static readonly SheetSlot Ordered = new(key: 5);
    public static readonly SheetSlot Grouped = new(key: 6);
    public static readonly SheetSlot Numbered = new(key: 7);
    public static readonly SheetSlot DetailCreated = new(key: 8);
    public static readonly SheetSlot DetailUpdated = new(key: 9);
    public static readonly SheetSlot Arranged = new(key: 10);
    public static readonly SheetSlot Audited = new(key: 11);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScaleConflict {
    private ScaleConflict() { }
    public sealed record RatioMismatchCase(string Sheet, string Detail, string Formatted, double LiveRatio, double DeclaredRatio) : ScaleConflict;
    public sealed record PerspectiveScaleCase(string Sheet, string Detail) : ScaleConflict;
    public sealed record PageUnitDriftCase(string Sheet, string Detail, UnitSystem PageUnits) : ScaleConflict;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SheetOp {
    private SheetOp() { }
    public sealed record EnsureCase(SheetSpec Spec) : SheetOp;
    public sealed record CloneCase(SheetSelect Sheets, bool WithGeometry) : SheetOp;
    public sealed record RetireCase(SheetSelect Sheets) : SheetOp;
    public sealed record AdoptCase(DocumentPath Source, Guid SourceViewportId, string Name) : SheetOp;
    public sealed record OrderCase(Seq<string> Names) : SheetOp;
    public sealed record GroupCase(SheetSelect Sheets, string Group, bool Exclusive) : SheetOp;
    public sealed record SpawnCase(SheetSelect Sheet, DetailSpec Spec) : SheetOp;
    public sealed record StateCase(SheetSelect Sheets, DetailSelect Details, DetailState State) : SheetOp;
    public sealed record ArrangeCase(SheetSelect Sheets, DetailSelect Details, DetailArrangement Arrangement, DetailAnchor Anchor, Point2d Offset, double Gutter, Dimension Columns) : SheetOp;
    public sealed record NumberCase(SheetSelect Sheets, NumberRule Rule) : SheetOp;
    public sealed record AuditCase(SheetSelect Sheets, DetailSelect Details, Option<SheetScale> Expected) : SheetOp;
    public sealed record BatchCase(Seq<SheetOp> Program) : SheetOp;

    internal Seq<SessionNeed> Needs() => this switch {
        AuditCase => Seq(SessionNeed.Read),
        _ => Seq(SessionNeed.Mutate, SessionNeed.Undo, SessionNeed.Redraw),
    };

    internal bool Mutates() => this switch {
        AuditCase => false,
        BatchCase batch => batch.Program.Exists(static inner => inner.Mutates()),
        _ => true,
    };

    internal bool Sessioned() => this switch {
        AdoptCase => true,
        BatchCase batch => batch.Program.Exists(static inner => inner.Sessioned()),
        _ => false,
    };
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct SheetSize(UnitSystem Units, double Width, double Height) {
    internal Fin<(double Width, double Height)> Resolve(RhinoDoc document, Op op) {
        SheetSize self = this;
        return from _units in guard(
                   self.Units is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits
                   && document.PageUnitSystem is not UnitSystem.None and not UnitSystem.Unset and not UnitSystem.CustomUnits,
                   op.InvalidInput()).ToFin()
               from _width in op.Positive(value: self.Width)
               from _height in op.Positive(value: self.Height)
               from scale in op.Positive(value: RhinoMath.UnitScale(self.Units, document.PageUnitSystem))
               select (self.Width * scale, self.Height * scale);
    }
}

public sealed record SheetSpec(string Name, Option<SheetSize> Size, Option<string> Group, Option<Dimension> Ordinal);

public sealed record NumberRule(string Template, Dimension Start);

public readonly record struct SheetFact(SheetSlot Slot, string Name, Option<Guid> Id);

public sealed record SheetReceipt(Seq<SheetFact> Facts, Seq<ScaleConflict> Conflicts) : IDetachedDocumentResult {
    internal static SheetReceipt Of(params ReadOnlySpan<SheetFact> facts) =>
        new(Facts: toSeq(facts.ToArray()), Conflicts: Seq<ScaleConflict>());

    public static SheetReceipt operator +(SheetReceipt left, SheetReceipt right) =>
        new(Facts: left.Facts + right.Facts, Conflicts: left.Conflicts + right.Conflicts);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Sheets {
    public static Fin<SheetReceipt> Commit(DocumentSession session, SheetOp request, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in Optional(request).ToFin(Fail: op.InvalidInput())
               from _sessioned in guard(admitted is SheetOp.AdoptCase || !admitted.Sessioned(), op.InvalidInput()).ToFin()
               from receipt in admitted switch {
                   SheetOp.AdoptCase adopt => Adopt(session: session, adopt: adopt, op: op),
                   _ => session.Demand(
                       use: document => Recorded(document: document, request: admitted, op: op),
                       key: op,
                       needs: [.. admitted.Needs()]),
               }
               select receipt;
    }

    private static Fin<SheetReceipt> Recorded(RhinoDoc document, SheetOp request, Op op) {
        if (!request.Mutates()) {
            return Apply(document: document, request: request, op: op);
        }
        uint record = document.BeginUndoRecord(description: nameof(Sheets));
        try {
            return Apply(document: document, request: request, op: op).Map(receipt => {
                document.Views.Redraw();
                return receipt;
            });
        } finally {
            _ = document.EndUndoRecord(undoRecordSerialNumber: record);
        }
    }

    private static Fin<SheetReceipt> Adopt(DocumentSession session, SheetOp.AdoptCase adopt, Op op) =>
        from name in op.AcceptText(value: adopt.Name)
        from row in TableOp.ImportPage(path: adopt.Source, mainViewportId: adopt.SourceViewportId, pageName: name)
        from transaction in TableTransaction.Recorded(nameof(Sheets), RedrawPolicy.Deferred, Seq<TableCustomUndo>(), row)
        from _receipt in Tables.Commit(session: session, transaction: transaction, key: op)
        select SheetReceipt.Of(new SheetFact(Slot: SheetSlot.Adopted, Name: name, Id: None));

    private static Fin<SheetReceipt> Apply(RhinoDoc document, SheetOp request, Op op) =>
        request.Switch(
            state: (Document: document, Op: op),
            ensureCase: static (ctx, edit) =>
                from name in ctx.Op.AcceptText(value: edit.Spec.Name)
                from existing in SheetSelect.Named(name: name).Resolve(document: ctx.Document, op: ctx.Op)
                from page in existing switch {
                    [var found, ..] => Fin.Succ(value: (Page: found, Created: false)),
                    _ =>
                        from size in edit.Spec.Size.Map(value => value.Resolve(document: ctx.Document, op: ctx.Op).Map(Some)).IfNone(Fin.Succ(value: Option<(double, double)>.None))
                        from minted in ctx.Op.Catch(() => Optional(size.Case switch {
                            (double width, double height) => ctx.Document.Views.AddPageView(title: name, pageWidth: width, pageHeight: height),
                            _ => ctx.Document.Views.AddPageView(title: name),
                        }).ToFin(Fail: ctx.Op.InvalidResult()))
                        select (Page: minted, Created: true),
                }
                from _size in edit.Spec.Size.Map(value =>
                    value.Resolve(document: ctx.Document, op: ctx.Op).Bind(resolved => ctx.Op.Catch(() => {
                        page.Page.PageWidth = resolved.Width;
                        page.Page.PageHeight = resolved.Height;
                        return Fin.Succ(value: unit);
                    }))).IfNone(Fin.Succ(value: unit))
                from _group in edit.Spec.Group.Map(groupName =>
                    Seated(document: ctx.Document, pages: Seq(page.Page), groupName: groupName, exclusive: false, op: ctx.Op).Map(static _ => unit)).IfNone(Fin.Succ(value: unit))
                from _ordinal in edit.Spec.Ordinal.Map(ordinal => ctx.Op.Catch(() => {
                    page.Page.PageNumber = ordinal.Value;
                    return Fin.Succ(value: unit);
                })).IfNone(Fin.Succ(value: unit))
                select SheetReceipt.Of(new SheetFact(
                    Slot: page.Created ? SheetSlot.Created : SheetSlot.Updated,
                    Name: name, Id: Some(page.Page.MainViewport.Id))),
            cloneCase: static (ctx, edit) =>
                from pages in edit.Sheets.Resolve(document: ctx.Document, op: ctx.Op)
                from facts in pages.TraverseM(page =>
                    from copy in ctx.Op.Catch(() => Optional(page.Duplicate(duplicatePageGeometry: edit.WithGeometry)).ToFin(Fail: ctx.Op.InvalidResult()))
                    select new SheetFact(Slot: SheetSlot.Cloned, Name: copy.PageName, Id: Some(copy.MainViewport.Id))).As()
                select new SheetReceipt(Facts: facts, Conflicts: Seq<ScaleConflict>()),
            retireCase: static (ctx, edit) =>
                from pages in edit.Sheets.Resolve(document: ctx.Document, op: ctx.Op)
                from facts in pages.TraverseM(page =>
                    from _pruned in DetailSelect.All.Resolve(page: page, op: ctx.Op).Bind(details =>
                        details.TraverseM(detail => Clips.Apply(rule: new ClipRule.PruneCase(), document: ctx.Document, detail: detail, op: ctx.Op)).As().Map(static _ => unit))
                    from name in Fin.Succ(value: page.PageName)
                    from id in Fin.Succ(value: page.MainViewport.Id)
                    from _closed in ctx.Op.Confirm(success: ctx.Document.Views.Delete(page))
                    select new SheetFact(Slot: SheetSlot.Removed, Name: name, Id: Some(id))).As()
                select new SheetReceipt(Facts: facts, Conflicts: Seq<ScaleConflict>()),
            adoptCase: static (ctx, _) => Fin.Fail<SheetReceipt>(error: ctx.Op.InvalidInput()),
            orderCase: static (ctx, edit) =>
                from names in edit.Names.TraverseM(name => ctx.Op.AcceptText(value: name)).As()
                from _unique in guard(names.Distinct().Count == names.Count, ctx.Op.InvalidInput()).ToFin()
                from named in names.TraverseM(name => SheetSelect.Named(name: name).Single(document: ctx.Document, op: ctx.Op)).As()
                from _ordered in ctx.Op.Catch(() => {
                    Seq<RhinoPageView> current = toSeq(ctx.Document.Views.GetPageViews()).OrderBy(static page => page.PageNumber).AsIterable().ToSeq();
                    LanguageExt.HashSet<Guid> seated = toHashSet(named.Map(static page => page.MainViewport.Id));
                    Seq<RhinoPageView> ordered = named + current.Filter(page => !seated.Contains(page.MainViewport.Id));
                    return ctx.Op.Confirm(success: ctx.Document.Views.ReorderPageViews(orderedPages: ordered.AsIterable()));
                })
                select SheetReceipt.Of(new SheetFact(Slot: SheetSlot.Ordered, Name: string.Join(';', names), Id: None)),
            groupCase: static (ctx, edit) =>
                from pages in edit.Sheets.Resolve(document: ctx.Document, op: ctx.Op)
                from groupName in ctx.Op.AcceptText(value: edit.Group)
                from facts in Seated(document: ctx.Document, pages: pages, groupName: groupName, exclusive: edit.Exclusive, op: ctx.Op)
                select new SheetReceipt(Facts: facts, Conflicts: Seq<ScaleConflict>()),
            spawnCase: static (ctx, edit) =>
                from page in edit.Sheet.Single(document: ctx.Document, op: ctx.Op)
                from name in ctx.Op.AcceptText(value: edit.Spec.Name)
                from fact in ctx.Op.Catch(() => {
                    RhinoView? prior = ctx.Document.Views.ActiveView;
                    try {
                        ctx.Document.Views.ActiveView = page;
                        page.SetPageAsActive();
                        return from detail in Optional(page.AddDetailView(
                                   title: name, corner0: edit.Spec.Corner, corner1: edit.Spec.Opposite, initialProjection: edit.Spec.Projection))
                                   .ToFin(Fail: ctx.Op.InvalidResult())
                               from commit in new DetailState(
                                   Name: Some(name),
                                   LockProjection: Some(edit.Spec.ProjectionLocked),
                                   DisplayMode: edit.Spec.DisplayMode,
                                   Scale: edit.Spec.Scale).Apply(document: ctx.Document, page: page, detail: detail, op: ctx.Op)
                               select new SheetFact(Slot: SheetSlot.DetailCreated, Name: name, Id: Some(detail.Id));
                    } finally {
                        _ = prior is { } view ? Op.Side(() => ctx.Document.Views.ActiveView = view) : unit;
                    }
                })
                select SheetReceipt.Of(fact),
            stateCase: static (ctx, edit) =>
                from _touches in guard(edit.State.Touches, ctx.Op.InvalidInput()).ToFin()
                from pages in edit.Sheets.Resolve(document: ctx.Document, op: ctx.Op)
                from facts in pages.TraverseM(page =>
                    edit.Details.Resolve(page: page, op: ctx.Op).Bind(details =>
                        details.TraverseM(detail =>
                            edit.State.Apply(document: ctx.Document, page: page, detail: detail, op: ctx.Op)
                                .Map(_ => new SheetFact(
                                    Slot: SheetSlot.DetailUpdated,
                                    Name: DetailSelect.NameOf(detail: detail).IfNone(page.PageName),
                                    Id: Some(detail.Id)))).As())).As()
                select new SheetReceipt(Facts: facts.Bind(identity), Conflicts: Seq<ScaleConflict>()),
            arrangeCase: static (ctx, edit) =>
                from pages in edit.Sheets.Resolve(document: ctx.Document, op: ctx.Op)
                from facts in pages.TraverseM(page =>
                    edit.Details.Resolve(page: page, op: ctx.Op).Bind(details =>
                        details.Map(static (detail, index) => (Detail: detail, Index: index)).TraverseM(row =>
                            from current in DetailState.DetailFrameOf(detail: row.Detail, op: ctx.Op)
                            from frame in edit.Arrangement.Frame(context: new LayoutContext(
                                Current: current, Page: (page.PageWidth, page.PageHeight),
                                Anchor: edit.Anchor, Offset: edit.Offset, Gutter: edit.Gutter, Columns: edit.Columns,
                                Index: row.Index, Count: details.Count, Key: ctx.Op))
                            from _moved in new DetailState(Frame: Some(frame)).Apply(document: ctx.Document, page: page, detail: row.Detail, op: ctx.Op)
                            select new SheetFact(Slot: SheetSlot.Arranged, Name: page.PageName, Id: Some(row.Detail.Id))).As())).As()
                select new SheetReceipt(Facts: facts.Bind(identity), Conflicts: Seq<ScaleConflict>()),
            numberCase: static (ctx, edit) =>
                from pages in edit.Sheets.Resolve(document: ctx.Document, op: ctx.Op)
                from _template in ctx.Op.AcceptText(value: edit.Rule.Template)
                from facts in pages.Map(static (page, index) => (Page: page, Index: index)).TraverseM(row => ctx.Op.Catch(() => {
                    int ordinal = edit.Rule.Start.Value + row.Index;
                    string name = edit.Rule.Template
                        .Replace("%pagenumber%", ordinal.ToString(provider: System.Globalization.CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
                        .Replace("%page%", row.Page.PageName, StringComparison.OrdinalIgnoreCase);
                    row.Page.PageName = name;
                    row.Page.PageNumber = ordinal;
                    return Fin.Succ(value: new SheetFact(Slot: SheetSlot.Numbered, Name: name, Id: Some(row.Page.MainViewport.Id)));
                })).As()
                select new SheetReceipt(Facts: facts, Conflicts: Seq<ScaleConflict>()),
            auditCase: static (ctx, edit) =>
                from pages in edit.Sheets.Resolve(document: ctx.Document, op: ctx.Op)
                from declared in edit.Expected.Map(scale =>
                    scale.Resolve(document: ctx.Document, op: ctx.Op).Map(resolved => Some(
                        (resolved.PageLength * RhinoMath.UnitScale(resolved.PageUnit, ctx.Document.ModelUnitSystem))
                        / (resolved.ModelLength * RhinoMath.UnitScale(resolved.ModelUnit, ctx.Document.ModelUnitSystem)))))
                    .IfNone(Fin.Succ(value: Option<double>.None))
                from conflicts in pages.TraverseM(page =>
                    edit.Details.Resolve(page: page, op: ctx.Op).Map(details =>
                        details.Bind(detail => Judge(page: page, detail: detail, declared: declared, pageUnits: ctx.Document.PageUnitSystem)))).As()
                select new SheetReceipt(
                    Facts: Seq(new SheetFact(Slot: SheetSlot.Audited, Name: nameof(SheetSlot.Audited), Id: None)),
                    Conflicts: conflicts.Bind(identity)),
            batchCase: static (ctx, edit) =>
                edit.Program
                    .TraverseM(inner => Apply(document: ctx.Document, request: inner, op: ctx.Op))
                    .As()
                    .Map(static receipts => receipts.Fold(new SheetReceipt(Facts: Seq<SheetFact>(), Conflicts: Seq<ScaleConflict>()), static (folded, receipt) => folded + receipt)));

    private static Seq<ScaleConflict> Judge(RhinoPageView page, DetailViewObject detail, Option<double> declared, UnitSystem pageUnits) {
        string detailName = DetailSelect.NameOf(detail: detail).IfNone(noneValue: string.Empty);
        return detail.DetailGeometry is { IsParallelProjection: true } geometry
            ? declared.Case switch {
                double expected when geometry.PageToModelRatio > 0.0
                    && Math.Abs(geometry.PageToModelRatio - expected) / expected > 1e-6 =>
                    Seq<ScaleConflict>(new ScaleConflict.RatioMismatchCase(
                        Sheet: page.PageName, Detail: detailName,
                        Formatted: SheetScale.Format(detail: detail).IfNone(noneValue: string.Empty),
                        LiveRatio: geometry.PageToModelRatio, DeclaredRatio: expected)),
                _ => pageUnits is UnitSystem.None or UnitSystem.Unset or UnitSystem.CustomUnits
                    ? Seq<ScaleConflict>(new ScaleConflict.PageUnitDriftCase(Sheet: page.PageName, Detail: detailName, PageUnits: pageUnits))
                    : Seq<ScaleConflict>(),
            }
            : declared.IsSome
                ? Seq<ScaleConflict>(new ScaleConflict.PerspectiveScaleCase(Sheet: page.PageName, Detail: detailName))
                : Seq<ScaleConflict>();
    }

    private static Fin<Seq<SheetFact>> Seated(RhinoDoc document, Seq<RhinoPageView> pages, string groupName, bool exclusive, Op op) =>
        from group in op.Catch(() => document.PageViewGroups.FindName(name: groupName) switch {
            PageViewGroup existing => Fin.Succ(value: existing),
            _ => document.PageViewGroups.Add(new PageViewGroup { Name = groupName }, pages.AsIterable()) switch {
                int index when index >= 0 => Optional(document.PageViewGroups.FindIndex(index: index)).ToFin(Fail: op.InvalidResult()),
                _ => Fin.Fail<PageViewGroup>(error: op.InvalidResult()),
            },
        })
        from facts in pages.TraverseM(page => op.Catch(() => {
            _ = Op.SideWhen(exclusive, () => ignore(toSeq(page.GetPageViewGroupList())
                .Filter(index => index != group.Index)
                .Map(index => page.RemoveFromPageViewGroup(pageViewGroupIndex: index))));
            _ = Op.SideWhen(!page.IsInPageViewGroup(pageViewGroupIndex: group.Index), () => page.AddToPageViewGroup(pageViewGroupIndex: group.Index));
            return Fin.Succ(value: new SheetFact(Slot: SheetSlot.Grouped, Name: page.PageName, Id: Some(page.MainViewport.Id)));
        })).As()
        select facts;
}
```

```mermaid
flowchart LR
    Select["SheetSelect · DetailSelect — addressing as data"] --> Rail["Sheets.Commit"]
    Request["SheetOp — Ensure · Clone · Retire · Adopt · Order · Group · Spawn · State · Arrange · Number · Audit · Batch"] --> Rail
    Rail -->|Adopt| Tables["Tables.Commit — TableOp.ImportPage row"]
    Rail -->|everything else, derived Needs| Demand["session.Demand — Mutate · Undo · Redraw or Read"]
    Demand --> State["DetailState correspondence — field rows → host writers + DetailCommit union"]
    State -->|commit at most once each| Commit["CommitChanges · CommitViewportChanges"]
    Demand --> Layout["DetailArrangement rows — UnitInterval anchors · Dimension grids"]
    Demand --> Audit["scale audit — PageToModelRatio vs SheetScale.Resolve"]
    Audit --> Evidence["ScaleConflict evidence"]
    Commit --> Receipt["SheetReceipt — SheetFact stream"]
    Evidence --> Receipt
```

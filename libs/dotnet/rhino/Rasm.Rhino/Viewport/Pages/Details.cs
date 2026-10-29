using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using UnitsNet;

namespace Rasm.Rhino.Viewport.Pages;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DetailState(
    Option<string> Name = default, Option<DrawingScale> Scale = default, Option<bool> ProjectionLocked = default, Option<bool> Active = default,
    Option<HashMap<Guid, bool>> Layers = default) {
    public DetailState Except(DetailState held) =>
        new(Name.Filter(name => held.Name != name), Scale.Filter(scale => held.Scale != scale),
            ProjectionLocked.Filter(locked => held.ProjectionLocked != locked), Active.Filter(active => held.Active != active),
            Layers.Map(layers => layers.Filter((layer, visible) => held.Layers.Bind(stored => stored.Find(layer)).IfNone(noneValue: true) != visible)).Filter(static layers => !layers.IsEmpty));
}

[SmartEnum]
public sealed partial class FrameAlignment {
    public static readonly FrameAlignment Start = new(0d);
    public static readonly FrameAlignment Center = new(0.5d);
    public static readonly FrameAlignment End = new(1d);

    private readonly double _fraction;

    public Vector3d Shift(BoundingBox frame, BoundingBox target) => target.PointAt(_fraction, _fraction, _fraction) - frame.PointAt(_fraction, _fraction, _fraction);
}

[SmartEnum]
public sealed partial class PaperAxis {
    public static readonly PaperAxis Horizontal = new(Vector3d.XAxis);
    public static readonly PaperAxis Vertical = new(Vector3d.YAxis);

    private readonly Vector3d _direction;

    public Vector3d Along(Vector3d offset) => _direction * (offset * _direction);
}

[Union]
public abstract partial record Arrangement {
    public sealed record Grid(Seq<Seq<Guid>> Rows, FrameAlignment Across, FrameAlignment Down) : Arrangement;

    public sealed record Align(Seq<Guid> Ids, PaperAxis Axis, FrameAlignment Alignment) : Arrangement;

    public sealed record Distribute(Seq<Guid> Ids, PaperAxis Axis) : Arrangement;

    public Seq<Guid> Details => Switch(grid: static grid => grid.Rows.Flatten(), align: static align => align.Ids, distribute: static distribute => distribute.Ids);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Details {
    // --- [READS]
    public static IO<Seq<(DetailViewObject Detail, DetailState State)>> Read(RhinoPageView page) =>
        IO.lift(() => Products.Each(toSeq(page.GetDetailViews()), (detail, index) =>
            Conversions.Required(detail.DetailGeometry, index).Bind(geometry => State(page.Document, detail, geometry)).Map(state => (detail, state))));

    private static Fin<DetailState> State(RhinoDoc doc, DetailViewObject detail, DetailView geometry) =>
        Conversions.Present(geometry.IsParallelProjection && geometry.PageToModelRatio > 0d, geometry.PageToModelRatio)
            .Traverse(static ratio => DrawingScale.Validated(ratio < 1d ? QuantityValue.Inverse(1d / ratio) : ratio)).As()
            .Map(scale => new DetailState(Conversions.Present(detail.Attributes.Name), scale, geometry.IsProjectionLocked, detail.IsActive, Some(Hidden(doc, detail.Viewport.Id))));

    private static HashMap<Guid, bool> Hidden(RhinoDoc doc, Guid viewport) =>
        toHashMap(Conversions.Rows(doc.Layers).Filter(layer => !layer.IsDeleted && !layer.PerViewportIsVisible(viewport)).Map(static layer => (layer.Id, false)));

    // --- [WRITES]
    public static IO<Guid> Add(RhinoPageView page, string title, BoundingBox frame, DefinedViewportProjection projection) =>
        Products.Of(() => page.AddDetailView(title, new Point2d(frame.Min), new Point2d(frame.Max), projection)).Map(static detail => detail.Id);

    public static IO<Unit> Write(RhinoDoc doc, Guid detailId, DetailState wanted) =>
        from found in ObjectTarget.Resolve<DetailViewObject, DetailView>(doc, detailId)
        from page in Products.Of(() => found.Object.ParentPageView)
        from held in IO.lift(() => State(doc, found.Object, found.Geometry))
        let change = wanted.Except(held)
        from activated in change.Active.Traverse(active => active
            ? Products.Accepted(() => page.SetActiveDetail(detailId))
            : IO.lift(page.SetPageAsActive)).As()
        from scaled in change.Scale.Traverse(scale => Products.Accepted(() => found.Geometry.SetScale(
            Quantities.As(scale.ToModel(Quantities.From(1d, doc.PageUnits)), doc.ModelUnits), doc.ModelUnits, 1d, doc.PageUnits))).As()
        from locked in IO.lift(() => change.ProjectionLocked.Iter(value => found.Geometry.IsProjectionLocked = value))
        from committed in when(change.Scale.IsSome || change.ProjectionLocked.IsSome, Products.Accepted(found.Object.CommitChanges)).As()
        from layered in change.Layers.Traverse(layers => toSeq(layers.AsIterable()).TraverseM(row => Rows.Edit(doc, Layers.Kind, new ComponentRef<Layer>.ById(row.Key), [
            Rows.Set<Layer, bool>(layer => layer.PerViewportIsVisible(detailId), (layer, visible) => layer.SetPerViewportVisible(detailId, visible), row.Value)])).As()).As()
        from named in change.Name.Traverse(name => Objects.AttributeEdits.Modify(doc, [found.Object], _ => [Rows.Set<ObjectAttributes, string>(static attributes => attributes.Name, static (attributes, held) => attributes.Name = held, name).Write], quiet: true)).As()
        select unit;

    // --- [ARRANGEMENT]
    public static BoundingBox Field(PaperRectangle field, LengthUnit page) =>
        new(Quantities.As(field.Left, page), Quantities.As(field.Bottom, page), 0d, Quantities.As(field.Right, page), Quantities.As(field.Top, page), 0d);

    public static BoundingBox Clear(BoundingBox field, BoundingBox keepOut) =>
        BoundingBox.Intersection(field, new BoundingBox(keepOut.Min.X, keepOut.Min.Y, field.Min.Z, keepOut.Max.X, keepOut.Max.Y, field.Max.Z)) is { IsValid: true } overlap
            ? toSeq(PaperAxis.Items).Bind(axis => Seq(
                    new BoundingBox(field.Min, field.Max - axis.Along(field.Max - overlap.Min)), new BoundingBox(field.Min + axis.Along(overlap.Max - field.Min), field.Max)))
                .MaxBy(static box => box.Diagonal.X * box.Diagonal.Y)
            : field;

    public static Seq<(TItem Item, BoundingBox Cell)> Cells<TItem>(BoundingBox region, Seq<Seq<TItem>> rows) =>
        rows.Map((row, r) => row.Map((item, c) => (Item: item, Cell: new BoundingBox(
            region.PointAt((double)c / row.Count, 1d - ((r + 1d) / rows.Count), 0d), region.PointAt((c + 1d) / row.Count, 1d - ((double)r / rows.Count), 1d))))).Flatten();

    public static Seq<(Guid Id, Vector3d Offset)> Offsets(BoundingBox region, Arrangement arrangement, HashMap<Guid, BoundingBox> frames) =>
        arrangement.Switch((Region: region, Frames: frames),
            grid: static (state, grid) => Cells(state.Region, grid.Rows).Map(cell => (Id: cell.Item, Offset:
                PaperAxis.Horizontal.Along(grid.Across.Shift(state.Frames[cell.Item], cell.Cell)) + PaperAxis.Vertical.Along(grid.Down.Shift(state.Frames[cell.Item], cell.Cell)))),
            align: static (state, align) => align.Ids.Map(id => (Id: id, Offset: align.Axis.Along(align.Alignment.Shift(state.Frames[id], state.Region)))),
            distribute: static (state, distribute) => Spaced(distribute.Axis, state.Region, distribute.Ids.Map(id => (Id: id, Frame: state.Frames[id]))));

    public static IO<Seq<Guid>> Arrange(RhinoDoc doc, BoundingBox region, Arrangement arrangement) =>
        from ids in IO.lift(Conversions.Unique(arrangement.Details, identity))
        from frames in ids.TraverseM(id => ObjectTarget.Resolve<DetailViewObject, DetailView>(doc, id).Map(found => (Key: id, Value: found.Geometry.GetBoundingBox(accurate: true)))).As()
        from moved in Offsets(region, arrangement, toHashMap(frames)).Filter(static row => !row.Offset.IsZero).TraverseM(row =>
            new ObjectTarget.Ids(Seq(row.Id)).Each(doc, found => doc.Objects.Transform(found, Transform.Translation(row.Offset), deleteOriginal: true) != Guid.Empty)).As()
        select moved.Flatten();

    private static Seq<(Guid Id, Vector3d Offset)> Spaced(PaperAxis axis, BoundingBox region, Seq<(Guid Id, BoundingBox Frame)> rows) =>
        fun((Vector3d gap) => rows.Zip(rows.Scan(gap, (start, row) => start + axis.Along(row.Frame.Diagonal) + gap), (row, start) => (row.Id, Offset: axis.Along(region.Min - row.Frame.Min) + start)))(
            rows.Fold(axis.Along(region.Diagonal), (free, row) => free - axis.Along(row.Frame.Diagonal)) / (rows.Count + 1));
}

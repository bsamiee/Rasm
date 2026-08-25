# [APPUI_CAD_WRITE]

The CAD write leg turns one `Render/drafting#DRAFT_EMIT` `SheetEntity` run into one `ACadSharp` `CadDocument` and serializes it through the DWG, DXF, or SVG writer row. `CadDraw` is the ONE entity fold — the Y-down-to-Y-up reframing, the model-space placement of each sheet on the frame's own pitch, and the per-sheet arc bound inversion all live here and nowhere else — while `CadWriter` carries the three serializations as rows over that single document so a DWG, a DXF, and an SVG of one drawing set hold identical entities by construction. `CadVersionPolicy` is the writer ABI: output version per format, the DXF encoding, and the SVG line-weight ratio.

Layer identity, line rhythm, and line width are all `Rasm/Drawing/sheet`'s: an `EdgeStyle` row answers its kernel `LayerName` and its ISO 128-24 width, `LineType.Rhythm(width)` derives the dash-and-gap pattern the table entry records, and `HostLayerScheme.AutoCadFlat` projects the name into the DWG layer table. This page decides nothing about what a layer is called, how wide a line draws, or how a dash spaces — it decides only which ACadSharp object carries each drawn primitive, and it names every capability of that library it deliberately does not reach.

## [01]-[INDEX]

- [02]-[CAD_DOCUMENT]: The one `SheetEntity` fold onto the `CadDocument` graph — layer and line-type registration, the Y flip, the per-sheet model-space pitch.
- [03]-[CAD_WRITE]: The writer rows and the version ABI they read.
- [04]-[CAD_BOUNDARY]: What the entity fold reaches in ACadSharp and what it refuses by name.

## [02]-[CAD_DOCUMENT]

- Owner: `CadDraw` the one `Seq<Seq<SheetEntity>>`-to-`CadDocument` fold with its layer and line-type registration, its single reframing, and its per-sheet placement.
- Entry: `public static Fin<CadDocument> Build(SheetSize size, Seq<Seq<SheetEntity>> pages, Op? key = null)` — the whole set folds into ONE document: every distinct `(EdgeStyle, Part)` seat registers its layer and its line type once, then each page's entities place at that sheet's own model-space origin. The rail is `Fin` because the layer name, the line width, and the frame margin are all kernel admissions.
- Auto: layer names are the kernel `LayerStandard.Estate` grammar projected through `HostLayerScheme.AutoCadFlat`, so a chrome stroke lands on its style's layer and a model-edge stroke carrying its kernel `Part` ordinal lands on that style's part-fielded layer, and a multi-part drawing's parts toggle independently in any CAD host; line types are `LineType.Rhythm(width)` at the sheet's own ISO 128-24 width, so a hidden line and a long-dashed-dotted axis emit distinguishable patterns and a widened line group re-spaces every dash with no edit here; the seat roster is DISTINCT before anything registers, so a layer and a line type enter the document's tables exactly once for the whole set; sheets lay at their own standard's pitch — the set's extent plus the frame's two facing margins — so sheet N's border never crosses sheet N+1's and the gap is the standard's clear space rather than an authored constant.
- Packages: ACadSharp (`CadDocument`, `Entity`, `Line`, `Arc`, `MText`, `LwPolyline`, `Tables.Layer`, `Tables.LineType`, `CSMath.XYZ`/`XY`), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`, `Seq.Traverse`, `HashMap`), Rasm (project — `Rasm/Drawing/sheet`: `SheetSize`, `SheetFrame`, `SheetMargin`, `LineType`, `LineWidth`, `LayerName`, `HostLayerScheme`; `Rasm/Domain/rails`: `Op`), Rasm.AppUi `Render/drafting` (`SheetEntity`, `EdgeStyle`, `DraftFault`), BCL inbox
- Growth: a new drawn primitive is one `SheetEntity` case that breaks this fold at compile time so no writer can silently drop it; a new drawing role is an `EdgeStyle` row whose layer and line type mint by construction; zero new surface.
- Exemption: `Build`, `Registered`, `Added`, and `Polyline` carry STATEMENT bodies under the boundary-kernel law — `CadDocument`, its table collections, and `LwPolyline.Vertices` are mutable native builders with get-only collections, so the graph is assembled by ordered mutation. What that exemption does NOT buy is expression-shaped statement smuggling: the three `(sideEffect, value).Item2` comma-operator switches this fold carried were side effects wearing an expression, and they delete.
- Boundary: sheet space is Y-DOWN — the frame the Skia canvas draws in and the one the title block anchors in — while CAD model space is Y-UP, so the flip lives at THIS one boundary exactly as the millimetre-to-point scale lives at the PDF one; the same projection carries the sheet's model-space origin, so a second placement rule per entity arm cannot exist. Writing sheet ordinates raw into a `CadDocument` mirrors the whole drawing against its own PDF while every congruence claim still reads true, so a second pre-reframed entity run and a per-format projection are both the deleted forms. Arc bounds negate AND swap, because an ACadSharp `Arc` always sweeps start to end COUNTER-clockwise in a Y-up frame while `Math.Atan2` over Y-down deltas measured clockwise — negating alone draws the complementary arc and leaving both sweeps the wedge the opposite way from the PDF. The solid line type is the document's OWN registered entry read off its table; the static `LineType.Continuous` is a factory property minting a fresh unregistered entry on every read, so binding it per layer seats one distinct "Continuous" instance per style and the writers reject the duplicate table rows (`RULINGS.md:146`). Text lands as `MText` at the entity's own kernel `TextHeight` in millimetres on the annotation style's layer.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------

public static class CadDraw {
    public static Fin<CadDocument> Build(SheetSize size, Seq<Seq<SheetEntity>> pages, Op? key = null) {
        Op seat = key.OrDefault();
        CadDocument doc = new();
        ACadSharp.Tables.LineType solid = doc.LineTypes.Continuous;
        Fin<SheetMargin> margin = SheetFrame.For(size.Standard).Margin(size: size, key: seat);
        Fin<HashMap<(EdgeStyle Style, Option<int> Part), Layer>> registered = Seats(pages)
            .Traverse(row => Minted(doc, solid, size, row, seat)).As()
            .Map(static rows => rows.Fold(
                HashMap<(EdgeStyle, Option<int>), Layer>(),
                static (held, row) => held.Add(row.Seat, row.Layer)));
        return from insets in margin
               from layers in registered
               let pitch = size.Width.Millimeters + insets.Left.Millimeters + insets.Right.Millimeters
               let height = size.Height.Millimeters
               from placed in pages
                   .Map((page, ordinal) => Placed(doc, height, ordinal * pitch, page, layers))
                   .Traverse(static row => row).As()
               select doc;
    }

    private static Seq<(EdgeStyle Style, Option<int> Part)> Seats(Seq<Seq<SheetEntity>> pages) =>
        pages.Bind(static page => page).Map(static entity => entity.Switch(
            stroke: static s => (s.Style, s.Part),
            sweep: static s => (s.Style, Option<int>.None),
            textRun: static _ => (EdgeStyle.Annotation, Option<int>.None),
            glyph: static _ => (EdgeStyle.Annotation, Option<int>.None),
            fill: static f => (f.Style, Option<int>.None))).Distinct();

    private static Fin<((EdgeStyle Style, Option<int> Part) Seat, Layer Layer)> Minted(
        CadDocument doc, ACadSharp.Tables.LineType solid, SheetSize size,
        (EdgeStyle Style, Option<int> Part) seat, Op key) =>
        from name in seat.Style.Layer(part: seat.Part, key: key)
        from width in seat.Style.Width(size: size, key: key)
        select (Seat: seat, Layer: Registered(doc, HostLayerScheme.AutoCadFlat.Path(name), seat.Style.Type, width, solid));

    private static Layer Registered(
        CadDocument doc, string path, Rasm.Drawing.LineType type, LineWidth width, ACadSharp.Tables.LineType solid) {
        if (type.IsContinuous) {
            Layer plain = new(path) { LineType = solid };
            doc.Layers.Add(plain);
            return plain;
        }
        ACadSharp.Tables.LineType patterned =
            new($"{type.Key}-{width.Key}") { Description = $"ISO 128-2 type {type.Key} at {width.Key} mm" };
        type.Rhythm(width).Iter(pair => {
            patterned.AddSegment(new ACadSharp.Tables.LineType.Segment { Length = pair.Drawn.Millimeters });
            patterned.AddSegment(new ACadSharp.Tables.LineType.Segment { Length = -pair.Gap.Millimeters });
        });
        doc.LineTypes.Add(patterned);
        Layer row = new(path) { LineType = patterned };
        doc.Layers.Add(row);
        return row;
    }

    private static Fin<Unit> Placed(
        CadDocument doc, double heightMm, double originX, Seq<SheetEntity> entities,
        HashMap<(EdgeStyle Style, Option<int> Part), Layer> layers) =>
        entities.Traverse(entity => entity.Switch(
            state: (Doc: doc, Height: heightMm, Origin: originX, Layers: layers),
            stroke: static (ctx, s) => Seated(ctx.Layers, s.Style, s.Part).Map(layer => Added(
                ctx.Doc, new Line(Cad(ctx.Height, ctx.Origin, s.A), Cad(ctx.Height, ctx.Origin, s.B)) { Layer = layer })),
            sweep: static (ctx, s) => Seated(ctx.Layers, s.Style, None).Map(layer => Added(ctx.Doc, new Arc {
                Center = Cad(ctx.Height, ctx.Origin, s.Center),
                Radius = s.Radius,
                StartAngle = double.DegreesToRadians(-(s.StartDeg + s.SweepDeg)),
                EndAngle = double.DegreesToRadians(-s.StartDeg),
                Layer = layer,
            })),
            textRun: static (ctx, t) => Seated(ctx.Layers, EdgeStyle.Annotation, None).Map(layer => Added(ctx.Doc, new MText {
                Value = t.Value,
                InsertPoint = Cad(ctx.Height, ctx.Origin, t.At),
                Height = t.Height.Height.Millimeters,
                Layer = layer,
            })),
            glyph: static (ctx, g) => Seated(ctx.Layers, EdgeStyle.Annotation, None).Map(layer => Added(ctx.Doc, new MText {
                Value = g.Symbol,
                InsertPoint = Cad(ctx.Height, ctx.Origin, g.At),
                Height = g.Height.Height.Millimeters,
                Layer = layer,
            })),
            fill: static (ctx, f) => Seated(ctx.Layers, f.Style, None).Map(layer =>
                f.Courses.Fold(unit, (_, course) => Added(ctx.Doc, Polyline(ctx.Height, ctx.Origin, course, layer))))))
            .As().Map(static _ => unit);

    private static Fin<Layer> Seated(
        HashMap<(EdgeStyle Style, Option<int> Part), Layer> layers, EdgeStyle style, Option<int> part) =>
        layers.Find((style, part))
            .ToFin(new DraftFault.EmptyView($"cad/layer: {style.Key} was never registered for this set"));

    private static Unit Added(CadDocument doc, Entity entity) {
        doc.Entities.Add(entity);
        return unit;
    }

    private static Entity Polyline(double heightMm, double originX, Seq<(double X, double Y)> course, Layer layer) {
        LwPolyline poly = new() { Layer = layer };
        course.Iter(point => poly.Vertices.Add(new LwPolyline.Vertex(new CSMath.XY(originX + point.X, heightMm - point.Y))));
        return poly;
    }

    private static CSMath.XYZ Cad(double heightMm, double originX, (double X, double Y) at) =>
        new(originX + at.X, heightMm - at.Y, 0d);
}
```

## [03]-[CAD_WRITE]

- Owner: `DxfEncoding` the DXF serialization form; `CadVersionPolicy` the writer ABI value; `CadWriter` the three writer rows over one document fold.
- Cases: `DxfEncoding` = ascii · binary; `CadWriter` = dwg · dxf · svg under the locked format literals.
- Entry: `public Fin<byte[]> Emit(SheetSize size, Seq<Seq<SheetEntity>> pages, CadVersionPolicy version, Op? key = null)` — one row builds the shared document and serializes it through its own writer; the `Render/drafting#DRAFT_EMIT` `CadArm` gauges that call on the kernel timeline and delivers the bytes, so the writer row owns bytes and nothing else.
- Packages: ACadSharp (`ACadVersion`, `IO.DwgWriter`, `IO.DxfWriter`, `IO.SvgWriter`, `IO.SvgConfiguration`), Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project — `Rasm/Drawing/sheet`: `SheetSize`), Rasm.AppUi `Render/drafting` (`SheetEntity`), BCL inbox (`MemoryStream`, `Stream`)
- Growth: a new CAD serialization is one `CadWriter` row carrying its own writer call; a new version axis is one `CadVersionPolicy` column; zero new surface.
- Boundary: the version policy is ROW-THREADED — each writer row names the version column it reads and no writer arm carries a call-site literal, so the hardcoded `AutoCad2018` this replaces is the deleted form; the DXF serialization form is a `DxfEncoding` ROW rather than a `bool` on a policy record, and its `Binary` column is the HOST projection read at exactly one call, which is where boundary spellings belong; the SVG writer's `Configuration` is the `CadWriterBase<SvgConfiguration>` slot, so the policy row's line-weight ratio lands on the writer's own config rather than on a constructed one, and the `SKSvgCanvas` presentation arm is the deleted second-SVG-semantic form; all three rows fold the SAME document, so the three formats cannot carry different entities.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DxfEncoding {
    public static readonly DxfEncoding Ascii = new(key: "ascii", binary: false);
    public static readonly DxfEncoding Binary = new(key: "binary", binary: true);
    internal bool IsBinary { get; }
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record CadVersionPolicy(ACadVersion Dwg, ACadVersion Dxf, DxfEncoding Encoding, double SvgLineWeightRatio) {
    public static readonly CadVersionPolicy Default =
        new(Dwg: ACadVersion.AC1032, Dxf: ACadVersion.AC1032, Encoding: DxfEncoding.Ascii, SvgLineWeightRatio: 1d);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CadWriter {
    public static readonly CadWriter Dwg = new(key: "dwg", serialize: static (doc, version, sink) => {
        doc.Header.Version = version.Dwg;
        DwgWriter.Write(sink, doc);
    });
    public static readonly CadWriter Dxf = new(key: "dxf", serialize: static (doc, version, sink) => {
        doc.Header.Version = version.Dxf;
        DxfWriter.Write(sink, doc, binary: version.Encoding.IsBinary);
    });
    public static readonly CadWriter Svg = new(key: "svg", serialize: static (doc, version, sink) => {
        SvgWriter writer = new(sink, doc);
        writer.Configuration.LineWeightRatio = version.SvgLineWeightRatio;
        writer.Write();
    });

    [UseDelegateFromConstructor]
    internal partial void Serialize(CadDocument doc, CadVersionPolicy version, Stream sink);

    public Fin<byte[]> Emit(SheetSize size, Seq<Seq<SheetEntity>> pages, CadVersionPolicy version, Op? key = null) =>
        CadDraw.Build(size: size, pages: pages, key: key).Map(doc => {
            using MemoryStream sink = new();
            Serialize(doc, version, sink);
            return sink.ToArray();
        });
}
```

## [04]-[CAD_BOUNDARY]

- [DRAFT_ENTITY]: one `Seq<Seq<SheetEntity>>` fold constructs the `CadDocument`, `Line`, `Arc`, `MText`, `LwPolyline`, `Layer`, and `LineType` graph consumed by `DwgWriter.Write`, `DxfWriter.Write`, and `SvgWriter.Write`, taking the sheet size so the one Y-down-to-Y-up reframing and the one per-sheet pitch land here and nowhere else. `EdgeStyle` selects the registered layer and line type under the kernel `LayerName` grammar and the kernel `LineType.Rhythm` derivation, `CadVersionPolicy` carries the version, encoding, and SVG line-weight ABI, and `DraftFormat.Switch` at `Render/drafting#DRAFT_EMIT` stays total across PDF, SVG, DWG, and DXF.
- [HATCH_REFUSED]: the `Hatch` entity is REFUSED. It re-solves a pattern inside the writer from a boundary the kernel already clipped exactly against the view's own loops, so the CAD file would carry a different fill than the PDF and the SVG. One `LwPolyline` per chained course keeps the three writers and the raster arm congruent by construction, and the entity count tracks the kernel's own chaining rather than its segment count.
- [ASSOCIATIVE_DIMENSION_REFUSED]: `Dimension` and `Tables.DimensionStyle` are REFUSED, so a DWG consumer receives dimension linework as `Line` and `MText` on the marking layer with the refusal stated here rather than inferred. The reason is anchor space: an associative CAD dimension measures MODEL-space anchors and re-renders its own text from a style table, while every anchor this page holds has already crossed the Fabrication projection into sheet space at the region's own scale and every label has already crossed `DraftUnits.Text` under the sheet's elected unit and the locale's grammar. Emitting associative dimensions would hand the CAD host a second measurement authority over figures the drawing already resolved, and the host's own `DimensionStyle` would re-elect the unit, the precision, and the terminator the sheet's standard already fixed. NAMED LOSS: a DWG consumer cannot drag a dimension and watch its figure update, and the seat that would close it is a model-space anchor column on `DraftDimension` beside the projected pair — a `Render/drafting` widening, not a writer change.
- [PAPER_SPACE_REFUSED]: N paper-space layouts are REFUSED and the set lays on ONE model space at the frame's own pitch. `CadDocument` publishes `ModelSpace` and `PaperSpace` as SINGULAR properties over its `BlockRecords`, and the verified writer surface publishes no per-sheet `Layout` mint or `Viewport` seating, so a three-sheet set cannot become three named layouts through anything this catalogue states. The pitch that stands in for them is the standard's own clear space — the sheet extent plus the frame's two facing margins — rather than the authored 20 mm gutter it replaces, and the prior claim that "a CAD document has one model space" is corrected: the document has one model space AND one paper space, and it is the second that this leg cannot fan.
- [ROTATED_TEXT_REFUSED]: rotated annotation text is REFUSED on both arms and stated here because the refusal is shared. `SheetEntity.TextRun` and `Glyph` carry no rotation and `ShapedTextSeam.Draw` takes none, so a vertical or oblique dimension label renders horizontal in the raster arm; ACadSharp compounds it because `MText.Rotation` is DERIVED and get-only, so the CAD arm could not express it even if the entity carried one. The seat is a rotation column on `TextRun` and `Glyph` beside a rotation parameter on the shaping seam, with the CAD arm setting `AlignmentPoint` rather than `Rotation` — a `Render/drafting` and typography widening whose consumers are every `DraftDimension` arm, the title-block layout, and `SheetCard.Entities`.

## [05]-[RESEARCH]

(none)

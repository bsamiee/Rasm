# [APPUI_CUSTOM_VISUALS]

Custom visuals are the package's Skia layout-algebra rail for every diagram and deck.gl-class geo layer LiveCharts structurally cannot supply: `CustomVisual` is the fourteen-row frozen layout catalog (sankey, treemap, waterfall, funnel, parallel-coordinates, radar, network, gantt, sunburst, hexbin, geo-arc, trip, extrusion, terrain) whose every row carries a pure layout fold from `CustomVisualData` to an `SKPath` run as its delegate column, materialized through the one offscreen draw capsule, emitted as an SVG vector twin on demand, and sealed as a per-cell render-hash twin; `ColorSpaceAxis` is the chart-side KEYED PROJECTION of the capture-owned `VisualCodec.ColorPolicy` rows — the one suite gamut/transfer vocabulary lives on `Render/capture.md#ENCODE_IDENTITY`, this axis derives and never diverges. The page owns the custom-visual union, its layout-fold and render-twin algebra, the synthesized live-region peer binding, and the four-row keyed projection the encode identity tags. The package spine is SkiaSharp path geometry behind the `DrawSource.Owned` capsule and the `VisualCodec` encode path; paints, label fonts, automation peers, and capture lanes arrive as settled vocabulary and are never re-minted here.

## [01]-[INDEX]

- [01]-[SKIA_KINDS]: Fourteen custom-visual cases; layout folds; render-hash twins.
- [02]-[COLOR_SPACE]: Four wide-gamut rows; working-space factory; encode-format tag.

## [02]-[SKIA_KINDS]

- Owner: `CustomVisual` `[SmartEnum<string>]` — the frozen layout-row catalog whose `Layout` fold is a `[UseDelegateFromConstructor]` column · `CustomVisualData` · `CustomVisuals` — the fold table
- Cases: Sankey · Treemap · Waterfall · Funnel · ParallelCoordinates · Radar · Network · Gantt · Sunburst · Hexbin · GeoArc · Trip · Extrusion · Terrain — the four flow-diagram kinds plus the five analytical-chart kinds (multi-axis parallel-coordinates, polar radar, force-laid network, time-tracked gantt, hierarchical sunburst) and the five deck.gl-class geo-layer kinds (hexagonal binning, great-circle arcs, time-ordered trips, pseudo-3D extruded columns, height-shaded terrain) projected through one equirectangular `Project`; every kind shares one generative structure — a wire key and a pure layout fold — so the family is row DATA under `DERIVED_LOGIC`, never fourteen enumerated case records re-spelling one payload
- Entry: `public IO<RenderReceipt> Materialize(VisualRuntime runtime, CustomVisualData data, SKImageInfo info, ColorSpaceAxis space)` — IO rail through the settled encode fold; `public Fin<string> VectorTwin(CustomVisualData data, SKImageInfo info)` — the same fold emitted as SVG path data for the drafting and export codecs; `public static TelemetryContributorPort TelemetryRow(string version)` — the one contribution surface for the rendered and layout-elapsed instruments
- Auto: each case carries one pure `Func<CustomVisualData, SKImageInfo, Fin<SKPath>>` layout fold resolved at declaration — the sankey fold cubic-bridges weighted ribbons, the treemap fold squarified-rect packs the node weights through the Bruls worst-aspect-ratio row algebra that grows a row while the worst rect aspect ratio improves and flips the layout-row orientation on the shorter box side toward unit aspect, the waterfall fold bridges signed delta columns, the funnel fold trapezoids the descending stage widths, the parallel-coordinates fold polylines each series across min-max-normalized vertical axes, the radar fold closes each series over normalized polar spokes, the network fold draws edges then vertex nodes from the pre-laid vertex positions, the gantt fold rounds-rects each span on its track over the shared time scale, the sunburst fold arcs each wedge over its depth ring from the parent-share sweep fold, the hexbin fold pointy-top-hexagons the spatial bins the `Bin` fold aggregates, the geo-arc fold quad-bezier great-circle-approximates each arc over the equirectangular projection, the trip fold polylines each time-ordered path, the extrusion fold builds a pseudo-3D column per weighted point, and the terrain fold height-shades a square sample grid — the geo folds share one `Project` equirectangular lon-lat-to-canvas projection whose Web-Mercator refinement is the geo `MapProjection` research row; `Materialize` marks the clock around the layout fold and folds the elapsed onto the layout-elapsed instrument through `runtime.Measure` distinctly from the encode-elapsed, composing the fold through `DrawSource.Owned.Materialize` so the projected `SKPath` rasters onto an owned `SKImage` and never a host lease; the render-twin derives its `CaptureRow` from the same `Key` and the resolved `(ThemeVariantRow, DensityRow)` cell exactly as `ChartSeriesSpec.Baseline` does, so the proof lane captures the same materialized kind through `CaptureRenderedFrame` and the `FrameHash` baseline derives from one row with no parallel fixture.
- Receipt: every materialize lands one `RenderReceipt` of kind custom-visual carrying the blob artifact key as its destination and the `ColorSpaceAxis` row key as its `ColorSpace` tag; `TelemetryRow` contributes the rendered count and the layout-elapsed duration inward through the AppHost `TelemetryContributorPort`, the layout-fold duration measured around `Layout` distinctly from the encode-elapsed the encode receipt carries, so a slow pack folds onto the layout-elapsed instrument and never blurs into encode cost.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new diagram or geo-layer kind is ONE catalog row referencing its fold — no `Key` or `Layout` dispatch arm exists to extend because both derive from the row; a fifteenth kind carries its render-hash baseline by construction of the same fold; a new layout input is one `CustomVisualData` field, never a parallel data record; zero new surface.
- Boundary:
  - `CustomVisual` mints zero Skia-surface, encode, placement, or peer owner — the layout fold composes through `DrawSource.Owned.Materialize` (the only Skia-surface owner) exactly as `PreviewRow.Render` does, `VisualCodec.Encode` is the only encode path, `DashboardTile.Custom` places a kind in a board, and the `custom-visual` `AnnouncementRow` synthesized row gives each kind its live-region peer through the one `ControlAutomationPeer` synthesized-peer construction.
  - The projected `SKPath` is using-scoped inside the fold and never outlives the materialize so a layout fault leaks no native handle.
  - Ribbon and trapezoid fills resolve from `TokenRow` paint keys as `SKColorF` token paints under the color-managed law and stage labels shape through the typography rail's `DrawShapedText` so glyphs raster through HarfBuzz, never a raw `DrawText` loop or a sRGB-quantized fill.
  - Gradient ribbons enter through `SKShader.CreateLinearGradient(SKPoint, SKPoint, SKColorF[], SKColorSpace, SKShaderTileMode)` so a wide-gamut ramp stays float end-to-end.
  - The layout folds are managed Skia geometry only and carry no native, bridge, or live-host probe and cross no TS wire — `CustomVisual`, `CustomVisualData`, `CustomVisuals`, and `ColorSpaceAxis` are host-local desktop-Skia owners with no browser or peer crossing, so the page authors no `TS_PROJECTION` cluster.
  - A custom-tile dashboard feed crosses only as the already-projected `EvidenceTimeline`/`RenderReceipt` evidence wire on Diagnostics/evidence#TS_PROJECTION and any remote numeric input arrives through the existing Compute Runtime/wire#PROTO_VOCABULARY `Solve` rpc, never a new AppUi wire shape — a custom-visual wire contract is the deleted form.
  - Each materialize folds one observation into the rendered count and the measured layout-fold duration into the layout-elapsed instrument through the one `AppUiTelemetry.Contribute` spine, so a custom-tile render contributes through `TelemetryContributorPort` and a layout-local meter is the deleted form.
  - Boolean path algebra rides `SKPath.Op` — the extrusion column merges its shaft and sheared face through `SKPathOp.Union` into one clean silhouette — and `VectorTwin` emits the fold as `ToSvgPathData` text so a diagram's geometry reaches the drafting and export vector codecs without a raster hop; a hand-rolled winding workaround or a second vector-emit path is the deleted form.
  - A fork of `ChartSeriesSpec` for these kinds, a hand-rolled diagram control, and a second Skia-surface owner are the deleted patterns.

```csharp signature
public sealed record CustomVisualData(
    string Key,
    Seq<(int From, int To, double Weight)> Flows,
    Seq<(string Label, double Value)> Nodes,
    Seq<(string Label, double Delta, bool Total)> Steps,
    Seq<(string Series, Seq<double> Axes)> Series,
    Seq<(int From, int To, double Weight)> Edges,
    Seq<(double X, double Y)> Vertices,
    Seq<(string Label, double Start, double End, int Track)> Spans,
    Seq<(string Label, double Value, int Depth, int Parent)> Wedges,
    Seq<(double Lon, double Lat, double Weight)> Points,
    Seq<((double Lon, double Lat) From, (double Lon, double Lat) To, double Weight)> Arcs,
    Seq<(Seq<(double Lon, double Lat, Instant At)> Path, double Weight)> Trips,
    string PaintFamily,
    string LabelRole);

// DERIVED_LOGIC collapse: every kind shares one generative structure — a wire key and a pure layout fold
// — so the family is ONE frozen [SmartEnum<string>] row catalog with the fold as a delegate column; the
// fourteen enumerated case records re-spelling one identical payload are the deleted form, and a new kind
// is one row referencing its fold, never a case plus two dispatch arms.
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CustomVisual {
    public static readonly CustomVisual Sankey = new("sankey", CustomVisuals.Sankey);
    public static readonly CustomVisual Treemap = new("treemap", CustomVisuals.Treemap);
    public static readonly CustomVisual Waterfall = new("waterfall", CustomVisuals.Waterfall);
    public static readonly CustomVisual Funnel = new("funnel", CustomVisuals.Funnel);
    public static readonly CustomVisual ParallelCoordinates = new("parallel-coordinates", CustomVisuals.ParallelCoordinates);
    public static readonly CustomVisual Radar = new("radar", CustomVisuals.Radar);
    public static readonly CustomVisual Network = new("network", CustomVisuals.Network);
    public static readonly CustomVisual Gantt = new("gantt", CustomVisuals.Gantt);
    public static readonly CustomVisual Sunburst = new("sunburst", CustomVisuals.Sunburst);
    public static readonly CustomVisual Hexbin = new("hexbin", CustomVisuals.Hexbin);
    public static readonly CustomVisual GeoArc = new("geo-arc", CustomVisuals.GeoArc);
    public static readonly CustomVisual Trip = new("trip", CustomVisuals.Trip);
    public static readonly CustomVisual Extrusion = new("extrusion", CustomVisuals.Extrusion);
    public static readonly CustomVisual Terrain = new("terrain", CustomVisuals.Terrain);

    [UseDelegateFromConstructor]
    public partial Fin<SKPath> Layout(CustomVisualData data, SKImageInfo info);

    // The vector-interchange twin: the same layout fold emitted as SVG path data (`SKPath.ToSvgPathData`)
    // so a diagram's geometry feeds the drafting and export codecs without a raster hop.
    public Fin<string> VectorTwin(CustomVisualData data, SKImageInfo info) =>
        Layout(data, info).Map(path => {
            using SKPath scoped = path;
            return scoped.ToSvgPathData();
        });

    public IO<RenderReceipt> Materialize(VisualRuntime runtime, CustomVisualData data, SKImageInfo info, ColorSpaceAxis space) =>
        from mark in IO.lift(runtime.Clocks.Mark)
        from image in IO.lift(() => new DrawSource.Owned(info.WithColorSpace(space.Working()))
            .Materialize(canvas => Layout(data, info).Bind(path => {
                using SKPath scoped = path;
                using SKPaint paint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
                canvas.DrawPath(scoped, paint);
                return FinSucc(unit);
            })).ThrowIfFail())
        from layout in IO.lift(() => runtime.Clocks.Elapsed(mark))
        from _ in runtime.Measure(CustomVisuals.LayoutInstrument, Key, layout)
        from receipt in VisualCodec.Encode(runtime, image, space.Encode, CustomVisuals.Kind, $"custom-visuals/{Key}@{space.Key}.png")
            .Map(sealed_ => (fun(image.Dispose)(), sealed_).Item2)
        select receipt;

    public CaptureRow RenderTwin((ThemeVariantRow Variant, DensityRow Density) cell, double scale,
        Func<CustomVisual, (ThemeVariantRow, DensityRow), Func<double, Func<IO<Unit>>, IO<SKImage>>> grab) =>
        new($"{Key}@{cell.Variant.Key}-{cell.Density.Key}", static host => host is SurfaceHost.Headless, scale, 1, grab(this, cell));
}

public static class CustomVisuals {
    public const string Kind = "custom-visual";
    public const string RenderedInstrument = "rasm.appui.customvisual.rendered";
    public const string LayoutInstrument = "rasm.appui.customvisual.layout-elapsed";

    public static TelemetryContributorPort TelemetryRow(string version) =>
        AppUiTelemetry.Contribute(version, RenderedInstrument, LayoutInstrument);

    // --- [OPERATIONS] — the fourteen layout folds: the row catalog's delegate-column values.

    internal static Fin<SKPath> Sankey(CustomVisualData data, SKImageInfo info) =>
        data.Flows.Fold(Fin.Succ(new SKPath()), (rail, flow) => rail.Map(path => {
            float lane = info.Height / (float)(data.Nodes.Count + 1);
            float x0 = 0f, x1 = info.Width;
            float y0 = lane * (flow.From + 1), y1 = lane * (flow.To + 1);
            float thickness = (float)(flow.Weight) * lane * 0.5f;
            path.MoveTo(x0, y0 - thickness);
            path.CubicTo(info.Width * 0.5f, y0 - thickness, info.Width * 0.5f, y1 - thickness, x1, y1 - thickness);
            path.LineTo(x1, y1 + thickness);
            path.CubicTo(info.Width * 0.5f, y1 + thickness, info.Width * 0.5f, y0 + thickness, x0, y0 + thickness);
            path.Close();
            return path;
        }));

    internal static Fin<SKPath> Treemap(CustomVisualData data, SKImageInfo info) =>
        Squarify(data.Nodes, new SKRect(0f, 0f, info.Width, info.Height)).Map(rects =>
            rects.Fold(new SKPath(), static (path, rect) => { path.AddRect(rect, SKPathDirection.Clockwise); return path; }));

    internal static Fin<SKPath> Waterfall(CustomVisualData data, SKImageInfo info) =>
        Fin.Succ(data.Steps.Fold(
                (Path: new SKPath(), Cursor: 0d, Index: 0),
                (state, step) => {
                    float width = info.Width / (float)data.Steps.Count;
                    float x = state.Index * width;
                    float baseTop = (float)(info.Height - (state.Cursor / data.Steps.Count * info.Height));
                    float top = step.Total ? 0f : baseTop;
                    float bottom = step.Total ? info.Height : (float)(info.Height - ((state.Cursor + step.Delta) / data.Steps.Count * info.Height));
                    state.Path.AddRect(new SKRect(x, Math.Min(top, bottom), x + width * 0.8f, Math.Max(top, bottom)), SKPathDirection.Clockwise);
                    return (state.Path, Cursor: step.Total ? 0d : state.Cursor + step.Delta, Index: state.Index + 1);
                })
            .Path);

    internal static Fin<SKPath> Funnel(CustomVisualData data, SKImageInfo info) =>
        Fin.Succ(data.Nodes.Fold(
                (Path: new SKPath(), Top: 0f, Index: 0),
                (state, node) => {
                    float bandHeight = info.Height / (float)data.Nodes.Count;
                    float bottom = state.Top + bandHeight;
                    float topWidth = (float)(node.Value) * info.Width;
                    float nextWidth = state.Index + 1 < data.Nodes.Count ? (float)(data.Nodes[state.Index + 1].Value) * info.Width : topWidth;
                    float center = info.Width * 0.5f;
                    state.Path.MoveTo(center - topWidth * 0.5f, state.Top);
                    state.Path.LineTo(center + topWidth * 0.5f, state.Top);
                    state.Path.LineTo(center + nextWidth * 0.5f, bottom);
                    state.Path.LineTo(center - nextWidth * 0.5f, bottom);
                    state.Path.Close();
                    return (state.Path, Top: bottom, Index: state.Index + 1);
                })
            .Path);

    internal static Fin<SKPath> ParallelCoordinates(CustomVisualData data, SKImageInfo info) =>
        data.Series.IsEmpty || data.Series[0].Axes.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("parcoords: no series axes"))
            : NormalizeAxes(data.Series) switch {
                var normalized => Fin.Succ(data.Series.Map((row, _) => row).Fold(new SKPath(), (path, row) => {
                    int axisCount = row.Axes.Count;
                    float gap = axisCount > 1 ? info.Width / (float)(axisCount - 1) : info.Width;
                    row.Axes.Iter((value, axis) => {
                        float x = gap * axis;
                        float y = (float)(info.Height * (1d - normalized(axis, value)));
                        if (axis == 0) { path.MoveTo(x, y); } else { path.LineTo(x, y); }
                    });
                    return path;
                })),
            };

    internal static Fin<SKPath> Radar(CustomVisualData data, SKImageInfo info) =>
        data.Series.IsEmpty || data.Series[0].Axes.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("radar: no series axes"))
            : NormalizeAxes(data.Series) switch {
                var normalized => Fin.Succ(data.Series.Fold(new SKPath(), (path, row) => {
                    int spokes = row.Axes.Count;
                    float cx = info.Width * 0.5f, cy = info.Height * 0.5f, radius = Math.Min(cx, cy);
                    row.Axes.Iter((value, axis) => {
                        double angle = (2d * Math.PI * axis / spokes) - (Math.PI * 0.5d);
                        float r = (float)(radius * normalized(axis, value));
                        float x = cx + (r * (float)Math.Cos(angle));
                        float y = cy + (r * (float)Math.Sin(angle));
                        if (axis == 0) { path.MoveTo(x, y); } else { path.LineTo(x, y); }
                    });
                    path.Close();
                    return path;
                })),
            };

    internal static Fin<SKPath> Network(CustomVisualData data, SKImageInfo info) =>
        data.Vertices.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("network: no vertices"))
            : Fin.Succ(data.Edges.Fold(new SKPath(), (path, edge) => {
                if (edge.From >= data.Vertices.Count || edge.To >= data.Vertices.Count) { return path; }
                var (fx, fy) = data.Vertices[edge.From];
                var (tx, ty) = data.Vertices[edge.To];
                path.MoveTo((float)(fx * info.Width), (float)(fy * info.Height));
                path.LineTo((float)(tx * info.Width), (float)(ty * info.Height));
                return path;
            }) switch {
                var edges => data.Vertices.Fold(edges, (path, vertex) => {
                    path.AddCircle((float)(vertex.X * info.Width), (float)(vertex.Y * info.Height), 4f, SKPathDirection.Clockwise);
                    return path;
                }),
            });

    internal static Fin<SKPath> Gantt(CustomVisualData data, SKImageInfo info) =>
        data.Spans.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("gantt: no spans"))
            : (Lo: data.Spans.Min(static s => s.Start), Hi: data.Spans.Max(static s => s.End), Tracks: data.Spans.Max(static s => s.Track) + 1) switch {
                var bounds when bounds.Hi > bounds.Lo => Fin.Succ(data.Spans.Fold(new SKPath(), (path, span) => {
                    float scale = info.Width / (float)(bounds.Hi - bounds.Lo);
                    float x0 = (float)((span.Start - bounds.Lo) * scale);
                    float x1 = (float)((span.End - bounds.Lo) * scale);
                    float trackHeight = info.Height / (float)bounds.Tracks;
                    float y0 = span.Track * trackHeight + (trackHeight * 0.15f);
                    path.AddRoundRect(new SKRoundRect(new SKRect(x0, y0, x1, y0 + (trackHeight * 0.7f)), 3f, 3f));
                    return path;
                })),
                _ => Fin.Fail<SKPath>(new ChartFault.VisualDegenerate("gantt: zero time span")),
            };

    internal static Fin<SKPath> Sunburst(CustomVisualData data, SKImageInfo info) =>
        data.Wedges.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("sunburst: no wedges"))
            : Fin.Succ(SunburstArcs(data.Wedges).Fold(new SKPath(), (path, arc) => {
                float cx = info.Width * 0.5f, cy = info.Height * 0.5f;
                float ringWidth = Math.Min(cx, cy) / (float)(data.Wedges.Max(static w => w.Depth) + 1);
                float inner = arc.Depth * ringWidth, outer = inner + ringWidth;
                using SKPath wedge = new();
                wedge.AddArc(new SKRect(cx - outer, cy - outer, cx + outer, cy + outer), (float)arc.StartDeg, (float)(arc.SweepDeg));
                wedge.ArcTo(new SKRect(cx - inner, cy - inner, cx + inner, cy + inner), (float)(arc.StartDeg + arc.SweepDeg), (float)(-arc.SweepDeg), false);
                wedge.Close();
                path.AddPath(wedge);
                return path;
            }));

    internal static Fin<SKPath> Hexbin(CustomVisualData data, SKImageInfo info) =>
        data.Points.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("hexbin: no points"))
            : Fin.Succ(Bin(data.Points, info, radiusPx: 18f).Fold(new SKPath(), static (path, cell) => {
                path.AddPath(Hexagon(cell.Cx, cell.Cy, cell.Radius));
                return path;
            }));

    internal static Fin<SKPath> GeoArc(CustomVisualData data, SKImageInfo info) =>
        data.Arcs.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("geoarc: no arcs"))
            : Fin.Succ(data.Arcs.Fold(new SKPath(), (path, arc) => {
                var (sx, sy) = Project(arc.From.Lon, arc.From.Lat, info);
                var (ex, ey) = Project(arc.To.Lon, arc.To.Lat, info);
                float midX = (sx + ex) * 0.5f, midY = Math.Min(sy, ey) - (Math.Abs(ex - sx) * 0.3f);
                path.MoveTo(sx, sy);
                path.QuadTo(midX, midY, ex, ey);
                return path;
            }));

    internal static Fin<SKPath> Trip(CustomVisualData data, SKImageInfo info) =>
        data.Trips.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("trip: no trips"))
            : Fin.Succ(data.Trips.Fold(new SKPath(), (path, trip) => {
                trip.Path.Iter((node, index) => {
                    var (x, y) = Project(node.Lon, node.Lat, info);
                    if (index == 0) { path.MoveTo(x, y); } else { path.LineTo(x, y); }
                });
                return path;
            }));

    // The column shaft and its sheared top face merge through SKPath.Op(Union) into ONE clean silhouette
    // per column, so overlapping subpaths never double-fill or cancel under the fill winding rule.
    internal static Fin<SKPath> Extrusion(CustomVisualData data, SKImageInfo info) =>
        data.Points.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("extrusion: no columns"))
            : (Max: data.Points.Max(static p => p.Weight)) switch {
                var bounds when bounds.Max > 0d => Fin.Succ(data.Points.Fold(new SKPath(), (path, column) => {
                    var (x, y) = Project(column.Lon, column.Lat, info);
                    float height = (float)(column.Weight / bounds.Max * info.Height * 0.25d);
                    float half = 6f;
                    using SKPath face = new();
                    face.MoveTo(x - half, y - height);
                    face.LineTo(x + half, y - height - 4f);
                    face.LineTo(x + half, y - 4f);
                    face.LineTo(x - half, y);
                    face.Close();
                    using SKPath shaft = new();
                    shaft.AddRect(new SKRect(x - half, y - height, x + half, y));
                    using SKPath column3d = face.Op(shaft, SKPathOp.Union);
                    path.AddPath(column3d);
                    return path;
                })),
                _ => Fin.Fail<SKPath>(new ChartFault.VisualDegenerate("extrusion: zero column weight")),
            };

    // Exact-square admission: a sample count that is not a perfect square >= 4 rejects — the floor-square
    // acceptance that silently rendered a truncated prefix is the deleted form.
    internal static Fin<SKPath> Terrain(CustomVisualData data, SKImageInfo info) =>
        data.Points.IsEmpty
            ? Fin.Fail<SKPath>(new ChartFault.VisualEmpty("terrain: no samples"))
            : (Side: (int)Math.Round(Math.Sqrt(data.Points.Count))) switch {
                var grid when grid.Side >= 2 && grid.Side * grid.Side == data.Points.Count => Fin.Succ(
                    Enumerable.Range(0, grid.Side - 1).Aggregate(new SKPath(), (path, row) => {
                        float cell = info.Width / (float)(grid.Side - 1);
                        Enumerable.Range(0, grid.Side - 1).Iter(col => {
                            int origin = row * grid.Side + col;
                            float z = (float)(data.Points[origin].Weight * info.Height * 0.2d);
                            path.AddRect(new SKRect(col * cell, (row * cell) - z, (col + 1) * cell, ((row + 1) * cell) - z));
                        });
                        return path;
                    })),
                _ => Fin.Fail<SKPath>(new ChartFault.VisualDegenerate("terrain: sample count is not a square grid")),
            };

    static Func<int, double, double> NormalizeAxes(Seq<(string Series, Seq<double> Axes)> series) {
        int axisCount = series[0].Axes.Count;
        var bounds = Enumerable.Range(0, axisCount)
            .Select(axis => series.Map(row => row.Axes[axis]) switch { var column => (Lo: column.Min(), Hi: column.Max()) })
            .ToArray();
        return (axis, value) => bounds[axis] is var b && b.Hi > b.Lo ? (value - b.Lo) / (b.Hi - b.Lo) : 0.5d;
    }

    static Seq<(double StartDeg, double SweepDeg, int Depth)> SunburstArcs(Seq<(string Label, double Value, int Depth, int Parent)> wedges) {
        double rootTotal = wedges.Filter(static w => w.Depth == 0).Sum(static w => w.Value);
        return rootTotal <= 0d
            ? Seq<(double, double, int)>()
            : wedges.Fold(
                (Arcs: Seq<(double StartDeg, double SweepDeg, int Depth)>(), Cursor: 0d),
                (state, wedge) => {
                    double sweep = wedge.Value / rootTotal * 360d;
                    return (state.Arcs.Add((state.Cursor, sweep, wedge.Depth)), state.Cursor + sweep);
                }).Arcs;
    }

    static (float X, float Y) Project(double lon, double lat, SKImageInfo info) =>
        ((float)((lon + 180d) / 360d * info.Width), (float)((90d - lat) / 180d * info.Height));

    static Seq<(float Cx, float Cy, float Radius, int Count)> Bin(Seq<(double Lon, double Lat, double Weight)> points, SKImageInfo info, float radiusPx) {
        float dx = radiusPx * 1.5f, dy = radiusPx * 1.732f;
        return toSeq(points
            .Map(p => Project(p.Lon, p.Lat, info))
            .GroupBy(p => ((int)Math.Round(p.X / dx), (int)Math.Round(p.Y / dy)))
            .Select(group => {
                var centroid = group.Aggregate((X: 0f, Y: 0f, N: 0), static (acc, p) => (acc.X + p.X, acc.Y + p.Y, acc.N + 1));
                return (Cx: centroid.X / centroid.N, Cy: centroid.Y / centroid.N, Radius: radiusPx, Count: centroid.N);
            }));
    }

    static SKPath Hexagon(float cx, float cy, float radius) {
        var path = new SKPath();
        Enumerable.Range(0, 6).Iter(corner => {
            double angle = Math.PI / 3d * corner;
            float x = cx + (radius * (float)Math.Cos(angle));
            float y = cy + (radius * (float)Math.Sin(angle));
            if (corner == 0) { path.MoveTo(x, y); } else { path.LineTo(x, y); }
        });
        path.Close();
        return path;
    }

    static Fin<Seq<SKRect>> Squarify(Seq<(string Label, double Value)> nodes, SKRect bounds) {
        double total = nodes.Sum(static n => n.Value);
        if (total <= 0d) return Fin.Fail<Seq<SKRect>>(new ChartFault.VisualEmpty("treemap: node weights sum to zero"));
        double area = bounds.Width * bounds.Height;
        Seq<double> scaled = nodes.OrderByDescending(static n => n.Value).Map(n => n.Value / total * area).ToSeq();
        return Fin.Succ(Pack(scaled, Seq<double>(), bounds, Seq<SKRect>()));
    }

    static double Worst(Seq<double> row, double side, double withCandidate) {
        Seq<double> trial = withCandidate <= 0d ? row : row.Add(withCandidate);
        if (trial.IsEmpty) return double.PositiveInfinity;
        double sum = trial.Sum(), max = trial.Max(), min = trial.Min(), s2 = sum * sum, w2 = side * side;
        return Math.Max(w2 * max / s2, s2 / (w2 * min));
    }

    static Seq<SKRect> Pack(Seq<double> remaining, Seq<double> row, SKRect box, Seq<SKRect> placed) {
        float side = Math.Min(box.Width, box.Height);
        if (remaining.IsEmpty)
            return row.IsEmpty ? placed : placed + LayoutRow(row, box, side).Rects;
        double head = remaining.Head;
        if (Worst(row, side, 0d) >= Worst(row, side, head) || row.IsEmpty)
            return Pack(remaining.Tail, row.Add(head), box, placed);
        var laid = LayoutRow(row, box, side);
        return Pack(remaining, Seq<double>(), laid.Rest, placed + laid.Rects);
    }

    static (Seq<SKRect> Rects, SKRect Rest) LayoutRow(Seq<double> row, SKRect box, float side) {
        double rowSum = row.Sum();
        float thickness = (float)(rowSum / side);
        bool vertical = box.Width >= box.Height;
        var built = row.Fold(
            (Rects: Seq<SKRect>(), Offset: vertical ? box.Top : box.Left),
            (state, cell) => {
                float extent = (float)(cell / rowSum * side);
                SKRect rect = vertical
                    ? new SKRect(box.Left, state.Offset, box.Left + thickness, state.Offset + extent)
                    : new SKRect(state.Offset, box.Top, state.Offset + extent, box.Top + thickness);
                return (state.Rects.Add(rect), state.Offset + extent);
            });
        SKRect rest = vertical
            ? new SKRect(box.Left + thickness, box.Top, box.Right, box.Bottom)
            : new SKRect(box.Left, box.Top + thickness, box.Right, box.Bottom);
        return (built.Rects, rest);
    }
}
```

```mermaid
flowchart LR
    CustomVisualData --> CustomVisual
    CustomVisual -->|Layout| SKPath
    SKPath --> DrawSource
    DrawSource -->|Materialize| SKImage
    SKImage -->|Encode| VisualCodec
    CustomVisual -->|RenderTwin| CaptureRow
    VisualCodec --> RenderReceipt
```

| [INDEX] | [KIND]               | [DATA_FIELD]   | [LAYOUT_PRIMITIVE]                         |
| :-----: | :------------------- | :------------- | :----------------------------------------- |
|  [01]   | sankey               | Flows          | cubic ribbon `SKPath.CubicTo`              |
|  [02]   | treemap              | Nodes          | squarified `SKPath.AddRect`                |
|  [03]   | waterfall            | Steps          | bridged column `SKPath.AddRect`            |
|  [04]   | funnel               | Nodes          | trapezoid `SKPath.LineTo`                  |
|  [05]   | parallel-coordinates | Series         | normalized polyline `SKPath.LineTo`        |
|  [06]   | radar                | Series         | polar polygon `SKPath.LineTo`+`Close`      |
|  [07]   | network              | Edges,Vertices | edge line + node `SKPath.AddCircle`        |
|  [08]   | gantt                | Spans          | track bar `SKPath.AddRoundRect`            |
|  [09]   | sunburst             | Wedges         | ring arc `SKPath.AddArc`+`ArcTo`           |
|  [10]   | hexbin               | Points         | binned hexagon `SKPath.LineTo`+`Close`     |
|  [11]   | geo-arc              | Arcs           | great-circle `SKPath.QuadTo`               |
|  [12]   | trip                 | Trips          | time-path polyline `SKPath.LineTo`         |
|  [13]   | extrusion            | Points         | pseudo-3D column `SKPath.LineTo`+`AddRect` |
|  [14]   | terrain              | Points         | grid height-shade `SKPath.AddRect`         |

## [03]-[COLOR_SPACE]

- Owner: `ColorSpaceAxis` SmartEnum — a KEYED PROJECTION of the capture-owned `VisualCodec.ColorPolicy` rows (`[V10]`: `ColorPolicy` is the ONE gamut/transfer family; this axis derives, never diverges) · `ComparerAccessors.StringOrdinal` accessor
- Cases: srgb · display-p3 · rec2020 · scrgb-float — the baseline plus three wide-gamut rows
- Entry: `public SKColorSpace Working()` — the working-space factory per row; the `Encode` member projects the row onto the codec encode policy
- Auto: each row wraps exactly ONE `VisualCodec.ColorPolicy` row and derives every column from it — `Working()` reads the policy's working-space factory, `Surface` its pixel format, `Encode` its matching `EncodeRow` — so the axis cannot diverge from the capture family by construction; a materialize tags its `RenderReceipt.ColorSpace` with the policy key, so a cross-host byte swap is attributable to the exact gamut, never silent.
- Packages: SkiaSharp, SkiaSharp.NativeAssets.macOS, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new gamut lands as one `ColorPolicy` row on the capture codec FIRST; this axis gains a one-line keyed projection of it only when a chart consumes it; zero new surface.
- Boundary: `VisualCodec.ColorPolicy` (`Render/capture.md#ENCODE_IDENTITY`) is the single suite-wide gamut/transfer vocabulary and `ColorSpaceAxis` is its chart-side keyed projection — a parallel enum with divergent membership, an axis-local working-space factory, or a per-encode color struct is the deleted form; the working space converts once at projection through `SKImageInfo.WithColorSpace` and `SKColorSpace.Equal` is the only identity test the reproject runs fail-closed against an already-matching space; the ICC-primary path uses `SKColorSpaceXyz.DisplayP3` and `SKColorSpaceXyz.Rec2020` with `SKColorSpaceTransferFn.Srgb` for the display-referred rows and `SKColorSpaceTransferFn.Linear` with `SKColorSpaceXyz.Srgb` for the scene-referred float row, so the byte `SKColor` path that assumes sRGB and quantizes before conversion is the deleted form and a wide-gamut custom visual hashes its float or ICC-tagged pixels, never a quantized sRGB shadow; the gamut row key crosses no TS wire on its own — it tags `RenderReceipt.ColorSpace` which crosses host-local only as the existing evidence wire on Diagnostics/evidence#TS_PROJECTION, so `ColorSpaceAxis` authors no `TS_PROJECTION` cluster.

```csharp signature

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColorSpaceAxis {
    // Every row is a keyed projection of ONE capture-owned ColorPolicy row — zero axis-local color science.
    public static readonly ColorSpaceAxis Srgb = new("srgb", VisualCodec.Png);
    public static readonly ColorSpaceAxis DisplayP3 = new("display-p3", VisualCodec.PngP3);
    public static readonly ColorSpaceAxis Rec2020 = new("rec2020", VisualCodec.PngRec2020);
    public static readonly ColorSpaceAxis ScrgbFloat = new("scrgb-float", VisualCodec.PngScrgb);

    public VisualCodec.EncodeRow Encode { get; }

    public VisualCodec.ColorPolicy Policy => Encode.Color;

    public SKColorType Surface => Policy.Surface;

    public SKColorSpace Working() => Policy.Working();
}
```

| [INDEX] | [ROW]       | [TRANSFER]                      | [PRIMARIES]                 | [SURFACE]  |
| :-----: | :---------- | :------------------------------ | :-------------------------- | :--------- |
|  [01]   | srgb        | `SKColorSpaceTransferFn.Srgb`   | `SKColorSpaceXyz.Srgb`      | `Rgba8888` |
|  [02]   | display-p3  | `SKColorSpaceTransferFn.Srgb`   | `SKColorSpaceXyz.DisplayP3` | `Rgba8888` |
|  [03]   | rec2020     | `SKColorSpaceTransferFn.Srgb`   | `SKColorSpaceXyz.Rec2020`   | `Rgba8888` |
|  [04]   | scrgb-float | `SKColorSpaceTransferFn.Linear` | `SKColorSpaceXyz.Srgb`      | `RgbaF16`  |

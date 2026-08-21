# [APPUI_VFX_SHADER]

Rasm.AppUi shader effects are the effects plane's procedural-source owner: one closed roster of SkSL programs compiled once and re-bound per frame, one uniform frame carrying every value that moves, one path-effect family for stroked geometry patterns beside the closed pattern roster that seeds it, and one recorded-tile cache admitting through the folder's single budgeted cache owner. The plane exists for exactly the terms a frozen native cannot carry — a glow whose radius follows a focus transition, noise seeded per surface, a wash gradient whose angle tracks a layout, a hatch whose phase advances — and both rosters are CLOSED because an SkSL program and an estate pattern are source the estate compiles, not data a consumer supplies.

`SKRuntimeEffect`, `SKRuntimeShaderBuilder`, `SKPathEffect`, `SKPicture`, and `SKPictureRecorder` are the composed natives; `EffectTokens`, `FxEffect`, and `PaintCatalog` arrive settled from `Render/capture#DRAW_CAPSULE` and this plane produces values that bind into the same one paint. Compilation here is the 2D chrome partition — `Render/shading#SHADER_ASSET` owns the per-`GpuBackend` appearance-shader cache with its plane residency and VRAM budget, while a chrome program carries no backend variant, no resident plane, and a CPU-side op-list budget instead, so the two are disjoint INSTANCES of one owner rather than two mechanisms. Programs resolve by ROW and uniform frame for the `material#FILTER_ROWS` refraction field and the `material#MATERIAL_EXECUTION` grain draw, and `EffectFault` carries each failure through a direct generated union case.

## [01]-[INDEX]

- [02]-[EFFECT_PROGRAM]: The closed SkSL roster over one slot vocabulary, the compile gate on the typed rail, and the retained builder cell.
- [03]-[UNIFORM_FRAME]: Per-frame uniform and child binding under one case-to-shape correspondence with accumulating coverage.
- [04]-[PATH_ROWS]: Dash, trim, stamped, and tiled geometry patterns beside the estate pattern roster that seeds them.
- [05]-[TILE_CACHE]: Recorded-tile admission over the folder's `BudgetedCache` under a generation floor.

## [02]-[EFFECT_PROGRAM]

- Owner: `EffectRow` `[SmartEnum<string>]` the closed program roster carrying its SkSL source beside one slot roster; `SlotRow` with `SlotKind` the declared-slot vocabulary; `UniformShape` the block-shape axis; `EffectProgram` the compiled cell; `EffectCatalog` the process roster; `EffectFault` the direct generated `[Union]` with one `[FaultCase]` leaf per effect failure.
- Cases: `EffectRow` = glow | grain | refract | wash | sheen; `SlotKind` = Uniform | Content | Seed; `UniformShape` = float | float2 | float4; `SlotDefect` = diverged | undeclared | unbound | misplaced | refused; `EffectFault` = SkslRejected | UniformUndeclared | ChildUndeclared | ProgramMissing | PatternDegenerate | TileOversize | BudgetExhausted.
- Law: compilation takes the REFUSAL channel, never the throwing one — `SKRuntimeEffect.CreateShader(sksl, out string errors)` returns a null effect with the diagnostic in `errors` on a rejected program, while `BuildShader(sksl)` validates the same pair and throws; a compile that throws inside a static roster initializer takes the whole vocabulary down at type load, so the typed gate is the only admissible arm.
- Law: `EffectCatalog.Source` is the projection `material#FILTER_ROWS` and `material#MATERIAL_EXECUTION` bind as their `Sources` column — the catalog is the composition-held owner and the delegate is its bound member, never a caller-assembled arrow.
- Entry: `EffectCatalog.Of()` — the one process mint; `Source(EffectRow row, UniformFrame frame)` — the one by-ROW projection every consumer names; `EffectProgram.Compile(row)` — the compile gate the catalog folds.
- Auto: the roster compiles once per process and the cell retains its `SKRuntimeShaderBuilder` beside every seeded child, so a frame re-binds uniforms and mints a shader without touching the SkSL compiler and without re-minting a noise source per draw; declared-name conformance folds at compile against `SKRuntimeEffect.Uniforms` and `Children` as one accumulating admission, so a source edit that renamed a uniform AND dropped a child reports both; every refusal past the compile releases what it refused through `Custody`, so neither a diverged row nor a rejected sibling leaves a live native behind.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (`FaultBand`, `[FaultCase]`, `Fault`, `Custody`, `Band`, `Op`)
- Growth: a new procedural term is one `EffectRow` row carrying its source and its slot roster; a new declared-slot class is one `SlotKind` case; zero new surface.
- Boundary: `SKRuntimeEffectBuilder.Dispose` disposes the EFFECT it wraps, so a cached effect handed to a per-frame builder is freed the moment that frame's builder falls out of scope and every later frame binds a dead handle — the cell therefore retains the BUILDER and constructs no second one, and `SKRuntimeEffect.BuildShader` is unreachable here because its throwing validation is the arm this rail replaced. A program is SOURCE the estate ships, so the roster is closed and no consumer supplies SkSL.
- Boundary: slots split by who owns the pixels, and the split is a CASE rather than three parallel columns. A `Seed` slot is the estate's own source and its native mints once at compile — the film field is `SKShader.CreatePerlinNoiseFractalNoise`, a shipped Skia generator rather than a hand-rolled SkSL lattice, and the grain and the glass cite ONE mint so the two sample the same field at the pixel — while a `Content` slot is the caller's already-painted draw entering through `SKRuntimeEffectChild`. That split is what makes the coverage gate total: a seeded slot a frame never binds would otherwise refuse every draw of a program whose own source it is.
- Boundary: both `SKRuntimeEffectUniforms.Add` and `SKRuntimeEffectChildren.Add` THROW on a name the compiled program never declared, and the uniform overload validates the value's own data type against that declaration and throws on a mismatch — so a `Uniform` slot declares its block SHAPE and the frame resolves both axes against the slot before it writes, which is what leaves the throwing arms unreachable rather than trapped. A `Uniform` slot also carries the numeric `Band` its value must land inside, so the `max(x, 1e-3)` floors inside the shader sources are a GPU-side belt over a refusal the C# edge already owns rather than the only guard a zero meets.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------

// WHY a slot roster refused, beside the two cases naming WHICH HALF of the roster refused. A consumer partitions
// a source that drifted from a frame that skipped a slot without reading a message, and the requirement column is
// what each rendered message states rather than five hand-interpolated sentences.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SlotDefect {
    public static readonly SlotDefect Diverged = new("diverged", "declared by the row and by the compiled source alike");
    public static readonly SlotDefect Undeclared = new("undeclared", "a slot this row declares");
    public static readonly SlotDefect Unbound = new("unbound", "written on every bind");
    public static readonly SlotDefect Misplaced = new("misplaced", "a value of the slot's declared block shape");
    public static readonly SlotDefect Refused = new("refused", "a value inside the slot's declared band");
    public string Requirement { get; }
}



// One band spans three owners on this page — the program cluster, the pattern family, and the tile cache — which
// is legal because a band is a PAGE's neighbourhood and the offsets stay unique inside it; `generated identity admission`
// proves that at the family's first construction.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EffectFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Effect;
    private EffectFault() { }

    [FaultCase(0)]
    public sealed partial record SkslRejected(string Program, string Diagnostic) : EffectFault() {
        public override string Message => $"{Program}: the runtime effect compiler rejected the source — {Diagnostic}";
    }
    // Names ride as a SEQ: a joined string cannot be filtered, counted, or diffed by the consumer that reads it,
    // and the divergence arm reports both directions at once.
    [FaultCase(1)]
    public sealed partial record UniformUndeclared(string Program, SlotDefect Defect, Seq<string> Names) : EffectFault() {
        public override string Message => $"{Program}: uniform {Names} must be {Defect.Requirement}";
    }
    [FaultCase(2)]
    public sealed partial record ChildUndeclared(string Program, SlotDefect Defect, Seq<string> Names) : EffectFault() {
        public override string Message => $"{Program}: child {Names} must be {Defect.Requirement}";
    }
    [FaultCase(3)]
    public sealed partial record ProgramMissing(string Program) : EffectFault() {
        public override string Message => $"{Program}: the catalog holds no compiled program for this row";
    }
    [FaultCase(4)]
    public sealed partial record PatternDegenerate(string Detail) : EffectFault() {
        public override string Message => Detail;
    }
    [FaultCase(5)]
    public sealed partial record TileOversize(PictureTileKey Key, long Generation, long Bytes, long Ceiling) : EffectFault() {
        public override string Message => $"{Key.Key}@{Generation}: {Bytes} recorded bytes over a {Ceiling} ceiling";
    }
    [FaultCase(6)]
    public sealed partial record BudgetExhausted(long Ceiling) : EffectFault() {
        public override string Message => $"a tile ceiling of {Ceiling} bytes admits no record at all";
    }
}
```

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// The uniform-block shapes a chrome program declares. `SKRuntimeEffectUniforms.Add` validates the value's own
// data type against the compiled declaration and THROWS on a mismatch, so the shape rides the slot beside the
// name and a scalar aimed at a float2 slot refuses on the rail instead of raising inside a draw.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UniformShape {
    public static readonly UniformShape Scalar = new("float");
    public static readonly UniformShape Extent = new("float2");
    public static readonly UniformShape Pigment = new("float4");
}

// What a declared slot IS, as one closed family rather than three parallel name lists. Conformance, coverage,
// seeding, and shape resolution were five projections of one roster before this, three of them recomputed on
// every read; each is now a filter over the same Seq and the two-way source check reads one set.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SlotKind {
    private SlotKind() { }
    // The band is the numeric admission the SkSL body's own `max` floor was silently standing in for; absent, the
    // slot takes any finite value its shape can carry.
    public sealed record Uniform(UniformShape Shape, Option<Band> Admits) : SlotKind;
    public sealed record Content : SlotKind;
    public sealed record Seed(Func<SKShader> Mint) : SlotKind;

    public Option<UniformShape> Shape => Switch(
        uniform: static row => Some(row.Shape),
        content: static _ => Option<UniformShape>.None,
        seed: static _ => Option<UniformShape>.None);

    public bool Admits(double value) => Switch(
        state: value,
        uniform: static (probe, row) => row.Admits.Match(Some: band => band.Admits(probe), None: static () => true),
        content: static (_, _) => true,
        seed: static (_, _) => true);
}

public readonly record struct SlotRow(string Name, SlotKind Kind) {
    public static SlotRow Uniform(string name, UniformShape shape) => new(name, new SlotKind.Uniform(shape, None));
    public static SlotRow Bounded(string name, UniformShape shape, Band admits) => new(name, new SlotKind.Uniform(shape, Some(admits)));
    public static SlotRow Content(string name) => new(name, new SlotKind.Content());
    public static SlotRow Seed(string name, Func<SKShader> mint) => new(name, new SlotKind.Seed(mint));
    public static SlotRow Extent() => new(EffectRow.ExtentName, new SlotKind.Uniform(UniformShape.Extent, None));
}

// The closed roster. Source is the LAW here — a uniform name, a channel order, and a falloff shape are the
// contract the binding fold and every consumer read against, so the SkSL body is transcribed rather than
// described. Every program returns PREMULTIPLIED half4, which is the shader contract Skia composites under; an
// unpremultiplied return reads as a halo brighter than its own alpha at every partially covered pixel.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EffectRow {
    // The extent name every resolution-independent falloff divides by. The frame seats it from the size it is
    // already drawing at, so a consumer binds the values that MOVE and never the extent beneath them, and the
    // seat is conditional on the row declaring it because an undeclared name is a refusal rather than a drop.
    public const string ExtentName = "extent";

    // The ONE film field the grain and the glass both sample. Base frequency is per-DEVICE-unit and the seed is
    // fixed, so two programs citing this mint read the same field and the glass cannot disagree with the grain
    // at the pixel; the octave count carries the film's fineness rather than a resolution-dependent frequency.
    static SKShader Field() => SKShader.CreatePerlinNoiseFractalNoise(0.8f, 0.8f, numOctaves: 3, seed: 0f);

    // Emissive edge falloff added to the child's own sample, so a focused control glows without a second draw
    // pass and an unfocused one composes to the identity. `radius` is the fraction of the half-extent the
    // falloff occupies, so the term is resolution-independent and a density change re-derives nothing.
    public static readonly EffectRow Glow = new("glow",
        """
        uniform shader content;
        uniform float2 extent;
        uniform float4 tint;
        uniform float radius;
        uniform float intensity;
        half4 main(float2 coord) {
            half4 src = content.eval(coord);
            float2 d = abs((coord / extent) - 0.5) * 2.0;
            float reach = max(radius, 1e-3);
            float edge = clamp((max(d.x, d.y) - (1.0 - reach)) / reach, 0.0, 1.0);
            half halo = half(edge * intensity);
            return src + (half4(half3(tint.rgb) * halo, halo) * (1.0h - src.a));
        }
        """,
        slots: [SlotRow.Extent(), SlotRow.Uniform("tint", UniformShape.Pigment),
            SlotRow.Bounded("radius", UniformShape.Scalar, Band.Positive),
            SlotRow.Uniform("intensity", UniformShape.Scalar), SlotRow.Content("content")]);

    // Zero-mean film grain over the sampled noise child: the material lays this under an Overlay blend, so the
    // output centres on mid grey and the paint's own alpha carries the weight. A luminance-shifted grain would
    // wash every rung beneath it toward the noise's own mean. The noise is the estate's own source and mints from
    // the shared field, because a per-surface noise would re-seed the field the refraction row samples.
    public static readonly EffectRow Grain = new("grain",
        """
        uniform shader noise;
        uniform float weight;
        half4 main(float2 coord) {
            half n = noise.eval(coord).r;
            half g = clamp(0.5h + ((n - 0.5h) * half(weight)), 0.0h, 1.0h);
            return half4(g, g, g, 1.0h);
        }
        """,
        slots: [SlotRow.Uniform("weight", UniformShape.Scalar), SlotRow.Seed("noise", Field)]);

    // The displacement field the glass reads. Skia's displacement map takes ONE channel per axis and offsets by
    // `(channel - 0.5) * scale`, so an achromatic source hands the same value to both axes and every pixel
    // slides along one diagonal — a shear wearing glass's name. The two axes therefore sample the shared field
    // at decorrelated positions and land in the red and green channels, and `coarse` sets that separation as a
    // fraction of the extent so the field's structure scales with the surface rather than with the device.
    public static readonly EffectRow Refract = new("refract",
        """
        uniform shader field;
        uniform float2 extent;
        uniform float coarse;
        half4 main(float2 coord) {
            float2 span = extent * max(coarse, 1e-3);
            half x = field.eval(coord).r;
            half y = field.eval(coord + span).r;
            return half4(x, y, 0.0h, 1.0h);
        }
        """,
        slots: [SlotRow.Extent(), SlotRow.Bounded("coarse", UniformShape.Scalar, Band.Positive),
            SlotRow.Seed("field", Field)]);

    // The module ambient wash: one directional falloff over the surface at the row's coverage. Angle is a
    // uniform because a layout change re-aims the wash without recompiling, and coverage arrives already
    // clamped against its own luminance ceiling at the token owner.
    public static readonly EffectRow Wash = new("wash",
        """
        uniform float2 extent;
        uniform float4 hue;
        uniform float coverage;
        uniform float angle;
        half4 main(float2 coord) {
            float2 dir = float2(cos(angle), sin(angle));
            float t = clamp(dot((coord / extent) - 0.5, dir) + 0.5, 0.0, 1.0);
            half a = half(coverage * (1.0 - t));
            return half4(half3(hue.rgb) * a, a);
        }
        """,
        slots: [SlotRow.Extent(), SlotRow.Uniform("hue", UniformShape.Pigment),
            SlotRow.Bounded("coverage", UniformShape.Scalar, Band.Unit),
            SlotRow.Uniform("angle", UniformShape.Scalar)]);

    // A travelling specular band for indeterminate progress and skeleton states. Phase is the only moving
    // value, so the motion plane advances one float rather than re-authoring a gradient per frame.
    public static readonly EffectRow Sheen = new("sheen",
        """
        uniform shader content;
        uniform float2 extent;
        uniform float phase;
        uniform float width;
        half4 main(float2 coord) {
            float u = coord.x / extent.x;
            float band = 1.0 - clamp(abs(u - phase) / max(width, 1e-3), 0.0, 1.0);
            half4 src = content.eval(coord);
            return src + (half4(band * band) * (1.0h - src.a) * 0.5h);
        }
        """,
        slots: [SlotRow.Extent(), SlotRow.Bounded("phase", UniformShape.Scalar, Band.Unit),
            SlotRow.Bounded("width", UniformShape.Scalar, Band.Positive), SlotRow.Content("content")]);

    public string Sksl { get; }

    // ONE declared roster; every reading below is a filter over it, so the conformance check, the coverage gate,
    // the seeding fold, and the shape resolution can never disagree about what this row declares.
    public Seq<SlotRow> Slots { get; }

    public Seq<string> UniformNames => Slots.Filter(static slot => slot.Kind is SlotKind.Uniform).Map(static slot => slot.Name);

    // The child half is the union of caller-supplied and estate-seeded slots, because the compiled program
    // publishes ONE child roster and the split governs only who supplies the native.
    public Seq<string> ChildNames => Slots.Filter(static slot => slot.Kind is not SlotKind.Uniform).Map(static slot => slot.Name);

    public Seq<SlotRow> Seeds => Slots.Filter(static slot => slot.Kind is SlotKind.Seed);

    // One TOTAL name resolution per frame cell: a name this row never declared refuses here, because both native
    // `Add` members throw on an unknown name and a throw inside a bind is the arm this rail exists to replace.
    public Fin<SlotRow> Slot(string name) =>
        Slots.Find(row => row.Name == name)
            .ToFin(new EffectFault.UniformUndeclared(Key, SlotDefect.Undeclared, Seq(name)));
}
```

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// The compiled cell. It retains the BUILDER, not the effect: SKRuntimeEffectBuilder.Dispose disposes the
// effect it wraps, so a per-frame builder over a cached effect frees that effect at the end of the frame and
// every later frame binds a dead handle. One builder per row, re-bound per frame, disposed with the catalog —
// and the row's seeded children mint once beside it, because a per-frame mint would leak one native generator
// per draw while the block's own child array holds nothing it owns. Identity is the ROW: compiler-generated
// equality over a native builder handle and a shader map answers reference identity wearing a value's name.
[Equatable]
public sealed partial record EffectProgram(
    EffectRow Row,
    [property: IgnoreEquality] SKRuntimeShaderBuilder Builder,
    [property: IgnoreEquality] HashMap<string, SKShader> Seeded) : IDisposable {
    // The typed compile gate. CreateShader returns a NULL effect and a non-null diagnostic on rejection and a
    // non-null effect with a null diagnostic on success, so both halves of the pair discriminate one outcome.
    // BuildShader validates the identical pair and throws, which is why it has no call site on this rail.
    public static Fin<EffectProgram> Compile(EffectRow row) =>
        SKRuntimeEffect.CreateShader(row.Sksl, out string? errors) switch {
            null => Fin.Fail<EffectProgram>(new EffectFault.SkslRejected(row.Key, errors ?? "no diagnostic")),
            var effect => Declared(row, effect).Map(builder => new EffectProgram(
                row, builder, toHashMap(row.Seeds.Map(static slot =>
                    (slot.Name, ((SlotKind.Seed)slot.Kind).Mint()))))),
        };

    // Declared-name conformance at ADMISSION, as an ACCUMULATING admission over two independent axes: the
    // compiled program publishes its own uniform and child names, and a row whose declared roster drifted on both
    // halves reports both rather than the first. Custody transfers on success — the builder adopts the effect —
    // and rolls the effect back on refusal, so the hand dispose-inside-the-refusal-arm shape that double-releases
    // the day a second refusal path lands has no spelling here.
    static Fin<SKRuntimeShaderBuilder> Declared(EffectRow row, SKRuntimeEffect effect) =>
        (Agrees(row.UniformNames, toSeq(effect.Uniforms),
             names => new EffectFault.UniformUndeclared(row.Key, SlotDefect.Diverged, names)),
         Agrees(row.ChildNames, toSeq(effect.Children),
             names => new EffectFault.ChildUndeclared(row.Key, SlotDefect.Diverged, names)))
            .Apply((_, _) => new SKRuntimeShaderBuilder(effect))
            .ToFin()
            .Rollback(effect);

    // Symmetric difference in ONE pass answering the offending NAMES: the two-pass `Except(...).Any()` pair this
    // replaced reported that the rosters disagreed and never which member disagreed.
    static Validation<Error, Unit> Agrees(Seq<string> declared, Seq<string> source, Func<Seq<string>, EffectFault> refuse) =>
        (declared.Except(source).ToSeq() + source.Except(declared).ToSeq()).Strict() switch {
            { IsEmpty: true } => Validation<Error, Unit>.Success(unit),
            var diverged => Validation<Error, Unit>.Fail((Error)refuse(diverged)),
        };

    // Release order is ownership order: the builder holds the effect and the block references, so the seeded
    // generators the block pointed at drop after it. `SKRuntimeEffectChildren.Dispose` releases NOTHING it was
    // handed, which is exactly why the cell owns the seeded shaders rather than the block it bound them into.
    public void Dispose() {
        Builder.Dispose();
        Seeded.Iter(static shader => shader.Dispose());
    }
}

// --- [COMPOSITION] ----------------------------------------------------------------------
// The process roster, compiled once and addressed by ROW, and the ONE surface a consumer names. A refused row
// fails the whole catalog rather than leaving a hole a draw discovers: a chrome surface that silently loses its
// glow reads as a rendering bug at every consumer, where a refused catalog names the offending source at boot.
public sealed record EffectCatalog(HashMap<EffectRow, EffectProgram> Programs) : IDisposable {
    // The fold is what a traverse cannot be here: a short-circuiting traverse drops every program compiled
    // before the offending row with no other owner holding them, so the refusal rolls its own prefix back.
    public static Fin<EffectCatalog> Of() =>
        toSeq(EffectRow.Items)
            .Fold(Fin.Succ(Seq<EffectProgram>()), static (state, row) => state.Bind(built =>
                EffectProgram.Compile(row).Map(built.Add).Rollback([.. built])))
            .Map(static programs => new EffectCatalog(toHashMap(programs.Map(static p => (p.Row, p)))));

    // `Build` mints a FRESH shader over the block as it stands, so the product is the caller's to release and
    // the cell keeps only the builder — a consumer holding a built shader past its own draw holds a native the
    // next frame's bind has already rewritten the inputs of.
    public Fin<SKShader> Source(EffectRow row, UniformFrame frame) =>
        Programs.Find(row)
            .ToFin(new EffectFault.ProgramMissing(row.Key))
            .Bind(frame.Bind)
            .Map(static builder => builder.Build());

    public void Dispose() => Programs.Iter(static program => program.Dispose());
}
```

## [03]-[UNIFORM_FRAME]

- Owner: `UniformFrame` the per-frame binding value; `UniformValue` `[Union]` its typed cell.
- Cases: `UniformValue` = Scalar | Extent | Pigment | Child.
- Law: the case-to-shape correspondence is declared ONCE, on `UniformValue.Shape`; the bind compares that answer against the slot's own declared shape and every other reading derives. A fifth value kind cannot compile without electing its block shape or declaring itself child-shaped, where the three hand-written pairings this replaced let a new kind bind and silently fail the shape test.
- Law: a frame binds by NAME against the compiled program's declaration and COVERAGE is what the fold proves — every declared uniform and every declared child is written on every bind, so a value the previous frame set can never survive into this one and no slot reaches the shader unwritten; partial re-binding is the defect that makes a glow keep a radius from the frame before it.
- Entry: `public Fin<SKRuntimeShaderBuilder> Bind(EffectProgram program)` — the one binding fold; `public static UniformFrame Of(SKSize extent, params (string Name, UniformValue Value)[] cells)` — the mint every consumer takes.
- Auto: the extent cell seats itself on every program declaring one, because a resolution-independent falloff divides by it and a program that declares none refuses the name outright; the row's seeded children re-enter the block from the compiled cell, so a caller binds the values that MOVE and never the sources the estate ships; pigment cells project through the same `ColorPolicy.Resolve` the material tint takes, so a shader colour and a painted colour agree in the generation's one working space.
- Packages: SkiaSharp, LanguageExt.Core, Rasm (`Op`, `Band`)
- Growth: a new uniform kind is one `UniformValue` case with its `Shape` row, its write arm, and one `UniformShape` row; zero new surface.
- Boundary: `Contains` on either block answers the compiled program's own DECLARED name roster and never whether a value landed at that name, so a coverage probe against the block reads every declared name as present on a block a `Reset` just zeroed — coverage is therefore the fold's own record of what it wrote, read through a hash set rather than a quadratic `Seq.Contains` rescan. Neither block is reset: `SKRuntimeEffectUniforms.Reset` re-creates the uniform `SKData` and abandons the previous one undisposed, so a per-frame reset trades one native allocation per program per frame for a staleness total coverage already forecloses.
- Boundary: the write is lifted through `Op.Catch`, not through a bare invoke — both `Add` members THROW on an unknown name and on a data type the declaration cannot take, and the shape gate ahead of the write makes those arms unreachable rather than trapped, so the lift is the belt that keeps a native throw a value on the fold instead of an escape past the rail. A frame carries no time: motion advances the values a frame BINDS, and the clock that advances them belongs to `compose#CUSTOM_VISUAL_TICK`, so a shader cannot read a wall clock and drift from the animation driving it.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// One typed cell per uniform kind, each owning the write its own case admits and each ANSWERING its block shape.
// That answer is the page's one primary correspondence: the bind is a single equality against the slot's declared
// shape, so the child arm is total against the split — a shader aimed at a uniform name and a value aimed at a
// child name both refuse before the block sees either.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UniformValue {
    static readonly Op Binding = Op.Of(name: "appui.effect.bind");

    private UniformValue() { }
    public sealed record Scalar(float Value) : UniformValue;
    public sealed record Extent(SKSize Value) : UniformValue;
    public sealed record Pigment(SKColorF Value) : UniformValue;
    public sealed record Child(SKShader Value) : UniformValue;

    public Option<UniformShape> Shape => Switch(
        scalar: static _ => Some(UniformShape.Scalar),
        extent: static _ => Some(UniformShape.Extent),
        pigment: static _ => Some(UniformShape.Pigment),
        child: static _ => Option<UniformShape>.None);

    public Fin<Unit> Bind(SKRuntimeShaderBuilder builder, string program, SlotRow slot) =>
        Shape == slot.Kind.Shape
            ? Admitted(program, slot).Bind(_ => Written(builder, slot.Name))
            : Fin.Fail<Unit>(new EffectFault.UniformUndeclared(program, SlotDefect.Misplaced, Seq(slot.Name)));

    // The numeric band is the SCALAR axis alone: an extent and a pigment carry their own admitted carriers, and
    // a child slot carries a native. A slot declaring no band admits every finite value its shape can hold.
    Fin<Unit> Admitted(string program, SlotRow slot) => Switch(
        state: (Program: program, Slot: slot),
        scalar: static (s, cell) => s.Slot.Kind.Admits(cell.Value)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new EffectFault.UniformUndeclared(s.Program, SlotDefect.Refused, Seq(s.Slot.Name))),
        extent: static (_, _) => Fin.Succ(unit),
        pigment: static (_, _) => Fin.Succ(unit),
        child: static (_, _) => Fin.Succ(unit));

    Fin<Unit> Written(SKRuntimeShaderBuilder builder, string name) => Binding.Catch(() => Switch(
        state: (Builder: builder, Name: name),
        scalar: static (s, cell) => { s.Builder.Uniforms.Add(s.Name, cell.Value); return Fin.Succ(unit); },
        extent: static (s, cell) => { s.Builder.Uniforms.Add(s.Name, cell.Value); return Fin.Succ(unit); },
        pigment: static (s, cell) => { s.Builder.Uniforms.Add(s.Name, cell.Value); return Fin.Succ(unit); },
        child: static (s, cell) => { s.Builder.Children.Add(s.Name, cell.Value); return Fin.Succ(unit); }));
}

// The per-frame binding value. Neither block is reset: `Uniforms.Reset` re-creates the uniform data and
// abandons the previous `SKData` undisposed, and total coverage is the stronger guarantee anyway. The frame
// carries the caller's cells alone; the extent it is drawing at and the row's seeded children seat inside the
// fold, so a consumer never names a slot whose source the estate already ships.
public sealed record UniformFrame(SKSize Extent, Seq<(string Name, UniformValue Value)> Cells) {
    public static UniformFrame Of(SKSize extent, params (string Name, UniformValue Value)[] cells) =>
        new(extent, toSeq(cells));

    // Resolve, write, record — one pass. The recorded name set is what the coverage gate reads, because the
    // block itself cannot answer which of its declared names carry a value.
    public Fin<SKRuntimeShaderBuilder> Bind(EffectProgram program) =>
        Seated(program)
            .Fold(Fin.Succ(Seq<string>()), (state, cell) => state.Bind(bound =>
                program.Row.Slot(cell.Name)
                    .Bind(slot => cell.Value.Bind(program.Builder, program.Row.Key, slot))
                    .Map(_ => bound.Add(cell.Name))))
            .Bind(bound => Covered(program, bound));

    // The caller's cells, the extent where the row declares one, and the row's seeded children as ONE cell
    // stream, so the resolution, the write, and the coverage gate each read one shape. The extent seat is
    // CONDITIONAL because an undeclared name is a refusal here rather than a silently dropped cell, and the
    // grain program declares no extent at all.
    Seq<(string Name, UniformValue Value)> Seated(EffectProgram program) =>
        (program.Row.Slot(EffectRow.ExtentName).IsSucc
            ? Cells.Add((EffectRow.ExtentName, (UniformValue)new UniformValue.Extent(Extent)))
            : Cells)
        + toSeq(program.Seeded).Map(static seed =>
            (Name: seed.Key, Value: (UniformValue)new UniformValue.Child(seed.Value)));

    // Coverage is a VALUE, and both halves accumulate: a source edit leaving one uniform and one child unbound
    // names both. The written set folds into a hash set once, so the gate is one pass over the declared roster
    // rather than a `Seq.Contains` scan per declared name.
    static Fin<SKRuntimeShaderBuilder> Covered(EffectProgram program, Seq<string> bound) {
        // CS0104 guard: `LanguageExt.HashSet` collides with the BCL name under the dual usings.
        LanguageExt.HashSet<string> written = toHashSet(bound);
        Seq<SlotRow> absent = program.Row.Slots.Filter(slot => !written.Contains(slot.Name)).Strict();
        return (Whole(absent.Filter(static slot => slot.Kind is SlotKind.Uniform),
                    names => new EffectFault.UniformUndeclared(program.Row.Key, SlotDefect.Unbound, names)),
                Whole(absent.Filter(static slot => slot.Kind is not SlotKind.Uniform),
                    names => new EffectFault.ChildUndeclared(program.Row.Key, SlotDefect.Unbound, names)))
            .Apply((_, _) => program.Builder)
            .ToFin();
    }

    static Validation<Error, Unit> Whole(Seq<SlotRow> absent, Func<Seq<string>, EffectFault> refuse) =>
        absent.IsEmpty
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail((Error)refuse(absent.Map(static slot => slot.Name)));
}
```

| [INDEX] | [PROGRAM] | [MOVING_UNIFORM] | [CONSUMER]                                  |
| :-----: | :-------- | :--------------- | :------------------------------------------ |
|  [01]   | `glow`    | `intensity`      | focus and call-to-action emphasis           |
|  [02]   | `grain`   | `weight`         | the `material#MATERIAL_EXECUTION` grain lay |
|  [03]   | `refract` | `coarse`         | the `material#FILTER_ROWS` refraction row   |
|  [04]   | `wash`    | `angle`          | the `material#MATERIAL_EXECUTION` wash lane |
|  [05]   | `sheen`   | `phase`          | indeterminate progress and skeleton states  |

## [04]-[PATH_ROWS]

- Owner: `PathRow` `[Union]` the per-draw stroked-geometry pattern family; `TileMark` `[Union]` the tiled operand; `DashRun` the admitted alternation carrier; `PatternRow` `[SmartEnum<string>]` the estate's closed pattern roster.
- Cases: `PathRow` = Dash | Trim | Stamp | Tiled; `TileMark` = Cell | Rule; `PatternRow` = marching | leader | hatch.
- Law: a row lands here only when its geometry MOVES per draw or per frame — a marching selection phase, a leader trim drawing on, a stamped run advancing along its contour. A pattern whose intervals hold for a whole theme generation belongs to the frozen `FxRow.Dashes` catalogue at its capture-side owner, and a pattern whose intervals derive from a resolved stroke width belongs to the mark geometry at `Charts/custom#SKIA_KINDS`.
- Law: admission is at CONSTRUCTION, not at the native mint. Every span is a `PositiveMagnitude`, every advance and rule width likewise, and a dash run refuses below two spans at `DashRun.Of`; the only refusal left inside `Build` is the native's own null answer, which is a boundary fact and not a re-run of a guard the value already carries.
- Entry: `public Fin<FxEffect> Build()` — the one native mint, producing the capture-side pathing case every consumer already binds; `public PathRow AtPhase(UnitInterval progress)` — the one per-frame advance the render-thread tick reads; `PatternRow.Seed()` — the estate row's own admitted seed value.
- Auto: `PatternRow` seats the three estate patterns whose seeds hold no caller geometry, so `compose#CUSTOM_VISUAL_TICK` addresses a marching outline, a leader trim, and a hatch by ROW exactly as a shader draw addresses a program by row; the phase advance stays NORMALIZED on the row and each cycle multiply happens once at the native mint, so a re-timed token changes the speed without re-authoring the pattern and no arm re-folds an interval period per frame.
- Packages: SkiaSharp, Rasm (`UnitInterval`, `PositiveMagnitude`, `Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new moving pattern is one `PathRow` case with its native arm and its phase arm; a new estate pattern is one `PatternRow` row; a new tiled operand is one `TileMark` case; zero new surface.
- Boundary: a pattern is GEOMETRY applied to a stroke, so every row binds the `SKPaint.PathEffect` slot and none of them reaches the shader or colour slots — a pattern spelled as a shader tiles the FILL and leaves the stroke smooth, which reads as a solid line over a patterned interior. `Trim` carries a START and a normalized PROGRESS rather than a start and a stop: the stop derives as `start + progress·(1 − start)`, so a range whose stop precedes its start is unrepresentable and the degenerate-trim refusal it used to need has no arm. NAMED LOSS: a caller wanting an explicit stop states the progress that reaches it.
- Boundary: the tiled row takes its tiling from an `SKMatrix`, so spacing and rotation are one transform rather than a spacing knob beside an angle knob — the pair drifts the moment either moves — and it carries no phase because a tiling advance is the matrix's own translation. A `Cell` operand and a `Rule` operand differ only in which native factory takes that matrix, which is why they are ONE case over a two-armed operand rather than two rows. The stamped row's advance is contour-space, not device-space, so a stamp on a scaled canvas keeps its spacing relative to the geometry it decorates rather than to the pixels beneath it; the estate roster seats no stamped or celled row because both carry a caller's `SKPath` and the roster ships source, never a native a row would have to own.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// A dash interval run admitted WHOLE: two spans minimum, every span positive by its carrier, and the alternation
// period derived from the one roster that holds it rather than re-folded at every phase read.
public readonly record struct DashRun {
    static readonly Op Runs = Op.Of(name: "appui.effect.dash");

    DashRun(Seq<PositiveMagnitude> intervals) => Intervals = intervals;

    public Seq<PositiveMagnitude> Intervals { get; }

    public float Period => (float)Intervals.Fold(0d, static (acc, span) => acc + span.Value);

    public static Fin<DashRun> Of(params ReadOnlySpan<double> spans) =>
        spans.Length >= 2
            ? toSeq(spans.ToArray())
                .Traverse(span => Runs.AcceptValidated<PositiveMagnitude>(span)).As()
                .Map(static intervals => new DashRun(intervals))
            : Fin.Fail<DashRun>(new EffectFault.PatternDegenerate(
                "a dash run alternates at least one on span and one off span"));
}

// The tiled operand. Both arms hand the SAME matrix to a 2D path-effect factory and differ only in what the
// factory repeats, so a second top-level row for the ruled case would have carried one operand as a whole type.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileMark {
    private TileMark() { }
    public sealed record Cell(SKPath Path) : TileMark;
    public sealed record Rule(PositiveMagnitude Width) : TileMark;
}

// Per-draw stroked-geometry patterns. Every row binds the PathEffect slot: a pattern spelled as a shader tiles
// the fill and leaves the stroke smooth, which is a defect that renders as a plausible picture. Every case
// carries its parameters as ROW DATA and mints its native at Build, because each one's geometry moves — a
// marching phase per frame, a trim progress as a leader draws on, a stamp advancing along its contour.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathRow {
    private PathRow() { }
    public sealed record Dash(DashRun Run, UnitInterval Phase) : PathRow;
    public sealed record Trim(UnitInterval Start, UnitInterval Progress, SKTrimPathEffectMode Mode) : PathRow;
    public sealed record Stamp(SKPath Glyph, PositiveMagnitude Advance, UnitInterval Phase, SKPath1DPathEffectStyle Style) : PathRow;
    public sealed record Tiled(SKMatrix Tiling, TileMark Mark) : PathRow;

    // The one per-frame read, and it is TOTAL by construction: the phase a row carries is the NORMALIZED
    // progress, so every moving arm seats one admitted value and no arm re-mints a fallible carrier from
    // arithmetic. The tiled row advances through its matrix and holds.
    public PathRow AtPhase(UnitInterval progress) => Switch(
        state: progress,
        dash: static (p, row) => (PathRow)(row with { Phase = p }),
        trim: static (p, row) => row with { Progress = p },
        stamp: static (p, row) => row with { Phase = p },
        tiled: static (_, row) => row);

    // The cycle multiply happens HERE, once, against the period the run already holds: a dash walks one full
    // interval period and a stamp one glyph advance per unit of progress, so a run reads as continuous whatever
    // the pattern measures. A trim draws on FROM its own start toward the end, so the derived stop can never
    // precede the start it was measured from. Each factory answers null on native-degenerate input, which is the
    // one refusal left after admission moved to construction.
    public Fin<FxEffect> Build() => Switch(
        dash: static row => Minted(SKPathEffect.CreateDash(
            [.. row.Run.Intervals.Map(static span => (float)span.Value)], (float)row.Phase.Value * row.Run.Period),
            $"dash {row.Run.Intervals}"),
        trim: static row => Minted(SKPathEffect.CreateTrim(
            (float)row.Start.Value,
            (float)(row.Start.Value + (row.Progress.Value * (1d - row.Start.Value))), row.Mode),
            $"trim {row.Start.Value}+{row.Progress.Value}"),
        stamp: static row => Minted(SKPathEffect.Create1DPath(
            row.Glyph, (float)row.Advance.Value, (float)row.Phase.Value * (float)row.Advance.Value, row.Style),
            $"stamp advance {row.Advance.Value}"),
        tiled: static row => Minted(row.Mark switch {
            TileMark.Cell cell => SKPathEffect.Create2DPath(row.Tiling, cell.Path),
            var mark => SKPathEffect.Create2DLine((float)((TileMark.Rule)mark).Width.Value, row.Tiling),
        }, $"tiled {row.Tiling}"));

    static Fin<FxEffect> Minted(SKPathEffect? native, string detail) =>
        native is { } effect
            ? Fin.Succ<FxEffect>(new FxEffect.Pathing(effect))
            : Fin.Fail<FxEffect>(new EffectFault.PatternDegenerate(detail));
}

// --- [TABLES] ---------------------------------------------------------------------------
// The estate's own pattern vocabulary, seated exactly like the SkSL roster above and for the same reason: a
// pattern the tick addresses by ROW is source the estate ships. Every row's seed is native-FREE, so the roster
// owns no handle and re-seeds without leaking; a stamped glyph and a celled tile carry a caller's `SKPath` and
// stay caller-minted cases rather than rows.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PatternRow {
    public static readonly PatternRow Marching = new("marching", static () =>
        DashRun.Of(6d, 4d).Map(static run => (PathRow)new PathRow.Dash(run, UnitInterval.Create(0d))));
    public static readonly PatternRow Leader = new("leader", static () =>
        Fin.Succ<PathRow>(new PathRow.Trim(UnitInterval.Create(0d), UnitInterval.Create(0d), SKTrimPathEffectMode.Normal)));
    public static readonly PatternRow Hatch = new("hatch", static () =>
        Fin.Succ<PathRow>(new PathRow.Tiled(
            SKMatrix.CreateScale(8f, 8f), new TileMark.Rule(PositiveMagnitude.Create(1d)))));

    [UseDelegateFromConstructor]
    public partial Fin<PathRow> Seed();
}
```

## [05]-[TILE_CACHE]

- Owner: `PictureTileKey` the cache coordinate; `TileSubject` its typed addressing half; `TileCell` the recorded tile beside its cost; `TileOutcome` the admission verdict; `PictureTileHit` the product beside its receipt; `PictureTileCache` the plane's instance over the folder's one budgeted cache.
- Cases: `TileSubject` = Pattern | Program; `TileOutcome` = reuse | admit | refuse.
- Law: retention is the folder's ONE `Theme/assets#ASSET_CACHE` `BudgetedCache` under `RetentionPosture.Bound`, and the generation rides the KEY. A tile recorded under an earlier theme generation is addressed by a key no live caller mints, so a stale cell can never be replayed; the posture's floor then makes it the FIRST thing pressure releases and makes a cell at the live generation unreleasable, which is the RULINGS device-cache law read straight off the owner's row data.
- Law: every path SEALS. `Tile` answers a hit carrying the rail beside the receipt rather than a rail carrying a hit, so a refused admission emits the same evidence a reuse and an admit do — the shape that used to fail silently, leaving the effect plane's one refusal invisible to the fan.
- Entry: `public static Fin<PictureTileCache> Of(long ceilingBytes, IClock clock)` — the mint composition binds; `public PictureTileHit Tile(PictureTileKey key, SKRect cull, Func<SKCanvas, Fin<Unit>> record)` — the one admission-and-read; `public long Cycle()` — the theme-generation edge composition binds beside `AssetCache.Cycle`.
- Auto: a hatch pattern, a checker backplate, and a recorded wash all replay from one sealed op list instead of re-running their layout per frame; a tile projects to a shader through `SKPicture.ToShader`, so the same record serves a fill without a second raster; the owner's own admission path carries the probe, the build-on-miss, the least-touched pressure release, and the CAS loser releasing its own mint, so this plane spells none of them.
- Receipt: `PictureTileReceipt` — key, generation, recorded bytes, resident bytes, outcome row, released count, strain flag, `Instant`; it crosses through `Diagnostics/evidence` `EvidenceMap.ToEvidence` onto the `Effect` case under the `tile` plane literal, whose `Flag` slot carries the strain and whose `Magnitude` slot carries the resident total the mapper formats from this receipt's own `long`.
- Packages: SkiaSharp, NodaTime, LanguageExt.Core, Rasm (`Custody`, `Dimension`, `Op`), Thinktecture.Runtime.Extensions
- Growth: a new tiled surface is one `TileSubject` case or one row on a roster it already names; zero new surface.
- Boundary: a tile is a device-INDEPENDENT op list, not pixels, so one record serves every scale and a scale change re-plays rather than re-records — which is exactly why the cost measure is the op-list byte count and not a pixel area. A record exceeding the whole ceiling refuses at admission as `TileOversize` rather than retiring the table to seat one cell, and a ceiling admitting no record at all refuses at the mint as `BudgetExhausted`. The cache is the ONLY owner that disposes a `TileCell`; a caller holding a tile shader never releases it and never holds it past its own draw.
- Boundary: NAMED LOSS — under the owner's generation floor the ceiling DEGRADES rather than refusing. When every live cell belongs to the current generation, pressure frees nothing and the incoming record seats over the bound, where the retired hand cache refused the admission instead. That is the owner's declared law and this plane consumes it rather than re-deciding it: the overshoot lasts one generation, drains at the next `Cycle`, and the receipt's strain flag publishes it so a board reads the breach the refusal used to name. `SKPicture.ToShader` retains the picture, so the pair is one cell releasing in ownership order, and the recorder rides `Custody.Bracket` so a record fold that never reaches `EndRecording` still releases its own scope.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// What a tile RECORDS, as a typed pair of the two closed rosters this page owns. Two raw strings were spellable
// as any two strings and addressed nothing the estate declares.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TileSubject {
    private TileSubject() { }
    public sealed record Pattern(PatternRow Row) : TileSubject;
    public sealed record Program(EffectRow Row) : TileSubject;

    public string Key => Switch(pattern: static c => c.Row.Key, program: static c => c.Row.Key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TileOutcome {
    public static readonly TileOutcome Reuse = new("reuse");
    public static readonly TileOutcome Admit = new("admit");
    public static readonly TileOutcome Refuse = new("refuse");
}

// `Key` is the WIRE rendering the evidence projection reads, never an address: lookup is the value's own
// structural equality, so a composed string never resolves a cell.
public readonly record struct PictureTileKey(TileSubject Subject, Dimension CellPx, ThemeVariantRow Variant) {
    public string Key => $"{Subject.Key}/{CellPx.Value}/{Variant.Key}";
}

// --- [MODELS] ---------------------------------------------------------------------------

// A recorded tile beside its retained cost. Touch order and generation stamp belong to the cache owner's own
// slot, so this cell carries neither and cannot disagree with the ledger about either.
public sealed record TileCell(SKPicture Picture, SKShader Shader, long Bytes) : IDisposable {
    // Release order is ownership order: the shader samples the picture, so it drops first.
    public void Dispose() {
        Shader.Dispose();
        Picture.Dispose();
    }
}

// The product beside the receipt describing how it was obtained. The RAIL rides inside the hit rather than
// around it, which is what makes the receipt total — a refusal seals its own evidence instead of vanishing into
// a `Fin.Fail` the fan never sees.
public readonly record struct PictureTileHit(Fin<SKShader> Product, PictureTileReceipt Receipt);

// Byte columns stay `long` at this stratum: `EvidenceMap` formats each invariant-decimal text column from its
// own magnitude at the wire edge, so a receipt formatted here could not be compared, summed, or thresholded
// without re-parsing text the same page produced.
public readonly record struct PictureTileReceipt(
    PictureTileKey Key, long Generation, long RecordedBytes, long ResidentBytes,
    TileOutcome Outcome, int Evicted, bool Strained, Instant At);

// --- [SERVICES] -------------------------------------------------------------------------
// The tile plane's instance of the folder's one budgeted cache. The ceiling is BYTES and the measure is the op
// list's own retained cost, so a full-surface record and a hatch cell are comparable; a handle count would rank
// them identically. A sealed CLASS, not a record: this is one identity over a live lane.
public sealed class PictureTileCache : IDisposable {
    static readonly Op Tiling = Op.Of(name: "appui.tile.cache");

    // The generation rides the KEY, which is what makes the reach EXACT under a posture whose reads are
    // generation-blind: a caller mints its key against the live lane, so an older stamp is simply a key nobody
    // asks for, and the posture's release floor then retires exactly those stale cells first.
    readonly record struct Stamped(PictureTileKey At, long Generation);

    readonly BudgetedCache<Stamped, TileCell> cells;
    readonly IClock clock;
    readonly long ceiling;

    PictureTileCache(BudgetedCache<Stamped, TileCell> cells, long ceiling, IClock clock) =>
        (this.cells, this.ceiling, this.clock) = (cells, ceiling, clock);

    public static Fin<PictureTileCache> Of(long ceilingBytes, IClock clock) =>
        BudgetedCache<Stamped, TileCell>.Of(
                ceilingBytes, RetentionPosture.Bound,
                static cell => cell.Bytes, static cell => cell.Dispose(),
                (at, cost) => new EffectFault.TileOversize(at.At, at.Generation, cost, ceilingBytes), Tiling)
            .MapFail(_ => (Error)new EffectFault.BudgetExhausted(ceilingBytes))
            .Map(lane => new PictureTileCache(lane, ceilingBytes, clock));

    public long Generation => cells.Generation;

    public long Resident => cells.Bytes;

    // The theme-generation edge, bound at composition beside `AssetCache.Cycle` on the one `Rematerialize`
    // row naming a re-tinted asset: a tile recorded in the old palette is stale by construction, so raising the
    // lane retires it by making its key unmintable and its cell releasable in one move.
    public long Cycle() => cells.Retire(static (_, _) => false, advance: true).Generation;

    // Probe, record on a miss, seal on every path. The MINT's own identity is what separates an admit from a
    // reuse — `Take` answers the winner, so a CAS loser reports the reuse it became rather than the record it
    // paid for — and the sweep the seal drains is what the receipt's released count reports, because a count
    // column reads what happened since the previous receipt.
    public PictureTileHit Tile(PictureTileKey key, SKRect cull, Func<SKCanvas, Fin<Unit>> record) {
        TileCell? minted = null;
        Fin<TileCell> held = cells.Take(
            new Stamped(key, cells.Generation),
            () => Record(cull, record).Map(cell => minted = cell));
        CacheSweep sweep = cells.Seal();
        return new PictureTileHit(
            held.Map(static cell => cell.Shader),
            new PictureTileReceipt(
                Key: key,
                Generation: sweep.Generation,
                RecordedBytes: held.Match(Succ: static cell => cell.Bytes, Fail: static _ => 0L),
                ResidentBytes: sweep.Bytes,
                Outcome: held.Match(
                    Succ: cell => ReferenceEquals(cell, minted) ? TileOutcome.Admit : TileOutcome.Reuse,
                    Fail: static _ => TileOutcome.Refuse),
                Evicted: sweep.Released,
                Strained: sweep.Bytes > ceiling,
                At: clock.GetCurrentInstant()));
    }

    // --- [OPERATIONS] -----------------------------------------------------------------------
    // Record, measure, pair. The retained cost is knowable only after the op list seals, so the owner's ceiling
    // test runs against a real measure rather than an estimate, and `ToShader` retains the picture so the pair
    // is one cell from here on. The recorder's own scope releases on every path including the fold that never
    // reached `EndRecording`.
    static Fin<TileCell> Record(SKRect cull, Func<SKCanvas, Fin<Unit>> record) =>
        Custody.Bracket(
            static () => new SKPictureRecorder(),
            recorder => record(recorder.BeginRecording(cull))
                .Map(_ => recorder.EndRecording())
                .Map(static picture => new TileCell(
                    picture,
                    picture.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat),
                    picture.ApproximateBytesUsed)),
            Tiling);

    public void Dispose() => cells.Dispose();
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: AppUi Vfx shader plane from closed roster to sealed tile receipt
    accDescr: The closed SkSL roster compiling into the process catalog, the uniform frame binding through one slot vocabulary, the pattern roster seeding the path-effect family, and the tile cache composing the folder's budgeted cache into an evidence receipt.
    EffectRow --> SlotRow
    EffectRow --> EffectProgram
    EffectProgram --> EffectCatalog
    UniformFrame --> EffectProgram
    UniformValue --> UniformFrame
    PatternRow --> PathRow
    PathRow --> FxEffect
    PatternRow --> TileSubject
    EffectRow --> TileSubject
    TileSubject --> PictureTileKey
    PictureTileKey --> PictureTileCache
    PictureTileCache --> BudgetedCache
    PictureTileCache --> PictureTileReceipt
    PictureTileReceipt --> EvidenceMap
```

## [06]-[RESEARCH]

(none)

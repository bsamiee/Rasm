# [APPUI_VFX_SHADER]

Rasm.AppUi shader effects are the effects plane's procedural-source owner: one closed roster of SkSL programs compiled once and re-bound per frame, one uniform frame carrying every value that moves, one path-effect family for stroked geometry patterns, and one recorded-tile cache admitting under a byte ceiling. The plane exists for exactly the terms a frozen native cannot carry — a glow whose radius follows a focus transition, noise seeded per surface, a wash gradient whose angle tracks a layout, a hatch whose phase advances — and its roster is CLOSED because an SkSL program is source the estate compiles, not data a consumer supplies.

`SKRuntimeEffect`, `SKRuntimeShaderBuilder`, `SKPathEffect`, `SKPicture`, and `SKPictureRecorder` are the composed natives; `EffectTokens`, `FxEffect`, and `PaintCatalog` arrive settled from `Render/capture#DRAW_CAPSULE` and this plane produces values that bind into the same one paint. Compilation here is the 2D chrome partition — `Render/shading#SHADER_ASSET` owns the per-`GpuBackend` appearance-shader cache with its plane residency and VRAM budget, while a chrome program carries no backend variant, no resident plane, and a CPU-side program-and-picture budget instead, so the two caches are disjoint by type domain rather than by convention and neither holds the other's programs. Both rosters are ESTATE-SHIPPED source, so caller-supplied shader text has no admission on either. Programs resolve by ROW and uniform frame for the `material#FILTER_ROWS` refraction field and the `material#MATERIAL_EXECUTION` grain draw, and every fault derives through `AppUiFaultBand.Effect` (6810).

## [01]-[INDEX]

- [02]-[EFFECT_PROGRAM]: The closed SkSL roster, the compile gate on the typed rail, and the retained builder cell.
- [03]-[UNIFORM_FRAME]: Per-frame uniform and child binding with declared-name conformance.
- [04]-[PATH_ROWS]: Dash, trim, stamped, lattice, and ruled geometry patterns.
- [05]-[TILE_CACHE]: Recorded-tile admission under a byte ceiling with generation-stamped eviction.

## [02]-[EFFECT_PROGRAM]

- Owner: `EffectRow` `[SmartEnum<string>]` the closed program roster carrying its SkSL source, its declared names and their shapes, and the estate mint behind every procedural child slot; `UniformShape` `[SmartEnum<string>]` the declared uniform-block shape axis; `EffectProgram` the compiled cell; `EffectFault` the typed rail on the `AppUiFaultBand.Effect` 6810 registry row.
- Cases: glow | grain | refract | wash | sheen; `UniformShape` = float | float2 | float4; `EffectFault` = SkslRejected | UniformUndeclared | ChildUndeclared | ProgramMissing | PatternDegenerate | TileOversize | BudgetExhausted.
- Law: compilation takes the REFUSAL channel, never the throwing one — `SKRuntimeEffect.CreateShader(sksl, out string errors)` returns a null effect with the diagnostic in `errors` on a rejected program, while `BuildShader(sksl)` validates the same pair and throws; a compile that throws inside a static roster initializer takes the whole vocabulary down at type load, so the typed gate is the only admissible arm.
- Entry: `public static Fin<EffectProgram> Compile(EffectRow row)` — the one compile; `public Fin<SKShader> Shader(UniformFrame frame)` — the per-frame projection; `public Fin<SKShader> Source(EffectRow row, UniformFrame frame)` — the by-ROW projection `material#FILTER_ROWS` and the grain draw consume.
- Auto: the roster compiles once per process and the cell retains its `SKRuntimeShaderBuilder` beside every seeded child, so a frame re-binds uniforms and mints a shader without touching the SkSL compiler and without re-minting a noise source per draw; declared-name conformance folds at compile against `SKRuntimeEffect.Uniforms` and `Children`, so a source edit that renames a uniform fails at admission rather than binding nothing at draw time; every refusal past the compile releases what it refused, so neither a diverged row nor a rejected sibling leaves a live native behind.
- Packages: SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new procedural term is one `EffectRow` row carrying its source, its declared names and shapes, and its own child mints; zero new surface.
- Boundary: `SKRuntimeEffectBuilder.Dispose` disposes the EFFECT it wraps, so a cached effect handed to a per-frame builder is freed the moment that frame's builder falls out of scope and every later frame binds a dead handle — the cell therefore retains the BUILDER and constructs no second one, and `SKRuntimeEffect.BuildShader` is unreachable here because its throwing validation is the arm this rail replaced. A program is SOURCE the estate ships, so the roster is closed and no consumer supplies SkSL. Child slots split by who owns the pixels: a PROCEDURAL child is the estate's own source and its native is minted once at compile — the film field is `SKShader.CreatePerlinNoiseFractalNoise`, a shipped Skia generator rather than a hand-rolled SkSL lattice, and the grain and the glass cite ONE mint so the two sample the same field at the pixel — while a CONTENT child is the caller's already-painted draw entering through `SKRuntimeEffectChild`, which converts from a shader, a colour filter, or a blender. That split is what makes the coverage gate total: a procedural slot a frame never binds would otherwise refuse every draw of a program whose own source it is. Both `SKRuntimeEffectUniforms.Add` and `SKRuntimeEffectChildren.Add` THROW on a name the compiled program never declared, and the uniform overload additionally validates the value's own data type against that declaration and throws on a mismatch — so the row declares a SHAPE beside every uniform name and the frame resolves both axes against the row before it writes, which is what leaves the throwing arms unreachable rather than trapped.

```csharp signature
// --- [ERRORS] ---------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EffectFault : Expected, IValidationError<EffectFault> {
    private EffectFault(string detail, int code) : base(detail, code) { }
    public static EffectFault Create(string message) => new SkslRejected(message);
    public sealed record SkslRejected(string Detail)
        : EffectFault($"effect/sksl: {Detail}", AppUiFaultBand.Effect.Code(0));
    public sealed record UniformUndeclared(string Detail)
        : EffectFault($"effect/uniform: {Detail}", AppUiFaultBand.Effect.Code(1));
    public sealed record ChildUndeclared(string Detail)
        : EffectFault($"effect/child: {Detail}", AppUiFaultBand.Effect.Code(2));
    public sealed record ProgramMissing(string Detail)
        : EffectFault($"effect/program: {Detail}", AppUiFaultBand.Effect.Code(3));
    public sealed record PatternDegenerate(string Detail)
        : EffectFault($"effect/pattern: {Detail}", AppUiFaultBand.Effect.Code(4));
    public sealed record TileOversize(string Detail)
        : EffectFault($"effect/tile: {Detail}", AppUiFaultBand.Effect.Code(5));
    public sealed record BudgetExhausted(string Detail)
        : EffectFault($"effect/budget: {Detail}", AppUiFaultBand.Effect.Code(6));
}
```

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------

// The uniform-block shapes a chrome program declares. `SKRuntimeEffectUniforms.Add` validates the value's own
// data type against the compiled declaration and THROWS on a mismatch, so the shape rides the row beside the
// name and a scalar aimed at a float2 slot refuses on the rail instead of raising inside a draw.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UniformShape {
    public static readonly UniformShape Scalar = new("float");
    public static readonly UniformShape Extent = new("float2");
    public static readonly UniformShape Pigment = new("float4");
}

// The closed roster. Source is the LAW here — a uniform name, a channel order, and a falloff shape are the
// contract the binding fold and every consumer read against, so the SkSL body is transcribed rather than
// described. Every program returns PREMULTIPLIED half4, which is the shader contract Skia composites under;
// an unpremultiplied return reads as a halo brighter than its own alpha at every partially covered pixel.
// `content` names the caller's already-painted draw and `seeds` the estate's own procedural children, whose
// natives mint once at compile — the declared child roster is the union of the two, so conformance still tests
// one set against the compiled program's own names.
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
        uniforms: [(ExtentName, UniformShape.Extent), ("tint", UniformShape.Pigment),
            ("radius", UniformShape.Scalar), ("intensity", UniformShape.Scalar)],
        content: ["content"], seeds: []);

    // Zero-mean film grain over the sampled noise child: the material lays this under an Overlay blend, so the
    // output centres on mid grey and the paint's own alpha carries the weight. A luminance-shifted grain would
    // wash every rung beneath it toward the noise's own mean.
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
        uniforms: [("weight", UniformShape.Scalar)], content: [],
        // The noise is the estate's own source, so it mints here from the shared field rather than arriving
        // from a caller: a per-surface noise would re-seed the field the refraction row samples and the glass
        // would disagree with the grain at the pixel.
        seeds: [("noise", (Func<SKShader>)Field)]);

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
        uniforms: [(ExtentName, UniformShape.Extent), ("coarse", UniformShape.Scalar)],
        content: [], seeds: [("field", (Func<SKShader>)Field)]);

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
        uniforms: [(ExtentName, UniformShape.Extent), ("hue", UniformShape.Pigment),
            ("coverage", UniformShape.Scalar), ("angle", UniformShape.Scalar)],
        content: [], seeds: []);

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
        uniforms: [(ExtentName, UniformShape.Extent), ("phase", UniformShape.Scalar),
            ("width", UniformShape.Scalar)],
        content: ["content"], seeds: []);

    public string Sksl { get; }

    // Every declared uniform beside the block SHAPE its declaration takes.
    public Seq<(string Name, UniformShape Shape)> Uniforms { get; }

    // The caller-supplied slots — the only names a frame is obliged to carry.
    public Seq<string> Content { get; }

    // The estate-owned slots beside the mint each one binds, minted once per process at compile.
    public Seq<(string Name, Func<SKShader> Mint)> Seeds { get; }

    // The declared roster the compiled program is tested against: conformance reads one set, and the split
    // above governs only who supplies the native.
    public Seq<string> Children => Content + Seeds.Map(static seed => seed.Name);

    public Seq<string> UniformNames => Uniforms.Map(static row => row.Name);

    // One name resolution per frame cell, and it is TOTAL: a declared uniform answers its SHAPE, a declared
    // child answers no shape, and a name this row never declared refuses here — because both native `Add`
    // members throw on an unknown name, and a throw inside a bind is the arm this rail exists to replace.
    public Fin<Option<UniformShape>> Slot(string name) =>
        Uniforms.Find(row => row.Name == name).Match(
            Some: row => Fin.Succ(Some(row.Shape)),
            None: () => Children.Contains(name)
                ? Fin.Succ(Option<UniformShape>.None)
                : Fin.Fail<Option<UniformShape>>(new EffectFault.UniformUndeclared(
                    $"{Key}: {name} is neither a declared uniform nor a declared child")));
}
```

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// The compiled cell. It retains the BUILDER, not the effect: SKRuntimeEffectBuilder.Dispose disposes the
// effect it wraps, so a per-frame builder over a cached effect frees that effect at the end of the frame and
// every later frame binds a dead handle. One builder per row, re-bound per frame, disposed with the catalog —
// and the row's procedural children mint once beside it, because a per-frame mint would leak one native
// generator per draw while the block's own child array holds nothing it owns.
public sealed record EffectProgram(EffectRow Row, SKRuntimeShaderBuilder Builder, HashMap<string, SKShader> Seeded)
    : IDisposable {
    // The typed compile gate. CreateShader returns a NULL effect and a non-null diagnostic on rejection and a
    // non-null effect with a null diagnostic on success, so both halves of the pair discriminate one outcome.
    // BuildShader validates the identical pair and throws, which is why it has no call site on this rail.
    public static Fin<EffectProgram> Compile(EffectRow row) =>
        SKRuntimeEffect.CreateShader(row.Sksl, out string? errors) switch {
            null => Fin.Fail<EffectProgram>(new EffectFault.SkslRejected($"{row.Key}: {errors}")),
            var effect => Declared(row, effect).Map(builder => new EffectProgram(
                row, builder, toHashMap(row.Seeds.Map(static seed => (seed.Name, seed.Mint()))))),
        };

    // Declared-name conformance at ADMISSION: the compiled program publishes its own uniform and child names,
    // so a row whose declared roster drifts from its source fails here rather than binding into an offset the
    // compiler assigned to a different value. The check is two-way — an undeclared row name and a source name
    // no row claims are both defects, and the second is the one no per-bind check can see. A refusal RELEASES
    // the effect it refused, because the builder that would have adopted the handle is never constructed and
    // the seeded generators past it are never minted, so nothing else owns either.
    static Fin<SKRuntimeShaderBuilder> Declared(EffectRow row, SKRuntimeEffect effect) =>
        Divergence(row, effect).Match(
            Some: fault => Refused(effect, fault),
            None: () => Fin.Succ(new SKRuntimeShaderBuilder(effect)));

    static Option<EffectFault> Divergence(EffectRow row, SKRuntimeEffect effect) =>
        (toSeq(effect.Uniforms), toSeq(effect.Children)) switch {
            var (uniforms, _) when !Agrees(row.UniformNames, uniforms) => Some<EffectFault>(
                new EffectFault.UniformUndeclared($"{row.Key}: row {row.UniformNames} against source {uniforms}")),
            var (_, children) when !Agrees(row.Children, children) => Some<EffectFault>(
                new EffectFault.ChildUndeclared($"{row.Key}: row {row.Children} against source {children}")),
            _ => None,
        };

    static bool Agrees(Seq<string> declared, Seq<string> source) =>
        !declared.Except(source).Any() && !source.Except(declared).Any();

    static Fin<SKRuntimeShaderBuilder> Refused(SKRuntimeEffect effect, EffectFault fault) {
        effect.Dispose();
        return Fin.Fail<SKRuntimeShaderBuilder>(fault);
    }

    // `Build` mints a FRESH shader over the block as it stands, so the product is the caller's to release and
    // the cell keeps only the builder — a consumer holding a built shader past its own draw holds a native the
    // next frame's bind has already rewritten the inputs of.
    public Fin<SKShader> Shader(UniformFrame frame) => frame.Bind(this).Map(static builder => builder.Build());

    // Release order is ownership order: the builder holds the effect and the block references, so the seeded
    // generators the block pointed at drop after it. `SKRuntimeEffectChildren.Dispose` releases NOTHING it was
    // handed, which is exactly why the cell owns the seeded shaders rather than the block it bound them into.
    public void Dispose() {
        Builder.Dispose();
        Seeded.Iter(static shader => shader.Dispose());
    }
}

// The process roster, compiled once and addressed by ROW. A refused row fails the whole catalog rather than
// leaving a hole a draw discovers: a chrome surface that silently loses its glow reads as a rendering bug at
// every consumer, where a refused catalog names the offending source at boot.
public sealed record EffectCatalog(HashMap<EffectRow, EffectProgram> Programs) : IDisposable {
    // The fold is what a traverse cannot be here: a short-circuiting traverse drops every program compiled
    // before the offending row with no other owner holding them, so the refusal releases its own prefix.
    public static Fin<EffectCatalog> Of() =>
        toSeq(EffectRow.Items)
            .Fold(Fin.Succ(Seq<EffectProgram>()), static (state, row) => state.Bind(built =>
                EffectProgram.Compile(row)
                    .MapFail(fault => {
                        built.Iter(static program => program.Dispose());
                        return fault;
                    })
                    .Map(program => built.Add(program))))
            .Map(static programs => new EffectCatalog(toHashMap(programs.Map(static p => (p.Row, p)))));

    public Fin<EffectProgram> Program(EffectRow row) =>
        Programs.Find(row).ToFin(new EffectFault.ProgramMissing(row.Key));

    public Fin<SKShader> Source(EffectRow row, UniformFrame frame) =>
        Program(row).Bind(program => program.Shader(frame));

    public void Dispose() => Programs.Iter(static program => program.Dispose());
}
```

## [03]-[UNIFORM_FRAME]

- Owner: `UniformFrame` the per-frame binding value; `UniformValue` `[Union]` its typed cell.
- Cases: `UniformValue` = Scalar | Extent | Pigment | Child.
- Law: a frame binds by NAME against the compiled program's declaration and COVERAGE is what the fold proves — every declared uniform and every declared child is written on every bind, so a value the previous frame set can never survive into this one and no slot reaches the shader unwritten; partial re-binding is the defect that makes a glow keep a radius from the frame before it.
- Entry: `public Fin<SKRuntimeShaderBuilder> Bind(EffectProgram program)` — the one binding fold; `public static UniformFrame Of(SKSize extent, params (string Name, UniformValue Value)[] cells)` — the mint every consumer takes.
- Auto: the extent cell seats itself on every program declaring one, because a resolution-independent falloff divides by it and a program that declares none refuses the name outright; the row's seeded procedural children re-enter the block from the compiled cell, so a caller binds the values that MOVE and never the sources the estate ships; pigment cells project through the same `ColorPolicy.Resolve` the material tint takes, so a shader colour and a painted colour agree in the generation's one working space.
- Packages: SkiaSharp, LanguageExt.Core
- Growth: a new uniform kind is one `UniformValue` case with its `Bind` arm and one `UniformShape` row; zero new surface.
- Boundary: `Contains` on either block answers the compiled program's own DECLARED name roster and never whether a value landed at that name, so a coverage probe against the block reads every declared name as present on a block a `Reset` just zeroed — coverage is therefore the fold's own record of what it wrote, which is the only reading that can see an unbound slot at all. Neither block is reset: `SKRuntimeEffectUniforms.Reset` re-creates the uniform `SKData` and abandons the previous one undisposed, so a per-frame reset trades one native allocation per program per frame for a staleness total coverage already forecloses. The typed cell exists because both `Add` members THROW rather than converting — an unknown name and a value whose data type the declaration cannot take are both `ArgumentOutOfRangeException` — so the cell's case and the row's declared shape resolve the pair on the rail and the throwing arms are unreachable rather than trapped. A frame carries no time: motion advances the values a frame BINDS, and the clock that advances them belongs to `compose#CUSTOM_VISUAL_TICK`, so a shader cannot read a wall clock and drift from the animation driving it.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// One typed cell per uniform kind, each owning the write its own case admits. Both native `Add` members throw
// rather than converting — an undeclared name and a value whose data type the declaration cannot take are the
// two axes — so the arm tests the row's declared shape and refuses on the rail, which is what leaves a scalar
// aimed at a float2 slot a named fault instead of an exception raised inside a draw.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UniformValue {
    private UniformValue() { }
    public sealed record Scalar(float Value) : UniformValue;
    public sealed record Extent(SKSize Value) : UniformValue;
    public sealed record Pigment(SKColorF Value) : UniformValue;
    public sealed record Child(SKShader Value) : UniformValue;

    // `declared` is the row's answer for this cell's name: a shape for a uniform slot, absent for a child slot.
    // The child arm is therefore total against the split — a shader aimed at a uniform name and a value aimed
    // at a child name both refuse before the block sees either.
    public Fin<Unit> Bind(SKRuntimeShaderBuilder builder, string name, Option<UniformShape> declared) => Switch(
        state: (Builder: builder, Name: name, Declared: declared),
        scalar: static (s, cell) => Shaped(s, UniformShape.Scalar, () => s.Builder.Uniforms.Add(s.Name, cell.Value)),
        extent: static (s, cell) => Shaped(s, UniformShape.Extent,
            () => s.Builder.Uniforms.Add(s.Name, new SKSize(cell.Value.Width, cell.Value.Height))),
        pigment: static (s, cell) => Shaped(s, UniformShape.Pigment, () => s.Builder.Uniforms.Add(s.Name, cell.Value)),
        child: static (s, cell) => s.Declared.IsSome
            ? Fin.Fail<Unit>(new EffectFault.ChildUndeclared($"{s.Name} is a declared uniform, not a child slot"))
            : Set(() => s.Builder.Children.Add(s.Name, cell.Value)));

    static Fin<Unit> Shaped(
        (SKRuntimeShaderBuilder Builder, string Name, Option<UniformShape> Declared) seat,
        UniformShape carried, Action write) =>
        seat.Declared.Match(Some: shape => shape == carried, None: () => false)
            ? Set(write)
            : Fin.Fail<Unit>(new EffectFault.UniformUndeclared(
                $"{seat.Name}: a {carried.Key} value against a {seat.Declared} declaration"));

    // The one write lift on this rail, so a native mutation is a value on the fold rather than a statement.
    static Fin<Unit> Set(Action write) {
        write();
        return Fin.Succ(unit);
    }
}

// The per-frame binding value. Neither block is reset: `Uniforms.Reset` re-creates the uniform data and
// abandons the previous `SKData` undisposed, and total coverage is the stronger guarantee anyway — every
// declared slot is written on every bind, so nothing can be inherited and nothing can be read unwritten. The
// frame carries the caller's cells alone; the extent it is drawing at and the row's seeded procedural children
// seat inside the fold, so a consumer never names a slot whose source the estate already ships.
public sealed record UniformFrame(SKSize Extent, Seq<(string Name, UniformValue Value)> Cells) {
    public static UniformFrame Of(SKSize extent, params (string Name, UniformValue Value)[] cells) =>
        new(extent, toSeq(cells));

    // Resolve, write, record — one pass. The recorded name set is what the coverage gate reads, because the
    // block itself cannot answer which of its declared names carry a value.
    public Fin<SKRuntimeShaderBuilder> Bind(EffectProgram program) =>
        Seated(program)
            .Fold(Fin.Succ(Seq<string>()), (state, cell) => state.Bind(bound =>
                program.Row.Slot(cell.Name)
                    .Bind(shape => cell.Value.Bind(program.Builder, cell.Name, shape))
                    .Map(_ => bound.Add(cell.Name))))
            .Bind(bound => Covered(program, bound));

    // The caller's cells, the extent where the row declares one, and the row's seeded procedural children as
    // ONE cell stream, so the resolution, the write, and the coverage gate each read one shape. The extent seat
    // is CONDITIONAL because an undeclared name is a refusal here rather than a silently dropped cell, and the
    // grain program declares no extent at all.
    Seq<(string Name, UniformValue Value)> Seated(EffectProgram program) =>
        (program.Row.Uniforms.Exists(static row => row.Name == EffectRow.ExtentName)
            ? Cells.Add((EffectRow.ExtentName, (UniformValue)new UniformValue.Extent(Extent)))
            : Cells)
        + toSeq(program.Seeded).Map(static seed =>
            (Name: seed.Key, Value: (UniformValue)new UniformValue.Child(seed.Value)));

    // Coverage is a VALUE, not a hope, and it is what retires the per-frame reset: a declared name the frame
    // never bound would leave the block holding whatever the last successful bind wrote there, so the fold
    // names every absent uniform and every absent child on the typed rail before a shader is built.
    static Fin<SKRuntimeShaderBuilder> Covered(EffectProgram program, Seq<string> bound) =>
        program.Row.Uniforms.Filter(row => !bound.Contains(row.Name)) switch {
            { IsEmpty: true } => program.Row.Children.Filter(name => !bound.Contains(name)) switch {
                { IsEmpty: true } => Fin.Succ(program.Builder),
                var absent => Fin.Fail<SKRuntimeShaderBuilder>(new EffectFault.ChildUndeclared(
                    $"{program.Row.Key}: unbound {string.Join(", ", absent)}")),
            },
            var absent => Fin.Fail<SKRuntimeShaderBuilder>(new EffectFault.UniformUndeclared(
                $"{program.Row.Key}: unbound {string.Join(", ", absent.Map(static row => row.Name))}")),
        };
}
```

| [INDEX] | [PROGRAM] | [MOVING_UNIFORM] | [CHILD_SLOT]               | [CONSUMER]                                  |
| :-----: | :-------- | :--------------- | :------------------------- | :------------------------------------------ |
|  [01]   | `glow`    | `intensity`      | `content`, caller-supplied | focus and call-to-action emphasis           |
|  [02]   | `grain`   | `weight`         | `noise`, the shared field  | the `material#MATERIAL_EXECUTION` grain lay |
|  [03]   | `refract` | `coarse`         | `field`, the shared field  | the `material#FILTER_ROWS` refraction row   |
|  [04]   | `wash`    | `angle`          | none                       | the `material#MATERIAL_EXECUTION` wash lane |
|  [05]   | `sheen`   | `phase`          | `content`, caller-supplied | indeterminate progress and skeleton states  |

## [04]-[PATH_ROWS]

- Owner: `PathRow` `[Union]` the per-draw stroked-geometry pattern family.
- Cases: Dash | Trim | Stamp | Lattice | Rule.
- Law: a row lands here only when its geometry MOVES per draw or per frame — a marching selection phase, a leader trim drawing on, a stamped run advancing along its contour. A pattern whose intervals hold for a whole theme generation belongs to the frozen `FxRow.Dashes` catalogue at its capture-side owner, and a pattern whose intervals derive from a resolved stroke width belongs to the mark geometry at `Charts/custom#SKIA_KINDS`; minting either here would rebuild a native per frame for a value that never moved and would put a second dash owner beside the two the estate already carries.
- Entry: `public Fin<FxEffect> Build()` — the one native mint, producing the capture-side pathing case every consumer already binds; `public PathRow AtPhase(UnitInterval progress)` — the one per-frame advance the render-thread tick reads.
- Auto: a pattern is GEOMETRY applied to a stroke, so every row binds the `SKPaint.PathEffect` slot and none of them reaches the shader or colour slots — a pattern spelled as a shader tiles the FILL and leaves the stroke smooth, which reads as a solid line over a patterned interior; the phase advance derives each row's own moving term from one normalized progress, so a marching run, a stamped run, and a trim draw-on all read the single tick `compose#CUSTOM_VISUAL_TICK` publishes.
- Packages: SkiaSharp, Rasm (project — `UnitInterval`), LanguageExt.Core
- Growth: a new pattern is one `PathRow` case with its native arm and its phase arm; zero new surface.
- Boundary: a dash interval run is a positive-length ALTERNATION, so an empty or non-positive run refuses rather than producing a null effect that silently strokes solid; a trim whose start equals its stop yields an empty contour, which is a legitimate zero-progress state and admits, while a start past its stop is a degenerate range and refuses. The phase advance is CYCLE-relative rather than absolute: a dash advances by one full interval period per unit of progress and a stamp by one glyph advance, so a marching run reads as continuous at every interval length and a re-timed token changes the speed without re-authoring the pattern. The lattice and rule rows take their tiling from an `SKMatrix`, so pattern spacing and rotation are one transform rather than a spacing knob beside an angle knob — the pair drifts the moment either moves, and neither row carries a phase because a tiling advance is the matrix's own translation. The stamped row's advance is contour-space, not device-space, so a stamp on a scaled canvas keeps its spacing relative to the geometry it decorates rather than to the pixels beneath it.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

// Per-draw stroked-geometry patterns. Every row binds the PathEffect slot: a pattern spelled as a shader tiles
// the fill and leaves the stroke smooth, which is a defect that renders as a plausible picture. Every case
// carries its parameters as ROW DATA and mints its native at Build, because each one's geometry moves — a
// marching phase per frame, a trim stop as a leader draws on, a stamp advancing along its contour. A pattern
// fixed for a whole generation is `FxRow.Dashes` at the capture-side owner and a pattern derived from a
// resolved stroke width is the mark geometry at `Charts/custom#SKIA_KINDS`; neither is spelled here.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathRow {
    private PathRow() { }
    public sealed record Dash(Seq<float> Intervals, float Phase) : PathRow;
    public sealed record Trim(UnitInterval Start, UnitInterval Stop, SKTrimPathEffectMode Mode) : PathRow;
    public sealed record Stamp(SKPath Glyph, float Advance, float Phase, SKPath1DPathEffectStyle Style) : PathRow;
    public sealed record Lattice(SKMatrix Tiling, SKPath Cell) : PathRow;
    public sealed record Rule(float Width, SKMatrix Tiling) : PathRow;

    // The one per-frame read, CYCLE-relative in every arm: a dash walks one full interval period and a stamp
    // one glyph advance per unit of progress, so the run reads as continuous whatever the pattern measures and
    // a re-timed token changes the speed rather than the geometry. A trim draws on FROM its own start toward
    // the end, so the advance spans the remaining range and a nonzero start can never invert its own contour.
    // The tiled rows advance through their matrix and hold.
    public PathRow AtPhase(UnitInterval progress) => Switch(
        state: progress.Value,
        dash: static (p, row) => (PathRow)(row with { Phase = (float)p * row.Intervals.Fold(0f, static (acc, span) => acc + span) }),
        trim: static (p, row) => row with {
            Stop = UnitInterval.Create(row.Start.Value + (p * (1d - row.Start.Value))),
        },
        stamp: static (p, row) => row with { Phase = (float)p * row.Advance },
        lattice: static (_, row) => row,
        rule: static (_, row) => row);

    public Fin<FxEffect> Build() => Switch(
        dash: static row => row.Intervals.Count >= 2 && row.Intervals.ForAll(static span => span > 0f)
            ? Fin.Succ<FxEffect>(new FxEffect.Pathing(SKPathEffect.CreateDash([.. row.Intervals], row.Phase)))
            : Fin.Fail<FxEffect>(new EffectFault.PatternDegenerate($"dash {row.Intervals}")),
        // Start equal to stop is a legitimate zero-progress state and admits as an empty contour; start PAST
        // stop is a range Skia cannot walk, so it refuses here rather than stroking nothing silently.
        trim: static row => row.Start.Value <= row.Stop.Value
            ? Fin.Succ<FxEffect>(new FxEffect.Pathing(
                SKPathEffect.CreateTrim((float)row.Start.Value, (float)row.Stop.Value, row.Mode)))
            : Fin.Fail<FxEffect>(new EffectFault.PatternDegenerate($"trim {row.Start.Value}>{row.Stop.Value}")),
        // Advance is CONTOUR-space: a stamped pattern keeps its spacing relative to the geometry it decorates
        // rather than to the device pixels beneath it, so a zoomed canvas neither crowds nor thins it.
        stamp: static row => row.Advance > 0f
            ? Fin.Succ<FxEffect>(new FxEffect.Pathing(
                SKPathEffect.Create1DPath(row.Glyph, row.Advance, row.Phase, row.Style)))
            : Fin.Fail<FxEffect>(new EffectFault.PatternDegenerate($"stamp advance {row.Advance}")),
        lattice: static row => Fin.Succ<FxEffect>(new FxEffect.Pathing(SKPathEffect.Create2DPath(row.Tiling, row.Cell))),
        rule: static row => row.Width > 0f
            ? Fin.Succ<FxEffect>(new FxEffect.Pathing(SKPathEffect.Create2DLine(row.Width, row.Tiling)))
            : Fin.Fail<FxEffect>(new EffectFault.PatternDegenerate($"rule width {row.Width}")));
}
```

## [05]-[TILE_CACHE]

- Owner: `TileKey` the cache coordinate; `TileCell` the recorded tile beside its cost; `TileState` the whole cache as one value; `TileCache` the one budgeted owner.
- Law: every retained tile carries a BYTE cost and a generation stamp — `SKPicture.ApproximateBytesUsed` is the retained-op measure admission reads, a handle count is not a budget because one full-surface record outweighs a thousand hatch cells, and eviction never releases a cell at or above the live generation, so a budgeted cache cannot free a tile the current draw is replaying. The map, the touch clock, the resident total, and each transition's own verdict are ONE cell, because a byte total re-summed beside the map it was taken from is the reading that silently disagrees with it.
- Entry: `public Fin<TileHit> Tile(TileKey key, long generation, SKRect cull, Func<SKCanvas, Fin<Unit>> record)` — the one admission-and-read, returning the shader beside the receipt that describes how it was obtained; `public static TileCache Of(long ceilingBytes, ClockPolicy clocks)` — the mint composition binds.
- Auto: a hatch pattern, a checker backplate, and a recorded wash all replay from one sealed op list instead of re-running their layout per frame; a tile projects to a shader through `SKPicture.ToShader`, so the same record serves a fill without a second raster.
- Receipt: `TileReceipt` — key, generation, recorded bytes, resident bytes, admit-or-reuse-or-refuse, evicted count, `Instant` — sealed under the evidence union's `Effect` case beside the material receipt.
- Packages: SkiaSharp, NodaTime, LanguageExt.Core
- Growth: a new tiled surface is one `TileKey` value; zero new surface.
- Boundary: a tile is a device-INDEPENDENT op list, not pixels, so one record serves every scale and a scale change re-plays rather than re-records — which is exactly why the cost measure is the op-list byte count and not a pixel area. A record exceeding the whole ceiling refuses at admission rather than evicting the entire cache to seat one tile, because a single oversized record would otherwise flush every live cell and then still not fit. Eviction is GUARDED and therefore fallible: a cache whose every cell belongs to the live generation frees nothing, so a record that fits the ceiling in isolation still refuses as `BudgetExhausted` when the freed total leaves the cache over its bound — and eviction is part of the SAME transition as the seat, so a refused admission releases nothing at all rather than emptying the cache for a tile it then declines. `Atom.Swap` re-runs its function under CAS contention, so the transition is pure and answers its own victims on the state it installs — the winning caller therefore owns exactly the cells its own winning pass unlinked, where a dispose inside the swap runs once per retry over cells a competing pass already released. `SKPicture.ToShader` retains the picture, so the pair mints before the transition and the cell owns both and releases them in ownership order; every refusal path releases the pair it built rather than leaking one picture per breach, and a caller never holds a tile past its own draw.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------

public readonly record struct TileKey(string Row, int CellPx, string Variant) {
    public override string ToString() => $"{Row}/{CellPx}/{Variant}";
}

// A recorded tile beside its retained cost and the generation that admitted it. Touch is the least-recently-
// used coordinate; Generation is the eviction guard, because a cell at the live generation is one the current
// draw may still replay.
public sealed record TileCell(SKPicture Picture, SKShader Shader, long Bytes, long Generation, long Touch) : IDisposable {
    // Release order is ownership order: the shader samples the picture, so it drops first.
    public void Dispose() {
        Shader.Dispose();
        Picture.Dispose();
    }
}

// The shader beside the receipt that describes how it was obtained. The cache owns every counter — resident
// bytes, eviction count, and the admit/reuse/refuse verdict all move together inside one swap — so a
// caller-assembled receipt would read three cells across a race the cache itself owns.
public readonly record struct TileHit(SKShader Shader, TileReceipt Receipt);

// The whole cache as ONE value: the seated map, the touch clock, the resident byte total, and the verdict and
// victims of the transition that produced it. Separate cells would need separate CAS windows to agree, and a
// byte total re-summed per read is the reading that silently disagrees with the map it was taken from.
// `Atom.Swap` re-runs its function under contention, so every transition here is PURE and answers its victims
// on the state it installs — a losing attempt recomputes against the winner's map and selects again, which is
// what makes the caller that reads the installed state the sole owner of exactly the cells it unlinked.
readonly record struct TileState(
    HashMap<TileKey, TileCell> Cells, long Clock, long Bytes, Seq<TileCell> Retired, bool Admitted) {
    public static readonly TileState Empty =
        new(HashMap<TileKey, TileCell>(), Clock: 0L, Bytes: 0L, Seq<TileCell>(), Admitted: false);

    // Touch on read is what makes the eviction order mean anything: a tile replayed every frame and a tile
    // recorded once and never read again are otherwise indistinguishable at the moment the ceiling binds.
    public TileState Touched(TileKey key) =>
        Cells.Find(key).Match(
            Some: cell => this with {
                Cells = Cells.AddOrUpdate(key, cell with { Touch = Clock + 1L }),
                Clock = Clock + 1L,
                Retired = Seq<TileCell>(),
                Admitted = false,
            },
            None: () => this with { Retired = Seq<TileCell>(), Admitted = false });

    // Evict and seat as ONE transition. The seat happens only where the freed total actually leaves room, and a
    // refusal unlinks nothing — evicting first and refusing after would empty the cache for a tile it declines.
    // A negative shortfall satisfies the fold immediately, so the no-eviction case falls out of the same body.
    public TileState Seated(TileKey key, TileCell incoming, long ceiling) =>
        Victims(Bytes + incoming.Bytes - ceiling, incoming.Generation) switch {
            var victims when Bytes - victims.Freed + incoming.Bytes > ceiling =>
                this with { Retired = Seq<TileCell>(), Admitted = false },
            var victims => new TileState(
                Cells: victims.Taken
                    .Fold(Cells, static (map, victim) => map.Remove(victim.Key))
                    .AddOrUpdate(key, incoming with { Touch = Clock + 1L }),
                Clock: Clock + 1L,
                Bytes: Bytes - victims.Freed + incoming.Bytes,
                Retired: victims.Taken.Map(static victim => victim.Cell),
                Admitted: true),
        };

    // Least-recently-touched selection, guarded on generation: a cell at or above the live generation belongs
    // to a draw that may still replay it, so freeing it would sample a disposed op list mid-frame — which is
    // also why a cell being reused is never a victim, since reuse demands an exact generation match. The fold
    // stops the moment the shortfall is covered, so a breach takes the least-recently-replayed tiles and
    // nothing beyond them. Ordering leaves the carrier, so the ordered run re-enters it before the fold reads a
    // carrier-generic member.
    (long Freed, Seq<(TileKey Key, TileCell Cell)> Taken) Victims(long shortfall, long generation) =>
        toSeq(toSeq(Cells)
                .Filter(pair => pair.Value.Generation < generation)
                .OrderBy(static pair => pair.Value.Touch))
            .Fold((Freed: 0L, Taken: Seq<(TileKey Key, TileCell Cell)>()), (victims, pair) =>
                victims.Freed >= shortfall
                    ? victims
                    : (victims.Freed + pair.Value.Bytes, victims.Taken.Add((pair.Key, pair.Value))));
}

// The one budgeted tile owner. The ceiling is BYTES and the measure is the op list's own retained cost, so a
// full-surface record and a hatch cell are comparable; a handle count would rank them identically.
public sealed class TileCache {
    readonly Atom<TileState> state = Atom(TileState.Empty);
    readonly ClockPolicy clocks;
    readonly long ceiling;

    TileCache(long ceilingBytes, ClockPolicy clocks) => (ceiling, this.clocks) = (ceilingBytes, clocks);

    public static TileCache Of(long ceilingBytes, ClockPolicy clocks) => new(ceilingBytes, clocks);

    public long Resident => state.Value.Bytes;

    // A cell admitted under an older generation is stale by construction — the theme re-resolved and its
    // pigments moved — so a generation mismatch re-records rather than replaying a tile drawn in the old
    // palette, and the stale cell falls out through the eviction guard rather than through a second sweep.
    public Fin<TileHit> Tile(TileKey key, long generation, SKRect cull, Func<SKCanvas, Fin<Unit>> record) =>
        state.Value.Cells.Find(key).Filter(cell => cell.Generation == generation).Match(
            Some: cell => Fin.Succ(Reuse(key, cell, generation)),
            None: () => Admit(key, generation, cull, record));

    TileHit Reuse(TileKey key, TileCell hit, long generation) =>
        state.Swap(current => current.Touched(key)) switch {
            var next => new TileHit(hit.Shader, Seal(key, generation, hit.Bytes, next, "reuse", evicted: 0)),
        };

    // Record, measure, then admit — the retained cost is knowable only after the op list seals, so the ceiling
    // test runs against a real measure rather than an estimate. A record larger than the whole ceiling refuses
    // outright: evicting every live cell for it would empty the cache and still not seat it. A record fold that
    // fails never reaches EndRecording, so the recorder's own scope is the whole release the refusal owes.
    Fin<TileHit> Admit(TileKey key, long generation, SKRect cull, Func<SKCanvas, Fin<Unit>> record) {
        using SKPictureRecorder recorder = new();
        return record(recorder.BeginRecording(cull))
            .Map(_ => recorder.EndRecording())
            .Bind(picture => (long)picture.ApproximateBytesUsed switch {
                var bytes when bytes > ceiling => Refuse(picture, None, new EffectFault.TileOversize(
                    $"{key}: {bytes} bytes over a {ceiling} ceiling")),
                var bytes => Seat(key, generation, picture, bytes),
            });
    }

    // ToShader RETAINS the picture, so the pair mints ahead of the transition — the transition is a pure
    // function that may run several times and can mint nothing — and the installed state answers both the
    // verdict and the victims this pass unlinked, which release once, here, outside the swap.
    Fin<TileHit> Seat(TileKey key, long generation, SKPicture picture, long bytes) {
        SKShader shader = picture.ToShader(SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
        TileState next = state.Swap(current =>
            current.Seated(key, new TileCell(picture, shader, bytes, generation, Touch: 0L), ceiling));
        next.Retired.Iter(static cell => cell.Dispose());
        return next.Admitted
            ? Fin.Succ(new TileHit(shader, Seal(key, generation, bytes, next, "admit", next.Retired.Count)))
            : Refuse(picture, Some(shader), new EffectFault.BudgetExhausted(
                $"{key}: {bytes} bytes against a {ceiling} ceiling holding {next.Bytes} unreleasable resident"));
    }

    // Every refusal releases what it built: a measured op list the cache declines to seat has no other owner,
    // and the shader minted over it holds the picture alive, so the pair drops in ownership order.
    static Fin<TileHit> Refuse(SKPicture picture, Option<SKShader> shader, EffectFault fault) {
        shader.Iter(static handle => handle.Dispose());
        picture.Dispose();
        return Fin.Fail<TileHit>(fault);
    }

    // The resident figure comes off the state the transition INSTALLED, so the receipt and the cache cannot
    // disagree about the total a second read would have taken after another pass moved it.
    TileReceipt Seal(TileKey key, long generation, long bytes, TileState at, string outcome, int evicted) =>
        new(key.ToString(), generation, bytes.ToString(CultureInfo.InvariantCulture),
            at.Bytes.ToString(CultureInfo.InvariantCulture), outcome, evicted, clocks.Now);
}

public readonly record struct TileReceipt(
    string Key, long Generation, string RecordedBytes, string ResidentBytes, string Outcome, int Evicted, Instant At) {
    public EvidenceReceipt ToEvidence() => new EvidenceReceipt.Effect(
        Plane: "tile", Key: Key, Outcome: Outcome,
        Flag: false, Count: Evicted, Magnitude: ResidentBytes);
}
```

## [06]-[RESEARCH]

(none)

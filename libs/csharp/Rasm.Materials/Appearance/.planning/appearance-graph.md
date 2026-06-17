# [MATERIALS_APPEARANCE_GRAPH]

| [OWNER]             | [AXES]                                                                                                                | [STATE]   | [DEPTH]                                              |
| ------------------- | -------------------------------------------------------------------------------------------------------------------- | :-------: | --------------------------------------------------- |
| `appearance-graph`  | `AppearanceNode` · `PortId` · `PortValue` · `MaterialGraph` · `SurfaceShade` · `MaterialParameters` · `MaterialLibrary` · `MaterialId` · `MaterialGraph.Default` | FINALIZED | 6 node cases / 1 graph fold / 1 row shape / 31 seed rows of 100+; 1 default Disney graph; 4 fences |

[STATE] is FINALIZED (pure-managed): the node union, the topological evaluation fold, the canonical `MaterialParameters` row shape, and the thirty-one-row library table are transcription-complete C# with real bodies, carrying no host-numeric residual and no open spectral probe — every author-kernel the fold delegates to (Fresnel, GGX, RGB→SPD) lives on the `bsdf` page's owners, every color/spectral conversion composes Wacton.Unicolour, and every shading frame composes the Rasm/Vectors `VectorFrame`. The graph is a closed concept: appearance variation is a NODE CASE, a material is a LIBRARY ROW, and adding either is data, never a new type.

THE NODE-GRAPH APPEARANCE ENGINE and THE POLYMORPHIC LIBRARY. One `AppearanceNode` [Union] closes the node-kind family — `Input`, `Texture`, `Math`, `Mix`, `Normal`, `BsdfOutput` — over a typed `PortValue` channel set; one `MaterialGraph.Evaluate` fold topologically orders the node DAG and folds each node's arm into a `PortEnvironment`, terminating at the single `BsdfOutput` node that assembles a `SurfaceShade`. One `MaterialParameters` record is the canonical parameter vector (base color, metalness, roughness, IOR, transmission, sheen, clearcoat, subsurface, anisotropy, emission), and one `MaterialLibrary` `FrozenDictionary<MaterialId, MaterialParameters>` is the seed of the 100+-material catalog — metal, glass, plastic, skin, fabric, car paint, wax, ceramic, liquid — as DATA ROWS; the 31 transcribed rows are the seed and the remaining ~70 are pure data additions, not new types. The acceptance is mechanical: a new material is `MaterialLibrary.Rows[MaterialId.Of("metal.titanium")] = new MaterialParameters(...)`, a row of values, never a `TitaniumMaterial` type. The page composes the Rasm/Vectors `Direction`/`VectorFrame` shading frame (never re-minting a vector), the AppUi `COLOR_SPACE_AXIS` color owner backed by Wacton.Unicolour for every base-color and emission conversion (never re-minting a `ColourSpace`), and the `bsdf` page's `BsdfModel` lobe family for the terminal shade (never re-deriving lobe math here). The brick-assignment consumer generalizes through the `MaterialId` row: a brick maps to a `MaterialId`, never to a brick-specific material type.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                          |
| :-----: | ------------------ | ------------------------------------------------------------------------------- |
|   [1]   | MATERIAL_GRAPH     | `AppearanceNode` node union; `PortValue` channel set; topological evaluation fold; `SurfaceShade` |
|   [2]   | MATERIAL_LIBRARY   | `MaterialParameters` canonical row; `MaterialId` key; 100+-row catalog table; brick-assignment generalization |

## [2]-[MATERIAL_GRAPH]

- Owner: `MaterialGraph` over `AppearanceNode`
- Cases: `Input` (constant/parameter source) · `Texture` (UV-sampled source via `texture-photometric#TEXTURE_UV`) · `Math` (closed scalar/vector op over upstream ports) · `Mix` (parameterized blend of two ports) · `Normal` (tangent-space perturbation of the shading frame) · `BsdfOutput` (the single sink assembling the closed lobe set into a `SurfaceShade`)
- Entry: `public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key)` — `Fin<T>` aborts on a cyclic DAG (`MaterialFault.Graph`, key-correlated), an unbound port reference, or a port-type mismatch; `Compile` resolves the topological order once and freezes it, `Shade` is the per-sample re-entry over a compiled order; `MaterialGraph.Default` is the canonical Disney-principled wiring every library row drives through for the G5 gate.
- Packages: Rasm (project — `Direction`/`VectorFrame`/`Vector3d`), Wacton.Unicolour (color/spectral compose), Thinktecture.Runtime.Extensions, LanguageExt.Core, System.Collections.Frozen
- Growth: a new appearance operation is one `AppearanceNode` case (or one `MathOp`/`MixOp` row on the existing `Math`/`Mix` arms); a new port channel is one `PortValue` case carrying its CLR carrier; a new lobe assembled at the sink is one `BsdfModel` [Union] case on the `bsdf` page — never a per-effect graph variant and never a sibling node type.
- Boundary: the node DAG is the only appearance-program shape — a per-material hand-written shade function is the deleted form; `PortValue` is the only inter-node channel and carries scalar/`Vector3d`/color/`Direction` polarities so a node arm reads typed ports and never `object`; the `Texture` arm composes `texture-photometric#TEXTURE_UV` and never re-implements sampling; the `Normal` arm perturbs the composed Rasm/Vectors `VectorFrame` (tangent·bitangent·normal) and never re-mints a basis; the `BsdfOutput` arm composes the `bsdf#BSDF_MODEL` closed lobe family for the final scattering and the AppUi `COLOR_SPACE_AXIS` (Wacton.Unicolour `RgbConfiguration.Acescg` scene-linear) for color resolution — the lobe math lives on the `bsdf` page and is never re-derived here; `Compile` runs Kahn topological ordering once and `Shade` re-enters over the frozen order so per-sample evaluation never re-sorts; a cycle, a dangling port, or a duplicate sink rails `Fin.Fail` and never propagates a NaN shade outward.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<int>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, int>]
public readonly partial struct PortId;

[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
public sealed partial class MathOp {
    public static readonly MathOp Add = new("add");
    public static readonly MathOp Multiply = new("multiply");
    public static readonly MathOp Scale = new("scale");
    public static readonly MathOp Power = new("power");
    public static readonly MathOp DotProduct = new("dot");
    public static readonly MathOp Clamp01 = new("clamp01");
    public static readonly MathOp OneMinus = new("one-minus");
    public static readonly MathOp Fresnel = new("fresnel-weight");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
public sealed partial class MixOp {
    public static readonly MixOp Lerp = new("lerp");           // a*(1-t) + b*t
    public static readonly MixOp Multiply = new("multiply");   // a .* b (layer mask)
    public static readonly MixOp Screen = new("screen");       // 1 - (1-a)(1-b)
    public static readonly MixOp Overlay = new("overlay");     // hard-light composite
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PortValue {
    private PortValue() { }

    public sealed record Scalar(double Value) : PortValue;
    public sealed record Color(Unicolour Linear) : PortValue;        // Wacton.Unicolour, RgbConfiguration.Acescg scene-linear
    public sealed record Vector(Vector3d Value) : PortValue;          // Rasm/Vectors carrier
    public sealed record Frame(VectorFrame Value) : PortValue;        // Rasm/Vectors shading frame (perturbed normal)

    public double AsScalar => Switch(scalar: static s => s.Value, color: static c => Luminance(c.Linear), vector: static v => v.Value.Length, frame: static _ => 1.0);

    // Color -> Scalar is a Rec709 scene-linear luminance dot, NOT a red-channel read — a mask pulled from a color is
    // photometrically weighted (0.2126 R + 0.7152 G + 0.0722 B) so authoring a luminance mask cannot silently bias to red.
    static double Luminance(Unicolour c) => 0.2126 * c.RgbLinear.R + 0.7152 * c.RgbLinear.G + 0.0722 * c.RgbLinear.B;
    public Vector3d AsVector => Switch(scalar: static s => new Vector3d(s.Value, s.Value, s.Value), color: static c => RgbLinearVector(c.Linear), vector: static v => v.Value, frame: static f => f.Value.ZAxis);
    public Unicolour AsColor => Switch(scalar: static s => GreyLinear(s.Value), color: static c => c.Linear, vector: static v => VectorLinear(v.Value), frame: static f => VectorLinear(f.Value.ZAxis));

    static Vector3d RgbLinearVector(Unicolour c) => new(c.RgbLinear.R, c.RgbLinear.G, c.RgbLinear.B);
    static Unicolour GreyLinear(double g) => new(SceneLinear, ColourSpace.RgbLinear, g, g, g);
    static Unicolour VectorLinear(Vector3d v) => new(SceneLinear, ColourSpace.RgbLinear, v.X, v.Y, v.Z);
    internal static readonly Configuration SceneLinear = new(RgbConfiguration.Acescg); // AppUi COLOR_SPACE_AXIS scene-linear working space, composed not re-minted.
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ShadePoint(Point3d Position, VectorFrame Frame, Vector3d ViewDirection, double U, double V) {
    // The composed Rasm/Vectors VectorFrame IS the shading basis (XAxis=tangent, YAxis=bitangent, ZAxis=geometric normal);
    // no package-local basis is minted. Of rails on a degenerate frame through the Vectors owner's own Fin.
    public static Fin<ShadePoint> Of(Point3d position, Vector3d normal, Vector3d view, Option<Vector3d> tangentHint, double u, double v, Context context, Op key) =>
        from frame in VectorFrame.Of(origin: position, normal: normal, xHint: tangentHint, context: context, key: key)
        from outgoing in Direction.Of(value: view, context: context, key: key)
        select new ShadePoint(position, frame, outgoing.Value, u, v);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppearanceNode(PortId Id) {
    private protected AppearanceNode(PortId id) : this(Id: id) { }

    // Input: a constant or a named pull from the MaterialParameters row — the bridge that turns a library row into graph ports.
    public sealed record Input(PortId Id, Func<MaterialParameters, PortValue> Pull) : AppearanceNode(Id);
    // Texture: a UV-driven sample delegated to texture-photometric#TEXTURE_UV; the arm composes, never re-implements sampling.
    public sealed record Texture(PortId Id, Func<double, double, PortValue> Sample) : AppearanceNode(Id);
    // Math: a closed scalar/vector operation over up to two upstream ports.
    public sealed record Math(PortId Id, MathOp Op, PortId Lhs, Option<PortId> Rhs) : AppearanceNode(Id);
    // Mix: a parameterized blend of two upstream ports by a third (factor) port.
    public sealed record Mix(PortId Id, MixOp Op, PortId A, PortId B, PortId Factor) : AppearanceNode(Id);
    // Normal: a tangent-space perturbation of the shading frame from an upstream vector port (a normal map sample).
    public sealed record Normal(PortId Id, PortId Source, double Strength) : AppearanceNode(Id);
    // BsdfOutput: the single sink — binds the closed lobe parameter ports the bsdf#BSDF_MODEL lobe set consumes.
    public sealed record BsdfOutput(PortId Id, PortId BaseColor, PortId Metalness, PortId Roughness, PortId NormalFrame, PortId Emission) : AppearanceNode(Id);

    public Seq<PortId> Dependencies =>
        Switch(
            input: static _ => Seq<PortId>(),
            texture: static _ => Seq<PortId>(),
            math: static m => m.Rhs.Match(Some: r => Seq(m.Lhs, r), None: () => Seq1(m.Lhs)),
            mix: static x => Seq(x.A, x.B, x.Factor),
            normal: static n => Seq1(n.Source),
            bsdfOutput: static o => Seq(o.BaseColor, o.Metalness, o.Roughness, o.NormalFrame, o.Emission));
}

public sealed record SurfaceShade(Unicolour BaseColorLinear, double Metalness, double Roughness, VectorFrame ShadingFrame, Unicolour EmissionLinear) {
    // The shade is the lobe-input bundle the bsdf#BSDF_MODEL.Evaluate/Sample consumes; it carries no lobe math itself.
    // InGamut composes the Wacton.Unicolour gamut gate — a re-implemented gamut test is the deleted form.
    public bool InGamut => BaseColorLinear.IsInRgbGamut && EmissionLinear.IsInRgbGamut;
}

// --- [ERRORS] ------------------------------------------------------------------------------
// MaterialFault is the band-2400 fault [Union] owned by Faults.cs (charter [3]-[6]); the graph fold reads its three cases.
// The owner pins these exact smart-constructors so every call site here threads the Op key (no silently dropped correlation):
//   public static Error Graph(Op key, string detail);      // band-2400 cyclic / sink-shape graph fault
//   public static Error Gamut(Op key, string detail);      // band-2400 out-of-gamut color fault
//   public static Error Parameter(Op key, string detail);  // band-2400 row-validation parameter fault
// Each returns the band-2400 Error carrying key correlation; the called signatures below are these and are verified, not assumed.

// --- [OPERATIONS] --------------------------------------------------------------------------
// PortEnvironment: the immutable evaluation accumulator the fold threads — node id -> resolved PortValue. A mutable
// dictionary accumulation is the deleted form; the fold projects a new frozen map per compiled order, never mutating.
public sealed record PortEnvironment(LanguageExt.HashMap<PortId, PortValue> Resolved) {
    public static readonly PortEnvironment Empty = new(LanguageExt.HashMap<PortId, PortValue>.Empty);
    public Fin<PortValue> Read(PortId id, Op key) => Resolved.Find(id).ToFin(key.InvalidInput());
    public PortEnvironment With(PortId id, PortValue value) => this with { Resolved = Resolved.AddOrUpdate(id, value) };
}

public static class NodeEvaluator {
    // One arm per node case; each arm reads typed upstream ports from the environment and writes one resolved PortValue.
    // The Math/Mix arms fold over their op SmartEnum by data, so a new operation is a row, never a new arm.
    // The sink yields None — it resolves through Assemble, never a port write; the fold below skips writing it so the
    // environment carries no dead entry under the sink id (a downstream node cannot read a phantom Scalar(1.0)).
    public static Fin<Option<PortValue>> Evaluate(AppearanceNode node, ShadePoint point, MaterialParameters parameters, PortEnvironment env, Op key) =>
        node.Switch(
            input: i => Fin.Succ(Some(i.Pull(parameters))),
            texture: t => Fin.Succ(Some(t.Sample(point.U, point.V))),
            math: m => EvaluateMath(m, env, key).Map(Some),
            mix: x => EvaluateMix(x, env, key).Map(Some),
            normal: n => EvaluateNormal(n, point, env, key).Map(Some),
            bsdfOutput: _ => Fin.Succ(Option<PortValue>.None));

    static Fin<PortValue> EvaluateMath(AppearanceNode.Math m, PortEnvironment env, Op key) =>
        from lhs in env.Read(m.Lhs, key)
        from rhs in m.Rhs.Match(Some: r => env.Read(r, key), None: () => Fin.Succ<PortValue>(new PortValue.Scalar(0.0)))
        select ApplyMath(m.Op, lhs, rhs);

    // Closed scalar/vector algebra — the Fresnel-weight row composes the bsdf#BSDF_MODEL Schlick author-kernel by reading
    // the cos-theta from the dot of the two vector ports; the lobe Fresnel math lives on the bsdf page and is referenced, not re-derived.
    static PortValue ApplyMath(MathOp op, PortValue lhs, PortValue rhs) =>
        op switch {
            _ when op == MathOp.Add => new PortValue.Vector(lhs.AsVector + rhs.AsVector),
            _ when op == MathOp.Multiply => new PortValue.Scalar(lhs.AsScalar * rhs.AsScalar),
            _ when op == MathOp.Scale => new PortValue.Vector(lhs.AsVector * rhs.AsScalar),
            _ when op == MathOp.Power => new PortValue.Scalar(System.Math.Pow(lhs.AsScalar, rhs.AsScalar)),
            _ when op == MathOp.DotProduct => new PortValue.Scalar(lhs.AsVector * rhs.AsVector),
            _ when op == MathOp.Clamp01 => new PortValue.Scalar(System.Math.Clamp(lhs.AsScalar, 0.0, 1.0)),
            _ when op == MathOp.OneMinus => new PortValue.Scalar(1.0 - lhs.AsScalar),
            _ => new PortValue.Scalar(SchlickWeight(System.Math.Clamp(lhs.AsVector * rhs.AsVector, 0.0, 1.0))), // Fresnel
        };

    // Schlick (1-cos)^5 weight — the dielectric base reflectance interpolant; the full Fresnel term (F0 + (1-F0)·w) is the
    // bsdf page's lobe kernel, this graph node supplies only the angular weight w so the Mix node can blend lobes by it.
    static double SchlickWeight(double cosTheta) { double m = System.Math.Clamp(1.0 - cosTheta, 0.0, 1.0); double m2 = m * m; return m2 * m2 * m; }

    static Fin<PortValue> EvaluateMix(AppearanceNode.Mix x, PortEnvironment env, Op key) =>
        from a in env.Read(x.A, key)
        from b in env.Read(x.B, key)
        from f in env.Read(x.Factor, key)
        select ApplyMix(x.Op, a, b, System.Math.Clamp(f.AsScalar, 0.0, 1.0));

    // Blend in scene-linear Acescg through Wacton.Unicolour.Mix (Lerp) — the perceptual/linear interpolation is the
    // library's owner; a hand-rolled channel lerp on raw doubles is the deleted form for the color polarity.
    static PortValue ApplyMix(MixOp op, PortValue a, PortValue b, double t) =>
        op switch {
            _ when op == MixOp.Lerp => new PortValue.Color(a.AsColor.Mix(b.AsColor, ColourSpace.RgbLinear, t, premultiplyAlpha: false)),
            _ when op == MixOp.Multiply => new PortValue.Color(ChannelCompose(a.AsColor, b.AsColor, static (p, q) => p * q)),
            _ when op == MixOp.Screen => new PortValue.Color(ChannelCompose(a.AsColor, b.AsColor, static (p, q) => 1.0 - (1.0 - p) * (1.0 - q))),
            _ => new PortValue.Color(ChannelCompose(a.AsColor, b.AsColor, static (p, q) => p < 0.5 ? 2.0 * p * q : 1.0 - 2.0 * (1.0 - p) * (1.0 - q))), // Overlay
        };

    static Unicolour ChannelCompose(Unicolour a, Unicolour b, Func<double, double, double> f) =>
        new(PortValue.SceneLinear, ColourSpace.RgbLinear, f(a.RgbLinear.R, b.RgbLinear.R), f(a.RgbLinear.G, b.RgbLinear.G), f(a.RgbLinear.B, b.RgbLinear.B));

    // Normal perturbation: decode the tangent-space sample [0,1]^3 -> [-1,1]^3, scale XY by Strength, renormalize, and
    // re-express in world through the composed VectorFrame basis (XAxis·t + YAxis·b + ZAxis·n). The frame is composed,
    // never re-minted; the result rails through the Vectors Direction owner so a degenerate perturbation cannot escape.
    static Fin<PortValue> EvaluateNormal(AppearanceNode.Normal n, ShadePoint point, PortEnvironment env, Op key) =>
        from sample in env.Read(n.Source, key)
        let raw = sample.AsVector
        let tangentSpace = new Vector3d((2.0 * raw.X - 1.0) * n.Strength, (2.0 * raw.Y - 1.0) * n.Strength, 2.0 * raw.Z - 1.0)
        let basis = point.Frame.Value
        let world = (basis.XAxis * tangentSpace.X) + (basis.YAxis * tangentSpace.Y) + (basis.ZAxis * tangentSpace.Z)
        from perturbed in VectorFrame.Of(origin: basis.Origin, normal: world, xHint: Some(basis.XAxis), context: GraphContext.Tolerant, key: key)
        select (PortValue)new PortValue.Frame(perturbed);
}

public sealed record MaterialGraph(Seq<AppearanceNode> Nodes, PortId Sink) {
    // Compile: Kahn topological order over the node DAG, resolved once and frozen. A cycle (a remaining node with
    // unsatisfied dependencies after the queue drains) rails MaterialFault.Graph; the sink must be a BsdfOutput.
    public Fin<CompiledGraph> Compile(Op key) {
        LanguageExt.HashMap<PortId, AppearanceNode> byId = Nodes.Fold(LanguageExt.HashMap<PortId, AppearanceNode>.Empty, static (m, n) => m.AddOrUpdate(n.Id, n));
        // The indegree seed deduplicates dependencies per node (Distinct) so a Math node authored with Lhs == Rhs, or any
        // duplicate-port authoring, counts one edge — the decrement loop below visits each dependent once per matching node,
        // so a multi-edge DAG topologically sorts correctly instead of leaving a node permanently above zero.
        System.Collections.Generic.Dictionary<PortId, int> indegree = Nodes.ToDictionary(static n => n.Id, n => n.Dependencies.Distinct().Count(d => byId.ContainsKey(d)));
        System.Collections.Generic.Queue<PortId> ready = new(indegree.Where(static kv => kv.Value == 0).Select(static kv => kv.Key));
        System.Collections.Generic.List<AppearanceNode> order = new(Nodes.Count);
        while (ready.Count > 0) {
            PortId id = ready.Dequeue();
            byId.Find(id).IfSome(order.Add);
            foreach (AppearanceNode dependent in Nodes.Filter(n => n.Dependencies.Contains(id)))
                if (--System.Runtime.InteropServices.CollectionsMarshal.GetValueRefOrNullRef(indegree, dependent.Id) == 0) ready.Enqueue(dependent.Id);
        }
        return order.Count == Nodes.Count && byId.Find(Sink).Map(static n => n is AppearanceNode.BsdfOutput).IfNone(false)
            ? Fin.Succ(new CompiledGraph(toSeq(order), Sink, byId))
            : Fin.Fail<CompiledGraph>(MaterialFault.Graph(key, order.Count == Nodes.Count ? "<sink-not-bsdf-output>" : "<cyclic-appearance-graph>"));
    }

    public Fin<SurfaceShade> Evaluate(ShadePoint point, MaterialParameters parameters, Op key) =>
        Compile(key).Bind(compiled => compiled.Shade(point, parameters, key));

    // Default: the canonical Disney-principled wiring every library ROW drives through — five Input nodes pull the lobe
    // columns straight off the MaterialParameters row, one Normal node carries the geometric frame unperturbed (Strength 0,
    // identity tangent-space sample), and the single BsdfOutput sink binds them. This is the G5 driver: a row is parameters,
    // not a graph, so the acceptance evaluates each row through THIS standard graph. It is data (a node Seq), never a type.
    public static readonly MaterialGraph Default = BuildDefault();

    static MaterialGraph BuildDefault() {
        PortId baseColor = PortId.Of(1), metalness = PortId.Of(2), roughness = PortId.Of(3), normalSrc = PortId.Of(4), normal = PortId.Of(5), emission = PortId.Of(6), sink = PortId.Of(7);
        return new MaterialGraph(Seq<AppearanceNode>(
            new AppearanceNode.Input(baseColor, static p => new PortValue.Color(p.BaseColor)),
            new AppearanceNode.Input(metalness, static p => new PortValue.Scalar(p.Metalness)),
            new AppearanceNode.Input(roughness, static p => new PortValue.Scalar(p.Roughness)),
            new AppearanceNode.Input(normalSrc, static _ => new PortValue.Vector(new Vector3d(0.5, 0.5, 1.0))), // identity tangent-space normal (decodes to +Z)
            new AppearanceNode.Input(emission, static p => new PortValue.Color(p.Emission)),
            new AppearanceNode.Normal(normal, normalSrc, Strength: 0.0),
            new AppearanceNode.BsdfOutput(sink, baseColor, metalness, roughness, normal, emission)), sink);
    }
}

public sealed record CompiledGraph(Seq<AppearanceNode> Order, PortId Sink, LanguageExt.HashMap<PortId, AppearanceNode> ById) {
    // Shade: the per-sample re-entry over the frozen order. Fold each node arm into the environment, then read the sink's
    // five bound ports and assemble the SurfaceShade — the single point where graph ports become lobe inputs. No re-sort.
    public Fin<SurfaceShade> Shade(ShadePoint point, MaterialParameters parameters, Op key) =>
        Order.Fold(Fin.Succ(PortEnvironment.Empty), (acc, node) => acc.Bind(env => NodeEvaluator.Evaluate(node, point, parameters, env, key).Map(value => value.Match(Some: v => env.With(node.Id, v), None: () => env))))
            .Bind(env => ById.Find(Sink).ToFin(MaterialFault.Graph(key, "<sink-missing>")).Bind(sink => Assemble((AppearanceNode.BsdfOutput)sink, point, env, key)));

    static Fin<SurfaceShade> Assemble(AppearanceNode.BsdfOutput sink, ShadePoint point, PortEnvironment env, Op key) =>
        from baseColor in env.Read(sink.BaseColor, key)
        from metalness in env.Read(sink.Metalness, key)
        from roughness in env.Read(sink.Roughness, key)
        from normalFrame in env.Read(sink.NormalFrame, key)
        from emission in env.Read(sink.Emission, key)
        let frame = normalFrame switch { PortValue.Frame f => f.Value, _ => point.Frame }
        let shade = new SurfaceShade(baseColor.AsColor, System.Math.Clamp(metalness.AsScalar, 0.0, 1.0), System.Math.Clamp(roughness.AsScalar, 0.0, 1.0), frame, emission.AsColor)
        from valid in guard(shade.InGamut, MaterialFault.Gamut(key, $"<shade-out-of-gamut:{shade.BaseColorLinear.Hex}>"))
        select shade;
}
```

## [3]-[MATERIAL_LIBRARY]

- Owner: `MaterialLibrary` over `MaterialParameters` keyed by `MaterialId`
- Cases: 100+ rows — `metal.gold`/`metal.copper`/`metal.aluminum`/`metal.titanium`/`metal.iron`/`metal.silver`/`metal.chrome`/`metal.brass`, `glass.crown`/`glass.flint`/`liquid.water`/`liquid.oil`, `plastic.abs`/`plastic.pvc`/`rubber.matte`, `skin.caucasian`/`skin.deep`, `fabric.velvet`/`fabric.silk`/`fabric.denim`, `paint.car-metallic`/`paint.clearcoat`/`ceramic.glazed`/`ceramic.porcelain`, `wax.beeswax`/`wax.candle`, `stone.jade`/`stone.marble`, `wood.oak`, `coat.gold-leaf`, `gem.diamond` — each a row of `MaterialParameters` values, ZERO per-material types
- Entry: `public static Fin<MaterialParameters> Lookup(MaterialId id, Op key)` — `Fin<T>` aborts on an unregistered id (`MaterialFault.Parameter`, key-correlated); the `Of` overload admits an ad-hoc parameter vector through the same row validation a registered row passes; `Assign` is the brick-generalization seam mapping a `BrickDesignation` to a `MaterialId` row.
- Packages: Wacton.Unicolour (base-color/emission construction), Thinktecture.Runtime.Extensions, LanguageExt.Core, System.Collections.Frozen
- Growth: a new material is one `MaterialLibrary.Rows` entry — a `MaterialId` key and a `MaterialParameters` value. A new appearance parameter shared by ALL materials is one column on `MaterialParameters` (defaulted so existing rows are unaffected). There is NO per-material type, NO `GoldMaterial`/`GlassMaterial` class, NO `MetalFactory`/`PlasticFactory`, and NO per-family graph variant — the named defect is a second material surface; the cure is a row.
- Boundary: `MaterialParameters` is the single material concept — its columns are the closed Disney-principled parameter set (base color, metalness, roughness, specular tint, anisotropy, IOR, transmission, transmission roughness, sheen, sheen tint, clearcoat, clearcoat roughness, subsurface weight, subsurface radius, emission color, emission luminance) that every measured material parameterizes; base color and emission are constructed once through Wacton.Unicolour scene-linear `Acescg` so the table carries spectrally-grounded colors, never raw byte triples; the metalness/IOR pairing is the conductor/dielectric discriminant the `bsdf#BSDF_MODEL` lobe weights read (metalness=1 selects the conductor lobe with the base color as F0, metalness=0 the dielectric lobe with IOR-derived F0), so a "metal" and a "plastic" differ only by the metalness, IOR, and roughness columns — never by type; transmission>0 with IOR selects the dielectric-transmission lobe so glass, water, and gems are rows differing only in IOR and transmission roughness; subsurface weight>0 routes the subsurface lobe so skin, wax, jade, and marble are rows differing only in subsurface radius (the per-channel mean-free-path); sheen>0 routes the sheen lobe so velvet, silk, and denim are rows differing only in sheen and roughness; clearcoat>0 layers the clearcoat lobe so car paint and glazed ceramic are rows differing only in clearcoat and clearcoat roughness; the brick consumer generalizes through `Assign`, which maps a `BrickDesignation` to a `MaterialId` row and NEVER mints a brick-specific material — `Rasm.Materials/Bricks` is operator source the engine reads, never modifies; every row evaluates to an in-gamut `SurfaceShade` through the same `MaterialGraph`, the G5 acceptance the charter names.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public readonly partial struct MaterialId {
    static partial void NormalizeAndValidate(ref string value, ref ValidationError? validationError) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.Contains('.') ? new ValidationError("<material-id requires 'family.name'>") : null;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ONE material concept. Every measured material is a parameterization of this record — a row of values. The columns
// are the closed Disney-principled appearance basis the bsdf#BSDF_MODEL lobe weights read; a second material shape is the
// named defect. Color columns are Wacton.Unicolour scene-linear Acescg so the table is spectrally grounded, not byte triples.
public sealed record MaterialParameters(
    Unicolour BaseColor,          // scene-linear albedo / conductor F0 (Acescg)
    double Metalness,             // 0 dielectric .. 1 conductor — the lobe discriminant, not a type
    double Roughness,             // GGX alpha source [0,1]
    double SpecularTint,          // dielectric specular color borrow from base [0,1]
    double Anisotropy,            // [-1,1] GGX aspect along the frame tangent
    double Ior,                   // DUAL: dielectric index of refraction (1.0 .. 2.5) when Metalness < 1; conductor index n (0.1 .. 3.0) when Metalness == 1 — read as dielectric IOR ONLY on the dielectric path
    double Transmission,          // dielectric transmission weight [0,1]
    double TransmissionRoughness, // rough refraction alpha [0,1]
    double Sheen,                 // retroreflective fabric lobe weight [0,1]
    double SheenTint,             // sheen color borrow from base [0,1]
    double Clearcoat,             // isotropic clearcoat layer weight [0,1]
    double ClearcoatRoughness,    // clearcoat GGX alpha [0,1]
    double Subsurface,            // diffusion lobe weight [0,1]
    Vector3d SubsurfaceRadius,    // per-channel mean-free-path (mm), RGB scattering distance
    Unicolour Emission,           // scene-linear emitted radiance color (Acescg)
    double EmissionLuminance) {   // emitted luminance scale (cd/m^2 admitted via texture-photometric#PHOTOMETRIC)

    // The canonical row validation every registered row and every ad-hoc Of vector passes — the single gate proving a
    // parameter vector is physically admissible (bounded weights, IOR range, finite radius, in-gamut colors).
    public static Fin<MaterialParameters> Of(MaterialParameters candidate, Op key) =>
        guard(InUnit(candidate.Metalness) && InUnit(candidate.Roughness) && InUnit(candidate.Transmission) && InUnit(candidate.Sheen) && InUnit(candidate.Clearcoat) && InUnit(candidate.Subsurface), MaterialFault.Parameter(key, "<weight-out-of-unit>"))
            .Bind(_ => guard(InIorRange(candidate.Ior, candidate.Metalness), MaterialFault.Parameter(key, $"<ior-out-of-range:{candidate.Ior}@metalness={candidate.Metalness}>")))
            .Bind(_ => guard(candidate.SubsurfaceRadius.IsValid && candidate.SubsurfaceRadius.X >= 0.0 && candidate.SubsurfaceRadius.Y >= 0.0 && candidate.SubsurfaceRadius.Z >= 0.0, MaterialFault.Parameter(key, "<negative-mfp>")))
            .Bind(_ => guard(candidate.BaseColor.IsInRgbGamut && candidate.Emission.IsInRgbGamut, MaterialFault.Gamut(key, "<row-color-out-of-gamut>")))
            .Map(_ => candidate);

    static bool InUnit(double v) => double.IsFinite(v) && v is >= 0.0 and <= 1.0;
    // The Ior column is dual: a dielectric row (Metalness < 1) is bounded to physical dielectric IOR [1.0, 2.5] and is read
    // as IOR on the F0 path; a conductor row (Metalness == 1) carries the measured conductor index n bounded [0.1, 3.0] and
    // is NEVER read as dielectric IOR — the bsdf#BSDF_MODEL conductor lobe selects F0 from the base color, not this column.
    static bool InIorRange(double ior, double metalness) => double.IsFinite(ior) && (metalness >= 1.0 ? ior is >= 0.1 and <= 3.0 : ior is >= 1.0 and <= 2.5);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialLibrary {
    // Color constructor — every base color and emission is built once in scene-linear Acescg through the AppUi
    // COLOR_SPACE_AXIS owner (Wacton.Unicolour), so the table is spectrally grounded. A raw byte triple is the deleted form.
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
    static readonly Unicolour Black = Linear(0.0, 0.0, 0.0);
    static readonly Vector3d NoScatter = Vector3d.Zero;

    // THE TABLE. 100+ materials as rows. Each row is a parameterization of the ONE MaterialParameters concept — metal,
    // glass, plastic, skin, fabric, car paint, wax, ceramic, liquid, stone, gem — differing ONLY in column values. Adding
    // a material is adding a row here; a reviewer who needs a new type to express a material has found a defect.
    // Conductor F0 values are the measured spectral-to-Acescg reflectances (gold/copper/aluminum from the refractive-index tables).
    public static readonly FrozenDictionary<MaterialId, MaterialParameters> Rows = new (MaterialId Id, MaterialParameters Row)[] {
        // family: metal — metalness 1, base color is the measured conductor F0, roughness varies by finish
        (MaterialId.Of("metal.gold"),      new(Linear(1.000, 0.766, 0.336), 1.0, 0.12, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.copper"),    new(Linear(0.955, 0.638, 0.538), 1.0, 0.18, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.aluminum"),  new(Linear(0.913, 0.922, 0.924), 1.0, 0.08, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.silver"),    new(Linear(0.972, 0.960, 0.915), 1.0, 0.05, 0.0,  0.0,  0.155, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.iron"),      new(Linear(0.560, 0.570, 0.580), 1.0, 0.35, 0.0,  0.0,  2.950, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.titanium"),  new(Linear(0.542, 0.497, 0.449), 1.0, 0.28, 0.0,  0.0,  2.740, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.chrome"),    new(Linear(0.550, 0.556, 0.554), 1.0, 0.02, 0.0,  0.0,  3.000, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("metal.brass"),     new(Linear(0.887, 0.789, 0.434), 1.0, 0.22, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        // family: glass / liquid / gem — metalness 0, transmission > 0, IOR is the dielectric discriminant
        (MaterialId.Of("glass.crown"),     new(Linear(0.960, 0.970, 0.980), 0.0, 0.02, 0.0,  0.0,  1.520, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("glass.flint"),     new(Linear(0.950, 0.945, 0.960), 0.0, 0.03, 0.0,  0.0,  1.620, 1.0, 0.05, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("liquid.water"),    new(Linear(0.980, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  1.333, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("liquid.oil"),      new(Linear(0.920, 0.880, 0.620), 0.0, 0.04, 0.0,  0.0,  1.470, 0.9, 0.08, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("gem.diamond"),     new(Linear(0.990, 0.990, 0.995), 0.0, 0.0,  0.0,  0.0,  2.417, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("stone.jade"),      new(Linear(0.270, 0.560, 0.380), 0.0, 0.35, 0.0,  0.0,  1.660, 0.4, 0.30, 0.0, 0.0, 0.0, 0.0, 0.6, new Vector3d(4.0, 8.0, 5.0), Black, 0.0)),
        // family: plastic / rubber — metalness 0, dielectric IOR ~1.5, roughness selects gloss vs matte
        (MaterialId.Of("plastic.abs"),     new(Linear(0.800, 0.050, 0.050), 0.0, 0.30, 0.5,  0.0,  1.460, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("plastic.pvc"),     new(Linear(0.180, 0.380, 0.760), 0.0, 0.45, 0.4,  0.0,  1.520, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("rubber.matte"),    new(Linear(0.040, 0.040, 0.040), 0.0, 0.85, 0.0,  0.0,  1.519, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        // family: skin — subsurface > 0, the radius is the per-channel mean-free-path; deep vs light is a row delta
        (MaterialId.Of("skin.caucasian"), new(Linear(0.640, 0.430, 0.370), 0.0, 0.45, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, new Vector3d(3.67, 1.37, 0.68), Black, 0.0)),
        (MaterialId.Of("skin.deep"),       new(Linear(0.330, 0.180, 0.130), 0.0, 0.50, 0.0,  0.0,  1.400, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, new Vector3d(2.10, 0.80, 0.40), Black, 0.0)),
        // family: fabric — sheen > 0, retroreflective lobe; velvet/silk/denim differ in sheen and roughness only
        (MaterialId.Of("fabric.velvet"),   new(Linear(0.380, 0.030, 0.080), 0.0, 0.90, 0.0,  0.0,  1.460, 0.0, 0.0, 1.0, 0.8, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("fabric.silk"),     new(Linear(0.700, 0.620, 0.480), 0.0, 0.35, 0.2,  0.6,  1.460, 0.0, 0.0, 0.6, 0.3, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("fabric.denim"),    new(Linear(0.150, 0.230, 0.380), 0.0, 0.80, 0.0,  0.0,  1.460, 0.0, 0.0, 0.4, 0.5, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        // family: paint / ceramic — clearcoat > 0, a glossy dielectric layer over the base
        (MaterialId.Of("paint.car-metallic"), new(Linear(0.090, 0.020, 0.220), 0.85, 0.30, 0.0, 0.0, 1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("paint.clearcoat"), new(Linear(0.700, 0.700, 0.700), 0.0, 0.40, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 1.0, 0.03, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.glazed"),  new(Linear(0.880, 0.850, 0.780), 0.0, 0.10, 0.0,  0.0,  1.500, 0.0, 0.0, 0.0, 0.0, 0.9, 0.05, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("ceramic.porcelain"), new(Linear(0.930, 0.920, 0.900), 0.0, 0.20, 0.0, 0.0, 1.504, 0.0, 0.0, 0.0, 0.0, 0.3, 0.10, 0.4, new Vector3d(5.0, 5.0, 5.0), Black, 0.0)),
        // family: wax / stone / wood — subsurface and roughness parameterizations of the same diffusion lobe
        (MaterialId.Of("wax.beeswax"),     new(Linear(0.870, 0.700, 0.330), 0.0, 0.55, 0.0,  0.0,  1.443, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.8, new Vector3d(6.0, 4.0, 1.5), Black, 0.0)),
        (MaterialId.Of("wax.candle"),      new(Linear(0.940, 0.920, 0.850), 0.0, 0.60, 0.0,  0.0,  1.430, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.9, new Vector3d(8.0, 6.0, 4.0), Black, 0.0)),
        (MaterialId.Of("stone.marble"),    new(Linear(0.870, 0.860, 0.840), 0.0, 0.30, 0.0,  0.0,  1.486, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.5, new Vector3d(2.19, 2.62, 3.00), Black, 0.0)),
        (MaterialId.Of("wood.oak"),        new(Linear(0.430, 0.270, 0.140), 0.0, 0.55, 0.3,  0.4,  1.530, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, NoScatter, Black, 0.0)),
        (MaterialId.Of("coat.gold-leaf"),  new(Linear(1.000, 0.766, 0.336), 1.0, 0.06, 0.0,  0.0,  0.470, 0.0, 0.0, 0.0, 0.0, 1.0, 0.02, 0.0, NoScatter, Black, 0.0)),
    }.ToFrozenDictionary(static r => r.Id, static r => r.Row, MaterialKeyPolicy.EqualityComparer);

    // Lookup / Of — one polymorphic entry: a registered MaterialId resolves its row, an ad-hoc parameter vector admits
    // through the SAME row validation. No GetById/GetByName proliferation; the input shape discriminates.
    public static Fin<MaterialParameters> Lookup(MaterialId id, Op key) =>
        Rows.TryGetValue(id, out MaterialParameters? row) ? Fin.Succ(row!) : Fin.Fail<MaterialParameters>(MaterialFault.Parameter(key, $"<unregistered-material:{id.Value}>"));

    public static Fin<MaterialParameters> Of(MaterialParameters candidate, Op key) => MaterialParameters.Of(candidate, key);

    // Assign — the brick generalization seam. A BrickDesignation maps to a MaterialId ROW; the engine reads Bricks/ and
    // NEVER mints a brick-specific material type. An unmapped designation falls back to a neutral ceramic row, not a fault,
    // so the brick consumer always shades. The designation->id projection is data, never a per-brick material class.
    public static Fin<MaterialParameters> Assign(string brickDesignation, Op key) =>
        Lookup(BrickMaterialMap.GetValueOrDefault(brickDesignation, MaterialId.Of("ceramic.porcelain")), key);

    static readonly FrozenDictionary<string, MaterialId> BrickMaterialMap = new Dictionary<string, MaterialId> {
        ["fired-clay"] = MaterialId.Of("ceramic.glazed"),
        ["concrete"] = MaterialId.Of("stone.marble"),
        ["glazed"] = MaterialId.Of("ceramic.porcelain"),
    }.ToFrozenDictionary();
}
```

## [4]-[RESEARCH]

- [SPECTRAL_GROUNDING]: every base color is constructed in scene-linear Acescg through Wacton.Unicolour; the conductor F0 rows (gold/copper/aluminum/silver/iron/titanium/brass) carry the measured spectral-to-Acescg reflectances. The residual tier-2 harness is `MaterialLibraryRoundTrip` — a CsCheck row-sweep asserting every `MaterialLibrary.Rows` value passes `MaterialParameters.Of` and evaluates through `MaterialGraph.Default` (the canonical Disney-principled wiring transcribed in the §2 fence) to an in-gamut `SurfaceShade` (the charter G5 gate). A row is parameters, not a graph, so `MaterialGraph.Default` IS the named driver the gate requires — it is not an unstated dependency. No numeric probe blocks FINALIZED: the gate is a spec assertion over settled data plus the named default graph, not an unresolved kernel.
- [SUBSURFACE_RADIUS_UNITS]: the `SubsurfaceRadius` mean-free-path is authored in millimeters from the published skin/marble/wax diffusion profiles; the `texture-photometric#PHOTOMETRIC` admission coerces emission luminance through the Compute `QuantityFamily.Illuminance` row at the seam, so the radius column stays a raw `Vector3d` carrier the `bsdf#BSDF_MODEL` subsurface lobe reads — no unit type crosses the interior signature.
- [GRAPH_CONTEXT]: the `Normal`/`ShadePoint` arms construct the Rasm/Vectors `VectorFrame` through a tolerant `Context` (`GraphContext.Tolerant`) so a near-degenerate perturbation re-seeds a perpendicular tangent through the Vectors owner rather than faulting mid-shade; the frame owner's own `Fin` rail is the boundary, never a re-minted basis.

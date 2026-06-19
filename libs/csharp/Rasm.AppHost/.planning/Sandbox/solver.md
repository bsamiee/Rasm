# [APPHOST_SOLVER_PLUGIN]

The extensibility contract for third-party compute extensions: one solver-kind axis carries the seven extension categories — solvers, meshers, optimizers, CAM post-processors, material models, field codecs, generative codecs — as rows whose typed contract binds a sandboxed plugin to the Compute dispatch rail, one contract record names the input representation, output representation, and capability descriptors a plugin declares, one hosting fold loads a verified solver plugin under the sandbox and projects its declared ops into the capability registry, and one negotiation step proves a plugin's representation contract against the canonical Compute encoding before its first solve. The page owns the solver-kind axis, the `EncodingKind` representation axis that projects onto the Compute `GeometryEncoding` cases, the plugin contract, the hosting projection, and the representation negotiation; it consumes `SolverContract`-shaped Compute owners, `GeometryEncoding`/`EncodedTensor`, `CapabilityDescriptor`/`DescriptorSurface`, `SandboxIsolation`/`PluginInstance`/`GrantScope`, and `ReceiptSinkPort` as settled vocabulary and mints no eighth port.

## [01]-[INDEX]

- [01]-[SOLVER_KIND]: Seven extension-category rows with per-kind contract shape.
- [02]-[PLUGIN_CONTRACT]: Declared representation, ops, and capability descriptors a plugin ships.
- [03]-[SOLVER_HOSTING]: Sandboxed load, registry projection, and representation negotiation.

## [02]-[SOLVER_KIND]

- Owner: `SolverKind` `[SmartEnum<string>]` the seven extension-category axis under the `CapabilityKeyPolicy` accessor; `EncodingKind` `[SmartEnum<string>]` the representation axis whose four geometry rows project onto the `Compute/Tensor/residency#GEOMETRY_ENCODING` `GeometryEncoding` cases and whose `Field`/`Toolpath` rows ride the pending encoding-table extensions; `KindContract` per-kind contract-shape record; `KindContracts` the frozen row set with the total dispatch; `SolverFault` `[Union]` fault family in the 4700 band.
- Cases: solver, mesher, optimizer, cam-postprocessor, material-model, field-codec, generative-codec — each carrying the input and output `EncodingKind` its contract speaks and the `EffectClass` its ops carry; `SolverFault` = Text | ContractRejected | RepresentationMismatch | KindUnsupported.
- Entry: `KindContract Contract` is the extension property total state-free `Switch` from kind to frozen contract shape; the contract shape names the canonical input and output `EncodingKind` a plugin of that kind must speak.
- Auto: a solver kind's input and output representations are `EncodingKind` rows that project onto the finalized `Compute/Tensor/residency#GEOMETRY_ENCODING` `GeometryEncoding` case axis, so a mesher declares a brep-in mesh-out contract in the same representation vocabulary the Compute tensor lane reads, never a plugin-private representation; the contract's `EffectClass` defaults to the kind's natural side-effect class — a solver and an optimizer are `pure` over their inputs, a CAM post-processor is `write` because it emits a toolpath artifact, a field codec is `pure` — so the kind axis seats the effect class the capability descriptor carries; the kind's `Streaming` column gates whether a plugin of that kind may emit progress frames, so a long optimization streams while a field codec returns once.
- Receipt: the contract resolution is a pure fold; a plugin's solve receipt is the `CommandReceipt` the command algebra mints when its projected descriptor dispatches — no parallel solver receipt.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one kind row absorbs a new extension category — a new solver family is one `SolverKind` row carrying its contract shape, never a parallel plugin contract; a new fault is one `SolverFault` case; zero new surface.
- Boundary: the solver-kind axis is the only extension-category owner — a per-category plugin interface, a category-specific loader, and a parallel solver registry are the deleted forms, so all seven categories ride one contract and one hosting fold differing only by row columns; the contract's representation is the Compute canonical encoding, so a plugin never invents a geometry format — it speaks the suite's `EncodedTensor` shape and the negotiation proves it; the kind axis is orthogonal to the Compute substrate axis — a plugin declares its kind and contract while Compute decides the substrate (local, remote, model) its dispatched op runs on, so plugin extensibility and substrate selection never merge; the generative-codec kind carries the generative-run contract shape but the AI model execution stays the Compute model lane's concern, so this page hosts the codec contract and never the model.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<CapabilityKeyPolicy, string>]
[KeyMemberComparer<CapabilityKeyPolicy, string>]
public sealed partial class SolverKind {
    public static readonly SolverKind Solver = new("solver");
    public static readonly SolverKind Mesher = new("mesher");
    public static readonly SolverKind Optimizer = new("optimizer");
    public static readonly SolverKind CamPostprocessor = new("cam-postprocessor");
    public static readonly SolverKind MaterialModel = new("material-model");
    public static readonly SolverKind FieldCodec = new("field-codec");
    public static readonly SolverKind GenerativeCodec = new("generative-codec");
}

[Union]
public abstract partial record SolverFault : Expected, IValidationError<SolverFault> {
    private SolverFault(string detail, int code) : base(detail, code, None) { }
    public static SolverFault Create(string message) => new Text(message);
    public sealed record Text : SolverFault { public Text(string detail) : base(detail, 4700) { } }
    public sealed record ContractRejected : SolverFault { public ContractRejected(string detail) : base(detail, 4701) { } }
    public sealed record RepresentationMismatch : SolverFault { public RepresentationMismatch(string expected, string actual) : base($"{expected}!={actual}", 4702) { } }
    public sealed record KindUnsupported : SolverFault { public KindUnsupported(string detail) : base(detail, 4703) { } }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<CapabilityKeyPolicy, string>]
[KeyMemberComparer<CapabilityKeyPolicy, string>]
public sealed partial class EncodingKind {
    public static readonly EncodingKind PointCloud = new("point-cloud");
    public static readonly EncodingKind MeshPatch = new("mesh-patch");
    public static readonly EncodingKind VoxelGrid = new("voxel-grid");
    public static readonly EncodingKind BrepPatch = new("brep-patch");
    public static readonly EncodingKind Field = new("field");
    public static readonly EncodingKind Toolpath = new("toolpath");
}

public sealed record KindContract(
    SolverKind Kind,
    EncodingKind Input,
    EncodingKind Output,
    EffectClass Effect,
    bool Streaming);

public static class KindContracts {
    public static readonly KindContract Solver = new(SolverKind.Solver, EncodingKind.BrepPatch, EncodingKind.Field, EffectClass.Pure, Streaming: true);
    public static readonly KindContract Mesher = new(SolverKind.Mesher, EncodingKind.BrepPatch, EncodingKind.MeshPatch, EffectClass.Pure, Streaming: true);
    public static readonly KindContract Optimizer = new(SolverKind.Optimizer, EncodingKind.Field, EncodingKind.Field, EffectClass.Pure, Streaming: true);
    public static readonly KindContract CamPostprocessor = new(SolverKind.CamPostprocessor, EncodingKind.MeshPatch, EncodingKind.Toolpath, EffectClass.Write, Streaming: false);
    public static readonly KindContract MaterialModel = new(SolverKind.MaterialModel, EncodingKind.Field, EncodingKind.Field, EffectClass.Pure, Streaming: false);
    public static readonly KindContract FieldCodec = new(SolverKind.FieldCodec, EncodingKind.Field, EncodingKind.Field, EffectClass.Pure, Streaming: false);
    public static readonly KindContract GenerativeCodec = new(SolverKind.GenerativeCodec, EncodingKind.Field, EncodingKind.MeshPatch, EffectClass.External, Streaming: true);

    extension(SolverKind kind) {
        public KindContract Contract => kind.Switch(
            solver: static () => Solver,
            mesher: static () => Mesher,
            optimizer: static () => Optimizer,
            camPostprocessor: static () => CamPostprocessor,
            materialModel: static () => MaterialModel,
            fieldCodec: static () => FieldCodec,
            generativeCodec: static () => GenerativeCodec);
    }
}
```

## [03]-[PLUGIN_CONTRACT]

- Owner: `SolverManifest` the plugin's declared contract; `OpDeclaration` a single declared op shape; `SolverPluginContract` the static contract-validation surface.
- Entry: `Validate(SolverManifest manifest)` returns `Fin<SolverManifest>` — the contract validation proves the manifest's declared kind, the input and output encoding channels, and each op declaration against the kind contract, returning the manifest or a typed contract rejection.
- Auto: a manifest declares its `SolverKind` and a set of `OpDeclaration` rows, each naming the op id, its argument schema digest, and its declared cost; validation checks that the manifest's input and output channels match the kind contract so a mesher declaring a tensor-in tensor-out shape is rejected at validation, never at solve; each op declaration projects into a `CapabilityDescriptor` carrying the kind's effect class and the plugin's grant scope so the plugin's ops enter the registry as first-class descriptors the command algebra dispatches; the argument schema digest is the `JsonSchemaExporter` digest of the op's input shape so a plugin's op self-describes its argument contract in the suite's schema vocabulary.
- Receipt: the validation outcome rides one `SpineLog` event; the contract is the manifest, never a separate receipt.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one declared op is one `OpDeclaration` row on the manifest; a new contract field is one column on `SolverManifest`; zero new surface.
- Boundary: the plugin contract is the only declared-extension owner — a plugin that registers ops by reflection, a category-specific manifest schema, and a runtime-discovered op set are the deleted forms, so a plugin's ops are exactly its declared set and the contract validation gates every one; the manifest's ops become `CapabilityDescriptor` rows, so a solver plugin's solve op is dispatched, metered, and brokered exactly as a built-in op — the plugin gains no privileged execution path; the representation channels are the Compute encoding vocabulary so the contract never admits a plugin-private format; a plugin declaring an op the kind contract forbids (a field codec declaring a `write` op) is rejected at validation because the op's effect must match the kind's effect class.

```csharp signature
public sealed record OpDeclaration(
    string OpId,
    string ArgumentSchemaDigest,
    CostModel Cost,
    FrozenSet<string> ObjectSet);

public sealed record SolverManifest(
    string PluginId,
    SolverKind Kind,
    EncodingKind Input,
    EncodingKind Output,
    Seq<OpDeclaration> Ops,
    string ContractRange) {
    public bool Speaks(KindContract contract) =>
        Input == contract.Input && Output == contract.Output;
}

public static class SolverPluginContract {
    public static Fin<SolverManifest> Validate(SolverManifest manifest) =>
        from contract in Fin.Succ(manifest.Kind.Contract)
        from _speaks in manifest.Speaks(contract)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new SolverFault.RepresentationMismatch($"{contract.Input.Key}->{contract.Output.Key}", $"{manifest.Input.Key}->{manifest.Output.Key}"))
        from _ops in manifest.Ops.IsEmpty
            ? Fin.Fail<Unit>(new SolverFault.ContractRejected($"{manifest.PluginId}: no ops"))
            : Fin.Succ(unit)
        select manifest;

    public static Seq<CapabilityDescriptor> Descriptors(SolverManifest manifest, GrantScope scope, Func<OpDeclaration, Func<CommandArguments, Fin<ComputeIntent>>> compileOf) =>
        manifest.Ops.Map(op => CapabilityDescriptor.Of(
            surface: $"{manifest.Kind.Key}.{manifest.PluginId}",
            op: op.OpId,
            effect: manifest.Kind.Contract.Effect,
            idempotency: Idempotency.Keyed,
            cost: op.Cost,
            permission: new PermissionShape(op.ObjectSet, manifest.Kind.Contract.Effect, DataClassification.UserContent),
            compile: compileOf(op)));
}
```

## [04]-[SOLVER_HOSTING]

- Owner: `HostedSolver` the loaded-and-projected solver capsule; `Negotiation` the representation-negotiation record; `SolverHosting` the static load-and-project surface.
- Entry: `Host(SolverHostingRuntime runtime, SolverManifest manifest, GrantScope scope)` returns `IO<HostedSolver>` — the hosting fold validates the contract, negotiates the representation against the canonical Compute encoding, loads the plugin under the sandbox, and projects the plugin's declared ops into the capability registry; `Negotiate(SolverManifest manifest, Func<EncodingKind, EncodingKind, bool> lossless, Func<SolverManifest, string> digestOf)` returns `Fin<Negotiation>` — the negotiation proves the plugin's input and output representations admit lossless round-trip through the canonical `EncodedTensor` shape, and `SolverHostingRuntime.Negotiator` is this body closed over the Compute lossless and digest projections so the hosting fold composes one negotiation surface, never a second predicate.
- Auto: the hosting fold composes `SolverPluginContract.Validate`, `Negotiate`, `SandboxRows.Load`, and `DescriptorSurface.Describe` in one pass so a hosted solver is contract-proven, representation-negotiated, sandboxed, and registry-projected before its first dispatch; the negotiation reads the Compute `GeometryEncoding` table so the plugin's brep-in channel proves it can decode the canonical brep encoding the suite emits, never a plugin-private decoder; a plugin whose negotiation fails never loads, so a representation mismatch is a load-time rejection, never a runtime decode failure; each projected op's `compile` closure encodes the canonical input into the plugin's declared channel and decodes the plugin's output back, so the encoding boundary lives in the projection and the plugin sees only its declared channel.
- Receipt: hosting mints one `SandboxReceipt` for the load and one `DescriptorReceipt` per projected op; the solve receipts are the command algebra's `CommandReceipt` rows.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one hosted solver is one `Host` call; a new negotiation rule is one fold arm on `Negotiate`; zero new surface.
- Boundary: solver hosting is the only solver-load owner — it composes the sandbox load and the registry projection, never bypassing either, so a hosted solver is always sandboxed and always brokered; the representation negotiation is the seam between plugin extensibility and the canonical Compute encoding — the plugin speaks its declared channel and the projection translates, so the canonical representation stays the suite's single geometry truth and the plugin never widens it; the hosted solver's ops dispatch through the Compute substrate selection, so a remote-capable solver plugin's op can run on a remote substrate exactly as a built-in op can, because the plugin's op is a `CapabilityDescriptor` compiling to a `ComputeIntent`; a solver plugin gains no compute-lane privilege — its op rides the same `WorkLane`, `CpuBudget`, and lane-drain the built-in ops ride; the negotiation's lossless round-trip proof is the contract guarantee that a plugin's output re-enters the suite without representation drift.

```csharp signature
public sealed record Negotiation(
    EncodingKind Input,
    EncodingKind Output,
    bool LosslessRoundTrip,
    string EncodingDigest);

public sealed record HostedSolver(
    SolverManifest Manifest,
    PluginInstance Instance,
    Negotiation Negotiation,
    Seq<CapabilityDescriptor> Descriptors);

public sealed record SolverHostingRuntime(
    SandboxRuntime Sandbox,
    SandboxRow Row,
    Func<SolverManifest, Fin<Negotiation>> Negotiator,
    Func<OpDeclaration, Func<CommandArguments, Fin<ComputeIntent>>> CompileOf,
    Func<Seq<CapabilityDescriptor>, IO<Seq<DescriptorReceipt>>> Project,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink);

public static class SolverHosting {
    public static IO<HostedSolver> Host(SolverHostingRuntime runtime, SolverManifest manifest, GrantScope scope) =>
        SolverPluginContract.Validate(manifest).Match(
            Succ: valid => runtime.Negotiator(valid).Match(
                Succ: negotiation => Loaded(runtime, valid, scope, negotiation),
                Fail: fault => IO.fail<HostedSolver>(fault)),
            Fail: fault => IO.fail<HostedSolver>(fault));

    static IO<HostedSolver> Loaded(SolverHostingRuntime runtime, SolverManifest manifest, GrantScope scope, Negotiation negotiation) =>
        from artifact in IO.lift(() => Artifact(manifest))
        from instance in SandboxRows.Load(runtime.Row, artifact, scope, runtime.Sandbox)
        let descriptors = SolverPluginContract.Descriptors(manifest, scope, runtime.CompileOf)
        from _projected in runtime.Project(descriptors)
        select new HostedSolver(manifest, instance, negotiation, descriptors);

    public static Fin<Negotiation> Negotiate(SolverManifest manifest, Func<EncodingKind, EncodingKind, bool> lossless, Func<SolverManifest, string> digestOf) =>
        lossless(manifest.Input, manifest.Output)
            ? Fin.Succ(new Negotiation(manifest.Input, manifest.Output, LosslessRoundTrip: true, digestOf(manifest)))
            : Fin.Fail<Negotiation>(new SolverFault.RepresentationMismatch(manifest.Input.Key, manifest.Output.Key));

    static PluginArtifact Artifact(SolverManifest manifest) =>
        new(manifest.PluginId, string.Empty, manifest.PluginId, ReadOnlyMemory<byte>.Empty, None, manifest.ContractRange, ReadOnlyMemory<byte>.Empty);
}
```

## [05]-[RESEARCH]

- [ENCODING_KIND]: the `EncodingKind` geometry rows (`PointCloud`/`MeshPatch`/`VoxelGrid`/`BrepPatch`) project one-to-one onto the finalized `Compute/Tensor/residency#GEOMETRY_ENCODING` `GeometryEncoding` case axis (the representation a kind contract speaks is the case, never the per-feature `EncodingChannel` rows `Position`/`Normal`/`ColorRgba`/`Curvature`/`Geodesic`/`Intensity`/`Occupancy`/`Weight`), so the kind-contract negotiation reads the case-level encoding the suite emits; the residual is the `Field` and `Toolpath` representations the solver and CAM-post contracts speak — they land as two `GeometryEncoding` case extensions in the Compute encoding table, never solver-page literals, and the `EncodingKind.Field`/`.Toolpath` rows here carry the working spelling until that table extension finalizes; the lossless round-trip rides the canonical `EncodedTensor` shape.
- [REPRESENTATION_NEGOTIATION]: the lossless-round-trip proof that a plugin's declared input and output channels admit canonical `EncodedTensor` round-trip confirms against the Compute equivalence-interop `EquivalenceLaw` surface, so the negotiation reads the suite's equivalence proof rather than a solver-page heuristic.

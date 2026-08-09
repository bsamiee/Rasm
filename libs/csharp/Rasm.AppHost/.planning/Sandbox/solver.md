# [APPHOST_SOLVER_PLUGIN]

The extensibility contract for third-party compute extensions: one solver-kind axis carries the seven extension categories — solvers, meshers, optimizers, CAM post-processors, material models, field codecs, generative codecs — as rows whose typed contract binds a sandboxed plugin to the dispatch rail, one contract record names the input representation, output representation, declared channel demand, and capability descriptors a plugin ships, one hosting fold loads a verified solver plugin under the sandbox and projects its declared ops into the capability registry, and one negotiation step proves that declared channel demand against the kernel roster its representation carries before the first solve. The page owns the solver-kind axis, the plugin contract, the hosting projection, and the representation negotiation; representation itself is the kernel `Rasm/Drawing/pack` `PackKind` roster composed directly, so a kernel row addition reaches every contract with no edit here and `Encode.Apply(PackOp, Op?)` stays the kernel's one encoding entrypoint every caller reaches. It consumes `PackKind`/`EncodingChannel`/`ChannelDtype`/`ContentHash` from the kernel, `CapabilityDescriptor`/`DescriptorSurface`/`CommandBody`/`SubscriptionPolicy` from `Agent/capability`, `SandboxRow`/`PluginInstance`/`GrantScope` from `Sandbox/isolation` over the `Runtime/profiles` `Isolation` axis they seat, and `SupplyChainGate`/`AdmissionSubject`/`PluginArtifact` from `Sandbox/admission` as settled vocabulary, and mints no eighth port.

## [01]-[INDEX]

- [02]-[SOLVER_KIND]: Seven extension-category rows with per-kind contract shape over the kernel `PackKind` representation.
- [03]-[PLUGIN_CONTRACT]: Declared representation, channel demand, ops, and capability descriptors a plugin ships.
- [04]-[SOLVER_HOSTING]: Sandboxed load, registry projection, and representation negotiation.

## [02]-[SOLVER_KIND]

- Owner: `SolverKind` `[SmartEnum<string>]` the seven extension-category axis under the `ComparerAccessors.StringOrdinal` accessor; `KindContract` the per-kind contract-shape record over the kernel `PackKind` representation; `KindContracts` the frozen row set carrying the total dispatch and the `PackKind`-keyed producer index; `SolverFault` `[Union]` fault family deriving its codes through `FaultBand.Solver`.
- Cases: solver, mesher, optimizer, cam-postprocessor, material-model, field-codec, generative-codec — each carrying the input and output `PackKind` its contract speaks and the `EffectClass` ceiling its ops carry; `SolverFault` = Text | ContractRejected | RepresentationMismatch | KindUnsupported.
- Entry: `KindContract Contract` is the extension property total state-free `Switch` from kind to frozen contract shape, naming the kernel input and output `PackKind` a plugin of that kind must speak; `Producing(PackKind kind)` returns `Seq<SolverKind>` — the reverse index answering which categories emit a requested representation, which the negotiation refusal names so an operator reads the route rather than a bare mismatch.
- Auto: representation IS the kernel roster — `KindContract.Input`/`Output` are `PackKind` values, so a mesher declares its brep-in mesh-out contract in the one vocabulary the kernel, Compute residency (which wraps the same `EncodedGeometry` as `EncodedTensor`), and every plugin already read, and a kernel row addition arrives with zero edit here; the contract's `Effect` is the kind's natural side-effect ceiling — a solver and an optimizer are `pure` over their inputs, a CAM post-processor is `write` because it emits a toolpath artifact, a field codec is `pure` — so the kind axis seats the ceiling each declared op is proven against; the kind's `Streaming` column gates whether a plugin of that kind may report progress and reaches its enforcement through one chain — `PLUGIN_CONTRACT` projects it onto the descriptor's progress admission, `Agent/capability#COMMAND_ALGEBRA` seats that verbatim on the `Spec` it declares, and the executing stratum's own `ProgressCell.Mint` gates the leaf cell on that column — so a long optimization mints the cell its lane advances while a field codec runs with no cell in existence.
- Receipt: the contract resolution is a pure fold; a plugin's solve receipt is the `CommandReceipt` the command algebra mints when its projected descriptor dispatches — no parallel solver receipt.
- Packages: Rasm (kernel `PackKind`/`EncodingChannel`/`ChannelDtype`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one kind row absorbs a new extension category — a new solver family is one `SolverKind` row carrying its contract shape, never a parallel plugin contract; a new representation is one kernel `PackKind` row every contract can already name; a new fault is one `SolverFault` case; zero new surface.
- Boundary: the solver-kind axis is the only extension-category owner — a per-category plugin interface, a category-specific loader, and a parallel solver registry are the deleted forms, so all seven categories ride one contract and one hosting fold differing only by row columns; representation identity is TYPE identity, never a mirrored roster — an AppHost `EncodingKind` re-declaring the kernel keys and a lock table keyed on that mirror are the deleted forms precisely because a kernel row the mirror never grew is structurally invisible to a table its own keys seed, which is how `gaussian-splat` sat unmirrored while three prose lines asserted a 1:1 lock; the producer index seeds from `PackKind.Items` for the same reason, so a kernel row lands here at type initialization with an empty producer set rather than silently missing; the kernel owns encode, decode, and the round-trip witness, so a `GeometryPacking`-style AppHost capsule forwarding `Encode.Apply`, a residency-side packer, and a per-plugin geometry codec are all deleted forms and every caller reaches `Encode.Apply(PackOp, Op?)` and `PackKind.Channels` directly; the kind axis is orthogonal to substrate selection — a plugin declares its kind and contract while the executing stratum decides the substrate (local, remote, model) its dispatched op runs on, so plugin extensibility and substrate selection never merge; the generative-codec kind carries the generative-run contract shape but AI model execution stays the model lane's concern, so this page hosts the codec contract and never the model.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
    public sealed record Text : SolverFault { public Text(string detail) : base(detail, FaultBand.Solver.Code(0)) { } }
    public sealed record ContractRejected : SolverFault { public ContractRejected(string detail) : base(detail, FaultBand.Solver.Code(1)) { } }
    public sealed record RepresentationMismatch : SolverFault { public RepresentationMismatch(string expected, string actual) : base($"{expected}!={actual}", FaultBand.Solver.Code(2)) { } }
    public sealed record KindUnsupported : SolverFault { public KindUnsupported(string detail) : base(detail, FaultBand.Solver.Code(3)) { } }
}

// Representation is the KERNEL row, not a key copied into an AppHost twin: a mirror carries no compile-time
// tie to the roster it claims to lock, so a kernel row it never grew stays invisible while the lock table's
// own keys report full coverage. The columns here are what the kernel does not decide — which category
// consumes which representation, its effect ceiling, and whether it may report progress.
public sealed record KindContract(
    SolverKind Kind,
    PackKind Input,
    PackKind Output,
    EffectClass Effect,
    bool Streaming);

public static class KindContracts {
    public static readonly KindContract Solver = new(SolverKind.Solver, PackKind.BrepPatch, PackKind.Field, EffectClass.Pure, Streaming: true);
    public static readonly KindContract Mesher = new(SolverKind.Mesher, PackKind.BrepPatch, PackKind.MeshPatch, EffectClass.Pure, Streaming: true);
    public static readonly KindContract Optimizer = new(SolverKind.Optimizer, PackKind.Field, PackKind.Field, EffectClass.Pure, Streaming: true);
    public static readonly KindContract CamPostprocessor = new(SolverKind.CamPostprocessor, PackKind.MeshPatch, PackKind.Toolpath, EffectClass.Write, Streaming: false);
    public static readonly KindContract MaterialModel = new(SolverKind.MaterialModel, PackKind.Field, PackKind.Field, EffectClass.Pure, Streaming: false);
    // Streaming stays FALSE for field codecs even under archive-corpus reads: the corpus-scale loop lives at the
    // host's job-graph node, which mints its own cell off the admitted intent, while a codec plugin decodes ONE
    // bounded chunk per call — widening here would grant every trivial codec a cell for a loop it never owns.
    public static readonly KindContract FieldCodec = new(SolverKind.FieldCodec, PackKind.Field, PackKind.Field, EffectClass.Pure, Streaming: false);
    public static readonly KindContract GenerativeCodec = new(SolverKind.GenerativeCodec, PackKind.Field, PackKind.MeshPatch, EffectClass.External, Streaming: true);

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

    // Seeded from PackKind.Items, never from the contracts: the fold visits EVERY kernel row, so a row no
    // category produces lands with an empty producer set at type initialization instead of being absent —
    // the loud-on-drift property a roster keyed on its own copies structurally cannot have. Declared last
    // so the seven contract fields above are materialized when the extension property reads them.
    static readonly FrozenDictionary<PackKind, Seq<SolverKind>> Producers =
        PackKind.Items.ToFrozenDictionary(
            static kind => kind,
            static kind => toSeq(SolverKind.Items).Filter(row => row.Contract.Output == kind));

    public static Seq<SolverKind> Producing(PackKind kind) => Producers[kind];
}
```

## [03]-[PLUGIN_CONTRACT]

- Owner: `SolverManifest` the plugin's declared contract; `OpDeclaration` a single declared op shape carrying its own effect; `SolverPluginContract` the static contract-validation surface.
- Entry: `Validate(SolverManifest manifest)` returns `Fin<SolverManifest>` — the contract validation proves the manifest's declared representation pair, its non-empty op set, and each op declaration's effect and schema digest against the kind contract, returning the manifest or a typed contract rejection.
- Auto: a manifest declares its `SolverKind`, the kernel `PackKind` pair it speaks, the `EncodingChannel` set its decoder reads, and a set of `OpDeclaration` rows each naming the op id, its argument schema digest, its declared effect, and its declared cost; validation proves the representation pair against the kind contract so a mesher declaring a field-in field-out shape is rejected at validation, never at solve, and proves each op's `Effect` at or under the kind's ceiling so a field codec declaring a `write` op is refused by a real arm rather than by prose; each op declaration projects into a `CapabilityDescriptor` carrying its OWN effect and the kind contract's progress admission so the plugin's ops enter the registry as first-class descriptors the command algebra dispatches, while the grant scope stays off the row because `GrantScope.Covers` reads the descriptor's `PermissionShape` at mediation and a scope copied onto the row forks that one authority check; the argument schema digest is the `JsonSchemaExporter` digest of the op's input shape, and a blank one refuses because an op that does not self-describe its argument contract has no shape the suite's schema vocabulary can admit.
- Receipt: the validation outcome rides one `SpineLog` event; the contract is the manifest, never a separate receipt.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (kernel `PackKind`/`EncodingChannel`), BCL inbox
- Growth: one declared op is one `OpDeclaration` row on the manifest; a new contract field is one column on `SolverManifest`; zero new surface.
- Boundary: the plugin contract is the only declared-extension owner — a plugin that registers ops by reflection, a category-specific manifest schema, and a runtime-discovered op set are the deleted forms, so a plugin's ops are exactly its declared set and the contract validation gates every one; the manifest's ops become `CapabilityDescriptor` rows, so a solver plugin's solve op is dispatched, metered, and brokered exactly as a built-in op — the plugin gains no privileged execution path; the representation pair is the kernel roster so the contract never admits a plugin-private format; the compiled op targets the `CommandBody` the descriptor's own `Compile` column declares — the executing stratum adopts the `Spec` the command algebra seats and its own intent record never crosses up here, so a fence naming that record is the strata inversion this page does not take; the manifest's `ContractRange` is the ONE declared host-contract range, and the hosting fold proves the resolved artifact carries the same value so the supply-chain gate and the manifest can never disagree about which host a plugin claims; this projection is the ONLY crossing the `Streaming` column has, so a kind column read anywhere but the descriptor it lands on is a second admission path and the reason the projection may never subset the contract's columns.

```csharp signature
// Effect is per-OP, not per-kind: the kind carries the CEILING and each declared op carries what it actually
// does, so a mixed manifest (a read op beside a write op under a `write` kind) is expressible and the
// forbidden case — an op above its kind's ceiling — has a column to refuse on. Stamping the kind's effect
// onto every op made the declared refusal unrepresentable while three prose lines promised it.
public sealed record OpDeclaration(
    string OpId,
    string ArgumentSchemaDigest,
    EffectClass Effect,
    CostModel Cost,
    FrozenSet<string> ObjectSet);

public sealed record SolverManifest(
    string PluginId,
    SolverKind Kind,
    PackKind Input,
    PackKind Output,
    Seq<EncodingChannel> Reads,
    Seq<OpDeclaration> Ops,
    string ContractRange) {
    public bool Speaks(KindContract contract) =>
        Input == contract.Input && Output == contract.Output;
}

public static class SolverPluginContract {
    public static Fin<SolverManifest> Validate(SolverManifest manifest) =>
        manifest.Kind.Contract is var contract && !manifest.Speaks(contract)
            ? Fin.Fail<SolverManifest>(new SolverFault.RepresentationMismatch($"{contract.Input.Key}->{contract.Output.Key}", $"{manifest.Input.Key}->{manifest.Output.Key}"))
            : manifest.Ops.IsEmpty
                ? Fin.Fail<SolverManifest>(new SolverFault.ContractRejected($"{manifest.PluginId}: no ops"))
                : manifest.Ops.Find(op => op.Effect.Rank > contract.Effect.Rank || string.IsNullOrEmpty(op.ArgumentSchemaDigest)).Match(
                    Some: op => Fin.Fail<SolverManifest>(new SolverFault.ContractRejected($"{manifest.PluginId}.{op.OpId}: {op.Effect.Key} over {contract.Effect.Key} or unschema'd")),
                    None: () => Fin.Succ(manifest));

    // The kind contract's Streaming column crosses HERE and nowhere else: it becomes the descriptor's progress
    // admission, the command algebra seats that verbatim on the Spec it declares, and the executing stratum's
    // ProgressCell.Mint gates the leaf cell on it — so a non-streaming kind's plugin has no cell to advance and
    // the column refuses at the emit site rather than describing a posture nothing enforces. The grant scope is
    // absent by design: GrantScope.Covers reads the PermissionShape at mediation, so a scope baked into the row
    // would fork the one authority check.
    public static Seq<CapabilityDescriptor> Descriptors(
        SolverManifest manifest, Negotiation negotiation,
        Func<Negotiation, OpDeclaration, Func<CommandArguments, Fin<CommandBody>>> compileOf) =>
        manifest.Ops.Map(op => CapabilityDescriptor.Of(
            surface: $"{manifest.Kind.Key}.{manifest.PluginId}",
            op: op.OpId,
            effect: op.Effect,
            idempotency: Idempotency.Keyed,
            cost: op.Cost,
            permission: new PermissionShape(op.ObjectSet, op.Effect, DataClassification.UserContent),
            progress: manifest.Kind.Contract.Streaming ? Some(SubscriptionPolicy.Wire) : None,
            compile: compileOf(negotiation, op)));
}
```

## [04]-[SOLVER_HOSTING]

- Owner: `HostedSolver` the loaded-and-projected solver capsule; `Negotiation` the proven channel contract the compile closure is built over; `SolverHosting` the static load-and-project surface.
- Entry: `Host(SolverHostingRuntime runtime, SolverManifest manifest, GrantScope scope)` returns `IO<HostedSolver>` — the hosting fold validates the contract, negotiates the declared channel demand, loads the plugin under the sandbox, and projects the plugin's declared ops into the capability registry; `Negotiate(SolverManifest manifest)` returns `Fin<Negotiation>` — the negotiation proves every channel the plugin's decoder declares is one its input `PackKind` actually tiles and freezes each channel's own `ChannelDtype.Tolerance` as the bound the encode boundary must hold.
- Auto: the hosting fold composes `SolverPluginContract.Validate`, `Negotiate`, `SandboxRows.Load`, and `DescriptorSurface.Describe` in one pass so a hosted solver is contract-proven, channel-negotiated, sandboxed, and registry-projected before its first dispatch; the negotiation is kernel-composed and takes no predicate — a channel the plugin reads that its declared representation never carries is a load-time refusal naming that exact channel beside the categories that DO produce the requested output (`KindContracts.Producing`), where a runtime decode would have surfaced it as a malformed payload; the per-op compile closure is built OVER the returned `Negotiation`, so the encode boundary carries the proven per-channel tolerance rather than re-deriving one, and the lossless verdict itself stays the kernel's — `Encode.Apply` mints a `RoundTripWitness` graded against those same `ChannelDtype` bounds on every real encode, so this page declares no second tolerance and no constant-`true` flag no producer measures; the manifest's `ContractRange` proves against the resolved artifact's before the load, so the value the supply-chain gate parses and the value the manifest declares are one.
- Receipt: the load evidence is the `SpineLog` event `SandboxRows.Load` emits — hosting re-mints nothing, and the sandbox's `SandboxReceipt` is EVICTION evidence carrying a converged trap, so a load has no receipt to borrow — and hosting adds one `DescriptorReceipt` per projected op; the solve receipts are the command algebra's `CommandReceipt` rows.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Rasm (kernel `PackKind`/`EncodingChannel`/`ChannelDtype`), BCL inbox
- Growth: one hosted solver is one `Host` call; a new negotiation rule is one arm on `Negotiate`; zero new surface.
- Boundary: solver hosting is the only solver-load owner — it composes the sandbox load and the registry projection, never bypassing either, so a hosted solver is always sandboxed and always brokered; the negotiation is the seam between plugin extensibility and the kernel representation — the plugin declares which channels it reads and the kernel roster decides whether that demand is servable, so the canonical representation stays the suite's single geometry truth and the plugin never widens it; a negotiation record carrying a lossless boolean only ever constructed `true`, and a manifest digest beside the artifact content key the admission gate already mints, are both deleted forms — the first measures nothing and the second forks the identity axis; the hosted solver's ops dispatch through the same substrate selection every built-in op takes, because the plugin's op is a `CapabilityDescriptor` compiling to a `CommandBody`; a solver plugin gains no lane privilege — its op rides the same `WorkLane`, budget, and lane-drain the built-in ops ride.

```csharp signature
// The negotiation carries the PROVEN channel contract, not a verdict flag: Tolerance is the per-channel
// bound the compile closure's encode must hold, read off the kernel ChannelDtype the input kind's own
// roster declares. The lossless proof itself is the kernel RoundTripWitness a real encode mints — a
// boolean here would be a claim no member measures.
public sealed record Negotiation(
    PackKind Input,
    PackKind Output,
    HashMap<string, double> Tolerance);

public sealed record HostedSolver(
    SolverManifest Manifest,
    PluginInstance Instance,
    Negotiation Negotiation,
    Seq<CapabilityDescriptor> Descriptors);

public sealed record SolverHostingRuntime(
    SandboxRuntime Sandbox,
    SandboxRow Row,
    Func<SolverManifest, Fin<PluginArtifact>> Resolve,
    Func<Negotiation, OpDeclaration, Func<CommandArguments, Fin<CommandBody>>> CompileOf,
    Func<Seq<CapabilityDescriptor>, IO<Seq<DescriptorReceipt>>> Project);

public static class SolverHosting {
    public static IO<HostedSolver> Host(SolverHostingRuntime runtime, SolverManifest manifest, GrantScope scope) =>
        (from valid in SolverPluginContract.Validate(manifest)
         from negotiation in Negotiate(valid)
         select (Manifest: valid, Negotiation: negotiation)).Match(
            Succ: proven => Loaded(runtime, proven.Manifest, scope, proven.Negotiation),
            Fail: fault => IO.fail<HostedSolver>(fault));

    // Real material only: Resolve loads component bytes + the cosign bundle from the manifest source
    // through PluginArtifact.From, so a manifest with no resolvable artifact rejects AttestationMissing
    // by construction and a hollow artifact never reaches the gate. The range agreement is proven HERE
    // because the gate parses the artifact's copy and the contract declares the manifest's — one value
    // read at two seams is two values the moment nothing compares them.
    static IO<HostedSolver> Loaded(SolverHostingRuntime runtime, SolverManifest manifest, GrantScope scope, Negotiation negotiation) =>
        from artifact in IO.lift(() => runtime.Resolve(manifest).Bind(resolved =>
            resolved.ContractRange == manifest.ContractRange
                ? Fin.Succ(resolved)
                : Fin.Fail<PluginArtifact>(new SolverFault.ContractRejected($"{manifest.PluginId}: {manifest.ContractRange} != {resolved.ContractRange}"))))
            .Bind(static resolved => resolved.Match(Succ: IO.pure, Fail: IO.fail<PluginArtifact>))
        from instance in SandboxRows.Load(runtime.Row, artifact, scope, runtime.Sandbox)
        let descriptors = SolverPluginContract.Descriptors(manifest, negotiation, runtime.CompileOf)
        from _projected in runtime.Project(descriptors)
        select new HostedSolver(manifest, instance, negotiation, descriptors);

    public static Fin<Negotiation> Negotiate(SolverManifest manifest) =>
        manifest.Reads.Filter(channel => !manifest.Input.Channels.Contains(channel)) is { IsEmpty: true }
            ? Fin.Succ(new Negotiation(
                manifest.Input, manifest.Output,
                manifest.Reads.Fold(HashMap<string, double>(), static (bounds, channel) => bounds.Add(channel.Key, channel.Dtype.Tolerance))))
            : Fin.Fail<Negotiation>(new SolverFault.RepresentationMismatch(
                $"{manifest.Input.Key} channels (produced by {string.Join('/', KindContracts.Producing(manifest.Output).Map(static kind => kind.Key))})",
                string.Join(',', manifest.Reads.Filter(channel => !manifest.Input.Channels.Contains(channel)).Map(static channel => channel.Key))));
}
```

## [05]-[RESEARCH]

(none)

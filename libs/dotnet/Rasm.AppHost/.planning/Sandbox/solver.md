# [APPHOST_SOLVER_PLUGIN]

Extensibility contract for third-party compute extensions: one `SolverKind` axis carries the seven extension categories — solvers, meshers, optimizers, CAM post-processors, material models, field codecs, generative codecs — as rows whose OWN columns name the input representation, the output representation, the effect ceiling, and the progress admission a plugin of that category ships. One manifest declares what a plugin ships, one negotiation proves its declared channel demand against the kernel roster its representation carries, and one hosting fold loads it under the sandbox, binds its grant handle, and projects its declared ops into the capability registry.

Representation is the kernel `Rasm/Drawing/pack` `PackKind` roster composed directly, so a kernel row addition reaches every category with no edit here and `Encode.Apply(PackOp, Op?)` stays the kernel's one encoding entrypoint. Settled composition: `PackKind`/`EncodingChannel`/`ChannelDtype` from the kernel pack owner; `CapabilityDescriptor`/`DescriptorReceipt`/`EffectClass`/`CostModel`/`Idempotency`/`PermissionShape`/`SubscriptionPolicy`/`CommandBody`/`CommandArguments`/`GrantScope` from Agent/capability#COMMAND_ALGEBRA; `McpRuntime` from Agent/mcp; `SandboxRow`/`SandboxRuntime`/`PluginInstance`/`GrantHandle`/`GrantHandleSurface.Bind`/`QuotaControl.Evict`/`EvictionCause` from Sandbox/isolation#ISOLATION_AXIS; `PluginArtifact` from Sandbox/admission#ADMISSION_SUBJECTS; `CapabilitySet<T>`/`ICapability<TSelf>` from Rasm/Domain/validation#CAPABILITY; `Fault`/`FaultBand`/`Op` from Rasm/Domain/rails. This page mints no eighth port and no content key of its own.

## [01]-[INDEX]

- [02]-[SOLVER_KIND]: Seven extension-category rows carrying their contract as instance columns over the kernel `PackKind` representation.
- [03]-[PLUGIN_CONTRACT]: Declared representation, channel demand, ops, and capability descriptors a plugin ships.
- [04]-[SOLVER_HOSTING]: Sandboxed load, grant binding, registry projection, and representation negotiation.

## [02]-[SOLVER_KIND]

- Owner: `SolverKind` `[SmartEnum<string>]` realizing kernel `ICapability<SolverKind>` — the seven extension-category rows carrying `Input`, `Output`, `Effect`, `Progress`, and `Rank` as instance columns, under the `ComparerAccessors.StringOrdinal` accessor; `SolverFault` `[Union]` the fault family riding the kernel `[FaultCase]`/`Fault` floor (`[FaultCase]` realizes the registry over `FaultBand.Solver`; `Code` derive SEALED); the `PackKind`-keyed producer index this roster seeds.
- Cases: solver, mesher, optimizer, cam-postprocessor, material-model, field-codec, generative-codec; `SolverFault` = ContractRejected | RepresentationMismatch | KindUnsupported | Foreign, the last carrying a crossing refusal whole so the sandbox load's and the supply-chain gate's own cause survives adoption.
- Entry: `Producing(PackKind kind)` returns `Seq<SolverKind>` — the reverse index answering which categories emit a requested representation, which the negotiation refusal names so an operator reads the route rather than a bare mismatch.
- Law: the contract lives ON the row, where three parallel seven-member rosters carried it before — bare keys, a sibling `KindContract` roster, and a hand seven-arm `Switch` joining them — so a new category was three edits and a forgotten one still compiled. Columns here are what the kernel does not decide: which category consumes which representation, its effect ceiling, and whether it reports progress.
- Law: `Progress` is `Option<SubscriptionPolicy>` rather than a `Streaming` bool, per the folder ruling that progress rides one column the command algebra seats verbatim; that bool was a lossy pre-image of the value the descriptor wanted, re-derived at the one site reading it, so a batched or immediate cadence had nowhere to land but a second bool.
- Law: `SolverKind` realizes `ICapability<SolverKind>`, so a deployment states which categories it hosts as ONE `CapabilitySet<SolverKind>` value and `KindUnsupported` refuses a manifest naming a category no host seats. That case had no producing arm at all while the boundary claimed the axis governed which extensions load.
- Law: representation IS the kernel roster — `Input` and `Output` are `PackKind` values, so a mesher declares its brep-in mesh-out contract in the one vocabulary the kernel, Compute residency, and every plugin already read, and a kernel row addition arrives with zero edit here.
- Law: the producer index SEEDS from `PackKind.Items`, so a kernel row this axis produces nothing for lands at type initialization with an empty producer set rather than being absent — the loud-on-drift property a roster keyed on its own copies structurally cannot have. Its read is total by construction and never an indexer that throws.
- Receipt: kind resolution is a pure column read; a plugin's solve receipt is the `CommandReceipt` the command algebra mints when its projected descriptor dispatches — no parallel solver receipt.
- Packages: Rasm (kernel `PackKind`/`EncodingChannel`/`ChannelDtype`/`ICapability`/`CapabilitySet`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new extension category is ONE `SolverKind` row carrying its columns, never a parallel plugin contract and never a second roster to keep in step; a new representation is one kernel `PackKind` row every row can already name; a new fault is one `SolverFault` case; zero new surface.
- Boundary: the solver-kind axis is the only extension-category owner — a per-category plugin interface, a category-specific loader, and a parallel solver registry are the deleted forms, so all seven categories ride one contract and one hosting fold differing only by row columns; representation identity is TYPE identity, never a mirrored roster — an AppHost `EncodingKind` re-declaring the kernel keys and a lock table keyed on that mirror are the deleted forms precisely because a kernel row the mirror never grew is structurally invisible to a table its own keys seed, which is how `gaussian-splat` sat unmirrored while three prose lines asserted a 1:1 lock; the kernel owns encode, decode, and the round-trip witness, so a `GeometryPacking`-style AppHost capsule forwarding `Encode.Apply`, a residency-side packer, and a per-plugin geometry codec are all deleted forms and every caller reaches `Encode.Apply(PackOp, Op?)` and `PackKind.Channels` directly; the kind axis is orthogonal to substrate selection — a plugin declares its category and contract while the executing stratum decides the substrate its dispatched op runs on, so plugin extensibility and substrate selection never merge; the generative-codec row carries the generative-run contract shape but AI model execution stays the model lane's concern, so this page hosts the codec contract and never the model; `MaterialModel` and `FieldCodec` share an identical column set today and are held apart by their operator-facing category alone, which is a real discriminant a registry listing reads and a `Producing(PackKind.Field)` answer therefore names both.

```csharp signature
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SolverFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Solver;
    private SolverFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    public static SolverFault Of(Error error) => error as SolverFault ?? new Foreign(error);

    [FaultCase(0)]
    public sealed partial record ContractRejected : SolverFault { public ContractRejected(string detail) : base(detail) { } }
    [FaultCase(1)]
    public sealed partial record RepresentationMismatch : SolverFault { public RepresentationMismatch(string expected, string actual) : base($"{expected}!={actual}") { } }
    [FaultCase(2)]
    public sealed partial record KindUnsupported : SolverFault { public KindUnsupported(string detail) : base(detail) { } }

    [FaultCase(3)]
    public sealed partial record Foreign : SolverFault, ICausedFault {
        public Foreign(Error cause) : base(cause.Message) => Cause = cause;

        public Error Cause { get; }

        public override Retriability Retriability => Cause is Fault fault ? fault.Retriability : Retriability.Terminal;
    }
}

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolverKind : ICapability<SolverKind> {
    public static readonly SolverKind Solver = new("solver",
        rank: 0, input: PackKind.BrepPatch, output: PackKind.Field, effect: EffectClass.Pure, progress: Some(SubscriptionPolicy.Wire));
    public static readonly SolverKind Mesher = new("mesher",
        rank: 1, input: PackKind.BrepPatch, output: PackKind.MeshPatch, effect: EffectClass.Pure, progress: Some(SubscriptionPolicy.Wire));
    public static readonly SolverKind Optimizer = new("optimizer",
        rank: 2, input: PackKind.Field, output: PackKind.Field, effect: EffectClass.Pure, progress: Some(SubscriptionPolicy.Wire));
    public static readonly SolverKind CamPostprocessor = new("cam-postprocessor",
        rank: 3, input: PackKind.MeshPatch, output: PackKind.Toolpath, effect: EffectClass.Write, progress: None);
    public static readonly SolverKind MaterialModel = new("material-model",
        rank: 4, input: PackKind.Field, output: PackKind.Field, effect: EffectClass.Pure, progress: None);
    public static readonly SolverKind FieldCodec = new("field-codec",
        rank: 5, input: PackKind.Field, output: PackKind.Field, effect: EffectClass.Pure, progress: None);
    public static readonly SolverKind GenerativeCodec = new("generative-codec",
        rank: 6, input: PackKind.Field, output: PackKind.MeshPatch, effect: EffectClass.External, progress: Some(SubscriptionPolicy.Wire));

    public int Rank { get; }
    public PackKind Input { get; }
    public PackKind Output { get; }
    public EffectClass Effect { get; }
    public Option<SubscriptionPolicy> Progress { get; }

    public static Seq<SolverKind> Producing(PackKind kind) => Producers.Value.GetValueOrDefault(kind, Seq<SolverKind>());

    private static readonly Lazy<FrozenDictionary<PackKind, Seq<SolverKind>>> Producers = new(
        static () => PackKind.Items.ToFrozenDictionary(
            static kind => kind,
            static kind => toSeq(Items).Filter(row => row.Output == kind)),
        LazyThreadSafetyMode.ExecutionAndPublication);
}
```

## [03]-[PLUGIN_CONTRACT]

- Owner: `SolverManifest` the plugin's declared contract; `OpDeclaration` a single declared op shape carrying its own effect; `SolverPluginContract` the static contract-validation and descriptor-projection surface.
- Entry: `Validate(SolverManifest manifest, CapabilitySet<SolverKind> hosted)` returns `Validation<Error, SolverManifest>` — proves the hosted category, the declared representation pair, the non-empty op set, and every op declaration's effect and published argument schema, ACCUMULATING every refusal; `Descriptors(SolverManifest manifest, Negotiation negotiation, Func<Negotiation, OpDeclaration, Func<CommandArguments, Fin<CommandBody>>> compileOf)` returns `Seq<CapabilityDescriptor>` — the per-op projection into the command algebra.
- Law: validation ACCUMULATES over four independent admissions — hosted category, representation pair, non-empty ops, per-op ceiling and schema — which ran as a `Fin` abort ladder whose bottom arm inverted an `Option` into an error channel, so a manifest both mis-represented and carrying an over-ceiling op reported one cause and re-admitted after a partial fix; traversing the op leg names every offending op in one pass.
- Law: effect is per-OP, not per-kind — the row carries the CEILING and each declared op carries what it performs, so a mixed manifest (a read op beside a write op under a `write` category) is expressible and the forbidden case, an op above its category's ceiling, has a column to refuse on.
- Law: `Progress` crosses HERE and nowhere else — the row's column becomes the descriptor's progress admission, `Agent/capability#COMMAND_ALGEBRA` seats that verbatim on the `Spec` it declares, and the executing stratum's own `ProgressCell.Mint` gates the leaf cell on it — so a non-streaming category's plugin has no cell to advance and the column refuses at the emit site rather than describing a posture nothing enforces.
- Law: the grant scope stays OFF the descriptor row because `GrantScope.Covers` reads the descriptor's `PermissionShape` at mediation, and a scope copied onto the row forks that one authority check.
- Law: the op carries its published argument schema document, not a digest standing in for unavailable bytes; the descriptor adopts that exact document as `ArgumentContract.Published`, while generated contract packages remain the wire-compatibility authority and the catalog pin addresses catalog coordinates alone.
- Receipt: the validation outcome rides one `SpineLog` event; the contract is the manifest, never a separate receipt.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Generator.Equals, Rasm (kernel `PackKind`/`EncodingChannel`/`CapabilitySet`), BCL inbox
- Growth: one declared op is one `OpDeclaration` row on the manifest; a new contract field is one column on `SolverManifest`; zero new surface.
- Boundary: the plugin contract is the only declared-extension owner — a plugin that registers ops by reflection, a category-specific manifest schema, and a runtime-discovered op set are the deleted forms, so a plugin's ops are exactly its declared set and validation gates every one; the manifest's ops become `CapabilityDescriptor` rows, so a solver plugin's solve op is dispatched, metered, and brokered exactly as a built-in op and the plugin gains no privileged execution path; the representation pair is the kernel roster so the contract never admits a plugin-private format; the compiled op targets the `CommandBody` the descriptor's own `Compile` column declares — the executing stratum adopts the `Spec` the command algebra seats and its own intent record never crosses up here, so a fence naming that record is the strata inversion this page does not take; the manifest's `ContractRange` is the ONE declared host-contract range and the hosting fold proves the resolved artifact carries the same value, so the supply-chain gate and the manifest can never disagree about which host a plugin claims; `[Equatable]` keys the manifest structurally because it holds three collections whose synthesized record equality compares by reference, which silently answers false for two byte-identical declarations.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record OpDeclaration(
    string OpId,
    JsonElement ArgumentSchema,
    EffectClass Effect,
    CostModel Cost,
    [property: SetEquality] FrozenSet<string> ObjectSet);

[Equatable]
public sealed partial record SolverManifest(
    string PluginId,
    SolverKind Kind,
    PackKind Input,
    PackKind Output,
    [property: OrderedEquality] Seq<EncodingChannel> Reads,
    [property: OrderedEquality] Seq<OpDeclaration> Ops,
    string ContractRange) {
    public bool Speaks => Input == Kind.Input && Output == Kind.Output;

    public Seq<EncodingChannel> Unmet => Reads.Filter(channel => !Input.Channels.Contains(channel));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolverPluginContract {
    public static Validation<Error, SolverManifest> Validate(SolverManifest manifest, CapabilitySet<SolverKind> hosted) =>
        (Hosted(manifest, hosted), Represents(manifest), Declares(manifest), Ceilings(manifest))
            .Apply(static (held, _represents, _declares, _ceilings) => held)
            .As();

    static Validation<Error, SolverManifest> Hosted(SolverManifest manifest, CapabilitySet<SolverKind> hosted) =>
        hosted.Admits(manifest.Kind)
            ? Success<Error, SolverManifest>(manifest)
            : Fail<Error, SolverManifest>(new SolverFault.KindUnsupported($"{manifest.PluginId}: {manifest.Kind.Key} ∉ <{hosted.Wire}>"));

    static Validation<Error, Unit> Represents(SolverManifest manifest) =>
        manifest.Speaks
            ? Success<Error, Unit>(unit)
            : Fail<Error, Unit>(new SolverFault.RepresentationMismatch(
                $"{manifest.Kind.Input.Key}->{manifest.Kind.Output.Key}", $"{manifest.Input.Key}->{manifest.Output.Key}"));

    static Validation<Error, Unit> Declares(SolverManifest manifest) =>
        manifest.Ops.IsEmpty
            ? Fail<Error, Unit>(new SolverFault.ContractRejected($"{manifest.PluginId}: no ops"))
            : Success<Error, Unit>(unit);

    static Validation<Error, Seq<OpDeclaration>> Ceilings(SolverManifest manifest) =>
        manifest.Ops.Traverse(op =>
            op.Effect.Rank <= manifest.Kind.Effect.Rank && op.ArgumentSchema.ValueKind == JsonValueKind.Object
                ? Success<Error, OpDeclaration>(op)
                : Fail<Error, OpDeclaration>(new SolverFault.ContractRejected(
                    $"{manifest.PluginId}.{op.OpId}: {op.Effect.Key} over {manifest.Kind.Effect.Key} or unschema'd"))).As();

    public static Seq<CapabilityDescriptor> Descriptors(
        SolverManifest manifest, Negotiation negotiation,
        Func<Negotiation, OpDeclaration, Func<CommandArguments, Fin<CommandBody>>> compileOf) =>
        manifest.Ops.Map(op => CapabilityDescriptor.Of(
            surface: $"{manifest.Kind.Key}.{manifest.PluginId}",
            op: op.OpId,
            arguments: new ArgumentContract.Published(op.ArgumentSchema),
            effect: op.Effect,
            idempotency: Idempotency.Keyed,
            cost: op.Cost,
            permission: new PermissionShape(op.ObjectSet, op.Effect, DataClassification.UserContent),
            progress: manifest.Kind.Progress,
            compile: compileOf(negotiation, op)));
}
```

## [04]-[SOLVER_HOSTING]

- Owner: `Negotiation` the proven channel contract the compile closure is built over; `HostedSolver` the loaded, bound, and projected solver capsule; `SolverHostRuntime` the hosting dependency capsule; `SolverHost` the static load-and-project surface and the boot gate the module ledger seats.
- Entry: `Register(SolverHostRuntime runtime, Seq<SolverManifest> declared, GrantScope scope, Op key)` returns `IO<Validation<Error, Seq<HostedSolver>>>` — the boot gate `Runtime/modules#MODULE_LEDGER` composes in the Sandbox module fold, hosting every declared manifest and accumulating every refusal so one boot names each bad plugin; `Host(SolverHostRuntime runtime, SolverManifest manifest, GrantScope scope, Op key)` returns `IO<Validation<Error, HostedSolver>>` — one plugin per call; `Negotiate(SolverManifest manifest)` returns `Validation<Error, Negotiation>` — proves every channel the plugin's decoder declares is one its input `PackKind` tiles and freezes each channel's own `ChannelDtype.Tolerance` as the bound the encode boundary must hold.
- Law: `Register` is the producer this page lacked entirely. Eleven declared owners reached no composition fence, so the whole extensibility contract was law with no producer; the module ledger seats this one entry and every owner below it is reached through it.
- Law: ONE CHANNEL LEAVES THE FOLD. Every per-manifest refusal — a contract leg, an unresolvable artifact, a rejected signature, an unservable isolation axis, a refused registry write — reaches `Register` on `Validation<Error,T>`, where `Error` accumulates every independent refusal. Load legs still sequence on the IO error channel, since a manifest with no artifact must never reach a load; one `IO.fail` crossing that boundary would short-circuit the traversal and leave the boot naming the first bad plugin alone.
- Law: a failing projection EVICTS the loaded plugin. `Host` acquires a `PluginInstance` owning a disposable Wasmtime capsule and previously let a refused registry projection return past it, leaving a linked store alive for process life; the fold now drains the vehicle through `QuotaControl.Evict` under a `CommandedCase` and re-raises, so the acquisition is bracketed by the outcome that owns it.
- Law: the hosting fold BINDS the grant handle, since a loaded plugin holding none reaches no host capability at all; `GrantHandleSurface.Bind` sits inside this fold and the handle rides the capsule, so the sandbox's own dispatch gate reads the plugin's disposition on every call.
- Law: negotiation is kernel-composed and takes no predicate — a channel the plugin reads that its declared representation never carries is a load-time refusal naming that exact channel beside the categories that DO produce the requested output, where a runtime decode surfaces it as a malformed payload instead. Unmet channels fold ONCE on the manifest rather than being computed for the test and again for the message.
- Law: the per-op compile closure is built OVER the returned `Negotiation` and the LOADED instance, so the encode boundary carries the proven per-channel tolerance rather than re-deriving one and the dispatch reaches the guest the fold just admitted; the lossless verdict itself stays the kernel's — `Encode.Apply` mints a `RoundTripWitness` graded against those same `ChannelDtype` bounds on every real encode — so this page declares no second tolerance and no constant-`true` flag no producer measures.
- Law: the manifest's `ContractRange` proves against the resolved artifact's BEFORE the load, so the value the supply-chain gate parses and the value the manifest declares are one; a `Fin` lifted into `IO` and unwrapped by hand became one rail hop.
- Receipt: the load evidence is the `SpineLog` event `SandboxRows.Load` emits — hosting re-mints nothing, and the sandbox's `SandboxReceipt` is EVICTION evidence carrying a drained trap, so a load has no receipt to borrow; hosting carries the `DescriptorReceipt` the registry projection returned, which names every projected op and replaces the second descriptor copy the capsule stored beside it.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions, Generator.Equals, Rasm (kernel `PackKind`/`EncodingChannel`/`ChannelDtype`/`CapabilitySet`/`Op`), BCL inbox
- Growth: one hosted solver is one manifest row in the `Register` roster; a new negotiation rule is one arm on `Negotiate`; zero new surface.
- Boundary: solver hosting is the only solver-load owner — it composes the sandbox load, the grant binding, and the registry projection, never bypassing any, so a hosted solver is always sandboxed and always brokered; the negotiation is the seam between plugin extensibility and the kernel representation — the plugin declares which channels it reads and the kernel roster decides whether that demand is servable, so the canonical representation stays the suite's single geometry truth and the plugin never widens it; a negotiation record carrying a lossless boolean only ever constructed `true`, and a manifest digest beside the artifact content key the admission gate already mints, are both deleted forms — the first measures nothing and the second forks the identity axis, which is also why this page composes no `ContentHash` of its own; the hosted solver's ops dispatch through the same substrate selection every built-in op takes, because the plugin's op is a `CapabilityDescriptor` compiling to a `CommandBody`; a solver plugin gains no lane privilege — its op rides the same `WorkLane`, budget, and lane-drain the built-in ops ride; the gate this page reaches is the supply-chain gate INSIDE `SandboxRows.Load`, never a second direct `Admit` call, so `Sandbox/admission` counts one consumer here and the load path owns the crossing; the three `Func<>` columns on the runtime capsule are per-call effects the composition supplies — resolving an artifact off a plugin source, building a compile closure over a loaded instance and its proven negotiation, and projecting descriptors into the live registry — which is the one shape the folder's capsule law admits a delegate column for; the compile column takes the INSTANCE because its closure crosses into that guest through `SandboxRows.Enter`, and a factory shaped without it is one the composition root cannot bind at all, which is exactly the state it sat in.

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record Negotiation(
    SolverKind Kind,
    [property: UnorderedEquality] HashMap<string, double> Tolerance) {
    public PackKind Input => Kind.Input;
    public PackKind Output => Kind.Output;
}

public sealed record HostedSolver(
    SolverManifest Manifest,
    PluginInstance Instance,
    GrantHandle Handle,
    Negotiation Negotiation,
    Seq<DescriptorReceipt> Projected);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record SolverHostRuntime(
    SandboxRuntime Sandbox,
    SandboxRow Row,
    McpRuntime Mcp,
    CapabilitySet<SolverKind> Hosted,
    Func<SolverManifest, Fin<PluginArtifact>> Resolve,
    Func<PluginInstance, Negotiation, OpDeclaration, Func<CommandArguments, Fin<CommandBody>>> CompileOf,
    Func<Seq<CapabilityDescriptor>, IO<Seq<DescriptorReceipt>>> Project);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolverHost {
    public static IO<Validation<Error, Seq<HostedSolver>>> Register(
        SolverHostRuntime runtime, Seq<SolverManifest> declared, GrantScope scope, Op key) =>
        declared.TraverseM(manifest => Host(runtime, manifest, scope, key)).As()
            .Map(static hosted => hosted.Traverse(static row => row).As());

    public static IO<Validation<Error, HostedSolver>> Host(
        SolverHostRuntime runtime, SolverManifest manifest, GrantScope scope, Op key) =>
        (from valid in SolverPluginContract.Validate(manifest, runtime.Hosted)
         from negotiation in Negotiate(valid)
         select (Manifest: valid, Negotiation: negotiation)).Match(
            Succ: proven => Loaded(runtime, proven.Manifest, scope, proven.Negotiation, key),
            Fail: faults => IO.pure(Fail<Error, HostedSolver>(faults)));

    static IO<Validation<Error, HostedSolver>> Loaded(
        SolverHostRuntime runtime, SolverManifest manifest, GrantScope scope, Negotiation negotiation, Op key) =>
        (from artifact in runtime.Resolve(manifest)
            .Bind(resolved => resolved.ContractRange == manifest.ContractRange
                ? Fin.Succ(resolved)
                : Fin.Fail<PluginArtifact>(new SolverFault.ContractRejected(
                    $"{manifest.PluginId}: {manifest.ContractRange} != {resolved.ContractRange}")))
            .Match(Succ: IO.pure, Fail: IO.fail<PluginArtifact>)
        from instance in SandboxRows.Load(runtime.Row, artifact, scope, runtime.Sandbox, key)
        from hosted in Projected(runtime, manifest, negotiation, instance).Catch(error =>
            QuotaControl.Evict(runtime.Sandbox, instance, new EvictionCause.CommandedCase(nameof(SolverHost)), key)
                .Bind(_ => IO.fail<HostedSolver>(error)))
        select Success<Error, HostedSolver>(hosted))
        .Catch(static error => IO.pure(Fail<Error, HostedSolver>(SolverFault.Of(error))));

    static IO<HostedSolver> Projected(
        SolverHostRuntime runtime, SolverManifest manifest, Negotiation negotiation, PluginInstance instance) =>
        from receipts in runtime.Project(SolverPluginContract.Descriptors(
            manifest, negotiation, (proven, op) => runtime.CompileOf(instance, proven, op)))
        let handle = GrantHandleSurface.Bind(instance, runtime.Mcp)
        select new HostedSolver(manifest, instance, handle, negotiation, receipts);

    public static Validation<Error, Negotiation> Negotiate(SolverManifest manifest) =>
        manifest.Unmet.IsEmpty
            ? Success<Error, Negotiation>(new Negotiation(
                manifest.Kind,
                manifest.Reads.Fold(HashMap<string, double>(), static (bounds, channel) => bounds.Add(channel.Key, channel.Dtype.Tolerance))))
            : Fail<Error, Negotiation>(new SolverFault.RepresentationMismatch(
                $"{manifest.Input.Key} channels (produced by {string.Join('/', SolverKind.Producing(manifest.Output).Map(static kind => kind.Key))})",
                string.Join(',', manifest.Unmet.Map(static channel => channel.Key))));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

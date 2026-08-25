# [DOTNET_BRANCH_RULINGS]

`libs/dotnet` rulings settle branch-spanning decisions.

## [01]-[PACKAGES]

- `Silk.NET.WebGPU` and its `Native.WGPU` train are branch substrate — each consumer composes behind one folder seam, so a swap edits a row per seam.
- `Generator.Equals` injects once at `Directory.Build.props`, never `PrivateAssets` — comparers cross assemblies; `Riok.Mapperly` inverts both.
- Duplicate full type names alias the package the product does NOT name via `PackageReference` — aliasing the pinned one rebinds every mention.
- `Grpc.Tools` refused — a per-csproj `<Protobuf>` item re-emits one contract per consumer at its own `GrpcServices` mode; bindings generate once.
- `Grpc.AspNetCore.Server` is the server-rail admission — the `Grpc.AspNetCore` metapackage ships no assembly and drags in `Grpc.Tools`.
- `libs/contracts/gen/dotnet` emits the sole `Rasm.Contracts` assembly; every workspace consumer reaches it by `ProjectReference`.
- `Rasm.Contracts` consumers keep a direct `Google.Protobuf` row only where their pages name that API — transitive reach is never a manifest row.
- `libs/dotnet` projects design as if PUBLISHED tomorrow — pack metadata rides the root estate behind `IsPackable=false`, one flip from a package.

## [02]-[SHAPE]

- Host-neutral names an absent host flag in a package's own manifest, never freedom from the kernel bundle — all above `Rasm` links RhinoCommon.
- Composition roots home at the `apps/<plugin>/` shell, itself an app — a package blocked on it waits; pulling composition down is rejected.
- Hook-point rosters name `<Package>Point`, never `HookPoint` — the kernel capsule is in scope at every fence, and ids mint through `HookId`.
- Folder hook registries beside the kernel point are earned solely by plugin-identity grant custody — re-keying `(point, scope, token)` is deleted.
- Benchmark-grading AEC peers may reference `Rasm.AppHost` by name; Materials also reaches the branch's one `WireJson` edge.
- `Rasm.Persistence` alone owns columnar lake custody — a producer joins by handing its typed record-batch schema, never folder-local storage.
- `.bcfzip` has ONE codec custodian in `Rasm.Bim` — Persistence joins by typed `IssueTopic` rows the root transcribes, holding the durable half.
- Causal-frame primitives home at the kernel signal capsule — an AppHost home forces an S2-to-spine reference or a `Guid` twin per package.
- Seam property-bag row names are single-owned by their `Rasm.Element` declarer — a call-site `PropertyName.Create` forks the bag's key space.
- `Rasm.AppHost` IS the branch telemetry composition owner — every other tier emits `ILogger` and its minted `Meter` alone, no rank carved out.
- Per-signal `AddOtlpExporter` is the one OTLP form and `UseOtlpExporter` is declined — its builder options are `internal`, pinned policy unsettable.
- Host-boundary egress attaches `DataClassification` at the producer — only it sees a payload's user content, `IRedactorProvider` runs at the root.
- `SpringShape` is the branch's sole spring algebra — a shell or host names retention and epsilon as VALUES, never an inversion of its own.
- Package self-identity homes at the kernel signal capsule — the `ReceiptSinkPort` scope seam is string-typed, so a distant emitter hand-spells it.
- Host beat evidence composes the kernel `MonotonicBeat` — cadence-only columns extend the receipt, and a flat host beat re-mints temporal identity.
- Kernel measures leave as bare `double` and `Rasm.Element` `MeasureValue` is the dimensioned carrier — unit identity federates at `BaseDimensions`.
- `FaultBand` row names never shadow a kernel TYPE in consumer scope — `LaneGuard`/`StoreStat` prefix, a row name being free where a wire key is not.
- No declaration takes the simple name of an ADMITTED PACKAGE type its folder references — `FieldPack`, never `FieldCodec`, beside `Google.Protobuf`.
- Host-boundary gesture and pick receipts stay PLURAL — a viewport pick and a canvas pick share no consumer, so one owner forces a cross-host edge.
- `Option<T>` crosses to a host `T?` slot through `Op.ToHostSlot`/`ToHostNullable` ALONE — the one place `null` is a legal spelling on this branch.
- Value records holding a sequence, array, or map member declare `[Equatable]` with explicit member equality, or hold `Seq`/`Arr` over an array.
- Boundary declarations whose simple name matches a kernel owner RENAME at the boundary — one assembly resolves bare names and the kernel keeps it.
- `FaultBand.OwnerOf` takes the `BandKind` it decodes — the two id spaces partition, so an interval-only read answers whichever row sorts first.
- `Retriability` on `Fault` is the ONE retriability discriminant and `Redrive` the ONE re-drive owner — Polly executes at the HTTP hop alone.
- `ContentHash.Hex` and `ContentHash.Admit` are the ONE identity text and its inverse — admission REFUSES uppercase, so the round trip is exact.
- `Stat<TCarrier>` and `Distribution<TCarrier>` are the ONE moment and order-statistic owners — a local fold re-derives the recurrence and forks it.
- `MonotonicTimeline.Gauged` is the ONE gauged span from S3 upward; the S0 op-cost capsule marks its own pair — the floor cannot read up-strata.
- `MeshSource` is the ONE mesh admission discriminant on `MeshSpace` — a new carrier lands as one arm or one `MeshBlock` band.
- Combinable capability rides `CapabilitySet<TCapability>` over an `ICapability` vocabulary — set algebra collapses every parallel bool column.
- `Transition<TState>` carries every CAS verdict a step can DECLINE or a budget EXHAUST — a refused swap and a committed no-op read one state.
- `PackageIdentity<TKey,THostFact>` at the kernel causal frame is the ONE resolve over `PluginKey`/`HookScope` host keys — a raw string forks both.
- `MotionDrive` at `Rasm/Parametric/projections` is the ONE motion sampler; a host owns its timer lease alone.
- `Rasm/Drawing/sheet` is the ONE drawing-standards owner — a package re-declaring a sheet, scale, or naming row forks the standard it composes.
- Generated-owner roster proofs read through an ACCESSOR-backed lazy — the generator fills `Items` at static init, so an eager fold passes vacuously.
- Material occurrence-usage rides the seam `Associate` edge's typed `MaterialUsage` payload — a parallel usage node double-owns the fact.
- Tenancy crosses as the kernel pair under ONE text — `TenantId.Text` renders through `ContentHash.Hex`, so a raw `Guid` forks alphabets.
- Dimension keys are owner-declared consts — `CorrelationId.Slot` and `TenantContext.TenantSlot` spell the causal frame; a bare noun forks.
- `Sli.Partition` binds ONE counter partitioned on the outcome dimension its arm stamps — a good-half twin doubles the series and strands its floor.
- Package receipt unions carry one kind vocabulary — the `[JsonDerivedType]` roster projected once at type init, a case-to-literal dispatch twins it.
- Typed rails a consumer seam cannot carry outward park on the composing app's evidence cell — a `void` delegate licenses no discard.
- Measurement polarity enforces at one gate — `InstrumentSet.Write` is the pushed entry, `Level` and `Bind` the pulled pair, each refusing the other.
- Span custody is the kernel `SpanBand`'s and a library tier owns none — a folder-local `ActivitySource` re-mints the app's lacing and leaks a scope.
- Host-boundary packages resolve host names bare — one csproj `<Using Alias>` per collision ONLY where one winner serves the assembly, else `global`.
- `TelemetryContributorPort` carries its `BoardPack`, admitted against the port's OWN `Declared` roster before the first meter mints.
- Analytics datasets declare their `TimeSpine` CATEGORY — event-time stamps its own clock column, landing-time reads the custodian's.
- Hook censuses freeze at the composition's ONE `HookRegistry.Mount` — a contributing rail hands its `Points` in and calls nothing itself.
- `ArtifactKind` seats at the lowest stratum both its strata peers reach — seating a taxonomy beside the first consumer's index recurs it.
- Every declared `HookPoint` lands its FIRE SITE in the same change — a veto point nothing fires advertises an admission gate that admits everything.
- Compiled models source the generation artifact ALONE — `MigrationsAssembly` binds nothing, and a second assembly forks the shape one digest names.
- `Rasm` `Deterministic` is the branch's ONE splitmix64 owner. [NOT] a frozen wire constant whose VALUES define a format and re-cut stored payloads.
- Present-but-sealed upstream payloads bind through ONE `[UnsafeAccessor]` capsule at the boundary page, pinned to the manifest — never a re-parse.
- Domain-carrier accessors seat with the field-name roster — the message-envelope owner, else the egress leg; `TraceContext` declares shape alone.
- Bounded wire columns admit one shared ceiling at BOTH ends — `SearchLimit` and `LimitCeiling` are that pair, never per-end literals.
- Unions crossing JSON declare their own `[JsonPolymorphic]` roster — the generator emits none, so an undeclared union serializes `{}` per case.
- Mapperly source-side completeness proves only on reader-free mappings — a whole-source reader suppresses `RMG020`; target-side keeps full force.
- App-platform dispatch vocabulary declares at `Rasm.AppHost` and `Rasm.Compute` composes it downward — the reverse closes a forbidden cycle.
- LanguageExt carriers cross STJ through the kernel `LanguageExtJsonConverterFactory` each wire mint registers — a bare carrier throws on read.
- Merged wires have ONE producer surface, the composition-bound options handle — a `.Default` type-info or context instance drops resolver modifiers.
- Library tiers classify retriability and execute none — the root-bound executor drives, so a tier-local policy forks the estate's one schedule.
- Store bands seat executors by SEAM — `RemoteStoreFault` and `CacheFault` at `OutboundHop`, `CoordinationFault` at the store strategy.
- `HopOutcome` is transport-neutral, so its exception arms alone are HTTP-shaped — a store rail joins the keyed lane as a row, never a second family.
- `MeterVector` crosses as REMAINING BALANCE per unit — a spend reading re-supplies its ceiling at the caller and re-opens the fenced write's TOCTOU.
- ONE `Wgpu` runtime loads per process and device arity is BY ROLE — one presented `Device` every dispatch lane binds, one surfaceless `PressDevice`.
- sRGB transfers READ off `RgbProfile.Srgb` — a local piecewise body forks the branch's one IEC 61966-2-1 curve and clamps its negative reflection.
- Provider error CODES discriminate ahead of status where one status carries two meanings — `InvalidObjectState` inverts by verb.
- Retrying execution strategies admit a non-idempotent tail only under `verifySucceeded` — an ambiguous commit double-applies delta work.
- `Interceptor` and `ISaveChangesInterceptor` land BOTH modality twins — a pass-through default leaves the async path every leg takes unintercepted.
- `RecoveryObjective` declares ONCE at `Rasm.AppHost` and `Rasm.Persistence` imports it — a port shape earns its seat by RE-SHAPING, never by name.
- Declaring a recovery target and grading one split owners — `RecoveryWindow.Gauged` is the one gauge, a `Meets*` twin blind to the unmeasured half.
- Kernel vectors cross as `Vector3d` and a float engine narrows at ITS edge — `SunPosition.Direction` on `Vector3` floors its inverse at `1.1e-3°`.
- Exact planar adjudication is the kernel's — Clipper2/NTS serve float planes, a consumer's tolerance source picks the tier, a verdict never rises.
- Corpus proto descriptors keep ONE spelling — an owner page carries the header-only fence alone; the roster is gate-emitted and a hand mirror forks.
- `params` entrypoints mint their own `Op` at the entry — an optional `Op? key = null` ahead of the spread forecloses every positional call.
- Host-enum reads resolve a `[SmartEnum]` row through `Op.Row` — it folds `Enum.IsDefined` and the ordinal once, so a call-site cast forks admission.
- `Rasm.Bim`'s graph set renames `ElementQuery` and `Rasm.Persistence`'s keyed receipt renames `KeySelection`, both over the seam `Selection<TKey>`.
- `Rasm.Element` `Predicate<ElementLeaf>` is the ONE class-selection closure — `NodeClassSelector` admits through `All(ByKind, ByClassification)`.
- `Rasm.Element` owns `SectionProperties`, the measure-columned cross-section algebra — `Rasm.Fabrication` `Forming/tube`'s mm-basis record renames.
- `Rasm.Element` `EvidenceRun` owns the solver-run audit and retires the name `Provenance` branch-wide — a sourcing or capture record renames.
- `Rasm.Element`'s `[Mapper]` family owns `WireCodec` — `Rasm.Materials`' serialization owner and `Rasm.Bim`'s converter set each rename.
- `AttestationRole` is the ONE attestation-role vocabulary branch-wide — a folder-local role roster deletes onto its report-family roster.
- `SolutionAudit` is `Rasm.Grasshopper`'s completion-audit noun and `RunEvidence` is `Rasm.Fabrication`'s — one name means one thing estate-wide.
- `ComputeEndpoint` is `Rasm.Compute`'s dispatch-endpoint name alone — `Rasm.Rhino`'s host roster renames to `HostEndpoint`/`HostEndpoints`.
- Kernel `Solving` `ObjectiveSense` is the ONE objective-direction vocabulary — `Sign` folds a maximizing objective onto the minimizing kernel.
- `Rasm.Persistence` `ColumnCell.Absent` is the ONE landing absence spelling — `ColumnRow.Admits` proves it against `TableColumn.Nullable`.
- `Rasm.Persistence` `ContentBlobPort` is the ONE key-minting object-plane byte seam — derived off `BlobRemote`, bound at the composition root.
- Kernel `MeasureBundle` is the ONE multi-kind takeoff carrier over `(MassKind, Magnitude)` rows — `GeometryMeasures` stays the single-domain bundle.
- `Fault` is the sole expected-failure base; `KernelFault` its universal family — each bounded local fault family is one direct `[Union]` root.
- `Op.Catch` is the one exception admission — it preserves foreign `Error`; only token-proved cancellation or a typed provider refusal remaps.
- `ICausedFault` carries the original `Error` on every case minted from a capture — `Fault.Inner` projects that cause unchanged.
- Generated value admission crosses ONCE into `KernelFault.InvalidValue`/`OutOfRange` — a package `IValidationError<T>` mints a parallel error plane.
- `Validation<Error,T>` accumulates package faults — a `TFault` rail compiles until `Fail` demands a `Monoid` whose `Empty` means success.
- `KernelInstrument` owns every `rasm.fault.*` key — an emitter prefixing its own estate segment forks one axis into a per-package pair.
- `Rasm.AppHost` `CommandIntent` is the ONE command identity every UI row crosses through `Run` — a second in a referencing package is a strata twin.
- AppHost's `CommandReceipt` and `CommandFault` name suite transaction facts — a referencing package's own evidence and refusal take their own names.
- Factories short-circuiting to one argument hand back that instance — a custody fold probes `ReferenceEquals` per arm before releasing.
- `Rasm.Element` `RepresentationSlot` is the ONE representation roster — a peer composes it and seats its own admission column.
- `Rasm.Materials` `DeclarationUnit` rosters declaration contracts and `Rasm.Compute` `DeclaredUnit` openEPD REST keys — neither set holds the other.
- `Rasm.Materials` `SectionFactors` and `Rasm.Compute` `ResistanceFactors` split on the ALTITUDE word, as `SectionCapacity` splits `MemberCapacity`.

## [03]-[COLLAPSE]

- Host-boundary MECHANISM seats in `Rasm/Interaction/`, which both boundaries reference — only instrument and `<Package>Point` rosters stay plural.
- Instrument-shape bind factories collapse to one kernel `InstrumentSpec` carrying the full kind space beside the `MeasureForm` axis.
- `Sli`, `AlertSeverity`, `PanelKind`, `PanelSpec`, and `BoardPack` are kernel rows every sink composes — hand-typed windows fork alerting.
- `InstrumentSpec` families partition by UCUM unit, never domain case — the case key rides its `Dimensions`, so a landed unit needs no roster edit.
- `Distribution<TCarrier>.Of` and `QuantileSketch` stay two-formed on the exact-versus-estimator split — a stream holds no order statistic.
- Mounted sets fold over any number of meters through one `InstrumentSet.Of` — a root re-folding pre-bound pairs positionally re-mints it.
- `InstrumentSet.Tags` materializes dimension keys at the write entry — a folder re-spelling that fold copies the one materialization.
- `RasterPolicy`/`VisualCodec`/`RasterCodec` and the Rhino/AppUi PDF owners stay plural per stratum — a shared owner forces a cross-stratum edge.
- `SolarPosition` is the kernel's ONE ephemeris almanac — every consumer projects the ANGLES into its own frame, a local Meeus fold being deleted.
- Kernel `FieldNoise` and Materials `ProceduralNoise` stay two-formed — differentiability-gating versus byte-parity is the whole discriminant.
- `ImportedGeometry` is the ONE decoded-geometry carrier at the seam — an absent lane is a missing descriptor, never an empty buffer.
- Colour vocabulary has ONE branch owner — `Configuration` short-circuits on reference equality; space, hue, and view ride THREE axes, not a product.
- `UInt128` extraction composes `ContentHash.Half` with `ContentHash.Wire` and `Admit`; those owners fix byte order for every seam.
- Folder claim rosters stay plural under one `BenchmarkGate` — folding them into a parameterized row erases each closed-roster guard.
- `BenchClaim.Corpus` spells realization by prefix — `corpus-<row.Key>` a committed fixture, `forge-<grade.Key>` a minted Element grade; no literal.
- Perceptual colour has ONE interaction owner, the kernel `Interaction/paint` band — each boundary quantizes to its host colour type at its own edge.
- `Rasm` `FaultBand` is the sole band ledger; `Disjoint` forces inside `FaultId` construction, and rows exist only for realized fault families.
- `CanonicalWriter` at `Rasm/Domain/identity` is the ONE preimage writer — peers compose it and re-export nothing, so one framing keys the estate.
- `HookRail<TPoint,TFact,TOwner>` is the ONE hook mechanism — a folder declares its `<Package>Point` roster and closed fact union alone.
- `ToleranceLane` rows are the ONE branch tolerance vocabulary, each carrying its own `Band` — Fabrication GD&T stackup composes them as scalars.
- `Custody` owns reverse release, failure-only rollback, and settled cleanup — every cleanup error joins the primary through `Error.Many`.
- Kernel `SparseMatrix.Transpose()` is the ONE materialized CSR transpose — an APPLIED transpose stays `Multiply` under `OperatorSense`.
- Kernel `Ranked` at `Domain/stats` is the ONE bounded top-K selection — cost-scheduled queues and best-first frontiers keep `PriorityQueue`.
- `WireFault` stays plural by transport — `FaultBand.LiveWire` and `FaultBand.Wire` mint local failures; remote faults remain opaque evidence.
- ONE producer leg crosses a fault — `Rasm.AppHost` `FaultWire.Raise` over `Grpc.StatusProto`; a folder-local `Error → StatusCode` switch forks it.
- ONE ProtoJSON edge serves the branch — `Rasm.AppHost` `WireJson` with the registry over every `<File>Reflection`; `.Default` is the deleted form.
- Every cross-language C# wire is a generated `Rasm.Contracts` message or a publisher type — a record mirroring one under any codec is the twin.
- MessagePack frames the op-log envelope; its `crdt` slot carries generated `CrdtOpWire` across the registered process seam.
- Fault families carry no category mirror — the union case IS the identity, and a parallel roster publishes a second discriminant per family.
- Family-local `Semigroup`/`Aggregate`/`Combine` folds collapse to `Error.Many` — two accumulation paradigms disagree on flattening and on posture.
- Generic text faults collapse into typed cases — a leaf carrying only `Detail` states no identity, and its string is presentation, never routed.
- `WideColumnFault` folds into `CacheFault` and `TypographyFault` into the theme family — one band split across two roots forks its own interval.

## [04]-[STRUCTURE]

- One package carries one `.api` catalogue at one tier, and the branch README carve decides that tier ahead of consumer count.
- Protocol-binding catalogues seat at the tier their transport carrier holds — promotion above the carrier makes a branch row stack on a folder row.
- Dual-homing a package across `libs/dotnet/.api/` and a folder tier merges the losing rows whole and deletes the file — a redirect stub re-mints it.
- `RhinoCommon` and `Eto` catalogue by namespace-and-subsystem, so the tier law binds the catalogued PARTITION and never the assembly.
- Registry packages sit under `[SUBSTRATE_PACKAGES]` and folder-local under `[DOMAIN_PACKAGES]` — the section token, not its ordinal, is the anchor.
- Fault-band growth follows semantic partitioning — one family and one band stay bijective; only a genuine second family earns another row.
- Conceptual doctrine fences never bind a LIVE `FaultBand` row — an illustrative row carries the example beside the stated allocation obligation.
- `[Union]` record roots seat a member only where every leaf DERIVES it from positional state — a universal cell rides the base column instead.
- `[Union]`/`Switch` case-collapse projects near-zero net LOC — a LOC ceiling falls by unifying repeated helpers, never by case discrimination.
- Projection seam rows enter an `ARCHITECTURE.md` seams map only on a live consumer fence — the row re-enters the moment a consuming page names it.
- `ICapability<TSelf>` rosters ranked by declaration order carry `[NoReorder]` — a reorder restrides every rank, and a domain-fact override unpins.
- Seam edges collapse to ONE per home owner, counterpart, kind, and direction — members join ` + `; a both-ends mint keeps its own arrow.
- Seam labels name shapes DECLARED at the producing package's own fences — a substrate or BCL type sharing the spelling rides no seam edge.
- Seam labels name a member the CONSUMING package spells at its own fence too — a producer declaration alone registers reach, never a crossing.
- Host-boundary seam ends DERIVE — the boundary end keys (kernel owner, sub-domain, kind) and the kernel end joins those members alphabetically.
- Band base and span never mirror into an attribute, constant, additional file, or manifest — a mirror mints a second allocation authority.
- Fault ordinals are explicit and unique within the current family — spec-stage deletions compact the unshipped plane instead of preserving gaps.
- Per-family fault registries do NOT re-enter — `[FaultCase]` ordinals generate the one numeric plane a consumer reads.
- Generator proofs cover ordinal presence, sign, uniqueness, and leaf-directness — RUNTIME `FaultBand` proves SPAN through `Code(offset)`/`Disjoint`.

## [05]-[PROCESS]

- Numeric-identity defects land ahead of the generator pass — moving annotations reads as repair while the collision survives in every stored row.
- MSBuild analyzer verdicts go green by REPAIR, never suppression — severity lives in `.editorconfig` and `RASM0002` refuses project-body overrides.

# [PERSISTENCE_STORE_SCHEMA]

Rasm.Persistence composes framework-owned schema artifacts and the provisioning vocabulary into the generated `Rasm.Contracts.Parity.V1.Backend` contract. One strict semantic projection admits generated rows, the kernel `CanonicalWriter` mints generation identity from known fields, and ProtoJSON carries the admitted message without lending its bytes to identity. One verdict grades a deployment on contract realization and recovery recency.

## [01]-[INDEX]

- [02]-[CONTRACT]: generated backend vocabulary, local artifact sources, capability projection, and contract faults.
- [03]-[IDENTITY]: strict repeated-row admission, semantic merge, and the canonical generation preimage.
- [04]-[PROJECTION]: ProtoJSON publication and the two-proof recovery verdict over realized evidence.
- [05]-[CONFORMANCE]: descriptor-owned validation and foreign ProtoJSON admission.
- [06]-[RESEARCH]: external package and framework capabilities required by this owner.

## [02]-[CONTRACT]

- Owner: generated `Backend`, `Artifact`, and `Capability` messages own the contract vocabulary; `SchemaContract` couples one admitted `Backend` value to its semantic `GenerationId`; `[FaultCase]` keeps every refusal on the kernel fault rail.
- Law: generated `ArtifactRole`, `Provider`, `FailureRank`, and `RestartClass` enums carry every cross-runtime discriminant. `SchemaArtifact` carries local framework bytes and generated enum values without minting a wire twin.
- Law: `CapabilityProjection.Message` lowers each provisioning `ServerExtension` into one generated `Capability`; provisioning retains absence behavior and disruption rank, while the generated enum values alone cross the contract.
- Boundary: operator settings, capacity, coordinates, secrets, schedules, observations, and recovery objectives stay outside `Backend`. Recovery evidence enters the admission verdict after generation identity is settled.
- Packages: Rasm.Contracts (generated parity messages and enums), Rasm.AppHost (`WireJson` ProtoJSON and Celly-backed generated-message admission), Google.Protobuf (`ByteString` and `RepeatedField<T>`), LanguageExt.Core, QuikGraph, Thinktecture.Runtime.Extensions, and kernel `ContentHash`/`CanonicalWriter` beside the generated fault floor.
- Growth: a framework adds one `SchemaArtifact`; a server capability remains one `ServerExtension` row; a contract-shape change lands in the corpus proto and regenerates this boundary.

```csharp signature
using System.Collections.Frozen;
using Google.Protobuf;
using LanguageExt;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.AppHost.Runtime;
using Rasm.Domain;
using Parity = Rasm.Contracts.Parity.V1;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Store;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public readonly partial struct ContractKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim().ToLowerInvariant();
        if (value is not [_, ..]
            || char.IsAsciiDigit(value[0])
            || !value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '.' or '-')) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { value }));
        }
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
// `ArtifactPath` coins an `owner/artifact` PATH, never a minted address: `Rasm.Bim`'s `ArtifactKey` is a content address plus an
// interchange format, and the branch seam map draws that type INTO this namespace region — so the two spellings
// collided on a name while naming opposite identity regimes, and this side takes the one that states its own
// shape. The discriminant is the SPLIT arity, named at both sites.
public readonly partial struct ArtifactPath {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim().ToLowerInvariant();
        if (value is not [_, ..]
            || value.Split('/') is not [var owner, var artifact]
            || owner.Length == 0
            || artifact.Length == 0
            || !value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '/' or '.' or '-')) {
            validationError = new ValidationError(string.Join(" | ", new object?[] { value }));
        }
    }
}

public sealed record SchemaArtifact(
    ArtifactPath Key,
    Parity.ArtifactRole Role,
    ReadOnlyMemory<byte> Content,
    Seq<Parity.Provider> Providers,
    Seq<ArtifactPath> DependsOn);

// Contract failures own their typed payloads; generated case identity supplies their numeric codes.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContractFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Contract;
    private ContractFault() { }

    [FaultCase(0)]
    public sealed partial record InvalidKey(string Value) : ContractFault();
    [FaultCase(1)]
    public sealed partial record DuplicateArtifact(string Key) : ContractFault();
    [FaultCase(2)]
    public sealed partial record MissingDependency(string Owner, string Dependency) : ContractFault();
    [FaultCase(3)]
    public sealed partial record CyclicArtifacts(Seq<string> Keys) : ContractFault();
    [FaultCase(4)]
    public sealed partial record InvalidProjection(Error Cause) : ContractFault(), ICausedFault;
    [FaultCase(5)]
    public sealed partial record ContributionCollision(string Key) : ContractFault();
    [FaultCase(6)]
    public sealed partial record ProjectionRejected(string Detail) : ContractFault();

    public override string Message => Switch(
        invalidKey:        static fault => $"<contract-key:{fault.Value}>",
        duplicateArtifact: static fault => $"<contract-duplicate:{fault.Key}>",
        missingDependency: static fault => $"<contract-dependency:{fault.Owner}:{fault.Dependency}>",
        cyclicArtifacts:   static fault => $"<contract-cycle:{String.Join(',', fault.Keys)}>",
        invalidProjection: static fault => $"<contract-projection:{fault.Cause.Message}>",
        contributionCollision: static fault => $"<contract-collision:{fault.Key}>",
        projectionRejected: static fault => $"<contract-projection:{fault.Detail}>");
}

public static class CapabilityProjection {
    public static Parity.Capability Message(ServerExtension extension) {
        (string requirement, string value) = extension.Admission.Switch(
            preload: static row => ("preload", row.Library),
            baseType: static row => ("base-type", row.Extension.Key),
            accessMethod: static row => ("access-method", row.Method),
            standalone: static row => ("standalone", row.Reason));

        return new Parity.Capability {
            Key = extension.Key,
            Lane = extension.Lane.Key,
            Requirement = requirement,
            RequirementValue = value,
            FailureRank = extension.Absence == FailureRank.Required
                ? Parity.FailureRank.Required
                : extension.Absence == FailureRank.Degradable
                    ? Parity.FailureRank.Degradable
                    : Parity.FailureRank.Observational,
            RestartClass = extension.Restart == RestartClass.Session
                ? Parity.RestartClass.Session
                : extension.Restart == RestartClass.Reload
                    ? Parity.RestartClass.Reload
                    : Parity.RestartClass.Restart,
        };
    }
}
```

## [03]-[IDENTITY]

- Law: `ContractComposition.Compose` builds one generated `Backend` from local artifact bytes and capability rows, publishes artifact and capability rows by ordinal key, and publishes each provider and dependency collection under its own generated-field order.
- Law: `ContractComposition.Project` accepts only the exact canonical repeated-row projection. Foreign documents with reordered artifacts, capabilities, providers, or dependencies refuse instead of re-minting one logical set under a second order.
- Law: `ContractComposition.Merge` derives the contract coordinate from a non-empty contribution set, proves every coordinate equal, unions by key, and refuses every generated-message disagreement before one canonical re-projection mints the merged generation.
- Law: `ContentHash.Of` frames generation from the generated message's known semantic fields: contract string; artifact rows as key, role ordinal, content bytes, provider ordinals, dependency strings; capability rows as their six fields in tag order. Protobuf binary, ProtoJSON, maps, floats, and unknown fields contribute no bytes.
- Law: artifact key ordinal order is the whole wire order. Dependency closure and acyclicity grade the canonical projection without imposing a topological re-order.
- Entry: framework-native compilation supplies canonical artifact bytes; generated messages and descriptor validation own the cross-runtime boundary.

```csharp signature
[ValueObject<UInt128>]
public readonly partial struct GenerationId;

public sealed record SchemaContract(
    Parity.Backend Document,
    GenerationId Generation);

public static class ContractComposition {
    public static Fin<SchemaContract> Compose(
        ContractKey contract,
        IEnumerable<SchemaArtifact> artifacts,
        IEnumerable<ServerExtension> capabilities) {
        var document = new Parity.Backend { Contract = contract.ToString() };
        document.Artifacts.Add(artifacts.Select(Message).OrderBy(static row => row.Key, StringComparer.Ordinal));
        document.Capabilities.Add(capabilities.Select(CapabilityProjection.Message)
            .OrderBy(static row => row.Key, StringComparer.Ordinal));
        return Admit(document);
    }

    public static Fin<SchemaContract> Merge(Seq<SchemaContract> contributions) =>
        contributions.Head.ToFin((Error)new ContractFault.ProjectionRejected("<empty-contribution-set>"))
            .Bind(head => {
                Seq<Parity.Artifact> artifacts = contributions.Bind(static one => toSeq(one.Document.Artifacts));
                Seq<Parity.Capability> capabilities = contributions.Bind(static one => toSeq(one.Document.Capabilities));
                Seq<string> contracts = contributions.Map(static one => one.Document.Contract);

                return (contracts.Filter(contract => contract != head.Document.Contract)
                            .Traverse(contract => (Validation<Error, Unit>)new ContractFault.ProjectionRejected(
                                $"<contract-mismatch:{head.Document.Contract}:{contract}>")).As().Map(static _ => unit),
                        Collided(artifacts, static row => row.Key, static (left, right) => left.Equals(right)),
                        Collided(capabilities, static row => row.Key, static (left, right) => left.Equals(right)))
                    .Apply(static (_, _, _) => unit).As().ToFin()
                    .Bind(_ => {
                        var merged = new Parity.Backend { Contract = head.Document.Contract };
                        merged.Artifacts.Add(artifacts
                            .DistinctBy(static row => row.Key, StringComparer.Ordinal)
                            .OrderBy(static row => row.Key, StringComparer.Ordinal));
                        merged.Capabilities.Add(capabilities
                            .DistinctBy(static row => row.Key, StringComparer.Ordinal)
                            .OrderBy(static row => row.Key, StringComparer.Ordinal));
                        return Admit(merged);
                    });
            });

    internal static Fin<SchemaContract> Project(Parity.Backend admitted) =>
        Proof(admitted).ToFin().Bind(_ => Mint(admitted));

    static Fin<SchemaContract> Admit(Parity.Backend document) =>
        WireAdmission.Admit(document, WireBoundary.OutboundPayload, Op.Of())
            .MapFail(static error => (Error)new ContractFault.InvalidProjection(error))
            .Bind(Project);

    internal static Validation<Error, Unit> Proof(Parity.Backend document) {
        Parity.Backend canonical = Canonical(document);
        FrozenSet<string> keys = canonical.Artifacts
            .Select(static row => row.Key)
            .ToFrozenSet(StringComparer.Ordinal);
        var graph = new AdjacencyGraph<string, Edge<string>>(allowParallelEdges: false);
        graph.AddVertexRange(keys);
        foreach (Parity.Artifact row in canonical.Artifacts) {
            foreach (string dependency in row.DependsOn.Where(keys.Contains)) {
                graph.AddEdge(new Edge<string>(dependency, row.Key));
            }
        }

        IEnumerable<ContractFault> findings = (document.Equals(canonical)
            ? Enumerable.Empty<ContractFault>()
            : [new ContractFault.ProjectionRejected("<noncanonical-contract-order>")])
            .Concat(canonical.Artifacts
                .GroupBy(static row => row.Key, StringComparer.Ordinal)
                .Where(static group => group.Count() > 1)
                .Select(static group => (ContractFault)new ContractFault.DuplicateArtifact(group.Key)))
            .Concat(canonical.Artifacts.SelectMany(row => row.DependsOn
                .Where(dependency => !keys.Contains(dependency))
                .Select(dependency => (ContractFault)new ContractFault.MissingDependency(row.Key, dependency))))
            .Concat(graph.IsDirectedAcyclicGraph()
                ? Enumerable.Empty<ContractFault>()
                : [new ContractFault.CyclicArtifacts(toSeq(canonical.Artifacts
                    .Where(static row => row.DependsOn.Count > 0)
                    .Select(static row => row.Key)))]);

        return toSeq(findings)
            .Traverse(fault => (Validation<Error, Unit>)fault)
            .As().Map(static _ => unit);
    }

    static Validation<Error, Unit> Collided<TRow>(
        Seq<TRow> rows,
        Func<TRow, string> key,
        Func<TRow, TRow, bool> same)
        where TRow : class {
        Seq<string> keys = toSeq(rows
            .GroupBy(key, StringComparer.Ordinal)
            .Where(group => group.Skip(1).Any(row => !same(group.First(), row)))
            .Select(static group => group.Key));
        return keys.Traverse(key => (Validation<Error, Unit>)new ContractFault.ContributionCollision(key))
            .As().Map(static _ => unit);
    }

    static Parity.Artifact Message(SchemaArtifact source) {
        var artifact = new Parity.Artifact {
            Key = source.Key.ToString(),
            Role = source.Role,
            Content = ByteString.CopyFrom(source.Content.Span),
        };
        artifact.Providers.Add(source.Providers.Distinct().OrderBy(static row => (int)row));
        artifact.DependsOn.Add(source.DependsOn.Map(static row => row.ToString())
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        return artifact;
    }

    static Parity.Backend Canonical(Parity.Backend source) {
        var canonical = new Parity.Backend { Contract = source.Contract };
        canonical.Artifacts.Add(source.Artifacts.OrderBy(static row => row.Key, StringComparer.Ordinal).Select(row => {
            var artifact = new Parity.Artifact {
                Key = row.Key,
                Role = row.Role,
                Content = row.Content,
            };
            artifact.Providers.Add(row.Providers.OrderBy(static provider => (int)provider));
            artifact.DependsOn.Add(row.DependsOn.OrderBy(static dependency => dependency, StringComparer.Ordinal));
            return artifact;
        }));
        canonical.Capabilities.Add(source.Capabilities
            .OrderBy(static row => row.Key, StringComparer.Ordinal)
            .Select(static row => new Parity.Capability {
                Key = row.Key,
                Lane = row.Lane,
                Requirement = row.Requirement,
                RequirementValue = row.RequirementValue,
                FailureRank = row.FailureRank,
                RestartClass = row.RestartClass,
            }));
        return canonical;
    }

    static Fin<SchemaContract> Mint(Parity.Backend document) =>
        Fin.Succ(new SchemaContract(document, GenerationId.From(ContentHash.Of(document, Preimage))));

    static void Preimage(Parity.Backend document, CanonicalWriter writer) => writer
        .String(document.Contract)
        .Rows(toSeq(document.Artifacts), static (row, artifact) => artifact
            .String(row.Key)
            .Ordinal((int)row.Role)
            .Bytes(row.Content.Span)
            .Rows(toSeq(row.Providers), static (provider, providers) => providers.Ordinal((int)provider))
            .Rows(toSeq(row.DependsOn), static (dependency, dependencies) => dependencies.String(dependency)))
        .Rows(toSeq(document.Capabilities), static (row, capability) => capability
            .String(row.Key)
            .String(row.Lane)
            .String(row.Requirement)
            .String(row.RequirementValue)
            .Ordinal((int)row.FailureRank)
            .Ordinal((int)row.RestartClass));
}
```

## [04]-[PROJECTION]

- Owner: `ContractProjection.Emit` carries one generated `Backend` as ProtoJSON beside its semantic generation; `BackendObservation` carries realized capabilities, artifacts, and recovery stamps; `RecoveryAxis` owns each absence law; `BackendAdmission` grades the complete observation.
- Law: `BackendObservation.Of` projects a `ProvisionVerdict.Provisioned` onto the observed capability set; the verdict already names the exact `ServerExtension` keys the contract's capability rows carry, so this branch interposes no canonical-to-local adapter table, and artifact evidence arrives from the owners that realized each artifact.
- Law: the emitted set is THIS branch's contribution conforming to the corpus `BACKEND_CONTRACT` schema; a C#-only application deploys it directly, and a polyglot application merges it with peer contributions at the app root by artifact key.
- Law: each branch mints its own generated contribution and admits peer ProtoJSON through its generated-message boundary; transported octets never become a canonical-content surrogate.
- Law: adapters compare expected generation and observed evidence; availability or desired declarations prove nothing.
- Law: contract identity and data recency are TWO proofs on one verdict, never two generations — `GenerationDrift` proves the store carries the composed contract off the existing digest, and `RecoveryWindowExceeded` proves the data behind it is recent enough for the window the deployment declared, so a verdict carrying one alone cannot tell a moved schema from an intact schema behind a stale recovery point; `RecoveryWindow` derives both halves from the observation's OWN stamps — a lag admits at ZERO, the freshest measured recency, while only a frontier stamped after its own reading is skew dropping to unmeasured — and the two halves absorb absence oppositely under each `RecoveryAxis` row's declared law.
- Entry: `public static BackendVerdict Admit(SchemaContract expected, BackendObservation observed, RecoveryObjective objective)` grades one contract against one observation, taking the declared objective as a PARAMETER — the composition root threads its `ResolvedProfile.Recovery` value in — so deployment shape reaches this owner as data and no runtime import inverts the strata.
- Output: `contract.json` carries the generated message's ProtoJSON inside a generation-qualified publication.
- Packages: Rasm.AppHost (`Runtime/profiles#PROFILE_AXIS` `RecoveryObjective`), NodaTime carrying the observation stamps and the durations both recovery halves gauge in.
- Growth: a new recovery axis is one `RecoveryAxis` row carrying its measured accessor, its declared accessor, and its own absence law, beside its matching column on the `ResolvedProfile.Recovery` objective a composition root fills — the one `Gauged` fold then carries it into the verdict and the headroom readout alike.
- Boundary: providers execute native migrations and provisioning; this owner neither synthesizes DDL nor orchestrates deployment; recovery evidence stays OBSERVATION-side, so a stamp, a lag, or an objective never enters the contract wire and `RecoveryObjective` is read settled from the profile row rather than re-spelled as a second DR vocabulary.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed record ContractProjection(
    GenerationId Generation,
    string File,
    ReadOnlyMemory<byte> ProtoJson) {
    public static Fin<ContractProjection> Emit(SchemaContract contract) =>
        BackendConformance.Emit(contract)
            .Map(protoJson => new ContractProjection(contract.Generation, "contract.json", protoJson));
}

// Recovery axes carrying OPPOSITE absence laws, which is the whole split: an observation that took no frontier
// reading proves no recency and refuses, while a store that never restored owes no bounce time and passes.
// `Refusing` rides the ROW so a third axis states its own absence law instead of a fold branching on which axis
// it walks, and the two accessor columns keep the gauge one fold over `Items` rather than a per-axis body.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RecoveryAxis {
    public static readonly RecoveryAxis Rpo = new("rpo", refusing: true,
        static window => window.Rpo, static objective => objective.Rpo);
    public static readonly RecoveryAxis Rto = new("rto", refusing: false,
        static window => window.Rto, static objective => objective.Rto);

    public bool Refusing { get; }
    public Func<RecoveryWindow, Option<Duration>> Measured { get; }
    public Func<RecoveryObjective, Duration> Declared { get; }

    private RecoveryAxis(string key, bool refusing,
        Func<RecoveryWindow, Option<Duration>> measured,
        Func<RecoveryObjective, Duration> declared) : this(key) =>
        (Refusing, Measured, Declared) = (refusing, measured, declared);
}

// One reading per axis answers BOTH questions an operator asks — whether that half breached, and what headroom it
// kept — so the verdict and the readout fold ONE row set instead of a hand-written body per answer that a third
// axis row reaches only half of. `Measured` absent IS the unmeasured state and never a zero, since a constructed
// zero publishes a reading no provider took, and `Breached` resolves that absence through the axis's own law
// rather than a branch naming RPO by hand.
public readonly record struct RecoveryReading(RecoveryAxis Axis, Option<Duration> Measured, Duration Declared) {
    public bool Breached => Measured.Match(Some: held => held > Declared, None: () => Axis.Refusing);
    public Option<Duration> Headroom => Measured.Map(held => Declared - held);
}

// Measured window the declared objective grades against. Each half is optional because each has a real absence a
// zero would forge: a provider that took no frontier reading, a live store that was never restored.
public readonly record struct RecoveryWindow(Option<Duration> Rpo, Option<Duration> Rto) {
    // `Gauged` carries every axis into one reading set the verdict filters and the operator reads headroom off,
    // so a third axis row lands in both answers at once. An admitted generation therefore always carries a
    // measured recency half, while an absent bounce is still the store that never restored.
    public Seq<RecoveryReading> Gauged(RecoveryObjective objective) =>
        toSeq(RecoveryAxis.Items)
            .Map(axis => new RecoveryReading(axis, axis.Measured(this), axis.Declared(objective)));

    public Seq<RecoveryReading> Exceeding(RecoveryObjective objective) =>
        Gauged(objective).Filter(static reading => reading.Breached);
}

// `HeldCapabilities` is the TYPED capability column the verification verdict now carries, so the observation
// stops erasing a rostered vocabulary into text one hop after the probe minted it; the untrusted side is the
// CONTRACT's wire rows, and `CapabilitySet.Admits(string)` is exactly the boundary arm that resolves one of those
// tokens against the vocabulary before any membership test. `HeldArtifacts` stays text because an artifact key is
// a name a CONTRIBUTOR coins rather than a row this estate declares.
public sealed record BackendObservation(
    GenerationId Generation,
    CapabilitySet<ServerExtension> HeldCapabilities,
    FrozenSet<string> HeldArtifacts,
    Instant ObservedAt,
    Option<Instant> Frontier,
    Option<Duration> RestoredIn) {
    // Lag derives HERE off the two stamps this observation carries, so no provider hands in a window it computed
    // against a clock the verdict never saw. `ObservedAt` is this adapter's own read and never absent, `Frontier`
    // names the newest datum the store proves durable, and `RestoredIn` names the restore's own span — each one
    // stated or stated missing, since a default lets a provider skip the question and grade a window nobody took.
    // Lag SIGN alone discriminates, and ZERO sits on the measured side: a frontier stamped at its own reading
    // instant is the freshest store this verdict exists to admit, where a frontier stamped AFTER the reading is
    // skew whose negative lag drops to unmeasured and refuses.
    public RecoveryWindow Window => new(
        Frontier.Map(seen => ObservedAt - seen).Filter(static lag => lag >= Duration.Zero),
        RestoredIn);

    // `Created` is the cluster's realized extension set read in the one verification batch, and contract
    // capability rows are keyed by that same `ServerExtension.Key` space, so the projection is direct. Artifact
    // evidence enters as a caller argument because only the migration and storage owners that realized an
    // artifact can witness it; a desired declaration is not evidence.
    public static BackendObservation Of(
        GenerationId observed,
        ProvisionVerdict.Provisioned cluster,
        FrozenSet<string> heldArtifacts,
        Instant observedAt,
        Option<Instant> frontier,
        Option<Duration> restoredIn) =>
        new(observed, cluster.Created, heldArtifacts, observedAt, frontier, restoredIn);
}

[Union]
public abstract partial record BackendVerdict {
    public sealed record Admitted(BackendObservation Observation) : BackendVerdict;
    public sealed record GenerationDrift(GenerationId Expected, GenerationId Observed) : BackendVerdict;
    public sealed record CapabilityGap(Seq<string> Keys) : BackendVerdict;
    public sealed record ArtifactGap(Seq<string> Keys) : BackendVerdict;
    public sealed record RecoveryWindowExceeded(Seq<RecoveryReading> Breaches) : BackendVerdict;
}

public static class BackendAdmission {
    // Identity and recency are two proofs on ONE verdict: drift answers whether the store carries the composed
    // contract, and the window answers whether the data behind it is current. Recency seats LAST, after the
    // realization arms, because a store missing the artifacts has no window worth grading, and the ladder is
    // what separates a moved schema from an intact schema behind a stale recovery point. `objective` rides in as
    // one parameter off the caller's resolved profile row, so this owner reads deployment shape as data.
    public static BackendVerdict Admit(
        SchemaContract expected, BackendObservation observed, RecoveryObjective objective) {
        // Wire rows carry untrusted key text, so the membership test takes the capability column's BOUNDARY arm,
        // which resolves that token against the vocabulary first — a key no row names can never read as held.
        Seq<string> requiredCapabilities = toSeq(expected.Document.Capabilities
            .Where(static row => row.FailureRank == Parity.FailureRank.Required)
            .Select(static row => row.Key)
            .Where(key => !observed.HeldCapabilities.Admits(key)));
        Seq<string> requiredArtifacts = toSeq(expected.Document.Artifacts
            .Select(static row => row.Key)
            .Where(key => !observed.HeldArtifacts.Contains(key)));
        Seq<RecoveryReading> breaches = observed.Window.Exceeding(objective);

        return observed.Generation != expected.Generation
            ? new BackendVerdict.GenerationDrift(expected.Generation, observed.Generation)
            : !requiredCapabilities.IsEmpty
                ? new BackendVerdict.CapabilityGap(requiredCapabilities)
                : !requiredArtifacts.IsEmpty
                    ? new BackendVerdict.ArtifactGap(requiredArtifacts)
                    : !breaches.IsEmpty
                        ? new BackendVerdict.RecoveryWindowExceeded(breaches)
                        : new BackendVerdict.Admitted(observed);
    }
}
```

## [05]-[CONFORMANCE]

- Owner: `BackendConformance.Emit` writes an admitted generated `Backend` through the branch ProtoJSON edge; `BackendConformance.Project` parses and Celly-validates foreign ProtoJSON before the strict semantic projection mints generation identity.
- Law: `WireAdmission` owns the descriptor registry and Celly evaluator; `WireJson` owns the unknown-field posture, recursion bound, and ProtoJSON pair. This page builds no validator, parser, formatter, JSON Schema, STJ context, or hand-authored conformance document beside them.
- Law: `DocumentCeiling` rejects transported ProtoJSON before generated decode and emitted ProtoJSON before publication. Its 512 KiB budget sits beneath the 1 MiB ConfigMap residence after base64 and object metadata; descriptor string, content, and repeated-field ceilings remain the constructed-message floor.
- Law: transported ProtoJSON octets survive as deployment content only. `Project` never compares them with a local re-encode and `ContractComposition.Preimage` never reads them.
- Law: descriptor admission precedes canonical repeated-row proof; a schema-valid document with reordered rows refuses before generation minting.
- Boundary: conformance proves contract projection semantics; realized provider and recovery evidence still enter through `BackendAdmission`.

```csharp signature
public static class BackendConformance {
    const int DocumentCeiling = 512 * 1024;

    public static Fin<ReadOnlyMemory<byte>> Emit(SchemaContract contract) =>
        Op.Of().Catch(() => {
            using var sink = new MemoryStream();
            WireJson.Write(contract.Document, sink);
            ReadOnlyMemory<byte> document = sink.ToArray();
            return document.Length <= DocumentCeiling
                ? Fin.Succ<ReadOnlyMemory<byte>>(document)
                : throw new InvalidDataException($"backend contract exceeds {DocumentCeiling} bytes");
        }).MapFail(static error => (Error)new ContractFault.InvalidProjection(error));

    public static Fin<SchemaContract> Project(ReadOnlyMemory<byte> protoJson) {
        if (protoJson.Length > DocumentCeiling) {
            return Fin.Fail<SchemaContract>(new ContractFault.ProjectionRejected(
                $"<contract-document-ceiling:{protoJson.Length}:{DocumentCeiling}>"));
        }
        using var source = new MemoryStream(protoJson.ToArray(), writable: false);
        return WireJson.Read<Parity.Backend>(source, Op.Of())
            .MapFail(static error => (Error)new ContractFault.InvalidProjection(error))
            .Bind(ContractComposition.Project);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

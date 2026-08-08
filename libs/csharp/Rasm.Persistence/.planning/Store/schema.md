# [PERSISTENCE_STORE_SCHEMA]

Rasm.Persistence composes framework-owned schema artifacts and the provisioning vocabulary into one immutable `SchemaContract`. One canonical UTF-8 projection mints the generation identity, JSON Schema, language-neutral contract artifact, and conformance corpus. EF, Marten, raw SQL, providers, deploy code, and runtimes retain their native execution models; none may mint a parallel schema identity or restate the capability catalog.

## [01]-[INDEX]

- [02]-[CONTRACT]: admitted artifact graph, existing capability rows, and generation-bearing boundary.
- [03]-[IDENTITY]: deterministic composition, canonical bytes, contract schema, and generation identity.
- [04]-[PROJECTION]: one artifact set consumed by TypeScript, Python, IaC, fixtures, and runtime adapters.
- [05]-[CONFORMANCE]: derived corpus and adapter admission without copied constants.
- [06]-[RESEARCH]: external package and framework capabilities required by this owner.

## [02]-[CONTRACT]

- Owner: `SchemaContract` composes content-addressed artifacts already produced by EF, Marten, and raw SQL owners.
- Law: `CapabilityContract` projects each `ServerExtension` onto the wire carrying both closed vocabularies — `FailureRank` and `RestartClass` ride as their own types and reach the wire as keys; provisioning owns the roster, the absence policy, and the restart rank order an aggregated repair folds through.
- Boundary: operator settings, capacity, coordinates, secrets, schedules, observations, and recovery points never enter identity.
- Packages: Thinktecture, LanguageExt, QuikGraph, `System.Text.Json.Schema`, JsonSchema.Net, and kernel `ContentHash`.
- Growth: a framework adds one artifact row; a server capability remains one `ServerExtension` row; no schema DSL grows here.

```csharp signature
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using Json.Schema;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Store;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ContractFault>]
public readonly partial struct ContractKey {
    static partial void ValidateFactoryArguments(ref ContractFault? validationError, ref string value) {
        value = value.Trim().ToLowerInvariant();
        if (value is not [_, ..]
            || char.IsAsciiDigit(value[0])
            || !value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '.' or '-')) {
            validationError = new ContractFault.InvalidKey(value);
        }
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ContractFault>]
public readonly partial struct ArtifactKey {
    static partial void ValidateFactoryArguments(ref ContractFault? validationError, ref string value) {
        value = value.Trim().ToLowerInvariant();
        if (value is not [_, ..]
            || value.Split('/') is not [var owner, var artifact]
            || owner.Length == 0
            || artifact.Length == 0
            || !value.All(static ch => char.IsAsciiLetterOrDigit(ch) || ch is '/' or '.' or '-')) {
            validationError = new ContractFault.InvalidKey(value);
        }
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArtifactRole {
    public static readonly ArtifactRole RelationalModel = new("relational-model");
    public static readonly ArtifactRole MigrationBundle = new("migration-bundle");
    public static readonly ArtifactRole EventStorage = new("event-storage");
    public static readonly ArtifactRole SqlObjectSet = new("sql-object-set");
    public static readonly ArtifactRole SemanticProbeSet = new("semantic-probe-set");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BackendProvider {
    public static readonly BackendProvider PostgreSql = new("postgresql");
    public static readonly BackendProvider Sqlite = new("sqlite");
    public static readonly BackendProvider Pglite = new("pglite");
}

public sealed record SchemaArtifact(
    ArtifactKey Key,
    ArtifactRole Role,
    UInt128 Content,
    Seq<BackendProvider> Providers,
    Seq<ArtifactKey> DependsOn);

public sealed record CapabilityContract(
    string Key,
    string Lane,
    string Requirement,
    string RequirementValue,
    FailureRank Rank,
    RestartClass Restart) {
    // Both vocabularies ride as their own owners here and flatten to keys only at the wire, so a capability row
    // carries the provisioning rank order the aggregated-repair fold reads rather than a re-parsed token.
    public static CapabilityContract From(ServerExtension extension) {
        (string requirement, string value) = extension.Admission.Switch(
            preload: static row => ("preload", row.Library),
            baseType: static row => ("base-type", row.Extension),
            accessMethod: static row => ("access-method", row.Method),
            standalone: static row => ("standalone", row.Reason));

        return new(
            extension.Key,
            extension.Lane,
            requirement,
            value,
            extension.Rank,
            extension.Restart);
    }
}

[Union]
public abstract partial record ContractFault : Expected, IValidationError<ContractFault> {
    private ContractFault() : base() { }

    public sealed record InvalidKey(string Value) : ContractFault;
    // Composition faults carry WIRE tokens because the projection funnel proves them over a decoded stream whose
    // keys are untrusted text, and one collision case names an artifact or capability key from one flat space.
    public sealed record DuplicateArtifact(string Key) : ContractFault;
    public sealed record MissingDependency(string Owner, string Dependency) : ContractFault;
    public sealed record CyclicArtifacts(Seq<string> Keys) : ContractFault;
    public sealed record InvalidProjection(string Detail) : ContractFault;
    public sealed record ContributionCollision(string Key) : ContractFault;

    public override int Code => FaultBand.Contract + Switch(
        invalidKey:        static _ => 0,
        duplicateArtifact: static _ => 1,
        missingDependency: static _ => 2,
        cyclicArtifacts:   static _ => 3,
        invalidProjection: static _ => 4,
        contributionCollision: static _ => 5);

    public override string Message => Switch(
        invalidKey:        static fault => $"<contract-key:{fault.Value}>",
        duplicateArtifact: static fault => $"<contract-duplicate:{fault.Key}>",
        missingDependency: static fault => $"<contract-dependency:{fault.Owner}:{fault.Dependency}>",
        cyclicArtifacts:   static fault => $"<contract-cycle:{String.Join(',', fault.Keys)}>",
        invalidProjection: static fault => $"<contract-projection:{fault.Detail}>",
        contributionCollision: static fault => $"<contract-collision:{fault.Key}>");

    public override string Category => Switch(
        invalidKey:        static _ => "Admission",
        duplicateArtifact: static _ => "Composition",
        missingDependency: static _ => "Composition",
        cyclicArtifacts:   static _ => "Composition",
        invalidProjection: static _ => "Projection",
        contributionCollision: static _ => "Composition");

    public static ContractFault Create(string message) => new InvalidProjection(message);
}
```

## [03]-[IDENTITY]

- Law: `ContractComposition.Merge` folds branch contributions into the deployment unit — union by artifact key under the same ordinal order, one re-projection minting the merged generation, and a `ContributionCollision` refusal wherever two branches claim one key with differing content, capability rows judged on their whole record and artifacts on their content digest.
- Law: artifact key ordinal order is the whole wire order — a dependency-depth or topological rank inside the stream mints a second generation from one artifact set, so no path re-sorts.
- Law: `ContractComposition.Project` is the one funnel every mint path reaches, proving key uniqueness, dependency closure, acyclicity, and the closed capability vocabulary before a byte is framed; a proof bolted to an ordering function leaves every path skipping that function unproved.
- Law: source-generated `System.Text.Json` writes a scalar-and-array wire shape with no dictionary-order ambiguity.
- Law: kernel `ContentHash.Of` mints the `UInt128` generation key; no cryptographic or language-local digest competes.
- Law: `JsonSchemaExporter` derives the JSON Schema from the same source-generated contract used for serialization.
- Law: `CapabilityContract`→`CapabilityWire` is generated `[Mapper]` transcription — the `Rank.Key`/`Restart.Key` SmartEnum keys flatten under `[MapProperty]` — while `ArtifactWire` keeps its hand copyist because provider dedup-and-order is a value transform no mapper expresses.
- Entry: callers supply admitted artifacts, and framework-native compilation produces each artifact's canonical content bytes.

```csharp signature
public sealed record ArtifactWire(
    string Key,
    string Role,
    string Content,
    ImmutableArray<string> Providers,
    ImmutableArray<string> DependsOn);

public sealed record CapabilityWire(
    string Key,
    string Lane,
    string Requirement,
    string RequirementValue,
    string FailureRank,
    string RestartClass);

public sealed record SchemaContractWire(
    string Contract,
    ImmutableArray<ArtifactWire> Artifacts,
    ImmutableArray<CapabilityWire> Capabilities);

[ValueObject<UInt128>]
public readonly partial struct GenerationId;

public sealed record SchemaContract(
    SchemaContractWire Wire,
    ReadOnlyMemory<byte> Canonical,
    GenerationId Generation,
    ReadOnlyMemory<byte> JsonSchema,
    JsonSchema Validator);

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    GenerationMode = JsonSourceGenerationMode.Metadata | JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(SchemaContractWire))]
[JsonSerializable(typeof(ConformanceCorpus))]
public sealed partial class BackendJson : JsonSerializerContext;

public static class ContractComposition {
    // Composition sorts by artifact key and nothing else, then funnels: dependency keys are digest-bearing
    // payload the projection proves rather than an ordering input, so one artifact set mints one generation
    // whatever order its caller supplies it in.
    public static Fin<SchemaContract> Compose(
        ContractKey contract,
        IEnumerable<SchemaArtifact> artifacts,
        IEnumerable<ServerExtension> capabilities) =>
        Project(new SchemaContractWire(
            contract.ToString(),
            [.. artifacts
                .OrderBy(static row => row.Key.ToString(), StringComparer.Ordinal)
                .Select(Wire)],
            [.. capabilities
                .Select(CapabilityContract.From)
                .OrderBy(static row => row.Key, StringComparer.Ordinal)
                .Select(CapabilityMap.Wire)]));

    // Polyglot merge unions branch contributions by artifact key under the one ordinal order every mint path
    // shares, then re-projects so the merged generation mints over the merged canonical bytes. Two branches
    // claiming one key with differing content elect no winner, so the fold refuses rather than letting
    // enumeration order decide — capability rows on their whole record, because a lane, requirement, rank, or
    // restart-class disagreement is the same fork as a byte disagreement.
    public static Fin<SchemaContract> Merge(ContractKey contract, Seq<SchemaContract> contributions) {
        Seq<ArtifactWire> artifacts = contributions.Bind(static one => toSeq(one.Wire.Artifacts));
        Seq<CapabilityWire> capabilities = contributions.Bind(static one => toSeq(one.Wire.Capabilities));

        Option<ContractFault> onArtifacts = Collided(artifacts, static row => row.Key, static row => row.Content);
        return (onArtifacts.IsSome
                ? onArtifacts
                : Collided(capabilities, static row => row.Key, static row => row))
            .Match(
                Some: Fin.Fail<SchemaContract>,
                None: () => Project(new SchemaContractWire(
                    contract.ToString(),
                    [.. artifacts
                        .DistinctBy(static row => row.Key, StringComparer.Ordinal)
                        .OrderBy(static row => row.Key, StringComparer.Ordinal)],
                    [.. capabilities
                        .DistinctBy(static row => row.Key, StringComparer.Ordinal)
                        .OrderBy(static row => row.Key, StringComparer.Ordinal)])));
    }

    // One key whose group carries more than one distinct mark is a collision; deduplication downstream is then
    // safe because every surviving group agrees, so first-wins never decides anything.
    static Option<ContractFault> Collided<TRow, TMark>(
        Seq<TRow> rows,
        Func<TRow, string> key,
        Func<TRow, TMark> mark) =>
        toSeq(rows.GroupBy(key, StringComparer.Ordinal))
            .Filter(group => group.Select(mark).Distinct().Count() > 1)
            .Map(group => (ContractFault)new ContractFault.ContributionCollision(group.Key))
            .Head;

    // Every mint path lands here, so the dependency proof binds Compose and Merge alike instead of only the
    // path that happened to sort. Proof order is load-bearing: a repeated key makes the key set a lie, an
    // out-of-set edge has no vertex to hang on, and the acyclicity verdict reads a closed graph.
    static Fin<SchemaContract> Project(SchemaContractWire wire) =>
        Proof(wire).Match(Some: Fin.Fail<SchemaContract>, None: () => Mint(wire));

    // The graph proof is reachable to the transport verifier because a foreign bundle carries exactly the same
    // structural claims a locally composed one does; `Mint` is not, because it re-derives canonical bytes and a
    // schema from this exporter and would bind every peer to one type system.
    internal static Option<ContractFault> Proof(SchemaContractWire wire) {
        FrozenSet<string> keys = wire.Artifacts
            .Select(static row => row.Key)
            .ToFrozenSet(StringComparer.Ordinal);
        var graph = new AdjacencyGraph<string, Edge<string>>(allowParallelEdges: false);
        graph.AddVertexRange(keys);
        foreach (ArtifactWire row in wire.Artifacts) {
            foreach (string dependency in row.DependsOn.Where(keys.Contains)) {
                graph.AddEdge(new Edge<string>(dependency, row.Key));
            }
        }

        return toSeq(wire.Artifacts
            .GroupBy(static row => row.Key, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => (ContractFault)new ContractFault.DuplicateArtifact(group.Key))
            .Concat(wire.Artifacts.SelectMany(row => row.DependsOn
                .Where(dependency => !keys.Contains(dependency))
                .Select(dependency => (ContractFault)new ContractFault.MissingDependency(row.Key, dependency))))
            // `IsDirectedAcyclicGraph` answers the verdict as a predicate, so acyclicity costs no throw and no
            // discarded ordering; the witness names the dependency-bearing keys, the only rows a cycle admits.
            .Concat(graph.IsDirectedAcyclicGraph()
                ? Enumerable.Empty<ContractFault>()
                : [new ContractFault.CyclicArtifacts(toSeq(wire.Artifacts
                    .Where(static row => row.DependsOn.Length > 0)
                    .Select(static row => row.Key)))])
            .Concat(wire.Capabilities
                .Where(static row => !FailureRank.TryGet(row.FailureRank, out _)
                    || !RestartClass.TryGet(row.RestartClass, out _))
                .Select(static row => (ContractFault)new ContractFault.InvalidProjection(
                    $"<capability-vocabulary:{row.Key}>"))))
            .Head;
    }

    static Fin<SchemaContract> Mint(SchemaContractWire wire) {
        try {
            byte[] canonical = JsonSerializer.SerializeToUtf8Bytes(wire, BackendJson.Default.SchemaContractWire);
            JsonNode schemaNode = BackendJson.Default.SchemaContractWire.GetJsonSchemaAsNode(
                JsonSchemaExporterOptions.Default);
            byte[] schemaBytes = JsonSerializer.SerializeToUtf8Bytes(schemaNode);
            JsonSchema schema = JsonSchema.Build(JsonSerializer.SerializeToElement(schemaNode));
            JsonElement instance = JsonSerializer.SerializeToElement(wire, BackendJson.Default.SchemaContractWire);
            return schema.Evaluate(instance).IsValid
                ? Fin.Succ(new SchemaContract(
                    wire,
                    canonical,
                    GenerationId.From(ContentHash.Of(canonical)),
                    schemaBytes,
                    schema))
                : Fin.Fail<SchemaContract>(new ContractFault.InvalidProjection("<schema-refused-instance>"));
        } catch (Exception failure) when (
            failure is JsonException
            or InvalidOperationException
            or JsonSchemaException
            or RefResolutionException) {
            return Fin.Fail<SchemaContract>(new ContractFault.InvalidProjection(failure.Message));
        }
    }

    static ArtifactWire Wire(SchemaArtifact row) => new(
        row.Key.ToString(),
        row.Role.Key,
        row.Content.ToString("x32"),
        [.. row.Providers
            .Map(static provider => provider.Key)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)],
        [.. row.DependsOn
            .Map(static dependency => dependency.ToString())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)]);

}

// Pure rename-and-flatten crossing, so the projection is generated: the SmartEnum keys flatten under
// [MapProperty] paths, and the strategy pin keeps every unmapped member a build break. ArtifactWire keeps its
// hand copyist above — provider dedup-and-order is a value transform no mapper expresses.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
internal static partial class CapabilityMap {
    [MapProperty("Rank.Key", nameof(CapabilityWire.FailureRank))]
    [MapProperty("Restart.Key", nameof(CapabilityWire.RestartClass))]
    public static partial CapabilityWire Wire(CapabilityContract row);
}
```

## [04]-[PROJECTION]

- Owner: `ContractProjection.Emit` writes one instance, one derived JSON Schema, and one conformance corpus, carrying them as one `ContractBundle` beside the identity-stamped publication files.
- Law: `BackendObservation.Of` projects a `ProvisionVerdict.Provisioned` onto the observed capability set; the verdict already names the exact `ServerExtension` keys the contract's capability rows carry, so this branch interposes no canonical-to-local adapter table, and artifact evidence arrives from the owners that realized each artifact.
- Law: the emitted set is THIS branch's contribution conforming to the corpus `BACKEND_CONTRACT` schema; a C#-only application deploys it directly, and a polyglot application merges it with peer contributions at the app root by artifact key.
- Law: C#, TypeScript, Python, IaC, and fixtures decode these machine artifacts through local boundary adapters; each peer mints its own contribution rather than reading this one as its source.
- Law: adapters compare expected generation and observed evidence; availability or desired declarations prove nothing.
- Output: fixed file names live inside a generation-qualified bundle; deployment transports bytes without editing them.
- Boundary: providers execute native migrations and provisioning; this owner neither synthesizes DDL nor orchestrates deployment.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContractArtifact {
    public static readonly ContractArtifact Instance = new("contract.json");
    public static readonly ContractArtifact Schema = new("contract.schema.json");
    public static readonly ContractArtifact Conformance = new("contract.conformance.json");
}

public sealed record ContractFile(ContractArtifact Artifact, ReadOnlyMemory<byte> Content, UInt128 Identity);

// `ContractBundle` carries the three published blobs as one transported value, so verification reads what the
// wire delivered and re-derives none of it locally.
public sealed record ContractBundle(
    ReadOnlyMemory<byte> Instance,
    ReadOnlyMemory<byte> Schema,
    ReadOnlyMemory<byte> Conformance);

public sealed record ContractProjection(
    GenerationId Generation,
    ContractBundle Bundle,
    Seq<ContractFile> Files) {
    public static Fin<ContractProjection> Emit(SchemaContract contract) =>
        from corpus in BackendConformance.Emit(contract)
        let bundle = new ContractBundle(
            contract.Canonical,
            contract.JsonSchema,
            JsonSerializer.SerializeToUtf8Bytes(corpus, BackendJson.Default.ConformanceCorpus))
        select new ContractProjection(
            contract.Generation,
            bundle,
            Seq(
                File(ContractArtifact.Instance, bundle.Instance),
                File(ContractArtifact.Schema, bundle.Schema),
                File(ContractArtifact.Conformance, bundle.Conformance)));

    static ContractFile File(ContractArtifact artifact, ReadOnlyMemory<byte> content) =>
        new(artifact, content, ContentHash.Of(content.Span));
}

public sealed record BackendObservation(
    GenerationId Generation,
    FrozenSet<string> HeldCapabilities,
    FrozenSet<string> HeldArtifacts) {
    // `Created` is the cluster's realized extension set read in the one verification batch, and contract
    // capability rows are keyed by that same `ServerExtension.Key` space, so the projection is direct. Artifact
    // evidence enters as a caller argument because only the migration and storage owners that realized an
    // artifact can witness it; a desired declaration is not evidence.
    public static BackendObservation Of(
        GenerationId observed,
        ProvisionVerdict.Provisioned cluster,
        FrozenSet<string> heldArtifacts) =>
        new(observed, cluster.Created, heldArtifacts);
}

[Union]
public abstract partial record BackendVerdict {
    public sealed record Admitted(BackendObservation Observation) : BackendVerdict;
    public sealed record GenerationDrift(GenerationId Expected, GenerationId Observed) : BackendVerdict;
    public sealed record CapabilityGap(Seq<string> Keys) : BackendVerdict;
    public sealed record ArtifactGap(Seq<string> Keys) : BackendVerdict;
}

public static class BackendAdmission {
    public static BackendVerdict Admit(SchemaContract expected, BackendObservation observed) {
        Seq<string> requiredCapabilities = toSeq(expected.Wire.Capabilities
            .Where(static row => row.FailureRank == FailureRank.Required.Key)
            .Select(static row => row.Key)
            .Where(key => !observed.HeldCapabilities.Contains(key)));
        Seq<string> requiredArtifacts = toSeq(expected.Wire.Artifacts
            .Select(static row => row.Key)
            .Where(key => !observed.HeldArtifacts.Contains(key)));

        return observed.Generation != expected.Generation
            ? new BackendVerdict.GenerationDrift(expected.Generation, observed.Generation)
            : !requiredCapabilities.IsEmpty
                ? new BackendVerdict.CapabilityGap(requiredCapabilities)
                : !requiredArtifacts.IsEmpty
                    ? new BackendVerdict.ArtifactGap(requiredArtifacts)
                    : new BackendVerdict.Admitted(observed);
    }
}
```

## [05]-[CONFORMANCE]

- Owner: `BackendConformance.Emit` derives every expected value from one composed contract.
- Law: one `ConformanceCorpus` fixture carries canonical bytes, schema bytes, generation identity, artifact identities, and capability rows.
- Law: `BackendConformance.Verify` reads the transported `ContractBundle`, proves the corpus against the instance and schema bytes the wire carried, then crosses the SAME `ContractComposition.Proof` funnel every local mint crosses and evaluates the decoded instance against the transported validator; a locally re-derived JSON Schema binds every peer to one exporter over one type system, so no peer satisfies it, while a bundle admitted on corpus agreement alone enters the merge with duplicate keys, dangling dependencies, or a cycle nothing rejected.
- Law: key sequences stay in the proof because the corpus is the index a peer trusts WITHOUT decoding the instance — a roster disagreeing with the instance it names is drift even where both files decode clean.
- Law: consumers exercise reordered input and missing required evidence locally; no copied expected digest survives.
- Boundary: conformance proves projection semantics only; provider readiness still requires realized observations.

```csharp signature
public sealed record ConformanceCorpus(
    string Contract,
    string Generation,
    string Canonical,
    string JsonSchema,
    ImmutableArray<string> ArtifactKeys,
    ImmutableArray<string> CapabilityKeys,
    ImmutableArray<string> RequiredCapabilities);

public static class BackendConformance {
    public static Fin<ConformanceCorpus> Emit(SchemaContract contract) =>
        Fin.Succ(new ConformanceCorpus(
            contract.Wire.Contract,
            contract.Generation.Value.ToString("x32"),
            Convert.ToBase64String(contract.Canonical.Span),
            Convert.ToBase64String(contract.JsonSchema.Span),
            [.. contract.Wire.Artifacts.Select(static row => row.Key)],
            [.. contract.Wire.Capabilities.Select(static row => row.Key)],
            [.. contract.Wire.Capabilities
                .Where(static row => row.FailureRank == FailureRank.Required.Key)
                .Select(static row => row.Key)]));

    // Verification reads the TRANSPORTED bundle end to end: the corpus is proved against the instance and schema
    // bytes the wire carried, so a peer whose exporter and type system differ satisfies the same corpus. The
    // returned contract therefore carries transport bytes and a validator parsed from the transported schema,
    // and it feeds `ContractComposition.Merge` where the dependency funnel proves the foreign artifact graph.
    public static Fin<SchemaContract> Verify(ContractBundle bundle) {
        try {
            SchemaContractWire? wire = JsonSerializer.Deserialize(
                bundle.Instance.Span, BackendJson.Default.SchemaContractWire);
            ConformanceCorpus? corpus = JsonSerializer.Deserialize(
                bundle.Conformance.Span, BackendJson.Default.ConformanceCorpus);
            if (wire is null || corpus is null) {
                return Fin.Fail<SchemaContract>(
                    new ContractFault.InvalidProjection("<empty-contract>"));
            }

            UInt128 generation = ContentHash.Of(bundle.Instance.Span);
            bool keysHeld = wire.Artifacts.Select(static row => row.Key)
                    .SequenceEqual(corpus.ArtifactKeys, StringComparer.Ordinal)
                && wire.Capabilities.Select(static row => row.Key)
                    .SequenceEqual(corpus.CapabilityKeys, StringComparer.Ordinal)
                && wire.Capabilities
                    .Where(static row => row.FailureRank == FailureRank.Required.Key)
                    .Select(static row => row.Key)
                    .SequenceEqual(corpus.RequiredCapabilities, StringComparer.Ordinal);
            bool corpusHeld = generation.ToString("x32") == corpus.Generation
                && wire.Contract == corpus.Contract
                && bundle.Instance.Span.SequenceEqual(Convert.FromBase64String(corpus.Canonical))
                && bundle.Schema.Span.SequenceEqual(Convert.FromBase64String(corpus.JsonSchema))
                && keysHeld;
            // Corpus agreement proves the index; the CONTRACT is proved after it. The transported wire crosses
            // the one composition proof — duplicate keys, out-of-set dependencies, cycles, and capability rows
            // outside the closed vocabulary — and the decoded instance is then evaluated against the
            // TRANSPORTED validator, so a foreign exporter satisfies the same law a local mint does while a
            // bundle whose corpus agrees with an unprovable graph stops here instead of entering the merge.
            JsonSchema validator = JsonSchema.FromText(Encoding.UTF8.GetString(bundle.Schema.Span));
            using JsonDocument instance = JsonDocument.Parse(bundle.Instance);
            return !corpusHeld
                ? Fin.Fail<SchemaContract>(new ContractFault.InvalidProjection("<conformance-drift>"))
                : ContractComposition.Proof(wire).Match(
                    Some: Fin.Fail<SchemaContract>,
                    None: () => validator.Evaluate(instance.RootElement).IsValid
                        ? Fin.Succ(new SchemaContract(
                            wire, bundle.Instance, GenerationId.From(generation), bundle.Schema, validator))
                        : Fin.Fail<SchemaContract>(
                            new ContractFault.InvalidProjection("<schema-refused-instance>")));
        } catch (Exception failure) when (
            failure is FormatException or JsonException or NotSupportedException or JsonSchemaException) {
            return Fin.Fail<SchemaContract>(
                new ContractFault.InvalidProjection(failure.Message));
        }
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

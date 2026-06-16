# [SERVICES_PROVISIONING]

One page owns the infrastructure-as-code tier that hosts the node services — the data/compute/observe `ComponentResource` model, the two-mode deploy dispatch (cloud Kubernetes vs self-hosted Docker-compose+Traefik), the entry-thin/impl-dense split, the `StackOutputs` typed cross-stack topology contract, the Doppler secret-at-deploy-boundary, the pluggable state backend, the idempotent bootstrap script, the self-hosted service-equivalence map, and the `ObservabilityStack` collector tier. Deploy-as-code is itself infrastructure, so observability-provisioning and IaC are one concern. The page lives behind the `./provisioning` exports subpath so the `@pulumi/*` deploy-time closure never loads on the durable runtime hot path. It crosses no .NET wire and carries no wire type.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                               |
| :-----: | :----------- | :----------------------------------------------------------------------------------- |
|   [1]   | PROVISIONING | the tier model, two-mode dispatch, secrets, state, bootstrap, outputs, observability |

## [2]-[PROVISIONING]

- Owner: `TierStack`, the data/compute/observe `ComponentResource` owners; `AutomationDriver`, the up/preview/refresh/destroy lifecycle bound as one typed `@effect/cli` verb tree over the Automation API; `SecretResolver`, the Doppler/ESC/config secret resolution at the deploy boundary; `PolicyGuard`, the engine-side `PolicyPack` and program-side guardrails; `StackOutputs`, the typed cross-stack `StackReference`; `StackDrift`, the desired-vs-actual reconciliation fold over the engine event stream; and `ObservabilityStack`, the collector tier provisioned as code. The entry-thin/impl-dense split (`platform.ts` entry + `deploy.ts` impl) is an ALTITUDE boundary, not concept fragmentation.
- Cases: `TierStack` instances are `ComponentResource` classes extending the deployment-core base, instantiating children with `parent: this` and exposing outputs through `registerOutputs` — the data tier (Postgres + Redis + object store: RDS/ElastiCache/S3 in cloud, `postgres:18-alpine` + `redis-alpine` + Garage/MinIO self-hosted), the compute tier (the API workload with autoscaling/probes: K8s Deployment+Service+HPA+Ingress in cloud, Container+Network+Volume+Traefik with ACME self-hosted), and the observe tier (`ObservabilityStack`: Grafana Alloy OTLP collector -> Prometheus remote-write -> Grafana provisioned datasource, K8s DaemonSet in cloud vs container self-hosted); the deploy-mode dispatch is one `DeployMode` `Schema.Literal` row (`cloud`|`self-hosted`) selecting the tier builders through a `Match.exhaustive` table, the two-mode parity the load-bearing architecture concept; `AutomationDriver` drives the four lifecycle verbs from an effect program over the Automation API `LocalWorkspace` and `Stack`, never a bare CLI, binding them as one typed command tree; `SecretResolver` resolves Doppler-injected secrets at deploy time (`doppler run -- pulumi up`) with no secret in code or stack state, the file:// backend using a per-stack passphrase stored in Doppler; the state backend is pluggable and vendor-neutral (file:// for bootstrap, S3/GCS/R2 remote) with cross-stack references via `StackReference`; `StackOutputs` exports the .NET-published server-topology — the Postgres/Timescale DSN, the object-store bucket and endpoint, the Redis URL, the collector OTLP endpoint, and the SPA-host origin — into the consuming tiers as typed reference rows; the idempotent `bootstrap.sh` is a strict-mode Bash entry provisioning the Doppler project + per-stack passphrase + service tokens, running `pulumi login`, and creating/selecting stacks, flock-guarded and retry-with-backoff with `[SKIP]` on already-provisioned resources; `PolicyGuard` is the CrossGuard `PolicyPack` of `ResourceValidationPolicy` rules each carrying a `PolicyLevel` (`advisory`/`mandatory`/`disabled`), `enforce` selecting the active rule set per `DeployMode` so a mandatory guard (no public object-store bucket, encryption-at-rest required, tag-presence) fails the deploy at engine time rather than a runtime audit; `StackDrift` computes the desired-vs-actual reconciliation diff by driving the Automation API `previewRefresh` (a read-only refresh-preview that re-reads provider state without mutating it) with an `onEvent` callback bridged into an Effect via `Effect.async`, folding each `resourcePreEvent.metadata` `StepEventMetadata` `OpType` into a per-URN `DriftResource` `Data.TaggedEnum` (`Added` for an `op:"create"` the program declares but the provider lacks, `Removed` for an `op:"delete"`/`"delete-replaced"` the provider holds but the program no longer declares, `Changed` for an `op:"update"`/`"replace"`/`"create-replacement"` carrying the `detailedDiff` `Record<string, PropertyDiff>` property-path delta, `Unchanged` for an `op:"same"`/`"refresh"`), then `$match`-folding the per-URN `HashMap` into a typed `StackDrift` summary (added/removed/changed resource arrays + a `clean` predicate), so drift is a typed receipt rather than a stdout scrape and a `mandatory` divergence fails CI the same engine-time way a `PolicyGuard` rule does.
- Entry: the lifecycle is driven programmatically from the node platform layer as the driver host; the four lifecycle verbs are one typed command tree the `@effect/cli` binding exposes, dispatched over the Automation API; `platform.ts` is the thin entry (config assembly + deploy orchestration + outputs) and `deploy.ts` owns the tier `ComponentResource` classes, the pure config functions, and the dispatch table — the entry-thin/impl-dense split is an altitude boundary; the `provisioning/` folder takes the `./provisioning` subpath in the implementation-time root/folder `package.json` `exports` (authored at implementation, not now) so the durable runtime never transitively loads `@pulumi/*` on the execution hot path — a subpath export, not a new package, preserving the flat mandate; the `StackOutputs` DSN is the sole seam by which the .NET-provisioned topology enters the consuming tiers, the `persistence.md` `SqlBoundary` consuming the DSN, the `internal-rpc.md` runner stores the DSN, the `ObservabilityStack` the OTLP endpoint, and the `platform` browser SPA the SPA origin.
- Auto: the self-hosted service-equivalence map is the design artifact making two-mode parity tractable — each cloud managed service has a pinned self-hosted container equivalent (RDS->postgres-alpine, ElastiCache->redis-alpine, S3->MinIO/Garage, CloudWatch->Prometheus+Grafana, X-Ray->Grafana Alloy OTLP, Secrets Manager->Doppler), so the `DeployMode` dispatch selects the equivalent row rather than a parallel codebase.
- Packages: `@pulumi/pulumi` for the deployment core and Automation API, `@pulumi/awsx` and `@pulumi/aws` for the cloud and image-build/registry executors, `@pulumi/kubernetes` for the k8s native/Helm/kustomize provisioning, `@pulumi/docker` for the self-hosted container executor, `@pulumi/command` and `@pulumi/random` for the command and random helpers, `@pulumi/esc-sdk` and `@dopplerhq/node-sdk` for the secret-resolution SDK, `@pulumi/policy` for the CrossGuard policy SDK, `@effect/cli` for the typed-command binding, `@effect/opentelemetry` and `@opentelemetry/sdk-trace-node` for the node collector, and `@effect/platform-node` for the driver host.
- Growth: a new tier resource lands as one `ComponentResource` child; a new deploy mode lands as one `DeployMode` literal and one dispatch arm; a new provider lands as one provider row; a new lifecycle verb lands as one command on the existing tree; a new topology output lands as one `StackOutputs` reference row; a new self-hosted equivalent lands as one row on the equivalence map; a new policy lands as one `PolicyGuard` guardrail row; a new drift classification lands as one `DriftResource` `Data.TaggedEnum` variant breaking the `$match` fold at compile time, never a parallel diff surface.
- Boundary: this domain crosses no .NET wire and carries no wire type; `StackOutputs` reads infrastructure topology values a deployment publishes, structurally distinct from the eleven runtime wire contracts and crossing none of them; the `@pulumi/*` set is reachable only behind the `./provisioning` subpath and never on the durable runtime hot path; `StackDrift` is no exception — the `EngineEvent`/`StepEventMetadata`/`PropertyDiff` types it folds are `@pulumi/pulumi/automation` types consumed inside the subpath closure, the `StackDrift` summary it returns is a domain receipt carrying only primitive URN/type/property-path strings, so no `@pulumi/*` type escapes onto the durable hot path; the secrets are Doppler-injected at deploy time and never in code or stack state; the deploy-mode dispatch is one table and never a parallel cloud/self-hosted codebase; this is a node-only deploy-time surface, never browser-reachable.

```ts contract
const DeployMode = Schema.Literal("cloud", "self-hosted");
type DeployMode = Schema.Schema.Type<typeof DeployMode>;

type LifecycleVerb = "up" | "preview" | "refresh" | "destroy";

interface TierStack {
  readonly data: (mode: DeployMode) => pulumi.ComponentResource;
  readonly compute: (mode: DeployMode) => pulumi.ComponentResource;
  readonly observe: (mode: DeployMode) => pulumi.ComponentResource;
}

const buildTier = (tier: TierStack, mode: DeployMode): ReadonlyArray<pulumi.ComponentResource> =>
  Match.value(mode).pipe(
    Match.when("cloud", () => [tier.data("cloud"), tier.compute("cloud"), tier.observe("cloud")]),
    Match.when("self-hosted", () => [tier.data("self-hosted"), tier.compute("self-hosted"), tier.observe("self-hosted")]),
    Match.exhaustive,
  );

interface AutomationDriver {
  readonly workspace: Effect.Effect<LocalWorkspace, never, Scope.Scope>;
  readonly stack: (stackName: string) => Effect.Effect<Stack, never, Scope.Scope>;
  readonly run: (verb: LifecycleVerb, stackName: string) => Effect.Effect<OutputMap, AutomationFault>;
  readonly command: Command.Command<"deploy", never, AutomationFault, { readonly verb: LifecycleVerb; readonly stack: string }>;
}

interface StackOutputs {
  readonly reference: StackReference;
  readonly postgresDsn: Effect.Effect<Redacted.Redacted, ConfigError.ConfigError>;
  readonly timescaleDsn: Effect.Effect<Redacted.Redacted, ConfigError.ConfigError>;
  readonly objectStore: Effect.Effect<{ readonly bucket: string; readonly endpoint: string }, ConfigError.ConfigError>;
  readonly redisUrl: Effect.Effect<Redacted.Redacted, ConfigError.ConfigError>;
  readonly collectorOtlp: Effect.Effect<string, ConfigError.ConfigError>;
  readonly spaOrigin: Effect.Effect<string, ConfigError.ConfigError>;
}

interface SecretResolver {
  readonly config: Config.Config<DeploymentConfig>;
  readonly secret: (key: string) => Effect.Effect<Redacted.Redacted, ConfigError.ConfigError>;
  readonly provider: Layer.Layer<never>;
}

type PolicyLevel = "advisory" | "mandatory" | "disabled";

interface PolicyGuard {
  readonly pack: policy.PolicyPack;
  readonly resourceRule: (name: string, level: PolicyLevel, validate: (args: policy.ResourceValidationArgs) => void) => policy.ResourceValidationPolicy;
  readonly enforce: (mode: DeployMode) => ReadonlyArray<policy.ResourceValidationPolicy>;
}
```

```ts drift
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { EngineEvent, OpType, PreviewResult, PropertyDiff, StepEventMetadata, Stack } from "@pulumi/pulumi/automation";
import type { AutomationDriver } from "./contract.js";
import { DiffKind } from "@pulumi/pulumi/automation";
import { Command, Options } from "@effect/cli";
import { Array as Arr, Data, Effect, HashMap, Match, Option, pipe, Schema } from "effect";

// --- [TYPES] ---------------------------------------------------------------------------
// One closed family for the per-resource verdict; the engine OpType axis re-closes into the
// domain on fold. Added/Removed/Changed/Unchanged are the four drift verdicts; `$match` is the
// exhaustive fold and a fifth verdict is a compile-time break across every fold site.
type DriftResource = Data.TaggedEnum<{
  readonly Added: { readonly urn: string; readonly type: string };
  readonly Removed: { readonly urn: string; readonly type: string };
  readonly Changed: { readonly urn: string; readonly type: string; readonly diff: ReadonlyArray<PropertyChange> };
  readonly Unchanged: { readonly urn: string; readonly type: string };
}>;

// The detailedDiff Record<string, PropertyDiff> re-closes into a flat property-path row carrying
// only primitives so no @pulumi/* type escapes the subpath onto the durable hot path.
type PropertyChange = {
  readonly path: string;
  readonly kind: DiffKind;
  readonly inputDiff: boolean;
};

const DriftResource = Data.taggedEnum<DriftResource>();

// --- [MODELS] --------------------------------------------------------------------------
// The drift summary IS the receipt: three resource arrays plus a clean predicate, decoded from
// primitives only. One Model.Class, projections derive — no parallel added/removed/changed schema.
class StackDriftSummary extends Schema.Class<StackDriftSummary>("StackDriftSummary")({
  stack: Schema.String,
  added: Schema.Array(Schema.Struct({ urn: Schema.String, type: Schema.String })),
  removed: Schema.Array(Schema.Struct({ urn: Schema.String, type: Schema.String })),
  changed: Schema.Array(
    Schema.Struct({
      urn: Schema.String,
      type: Schema.String,
      diff: Schema.Array(
        Schema.Struct({
          path: Schema.String,
          kind: Schema.Enums(DiffKind),
          inputDiff: Schema.Boolean,
        }),
      ),
    }),
  ),
}) {
  get clean(): boolean {
    return this.added.length === 0 && this.removed.length === 0 && this.changed.length === 0;
  }
}

// --- [ERRORS] --------------------------------------------------------------------------
// One boundary fault for the deploy-time refresh-preview; the engine-stage cause is the payload,
// not a parallel error rail. A mandatory divergence is signalled by the dedicated `divergent` stage
// so the @effect/cli verb fails CI engine-time the way a PolicyGuard rule does.
class DriftFault extends Schema.TaggedError<DriftFault>()("DriftFault", {
  stack: Schema.String,
  stage: Schema.Literal("refresh", "decode", "divergent"),
  detail: Schema.String,
}) {}

// --- [SERVICES] ------------------------------------------------------------------------
// StackDrift is a ROW on the provisioning surface, not a parallel owner: it reuses the
// AutomationDriver-resolved Stack and returns the typed summary receipt. `gate` is the mandatory
// arm — it re-runs the diff and fails `divergent` when the summary is not clean.
interface StackDrift {
  readonly diff: (stack: Stack, stackName: string) => Effect.Effect<StackDriftSummary, DriftFault>;
  readonly gate: (stack: Stack, stackName: string) => Effect.Effect<StackDriftSummary, DriftFault>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
// classifyStep: the engine OpType axis -> the domain DriftResource family. `previewRefresh` emits
// at most one resourcePreEvent per URN; `op:"same"`/`"refresh"` is no drift, the mutating ops are the
// three drift verdicts. Match.exhaustive guards the full 15-member OpType union at compile time.
const classifyStep = (m: StepEventMetadata): DriftResource =>
  Match.value(m.op).pipe(
    Match.when("create", () => DriftResource.Added({ urn: m.urn, type: m.type })),
    Match.when("import", () => DriftResource.Added({ urn: m.urn, type: m.type })),
    Match.whenOr("delete", "delete-replaced", "remove-pending-replace", "discard", "discard-replaced", () =>
      DriftResource.Removed({ urn: m.urn, type: m.type }),
    ),
    Match.whenOr("update", "replace", "create-replacement", "import-replacement", "read-replacement", () =>
      DriftResource.Changed({ urn: m.urn, type: m.type, diff: detailedDiff(m.detailedDiff) }),
    ),
    Match.whenOr("same", "refresh", "read", () => DriftResource.Unchanged({ urn: m.urn, type: m.type })),
    Match.exhaustive,
  );

// detailedDiff: Record<string, PropertyDiff> -> flat PropertyChange rows; the optional record folds to
// an empty array (a `replace` with no detailedDiff is still a Changed verdict carrying zero paths).
const detailedDiff = (record: Record<string, PropertyDiff> | undefined): ReadonlyArray<PropertyChange> =>
  pipe(
    Option.fromNullable(record),
    Option.map((r) =>
      pipe(
        Object.entries(r),
        Arr.map(([path, d]) => ({ path, kind: d.diffKind, inputDiff: d.inputDiff }) satisfies PropertyChange),
      ),
    ),
    Option.getOrElse(() => Arr.empty<PropertyChange>()),
  );

// foldVerdicts: the per-URN HashMap accumulator -> the StackDriftSummary via one `$match` fold over
// the closed family. `Unchanged` contributes to no bucket; the three drift buckets project to the
// three Model arrays. HashMap.values is the immutable accumulator drained once, no .set mutation.
const foldVerdicts = (stack: string, verdicts: HashMap.HashMap<string, DriftResource>): StackDriftSummary =>
  pipe(
    HashMap.values(verdicts),
    Arr.fromIterable,
    Arr.reduce(
      { added: Arr.empty<{ urn: string; type: string }>(), removed: Arr.empty<{ urn: string; type: string }>(), changed: Arr.empty<{ urn: string; type: string; diff: ReadonlyArray<PropertyChange> }>() },
      (acc, v) =>
        DriftResource.$match(v, {
          Added: ({ urn, type }) => ({ ...acc, added: Arr.append(acc.added, { urn, type }) }),
          Removed: ({ urn, type }) => ({ ...acc, removed: Arr.append(acc.removed, { urn, type }) }),
          Changed: ({ urn, type, diff }) => ({ ...acc, changed: Arr.append(acc.changed, { urn, type, diff }) }),
          Unchanged: () => acc,
        }),
    ),
    (buckets) => new StackDriftSummary({ stack, ...buckets }),
  );

// mutatedCount: the authoritative drift cardinality from PreviewResult.changeSummary (the engine's own
// per-OpType OpMap), summing every mutating OpType and excluding the three no-drift ops. The Match over
// the closed OpType union decides drift membership; `same`/`refresh`/`read` contribute zero.
const NO_DRIFT_OPS: ReadonlyArray<OpType> = ["same", "refresh", "read"];
const mutatedCount = (summary: PreviewResult["changeSummary"]): number =>
  pipe(
    Object.entries(summary) as ReadonlyArray<readonly [OpType, number]>,
    Arr.reduce(0, (total, [op, count]) => (Arr.contains(NO_DRIFT_OPS, op) ? total : total + count)),
  );

// computeStackDrift: the transcription-complete drift-diff body. `previewRefresh` re-reads provider
// state read-only; the structured `onEvent` callback is bridged into an Effect via `Effect.async`, the
// per-URN verdict accumulated into a HashMap closed over the callback (boundary-local mutable handle,
// drained into the immutable summary on resolve). The last verdict per URN wins (refresh emits one).
// `PreviewResult.changeSummary` is captured alongside so the folded buckets reconcile against the
// engine's own count, making the receipt self-validating and catching any onEvent/promise race.
const computeStackDrift = (stack: Stack, stackName: string): Effect.Effect<StackDriftSummary, DriftFault> =>
  Effect.async<readonly [HashMap.HashMap<string, DriftResource>, PreviewResult], DriftFault>((resume) => {
    let verdicts = HashMap.empty<string, DriftResource>();
    const onEvent = (event: EngineEvent): void => {
      const meta = Option.fromNullable(event.resourcePreEvent?.metadata);
      Option.match(meta, {
        onNone: () => undefined,
        onSome: (m) => {
          verdicts = HashMap.set(verdicts, m.urn, classifyStep(m));
        },
      });
    };
    stack.previewRefresh({ onEvent }).then(
      (result: PreviewResult) => resume(Effect.succeed([verdicts, result] as const)),
      (cause: unknown) =>
        resume(Effect.fail(new DriftFault({ stack: stackName, stage: "refresh", detail: String(cause) }))),
    );
  }).pipe(
    Effect.map(([verdicts, result]) => [foldVerdicts(stackName, verdicts), result] as const),
    Effect.filterOrFail(
      ([summary, result]) =>
        summary.added.length + summary.removed.length + summary.changed.length === mutatedCount(result.changeSummary),
      ([summary, result]) =>
        new DriftFault({
          stack: stackName,
          stage: "decode",
          detail: `folded ${String(summary.added.length + summary.removed.length + summary.changed.length)} != changeSummary ${String(mutatedCount(result.changeSummary))}`,
        }),
    ),
    Effect.map(([summary]) => summary),
  );

// driftGate: the mandatory arm — a clean summary passes through, a divergent summary fails `divergent`
// so the @effect/cli `drift` verb exits non-zero in CI exactly as a mandatory PolicyGuard rule does.
const driftGate = (stack: Stack, stackName: string): Effect.Effect<StackDriftSummary, DriftFault> =>
  computeStackDrift(stack, stackName).pipe(
    Effect.filterOrFail(
      (summary) => summary.clean,
      (summary) =>
        new DriftFault({
          stack: stackName,
          stage: "divergent",
          detail: `+${summary.added.length} -${summary.removed.length} ~${summary.changed.length}`,
        }),
    ),
  );

// --- [COMPOSITION] ---------------------------------------------------------------------
const stackDrift: StackDrift = { diff: computeStackDrift, gate: driftGate };

// AutomationDriver grows one `drift` verb on the SAME command tree (one command on the existing tree,
// not a parallel CLI): it resolves the Stack via `driver.stack`, runs `gate`, and the DriftFault is the
// verb's typed failure channel so a divergent stack exits non-zero engine-time.
const driftVerb = (driver: AutomationDriver): Command.Command<"drift", never, DriftFault, { readonly stack: string }> =>
  Command.make("drift", { stack: Options.text("stack") }, ({ stack }) =>
    driver.stack(stack).pipe(
      Effect.flatMap((s) => stackDrift.gate(s, stack)),
      Effect.scoped,
      Effect.asVoid,
    ),
  );
```

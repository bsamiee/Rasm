# [SERVICES_STACK_CONTRACT]

The infrastructure-as-code tier that hosts the node services — the data/compute/observe `ComponentResource` model, the two-mode deploy dispatch (cloud Kubernetes vs self-hosted Docker-compose+Traefik), the `StackOutputs` typed cross-stack topology contract, the Doppler/ESC secret-at-deploy-boundary, the `PolicyGuard` CrossGuard pack, the idempotent bootstrap, the self-hosted service-equivalence map, and the `ObservabilityStack` collector tier. Deploy-as-code is itself infrastructure, so observability-provisioning and IaC are one concern. The tier lives behind the `./provisioning` exports subpath so the `@pulumi/*` deploy-time closure never loads on the durable runtime hot path. It crosses no .NET wire and carries no wire type.

## [01]-[INDEX]

- [01]-[PROVISIONING]: owns the data/compute/observe tier model, the cloud/self-hosted two-mode dispatch, secrets, state, bootstrap, `StackOutputs`, `PolicyGuard`, and the `ObservabilityStack`.

## [02]-[PROVISIONING]

- Owner: `TierStack`, the data/compute/observe `ComponentResource` owners; `AutomationDriver`, the up/preview/refresh/destroy lifecycle bound as one typed `@effect/cli` verb tree over the Automation API; `SecretResolver`, the Doppler/ESC/config secret resolution at the deploy boundary; `PolicyGuard`, the engine-side `PolicyPack` and program-side guardrails; `StackOutputs`, the typed cross-stack `StackReference`; and `ObservabilityStack`, the collector tier provisioned as code. The entry-thin/impl-dense split (entry config-assembly + impl tier classes) is an ALTITUDE boundary, not concept fragmentation.
- Cases: `TierStack` instances are `ComponentResource` classes extending the deployment-core base, instantiating children with `parent: this` and exposing outputs through `registerOutputs` — the data tier (Postgres + Redis + object store: RDS/ElastiCache/S3 in cloud, `postgres:18-alpine` + `redis-alpine` + Garage/MinIO self-hosted), the compute tier (the API workload with autoscaling/probes: K8s Deployment+Service+HPA+Ingress in cloud, Container+Network+Volume+Traefik with ACME self-hosted), and the observe tier (`ObservabilityStack`: Grafana Alloy OTLP collector -> Prometheus remote-write -> Grafana provisioned datasource, K8s DaemonSet in cloud vs container self-hosted); the deploy-mode dispatch is one `DeployMode` `Schema.Literal` row (`cloud`|`self-hosted`) selecting the tier builders through a `Match.exhaustive` table, the two-mode parity the load-bearing architecture concept; `AutomationDriver` drives the four lifecycle verbs from an effect program over the Automation API `LocalWorkspace` and `Stack`, never a bare CLI, binding them as one typed command tree; `SecretResolver` resolves Doppler-injected secrets at deploy time (`doppler run -- pulumi up`) with no secret in code or stack state, the file:// backend using a per-stack passphrase stored in Doppler; the state backend is pluggable and vendor-neutral (file:// for bootstrap, S3/GCS/R2 remote) with cross-stack references via `StackReference`; `StackOutputs` exports this tier's OWN provisioned server-topology — the Postgres/Timescale DSN, the object-store bucket and endpoint, the Redis URL, the collector OTLP endpoint, and the SPA-host origin — into the consuming tiers as typed reference rows, infrastructure values this IaC tier itself stands up, never a .NET wire contract decoded from a peer; the idempotent `bootstrap.sh` is a strict-mode Bash entry provisioning the Doppler project + per-stack passphrase + service tokens, running `pulumi login`, and creating/selecting stacks, flock-guarded and retry-with-backoff with `[SKIP]` on already-provisioned resources; `PolicyGuard` is the CrossGuard `PolicyPack` of `ResourceValidationPolicy` rules each carrying a `PolicyLevel` (`advisory`/`mandatory`/`disabled`), `enforce` selecting the active rule set per `DeployMode` so a mandatory guard (no public object-store bucket, encryption-at-rest required, tag-presence) fails the deploy at engine time rather than a runtime audit.
- Entry: the lifecycle is driven programmatically from the node platform layer as the driver host; the four lifecycle verbs are one typed command tree the `@effect/cli` binding exposes, dispatched over the Automation API; the thin entry owns config assembly + deploy orchestration + outputs and the dense impl owns the tier `ComponentResource` classes, the pure config functions, and the dispatch table — the entry-thin/impl-dense split is an altitude boundary; the `provisioning/` folder takes the `./provisioning` subpath in the package's `exports` map declared in the one workspace manifest (at implementation) so the durable runtime never transitively loads `@pulumi/*` on the execution hot path — a subpath entry on the existing package, not a new package, preserving the flat mandate; the `StackOutputs` DSN is the sole seam by which this tier's OWN provisioned topology enters the consuming tiers, the `persistence/store#STORE_BOUNDARY` `SqlBoundary` consuming the DSN, the `execution/backplane#RUNNER_AND_SCHEDULING` runner stores the DSN, the `ObservabilityStack` the OTLP endpoint, and the `platform` browser SPA the SPA origin.
- Auto: the self-hosted service-equivalence map is the design artifact making two-mode parity tractable — each cloud managed service has a pinned self-hosted container equivalent (RDS->postgres-alpine, ElastiCache->redis-alpine, S3->MinIO/Garage, CloudWatch->Prometheus+Grafana, X-Ray->Grafana Alloy OTLP, Secrets Manager->Doppler), so the `DeployMode` dispatch selects the equivalent row rather than a parallel codebase.
- Packages: `@pulumi/pulumi` for the deployment core and Automation API, `@pulumi/awsx` and `@pulumi/aws` for the cloud and image-build/registry executors, `@pulumi/kubernetes` for the k8s native/Helm/kustomize provisioning, `@pulumi/docker` for the self-hosted container executor, `@pulumi/command` and `@pulumi/random` for the command and random helpers, `@pulumi/esc-sdk` and `@dopplerhq/node-sdk` for the secret-resolution SDK, `@pulumi/policy` for the CrossGuard policy SDK, `@effect/cli` for the typed-command binding, `@effect/opentelemetry` and `@opentelemetry/sdk-trace-node` and `@opentelemetry/sdk-metrics` for the node collector, and `@effect/platform-node` for the driver host.
- Growth: a new tier resource lands as one `ComponentResource` child; a new deploy mode lands as one `DeployMode` literal and one dispatch arm; a new provider lands as one provider row; a new lifecycle verb lands as one command on the existing tree; a new topology output lands as one `StackOutputs` reference row; a new self-hosted equivalent lands as one row on the equivalence map; a new policy lands as one `PolicyGuard` guardrail row. The continuous drift fold and its scheduled sweep land at `drift#PROVISIONING`.
- Boundary: this domain crosses no .NET wire and carries no wire type; `StackOutputs` reads infrastructure topology values this tier's own deployment publishes, structurally distinct from the runtime wire contracts and crossing none of them; the `@pulumi/*` set is reachable only behind the `./provisioning` subpath and never on the durable runtime hot path; the secrets are Doppler-injected at deploy time and never in code or stack state; the deploy-mode dispatch is one table and never a parallel cloud/self-hosted codebase; this is a node-only deploy-time surface, never browser-reachable.

```ts contract
const DeployMode = Schema.Literal("cloud", "self-hosted");
type DeployMode = Schema.Schema.Type<typeof DeployMode>;

type LifecycleVerb = "up" | "preview" | "refresh" | "destroy";

const TIER_KINDS = ["data", "compute", "observe"] as const;
type TierKind = (typeof TIER_KINDS)[number];

interface TierStack {
  readonly tier: (kind: TierKind, mode: DeployMode) => pulumi.ComponentResource;
}

const buildTier = (stack: TierStack, mode: DeployMode): ReadonlyArray<pulumi.ComponentResource> =>
  Arr.map(TIER_KINDS, (kind) => stack.tier(kind, mode));

interface AutomationDriver {
  readonly workspace: Effect.Effect<LocalWorkspace, never, Scope.Scope>;
  readonly stack: (stackName: string) => Effect.Effect<Stack, never, Scope.Scope>;
  readonly run: (verb: LifecycleVerb, stackName: string) => Effect.Effect<OutputMap, AutomationFault>;
  readonly command: Command.Command<"deploy", never, AutomationFault, { readonly verb: LifecycleVerb; readonly stack: string }>;
}

interface StackOutputShape {
  readonly postgresDsn: Redacted.Redacted;
  readonly timescaleDsn: Redacted.Redacted;
  readonly objectStore: { readonly bucket: string; readonly endpoint: string };
  readonly redisUrl: Redacted.Redacted;
  readonly collectorOtlp: string;
  readonly spaOrigin: string;
}
type StackOutputKey = keyof StackOutputShape;

interface StackOutputs {
  readonly reference: StackReference;
  readonly output: <K extends StackOutputKey>(key: K) => Effect.Effect<StackOutputShape[K], ConfigError.ConfigError>;
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

interface ObservabilityStack {
  readonly tier: (mode: DeployMode) => pulumi.ComponentResource;
  readonly collectorOtlp: Effect.Effect<string, ConfigError.ConfigError>;
  readonly emit: <A>(receipt: A) => Effect.Effect<void>;
}
```

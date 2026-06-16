# [SERVICES_PROVISIONING]

One page owns the infrastructure-as-code tier that hosts the node services — the data/compute/observe `ComponentResource` model, the two-mode deploy dispatch (cloud Kubernetes vs self-hosted Docker-compose+Traefik), the entry-thin/impl-dense split, the `StackOutputs` typed cross-stack topology contract, the Doppler secret-at-deploy-boundary, the pluggable state backend, the idempotent bootstrap script, the self-hosted service-equivalence map, and the `ObservabilityStack` collector tier. Deploy-as-code is itself infrastructure, so observability-provisioning and IaC are one concern. The page lives behind the `./provisioning` exports subpath so the `@pulumi/*` deploy-time closure never loads on the durable runtime hot path. It crosses no .NET wire and carries no wire type.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                               |
| :-----: | :----------- | :----------------------------------------------------------------------------------- |
|   [1]   | PROVISIONING | the tier model, two-mode dispatch, secrets, state, bootstrap, outputs, observability |

## [2]-[PROVISIONING]

- Owner: `TierStack`, the data/compute/observe `ComponentResource` owners; `AutomationDriver`, the up/preview/refresh/destroy lifecycle bound as one typed `@effect/cli` verb tree over the Automation API; `SecretResolver`, the Doppler/ESC/config secret resolution at the deploy boundary; `PolicyGuard`, the engine-side `PolicyPack` and program-side guardrails; `StackOutputs`, the typed cross-stack `StackReference`; and `ObservabilityStack`, the collector tier provisioned as code. The entry-thin/impl-dense split (`platform.ts` entry + `deploy.ts` impl) is an ALTITUDE boundary, not concept fragmentation.
- Cases: `TierStack` instances are `ComponentResource` classes extending the deployment-core base, instantiating children with `parent: this` and exposing outputs through `registerOutputs` — the data tier (Postgres + Redis + object store: RDS/ElastiCache/S3 in cloud, `postgres:18-alpine` + `redis-alpine` + Garage/MinIO self-hosted), the compute tier (the API workload with autoscaling/probes: K8s Deployment+Service+HPA+Ingress in cloud, Container+Network+Volume+Traefik with ACME self-hosted), and the observe tier (`ObservabilityStack`: Grafana Alloy OTLP collector -> Prometheus remote-write -> Grafana provisioned datasource, K8s DaemonSet in cloud vs container self-hosted); the deploy-mode dispatch is one `DeployMode` `Schema.Literal` row (`cloud`|`self-hosted`) selecting the tier builders through a `Match.exhaustive` table, the two-mode parity the load-bearing architecture concept; `AutomationDriver` drives the four lifecycle verbs from an effect program over the Automation API `LocalWorkspace` and `Stack`, never a bare CLI, binding them as one typed command tree; `SecretResolver` resolves Doppler-injected secrets at deploy time (`doppler run -- pulumi up`) with no secret in code or stack state, the file:// backend using a per-stack passphrase stored in Doppler; the state backend is pluggable and vendor-neutral (file:// for bootstrap, S3/GCS/R2 remote) with cross-stack references via `StackReference`; `StackOutputs` exports the .NET-published server-topology — the Postgres/Timescale DSN, the object-store bucket and endpoint, the Redis URL, the collector OTLP endpoint, and the SPA-host origin — into the consuming tiers as typed reference rows; the idempotent `bootstrap.sh` is a strict-mode Bash entry provisioning the Doppler project + per-stack passphrase + service tokens, running `pulumi login`, and creating/selecting stacks, flock-guarded and retry-with-backoff with `[SKIP]` on already-provisioned resources; `PolicyGuard` is the CrossGuard `PolicyPack` of `ResourceValidationPolicy` rules each carrying a `PolicyLevel` (`advisory`/`mandatory`/`disabled`), `enforce` selecting the active rule set per `DeployMode` so a mandatory guard (no public object-store bucket, encryption-at-rest required, tag-presence) fails the deploy at engine time rather than a runtime audit.
- Entry: the lifecycle is driven programmatically from the node platform layer as the driver host; the four lifecycle verbs are one typed command tree the `@effect/cli` binding exposes, dispatched over the Automation API; `platform.ts` is the thin entry (config assembly + deploy orchestration + outputs) and `deploy.ts` owns the tier `ComponentResource` classes, the pure config functions, and the dispatch table — the entry-thin/impl-dense split is an altitude boundary; the `provisioning/` folder takes the `./provisioning` `package.json` exports subpath so the durable runtime never transitively loads `@pulumi/*` on the execution hot path — a subpath export, not a new package, preserving the flat mandate; the `StackOutputs` DSN is the sole seam by which the .NET-provisioned topology enters the consuming tiers, the `persistence.md` `SqlBoundary` consuming the DSN, the `internal-rpc.md` runner stores the DSN, the `ObservabilityStack` the OTLP endpoint, and the `platform` browser SPA the SPA origin.
- Auto: the self-hosted service-equivalence map is the design artifact making two-mode parity tractable — each cloud managed service has a pinned self-hosted container equivalent (RDS->postgres-alpine, ElastiCache->redis-alpine, S3->MinIO/Garage, CloudWatch->Prometheus+Grafana, X-Ray->Grafana Alloy OTLP, Secrets Manager->Doppler), so the `DeployMode` dispatch selects the equivalent row rather than a parallel codebase.
- Packages: `@pulumi/pulumi` for the deployment core and Automation API, `@pulumi/awsx` and `@pulumi/aws` for the cloud and image-build/registry executors, `@pulumi/kubernetes` for the k8s native/Helm/kustomize provisioning, `@pulumi/docker` for the self-hosted container executor, `@pulumi/command` and `@pulumi/random` for the command and random helpers, `@pulumi/esc-sdk` and `@dopplerhq/node-sdk` for the secret-resolution SDK, `@pulumi/policy` for the CrossGuard policy SDK, `@effect/cli` for the typed-command binding, `@effect/opentelemetry` and `@opentelemetry/sdk-trace-node` for the node collector, and `@effect/platform-node` for the driver host.
- Growth: a new tier resource lands as one `ComponentResource` child; a new deploy mode lands as one `DeployMode` literal and one dispatch arm; a new provider lands as one provider row; a new lifecycle verb lands as one command on the existing tree; a new topology output lands as one `StackOutputs` reference row; a new self-hosted equivalent lands as one row on the equivalence map; a new policy lands as one `PolicyGuard` guardrail row.
- Boundary: this domain crosses no .NET wire and carries no wire type; `StackOutputs` reads infrastructure topology values a deployment publishes, structurally distinct from the eleven runtime wire contracts and crossing none of them; the `@pulumi/*` set is reachable only behind the `./provisioning` subpath and never on the durable runtime hot path; the secrets are Doppler-injected at deploy time and never in code or stack state; the deploy-mode dispatch is one table and never a parallel cloud/self-hosted codebase; this is a node-only deploy-time surface, never browser-reachable.

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

# [IAC_SECRET]

The material owner of the deploy plane — Doppler provisioning and TLS issuance on one page because both mint secrets other rows only reference. `Secrets` provisions the Doppler hierarchy (`Project → Environment → BranchConfig → Secret`), lands every generated credential in it under one char-class policy whose `keepers` map carries the spec `epoch`, scopes one read-access `ServiceToken` for runtime injection, serves every sibling provider's credential field through one parameterized fan-in read, and mirrors outward through one `integration` + `secretssync` pair per destination — external stores are mirrors, never sources. `Certs` is the certificate pipeline: one profile value drives `PrivateKey → CertRequest → {self-signed CA | CA-signed leaf}`, `allowedUses` is a bounded vocabulary, rotation is window-driven (`earlyRenewalHours` moves the window, `readyForRenewal` is the boolean the drift fold watches), and a CA key that must outlive the graph lands in Doppler as a `{ value }` entry — generated material has exactly one canonical store. A value leaves this page two ways only: an in-graph single-key `Output<string>` bound to a provider credential `Input` (state-encrypted, never process env), or the `ServiceToken.key` as the `DOPPLER_TOKEN` env fact the workload assembly injects. The module is `iac/src/operate/secret.ts`; a new credential is one entries row, a new consumer is one fan-in key, a new destination is one mirror pair row, a tenant's secret access is one `access` row, an mTLS leaf is one more issuance call, and a browser-trusted hostname outside a cluster is one `trusted` call on the ACME lane.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                            | [PUBLIC]  |
| :-----: | :---------------- | :------------------------------------------------------------------ | :-------- |
|  [01]   | `STORE_HIERARCHY` | the parent-chained hierarchy, generated entries, the token         | `Secrets` |
|  [02]   | `FAN_IN_AND_OUT`  | the single-key provider fan-in, the mirror fan-out, the webhook    | `Secrets` |
|  [03]   | `CERT_CHAIN`      | the usage vocabulary, the profile, the CA root, leaf issuance      | `Certs`   |

## [2]-[STORE_HIERARCHY]

[STORE_HIERARCHY]:
- Owner: `Secrets`, one tier chaining `Project` (named by the spec's `doppler.project`), `Environment` (slugged by the spec's `doppler.config`), `BranchConfig`, then one `Secret` per entries row — every resource `parent`-chained through `child()` so the tier owns the whole store.
- Law: entries are a discriminated record — a row is `{ generate: seed }` (a partial policy the owner completes through `_Policy.make`, minting a `RandomPassword` with `keepers: { epoch }` and storing `pulumi.secret(password.result)`) or `{ value }` (an upstream secret-tracked `Output` — a `Certs` CA key, a bootstrap artifact) — discriminated by field presence through `Predicate.hasProperty`, so sensitive-versus-supplied is one dispatch, never two entry surfaces, and a caller states only its policy deltas.
- Law: the char-class policy is one Schema shape — `length`, class toggles, per-class minimums, `overrideSpecial` — decoded once and shared by every generated row; `RandomString` never mints a credential, and the `bcryptHash` projection serves a consumer that stores a digest instead of the value.
- Law: the token is the only egress fact — `ServiceToken` with `access: "read"` scoped to the one config; `read/write` tokens exist only for provisioners and never leave the graph; the `key` output is sensitive by construction and crosses solely into the workload env assembly.
- Law: a missing read key aborts cleanly — `read(key)` resolves the whole-config map once and a key the config does not carry throws `pulumi.RunError` inside the apply, failing the run with the key named, never forging an empty string.
- Law: `store(key, value)` is the late-landing write — a value minted AFTER the tier constructs (a `Certs` CA key, a Grafana automation token, an ACME-issued edge pair) lands as one more `Secret` row under the same tier through the same parent chain, so construction-time `entries` and graph-late material share one canonical store and no second write surface exists.
- Entry: `new Secrets("secrets", { spec, entries }, opts)` inside every provider arm; `secrets.read("DB_PASSWORD")` at any credential `Input`; `secrets.store("MESH_CA_KEY", ca.key.privateKeyPem)` for graph-late material; `secrets.token` into `Workload.token`.
- Law: access is the `_ACCESS` handler record — `machine` mints the durable `ServiceAccount`/`ServiceAccountToken` identity (the workplace-RBAC upgrade over the config-scoped token), `group` binds a workplace group onto the project at a role with optional environment scoping, `member` binds a service account the same way; tenant secret isolation is rows of this record against the one store, never a second store per tenant.
- Growth: a new credential is one entries row (`digest: true` stores the `bcryptHash` projection for a consumer that never needs the value); a new policy axis is one `_Policy` field with its default; a new access posture is one `_ACCESS` row.
- Boundary: runtime consumption (`doppler run`, the security plane's leased-secret read path) is the workload assembly's process boundary; generated-material laws (`keepers`, encodings) are the entropy provider's contract; the bootstrap `DOPPLER_TOKEN` for the provider plugin itself is deploy-host env under `doppler run`.
- Packages: `@pulumiverse/doppler` (`Project`, `Environment`, `BranchConfig`, `Secret`, `ServiceToken`, `Webhook`, `getSecretsOutput`, the `integration`/`secretssync` namespaces); `@pulumi/random` (`RandomPassword`); `@pulumi/pulumi` (`Output`, `secret`, `RunError`); `effect` (`Schema`, `Predicate`, `Record`, `Array`, `Option`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as pulumi from "@pulumi/pulumi"
import * as random from "@pulumi/random"
import * as doppler from "@pulumiverse/doppler"
import * as integration from "@pulumiverse/doppler/integration"
import * as projectmember from "@pulumiverse/doppler/projectmember"
import * as secretssync from "@pulumiverse/doppler/secretssync"
import { Array, Option, Predicate, Record, Schema } from "effect"
import { Tier, type StackSpec } from "../program/spec.ts"

const _Policy = Schema.Struct({
  length: Schema.optionalWith(Schema.Int.pipe(Schema.between(16, 128)), { default: () => 32 }),
  special: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  minSpecial: Schema.optionalWith(Schema.Int.pipe(Schema.between(0, 8)), { default: () => 1 }),
  minNumeric: Schema.optionalWith(Schema.Int.pipe(Schema.between(0, 8)), { default: () => 1 }),
  minUpper: Schema.optionalWith(Schema.Int.pipe(Schema.between(0, 8)), { default: () => 1 }),
  overrideSpecial: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
})

declare namespace Secrets {
  type Policy = typeof _Policy.Type
  type Entry = { readonly generate: Partial<Policy>; readonly digest?: boolean } | { readonly value: pulumi.Input<string> }
  type Args = { readonly spec: StackSpec; readonly entries: Record.ReadonlyRecord<string, Entry> }
}

const _required = (map: Record<string, string>, key: string): string =>
  map[key] ?? (() => { throw new pulumi.RunError(`<missing-secret:${key}>`) })()

const _minted = (key: string, policy: Secrets.Policy, epoch: string, child: pulumi.CustomResourceOptions): random.RandomPassword =>
  new random.RandomPassword(key, {
    length: policy.length,
    special: policy.special,
    minSpecial: policy.minSpecial,
    minNumeric: policy.minNumeric,
    minUpper: policy.minUpper,
    ...Option.match(policy.overrideSpecial, {
      onNone: () => ({}),
      onSome: (overrideSpecial) => ({ overrideSpecial }),
    }),
    keepers: { epoch },
  }, child)

class Secrets extends Tier {
  static readonly mirrored = <K extends Secrets.Mirror["target"]>(
    owner: Secrets,
    name: string,
    row: Extract<Secrets.Mirror, { readonly target: K }>,
    child: pulumi.CustomResourceOptions,
  ): pulumi.CustomResource => _MIRRORS[row.target](owner, name, row, child)
  static readonly webhook = (
    owner: Secrets,
    name: string,
    args: { readonly url: pulumi.Input<string>; readonly secret?: pulumi.Input<string>; readonly payload?: pulumi.Input<string> },
    child: pulumi.CustomResourceOptions,
  ): doppler.Webhook =>
    new doppler.Webhook(name, {
      project: owner.project.name,
      url: args.url,
      enabledConfigs: [owner.config.name],
      ...(args.secret !== undefined && { secret: args.secret }),
      ...(args.payload !== undefined && { payload: args.payload }),
    }, child)
  static readonly access = <K extends Secrets.Access["kind"]>(
    owner: Secrets,
    name: string,
    row: Extract<Secrets.Access, { readonly kind: K }>,
    child: pulumi.CustomResourceOptions,
  ): pulumi.CustomResource => _ACCESS[row.kind](owner, name, row, child)
  readonly project: doppler.Project
  readonly config: doppler.BranchConfig
  readonly token: pulumi.Output<string>
  constructor(name: string, args: Secrets.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Secrets", name, opts)
    this.project = new doppler.Project(name, { name: args.spec.doppler.project }, this.child())
    const environment = new doppler.Environment(name, {
      name: args.spec.doppler.config,
      slug: args.spec.doppler.config,
      project: this.project.name,
    }, this.child())
    this.config = new doppler.BranchConfig(name, {
      project: this.project.name,
      environment: environment.slug,
    }, this.child())
    Array.map(Record.toEntries(args.entries), ([key, entry]) =>
      new doppler.Secret(key, {
        project: this.project.name,
        config: this.config.name,
        name: key,
        value: Predicate.hasProperty(entry, "generate")
          ? ((minted) => pulumi.secret(entry.digest === true ? minted.bcryptHash : minted.result))(
              _minted(key, _Policy.make(entry.generate), args.spec.epoch, this.child()))
          : entry.value,
      }, this.child()))
    this.token = new doppler.ServiceToken(name, {
      project: this.project.name,
      config: this.config.name,
      name: `${name}-runtime`,
      access: "read",
    }, this.child()).key
    this.seal({ project: this.project.name, config: this.config.name, token: this.token })
  }
  read(key: string): pulumi.Output<string> {
    return doppler.getSecretsOutput({ project: this.project.name, config: this.config.name }, { parent: this })
      .apply((result) => _required(result.map, key))
  }
  store(key: string, value: pulumi.Input<string>): doppler.Secret {
    return new doppler.Secret(key, {
      project: this.project.name,
      config: this.config.name,
      name: key,
      value,
    }, this.child())
  }
}
```

## [3]-[FAN_IN_AND_OUT]

[FAN_IN_AND_OUT]:
- Law: one read serves every provider — `read(key)` is the single-key pluck over `getSecretsOutput(...).map`, and the consumer roster is data: `postgresql.Provider.password` ← `DB_ADMIN_PASSWORD`, `postgresql.Role.password` ← `DB_PASSWORD`, `cloudflare.Provider.apiToken` ← `CLOUDFLARE_API_TOKEN`, `gcp.Provider.credentials` ← `GCP_CREDENTIALS`, grafana auth ← `GRAFANA_PASSWORD`, registry auth ← `REGISTRY_PASSWORD` — a new consuming provider is a key row on the config the store already owns, never a new read path.
- Law: the in-graph value stays in the graph — the plucked `Output<string>` binds a credential `Input`, is state-encrypted, and never touches process env; the env mode exists only for the runtime processes the workload assembly wraps.
- Law: mirroring is one pair per destination — `Secrets.mirrored` is the generic indexed call over the `_MIRRORS` handler record keyed by the target discriminant, each row's payload correlated through `Extract`: a destination whose credential link this plane owns constructs `integration.<Target>` then `secretssync.<Target>` referencing its id; a GitHub destination's integration is the pre-existing GitHub-App install, so its row carries the integration slug and constructs the sync row alone; targets are rows, the mirror is always FROM the canonical config outward, and `mirrored` rides the owner as a static so the tier grows no per-target methods.
- Law: rotation observes itself — `Secrets.webhook(owner, name, { url, secret, payload }, child)` is one `doppler.Webhook` row riding the owner, delivering signed secret-change events to the endpoint the composing arm names (`secret` binds a Doppler-generated HMAC entry the receiver verifies, `payload` shapes the delivery), so an epoch bump or an out-of-band edit surfaces as evidence the drift sweep correlates beside the `operate/cloud.md` drift-filter webhook — one evidence-delivery law with two sources; delivery is a signal, never a second read path.
- Growth: a new mirror destination is one `_MIRRORS` row plus its `Mirror` case — the shipped target set (secrets manager, parameter store, Actions, Terraform Cloud workspaces, CircleCI contexts, Fly.io apps) is realized whole, so growth is genuinely a new provider ship, not a backlog.
- Boundary: what a mirrored store's consumers do with the copy is theirs; the canonical write path never routes through a mirror.

```typescript
declare namespace Secrets {
  type Mirror =
    | { readonly target: "awsSecretsManager"; readonly assumeRoleArn: pulumi.Input<string>; readonly region: pulumi.Input<string>; readonly path: pulumi.Input<string> }
    | { readonly target: "awsParameterStore"; readonly assumeRoleArn: pulumi.Input<string>; readonly region: pulumi.Input<string>; readonly path: pulumi.Input<string> }
    | { readonly target: "githubActions"; readonly integration: pulumi.Input<string>; readonly repoName: pulumi.Input<string> }
    | { readonly target: "terraformCloud"; readonly apiKey: pulumi.Input<string>; readonly workspaceId: pulumi.Input<string> }
    | { readonly target: "circleci"; readonly apiToken: pulumi.Input<string>; readonly organizationSlug: pulumi.Input<string>; readonly resourceId: pulumi.Input<string>; readonly resourceType: "project" | "context" }
    | { readonly target: "flyio"; readonly apiKey: pulumi.Input<string>; readonly appId: pulumi.Input<string> }
  type Access =
    | { readonly kind: "machine"; readonly role?: pulumi.Input<string> }
    | { readonly kind: "group"; readonly slug: pulumi.Input<string>; readonly role: pulumi.Input<string>; readonly environments?: ReadonlyArray<string> }
    | { readonly kind: "member"; readonly account: pulumi.Input<string>; readonly role: pulumi.Input<string>; readonly environments?: ReadonlyArray<string> }
}

const _MIRRORS: {
  readonly [K in Secrets.Mirror["target"]]: (
    owner: Secrets,
    name: string,
    row: Extract<Secrets.Mirror, { readonly target: K }>,
    child: pulumi.CustomResourceOptions,
  ) => pulumi.CustomResource
} = {
  awsSecretsManager: (owner, name, row, child) =>
    new secretssync.AwsSecretsManager(name, {
      project: owner.project.name,
      config: owner.config.name,
      integration: new integration.AwsSecretsManager(name, { name, assumeRoleArn: row.assumeRoleArn }, child).id,
      region: row.region,
      path: row.path,
    }, child),
  awsParameterStore: (owner, name, row, child) =>
    new secretssync.AwsParameterStore(name, {
      project: owner.project.name,
      config: owner.config.name,
      integration: new integration.AwsParameterStore(name, { name, assumeRoleArn: row.assumeRoleArn }, child).id,
      region: row.region,
      path: row.path,
      secureString: true,
    }, child),
  githubActions: (owner, name, row, child) =>
    new secretssync.GithubActions(name, {
      project: owner.project.name,
      config: owner.config.name,
      integration: row.integration,
      syncTarget: "repo",
      repoName: row.repoName,
    }, child),
  terraformCloud: (owner, name, row, child) =>
    new secretssync.TerraformCloud(name, {
      project: owner.project.name,
      config: owner.config.name,
      integration: new integration.TerraformCloud(name, { name, apiKey: row.apiKey }, child).id,
      syncTarget: "workspace",
      workspaceId: row.workspaceId,
      variableSyncType: "env",
      nameTransform: "none",
    }, child),
  circleci: (owner, name, row, child) =>
    new secretssync.Circleci(name, {
      project: owner.project.name,
      config: owner.config.name,
      integration: new integration.Circleci(name, { name, apiToken: row.apiToken }, child).id,
      organizationSlug: row.organizationSlug,
      resourceId: row.resourceId,
      resourceType: row.resourceType,
    }, child),
  flyio: (owner, name, row, child) =>
    new secretssync.Flyio(name, {
      project: owner.project.name,
      config: owner.config.name,
      integration: new integration.Flyio(name, { name, apiKey: row.apiKey }, child).id,
      appId: row.appId,
      restartMachines: true,
    }, child),
}

const _ACCESS: {
  readonly [K in Secrets.Access["kind"]]: (
    owner: Secrets,
    name: string,
    row: Extract<Secrets.Access, { readonly kind: K }>,
    child: pulumi.CustomResourceOptions,
  ) => pulumi.CustomResource
} = {
  machine: (_owner, name, row, child) =>
    new doppler.ServiceAccountToken(name, {
      name,
      serviceAccountSlug: new doppler.ServiceAccount(name, {
        name,
        ...(row.role !== undefined && { workplaceRole: row.role }),
      }, child).slug,
    }, child),
  group: (owner, name, row, child) =>
    new projectmember.Group(name, {
      project: owner.project.name,
      groupSlug: row.slug,
      role: row.role,
      ...(row.environments !== undefined && { environments: [...row.environments] }),
    }, child),
  member: (owner, name, row, child) =>
    new projectmember.ServiceAccount(name, {
      project: owner.project.name,
      serviceAccountSlug: row.account,
      role: row.role,
      ...(row.environments !== undefined && { environments: [...row.environments] }),
    }, child),
}
```

## [4]-[CERT_CHAIN]

[CERT_CHAIN]:
- Owner: `Certs` — the issuance pipeline as three members over one profile value: `_Profile` (algorithm, curve, validity and renewal windows, the `allowedUses` subset) decodes once per caller; `Certs.root(name, profile?)` mints the CA — one `SelfSignedCert` with `isCaCertificate: true` over its own `PrivateKey`, doubled validity, `cert_signing + crl_signing` uses; `Certs.issue(name, { ca, hostname, profile? })` runs `PrivateKey → CertRequest → LocallySignedCert` and returns the `{ key, cert, renewal }` triple the traffic sink consumes.
- Law: `allowedUses` is a closed vocabulary — the `_uses` tuple spreads into `Schema.Literal`, a profile names a subset, and a free-string usage cannot enter the chain; server default is `server_auth + digital_signature + key_encipherment`, an mTLS client leaf is one `issue` call with the `client_auth` subset against the same CA.
- Law: key material never rides plain — `privateKeyPem` outputs are state-encrypted by the provider and cross this page only into the TLS secret's `stringData` or a `Secrets` `{ value }` entry when a consumer outside the graph needs the CA; a `publicKeyPem`/fingerprint projection is free.
- Law: rotation is window-driven — `validityPeriodHours` and `earlyRenewalHours` come from the profile, `readyForRenewal` rides the triple as `renewal` so the drift fold surfaces an approaching reissue as an `update` step on the cert resource; deleting a cert to rotate it is the named defect.
- Law: the trusted lane is ACME over the same chain — `Certs.register` binds one account per directory (the account key minted by the `tls` owner, adopted through `accountKeyPem`), and `Certs.trusted` runs the CSR posture (`PrivateKey → CertRequest → acme.Certificate`) so the private key never leaves the entropy owner: DNS-01 through `dnsChallenges` rows whose credential config binds fan-in reads, `minDaysRemaining` derived from the same profile renewal window, `useRenewalInfo` adopting the ARI window, `revokeCertificateOnDestroy` so a torn-down stack leaves no live cert; this is the browser-trusted lane for the docker arm's edge and bare metal — self-signed serves the mesh, cert-manager serves in-cluster, and duplicating either here is the lane split-brain.
- Law: CA semantics are explicit — the root stamps `setSubjectKeyId`/`setAuthorityKeyId` and `maxPathLength: 0`, so the chain verifies by key identifier and the mesh root can sign leaves only, never intermediates.
- Law: ECDSA is the floor — the profile defaults `ECDSA`/`P256`; RSA is admitted only where a consumer mandates it and then at 2048 bits minimum through the same profile field; a foreign endpoint's chain pins through `Certs.pin` (`getCertificateOutput` with `verifyChain`) when an arm must trust material it does not mint.
- Entry: `Certs.root` once per arm; `Certs.issue` per mesh hostname; `Certs.register` once per directory then `Certs.trusted` per public hostname; all take explicit `opts` so a tier parents them where they compose.
- Growth: a second CA (a mesh-internal root) is a second `root` value on the same pipeline; a new usage subset is profile data; an EAB-demanding CA is one `externalAccountBinding` row on `register`.
- Boundary: the TLS secret sink and ingress reference are `kube/traffic.md`'s; cloud-managed certs are the prepared columns' cert cells; the ACME directory URL is `acme.Provider.serverUrl` data — staging versus production is provider construction, never a resource fork.
- Packages: `@pulumi/tls` (`PrivateKey`, `CertRequest`, `SelfSignedCert`, `LocallySignedCert`, `getCertificateOutput`); `@pulumiverse/acme` (`Registration`, `Certificate`); `effect` (`Schema`).

```typescript
import * as acme from "@pulumiverse/acme"
import * as tls from "@pulumi/tls"

const _uses = [
  "server_auth", "client_auth", "digital_signature", "key_encipherment",
  "key_agreement", "cert_signing", "crl_signing", "code_signing",
] as const

const _Profile = Schema.Struct({
  algorithm: Schema.optionalWith(Schema.Literal("RSA", "ECDSA", "ED25519"), { default: () => "ECDSA" as const }),
  curve: Schema.optionalWith(Schema.Literal("P224", "P256", "P384", "P521"), { default: () => "P256" as const }),
  validityHours: Schema.optionalWith(Schema.Int.pipe(Schema.between(24, 87600)), { default: () => 8760 }),
  renewBeforeHours: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 8760)), { default: () => 720 }),
  uses: Schema.optionalWith(
    Schema.Array(Schema.Literal(..._uses)),
    { default: () => ["server_auth", "digital_signature", "key_encipherment"] as const },
  ),
})

declare namespace Certs {
  type Use = (typeof _uses)[number]
  type Profile = typeof _Profile.Type
  type Root = { readonly key: tls.PrivateKey; readonly cert: tls.SelfSignedCert }
  type Issued = {
    readonly key: pulumi.Output<string>
    readonly cert: pulumi.Output<string>
    readonly renewal: pulumi.Output<boolean>
  }
  type Trusted = {
    readonly key: pulumi.Output<string>
    readonly cert: pulumi.Output<string>
    readonly chain: pulumi.Output<string>
  }
}

const Certs = {
  profile: (seed?: Partial<Certs.Profile>): Certs.Profile => _Profile.make(seed ?? {}),
  root: (name: string, profile: Certs.Profile = _Profile.make({}), opts?: pulumi.CustomResourceOptions): Certs.Root => {
    const key = new tls.PrivateKey(`${name}-key`, { algorithm: profile.algorithm, ecdsaCurve: profile.curve }, opts)
    const cert = new tls.SelfSignedCert(name, {
      privateKeyPem: key.privateKeyPem,
      isCaCertificate: true,
      allowedUses: ["cert_signing", "crl_signing"],
      validityPeriodHours: profile.validityHours * 2,
      subject: { commonName: name },
      setSubjectKeyId: true,
      setAuthorityKeyId: true,
      maxPathLength: 0,
    }, opts)
    return { key, cert }
  },
  issue: (
    name: string,
    args: { readonly ca: Certs.Root; readonly hostname: string; readonly profile?: Certs.Profile },
    opts?: pulumi.CustomResourceOptions,
  ): Certs.Issued => {
    const profile = args.profile ?? _Profile.make({})
    const key = new tls.PrivateKey(`${name}-key`, { algorithm: profile.algorithm, ecdsaCurve: profile.curve }, opts)
    const request = new tls.CertRequest(`${name}-csr`, {
      privateKeyPem: key.privateKeyPem,
      subject: { commonName: args.hostname },
      dnsNames: [args.hostname],
    }, opts)
    const cert = new tls.LocallySignedCert(name, {
      certRequestPem: request.certRequestPem,
      caPrivateKeyPem: args.ca.key.privateKeyPem,
      caCertPem: args.ca.cert.certPem,
      allowedUses: [...profile.uses],
      validityPeriodHours: profile.validityHours,
      earlyRenewalHours: profile.renewBeforeHours,
    }, opts)
    return { key: key.privateKeyPem, cert: cert.certPem, renewal: cert.readyForRenewal }
  },
  register: (
    name: string,
    args: { readonly email: pulumi.Input<string> },
    opts?: pulumi.CustomResourceOptions,
  ): acme.Registration =>
    new acme.Registration(name, {
      emailAddress: args.email,
      accountKeyPem: new tls.PrivateKey(`${name}-account`, { algorithm: "ECDSA", ecdsaCurve: "P256" }, opts).privateKeyPem,
    }, opts),
  trusted: (
    name: string,
    args: {
      readonly registration: acme.Registration
      readonly hostname: string
      readonly challenge: { readonly provider: string; readonly config: Record<string, pulumi.Input<string>> }
      readonly profile?: Certs.Profile
    },
    opts?: pulumi.CustomResourceOptions,
  ): Certs.Trusted => {
    const profile = args.profile ?? _Profile.make({})
    const key = new tls.PrivateKey(`${name}-key`, { algorithm: profile.algorithm, ecdsaCurve: profile.curve }, opts)
    const request = new tls.CertRequest(`${name}-csr`, {
      privateKeyPem: key.privateKeyPem,
      subject: { commonName: args.hostname },
      dnsNames: [args.hostname],
    }, opts)
    const cert = new acme.Certificate(name, {
      accountKeyPem: args.registration.accountKeyPem,
      certificateRequestPem: request.certRequestPem,
      dnsChallenges: [{ provider: args.challenge.provider, config: args.challenge.config }],
      minDaysRemaining: Math.ceil(profile.renewBeforeHours / 24),
      useRenewalInfo: true,
      revokeCertificateOnDestroy: true,
    }, opts)
    return { key: key.privateKeyPem, cert: cert.certificatePem, chain: cert.issuerPem }
  },
  pin: (url: string, opts?: pulumi.InvokeOutputOptions): pulumi.Output<tls.GetCertificateResult> =>
    tls.getCertificateOutput({ url, verifyChain: true }, opts),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Certs, Secrets }
```

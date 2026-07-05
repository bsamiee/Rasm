# [IAC_SECRET]

The material owner of the deploy plane — Doppler provisioning and TLS issuance on one page because both mint secrets other rows only reference. `Secrets` provisions the Doppler hierarchy (`Project → Environment → BranchConfig → Secret`), lands every generated credential in it under one char-class policy whose `keepers` map carries the spec `epoch`, scopes one read-access `ServiceToken` for runtime injection, serves every sibling provider's credential field through one parameterized fan-in read, and mirrors outward through one `integration` + `secretssync` pair per destination — external stores are mirrors, never sources. `Certs` is the certificate pipeline: one profile value drives `PrivateKey → CertRequest → {self-signed CA | CA-signed leaf}`, `allowedUses` is a bounded vocabulary, rotation is window-driven (`earlyRenewalHours` moves the window, `readyForRenewal` is the boolean the drift fold watches), and a CA key that must outlive the graph lands in Doppler as a `{ value }` entry — generated material has exactly one canonical store. A value leaves this page two ways only: an in-graph single-key `Output<string>` bound to a provider credential `Input` (state-encrypted, never process env), or the `ServiceToken.key` as the `DOPPLER_TOKEN` env fact the workload assembly injects. The module is `iac/src/operate/secret.ts`; a new credential is one entries row, a new consumer is one fan-in key, a new destination is one mirror pair row, an mTLS leaf is one more issuance call.

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
- Entry: `new Secrets("secrets", { spec, entries }, opts)` inside every provider arm; `secrets.read("DB_PASSWORD")` at any credential `Input`; `secrets.token` into `Workload.token`.
- Growth: a new credential is one entries row; a new policy axis is one `_Policy` field with its default; a durable machine identity (`ServiceAccount`/`ServiceAccountToken`) is the identity upgrade over the config-scoped token when an estate earns workplace RBAC.
- Boundary: runtime consumption (`doppler run`, the security plane's leased-secret read path) is the workload assembly's process boundary; generated-material laws (`keepers`, encodings) are the entropy provider's contract; the bootstrap `DOPPLER_TOKEN` for the provider plugin itself is deploy-host env under `doppler run`.
- Packages: `@pulumiverse/doppler` (`Project`, `Environment`, `BranchConfig`, `Secret`, `ServiceToken`, `Webhook`, `getSecretsOutput`, the `integration`/`secretssync` namespaces); `@pulumi/random` (`RandomPassword`); `@pulumi/pulumi` (`Output`, `secret`, `RunError`); `effect` (`Schema`, `Predicate`, `Record`, `Array`, `Option`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as pulumi from "@pulumi/pulumi"
import * as random from "@pulumi/random"
import * as doppler from "@pulumiverse/doppler"
import * as integration from "@pulumiverse/doppler/integration"
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
  type Entry = { readonly generate: Partial<Policy> } | { readonly value: pulumi.Input<string> }
  type Args = { readonly spec: StackSpec; readonly entries: Record.ReadonlyRecord<string, Entry> }
}

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
  static readonly mirrored = (owner: Secrets, name: string, row: Secrets.Mirror, child: pulumi.CustomResourceOptions): pulumi.CustomResource =>
    row.target === "awsSecretsManager"
      ? new secretssync.AwsSecretsManager(name, {
          project: owner.project.name,
          config: owner.config.name,
          integration: new integration.AwsSecretsManager(name, { name, assumeRoleArn: row.assumeRoleArn }, child).id,
          region: row.region,
          path: row.path,
        }, child)
      : new secretssync.GithubActions(name, {
          project: owner.project.name,
          config: owner.config.name,
          integration: row.integration,
          syncTarget: "repo",
          repoName: row.repoName,
        }, child)
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
          ? pulumi.secret(_minted(key, _Policy.make(entry.generate), args.spec.epoch, this.child()).result)
          : entry.value,
      }, this.child()))
    this.token = new doppler.ServiceToken(name, {
      project: this.project.name,
      config: this.config.name,
      name: `${name}-runtime`,
      access: "read",
    }, this.child()).key
    this.seal({ project: this.project.name, config: this.config.name })
  }
  read(key: string): pulumi.Output<string> {
    return doppler.getSecretsOutput({ project: this.project.name, config: this.config.name }, { parent: this })
      .apply((result) => result.map[key] ?? (() => { throw new pulumi.RunError(`<missing-secret:${key}>`) })())
  }
}
```

## [3]-[FAN_IN_AND_OUT]

[FAN_IN_AND_OUT]:
- Law: one read serves every provider — `read(key)` is the single-key pluck over `getSecretsOutput(...).map`, and the consumer roster is data: `postgresql.Provider.password` ← `DB_ADMIN_PASSWORD`, `postgresql.Role.password` ← `DB_PASSWORD`, `cloudflare.Provider.apiToken` ← `CLOUDFLARE_API_TOKEN`, `gcp.Provider.credentials` ← `GCP_CREDENTIALS`, grafana auth ← `GRAFANA_PASSWORD`, registry auth ← `REGISTRY_PASSWORD` — a new consuming provider is a key row on the config the store already owns, never a new read path.
- Law: the in-graph value stays in the graph — the plucked `Output<string>` binds a credential `Input`, is state-encrypted, and never touches process env; the env mode exists only for the runtime processes the workload assembly wraps.
- Law: mirroring is one pair per destination — `Secrets.mirrored` dispatches on the target key: a destination whose credential link this plane owns constructs `integration.<Target>` then `secretssync.<Target>` referencing its id; a GitHub destination's integration is the pre-existing GitHub-App install, so its row carries the integration slug and constructs the sync row alone; targets are rows, the mirror is always FROM the canonical config outward, and `mirrored` rides the owner as a static so the tier grows no per-target methods.
- Law: rotation observes itself — `_webhook(owner, name, url, child)` is one `doppler.Webhook` row delivering secret-change events to the endpoint the composing arm names, so an epoch bump or an out-of-band edit surfaces as evidence the drift sweep correlates; delivery is a signal, never a second read path.
- Growth: a new mirror destination is one target row in the `mirrored` dispatch; the remaining shipped sync targets (parameter store, workspace variables, CI contexts, app platforms) land the same way when a consumer exists.
- Boundary: what a mirrored store's consumers do with the copy is theirs; the canonical write path never routes through a mirror.

```typescript
declare namespace Secrets {
  type Mirror =
    | { readonly target: "awsSecretsManager"; readonly assumeRoleArn: pulumi.Input<string>; readonly region: pulumi.Input<string>; readonly path: pulumi.Input<string> }
    | { readonly target: "githubActions"; readonly integration: pulumi.Input<string>; readonly repoName: pulumi.Input<string> }
}

const _webhook = (owner: Secrets, name: string, url: pulumi.Input<string>, child: pulumi.CustomResourceOptions): doppler.Webhook =>
  new doppler.Webhook(name, {
    project: owner.project.name,
    url,
    enabledConfigs: [owner.config.name],
  }, child)
```

## [4]-[CERT_CHAIN]

[CERT_CHAIN]:
- Owner: `Certs` — the issuance pipeline as three members over one profile value: `_Profile` (algorithm, curve, validity and renewal windows, the `allowedUses` subset) decodes once per caller; `Certs.root(name, profile?)` mints the CA — one `SelfSignedCert` with `isCaCertificate: true` over its own `PrivateKey`, doubled validity, `cert_signing + crl_signing` uses; `Certs.issue(name, { ca, hostname, profile? })` runs `PrivateKey → CertRequest → LocallySignedCert` and returns the `{ key, cert, renewal }` triple the traffic sink consumes.
- Law: `allowedUses` is a closed vocabulary — the `_uses` tuple spreads into `Schema.Literal`, a profile names a subset, and a free-string usage cannot enter the chain; server default is `server_auth + digital_signature + key_encipherment`, an mTLS client leaf is one `issue` call with the `client_auth` subset against the same CA.
- Law: key material never rides plain — `privateKeyPem` outputs are state-encrypted by the provider and cross this page only into the TLS secret's `stringData` or a `Secrets` `{ value }` entry when a consumer outside the graph needs the CA; a `publicKeyPem`/fingerprint projection is free.
- Law: rotation is window-driven — `validityPeriodHours` and `earlyRenewalHours` come from the profile, `readyForRenewal` rides the triple as `renewal` so the drift fold surfaces an approaching reissue as an `update` step on the cert resource; deleting a cert to rotate it is the named defect.
- Law: ECDSA is the floor — the profile defaults `ECDSA`/`P256`; RSA is admitted only where a consumer mandates it and then at 2048 bits minimum through the same profile field; a foreign endpoint's chain pins through `getCertificateOutput` when an arm must trust material it does not mint.
- Entry: `Certs.root` once per arm; `Certs.issue` per hostname; both take explicit `opts` so a tier parents them where they compose.
- Growth: a second CA (a mesh-internal root) is a second `root` value on the same pipeline; a new usage subset is profile data.
- Boundary: the TLS secret sink and ingress reference are `kube/traffic.md`'s; cloud-managed certs are the prepared columns' cert cells.
- Packages: `@pulumi/tls` (`PrivateKey`, `CertRequest`, `SelfSignedCert`, `LocallySignedCert`, `getCertificateOutput`); `effect` (`Schema`).

```typescript
import * as tls from "@pulumi/tls"

const _uses = [
  "server_auth", "client_auth", "digital_signature", "key_encipherment",
  "key_agreement", "cert_signing", "crl_signing", "code_signing",
] as const

const _Profile = Schema.Struct({
  algorithm: Schema.optionalWith(Schema.Literal("RSA", "ECDSA", "ED25519"), { default: () => "ECDSA" as const }),
  curve: Schema.optionalWith(Schema.NonEmptyString, { default: () => "P256" }),
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
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Certs, Secrets }
```

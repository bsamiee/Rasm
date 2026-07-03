# [IAC_TRAFFIC]

The cert/dns/ingress rows of the `selfhosted-k8s` arm: one `Traffic` tier issues the certificate chain from a decoded profile, sinks it into the `kubernetes.io/tls` secret, fronts the workload service with a typed `networking/v1.Ingress`, and exposes the hostname by the profile's exposure row — a proxied `cloudflare.DnsRecord` at the metal address (`direct`) or a Zero-Trust tunnel when the cluster has no public address (`tunnel`). Issuance is one pipeline, never two recipes: a tier-local CA root signs every leaf through `CertRequest → LocallySignedCert`, `allowedUses` is a bounded `Schema.Literal` vocabulary, and rotation is declarative — `earlyRenewalHours` moves the window, `readyForRenewal` is the boolean the `policy/drift` fold watches, and reissue happens by the provider's own renewal semantics, never by deletion. The module is `iac/src/kube/traffic.ts`; a new exposure mode is one dispatch row, a new key-usage is one vocabulary member, an mTLS mesh leaf is one more chain call against the same CA.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                              | [PUBLIC]  |
| :-----: | :------------- | :----------------------------------------------------------------- | :-------- |
|  [01]   | `CERT_CHAIN`   | the usage vocabulary, the cert profile, the CA root, leaf issuance  | `Traffic` |
|  [02]   | `INGRESS_EDGE` | the TLS secret sink, the typed ingress, the exposure dispatch       | `Traffic` |

## [2]-[CERT_CHAIN]

[CERT_CHAIN]:
- Owner: the interior chain pipeline — `_Profile` (algorithm, curve, validity and renewal windows, the `allowedUses` subset) decodes once per tier; the CA root is one `SelfSignedCert` with `isCaCertificate: true` over its own `PrivateKey`; `_issued` runs `PrivateKey → CertRequest → LocallySignedCert` for every leaf, returning the `{ key, cert, renewal }` triple the sinks consume.
- Law: `allowedUses` is a closed vocabulary — the `_uses` tuple spreads into `Schema.Literal`, a profile names a subset, and a free-string usage cannot enter the chain; server default is `server_auth + digital_signature + key_encipherment`, the CA carries `cert_signing + crl_signing`.
- Law: key material never rides plain — `privateKeyPem` outputs are state-encrypted by the provider and cross this tier only into the TLS secret's `stringData` or a `Secrets` entry (`{ value }` row) when a consumer outside the cluster needs the CA; a `publicKeyPem`/fingerprint projection is free.
- Law: rotation is window-driven — `validityPeriodHours` and `earlyRenewalHours` come from the profile, `readyForRenewal` rides the tier as `renewal` so the drift fold surfaces an approaching reissue as an `update` step on the cert resource; deleting a cert to rotate it is the named defect.
- Law: ECDSA is the floor — the profile defaults `ECDSA`/`P256`; RSA is admitted only where a consumer mandates it and then at 2048 bits minimum through the same profile field.
- Growth: an mTLS client leaf is one `_issued` call with the `client_auth` subset; a second CA (a mesh-internal root) is a second chain value on the same pipeline.
- Boundary: external chain reads (`getCertificateOutput`) belong to the arm that pins a foreign endpoint; cloud-managed certs are the prepared columns' cert cells.
- Packages: `@pulumi/tls` (`PrivateKey`, `CertRequest`, `SelfSignedCert`, `LocallySignedCert`); `effect` (`Schema`); `../program/spec.ts` (`StackSpec`); `../stack/component.ts` (`Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as cloudflare from "@pulumi/cloudflare"
import * as pulumi from "@pulumi/pulumi"
import * as tls from "@pulumi/tls"
import { Option, Schema } from "effect"
import type { StackSpec } from "../program/spec.ts"
import { Tier } from "../stack/component.ts"

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

declare namespace Traffic {
  type Use = (typeof _uses)[number]
  type Profile = typeof _Profile.Type
  type Issued = {
    readonly key: pulumi.Output<string>
    readonly cert: pulumi.Output<string>
    readonly renewal: pulumi.Output<boolean>
  }
  type Exposure =
    | { readonly mode: "direct"; readonly address: string }
    | { readonly mode: "tunnel" }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly service: pulumi.Input<string>
    readonly port: number
    readonly profile?: Profile
    readonly apiToken: pulumi.Input<string>
  }
}

const _ca = (name: string, profile: Traffic.Profile, child: pulumi.CustomResourceOptions): { readonly key: tls.PrivateKey; readonly cert: tls.SelfSignedCert } => {
  const key = new tls.PrivateKey(`${name}-ca-key`, { algorithm: profile.algorithm, ecdsaCurve: profile.curve }, child)
  const cert = new tls.SelfSignedCert(`${name}-ca`, {
    privateKeyPem: key.privateKeyPem,
    isCaCertificate: true,
    allowedUses: ["cert_signing", "crl_signing"],
    validityPeriodHours: profile.validityHours * 2,
    subject: { commonName: `${name}-ca` },
  }, child)
  return { key, cert }
}

const _issued = (
  name: string,
  profile: Traffic.Profile,
  ca: ReturnType<typeof _ca>,
  hostname: string,
  child: pulumi.CustomResourceOptions,
): Traffic.Issued => {
  const key = new tls.PrivateKey(`${name}-key`, { algorithm: profile.algorithm, ecdsaCurve: profile.curve }, child)
  const request = new tls.CertRequest(`${name}-csr`, {
    privateKeyPem: key.privateKeyPem,
    subject: { commonName: hostname },
    dnsNames: [hostname],
  }, child)
  const cert = new tls.LocallySignedCert(`${name}-cert`, {
    certRequestPem: request.certRequestPem,
    caPrivateKeyPem: ca.key.privateKeyPem,
    caCertPem: ca.cert.certPem,
    allowedUses: [...profile.uses],
    validityPeriodHours: profile.validityHours,
    earlyRenewalHours: profile.renewBeforeHours,
  }, child)
  return { key: key.privateKeyPem, cert: cert.certPem, renewal: cert.readyForRenewal }
}
```

## [3]-[INGRESS_EDGE]

[INGRESS_EDGE]:
- Owner: the tier's edge assembly — the issued triple sinks into one `core/v1.Secret` of `type: "kubernetes.io/tls"` (`stringData` carrying `tls.crt`/`tls.key`), the `networking/v1.Ingress` references it by `secretName` and routes the hostname to the workload service, and `_EXPOSED` dispatches the profile's exposure row under one Cloudflare provider built from the Doppler-read `apiToken`.
- Law: the hostname derives once — `app.domain` from the spec's `domain` Option; an absent domain or zone fails the tier loudly at construction (`RunError` naming the coordinate), because traffic without an address is a spec defect.
- Law: `direct` is a proxied record at the metal — `cloudflare.DnsRecord` (`type: "A"`, `content: address`, `proxied: true`, zone from the spec's `zone`), the current-spelling class, never the deprecated `Record` alias; `tunnel` is the no-public-address row — `ZeroTrustTunnelCloudflared` with its config and a CNAME onto the tunnel, held at signature depth until the tunnel arg surfaces are catalogued to operator depth.
- Law: one provider per arm — the Cloudflare provider constructs once here from the fan-in token and threads `{ provider }` to every record; a per-record provider is the named defect.
- Entry: `new Traffic("traffic", { spec, namespace, service, port, apiToken }, opts)`; `traffic.hostname` feeds `StackOutputs.ingress`, `traffic.renewal` feeds the drift watch.
- Growth: a rules row (WAF, redirect) is one `cloudflare.Ruleset` member on the same provider; a second hostname is a second issue-and-sink pass on the same CA.
- Boundary: `ZeroTrustAccessApplication`/`ZeroTrustAccessPolicy` authentication rows ride the tunnel arm when it settles; cloud-LB ingress is the prepared columns'.
- Packages: `@pulumi/kubernetes` (`core.v1.Secret`, `networking.v1.Ingress`); `@pulumi/cloudflare` (`Provider`, `DnsRecord`, `ZeroTrustTunnelCloudflared`); `effect` (`Option`).

```typescript
declare const _tunneled: (
  name: string,
  provider: cloudflare.Provider,
  hostname: string,
  zone: pulumi.Input<string>,
  child: pulumi.CustomResourceOptions,
) => pulumi.Output<string>

class Traffic extends Tier {
  readonly hostname: string
  readonly renewal: pulumi.Output<boolean>
  constructor(name: string, args: Traffic.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Traffic", name, opts)
    const profile = args.profile ?? _Profile.make({})
    const domain = Option.getOrThrowWith(args.spec.domain, () => new pulumi.RunError("<missing-domain>"))
    const zone = Option.getOrThrowWith(args.spec.zone, () => new pulumi.RunError("<missing-zone>"))
    this.hostname = `${args.spec.app}.${domain}`
    const ca = _ca(name, profile, this.child())
    const issued = _issued(name, profile, ca, this.hostname, this.child())
    this.renewal = issued.renewal
    const sink = new k8s.core.v1.Secret(`${name}-tls`, {
      metadata: { namespace: args.namespace },
      type: "kubernetes.io/tls",
      stringData: { "tls.crt": issued.cert, "tls.key": issued.key },
    }, this.child())
    new k8s.networking.v1.Ingress(name, {
      metadata: { namespace: args.namespace },
      spec: {
        tls: [{ hosts: [this.hostname], secretName: sink.metadata.name }],
        rules: [{
          host: this.hostname,
          http: {
            paths: [{
              path: "/",
              pathType: "Prefix",
              backend: { service: { name: args.service, port: { number: args.port } } },
            }],
          },
        }],
      },
    }, this.child())
    const provider = new cloudflare.Provider(`${name}-cf`, { apiToken: args.apiToken }, { parent: this })
    const exposure: Traffic.Exposure = args.spec.profile.exposure === "direct"
      ? { mode: "direct", address: Option.getOrThrowWith(args.spec.connection, () => new pulumi.RunError("<missing-connection>")).host }
      : { mode: "tunnel" }
    const _record = exposure.mode === "direct"
      ? new cloudflare.DnsRecord(name, {
          zoneId: zone,
          type: "A",
          name: this.hostname,
          content: exposure.address,
          proxied: true,
          ttl: 1,
        }, this.child({ provider })).name
      : _tunneled(name, provider, this.hostname, zone, this.child({ provider }))
    this.seal({ hostname: this.hostname, renewal: this.renewal })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Traffic }
```

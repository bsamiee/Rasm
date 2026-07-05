# [IAC_TRAFFIC]

The network edge of the `selfhosted-k8s` arm: one `Traffic` tier sinks the issued certificate triple into the `kubernetes.io/tls` secret, fronts the workload service with a typed `networking/v1.Ingress`, fences the namespace with the default-deny `NetworkPolicy` the policy pack demands, and exposes the hostname by the profile's exposure row — a proxied `cloudflare.DnsRecord` at the metal address (`direct`) or a Zero-Trust tunnel when the cluster has no public address (`tunnel`). Certificate material is not minted here: the CA root and leaf issuance are `operate/secret.md`'s `Certs` pipeline, and this tier receives the issuance capability — the arm injects `Certs.issue` partially applied over its CA and profile, the tier calls it once with the derived hostname, and the `{ key, cert, renewal }` triple lands in the sink — so the material owner and the network edge cannot blur and the hostname exists once. `renewal` re-projects as the tier's rotation watch, the boolean the drift fold reads as a `tls:`-prefixed step moving through its reissue window. One Cloudflare provider constructs per arm from the Doppler fan-in token and threads every record. The module is `iac/src/kube/traffic.ts`; a new exposure mode is one dispatch row, an mTLS mesh leaf is one more issuance call against the same CA, a WAF posture is one `Ruleset` member on the same provider.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                              | [PUBLIC]  |
| :-----: | :------------- | :------------------------------------------------------------------ | :-------- |
|  [01]   | `INGRESS_EDGE` | the TLS secret sink, the typed ingress, the namespace network fence | `Traffic` |
|  [02]   | `EXPOSURE`     | the direct-DNS-versus-tunnel dispatch under one provider            | `Traffic` |

## [2]-[INGRESS_EDGE]

[INGRESS_EDGE]:
- Owner: the tier's edge assembly — the issued triple sinks into one `core/v1.Secret` of `type: "kubernetes.io/tls"` (`stringData` carrying `tls.crt`/`tls.key`), the `networking/v1.Ingress` references it by `secretName` and routes the hostname to the workload service, and the `NetworkPolicy` closes the namespace: ingress admitted only from the ingress-controller namespace or the arm's tunnel connector pods to the service port, egress open — the connector row is inert under `direct` because no pod carries the label, and the fence is what the policy pack's cross-resource row verifies exists wherever a `Deployment` does.
- Law: the hostname derives once — `app.domain` from the spec's `domain` Option; an absent domain or zone fails the tier loudly at construction (`RunError` naming the coordinate), because traffic without an address is a spec defect.
- Law: material crosses as the triple only — `args.issue(hostname)` yields `Certs.Issued` with `key` and `cert` as state-encrypted `Output`s and `renewal` as the rotation boolean; the tier never sees a private-key PEM outside the sink write, and a second consumer of the same material is a second sink row, never a second issuance.
- Law: TLS is structural — the ingress carries its `tls` block by construction and the policy pack rejects one without it, so an unencrypted edge is unshippable from both directions.
- Law: the controller identity is one `_EDGE` anchor — the ingress's `ingressClassName` and the fence's namespace selector are its two projections, so the bootstrap-installed controller renames in one edit and the routing class and the admission fence cannot drift; a fence naming a namespace no controller occupies, or an ingress naming a class no controller serves, is the split this anchor deletes.
- Law: the connector identity is the `_connector` projection — the fence's same-namespace admission row and the tunnel Deployment's labels and selector read one spelling, so a fence that blocks its own connector is unspellable and a connector rename lands in one edit.
- Entry: `new Traffic("traffic", { spec, namespace, service, port, connector, issue, apiToken }, opts)`; `traffic.hostname` feeds `StackOutputs.ingress`, `traffic.renewal` feeds the drift watch.
- Growth: a second hostname is a second issue-and-sink pass on the same CA; a stricter fence (egress allowlist) is one `NetworkPolicy` spec row.
- Boundary: issuance mechanics, the usage vocabulary, and the renewal window are `operate/secret.md`'s; the workload service is `kube/workload.md`'s; the cloud-LB ingress cells are the prepared arms'.
- Packages: `@pulumi/kubernetes` (`core.v1.Secret`, `networking.v1.Ingress`, `networking.v1.NetworkPolicy`); `@pulumi/pulumi`; `effect` (`Option`); `../program/spec.ts` (`StackSpec`, `Tier`); `../operate/secret.ts` (`Certs`).

```typescript
import * as cloudflare from "@pulumi/cloudflare"
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import * as random from "@pulumi/random"
import { Option } from "effect"
import type { Certs } from "../operate/secret.ts"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace Traffic {
  type Exposure =
    | { readonly mode: "direct"; readonly address: string }
    | { readonly mode: "tunnel" }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly service: pulumi.Input<string>
    readonly port: number
    readonly connector: pulumi.Input<string>
    readonly issue: (hostname: string) => Certs.Issued
    readonly apiToken: pulumi.Input<string>
  }
}

const _EDGE = { className: "nginx", namespace: "ingress-nginx" } as const

const _connector = (name: string): { readonly "app.kubernetes.io/name": string } => ({
  "app.kubernetes.io/name": `${name}-connector`,
})

const _fenced = (
  name: string,
  args: Traffic.Args,
  child: pulumi.CustomResourceOptions,
): k8s.networking.v1.NetworkPolicy =>
  new k8s.networking.v1.NetworkPolicy(`${name}-fence`, {
    metadata: { namespace: args.namespace },
    spec: {
      podSelector: {},
      policyTypes: ["Ingress"],
      ingress: [{
        from: [
          { namespaceSelector: { matchLabels: { "kubernetes.io/metadata.name": _EDGE.namespace } } },
          { podSelector: { matchLabels: _connector(name) } },
        ],
        ports: [{ port: args.port, protocol: "TCP" }],
      }],
    },
  }, child)
```

## [3]-[EXPOSURE]

[EXPOSURE]:
- Law: `direct` is a proxied record at the metal — `cloudflare.DnsRecord` (`type: "A"`, `content: address`, `proxied: true`, zone from the spec's `zone`), the current-spelling class, never the deprecated `Record` alias; the address is the proven connection host, so exposure and bootstrap share one coordinate.
- Law: `tunnel` is the no-public-address row, settled whole — an epoch-keyed `RandomBytes` mints the tunnel secret, `ZeroTrustTunnelCloudflared` names the tunnel, its `…Config` `ingresses` rows route the hostname to the in-cluster service and close on the `http_status:404` catch-all, `getZeroTrustTunnelCloudflaredTokenOutput` feeds the secret-wrapped `TUNNEL_TOKEN` of the in-cluster `cloudflared` connector `Deployment` (image from the arm's `connector` pin, k8s-provider-bound while every Cloudflare row rides the arm provider), a `DnsRecord` CNAMEs the hostname onto `<tunnelId>.cfargotunnel.com`, and a `ZeroTrustAccessPolicy`/`…Application` pair fronts the edge; the `account` coordinate proves exactly as `domain`/`zone` do.
- Law: one provider per arm — the Cloudflare provider constructs once here from the fan-in token and threads `{ provider }` to every record; a per-record provider is the named defect.
- Law: a rules posture is a member, not a mode — a WAF or redirect need is one `cloudflare.Ruleset` row on the same provider (`phase` selecting the rules engine plane), growth the exposure dispatch never widens for.
- Growth: an identity-gated posture is one `includes` row on the access policy (`ZeroTrustAccessGroup` when a group earns it); a second zone is a second record on the same provider.
- Boundary: the token's mint and fan-in are `operate/secret.md`'s; the prepared `cloudflare` arm's own cells are `program/provider.md`'s.
- Packages: `@pulumi/cloudflare` (`Provider`, `DnsRecord`, `ZeroTrustTunnelCloudflared`, `ZeroTrustTunnelCloudflaredConfig`, `getZeroTrustTunnelCloudflaredTokenOutput`, `ZeroTrustAccessApplication`, `ZeroTrustAccessPolicy`, `Ruleset`); `@pulumi/random` (`RandomBytes`); `@pulumi/kubernetes` (`apps.v1.Deployment`); `effect` (`Option`).

```typescript
const _tunneled = (
  name: string,
  args: Traffic.Args,
  coords: { readonly hostname: string; readonly zone: pulumi.Input<string>; readonly account: string },
  opts: { readonly cf: pulumi.CustomResourceOptions; readonly kube: pulumi.CustomResourceOptions },
): pulumi.Output<string> => {
  const secret = new random.RandomBytes(`${name}-tunnel-key`, { length: 32, keepers: { epoch: args.spec.epoch } }, opts.cf)
  const tunnel = new cloudflare.ZeroTrustTunnelCloudflared(name, {
    accountId: coords.account,
    name: coords.hostname,
    tunnelSecret: secret.base64,
    configSrc: "cloudflare",
  }, opts.cf)
  new cloudflare.ZeroTrustTunnelCloudflaredConfig(name, {
    accountId: coords.account,
    tunnelId: tunnel.id,
    config: {
      ingresses: [
        { hostname: coords.hostname, service: pulumi.interpolate`http://${args.service}:${args.port}` },
        { service: "http_status:404" },
      ],
    },
  }, opts.cf)
  const labels = _connector(name)
  const token = cloudflare.getZeroTrustTunnelCloudflaredTokenOutput({ accountId: coords.account, tunnelId: tunnel.id }, opts.cf)
  new k8s.apps.v1.Deployment(`${name}-connector`, {
    metadata: { namespace: args.namespace, labels },
    spec: {
      replicas: 2,
      selector: { matchLabels: labels },
      template: {
        metadata: { labels },
        spec: {
          containers: [{
            name: "cloudflared",
            image: args.connector,
            args: ["tunnel", "run"],
            env: [{ name: "TUNNEL_TOKEN", value: pulumi.secret(token.token) }],
          }],
        },
      },
    },
  }, opts.kube)
  const admit = new cloudflare.ZeroTrustAccessPolicy(name, {
    accountId: coords.account,
    name: `${name}-admit`,
    decision: "allow",
    includes: [{ everyone: {} }],
  }, opts.cf)
  new cloudflare.ZeroTrustAccessApplication(name, {
    accountId: coords.account,
    name: coords.hostname,
    domain: coords.hostname,
    type: "self_hosted",
    sessionDuration: "24h",
    policies: [{ id: admit.id }],
  }, opts.cf)
  return new cloudflare.DnsRecord(name, {
    zoneId: coords.zone,
    type: "CNAME",
    name: coords.hostname,
    content: pulumi.interpolate`${tunnel.id}.cfargotunnel.com`,
    proxied: true,
    ttl: 1,
  }, opts.cf).name
}

class Traffic extends Tier {
  readonly hostname: string
  readonly renewal: pulumi.Output<boolean>
  constructor(name: string, args: Traffic.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Traffic", name, opts)
    const domain = Option.getOrThrowWith(args.spec.domain, () => new pulumi.RunError("<missing-domain>"))
    const zone = Option.getOrThrowWith(args.spec.zone, () => new pulumi.RunError("<missing-zone>"))
    this.hostname = `${args.spec.app}.${domain}`
    const issued = args.issue(this.hostname)
    this.renewal = issued.renewal
    const sink = new k8s.core.v1.Secret(`${name}-tls`, {
      metadata: { namespace: args.namespace },
      type: "kubernetes.io/tls",
      stringData: { "tls.crt": issued.cert, "tls.key": issued.key },
    }, this.child())
    new k8s.networking.v1.Ingress(name, {
      metadata: { namespace: args.namespace },
      spec: {
        ingressClassName: _EDGE.className,
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
    _fenced(name, args, this.child())
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
      : _tunneled(name, args, {
          hostname: this.hostname,
          zone,
          account: Option.getOrThrowWith(args.spec.account, () => new pulumi.RunError("<missing-account>")),
        }, { cf: this.child({ provider }), kube: this.child() })
    this.seal({ hostname: this.hostname, renewal: this.renewal })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Traffic }
```

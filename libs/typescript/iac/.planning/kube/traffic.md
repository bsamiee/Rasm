# [IAC_TRAFFIC]

The network edge of the `selfhosted-k8s` arm: one `Traffic` tier sinks the issued certificate triple into the `kubernetes.io/tls` secret, fronts the workload service through the Gateway API — a typed `Gateway` listener terminating TLS on the sink plus one `HTTPRoute` per hostname, generated as committed `crd2pulumi` classes — fences the namespace with the default-deny `NetworkPolicy` the policy pack demands, automates DNS through the external-dns chart row reading the route state, and realizes the exposure as the `Edge` tagged family the provider arm proves on the rail: `Direct` carries the metal address external-dns publishes through the target annotation, `Tunnel` carries the account for the Zero-Trust row. The legacy `networking/v1.Ingress` survives as the fallback row of the `_EDGES` vocabulary for clusters whose controller predates the Gateway class — one table row, never a second code path. Certificate material is not minted here: the CA root and leaf issuance are `operate/secret.md`'s `Certs` pipeline, and this tier receives the issuance capability — the arm injects `Certs.issue` partially applied over its CA and profile, the tier calls it once with the derived hostname, and the `{ key, cert, renewal }` triple lands in the sink — so the material owner and the network edge cannot blur and the hostname exists once. `renewal` re-projects as the tier's rotation watch. A WAF posture is `waf` rows compiled onto one `cloudflare.Ruleset`, per-tenant vanity domains are `vanity` rows compiled onto `CustomHostname`, and one Cloudflare provider constructs per arm from the Doppler fan-in token and threads every record. The module is `iac/src/kube/traffic.ts`; a new exposure mode is one `Edge` case plus its `$match` arm, an mTLS mesh leaf is one more issuance call against the same CA, a WAF rule is one data row.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                    | [PUBLIC]  |
| :-----: | :------------- | :------------------------------------------------------------------------ | :-------- |
|  [01]   | `EDGE_FAMILY`  | the proven exposure cases, the `_EDGES` api vocabulary, the fence         | `Traffic` |
|  [02]   | `EDGE_REALIZE` | gateway/route construction, external-dns, tunnel row, WAF and vanity rows | `Traffic` |

## [02]-[EDGE_FAMILY]

[EDGE_FAMILY]:
- Owner: `Traffic.Edge`, the `Data.taggedEnum` the provider arm constructs after proving every coordinate on the `DeployFault` rail — `Direct` (domain, zone, the metal address shared with the bootstrap connection) and `Tunnel` (domain, zone, account); the tier receives a proven case and dispatches `$match`, so no constructor throw exists for a spec-derivable value and traffic without an address is unspellable rather than a runtime error.
- Law: the edge api is the `_EDGES` vocabulary — the `gateway` row (the Gateway class name, the controller namespace, the typed `Gateway`+`HTTPRoute` realizer) is primary; the `ingress` row (`networking/v1.Ingress` with `ingressClassName`) is the legacy fallback a cluster without the Gateway class selects; both rows read one controller-identity anchor, so the routing class and the admission fence cannot drift, and a controller rename lands in one row edit.
- Law: the fence closes the namespace — ingress admitted only from the edge row's controller namespace or the arm's tunnel connector pods to the service port, egress open; the connector row is inert under `Direct` because no pod carries the label, and the fence is what the policy pack's cross-resource row verifies exists wherever a `Deployment` does.
- Law: material crosses as the triple only — `args.issue(hostname)` yields `Certs.Issued` with `key` and `cert` as state-encrypted `Output`s and `renewal` as the rotation boolean; the tier never sees a private-key PEM outside the sink write, and a second consumer of the same material is a second sink row, never a second issuance.
- Law: the cert lanes stay split — the injected `Certs.issue` is the mesh/self-signed lane this sink consumes; browser-trusted certs outside a cluster are `operate/secret.md`'s acme lane; in-cluster ACME lands as `crd2pulumi`-generated cert-manager `Certificate`/`ClusterIssuer` rows with the `gatewayHTTPRoute` solver when an estate finalizes it, replacing the sink's input, never this tier's shape.
- Law: the connector identity is the `_connector` projection — the fence's same-namespace admission row and the tunnel Deployment's labels and selector read one spelling, so a fence that blocks its own connector is unspellable and a connector rename lands in one edit.
- Entry: `new Traffic("traffic", { spec, namespace, service, port, connector, dnsVersion, issue, apiToken, edge }, opts)`; `traffic.hostname` feeds `StackOutputs.ingress`, `traffic.renewal` feeds the drift watch.
- Growth: a second hostname is a second issue-and-sink pass on the same CA; a stricter fence (egress allowlist) is one `NetworkPolicy` spec row; an identity-gated posture is one `includes` row on the access policy (`ZeroTrustAccessGroup` when a group earns it).
- Boundary: issuance mechanics, the usage vocabulary, and the renewal window are `operate/secret.md`'s; the workload service is `kube/workload.md`'s; the cloud-LB ingress cells are the prepared arms'; the generated `crds/gateway` module is committed `crd2pulumi` output regenerated on Gateway API bumps, never an npm pin.
- Packages: `@pulumi/kubernetes` (`core.v1.Secret`, `networking.v1.Ingress`, `networking.v1.NetworkPolicy`, `helm.v4.Chart`, `apps.v1.Deployment`); `../crds/gateway` (`v1.Gateway`, `v1.HTTPRoute` — crd2pulumi); `@pulumi/cloudflare`; `@pulumi/random` (`RandomBytes`); `effect` (`Array`, `Data`); `../program/spec.ts` (`StackSpec`, `Tier`); `../operate/secret.ts` (`Certs`).

```typescript
import * as cloudflare from "@pulumi/cloudflare"
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import * as random from "@pulumi/random"
import { Array, Data } from "effect"
import * as gateway from "../crds/gateway"
import type { Certs } from "../operate/secret.ts"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace Traffic {
  type Edge = Data.TaggedEnum<{
    Direct: { readonly domain: string; readonly zone: string; readonly address: string }
    Tunnel: { readonly domain: string; readonly zone: string; readonly account: string }
  }>
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly service: pulumi.Input<string>
    readonly port: number
    readonly connector: pulumi.Input<string>
    readonly dnsVersion: pulumi.Input<string>
    readonly issue: (hostname: string) => Certs.Issued
    readonly apiToken: pulumi.Input<string>
    readonly edge: Traffic.Edge
    readonly api?: keyof typeof _EDGES
    readonly waf?: ReadonlyArray<{ readonly expression: string; readonly action: "block" | "challenge" | "skip" }>
    readonly vanity?: ReadonlyArray<string>
  }
}

const _EDGES = {
  gateway: { class: "nginx", namespace: "nginx-gateway" },
  ingress: { class: "nginx", namespace: "ingress-nginx" },
} as const

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
          { namespaceSelector: { matchLabels: { "kubernetes.io/metadata.name": _EDGES[args.api ?? "gateway"].namespace } } },
          { podSelector: { matchLabels: _connector(name) } },
        ],
        ports: [{ port: args.port, protocol: "TCP" }],
      }],
    },
  }, child)
```

## [03]-[EDGE_REALIZE]

[EDGE_REALIZE]:
- Law: the gateway is the typed edge — one `gateway.v1.Gateway` under the `_EDGES.gateway.class` GatewayClass carries an HTTPS listener terminating TLS on the sink secret, one `gateway.v1.HTTPRoute` binds the hostname to the workload service backend, and every rendered object is a committed `crd2pulumi` class under full Pulumi diff and CrossGuard visibility; the `ingress` fallback row constructs the same hostname/secret/service triple through `networking/v1.Ingress` and nothing else differs.
- Law: DNS is automated from route state — the external-dns chart row installs at its pinned version with the gateway sources (`gateway-httproute`) and the Cloudflare token riding a namespace `Secret` reference, never a plaintext chart value, so a hostname on an `HTTPRoute` becomes its record with no per-hostname authoring; under `Direct` the route carries the `external-dns.alpha.kubernetes.io/target` annotation pinning the metal address, and under `Tunnel` the CNAME onto `<tunnelId>.cfargotunnel.com` stays the one explicitly authored record because its target is tunnel state, not route state.
- Law: `Tunnel` is the no-public-address row, settled whole — an epoch-keyed `RandomBytes` mints the tunnel secret, `ZeroTrustTunnelCloudflared` names the tunnel, its `…Config` `ingresses` rows route the hostname to the in-cluster service and close on the `http_status:404` catch-all, `getZeroTrustTunnelCloudflaredTokenOutput` feeds the secret-wrapped `TUNNEL_TOKEN` of the in-cluster `cloudflared` connector `Deployment`, and a `ZeroTrustAccessPolicy`/`…Application` pair fronts the edge.
- Law: one provider per arm — the Cloudflare provider constructs once here from the fan-in token and threads `{ provider }` to every record; a per-record provider is the named defect.
- Law: rules and vanity are data rows — `waf` rows compile onto one `cloudflare.Ruleset` (`phase: "http_request_firewall_custom"`, one rule per row) on the same provider, and `vanity` rows compile onto per-tenant `cloudflare.CustomHostname` rows against the proven zone, so a SaaS estate's custom domains and its WAF posture grow by rows the exposure dispatch never widens for.
- Growth: a second zone is a second record set on the same provider; a `GRPCRoute`/`TLSRoute` is one more generated class row beside the HTTPRoute; a `BackendTLSPolicy` lands beside the route when the backend hop earns TLS.
- Boundary: the token's mint and fan-in are `operate/secret.md`'s; the prepared `cloudflare` arm's own cells are `program/provider.md`'s; external-dns chart values drift with its pinned version.
- Packages: `@pulumi/cloudflare` (`Provider`, `DnsRecord`, `Ruleset`, `CustomHostname`, `ZeroTrustTunnelCloudflared`, `ZeroTrustTunnelCloudflaredConfig`, `getZeroTrustTunnelCloudflaredTokenOutput`, `ZeroTrustAccessApplication`, `ZeroTrustAccessPolicy`).

```typescript
const _tunneled = (
  name: string,
  args: Traffic.Args,
  coords: { readonly hostname: string; readonly zone: string; readonly account: string },
  opts: { readonly cf: pulumi.CustomResourceOptions; readonly kube: pulumi.CustomResourceOptions },
): void => {
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
  new cloudflare.DnsRecord(name, {
    zoneId: coords.zone,
    type: "CNAME",
    name: coords.hostname,
    content: pulumi.interpolate`${tunnel.id}.cfargotunnel.com`,
    proxied: true,
    ttl: 1,
  }, opts.cf)
}

const Edge = Data.taggedEnum<Traffic.Edge>()

const _EDGED: {
  readonly [K in keyof typeof _EDGES]: (
    name: string,
    args: Traffic.Args,
    hostname: string,
    sink: pulumi.Output<string>,
    child: pulumi.CustomResourceOptions,
  ) => void
} = {
  gateway: (name, args, hostname, sink, child) => {
    const plane = new gateway.v1.Gateway(name, {
      metadata: { namespace: args.namespace },
      spec: {
        gatewayClassName: _EDGES.gateway.class,
        listeners: [{
          name: "https",
          protocol: "HTTPS",
          port: 443,
          hostname,
          tls: { mode: "Terminate", certificateRefs: [{ name: sink }] },
        }],
      },
    }, child)
    new gateway.v1.HTTPRoute(name, {
      metadata: {
        namespace: args.namespace,
        annotations: Edge.$match(args.edge, {
          Direct: ({ address }) => ({ "external-dns.alpha.kubernetes.io/target": address }),
          Tunnel: () => ({}),
        }),
      },
      spec: {
        parentRefs: [{ name: plane.metadata.name }],
        hostnames: [hostname],
        rules: [{ backendRefs: [{ name: args.service, port: args.port }] }],
      },
    }, child)
  },
  ingress: (name, args, hostname, sink, child) =>
    void new k8s.networking.v1.Ingress(name, {
      metadata: { namespace: args.namespace },
      spec: {
        ingressClassName: _EDGES.ingress.class,
        tls: [{ hosts: [hostname], secretName: sink }],
        rules: [{
          host: hostname,
          http: {
            paths: [{
              path: "/",
              pathType: "Prefix",
              backend: { service: { name: args.service, port: { number: args.port } } },
            }],
          },
        }],
      },
    }, child),
}

class Traffic extends Tier {
  static readonly Edge = Edge
  readonly hostname: string
  readonly renewal: pulumi.Output<boolean>
  constructor(name: string, args: Traffic.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Traffic", name, opts)
    this.hostname = `${args.spec.app}.${args.edge.domain}`
    const issued = args.issue(this.hostname)
    this.renewal = issued.renewal
    const sink = new k8s.core.v1.Secret(`${name}-tls`, {
      metadata: { namespace: args.namespace },
      type: "kubernetes.io/tls",
      stringData: { "tls.crt": issued.cert, "tls.key": issued.key },
    }, this.child())
    _EDGED[args.api ?? "gateway"](name, args, this.hostname, sink.metadata.name, this.child())
    _fenced(name, args, this.child())
    const provider = new cloudflare.Provider(`${name}-cf`, { apiToken: args.apiToken }, { parent: this })
    const dnsToken = new k8s.core.v1.Secret(`${name}-dns-token`, {
      metadata: { namespace: args.namespace },
      stringData: { CF_API_TOKEN: args.apiToken },
    }, this.child())
    new k8s.helm.v4.Chart(`${name}-dns`, {
      chart: "external-dns",
      repositoryOpts: { repo: "https://kubernetes-sigs.github.io/external-dns/" },
      version: args.dnsVersion,
      namespace: args.namespace,
      values: {
        sources: ["gateway-httproute"],
        provider: { name: "cloudflare" },
        env: [{
          name: "CF_API_TOKEN",
          valueFrom: { secretKeyRef: { name: dnsToken.metadata.name, key: "CF_API_TOKEN" } },
        }],
        domainFilters: [args.edge.domain],
      },
    }, this.child())
    Edge.$match(args.edge, {
      Direct: () => undefined,
      Tunnel: ({ zone, account }) =>
        _tunneled(name, args, { hostname: this.hostname, zone, account }, {
          cf: this.child({ provider }),
          kube: this.child(),
        }),
    })
    Array.match(args.waf ?? [], {
      onEmpty: () => undefined,
      onNonEmpty: (rows) =>
        new cloudflare.Ruleset(`${name}-waf`, {
          zoneId: args.edge.zone,
          name: `${name}-waf`,
          kind: "zone",
          phase: "http_request_firewall_custom",
          rules: Array.map(rows, (row) => ({ expression: row.expression, action: row.action })),
        }, this.child({ provider })),
    })
    Array.map(args.vanity ?? [], (hostname) =>
      new cloudflare.CustomHostname(hostname, {
        zoneId: args.edge.zone,
        hostname,
      }, this.child({ provider })))
    this.seal({ hostname: this.hostname, renewal: this.renewal })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Traffic }
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

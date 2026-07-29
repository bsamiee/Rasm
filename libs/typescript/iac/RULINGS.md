# [TS_IAC_RULINGS]

`typescript/iac` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- Entries admitting caller data through a `Schema` type their parameter as the ENCODED row, never the decoded one — a defaulted or optional field exists so a caller may omit it, so demanding the decoded shape at the type boundary refuses forms the admission itself accepts and makes every caller roster restate what the default was minted to fill; the decoded row is what the owner's own derivations read, after admission, and the two spellings are one schema's two faces rather than two types. Reopens only where a field's default resolves from a value the entry cannot see.
- Publish planes prove their own addresses — an address derived from caller data and never joined against the artifact that carries the bytes is a declaration, and no downstream drift, diff, or reconcile signal converts it into evidence, because a resource the sync never created reports nothing; the coordinate that spells the address spells the on-disk location, once, and presence refuses at graph construction where both halves are in hand. Reopens where the publishing host cannot read the artifact it publishes.
- Every Helm chart row pins its rendered fullname AND carries its own rendered-Service projection, and every endpoint derives from that projection — the chart-side helper collapses to the release name only when that name CONTAINS the chart name, so a release named for its signal renders `<release>-<chart>`, while a chart whose workload is a custom resource hands naming to its controller, which decorates the pinned name further; re-deriving an endpoint from chart-name arithmetic, or assuming the pinned name and the rendered Service agree, re-opens the class of dead addresses the pin closes.
- Rule evaluation rides the selected store row's own evaluator, never a Grafana-managed resource — `alerting.v0alpha1.RecordingRule` writes its output series through the target datasource's Prometheus remote-write API, and the estate declines remote-write, so arming that resource re-opens the ingest door the one-ingress posture shuts; the burn numerator lands on the store row's `rules` column, and a row whose dialect evaluates nothing declares that on `degrade` and falls back to inline rendering. Reopens only when a store row's ingest posture admits a write path other than OTLP.
- Columnar analytics residences take logs and traces alone — those two carry the unbounded attribute shape a wide-event residence exists to hold, metrics stay on the metrics-store row because a TSDB is what answers alerting, and profiles stay on the profile store until Tier-0's own swap point arms; a residence taking a third signal turns one plane's declared degradation into a second unowned truth. Reopens when the residence's own metric support leaves alpha.
- Arrow transport arms on the collector-to-collector hop alone — a columnar dictionary-encoded gRPC stream pays for itself where telemetry crosses node and zone boundaries, while the gateway-to-backend hop is one in-namespace call against backends that speak no Arrow; the arrow receiver replaces the shared memory guard with its own admission bounds, so that door states its limits where every other pipeline states the guard. Reopens when a backend row admits Arrow ingest directly.
- Board datasource keys name the PLANE each answers and each row states its own realization — `metrics` resolves to whichever store row the spec selected and `residence` to whichever armed residence row carries a Grafana driver, while a plane the spec disarmed provisions nothing and refuses the panel naming it; keying by engine gives a victoriametrics stack a datasource named for its reference row and aims a wide-event driver at whichever plane the resident order led with.
- Tiers selecting a backend project the RENDER coordinates beside the addresses — translation column, histogram representation, and armed residence coordinate seal with the URL plane, so packs a composition root builds and rules the board tier compiles spell one series grammar; a root minting its own target re-spells the estate pin against a row that translated differently, silently.
- `fullnameOverride` nesting is a per-row projection, never a shared assumption — a flat key and a scaffold nesting it differ per chart, so one spelling applied across the family silently misses every row whose scaffold nests.
- Deploy-plane credential material reaches a chart as environment sourced from a Secret, never as a values literal — chart values render into a ConfigMap landing in cluster state and every stack export, so a DSN, password, or token spelled there is plaintext at two more residences than the in-graph `Output` it came from; the collector's `${env:…}` expansion and a container's `secretKeyRef` are the admitted forms, and a values literal survives only for a coordinate the spec already publishes.
- Workload injection carries `DOPPLER_TOKEN` alone — the read-scoped service token embeds project and config and outranks every run-time flag, so `doppler run --` resolves the scoped config from the one variable; a sweep aligning the seam to the security custodian's three-coordinate read re-adds `DOPPLER_PROJECT`/`DOPPLER_CONFIG` rows and forks the token path from the coordinate path. Reopens only when an in-workload process reads the SDK custodian on a path `doppler run` does not populate.

## [03]-[COLLAPSE]

- Selfhosted arms stay parallel authorings with no shared builder — each arm composes its own substrate vocabulary, typed CR classes on the `k8s.Provider` and bridged resource rows on the `docker.Provider`, and neither exists on the other, so a cross-arm builder leaks one substrate's model into both; a dedup sweep over the mirrored arm bodies re-proposes the fold, and only pure-data rosters hoist. Reopens only when a provider ships one resource plane both carriers compose.

## [04]-[STRUCTURE]

- Helm charts and their config documents catalogue at this folder's `.api/` tier under the chart's published name — a fence spelling a values key or a config component makes the same verifiable-member claim an imported package fence makes, and leaving the estate's most drift-prone external contract uncatalogued is what fed a receiver the wrong shape and let an endpoint resolve to nothing.
- Helm charts earn no README package card — a card names a library the folder imports and binds the touch-point set a central-manifest row anchors, while a chart is a deploy-time reference the program's own version record pins, so a card mints an orphaned touch-point aimed at a manifest owning no row for it.

## [05]-[PROCESS]

- (none)

# [TS_IAC_RULINGS]

`typescript/iac` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- Every Helm chart row pins its rendered fullname and every endpoint projection derives from that pinned name — the chart-side helper collapses to the release name only when that name CONTAINS the chart name, so a release named for its signal renders `<release>-<chart>` and any address spelled from the release name alone resolves to nothing; re-deriving an endpoint from chart-name arithmetic, or reading a rendered name off a chart's convention, re-opens the class of dead addresses the pin closes.
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

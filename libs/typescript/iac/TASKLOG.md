# [TS_IAC_TASKLOG]

Deploy-plane work ledger distilled from `IDEAS.md`: each open card leads with a status marker and three to four scoped bullets, and each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open task card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[0005]-[QUEUED]: Grafana delivery and tenant-identity depth rows.
- Capability: `alerting.AlertEnrichment` enriches severity-routed notification bodies beyond the message template; each tenant org gains its own read identity — an org-scoped `oss.ServiceAccount` viewer with `oss.ServiceAccountPermissionItem` grants and `oss.OrganizationPreferences` pinning the overview board as the org home.
- Shape: rows in the `Boards` apply fold on `operate/observe.md` — an enrichment row per contacts entry, a viewer-identity block inside the tenant-org loop.
- Anchors: `operate/observe.md` `_alerted` contacts fold and the tenant-organization loop; `.api/pulumiverse-grafana.md` alerting and oss member tables.
- Tension: tenant credentials egress like the automation token — Doppler custody, never stack outputs.

[0004]-[QUEUED]: Collector depth rows — self-telemetry, eBPF arm, operator-metrics ingest.
- Capability: collector internal telemetry lands in the metrics store through its own `service.telemetry` block; an OBI eBPF arm yields zero-code RED metrics for SDK-less workloads; CNPG operator metrics enter through the collector `prometheusreceiver` under the one-ingress law.
- Shape: rows on `operate/observe.md` — a telemetry block in the collector config fence, an eBPF chart row beside `pyroscope`, a `_pg`-adjacent receiver row scraping the CNPG pods.
- Anchors: `operate/observe.md` collector config fence and the `_pg` receiver pair; `kube/data.md` `Postgres` tier exposing operator metrics.
- Tension: OBI demands privileged eBPF host access — the arm binds only where the deploy target grants it.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

- [0001]-[COMPLETE]: `operate/observe.md` deepened — store family with tenancy/retention/degradation columns, `_pg` ingest arms, `_DEV` row, `_compiled` builder fold, `MessageTemplate`/`MuteTiming`/`Annotation` rows; `spec.md` gained the `observe` profile group.
- [0002]-[COMPLETE]: `@grafana/grafana-foundation-sdk` admitted — README row and `.api/grafana-grafana-foundation-sdk.md`; deploy-host self-telemetry law on `program/automation.md`.
- [0003]-[COMPLETE]: observe realize pass — `_compiled` emission-total (`time`/`annotation`/`withVariable` folds, `description`/`transparent`/`legendFormat`, sorted threshold steps over the `-Infinity` base), `slo.SLO` respell, Editor-role identity with `FolderPermissionItem` grant and `ServiceAccountRotatingToken`, org-scoped tenant fleets, `Dev` tier wired into the docker arm with `Boards` over one URL plane.

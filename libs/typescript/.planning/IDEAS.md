# [TYPESCRIPT_BRANCH_IDEAS]

Branch-level cross-package concert — higher-order ideas coupling two or more TS folders, distilled from the folder registers, never folder-local concepts. A cross-language idea lives in `libs/.planning/IDEAS.md`; `[1]-[OPEN]` holds the live concert and `[2]-[CLOSED]` records a finished or dropped idea with a one-line disposition so it is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[BRANCH_SIGNAL_PLANE]-[ACTIVE]: One signal plane spans the branch — every folder emits through the core `Convention` vocabulary, runtime alone bridges the OTLP wire, iac compiles the backend.
- Capability: Folder-blind observability — instruments, spans, and logs minted in owning folders correlate estate-wide, with raw `@opentelemetry/*` imports confined to `runtime` and the core semconv vocabulary.
- Shape: `core/observe` wire rows (dotted `rasm.*` names, UCUM units, scope = package id, pinned semconv schema, `NoUTF8EscapingWithSuffixes` translation), the `runtime/otel` emit/crash/vital/meter bridge projecting work-plane Facts to `Convention`-keyed instruments, and `iac/operate/observe` realizing stores, dashboards, and alerts from the same `DashboardModel` rows.
- Unlocks: Dashboards and SLO burn alerts compiled from the vocabulary the emitters use, breaking at type-check instead of drifting; any app inherits the full signal plane by composing the export layer at its root.
- Anchors: `core/observe/convention.md`; `runtime/otel/emit.md`; `runtime/otel/meter.md`; `iac/operate/observe.md`; the `dataflow-system.md` `AppIdentity` resource law.
- Ripple: `libs` `[UNIFIED_SIGNAL_FABRIC]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[CONTENT_IDENTITY]-[COMPLETE]: one seed-zero `XxHash128` mint at `core/value/contentKey`, every verifying and keying site delegating; law settled at `dataflow-system.md`.
[INTERCHANGE_DECODE_ONCE]-[COMPLETE]: one keyed codec census at `core/interchange/codec` decodes every C#-minted family exactly once; law settled at `dataflow-system.md`.
[JOURNAL_SPINE]-[COMPLETE]: `data/journal/append` owns the one atomic write with ledger and outbox in-commit, the read side folding through `data/read/fold`; law settled at `dataflow-system.md`.
[TENANCY_SCOPE]-[COMPLETE]: `Tenant.within` is the single scoped write path over `AppIdentity`, isolation a scope value never a fork; law settled at `dataflow-system.md`.
[CROSS_LANGUAGE_INVARIANTS]-[COMPLETE]: wire ownership, content identity, clock, quantity, and receipt-family invariants frozen under `tests/contracts` corpus assertion; law settled at `dataflow-system.md`.

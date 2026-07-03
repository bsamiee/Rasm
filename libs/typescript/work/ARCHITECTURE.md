# [WORK_ARCHITECTURE]

The domain map of `work` — the durable-execution folder. Cluster entities carry the durable-actor runtime, workflows carry the compensation folds, queues and schedules carry the job families, and one `deliver` sub-domain signs and drains every egress channel. Deployment topology is `iac`, queue-as-data is `store`, and each meets `work` only at a typed port or shape seam.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
work/
├── engine/             # cluster runtime: entities, sharding, MessageStorage composition
│   ├── entity.ts       # cluster Entities, sharding, runner discovery (K8sHttpClient is discovery, never provisioning); per-tenant fenced-quota rows; runner entrypoint Layers selected via host/exec at the app root
│   └── storage.ts      # MessageStorage composition law; SqlClient Tag satisfied at the app root
├── flow/               # durable workflows
│   ├── durable.ts      # durable workflow definitions + compensation/saga folds
│   └── activity.ts     # activity rows: retry/timeout budgets from kernel/fault
├── queue/              # durable job families and schedules
│   ├── job.ts          # DurableQueue job families: priority + dedupe/batch keys + DLQ/replay rows
│   └── schedule.ts     # ClusterCron + schedule vocabulary + misfire/window policy rows
└── deliver/            # signed egress: webhook, mail, report, outbox relay
    ├── webhook.ts      # durable signed egress + delivery receipts (signs via security/sign)
    ├── mail.ts         # nodemailer mail egress + locale-keyed template rows + suppression/unsubscribe rows
    ├── report.ts       # document egress rows: exceljs/jspdf/jszip/papaparse durable report jobs
    └── relay.ts        # outbox relay: SKIP LOCKED + LISTEN/NOTIFY drain feeding the channel rows; one fan-out policy row; per-tenant egress quota + DeliverAt
```

## [02]-[SEAMS]

No inbound C# wire seam: durable execution is TS-native capability. `work`'s alignments are intra-branch ports and shapes the app root satisfies — never a `store` import, never a `platform-node/bun` import.

```text seams
engine/storage.ts   ←  store/journal   # [PORT]: SqlClient (@effect/sql core) + MessageStorage Tags; the store-owned driver Layer satisfies at the app root
engine/entity.ts    →  edge/hook       # [PORT]: per-tenant fenced-quota rows + the Fence.Refusal type edge/hook types against
engine/entity.ts    ←  iac/stack       # [SHAPE]: StackOutputs → ShardingConfig, the sole iac↔work meeting seam
flow/durable.ts     →  edge/hook       # [PORT]: Flow.gate tokens resolved by verified inbound callbacks (DurableDeferred out-of-band completion)
deliver/webhook.ts  ←  security/sign   # [BOUNDARY]: HMAC egress signing via the security/sign envelope
deliver/mail.ts     ←  store/journal   # [PORT]: the Mailer.Suppress ledger Tag; store journal Layers satisfy at the app root
deliver/relay.ts    ←  store/journal   # [PORT]+[SHAPE]: the Relay.Wake LISTEN pulse Tag; the deliver_outbox row contract — store authors the ensure DDL at name-level parity with Relay.Row
```

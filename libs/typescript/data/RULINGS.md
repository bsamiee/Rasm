# [TS_DATA_RULINGS]

`typescript/data` rulings settle package-scoped decisions.

## [01]-[PACKAGES]

- `@aws-sdk/client-s3`, `@aws-sdk/lib-storage`, and `@aws-sdk/s3-request-presigner` carry ONE matched pin and move as one wave — the SDK's near-daily release cadence makes a per-package chase pure noise, and the presigner signs the request shapes the client builds, so a split line pairs a signer against a protocol core it was not released with. Bumps land from a deliberate package gate that moves all three and re-runs the object-plane conformance, never from a currency sweep over one row.
- `@gltf-transform/functions` reaches `sharp` transitively through `ndarray-pixels`, whose declared `^0.34` range resolves a SECOND libvips native beside the catalog pin, so the workspace overrides `sharp` to the catalog version and one lineage serves the whole branch. That transitive reach also refutes the "CLI drags an incompatible sharp" rationale for declining the `@gltf-transform/cli` package — the conflict arrives with the library either way, and the CLI stays declined because a spawned binary is not a composed library. `textureCompress`/`compressTexture` are refused on the same ground the override exists to protect: `object/file.md` is the one libvips composer, and a second encoder over the same native forks derivative policy while its encoder-less fallback silently drops every quality option.
- `@effect/sql-mysql2` and `@effect/sql-mssql` stay read-only interop ingress, never the journal/tenant/capability statement set — no statement page spells a `mysql`/`mssql` arm, and the neutral statements' `orElse` arm carries sqlite file-per-app semantics with no tenancy GUC, so a foreign-relational client on the write path silently drops isolation; the posture reopens only when a statement page realizes the arms with an explicit tenancy answer.
- `@qualithm/arrow-flight-client` refusals classify through `core/interchange/codec` `Hops` alone — the package declares `FlightServerError.code` a `string` and populates it with ConnectRPC's numeric `Code`, three of its six exported error classes mint nowhere, and `timeoutMs` reaches no transport, so recovery reads that one status algebra's retryability and fault-class columns while the lane owns every bound; a lane-local code roster, an arm for a never-thrown class, or a deadline resting on the package's own option each fork a classification the first hop already settled.
- `@effect/sql-clickhouse` publishes no query cost on any answer, so cluster profile evidence is a second `system.query_log` read keyed by the statement's own id behind a log flush — a design expecting cost on the result reads a surface the driver never carries. `withQueryId` binds a `FiberRef` for its whole effect, so the scope is exactly ONE statement: a wider scope files every statement under one log key, loses attribution, and hands the driver's `KILL QUERY` interrupt arm every sibling sharing that id.
- Object-engine conformance pins `If-None-Match: *` conditional put as the admission bar — the key IS the content, so atomic create-if-absent keeps concurrent writers of one `ContentKey` idempotent; a plain-put engine races writers into silent overwrite beneath dedup, reference counting, and sweep, so its refusal is a guarantee, never a gap.

## [02]-[SHAPE]

- Signals landing in an analytics residence form a DERIVED plane carrying zero authority — the fact journal keeps sole authority, a residence read never settles billing, admission, or erasure, and a dropped plane rebuilds from the journal window at warm-up cost; `Convention.identity` is the join key both planes agree on, so the fill stamps it on arrival and a plane filled without it answers no reconstruction until a second pass rewrites every row.
- Residences carrying a fill dialect carry their DDL at the same owner and mount both in one statement — a plane this branch writes but never creates fails its first insert against a relation nobody made, and a residence a foreign exporter plants carries neither column, so the pair is total and no caller can bind a plane no owner planted.
- Metric relations enumerate the OTLP POINT model, never the mount roster — the instrument kinds project onto a subset and the unreached relations still plant and read, so a foreign producer's rows land where the plane already declares them; mapping a kind onto an unreached relation forges a series no mount emits, and deleting the relation strands those rows.
- Residence scalars key per instrument KIND — a bucket relation carries no per-point value column its sum-relation sibling carries, so one residence-wide value column reads a name the relation never declared and empties every distribution tile with no error anywhere.
- Lake metrics rise from the emitting process's own live registry, folded into the point relations on a scheduled tick — that snapshot IS the cold tail a metrics store's retention cannot hold, and reaching for a second exporter or a second wire to carry it re-derives ingest the branch already owns.

## [03]-[COLLAPSE]

- Both embedded DuckDB drivers collapse onto ONE leased-session family — node and worker mint the same `Olap.Handle`, so the bulkhead permit, the budget bracket, the replay roster, and the engine-tagged meter fan reach the browser lane by construction; a second read entry beside `Olap.read` leaves a single-threaded worker taking unbounded concurrent statements and taps no measure at all.
- Driver divergence stays a ROW under that one family — result grain, bind vocabulary, and the three execution members ride `_DRIVERS`, so a worker read binds through its prepared seam where the node read binds inline and no geometry arm branches on a driver name.

## [04]-[STRUCTURE]

- (none)

## [05]-[PROCESS]

- (none)

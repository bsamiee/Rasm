# [H1][ASSAY_CONCURRENCY_CRITIQUE]
>**Dictum:** *Wave 2 asks whether assay can host dozens–hundreds of dotnet-heavy agents without hang, lock spin, or stdout corruption.*

<br>

**Verdict:** The design **inherits the right quality primitives** (non-blocking leases, `run_id` scopes, stderr-only process bytes, single Envelope stdout) and **adds polyglot read-only fan-out**, but it is **not yet adequate at hundreds of agents** until build-artifact pinning matches quality, fan-out is bounded, and global exclusives are modeled as capacity—not parallel proof. Implementation is still doc-only (`AUDIT.md` §5).

---
## [1][LEASE_MATRIX]
>**Dictum:** *Exclusive resources fail fast; everything else isolates by path.*

<br>

| [RESOURCE] | [LOCK PATH] | [HOLDER] | [PARALLELISM] | [AT SCALE] |
| :--------- | :---------- | :------- | :------------ | :--------- |
| MSBuild closure | `.artifacts/assay/locks/build-<closure>.lock` | `static build` / full | One winner per closure hash; distinct closures concurrent | Many agents on same touched project → **busy storm** (exit 5), not hang |
| Stryker mutation | `locks/mutation.lock` | `test` mutation row | Global exclusive | **1 mutation** across the fleet |
| Live Rhino + verify | `locks/bridge.lock` | `bridge`, package `quit`/`refresh` | Global exclusive | **1 live Rhino** proof lane |
| Yak stage commit | `locks/package-stage.lock` (per dir) | `package` | Per package dir | Low parallelism; stage dir collisions rare |
| Read-only tools | none | `static` report/plan, `test list`, `docs`, `api` | `fan_out` + distinct `run_id` | Scales with disk and CPU, not locks |

Leases port `exclusive_lease` / `leased` from `tools/quality/process.py` (L260–308): `LOCK_EX|LOCK_NB`, owner block records `run_id`/`pid`/`cwd`, truncate-on-release, **never block**. `@retried` must not retry `BUSY` (`aot-stamina.md`, `aspect.md`).

[CRITICAL] **Unpinned:** stable **per-closure** MSBuild tree (quality `ArtifactScope.build` → `.artifacts/quality/build/<closure>`, not `run_id`) is proven in production settings but assay `ArtifactScope.open` only documents `artifact(PROCESS, claim, run_id)`. Without an explicit `SCOPE`/`build(closure)` arm, agents either bloat disk (run-scoped `--artifacts-path`) or reintroduce `obj/` races (shared default paths).

---
## [2][RUN_ID_ISOLATION]
>**Dictum:** *Concurrency safety for parallel agents is mostly path uniqueness, not locks.*

<br>

| [ARTIFACT] | [QUALITY PATH] | [ASSAY PATH] | [ISOLATION] |
| :--------- | :------------- | :----------- | :---------- |
| Rail scope + `DOTNET_CLI_HOME` | `.artifacts/quality/<rail>/<run_id>/` | `.artifacts/assay/<claim>/<run_id>/` | Per invocation — **good** |
| Process stream logs | `.../process/<command-id>/` | Same under assay scope | Per run; **collision risk** if slug is argv-only (`engine.md` open #1) — prefer `check.id` |
| Unit test output | `.artifacts/test/<slice>/<run_id>/` | `artifact(TEST, slice, run_id)` | Per run — **good** |
| Mutation output | `.artifacts/mutation/<slice>/<run_id>/` | `artifact(MUTATION, slice, run_id)` | Per run; lock still global |
| Verify reports | `.artifacts/rhino/verify/<run_id>/` | `artifact(RHINO, verify, run_id)` | Per run + TTL |
| API surface cache | `.artifacts/quality/api/surface/` (cross-run) | Not specified | Read-only cache OK if fingerprint-keyed; **writes** need lock or atomic rename |

`run_id` default `"%Y-%m-%dT%H-%M-%S.%f-{pid}"` (`settings.md`) collides only if two processes share a clock tick and pid namespace (unlikely). **`ASSAY_RUN_ID` for CI replay** remains open — without it, log correlation across retries is harder, not unsafe.

Shared **read-only** inputs: repo source, `.cache/nuget/packages`, RhinoWIP bundle — safe. Shared **writable** hotspots: closure build tree (lease-governed), global locks above, and any tool that writes tracked files (`Mode.writes`) without row-level isolation.

---
## [3][READ_ONLY_FAN_OUT]
>**Dictum:** *Fan-out multiplies throughput; it does not remove global bottlenecks.*

<br>

Quality runs one dotnet invocation chain per rail scope; assay `fan_out` (`engine.md` §3, `rails.md` §1) runs independent read-only `Check`s under one `anyio` task group within **one** `run_id`. That is the main concurrency **upgrade** for polyglot `static`/`docs`/`api`/`test list`.

| [BENEFIT] | [RISK AT 100+ AGENTS] |
| :-------- | :-------------------- |
| C# + Py + TS lint/typecheck in parallel per agent | Unbounded task group size = **N tools × 1 agent** process storms |
| No lease on read-only rows | Correct — avoids lock convoys |
| Partial failure fold (a) | Many failing processes still consume CPU/disk |

**Missing vs production needs:** a **concurrency cap** (semaphore or `ASSAY_MAX_CHECKS`) on `fan_out`; quality never needed it because it rarely fanned out. **Nested `anyio.run`:** if `fan_out` calls the public `run_check` wrapper per slot, each check spins a new event loop — serializes or thrashes; inner path must call `_guarded` async directly inside the group's loop (`engine.md` §2 vs §3 tension).

`Engine.run_all` lease dispatch is specified (`rails.md` §4) but **`Mode.parallel` / `lease` on row data** (`research-holistic-shapes.md`) is not pinned — until then, lease rules live in prose, not catalog data.

---
## [4][DOTNET_ARTIFACT_PATHS]
>**Dictum:** *Dotnet isolation is env overlay + artifacts-path + disabled node reuse.*

<br>

Inherited splice (`engine.md` §1): for `_SCOPED_VERBS`, argv gains `scope.dotnet_flags` → `--artifacts-path <scope>`, `--disable-build-servers`, plus env `DOTNET_CLI_HOME=<scope>/dotnet-cli`, `MSBUILDDISABLENODEREUSE=1` (quality `settings.dotnet_env`).

| [KNOB] | [QUALITY] | [ASSAY] | [NOTE] |
| :----- | :-------- | :------ | :----- |
| `--artifacts-path` | Stable per **closure** for build; per **run** for rail scope | Claim/run scope documented; closure scope **not** | Warm build cache + analyzer obj isolation depend on closure path |
| `-maxcpucount` | Build args in README | `dotnet_max_cpu` / `mutation_max_cpu` in settings | Caps **per process**, not fleet-wide |
| Format restore | Implicit restore on format | Same row model | Parallel `fix` on same csproj → file races (git/worktree concern, not assay stdout) |
| Build servers | Disabled in scoped env | Same | Prevents MSBuild node reuse cross-agent |

**Hundreds of dotnet-heavy agents:** distinct closures scale (~O(projects)); identical closure does not queue — it **busy**s. Fleet-wide dotnet CPU becomes `agents × dotnet_max_cpu` with no global coordinator (quality §7: "artifact isolation replaces static locks" for *build*, not for *fleet CPU*).

---
## [5][FAIL_FAST_BUSY]
>**Dictum:** *Busy is a first-class outcome, not a wait.*

<br>

| [SIGNAL] | [EXIT] | [CONSUMER] |
| :------- | -----: | :--------- |
| `RailStatus.BUSY` | 5 | Orchestrator retries another rail or backs off |
| `TIMEOUT` | 5 | Same bucket as busy in quality README |
| Held lease detail | owner block in `Fault` | Must surface in Envelope `error`, not stderr |

Assay matches quality: no blocking flock, no "wait for bridge". **Stdout corruption** is architecturally prevented by invariant 1 (`ARCHITECTURE.md` §11): one `msgspec` Envelope line on stdout; engine streams to stderr and artifact logs. Residual risks: (1) a cataloged tool printing to stdout; (2) `@logged` mis-bound to stdout; (3) watch mode multi-Envelope stream (`registry.md` open #2). Quality already enforces the contract; assay registry design aligns if `_emit` stays the sole stdout writer.

---
## [6][QUALITY_COMPARISON]
>**Dictum:** *Assay should copy what quality proved, then fix what quality never solved.*

<br>

| [TOPIC] | [QUALITY DID RIGHT] | [QUALITY GAP / WRONG FOR AGENTS] | [ASSAY DESIGN RESPONSE] |
| :------ | :------------------ | :------------------------------- | :---------------------- |
| Leases | `build-<closure>`, `mutation`, `bridge`, stage — NB flock | Documented only under `.artifacts/quality`; **no polyglot** | Unified `artifact(LOCKS, …)`; same semantics |
| Artifacts | `run_id` rail dirs + isolated CLI home | Shared quality tree name; ARCHITECTURE §1 lists agent collision | `.artifacts/assay/<claim>/<run_id>/` |
| Build proof | Stable closure artifact dir + lease | Cursor rule drift ("no flock") vs README §3 — **README + code win** | Must codify `ArtifactScope.build(closure)` in settings/engine |
| Concurrency | Parallel static/test with distinct `run_id` | No cross-language fan-out; serial rail handlers | `fan_out` — needs cap + async discipline |
| Output | Single Envelope; stderr for dotnet | `rail`/`data`/`evidence` ladder | `claim`/`report` — cleaner if wire frozen |
| API cache | Cross-run surface cache | Safe read-only; restore probes per run | Specify cache path under assay or shared read-only |
| Global exclusives | Correctly fail-fast | **Throughput ceiling** for many agents on bridge/mutation | Same ceiling — design honest, not scalable |

---
## [7][SCALE_VERDICT_AND_GAPS]
>**Dictum:** *Hundreds of agents need hundreds of disjoint hot paths, not hundreds of waiters.*

<br>

| [SCENARIO] | [ADEQUATE?] | [WHY] |
| :--------- | :---------- | :---- |
| 100× read-only `static report` / `docs` / `api query` | **Mostly yes** | Disjoint `run_id` + no lease; bound fan-out and disk |
| 100× `static build` same closure | **No** | 99× `busy`; orchestrator must shard by closure or queue |
| 100× `test run` | **Mostly yes** | Per-run test artifacts; `--no-build` after warm closure |
| 100× `bridge verify` / mutation | **No** | Single global lock — by design |
| stdout integrity | **Yes** (if invariants hold) | Same contract as quality |

**Blockers before claiming production concurrency:** (1) pin closure-scoped `ArtifactScope` like quality; (2) implement engine leases from `process.py`, not stubs; (3) cap `fan_out`; (4) fix stream log key + avoid nested `anyio.run`; (5) throughput doc: busy rate vs closure cardinality; (6) complete `REGISTRY` so agents do not fall back to raw `dotnet`/`pnpm` (stdout corruption source).

---
## [8][FURTHER_CONSIDERATION]
- **Worktree-per-agent** beats more locks for `Mode.WRITE` rows (`dotnet format`, biome fix).
- **Shared read-only API surface cache** under assay vs quality paths — require content-addressed keys to avoid stale ilspy surfaces after package upgrade.
- **Fleet coordinator** (optional): global semaphore on dotnet CPU separate from per-closure locks — quality deliberately omitted; assay may need it only in CI agent pools, not local.

# [TS_DATA_API_ELECTRIC_SQL_PGLITE_TOOLS]

`@electric-sql/pglite-tools` runs a WebAssembly `pg_dump` against one live PGLite connection and returns executable SQL as a `File`. Data uses that logical artifact only to hydrate an unpublished replacement generation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@electric-sql/pglite-tools`
- package: `@electric-sql/pglite-tools` (Apache-2.0)
- module: ESM + CJS dual; root and `./pg_dump` both export `pgDump`
- runtime: browser and JavaScript runtimes supported by its exact `@electric-sql/pglite` peer
- depends: `@electric-sql/pglite` exact peer; `pgDump` operates on its single live connection
- rail: `lane/sqlite` logical generation-export row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: no named package type is exported; `pgDump` exposes one structural argument and one platform return carrier

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :----------------- | :------------ | :------------------------------------------------ |
|  [01]   | `pgDump` parameter | object        | `pg`, optional argument vector, and output name   |
|  [02]   | `File`             | carrier       | executable SQL text with caller-selected filename |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one logical export and its fresh-generation restore seam

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `pgDump({ pg, args?, fileName? }) -> Promise<File>`       | function | logical schema/data export from one connection |
|  [02]   | `File.text() -> Promise<string>`                          | platform | executable SQL restore input                   |
|  [03]   | `candidate.exec(await file.text()) -> Promise<Results[]>` | compose  | hydrate an unpublished fresh PGLite generation |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `pgDump` runs bundled WASM against one supplied PGLite connection and returns pruned SQL in a `File`.
- Package policy appends `--inserts`, one worker, fixed virtual output, and PostgreSQL user arguments.
- Export runs `DEALLOCATE ALL` and restores the observed search path.
- Prepared statements cannot survive an export.
- `fileName` names the returned `File`; PGLite retains its fixed virtual output.
- Caller arguments select content, never insert format, worker count, output path, or user.

[STACKING]:
- `@electric-sql/pglite`: `pgDump` consumes an admitted `PGlite`; `File.text()` feeds `PGlite.exec` only on a fresh candidate.
- `PGlite.dumpDataDir` retains the distinct physical-export seam.
- `effect` (`.api/effect.md`): `PgliteRuntime.snapshot("logical")` wraps `pgDump` under the same one-permit semaphore as statements.
- within-lib: `pgDump` snapshots and restores search path around WASM execution and `DEALLOCATE ALL`.
- within-lib: output pruning removes `\restrict` and `\unrestrict`, then constructs the returned `File`.

[LOCAL_ADMISSION]:
- Fence the source connection against writes and all prepared-statement users for the complete export.
- Pass lane-owned `--data-only` and `--column-inserts` through `args`; returned SQL remains directly executable by PGLite.
- Restore only into an unpublished generation and reapply the admitted search path.
- Admit generation and semantics before pointer publication.
- Hash the returned bytes with source generation and contract identity; export, restore, or admission failure leaves the active generation unchanged.

[RAIL_LAW]:
- Package: `@electric-sql/pglite-tools`
- Owns: logical `pg_dump` execution against one PGLite connection and executable-SQL `File` output
- Accept: exclusive fenced export, lane-owned dump arguments, content-addressed artifact, fresh-generation restore, post-restore admission
- Reject: concurrent prepared statements, active-generation restore, physical-backup claims, successful SQL execution treated as generation proof

# [TS_DATA_API_CHOKIDAR]

`chokidar` catalog-bound is the local filesystem watcher behind the `object/file` law that a watched directory is a `Stream` of admission events: `watch(paths, options)` mints an `FSWatcher` (a typed `EventEmitter<FSWatcherEventMap>`) whose event rows — `add`/`addDir`/`change`/`unlink`/`unlinkDir` with `(path, stats?)`, the aggregate `all` with `(event, path, stats?)`, `ready`, `raw`, `error` — fold into one admission stream. Glob support is gone by design: `paths` are literal (`string | string[]`), and filtering is the `ignored` matcher algebra — exact-path string, `RegExp`, predicate function, or `{ path, recursive? }` object — so selection is predicate rows, never glob strings. The intake-correctness levers are options, not code: `awaitWriteFinish` (`{ stabilityThreshold, pollInterval }`) holds `add`/`change` until a file's size settles so a half-written file is never digested, `atomic` absorbs editor rename-swap artifacts, `depth` bounds recursion, and `usePolling`/`interval`/`binaryInterval` are the network-FS degrade row. `close()` returns a Promise, strips all listeners synchronously, and MUST be awaited — it is the release arm of the scoped bracket. This is the local half of the watch strategy row; the remote halves (SSH exec-push, SFTP/DAV/FTP poll) live on the remote-origin rows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `chokidar`
- package: `chokidar` (MIT, paulmillr/chokidar)
- module format: ESM only (`type: module`); single runtime dependency (`readdirp`); node only
- exports: `watch` (factory), `FSWatcher` (class) — named exports; the default export bags both
- rail: local watch row (`object/file`); the local half of the watch strategy row

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the watcher, its event map, and the matcher algebra
- rail: boundaries

| [INDEX] | [SYMBOL]                                                                                                   | [TYPE_FAMILY]   | [CONSUMER]                                                            |
| :-----: | :--------------------------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `FSWatcher extends EventEmitter<FSWatcherEventMap>` (`add`, `unwatch`, `close`, `getWatched`)              | watcher         | the scoped resource; `add`/`unwatch` are chainable roster mutations   |
|  [02]   | events `add` / `addDir` / `change` / `unlink` / `unlinkDir` — `(path: string, stats?: Stats)`              | event rows      | the admission stream vocabulary; `stats` present under `alwaysStat`   |
|  [03]   | event `all` — `(event, path, stats?)`                                                                      | aggregate       | the one-listener fold the lift consumes instead of five registrations |
|  [04]   | events `ready` / `error` / `raw`                                                                           | lifecycle       | initial-census barrier, fault row, opaque backend tap                 |
|  [05]   | `ignored: Matcher \| Matcher[]` — `string` (exact path) \| `RegExp` \| predicate \| `{ path, recursive? }` | matcher algebra | selection as predicate rows; string is equality, NOT a glob           |
|  [06]   | `getWatched(): Record<string, string[]>`                                                                   | census          | the dir → entries snapshot for reconciliation                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: watch lifecycle and the option levers
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                                                     | [ENTRY_FAMILY] | [CONSUMER]                                                                                                                 |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `watch(paths: string \| string[], options?): FSWatcher`                                                                       | acquire        | literal paths only; the acquireRelease acquire arm                                                                         |
|  [02]   | `watcher.close(): Promise<void>`                                                                                              | release        | awaited release arm; listeners strip synchronously at call                                                                 |
|  [03]   | `ignoreInitial` (default `false`) + `ready`                                                                                   | census policy  | `false` replays the existing tree as `add` rows before `ready`; `ready` fires once either way — the initial-census barrier |
|  [04]   | `awaitWriteFinish: true \| { stabilityThreshold, pollInterval }` (defaults `2000`/`100`)                                      | settle guard   | hold `add`/`change` until size stabilizes — the half-written-file guard on intake                                          |
|  [05]   | `atomic: boolean \| number` (default on when not polling; delay `100`)                                                        | rename guard   | absorbs editor write-via-rename artifacts into one `change`                                                                |
|  [06]   | `usePolling` / `interval` (default `100`) / `binaryInterval` (default `300`)                                                  | degrade row    | the network-FS/container row where native events lie                                                                       |
|  [07]   | `depth` / `cwd` / `followSymlinks` (default `true`) / `alwaysStat` / `ignorePermissionErrors` / `persistent` (default `true`) | scope levers   | recursion bound, relative emission, link policy, stat guarantee                                                            |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `effect` (`libs/typescript/.api/effect.md`): the watcher acquires under `Effect.acquireRelease(watch(...), (w) => Effect.promise(() => w.close()))`; the `all` listener lifts through `Stream.asyncPush` into one typed admission stream; `error` joins the failure channel; `ready` resolves the initial-census barrier the intake fold gates on.
- `object/file` intake: an `add`/`change` row (post `awaitWriteFinish` settle) feeds the content-addressed intake fold — stream, digest, conditional put; the watcher supplies admission events, never bytes.
- `ssh2` / `webdav` / `basic-ftp` (`.api/ssh2.md`, `.api/webdav.md`, `.api/basic-ftp.md`): the remote halves of the watch strategy row — exec-push where the host carries a notify tool, `Schedule`-driven poll diffs elsewhere; the strategy row dispatches on origin kind, and this package is only ever the `file:` arm.
- `@effect/platform` `FileSystem` (`libs/typescript/.api/effect-platform.md`): stat/read follow-ups on an admission event ride the platform capability, not `node:fs` beside the watcher.

[LOCAL_ADMISSION]:
- Always await `close()` inside the release arm; an unawaited close leaks native handles across the scope boundary.
- Gate intake on `awaitWriteFinish` for any directory receiving whole-file writes; digesting an unsettled file is the named defect.
- Express selection as `ignored` predicate rows; a glob string is dead syntax — it matches nothing but its literal self.
- Choose `usePolling` as a per-origin config row for network mounts and containers; never as a global default.

[RAIL_LAW]:
- Package: `chokidar`
- Owns: local filesystem watching — the `FSWatcher` lifecycle, the typed event map, the matcher algebra, write-settle/atomic guards, polling degrade, initial-census replay
- Accept: scoped `watch`/awaited-`close` brackets, one `all`-listener lift into a typed admission stream, `ignoreInitial: false` + `ready` as the census barrier, predicate `ignored` rows, per-origin polling config
- Reject: glob strings anywhere in paths or matchers, unawaited `close()`, raw multi-listener consumption where the `all` fold suffices, intake without a settle guard, a remote origin forced through this local row

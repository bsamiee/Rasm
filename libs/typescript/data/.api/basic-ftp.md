# [TS_DATA_API_BASIC_FTP]

`basic-ftp` is the zero-dependency TypeScript FTP client covering the retired-protocol origin class many AEC and enterprise hosts still expose: one `Client` whose `access(options)` composes connect, TLS negotiation, login, and sane defaults into a single dial, whose transfer members take a Node stream OR a local path for either side (`uploadFrom`/`downloadTo`/`appendFrom`), and whose resume story is byte arithmetic — `downloadTo(destination, fromRemotePath, startAt)` restarts a download at an offset, `appendFrom` continues an interrupted upload, and `UploadOptions.localStart`/`localEndInclusive` slice the local source to the missing range. TLS mode is a config value, not a fork: `secure: true` upgrades explicit FTPS (`AUTH TLS` after plaintext connect) and `secure: "implicit"` wraps the socket before the first command. Protocol errors and transport errors part ways deliberately: an `FTPError` (server refusal with `code`) leaves the connection usable, while a timeout or connection loss closes the context and the next `access` re-dials. It is a remote-origin backend row of the `object/file` plane beside the SFTP and DAV rows — the control connection carries one transfer at a time, so concurrency is a pool of clients, never interleaving on one.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `basic-ftp`
- package: `basic-ftp` (MIT, patrickjuchli/basic-ftp)
- module format: CJS + ESM dual; zero runtime dependencies; node only (`net`/`tls` sockets)
- rail: remote-origin row (`object/file`); the FTP/FTPS protocol lane
- bounce guard: `ClientOptions.allowSeparateTransferHost` defaults `false` — data connections to a host other than the control host are refused unless ruled

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client, dial options, and census facts
- rail: boundaries
- `new Client(timeout?, { allowSeparateTransferHost?, maxListingBytes? }?)` mints one control connection; the field-bearing shapes carry their rosters in the signature fence below.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CONSUMER]                                              |
| :-----: | :------------------------------------------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `Client`                                                | client        | one control connection per instance; scoped service     |
|  [02]   | `AccessOptions`                                         | dial row      | the whole dial; `secureOptions` = `tls.connect` options |
|  [03]   | `FileInfo` / `FileType`                                 | census fact   | the listing unit; per-entry type/size/mtime/permissions |
|  [04]   | `FTPResponse` (`code`, `message`) / `FTPError` (`code`) | receipt/fault | server disposition; `FTPError` keeps connection usable  |
|  [05]   | `ProgressInfo`                                          | progress fact | the `trackProgress` stream unit                         |
|  [06]   | `client.ftp` (`FTPContext`)                             | context       | log capture (`log` overridable) and wire tuning         |
|  [07]   | `client.parseList` (`RawListParser` override)           | parser slot   | exotic-server listing formats as policy, never a fork   |

```typescript signature
type FileType = Unknown | File | Directory | SymbolicLink;
interface AccessOptions { host; port; user; password; secure: boolean | "implicit"; secureOptions; }
interface FileInfo { name; type: FileType; size; rawModifiedAt; modifiedAt?; permissions?; link?; uniqueID?; isDirectory(); isFile(); isSymbolicLink(); }
interface ProgressInfo { name; type: "upload" | "download" | "list"; bytes; bytesOverall; }
interface FTPContext { verbose; log(message); encoding; tlsOptions; ipFamily; }
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dial, transfer, resume, and census
- rail: boundaries
- Every entry is a `client` member; a `source`/`destination` takes a Node stream or a local path, so the rows drop the union and carry only the resume and slice options that differ.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CONSUMER]                                   |
| :-----: | :---------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `access(options): Promise<FTPResponse>`                                 | dial           | connect + TLS + login + defaults in one dial |
|  [02]   | `uploadFrom(source, toRemotePath, options?)`                            | upload         | stream or path; the slice-resume arm         |
|  [03]   | `appendFrom(source, toRemotePath, options?)`                            | append         | continue an interrupted upload at the tail   |
|  [04]   | `downloadTo(destination, fromRemotePath, startAt?)`                     | download       | `startAt` positional byte-offset resume arm  |
|  [05]   | `list(path?)` / `size(path)` / `lastMod(path)`                          | census         | `FileInfo[]` walk; size+mtime poll-diff      |
|  [06]   | `cd` / `cdup` / `pwd` / `ensureDir` / `clearWorkingDir`                 | navigation     | working-dir discipline for tree transfers    |
|  [07]   | `uploadFromDir` / `downloadToDir` `(localDirPath, remoteDirPath?)`      | tree transfer  | recursive directory mirror in one member     |
|  [08]   | `rename(srcPath, destPath)` / `remove` / `removeDir` / `removeEmptyDir` | namespace      | each remove takes a path; staged rename      |
|  [09]   | `trackProgress(handler?)`                                               | progress       | one handler for all transfers; detach none   |
|  [10]   | `send(command)` / `features()`                                          | probe          | capability probing, escape-hatch verbs       |
|  [11]   | `close()` / `closed`                                                    | teardown       | release arm of the scoped bracket            |

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `effect` (`libs/typescript/.api/effect.md`): every promise member converts through `Effect.tryPromise` at the seam; the client acquires under `Effect.acquireRelease` with `close()` as release; `trackProgress` lifts to a fact `Stream` via `Stream.asyncPush`; an `FTPError` folds to a typed refusal that keeps the connection row alive, while a closed-context fault triggers the re-`access` ride under a `Schedule` policy.
- `@effect/platform-node` (`.api/effect-platform-node.md`): stream-side transfer crosses through `NodeStream.toReadable`/`NodeSink.fromWritable` mints so an Effect `Stream` body uploads and a download drains into an Effect `Sink` — the path-string arms are for whole-file moves the platform `FileSystem` already staged.
- `ssh2` / `webdav` (`.api/ssh2.md`, `.api/webdav.md`): sibling remote-origin rows on the one origin-addressed `object/file` surface; capability flags dispatch — FTP has no server-side copy and no locks, so copy degrades to download + upload and coordination stays on the origin that owns it.
- `chokidar` (`.api/chokidar.md`): the local half of the watch strategy row; the FTP half is the poll row — `Schedule`-driven `list`/`size`/`lastMod` snapshots diffed on size + mtime, since no FTP change-notification lane exists.

[LOCAL_ADMISSION]:
- Dial through `access` only; the split `connect`/`useTLS`/`login` members are for capability probes, never the standing acquisition.
- One transfer per client by protocol law — concurrency is a bounded pool of scoped clients keyed by origin.
- Resume by arithmetic: `size` the remote, then `downloadTo(..., startAt)` or `appendFrom`/`localStart` — a full re-transfer where an offset resume was possible is the named defect.
- Stage uploads to a temp name and `rename` into place; a partial upload visible under its final name is rejected.

[RAIL_LAW]:
- Package: `basic-ftp`
- Owns: the FTP/FTPS protocol lane — the composed dial with explicit/implicit TLS rows, streamed and path transfer, byte-offset resume, directory mirroring, listing census with parser override, progress tracking, the usable-after-refusal error split
- Accept: scoped pooled clients dialed through `access`, `NodeStream`/`NodeSink` body seams, offset-resume arithmetic, rename-into-place staging, poll-diff watching
- Reject: interleaved transfers on one client, credential literals, full re-transfers where resume arithmetic applies, `secure: false` against origins that offer TLS, a second FTP wrapper over this surface

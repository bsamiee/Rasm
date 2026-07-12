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

| [INDEX] | [SYMBOL]                                                                                                                                                     | [TYPE_FAMILY] | [CONSUMER]                                                                    |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------ | :---------------------------------------------------------------------------- |
|  [01]   | `Client` (`new Client(timeout?, { allowSeparateTransferHost?, maxListingBytes? }?)`)                                                                         | client        | one control connection per instance, held as a scoped service                 |
|  [02]   | `AccessOptions` (`host`, `port`, `user`, `password`, `secure: boolean \| "implicit"`, `secureOptions`)                                                       | dial row      | the whole dial as config data; `secureOptions` = `tls.connect` options        |
|  [03]   | `FileInfo` (`name`, `type: FileType`, `size`, `rawModifiedAt`, `modifiedAt?`, `permissions?`, `link?`, `uniqueID?`; `isDirectory`/`isFile`/`isSymbolicLink`) | census fact   | the listing unit; `FileType` = `Unknown \| File \| Directory \| SymbolicLink` |
|  [04]   | `FTPResponse` (`code`, `message`) / `FTPError` (`code`)                                                                                                      | receipt/fault | server disposition; `FTPError` leaves the connection usable                   |
|  [05]   | `ProgressInfo` (`name`, `type: "upload" \| "download" \| "list"`, `bytes`, `bytesOverall`)                                                                   | progress fact | the `trackProgress` stream unit                                               |
|  [06]   | `client.ftp` (`FTPContext`: `verbose`, `log(message)`, `encoding`, `tlsOptions`, `ipFamily`)                                                                 | context       | log capture (`log` is an overridable method) and wire tuning                  |
|  [07]   | `client.parseList` (`RawListParser` override)                                                                                                                | parser slot   | exotic-server listing formats as a policy value, never a fork                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dial, transfer, resume, and census
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [CONSUMER]                                                                              |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `client.access(options): Promise<FTPResponse>`                                                              | dial           | connect + TLS + login + defaults in one member; also the re-dial after a closed context |
|  [02]   | `client.uploadFrom(source: Readable \| string, toRemotePath, { localStart?, localEndInclusive? }?)`         | upload         | stream or local path; the slice options are the upload-resume arm                       |
|  [03]   | `client.appendFrom(source, toRemotePath, options?)`                                                         | append         | continue an interrupted upload at the remote tail                                       |
|  [04]   | `client.downloadTo(destination: Writable \| string, fromRemotePath, startAt?)`                              | download       | `startAt` is the positional byte-offset resume arm                                      |
|  [05]   | `client.list(path?)` / `client.size(path)` / `client.lastMod(path)`                                         | census         | `FileInfo[]` walk source; size + mtime are the poll-diff inputs                         |
|  [06]   | `client.cd` / `cdup` / `pwd` / `ensureDir` / `clearWorkingDir`                                              | navigation     | working-directory discipline for tree transfers                                         |
|  [07]   | `client.uploadFromDir(localDirPath, remoteDirPath?)` / `client.downloadToDir(localDirPath, remoteDirPath?)` | tree transfer  | recursive directory mirror in one member                                                |
|  [08]   | `client.rename(srcPath, destPath)` / `remove(path)` / `removeDir(remoteDirPath)` / `removeEmptyDir(path)`   | namespace      | rename-into-place staging, tree maintenance                                             |
|  [09]   | `client.trackProgress(handler?)`                                                                            | progress       | one handler for all transfers; detach by calling with no handler                        |
|  [10]   | `client.send(command)` / `client.features()`                                                                | probe          | capability probing and escape-hatch verbs                                               |
|  [11]   | `client.close()` / `client.closed`                                                                          | teardown       | release arm of the scoped bracket                                                       |

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

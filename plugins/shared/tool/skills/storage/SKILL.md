---
name: storage
description: "Use when measuring or freeing space on the Mac, folders, cloud drives, VPS machines, or Docker engines, covering APFS clones, duplicates, litter, and cleans"
argument-hint: "[analyze|clean] [targets] [threshold]"
---

# [TOOL_STORAGE]

Arguments `$ARGUMENTS` name a mode (`analyze` by default, or `clean`), targets, and a threshold in any order, each told by its form, and the request supplies values they lack:
- Targets: paths by [01] and [02], rclone `<remote>:<path>`, `mega:<path>`, or `drive` by `references/cloud.md`, machines by `references/machines.md`, else the Mac
- Threshold: a size (`2G`) cutting folder rows of cloud and machine listings, 1 GiB when absent

Targets naming a cloud drive send your next call to Read `references/cloud.md`, and targets naming a VPS machine or its rclone sftp remote, a Docker engine, a Colima VM, CI runners, or the Nx remote cache to Read `references/machines.md` before any `ssh` or `docker` call, its steps replacing [STEPS]; Read each again after a compaction.

[REFERENCES]:
- [01]-[CLOUD](references/cloud.md): Cloud drives and rclone remotes with their versions, trash, and duplicates
- [02]-[MACHINES](references/machines.md): VPS machines with their runners and cache store, and Docker engines with the Mac's Colima VMs

[SCRIPTS]:
- [01]-[USAGE](scripts/usage.py): Walks a local target once, reporting bytes, clones, duplicates, and litter by [01] or cloning duplicates by [03]

Volume reads run as Claude Code loads this skill, and a Read of this file sends each line as its own parallel call first. Purgeable bytes are important-usage capacity minus available capacity, snapshots keep the blocks of deleted files until thinned and the processes `lsof +L1` names until each quits, and `df` available blocks are the baseline of every clean, as `du` counts each clone and misses what a clone keeps:

```!
diskutil apfs list -plist | plutil -convert json -o - - | jq -c '.Containers[] | {container: .ContainerReference, free: .CapacityFree, volumes: [.Volumes[] | {name: .Name, roles: .Roles, used: .CapacityInUse}]}'
diskutil apfs listSnapshots -plist /System/Volumes/Data | plutil -convert json -o - - | jq -c '.Snapshots[] | {SnapshotName, Purgeable}'
osascript -l JavaScript -e 'ObjC.import("Foundation"); JSON.stringify(ObjC.deepUnwrap($.NSURL.fileURLWithPath("/System/Volumes/Data").resourceValuesForKeysError($(["NSURLVolumeAvailableCapacityKey", "NSURLVolumeAvailableCapacityForImportantUsageKey"]), null)))'
df --libxo json -k /System/Volumes/Data | jq '.["storage-system-information"].filesystem[0]["available-blocks"]'
sudo -n lsof -nP +c 0 +L1 | awk 'NR > 1 && $5 == "REG" && !seen[$6 ":" $9]++ {held[$1 " " $2] += $7} END {for (p in held) print held[p], p}' | sort -rn | head -20
```

[STEPS]:
1. Measure: the walk and every [03] dry run whose store a read names go out as parallel calls in one message, each walk alone in the background
2. Clean: mode `clean` alone, each item found by [03], its row holding the `df` delta against the baseline or why it stayed
3. Report: the form below alone, no prose lead-in or process note, each row naming its path, bytes, and the call that frees them

```text
storage <mode> <targets>
  baseline: <df available bytes>
  totals: <allocated>, <physical>, <purgeable> purgeable, <snapshot bytes> snapshots
  gap: <bytes> <holder pid and command, or cause>
safe: <bytes>
  <path> bytes=<n> call=<clean> freed=<df delta in clean mode>
ruling: <bytes>
  <path> bytes=<n> call=<clean> decides=<what the owner rules>
held:
  <path> bytes=<n> holder=<pid command>
findings:
  <path> bytes=<n> cause=<file:line>
```

## [01]-[WALK]

`scripts/usage.py` walks a local target once and writes nothing, its environment sitting in the uv cache, and its report, read whole in one Read, answers every local question, `/System/Volumes/Data` standing for the whole Mac:

```bash
uv run --locked --script scripts/usage.py <target> [floor] > <scratchpad>/usage-<session>-<target>.txt
```

- Floor (`4K`, `1`) sets the smallest file the `files` and `duplicates` rows and `clone` take, 1 MiB when absent
- `allocated` counts every clone in full and each hard link once at its first path, the figure `du` gives
- `physical` counts each clone stream once, and a clone written after cloning takes a stream of its own, counting the blocks it still shares again
- `physical` equal to `allocated` means each shared stream holds one walked file, `private` below both counting blocks shared with clones outside the target
- `volume` on a volume root states `used` and the gap that snapshots, refused folders, metadata, and deleted files still open hold
- `private` counts bytes deleting a file or folder alone frees, a folder holding every clone of a stream freeing that stream, snapshots aside
- `logical` above `allocated` marks a sparse, compressed, or dataless file, `flags` names which, and the walk never downloads a dataless one
- Folders rank by bytes past their largest subfolder and files by `allocated`, 40 rows each, and a rerun on a listed folder reaches below the cut in place of `du`
- Duplicates compare files by size, then hash, across clone streams, copies sharing a `stream` number being clones
- Refused `EACCES` paths take the `sudo -H` rerun the line names, `EPERM` ones sit under SIP, or under TCC that Full Disk Access lifts

## [02]-[LITTER]

Walk `litter` rows rank folders by empty files, empty folders, and broken links, which free no space, beside these reads:
- Untracked files -> `git -C <repo> ls-files --others --exclude-standard --directory`, ignored outputs belong to the project's clean
- Temporary files -> `czkawka_cli temp -d <folder>... -C <out.json>` over `~/Downloads`, `~/Desktop`, and `~/Documents`
- Stale files -> walk rows of installers, archives, and disk images with old modification dates
- Stores -> `~/Library/Caches`, `~/Library/Logs`, `~/Library/Developer`, `~/.Trash`, `~/.cache`, `$TMPDIR`, `$(getconf DARWIN_USER_CACHE_DIR)`, tool caches

## [03]-[CLEAN]

Mode `clean` frees what [01] and [02] found, each item in this order:
1. Owner: a project's clean target, else the tool's prune, else deletion, previewed by its dry run or the item's walk rows
2. Holders: `lsof -nP -t -- <lock file>`, `pgrep -fl <tool>`, and `lsof -nP -- <file>...` over the log and temporary files a deletion names, a held item staying with holder pid and command, never forced

Cases:
- Rasm outputs and caches -> `nx clean`, and `nx clean -c cache` deleting the Colima VM with every cache, from the Rasm root after peer sessions and `docker --context colima-rasm ps -q` rows there finish
- `uv` -> `UV_CACHE_DIR=<root> uv cache prune` per root (`~/.cache/uv`, and the `uv cache dir` a project's mise env sets) when `<root>/.lock` has no holder, as `uvx` servers hold it for life
- Tool caches -> `pnpm store prune`, `npm cache verify`, `uvx --from huggingface_hub hf cache prune --dry-run`, `mise prune -n`, `cargo clean -n`, `cargo -Zgc clean gc -n --max-download-age=<age>`
- Data kept off the Mac -> `rsync -aHAXNS -n -i <src>/ <dst>/` to an APFS disk or `-aHXS` to a VPS, the real run, `rsync -c -n -i` printing nothing, deletion
- Rust toolchains -> `rustup toolchain list -v`, a version `mise prune -n` keeps unnamed being pinned by a config `rg -l <version> $(readlink ~/.local/state/mise/tracked-configs/*)` names
- Homebrew and Nix -> `brew cleanup -n -s --prune=all`, `nh clean all --dry --keep <n>`, and `nix-store --gc --print-dead | xargs du -sk | awk '{s += $1} END {print s}'` for dead KiB
- Git LFS -> `git -C <repo> lfs dedup --test` then `lfs dedup` clones working files from LFS storage, `lfs prune --dry-run --verify-remote` drops pushed objects, per submodule too
- Tool state under `.git` (`.git/<tool>`) -> deletion where `command -v <tool>` fails and no `.git/hooks` file names it
- Xcode -> DerivedData whose `info.plist` `WorkspacePath` is gone, and `ModuleCache.noindex` with `SDKExplicitPrecompiledModules` once Xcode quits, `CompilationCache.noindex` staying under its size limit
- Simulators -> `xcrun simctl delete unavailable`, `xcrun simctl runtime delete --dry-run` with `--unusable`, then `--outdated` for runtimes a newer one replaces
- Snapshots -> `tmutil thinlocalsnapshots /System/Volumes/Data <bytes> 4` for Time Machine, OS update snapshots stay
- Copies a tool writes again on its next run (a package clone per project) -> a `findings` row with the line causing each copy, never a clean
- Duplicates both paths need (files a `.indd`, `.ai`, or `.blend` links by path, project references) -> `usage.py clone <target> [floor]`, deletion taking a ruling
- Other duplicates, empty files and folders, broken links -> deletion outside `.git`, app libraries and containers, and tool state

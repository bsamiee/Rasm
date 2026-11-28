---
name: storage
description: "Use when measuring or freeing space on the Mac, folders, cloud drives, VPS machines, or Docker engines, covering APFS clones, duplicates, litter, and cleans"
argument-hint: "[analyze|clean] [targets] [threshold]"
---

# [TOOL_STORAGE]

Arguments `$ARGUMENTS` name a mode (`analyze` by default, or `clean`), targets, and a threshold in any order, each told by its form, and the request supplies values they lack:
- Targets: paths by [01] and [03], rclone `<remote>:<path>`, `mega:<path>`, or `drive` by [02], machines by `references/machines.md`, else the Mac
- Threshold: a size (`2G`) cutting folder rows of cloud and machine listings, 1 GiB when absent

Targets naming a VPS machine or its rclone sftp remote, a Docker engine, a Colima VM, CI runners, or the Nx remote cache send your next call to Read `references/machines.md`, before any `ssh` or `docker` call, and its steps replace [STEPS] for them; Read it again after a compaction.

[REFERENCES]:
- [01]-[MACHINES](references/machines.md): VPS machines with their runners and cache store, and Docker engines with the Mac's Colima VMs

[SCRIPTS]:
- [01]-[USAGE](scripts/usage.py): Walks a local target once, reporting bytes, clones, duplicates, and litter by [01] or cloning duplicates by [04]

Volume reads run as Claude Code loads this skill, and first in other harnesses. Purgeable bytes are important-usage capacity minus available capacity, snapshots keep the blocks of deleted files until thinned and the processes `lsof +L1` names until each quits, and `df` available blocks are the baseline of every clean, as `du` counts each clone and misses what a clone keeps:

```!
diskutil apfs list -plist | plutil -convert json -o - - | jq -c '.Containers[] | {container: .ContainerReference, free: .CapacityFree, volumes: [.Volumes[] | {name: .Name, roles: .Roles, used: .CapacityInUse}]}'
diskutil apfs listSnapshots -plist /System/Volumes/Data | plutil -convert json -o - - | jq -c '.Snapshots[] | {SnapshotName, Purgeable}'
osascript -l JavaScript -e 'ObjC.import("Foundation"); JSON.stringify(ObjC.deepUnwrap($.NSURL.fileURLWithPath("/System/Volumes/Data").resourceValuesForKeysError($(["NSURLVolumeAvailableCapacityKey", "NSURLVolumeAvailableCapacityForImportantUsageKey"]), null)))'
df --libxo json -k /System/Volumes/Data | jq '.["storage-system-information"].filesystem[0]["available-blocks"]'
sudo -n lsof -nP +c 0 +L1 | awk 'NR > 1 && $5 == "REG" && !seen[$6 ":" $9]++ {held[$1 " " $2] += $7} END {for (p in held) print held[p], p}' | sort -rn | head -20
```

[STEPS]:
1. Measure: independent reads go out as parallel calls in one message, each walk and each clean alone in the background, never chained
2. Clean: mode `clean` alone, each item found by [04], its row holding the `df` delta against the baseline or why it stayed
3. Report: subject `storage <mode> <targets>`, `baseline`, `totals`, and `gap` facts, then folder, file, duplicate, litter, and clean rows

## [01]-[WALK]

`scripts/usage.py` walks a local target once and its report answers every local question, `/System/Volumes/Data` standing for the whole Mac:

```bash
uv run --locked --script scripts/usage.py <target>
```

- `allocated` counts every clone in full and each hard link once at its first path, the figure `du` gives
- `physical` counts each clone stream once, and a clone written after cloning takes a stream of its own, counting the blocks it still shares again
- `volume` on a volume root states `used` and the gap that snapshots, refused folders, metadata, and deleted files still open hold
- `private` counts bytes deleting a file or folder alone frees, a folder holding every clone of a stream freeing that stream, snapshots aside
- `logical` above `allocated` marks a sparse, compressed, or dataless file, `flags` names which, and the walk never downloads a dataless one
- Folders rank by bytes past their largest subfolder and files by `allocated`, 40 rows each, and a rerun on a listed folder reaches below the cut
- Duplicates compare files from 1 MiB by size, then hash, across clone streams, copies sharing a `stream` number being clones
- Refused `EACCES` paths take the `sudo -H` rerun the line names, `EPERM` ones sit under SIP, or under TCC that Full Disk Access lifts

## [02]-[CLOUD]

Cloud targets report their own sizes, each Google account routed by `tool:google`, and `~/Library/CloudStorage/<provider>-<account>` and `~/Library/Mobile Documents` folders take [01]:
- Google Drive -> `gog api call drive v3 about.get --params '{"fields":"storageQuota"}' -j`, Gmail and Photos holding `usage` past `usageInDrive`
- Drive folders and files -> `gog drive du --depth 2 --max 0 -j`, `gog drive inventory --sort size --order desc --max 50 -j`
- Drive for desktop -> `DriveFS/<id>/content_cache` takes [01], `DriveFS/canceled_uploads/<id>/cancel_upload_data.txt` names original paths
- MEGA -> `mega-df` for account, rubbish bin, and versions, `mega-du -h --versions <path>`, `mega-find / --type=f --size=+<threshold> -l`
- Bytes a listing hides -> `rclone size --b2-versions --json <remote>:` for B2 versions, `rclone backend list-multipart-uploads <remote>:` for S3
- `rclone` remotes -> `rclone about <remote>: --json` for quota and trash, absent on R2 and B2, then one listing for folder and duplicate queries:

```bash
rclone lsjson -R --files-only --fast-list --onedrive-delta --hash --no-mimetype --no-modtime <remote>:<path> > <listing>.json
duckdb -c "select array_to_string(parts[:depth], '/') as folder, sum(Size) as bytes from (select string_split(Path, '/') as parts, Size from read_json('<listing>.json')), range(1, len(parts)) as levels(depth) group by folder having bytes >= <threshold> order by folder"
duckdb -c "select Size, count(*) as copies, (count(*) - 1) * Size as redundant, list(Path order by Path) as paths from read_json('<listing>.json') where Size > 0 and Hashes is not null group by Hashes, Size having copies > 1 order by redundant desc limit 40"
```

## [03]-[LITTER]

Walk `litter` rows rank folders by empty files, empty folders, and broken links, which free no space, beside these reads:
- Untracked files -> `git -C <repo> ls-files --others --exclude-standard --directory`, ignored outputs belong to the project's clean
- Temporary files -> `czkawka_cli temp -d <folder>... -C <out.json>` over `~/Downloads`, `~/Desktop`, and `~/Documents`
- Stale files -> walk rows of installers, archives, and disk images with old modification dates
- Stores -> `~/Library/Caches`, `~/Library/Logs`, `~/Library/Developer`, `~/.Trash`, `~/.cache`, `$(getconf DARWIN_USER_CACHE_DIR)`, tool caches

## [04]-[CLEAN]

Mode `clean` frees what [01] to [03] found, each item in this order:
1. Owner: a project's clean target, else the tool's prune, else deletion, previewed by its dry run or the item's walk rows
2. Holders: `lsof -nP -t -- <lock file>` and `pgrep -fl <tool>`, a held lock leaving the item in place with holder pid and command, never forced

Cases:
- Rasm outputs and caches -> `nx clean` and `nx clean -c cache` from the Rasm root, after peer sessions there finish
- `uv` -> `uv cache prune` when `$(uv cache dir)/.lock` has no holder, as running `uvx` servers hold it for life
- Tool caches -> `pnpm store prune`, `npm cache verify`, `mise prune -n`, `cargo clean -n`, `cargo -Zgc clean gc -n --max-download-age=<age>`
- Hugging Face -> `uvx --from huggingface_hub hf cache prune --dry-run`, which drops detached revisions and incomplete downloads
- Homebrew and Nix -> `brew cleanup -n -s --prune=all`, `nh clean all --dry --keep <n>`, as `nh` keeps one generation by default
- Colima homes Rasm `.cache/colima` and `~/.config/colima` -> `references/machines.md`
- Xcode -> DerivedData whose `info.plist` `WorkspacePath` is gone, `xcrun simctl delete unavailable`
- Simulator runtimes -> `xcrun simctl runtime delete --dry-run` with `--unusable`, then with `--outdated` for runtimes a newer one replaces
- Snapshots -> `tmutil thinlocalsnapshots /System/Volumes/Data <bytes> 4` for Time Machine, OS update snapshots stay
- Cloud trash -> `gog api call drive v3 files.emptyTrash --allow-write --force`, `mega-rm -r -f '//bin/*'`, OneDrive's on its page by `chrome:browser`
- Cloud versions -> `rclone cleanup --dry-run <remote>:` for old versions and unfinished uploads, `mega-deleteversions -f <path>` on MEGA
- Cloud duplicates -> `rclone deletefile <remote>:<path>` per redundant copy of [02], OneDrive holding each in its recycle bin
- Drive canceled uploads -> deletion only where `gog drive search` matches each by name and size, as one can be the only copy
- File Provider downloads -> Finder `Remove Download` through Rasad, as deleting a File Provider file deletes it in the cloud
- Duplicates both paths need -> `uv run --locked --script scripts/usage.py clone <target>`, `jdupes` keeping each copy's metadata
- Other duplicates, empty files and folders, broken links -> deletion outside `.git`, app libraries and containers, and tool state

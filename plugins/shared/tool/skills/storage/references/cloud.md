# [CLOUD]

Cloud targets report their own sizes, each Google account routed by `tool:google`, and `~/Library/CloudStorage/<provider>-<account>` and `~/Library/Mobile Documents` folders take the `SKILL.md` walk. Baselines are quota bytes (`rclone about` `used`, Drive `usage`), read again after each clean, and `SKILL.md` [STEPS] and its owner, then holders, order bind every clean.

## [01]-[MEASURE]

Reads go out as parallel calls in one message, each listing alone in the background, and long reads run as one shell pipeline or a script file under `uv run --script`, as hooks refuse inline interpreters:
- Google Drive -> `gog api call drive v3 about.get --params '{"fields":"storageQuota"}' -j`, Gmail and Photos holding `usage` past `usageInDrive`
- Drive folders and files -> `gog drive du --depth 2 --max 0 -j`, `gog drive inventory --sort size --order desc --max 50 -j`
- Drive for desktop -> `DriveFS/<id>/content_cache` takes the walk, `DriveFS/canceled_uploads/<id>/cancel_upload_data.txt` names original paths
- MEGA -> `mega-df` for account, rubbish bin, and versions, `mega-du -h --versions <path>`, `mega-find / --type=f --size=+<threshold> -l`
- B2 and S3 -> `rclone size --b2-versions --json <remote>:` for B2 versions, `rclone backend list-multipart-uploads <remote>:` for S3
- Google Drive orphans and trash -> `rclone backend rescue <remote>:` lists orphaned files holding quota, `rclone size --drive-trashed-only <remote>:`
- `rclone` remotes -> `rclone about <remote>: --json` for quota and trash, absent on R2 and B2, then one saved listing every later read takes:

```bash
rclone lsjson -R --files-only --fast-list --hash --no-mimetype --no-modtime <remote>:<path> > <listing>.json
duckdb -c "select array_to_string(parts[:depth], '/') as folder, sum(Size) as bytes from (select string_split(Path, '/') as parts, Size from read_json('<listing>.json')), range(1, len(parts)) as levels(depth) group by folder having bytes >= <threshold> order by folder"
duckdb -c "select Size, count(*) as copies, (count(*) - 1) * Size as redundant, list(Path order by Path) as paths from read_json('<listing>.json') where Size > 0 and Hashes is not null group by Hashes, Size having copies > 1 order by redundant desc limit 40"
```

Hidden bytes are `used` minus listing bytes (`duckdb -c "select sum(Size) from read_json('<listing>.json')"`) minus `trashed`, version history on OneDrive and SharePoint. Remotes holding hidden bytes take a version read over the 40 largest files, each version list opening with the current version, and hidden bytes the read leaves stand as the `gap` fact:

```bash
token=$(rclone config dump | jq -r '.["<remote>"].token | fromjson | .access_token')
drive=$(rclone config dump | jq -r '.["<remote>"].drive_id')
jq -r 'sort_by(-.Size)[:40][] | [(.ID | split("#") | last), .Path] | @tsv' <listing>.json | while IFS=$'\t' read -r item path; do xh -b GET "https://graph.microsoft.com/v1.0/drives/$drive/items/$item/versions" "Authorization:Bearer $token" | jq -c --arg path "$path" '{path: $path, versions: (.value | length), old: ([.value[1:][].size] | add)}'; done
```

## [02]-[JUDGMENT]

Duplicate groups and version rows decide each clean:
- Folder holding only copies of files elsewhere, renamed members included -> one folder purge
- Copies under unrelated names (a deed and a title policy) -> a misfile row for the owner, deleting none
- Many copies of one template -> stay
- Versions concentrated under one folder -> a cleanup scoped to that folder, keeping history elsewhere
- Version growth -> a `ruling` row for the library's version limit and for working files kept off the drive

## [03]-[CLEAN]

OneDrive cleans need `Files.ReadWrite.All` in the `access_scopes` that `rclone config show <remote>` prints, else `rclone config update <remote> access_scopes "Files.ReadWrite.All Sites.ReadWrite.All offline_access"` and `rclone config reconnect <remote>:` run first:
- Cloud trash -> `gog api call drive v3 files.emptyTrash --allow-write --force`, `mega-rm -r -f '//bin/*'`, OneDrive's on its page by `chrome:browser`
- Cloud versions -> `rclone cleanup --dry-run <remote>:<folder>` on the folder the version read names, as a run on the remote root drops every earlier revision, and `mega-deleteversions -f <path>` on MEGA
- Cloud duplicates -> `rclone purge <remote>:<folder>` for a copy folder, `rclone deletefile <remote>:<path>` per other redundant copy, OneDrive holding each in its recycle bin
- Drive canceled uploads -> deletion only where `gog drive search` matches each by name and size, as one can be the only copy
- File Provider downloads -> Finder `Remove Download` through Rasad, as deleting a File Provider file deletes it in the cloud
- Quota after a clean -> `rclone about <remote>: --json`, `trashed` growing where removed bytes wait in the recycle bin before `used` falls

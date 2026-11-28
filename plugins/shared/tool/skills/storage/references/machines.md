# [MACHINES]

Machines are the VPS hosts of Rasm `infra/platform-stack.ts` `hosts` and the Mac's Colima VMs, each running a Docker Engine. Steps here replace `SKILL.md` [STEPS] for them, and its [04] order of owner, then holders, binds every clean.

## [01]-[OWNERS]

Owner files state each setting a read meets, and a finding against one starts from that file:
- Use Rasm `.config/mise.machine.toml` and `.config/mise.runner.toml` for OS state, journal cap, masked logs and timers, and Docker daemon settings
- Use Rasm `docs/Infrastructure.md` for machine, cache server, runner, and update behavior
- Use Rasm `infra/platform-stack.ts` for images, volumes, containers, and cache expiry each stack declares
- Use Rasm `docs/Workflows.md` for what each workflow keeps, prunes, and caches on a runner instance
- Use mazan-group `runbooks/tools/remote.md` for VPS reads past these steps, `runbooks/tools/ci.md` for runs and Actions caches
- Use mazan-group `systems/machines.md` for machine sizes and the disk growth a past read traced

## [02]-[MEASURE]

Hosts take one background call per line, every host and line in one message, and the walk runs under the idle scope, which yields CPU to runner jobs:

```bash
ssh <host> 'df -B1 --output=source,target,size,used,avail -x tmpfs -x devtmpfs -x overlay; journalctl --disk-usage; ps -C Runner.Worker -o etime=,unit=' < /dev/null
ssh <host> 'docker system df -v; docker ps -a --format "{{.Names}} {{.Status}}"; docker buildx du' < /dev/null
ssh <host> "lsof -nP +L1 | awk 'NR > 1 && \$10 !~ \"^/memfd:\" && \$7 >= 1048576'" < /dev/null
ssh <host> 'systemd-run --scope -q -p CPUWeight=idle timeout 900 du -xah -t <threshold> /' < /dev/null
ssh <host> 'du -sh /var/cache/apt /var/lib/systemd/coredump /var/log ~/.cache/mise; apt-get -s autoremove --purge; coredumpctl list --no-pager; docker image ls -f dangling=true' < /dev/null
hostinger vps vm metrics <id> --date-from <date>T00:00:00Z --date-to <date>T00:00:00Z | jq -r '.disk_space.usage | to_entries | sort_by(.key) | group_by(.key | tonumber | todate[0:10]) | .[] | "\(.[-1].key | tonumber | todate) \(.[-1].value)"'
```

Colima VMs take these reads from the Rasm root, where mise points `COLIMA_HOME` and `DOCKER_HOST` at Rasm's VM, `colima` calls on profile `default` take `COLIMA_HOME=~/.config/colima`, and `--context` overrides `DOCKER_HOST`:

```bash
docker --context colima-rasm system df -v
docker --context colima system df -v
colima list --json
COLIMA_HOME=~/.config/colima colima list --json
```

Readings decide what each number means:
- Engines store images through the containerd snapshotter, image bytes in `/var/lib/containerd`, volumes and containers in `/var/lib/docker`
- Rows of `lsof +L1` name each process holding a deleted file's blocks, which `df` counts and no walk finds, freed when that process exits
- `du -t` lists files and folders past the threshold in one walk, and a rerun on a listed folder with a lower threshold answers below the cut
- Machine sizes come from `ssh <host> du`, as `rclone size` on its sftp remote ran 60 to 130 times slower and counts symlinks as files
- Hostinger disk metrics of the `hostinger vps vm list` id print day-end `used`, the growth read, and lag a clean until the guest agent's daily trim, as root mounts with `nodiscard`
- VM disks sit at `<home>/_lima/_disks/<instance>/datadisk`, a sparse file whose `allocated` bytes the `SKILL.md` [01] walk reports
- Colima data disks mount with online discard, so blocks a prune frees return to the Mac's `/System/Volumes/Data` baseline at once, with no `fstrim`
- Discard proof: `colima ssh -p <profile> -- sh -c 'grep -x discard /proc/fs/ext4/$(basename $(findmnt -nvo SOURCE -T /var/lib/docker))/options'`

## [03]-[HOLDERS]

Running work holds a host, and its cleans wait while any of these reads prints a row:
- `Runner.Worker` line of [02] -> engine and checkout cleans wait, as a job reads its checkout and engine for its whole run
- `gh run list -w Deploy -s in_progress` -> every host clean waits, as its image builds and `mise bootstrap remote` use each engine
- `lsof /var/lib/dpkg/lock-frontend /var/cache/apt/archives/lock` -> `apt-get` cleans wait for the daily unattended upgrade holding them
- `docker --context <context> ps -q` on the Mac -> that Colima VM's cleans wait, as a suite between its pull and its container loses that image

## [04]-[CASES]

Each item takes its owner's call, and a call with no dry run lists what it removes first:
- Runner work folder `actions-runner-<label>/_work` -> stays, as each registration of the runner deletes it before its one job
- Nx local cache of a checkout -> stays under `nx.json` `maxCacheSize` (`GB` as 1024^3 bytes) by eviction, bytes past it after a job being a fault
- Cargo build folder of a checkout -> report its size, as no setting bounds it and `clean--cargo` makes each Rust cache miss build from nothing
- Runner folders `_diag`, `bin.*`, and `externals.*` -> stay, as the runner prunes each itself
- Dangling images -> `docker image prune -f`, freeing the `docker image ls -f dangling=true` sizes alone, which timer `docker-image-prune` also runs weekly
- Unused tagged images -> `docker image rm <ref>` for one neither `infra/platform-stack.ts` nor a suite under Rasm `tests/` names
- Build cache -> `docker builder prune -f --max-used-space <size>`, freeing the `docker buildx du` total past `<size>`, as `RECLAIMABLE` counts records it keeps
- Containers `buildx_buildkit_*` and their `_state` volumes with no Deploy running -> `docker rm -f` and `docker volume rm` of each
- Stopped containers -> `docker container prune -f` once `docker ps -a -f status=exited` lists no service container a stack names
- Anonymous volumes -> a `ruling` row per `docker volume ls -f dangling=true` volume with its size and `CreatedAt`, as an image's unnamed `VOLUME` holds service data
- Volume `storage` of role `Cache` -> stays, as its bucket expires each object after role field `kept`, rounded up to the next UTC midnight
- Cache objects older than that bound in the per-day count of `runbooks/tools/remote.md` -> report the lifecycle fault, as `kept` and that count prove it
- Bucket peak -> `projection` row of `kept` + 1 write-days times daily bytes, `find <bucket> -type f -printf '%TY-%Tm-%Td %s\n'` summing each day in one walk
- Journal -> stays under `SystemMaxUse`, and `journalctl --disk-usage` past it is a journald fault to report
- Container logs -> stay, as service containers log to the journal and job containers' `local` logs go with each container
- Core dumps -> stay under `systemd-coredump` limits, `coredumpctl list` naming each
- `mise` download caches (`~/.cache/mise`, `actions-tools-<label>/mise/cache`) -> `mise cache clear`, and `MISE_CACHE_DIR=<folder> mise cache clear` for the second
- Package cache -> `sudo apt-get clean`, and `apt-get -s autoremove --purge` previews unused packages and kernels before `apt-get autoremove --purge`
- Actions caches -> stay, as GitHub evicts past the repository limit by last access, and `gh cache delete <key>` removes a retired key
- Rasm's Colima VM -> `nx run docker:clean--docker` from the Rasm root, and `nx clean -c cache` deletes the VM with its disk
- Default Colima VM (`~/.config/colima`) -> the dangling image, build cache, and anonymous volume cases above with `docker --context colima`, as no project owns it

Calls `docker image prune -a` and `docker system prune -a` stay out of every host case, as stack images return only through a Deploy pull, which `customTimeouts` lets reach 2 hours for an Unreal image, and fixture images through Docker Hub pulls under its rate limit.

## [05]-[STEPS]

1. Measure: [02] reads go out as parallel calls in one message, each walk alone in the background
2. Hold: [03] reads run first, and a held item keeps its place with the holder's job, run, or pid
3. Clean: mode `clean` alone, each item by [04], one background call per clean, never chained
4. Delta: `df -B1 --output=avail <mount>` before and after each clean, `/` on a host, `/var/lib/docker` through `colima ssh -p <profile> --` in a VM
5. Report: subject `storage <mode> <hosts>`, a `baseline` per host, rows `<host>:<item> bytes=<n> owner=<file> result=<proposed | freed | stayed | fault>`
6. Rows: `growth: <host> <day> used=<bytes>`, `projection: <item> peak=<bytes> at=<date>`, `ruling: <file> <field> from=<value> to=<value> effect=<bytes>`

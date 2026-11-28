# [CACHING]

Steps here join `SKILL.md` [STEPS] for caches, state stores, runners, disk, and minutes, as each fresh runner starts empty.

## [01]-[CACHES]

Actions cache entries belong to a key, a version hashed from their paths, and the ref that saved them:
1. Read `caches:` rows of `scripts/runs.py`: `entries`, `size`, `other` (saved off the default branch), then `hit`, `partial`, `miss`, `saved`
2. Key each entry by what invalidates it: lock and toolchain files through `hashFiles(...)`, OS, architecture, `$ImageOS-$ImageVersion`
3. Restore everywhere with `actions/cache/restore` and a `restore-keys` prefix, and save on the default branch alone
4. Save under `<prefix>-${{ github.sha }}` or the lock hash, so slow-changing entries outlive fast ones
5. Skip the save when `steps.<restore>.outputs.cache-hit == 'true'`, as entries are immutable and the key already exists
6. Set `cache-mode` with a literal: `read` on pull request and third-party-code jobs, `none` on release builds
7. Keep paths identical on every runner of one key, as the cache version hashes them, and a `..` path is never saved

Entry forms:
- Single writer -> one job or workflow saves with `lookup-only: true` checking first, every other job restores and never saves
- Rebuild on miss -> a job missing its entry rebuilds the state, never `fail-on-cache-miss`, as eviction under the cap is routine
- Force on miss -> a push whose build-state restore missed runs its tasks past the task cache, so its save holds real output
- Fixed restore key -> `key: <prefix>-never-saved` with `restore-keys: <prefix>-` always takes the newest entry of a prefix
- Slow service -> `SEGMENT_DOWNLOAD_TIMEOUT_MINS` in the restore step's `env:` (default 10), composite steps included, and `timeout-minutes` with `continue-on-error: true` on a workflow step
- Pull request cleanup -> on `pull_request: [closed]`, `gh cache list --ref refs/pull/<n>/merge --json id` piped to `gh cache delete`

Repository limits:
- Cap -> 10 GB by default, least recently used entries evicted, unused entries kept 7 days
- Above the cap -> opt-in under Settings, billed per GB-month, the cache turning read-only at a reached budget
- Rate -> 200 uploads, 1500 downloads, and 400 deletes per minute per repository
- Untrusted triggers -> `pull_request_target`, `issue_comment`, `workflow_run`, and other untrusted events in the default-branch scope get read-only tokens
- Writing events -> `push`, `schedule`, `workflow_dispatch`, `repository_dispatch`, `delete`, `registry_package`, and `page_build` write their ref, `pull_request` and `release` their own scope
- Effective mode -> `ACTIONS_CACHE_MODE` reaches JavaScript and Docker actions alone, and a refused save logs and continues

## [02]-[STATE]

State whose restore outlasts its rebuild in the Actions cache, or state the cap cannot hold beside every job's tool entries, moves outside the Actions cache:
1. Weigh compressed size and restore time against the time saved, read from `cold` rows of `scripts/runs.py`
2. Choose the layer by what replays (task cache, compiler cache, or build folder snapshot), each listed below
3. Split credentials: write on default-branch pushes, read-only on pull requests, as fork runs hold no secrets and fall back to a rebuild
4. Make restored state fresh by content where the build tool offers it, as checkouts reset mtimes
5. Turn off incremental state no fresh runner reuses

Replay layers, each with its form and constraint:
- Task cache (Nx, Turborepo, Bazel) -> replays a whole task on a full hash match, nothing on a miss
- Compiler cache (sccache, ccache) -> replays `rlib` and `staticlib` units, never build scripts, proc-macros, or links, `SCCACHE_S3_RW_MODE=READ_ONLY` on pull requests
- Compiler cache on the Actions cache backend (`SCCACHE_GHA_ENABLED`) -> writes one cache entry per object, thousands per run, against the repository cap
- Build folder snapshot -> replays everything the build tool judges fresh, a no-op build when no input changed
- Snapshots in an S3-compatible bucket -> `runs-on/cache` with ranged parallel downloads, a fork of `actions/cache` trailing it by releases, its bucket secrets absent on forks
- Cargo freshness by content -> `build.fingerprint = "content"` with `-Zchecksum-freshness`, a nightly toolchain alone, and `CARGO_INCREMENTAL=0` on CI jobs
- `Swatinem/rust-cache` -> keeps dependencies and strips workspace members, which compile or lint again on every miss
- `RUSTFLAGS` in workflow `env:` -> replaces every `build.rustflags` the repository sets, where `CARGO_PROFILE_<NAME>_<KEY>` merges

## [03]-[RUNNERS]

Runner choice sets CPU, disk, OS, cost, and what state survives between jobs, specs those of public repositories, as private ones get 2 vCPU and 8 GB on `ubuntu-<version>` and `windows-<version>` labels, arm64 included:

| [INDEX] | [RUNNER]                 | [SPEC]                             | [BILLING]                | [FITS]                                |
| :-----: | :----------------------- | :--------------------------------- | :----------------------- | :------------------------------------ |
|  [01]   | `ubuntu-slim`            | 1 vCPU, 5 GB, container, no Docker | Free on public repos     | Planners, gates, notices under 15 min |
|  [02]   | `ubuntu-<version>`       | 4 vCPU, 16 GB, 14 GB disk          | Free on public repos     | Builds sized to 14 GB                 |
|  [03]   | `ubuntu-<version>-arm`   | 4 vCPU arm64, 16 GB, 14 GB disk    | Free on public repos     | Jobs whose tools ship linux-arm64     |
|  [04]   | `windows-<version>`      | 4 vCPU, 16 GB, `D:` work drive     | Free on public repos     | Windows hosts, work on `D:`           |
|  [05]   | `xcode-<major>`          | 3-core M1, 7 GB, 14 GB disk        | Free on public repos     | Apple builds                          |
|  [06]   | `<label>-xlarge`         | 5-core M2 Pro, 14 GB               | Per minute on every repo | Apple builds bound by CPU             |
|  [07]   | Linux and Windows larger | 2 to 96 cores, 75 to 2040 GB SSD   | Per minute, Team org     | Disk-bound builds, custom images      |
|  [08]   | Self-hosted              | Owner's machine                    | Free minutes             | Persistent state, private networks    |

1. Read `runner=`, `p90=`, `queue=`, and `billed=` per job, `billed` summing hosted jobs rounded up to whole minutes as GitHub bills
2. Size tool sets to the 14 GB a standard runner guarantees, as a full disk loses the job's logs
3. Free preinstalled SDKs a job replaces in a background step (`/usr/local/lib/android`, `/usr/share/dotnet`, `/opt/ghc`), with `df -h /` lines
4. Keep Windows work, tools, and caches on `D:`, and mount a Dev Drive on larger Windows runners, which lack one
5. Pin Xcode by path (`sudo xcode-select -s /Applications/Xcode_<version>.app`), as an image default moves between releases
6. Read a paid label from a repository variable (`runs-on: ${{ vars.<NAME> }}`), so a budget stop swaps it with no edit
7. Choose dated labels (`ubuntu-26.04`) over `-latest`, as a moving image changes a job with no workflow change to review

Account facts:
- Concurrency -> 20 jobs and 5 macOS jobs at once on Free, standard and larger macOS sharing the 5
- Larger macOS labels -> run on a user account with a payment method and a spending limit above zero
- Included minutes -> larger runners draw none outside Team and Enterprise
- Custom images -> `jobs.<id>.snapshot` saves a larger Linux or Windows runner's VM after a job, in an organization alone
- Budgets -> personal accounts set them in the web UI, and "Stop usage" at the cap stops every paid job

Service containers per job:
- Linux runners alone run job and service containers
- `services.<id>.options` with `--health-cmd`, `--health-interval`, and `--health-retries` hold steps until the service is healthy
- Jobs on the runner reach services at `localhost:<port>` through `ports:`, and jobs in a container reach them by service name
- `--tmpfs <data folder>` keeps a database in memory, and `image: ${{ matrix.<image> }}` runs one service version per row

## [04]-[SELF_HOSTED]

Self-hosted runners keep state between jobs and need lifecycle work hosted runners do for free:
1. Register each runner just in time through `POST /repos/{owner}/{repo}/actions/runners/generate-jitconfig`, one job per registration
2. Start it with `run.sh --jitconfig <config>` under a supervisor starting the next, or an Actions Runner Controller scale set
3. Keep tool and build caches outside the work folder, as `_temp` empties each job while `_tool` and checkouts are never cleaned
4. Run maintenance through `ACTIONS_RUNNER_HOOK_JOB_STARTED` and `ACTIONS_RUNNER_HOOK_JOB_COMPLETED`, printing disk use per job
5. Drain by removing labels and waiting out a busy job, never by stopping the service mid-job
6. Hold public repositories' self-hosted jobs to `push`, `schedule`, and `workflow_dispatch`, or gate `pull_request` jobs on `github.event.pull_request.head.repo.full_name == github.repository`, as fork pull requests run in the base repository's context
7. Gate on `github.repository_owner` to keep forks' own runs off the owner's runners, a gate that passes fork pull request code

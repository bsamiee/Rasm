# [NX]

Nx behavior behind workspace plugins and `nx.json` that their files do not state.

## [01]-[PLUGINS]

Plugin globs, `targetDefaults` rows, and what Nx does with each:
- Changed files matching a plugin's `createNodes` glob are a configuration change of the project owning their path, root project `rasm` for unowned files
- Deleted or renamed matches select every task, as Nx checks only that a matching path is gone, never that it marked a project
- Plugins read side files inside `createNodes` and hash them into their cache key, as Nx accepts results keyed by a file outside the glob
- Root files marking a root target (`Cargo.toml`, `uv.lock`, `Workspace.slnx`, `CMakePresets.json`, `package.json`, `pnpm-lock.yaml`) select every root task
- Root tasks a root file selects restore from cache unless their own inputs changed, as Nx splits no project at `.`
- Plugin `include` and `exclude` rows of `nx.json` limit the graph alone, as `nx affected` matches changed files against raw `createNodes` globs
- Root `.gitignore` and root `.nxignore` alone keep a changed file out of `nx affected`
- `targetDefaults` keys take an array of rows with `filter.projects`, tags included, and merge into inferred targets without creating one
- Configuration options merge over a target's default options, so plugins state an option in each configuration its default value is wrong for
- `"..."` in a `targetDefaults` `dependsOn` keeps the entries a plugin inferred
- `dependsOn` entries naming projects, by name or pattern, warn "misconfigured" on each task they plan no dependency for
- `dependsOn` strings of form `<project>:<target>` naming no project warn as a target name holding `:` no project holds
- `^<target>` entries plan over graph edges alone and never warn, so a target existing on one platform alone needs a project holding it on every platform
- Conformance reports each `dependsOn` entry resolving to no target, and checks each exact `^` name

## [02]-[HASHING]

Task hash contents:
- Input files hash whole, and JSON files alone take a field filter
- `nx.json`, `.gitignore`, and `tsconfig.base.json` hash into every task, and no setting removes them
- Each file belongs to the deepest project holding it, and a `{projectRoot}` fileset matches a project's own files alone
- Plugins name files a tool loads from below a project folder by `{workspaceRoot}`, as no plugin sees projects another plugin infers there
- Each task hashes its project's root, tags, named inputs, and each target's name, executor, outputs, options, configurations, and parallelism
- Target `dependsOn` and inputs stay out of other tasks' hashes
- Inputs belong to a target and never to a configuration, and a task hash holds its configuration name and overrides
- `{ fileset, includeIgnored: true }` hashes a gitignored or linked path from disk, and a linked file where it points
- `includeIgnored` filesets count toward `nx affected` only where they overlap a task's declared outputs
- Projects owning no tracked file hold no entry in Nx's file map, and a task with inputs reaching one through a graph edge fails to hash

## [03]-[SELECTION]

How `nx affected` reaches tasks:
- Selection passes to a dependent task through `dependentTasksOutputFiles` inputs alone, never through plain `dependsOn`
- Runtime and env inputs change a hash, and `nx affected` never selects through them
- `nx affected` learns of moved external packages from JavaScript lock files alone
- `nx show projects --affected --withTarget=<target>` lists projects owning an affected task of that target, and without the flag every touched project
- `nx affected --explain --verbose` prints each task's selection reason

## [04]-[EXECUTION]

Task runs and their order:
- Nx runs one batch at a time, starts no other task while it runs, and stores its results when it ends
- Targets running `xcodebuild` set `parallelism: false`, as builds into one build folder queue behind each other
- Concurrent first package resolutions into one build folder can fail
- Project `Rasm-Package` tests every Swift suite in one `xcodebuild` run, as one suite costs about what all of them cost

## [05]-[REMOTE_CACHE]

Remote cache clients and the inputs deciding which machines share a result:
- Owner's Mac reads Doppler `dev` through mise `[env]`, and uses its local cache while `dev` holds no row
- Owner's Mac and CI jobs share one remote cache, and results serve each machine with matching hash inputs, row `macOS` the Mac's Swift and Xcode tasks
- .NET targets but format hash `CI`, as MSBuild sets properties under it
- Vitest targets hash no `CI`, as their executor refuses `.only` and writes no missing snapshot on every machine
- Vitest targets hash platform, as suites read host shells and file systems, and no Mac result of theirs serves a Linux or Windows job
- Playwright targets hash no `CI`, as each one runs `playwright test --forbid-only`, and `pack--playwright` hashes the platform its images render on
- Swift targets hash compiler version, SDK build, and platform, as `swift --version` adds its host's macOS version, which a runner and a Mac differ in
- Xcode targets add `xcodebuild -version` and `XCODE_XCCONFIG_FILE` content to those inputs
- Xcode targets hash the SwiftPM targets their linked package products reach alone, and other Swift changes leave an app's result cached
- Cargo targets hash `rustc -vV`, which prints its host triple, and no Mac result of theirs serves a Linux job
- Python checks and tests hash platform, and no Mac result of theirs serves a Linux job
- Root project `rasm` holds no Docker Engine target, and its tasks share keys between the Mac and Linux
- Nx sets no remote cache timeout and stops a run on a read answered other than 200 or 404 or left unconnected, and no setting overrides the stop
- `@rasm/nx-distribution` reads the cache server with its token once before the tasks of each `nx` run that uses the remote cache, outside any task
- Reads answered other than 200 or 404, refused, or past option `remoteCacheTimeout` print one warning and empty `NX_SELF_HOSTED_REMOTE_CACHE_SERVER`
- Nx stores nothing and reports no error for a write answered 403 or 409, and fails a task after 6 attempts of any other failed write

# Remaining work

Work from session `rasm-74` that is not finished and that this session will not build. It is stated here for a fresh session to take up
after the test-layout session (`rasm-36`, working from `TEST_LAYOUT.md`) is done. Written 2026-11-12.

Everything below is either a recorded fact with its source, or marked as not verified. Nothing here was re-proven today unless it says so.
The C++ and Unreal work happened in an earlier part of the session; what is said about it comes from the session ledger, the saved proof
logs, and the tree as it stands.

## Where the research is

All under `docs/research/remaining-work/` (git-ignored), copied out of the temporary scratch folder so it survives:

| Path | Holds |
| --- | --- |
| `session-ledger.md` | Every decision, finding and owner ruling of the session, in order |
| `cpp-unreal/scout-cpp-unreal.md` | 1,454 lines: how a language is registered here, Unreal as it exists here, Unreal 5.8 and Fab facts, C++ tool facts, the gap list (rows R01–R37 for C++, U01–U10 for Unreal), Pixi as a dependency source |
| `cpp-unreal/cpp-survey-{infrastructure,graphics,language,geometry,tooling}.md` | Five surveys of real C++ repositories |
| `cpp-unreal/proof-logs/` | The Unreal and C++ proof runs on this Mac: `buildplugin.log`, `ubt-project.log`, `ubt-analyze.log`, `test-pass.log`, `nx-test-pass.log`, `nx-test-fail.log`, `nx-test-library.log`, `nx-test-library-arm64.log`, `pack-lists.txt` |
| `cpp-unreal/cpp-env/`, `cpp-unreal/unreal-executor.ts` | The Pixi environment trial and an executor draft |
| `release/scout-release.md`, `review-release.md`, `review-release-2.md` | Release research and two adversarial reviews |
| `node-27-toolchain-audit.md` | Node 27 alpha changes, which tools test on which Node, source scan for breakages |

Exemplar copies of real C++ and Unreal repositories: `tests/.tmp/cpp/setup/` (Ladybird, eCAL, F3D, OpenEXR, the Beman exemplar, the Azure SDK for C++, sentry-unreal, cesium-unreal, lootlocker, UnrealFastNoise2, each with a `COMMIT` file), plus `tests/.tmp/cpp/test-architecture/`.
The temporary C++ packages and Unreal projects that were built and then removed are in git history at commit `01096abb8` under `libs/cpp/`, `tests/cpp/libs/`, and `apps/unreal/`.

## 1. C++ and Unreal

### Goal as recorded

The ledger records it as: C++ "registered in every place Rust is", proven with temporary packages, then Unreal delivery through Epic's own
tools on Unreal Engine 5.8, then a fresh review. The owner's original wording of the request is not in this session's context; ask the owner if the goal needs restating.

### Rebuilt on 2026-11-13

The C++ and Unreal tooling was deleted and rebuilt from the reading of 68 repositories in `.tmp/TOOLING.md`. README `[03]-[PRINCIPLES]`, `[06]-[OWNERS]` rows 25 and 26,
and `[07]-[STRUCTURE]` state the result: LLVM from a mise row, libraries from `vcpkg.json`, flags in the root `CMakeLists.txt`, a build folder per preset,
and the Unreal executor reading test results from the exported report.

### Not done

1. **Unreal on Linux CI.** Doppler `ci` and 1Password hold `EPIC_GHCR_TOKEN`, which reads `ghcr.io/epicgames/unreal-engine`. No workflow job and no `unreal-Linux.cmake`
   toolchain file exist: the image is x64 alone and 24 GB, so its proof is a workflow run on a hosted runner.
2. **Release rows.** The archive, CPS file and SBOM per package on the GitHub release, and agreement of archive name, CPS version and tag in a dry run. The builder was told to do this last and was stopped before it.
3. **The plug-in to package edge.** `nx run <package>:install--cmake:unreal --args=--prefix=<plug-in>/Source/ThirdParty/<Name>` installs a package into a plug-in by argument;
   no manifest row states that prefix yet.
4. **A first real consumer.** A decision for the owner: which C++ package or Unreal plug-in is built first.

## 2. Node 27 alignment pass

Owner ruling, verbatim: "remove the handtyped 2nd version - just fully align to alpha? but keep this as last so we fix all other things - but we can deep mine all new things it enables + allows us to do and other tools to move with it and their capabilities to fully land on that + ensur no mixed paradimgs we rebuild things to that".

Done on 2026-11-12, uncommitted:
- `.config/mise.lock` holds Node `27.0.0-alpha.0.1.0`, and `NODE_VERSION` in `.config/mise.toml` derives from `tools.node.version`.
- Compose Node images install that version through mise in a build stage of `infra/compose.yaml` and copy `node` and `libatomic.so.1` onto the distroless nonroot base, so Mac, CI, the Atelier helper, and containers run one nodejs.org build.
- `infra/platform-stack.ts` holds no `dhi.io` login, `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` stay in Doppler unread.
- Atelier's helper builds with `useCodeCache`, which under `useVfs` caches every module of its virtual file system.
- `Math.sumPrecise` sums in typography shaping and the portfolio grid, and `Iterator.zip` replaces the four nested zips in `apps/nx/devkit`, `apps/nx/distribution`, `tests/typescript/support/nx`, and `libs/typescript/data/database`.
- workerd 1.20261110.1 holds all five builtins, Safari gains `Iterator.zip`, `join`, and `includes` in 27.3, and Adobe hosts and the hooks module keep `Number.sumAll` and `Array.zip`.
- Proof: `nx run-many -t check` over ten edited and consumer projects passed, the `hooks` image ran `node index.js` as uid 65532 up to its `POSTGRES_URL` check, and the built helper started from its virtual file system.

Facts:
- Node derives its heap limit from the container memory limit (268 MiB under `-m 512m`, 1072 MiB under `-m 2g`), and hosted apps set `mem_limit`, so no stack states a heap size.
- `NX_USE_V8_SERIALIZER` governs Nx socket channels alone, and `useDaemonProcess: false` with `NX_ISOLATE_PLUGINS=false` opens none.
- `@types/node` has no 27 release, so `net/promises`, `fs.mkstemp`, `zlib.crc32c`, and `subprocess.timedOut` stay unused until it does.
- `cors: false` on `preview--storybook` stays while `http-server` depends on `corser`.
- No tool in the serve-and-test path runs its own CI on Node 27 except `ws` (`node-27-toolchain-audit.md`).

Not done:
- No test ran on `27.0.0-alpha.0.1.0`.
- Not measured: Node 26 against Node 27 timing, and helper start time with and without the code cache.

## 3. First release

Acceptance set with the owner: one real `release.yml` run produces tag `v0.1.0` with every project's file attached.

Facts:
- 0 git tags, 0 GitHub releases, 0 runs of `release.yml` ever.
- `nx release 0.1.0 --skip-publish --first-release --dry-run` exits 0 locally and writes 0.1.0 into all 49 manifests through each plugin's version actions. No manual baseline step is needed.
- The workflow runs `nx release --skip-publish`, then `nx run-many -t attach -c release`, then `nx release publish`. Dispatch inputs: `specifier`, `first-release`, `dry-run` (on by default), `publish`.
- The live tag ruleset on GitHub still matches the old `**@**` pattern until `nx deploy @rasm/infra` runs.
- Tokens in Doppler `release`: `NUGET_API_KEY` is present (expires 2026-12-12). `NODE_AUTH_TOKEN` and `UV_PUBLISH_TOKEN` are absent; the npm and PyPI accounts exist, the publish tokens were not made. Apple notarization rows are absent until the owner's enrollment finishes, so Mac apps release as a `.zip`.
- Not exercised: the Sparkle feed against a real `.dmg`, and the infra row.
- The second release review (`release/review-release-2.md`) was written before the release shape changed to one version and one tag; its findings have to be read against the current shape.

Not done: no dispatch of `release.yml`, dry or real. It waits on a green CI run on `main`, which is red for reasons in the test-layout session's work.

## 4. Smaller open items

- **No root `clean--cargo` target.** The Cargo build folder reached 101 GB in four days. Cargo has no build-folder garbage collection (cargo#5026 is open); `cargo-sweep` is unmaintained and ignores `build-dir`. Owner choice recorded and not made: wait for cargo#5026 and reclaim with `cargo clean`, or a prune in the plugin.
- **Fetched .NET inputs match no files.** `.cache/dotnet/fetched/<sha>` filesets are git-ignored and need `includeIgnored`; the dotnet plugin currently drops them.
- **Unused catalog rows** `sqlite-vec` and its `-bun`, `-wasm`, `-do` variants in `pnpm-workspace.yaml`, noted and not acted on.
- **The hooks module has no check on a CI runner.** Owner ruling: no Nx target yet; `claude plugin validate --strict plugins/function-hooks` and `claude plugin test plugins/function-hooks` are run by hand (both passed on 2026-11-12, 12 tests).
- **Review owed.** The session's two pushed fixes (`5fb1895a1`, `518e6ad90`) and the earlier pushed work of this session have had no fresh adversarial review.

## Order and dependencies

1. The test-layout session finishes first: it owns the test tree, the Storybook, Vitest and Playwright rebuild, the Nx regression that keeps CI red, and the CI job split.
2. Then C++ and Unreal, Node 27, and the first release, in the order the owner chooses. The owner's standing order put the Node pass last and the release after CI is green.

## Rules that bind this work

From the owner and the repository, as they applied all session:
- Official repositories, docs and real projects are the truth; our code is assumed wrong where it differs; sources are read in full.
- No retry, raised limit, guard or fallback hides a cause; a cause is proven before a fix lands.
- Temporary proof projects are removed after they prove their point.
- Tests come from exemplars copied into `tests/.tmp/<language>/` with source and commit, never shaped by our code.
- Commits, pushes, and writing or running tests happen on the owner's request.
- A fresh adversarial review follows any durable work.

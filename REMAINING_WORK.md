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

### Built on 2026-11-13

Research for this pass is under `docs/research/remaining-work/finalize/` (`a-cicd-vps.md`, `b-release-distribution.md`, `c-unreal.md`).

- **Package and plug-in.** `libs/cpp/numerics/tolerance` (CMake project `rasm-tolerance`) and `apps/unreal/Tolerance` with host project `apps/unreal/Sandbox` are the first consumers.
- **The plug-in to package edge.** `.uplugin` table `nx.packages` maps a folder to the Nx target installing a package there. The plug-in build runs it with prefix `<folder>/<Mac|Linux>`, and `install--cmake` builds what it installs.
- **Release rows.** The package states `install(PACKAGE_INFO)` and `install(SBOM)`, so the CPack archive holds the CPS file and the SBOM. No plugin change was needed.
- **Unreal on Linux.** `.config/cmake/toolchains/unreal-Linux.cmake`, job `unreal` of `ci.yml` and of `release.yml` in Epic's `dev-slim-<EngineAssociation>` image, and the `UE_<version>` row lookup of `Install.ini`.

Proven on this Mac with Unreal Engine 5.8.4:
- `nx run-many -t check,build,test,pack` over `rasm-tolerance`, `Tolerance`, and `Sandbox` passed, and the report lists `Tolerance.Within` alone with state `Success`.
- `NX_DRY_RUN=true nx run rasm-tolerance:attach` printed `gh release upload v0.0.0 .artifacts/cpack/rasm-tolerance/rasm-tolerance-0.0.0-Darwin-arm64.tar.xz`, and the CPS file in that archive states version `0.0.0`.
- `nx release 0.1.0 --skip-publish --first-release --dry-run` wrote `VERSION 0.1.0` in the package's `project()` call and `VersionName` `0.1.0` in the `.uplugin`.
- `Tolerance-5.8.zip` holds the tracked sources, `EngineVersion` `5.8.0`, and the installed header, archive, CPS files, and SBOM under `Source/ThirdParty/RasmTolerance/Mac`.

### Not done

1. **Linux proof.** Nothing of the Linux side ran on this Mac: the toolchain file, the mise tools on the image's Ubuntu 22.04, and job `unreal`. Its proof is the first CI run holding the job.
2. **Runner.** Job `unreal` runs on hosted `ubuntu-26.04` and pulls the 22 GiB image on each run. `derby` (8 vCPU, 31 GiB memory, 385 GB free, no service) can hold a self-hosted runner that keeps the image; `a-cicd-vps.md` `WORK` rows 1 to 4 state that form and its fork-approval setting.
3. **Fab.** Fab takes no upload from a tool, and no Fab publisher profile exists. After a release holds `Tolerance-<engine>.zip`, the owner creates the profile and a draft listing from that link, where Fab's upload scan answers. "Submit for review" then has Fab's build farm compile it, and "Cancel submission" stops it before a human review.
4. **One platform per source archive.** A pack on a Mac holds the `Mac` library and one on Linux the `Linux` library. An archive holding both needs the installed folders of both jobs.

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
- `nx release 0.1.0 --skip-publish --first-release --dry-run` exits 0 locally and writes 0.1.0 into all 47 manifests through each plugin's version actions. No manual baseline step is needed.
- The workflow runs `nx release --skip-publish`, then `nx run-many -t attach -c release`, then `nx release publish`. Dispatch inputs: `specifier`, `first-release`, `dry-run` (on by default), `publish`.
- The live tag ruleset on GitHub matches `refs/tags/v**`.
- Tokens in Doppler `release`: `NUGET_API_KEY` is present (expires 2026-12-12). `NODE_AUTH_TOKEN` and `UV_PUBLISH_TOKEN` are absent; the npm and PyPI accounts exist, the publish tokens were not made. Apple notarization rows are absent until the owner's enrollment finishes, so Mac apps release as a `.zip`.
- Not exercised: the Sparkle feed against a real `.dmg`, and the infra row.
- The second release review (`release/review-release-2.md`) was written before the release shape changed to one version and one tag; its findings have to be read against the current shape.

Not done: no dispatch of `release.yml`, dry or real. It waits on a green CI run on `main` holding job `unreal`.

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

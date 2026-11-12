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

### Decisions made (2026-11-11, by the main session from the surveys)

1. Toolchain: upstream LLVM 23 (`clangxx`, `libcxx`, `lld`, `clang-tools`) and Ninja in a Pixi `cpp` environment, locked in `pixi.lock`; CMake stays a mise tool. Unreal builds with Xcode's clang through UnrealBuildTool.
2. Language: C++26, named modules, `import std`, extensions off. CMake 4.5 needs no experimental gate for `import std`.
3. Dependencies: conda-forge through the same Pixi environment. No vcpkg, Conan, CPM, or FetchContent.
4. Shape: one root CMake build; each package states its own `project(VERSION)` and builds alone or under the root; the root finds members by a manifest glob, with no member list.
5. Nx: our own `@rasm/nx-cmake`, inferring projects, targets and link edges from the CMake file API, never from `CMakeLists.txt` text.
6. Tests: Catch2 v3 from conda-forge.
7. Checker files `.clang-format`, `.clang-tidy`, `.clangd` at the root; warnings and clang-tidy findings are errors.
8. Delivery: a CPack archive per package with a CPS file and an SBOM on the GitHub release; no registry until the owner picks one.
9. Unreal: targets only where an engine exists; Epic's `RunUAT BuildPlugin`; automation tests through `UnrealEditor-Cmd`; a source archive for Fab and a binary package for GitHub.

Builder notes accepted along the way: libc++ on macOS only, Linux uses the libstdc++ that `clangxx` brings, Windows is out of the Pixi environment
(conda-forge holds no C++ standard library for win-64); `catch_discover_tests` instead of CMake `discover_tests` (it cannot escape Catch2 test names);
presets hold no build folder and the plugin passes `-B` under `.artifacts/cmake/<preset>`; clang-tidy runs through `CMAKE_CXX_CLANG_TIDY` in its own build tree.

### What exists in the tree

- Pixi `cpp` environment in `pyproject.toml` (`[tool.pixi.environments.cpp]`: clangxx 23, clang-tools, lld, compiler-rt, ninja, catch2, fmt, gersemi), locked in `pixi.lock`
- Root `CMakeLists.txt`, `CMakePresets.json`, `.config/cpack.cmake`, `.clang-format`, `.clang-tidy`, `.clangd`, `.gersemirc`, `tests/cpp/.clang-tidy`
- `apps/nx/cmake/` (`plugin.ts`, `targets.ts`, `workspace.ts`, `version-actions.ts`, executors `cmake` and `package`) and its row in `nx.json` with the Pixi launcher
- Inference test `tests/typescript/apps/nx/inference/cmake.test.ts`
- Unreal support inside `apps/nx/distribution/` (`host-packages.ts` finds the installed engine; `executors/unreal-build/`)
- File-level check steps for clang-format and gersemi in `.config/hk.pkl`; the `cpp-coding` skill; README lines 35–40, 140–146, 190–191, 227–228, 240–241
- No C++ package and no Unreal project. Everything above is tooling without a consumer.

### What was proven (this Mac only, with the temporary projects)

- Two temporary C++ packages (`libs/cpp/numerics/tolerance`, `libs/cpp/geometry/extent`) with Catch2 tests built and ran through Nx.
- `RunUAT BuildPlugin` of a temporary plug-in `RasmProbe` on UE 5.8: "BUILD SUCCESSFUL", exit 0 (`buildplugin.log`).
- A project build through UnrealBuildTool succeeded (`ubt-project.log`).
- `RasmProbe:test--unreal` passed through Nx (`nx-test-pass.log`), and a deliberately failing run failed (`nx-test-fail.log`).
- The plug-in linking a C++ package as a static library under `Source/ThirdParty`: the first run exited 1 (`nx-test-library.log`), the arm64 run passed (`nx-test-library-arm64.log`). Proven for arm64 on macOS only.
- The plug-in package zip was produced (`pack-lists.txt`).

### Not done

1. **Linux proof.** C++ has never been built or tested off this Mac. CI has never run a C++ target, because no C++ project exists.
2. **Release rows.** The archive, CPS file and SBOM per package on the GitHub release, and agreement of archive name, CPS version and tag in a dry run. The builder was told to do this last and was stopped before it.
3. **Two follow-ups asked of the builder and never verified:** one owner for the compile-database path, and presets read only as typed JSON.
4. **Unreal consuming a C++ library beyond arm64 macOS,** and the standard-library question behind it (LLVM 23 libc++ against Apple's).
5. **No adversarial review** of any of the C++ or Unreal work.
6. **Gap list status.** `scout-cpp-unreal.md` Part 5 lists rows R01–R37 and U01–U10. Its "repository holds" column predates the builder; each row has to be checked against the tree.
7. **Paths under the new test layout.** `CMakeLists.txt` globs `tests/cpp/apps/*/*` and `tests/cpp/libs/*/*`, and README line 240 says tests sit "at `tests/cpp/` beside the same path". Under `TEST_LAYOUT.md` a C++ package's tests sit at `libs/cpp/<category>/tests/<package>/`. This belongs to the test-layout session if it reaches it first.
8. **A first real consumer.** A decision for the owner: which C++ package or Unreal plug-in is built first.

## 2. Node 27 alignment pass

Owner ruling, verbatim: "remove the handtyped 2nd version - just fully align to alpha? but keep this as last so we fix all other things - but we can deep mine all new things it enables + allows us to do and other tools to move with it and their capabilities to fully land on that + ensur no mixed paradimgs we rebuild things to that".

Facts:
- CI and this Mac both run Node `27.0.0-alpha.0.0.0`, because `.config/mise.toml` asks for `node = "latest"` with `prereleases = true`.
- `.config/mise.toml` `[env]` holds a hand-typed `NODE_VERSION = "26.13.0"`. It only names the Compose container image (`infra/compose.yaml:96`, `dhi.io/node:${NODE_VERSION}`) and is read by `infra/platform-stack.ts`.
- No Docker image of Node 27 alpha exists on dhi.io or Docker Hub. dhi.io has 26.13.0.
- Two forms were recorded for the Compose Node images and neither is decided or tried: install Node from the nodejs.org archive in the build, or install it through mise onto the distroless base.
- What Node 27 brings (nodejs.org release post, 2026-10-29): V8 15.6 (`Math.sumPrecise`, iterator `join`, `includes`, `zip`), native `fs.glob`, experimental `net/promises`, `--allow-env`, npm 12.1, OpenSSL 4.1, a default heap limit of half of physical memory with Buffer memory counted, `http` keep-alive 65 s, null-prototype `req.headers`, `net` throwing on a second `listen`.
- Breakages already met and fixed: the static file server (`corser`; commit `5fb1895a1`) and SQLite 3.54.1 JSON subqueries (`libs/typescript/data/database/sqlite.ts` wraps them in `json()`).
- No tool in the serve-and-test path runs its own CI on Node 27 except `ws`; Storybook's CI runs Node 22 only (`node-27-toolchain-audit.md`).
- Not measured: a Node 26 against Node 27 timing comparison, and the effect of the V8 heap cap.

Not done: all of it. The hand-typed row is still there.

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
- **Renovate.** `RENOVATE_HOST_RULES` is in Doppler `ci` and synced to GitHub (owner, 2026-11-12). Whether the `git-submodules` manager now moves the `assets` pin has not been observed in a run.
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

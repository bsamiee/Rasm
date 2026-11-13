# Remaining work

### Not done

1. **Linux proof.** Nothing of the Linux side ran on this Mac: the toolchain file, the mise tools on the image's Ubuntu 22.04, and job `unreal`. Its proof is the first CI run holding the job.
2. **Runner.** Job `unreal` runs on hosted `ubuntu-26.04` and pulls the 22 GiB image on each run. `derby` (8 vCPU, 31 GiB memory, 385 GB free, no service) can hold a self-hosted runner that keeps the image; `a-cicd-vps.md` `WORK` rows 1 to 4 state that form and its fork-approval setting.
3. **Fab.** Fab takes no upload from a tool, and no Fab publisher profile exists. After a release holds `Tolerance-<engine>.zip`, the owner creates the profile and a draft listing from that link, where Fab's upload scan answers. "Submit for review" then has Fab's build farm compile it, and "Cancel submission" stops it before a human review.
4. **One platform per source archive.** A pack on a Mac holds the `Mac` library and one on Linux the `Linux` library. An archive holding both needs the installed folders of both jobs.
5. **Fab rules the source archive can fail.** The installed `rasm/tolerance.hpp` holds no copyright notice (Fab rule 4.3.6.1.b names every header), the plug-in holds no `Content` folder and no `Resources/Icon128.png`, and its modules allow `Mac` and `Linux` alone. The draft listing and the cancelled submission of item 3 answer each.
6. **Toolchain and triplet files of presets other than `default` are no Nx input.** `@rasm/nx-cmake` reads inputs from the file API reply of its first workflow, so an edit to `unreal-Darwin.cmake`, `unreal-Linux.cmake`, or `triplets/unreal.cmake` leaves `Tolerance:build--unreal` cached. Choice not made: configure every workflow at graph creation, or another source for those files.
7. **`prose.py fix` deletes the Fab notice.** The clean-prose script reports the `// Copyright` line of each `.Build.cs` as a comment to remove, and `pack--unreal` then fails its store check. No predicate for a notice a host requires exists.

## 2. Node 27 alignment pass

Owner ruling, verbatim: "remove the handtyped 2nd version - just fully align to alpha? but keep this as last so we fix all other things - but we can deep mine all new things it enables + allows us to do and other tools to move with it and their capabilities to fully land on that + ensur no mixed paradimgs we rebuild things to that".

Done on 2026-11-12, uncommitted:
- `.config/mise.lock` holds Node `27.0.0-alpha.0.1.0`.
- App images install the version of the Node applying their stack through mise in a build stage of the Dockerfile `infra/hosting-stack.ts` states inline and copy `node` and `libatomic.so.1` onto the distroless nonroot base, so Mac, CI, the Atelier helper, and containers run one nodejs.org build.
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

---
description: Full-estate dependency freshening — every version-owner manifest to newest, every .api catalog and libs/ consumer integrated, zero stale residue
disable-model-invocation: true
---

# [FRESHEN]

One routine moves every ecosystem to its newest truthful state and integrates the delta to the estate bar: the manifest legs land the upgrades with their proof sweeps, the bump ledger partitions into `freshness-integrator` dispatches, majors run investigate-then-implement, and the close proves zero stale residue. Judgment stays with the orchestrator — holds, channel choices, and semver traps adjudicate here, never inside a leg script. Concurrency law: at most 14 agents in flight; launch as slots open, never in one burst.

## [01]-[UPGRADE_LEGS]

Every leg is one contract — a sole version-owner manifest, direct registry probes, an in-place rewrite, a proof sweep, a bump list from the manifest diff; a new ecosystem lands as one more leg under this contract. Run the legs concurrently, each backgrounded; a leg's bump list extracts only after its proof sweep passes, and a failed leg walks its offending pins back under hold law and closes on what proved — sibling legs and the dispatch phase proceed on whichever bump lists landed. Registry truth outranks any cached advisory tool.

- CHANNEL LAW: a stable pin takes newest stable; a prerelease pin stays on its own channel (`canary`/`next`/`dev`/`beta`) and takes that channel's newest — a channel pin never falls back to stable.
- HOLD LAW: a held pin — whichever carrier spells the hold: an inline hold comment, a pyproject bound or `python_version` marker, a channel lock — stays held only while its named blocker stands; re-probe the blocker every run, and a lifted hold deletes its carrier in the same edit.

[PYTHON] — the venv mutates only in a quiet window (no live agent reads):
1. `forge-scientific-env uv lock --upgrade` — Forge's wrapper carries the native toolchain sdist metadata builds need; bare `uv` dies at the first sdist. A resolution failure pins the named offender under hold law (pyproject bound with its evidence comment) and re-locks.
2. `forge-scientific-env uv sync` — its parallelism governor caps the compile fan.
3. DEAD-DYLIB SWEEP (mandatory — `uv sync` reuses cached wheels built against Nix store paths a Forge flake bump has since moved): every native under site-packages checks its linked `/nix/store/*.dylib` paths; each missing path names its owning dist, the owners rebuild in one `forge-scientific-env uv pip install --reinstall --no-cache <dists>`, and the sweep re-runs after each repair, closing at zero missing.
- A gfortran-linked failure (`--ld-path` rejection) is a Forge wrapper defect — fix in `Parametric_Forge/modules/home/programs/languages/scientific-tools.nix`, `forge-redeploy --switch`, re-run the rebuild.
- A path still missing after its dist's `--no-cache` rebuild links a store path the flake no longer carries — a Forge library row (`scientific-tools.nix`, `forge-redeploy --switch`), never a third blind rebuild.

```bash copy-safe
sp="$(.venv/bin/python -c 'import site; print(site.getsitepackages()[0])')"
fd -e so -e dylib . "$sp" -u | while read -r so; do
  otool -L "$so" 2>/dev/null | rg -o '/nix/store/[^ ]+\.dylib' | while read -r lib; do
    [ -e "$lib" ] || echo "MISSING $lib <- $so"; done; done | sort -u
```

1. BUMP LIST — name/old/new pairs from the lock diff, written to the ledger dir first so the proof consumes it:

```bash copy-safe
mkdir -p ".claude/scratch/freshen-$(date +%F)"
git diff -U2 uv.lock | awk '
/^[ +-]?name = / { gsub(/.*name = "|"$/,""); n=$0; old="" }  # reset: a removed package never pairs onto an added name
/^-version = / { gsub(/.*version = "|"$/,""); old=$0 }
/^\+version = / { gsub(/.*version = "|"$/,""); if (old!="") { printf "%s %s -> %s\n", n, old, $0; old="" } }' \
  > ".claude/scratch/freshen-$(date +%F)/bumps-python.txt"
```

5. PROOF — import every bumped installed dist's top-level modules (a marker-gated dist skips; an import failure names its dist for the repair loop), then `uv run --no-sync python -m tools.assay api status`:

```bash copy-safe
uv run --no-sync python - ".claude/scratch/freshen-$(date +%F)/bumps-python.txt" <<'EOF'
import importlib, importlib.metadata as md, sys, pathlib
fails = []
for name in {l.split()[0] for l in pathlib.Path(sys.argv[1]).read_text().splitlines() if l.strip()}:
    try: dist = md.distribution(name)
    except md.PackageNotFoundError: continue
    tops = (dist.read_text("top_level.txt") or name.replace("-", "_")).split()
    for t in tops:
        try: importlib.import_module(t)
        except ModuleNotFoundError: pass
        except Exception as e: fails.append(f"{name}:{t}: {e}")
print("\n".join(fails) or "all bumped dists import clean")
EOF
```

[TYPESCRIPT] — `pnpm-workspace.yaml` `catalog:` is the sole version owner:
1. PROBE every catalog entry's dist-tags at `registry.npmjs.org/<pkg>`; pick per channel law.
2. Rewrite the catalog rows in place, then `pnpm install`.
3. PROOF: install exits clean with zero unresolved peers; peer-resolution failure walks the offending rows back under hold law (channel-lock comment) and re-proves. Any auto-grown `minimumReleaseAgeExclude` block deletes (the standing `minimumReleaseAge: 0` is the one-line gate-off) and `pnpm install` re-proves.
4. BUMP LIST: `git diff pnpm-workspace.yaml` catalog rows.

[CSHARP] — `Directory.Packages.props` + `.config/dotnet-tools.json` are the sole version owners:
1. PROBE every `PackageVersion` and tool id at `api.nuget.org/v3-flatcontainer/<id>/index.json` (lowercase id); pick per channel law.
2. SEMVER-INVERSION TRAP: any major jump verifies its publish date via `api.nuget.org/v3/registration5-gz-semver2/<id>/<ver>.json` (gzip body) — a "newest" older than the current pin's date is a dead line, held with a hold comment naming the trap.
3. Rewrite versions preserving alignment and comments, then `dotnet restore Workspace.slnx --force-evaluate` (regenerates `packages.lock.json`) and `dotnet tool restore`.
4. PROOF: restore exits 0 with zero `NU` warnings; a resolution failure walks back the error's named pin set as one batch under one shared hold comment naming the blocker, then re-proves — an interdependent cluster (a major dragging its siblings) holds as one unit.
5. BUMP LIST: `git diff Directory.Packages.props .config/dotnet-tools.json`.

## [02]-[DISPATCH]

Map each bump to its owning `.api` catalogs (search catalog content for the package id — filenames alone miss multi-tier owners) and to its consuming `libs/` pages: consumer search terms derive from the owning catalog's `[01]-[PACKAGE_SURFACE]` fields (`module:`/`namespaces:`/`assembly:`), never the bare package id — a namespace and its package spell differently. A bump with no catalog and no consumer records in the ledger and dispatches nothing.

- MINOR/PATCH: one `freshness-integrator` per 4 bumps, grouped by tier/domain so sibling-seam reads stay cheap.
- MAJOR (or structural — a package split, an engine jump, a channel move): one `freshness-integrator` solo, investigation depth.
- Each dispatch prompt carries: package(s) with exact old -> new spans, the owning catalog paths, the known consumer-page set, the changelog source repo, and the assay verification key (`py:<dist>` | `nuget:<Id>` | `npm:<pkg>` | `host:<assembly>` — a new ecosystem rides its own `--key` scope). Everything else is the agent's standing law.
- A major's return includes a RIPPLE roster — integration points its investigation proved but did not land. Adjudicate each row here; an accepted cluster dispatches one focused `freshness-integrator` with the roster rows as its findings, and a ripple dispatch terminates the chain — second-generation discoveries card as IDEAS/TASKLOG rows, never a new roster.
- Write territories — catalogs AND consumer pages — partition by dispatch: two groups sharing any file merge or serialize, and a shared substrate catalog (numpy-grade) goes solo before its dependents' groups launch.
- A dispatch that dies or returns partial re-dispatches fresh with the same round data — the completion bar is state-shaped (catalog current against the installed version), so a re-run converges over partial edits.

## [03]-[CLOSE]

1. Drain all dispatches; adjudicate every RIPPLE roster to done or a carded IDEAS/TASKLOG row.
2. RESIDUE PROOF: `rg` the estate for every member the dispatch reports' removed/purged rosters name and for "blocked until"/wheel-gate prose naming packages the wave moved — zero hits on resolved facts; a hit fixes at its owner.
3. GATES, batched once: `uv run --no-sync python -m tools.assay docs check` over every touched markdown; the polyglot build proof through `tools.assay static --all`; `pnpm install` and `dotnet restore Workspace.slnx` idempotency (clean second run).
4. LEDGER: `.claude/scratch/freshen-<YYYY-MM-DD>/` carries the full bump table with holds and their reasons — reasons feed the next run's hold law.
5. `/snapshot` seals the landed state.

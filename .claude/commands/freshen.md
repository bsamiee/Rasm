---
description: Full-estate dependency freshening — every version-owner manifest to newest, every .api catalog and libs/ consumer integrated, zero stale residue
disable-model-invocation: true
---

# [FRESHEN]

Manifest legs align every version-owner manifest to registry truth; the bump ledger dispatches catalog and consumer integration, and closeout proves zero stale residue. Majors run investigate-then-implement. Orchestrator alone adjudicates holds, channel choices, and semver traps. Keep at most 14 agents in flight; launch as slots open.

## [01]-[UPGRADE_LEGS]

Each leg owns its version-owner set: direct registry probes, in-place rewrite, proof, then manifest-diff bump extraction; new ecosystems use this shape. Launch legs concurrently in the background. Failed legs apply hold law to offending pins and close on proved sets; siblings and dispatch continue from landed bump lists. Registry results override cached advisories.

- CHANNEL LAW: Stable pins take newest stable; `canary`/`next`/`dev`/`beta` pins take their channel's newest without stable fallback.
- HOLD CARRIERS: Inline comments, pyproject bounds, `python_version` markers, and channel locks.
- HOLD LAW: Carriers persist while named blockers stand; re-probe blockers each run and delete lifted carriers in the same edit.

[PYTHON] — The venv mutates only in a quiet window (no live agent reads):
1. `forge-scientific-env uv lock --upgrade` supplies sdist tooling; resolution failures hold offenders with evidenced pyproject bounds, then re-lock.
2. `forge-scientific-env uv sync` — Its parallelism governor caps the compile fan.
3. DEAD-DYLIB SWEEP (mandatory: `uv sync`): Site-packages check linked `/nix/store/*.dylib` paths; missing paths names owning dist, owners rebuild in one `forge-scientific-env uv pip install --reinstall --no-cache <dists>`, the sweep re-runs after each repair, closing at zero missing.
- GFORTRAN TRAP: `--ld-path` rejection identifies a Forge wrapper defect.
- GFORTRAN REPAIR: Fix `Parametric_Forge/modules/home/programs/languages/scientific-tools.nix`, run `forge-redeploy --switch`, then rebuild.
- MISSING-PATH TRAP: Store paths still missing after their dist's `--no-cache` rebuild prove the flake lacks them.
- MISSING-PATH REPAIR: Add each Forge library row in `scientific-tools.nix`, then run `forge-redeploy --switch`.

```bash copy-safe
sp="$(.venv/bin/python -c 'import site; print(site.getsitepackages()[0])')"
fd -e so -e dylib . "$sp" -u | while read -r so; do
  otool -L "$so" 2>/dev/null | rg -o '/nix/store/[^ ]+\.dylib' | while read -r lib; do
    [ -e "$lib" ] || echo "MISSING $lib <- $so"; done; done | sort -u
```

4. BUMP LIST — Name/old/new pairs from the lock diff, written to the ledger dir first so the proof consumes it:

```bash copy-safe
mkdir -p ".claude/scratch/freshen-$(date +%F)"
git diff -U2 uv.lock | awk '
/^[ +-]?name = / { gsub(/.*name = "|"$/,""); n=$0; old="" }  # reset: a removed package never pairs onto an added name
/^-version = / { gsub(/.*version = "|"$/,""); old=$0 }
/^\+version = / { gsub(/.*version = "|"$/,""); if (old!="") { printf "%s %s -> %s\n", n, old, $0; old="" } }' \
  > ".claude/scratch/freshen-$(date +%F)/bumps-python.txt"
```

5. PROOF — Run imports below, then `uv run --no-sync python -m tools.assay api status`; marker-gated dists skip; failures name dists for repair.

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
2. Rewrite The catalog rows in place, then `pnpm install`.
3. PROOF: `pnpm install` exits clean with zero unresolved peers.
4. FAILURE: Roll offending rows back under hold law with a channel-lock comment, then re-prove.
5. AGE-GATE CLEANUP: Delete auto-grown `minimumReleaseAgeExclude`; `minimumReleaseAge: 0` disables the gate; rerun `pnpm install`.
6. BUMP LIST: `git diff pnpm-workspace.yaml` catalog rows.

[CSHARP] — `Directory.Packages.props` + `.config/dotnet-tools.json` are the sole version owners:
1. PROBE every `PackageVersion` and tool id at `api.nuget.org/v3-flatcontainer/<id>/index.json` (lowercase id); pick per channel law.
2. SEMVER-INVERSION PROBE: Query `api.nuget.org/v3/registration5-gz-semver2/<id>/<ver>.json` for each major's publish date (gzip body).
3. SEMVER-INVERSION HOLD: Hold candidates older than current pins with comments naming the trap.
4. Rewrite versions preserving alignment and comments.
5. Run `dotnet restore Workspace.slnx --force-evaluate` to regenerate `packages.lock.json`, then `dotnet tool restore`.
6. PROOF: Restore exits 0 with zero `NU` warnings.
7. FAILURE: Roll back the error's named pin set as one batch under one shared hold comment naming the blocker, then re-prove.
8. CLUSTER LAW: Interdependent pin clusters hold as one unit.
9. BUMP LIST: `git diff Directory.Packages.props .config/dotnet-tools.json`.

## [02]-[DISPATCH]

Catalog content keyed by package id resolves each bump's owning `.api` catalogs across tiers. Catalog `[01]-[PACKAGE_SURFACE]` fields (`module:`, `namespaces:`, `assembly:`) drive `libs/` consumer searches. Record bumps with neither catalog nor consumer in the ledger; dispatch only mapped bumps.

- MINOR/PATCH: One `freshness-integrator` per 4 bumps, grouped by tier/domain so sibling-seam reads stay cheap.
- MAJOR (or structural — a package split, an engine jump, a channel move): One `freshness-integrator` solo, investigation depth.
- DISPATCH INPUT: Exact package `old -> new` spans, owning catalog paths, known consumer pages, and changelog source repo.
- VERIFICATION KEY: `py:<dist>` | `nuget:<Id>` | `npm:<pkg>` | `host:<assembly>`; each ecosystem owns its `--key` scope.
- RIPPLE RETURN: Majors report proved, unlanded integration points as `RIPPLE` rows.
- RIPPLE ADJUDICATION: Prchestrator adjudicates each row; accepted rows cluster into one focused `freshness-integrator` findings dispatch.
- RIPPLE TERMINUS: That dispatch closes the chain; later discoveries land as `IDEAS.md`/`TASKLOG.md` cards.
- WRITE TERRITORIES: Dispatches partition catalog and consumer-page writes; overlapping groups merge or serialize.
- SUBSTRATE ORDER: Shared substrate catalogs run solo before dependent groups.
- DISPATCH RETRY: Failed or partial dispatches re-dispatch fresh with the same round data.
- DISPATCH COMPLETION: Catalogs match installed versions; retries converge over partial edits.

## [03]-[CLOSE]

1. Drain all dispatches; adjudicate every RIPPLE roster to done or a carded IDEAS/TASKLOG row.
2. RESIDUE PROOF: Estate-wide `rg` finds no removed/purged member or moved-package `blocked until`/wheel-gate claim; repair each hit at its owner.
3. DOCS GATE: Run once over touched markdown with `uv run --no-sync python -m tools.assay docs check`.
4. STATIC GATE: Run the polyglot build proof once through `tools.assay static --all`.
5. IDEMPOTENCY: Rerun `pnpm install` and `dotnet restore Workspace.slnx`; both exit clean without changes.
6. LEDGER: `.claude/scratch/freshen-<YYYY-MM-DD>/` carries the full bump table with holds and their reasons — reasons feed the next run's hold law.

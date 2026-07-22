---
include:
  - "tools/rhino-bridge/**"
  - "tests/csharp/tools/**"
  - "tests/csharp/scenarios/**"
---

# [RHINO_BRIDGE]

Rhino-bridge carries live-host scenario proof; its Contract is the frozen wire and its Supervisor fold is the verdict authority.

- `tools/rhino-bridge/Contract` is additive-only: renaming or reusing an existing field, union discriminator, status rank, or exit code is a finding.
- A `[RhinoScenario]` entrypoint takes one `ScenarioContext`, returns `Fin<Unit>`, and emits through the typed evidence surface (`Fact`/`Note`/`Require`/`Expect`/`Certify`/`Capture.Snapshot`); scenario code carrying `#r`, `#load`, absolute build-output paths, local report paths, or direct capture files is a finding, as is capability access unbacked by a `Requires` probe.
- `--evidence author` output is candidate material, never proof: a committed `.reference.json` whose `admission` was not human-promoted, or an author-mode run treated as passing, is a finding.
- Interactive MCP host access stays idle during any bridge-held lifecycle — an interleaved probe contaminates the same command-history and spool evidence the cargo runner records; bridge writes land only under `.artifacts/assay/bridge/<runId>/` or `~/.rasm/`.
- Bridge protocol changes land with their `Contract`/`Supervisor` suite change under `tests/csharp/tools/rhino-bridge`, or they do not land.

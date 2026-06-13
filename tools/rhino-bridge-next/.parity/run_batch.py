#!/usr/bin/env python3
"""U11 parity batch: drive the OLD frozen client directly under assay's bridge flock lease.

One live host session runs the full OLD .verify.csx corpus (21 files) followed by the NEW
[RhinoScenario] corpus through the 10 reflection drivers, then quits via the Apple-Event ladder.
The assay `bridge verify` rail is bypassed because its `_build_closure` routing faults pre-spawn
("incoherent closure: 4 tails for one check"); the client itself is the verbatim execute lane.
"""

import fcntl
import json
import os
import pathlib
import subprocess
import sys
import time

ROOT = pathlib.Path(__file__).resolve().parents[3]
CLIENT = ROOT / "tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj"
LOCK = ROOT / ".artifacts/assay/locks/bridge.lock"
REPORT = ROOT / ".artifacts/rhino-bridge-next/parity"
ENV = {**os.environ, "RHINO_WIP_APP_PATH": "/Applications/RhinoWIP.app"}

PROJECTS = {
    "tests/csharp/libs/Rasm/": "tests/csharp/libs/Rasm/Rasm.Tests.csproj",
    "tests/csharp/libs/Rasm.Rhino/": "tests/csharp/libs/Rasm.Rhino/Rasm.Rhino.Tests.csproj",
    "tests/csharp/libs/Rasm.Grasshopper/": "tests/csharp/libs/Rasm.Grasshopper/Rasm.Grasshopper.Tests.csproj",
    "tools/rhino-bridge-next/.parity/": "tests/csharp/_testkit/Rasm.TestKit.csproj",
}


def project_for(scenario: pathlib.Path) -> pathlib.Path:
    rel = scenario.relative_to(ROOT).as_posix()
    for prefix, project in PROJECTS.items():
        if rel.startswith(prefix):
            return ROOT / project
    raise SystemExit(f"no project mapping for {rel}")


def client(*args: str, timeout: float) -> subprocess.CompletedProcess:
    return subprocess.run(
        ["dotnet", "run", "--no-build", "--project", str(CLIENT), "--configuration", "Release", "--", *args],
        env=ENV, cwd=ROOT, check=False, timeout=timeout, capture_output=True, text=True,
    )


def run_scenario(scenario: pathlib.Path) -> dict:
    stem = scenario.name.removesuffix(".verify.csx")
    result_path = REPORT / f"{stem}.json"
    started = time.monotonic()
    try:
        done = client("check", str(project_for(scenario)), str(scenario), "--result", str(result_path), timeout=300.0)
        rc = done.returncode
        note = ""
    except subprocess.TimeoutExpired:
        rc, note = -1, "timeout"
    elapsed = round((time.monotonic() - started) * 1000)
    row = {"stem": stem, "rc": rc, "ms": elapsed, "status": "missing-result", "fault_phase": "", "fault": note}
    if result_path.is_file():
        data = json.loads(result_path.read_text())
        row["status"] = data.get("status", "unknown")
        for phase in data.get("phases", []):
            if phase.get("status") not in ("ok", "skipped"):
                row["fault_phase"] = phase.get("phase", "")
                fault = phase.get("fault") or {}
                row["fault"] = str(fault.get("message", ""))[:300]
                break
    print(f"[{row['status']:>8}] {stem} ({elapsed} ms) {row['fault_phase']} {row['fault'][:120]}", flush=True)
    return row


def main() -> int:
    REPORT.mkdir(parents=True, exist_ok=True)
    old = sorted((ROOT / "tests/csharp").rglob("*.verify.csx"))
    new = sorted((ROOT / "tools/rhino-bridge-next/.parity").glob("*.verify.csx"))
    print(f"old corpus: {len(old)} files; new drivers: {len(new)} files", flush=True)

    fd = os.open(LOCK, os.O_RDWR | os.O_CREAT, 0o644)
    try:
        fcntl.flock(fd, fcntl.LOCK_EX | fcntl.LOCK_NB)
    except BlockingIOError:
        print("bridge lease busy", file=sys.stderr)
        return 5
    rows: dict[str, list[dict]] = {"old": [], "new": []}
    try:
        launch = client("launch", timeout=240.0)
        print(f"launch rc={launch.returncode}", flush=True)
        if launch.returncode != 0:
            print(launch.stdout[-2000:], launch.stderr[-2000:], file=sys.stderr)
            return 1
        for scenario in old:
            rows["old"].append(run_scenario(scenario))
        for scenario in new:
            rows["new"].append(run_scenario(scenario))
    finally:
        quit_done = client("quit", timeout=180.0)
        print(f"quit rc={quit_done.returncode}", flush=True)
        os.ftruncate(fd, 0)
        fcntl.flock(fd, fcntl.LOCK_UN)
        os.close(fd)
    (REPORT / "batch-summary.json").write_text(json.dumps(rows, indent=1))
    ok_old = sum(1 for r in rows["old"] if r["status"] == "ok")
    ok_new = sum(1 for r in rows["new"] if r["status"] == "ok")
    print(f"OLD {ok_old}/{len(rows['old'])} ok; NEW {ok_new}/{len(rows['new'])} ok", flush=True)
    return 0 if ok_old == len(rows["old"]) and ok_new == len(rows["new"]) else 1


if __name__ == "__main__":
    raise SystemExit(main())

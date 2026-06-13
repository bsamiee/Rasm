#!/usr/bin/env python3
"""U4 live bracket: hold the assay bridge flock for the WHOLE launch->probe->quit sequence.

argv[1] selects the bracket: 'healthy' (default) or 'poison'.
Drives Rhino via the OLD frozen client (launch/quit verbs); probes the NEW shell directly.
"""

import fcntl
import json
import os
import shutil
import subprocess
import sys
import time

ROOT = "/Users/bardiasamiee/Documents/99.Github/Rasm"
LOCK = os.path.join(ROOT, ".artifacts/assay/locks/bridge.lock")
CLIENT = os.path.join(ROOT, "tools/rhino-bridge/client/Rasm.RhinoBridge.Client.csproj")
PROBE = os.path.join(ROOT, "tools/rhino-bridge-next/.probes/shell-probe/ShellProbe.csproj")
RBX_ENDPOINT = os.path.expanduser("~/.rasm/rhino-bridge-rbx.json")
DEPLOY = os.path.expanduser(
    "~/Library/Application Support/McNeel/Rhinoceros/packages/9.0/rasm-bridge-next/1.0.0"
)
SHELL_DLL = os.path.join(DEPLOY, "Rasm.Bridge.Shell.dll")
ENV = {**os.environ, "RHINO_WIP_APP_PATH": "/Applications/RhinoWIP.app"}


def old_client(verb: str, timeout: float) -> int:
    print(f"[bracket] old client {verb}", file=sys.stderr, flush=True)
    done = subprocess.run(
        ["dotnet", "run", "--no-build", "--project", CLIENT, "--configuration", "Release", "--", verb],
        env=ENV, cwd=ROOT, check=False, timeout=timeout, capture_output=True, text=True,
    )
    print(f"[bracket] {verb} rc={done.returncode}", file=sys.stderr, flush=True)
    return done.returncode


def wait_rbx(deadline_s: float, want_fault: bool) -> dict | None:
    start = time.monotonic()
    while time.monotonic() - start < deadline_s:
        try:
            record = json.load(open(RBX_ENDPOINT))
            if want_fault and record.get("fault"):
                return record
            if not want_fault and record.get("pipeName", "").startswith("rbx-"):
                return record
        except (OSError, json.JSONDecodeError):
            pass
        time.sleep(1.0)
    return None


def main() -> int:
    bracket = sys.argv[1] if len(sys.argv) > 1 else "healthy"
    fd = os.open(LOCK, os.O_RDWR | os.O_CREAT, 0o644)
    try:
        fcntl.flock(fd, fcntl.LOCK_EX | fcntl.LOCK_NB)
    except BlockingIOError:
        print("lease busy", file=sys.stderr)
        return 5
    try:
        if bracket == "poison":
            # Evidence-first: remove the shell assembly so the stub's activation hop fails and
            # writes the poisoned endpoint record; restore afterwards.
            try:
                os.remove(RBX_ENDPOINT)  # the bracket regenerates its own evidence
            except FileNotFoundError:
                pass
            shutil.move(SHELL_DLL, SHELL_DLL + ".hidden")
            try:
                if old_client("launch", 180) != 0:
                    print("[bracket] launch failed", file=sys.stderr)
                record = wait_rbx(60, want_fault=True)
                print(json.dumps({"bracket": "poison", "poisonedEndpoint": record}, indent=2), flush=True)
            finally:
                rc_quit = old_client("quit", 120)
                shutil.move(SHELL_DLL + ".hidden", SHELL_DLL)
                print(f"[bracket] quit rc={rc_quit}", file=sys.stderr, flush=True)
            return 0 if record else 1

        if old_client("launch", 180) != 0:
            print("[bracket] launch failed", file=sys.stderr)
            return 1
        record = wait_rbx(90, want_fault=False)
        if record is None:
            print("[bracket] rbx endpoint never went live", file=sys.stderr)
            _ = old_client("quit", 120)
            return 1
        print(f"[bracket] rbx live: {record['pipeName']}", file=sys.stderr, flush=True)
        time.sleep(2.0)
        probe = subprocess.run(
            ["dotnet", "run", "--no-build", "-c", "Release", "--project", PROBE],
            env=ENV, cwd=ROOT, check=False, timeout=300, capture_output=True, text=True,
        )
        print(probe.stdout, flush=True)
        print(probe.stderr, file=sys.stderr, flush=True)
        rc_quit = old_client("quit", 120)
        print(f"[bracket] probe rc={probe.returncode} quit rc={rc_quit}", file=sys.stderr, flush=True)
        return probe.returncode
    finally:
        os.ftruncate(fd, 0)
        fcntl.flock(fd, fcntl.LOCK_UN)
        os.close(fd)


if __name__ == "__main__":
    sys.exit(main())

#!/usr/bin/env python3
"""SessionStart and CwdChanged hook, writes the mise environment of the working directory to CLAUDE_ENV_FILE for every Bash command.

Stdlib only, every outcome exits 0, and a skipped write states its reason on stderr.
"""

import json
import os
import pathlib
import subprocess
import sys

# --- [CONSTANTS] ------------------------------------------------------------------------

_ENV_FILE = "CLAUDE_ENV_FILE"
_MAX_PAYLOAD = 8 * 1024 * 1024
_TIMEOUT = 20.0

# --- [OPERATIONS] -----------------------------------------------------------------------


def _export(cwd: str) -> tuple[str, str]:
    """Return the bash export script of the mise environment at a directory, or the empty script with the reason mise gave none."""
    completed = subprocess.run(["mise", "env", "-s", "bash"], cwd=cwd, capture_output=True, text=True, timeout=_TIMEOUT, check=False)
    if completed.returncode:
        detail = next((line for line in completed.stderr.splitlines() if line.strip()), f"exit status {completed.returncode}")
        return "", f"mise env failed in {cwd}, {detail}"
    return completed.stdout, ""


# --- [ENTRY] ----------------------------------------------------------------------------


def _persist() -> str:
    """Write the mise environment of the payload directory to the environment file and return the reason when nothing was written."""
    target = os.environ.get(_ENV_FILE, "")
    if not target:
        return f"{_ENV_FILE} is unset, the hook runs under SessionStart and CwdChanged alone"
    payload = json.loads(sys.stdin.buffer.read(_MAX_PAYLOAD))
    script, reason = _export(str(payload["cwd"]))
    if not reason:
        _ = pathlib.Path(target).write_text(script, encoding="utf-8")
    return reason


def main() -> int:
    """Return 0 after writing the environment file, or after stating on stderr why nothing was written."""
    try:
        reason = _persist()
    except Exception as failure:  # ruff:ignore[blind-except] -- the hook never blocks a session
        reason = f"the environment was not exported ({failure})"
    if reason:
        sys.stderr.write(f"mise-env: {reason}\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())

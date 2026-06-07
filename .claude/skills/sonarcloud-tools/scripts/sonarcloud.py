#!/usr/bin/env -S uv run --quiet --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["httpx"]
# ///
"""SonarCloud API CLI -- code quality metrics via REST API.

Commands:
    quality-gate [branch]           Quality gate status (or: quality-gate pr <num>)
    issues [severities] [types]     Search issues (e.g., BLOCKER,CRITICAL BUG)
    measures [metrics]              Project metrics (e.g., coverage,bugs)
    analyses [page_size]            Analysis history (default: 10)
    projects [page_size]            List organization projects (default: 100)
    hotspots [status]               Security hotspots (e.g., TO_REVIEW)
"""
# LOC: 150

from collections.abc import Callable, Mapping
import json
import os
import sys
from typing import Final

from _commands import analyses, hotspots, issues, measures, projects, quality_gate
import httpx


# --- [CONSTANTS] --------------------------------------------------------------
BASE_URL: Final = "https://sonarcloud.io/api"
KEY_ENV: Final = "SONAR_TOKEN"
TIMEOUT: Final = 30
HELP: Final = __doc__ or ""


# --- [TYPES] ------------------------------------------------------------------
type JsonMap = dict[str, object]
type QueryParams = dict[str, str | int]
type Command = Callable[[tuple[str, ...]], JsonMap]


# --- [FUNCTIONS] --------------------------------------------------------------
def _get(path: str, params: QueryParams) -> tuple[bool, JsonMap]:
    """GET request with bearer auth.

    Args:
        path: API endpoint path.
        params: Query parameters.

    Returns:
        Tuple of (success, response_data).
    """
    match os.environ.get(KEY_ENV, ""):
        case "":
            return False, {"error": f"Missing {KEY_ENV} environment variable"}
        case token:
            pass
    headers = {"Authorization": f"Bearer {token}"}
    with httpx.Client(timeout=TIMEOUT) as client:
        response = client.get(f"{BASE_URL}{path}", headers=headers, params=params)
        response.raise_for_status()
        match response.json():
            case Mapping() as data:
                return True, {str(key): value for key, value in data.items()}
            case _:
                return True, {}


def _emit(result: JsonMap) -> int:
    """Print one JSON command result.

    Returns:
        Process exit code for the result status.
    """
    sys.stdout.write(json.dumps(result, indent=2) + "\n")
    return 0 if result["status"] == "success" else 1


# --- [DISPATCH] ---------------------------------------------------------------
COMMANDS: Final[dict[str, Command]] = {
    "quality-gate": lambda args: quality_gate(args[0] if len(args) >= 1 else "", args[1] if len(args) >= 2 else "", _get),
    "issues": lambda args: issues(args[0] if len(args) >= 1 else "", args[1] if len(args) >= 2 else "", _get),
    "measures": lambda args: measures(args[0] if len(args) >= 1 else "", _get),
    "analyses": lambda args: analyses(args[0] if len(args) >= 1 else "", _get),
    "projects": lambda args: projects(args[0] if len(args) >= 1 else "", _get),
    "hotspots": lambda args: hotspots(args[0] if len(args) >= 1 else "", _get),
}


# --- [ENTRY_POINT] ------------------------------------------------------------
def main() -> int:
    """Dispatch command and print JSON output.

    Returns:
        Process exit code.
    """
    match sys.argv[1:]:
        case [cmd_name, *args] if command := COMMANDS.get(cmd_name):
            try:
                return _emit(command(tuple(args)))
            except httpx.HTTPStatusError as error:
                sys.stdout.write(json.dumps({"status": "error", "code": error.response.status_code, "message": error.response.text[:200]}) + "\n")
                return 1
            except httpx.RequestError as error:
                sys.stdout.write(json.dumps({"status": "error", "message": str(error)}) + "\n")
                return 1

        case [cmd_name, *_]:
            sys.stdout.write(f"[ERROR] Unknown command '{cmd_name}'\n\n")
            sys.stdout.write(HELP + "\n")
            return 1

        case _:
            sys.stdout.write(HELP + "\n")
            return 1


if __name__ == "__main__":
    sys.exit(main())

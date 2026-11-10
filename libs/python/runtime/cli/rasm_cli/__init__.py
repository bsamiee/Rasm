"""Process entry of a command-line tool: settings and per-user folders from flags and the tool's variables, each command line run inside the process telemetry scope, results as JSON lines, and fault exit statuses."""

lazy from rasm_cli._invocations import DEFAULTS, Folder, Invocation, invoked, Settings
lazy from rasm_cli._results import exited, EXITS

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DEFAULTS", "EXITS", "Folder", "Invocation", "Settings", "exited", "invoked"]

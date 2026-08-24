#!/usr/bin/env python3
"""Block a tool run that would write its cache into the repo root; the routed form and every other command pass untouched.

Wire: PreToolUse matcher "Bash|Monitor" in .claude/settings.json, exec form, beside git-guard. Stdlib only under a `python3`
shebang, which is a platform guarantee where uv is not: a uv shebang exits 127 off PATH and 1 on a cold cache, silently
retiring the gate. POLICY is the whole edit surface: one row per tool whose cache default is hardcoded upstream, one fold
over it. A row earns its seat only where the tool offers no config-file key — a tool with one is routed there instead, per
the config-first ladder in tests/README.md; the roster censuses two-way against that page's [ARTIFACT_ROUTING] litter-guard
rows at tests/python/_testkit/test_policy.py. Litter is cosmetic, so every failure path allows: a malformed payload, an
unlexable command, or a wrapper form the peel does not know costs a stray directory, where over-blocking costs real work.
"""

import dataclasses
import json
import pathlib
import re
import shlex
import sys


# --- [CONSTANTS] ------------------------------------------------------------------------

_CTRL = re.compile(r"[\x00-\x1f\x7f]+")  # scrub before model-supplied text reaches the terminal
_ENV_ASSIGN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*=")  # leading NAME=value strips so FOO=1 lint-imports still resolves

CACHE_ROOT = ".cache/"  # the routed prefix ruled by tests/README.md [ARTIFACT_ROUTING]
OPERATORS = frozenset((";", "&&", "||", "|", "&", "(", ")"))
SUBVERBS = frozenset(("run", "exec", "tool", "x"))  # a runner takes one before the real command word
TOOLS = ("Bash", "Monitor")  # Monitor runs shell commands and shares Bash's tool_input.command field
WRAPPERS = frozenset(("env", "command", "nice", "nohup", "stdbuf", "timeout", "uv", "uvx", "poetry", "hatch", "pdm", "rye", "npm", "npx", "pnpm"))

ADVICE = "Blocked by litter-guard: route the cache or run the owning assay rail."

# --- [POLICIES] -------------------------------------------------------------------------


@dataclasses.dataclass(frozen=True, slots=True)
class Rule:
    """One tool whose upstream cache default lands in the repo root: what it litters, what reroutes it, and the governed rail."""

    litters: str
    flag: str
    rail: str
    waivers: tuple[str, ...] = ()


POLICY: dict[str, Rule] = {  # POLICY: command word -> the root entry a bare run creates, and the flag that reroutes it
    "lint-imports": Rule(".import_linter_cache/", "--cache-dir", "assay static", waivers=("--no-cache",))
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def _peel(argv: list[str]) -> list[str]:
    """Return the argv a leaf really invokes once env prefixes, runner wrappers, and their subverbs and options are stripped."""
    while argv and (_ENV_ASSIGN.match(argv[0]) or pathlib.PurePosixPath(argv[0]).name in WRAPPERS):
        argv = argv[1:]
        while argv and (argv[0] in SUBVERBS or argv[0].startswith("-") or "=" in argv[0]):
            argv = argv[1:]
    return argv


def _leaves(command: str) -> list[list[str]]:
    """Return every argv leaf of a shell command under a quote-aware split on the operators that separate commands."""
    lexer = shlex.shlex(command, posix=True, punctuation_chars=";&|<>()\n\r")
    lexer.whitespace, lexer.whitespace_split = " \t\f\v", True
    out: list[list[str]] = []
    argv: list[str] = []
    for token in lexer:
        if token in OPERATORS or (token and all(c in ";&|\n\r" for c in token)):
            out, argv = ([*out, argv] if argv else out), []
        else:
            argv.append(token)
    return [*out, argv] if argv else out


def _operand(argv: list[str], flag: str) -> str | None:
    """Return the flag's operand in either the separated or the inline form, or None when the flag is absent."""
    inline = next((t.split("=", 1)[1] for t in argv if t.startswith(f"{flag}=")), None)
    hit = next((i for i, t in enumerate(argv) if t == flag), -1)
    return inline if inline is not None else (argv[hit + 1] if 0 <= hit < len(argv) - 1 else None)


def _verdict(argv: list[str]) -> str:
    """Return why one argv would litter the repo root under POLICY, or the empty string when it may run."""
    row = POLICY.get(pathlib.PurePosixPath(argv[0]).name) if argv else None
    if row is None or any(w in argv for w in row.waivers):
        return ""
    operand = _operand(argv, row.flag)
    if operand is None:
        return f"{argv[0]} without {row.flag} writes its cache to {row.litters} in the repo root; run `{row.rail}`, or pass {row.flag} {CACHE_ROOT}<tool>"
    return "" if operand.startswith(CACHE_ROOT) else f"{argv[0]} routes its cache to {operand}, outside {CACHE_ROOT}"


# --- [ENTRY] ----------------------------------------------------------------------------


def main() -> int:
    """Return the hook exit status: 2 with a stderr reason blocks the first littering leaf, 0 allows."""
    try:  # fail OPEN: a malformed payload or unlexable command never blocks work over a cosmetic stray directory
        payload = json.loads(sys.stdin.buffer.read() or b"{}")
        if payload["tool_name"] not in TOOLS:
            return 0
        reason = next((r for leaf in _leaves(str(payload["tool_input"]["command"])) if (r := _verdict(_peel(leaf)))), "")
    except Exception:  # ruff:ignore[blind-except] -- a fail-open gate seam must be total, never a tuple
        return 0
    if reason:
        sys.stderr.write(f"litter-guard: {_CTRL.sub(' ', reason)}. {ADVICE}\n")
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())

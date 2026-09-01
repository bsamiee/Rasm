#!/usr/bin/env python3
"""PreToolUse gate over Bash and Monitor: exit 2 blocks the git leaf POLICY names destructive, anything else passes.

Stdlib only: a non-2 exit fails OPEN, and a uv shebang exits 127 off PATH. POLICY is the sole edit surface.
"""

from collections.abc import Callable
import dataclasses
import json
import pathlib
import re
import shlex
import sys

# --- [TYPES] ----------------------------------------------------------------------------

type _Probe = Callable[[list[str], str], str]

# --- [CONSTANTS] ------------------------------------------------------------------------

_INTERP = "<inline-interpreter>"
_BACKTICK = re.compile(r"`([^`]*)`")
_CONTINUE = re.compile(r"\\\n")
_CTRL = re.compile(r"[\x00-\x1f\x7f]+")
_HEREDOC = re.compile(r"<<-?\s*(['\"])(\w+)\1\n.*?^\t*\2$", re.DOTALL | re.MULTILINE)  # Quoted delimiter: the body expands nothing
_GIT_WORD = re.compile(r"\bgit\b")
_IFS = re.compile(r"\$\{IFS[^}]*\}|\$IFS")
_ENV_ASSIGN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*=")
_MAX_DEPTH = 8
_MAX_COMMAND = 128 * 1024
_MAX_PAYLOAD = 8 * 1024 * 1024
_ANY_ARG = ("",)
_CHECKOUT_CREATE = ("-b", "--orphan", "-t", "--track", "--detach")
_GIT_VALUE_OPTS = ("-C", "-c", "--git-dir", "--work-tree", "--namespace", "--config-env", "--exec-path")
_INLINE_FLAGS = ("--eval", "--command", "--print", "--execute")
_INTERPRETERS = ("python", "node", "ruby", "perl")
_SHELLS = ("sh", "bash", "zsh", "dash", "ksh", "eval")
_SUBVERBS = ("run", "exec", "tool", "x")
_TOOLS = ("Bash", "Monitor")
_VALUE_OPTS = ("-u", "-I", "-n", "-g", "--user", "--replace")
_RUNNERS = frozenset(("uv", "npm", "npx", "pnpm", "poetry", "hatch"))
_WRAPPERS = frozenset(("sudo", "doas", "env", "command", "nice", "nohup", "stdbuf", "timeout", "xargs", "caffeinate", "arch", "setsid")) | _RUNNERS
_ADVICE = "Blocked by git-guard: destructive git actions are disabled. Keep all work as-is."

# --- [POLICIES] -------------------------------------------------------------------------


def _allow(_args: list[str], _cwd: str) -> str:
    """Return the empty verdict: a row whose flag and prefix sets already decided needs no refinement."""
    return ""


def _reset(args: list[str], cwd: str) -> str:
    """Return why a reset moves HEAD off the branch tip, or the empty string for an index-only unstage."""
    targets = [t for t in args if not t.startswith("-")]
    if "--" in args or not targets or any(pathlib.Path(cwd or ".", t).exists(follow_symlinks=False) for t in targets):
        return ""
    return f"git reset {targets[0]} moves HEAD and drops commits from the branch"


def _restore(args: list[str], _cwd: str) -> str:
    """Return why a restore touches the working tree, or the empty string for an index-only --staged."""
    staged = any(t in ("-S", "--staged") or (t[:1] == "-" and t[:2] != "--" and "S" in t) for t in args)
    worktree = any(t in ("-W", "--worktree") or (t[:1] == "-" and t[:2] != "--" and "W" in t) for t in args)
    return "" if staged and not worktree else "git restore overwrites working-tree files"


def _pathspec(args: list[str], cwd: str) -> str:
    """Return why a checkout would overwrite working-tree files, or the empty string for a pure ref move."""
    if "--" not in args and any(t in _CHECKOUT_CREATE for t in args):
        return ""
    targets = [t for t in args if t == "-" or not t.startswith("-")]
    if "--" in args or len(targets) > 1 or targets[:1] == ["."] or (targets and targets[0].startswith(":")):
        return "git checkout with a pathspec overwrites working-tree files"
    if not targets or targets == ["-"]:
        return ""
    probe = pathlib.Path(cwd or ".", targets[0])
    return f"git checkout {targets[0]} names an existing path and would overwrite it" if probe.exists(follow_symlinks=False) else ""


@dataclasses.dataclass(frozen=True, slots=True)
class Rule:
    """One git subcommand path's destructive surface: exact flags, flag prefixes, and a filesystem refinement."""

    why: str
    flags: tuple[str, ...] = ()
    starts: tuple[str, ...] = ()
    safe: tuple[str, ...] = ()
    probe: _Probe = _allow


POLICY: dict[str, Rule] = {
    _INTERP: Rule("invokes git inside an opaque one-liner; run git directly", starts=_ANY_ARG),
    "branch": Rule("deletes or force-moves a branch", flags=("-d", "-D", "-M", "--delete"), starts=("--force",)),
    "checkout": Rule("discards local changes", flags=("-f", "-B", "-p", "--patch", "--ours", "--theirs"), starts=("--force",), probe=_pathspec),
    "clean": Rule("deletes untracked files", starts=_ANY_ARG),
    "config": Rule("defines a git alias that can smuggle a blocked verb", starts=("alias.",)),
    "push": Rule("rewrites or deletes remote history", flags=("-f", "-d", "--delete", "--mirror", "--prune"), starts=("--force", "+", ":")),
    "rebase": Rule("rewrites commits other agents may already hold", starts=_ANY_ARG),
    "reflog delete": Rule("erases reflog entries, the last recovery path", starts=_ANY_ARG),
    "reflog drop": Rule("erases reflog entries, the last recovery path", starts=_ANY_ARG),
    "reflog expire": Rule("erases reflog entries, the last recovery path", starts=_ANY_ARG),
    "reset": Rule("wipes working-tree or index state", flags=("--hard", "--merge", "--keep"), probe=_reset),
    "restore": Rule("discards working-tree state", probe=_restore),
    "revert": Rule("rewrites history direction mid-flight", starts=_ANY_ARG),
    "stash": Rule("hides in-flight work other agents depend on", starts=_ANY_ARG, safe=("list", "show")),
    "switch": Rule("discards local changes", flags=("-f", "-C", "--discard-changes"), starts=("--force",)),
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def _flag_operand(argv: list[str], letter: str) -> str:
    """Return the operand following a bare, clustered, or long inline flag carrying the given letter."""
    hit = next((i for i, t in enumerate(argv) if t in _INLINE_FLAGS or (t[:1] == "-" and t[:2] != "--" and t.endswith(letter))), -1)
    tail = [t for t in argv[hit + 1 :] if t != "--"] if hit >= 0 else []
    return tail[0] if tail else ""


def _resolve(argv: list[str], depth: int) -> list[list[str]]:
    """Return the argv leaves one command word yields once env prefixes, wrappers, shells, and interpreters are peeled."""
    while argv and (_ENV_ASSIGN.match(argv[0]) or argv[0] in _WRAPPERS):
        argv = argv[1:]
        while argv and ("=" in argv[0] or argv[0].isdigit() or argv[0] in _SUBVERBS or argv[0].startswith("-")):
            argv = argv[2:] if argv[0] in _VALUE_OPTS else argv[1:]
    if not argv:
        return []
    name = pathlib.PurePosixPath(argv[0]).name
    if name in _SHELLS:
        body = _flag_operand(argv, "c")
        return _leaves(body, depth + 1) if body and depth < _MAX_DEPTH else [["git", _INTERP]]
    if name.startswith(_INTERPRETERS) and (body := _flag_operand(argv, "c") or _flag_operand(argv, "e")):
        return [["git", _INTERP]] if _GIT_WORD.search(body) else []  # Ruled by implication, never keyword-scanned
    return [argv]


def _leaves(command: str, depth: int = 0) -> list[list[str]]:
    """Return every argv leaf of a shell command: substitution bodies lifted and descended, then a quote-aware split."""
    command = _HEREDOC.sub(" ", command)
    bodies, flat, cursor = _BACKTICK.findall(command), _BACKTICK.sub(" ", _CONTINUE.sub(" ", command)), 0
    while (opened := flat.find("$(", cursor)) >= 0:
        close, level = opened + 2, 1
        while close < len(flat) and level:
            level += (flat[close] == "(") - (flat[close] == ")")
            close += 1
        bodies.append(flat[opened + 2 : close - 1])
        flat, cursor = flat[:opened] + " \0sub " + flat[close:], opened + 6  # Splice, the residue never splits a leaf
    out: list[list[str]] = [leaf for body in bodies if depth < _MAX_DEPTH for leaf in _leaves(body, depth + 1)]
    lexer = shlex.shlex(_IFS.sub(" ", flat), posix=True, punctuation_chars=";&|<>()\n\r")
    lexer.whitespace, lexer.whitespace_split = " \t\f\v", True
    argv: list[str] = []
    for token in lexer:
        if token in {";", "&&", "||", "|", "&", "(", ")"} or (token and all(c in ";&|\n\r" for c in token)):
            out += _resolve(argv, depth) if argv else []
            argv = []
        else:
            argv.append(token)
    return out + (_resolve(argv, depth) if argv else [])


def _verdict(argv: list[str], cwd: str) -> str:
    """Return why one git argv is destructive under POLICY, or the empty string when it may run."""
    i = 1
    while i < len(argv) and argv[i].startswith("-"):  # Skip global options, consuming the operand of each value-taking one
        i += 2 if argv[i] in _GIT_VALUE_OPTS else 1
    if any(t.startswith("alias.") for t in argv[1:i]):
        return "an inline git alias (-c alias.*) can smuggle a blocked verb"
    words = argv[i:]
    key, args = next(((" ".join(words[:n]), words[n:]) for n in (2, 1) if " ".join(words[:n]) in POLICY), ("", []))
    if (row := POLICY.get(key)) is None or (args[:1] and args[0] in row.safe):
        return ""
    hit = next((t for t in ["", *args] if t in row.flags or t.startswith(row.starts)), None)
    return " ".join(w for w in ("git", key, hit, row.why) if w) if hit is not None else row.probe(args, cwd)


# --- [ENTRY] ----------------------------------------------------------------------------


def _refusal(command: str, cwd: str) -> str:
    """Return why the first destructive leaf in a command must be blocked, or the empty string to allow it."""
    if len(command) > _MAX_COMMAND:
        return "the command is too long to lex within the hook deadline, it cannot be checked safely"
    heads = (leaf[i:] for leaf in _leaves(command) for i, t in enumerate(leaf) if pathlib.PurePosixPath(t).name == "git")
    return next((r for head in heads if (r := _verdict(head, cwd))), "")


def main() -> int:
    """Return the hook exit status: 2 with a stderr reason blocks the first destructive leaf, 0 allows."""
    command = ""
    try:
        payload = json.loads(sys.stdin.buffer.read(_MAX_PAYLOAD) or b"{}")
        if payload["tool_name"] not in _TOOLS:
            return 0
        command, cwd = str(payload["tool_input"]["command"]), str(payload["cwd"])
        reason = _refusal(command, cwd)
    except Exception as failure:  # ruff:ignore[blind-except] -- a gate fail-closed boundary must be total, never a tuple
        reason = "" if command and not _GIT_WORD.search(command) else f"the payload or command cannot be parsed ({failure})"
    if reason:
        sys.stderr.write(f"git-guard: {_CTRL.sub(' ', reason)}. {_ADVICE}\n")
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())

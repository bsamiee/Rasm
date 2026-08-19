#!/usr/bin/env python3
"""Block destructive git verbs on the Bash PreToolUse seam; every other git use passes untouched.

Wire: PreToolUse matcher "Bash" in .claude/settings.json, exec form. Stdlib only — the gate must hold on a bare machine with no package manager warm.
POLICY tables are the edit surface. Verdict channel is exit code alone: 2 with a stderr reason blocks, 0 allows.
"""

import json
import os
import re
import shlex
import sys


# --- [CONSTANTS] ------------------------------------------------------------------------

_IFS = re.compile(r"\$\{IFS[^}]*\}|\$IFS")             # de-obfuscate git${IFS}stash forms before lexing
_ENV_ASSIGN = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*=")  # leading NAME=value env-prefix strips before argv[0]
_DOLLAR_SUB = re.compile(r"\$\(")                      # command-substitution open; the close is scanned with paren depth
_BACKTICK = re.compile(r"`([^`]*)`")                   # backtick substitution bodies execute and are descended like $(...)
_GIT_WORD = re.compile(r"\bgit\b")                     # implication test for opaque or unlexable commands
MAX_PAYLOAD = 8 * 1024 * 1024

WRAPPERS = frozenset(("sudo", "doas", "env", "command", "nice", "nohup", "stdbuf", "timeout", "xargs"))
SHELLS = frozenset(("sh", "bash", "zsh", "dash", "ksh"))
INTERPRETERS = ("python", "node", "ruby", "perl")
GIT_VALUE_OPTS = frozenset(("-C", "-c", "--git-dir", "--work-tree", "--namespace", "--config-env", "--exec-path"))

ADVICE = "Blocked by git-guard: destructive git actions are disabled. Keep all work as-is."

BLOCKED_SUBS: dict[str, str] = {  # POLICY: subcommand -> why the whole verb is off-limits
    "stash": "git stash hides in-flight work other agents depend on",
    "restore": "git restore discards working-tree or index state",
    "revert": "git revert rewrites history direction mid-flight",
    "reset": "git reset moves HEAD/index and can wipe the working tree",
    "clean": "git clean deletes untracked files",
}
CHECKOUT_FORCE = frozenset(("-f", "--force", "-B", "--ours", "--theirs", "-p", "--patch"))          # POLICY
CHECKOUT_CREATE = frozenset(("-b", "--orphan", "-t", "--track", "--detach"))                        # POLICY: creation forms stay open
SWITCH_FORCE = frozenset(("-f", "--force", "--discard-changes", "-C"))                              # POLICY
PUSH_FORCE = frozenset(("-f", "--force", "--force-with-lease", "--force-if-includes", "--mirror"))  # POLICY
BRANCH_FORCE = frozenset(("-D", "-f", "--force", "-M"))                                             # POLICY

# --- [OPERATIONS] -----------------------------------------------------------------------


def _subs(command: str) -> tuple[str, list[str]]:
    """Lift backtick and $(...) substitution bodies out of a command; each executes and is descended."""
    inner: list[str] = _BACKTICK.findall(command)
    stripped, i = _BACKTICK.sub(" ", command), 0
    while (hit := _DOLLAR_SUB.search(stripped, i)) is not None:
        depth, j = 1, hit.end()
        while j < len(stripped) and depth:
            depth += (stripped[j] == "(") - (stripped[j] == ")")
            j += 1
        inner.append(stripped[hit.end() : j - 1])
        i = j
    return stripped, inner


def _leaves(command: str, depth: int = 0) -> list[list[str]]:
    """Decompose a command into argv leaves via quote-aware split, sub descent, and wrapper/shell peeling."""
    stripped, inner = _subs(command)
    out: list[list[str]] = [leaf for body in inner if depth < 8 for leaf in _leaves(body, depth + 1)]
    lexer = shlex.shlex(_IFS.sub(" ", stripped), posix=True, punctuation_chars=";&|<>()\n\r")
    lexer.whitespace, lexer.whitespace_split = " \t\f\v", True
    argv: list[str] = []
    for token in lexer:
        if token in {";", "&&", "||", "|", "&", "(", ")"} or (token and all(c in ";&|\n\r" for c in token)):
            out += _resolve(argv, depth) if argv else []
            argv = []
        else:
            argv.append(token)
    return out + (_resolve(argv, depth) if argv else [])


def _resolve(argv: list[str], depth: int) -> list[list[str]]:
    """Peel env-prefixes and wrappers, then descend a shell -c or flag an inline interpreter body."""
    changed = True
    while changed and argv:
        changed = False
        while argv and _ENV_ASSIGN.match(argv[0]):
            argv, changed = argv[1:], True
        while argv and argv[0] in WRAPPERS:
            argv, changed = argv[1:], True
            while argv and (argv[0].startswith("-") or "=" in argv[0] or argv[0].isdigit()):
                argv = argv[1:]
    if not argv:
        return []
    name = os.path.basename(argv[0])
    if depth < 8 and name in SHELLS and (body := _flag_operand(argv, "c")):
        return _leaves(body, depth + 1)
    if name.startswith(INTERPRETERS) and (body := _flag_operand(argv, "c") or _flag_operand(argv, "e")):
        return [["\0interp", body]]  # opaque one-liner; ruled by git-implication, never keyword-scanned deeper
    return [argv]


def _flag_operand(argv: list[str], letter: str) -> str:
    """Return the operand after a short flag carrying letter, clustered or not."""
    return next(
        (argv[i + 1] for i, t in enumerate(argv) if t.startswith("-") and not t.startswith("--") and letter in t and i + 1 < len(argv)),
        "",
    )


def _split_git(argv: list[str]) -> tuple[str, list[str], list[str]]:
    """Split a git argv into subcommand, its args, and any -c inline-config values."""
    i, cvals = 1, []
    while i < len(argv):
        tok = argv[i]
        if tok in GIT_VALUE_OPTS:
            if tok == "-c" and i + 1 < len(argv):
                cvals.append(argv[i + 1])
            i += 2
            continue
        if tok.startswith("-"):
            i += 1
            continue
        return tok, argv[i + 1 :], cvals
    return "", [], cvals


def _rule_checkout(rest: list[str], cwd: str) -> str:
    """Rule on git checkout: branch movement and creation pass; any discard-shaped form blocks."""
    if hit := next((t for t in rest if t in CHECKOUT_FORCE), ""):
        return f"git checkout {hit} discards local changes"
    if "--" in rest:
        return "git checkout with a pathspec overwrites working-tree files"
    if any(t in CHECKOUT_CREATE for t in rest):
        return ""
    targets = [t for t in rest if t == "-" or not t.startswith("-")]
    if len(targets) > 1:
        return "git checkout <ref> <path> overwrites working-tree files"
    if not targets or targets[0] == "-":
        return ""
    target = targets[0]
    if target == "." or target.startswith(":"):
        return "git checkout with a pathspec overwrites working-tree files"
    probe = target if os.path.isabs(target) else os.path.join(cwd or os.getcwd(), target)
    return f"git checkout {target} names an existing path and would overwrite it" if os.path.exists(probe) else ""


def _rule_git(argv: list[str], cwd: str) -> str:
    """Rule on one git invocation; empty string allows."""
    sub, rest, cvals = _split_git(argv)
    if any(v.startswith("alias.") for v in cvals):
        return "inline git alias (-c alias.*) can smuggle a blocked verb"
    if reason := BLOCKED_SUBS.get(sub, ""):
        return reason
    if sub == "config" and any(a.startswith("alias.") for a in rest):
        return "defining git aliases can smuggle a blocked verb"
    if sub == "checkout":
        return _rule_checkout(rest, cwd)
    if sub == "switch" and (hit := next((t for t in rest if t in SWITCH_FORCE), "")):
        return f"git switch {hit} discards local changes"
    if sub == "push" and (hit := next((t for t in rest if t in PUSH_FORCE or t.startswith("--force-with-lease=") or (t.startswith("+") and not t.startswith("-"))), "")):
        return f"git push {hit} rewrites remote history"
    if sub == "branch" and (hit := next((t for t in rest if t in BRANCH_FORCE), "")):
        return f"git branch {hit} force-deletes or force-moves a branch"
    return ""


def _classify(argv: list[str], cwd: str) -> str:
    """Dispatch one argv leaf; only git and git-implicating opaque bodies can block."""
    if argv[0] == "\0interp":
        return "inline interpreter one-liner invokes git; run git directly so it can be checked" if _GIT_WORD.search(argv[1]) else ""
    return _rule_git(argv, cwd) if os.path.basename(argv[0]) == "git" else ""


def main() -> int:
    """Decode stdin, decompose the Bash command, and block the first destructive git leaf."""
    try:
        payload = json.loads(sys.stdin.buffer.read(MAX_PAYLOAD) or b"{}")
        if payload.get("tool_name") != "Bash":
            return 0
        command = str(payload.get("tool_input", {}).get("command", ""))
        cwd = str(payload.get("cwd", ""))
    except Exception as failure:  # malformed harness payload: fail closed, never guess
        sys.stderr.write(f"git-guard failed closed on payload decode: {failure}\n")
        return 2
    try:
        leaves = _leaves(command)
    except Exception:  # unlexable command: fail closed only when git is implicated
        if _GIT_WORD.search(command):
            sys.stderr.write(f"git-guard: command mentions git but cannot be parsed for safety. {ADVICE}\n")
            return 2
        return 0
    for leaf in leaves:
        if leaf and (reason := _classify(leaf, cwd)):
            sys.stderr.write(f"git-guard: {reason}. {ADVICE}\n")
            return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())

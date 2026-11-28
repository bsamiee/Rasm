#!/usr/bin/env -S uv run --script
# /// script
# requires-python = ">=3.15"
# dependencies = ["markdown-it-py", "mdit-py-plugins", "msgspec", "pyyaml"]
#
# [tool.uv]
# prerelease = "allow"
# ///
"""Validates memory files, regenerates MEMORY.md from their descriptions, and commits the folder."""

from collections import Counter
from functools import cache
import glob
from itertools import groupby, takewhile
import os
from pathlib import Path
import re
import subprocess
import sys
from typing import Annotated, Literal
lazy from collections.abc import Iterable, Mapping
lazy import tempfile
lazy import textwrap

from markdown_it import MarkdownIt
from markdown_it.token import Token
from mdit_py_plugins.front_matter import front_matter_plugin
import msgspec
import yaml
from yaml.cyaml import CSafeLoader
lazy from markdown_it.rules_core import StateCore
lazy from markdown_it.rules_inline import StateInline

# --- [CONSTANTS] ------------------------------------------------------------------------

_INDEX = "MEMORY.md"

# --- [MODELS] ---------------------------------------------------------------------------


class _Metadata(msgspec.Struct, frozen=True):
    type: Literal["user", "feedback", "project", "reference"]


class _Tool(msgspec.Struct, frozen=True, forbid_unknown_fields=True):
    name: Annotated[str, msgspec.Meta(pattern=r"^mcp__\S+$")]
    input: Mapping[str, tuple[Annotated[str, msgspec.Meta(pattern=r"\S")], ...]] = msgspec.field(default_factory=dict)


class _Refusal(msgspec.Struct, frozen=True, forbid_unknown_fields=True):
    programs: tuple[Annotated[str, msgspec.Meta(pattern=r"^[^\s-]\S*(?: \S+)*$")], ...] = ()
    tools: tuple[_Tool, ...] = ()


class _Triggers(msgspec.Struct, frozen=True, forbid_unknown_fields=True):
    paths: tuple[Annotated[str, msgspec.Meta(pattern=r"^[^{}\s]+$")], ...] = ()
    programs: tuple[Annotated[str, msgspec.Meta(pattern=r"^[^\s-]\S*(?: \S+)*$")], ...] = ()
    tools: tuple[_Tool, ...] = ()
    output: tuple[Annotated[str, msgspec.Meta(pattern=r"\S")], ...] = ()
    refuse: _Refusal = msgspec.field(default_factory=_Refusal)


class _Frontmatter(msgspec.Struct, frozen=True):
    name: str
    description: Annotated[str, msgspec.Meta(pattern=r"\S")]
    metadata: _Metadata
    triggers: _Triggers = msgspec.field(default_factory=_Triggers)


class _Row(msgspec.Struct, frozen=True):
    memory: str
    description: str
    file: str
    triggers: _Triggers


class _Finding(msgspec.Struct, frozen=True, order=True):
    path: str
    line: int
    message: str
    blocking: bool


# --- [OPERATIONS] -----------------------------------------------------------------------


def _linked(state: StateCore, /) -> None:
    for token in state.tokens:
        if token.type == "inline" and "[[" in token.content:
            token.children = []
            state.md.inline.parse(token.content, state.md, state.env, token.children)


def _wikilink(state: StateInline, silent: object, /) -> bool:
    if (hit := re.compile(r"\[\[([^\[\]\n]+)\]\]").match(state.src, state.pos, state.posMax)) is not None and not silent:
        token = state.push("wikilink", "", 0)
        token.content, token.map = hit[1], [state.src.count("\n", 0, state.pos)] * 2
    state.pos = state.pos if hit is None else hit.end()
    return hit is not None


@cache
def _markdown() -> MarkdownIt:
    return MarkdownIt("commonmark").use(front_matter_plugin).use(lambda md: md.core.ruler.at("inline", _linked)).use(lambda md: md.inline.ruler.before("link", "wikilink", _wikilink))


def _listed(text: str, /) -> tuple[frozendict[str, str], tuple[_Finding, ...]]:
    rows = tuple((number, line, re.fullmatch(r"## [^\s#]+|- \[([^\]\n]+)\]\(\1\.md\)(?: — .+)?", line)) for number, line in enumerate(text.split("\n"), 1) if line.strip())
    return frozendict({hit[1]: line for _, line, hit in rows if hit and hit[1] is not None}), tuple(
        _Finding(
            _INDEX,
            number,
            f"line holding `{link[1] if (link := re.search(r'\]\(([^)\s]+)\)', line)) else textwrap.shorten(line, 150)}` is not in the generated form, so the index drops it",
            blocking=False,
        )
        for number, line, hit in rows
        if not hit
    )


def _decoded(blocks: list[Token], /) -> _Frontmatter | yaml.YAMLError | msgspec.ValidationError | str:
    match blocks:
        case [Token(type="front_matter", map=[0, end], content=content), *_] if end <= 30:
            try:
                return msgspec.convert(yaml.load(content, Loader=CSafeLoader), type=_Frontmatter)
            except (yaml.YAMLError, msgspec.ValidationError) as error:
                return error
        case [Token(type="front_matter"), *_]:
            return "closes past line 30, where the harness stops reading"
        case _:
            return "is absent"


def _placed(pattern: str, top: Path, /) -> str:
    return str(Path.home() / pattern.removeprefix("~/")) if pattern.startswith("~/") else pattern if pattern.startswith("/") else str(top / pattern)


def _reaches(pattern: str, top: Path, tracked: tuple[str, ...], /) -> bool:
    if pattern.startswith(("/", "~/")):
        return Path(*takewhile(lambda part: re.search(r"[*?\[]", part) is None, Path(_placed(pattern, top)).parts)).exists()
    matcher = re.compile(glob.translate(pattern, recursive=True, include_hidden=True))
    return any(matcher.match(path) for path in tracked)


def _entry(stems: frozenset[str], listed: Mapping[str, str], project: tuple[Path, tuple[str, ...]], path: Path, /) -> tuple[str, tuple[_Finding, ...], str | None, _Row | None]:
    raw = path.read_bytes()
    blocks = _markdown().parse(raw.decode())
    links = (
        _Finding(path.name, block.map[0] + link.map[0] + 1, f"link [[{link.content}]] names no memory file", blocking=True)
        for block in blocks
        if block.children is not None and block.map is not None
        for link in block.children
        if link.type == "wikilink" and link.map is not None and link.content not in stems
    )
    budget = (_Finding(path.name, 1, f"file holds {len(raw)} bytes, past the 4096 recall shows", blocking=False),) if len(raw) > 4096 else ()
    match _decoded(blocks):
        case _Frontmatter(name=name, description=description, metadata=_Metadata(type=kind), triggers=triggers):
            hook, (top, tracked) = " ".join(description.split()), project
            unreached = tuple(
                _Finding(path.name, 1, f"trigger path `{pattern}` reaches no {'path' if pattern.startswith(('/', '~/')) else 'tracked file'}", blocking=True)
                for pattern in triggers.paths
                if not _reaches(pattern, top, tracked)
            )
            checks = (
                (name != path.stem, f"name `{name}` differs from the file stem"),
                ("\n" in description.strip(), "description spans more than one line"),
                (len(hook) > 150, f"description holds {len(hook)} characters, over 150"),
            )
            row = (
                None if unreached or triggers == _Triggers() else _Row(path.stem, hook, str(path), msgspec.structs.replace(triggers, paths=tuple(_placed(pattern, top) for pattern in triggers.paths)))
            )
            return f"- [{path.stem}]({path.name}) — {hook}", (*(_Finding(path.name, 1, message, blocking=True) for failed, message in checks if failed), *unreached, *links, *budget), kind, row
        case yaml.MarkedYAMLError(problem=str(problem), problem_mark=mark) if mark is not None:
            line, reason = mark.line + 2, f"frontmatter YAML {problem}"
        case refused:
            line, reason = 1, f"frontmatter {refused}"
    kept = f"{reason}, so the index {'keeps its earlier line' if path.stem in listed else 'lists it bare'}"
    return listed.get(path.stem, f"- [{path.stem}]({path.name})"), (_Finding(path.name, line, kept, blocking=True), *links, *budget), None, None


def _measured(text: str, /) -> Iterable[_Finding]:
    loaded = text.strip()
    return (
        _Finding(_INDEX, 1, f"index holds {size} {unit}, past the {cap} the harness loads", blocking=True)
        if size > cap
        else _Finding(_INDEX, 1, f"index holds {size} {unit}, and the harness asks for compaction from {int(cap * 0.8)}", blocking=False)
        for size, cap, unit in ((loaded.count("\n") + 1, 200, "lines"), (len(loaded.encode("utf-16-le")) // 2, 25_000, "characters"))
        if size / cap >= 0.8
    )


def _git(folder: Path, /, *args: str) -> str:
    return subprocess.run(("git", "-C", str(folder), *args), stdout=subprocess.PIPE, check=True, text=True).stdout


def _project(root: Path, /) -> tuple[Path, tuple[str, ...]]:
    found = subprocess.run(("git", "-C", str(root), "rev-parse", "--show-toplevel"), stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, check=False, text=True)
    top = Path(found.stdout.strip()) if found.returncode == 0 else root
    return top, tuple(_git(top, "ls-files", "-z").split("\0")[:-1]) if found.returncode == 0 else ()


# --- [COMPOSITION] ----------------------------------------------------------------------


def index(memory: Path, root: Path, /, *, table: bool) -> int:
    """Validate every memory file against the project at root, regenerate MEMORY.md from their descriptions, and commit the folder, initializing its repository when it has none; exit 1 on errors in changed files.

    With table, the trigger rows of every file whose triggers validate go to stdout as JSON, and the report to stderr.

    Returns:
        1 when a changed file holds an error, else 0.
    """
    if fresh := not (memory / ".git").exists():
        _git(memory, "init", "-q")
        _git(memory, "config", "commit.gpgsign", "false")
    target = memory / _INDEX
    try:
        previous = target.read_text(encoding="utf-8")
    except FileNotFoundError:
        previous = ""
    paths = [path for path in memory.glob("[!.]*.md") if path.name != _INDEX]
    (listed, foreign), stems = _listed(previous), frozenset(path.stem for path in paths)
    project = _project(root)
    read = [(path.stem.partition("-")[0], path.stem, *_entry(stems, listed, project, path)) for path in paths]
    shared, specific = Counter(word for word, *_ in read), frozenset(word for word, *_, kind, _ in read if kind not in {None, "feedback", "user"})
    entries = sorted(((word if shared[word] > 1 and word in specific else "", stem, line, found) for word, stem, line, found, _, _ in read), key=lambda entry: entry[:2])
    text = "\n".join((f"## {field}\n" if field else "") + "".join(f"{line}\n" for *_, line, _ in group) for field, group in groupby(entries, key=lambda entry: entry[0]))
    if written := text != previous:
        with tempfile.NamedTemporaryFile("w", encoding="utf-8", dir=memory, prefix=f".{_INDEX}.", delete=False) as staged:
            staged.write(text)
        Path(staged.name).replace(target)
    status = _git(memory, "--no-optional-locks", "status", "--porcelain", "-z", "--untracked-files=all", "--no-renames")
    changed = frozenset(entry[3:] for entry in status.split("\0") if entry)
    if changed:
        _git(memory, "add", "-A")
        trailer = (f"--trailer=Session: {session}",) if (session := os.environ.get("CLAUDE_CODE_SESSION_ID", "")) else ()
        named = frozenset(name.removesuffix(".md") for name in changed - {_INDEX})
        _git(memory, "commit", "-q", *trailer, "-m", f"Update {f'{len(named)} files' if len(named) > 1 else next(iter(named), _INDEX)}")
    graded = [(finding.blocking and not fresh and finding.path in changed, finding) for finding in sorted((*foreign, *(finding for *_, found in entries for finding in found), *_measured(text)))]
    errors = sum(error for error, _ in graded)
    commit = f"commit {_git(memory, 'rev-parse', '--short', 'HEAD').strip()}" if changed else "nothing to commit"
    if table:
        sys.stdout.buffer.write(msgspec.json.encode(sorted((row for *_, row in read if row is not None), key=lambda row: row.memory)))
    (sys.stderr if table else sys.stdout).writelines((
        *(f"{'error' if error else 'warning'}: {finding.path}:{finding.line}: {finding.message}\n" for error, finding in graded),
        f"{errors} errors, {len(graded) - errors} warnings, {_INDEX} {'written' if written else 'unchanged'}, {commit}\n",
    ))
    return int(bool(errors))


if __name__ == "__main__":
    _, folder, root, *mode = sys.argv
    try:
        sys.exit(index(Path(folder), Path(root), table=mode == ["--table"]))
    except subprocess.CalledProcessError as error:
        sys.exit(error.returncode)

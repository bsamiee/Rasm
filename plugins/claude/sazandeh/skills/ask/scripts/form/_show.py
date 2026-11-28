"""Content kinds drawn as card blocks: text, rows, records, flows, diffs, documents, and git ranges."""

from functools import partial
from itertools import chain, groupby, pairwise
import os
from pathlib import Path
import shlex
lazy from collections.abc import Callable, Iterable
lazy import difflib
lazy import subprocess

from expression import Error, Ok
import msgspec
import regex
import wcwidth
lazy from markdown_it.token import Token
lazy import mdformat
lazy from pygments.lexers import get_lexer_for_filename
lazy from pygments.lexers.markup import MarkdownLexer
lazy from pygments.util import ClassNotFound
lazy from ruamel.yaml import YAML
lazy from ruamel.yaml.error import YAMLError
lazy import unidiff

from form._card import ACROSS, authored, Block, Box, Cut, DOWN, escaped, fenced, FILE, MERGED, Rows, Source, spanned, Unit
from form._spec import clean, Content, Diff, Fault, Fields, File, FRONTED, Git, Loc, Outcome, Pair, plain, Range, Reason, Records, Row, Span, Steps, Text, visible

# --- [CONSTANTS] ------------------------------------------------------------------------

_WORD_SHARE = 0.4
_CONTEXT_WORDS = 4
_WORDS = regex.compile(r"\s+|\S+")
_WIKILINK = regex.compile(r"\[\[([^\[\]|#\n]+)[^\[\]\n]*\]\]")


# --- [MODELS] ---------------------------------------------------------------------------


class Document(msgspec.Struct, frozen=True, kw_only=True):
    path: Path
    leaves: tuple[Row, ...]
    body: Cut
    paths: frozenset[str] = frozenset()
    names: frozenset[str] = frozenset()

    def rows(self, keys: tuple[str, ...], /) -> tuple[Row, ...]:
        return tuple(chain.from_iterable((leaf for leaf in self.leaves if leaf[0] == key or leaf[0].startswith(f"{key}.")) for key in keys)) if keys else self.leaves

    def missing(self, keys: tuple[str, ...], /) -> frozenset[str]:
        return frozenset(key for key in keys if not any(leaf == key or leaf.startswith(f"{key}.") for leaf, _ in self.leaves))


class Binary(msgspec.Struct, frozen=True, kw_only=True):
    path: Path
    size: int


class Shown(msgspec.Struct, frozen=True, kw_only=True):
    blocks: tuple[Block, ...]
    reverse: str | None = None


type Read = Document | Binary


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GIT]


def git(repo: Path, /, *args: str) -> tuple[str, ...]:
    return ("git", "-C", str(repo), *args)


def ran(at: Loc, argv: tuple[str, ...], /) -> Outcome[bytes]:
    try:
        return Ok(subprocess.run(argv, capture_output=True, check=True).stdout)
    except (OSError, subprocess.CalledProcessError) as error:
        detail = error.stderr.decode(errors="replace") if isinstance(error, subprocess.CalledProcessError) else str(error)
        return Error(Fault.of(at, Reason.GIT, command=shlex.join(argv), cause=next(iter(detail.strip().splitlines()), "")))


def diff(repo: Path, span: Span, /, *options: str) -> tuple[str, ...]:
    base, tip = span
    return git(repo, "diff", "--no-color", "--no-ext-diff", *options, "--relative", "--end-of-options", base, *((tip,) if tip is not None else ()))


def _reverse(repo: Path, span: Span, paths: tuple[str, ...], raw: bytes, /) -> str:
    base, tip = span
    shown = git(repo, "diff", "--no-color", "--no-ext-diff", "--binary", "--end-of-options", base, *((tip,) if tip is not None else ()), "--", *paths)
    return f"{shlex.join(shown)} | {shlex.join(git(Path(raw.decode().strip()), 'apply', '-R', '--index'))}"


def _ends(repo: Path, revision: str, at: Loc, raw: bytes, /) -> Outcome[Span]:
    lines = raw.decode().split()
    tips, bases = tuple(line for line in lines if not line.startswith("^")), tuple(line.removeprefix("^") for line in lines if line.startswith("^"))
    match tips, bases:
        case (), ():
            return Error(Fault.of(at, Reason.RANGE_UNKNOWN, range=revision))
        case (tip,), (base,):
            return Ok((base, tip))
        case (tip,), ():
            parent = ran(at, git(repo, "rev-parse", "--verify", "--quiet", "--end-of-options", f"{tip}^")).or_else_with(lambda _: ran(at, git(repo, "hash-object", "-t", "tree", os.devnull)))
            return parent.map(lambda base: (base.decode().strip(), tip))
        case _:
            return Error(Fault.of(at, Reason.RANGE_FORM, range=revision, tips=len(tips), bases=len(bases)))


def endpoints(repo: Path, revision: str | None, at: Loc, /) -> Outcome[Span]:
    match revision:
        case None:
            upstream = ran(at, git(repo, "rev-parse", "--verify", "--quiet", "--end-of-options", "@{upstream}"))
            return upstream.or_else_with(lambda _: ran(at, git(repo, "rev-parse", "--verify", "--end-of-options", "HEAD"))).map(lambda raw: (raw.decode().strip(), None))
        case str():
            return ran(at, git(repo, "rev-parse", "--revs-only", "--end-of-options", revision)).bind(partial(_ends, repo, revision, at))


# --- [DIFFS]


type _Spans = tuple[tuple[bool, str, str], ...]


def _styled(spans: _Spans, _: Box, /) -> tuple[bool, str]:
    return False, f"{MERGED} {''.join(map(_span, spans))}"


def _span(span: tuple[bool, str, str], /) -> str:
    same, gone, came = span
    if same:
        return escaped(gone)
    edge = came or gone
    marks = (*((f"~~{escaped(gone.strip())}~~",) if gone.strip() else ()), *((f"**{escaped(came.strip())}**",) if came.strip() else ()))
    return f"{' ' * edge[:1].isspace()}{' '.join(marks)}{' ' * edge[-1:].isspace()}"


def _part(count: int, context: int, index: int, span: tuple[bool, str, str], /) -> str:
    same, gone, came = span
    kept, head, tail = gone.split(), context * (index > 0), context * (index < count - 1)
    match same:
        case False:
            return f"{f'[-{gone}-]' if gone else ''}{f'{{+{came}+}}' if came else ''}"
        case True if len(kept) <= head + tail + 2:
            return gone
        case True:
            return f"{' ' * gone[:1].isspace()}{' '.join((*kept[:head], '…', *kept[len(kept) - tail :]))}{' ' * gone[-1:].isspace()}"


def _context(spans: _Spans, context: int, /) -> str:
    return f"~{''.join(_part(len(spans), context, index, span) for index, span in enumerate(spans))}"


def _marked(spans: _Spans, box: Box, /) -> tuple[bool, str]:
    fitting = (line for line in map(partial(_context, spans), range(_CONTEXT_WORDS, 0, -1)) if all(wcwidth.width(line) <= width for width in box.width.to_list()))
    return True, next(fitting, None) or _context(spans, 0)


type _Render = Callable[[_Spans, Box], tuple[bool, str]]


def _paired(old: str, new: str, box: Box, render: _Render, /) -> tuple[tuple[bool, str], ...]:
    words = (tuple(_WORDS.findall(old)), tuple(_WORDS.findall(new)))
    spans = tuple(
        (tag == "equal", "".join(words[0][start:stop]), "".join(words[1][first:last])) for tag, start, stop, first, last in difflib.SequenceMatcher(None, *words, autojunk=False).get_opcodes()
    )
    changed = sum(max(len(gone), len(came)) for same, gone, came in spans if not same)
    wide = any(max(wcwidth.width(old), wcwidth.width(new)) + 1 > width for width in box.width.to_list())
    if wide and changed / max(len(old), len(new), 1) <= _WORD_SHARE:
        return (render(spans, box),)
    return ((True, f"-{old}"), (True, f"+{new}"))


def _run(runs: tuple[tuple[str, tuple[str, ...]], ...], paired: frozenset[int], box: Box, render: _Render, index: int, /) -> tuple[tuple[bool, str], ...]:
    kind, values = runs[index]
    match index in paired, index - 1 in paired:
        case True, _:
            return tuple(chain.from_iterable(_paired(old, new, box, render) for old, new in zip(values, runs[index + 1][1], strict=True)))
        case _, True:
            return ()
        case _:
            return tuple((True, f"{kind}{value}") for value in values)


def _hunk(hunk: unidiff.Hunk, box: Box, render: _Render, /) -> str:
    runs = tuple(
        (kind, tuple(line.value.rstrip("\n").expandtabs() for line in lines)) for kind, lines in groupby((line for line in hunk if line.line_type in {" ", "-", "+"}), key=lambda line: line.line_type)
    )
    head = f"@@ -{hunk.source_start},{hunk.source_length} +{hunk.target_start},{hunk.target_length} @@"
    context = f" {hunk.section_header.strip()}" if hunk.section_header.strip() else ""
    clipped = box.width.map(lambda width: context if wcwidth.width(head + context) <= width else f"{wcwidth.clip(context, 0, max(0, width - wcwidth.width(head) - 1))}…").default_value(context)
    paired = frozenset(index for index, ((kind, gone), (later, came)) in enumerate(pairwise(runs)) if (kind, later) == ("-", "+") and len(gone) == len(came))
    tagged = ((True, head + clipped), *chain.from_iterable(map(partial(_run, runs, paired, box, render), range(len(runs)))))
    return "\n\n".join(fenced("diff", (text for _, text in group)) if inside else "\n".join(text for _, text in group) for inside, group in groupby(tagged, key=lambda item: item[0]))


def _info(path: Path, /) -> str | None:
    try:
        lexer = get_lexer_for_filename(path.name)
    except ClassNotFound:
        return "text"
    return None if isinstance(lexer, MarkdownLexer) else next(iter(lexer.aliases), "text")


def patched(text: str, at: Loc, source: Source | None, box: Box, /) -> Outcome[tuple[Block, ...]]:
    patch = unidiff.PatchSet(clean(text))
    files = tuple(file for file in patch if len(file))
    many = len(files) > 1
    pieces = tuple(
        "\n\n".join((
            *((f"{FILE} {spanned(file.path)} · +{file.added} \N{MINUS SIGN}{file.removed}",) if many and index == 0 else ()),
            _hunk(hunk, box, _styled if _info(Path(file.path)) is None else _marked),
        ))
        for file in files
        for index, hunk in enumerate(file)
    )
    match files:
        case ():
            return Error(Fault.of(at, Reason.DIFF_EMPTY))
        case _:
            named = spanned(files[0].path) if not many else f"{len(files)} files"
            return Ok((f"{named} · +{patch.added} \N{MINUS SIGN}{patch.removed}", Cut(pieces=pieces, unit=Unit.HUNKS, source=source)))


# --- [DOCUMENTS]


def _value(value: object, /) -> str:
    return f"[{', '.join(map(str, value))}]" if isinstance(value, list) else str(value)


def _leaves(tree: object, prefix: str = "", /) -> tuple[Row, ...]:
    match tree:
        case dict():
            return tuple(chain.from_iterable(_leaves(value, f"{prefix}{key}.") if isinstance(value, dict) else ((f"{prefix}{key}", plain(_value(value))),) for key, value in tree.items()))
        case _:
            return ()


def _texts(token: Token, /) -> Iterable[str]:
    return (child.content for child in token.children or () if child.type == "text")


def _marked_up(path: Path, source: Source, at: Loc, text: str, /) -> Outcome[Read]:
    tokens = FRONTED.parse(text)
    lines = text.split("\n")
    children = tuple(child for token in tokens for child in token.children or ())
    hrefs = (str(child.attrs.get("href", "")) for child in children if child.type == "link_open")
    paths = frozenset(os.path.normpath(path.parent / href.partition("#")[0]) for href in hrefs if "://" not in href and href.partition("#")[0])
    names = frozenset(found[1].strip() for token in tokens for content in _texts(token) for found in _WIKILINK.finditer(content))
    pieces = tuple(
        "\n".join(lines[start:end]).strip("\n") for token in tokens if token.level == 0 and token.nesting >= 0 and token.type != "front_matter" and token.map is not None for start, end in (token.map,)
    )
    front = next((token.content for token in tokens if token.type == "front_matter"), None)
    try:
        leaves = _leaves(YAML(typ="base").load(front)) if front is not None else ()
    except YAMLError as error:
        return Error(Fault.of(at, Reason.YAML, path=path, cause=type(error).__name__))
    return Ok(Document(path=path, leaves=leaves, body=Cut(pieces=pieces, unit=Unit.BLOCKS, source=source), paths=paths, names=names))


def decoded(path: Path, source: Source, at: Loc, raw: bytes, /) -> Outcome[Read]:
    try:
        text = visible(raw.decode()) if b"\0" not in raw else None
    except UnicodeDecodeError:
        text = None
    match text, _info(path):
        case None, _:
            return Ok(Binary(path=path, size=len(raw)))
        case str(), str() as info:
            lines = tuple(text.strip("\n").split("\n")) if text.strip("\n") else ()
            return Ok(Document(path=path, leaves=(), body=Cut(pieces=lines, unit=Unit.LINES, source=source, fence=info)))
        case str(), None:
            return _marked_up(path, source, at, text)


def read(path: Path, at: Loc, /) -> Outcome[bytes]:
    try:
        return Ok(path.read_bytes())
    except OSError as error:
        return Error(Fault.of(at, Reason.UNREADABLE, path=path, cause=type(error).__name__))


# --- [CONTENT]


def _flow(steps: tuple[str, ...], box: Box, /) -> str:
    widths = tuple(wcwidth.width(step) + 2 for step in steps)
    gap = " " * wcwidth.width(ACROSS)
    across = (gap.join(f"┌{'─' * width}┐" for width in widths), ACROSS.join(f"│ {step} │" for step in steps), gap.join(f"└{'─' * width}┘" for width in widths))
    widest = max(widths)
    down = tuple(
        chain.from_iterable(
            (*((f"{' ' * (widest // 2 + 1)}{DOWN}",) if index else ()), f"┌{'─' * widest}┐", f"│ {wcwidth.ljust(step, widest - 2)} │", f"└{'─' * widest}┘") for index, step in enumerate(steps)
        )
    )
    return fenced("text", across if all(wcwidth.width(across[1]) <= width for width in box.width.to_list()) else down)


def _records(records: tuple[frozendict[str, str], ...], box: Box, /) -> tuple[Block, ...]:
    keys = tuple(records[0])
    sections = (Rows(sections=tuple((next(iter(record.values())), tuple(record.items())[1:]) for record in records), unit=Unit.RECORDS),)
    if len(records) < 2 or any(tuple(record) != keys for record in records):
        return sections
    raw = "\n".join((f"| {' | '.join(map(escaped, keys))} |", f"|{'|'.join('---' for _ in keys)}|", *(f"| {' | '.join(escaped(record[key]) for key in keys)} |" for record in records)))
    head, rule, *body = mdformat.text(raw, extensions={"gfm"}).rstrip("\n").split("\n")
    return (Cut(pieces=tuple(body), unit=Unit.RECORDS, head=(head, rule)),) if all(wcwidth.width(head) <= width for width in box.width.to_list()) else sections


def _document(path: Path, read: Read, /) -> tuple[Block, ...]:
    match read:
        case Binary(size=count):
            return (f"{spanned(path.name)} · binary, {count} bytes",)
        case Document(leaves=leaves, body=body):
            return (f"{spanned(path.name)} · {body.size} {body.unit.noun}", *((Rows.of(leaves),) if leaves else ()), body)


def content(show: Content, at: Loc, box: Box, /) -> Outcome[Shown]:
    match show:
        case Text(text=text):
            return authored(text, (*at, "text")).map(lambda body: Shown(blocks=(body,)))
        case Fields(fields=fields):
            return Ok(Shown(blocks=(Rows.of(fields.items(), unit=Unit.ROWS),)))
        case Records(records=records):
            return Ok(Shown(blocks=_records(records, box)))
        case Steps(steps=steps):
            return Ok(Shown(blocks=(_flow(steps, box),)))
        case Diff(diff=Pair(path=path, before=before, after=after)):
            text = "\n".join(difflib.unified_diff(before.splitlines(), after.splitlines(), f"a/{path}", f"b/{path}", n=1, lineterm=""))
            return patched(text, (*at, "diff"), None, box).map(lambda blocks: Shown(blocks=blocks))
        case Git(git=Range(repo=repo, range=revision, paths=paths)):
            place = (*at, "git")
            command = shlex.join(("git", "diff", revision, *(("--", *paths) if paths else ())))
            reversing = endpoints(repo, revision, place).bind(
                lambda span: ran(place, git(repo, "rev-parse", "--show-toplevel")).map(partial(_reverse, repo, span, paths)).map(lambda command: (span, command))
            )
            return reversing.bind(
                lambda held: ran(place, (*diff(repo, held[0], "--unified=1"), "--", *paths)).bind(
                    lambda raw: patched(raw.decode(errors="replace"), place, Source(shown=command, full=command), box).map(lambda blocks: Shown(blocks=blocks, reverse=held[1]))
                )
            )
        case File(file=path):
            place = (*at, "file")
            return read(path, place).bind(partial(decoded, path, Source(shown=str(path), full=str(path.resolve())), place)).map(lambda found: Shown(blocks=_document(path, found)))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Binary", "Document", "Read", "Shown", "content", "decoded", "diff", "endpoints", "git", "patched", "ran", "read"]

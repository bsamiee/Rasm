"""Checks and fixes Markdown and source comments."""

from pathlib import Path
from typing import override
lazy from bisect import bisect_right
lazy from collections.abc import Callable, Iterable, Iterator
lazy from functools import reduce
lazy from itertools import accumulate, groupby, pairwise, starmap, takewhile
lazy import shlex
lazy import subprocess
lazy import sys

from cyclopts import App
from cyclopts.types import ExistingPath
from markdown_it import MarkdownIt
from markdown_it.tree import SyntaxTreeNode
from mdit_py_plugins.front_matter import front_matter_plugin
import msgspec
import regex
lazy from markdown_it.helpers import parseLinkDestination, parseLinkLabel, parseLinkTitle
lazy from markdown_it.rules_block.table import escapedSplit
lazy from markdown_it.rules_inline import StateInline
lazy from markdown_it.token import Token
lazy from pygments.lexers import get_lexer_for_filename
lazy from pygments.token import Comment, is_token_subtype
lazy from wcwidth import wcswidth

# --- [TYPES] ----------------------------------------------------------------------------

type _Comments = tuple[_Directive | _Divider, ...]
type _MappedNode = tuple[SyntaxTreeNode, tuple[int, int]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_LEADER = regex.compile(r"(?P<chain>\[[^\]]*\](?:-\[[^\]]*\])*)(?:\((?P<path>[^)]*)\):|:?) ")
_DIVIDER = regex.compile(r"--- (?P<label>\[[^\]]+\]).*?(?P<fill> -+)?")
_DIRECTIVE = regex.compile(r"@?[a-z][a-z0-9-]*(?::|-(?:ignore|disable|enable|expect-error|nocheck)\b| (?:disable|enable)\b)|noqa\b")
_LEAVES = frozenset({"paragraph", "heading", "fence", "code_block", "table", "html_block", "hr", "front_matter", "definition"})
_ALIGNS = frozendict({"text-align:center": (":-:", "^"), "text-align:right": ("-:", ">")})
_WIDTH = 150
_MARKDOWN = MarkdownIt("commonmark", {"inline_definitions": True, "store_labels": True}).enable("table").use(front_matter_plugin).disable("text_join")

# --- [MODELS] ---------------------------------------------------------------------------


class _Edit(msgspec.Struct, frozen=True, order=True):
    """0-based half-open line range and replacement lines."""

    start: int
    stop: int
    lines: tuple[str, ...]


class _Text(msgspec.Struct, frozen=True):
    """Prose unit at a 0-based line, source after its leader, code spans excluded from prose, and capitalization rule."""

    line: int
    raw: str
    prose: str
    capital: bool


class _Comment(msgspec.Struct, frozen=True):
    line: int
    indent: str
    marker: str


class _Directive(_Comment, frozen=True):
    body: str

    def fixed(self) -> str:
        return f"{self.indent}{self.marker} {self.body}"


class _Remark(_Directive, frozen=True):
    @override
    def fixed(self) -> str:
        return f"{self.indent}{self.marker} {_unpunctuated(_strip_emoji(self.body))}"


class _Divider(_Comment, frozen=True):
    label: str
    full: bool

    def fixed(self) -> str:
        """File-level full dividers end at column 90; other dividers omit fill."""
        line = f"{self.indent}{self.marker} --- {_label(self.label)}"
        return f"{line} ".ljust(90, "-") if self.full and not self.indent else line


class _Doc[T](msgspec.Struct, frozen=True):
    path: Path
    lines: tuple[str, ...]
    parts: T
    edits: int = 0


class _Grammar[T](msgspec.Struct, frozen=True):
    parse: Callable[[Path, tuple[str, ...]], T]
    texts: Callable[[_Doc[T]], Iterable[_Text]]
    fixes: tuple[Callable[[_Doc[T]], Iterable[_Edit]], ...]
    reports: frozendict[Callable[[_Doc[T]], Iterable[tuple[int, str]]], str]

    def lint(self, path: Path) -> _Doc[tuple[str, ...]]:
        """Returns normalized file lines, edit count, and `path:line: message` findings in line order."""

        def edited(doc: _Doc[T], fix: Callable[[_Doc[T]], Iterable[_Edit]]) -> _Doc[T]:
            match sorted(edit for edit in fix(doc) if edit.lines != doc.lines[edit.start : edit.stop]):
                case []:
                    return doc
                case changed:
                    lines = (*(*(*doc.lines[start : edit.start], *edit.lines) for start, edit in zip((0, *(edit.stop for edit in changed)), changed)), *doc.lines[changed[-1].stop :])
                    return msgspec.structs.replace(doc, lines=lines, parts=self.parse(doc.path, lines), edits=doc.edits + len(changed))

        with path.open(encoding="utf-8") as file:
            lines = tuple(line.removesuffix("\n") for line in file)
        doc = reduce(edited, self.fixes, _Doc(path, lines, self.parse(path, lines)))
        texts = [*self.texts(doc)]
        ran = (*((message, report(doc)) for report, message in self.reports.items()), *((message, [(t.line, s) for t in texts for s in report(t)]) for report, message in _TEXTS.items()))
        hits = sorted(((line, message.format(subject)) for message, found in ran for line, subject in found), key=lambda hit: hit[0])
        return _Doc(path, doc.lines, tuple(f"{path}:{line + 1}: {message}\n" for line, message in hits), doc.edits)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TEXT]


def _label(word: str) -> str:
    """Numeric labels use `[NN]`, other labels use `[UPPER_SNAKE]`."""
    inner = regex.sub(r"\W+", "_", word.strip("[] ")).strip("_").upper()
    return f"[{int(inner):02}]" if inner.isdecimal() else f"[{inner}]"


def _strip_emoji(s: str) -> str:
    """Text without emoji and one space beside each."""
    pictograph = "(?:\\p{Extended_Pictographic}[\\p{Emoji_Modifier}\ufe0f]*\u200d?)+"
    return regex.sub(rf" {pictograph}|{pictograph} ?", "", s)


def _unpunctuated(s: str) -> str:
    return regex.sub(r"(?<=[^\s.,;!?])[.,;!?]+$", "", s)


def _token_sources(inline: SyntaxTreeNode) -> list[tuple[Token, str, bool]]:
    """Token/source slices in order; reference regions retain identifier spelling."""
    text, children = inline.content, [*child.to_tokens() for child in inline.children]

    def closed(label: int) -> int:
        opened = len(text) - len(text[label + 2 :].lstrip(" \t\n"))
        destination = parseLinkDestination(text, opened, len(text))
        after = len(text) - len(text[destination.pos if destination.ok else opened :].lstrip(" \t\n"))
        title = parseLinkTitle(text, after, len(text))
        return text.index(")", title.pos if title.ok else after) + 1

    def referenced(label: int) -> int:
        if not text.startswith("[", label + 1):
            return label + 1
        ending = parseLinkLabel(StateInline(text, _MARKDOWN, {}, []), label + 1)
        return label + 1 if ending < 0 else ending + 1

    at = 0
    previous: Token | None = None
    references: list[bool] = []
    sources: list[tuple[Token, str, bool]] = []
    for child in children:
        own_reference = "label" in child.meta
        if child.type == "link_open":
            references.append(own_reference)
        reference = any(references) or (child.type == "image" and own_reference)
        match previous, child:
            case Token(type="link_open", markup="autolink"), Token(type="text"):
                stop = at
            case _, Token(type="code_inline", markup=run):
                _, rest = regex.split(rf"(?<!`){run}(?!`)", text[text.index(run, at) + len(run) :], maxsplit=1)
                stop = len(text) - len(rest)
            case _, Token(type="link_open", markup="autolink"):
                stop = text.index(">", at)
            case _, Token(type="link_close", markup="autolink"):
                references.pop()
                stop = text.index(">", at) + 1
            case _, Token(type="link_open"):
                stop = text.index("[", at) + 1
            case _, Token(type="link_close"):
                label = text.index("]", at)
                stop = referenced(label) if references.pop() else closed(label)
            case _, Token(type="image"):
                label = parseLinkLabel(StateInline(text, _MARKDOWN, {}, []), text.index("![", at) + 1)
                stop = referenced(label) if own_reference else closed(label)
            case _, Token(type="text" | "html_inline", content=content):
                stop = text.index(content, at) + len(content)
            case _, Token(type="softbreak" | "hardbreak"):
                stop = text.index("\n", at) + 1
            case _, Token(markup=mark):
                stop = text.index(mark, at) + len(mark)
        sources.append((child, text[at:stop], reference))
        at, previous = stop, child
    return sources


def _prose(inline: SyntaxTreeNode) -> str:
    """Inline content without code spans."""
    return "".join(s for child, s, _ in _token_sources(inline) if child.type != "code_inline")


# --- [MARKDOWN]


def _parsed(_path: Path, lines: tuple[str, ...]) -> SyntaxTreeNode:
    return SyntaxTreeNode(_MARKDOWN.parse("\n".join(lines)))


def _ranged(nodes: Iterable[SyntaxTreeNode]) -> Iterator[_MappedNode]:
    """Native mapped nodes and line ranges in selection order."""
    return ((node, span) for node in nodes if (span := node.map) is not None)


def _inlines(root: SyntaxTreeNode) -> Iterator[tuple[SyntaxTreeNode, *_MappedNode]]:
    """Blocks, inline children and native line ranges in page order."""
    return ((block, inline, span) for block in root.walk() for inline, span in _ranged(child for child in block.children if child.type == "inline"))


def _starts_item(block: SyntaxTreeNode) -> bool:
    return block.previous_sibling is None and block.parent is not None and block.parent.type == "list_item"


def _leader(inline: SyntaxTreeNode) -> regex.Match[str] | None:
    """Prose leader match whose prefix contains no native reference."""
    matched = _LEADER.match(inline.content)
    return matched if matched is not None and sum(len(source) for _, source, _ in takewhile(lambda piece: not piece[2], _token_sources(inline))) >= matched.end() else None


def _grid(table: SyntaxTreeNode) -> list[list[SyntaxTreeNode]]:
    """Cell inlines by row, header first."""
    return [[*cell.children for cell in row.children] for part in table.children for row in part.children]


def _inline_edit(doc: _Doc[SyntaxTreeNode], inline: SyntaxTreeNode, span: tuple[int, int], text: str) -> _Edit:
    """Single-line replacement preserving the first line's prefix and last line's suffix."""
    start, stop = span
    first, last, opening, closing = doc.lines[start], doc.lines[stop - 1], inline.content.partition("\n")[0], inline.content.rpartition("\n")[2]
    return _Edit(start, stop, (f"{first[: first.rfind(opening)]}{text}{last[last.rfind(closing) + len(closing) :]}",))


def _plain_text(inline: SyntaxTreeNode, *, item: bool) -> str:
    """Inline source without emphasis markers and emoji, an item ending in text without closing punctuation."""
    pieces = _token_sources(inline)
    out = "".join(s if reference else "" if child.tag in {"em", "strong"} else _strip_emoji(s) if child.type == "text" else s for child, s, reference in pieces)
    return _unpunctuated(out) if item and pieces and pieces[-1][0].type == "text" else out


def _trimmed(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Trims trailing prose whitespace, preserving code and reference definitions."""
    verbatim = {*range(*span) for _, span in _ranged(node for node in doc.parts.walk() if node.type in {"fence", "code_block", "definition"})}
    return (_Edit(n, n + 1, (line.rstrip(),)) for n, line in enumerate(doc.lines) if n not in verbatim)


def _joined(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Joins paragraph continuation lines with one space."""
    return (_inline_edit(doc, inline, span, " ".join(line.strip() for line in inline.content.split("\n"))) for block, inline, span in _inlines(doc.parts) if block.type == "paragraph")


def _plain(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    return (_inline_edit(doc, inline, span, _plain_text(inline, item=_starts_item(block))) for block, inline, span in _inlines(doc.parts) if block.type not in {"th", "td"})


def _leaders(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Number cards from `[01]` among siblings with target stems; normalize other bracket leaders."""
    for node in (node for node in doc.parts.walk() if node.type in {"bullet_list", "ordered_list"}):
        n = 0
        for inline, span in _ranged(*block.children for item in node.children for block in item.children[:1] if block.type == "paragraph"):
            match _leader(inline):
                case None:
                    continue
                case m if m["path"] is not None:
                    n += 1
                    text = f"[{n:02}]-{_label(Path(m['path']).stem)}({m['path']}): {inline.content[m.end() :]}"
                case m:
                    text = f"{'-'.join(map(_label, regex.findall(r'\[[^\]]*\]', m['chain'])))}: {inline.content[m.end() :]}"
            yield _inline_edit(doc, inline, span, text)


def _headings(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Number `##` headings as `[NN]` and `###` headings as `[NN.N]` within their parent."""
    h2 = h3 = 0
    for heading, span in _ranged(node for node in doc.parts.children if node.type == "heading" and node.tag in {"h2", "h3"} and any(inline.content for inline in node.children)):
        h2, h3 = (h2 + 1, 0) if heading.tag == "h2" else (h2, h3 + 1)
        title = regex.sub(r"^\[[\d.]+\]-", "", "".join(inline.content for inline in heading.children))
        tokens = title if any(child.type in {"link", "image"} for child in heading.walk()) else "-".join(regex.findall(r"\[[^\]]+\]", title) or [_label(title)])
        yield _Edit(*span, (f"{'#' * int(heading.tag[1])} [{h2:02}{f'.{h3}' if h3 else ''}]-{tokens}",))


def _labels(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """`[TOKEN]` label paragraphs closed with `:` before a list or table."""
    return (
        _inline_edit(doc, inline, span, f"{inline.content}:")
        for block, inline, span in _inlines(doc.parts)
        if block.type == "paragraph"
        and block.next_sibling is not None
        and block.next_sibling.type in {"bullet_list", "ordered_list", "table"}
        and regex.fullmatch(r"\[[A-Z_]+\]", inline.content)
        and not any(child.type == "link" for child in inline.children)
    )


def _overflow_rows(doc: _Doc[SyntaxTreeNode], table: SyntaxTreeNode) -> list[int]:
    """Parser drops excess cells; source rows retain overflow diagnostics."""
    head, *body = (span[0] for _, span in _ranged(row for row in table.walk() if row.type == "tr"))
    columns = len(escapedSplit(doc.lines[head].strip()))
    return [n for n in body if len(escapedSplit(doc.lines[n].strip())) > columns]


def _tables(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Formats non-overflowing tables with bracket headers, aligned cells, and an index for multiple rows or an existing index column."""

    def rendered(table: SyntaxTreeNode, span: tuple[int, int]) -> _Edit:
        start, stop = span
        head, *rows = _grid(table)
        names = [
            "".join(s if reference else "" if child.tag in {"em", "strong"} else _label(s) if child.type == "text" and s.strip() else s for child, s, reference in _token_sources(inline))
            for inline in head
        ]
        body = [[_plain_text(inline, item=True) for inline in row] for row in rows]
        aligns = [_ALIGNS.get(str(cell.attrGet("style")), (":-", "<")) for cell in table.walk() if cell.type == "th"]
        match names:
            case ["[INDEX]", *rest]:
                names, aligns, body, indexed = rest, aligns[1:], [row[1:] for row in body], True
            case _:
                indexed = len(body) > 1
        grid = [[c.replace("|", "\\|") for c in row] for row in ([["[INDEX]", *names], *([f"[{n:02}]", *row] for n, row in enumerate(body, 1))] if indexed else [names, *body])]
        marks = [_ALIGNS["text-align:center"], *aligns] if indexed else aligns
        widths = [max(len(mark), *map(wcswidth, column)) for (mark, _), column in zip(marks, zip(*grid))]
        indent = doc.lines[start][: len(doc.lines[start]) - len(doc.lines[start].lstrip(" >"))]
        named, *piped = (f"{indent}| {' | '.join(format(c, f'{spec}{w + len(c) - wcswidth(c)}') for (_, spec), c, w in zip(marks, row, widths))} |" for row in grid)
        rule = f"{indent}| {' | '.join(mark.replace('-', '-' * (w - len(mark) + 1)) for (mark, _), w in zip(marks, widths))} |"
        return _Edit(start, stop, (named, rule, *piped))

    return (rendered(node, span) for node, span in _ranged(node for node in doc.parts.walk() if node.type == "table") if not _overflow_rows(doc, node))


def _spacing(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """One blank follows headings and surrounds tables; labels touch their lists, other gaps keep at most one blank, and file ends keep none."""
    leaves = [*_ranged(node for node in doc.parts.walk() if node.type in _LEAVES)]

    def gap(a: _MappedNode | None, b: _MappedNode | None) -> _Edit:
        start, stop = a[1][1] if a else 0, b[1][0] if b else len(doc.lines)
        match a[0] if a else None, b[0] if b else None:
            case (None, _) | (_, None):
                need = 0
            case (SyntaxTreeNode(type="heading" | "table"), _) | (_, SyntaxTreeNode(type="table")):
                need = 1
            case _, SyntaxTreeNode() as item if _starts_item(item) and doc.lines[start - 1].endswith(":"):
                need = 0
            case _:
                need = min(stop - start, 1)
        return _Edit(start, stop, ("",) * need if not "".join(doc.lines[start:stop]).strip() else doc.lines[start:stop])

    return starmap(gap, pairwise((None, *leaves, None)))


def _markdown_texts(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Text]:

    def text(block: SyntaxTreeNode, inline: SyntaxTreeNode, span: tuple[int, int]) -> _Text:
        lead = _leader(inline) if block.type == "paragraph" else None
        return _Text(span[0], inline.content[lead.end() if lead else 0 :], _prose(inline), capital=block.type in {"th", "td"} or _starts_item(block))

    return starmap(text, _inlines(doc.parts))


def _h1s(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Reports repeated H1s and titles outside `[TOKEN]` form."""
    ones = [(span[0], inline.content) for node, span in _ranged(node for node in doc.parts.children if node.type == "heading" and node.tag == "h1") for inline in node.children]
    return [(n, doc.lines[n]) for k, (n, title) in enumerate(ones) if k or title != _label(title)]


def _ordinals(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Paragraph lines opening with a dotted number marker."""
    paragraphs = _ranged(node for node in doc.parts.walk() if node.type == "paragraph")
    return [(n, m[0]) for _, span in paragraphs for n in range(*span) if (m := regex.match(r"\d+(?:\.\w+)+\.(?= )", doc.lines[n].lstrip()))]


def _dead_links(doc: _Doc[SyntaxTreeNode]) -> Iterator[tuple[int, str]]:
    for _, inline, span in _inlines(doc.parts):
        for node in inline.walk():
            match node:
                case SyntaxTreeNode(type="link", attrs={"href": str(target)}) | SyntaxTreeNode(type="image", attrs={"src": str(target)}) if (
                    (target := _MARKDOWN.normalizeLinkText(target).partition("#")[0]) and ":" not in target and not (doc.path.parent / target).exists()
                ):
                    yield span[0], target


def _overflows(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    return [(n, "") for node in doc.parts.walk() if node.type == "table" for n in _overflow_rows(doc, node)]


def _wide(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Code spans do not count toward list widths; complete table rows do."""
    items = [
        (n, "List item") for block, inline, span in _inlines(doc.parts) if _starts_item(block) and wcswidth(doc.lines[n := span[0]]) - wcswidth(inline.content) + wcswidth(_prose(inline)) > _WIDTH
    ]
    tables = _ranged(node for node in doc.parts.walk() if node.type == "table")
    return [*items, *((span[0], "Table") for _, span in tables if any(wcswidth(line) > _WIDTH for line in doc.lines[slice(*span)]))]


def _enumerations(root: SyntaxTreeNode) -> list[str]:
    """Code spans from `[WORDS]` cells whose `[CATEGORY]` is `Enumeration`."""

    def words(head: list[SyntaxTreeNode], row: list[SyntaxTreeNode]) -> list[str]:
        match {name.content: cell for name, cell in zip(head, row)}:
            case {"[CATEGORY]": SyntaxTreeNode(content="Enumeration"), "[WORDS]": cell}:
                return [child.content for child in cell.walk() if child.type == "code_inline"]
            case _:
                return []

    return [*words(head, row) for table in root.walk() if table.type == "table" for head, *body in (_grid(table),) for row in body]


# --- [SOURCE]


def _comments(path: Path, lines: tuple[str, ...]) -> _Comments:
    """Full-line single-line comments at 0-based source lines, classified by the filename's lexer as directive, remark, or divider."""
    starts = [*accumulate((len(line) + 1 for line in lines), initial=0)]

    def classified(line: int, at: int, value: str) -> _Comments:
        indent = lines[line][: at - starts[line]]
        match regex.fullmatch(r"(?P<marker>\p{P}+) (?P<body>\S.*?)\s*", value) if not indent.strip() else None:
            case None:
                return ()
            case comment if divider := _DIVIDER.fullmatch(comment["body"]):
                return (_Divider(line, indent, comment["marker"], divider["label"], full=divider["fill"] is not None),)
            case comment:
                return ((_Directive if _DIRECTIVE.match(comment["body"]) else _Remark)(line, indent, comment["marker"], comment["body"]),)

    tokens = get_lexer_for_filename(path.name, stripnl=False, ensurenl=False).get_tokens_unprocessed("".join(f"{line}\n" for line in lines))
    runs = groupby(tokens, key=lambda token: (is_token_subtype(token[1], Comment.Single), bisect_right(starts, token[0]) - 1))
    return tuple(*classified(line, min(offsets), "".join(values)) for (comment, line), run in runs if comment for offsets, _, values in [zip(*run)])


def _comment_edits(doc: _Doc[_Comments]) -> Iterator[_Edit]:
    return (_Edit(note.line, note.line + 1, (note.fixed(),)) for note in doc.parts)


def _empty_sections(doc: _Doc[_Comments]) -> Iterator[_Edit]:
    """Delete dividers with no content before the next equal/higher divider, following blanks, and trailing blanks after a deletion."""
    dividers = {note.line: note for note in doc.parts if isinstance(note, _Divider)}
    content = section = following = solid = len(doc.lines)
    drops: set[int] = set()
    for n in (n for n in reversed(range(len(doc.lines))) if doc.lines[n].strip()):
        match dividers.get(n):
            case _Divider(full=full):
                if content >= (section if full else following):
                    drops.update(range(n, solid))
                section = n if full else section
                following = solid = n
            case None:
                content = solid = n
    tail = {*takewhile(lambda n: n in drops or not doc.lines[n].strip(), reversed(range(len(doc.lines))))} if drops else set()
    return (_Edit(n, n + 1, ()) for n in drops | tail)


def _comment_texts(doc: _Doc[_Comments]) -> Iterator[_Text]:
    return (_Text(note.line, note.body, _prose(SyntaxTreeNode(_MARKDOWN.parseInline(note.body))[0]), capital=True) for note in doc.parts if isinstance(note, _Remark))


def _repeated_labels(doc: _Doc[_Comments]) -> list[tuple[int, str]]:
    """Only full dividers share a file-wide label namespace."""
    first: dict[str, int] = {}
    return [(note.line, note.label) for note in doc.parts if isinstance(note, _Divider) and note.full and first.setdefault(note.label, note.line) != note.line]


def _orphan_dividers(doc: _Doc[_Comments]) -> list[tuple[int, str]]:
    """Subdividers require a preceding full divider."""
    return [(note.line, note.label) for note in takewhile(lambda note: not note.full, (note for note in doc.parts if isinstance(note, _Divider)))]


# --- [REGISTRY] -------------------------------------------------------------------------

_COUNTED = regex.compile(
    rf"(?i)(?<!\ba )\b(?:{'|'.join(map(regex.escape, _enumerations(SyntaxTreeNode(_MARKDOWN.parse((Path(__file__).parents[1] / 'references' / 'word-map.md').read_text(encoding='utf-8'))))))})\b(?!-\w)"
)
_TEXTS: frozendict[Callable[[_Text], list[str]], str] = frozendict({
    (lambda t: regex.findall(r"(?i)\A(?:a|an|the)\s+\S", t.raw)): "Article opener",
    (lambda t: regex.findall(r"\A\p{Ll}", t.raw) if t.capital else []): "Lowercase opener",
    (lambda t: _COUNTED.findall(t.prose)): "Visible item count `{}`",
})
_PAGES = _Grammar(
    _parsed,
    _markdown_texts,
    (_trimmed, _joined, _plain, _leaders, _headings, _labels, _tables, _spacing),
    frozendict({
        _h1s: "Extra or malformed H1 `{}`",
        _ordinals: "Invalid list marker `{}`",
        _dead_links: "Missing target `{}`",
        _overflows: "Table row has excess cells",
        _wide: f"{{}} exceeds {_WIDTH} columns",
    }),
)
_CODE = _Grammar(_comments, _comment_texts, (_comment_edits, _empty_sections), frozendict({_repeated_labels: "Repeated section label `{}`", _orphan_dividers: "Subsection `{}` precedes any section"}))

# --- [COMPOSITION] ----------------------------------------------------------------------


def _linted(paths: tuple[Path, ...]) -> list[_Doc[tuple[str, ...]]]:
    """Lint named files or Git-visible folder contents, Markdown first, excluding symlinks; no paths selects the working directory."""
    named, folders = [p for p in paths if p.is_file()], [p for p in paths if p.is_dir()]
    listed = subprocess.check_output(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard", "--deduplicate", "--", *folders], text=True) if folders or not named else ""
    found = [f for f in sorted({*named, *(Path(f) for f in listed.split("\0") if f)}) if f.is_file() and not f.is_symlink()]
    return [
        *(_PAGES.lint(f) for f in found if f.suffix == ".md"),
        *(_CODE.lint(f) for f in found if f.suffix in {".py", ".sh", ".toml", ".nix", ".yml", ".yaml", ".cs", ".ts", ".tsx", ".js", ".jsx", ".java", ".swift", ".sql", ".lua"}),
    ]


def check(*paths: ExistingPath) -> int:
    """Report fix commands and remaining findings; exit 1 for either."""
    linted = _linted(paths)
    found = [*d.parts for d in linted]
    total = sum(d.edits for d in linted)
    sys.stdout.writelines((
        *(f"{d.path}: {d.edits} fixable, {shlex.join((sys.executable, sys.argv[0], 'fix', str(d.path)))}\n" for d in linted if d.edits),
        *found,
        f"Found {len(found) + total} findings, {total} fixable\n",
    ))
    return int(bool(found or total))


def fix(*paths: ExistingPath) -> int:
    """Write fixes and report remaining findings; exit 1 when findings remain."""
    linted = _linted(paths)
    for d in (d for d in linted if d.edits):
        d.path.write_text("".join(f"{line}\n" for line in d.lines), encoding="utf-8")
    left = [*d.parts for d in linted]
    sys.stdout.writelines((*left, f"Fixed {sum(d.edits for d in linted)}, {len(left)} remaining\n"))
    return int(bool(left))


app = App(help=__doc__)
app.command(check)
app.command(fix)

if __name__ == "__main__":
    app()

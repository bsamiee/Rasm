"""Checks and fixes house style of markdown pages and source comments."""

from functools import cache
from pathlib import Path
from typing import override
lazy from bisect import bisect_right
lazy from collections.abc import Callable, Iterable, Iterator
lazy from functools import reduce
lazy from itertools import accumulate, groupby, pairwise, starmap, takewhile
lazy import subprocess
lazy import sys

from cyclopts import App
from cyclopts.types import ExistingPath
from markdown_it import MarkdownIt
from markdown_it.tree import SyntaxTreeNode
from mdit_py_plugins.front_matter import front_matter_plugin
import msgspec
import regex
lazy from editorconfig import get_properties
lazy from expression.extra.result import catch
lazy from markdown_it.helpers import parseLinkDestination, parseLinkLabel, parseLinkTitle
lazy from markdown_it.rules_block.table import escapedSplit
lazy from markdown_it.rules_inline import StateInline
lazy from markdown_it.token import Token
lazy from pygments.lexers import find_lexer_class_by_name, get_lexer_for_filename
lazy from pygments.lexers.special import TextLexer
lazy from pygments.token import Comment, is_token_subtype
lazy from pygments.util import ClassNotFound
lazy from wcwidth import wcswidth

# --- [CONSTANTS] ------------------------------------------------------------------------

_LEADER = regex.compile(r"(?P<chain>\[[^\]]*\](?:-\[[^\]]*\])*)(?:\((?P<path>[^)]*)\):|:?) ")
_DIVIDER = regex.compile(r"--- (?P<tok>\[[^\]]+\]).*?(?P<fill> -+)?")
_DIRECTIVE = regex.compile(r"@?[a-z][a-z0-9-]*(?::|-(?:ignore|disable|enable|expect-error|nocheck)\b| (?:disable|enable)\b)|noqa\b")
_LEAVES = frozenset({"paragraph", "heading", "fence", "code_block", "table", "html_block", "hr", "front_matter"})
_ALIGNS = frozendict({"text-align:center": (":-:", "^"), "text-align:right": ("-:", ">")})
_WIDTH = 150
_MARKDOWN = MarkdownIt("commonmark").enable("table").use(front_matter_plugin).disable(["text_join", "reference"])

# --- [MODELS] ---------------------------------------------------------------------------


class _Edit(msgspec.Struct, frozen=True, order=True):
    """0-based line range and the lines replacing it."""

    start: int
    stop: int
    lines: tuple[str, ...]


class _Text(msgspec.Struct, frozen=True):
    """Text unit at a 0-based line, raw text after any leader, its prose outside code spans, and whether it must open with a capital."""

    line: int
    raw: str
    prose: str
    capital: bool


class _Note(msgspec.Struct, frozen=True):
    """Full-line comment at a 0-based line, `head` its indent, a tool directive unless a case below."""

    line: int
    head: str
    marker: str
    body: str

    def fixed(self) -> str:
        """Directive as written."""
        return f"{self.head}{self.marker} {self.body}"


class _Remark(_Note, frozen=True):
    @override
    def fixed(self) -> str:
        """Prose comment without emoji and closing punctuation."""
        return f"{self.head}{self.marker} {_unpunctuated(_emoji(self.body))}"


class _Divider(_Note, frozen=True):
    tok: str
    full: bool

    @override
    def fixed(self) -> str:
        """Divider as `<marker> --- [TOKEN]` of label `tok`, filled to column 90 when `full` at file level, else unfilled."""
        line = f"{self.head}{self.marker} --- {_token(self.tok)}"
        return f"{line} ".ljust(90, "-") if self.full and not self.head else line


class _Doc[T](msgspec.Struct, frozen=True):
    """File lines, the parts its grammar parsed from them, and the count of edits that changed lines."""

    path: Path
    lines: tuple[str, ...]
    parts: T
    edits: int = 0


class _Grammar[T](msgspec.Struct, frozen=True):
    """Parse, text units, fixes, and each report's message template of one file grammar."""

    parse: Callable[[Path, tuple[str, ...]], T]
    texts: Callable[[_Doc[T]], Iterable[_Text]]
    fixes: tuple[Callable[[_Doc[T]], Iterable[_Edit]], ...]
    reports: frozendict[Callable[[_Doc[T]], Iterable[tuple[int, str]]], str]

    def lint(self, path: Path) -> _Doc[tuple[str, ...]]:
        """Lines of `path` after every fix, the count of edits that changed lines, and as parts each `path:line: message` the reports and `_TEXTS` make, in line order."""

        def edited(doc: _Doc[T], fix: Callable[[_Doc[T]], Iterable[_Edit]]) -> _Doc[T]:
            match sorted((edit for edit in fix(doc) if edit.lines != doc.lines[edit.start : edit.stop]), reverse=True):
                case []:
                    return doc
                case changed:
                    lines = reduce(lambda done, edit: (*done[: edit.start], *edit.lines, *done[edit.stop :]), changed, doc.lines)
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


def _token(word: str) -> str:
    """`[NN]` for a number, `[UPPER_SNAKE]` for words, outer brackets dropped."""
    inner = regex.sub(r"\W+", "_", word.strip("[] ")).strip("_").upper()
    return f"[{int(inner):02}]" if inner.isdecimal() else f"[{inner}]"


def _emoji(s: str) -> str:
    """Text without emoji and one space beside each."""
    pictograph = "(?:\\p{Extended_Pictographic}[\\p{Emoji_Modifier}\ufe0f]*\u200d?)+"
    return regex.sub(rf" {pictograph}|{pictograph} ?", "", s)


def _unpunctuated(s: str) -> str:
    """Text without closing sentence punctuation."""
    return regex.sub(r"(?<=[^\s.,;!?])[.,;!?]+$", "", s)


def _pieces(inline: SyntaxTreeNode) -> list[tuple[Token, str]]:
    """Inline tokens with their source slices, the content partitioned in token order."""
    text, children = inline.content, [*child.to_tokens() for child in inline.children]

    def closed(label: int) -> int:
        opened = len(text) - len(text[label + 2 :].lstrip(" \t\n"))
        destination = parseLinkDestination(text, opened, len(text))
        after = len(text) - len(text[destination.pos if destination.ok else opened :].lstrip(" \t\n"))
        title = parseLinkTitle(text, after, len(text))
        return text.index(")", title.pos if title.ok else after) + 1

    def end(at: int, pair: tuple[Token | None, Token]) -> int:
        match pair:
            case Token(type="link_open", markup="autolink"), Token(type="text"):
                return at
            case _, Token(type="code_inline", markup=run):
                _, rest = regex.split(rf"(?<!`){run}(?!`)", text[text.index(run, at) + len(run) :], maxsplit=1)
                return len(text) - len(rest)
            case _, Token(type="link_open", markup="autolink"):
                return text.index(">", at)
            case _, Token(type="link_close", markup="autolink"):
                return text.index(">", at) + 1
            case _, Token(type="link_open"):
                return text.index("[", at) + 1
            case _, Token(type="link_close"):
                return closed(text.index("]", at))
            case _, Token(type="image"):
                return closed(parseLinkLabel(StateInline(text, _MARKDOWN, {}, []), text.index("![", at) + 1))
            case _, Token(type="text" | "html_inline", content=content):
                return text.index(content, at) + len(content)
            case _, Token(type="softbreak" | "hardbreak"):
                return text.index("\n", at) + 1
            case _, Token(markup=mark):
                return text.index(mark, at) + len(mark)

    return [*zip(children, (text[a:b] for a, b in pairwise(accumulate(zip((None, *children), children), end, initial=0))))]


def _prose(inline: SyntaxTreeNode) -> str:
    """Inline content without code spans."""
    return "".join(s for child, s in _pieces(inline) if child.type != "code_inline")


# --- [MARKDOWN]


def _span(node: SyntaxTreeNode) -> tuple[int, int]:
    """0-based line range of a block node."""
    match node.map:
        case (start, stop):
            return start, stop
        case None:
            raise TypeError(f"{node.type} node holds no line range")


def _parsed(_path: Path, lines: tuple[str, ...]) -> SyntaxTreeNode:
    """Syntax tree of a page."""
    return SyntaxTreeNode(_MARKDOWN.parse("\n".join(lines)))


def _inlines(root: SyntaxTreeNode) -> Iterator[tuple[SyntaxTreeNode, SyntaxTreeNode]]:
    """Block and inline child of every block holding text, in page order."""
    return ((block, inline) for block in root.walk() for inline in block.children if inline.type == "inline")


def _opens(block: SyntaxTreeNode) -> bool:
    """Whether `block` opens a list item."""
    return block.previous_sibling is None and block.parent is not None and block.parent.type == "list_item"


def _grid(table: SyntaxTreeNode) -> list[list[SyntaxTreeNode]]:
    """Inline node of each cell by row, header row first."""
    return [[*cell.children for cell in row.children] for part in table.children for row in part.children]


def _retexted(doc: _Doc[SyntaxTreeNode], inline: SyntaxTreeNode, text: str) -> _Edit:
    """Edit replacing the inline's line range with one line, the first line's prefix and the last line's suffix kept around `text`."""
    start, stop = _span(inline)
    first, last, opening, closing = doc.lines[start], doc.lines[stop - 1], inline.content.partition("\n")[0], inline.content.rpartition("\n")[2]
    return _Edit(start, stop, (f"{first[: first.rfind(opening)]}{text}{last[last.rfind(closing) + len(closing) :]}",))


def _plain_text(inline: SyntaxTreeNode, *, item: bool) -> str:
    """Inline source without emphasis markers and emoji, an item ending in text without closing punctuation."""
    pieces = _pieces(inline)
    out = "".join("" if child.tag in {"em", "strong"} else _emoji(s) if child.type == "text" else s for child, s in pieces)
    return _unpunctuated(out) if item and pieces and pieces[-1][0].type == "text" else out


def _trimmed(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Lines outside code blocks without trailing whitespace."""
    code = {n for node in doc.parts.walk() if node.type in {"fence", "code_block"} for n in range(*_span(node))}
    return (_Edit(n, n + 1, (line.rstrip(),)) for n, line in enumerate(doc.lines) if n not in code)


def _joined(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Paragraphs as one line, content lines joined by one space."""
    return (_retexted(doc, inline, " ".join(line.strip() for line in inline.content.split("\n"))) for block, inline in _inlines(doc.parts) if block.type == "paragraph")


def _plain(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Inlines outside tables without emphasis markers and emoji, list items without closing punctuation."""
    return (_retexted(doc, inline, _plain_text(inline, item=_opens(block))) for block, inline in _inlines(doc.parts) if block.type not in {"th", "td"})


def _leaders(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """List items with leader tokens joined by `-` before `: `, link cards numbered from 01 among siblings with the path stem as token."""

    def led(inline: SyntaxTreeNode, n: int) -> str:
        match _LEADER.match(inline.content):
            case None:
                return inline.content
            case m if m["path"] is not None:
                return f"[{n:02}]-{_token(Path(m['path']).stem)}({m['path']}): {inline.content[m.end() :]}"
            case m:
                return f"{'-'.join(map(_token, regex.findall(r'\[[^\]]*\]', m['chain'])))}: {inline.content[m.end() :]}"

    def renumbered(items: list[SyntaxTreeNode]) -> Iterator[_Edit]:
        cards = accumulate(int(bool((m := _LEADER.match(inline.content)) and m["path"] is not None)) for inline in items)
        return (_retexted(doc, inline, led(inline, n)) for inline, n in zip(items, cards))

    lists = (node for node in doc.parts.walk() if node.type in {"bullet_list", "ordered_list"})
    return (*renumbered([*block.children for item in node.children for block in item.children[:1] if block.type == "paragraph"]) for node in lists)


def _headings(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """`##` headings with a title numbered from 01 and `###` restarting under each `##`, as the number then title tokens joined by `-`."""
    headings = [node for node in doc.parts.children if node.type == "heading" and node.tag in {"h2", "h3"} and any(inline.content for inline in node.children)]

    def numbered(heading: SyntaxTreeNode, h2: int, h3: int) -> _Edit:
        title = regex.sub(r"^\[[\d.]+\]-", "", "".join(inline.content for inline in heading.children))
        tokens = "-".join(regex.findall(r"\[[^\]]+\]", title) or [_token(title)])
        return _Edit(*_span(heading), (f"{'#' * int(heading.tag[1])} [{h2:02}{f'.{h3}' if h3 else ''}]-{tokens}",))

    numbers = accumulate(headings, lambda number, heading: (number[0] + 1, 0) if heading.tag == "h2" else (number[0], number[1] + 1), initial=(0, 0))
    return (numbered(heading, *number) for heading, number in zip(headings, [*numbers][1:]))


def _labels(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """`[TOKEN]` label paragraphs closed with `:` before a list or table."""
    labels = (node for node in doc.parts.walk() if node.type == "paragraph" and node.next_sibling is not None and node.next_sibling.type in {"bullet_list", "ordered_list", "table"})
    return (_retexted(doc, inline, f"{inline.content}:") for node in labels for inline in node.children if regex.fullmatch(r"\[[A-Z_]+\]", inline.content))


def _crowded(doc: _Doc[SyntaxTreeNode], table: SyntaxTreeNode) -> list[int]:
    """0-based lines of table rows splitting into more cells than the header row, the cells past it dropped by the parser."""
    head, *body = (_span(row)[0] for row in table.walk() if row.type == "tr")
    return [n for n in body if len(escapedSplit(doc.lines[n].strip())) > len(escapedSplit(doc.lines[head].strip()))]


def _tables(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Tables holding no crowded row with `[UPPER_SNAKE]` headers, an `[INDEX]` column over two or more rows, and cells without closing punctuation, padded to column width."""

    def rendered(table: SyntaxTreeNode) -> _Edit:
        start, stop = _span(table)
        head, *rows = _grid(table)
        names = ["".join("" if child.tag in {"em", "strong"} else _token(s) if child.type == "text" and s.strip() else s for child, s in _pieces(inline)) for inline in head]
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

    return (rendered(node) for node in doc.parts.walk() if node.type == "table" and not _crowded(doc, node))


def _spacing(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    """Blank runs between leaf blocks, one after heading or around table, none after `:` before list item, else at most one, none at file ends."""
    leaves = [node for node in doc.parts.walk() if node.type in _LEAVES]

    def gap(a: SyntaxTreeNode | None, b: SyntaxTreeNode | None) -> _Edit:
        start, stop = _span(a)[1] if a else 0, _span(b)[0] if b else len(doc.lines)
        match a, b:
            case (None, _) | (_, None):
                need = 0
            case (SyntaxTreeNode(type="heading" | "table"), _) | (_, SyntaxTreeNode(type="table")):
                need = 1
            case _, SyntaxTreeNode() as item if _opens(item) and doc.lines[start - 1].endswith(":"):
                need = 0
            case _:
                need = min(stop - start, 1)
        return _Edit(start, stop, ("",) * need if not "".join(doc.lines[start:stop]).strip() else doc.lines[start:stop])

    return starmap(gap, pairwise((None, *leaves, None)))


def _markdown_texts(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Text]:
    """Text units of headings, paragraphs, list items, and table cells."""

    def text(block: SyntaxTreeNode, inline: SyntaxTreeNode) -> _Text:
        lead = _LEADER.match(inline.content) if block.type == "paragraph" else None
        return _Text(_span(inline)[0], inline.content[lead.end() if lead else 0 :], _prose(inline), capital=block.type in {"th", "td"} or _opens(block))

    return starmap(text, _inlines(doc.parts))


def _h1s(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Every later H1, and the first H1 when its title is not one `[TOKEN]`."""
    ones = [(_span(node)[0], inline.content) for node in doc.parts.children if node.type == "heading" and node.tag == "h1" for inline in node.children]
    return [(n, doc.lines[n]) for k, (n, title) in enumerate(ones) if k or title != _token(title)]


def _ordinals(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Paragraph lines opening with a dotted number marker."""
    paragraphs = (node for node in doc.parts.walk() if node.type == "paragraph")
    return [(n, m[0]) for node in paragraphs for n in range(*_span(node)) if (m := regex.match(r"\d+(?:\.\w+)+\.(?= )", doc.lines[n].lstrip()))]


def _dead(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Relative link and image targets naming no file."""

    def targets(node: SyntaxTreeNode) -> list[str]:
        match node:
            case SyntaxTreeNode(type="link", attrs={"href": str(target)}) | SyntaxTreeNode(type="image", attrs={"src": str(target)}):
                return [_MARKDOWN.normalizeLinkText(target).partition("#")[0]]
            case _:
                return []

    found = ((_span(inline)[0], target) for _, inline in _inlines(doc.parts) for node in inline.walk() for target in targets(node))
    return [(n, target) for n, target in found if target and ":" not in target and not (doc.path.parent / target).exists()]


def _overflows(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Table rows holding cells past the header row's columns."""
    return [(n, "") for node in doc.parts.walk() if node.type == "table" for n in _crowded(doc, node)]


def _wide(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """List items past column `_WIDTH` outside code spans, then tables with a line past column `_WIDTH`, each named by its kind."""
    items = [(n, "List item") for block, inline in _inlines(doc.parts) if _opens(block) and wcswidth(doc.lines[n := _span(inline)[0]]) - wcswidth(inline.content) + wcswidth(_prose(inline)) > _WIDTH]
    return [*items, *((_span(node)[0], "Table") for node in doc.parts.walk() if node.type == "table" and any(wcswidth(line) > _WIDTH for line in doc.lines[slice(*_span(node))]))]


@cache
def _steps(path: Path, info: str) -> range:
    """Indent steps below the `.editorconfig` space indent size of the `info` language at `path`, none for an unknown language."""
    patterns = catch(find_lexer_class_by_name, exception=ClassNotFound)(info).map(lambda lexer: () if lexer is TextLexer else lexer.filenames).default_value(())
    props = get_properties(str(path.with_name(patterns[0].replace("*", path.stem)))) if patterns else {}
    return range(2, int(props["indent_size"])) if props.get("indent_style") == "space" else range(0)


def _shallow(doc: _Doc[SyntaxTreeNode]) -> list[tuple[int, str]]:
    """Fence lines stepping past the nearest line at or below their depth by 2 to under indent size columns, list item depth from content after `- `."""

    def opened(fence: SyntaxTreeNode) -> list[tuple[int, str]]:
        allowed = _steps(doc.path.resolve(), fence.info.partition(" ")[0])
        solid = [
            (_span(fence)[0] + 1 + k, body.strip(), (n := len(line) - len(body)), n + 2 if body.startswith("- ") else n)
            for k, line in enumerate(fence.content.split("\n"))
            if (body := line.lstrip(" ")).strip()
        ]
        return [(at, text) for i, (at, text, n, _) in enumerate(solid) if n - next((depth for *_, depth in reversed(solid[:i]) if depth <= n), 0) in allowed]

    return [*opened(fence) for fence in doc.parts.walk() if fence.type == "fence"]


def _words(root: SyntaxTreeNode) -> list[str]:
    """Code span words of each table row whose `[CATEGORY]` cell is `Enumeration`, read from its `[WORDS]` cell."""

    def words(head: list[SyntaxTreeNode], row: list[SyntaxTreeNode]) -> list[str]:
        match {name.content: cell for name, cell in zip(head, row)}:
            case {"[CATEGORY]": SyntaxTreeNode(content="Enumeration"), "[WORDS]": cell}:
                return [child.content for child in cell.walk() if child.type == "code_inline"]
            case _:
                return []

    return [*words(head, row) for table in root.walk() if table.type == "table" for head, *body in (_grid(table),) for row in body]


# --- [SOURCE]


def _noted(path: Path, lines: tuple[str, ...]) -> tuple[_Note, ...]:
    """Full-line single-line comments the lexer of `path` finds, the comment tokens of one line joined, each as a divider, directive, or remark."""
    starts = [*accumulate((len(line) + 1 for line in lines), initial=0)]

    def note(line: int, at: int, value: str) -> list[_Note]:
        head = lines[line][: at - starts[line]]
        match regex.fullmatch(r"(?P<marker>\p{P}+) (?P<body>\S.*?)\s*", value) if not head.strip() else None:
            case None:
                return []
            case comment if divider := _DIVIDER.fullmatch(comment["body"]):
                return [_Divider(line, head, comment["marker"], comment["body"], divider["tok"], full=divider["fill"] is not None)]
            case comment:
                return [(_Note if _DIRECTIVE.match(comment["body"]) else _Remark)(line, head, comment["marker"], comment["body"])]

    tokens = get_lexer_for_filename(path.name, stripnl=False, ensurenl=False).get_tokens_unprocessed("".join(f"{line}\n" for line in lines))
    runs = groupby(tokens, key=lambda token: (is_token_subtype(token[1], Comment.Single), bisect_right(starts, token[0]) - 1))
    return tuple(*note(line, min(offsets), "".join(values)) for (comment, line), run in runs if comment for offsets, _, values in [zip(*run)])


def _comments(doc: _Doc[tuple[_Note, ...]]) -> Iterator[_Edit]:
    """Comment lines in the form each comment case fixes."""
    return (_Edit(note.line, note.line + 1, (note.fixed(),)) for note in doc.parts)


def _unlabeled(doc: _Doc[tuple[_Note, ...]]) -> Iterator[_Edit]:
    """Dividers lacking comment or code before the next divider at or above their level, with the blank lines after them, and trailing blank lines once any drops."""
    dividers = [note for note in doc.parts if isinstance(note, _Divider)]
    marks = {divider.line for divider in dividers}

    def empty(divider: _Divider) -> bool:
        stop = next((d.line for d in dividers if d.line > divider.line and (d.full or not divider.full)), len(doc.lines))
        return not any(doc.lines[n].strip() and n not in marks for n in range(divider.line + 1, stop))

    drops = {n for divider in dividers if empty(divider) for n in range(divider.line, next((k for k in range(divider.line + 1, len(doc.lines)) if doc.lines[k].strip()), len(doc.lines)))}
    tail = {*takewhile(lambda n: n in drops or not doc.lines[n].strip(), reversed(range(len(doc.lines))))} if drops else set()
    return (_Edit(n, n + 1, ()) for n in drops | tail)


def _source_texts(doc: _Doc[tuple[_Note, ...]]) -> Iterator[_Text]:
    """Text units of prose comments."""
    return (_Text(note.line, note.body, _prose(SyntaxTreeNode(_MARKDOWN.parseInline(note.body))[0]), capital=True) for note in doc.parts if isinstance(note, _Remark))


def _repeats(doc: _Doc[tuple[_Note, ...]]) -> list[tuple[int, str]]:
    """Full dividers with a token an earlier full divider used."""
    fulls = [note for note in doc.parts if isinstance(note, _Divider) and note.full]
    return [(divider.line, divider.tok) for i, divider in enumerate(fulls) if any(earlier.tok == divider.tok for earlier in fulls[:i])]


def _orphans(doc: _Doc[tuple[_Note, ...]]) -> list[tuple[int, str]]:
    """Sub dividers before the first full divider."""
    return [(note.line, note.tok) for note in takewhile(lambda note: not note.full, (note for note in doc.parts if isinstance(note, _Divider)))]


# --- [REGISTRY] -------------------------------------------------------------------------

_COUNTED = regex.compile(
    rf"(?i)(?<!\ba )\b(?:{'|'.join(map(regex.escape, _words(SyntaxTreeNode(_MARKDOWN.parse((Path(__file__).parents[1] / 'references' / 'word-map.md').read_text(encoding='utf-8'))))))})\b(?!-\w)"
)
_TEXTS: frozendict[Callable[[_Text], list[str]], str] = frozendict({
    (lambda t: regex.findall(r"(?i)\A(?:a|an|the)\s+\S", t.raw)): "Text opens with an article",
    (lambda t: regex.findall(r"\A\p{Ll}", t.raw) if t.capital else []): "Text opens with a lowercase letter",
    (lambda t: _COUNTED.findall(t.prose)): "Text counts visible items with `{}`",
})
_PAGES = _Grammar(
    _parsed,
    _markdown_texts,
    (_trimmed, _joined, _plain, _leaders, _headings, _labels, _tables, _spacing),
    frozendict({
        _h1s: "`{}` is not the one `[TOKEN]` H1",
        _ordinals: "Marker `{}` opens no Markdown list item",
        _dead: "`{}` resolves to no file",
        _overflows: "Table row holds cells past its header columns",
        _wide: f"{{}} runs past column {_WIDTH}",
        _shallow: "Fence line `{}` indents by fewer columns than `.editorconfig` sets for its language",
    }),
)
_CODE = _Grammar(_noted, _source_texts, (_comments, _unlabeled), frozendict({_repeats: "Divider `{}` repeats an earlier full divider", _orphans: "Sub divider `{}` precedes first full divider"}))

# --- [COMPOSITION] ----------------------------------------------------------------------


def _linted(paths: tuple[Path, ...]) -> list[_Doc[tuple[str, ...]]]:
    """Linted named files and unignored non-symlink files under named folders or the working directory, markdown first."""
    named, folders = [p for p in paths if p.is_file()], [p for p in paths if p.is_dir()]
    listed = subprocess.check_output(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard", "--deduplicate", "--", *folders], text=True) if folders or not named else ""
    found = [f for f in sorted({*named, *(Path(f) for f in listed.split("\0") if f)}) if f.is_file() and not f.is_symlink()]
    return [
        *(_PAGES.lint(f) for f in found if f.suffix == ".md"),
        *(_CODE.lint(f) for f in found if f.suffix in {".py", ".sh", ".toml", ".nix", ".yml", ".yaml", ".cs", ".ts", ".tsx", ".js", ".jsx", ".java", ".swift", ".sql", ".lua"}),
    ]


def check(*paths: ExistingPath) -> int:
    """Prints fix command per fixable file, then each report as `path:line: message`, exiting 1 on any."""
    linted = _linted(paths)
    found = [*d.parts for d in linted]
    total = sum(d.edits for d in linted)
    sys.stdout.writelines((*(f"{d.path}: {d.edits} fixable, python {sys.argv[0]} fix {d.path}\n" for d in linted if d.edits), *found, f"Found {len(found) + total} findings, {total} fixable\n"))
    return int(bool(found or total))


def fix(*paths: ExistingPath) -> int:
    """Writes fixed files and prints remaining reports, exiting 1 on any."""
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

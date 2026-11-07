#!/usr/bin/env python3
"""Checks and fixes Markdown and source comments."""

from functools import cache, partial, reduce
from pathlib import Path
import sys
lazy from bisect import bisect_left, bisect_right
lazy from collections.abc import Callable, Iterable, Iterator, Sequence
lazy from itertools import accumulate, count, pairwise, starmap, takewhile
lazy import shlex
lazy import time

import msgspec
from msgspec.structs import replace
from pygments.token import Comment, is_token_subtype, Punctuation, String, Text
import regex
lazy import anyio
lazy import anyio.to_process
lazy from cyclopts import App
lazy from cyclopts.types import ExistingPath
lazy from markdown_it import MarkdownIt
lazy from markdown_it.helpers import parseLinkDestination, parseLinkLabel, parseLinkTitle
lazy from markdown_it.rules_block.table import escapedSplit
lazy from markdown_it.rules_inline import StateInline
lazy from markdown_it.token import Token
lazy from markdown_it.tree import SyntaxTreeNode
lazy from mdit_py_plugins.front_matter import front_matter_plugin
lazy import pygments.lexers
lazy from wcwidth import width

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINE_COMMENT = regex.compile(r"(?P<marker>\p{P}+) (?P<body>--- (?P<label>\[[^\]]+\])(?P<fill> -+)?|\S.*)")
_DIRECTIVE = regex.compile(r"@?[a-z][a-z0-9-]*(?::|-(?:ignore|disable|enable|expect-error|nocheck)\b| (?:disable|enable)\b)|noqa\b|<(?:reference|amd-module)\b")
_LEADER = regex.compile(r"(?P<chain>\[[^\]]*\](?:-\[[^\]]*\])*)(?:\((?P<path>[^)]*)\):|:?) ")
_LABEL = regex.compile(r"\[[^\]]*\]")
_NONWORD = regex.compile(r"\W+")
_EMOJI = regex.compile(r"(?(DEFINE)(?<emoji>\p{Extended_Pictographic}[\p{Emoji_Modifier}\ufe0f]*\u200d?)) (?&emoji)+|(?&emoji)+ ?")
_TERMINAL_PUNCTUATION = regex.compile(r"(?<=[^\s.,;!?])[.,;!?]+$")
_LINE_BREAK = regex.compile(r"[ \t]*\\?\n[ \t]*")
_MULTILEVEL_NUMBER = regex.compile(r"\d+(?:\.\w+)+\.(?= )")
_ARTICLE = regex.compile(r"(?i)\A(?:a|an|the)\s+\S")
_LOWERCASE = regex.compile(r"\A\p{Ll}")
_BACKTICK_STRING = regex.compile(r"`+")
_BRACKETS = frozenset("()[]{}")
_COLUMN_LIMIT = 150

# --- [MODELS] ---------------------------------------------------------------------------


class _Edit(msgspec.Struct, frozen=True, order=True):
    start: int
    stop: int
    lines: tuple[str, ...]


class _Line(msgspec.Struct, frozen=True):
    open_brackets: str
    last_token_type: tuple[str, ...]


class _Comment(_Line, frozen=True):
    fixed: str


class _ProseComment(_Comment, frozen=True):
    text: str


class _Divider(_Comment, frozen=True):
    label: str
    full: bool


class _Doc[Node](msgspec.Struct, frozen=True):
    language: _Language[Node]
    path: Path
    section_order: frozendict[str, int]
    lines: tuple[str, ...]
    origin: tuple[int, ...]
    nodes: tuple[Node, ...]
    edit_count: int = 0


class _Language[Node](msgspec.Struct, frozen=True):
    parse: Callable[[_Language[Node], tuple[str, ...]], tuple[Node, ...]]
    reparse: Callable[[_Doc[Node], Sequence[_Edit]], tuple[Node, ...]]
    prose: tuple[Callable[[_Doc[Node]], Iterable[tuple[int, str, str, bool]]], ...]
    fixes: tuple[Callable[[_Doc[Node]], Iterable[_Edit]], ...]
    reports: frozendict[str, Callable[[_Doc[Node]], Iterable[tuple[int, str]]]]
    lexer: str = ""
    marker: str = ""
    mask: Callable[[str], str] = str
    formatter_gaps: tuple[Callable[[_Doc[Node], int, int], bool], ...] = ()

    def lint(self, section_order: frozendict[str, int], path: Path) -> tuple[tuple[str, ...], tuple[tuple[int, int, str], ...], int]:
        with path.open(encoding="utf-8") as file:
            lines = tuple(line.removesuffix("\n") for line in file)
        doc = reduce(_edited, self.fixes, _Doc(self, path, section_order, lines, tuple(range(len(lines) + 1)), self.parse(self, lines)))
        texts = [*source(doc) for source in self.prose]
        wording = ((message, [(n, word) for n, raw, prose, entry in texts for word in check(raw, prose, entry)]) for message, check in _WORDING.items())
        reported = (*((message, report(doc)) for message, report in self.reports.items()), *wording)
        findings = sorted(((n, message.format(subject)) for message, subjects in reported for n, subject in subjects), key=lambda finding: finding[0])
        return doc.lines, tuple((doc.origin[n], n, message) for n, message in findings), doc.edit_count


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [EDITS]


def _spliced[T](items: tuple[T, ...], edits: Sequence[_Edit], replacement: Callable[[_Edit], Iterable[T]]) -> tuple[T, ...]:
    return (*(*(*items[start : edit.start], *replacement(edit)) for start, edit in zip((0, *(edit.stop for edit in edits)), edits)), *items[edits[-1].stop :])


def _edited[Node](doc: _Doc[Node], fix: Callable[[_Doc[Node]], Iterable[_Edit]]) -> _Doc[Node]:
    match sorted(edit for edit in fix(doc) if edit.lines != doc.lines[edit.start : edit.stop]):
        case []:
            return doc
        case edits:
            origin = _spliced(doc.origin, edits, lambda edit: (doc.origin[edit.start],) * len(edit.lines))
            spliced = replace(doc, lines=_spliced(doc.lines, edits, lambda edit: edit.lines), origin=origin, edit_count=doc.edit_count + len(edits))
            return replace(spliced, nodes=doc.language.reparse(spliced, edits))


# --- [TEXT]


def _label(word: str) -> str:
    name = _NONWORD.sub("_", word.strip("[] ")).strip("_").upper()
    return f"[{int(name):02}]" if name.isdecimal() else f"[{name}]"


def _indent(line: str) -> int:
    return len(line) - len(line.lstrip())


@cache
def _markdown_it() -> MarkdownIt:
    return MarkdownIt("commonmark", {"inline_definitions": True, "store_labels": True, "tasklists": True}).enable("table").use(front_matter_plugin).disable("text_join")


def _token_sources(inline: SyntaxTreeNode) -> list[tuple[Token, str, bool]]:
    text, tokens = inline.content, [*child.to_tokens() for child in inline.children]

    def label_end(start: int) -> int:
        return parseLinkLabel(StateInline(text, _markdown_it(), {}, []), start)

    def reference_end(label: int) -> int:
        return label + 1 if (closing := label_end(label + 1) if text.startswith("[", label + 1) else -1) < 0 else closing + 1

    def inline_link_end(label: int) -> int:
        destination_start = len(text) - len(text[label + 2 :].lstrip(" \t\n"))
        destination = parseLinkDestination(text, destination_start, len(text))
        title_start = len(text) - len(text[destination.pos if destination.ok else destination_start :].lstrip(" \t\n"))
        title = parseLinkTitle(text, title_start, len(text))
        return text.index(")", title.pos if title.ok else title_start) + 1

    offset = 0
    previous: Token | None = None
    reference_links: list[bool] = []
    sources: list[tuple[Token, str, bool]] = []
    for token in tokens:
        is_reference = "label" in token.meta
        if token.type == "link_open":
            reference_links.append(is_reference)
        in_reference = any(reference_links) or (token.type == "image" and is_reference)
        match previous, token:
            case Token(type="link_open", markup="autolink"), Token(type="text"):
                end = offset
            case _, Token(type="code_inline", markup=backticks):
                end = next(closing.end() for closing in _BACKTICK_STRING.finditer(text, text.index(backticks, offset) + len(backticks)) if len(closing[0]) == len(backticks))
            case _, Token(type="link_open", markup="autolink"):
                end = text.index(">", offset)
            case _, Token(type="link_close", markup="autolink"):
                reference_links.pop()
                end = text.index(">", offset) + 1
            case _, Token(type="link_open"):
                end = text.index("[", offset) + 1
            case _, Token(type="link_close"):
                label = text.index("]", offset)
                end = reference_end(label) if reference_links.pop() else inline_link_end(label)
            case _, Token(type="image"):
                label = label_end(text.index("![", offset) + 1)
                end = reference_end(label) if is_reference else inline_link_end(label)
            case _, Token(type="text" | "html_inline", content=content):
                end = text.index(content, offset) + len(content)
            case _, Token(type="softbreak" | "hardbreak"):
                end = text.index("\n", offset) + 1
            case _, Token(markup=markup):
                end = text.index(markup, offset) + len(markup)
        sources.append((token, text[offset:end], in_reference))
        offset, previous = end, token
    return sources


def _prose(inline: SyntaxTreeNode) -> str:
    return "".join(source for token, source, _ in _token_sources(inline) if token.type != "code_inline")


def _plain_text(inline: SyntaxTreeNode, *, entry: bool, rewrite: Callable[[str], str] = lambda s: _EMOJI.sub("", s)) -> str:
    sources = _token_sources(inline)
    text = "".join(source if in_reference else "" if token.tag in {"em", "strong"} else rewrite(source) if token.type == "text" else source for token, source, in_reference in sources)
    return _TERMINAL_PUNCTUATION.sub("", text) if entry and sources and sources[-1][0].type == "text" else text


def _rows(table: SyntaxTreeNode) -> list[list[SyntaxTreeNode]]:
    return [[*cell.children for cell in row.children] for part in table.children for row in part.children]


def _code_spans(path: Path, column: str, matches: Callable[[SyntaxTreeNode, dict[str, SyntaxTreeNode]], bool]) -> list[str]:
    tables = (node for node in SyntaxTreeNode(_markdown_it().parse(path.read_text(encoding="utf-8"))).walk() if node.type == "table")
    rows = ((table, {name.content: cell for name, cell in zip(head, row)}) for table in tables for head, *body in (_rows(table),) for row in body)
    return [node.content for table, row in rows if matches(table, row) for node in row[column].walk() if node.type == "code_inline"]


@cache
def _enumeration_words() -> regex.Pattern[str]:
    words = _code_spans(Path(__file__).parents[1] / "references" / "word-map.md", "[WORDS]", lambda _, row: "[CATEGORY]" in row and row["[CATEGORY]"].content == "Enumeration")
    return regex.compile(rf"(?i)(?<!\ba )\b(?:{'|'.join(map(regex.escape, words))})\b(?!-\w)")


# --- [MARKDOWN]


def _parsed(_language: _Language[SyntaxTreeNode], lines: tuple[str, ...]) -> tuple[SyntaxTreeNode, ...]:
    return (*SyntaxTreeNode(_markdown_it().parse("\n".join(lines))).walk(),)


def _mapped(nodes: Iterable[SyntaxTreeNode], *types: str) -> Iterator[tuple[SyntaxTreeNode, tuple[int, int]]]:
    return ((node, span) for node in nodes if node.type in types and (span := node.map) is not None)


def _inlines(nodes: Iterable[SyntaxTreeNode]) -> Iterator[tuple[SyntaxTreeNode, SyntaxTreeNode, tuple[int, int]]]:
    return ((block, inline, span) for block in nodes for inline, span in _mapped(block.children, "inline"))


def _starts_item(block: SyntaxTreeNode) -> bool:
    return block.previous_sibling is None and block.parent is not None and block.parent.type == "list_item"


def _leader(inline: SyntaxTreeNode) -> regex.Match[str] | None:
    found = _LEADER.match(inline.content)
    return found if found is not None and sum(len(source) for _, source, _ in takewhile(lambda token_source: not token_source[2], _token_sources(inline))) >= found.end() else None


def _inline_edit(doc: _Doc[SyntaxTreeNode], inline: SyntaxTreeNode, span: tuple[int, int], text: str) -> _Edit:
    start, stop = span
    first, last, first_text, last_text = doc.lines[start], doc.lines[stop - 1], inline.content.partition("\n")[0], inline.content.rpartition("\n")[2]
    return _Edit(start, stop, (f"{first[: first.rfind(first_text)]}{text}{last[last.rfind(last_text) + len(last_text) :]}",))


def _excess_rows(doc: _Doc[SyntaxTreeNode], table: SyntaxTreeNode) -> list[int]:
    header, *body = (span[0] for _, span in _mapped(table.walk(), "tr"))
    columns = len(escapedSplit(doc.lines[header].strip()))
    return [n for n in body if len(escapedSplit(doc.lines[n].strip())) > columns]


def _headings(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    h2 = h3 = 0
    for heading, span in _mapped((node for node in doc.nodes[0].children if node.tag in {"h2", "h3"} and any(inline.content for inline in node.children)), "heading"):
        h2, h3 = (h2 + 1, 0) if heading.tag == "h2" else (h2, h3 + 1)
        title = regex.sub(r"^\[[\d.]+\]-", "", "".join(inline.content for inline in heading.children))
        labels = title if any(node.type in {"link", "image"} for node in heading.walk()) else "-".join(_LABEL.findall(title) or [_label(title)])
        yield _Edit(*span, (f"{'#' * int(heading.tag[1])} [{h2:02}{f'.{h3}' if h3 else ''}]-{labels}",))


def _tables(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    for table, (start, stop) in ((table, span) for table, span in _mapped(doc.nodes, "table") if not _excess_rows(doc, table)):
        head, *rows = _rows(table)
        headers = [_plain_text(inline, entry=False, rewrite=lambda s: _label(s) if s.strip() else s) for inline in head]
        cells = [[_plain_text(inline, entry=True) for inline in row] for row in rows]
        aligns = [{"text-align:center": (":-:", "^"), "text-align:right": ("-:", ">")}.get(str(cell.attrGet("style")), (":-", "<")) for cell in table.walk() if cell.type == "th"]
        indexed = headers[:1] == ["[INDEX]"] or len(cells) > 1
        headers, aligns, cells = (headers[1:], aligns[1:], [row[1:] for row in cells]) if headers[:1] == ["[INDEX]"] else (headers, aligns, cells)
        grid = [[cell.replace("|", "\\|") for cell in row] for row in ([["[INDEX]", *headers], *([f"[{n:02}]", *row] for n, row in enumerate(cells, 1))] if indexed else [headers, *cells])]
        delimiters = [(":-:", "^"), *aligns] if indexed else aligns
        widths = [max(len(delimiter), *map(width, column)) for (delimiter, _), column in zip(delimiters, zip(*grid))]
        indent = doc.lines[start][: len(doc.lines[start]) - len(doc.lines[start].lstrip(" >"))]
        header, *body = (f"{indent}| {' | '.join(format(cell, f'{spec}{column_width + len(cell) - width(cell)}') for (_, spec), cell, column_width in zip(delimiters, row, widths))} |" for row in grid)
        delimiter_row = f"{indent}| {' | '.join(delimiter.replace('-', '-' * (column_width - len(delimiter) + 1)) for (delimiter, _), column_width in zip(delimiters, widths))} |"
        yield _Edit(start, stop, (header, delimiter_row, *body))


def _block_spacing(doc: _Doc[SyntaxTreeNode]) -> Iterator[_Edit]:
    blocks = [*_mapped(doc.nodes, "paragraph", "heading", "fence", "code_block", "table", "html_block", "hr", "front_matter", "definition")]

    def gap(above: tuple[SyntaxTreeNode, tuple[int, int]] | None, below: tuple[SyntaxTreeNode, tuple[int, int]] | None) -> _Edit:
        start, stop = above[1][1] if above else 0, below[1][0] if below else len(doc.lines)
        match above[0] if above else None, below[0] if below else None:
            case (None, _) | (_, None):
                blanks = 0
            case (SyntaxTreeNode(type="heading" | "table"), _) | (_, SyntaxTreeNode(type="table")):
                blanks = 1
            case _, SyntaxTreeNode() as block if _starts_item(block) and doc.lines[start - 1].endswith(":"):
                blanks = 0
            case _:
                blanks = min(stop - start, 1)
        return _Edit(start, stop, ("",) * blanks if not "".join(doc.lines[start:stop]).strip() else doc.lines[start:stop])

    return starmap(gap, pairwise((None, *blocks, None)))


# --- [SOURCE]


def _masked_raw_strings(text: str) -> str:
    return regex.sub(r'(?s)(\$*("{4,}))(?!")(.*?)\2(?!")', lambda raw: f"{raw[1]}{regex.sub(r'[^\n]', ' ', raw[3])}{raw[2]}", text)


def _comment(marker: str, line: str, open_brackets: str, last_token_type: tuple[str, ...]) -> _Comment:
    indent = line[: _indent(line)]
    match _LINE_COMMENT.fullmatch(line.strip()):
        case None:
            return _Comment(open_brackets, last_token_type, line)
        case comment if comment["label"]:
            label, full = _label(comment["label"]), comment["fill"] is not None and not indent
            prefix = f"{indent}{comment['marker']} --- {label}"
            return _Divider(open_brackets, last_token_type, f"{prefix} -".ljust(90, "-") if full else prefix, label, full)
        case comment if len(comment["marker"]) > len(marker) or _DIRECTIVE.match(comment["body"]):
            return _Comment(open_brackets, last_token_type, f"{indent}{comment['marker']} {comment['body']}")
        case comment:
            return _ProseComment(open_brackets, last_token_type, f"{indent}{comment['marker']} {_TERMINAL_PUNCTUATION.sub('', _EMOJI.sub('', comment['body']))}", comment["body"])


def _lexed(language: _Language[_Line], lines: tuple[str, ...]) -> tuple[_Line, ...]:
    line_offsets = (*accumulate((len(line) + 1 for line in lines), initial=0),)
    tokens = [*pygments.lexers.get_lexer_by_name(language.lexer).get_tokens_unprocessed(language.mask("".join(f"{line}\n" for line in lines)))]
    token_offsets = [offset for offset, _, _ in tokens]
    brackets = [(offset, char) for offset, token_type, value in tokens if not _BRACKETS.isdisjoint(value) and is_token_subtype(token_type, Punctuation) for char in value if char in _BRACKETS]
    bracket_stacks, bracket_offsets = [*accumulate((char for _, char in brackets), lambda stack, char: stack + char if char in "([{" else stack[:-1], initial="")], [offset for offset, _ in brackets]

    def lexed_line(start: int, line: str) -> _Line:
        open_brackets, text_start, first_token = bracket_stacks[bisect_left(bracket_offsets, start)], start + _indent(line), bisect_right(token_offsets, start + _indent(line)) - 1
        last_token_type = tokens[bisect_right(token_offsets, start + len(line.rstrip()) - 1) - 1][1] if line.strip() else Text
        is_comment = (
            line.strip() and token_offsets[first_token] == text_start and (is_token_subtype(tokens[first_token][1], Comment.Single) or is_token_subtype(tokens[first_token][1], Comment.Hashbang))
        )
        return _comment(language.marker, line, open_brackets, last_token_type) if is_comment else _Line(open_brackets, last_token_type)

    return tuple(map(lexed_line, line_offsets, lines, strict=False))


def _lines_after_edits(doc: _Doc[_Line], edits: Sequence[_Edit]) -> tuple[_Line, ...]:
    return _spliced(doc.nodes, edits, lambda edit: doc.nodes[edit.start : edit.stop] if len(edit.lines) == edit.stop - edit.start else (_Line("", Text),) * len(edit.lines))


def _dividers(doc: _Doc[_Line]) -> list[tuple[int, _Divider]]:
    return [(n, node) for n, node in enumerate(doc.nodes) if isinstance(node, _Divider)]


def _empty_sections(doc: _Doc[_Line]) -> Iterator[_Edit]:
    next_content = next_section = next_divider = next_nonblank = len(doc.lines)
    deleted: set[int] = set()
    for n in (n for n in reversed(range(len(doc.lines))) if doc.lines[n].strip()):
        match doc.nodes[n]:
            case _Divider(full=full):
                if next_content >= (next_section if full else next_divider):
                    deleted.update(range(n, next_nonblank))
                next_section = n if full else next_section
                next_divider = next_nonblank = n
            case _:
                next_content = next_nonblank = n
    trailing = {*takewhile(lambda n: n in deleted or not doc.lines[n].strip(), reversed(range(len(doc.lines))))} if deleted else set()
    return (_Edit(n, n + 1, ()) for n in deleted | trailing)


def _lone_subsections(doc: _Doc[_Line]) -> list[_Edit]:
    indents = [_indent(line) if line.strip() else -1 for line in doc.lines]
    dividers = _dividers(doc)
    subsections = [(section, indents[n], n) for (n, divider), section in zip(dividers, accumulate((n if divider.full else -1 for n, divider in dividers), max)) if not divider.full and section >= 0]
    keys = [key for *key, _ in subsections]
    return [_Edit(n, n + 1 + (doc.lines[n + 1 : n + 2] == ("",)), ()) for section, indent, n in subsections if keys.count([section, indent]) == 1 and indent not in indents[section + 1 : n]]


def _divider_spacing(doc: _Doc[_Line]) -> Iterator[_Edit]:
    nonblank = [n for n, line in enumerate(doc.lines) if line.strip()]
    dividers = {n for n in nonblank if isinstance(doc.nodes[n], _Divider)}
    first_divider = min(dividers, default=len(doc.lines))
    comment_preamble = all(isinstance(doc.nodes[n], _Comment) for n in nonblank if n < first_divider)
    return (
        _Edit(above + 1, below, ("",) * int(below != first_divider or not comment_preamble))
        for above, below in pairwise(nonblank)
        if (above in dividers or below in dividers) and not any(formatter_gap(doc, above, below) for formatter_gap in doc.language.formatter_gaps)
    )


# --- [FORMATTER_GAPS]


def _neighbor_matches(pattern: str, doc: _Doc[_Line], above: int, below: int, *, divider_below: bool, token_type: tuple[str, ...] | None = None) -> bool:
    divider, neighbor = (below, above) if divider_below else (above, below)
    return isinstance(doc.nodes[divider], _Divider) and (token_type is None or doc.nodes[neighbor].last_token_type == token_type) and regex.search(pattern, doc.lines[neighbor].rstrip()) is not None


def _in_brackets(brackets: str, comma_brackets: str, doc: _Doc[_Line], above: int, below: int) -> bool:
    open_brackets = doc.nodes[below].open_brackets
    return bool(innermost := open_brackets[-1:]) and (
        innermost in brackets or (innermost in comma_brackets and any(doc.nodes[n].open_brackets == open_brackets and doc.lines[n].rstrip().endswith(",") for n in (above, below)))
    )


def _first_in_block(doc: _Doc[_Line], above: int, below: int) -> bool:
    header = next((m for m in range(above, -1, -1) if doc.lines[m].strip() and not doc.nodes[m].open_brackets), above)
    return (
        isinstance(doc.nodes[below], _Divider)
        and doc.lines[above].rstrip().endswith(":")
        and not regex.match(r"(?:async\s+)?def\b", doc.lines[header].lstrip())
        and _indent(doc.lines[below]) > _indent(doc.lines[header])
    )


def _top_level_blank_lines(doc: _Doc[_Line], above: int, below: int) -> bool:
    is_statement = lambda m: bool(doc.lines[m][:1].strip()) and not doc.nodes[m].open_brackets and not isinstance(doc.nodes[m], _Comment)
    return (
        below - above == 3
        and any(isinstance(doc.nodes[n], _Divider) and not _indent(doc.lines[n]) for n in (above, below))
        and not all(isinstance(doc.nodes[n], _Comment) for n in (above, below))
        and any(
            m is not None and regex.match(r"@|(?:async\s+)?def\b|class\b", doc.lines[m])
            for m in (next(filter(is_statement, range(above, -1, -1)), None), next(filter(is_statement, range(below, len(doc.lines))), None))
        )
    )


def _keep_chomping(doc: _Doc[_Line], above: int, below: int) -> bool:
    nonblank = [m for m in range(above, -1, -1) if doc.lines[m].strip()]
    header = next((m for m, indent in zip(nonblank[1:], accumulate((_indent(doc.lines[m]) for m in nonblank), min)) if _indent(doc.lines[m]) < indent), below)
    return doc.nodes[header].last_token_type == Punctuation.Indicator and doc.lines[header].rstrip().endswith("+")


# --- [REGISTRY] -------------------------------------------------------------------------

_WORDING: frozendict[str, Callable[[str, str, bool], list[str]]] = frozendict({
    "Opens with an article": lambda raw, _prose, _entry: _ARTICLE.findall(raw),
    "Opens with a lowercase letter": lambda raw, _prose, entry: _LOWERCASE.findall(raw) if entry else [],
    "Enumeration word `{}`": lambda _raw, prose, _entry: _enumeration_words().findall(prose),
})
_MARKDOWN: _Language[SyntaxTreeNode] = _Language(
    _parsed,
    lambda doc, _: _parsed(doc.language, doc.lines),
    (
        lambda doc: (
            (span[0], inline.content[leader.end() if (leader := _leader(inline) if block.type == "paragraph" else None) else 0 :], _prose(inline), block.type in {"th", "td"} or _starts_item(block))
            for block, inline, span in _inlines(doc.nodes)
        ),
    ),
    (
        lambda doc: (
            _Edit(n, n + 1, (line.rstrip(),))
            for verbatim in ({*range(span[0] + (node.type == "fence"), span[1] - (node.type == "fence")) for node, span in _mapped(doc.nodes, "fence", "code_block", "definition")},)
            for n, line in enumerate(doc.lines)
            if n not in verbatim
        ),
        lambda doc: (_inline_edit(doc, inline, span, _LINE_BREAK.sub(" ", inline.content)) for block, inline, span in _inlines(doc.nodes) if block.type == "paragraph"),
        lambda doc: (_inline_edit(doc, inline, span, _plain_text(inline, entry=_starts_item(block))) for block, inline, span in _inlines(doc.nodes) if block.type not in {"th", "td"}),
        lambda doc: (
            _inline_edit(doc, inline, span, f"{leader}: {inline.content[found.end() :]}")
            for list_node, card_numbers in ((node, count(1)) for node in doc.nodes if node.type in {"bullet_list", "ordered_list"})
            for inline, span in _mapped((child for item in list_node.children for block in item.children[:1] if block.type == "paragraph" for child in block.children), "inline")
            if (found := _leader(inline)) is not None
            for leader in (f"[{next(card_numbers):02}]-{_label(Path(found['path']).stem)}({found['path']})" if found["path"] is not None else "-".join(map(_label, _LABEL.findall(found["chain"]))),)
        ),
        _headings,
        lambda doc: (
            _inline_edit(doc, inline, span, f"{inline.content}:")
            for block, inline, span in _inlines(doc.nodes)
            if block.type == "paragraph" and block.next_sibling is not None and block.next_sibling.type in {"bullet_list", "ordered_list", "table"}
            if regex.fullmatch(r"\[[A-Z_]+\]", inline.content) and not any(child.type == "link" for child in inline.children)
        ),
        _tables,
        _block_spacing,
    ),
    frozendict({
        "H1 `{}` is extra or not a `[TOKEN]` label": lambda doc: [
            (n, doc.lines[n])
            for index, (n, title) in enumerate((span[0], inline.content) for node, span in _mapped(doc.nodes[0].children, "heading") if node.tag == "h1" for inline in node.children)
            if index or title != _label(title)
        ],
        "Number `{}` is not a list marker": lambda doc: [
            (n, number[0]) for _, span in _mapped(doc.nodes, "paragraph") for n in range(*span) if (number := _MULTILEVEL_NUMBER.match(doc.lines[n].lstrip()))
        ],
        "Target `{}` does not exist": lambda doc: (
            (span[0], target)
            for _, inline, span in _inlines(doc.nodes)
            for node in inline.walk()
            if node.type in {"link", "image"} and isinstance(href := node.attrs.get("href" if node.type == "link" else "src"), str)
            if (target := _markdown_it().normalizeLinkText(href).partition("#")[0]) and ":" not in target and not (doc.path.parent / target).exists()
        ),
        "Table row has more cells than its header": lambda doc: [(n, "") for table, _ in _mapped(doc.nodes, "table") for n in _excess_rows(doc, table)],
        f"{{}} exceeds {_COLUMN_LIMIT} columns": lambda doc: [
            *(
                (n, "List item")
                for block, inline, span in _inlines(doc.nodes)
                if _starts_item(block) and width(doc.lines[n := span[0]]) - width(inline.content) + width(_prose(inline)) > _COLUMN_LIMIT
            ),
            *((span[0], "Table") for _, span in _mapped(doc.nodes, "table") if any(width(line) > _COLUMN_LIMIT for line in doc.lines[slice(*span)])),
        ],
    }),
)
_CONFIG: _Language[_Line] = _Language(
    _lexed,
    _lines_after_edits,
    (lambda doc: ((n, node.text, _prose(SyntaxTreeNode(_markdown_it().parseInline(node.text))[0]), True) for n, node in enumerate(doc.nodes) if isinstance(node, _ProseComment)),),
    (lambda doc: (_Edit(n, n + 1, (node.fixed,)) for n, node in enumerate(doc.nodes) if isinstance(node, _Comment)), _empty_sections, _lone_subsections, _divider_spacing),
    frozendict({
        "Section `{}` repeats": lambda doc: [
            (n, divider.label) for first_lines in (dict[str, int](),) for n, divider in _dividers(doc) if divider.full and first_lines.setdefault(divider.label, n) != n
        ],
        "Subsection `{}` precedes any section": lambda doc: [
            (n, divider.label) for dividers in (_dividers(doc),) for n, divider in dividers[: next((index for index, (_, divider) in enumerate(dividers) if divider.full), len(dividers))]
        ],
        "Section `{}` is out of `CLAUDE.md` order": lambda doc: [
            (n, label)
            for ranks in ([(n, divider.label, doc.section_order[divider.label]) for n, divider in _dividers(doc) if divider.full and divider.label in doc.section_order],)
            for (n, label, rank), highest in zip(ranks, accumulate((rank for *_, rank in ranks), max, initial=-1))
            if rank < highest
        ],
    }),
    marker="#",
)
_CODE = replace(
    _CONFIG,
    prose=(),
    reports=frozendict({**_CONFIG.reports, "Comment `{}` is not a divider or directive": lambda doc: [(n, node.text) for n, node in enumerate(doc.nodes) if isinstance(node, _ProseComment)]}),
)
_C_STYLE = replace(_CODE, marker="//")
_ECMASCRIPT = replace(
    _C_STYLE, formatter_gaps=(partial(_neighbor_matches, r"[{[(]$|^\s*(?:case\b.*|default\s*):$|\btype\b.*=$", divider_below=True), partial(_neighbor_matches, r"^\s*\|", divider_below=False))
)
_LANGUAGES: frozendict[str, _Language[SyntaxTreeNode] | _Language[_Line]] = frozendict({
    ".md": _MARKDOWN,
    ".py": replace(
        _CODE,
        lexer="python",
        formatter_gaps=(_first_in_block, partial(_in_brackets, "([{", ""), _top_level_blank_lines, partial(_neighbor_matches, r"^[\s\x1c-\x1f]", divider_below=True, token_type=String.Doc)),
    ),
    ".sh": replace(_CODE, lexer="bash"),
    ".nix": replace(_CODE, lexer="nix"),
    ".toml": replace(_CONFIG, lexer="toml", formatter_gaps=(partial(_neighbor_matches, r"^\[.*\]$|\[$", divider_below=True),)),
    ".yml": replace(_CONFIG, lexer="yaml", formatter_gaps=(_keep_chomping,)),
    ".yaml": replace(_CONFIG, lexer="yaml", formatter_gaps=(_keep_chomping,)),
    ".cs": replace(_C_STYLE, lexer="csharp", mask=_masked_raw_strings),
    ".ts": replace(_ECMASCRIPT, lexer="typescript"),
    ".tsx": replace(_ECMASCRIPT, lexer="tsx"),
    ".mts": replace(_ECMASCRIPT, lexer="typescript"),
    ".cts": replace(_ECMASCRIPT, lexer="typescript"),
    ".js": replace(_ECMASCRIPT, lexer="javascript"),
    ".jsx": replace(_ECMASCRIPT, lexer="jsx"),
    ".java": replace(_C_STYLE, lexer="java"),
    ".swift": replace(_C_STYLE, lexer="swift", formatter_gaps=(partial(_neighbor_matches, r"\{$", divider_below=True),)),
    ".sql": replace(_CODE, lexer="tsql", marker="--"),
    ".lua": replace(_CODE, lexer="lua", marker="--"),
    ".rs": replace(_C_STYLE, lexer="rust", formatter_gaps=(partial(_neighbor_matches, r"^(?!\s*(?:pub(?:\([^)]*\))?\s+)?mod\b).*[{[(]$", divider_below=True), partial(_in_brackets, "([", "{"))),
})

# --- [COMPOSITION] ----------------------------------------------------------------------


def _lint(section_order: frozendict[str, int], path: Path) -> tuple[tuple[str, ...], tuple[tuple[int, int, str], ...], int]:
    return _LANGUAGES[path.suffix].lint(section_order, path)


async def _linted(paths: tuple[Path, ...]) -> list[tuple[Path, tuple[str, ...], tuple[tuple[int, int, str], ...], int]]:
    scope = paths or (Path(),)
    toplevel = await anyio.run_process(["git", "rev-parse", "--show-toplevel"], check=False)
    match toplevel.returncode:
        case 0:
            files = await anyio.run_process(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard", "--deduplicate", "--", *scope, ":(exclude,attr:linguist-generated=true)"])
            claude_md = Path(toplevel.stdout.decode().strip()) / "CLAUDE.md"
            is_sections_table = lambda table, _: (lead_in := table.previous_sibling) is not None and lead_in.type == "paragraph" and lead_in.children[0].content.startswith("[SECTIONS]:")
            candidates = [Path(name) for name in files.stdout.decode().split("\0") if name]
            section_order = frozendict(zip(_code_spans(claude_md, "[LABEL]", is_sections_table) if claude_md.is_file() else (), count()))
        case _:
            candidates, section_order = [*(*(path.rglob("*") if path.is_dir() else (path,)) for path in scope)], frozendict()
    lintable = sorted((path for path in {*candidates} if path.suffix in _LANGUAGES and path.is_file() and not path.is_symlink()), key=lambda path: (path.suffix != ".md", path))
    deadline = 2 * time.process_time()
    in_process = [_lint(section_order, path) for path in takewhile(lambda _: time.process_time() < deadline, lintable)]
    in_workers = await anyio.amap(lambda path: anyio.to_process.run_sync(_lint, section_order, path), lintable[len(in_process) :])
    return [(path, *result) for path, result in zip(lintable, (*in_process, *in_workers))]


async def check(*paths: ExistingPath) -> int:
    """Report findings at their lines on disk and a fix command per fixable file; exit 1 when any exist."""
    results = await _linted(paths)
    findings = [f"{path}:{origin + 1}: {message}\n" for path, _, file_findings, _ in results for origin, _, message in file_findings]
    fixable = sum(edit_count for *_, edit_count in results)
    sys.stdout.writelines((
        *(f"{path}: {edit_count} fixable, run {shlex.join((sys.executable, sys.argv[0], 'fix', str(path)))}\n" for path, _, _, edit_count in results if edit_count),
        *findings,
        f"{len(findings) + fixable} findings, {fixable} fixable\n",
    ))
    return int(bool(findings or fixable))


async def fix(*paths: ExistingPath) -> int:
    """Write fixes and report remaining findings; exit 1 when any remain."""
    results = await _linted(paths)
    for path, lines, _, _ in (result for result in results if result[3]):
        path.write_text("".join(f"{line}\n" for line in lines), encoding="utf-8")
    remaining = [f"{path}:{line + 1}: {message}\n" for path, _, findings, _ in results for _, line, message in findings]
    sys.stdout.writelines((*remaining, f"{sum(edit_count for *_, edit_count in results)} fixed, {len(remaining)} remaining\n"))
    return int(bool(remaining))


if __name__ == "__main__":
    app = App(help=__doc__)
    app.command(check)
    app.command(fix)
    app()

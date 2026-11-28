"""Preview cards: the host's layout and box, Markdown measures, row forms, fit, and wire records."""

from bisect import bisect_left
from enum import auto, Enum, StrEnum
from functools import partial, reduce
from itertools import accumulate, chain
import os
lazy from collections.abc import Callable, Iterable

from expression import Error, NOTHING, Ok, Option, Some
import msgspec
from pydantic_settings import BaseSettings
import regex
import wcwidth
lazy from markdown_it.token import Token
lazy from markdown_it.tree import SyntaxTreeNode
lazy import mdformat
lazy import psutil

from form._spec import EMOJI, Fault, flat, Loc, MARKDOWN, Outcome, plain, quoted, Reason, RECOMMENDED, Row, units, visible

# --- [TYPES] ----------------------------------------------------------------------------


class _Entrypoint(StrEnum):
    CLI = "cli"
    VSCODE = "claude-vscode"


class Layout(Enum):
    TERMINAL = auto()
    WEBVIEW = auto()
    PLAIN = auto()

    @property
    def previews(self) -> bool:
        return self is not Layout.PLAIN


class Unit(Enum):
    BLOCKS = ("blocks", "\n\n")
    LINES = ("lines", "\n")
    ROWS = ("rows", "\n")
    RECORDS = ("records", "\n")
    HUNKS = ("hunks", "\n\n")

    @property
    def noun(self) -> str:
        return self.value[0]

    @property
    def joiner(self) -> str:
        return self.value[1]


# --- [CONSTANTS] ------------------------------------------------------------------------


PREVIEW_UNITS = 2000
_GUTTER = 38
_CHROME_ROWS = 14
_QUESTION_GUTTER = 80
_HINT = "Enter to select · ↑/↓ to navigate · n to add notes{tabs} · Esc to cancel"
_TABS = " · Tab to switch questions"
_NOTES = "press n to add notes"
_NOTES_GUTTER = 41
_COMBINING_MARKS = 8
_WITHHELD = regex.compile(rf"… \[\+\d+ graphemes\]|\p{{M}}{{{_COMBINING_MARKS + 1},}}")
_LITERAL = regex.compile(r"\bon|off|true|false|yes|no\b|[;#\"']", regex.IGNORECASE)
_INI_KEY = regex.compile(r"[A-Za-z0-9_-]+(?:\.[A-Za-z0-9_-]+)*")
_ESCAPED = regex.compile(r"([\\`*_\[\]~<>|#])")
_BACKTICKS = regex.compile(r"`+")
_FENCE_LINE = regex.compile(r"^ {0,3}`{3,}[^`]*$", regex.MULTILINE)
CUT = "✂"
FILE = "▸"
MERGED = "◆"
ACROSS = "──▶"
DOWN = "▼"


# --- [MODELS] ---------------------------------------------------------------------------

# --- [CARDS]


class Rows(msgspec.Struct, frozen=True, kw_only=True):
    sections: tuple[tuple[str, tuple[Row, ...]], ...]
    unit: Unit | None = None

    @classmethod
    def of(cls, rows: Iterable[Row], /, unit: Unit | None = None) -> Rows:
        return cls(sections=(("", tuple(rows)),), unit=unit)

    @property
    def size(self) -> int:
        match self.unit:
            case Unit.RECORDS:
                return len(self.sections)
            case None:
                return 0
            case _:
                return sum(len(rows) for _, rows in self.sections)


class Source(msgspec.Struct, frozen=True, kw_only=True):
    shown: str
    full: str


class Cut(msgspec.Struct, frozen=True, kw_only=True):
    pieces: tuple[str, ...]
    unit: Unit
    source: Source | None = None
    fence: str | None = None
    head: tuple[str, ...] = ()

    @property
    def size(self) -> int:
        return len(self.pieces)


type Block = str | Rows | Cut


class Offer(msgspec.Struct, frozen=True, kw_only=True):
    name: str
    does: str
    facts: tuple[Row, ...] = ()
    show: tuple[Block, ...] = ()
    recommended: bool = False
    runs: tuple[str, ...] = ()

    @property
    def label(self) -> str:
        return f"{self.name}{RECOMMENDED}" if self.recommended else self.name

    @property
    def blocks(self) -> tuple[Block, ...]:
        return (heading(self.name, self.does), *self.show, *((Rows.of(self.facts),) if self.facts else ()))


class RowForm(msgspec.Struct, frozen=True, kw_only=True):
    code: frozenset[str] = frozenset()
    markdown: bool = True


class Asked(msgspec.Struct, frozen=True, kw_only=True):
    question: str
    header: str
    offers: tuple[Offer, ...]
    shared: tuple[Block, ...] = ()
    multiple: bool = False
    at: Loc = ()

    @property
    def previewed(self) -> bool:
        return not self.multiple and (bool(self.shared) or any(offer.show or offer.facts for offer in self.offers))


# --- [FACTS]


class Shortened(msgspec.Struct, frozen=True, kw_only=True):
    noun: str
    kept: int
    total: int
    source: Source | None

    @property
    def row(self) -> str:
        return f"cut {self.total - self.kept} of {self.total} {self.noun}{f', full {self.source.full}' if self.source is not None else ''}"


class Hidden(msgspec.Struct, frozen=True, kw_only=True, order=True):
    lines: int
    height: int
    rows: int

    @property
    def row(self) -> str:
        return f"hidden {self.lines} of {self.height} lines, rows {self.rows}"


class Wide(msgspec.Struct, frozen=True, kw_only=True):
    cells: int
    box: int

    @property
    def row(self) -> str:
        return f"wide {self.cells} cells, box {self.box}"


class _Drawn(msgspec.Struct, frozen=True, kw_only=True):
    question: str
    cards: tuple[str, ...]
    shared: tuple[Shortened, ...]
    owned: tuple[tuple[Shortened, ...], ...]
    hidden: Option[Hidden]


# --- [WIRE]


class _Choice(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    label: str
    description: str
    preview: str | msgspec.UnsetType = msgspec.UNSET


class Question(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True, rename="camel"):
    question: str
    header: str
    options: tuple[_Choice, ...]
    multi_select: bool = False


class Call(msgspec.Struct, frozen=True, kw_only=True):
    questions: tuple[Question, ...]


# --- [SERVICES] -------------------------------------------------------------------------


class Box(msgspec.Struct, frozen=True, kw_only=True):
    layout: Layout
    size: Option[os.terminal_size]

    @property
    def width(self) -> Option[int]:
        return self.size.map(lambda size: max(1, size.columns - _GUTTER))

    def budget(self, question: str, /, *, multi: bool) -> Option[int]:
        return self.size.map(
            lambda size: max(
                2, size.lines - _CHROME_ROWS - _asked(question, size.columns) - _height(_HINT.format(tabs=_TABS if multi else ""), size.columns) - _height(_NOTES, max(8, size.columns - _NOTES_GUTTER))
            )
        )


class Host(BaseSettings, frozen=True, case_sensitive=True, env_ignore_empty=True):
    CLAUDE_CODE_ENTRYPOINT: _Entrypoint | str = _Entrypoint.CLI

    @property
    def layout(self) -> Layout:
        match self.CLAUDE_CODE_ENTRYPOINT:
            case _Entrypoint.CLI:
                return Layout.TERMINAL
            case _Entrypoint.VSCODE:
                return Layout.WEBVIEW
            case _:
                return Layout.PLAIN

    @staticmethod
    def measured() -> Option[os.terminal_size]:
        try:
            terminal = next((path for process in psutil.Process().parents() if (path := process.terminal()) is not None), None)
            descriptor = None if terminal is None else os.open(terminal, os.O_RDONLY | os.O_NOCTTY)
        except OSError, psutil.Error:
            return NOTHING
        if descriptor is None:
            return NOTHING
        try:
            return Some(os.get_terminal_size(descriptor))
        except OSError:
            return NOTHING
        finally:
            os.close(descriptor)

    def box(self, layout: Layout, /) -> Box:
        return Box(layout=layout, size=self.measured() if layout is Layout.TERMINAL else NOTHING)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MARKDOWN]


def escaped(text: str, /) -> str:
    return _ESCAPED.sub(r"\\\1", text)


def spanned(text: str, /) -> str:
    run = "`" * (max(map(len, _BACKTICKS.findall(text)), default=0) + 1)
    pad = " " * (text.startswith("`") or text.endswith("`"))
    return f"{run}{pad}{text}{pad}{run}"


def fenced(info: str, lines: Iterable[str], /) -> str:
    held = tuple(lines)
    fence = "`" * max(3, max((len(run) + 1 for line in held for run in _BACKTICKS.findall(line)), default=0))
    return "\n".join((f"{fence}{info}", *held, fence))


def heading(name: str, lead: str, /) -> str:
    return f"**{escaped(name)}** · {escaped(lead)}"


def _sources(token: Token, /) -> tuple[str, ...]:
    return (
        *(("html",) if token.type == "html_block" else ()),
        *(("mermaid",) if token.type == "fence" and token.info.strip().lower() == "mermaid" else ()),
        *(child.type.removesuffix("_inline") for child in token.children or () if child.type in {"html_inline", "image"}),
        *(("emoji",) if EMOJI.search(token.content) else ()),
    )


def authored(text: str, at: Loc, /) -> Outcome[str]:
    source = visible(text).strip("\n")
    match tuple(chain.from_iterable(map(_sources, MARKDOWN.parse(source)))):
        case (kind, *_):
            return Error(Fault.of(at, Reason.MARKDOWN_SOURCE, source=kind))
        case _:
            return Ok(mdformat.text(source, options={"wrap": "no"}, extensions={"gfm"}).rstrip("\n"))


# --- [MEASURES]


def _glyph(width: int, held: tuple[int, int], glyph: str, /) -> tuple[int, int]:
    rows, used = held
    size = wcwidth.width(glyph)
    return (rows, used + size) if used + size <= width else (rows + 1, size)


def _word(width: int, held: tuple[int, int], item: tuple[int, str], /) -> tuple[int, int]:
    index, word = item
    size = wcwidth.width(word)
    rows, used = held if not index else (held[0] + 1, 1) if held[1] >= width else (held[0], held[1] + 1)
    match size > width, used + size > width and used > 0 and size > 0:
        case True, _:
            start = (rows + 1, 0) if (size - 1) // width < 1 + (size - (width - used) - 1) // width else (rows, used)
            return reduce(partial(_glyph, width), word, start)
        case False, True:
            return rows + 1, size
        case _:
            return rows, used + size


def _height(line: str, width: int, /) -> int:
    return reduce(partial(_word, width), enumerate(line.split(" ")), (1, 0))[0] if wcwidth.width(line) > width else 1


def _asked(question: str, columns: int, /) -> int:
    width = columns - 2 if "\n" in question or wcwidth.width(question) > _QUESTION_GUTTER else columns
    return sum(_height(line, width) for line in question.split("\n"))


def _fences(markdown: str, /) -> tuple[frozenset[int], frozenset[int], frozenset[int]]:
    spans = tuple((token.map, token.info.strip()) for token in MARKDOWN.parse(markdown) if token.type == "fence" and token.map is not None)
    edges = frozenset(chain.from_iterable((start, end - 1) for (start, end), _ in spans))
    inside = frozenset(chain.from_iterable(range(start + 1, end - 1) for (start, end), _ in spans))
    drawings = frozenset(chain.from_iterable(range(start + 1, end - 1) for (start, end), info in spans if info in {"", "text"}))
    return edges, inside, drawings


def extent(markdown: str, width: int, /) -> tuple[int, int]:
    edges, inside, drawings = _fences(markdown)
    lines = tuple((index, line if index in inside else flat(SyntaxTreeNode(MARKDOWN.parseInline(line)))) for index, line in enumerate(markdown.split("\n")) if index not in edges)
    shown = lines[: next((end for end in range(len(lines), 0, -1) if lines[end - 1][1].strip()), 0)]
    return sum(_height(text, width) for _, text in shown), max((wcwidth.width(text) for index, text in shown if index in drawings or (index not in inside and text.startswith("|"))), default=0)


def unfenced(markdown: str, /) -> str:
    edges, _, _ = _fences(markdown)
    return "\n".join(line for index, line in enumerate(markdown.split("\n")) if index not in edges)


# --- [ROWS]


def row_form(rows: Iterable[Row], /) -> tuple[RowForm, tuple[str, ...]]:
    held = tuple(rows)
    keyed = frozendict((key, "key") for key, _ in held if not _INI_KEY.fullmatch(key))
    found = frozendict((key, (value, hit[0])) for key, value in reversed(held) if key not in keyed and (hit := _LITERAL.search(value)) is not None)
    rows_out = (
        *(f"code rows {quoted(key)}: key outside ini keys" for key in keyed),
        *(f"code rows {quoted(key)}: value {quoted(value)} holds ini token {quoted(token)}" for key, (value, token) in found.items()),
    )
    return RowForm(code=frozenset((*keyed, *found))), rows_out


def _ini(width: int, rows: tuple[Row, ...], /) -> tuple[str, ...]:
    return tuple(f"{wcwidth.ljust(key, width)} = {value}" for key, value in rows)


def _coded(width: int, rows: tuple[Row, ...], /) -> tuple[str, ...]:
    return tuple(f"{spanned(key)}{' ' * (width - wcwidth.width(key))} = {escaped(value)}" for key, value in rows)


def _section(form: RowForm, title: str, rows: tuple[Row, ...], /) -> str:
    width = max((wcwidth.width(key) for key, _ in rows), default=0)
    coded, plain_rows = tuple(row for row in rows if row[0] in form.code), tuple(row for row in rows if row[0] not in form.code)
    match form.markdown, coded:
        case False, _:
            return "\n".join((*((f"{title}:",) if title else ()), *_ini(width, rows)))
        case True, ():
            return fenced("ini", (*((f"[{title}]",) if title else ()), *_ini(width, rows)))
        case True, _:
            head = "\n".join((*((f"**{escaped(title)}**",) if title else ()), *_coded(width, coded)))
            return "\n".join((head, *((fenced("ini", _ini(width, plain_rows)),) if plain_rows else ())))


def _table(rows: Rows, form: RowForm, kept: int, /) -> str:
    match rows.unit:
        case Unit.ROWS:
            starts = tuple(accumulate((len(section) for _, section in rows.sections), initial=0))
            sections = tuple((title, section[: max(0, kept - start)]) for (title, section), start in zip(rows.sections, starts, strict=False))
        case Unit.RECORDS:
            sections = rows.sections[:kept]
        case _:
            sections = rows.sections
    held = tuple((title, section) for title, section in sections if title or section)
    match form.markdown and not any(key in form.code for _, section in held for key, _ in section):
        case True if held:
            return fenced("ini", tuple(chain.from_iterable((*(("",) if index else ()), *_section(form, title, section).split("\n")[1:-1]) for index, (title, section) in enumerate(held))))
        case _:
            return "\n\n".join(_section(form, title, section) for title, section in held)


# --- [DRAW]


def size(block: Block, /) -> int:
    return 0 if isinstance(block, str) else block.size


def _note(kept: int, total: int, unit: Unit, source: Source | None, /) -> str:
    return f"{CUT} {total - kept} of {total} {unit.noun} cut{f' · full: {spanned(source.shown)}' if source is not None else ''}" if kept < total else ""


def _block(part: Block, form: RowForm, kept: int, /) -> str:
    match part:
        case str():
            return part
        case Rows(unit=unit):
            note = "" if unit is None else _note(kept, part.size, unit, None)
            return "\n\n".join(text for text in (_table(part, form, kept), note) if text)
        case Cut(pieces=pieces, unit=unit, fence=fence, head=head):
            body = unit.joiner.join((*head, *pieces[:kept])) if head or kept else ""
            shown = fenced(fence, body.split("\n")) if fence is not None and body else body
            return "\n\n".join(text for text in (shown, _note(kept, part.size, unit, part.source)) if text)


def drawn(blocks: tuple[Block, ...], form: RowForm, kept: int, /) -> str:
    starts = accumulate(map(size, blocks), initial=0)
    return "\n\n".join(text for block, start in zip(blocks, starts, strict=False) if (text := _block(block, form, min(size(block), max(0, kept - start)))))


def _last(fits: Callable[[int], bool], total: int, /) -> int:
    return total if fits(total) else max(0, bisect_left(range(total), x=True, key=lambda count: not fits(count)) - 1)


def fit(blocks: tuple[Block, ...], form: RowForm, room: int, /) -> tuple[str, tuple[Shortened, ...]]:
    kept = _last(lambda count: units(drawn(blocks, form, count)) <= room, sum(map(size, blocks)))
    starts = accumulate(map(size, blocks), initial=0)
    cuts = tuple(
        Shortened(noun=block.unit.noun, kept=shown, total=block.size, source=block.source if isinstance(block, Cut) else None)
        for block, start in zip(blocks, starts, strict=False)
        if isinstance(block, Rows | Cut) and block.unit is not None and (shown := min(block.size, max(0, kept - start))) < block.size
    )
    return drawn(blocks, form, kept), cuts


# --- [RENDER]


def _joined(*texts: str) -> str:
    return "\n\n".join(text for text in texts if text)


def _webview(text: str, /) -> str:
    match text.partition("\n"):
        case (head, _, rest) if not _FENCE_LINE.search(text):
            return _joined(fenced("text", (plain(head),)), rest.lstrip("\n"))
        case _:
            return text


def _described(offer: Offer, layout: Layout, /) -> str:
    return " · ".join((offer.does, *(f"{key} {value}" for key, value in offer.facts))) if not layout.previews else offer.does


def _plain(asked: Asked, /) -> tuple[Block, ...]:
    return (*asked.shared, *chain.from_iterable((f"{offer.name}:", *offer.show) for offer in asked.offers if offer.show))


def _routed(question: str, hidden: Option[Hidden], /) -> str:
    return hidden.map(lambda held: f"{question} Enlarge the pane to show {held.lines} hidden lines.").default_value(question)


def _hidden(box: Box, question: str, cards: tuple[str, ...], /, *, multi: bool) -> Option[Hidden]:
    measured = tuple(
        Hidden(lines=height - room + 1, height=height, rows=room)
        for room in box.budget(question, multi=multi).to_list()
        for width in box.width.to_list()
        for card in cards
        if (height := extent(card, width)[0]) > room
    )
    return Some(max(measured)) if measured else NOTHING


def _drawn(asked: Asked, form: RowForm, box: Box, /, *, multi: bool) -> _Drawn:
    owns = tuple(offer.blocks for offer in asked.offers) if box.layout.previews and asked.previewed else ()
    room = PREVIEW_UNITS - max((units(drawn(blocks, form, sum(map(size, blocks)))) + 2 for blocks in owns), default=0)
    shared, shared_cuts = fit(asked.shared, form, room) if box.layout.previews else fit(_plain(asked), RowForm(markdown=False), room)
    fitted = tuple(fit(blocks, form, PREVIEW_UNITS - units(shared) - 2 * bool(shared)) for blocks in owns)
    joined = tuple(_joined(text, shared) for text, _ in fitted)
    cards = tuple(map(_webview, joined)) if box.layout is Layout.WEBVIEW else joined
    question = asked.question if box.layout.previews else _joined(asked.question, unfenced(shared))
    hidden = _hidden(box, question, cards, multi=multi)
    return _Drawn(question=question, cards=cards, shared=shared_cuts, owned=tuple(cuts for _, cuts in fitted), hidden=hidden)


def overflow(asked: Asked, form: RowForm, box: Box, /, *, multi: bool) -> Option[str]:
    held = _drawn(asked, form, box, multi=multi)
    reasons = (*(cut.row for cut in held.shared), *(hidden.row for hidden in held.hidden.to_list()))
    return Some(reasons[0]) if reasons else NOTHING


def rendered(asked: Asked, form: RowForm, box: Box, /, *, multi: bool) -> Outcome[tuple[Question, tuple[str, ...]]]:
    held = _drawn(asked, form, box, multi=multi)
    hidden = held.hidden.bind(lambda first: _hidden(box, _routed(held.question, Some(first)), held.cards, multi=multi))
    wide = tuple(
        (offer.label, Wide(cells=widest, box=width)) for offer, card in zip(asked.offers, held.cards, strict=False) for width in box.width.to_list() if (widest := extent(card, width)[1]) > width
    )
    shared_wide = len(wide) == len(held.cards) and len({fact for _, fact in wide}) == 1
    rows = (
        *(f"{asked.header}: {cut.row}" for cut in held.shared),
        *(f"{asked.header} {quoted(offer.label)}: {cut.row}" for offer, cuts in zip(asked.offers, held.owned, strict=False) for cut in cuts),
        *(f"{asked.header}: {fact.row}" for fact in hidden.to_list()),
        *((f"{asked.header}: {wide[0][1].row}",) if shared_wide else (f"{asked.header} {quoted(label)}: {fact.row}" for label, fact in wide)),
        *(f"{asked.header} {quoted(offer.label)}: runs {command}" for offer in asked.offers for command in offer.runs),
    )
    previews: tuple[str | msgspec.UnsetType, ...] = held.cards or tuple(msgspec.UNSET for _ in asked.offers)
    wire = Question(
        question=_routed(held.question, hidden),
        header=asked.header,
        multi_select=asked.multiple,
        options=tuple(_Choice(label=offer.label, description=_described(offer, box.layout), preview=card) for offer, card in zip(asked.offers, previews, strict=True)),
    )
    match next((card for card in held.cards if units(card) > PREVIEW_UNITS or _WITHHELD.search(card)), None):
        case str() as over if _WITHHELD.search(over):
            return Error(Fault.of(asked.at, Reason.PREVIEW_WITHHELD))
        case str() as over:
            return Error(Fault.of(asked.at, Reason.PREVIEW_UNITS, units=units(over), limit=PREVIEW_UNITS, route="artifact page the question links"))
        case _:
            return Ok((wire, rows))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "ACROSS",
    "CUT",
    "DOWN",
    "FILE",
    "MERGED",
    "PREVIEW_UNITS",
    "Asked",
    "Block",
    "Box",
    "Call",
    "Cut",
    "Host",
    "Layout",
    "Offer",
    "Question",
    "RowForm",
    "Rows",
    "Source",
    "Unit",
    "authored",
    "drawn",
    "escaped",
    "extent",
    "fenced",
    "fit",
    "heading",
    "overflow",
    "rendered",
    "row_form",
    "size",
    "spanned",
]

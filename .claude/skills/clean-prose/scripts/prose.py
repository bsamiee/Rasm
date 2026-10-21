"""Check and fix house style in markdown files and in section dividers and comments of source files."""

from collections.abc import Callable, Iterable
from copy import replace
from enum import Enum, Flag
from functools import cache, reduce
from itertools import accumulate, groupby, pairwise, starmap, takewhile, zip_longest
from operator import or_
from pathlib import Path
import subprocess
import sys
from typing import Self

from cyclopts import App
from cyclopts.types import ExistingPath
from editorconfig import get_properties
import msgspec
from pygments.lexers import get_all_lexers
from pygments.lexers.special import TextLexer
import regex
from wcwidth import wcswidth

# --- [TYPES] ----------------------------------------------------------------------------


class Marker(Enum):
    """Comment sign per source language with the suffixes it owns and the openings of its preamble lines, markdown the member with no sign and data formats the members with no preamble."""

    sign: str
    suffixes: tuple[str, ...]
    preamble: str
    MARKDOWN = "", (".md",), "(?!)"
    HASH = "#", (".py", ".sh"), r"#!|import |from [\w.]+ import |if (?:typing\.)?TYPE_CHECKING:|set [-+]\w"
    HASH_DATA = "#", (".toml", ".nix", ".yml", ".yaml"), "(?!)"
    SLASH = (
        "//",
        (".cs", ".ts", ".tsx", ".js", ".jsx", ".java", ".swift"),
        r"#[!:]|(?:@\w+ )?import |(?:global )?using (?:static )?[\w.]+(?: = [^;]+)?;|namespace [\w.]+;|package [\w.]+;|extern alias \w+;|\[assembly: ",
    )
    SLASH_DATA = "//", (".jsonc",), "(?!)"
    DASH = "--", (".sql", ".lua"), "(?!)"

    def __init__(self, sign: str, suffixes: tuple[str, ...], preamble: str) -> None:
        """Keep the sign, the owned suffixes, and the preamble openings on the member."""
        self.sign = sign
        self.suffixes = suffixes
        self.preamble = preamble

    @property
    def kinds(self) -> Kind:
        """Block kinds a file of this marker holds."""
        return (SOURCE if self.sign else MARKDOWN) | Kind.BLANK


class Kind(Flag):
    """Block kinds in claim order, each pattern matching a whole block with its text spans as `t`, `claimed` a line an earlier kind claims, `sign` the comment sign, `preamble` a preamble opening."""

    pattern: str
    rx: regex.Pattern[str]
    FRONTMATTER = r"\A---\n(?:.*\n)*?---$"
    FENCE = r"^(?P<fence>```|~~~)[`~]*(?P<info>[^\s`~]*).*\n(?:.*\n)*?(?P=fence).*$"
    METADATA = r"^# /// [a-zA-Z0-9-]+$(?:\n#(?: .*)?$)*?\n# ///$"
    HEADING = r"^(?P<level>#{1,6}) (?P<t>.+)$"
    TABLE = r"^(?P<row>\|(?: *(?P<t>(?:\\\||[^|\n])*?) *\|)+ *$)(?:\n(?&row))*"
    LIST_ITEM = r"^(?P<bullet> *(?:[-*+]|\d+(?:\.\w+)*\.) )(?:(?P<chain>\[[^\]]*\](?:-\[[^\]]*\])*)(?:\((?P<path>[^)]*)\):|:?) )?(?P<t>.+)$(?:\n(?!(?&bullet)) +(?P<t>\S.*)$)*"
    LABEL = r"^\[[A-Z_]+\](?::(?: (?P<t>.+))?)?$"
    DIVIDER = r"^(?P<head>[ \t]*(?&sign) --- )(?P<tok>\[[^\]]+\]).*?(?P<fill> -+)?$"
    DIRECTIVE = r"^[ \t]*(?&sign) (?:@?[a-z][a-z0-9-]*(?::|-(?:ignore|disable|enable|expect-error|nocheck)\b| (?:disable|enable)\b)|noqa\b).*$"
    COMMENT = r"^[ \t]*(?&sign) (?P<t>\S.*)$"
    PREAMBLE = r"^(?&preamble).*$(?:\n(?:(?&preamble)|(?![ \t]*(?&sign) )[ \t)\]}]).*$)*"
    BLANK = r"^[ \t]*$"
    PARAGRAPH = r"^(?P<t>.+)$(?:\n(?!(?&claimed))(?P<t>.+)$)*"
    CODE = r"^.+$(?:\n(?!(?&claimed)).+$)*"

    def __new__(cls, pattern: str) -> Self:
        """Member with the next bit, its pattern, and the pattern compiled alone, `claimed` matching nothing, `sign` any comment sign, and `preamble` any preamble opening."""
        member = object.__new__(cls)
        member._value_ = 2 ** len(cls.__members__)
        member.pattern = pattern
        signs = "|".join(regex.escape(m.sign) for m in Marker if m.sign)
        openings = "|".join(m.preamble for m in Marker)
        member.rx = regex.compile(rf"(?(DEFINE)(?P<claimed>(?!))(?P<sign>{signs})(?P<preamble>{openings}))\A{pattern}", regex.MULTILINE)
        return member


TEXT = reduce(or_, (k for k in Kind if "t" in k.rx.groupindex))
MARKDOWN = Kind.FRONTMATTER | Kind.FENCE | Kind.HEADING | Kind.TABLE | Kind.LIST_ITEM | Kind.LABEL | Kind.PARAGRAPH
SOURCE = Kind.METADATA | Kind.DIVIDER | Kind.DIRECTIVE | Kind.COMMENT | Kind.PREAMBLE | Kind.CODE


class Align(Enum):
    """Column alignment as its alignment row cell in shortest form and the format spec that pads a cell to it."""

    mark: str
    spec: str
    LEFT = ":-", "<"
    CENTER = ":-:", "^"
    RIGHT = "-:", ">"

    def __init__(self, mark: str, spec: str) -> None:
        """Keep the mark and the spec on the member."""
        self.mark = mark
        self.spec = spec

    @classmethod
    def of(cls, mark: str) -> Align:
        """Alignment a cell of the alignment row states by its colons, left when it states none."""
        return next((a for a in cls if a.mark == regex.sub(r"-+", "-", mark)), cls.LEFT)

    def rule(self, width: int) -> str:
        """Alignment row cell, its dash widened to the width."""
        return self.mark.replace("-", "-" * (width - len(self.mark) + 1))

    def pad(self, cell: str, width: int) -> str:
        """Cell padded to the width by display width, a centered cell's odd space on the right."""
        return format(cell, f"{self.spec}{width + len(cell) - wcswidth(cell)}")


# --- [CONSTANTS] ------------------------------------------------------------------------

SPAN = r"(?<!`)(?P<run>`+)(?!`).+?(?<!`)(?P=run)(?!`)"

# --- [MODELS] ---------------------------------------------------------------------------


class Line(msgspec.Struct, frozen=True):
    """One line of text, `n` the source line it descends from through every join and insert."""

    n: int
    text: str


class Block(msgspec.Struct, frozen=True):
    """Consecutive lines one kind claimed."""

    kind: Kind
    lines: tuple[Line, ...]

    @property
    def head(self) -> Line:
        """First line."""
        return self.lines[0]

    @property
    def text(self) -> str:
        """Lines joined by newlines."""
        return "\n".join(line.text for line in self.lines)

    def rewrite(self, texts: Iterable[str]) -> Block:
        """Block with every line's text replaced, one text per line."""
        return replace(self, lines=tuple(replace(line, text=t) for line, t in zip(self.lines, texts)))

    def retext(self, text: str) -> Block:
        """Block with its first line's text replaced."""
        return replace(self, lines=(replace(self.head, text=text), *self.lines[1:]))


class Doc(msgspec.Struct, frozen=True):
    """Blocks of a file and the count of blocks fixes changed."""

    path: Path
    blocks: list[Block]
    fixed: int = 0

    def renumbered(self) -> Doc:
        """Doc with every line numbered by its place in the file its blocks write."""
        starts = accumulate((len(b.lines) for b in self.blocks), initial=1)
        return replace(self, blocks=[replace(b, lines=tuple(Line(start + i, line.text) for i, line in enumerate(b.lines))) for b, start in zip(self.blocks, starts)])


class Fix(msgspec.Struct, frozen=True):
    """Registry row `fix` writes and `check` counts, each block the transform changed, dropped, or added counted once."""

    mask: Kind
    fn: Callable[[Kind, list[Block]], list[Block]]

    def apply(self, doc: Doc) -> Doc:
        """Replace the blocks by their transform and count the blocks that differ by their first line."""
        out = self.fn(self.mask, doc.blocks)
        return replace(doc, blocks=out, fixed=doc.fixed + len({b.head.n for b in set(doc.blocks) ^ set(out)}))


class Report(msgspec.Struct, frozen=True):
    """Registry row for findings with no mechanical fix, the message formatted with each reported line's text."""

    mask: Kind
    message: str
    fn: Callable[[Doc], list[Line]]

    def apply(self, doc: Doc) -> list[Line]:
        """Each line the report names over the masked blocks, its text replaced by the message."""
        return [replace(line, text=self.message.format(line.text)) for line in self.fn(replace(doc, blocks=[b for b in doc.blocks if b.kind in self.mask]))]


class Number(msgspec.Struct, frozen=True):
    """Heading number as the `##` count so far and the `###` count under the last `##`, zero at the `##` itself."""

    h2: int = 0
    h3: int = 0

    def step(self, level: int) -> Number:
        """Number after a heading of the level, unchanged past any level but `##` and `###`."""
        match level:
            case 2:
                return Number(self.h2 + 1)
            case 3:
                return replace(self, h3=self.h3 + 1)
            case _:
                return self

    @property
    def label(self) -> str:
        """`[NN]` at a `##`, `[NN.N]` under it."""
        return f"[{self.h2:02}]" if self.h3 == 0 else f"[{self.h2:02}.{self.h3}]"


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPANS]


def parts(kind: Kind, text: str) -> regex.Match[str]:
    """Return captured parts of a block claimed by `kind`."""
    return next(kind.rx.finditer(text))


def spliced(kind: Kind, text: str, fn: Callable[[str], str]) -> str:
    """Text with `fn` run over each `t` span the kind's pattern captures in it."""
    return reduce(lambda done, span: done[: span[0]] + fn(done[span[0] : span[1]]) + done[span[1] :], reversed(parts(kind, text).spans("t")), text)


def opaque(fn: Callable[[str], str]) -> Callable[[str], str]:
    """Text transform over the prose of a span alone, code spans and link targets returned as they are."""
    return lambda s: regex.sub(rf"{SPAN}|\]\([^)]*\)|(?P<prose>(?:[^`\]]|\](?!\())+)", lambda m: fn(m["prose"]) if m["prose"] else m[0], s)


# --- [LIFTS]


def lift_block(fn: Callable[[Block], Block]) -> Callable[[Kind, list[Block]], list[Block]]:
    """Lift a block transform over every masked block."""
    return lambda mask, blocks: [fn(b) if b.kind in mask else b for b in blocks]


def lift_text(fn: Callable[[str], str]) -> Callable[[Kind, list[Block]], list[Block]]:
    """Lift a text transform over every `t` span of every masked block."""
    return lift_block(lambda block: block.rewrite(spliced(block.kind, block.text, fn).split("\n")))


def lift_sequence(fn: Callable[[list[Block]], list[Block]]) -> Callable[[Kind, list[Block]], list[Block]]:
    """Lift a whole-sequence transform over the blocks, the mask selecting the files alone."""
    return lambda _, blocks: fn(blocks)


def opening_matches(group: str, pattern: str) -> Callable[[Doc], list[Line]]:
    """Lift a pattern to a report over the opening of every span the group captures, the match its subject."""
    rx = regex.compile(pattern)
    return lambda d: [Line(b.head.n + b.text.count("\n", 0, a), m[0]) for b in d.blocks for a, e in parts(b.kind, b.text).spans(group) if (m := rx.match(b.text, a, e))]


# --- [TEXT]


def token(word: str) -> str:
    """`[NN]` for a number, `[UPPER_SNAKE]` for words, brackets around the word dropped first."""
    inner = word.strip("[] ")
    return f"[{int(inner):02}]" if inner.isdecimal() else f"[{regex.sub(r'\W+', '_', inner).strip('_').upper()}]"


def emoji(s: str) -> str:
    """Drop every emoji sequence with the one space beside it."""
    pictograph = r"(?:\p{Extended_Pictographic}[\p{Emoji_Modifier}\uFE0F]*\u200D?)+"
    return regex.sub(rf" {pictograph}|{pictograph} ?", "", s)


def leader(block: Block) -> Block:
    """List item leader as `[NN]` and `[UPPER_SNAKE]` tokens joined by `-` and followed by `: `, a link card left to its own rule."""
    if (m := parts(block.kind, block.head.text))["chain"] is None or m["path"] is not None:
        return block
    tokens = "-".join(map(token, regex.findall(r"\[[^\]]*\]", m["chain"])))
    return block.retext(f"{m['bullet']}{tokens}: {m['t']}")


def cards(blocks: list[Block]) -> list[Block]:
    """Number every link card among its sibling list items from 01, its token from the path stem."""

    def renumber(items: list[Block]) -> list[Block]:
        matches = [parts(b.kind, b.head.text) for b in items]
        counts = accumulate(int(m["path"] is not None) for m in matches)
        return [b.retext(f"{m['bullet']}[{n:02}]-{token(Path(m['path']).stem)}({m['path']}): {m['t']}") if m["path"] is not None else b for b, m, n in zip(items, matches, counts)]

    return [b for listed, run in groupby(blocks, key=lambda b: b.kind is Kind.LIST_ITEM) for b in (renumber(list(run)) if listed else run)]


def labels(blocks: list[Block]) -> list[Block]:
    """`[TOKEN]` alone with its colon when a list or table follows."""
    solid = [b for b in blocks if b.kind is not Kind.BLANK]
    opens = {a for a, b in pairwise(solid) if a.kind is Kind.LABEL and a.text.endswith("]") and b.kind in Kind.LIST_ITEM | Kind.TABLE}
    return [b.retext(f"{b.text}:") if b in opens else b for b in blocks]


def wrap(block: Block) -> Block:
    """Paragraph or list item as one logical line, its lines joined by one space."""
    return replace(block, lines=(replace(block.head, text=" ".join((block.head.text, *(line.text.strip() for line in block.lines[1:])))),))


# --- [TABLES]


def cells(block: Block) -> list[list[str]]:
    """Cells of every row."""
    return [parts(Kind.TABLE, line.text).captures("t") for line in block.lines]


def piped(row: Iterable[str]) -> str:
    """Row with its cells between pipes."""
    return "| " + " | ".join(row) + " |"


def header(block: Block) -> Block:
    """Header cells as `[UPPER_SNAKE]`, in place, a cell that is a code span kept."""
    return block.retext(spliced(Kind.TABLE, block.head.text, opaque(token)))


def index(grid: list[list[str]]) -> list[list[str]]:
    """Rows with a first `[INDEX]` column of `[NN]` cells, renumbered where it exists and added over two or more rows."""
    match grid:
        case [["[INDEX]", *head], marks, *body]:
            body = [row[1:] for row in body]
        case [head, marks, *body] if len(body) > 1:
            marks = [Align.CENTER.mark, *marks]
        case _:
            return grid
    return [["[INDEX]", *head], marks, *([f"[{i:02}]", *row] for i, row in enumerate(body, 1))]


def render(block: Block) -> Block:
    """Table with its index column, alignment row rebuilt with the index column centered, every cell padded to its column's widest cell by its alignment."""
    match index(cells(block)):
        case [_, _, *_] as grid:
            head, marks, *body = zip(*zip_longest(*grid, fillvalue=""))
            aligns = [Align.CENTER if h == "[INDEX]" else Align.of(m) for h, m in zip(head, marks)]
            widths = [max(len(a.mark), *map(wcswidth, column)) for a, column in zip(aligns, zip(head, *body))]
            rows = [head, [a.rule(w) for a, w in zip(aligns, widths)], *body]
            return block.rewrite(piped(a.pad(c, w) for a, c, w in zip(aligns, row, widths)) for row in rows)
        case _:
            return block


# --- [HEADINGS]


def numbered(block: Block, number: Number) -> Block:
    """Heading as its number, then its bracket tokens joined by `-` or a plain title as one token, any number the title opens with replaced."""
    m = parts(block.kind, block.text)
    title = regex.sub(r"^\[[\d.]+\]-", "", m["t"])
    tokens = regex.findall(r"\[[^\]]+\]", title) or [token(title)]
    return block.retext(f"{m['level']} {number.label}-{'-'.join(tokens)}")


def headings(blocks: list[Block]) -> list[Block]:
    """Number `##` from 01 in file order and `###` restarting under each `##`."""
    levels = [len(parts(b.kind, b.text)["level"]) if b.kind is Kind.HEADING else 0 for b in blocks]
    numbers = list(accumulate(levels, Number.step, initial=Number()))[1:]
    return [numbered(b, n) if level in {2, 3} else b for b, level, n in zip(blocks, levels, numbers)]


# --- [SPACING]


def gap(a: Block, b: Block, blanks: int) -> int:
    """Blank lines between adjacent non-blank blocks: one after a heading, one around a table, none after `:` before a list item, at most one elsewhere."""
    match a.kind, b.kind:
        case (Kind.HEADING | Kind.TABLE, _) | (_, Kind.TABLE):
            return 1
        case (_, Kind.LIST_ITEM) if a.text.endswith(":"):
            return 0
        case _:
            return min(blanks, 1)


def spacing(blocks: list[Block]) -> list[Block]:
    """Every blank run rebuilt from `gap`, none at the end of the file."""
    solid = [b for b in blocks if b.kind is not Kind.BLANK]
    at = {b: i for i, b in enumerate(blocks)}

    def run(a: Block, b: Block) -> list[Block]:
        have = blocks[at[a] + 1 : at[b]]
        need = gap(a, b, len(have))
        return (have + [Block(Kind.BLANK, (Line(a.lines[-1].n, ""),))] * need)[:need]

    return [x for a, b in pairwise(solid) for x in (a, *run(a, b))] + solid[-1:]


# --- [DIVIDERS]


def full(block: Block) -> bool:
    """Whether the block is a divider with dash fill."""
    return block.kind is Kind.DIVIDER and parts(block.kind, block.text)["fill"] is not None


def subject(block: Block) -> Line:
    """Divider's first line naming its token."""
    return Line(block.head.n, parts(block.kind, block.text)["tok"])


def divider(block: Block) -> Block:
    """`<marker> --- [TOKEN]` with nothing after the token, fill padded with `-` to end at column 90 at file level and dropped from an indented divider."""
    m = parts(block.kind, block.text)
    line = m["head"] + token(m["tok"])
    return block.retext(f"{line} ".ljust(90, "-") if m["fill"] and not line[0].isspace() else line)


def unlabeled(blocks: list[Block]) -> list[Block]:
    """Dividers dropped with the blank lines after them when no comment or code follows before the next divider of their level or above, none left ending the file."""

    def bare(i: int) -> bool:
        stop = full if full(blocks[i]) else (lambda b: b.kind is Kind.DIVIDER)
        return blocks[i].kind is Kind.DIVIDER and not any(b.kind in SOURCE & ~(Kind.DIVIDER | Kind.PREAMBLE) for b in takewhile(lambda b: not stop(b), blocks[i + 1 :]))

    drops = list(accumulate(range(len(blocks)), lambda prior, i: bare(i) or (prior and blocks[i].kind is Kind.BLANK), initial=False))[1:]
    kept = [b for b, drop in zip(blocks, drops) if not drop]
    return kept if kept == blocks else kept[: len(kept) - len(list(takewhile(lambda b: b.kind is Kind.BLANK, reversed(kept))))]


def repeats(d: Doc) -> list[Line]:
    """Full dividers with a token an earlier full divider used."""
    fulls = [subject(b) for b in d.blocks if full(b)]
    return [line for i, line in enumerate(fulls) if any(earlier.text == line.text for earlier in fulls[:i])]


def orphans(d: Doc) -> list[Line]:
    """Sub dividers before the first full divider."""
    return [subject(b) for b in takewhile(lambda b: not full(b), d.blocks)]


def ranked(sections: list[str]) -> Callable[[Doc], list[Line]]:
    """Lift the section table to a report over full dividers naming a section the table places before one an earlier full divider named, labels outside the table skipped."""

    def late(d: Doc) -> list[Line]:
        named = [(line, sections.index(line.text)) for b in d.blocks if full(b) and (line := subject(b)).text in sections]
        return [line for (line, rank), peak in zip(named, accumulate((rank for _, rank in named), max, initial=-1)) if rank < peak]

    return late


def premature(d: Doc) -> list[Line]:
    """Dividers before the last preamble block."""
    last = max((i for i, b in enumerate(d.blocks) if b.kind is Kind.PREAMBLE), default=0)
    return [subject(b) for b in d.blocks[:last] if b.kind is Kind.DIVIDER]


# --- [REPORTS]


def h1s(d: Doc) -> list[Line]:
    """Every H1 after the first, and the first when its title is not one `[TOKEN]`."""
    ones = [(b.head, m["t"]) for b in d.blocks if len((m := parts(b.kind, b.text))["level"]) == 1]
    return [line for line, title in ones[:1] if title != token(title)] + [line for line, _ in ones[1:]]


def prose(text: str) -> str:
    """Text with its code spans dropped."""
    return regex.sub(SPAN, "", text)


def dead(d: Doc) -> list[Line]:
    """Relative link targets that resolve to no file from the file's directory, code spans opaque."""
    targets = [(line, t) for b in d.blocks for line in b.lines for t in regex.findall(r"\]\(([^)#:]+)[)#]", prose(line.text))]
    return [Line(line.n, t) for line, t in targets if not (d.path.parent / t).exists()]


def wide(view: Callable[[str], str]) -> Callable[[Doc], list[Line]]:
    """Report over blocks with a line past column 150 in the view of its text, named at their first line."""
    return lambda d: [b.head for b in d.blocks if any(wcswidth(view(line.text)) > 150 for line in b.lines)]


def counted(words: list[str]) -> Callable[[Doc], list[Line]]:
    """Lift the words to a report over every line of the masked blocks, code spans, hyphenated compounds, and the noun after `a` opaque, each hit named by the word."""
    rx = regex.compile(rf"(?i)(?<!\ba )\b(?:{'|'.join(map(regex.escape, words))})\b(?!-\w)")
    return lambda d: [Line(line.n, m[0]) for b in d.blocks for line in b.lines for m in rx.finditer(prose(line.text))]


@cache
def narrow(path: Path, info: str) -> range:
    """Indent steps below the `.editorconfig` space indent size of the info string's language beside the path, none for plain text or an unknown language."""
    match [patterns[0] for name, aliases, patterns, _ in get_all_lexers() if info in aliases and patterns and name != TextLexer.name]:
        case [pattern, *_]:
            props = get_properties(str(path.with_name(pattern.replace("*", path.stem))))
            return range(2, int(props["indent_size"])) if props.get("indent_style") == "space" else range(0)
        case _:
            return range(0)


def shallow(d: Doc) -> list[Line]:
    """Fence lines indented fewer columns past the nearest earlier line at or below their depth than their language's indent size, a list item's depth starting at its content after `- `, a one-column step aligning a doc comment star."""

    def opened(b: Block) -> list[Line]:
        steps = narrow(d.path.resolve(), parts(b.kind, b.text)["info"])
        solid = [(line, (n := len(line.text) - len(body)), n + 2 if body.startswith("- ") else n) for line in b.lines[1:-1] if (body := line.text.lstrip(" ")).strip()]
        return [Line(line.n, line.text.strip()) for i, (line, n, _) in enumerate(solid) if n - next((depth for _, _, depth in reversed(solid[:i]) if depth <= n), 0) in steps]

    return [line for b in d.blocks for line in opened(b)]


# --- [REGISTRY] -------------------------------------------------------------------------

FIXES = (
    Fix(TEXT | Kind.BLANK, lift_block(lambda b: b.rewrite(line.text.rstrip(" \t") for line in b.lines))),
    Fix(TEXT & MARKDOWN, lift_text(opaque(lambda s: regex.sub(r"(?<!\w)(\*\*|__|\*|_)(?=\S)(.+?)(?<=\S)\1(?!\w)", r"\2", s)))),
    Fix(TEXT, lift_text(opaque(emoji))),
    Fix(Kind.LIST_ITEM, lift_block(leader)),
    Fix(Kind.LIST_ITEM, lift_sequence(cards)),
    Fix(Kind.TABLE, lift_block(header)),
    Fix(Kind.DIVIDER, lift_block(divider)),
    Fix(Kind.DIVIDER, lift_sequence(unlabeled)),
    Fix(Kind.HEADING, lift_sequence(headings)),
    Fix(Kind.LABEL, lift_sequence(labels)),
    Fix(Kind.PARAGRAPH | Kind.LIST_ITEM, lift_block(wrap)),
    Fix(Kind.LIST_ITEM | Kind.TABLE | Kind.COMMENT, lift_text(lambda s: regex.sub(r"(?<=[^\s.,;!?])[.,;!?]+$", "", s))),
    Fix(Kind.TABLE, lift_block(render)),
    Fix(MARKDOWN, lift_sequence(spacing)),
)
REPORTS = (
    Report(Kind.HEADING, "`{}` is not the one `[TOKEN]` H1", h1s),
    Report(TEXT, "Text opens with an article", opening_matches("t", r"(?i)(?:a|an|the)\s+\S")),
    Report(Kind.LIST_ITEM | Kind.TABLE | Kind.COMMENT, "Text opens with a lowercase letter", opening_matches("t", r"\p{Ll}")),
    Report(Kind.LIST_ITEM, "Marker `{}` opens no Markdown list item", opening_matches("bullet", r" *\K\d+(?:\.\w+)+\.")),
    Report(TEXT & MARKDOWN, "`{}` resolves to no file", dead),
    Report(
        TEXT,
        "Text counts visible items with `{}`",
        counted(regex.findall(r"(?<=^\| *\[\d+\] *\| Enumeration .*)`([^`]+)`", (Path(__file__).parents[1] / "references" / "word-map.md").read_text(encoding="utf-8"), regex.MULTILINE)),
    ),
    Report(Kind.LIST_ITEM, "List item runs past column 150", wide(prose)),
    Report(Kind.TABLE, "Table runs past column 150", wide(str)),
    Report(Kind.DIVIDER, "Divider `{}` repeats an earlier full divider", repeats),
    Report(Kind.DIVIDER, "Sub divider `{}` precedes the first full divider", orphans),
    Report(
        Kind.DIVIDER,
        "Section `{}` follows a section the table places after it",
        ranked(regex.findall(r"^\| +\[\d+\] +\| `(\[[A-Z_]+\])`", (Path(__file__).parents[4] / "CLAUDE.md").read_text(encoding="utf-8"), regex.MULTILINE)),
    ),
    Report(Kind.DIVIDER | Kind.PREAMBLE, "Divider `{}` sits over the preamble", premature),
    Report(Kind.FENCE, "Fence line `{}` indents by fewer columns than `.editorconfig` sets for its language", shallow),
)

# --- [COMPOSITION] ----------------------------------------------------------------------


def lint(path: Path, marker: Marker) -> Doc:
    """Fold every fix with a mask meeting the marker's kinds over the file's blocks, lexed in one pass with the kinds before the last as `claimed`, none for an empty file."""
    src = path.read_text(encoding="utf-8").removesuffix("\n")
    kinds = {name: k for name, k in Kind.__members__.items() if k in marker.kinds}
    *claimed, rest = (f"(?P<{name}>{k.pattern})" for name, k in kinds.items())
    lexer = regex.compile(f"(?(DEFINE)(?P<sign>{regex.escape(marker.sign)})(?P<preamble>{marker.preamble}))(?P<claimed>{'|'.join(claimed)})|{rest}", regex.MULTILINE)

    def claim(m: regex.Match[str]) -> Block:
        first = src.count("\n", 0, m.start()) + 1
        kind = next(k for name, k in kinds.items() if m[name] is not None)
        return Block(kind, tuple(Line(first + i, t) for i, t in enumerate(m[0].split("\n"))))

    return reduce(lambda d, f: f.apply(d), (f for f in FIXES if f.mask & marker.kinds), Doc(path, [claim(m) for m in lexer.finditer(src)] if src else []))


def findings(doc: Doc) -> list[str]:
    """`path:line: message` line of each finding every report makes, in line order."""
    return [f"{doc.path}:{line.n}: {line.text}\n" for line in sorted((line for r in REPORTS for line in r.apply(doc)), key=lambda line: line.n)]


def files(paths: tuple[Path, ...]) -> dict[Path, Marker]:
    """Every named file and every file under the named folders, the working directory when no path is named, that git tracks or leaves unignored and is no symlink, with its marker, markdown files first."""
    named, folders = [p for p in paths if p.is_file()], [p for p in paths if p.is_dir()]
    listed = (
        subprocess.run(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard", "--deduplicate", "--", *folders], capture_output=True, check=True, text=True).stdout
        if folders or not named
        else ""
    )
    found = sorted({*named, *(Path(f) for f in listed.split("\0")[:-1])})
    return {f: m for m in Marker for f in found if f.suffix in m.suffixes and f.is_file() and not f.is_symlink()}


def check(*paths: ExistingPath) -> int:
    """Print one line per file with fixable blocks naming the fix command, then every report as `path:line: message`, exit 1 on any."""
    docs = list(starmap(lint, files(paths).items()))
    fixable = "".join(f"{d.path}: {d.fixed} fixable, python {sys.argv[0]} fix {d.path}\n" for d in docs if d.fixed)
    found = [f for d in docs for f in findings(d)]
    total = sum(d.fixed for d in docs)
    sys.stdout.write(fixable + "".join(found) + f"Found {len(found) + total} findings, {total} fixable\n")
    return int(bool(found or total))


def fix(*paths: ExistingPath) -> int:
    """Write every file a fix changed, print the reports that remain at their lines in the written files, exit 1 on any report."""
    docs = list(starmap(lint, files(paths).items()))
    for d in (d for d in docs if d.fixed):
        d.path.write_text("".join(f"{b.text}\n" for b in d.blocks), encoding="utf-8")
    left = [f for d in docs for f in findings(d.renumbered())]
    sys.stdout.write("".join(left) + f"Fixed {sum(d.fixed for d in docs)}, {len(left)} remaining\n")
    return int(bool(left))


app = App(help=__doc__)
app.command(check)
app.command(fix)

if __name__ == "__main__":
    app()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["app"]

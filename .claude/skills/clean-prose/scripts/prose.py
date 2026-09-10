# /// script
# requires-python = ">=3.15"
# dependencies = ["msgspec", "regex", "wcwidth"]
# ///
"""Check and fix the house style of markdown files and of section dividers in source files."""

from collections.abc import Callable, Iterable
from copy import replace
from enum import Enum, Flag
from functools import reduce
from itertools import accumulate, groupby, pairwise, starmap, takewhile, zip_longest
from operator import or_
from pathlib import Path
import sys
from typing import Self

import msgspec
import regex
from wcwidth import wcswidth

# --- [LATTICE] --------------------------------------------------------------------------


class Marker(Enum):
    """Comment sign per source language with the suffixes it owns, markdown the member with no sign."""

    sign: str
    suffixes: tuple[str, ...]
    MARKDOWN = "", (".md",)
    HASH = "#", (".py", ".sh", ".toml", ".nix", ".yml", ".yaml")
    SLASH = "//", (".ts", ".tsx", ".cs", ".jsonc")
    DASH = "--", (".sql", ".lua")

    def __init__(self, sign: str, suffixes: tuple[str, ...]) -> None:
        """Keep the sign and the owned suffixes on the member."""
        self.sign = sign
        self.suffixes = suffixes

    @property
    def lattice(self) -> Ctx:
        """The members that claim blocks in a file of this kind."""
        return SOURCE if self.sign else MARKDOWN


class Ctx(Flag):
    """Block kinds in claim order: each pattern claims a whole block and captures its text spans as `t`, `claimed` names the members before it."""

    pattern: str
    rx: regex.Pattern[str]
    FRONTMATTER = r"\A---\n(?:.*\n)*?---$"
    FENCE = r"^(?P<fence>```|~~~).*\n(?:.*\n)*?(?P=fence).*$"
    HEADING = r"^(?P<level>#{1,6}) (?P<t>.+)$"
    TABLE = r"^(?P<row>\|(?: *(?P<t>(?:\\\||[^|\n])*?) *\|)+ *$)(?:\n(?&row))*"
    ENTRY = r"^(?P<bullet> *(?:[-*+]|\d+\.) )(?:(?P<chain>\[[^\]]*\](?:-\[[^\]]*\])*)(?:\((?P<path>[^)]*)\):|:?) )?(?P<t>.+)$(?:\n(?!(?&bullet)) +(?P<t>\S.*)$)*"
    LABEL = r"^\[[A-Z_]+\](?::(?: (?P<t>.+))?)?$"
    DIVIDER = rf"^(?P<head>[ \t]*(?:{'|'.join(regex.escape(m.sign) for m in Marker if m.sign)}) --- )(?P<tok>\[[^\]]+\]).*?(?P<fill> -+)?$"
    BLANK = r"^[ \t]*$"
    PARAGRAPH = r"^(?P<t>.+)$(?:\n(?!(?&claimed))(?P<t>.+)$)*"
    CODE = r"^.+$(?:\n(?!(?&claimed)).+$)*"

    def __new__(cls, pattern: str) -> Self:
        """Give each member the next bit, its pattern, and the pattern compiled to read one block alone, where `claimed` matches nothing."""
        member = object.__new__(cls)
        member._value_ = 2 ** len(cls.__members__)
        member.pattern = pattern
        member.rx = regex.compile(rf"(?(DEFINE)(?P<claimed>(?!))){pattern}", regex.MULTILINE)
        return member


OPAQUE = Ctx.FRONTMATTER | Ctx.FENCE
TEXT = reduce(or_, (c for c in Ctx if "t" in c.rx.groupindex))
MARKDOWN = OPAQUE | TEXT | Ctx.BLANK
SOURCE = Ctx.DIVIDER | Ctx.BLANK | Ctx.CODE

# --- [ENGINE] ---------------------------------------------------------------------------


class Line(msgspec.Struct, frozen=True):
    """One line of text, `n` the source line it descends from through every join and insert."""

    n: int
    text: str


class Block(msgspec.Struct, frozen=True):
    """The consecutive lines one lattice member claimed."""

    ctx: Ctx
    lines: tuple[Line, ...]

    @property
    def head(self) -> Line:
        """The first line."""
        return self.lines[0]

    @property
    def text(self) -> str:
        """The lines joined by newlines."""
        return "\n".join(line.text for line in self.lines)

    def rewrite(self, texts: Iterable[str]) -> Block:
        """The block with every line's text replaced, one text per line."""
        return replace(self, lines=tuple(replace(line, text=t) for line, t in zip(self.lines, texts, strict=True)))

    def retext(self, text: str) -> Block:
        """The block with its first line's text replaced."""
        return replace(self, lines=(replace(self.head, text=text), *self.lines[1:]))


class Finding(msgspec.Struct, frozen=True):
    """One line a report names, with the problem."""

    path: Path
    line: int
    message: str


class Doc(msgspec.Struct, frozen=True):
    """The blocks of a file, the count of blocks fixes changed, and the findings reports made."""

    path: Path
    blocks: list[Block]
    fixed: int = 0
    found: list[Finding] = []


class Fix(msgspec.Struct, frozen=True):
    """A registry row `fix` writes and `check` counts: every block the transform changed, dropped, or added counts once."""

    mask: Ctx
    fn: Callable[[Ctx, list[Block]], list[Block]]

    def apply(self, doc: Doc) -> Doc:
        """Replace the blocks by their transform and count the blocks that differ by their first line."""
        out = self.fn(self.mask, doc.blocks)
        return replace(doc, blocks=out, fixed=doc.fixed + len({b.head.n for b in set(doc.blocks) ^ set(out)}))


class Report(msgspec.Struct, frozen=True):
    """A registry row whose findings have no mechanical fix, the message formatted with each reported line's text."""

    mask: Ctx
    message: str
    fn: Callable[[Doc], list[Line]]

    def apply(self, doc: Doc) -> Doc:
        """Add one finding per line the report names over the masked blocks, blocks untouched."""
        lines = self.fn(replace(doc, blocks=[b for b in doc.blocks if b.ctx in self.mask]))
        return replace(doc, found=[*doc.found, *(Finding(doc.path, line.n, self.message.format(line.text)) for line in lines)])


def parts(ctx: Ctx, text: str) -> regex.Match[str]:
    """The groups the member's pattern captures over a block it claimed, a block it no longer claims is a defect of the rule that rewrote it."""
    if (m := ctx.rx.match(text)) is None:
        raise LookupError(ctx, text)
    return m


def splice(text: str, spans: list[tuple[int, int]], fn: Callable[[str], str]) -> str:
    """The text with `fn` run over each span, later spans first so earlier offsets hold."""
    for a, b in reversed(spans):
        text = text[:a] + fn(text[a:b]) + text[b:]
    return text


def opaque(fn: Callable[[str], str]) -> Callable[[str], str]:
    """The text transform over the prose of a span alone, code spans and link targets returned as they are."""
    return lambda s: regex.sub(r"`[^`]*`|\]\([^)]*\)|(?P<prose>(?:[^`\]]|\](?!\())+)", lambda m: fn(m["prose"]) if m["prose"] else m[0], s)


def text(fn: Callable[[str], str]) -> Callable[[Ctx, list[Block]], list[Block]]:
    """Lift a text transform over every `t` span of every masked block, code spans and link targets opaque."""

    def over(block: Block) -> Block:
        return block.rewrite(splice(block.text, parts(block.ctx, block.text).spans("t"), opaque(fn)).split("\n"))

    return lambda mask, blocks: [over(b) if b.ctx in mask else b for b in blocks]


def block(fn: Callable[[Block], Block]) -> Callable[[Ctx, list[Block]], list[Block]]:
    """Lift a block transform over every masked block."""
    return lambda mask, blocks: [fn(b) if b.ctx in mask else b for b in blocks]


def doc(fn: Callable[[list[Block]], list[Block]]) -> Callable[[Ctx, list[Block]], list[Block]]:
    """Lift a whole-sequence transform, for rules that read neighbors or count."""
    return lambda _, blocks: fn(blocks)


def probe(pattern: str) -> Callable[[Doc], list[Line]]:
    """Lift a text pattern to a report over the opening of every text span, the span its subject."""

    def opening(b: Block) -> list[Line]:
        hits = [(a, e) for a, e in parts(b.ctx, b.text).spans("t") if regex.match(pattern, b.text, pos=a, endpos=e)]
        return [Line(b.head.n + b.text.count("\n", 0, a), b.text[a:e]) for a, e in hits]

    return lambda d: [line for b in d.blocks for line in opening(b)]


# --- [TEXT] -----------------------------------------------------------------------------


def token(word: str) -> str:
    """`[NN]` for a number, `[UPPER_SNAKE]` for words, brackets around the word dropped first."""
    bare = word.strip("[] ")
    return f"[{int(bare):02}]" if bare.isdigit() else f"[{regex.sub(r'\W+', '_', bare).strip('_').upper()}]"


def emoji(s: str) -> str:
    """Drop every emoji sequence with the one space beside it."""
    pictograph = r"(?:\p{Extended_Pictographic}[\p{Emoji_Modifier}\uFE0F]*\u200D?)+"
    return regex.sub(rf" {pictograph}|{pictograph} ?", "", s)


def leader(block: Block) -> Block:
    """The entry leader as `[NN]` and `[UPPER_SNAKE]` tokens joined by `-` and followed by `: `, a link card left to its own rule."""
    if (m := parts(block.ctx, block.head.text))["chain"] is None or m["path"] is not None:
        return block
    tokens = "-".join(map(token, regex.findall(r"\[[^\]]*\]", m["chain"])))
    return block.retext(f"{m['bullet']}{tokens}: {m['t']}")


def cards(blocks: list[Block]) -> list[Block]:
    """Number every link card among its sibling entries from 01, its token from the path stem."""

    def renumber(entries: list[Block]) -> list[Block]:
        matches = [parts(b.ctx, b.head.text) for b in entries]
        counts = accumulate(int(m["path"] is not None) for m in matches)
        return [
            b.retext(f"{m['bullet']}[{n:02}]-{token(Path(m['path']).stem)}({m['path']}): {m['t']}") if m["path"] is not None else b
            for b, m, n in zip(entries, matches, counts, strict=True)
        ]

    return [b for entry, run in groupby(blocks, key=lambda b: b.ctx is Ctx.ENTRY) for b in (renumber(list(run)) if entry else run)]


def labels(blocks: list[Block]) -> list[Block]:
    """`[TOKEN]` alone with its colon when a list or table follows."""
    solid = [b for b in blocks if b.ctx is not Ctx.BLANK]
    opens = {a for a, b in pairwise(solid) if a.ctx is Ctx.LABEL and a.text.endswith("]") and b.ctx in Ctx.ENTRY | Ctx.TABLE}
    return [b.retext(f"{b.text}:") if b in opens else b for b in blocks]


def wrap(block: Block) -> Block:
    """The paragraph or entry as one logical line, its lines joined by one space."""
    return replace(block, lines=(replace(block.head, text=" ".join((block.head.text, *(line.text.strip() for line in block.lines[1:])))),))


def content(block: Block) -> Block:
    """The divider without text after its token, its dash fill kept."""
    m = parts(block.ctx, block.text)
    return block.retext(f"{m['head']}{m['tok']}{m['fill'] or ''}")


def divider(block: Block) -> Block:
    """`<marker> --- [TOKEN]` with the token uppercased, a full divider's fill padded with `-` to end at column 90."""
    m = parts(block.ctx, block.text)
    line = m["head"] + m["tok"].upper()
    return block.retext(f"{line} ".ljust(90, "-") if m["fill"] else line)


# --- [TABLE] ----------------------------------------------------------------------------


class Align(Enum):
    """Column alignment: the alignment row cell in its shortest form and the format spec that pads a cell to it."""

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
        """The alignment a cell of the alignment row states by its colons, left when it states none."""
        return next((a for a in cls if a.mark == regex.sub(r"-+", "-", mark)), cls.LEFT)

    def rule(self, width: int) -> str:
        """The alignment row cell, its dash widened to the width."""
        return self.mark.replace("-", "-" * (width - len(self.mark) + 1))

    def pad(self, cell: str, width: int) -> str:
        """The cell padded to the width by display width, a centered cell's odd space on the right."""
        return format(cell, f"{self.spec}{width + len(cell) - wcswidth(cell)}")


def _grid(block: Block) -> list[list[str]]:
    return [parts(Ctx.TABLE, line.text).captures("t") for line in block.lines]


def _piped(cells: Iterable[str]) -> str:
    return "| " + " | ".join(cells) + " |"


def header(block: Block) -> Block:
    """Header cells as `[UPPER_SNAKE]`, in place, a cell that is a code span kept."""
    return block.retext(splice(block.head.text, parts(Ctx.TABLE, block.head.text).spans("t"), opaque(token)))


def index(block: Block) -> Block:
    """A first `[INDEX]` column with `[NN]` cells for two or more rows, a column under another index name renamed and renumbered."""
    grid = _grid(block)
    match grid:
        case [[name, *head], marks, *body] if regex.fullmatch(r"\[(INDEX|IDX|NN|NO|NUM|ROW)\]", name):
            body = [row[1:] for row in body]
        case [head, marks, *body] if len(body) > 1:
            marks = [Align.CENTER.mark, *marks]
        case _:
            return block
    rows = [["[INDEX]", *head], marks, *([f"[{i:02}]", *row] for i, row in enumerate(body, 1))]
    return block if rows == grid else block.rewrite(map(_piped, rows))


def render(block: Block) -> Block:
    """The alignment row rebuilt with the index column centered, every cell padded to its column's widest cell by its alignment, pipes aligned."""
    match _grid(block):
        case [_, _, *_] as grid:
            head, marks, *body = zip(*zip_longest(*grid, fillvalue=""), strict=True)
            aligns = [Align.CENTER if h == "[INDEX]" else Align.of(m) for h, m in zip(head, marks, strict=True)]
            widths = [max(3, *map(wcswidth, column)) for column in zip(head, *body, strict=True)]
            rows = [head, [a.rule(w) for a, w in zip(aligns, widths, strict=True)], *body]
            return block.rewrite(_piped(a.pad(c, w) for a, c, w in zip(aligns, row, widths, strict=True)) for row in rows)
        case _:
            return block


# --- [HEADINGS] -------------------------------------------------------------------------


class Number(msgspec.Struct, frozen=True):
    """A heading number: the `##` count so far and the `###` count under the last `##`, zero at the `##` itself."""

    h2: int = 0
    h3: int = 0

    def step(self, level: int) -> Number:
        """The number after a heading of the level, unchanged past any level but `##` and `###`."""
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


def chain(block: Block) -> Block:
    """A `##` or `###` title as its bracket tokens joined by `-`, a plain title as its one token."""
    m = parts(block.ctx, block.text)
    tokens = regex.findall(r"\[[^\]]+\]", m["t"]) or [token(m["t"])]
    return block.retext(f"{m['level']} {'-'.join(tokens)}") if len(m["level"]) in {2, 3} else block


def numbered(block: Block, number: Number) -> Block:
    """The heading with its number before the chain, any number the title opens with replaced."""
    m = parts(block.ctx, block.text)
    title = regex.sub(r"^\[[\d.]+\]-", "", m["t"])
    return block.retext(f"{m['level']} {number.label}-{title}")


def headings(blocks: list[Block]) -> list[Block]:
    """Number `##` from 01 in file order and `###` restarting under each `##`."""
    levels = [len(parts(b.ctx, b.text)["level"]) if b.ctx is Ctx.HEADING else 0 for b in blocks]
    numbers = list(accumulate(levels, Number.step, initial=Number()))[1:]
    return [numbered(b, n) if level in {2, 3} else b for b, level, n in zip(blocks, levels, numbers, strict=True)]


# --- [SPACING] --------------------------------------------------------------------------


def gap(a: Block, b: Block, now: int) -> int:
    """Blank lines between adjacent non-blank blocks: one after a heading, one around a table, none after `:` before an entry, at most one elsewhere."""
    if a.ctx is Ctx.HEADING or Ctx.TABLE in a.ctx | b.ctx:
        return 1
    if a.text.endswith(":") and b.ctx is Ctx.ENTRY:
        return 0
    return min(now, 1)


def spacing(blocks: list[Block]) -> list[Block]:
    """Every blank run rebuilt from `gap`, none at the end of the file."""
    solid = [b for b in blocks if b.ctx is not Ctx.BLANK]
    at = {b: i for i, b in enumerate(blocks)}

    def run(a: Block, b: Block) -> list[Block]:
        have = blocks[at[a] + 1 : at[b]]
        need = gap(a, b, len(have))
        return (have + [Block(Ctx.BLANK, (Line(a.lines[-1].n, ""),))] * need)[:need]

    return [x for a, b in pairwise(solid) for x in (a, *run(a, b))] + solid[-1:]


# --- [REPORTS] --------------------------------------------------------------------------


def h1s(d: Doc) -> list[Line]:
    """Every H1 after the first, and the first when it is not one `[TOKEN]`."""
    ones = [b.head for b in d.blocks if len(parts(b.ctx, b.text)["level"]) == 1]
    return [line for line in ones[:1] if not regex.fullmatch(r"# \[[A-Z_]+\]", line.text)] + ones[1:]


def dead(d: Doc) -> list[Line]:
    """Relative link targets that resolve to no file from the file's directory, code spans opaque."""
    targets = [(line, t) for b in d.blocks for line in b.lines for t in regex.findall(r"\]\(([^)#:]+)[)#]", regex.sub(r"`[^`]*`", "", line.text))]
    return [Line(line.n, t) for line, t in targets if not (d.path.parent / t).exists()]


def wide(d: Doc) -> list[Line]:
    """Blocks with a line past column 150, named at their first line."""
    return [b.head for b in d.blocks if any(wcswidth(line.text) > 150 for line in b.lines)]


def full(block: Block) -> bool:
    """Whether the block is a divider carrying dash fill."""
    return block.ctx is Ctx.DIVIDER and parts(block.ctx, block.text)["fill"] is not None


def subject(block: Block) -> Line:
    """The divider's first line naming its token."""
    return Line(block.head.n, parts(block.ctx, block.text)["tok"])


def repeats(d: Doc) -> list[Line]:
    """Full dividers whose token an earlier full divider used."""
    fulls = [subject(b) for b in d.blocks if full(b)]
    return [line for i, line in enumerate(fulls) if any(earlier.text == line.text for earlier in fulls[:i])]


def empties(d: Doc) -> list[Line]:
    """Full dividers with no code before the next full divider or the end of the file."""
    starts = [i for i, b in enumerate(d.blocks) if full(b)]
    return [subject(d.blocks[i]) for i, j in pairwise([*starts, len(d.blocks)]) if all(b.ctx is not Ctx.CODE for b in d.blocks[i + 1 : j])]


def orphans(d: Doc) -> list[Line]:
    """Sub dividers before the first full divider."""
    return [subject(b) for b in takewhile(lambda b: not full(b), d.blocks)]


# --- [REGISTRY] -------------------------------------------------------------------------

RULES: tuple[Fix | Report, ...] = (
    Fix(TEXT | Ctx.BLANK, block(lambda b: b.rewrite(line.text.rstrip(" \t") for line in b.lines))),
    Fix(TEXT, text(lambda s: regex.sub(r"(?<!\w)(\*\*|__|\*|_)(?=\S)(.+?)(?<=\S)\1(?!\w)", r"\2", s))),
    Fix(TEXT, text(emoji)),
    Fix(Ctx.ENTRY, block(leader)),
    Fix(Ctx.ENTRY, doc(cards)),
    Fix(Ctx.TABLE, block(header)),
    Fix(Ctx.DIVIDER, block(content)),
    Fix(Ctx.DIVIDER, block(divider)),
    Fix(Ctx.HEADING, block(chain)),
    Fix(Ctx.HEADING, doc(headings)),
    Fix(Ctx.LABEL, doc(labels)),
    Fix(Ctx.TABLE, block(index)),
    Fix(Ctx.TABLE, block(render)),
    Fix(Ctx.PARAGRAPH | Ctx.ENTRY, block(wrap)),
    Fix(MARKDOWN, doc(spacing)),
    Report(Ctx.HEADING, "`{}` is not the one `[TOKEN]` H1", h1s),
    Report(TEXT, "`{}` resolves to no file", dead),
    Report(Ctx.ENTRY, "Entry runs past column 150", wide),
    Report(Ctx.TABLE, "Table runs past column 150", wide),
    Report(Ctx.DIVIDER, "Divider `{}` repeats an earlier full divider", repeats),
    Report(Ctx.DIVIDER | Ctx.CODE, "Divider `{}` opens an empty section", empties),
    Report(Ctx.DIVIDER, "Sub divider `{}` precedes the first full divider", orphans),
)

# --- [HOST] -----------------------------------------------------------------------------


def lex(path: Path, marker: Marker) -> list[Block]:
    """Tokenize the file into blocks in one pass over the lattice, the members before the last as `claimed`, line numbers from newline counts, no block for a file with no content."""
    src = path.read_text(encoding="utf-8").removesuffix("\n")
    members = {name: c for name, c in Ctx.__members__.items() if c in marker.lattice}
    *claimed, rest = (f"(?P<{name}>{c.pattern})" for name, c in members.items())
    lexer = regex.compile(f"(?P<claimed>{'|'.join(claimed)})|{rest}", regex.MULTILINE)

    def claim(m: regex.Match[str]) -> Block:
        first = src.count("\n", 0, m.start()) + 1
        ctx = next(c for name, c in members.items() if m[name] is not None)
        return Block(ctx, tuple(Line(first + i, t) for i, t in enumerate(m[0].split("\n"))))

    return [claim(m) for m in lexer.finditer(src)] if src else []


def lint(path: Path, marker: Marker) -> Doc:
    """Fold every rule whose mask the lattice holds over the file, findings in line order."""
    rules = [r for r in RULES if r.mask in marker.lattice]
    done = reduce(lambda d, r: r.apply(d), rules, Doc(path, lex(path, marker)))
    return replace(done, found=sorted(done.found, key=lambda f: f.line))


def files(paths: list[Path]) -> dict[Path, Marker]:
    """Every owned file under the paths with its marker, markdown files first."""
    found = sorted(f for p in paths for f in (p.rglob("*") if p.is_dir() else [p]))
    return {f: m for m in Marker for f in found if f.suffix in m.suffixes}


def show(found: list[Finding]) -> str:
    """One `path:line: message` line per finding."""
    return "".join(f"{f.path}:{f.line}: {f.message}\n" for f in found)


def check(paths: list[Path]) -> int:
    """Print one line per file with fixable blocks naming the fix command, then every report as `path:line: message`, exit 1 on any."""
    docs = list(starmap(lint, files(paths).items()))
    fixable = "".join(f"{d.path}: {d.fixed} fixable, uv run --script {sys.argv[0]} fix {d.path}\n" for d in docs if d.fixed)
    found = [f for d in docs for f in d.found]
    total = sum(d.fixed for d in docs)
    sys.stdout.write(fixable + show(found) + f"Found {len(found) + total} findings, {total} fixable\n")
    return int(bool(found or total))


def fix(paths: list[Path]) -> int:
    """Write every fix, print the reports that remain, exit 1 on any report."""
    docs = list(starmap(lint, files(paths).items()))
    for d in docs:
        d.path.write_text("".join(f"{b.text}\n" for b in d.blocks), encoding="utf-8")
    left = [f for d in docs for f in d.found]
    sys.stdout.write(show(left) + f"Fixed {sum(d.fixed for d in docs)}, {len(left)} remaining\n")
    return int(bool(left))


def main() -> int:
    """Run the command the arguments name, usage and 2 for any other form."""
    match sys.argv[1:]:
        case ["check", *paths]:
            return check([Path(p) for p in paths])
        case ["fix", *paths]:
            return fix([Path(p) for p in paths])
        case _:
            sys.stdout.write("prose.py check|fix <path>...\n")
            return 2


if __name__ == "__main__":
    sys.exit(main())

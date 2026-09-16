"""Write the `decompiled/` excerpt files: one file per grid behaviour, each holding the routines that carry it.

Usage: excerpts.py <decompile dir> <out dir>
The decompile dir holds the `Decompile.java` outputs named below, the out dir receives `<address>-<behaviour>.c`.
"""

from pathlib import Path
import re
import sys

ROUTINE = re.compile(r"^//==== FUNC @ (\S+) .*?(?=^//==== FUNC |\Z)", re.MULTILINE | re.DOTALL)
"""One routine of a `Decompile.java` output, from its marker line to the next marker, the requested address captured."""

LEGEND = (
    "PMReal helpers: construct 0x13490, operator/ 0x157c8, operator* 0x15a7c, operator+ 0x17c4c, operator- 0x28080,\n"
    "Round 0x15abc (floor(x + 0.5)), floor 0x37d70, a > b + 1e-8 0x14140, a >= b - 1e-8 0x15808, a <= b + 1e-8 0x31d38,\n"
    "a < b - 1e-8 0x3208c, |a - b| < 1e-8 0x132fc, not equal 0x18504, quantise 0x152f0 (mode 3 three decimals, mode 4 four).\n"
    "Widget access: read value 0x13338, read value in the panel unit 0x1544c, read checkbox 0x12ef4, read dropdown index 0x13034,\n"
    "write value 0x13c20, set maximum 0x196d0, unit factor at object + 0x88."
)
"""Helper-call and widget-access addresses every excerpt's opening comment ends with."""

type Excerpt = tuple[str, str, tuple[tuple[str, str], ...]]
"""File name, opening comment, and the (decompile output, requested address) pairs in order."""

EXCERPTS: tuple[Excerpt, ...] = (
    (
        "1101a0-fit-leading.c",
        (
            "Fit Leading, Quick (typography.md [01] row 01, README [08] row 01).\n"
            "FUN_001101a0 is the leading field observer: N = Round(H / L) at 0x157c8 then 0x15abc, L_fit = H / N at 0x157c8,\n"
            "quantised at 0x152f0 mode 3, guarded by N != 0 (0x132fc), L_fit > 1.0 (0x3208c) and L_fit < H (0x15808)."
        ),
        (("targets.c", "110574"),),
    ),
    (
        "10772c-subdivision.c",
        "Subdivision and the Quick horizontal unit (rows 02 and 03).\nFUN_0010772c reads the subdivision dropdown index, fits L or L * k at 0x157c8/0x15abc/0x157c8 and derives u_h = L_fit * (W / H).",
        (("targets.c", "107cc8"),),
    ),
    ("110ffc-grid-width.c", "Grid Width, Calculate (row 04). FUN_00110ffc: M_h = Round(W / g_desired), u_h = W / M_h, M_h * subdiv_h sent to the document grid.", (("targets.c", "1116b4"),)),
    ("16bc0-modular-unit.c", "Modular unit (row 05). FUN_00016bc0: u = H / modules / subdivisions, a blank module count becomes Round(H / moduleSize).", (("targets.c", "16dd8"),)),
    (
        "109e04-margins-lines.c",
        (
            "Margins in line mode (row 06). FUN_00109e04, FUN_001773a4 and FUN_0017e768: k = Round(m / u), applied m' = k * u,\n"
            "the paired form Round((m_a + m_b) / u) * u - m_b; FUN_001773a4 also builds the type-area span T for the Smart list."
        ),
        (("targets.c", "10a010"), ("targets.c", "177ab0"), ("targets.c", "17edb4")),
    ),
    ("12335c-square-grid.c", "Apply Square Grid (row 07). FUN_0012335c: k = Round(W / u_v), W' = k * u_v, u_h := u_v.", (("targets2.c", "123500"),)),
    ("131edc-columns.c", "Columns (row 08). FUN_00131edc: w_col = (T - (c - 1) * g) / c, the gutter and minimum alerts, guide emission.", (("targets2.c", "132800"),)),
    (
        "15abc-primitives.c",
        (
            "The arithmetic primitives every row relies on: Round, quantise, and the six tolerance comparisons, plus FUN_0005c3d4 (ceil for\n"
            "positive non-integers) and the std::sort and std::reverse wrappers the Smart list uses."
        ),
        (
            ("batch-g.c", "15abc"),
            ("batch-g.c", "152f0"),
            ("batch-g.c", "132fc"),
            ("batch-g.c", "18504"),
            ("batch-g.c", "14140"),
            ("batch-g.c", "15808"),
            ("batch-g.c", "31d38"),
            ("batch-g.c", "3208c"),
            ("batch-g.c", "5c3d4"),
            ("batch-g.c", "c54f8"),
            ("batch-g.c", "c5540"),
        ),
    ),
    (
        "128118-fit-leading-off.c",
        (
            "Fit Leading off (row 15). FUN_00128118 runs when the checkbox is off: L_fit = L (quantised, 0x152f0), N = Round(H / L),\n"
            "N * L > H drops one line, r = H - N * L is stored through the model setter at vtable + 0x130 and shown as the bottom margin,\n"
            "the horizontal unit still uses Fit(H, L) * W / H. FUN_00014a5c lists exact multiples L * k in the subdivision dropdown when the\n"
            "checkbox is off. FUN_00163ee0 writes the applied margins: top = k_t * L (+ x with image-lines), bottom = k_b * L + r, the\n"
            "remainder read back through vtable + 0x128."
        ),
        (("apply-leading.c", "128118"), ("targets.c", "150a0"), ("batch-h.c", "163ee0")),
    ),
    (
        "17184-vertical-value-mode.c",
        (
            "Vertical Value mode (row 14). FUN_00017184 is the vertical unit: with the checkbox on, T = the type-area height field 0x15d4a9\n"
            "(H when blank), L_fit = T / Round(T / L), or (T + x) / Round((T + x) / L) with image-lines on, quantised to 0.001.\n"
            "FUN_00166204 fills that field: T = H - m_t - m_b and the width W - m_i - m_o from the value fields 0x15d49b/0x15d49d and\n"
            "0x15d49f/0x15d4a1. FUN_0012fc3c is the value-field observer that reapplies the fit and derives u_h = L_fit * W_t / T (+ x).\n"
            "FUN_000e8428 is the apply routine after the leading observer; its branch at the vertical Value checkbox holds the same fit."
        ),
        (("batch-c.c", "17184"), ("batch-l.c", "1665d4"), ("batch-e.c", "12fc3c"), ("apply-leading.c", "e8428")),
    ),
    (
        "1668f4-smart-enumeration.c",
        (
            "Smart enumeration, ordering, and browsing (row 16). FUN_001668f4 fills the column or row dropdown: for n = 2.. while\n"
            "n * L <= T / 2, for g = 1.. while g * L <= n * L + 0.001, q = quantise3((T - n * L) / ((n + g) * L)) must equal Round(q)\n"
            "(0x132fc) and be < 41 (0x3208c), entry 'q + 1 columns (n lines); gutter: g lines'; then gutterless entries c = 2..K / 2\n"
            "with K = Round(T / L), K mod c = 0, c < 41. The list is std::sort'ed with the comparator picked by the Sort Columns & Rows\n"
            "Based On dropdown (vtable + 0xf8): FUN_001d2528 columns, lines, gutter; FUN_001d2648 lines, columns, gutter;\n"
            "FUN_001d2768 gutter, columns, lines; each key comparator (FUN_001d1c74, FUN_001d1ed0, FUN_001d21fc) parses the entry text and\n"
            "orders ascending; Descending (vtable + 0x108) reverses. FUN_001256bc is the > and < button observer: > selects index + 1 and\n"
            "wraps from the last entry to 0, < selects index - 1 and wraps from 0 to the last entry."
        ),
        (
            ("batch-f.c", "1668f4"),
            ("batch-i.c", "1d1c74"),
            ("batch-i.c", "1d1ed0"),
            ("batch-i.c", "1d21fc"),
            ("batch-g.c", "1d2528"),
            ("batch-g.c", "1d2648"),
            ("batch-g.c", "1d2768"),
            ("batch-c.c", "1256bc"),
        ),
    ),
    (
        "1ca224-image-lines.c",
        (
            "Image-line heights, the top margin with image-lines, and size matching (rows 17, 18, 19). FUN_001ca224 measures a glyph: it\n"
            "creates a text frame on the '[GC] Text' layer, applies the font (kTextAttrFontUID 0x1b2b), style (0x1b02) and size (0x1b03),\n"
            "inserts the glyph string, converts it to outlines, reads the outline bounding box through IGeometry (vtable + 0x20) and returns\n"
            "bottom - top (PMRect fields at +0x18 and +0x8, subtracted at 0x28080) quantised to 0.001. FUN_00163ee0 writes the top margin as\n"
            "k_t * L + x when x != 0. FUN_0014f5bc matches the image-line height to the grid width: sizes climb from 4 pt in 1 pt steps\n"
            "until the measured height reaches u_h, then a bisection to 0.001 pt between the last two sizes, else the closest size and an alert."
        ),
        (("apply-leading.c", "1ca224"), ("batch-h.c", "163ee0"), ("batch-c.c", "14f5bc")),
    ),
    (
        "1049ec-based-on.c",
        (
            "Based On (row 20). FUN_0001d1bc fills the dropdown with the document's paragraph styles after '[No Paragraph Style]'.\n"
            "FUN_001049ec applies the choice: it reads the style's leading (kTextAttrLead 0x1b1b) and point size (0x1b03); a leading of\n"
            "-1 (auto) becomes autoLeading (0x1b1a) * size at 0x15a7c; the value is written into the leading field 0x15d31c, the size into\n"
            "0x15d3a7, and FUN_001101a0 runs the ordinary fit."
        ),
        (("batch-m.c", "1049ec"), ("batch-i.c", "1d1bc")),
    ),
    (
        "157dec-lock.c",
        (
            "Lock (row 21). FUN_00157dec handles the two Lock checkboxes 0x15d4c5 (rows) and 0x15d4c6 (columns): on, the line fields\n"
            "are disabled and the margin sum k_t + k_b (or k_i + k_o) is stored through vtable + 0x1e0 (or + 0x1f0); off, 0 is stored.\n"
            "FUN_00129a38 is the top and bottom line observer: with the lock on and sum X read at vtable + 0x1d8, editing one margin\n"
            "writes the other as X - new, and an edit with X - new < 0 is reverted to X - other; the rows are then reapplied over the\n"
            "moved type area with the same counts and gutters (FUN_0015d6f4, FUN_00163ee0)."
        ),
        (("batch-d.c", "157dec"), ("batch-d.c", "129a38")),
    ),
    (
        "1154e4-module-cap.c",
        (
            "Module cap (row 22). FUN_001154e4 is the horizontal module count observer: s = W / modules at 0x157c8, and the module size\n"
            "must satisfy 1 pt <= s <= 1000 pt (0.352778 mm to 352.778 mm, 0.0138889 in to 13.8889 in, the literals at 0x3208c and\n"
            "0x14140); outside the range the module size field 0x15d33a is blanked and nothing applies; inside, modules * subdivisions is\n"
            "sent to the grid division through vtable + 0xf0."
        ),
        (("batch-k.c", "1154e4"),),
    ),
)
"""The excerpt files in the order they are written."""


def routines(directory: Path, names: frozenset[str]) -> dict[tuple[str, str], str]:
    """Indexes every routine of the named decompile outputs by output name and requested address.

    Args:
        directory: Directory holding the decompile outputs.
        names: Output file names to read.

    Returns:
        The routine text, trailing blank lines dropped, per (output name, requested address).
    """
    return {(name, found.group(1)): found.group(0).rstrip() + "\n" for name in names for found in ROUTINE.finditer((directory / name).read_text(encoding="utf-8"))}


def main() -> int:
    """Writes the excerpt files from the decompile outputs the arguments name, 1 naming each missing routine, usage and 2 for any other form.

    Returns:
        The process exit code.
    """
    match sys.argv[1:]:
        case [source, target]:
            index = routines(Path(source), frozenset(name for _, _, sources in EXCERPTS for name, _ in sources))
            if missing := [key for _, _, sources in EXCERPTS for key in sources if key not in index]:
                sys.stdout.write("".join(f"{name}: no routine {address}\n" for name, address in missing))
                return 1
            Path(target).mkdir(parents=True, exist_ok=True)
            for name, comment, sources in EXCERPTS:
                (Path(target) / name).write_text(f"/*\n{comment}\n\n{LEGEND}\n*/\n" + "".join("\n" + index[key] for key in sources), encoding="utf-8")
                sys.stdout.write(f"{name}\n")
            return 0
        case _:
            sys.stdout.write("excerpts.py <decompile dir> <out dir>\n")
            return 2


if __name__ == "__main__":
    sys.exit(main())

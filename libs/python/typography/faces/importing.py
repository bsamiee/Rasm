"""Binary original import into a face source: glyph names, roles, outlines, anchors, records, and substitution lookups."""

from pathlib import Path
from typing import Literal
lazy from collections import Counter
lazy from collections.abc import Iterable
lazy from datetime import datetime, UTC
lazy from functools import partial
lazy from io import BytesIO
lazy from itertools import chain, pairwise
lazy import unicodedata

from expression.extra.result import catch
lazy from fontTools.misc.timeTools import epoch_diff
lazy from fontTools.ttLib import TTFont
lazy from fontTools.ttLib.tables.otBase import BaseTable
lazy from fontTools.ttLib.tables.otTables import Coverage as Covered, LigatureSubst, SingleSubst

lazy from typography.faces.declaration import Face, Faces, OrnamentFace
lazy from typography.faces.faults import Fault, Outcome, Reason
lazy from typography.faces.glyphs import Anchor, Component, Glyph, Info, Metrics, Ornament, Outline, Point, role, Side, Source
lazy from typography.faces.layout import Ligatures, Lookups, Single
lazy from typography.faces.names import Form, GlyphName

# --- [CONSTANTS] ------------------------------------------------------------------------

_READ = frozenset({"GlyphOrder", "head", "hhea", "hmtx", "maxp", "OS/2", "post", "name", "cmap", "loca", "glyf", "GSUB", "GPOS"})

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READ]


@catch(exception=Exception)
def _opened(path: Path, /) -> TTFont:
    """Opens a binary with every table import reads decompiled, every other table dropped unread."""
    font = TTFont(BytesIO(path.read_bytes()), lazy=True, recalcBBoxes=False, recalcTimestamp=False)
    for tag in frozenset(font.keys()) - _READ:
        del font[tag]
    font.ensureDecompiled()
    return font


def _subtables(font: TTFont, tag: Literal["GSUB", "GPOS"], /) -> tuple[tuple[str, int, BaseTable], ...]:
    """Returns each feature tag, lookup index, and subtable of a layout table, extension subtables unwrapped."""
    return tuple(
        (str(record.FeatureTag), index, vars(subtable).get("ExtSubTable", subtable))
        for table in ((font[tag].table,) if tag in font else ())
        for record in table.FeatureList.FeatureRecord
        for index in record.Feature.LookupListIndex
        for subtable in table.LookupList.Lookup[index].SubTable
    )


def _outline(font: TTFont, name: str, /) -> Outline:
    """Returns a glyph's quadratic contours with components decomposed, points in drawn order."""
    coordinates, ends, flags = font["glyf"][name].getCoordinates(font["glyf"])
    return tuple(tuple(Point(x=coordinates[index][0], y=coordinates[index][1], on=bool(flags[index] & 1)) for index in range(start + 1, end + 1)) for start, end in pairwise((-1, *ends)))


# --- [NAMING]


def _named(order: Iterable[str], cmap: dict[int, str], singles: dict[str, dict[str, str]], ligatures: tuple[tuple[tuple[str, ...], str], ...], /) -> dict[str, GlyphName]:
    """Names each glyph by the first claim within the length limit: its nominal code point, a presentation form's decomposition, a GSUB form, a GSUB ligature, then a secondary code point."""
    named, taken = {".notdef": GlyphName(base=".notdef")}, {GlyphName(base=".notdef")}
    coded = sorted((code, glyph, unicodedata.decomposition(chr(code)).split()) for code, glyph in cmap.items())
    claims = chain(
        ((glyph, GlyphName(codes=(code,))) for code, glyph, parts in coded if not parts[0:1] or not parts[0].startswith("<")),
        ((glyph, GlyphName(codes=tuple(int(part, 16) for part in parts[1:]), form=Form(parts[0][1:5]))) for _, glyph, parts in coded if parts[:1] and parts[0][1:5] in Form),
        (
            (target, GlyphName(codes=stem.codes, form=form))
            for form in (Form.INIT, Form.MEDI, Form.FINA)
            for base, target in singles.get(form, {}).items()
            if (stem := named.get(base)) and len(stem.codes) == 1 and stem.form is Form.ISOL
        ),
        (
            (ligature, GlyphName(codes=tuple(*named[part].codes for part in parts), form=Form.joined(right=named[parts[0]].form.right, left=named[parts[-1]].form.left)))
            for parts, ligature in ligatures
            if all(part in named and named[part].codes for part in parts)
        ),
        ((glyph, GlyphName(codes=(code,))) for code, glyph, _ in coded),
    )
    for glyph, name in claims:
        if glyph not in named and name not in taken and len(name.text) <= GlyphName.LIMIT:
            named[glyph] = name
            taken.add(name)
    return {glyph: named[glyph] for glyph in order if glyph in named}


# --- [ANCHORS]


def _anchors(font: TTFont, named: dict[str, GlyphName], /) -> dict[str, dict[str, tuple[float, float]]]:
    """Returns anchors by glyph from GPOS mark attachment, each mark class voted to the side most of its marks' code points take."""
    found: dict[str, dict[str, tuple[float, float]]] = {}
    for _, _, sub in _subtables(font, "GPOS"):
        match sub:
            case BaseTable(MarkCoverage=Covered(glyphs=marks), MarkArray=BaseTable(MarkRecord=records), BaseCoverage=Covered(glyphs=bases), BaseArray=BaseTable(BaseRecord=rows)):
                attached = tuple((glyph, "", row.BaseAnchor) for glyph, row in zip(bases, rows))
            case BaseTable(MarkCoverage=Covered(glyphs=marks), MarkArray=BaseTable(MarkRecord=records), LigatureCoverage=Covered(glyphs=bases), LigatureArray=BaseTable(LigatureAttach=rows)):
                attached = tuple((glyph, f"_{index}", component.LigatureAnchor) for glyph, row in zip(bases, rows) for index, component in enumerate(row.ComponentRecord, 1))
            case BaseTable(Mark1Coverage=Covered(glyphs=marks), Mark1Array=BaseTable(MarkRecord=records), Mark2Coverage=Covered(glyphs=bases), Mark2Array=BaseTable(Mark2Record=rows)):
                attached = tuple((glyph, "", row.Mark2Anchor) for glyph, row in zip(bases, rows))
            case _:
                continue
        votes = Counter((record.Class, Side.of(code)) for glyph, record in zip(marks, records) if glyph in named for code in named[glyph].codes[:1])
        sides = {klass: Side.BELOW if votes[klass, Side.BELOW] > votes[klass, Side.ABOVE] else Side.ABOVE for klass in {record.Class for record in records}}
        placed = (
            *((glyph, f"_{sides[record.Class].anchor}", record.MarkAnchor) for glyph, record in zip(marks, records)),
            *((glyph, f"{sides[klass].anchor}{suffix}", point) for glyph, suffix, points in attached for klass, point in enumerate(points) if point and klass in sides),
        )
        for glyph, name, point in placed:
            found.setdefault(glyph, {}).setdefault(name, (point.XCoordinate, point.YCoordinate))
    return found


# --- [SOURCE]


def _source(font: TTFont, face: Faces, /) -> Source:
    """Builds the source of an opened binary: text faces named and anchored from cmap and layout, ornament faces kept under their original names."""
    cmap, advances, order = font.getBestCmap() or {}, font["hmtx"].metrics, font.getGlyphOrder()
    subtables = _subtables(font, "GSUB") if isinstance(face, Face) else ()
    orders = {tag: min(index for held, index, _ in subtables if held == tag) for tag, _, _ in subtables}
    singles = {tag: {a: b for held, _, sub in subtables if held == tag and isinstance(sub, SingleSubst) for a, b in sub.mapping.items()} for tag, _, sub in subtables if isinstance(sub, SingleSubst)}
    ligatures = {
        tag: tuple(
            ((first, *entry.Component), entry.LigGlyph) for held, _, sub in subtables if held == tag and isinstance(sub, LigatureSubst) for first, rows in sub.ligatures.items() for entry in rows
        )
        for tag, _, sub in subtables
        if isinstance(sub, LigatureSubst)
    }
    rules = tuple(*entries for entries in ligatures.values())
    match face:
        case Face(genre=genre):
            kept = {code: glyph for code, glyph in cmap.items() if genre.coverage.exclusion(code).is_none()}
            named, art = _named(order, kept, singles, rules), False
        case OrnamentFace():
            kept, spaces = {}, {glyph: code for code, glyph in sorted(cmap.items(), reverse=True) if unicodedata.category(chr(code)) == "Zs"}
            named, art = (
                {glyph: GlyphName(codes=(spaces[glyph],)) if glyph in spaces else GlyphName(base=glyph) for glyph in order if glyph == ".notdef" or glyph in spaces or _outline(font, glyph)},
                True,
            )
    anchors = {} if art else _anchors(font, named)
    renamed = {source: tuple(named[part].text for part in parts) for parts, source in rules if source in named and all(part in named for part in parts)}
    os2, hhea, head, post = (vars(font[tag]) for tag in ("OS/2", "hhea", "head", "post"))
    lookups: tuple[Lookups, ...] = (
        *(Single(feature=tag, order=orders[tag], mapping=frozendict({named[a].text: named[b].text for a, b in mapping.items() if a in named and b in named})) for tag, mapping in singles.items()),
        *(
            Ligatures(
                feature=tag,
                order=orders[tag],
                rules=tuple((tuple(named[part].text for part in parts), named[target].text) for parts, target in entries if target in named and all(part in named for part in parts)),
            )
            for tag, entries in ligatures.items()
        ),
    )
    return Source(
        face=face,
        upm=head["unitsPerEm"],
        glyphs=(
            *(
                Glyph(
                    name=name,
                    role=Ornament() if art and name.base not in {None, ".notdef"} else role(name, components=renamed.get(glyph, ())),
                    advance=advances[glyph][0],
                    contours=_outline(font, glyph),
                    anchors=tuple(Anchor(name=anchor, x=x, y=y) for anchor, (x, y) in sorted(anchors.get(glyph, {}).items())),
                )
                for glyph, name in named.items()
            ),
            *(
                Glyph(name=alias, role=role(alias), advance=advances[glyph][0], components=(Component(name=named[glyph].text),))
                for code, glyph in sorted(kept.items())
                if glyph in named and named[glyph] != (alias := GlyphName(codes=(code,)))
            ),
        ),
        info=Info(
            created=datetime.fromtimestamp(head["created"] + epoch_diff, UTC),
            version=divmod(round(head["fontRevision"] * 1000), 1000),
            copyright=(font["name"].getDebugName(0) or "").strip(),
            designer=(font["name"].getDebugName(9) or "").strip(),
            vendor=os2["achVendID"].strip(),
        ),
        metrics=Metrics(
            ascender=hhea["ascent"],
            descender=hhea["descent"],
            win_ascent=os2["usWinAscent"],
            win_descent=os2["usWinDescent"],
            caret_offset=hhea["caretOffset"],
            italic_angle=post["italicAngle"],
            lowest_ppem=head["lowestRecPPEM"],
        ),
        lookups=tuple(sorted(lookups, key=lambda lookup: lookup.order)),
    )


def imported(path: Path, face: Faces, /) -> Outcome[Source]:
    """Reads a binary original into a source, `UNREADABLE` where the binary fails to open or decompile."""
    return _opened(path).map_error(partial(Fault.of, Reason.UNREADABLE, str(path))).map(lambda font: _source(font, face))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["imported"]

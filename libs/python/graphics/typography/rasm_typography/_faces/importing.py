"""Binary original import into a face source: glyph names, roles, outlines, anchors, records, and substitution lookups."""

from pathlib import Path
from typing import Literal
lazy from collections import ChainMap, Counter, defaultdict, deque
lazy from collections.abc import Callable, Sequence
lazy from datetime import datetime, UTC
lazy from functools import cache
lazy from io import BytesIO
lazy from itertools import chain
lazy import unicodedata

lazy from fontTools.misc.timeTools import epoch_diff
lazy from fontTools.otlLib.builder import LOOKUP_FLAG_USE_MARK_FILTERING_SET
lazy from fontTools.pens.filterPen import DecomposingFilterPen
lazy from fontTools.ttLib import TTFont, TTLibError
lazy from fontTools.ttLib.tables.otBase import BaseTable
lazy from fontTools.ttLib.tables.otTables import Coverage, ExtensionPos, ExtensionSubst, LigatureSubst, SingleSubst  # type: ignore[attr-defined]  # ty: ignore[unresolved-import]
lazy from fontTools.unicodedata import script_extension

lazy from rasm_records import Error, Fault, Ok, Outcome, Reason
lazy from rasm_typography._faces.declaration import Face, Faces, OrnamentFace
lazy from rasm_typography._faces.glyphs import _Anchor, _Glyph, _Info, _Metrics, _Outline, _role, _Side, _Source, _traced, Ornament
lazy from rasm_typography._faces.layout import Ligatures, Lookups, Single
lazy from rasm_typography._faces.names import _Form, _GlyphName

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [READS]


def _lookups(font: TTFont, tag: Literal["GSUB", "GPOS"], /) -> tuple[tuple[int, frozendict[str, frozendict[tuple[str, str], bool]], BaseTable], ...]:
    """Joins native feature records to their language systems and shared lookup identities."""
    if tag not in font:
        return ()
    table = font[tag].table
    associations: defaultdict[int, dict[tuple[str, str], bool]] = defaultdict(dict)
    registrations = (
        (index, (script.ScriptTag, language), index == system.ReqFeatureIndex)
        for script in table.ScriptList.ScriptRecord
        for language, system in (("dflt", script.Script.DefaultLangSys), *((record.LangSysTag, record.LangSys) for record in script.Script.LangSysRecord))
        if system is not None
        for index in (*system.FeatureIndex, *((system.ReqFeatureIndex,) if system.ReqFeatureIndex != 0xFFFF else ()))
    )
    for index, system, required in registrations:
        associations[index][system] = required
    features: defaultdict[int, dict[str, dict[tuple[str, str], bool]]] = defaultdict(dict)
    for index, record in enumerate(table.FeatureList.FeatureRecord):
        for lookup in record.Feature.LookupListIndex if index in associations else ():
            systems = features[lookup].setdefault(record.FeatureTag, {})
            systems.update({system: systems.get(system, False) or required for system, required in associations[index].items()})
    return tuple(  # ty: ignore[unsound-return-statement]
        (index, frozendict({tag: frozendict(systems) for tag, systems in held.items()}), table.LookupList.Lookup[index]) for index, held in sorted(features.items())
    )


def _subtables(lookup: BaseTable, /) -> tuple[BaseTable, ...]:
    """Unwraps native extension subtables without changing their priority."""
    return tuple(subtable.ExtSubTable if isinstance(subtable, ExtensionPos | ExtensionSubst) else subtable for subtable in lookup.SubTable)  # ty: ignore[unsound-return-statement]


# --- [NAMES]


def _named(
    order: Sequence[str],
    coded: tuple[tuple[int, str, list[str]], ...],
    singles: dict[int, dict[str, str]],
    features: dict[int, frozendict[str, frozendict[tuple[str, str], bool]]],
    ligatures: tuple[tuple[tuple[str, ...], str], ...],
    glyph_id: Callable[[str], int],
    /,
) -> dict[str, _GlyphName]:
    """Names each glyph by its first semantic claim: primary script and digit system first, then native code order, presentation forms and layout."""
    outgoing: defaultdict[str, list[tuple[str, int, str]]] = defaultdict(list)
    for index, mapping in singles.items():
        for feature in features[index]:
            for source, target in mapping.items():
                outgoing[source].append((feature, index, target))
    joined: defaultdict[str, list[tuple[tuple[str, ...], str]]] = defaultdict(list)
    for parts, target in ligatures:
        for part in dict.fromkeys(parts):
            joined[part].append((parts, target))
    named: dict[str, _GlyphName] = {}
    taken: set[str] = set()
    claims = deque(
        chain(
            ((".notdef", _GlyphName(stem=".notdef")),),
            ((glyph, _GlyphName(stem=(code,))) for code, glyph, parts in coded if not parts[0:1] or not parts[0].startswith("<")),
            ((glyph, _GlyphName(stem=(int(parts[1], 16), *(int(part, 16) for part in parts[2:])), form=_Form(parts[0][1:5]))) for _, glyph, parts in coded if parts[:1] and parts[0][1:5] in _Form),
            ((glyph, _GlyphName(stem=(code,))) for code, glyph, _ in coded),
        )
    )
    while claims:
        glyph, name = claims.popleft()
        if glyph in named:
            continue
        name = _GlyphName(stem=name.stem, form=name.form, variant=".".join(filter(None, (name.variant, f"gid{glyph_id(glyph)}")))) if name.text in taken else name
        if len(name.text) > _GlyphName.LIMIT:
            continue
        named[glyph] = name
        taken.add(name.text)
        claims.extend(
            (target, _GlyphName(stem=name.stem, form=_Form(feature), variant=name.variant))
            if feature in _Form and feature != _Form.ISOL and len(name.codes) == 1
            else (target, _GlyphName(stem=name.stem, form=name.form, variant=".".join(filter(None, (name.variant, f"alt{index}")))))
            for feature, index, target in outgoing[glyph]
        )
        claims.extend(
            (target, _GlyphName(stem=tuple(*named[part].codes for part in parts), form=_Form.joined(right=named[parts[0]].form.right, left=named[parts[-1]].form.left)))
            for parts, target in joined[glyph]
            if all(part in named and named[part].codes for part in parts)
        )
    return {glyph: named[glyph] for glyph in order if glyph in named}


# --- [ANCHORS]


def _anchors(font: TTFont, named: dict[str, _GlyphName], /) -> dict[str, dict[str, tuple[float, float]]]:
    """Returns anchors by glyph from GPOS mark attachment, each mark class voted to the side most of its marks' code points take."""
    found: dict[str, dict[str, tuple[float, float]]] = {}
    for sub in tuple(*_subtables(lookup) for _, _, lookup in _lookups(font, "GPOS")):
        match sub:
            case BaseTable(MarkCoverage=Coverage(glyphs=marks), MarkArray=BaseTable(MarkRecord=records), BaseCoverage=Coverage(glyphs=bases), BaseArray=BaseTable(BaseRecord=rows)):
                attached = tuple((glyph, "", row.BaseAnchor) for glyph, row in zip(bases, rows))
            case BaseTable(MarkCoverage=Coverage(glyphs=marks), MarkArray=BaseTable(MarkRecord=records), LigatureCoverage=Coverage(glyphs=bases), LigatureArray=BaseTable(LigatureAttach=rows)):
                attached = tuple((glyph, f"_{index}", component.LigatureAnchor) for glyph, row in zip(bases, rows) for index, component in enumerate(row.ComponentRecord, 1))
            case BaseTable(Mark1Coverage=Coverage(glyphs=marks), Mark1Array=BaseTable(MarkRecord=records), Mark2Coverage=Coverage(glyphs=bases), Mark2Array=BaseTable(Mark2Record=rows)):
                attached = tuple((glyph, "", row.Mark2Anchor) for glyph, row in zip(bases, rows))
            case _:
                continue
        votes = Counter((record.Class, _Side.of(code)) for glyph, record in zip(marks, records) if glyph in named for code in named[glyph].codes[:1])
        sides = {klass: _Side.BELOW if votes[klass, _Side.BELOW] > votes[klass, _Side.ABOVE] else _Side.ABOVE for klass in {record.Class for record in records}}
        placed = (
            *((glyph, f"_{sides[record.Class].anchor}", record.MarkAnchor) for glyph, record in zip(marks, records)),
            *((glyph, f"{sides[klass].anchor}{suffix}", point) for glyph, suffix, points in attached for klass, point in enumerate(points) if point and klass in sides),
        )
        for glyph, name, point in placed:
            found.setdefault(glyph, {}).setdefault(name, (point.XCoordinate, point.YCoordinate))
    return found


# --- [SOURCES]


def _source(font: TTFont, face: Faces, /) -> _Source:
    """Builds the source of an opened binary: text faces named and anchored from cmap and layout, ornament faces kept under their original names."""
    cmap, advances, order = font.getBestCmap() or {}, font["hmtx"].metrics, font.getGlyphOrder()
    glyphs = font.getGlyphSet()

    @cache
    def outline(name: str, /) -> _Outline:
        return _traced(lambda pen: glyphs[name].draw(DecomposingFilterPen(pen, glyphs)))

    native = _lookups(font, "GSUB") if isinstance(face, Face) else ()
    features = {index: features for index, features, _ in native}
    grouped = {index: _subtables(lookup) for index, _, lookup in native}
    singles = {key: dict(ChainMap(*(sub.mapping for sub in group if isinstance(sub, SingleSubst)))) for key, group in grouped.items() if any(isinstance(sub, SingleSubst) for sub in group)}
    ligatures = {
        key: tuple(((first, *entry.Component), entry.LigGlyph) for sub in group if isinstance(sub, LigatureSubst) for first, rows in sub.ligatures.items() for entry in rows)
        for key, group in grouped.items()
        if any(isinstance(sub, LigatureSubst) for sub in group)
    }
    rules = tuple(*entries for entries in ligatures.values())
    match face:
        case Face(genre=genre):
            kept = {code: glyph for code, glyph in cmap.items() if genre.coverage.exclusion(code).is_none()}

            def priority(code: int, /) -> tuple[bool, int, int]:
                character = chr(code)
                digit = genre.script.digits.index(zero) if (zero := code - unicodedata.decimal(character, 0)) in genre.script.digits else 0
                return (not (genre.script.scripts & script_extension(character)), digit, code)  # type: ignore[no-untyped-call]

            coded = tuple(sorted(((code, glyph, unicodedata.decomposition(chr(code)).split()) for code, glyph in kept.items()), key=lambda row: priority(row[0])))
            named, art = _named(order, coded, singles, features, rules, font.getGlyphID), False
        case OrnamentFace():
            kept, spaces = cmap, {glyph: code for code, glyph in sorted(cmap.items(), reverse=True) if unicodedata.category(chr(code)) == "Zs"}
            named, art = ({glyph: _GlyphName(stem=(spaces[glyph],)) if glyph in spaces else _GlyphName(stem=glyph) for glyph in order if glyph == ".notdef" or glyph in spaces or outline(glyph)}, True)
    encodings: defaultdict[str, list[int]] = defaultdict(list)
    for code, glyph in kept.items():
        encodings[glyph].append(code)
    anchors = {} if art else _anchors(font, named)
    renamed = {source: tuple(named[part].text for part in parts) for parts, source in rules if source in named and all(part in named for part in parts)}
    os2 = font["OS/2"]
    hhea = font["hhea"]
    head = font["head"]
    post = font["post"]
    qualifiers = {
        index: (
            lookup.LookupFlag & 0xFF & ~LOOKUP_FLAG_USE_MARK_FILTERING_SET,
            frozenset(named[glyph].text for glyph, klass in font["GDEF"].table.MarkAttachClassDef.classDefs.items() if klass == lookup.LookupFlag >> 8 and glyph in named)
            if lookup.LookupFlag >> 8
            else None,
            frozenset(named[glyph].text for glyph in font["GDEF"].table.MarkGlyphSetsDef.Coverage[lookup.MarkFilteringSet].glyphs if glyph in named)
            if lookup.LookupFlag & LOOKUP_FLAG_USE_MARK_FILTERING_SET
            else None,
        )
        for index, _, lookup in native
    }
    lookups: tuple[Lookups, ...] = (
        *(
            Single(
                features=features[index],
                order=index,
                flags=qualifiers[index][0],
                mark_attachment=qualifiers[index][1],
                mark_filtering=qualifiers[index][2],
                mapping=frozendict({named[a].text: named[b].text for a, b in mapping.items() if a in named and b in named}),
            )
            for index, mapping in singles.items()
        ),
        *(
            Ligatures(
                features=features[index],
                order=index,
                flags=qualifiers[index][0],
                mark_attachment=qualifiers[index][1],
                mark_filtering=qualifiers[index][2],
                rules=tuple((tuple(named[part].text for part in parts), named[target].text) for parts, target in entries if target in named and all(part in named for part in parts)),
            )
            for index, entries in ligatures.items()
        ),
    )
    return _Source(
        face=face,
        upm=head.unitsPerEm,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        glyphs=tuple(
            _Glyph(
                name=name,
                role=Ornament() if art and isinstance(name.stem, str) and name.stem != ".notdef" else _role(name, components=renamed.get(glyph, ())),
                advance=advances[glyph][0],
                codes=frozenset(encodings[glyph]),
                contours=outline(glyph),
                anchors=tuple(_Anchor(name=anchor, x=x, y=y) for anchor, (x, y) in sorted(anchors.get(glyph, {}).items())),
            )
            for glyph, name in named.items()
        ),
        info=_Info(
            created=datetime.fromtimestamp(head.created + epoch_diff, UTC),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            version=divmod(round(head.fontRevision * 1000), 1000),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            copyright=(font["name"].getDebugName(0) or "").strip(),
            designer=(font["name"].getDebugName(9) or "").strip(),
            vendor=os2.achVendID.strip(),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        ),
        metrics=_Metrics(
            ascender=hhea.ascent,
            descender=hhea.descent,
            win_ascent=os2.usWinAscent,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            win_descent=os2.usWinDescent,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            caret_offset=hhea.caretOffset,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            italic_angle=post.italicAngle,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            lowest_ppem=head.lowestRecPPEM,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        ),
        lookups=tuple(sorted(lookups, key=lambda lookup: lookup.order)),
    )


def _imported(original: Path | bytes, face: Faces, /) -> Outcome[_Source]:
    """Reads a TrueType or CFF original into a source, `UNREADABLE` for an unreadable file or a refused font."""
    try:
        with TTFont(BytesIO(original.read_bytes() if isinstance(original, Path) else original), lazy=True, recalcBBoxes=False, recalcTimestamp=False) as font:
            return Ok(_source(font, face))
    except (OSError, TTLibError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(original) if isinstance(original, Path) else face.stem, error))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []

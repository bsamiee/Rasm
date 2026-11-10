"""Binary original import into a face source: glyph names, roles, outlines, anchors, records, and substitution lookups."""

from typing import Literal
lazy from collections import ChainMap, Counter, defaultdict, deque
lazy from collections.abc import Iterator
lazy from datetime import datetime, UTC
lazy from functools import cache, partial
lazy from io import BytesIO
lazy from itertools import chain
lazy import unicodedata

lazy from expression import Option, Some
lazy from expression.extra.result import catch
lazy from fontTools.misc.timeTools import epoch_diff
lazy from fontTools.otlLib.builder import LOOKUP_FLAG_USE_MARK_FILTERING_SET
lazy from fontTools.pens.filterPen import DecomposingFilterPen
lazy from fontTools.ttLib import TTFont, TTLibError
lazy from fontTools.ttLib.tables.otBase import BaseTable
lazy from fontTools.ttLib.tables.otTables import Coverage, LigatureSubst, SingleSubst
lazy from fontTools.unicodedata import script_extension

lazy from rasm_records import Fault, Outcome, Reason
lazy from rasm_typography._faces.declaration import Face, Faces, OrnamentFace
lazy from rasm_typography._faces.glyphs import Anchor, Glyph, Info, Joinings, Ornament, Point, role, Side, Source, traced, VerticalMetrics
lazy from rasm_typography._faces.layout import Ligatures, Lookups, Single
lazy from rasm_typography._faces.names import Form, GlyphName

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
    return tuple(
        (index, frozendict({tag: frozendict(systems) for tag, systems in held.items()}), lookup)
        for index, held in sorted(features.items())
        for lookup in (table.LookupList.Lookup[index],)
        if isinstance(lookup, BaseTable)
    )


def subtables(lookup: BaseTable, /) -> Iterator[BaseTable]:
    """Unwraps native extension subtables without changing their priority."""
    for entry in lookup.iterSubTables():
        match entry.value:
            case BaseTable(ExtSubTable=BaseTable() as held) | held:
                yield held


# --- [SOURCES]


def _source(original: bytes, face: Faces, joinings: Joinings, /) -> Source:
    """Builds the source of a binary: ornament faces under their original names, text glyphs named by their first claim (primary script and digits, code order, presentation forms, layout) and anchored from GPOS mark attachment, each mark class on the side most of its marks take."""
    font = TTFont(BytesIO(original), lazy=True, recalcBBoxes=False, recalcTimestamp=False)
    cmap, advances, order = font.getBestCmap() or {}, font["hmtx"].metrics, font.getGlyphOrder()
    glyphs = font.getGlyphSet()

    @cache
    def outline(name: str, /) -> tuple[tuple[Point, ...], ...]:
        return traced(lambda pen: glyphs[name].draw(DecomposingFilterPen(pen, glyphs)))

    native = _lookups(font, "GSUB") if isinstance(face, Face) else ()
    features = {index: features for index, features, _ in native}
    grouped = {index: tuple(subtables(lookup)) for index, _, lookup in native}
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
                return (not (genre.script.scripts & script_extension(character)), digit, code)

            coded = tuple(sorted(((code, glyph, unicodedata.decomposition(chr(code)).split()) for code, glyph in kept.items()), key=lambda row: priority(row[0])))
            outgoing: defaultdict[str, list[tuple[str, int, str]]] = defaultdict(list)
            for source, row in ((source, (feature, index, target)) for index, mapping in singles.items() for feature in features[index] for source, target in mapping.items()):
                outgoing[source].append(row)
            joined: defaultdict[str, list[tuple[tuple[str, ...], str]]] = defaultdict(list)
            for part, found in ((part, ligature) for ligature in rules for part in dict.fromkeys(ligature[0])):
                joined[part].append(found)
            claimed: dict[str, GlyphName] = {}
            taken: set[str] = set()
            claims = deque(
                chain(
                    ((".notdef", GlyphName(stem=".notdef")),),
                    ((glyph, GlyphName(stem=(code,))) for code, glyph, parts in coded if not parts[0:1] or not parts[0].startswith("<")),
                    (
                        (glyph, GlyphName(stem=(int(parts[1], 16), *(int(part, 16) for part in parts[2:])), form=Form(parts[0][1:5])))
                        for _, glyph, parts in coded
                        if parts[:1] and parts[0][1:5] in Form
                    ),
                    ((glyph, GlyphName(stem=(code,))) for code, glyph, _ in coded),
                )
            )
            while claims:
                glyph, claim = claims.popleft()
                name = GlyphName(stem=claim.stem, form=claim.form, suffixes=(*claim.suffixes, f"gid{font.getGlyphID(glyph)}")) if claim.text in taken else claim
                if glyph in claimed or len(name.text) > GlyphName.LIMIT:
                    continue
                claimed[glyph] = name
                taken.add(name.text)
                claims.extend(
                    (target, GlyphName(stem=name.stem, form=Form(feature), suffixes=name.suffixes))
                    if feature in Form and feature != Form.ISOL and len(name.codes) == 1
                    else (target, GlyphName(stem=name.stem, form=name.form, suffixes=(*name.suffixes, f"alt{index}")))
                    for feature, index, target in outgoing[glyph]
                )
                claims.extend(
                    (target, GlyphName(stem=tuple(*claimed[part].codes for part in parts), form=Form.joined(right=claimed[parts[0]].form.right, left=claimed[parts[-1]].form.left)))
                    for parts, target in joined[glyph]
                    if all(part in claimed and claimed[part].codes for part in parts)
                )
            named, art = {glyph: claimed[glyph] for glyph in order if glyph in claimed}, False
        case OrnamentFace():
            kept, spaces = cmap, {glyph: code for code, glyph in sorted(cmap.items(), reverse=True) if unicodedata.category(chr(code)) == "Zs"}
            named, art = ({glyph: GlyphName(stem=(spaces[glyph],)) if glyph in spaces else GlyphName(stem=glyph) for glyph in order if glyph == ".notdef" or glyph in spaces or outline(glyph)}, True)
    encodings: defaultdict[str, list[int]] = defaultdict(list)
    for code, glyph in kept.items():
        encodings[glyph].append(code)
    anchors: dict[str, dict[str, tuple[float, float]]] = {}
    for sub in () if art else tuple(*subtables(lookup) for _, _, lookup in _lookups(font, "GPOS")):
        match sub:
            case BaseTable(MarkCoverage=Coverage(glyphs=marks), MarkArray=BaseTable(MarkRecord=records), BaseCoverage=Coverage(glyphs=bases), BaseArray=BaseTable(BaseRecord=rows)):
                attached = tuple((base, "", row.BaseAnchor) for base, row in zip(bases, rows))
            case BaseTable(MarkCoverage=Coverage(glyphs=marks), MarkArray=BaseTable(MarkRecord=records), LigatureCoverage=Coverage(glyphs=bases), LigatureArray=BaseTable(LigatureAttach=rows)):
                attached = tuple((base, f"_{index}", component.LigatureAnchor) for base, row in zip(bases, rows) for index, component in enumerate(row.ComponentRecord, 1))
            case BaseTable(Mark1Coverage=Coverage(glyphs=marks), Mark1Array=BaseTable(MarkRecord=records), Mark2Coverage=Coverage(glyphs=bases), Mark2Array=BaseTable(Mark2Record=rows)):
                attached = tuple((base, "", row.Mark2Anchor) for base, row in zip(bases, rows))
            case _:
                continue
        votes = Counter((record.Class, Side.of(code)) for mark, record in zip(marks, records) if mark in named for code in named[mark].codes[:1])
        sides = {klass: Side.BELOW if votes[klass, Side.BELOW] > votes[klass, Side.ABOVE] else Side.ABOVE for klass in {record.Class for record in records}}
        placed = (
            *((mark, f"_{sides[record.Class].anchor}", record.MarkAnchor) for mark, record in zip(marks, records)),
            *((base, f"{sides[klass].anchor}{suffix}", point) for base, suffix, points in attached for klass, point in enumerate(points) if point and klass in sides),
        )
        for owner, label, point in placed:
            anchors.setdefault(owner, {}).setdefault(label, (point.XCoordinate, point.YCoordinate))
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
    return Source(
        face=face,
        upm=head.unitsPerEm,  # ty: ignore[missing-attribute]
        glyphs=tuple(
            Glyph(
                name=name,
                role=Ornament() if art and isinstance(name.stem, str) and name.stem != ".notdef" else role(joinings, name, components=renamed.get(glyph, ())),
                advance=advances[glyph][0],
                codes=frozenset(encodings[glyph]),
                contours=outline(glyph),
                anchors=tuple(Anchor(name=anchor, x=x, y=y) for anchor, (x, y) in sorted(anchors.get(glyph, {}).items())),
            )
            for glyph, name in named.items()
        ),
        info=Info(
            created=datetime.fromtimestamp(head.created + epoch_diff, UTC),  # ty: ignore[missing-attribute]
            version=divmod(round(head.fontRevision * 1000), 1000),  # ty: ignore[missing-attribute]
            copyright=Option.of_optional(font["name"].getDebugName(0)).map(str.strip).filter(bool).to_optional(),
            designer=Option.of_optional(font["name"].getDebugName(9)).map(str.strip).filter(bool).to_optional(),
            vendor=Some(os2.achVendID.strip()).filter(bool).to_optional(),  # ty: ignore[missing-attribute]
        ),
        metrics=VerticalMetrics(
            ascender=hhea.ascent,
            descender=hhea.descent,
            win_ascent=os2.usWinAscent,  # ty: ignore[missing-attribute]
            win_descent=os2.usWinDescent,  # ty: ignore[missing-attribute]
            caret_offset=hhea.caretOffset,  # ty: ignore[missing-attribute]
            italic_angle=post.italicAngle,  # ty: ignore[missing-attribute]
            lowest_ppem=head.lowestRecPPEM,  # ty: ignore[missing-attribute]
        ),
        lookups=tuple(sorted(lookups, key=lambda lookup: lookup.order)),
    )


def imported(original: bytes, face: Faces, joinings: Joinings, /) -> Outcome[Source]:
    """Reads a TrueType or CFF original into a source under a face's design record.

    Args:
        original: Font file contents the face is rebuilt from.
        face: Design record the source carries.
        joinings: Unicode joining type and group of each code point `fetched` read.

    Returns:
        Ok(source), `UNREADABLE` for bytes fontTools refuses.
    """
    return catch(_source, exception=TTLibError)(original, face, joinings).map_error(partial(Fault.of, Reason.UNREADABLE, face.postscript))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["imported", "subtables"]

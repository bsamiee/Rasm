"""Binary original import into a face source: glyph names, roles, outlines, anchors, records, and substitution lookups."""

from typing import Literal
lazy from collections import ChainMap, Counter, defaultdict, deque
lazy from collections.abc import Iterator
lazy from datetime import datetime, UTC
lazy from functools import cache
lazy from io import BytesIO
lazy from itertools import chain, groupby
lazy import unicodedata

import msgspec
lazy from expression import Option, Some
lazy from fontTools.misc.timeTools import epoch_diff
lazy from fontTools.otlLib.builder import LOOKUP_FLAG_USE_MARK_FILTERING_SET
lazy from fontTools.pens.filterPen import DecomposingFilterPen
lazy from fontTools.ttLib import TTFont, TTLibError
lazy from fontTools.ttLib.tables.otBase import BaseTable
lazy from fontTools.ttLib.tables.otTables import Coverage, LigatureSubst, SingleSubst
lazy from fontTools.unicodedata import script_extension

lazy from rasm_records import Error, Fault, Ok, Outcome, Reason
lazy from rasm_typography._faces.declaration import Face, Faces, OrnamentFace
lazy from rasm_typography._faces.glyphs import Anchor, Glyph, Info, Joinings, Ornament, Point, role, Side, Source, traced, VerticalMetrics
lazy from rasm_typography._faces.layout import Chain, Contextual, Ligatures, Lookups, Single
lazy from rasm_typography._faces.names import Form, GlyphName
lazy from rasm_typography._shaping.runs import Header

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


def _records(rule: BaseTable, /) -> tuple[tuple[int, int], ...]:
    """Sequence index and lookup index of each nested substitution a contextual rule applies, in record order."""
    return tuple((int(record.SequenceIndex), int(record.LookupListIndex)) for record in rule.SubstLookupRecord)


def _source(original: bytes, face: Faces, joinings: Joinings, /) -> Outcome[Source]:
    """Builds the source of a binary: ornament faces under their original names, text glyphs named by their first claim (primary script and digits, code order, presentation forms, layout, nested lookups) and anchored from GPOS marks, each mark class on the side most of its marks take."""
    font = TTFont(BytesIO(original), lazy=True, recalcBBoxes=False, recalcTimestamp=False)
    cmap, advances, order = font.getBestCmap() or {}, font["hmtx"].metrics, font.getGlyphOrder()
    glyphs = font.getGlyphSet()

    @cache
    def outline(name: str, /) -> tuple[tuple[Point, ...], ...]:
        return traced(lambda pen: glyphs[name].draw(DecomposingFilterPen(pen, glyphs)))

    def classes(definition: BaseTable | None, /) -> dict[int, tuple[str, ...]]:
        held = definition.classDefs if definition is not None else {}
        return {klass: tuple(members) for klass, members in groupby(sorted(order, key=lambda glyph: held.get(glyph, 0)), key=lambda glyph: held.get(glyph, 0))}

    def chains(sub: BaseTable, /) -> tuple[tuple[tuple[tuple[str, ...], ...], tuple[tuple[str, ...], ...], tuple[tuple[str, ...], ...], tuple[tuple[int, int], ...]], ...] | None:
        """Rules of a contextual or chaining substitution of any format as backtrack in reading order, input, and lookahead classes with each nested lookup's sequence index, classes inverted once per definition, `None` for no contextual format."""
        match sub:
            case BaseTable(Format=1, Coverage=Coverage(glyphs=firsts), SubRuleSet=sets):
                return tuple(((), ((str(first),), *((str(glyph),) for glyph in rule.Input)), (), _records(rule)) for first, held in zip(firsts, sets, strict=True) if held for rule in held.SubRule)
            case BaseTable(Format=2, Coverage=Coverage(glyphs=covered), ClassDef=definition, SubClassSet=sets):
                inputs, cover = classes(definition), frozenset(map(str, covered))
                return tuple(
                    ((), (tuple(glyph for glyph in inputs.get(first, ()) if glyph in cover), *(inputs.get(int(klass), ()) for klass in rule.Class)), (), _records(rule))
                    for first, held in enumerate(sets)
                    if held
                    for rule in held.SubClassRule
                )
            case BaseTable(Format=3, Coverage=covers, SubstLookupRecord=_):
                return (((), tuple(tuple(map(str, cover.glyphs)) for cover in covers), (), _records(sub)),)
            case BaseTable(Format=1, Coverage=Coverage(glyphs=firsts), ChainSubRuleSet=sets):
                return tuple(
                    (
                        tuple((str(glyph),) for glyph in reversed(rule.Backtrack)),
                        ((str(first),), *((str(glyph),) for glyph in rule.Input)),
                        tuple((str(glyph),) for glyph in rule.LookAhead),
                        _records(rule),
                    )
                    for first, held in zip(firsts, sets, strict=True)
                    if held
                    for rule in held.ChainSubRule
                )
            case BaseTable(Format=2, Coverage=Coverage(glyphs=covered), BacktrackClassDef=back, InputClassDef=inputs, LookAheadClassDef=ahead, ChainSubClassSet=sets):
                (before, within, after), cover = map(classes, (back, inputs, ahead)), frozenset(map(str, covered))
                return tuple(
                    (
                        tuple(before.get(int(klass), ()) for klass in reversed(rule.Backtrack)),
                        (tuple(glyph for glyph in within.get(first, ()) if glyph in cover), *(within.get(int(klass), ()) for klass in rule.Input)),
                        tuple(after.get(int(klass), ()) for klass in rule.LookAhead),
                        _records(rule),
                    )
                    for first, held in enumerate(sets)
                    if held
                    for rule in held.ChainSubClassRule
                )
            case BaseTable(Format=3, BacktrackCoverage=back, InputCoverage=inputs, LookAheadCoverage=ahead):
                return (
                    (
                        tuple(tuple(map(str, cover.glyphs)) for cover in reversed(back)),
                        tuple(tuple(map(str, cover.glyphs)) for cover in inputs),
                        tuple(tuple(map(str, cover.glyphs)) for cover in ahead),
                        _records(sub),
                    ),
                )
            case _:
                return None

    native = _lookups(font, "GSUB") if isinstance(face, Face) else ()
    table = font["GSUB"].table.LookupList.Lookup if native else []
    features = {index: features for index, features, _ in native}
    grouped = {index: tuple(subtables(lookup)) for index, _, lookup in native}
    decoded = {key: tuple((sub, chains(sub)) for sub in group if not isinstance(sub, SingleSubst | LigatureSubst)) for key, group in grouped.items()}
    contexts = {key: tuple(*rules for _, rules in found if rules is not None) for key, found in decoded.items() if found}
    inherited = {inner: features[key] for key, found in contexts.items() for *_, records in found for _, inner in records}
    every = grouped | {inner: tuple(subtables(table[inner])) for inner in inherited.keys() - grouped.keys()}
    singles = {key: dict(ChainMap(*(sub.mapping for sub in group if isinstance(sub, SingleSubst)))) for key, group in every.items() if any(isinstance(sub, SingleSubst) for sub in group)}
    ligatures = {
        key: tuple(((first, *entry.Component), entry.LigGlyph) for sub in group if isinstance(sub, LigatureSubst) for first, rows in sub.ligatures.items() for entry in rows)
        for key, group in every.items()
        if any(isinstance(sub, LigatureSubst) for sub in group)
    }
    rules = tuple(*entries for entries in ligatures.values())
    if refused := tuple(
        dict.fromkeys(
            f"lookup {key} {type(sub).__name__} format {sub.Format}"
            for key, found in (*decoded.items(), *((inner, tuple((sub, None) for sub in every[inner] if not isinstance(sub, SingleSubst | LigatureSubst))) for inner in every.keys() - grouped.keys()))
            for sub, rules in found
            if rules is None
        )
    ):
        return Error(Fault.of(Reason.OUTSIDE, f"{face.postscript} GSUB", ", ".join(refused), member="single and ligature substitutions and contextual ones of formats 1 to 3 nesting them"))
    match face:
        case Face(genre=genre):
            kept = {code: glyph for code, glyph in cmap.items() if genre.coverage.exclusion(code).is_none()}

            def priority(code: int, /) -> tuple[bool, int, int]:
                character = chr(code)
                digit = genre.script.digits.index(zero) if (zero := code - unicodedata.decimal(character, 0)) in genre.script.digits else 0
                return (not (genre.script.scripts & script_extension(character)), digit, code)

            coded = tuple(sorted(((code, glyph, unicodedata.decomposition(chr(code)).split()) for code, glyph in kept.items()), key=lambda row: priority(row[0])))
            outgoing: defaultdict[str, list[tuple[str, int, str]]] = defaultdict(list)
            for source, row in (
                (source, (feature, index, target)) for index, mapping in singles.items() for feature in features.get(index, inherited.get(index, frozendict())) for source, target in mapping.items()
            ):
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
    header = Header.read(font)
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
        for index in every
        for lookup in (table[index],)
    }
    models: dict[int, Single | Ligatures] = {
        **{
            index: Single(
                features=features.get(index, frozendict()),
                order=index,
                mapping=frozendict({named[a].text: named[b].text for a, b in mapping.items() if a in named and b in named}),
                flags=flags,
                mark_attachment=attachment,
                mark_filtering=filtering,
            )
            for index, mapping in singles.items()
            for flags, attachment, filtering in (qualifiers[index],)
        },
        **{
            index: Ligatures(
                features=features.get(index, frozendict()),
                order=index,
                rules=tuple((tuple(named[part].text for part in parts), named[target].text) for parts, target in entries if target in named and all(part in named for part in parts)),
                flags=flags,
                mark_attachment=attachment,
                mark_filtering=filtering,
            )
            for index, entries in ligatures.items()
            for flags, attachment, filtering in (qualifiers[index],)
        },
    }
    lookups: tuple[Lookups, ...] = (
        *(model for index, model in models.items() if index in features),
        *(
            Contextual(
                features=features[index],
                order=index,
                rules=tuple(
                    Chain(
                        backtrack=tuple(tuple(named[glyph].text for glyph in klass if glyph in named) for klass in back),
                        input=tuple(
                            (tuple(named[glyph].text for glyph in klass if glyph in named), tuple(models[inner] for at, inner in records if at == position)) for position, klass in enumerate(inputs)
                        ),
                        lookahead=tuple(tuple(named[glyph].text for glyph in klass if glyph in named) for klass in ahead),
                    )
                    for back, inputs, ahead, records in found
                    if all(any(glyph in named for glyph in klass) for klass in (*back, *inputs, *ahead))
                ),
                flags=flags,
                mark_attachment=attachment,
                mark_filtering=filtering,
            )
            for index, found in contexts.items()
            for flags, attachment, filtering in (qualifiers[index],)
        ),
    )
    return Ok(
        Source(
            face=face,
            upm=header.head.units_per_em,
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
                created=datetime.fromtimestamp(header.head.created + epoch_diff, UTC),
                version=divmod(round(header.head.font_revision * 1000), 1000),
                copyright=Option.of_optional(font["name"].getDebugName(0)).map(str.strip).filter(bool).to_optional(),
                designer=Option.of_optional(font["name"].getDebugName(9)).map(str.strip).filter(bool).to_optional(),
                vendor=Some(header.os2.ach_vend_id.strip()).filter(bool).to_optional(),
            ),
            metrics=VerticalMetrics(
                ascender=header.hhea.ascent,
                descender=header.hhea.descent,
                win_ascent=header.os2.us_win_ascent,
                win_descent=header.os2.us_win_descent,
                caret_offset=header.hhea.caret_offset,
                italic_angle=header.post.italic_angle,
                lowest_ppem=header.head.lowest_rec_ppem,
            ),
            lookups=tuple(sorted(lookups, key=lambda lookup: lookup.order)),
        )
    )


def imported(original: bytes, face: Faces, joinings: Joinings, /) -> Outcome[Source]:
    """Reads a TrueType or CFF original into a source under a face's design record.

    Args:
        original: Font file contents the face is rebuilt from.
        face: Design record the source carries.
        joinings: Unicode joining type and group of each code point `fetched` read.

    Returns:
        Ok(source), `UNREADABLE` for bytes fontTools refuses or header tables lacking a field, `OUTSIDE` naming each GSUB lookup kind or format the source holds no record for.
    """
    try:
        return _source(original, face, joinings)
    except (TTLibError, msgspec.ValidationError) as error:
        return Error(Fault.of(Reason.UNREADABLE, face.postscript, error))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["imported", "subtables"]

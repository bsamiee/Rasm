"""Feature text from ordered source lookups through native feature statements and compiler compression."""

from itertools import starmap
from typing import ClassVar
lazy from collections import Counter
lazy from collections.abc import Iterable, Mapping

import msgspec
lazy from fontTools.feaLib import ast
lazy from fontTools.unicodedata import ot_tags_from_script
lazy from ufo2ft.featureCompiler import FeatureCompiler
lazy from ufo2ft.featureWriters import CursFeatureWriter, GdefFeatureWriter, MarkFeatureWriter
lazy from ufoLib2 import Font

lazy from rasm.typography._faces.glyphs import Source
lazy from rasm.typography._faces.layout import Adjustment, Chain, Contextual, Lifts, Ligatures, Lookups, Pairs, Positions, Single

# --- [SERVICES] -------------------------------------------------------------------------


class FeatureWriter(msgspec.Struct, frozen=True, kw_only=True):
    """Native feature-writer conformance composing generated attachment handles with scoped source positioning."""

    source: Source
    tableTag: ClassVar[str] = "GPOS"  # ruff: ignore[mixed-case-variable-in-class-scope]
    mode: ClassVar[str] = "append"
    insertFeatureMarker: ClassVar[None] = None  # ruff: ignore[mixed-case-variable-in-class-scope]

    @property
    def substitutions(self) -> str:
        """Source substitutions available to native glyph classification before positioning is generated."""
        fea, systems = ast.FeatureFile(), _systems(self.source)
        entries = ((index, lookup) for index, lookup in enumerate(self.source.lookups) if isinstance(lookup, Single | Ligatures | Contextual))
        fea.statements.extend((*starmap(ast.LanguageSystemStatement, systems), *_statements(entries, systems, {})))
        return str(fea.asFea())

    def write(self, font: Font, feaFile: ast.FeatureFile, compiler: FeatureCompiler | None = None) -> bool:  # ruff: ignore[invalid-argument-name]
        """Constructs native attachments before custom positioning, then stores complete executable feature text on its UFO."""
        before = frozenset(feaFile.statements)
        written = [writer.write(font, feaFile, compiler=compiler) for writer in [CursFeatureWriter(mode="append"), MarkFeatureWriter(mode="append")]]
        inherited = {
            block.name: tuple(statement if isinstance(statement, ast.LookupReferenceStatement) else ast.LookupReferenceStatement(statement) for statement in block.statements)
            for block in feaFile.statements
            if isinstance(block, ast.FeatureBlock) and block not in before
        }
        statements = _statements(((index, lookup) for index, lookup in enumerate(self.source.lookups) if isinstance(lookup, Positions | Pairs | Lifts)), _systems(self.source), inherited)
        feaFile.statements.extend(statements)
        written.append(GdefFeatureWriter().write(font, feaFile, compiler=compiler))
        font.features.text = feaFile.asFea()
        return bool(statements) or any(written)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STATEMENTS]


def _class(glyphs: Iterable[str], /) -> ast.GlyphClass:
    return ast.GlyphClass(list(glyphs))


def _value(adjustment: Adjustment, /) -> ast.ValueRecord:
    return ast.ValueRecord(xPlacement=adjustment.dx, yPlacement=adjustment.dy, xAdvance=adjustment.advance, yAdvance=0)


def _blocks(name: str, lookup: Lookups, /) -> tuple[ast.LookupBlock, ...]:
    """Returns a lookup's block under its own flags, a chaining lookup's nested lookups first and its chaining block last."""
    block = ast.LookupBlock(name)
    block.statements.append(
        ast.LookupFlagStatement(
            value=lookup.flags,
            markAttachment=_class(sorted(lookup.mark_attachment)) if lookup.mark_attachment is not None else None,
            markFilteringSet=_class(sorted(lookup.mark_filtering)) if lookup.mark_filtering is not None else None,
        )
    )
    match lookup:
        case Single(mapping=mapping):
            block.statements.extend([ast.SingleSubstStatement([_class(mapping)], [_class(mapping.values())], [], [], forceChain=False)] if mapping else [])
            return (block,)
        case Ligatures(rules=rules):
            targets = dict(reversed(rules))
            block.statements.extend(
                *(*((ast.SubtableStatement(),) if position else ()), ast.LigatureSubstStatement([], [ast.GlyphName(part) for part in parts], [], ast.GlyphName(targets[parts]), forceChain=False))
                for position, parts in enumerate(dict.fromkeys(parts for parts, _ in rules))
            )
            return (block,)
        case Positions(values=values):
            block.statements.extend(ast.SinglePosStatement([(ast.GlyphName(glyph), _value(adjustment))], [], [], forceChain=False) for glyph, adjustment in values.items())
            return (block,)
        case Pairs(values=values):
            block.statements.extend(ast.PairPosStatement(ast.GlyphName(first), _value(adjustment), ast.GlyphName(second), None) for (first, second), adjustment in values.items())
            return (block,)
        case Contextual(rules=rules):
            nested = tuple((rule, tuple(applied for _, applied in rule.input)) for rule in rules)
            return _chained(name, block, nested, ast.ChainContextSubstStatement, ast.IgnoreSubstStatement)
        case Lifts(rules=rules):
            adjusted = tuple(
                (
                    rule,
                    tuple(
                        ()
                        if change is None
                        else (
                            Positions(
                                features=frozendict(), flags=lookup.flags, mark_attachment=lookup.mark_attachment, mark_filtering=lookup.mark_filtering, values=frozendict.fromkeys(glyphs, change)
                            ),
                        )
                        for glyphs, change in rule.input
                    ),
                )
                for rule in rules
            )
            return _chained(name, block, adjusted, ast.ChainContextPosStatement, ast.IgnorePosStatement)


def _chained(
    name: str,
    block: ast.LookupBlock,
    rules: tuple[tuple[Chain[tuple[Single | Ligatures, ...]] | Chain[Adjustment | None], tuple[tuple[Single | Ligatures | Positions, ...], ...]], ...],
    statement: type[ast.ChainContextSubstStatement | ast.ChainContextPosStatement],
    ignore: type[ast.IgnoreSubstStatement | ast.IgnorePosStatement],
    /,
) -> tuple[ast.LookupBlock, ...]:
    """Returns the nested lookups the rules apply, each single change packed into the first nested lookup of its kind and flags agreeing on its glyphs, then the chaining block, each repeated class named once, in subtables of 64 rules a 16-bit offset reaches, a rule applying none an exception."""
    slots: list[Single | Ligatures | Positions] = []
    placed: dict[Single | Ligatures | Positions, int] = {}
    for applied in dict.fromkeys(applied for _, positions in rules for held in positions for applied in held):
        slot, merged = next(((index, merged) for index, held in enumerate(slots) if (merged := _packed(held, applied)) is not None), (len(slots), applied))
        slots[slot : slot + 1], placed[applied] = (merged,), slot
    nested = tuple(_blocks(f"{name}_{index}", lookup) for index, lookup in enumerate(slots))
    counted = Counter(*(*rule.backtrack, *(glyphs for glyphs, _ in rule.input), *rule.lookahead) for rule, _ in rules)
    shared = {glyphs: ast.GlyphClassDefinition(f"{name}_{index}", _class(glyphs)) for index, glyphs in enumerate(glyphs for glyphs, count in counted.items() if count > 1 and len(glyphs) > 1)}

    def classed(glyphs: tuple[str, ...], /) -> ast.GlyphClass | ast.GlyphClassName:
        return ast.GlyphClassName(shared[glyphs]) if glyphs in shared else _class(glyphs)

    block.statements.extend(shared.values())
    block.statements.extend(
        *(
            *((ast.SubtableStatement(),) if index and not index % 64 else ()),
            statement(
                list(map(classed, rule.backtrack)),
                [classed(glyphs) for glyphs, _ in rule.input],
                list(map(classed, rule.lookahead)),
                [[nested[placed[applied]][-1] for applied in held] or None for held in positions],
            )
            if any(positions)
            else ignore([(list(map(classed, rule.backtrack)), [classed(glyphs) for glyphs, _ in rule.input], list(map(classed, rule.lookahead)))]),
        )
        for index, (rule, positions) in enumerate(rules)
    )
    return (*(*blocks for blocks in nested), block)


def _packed(held: Single | Ligatures | Positions, applied: Single | Ligatures | Positions, /) -> Single | Positions | None:
    """Returns one nested lookup holding the changes of two of the same kind and flags that disagree on no glyph, `None` for any other pair."""
    match held, applied:
        case (Single(mapping=one), Single(mapping=two)) | (Positions(values=one), Positions(values=two)) if (held.flags, held.mark_attachment, held.mark_filtering) == (
            applied.flags,
            applied.mark_attachment,
            applied.mark_filtering,
        ) and all(one.get(glyph, change) == change for glyph, change in two.items()):
            return msgspec.structs.replace(held, mapping=one | two) if isinstance(held, Single) else msgspec.structs.replace(held, values=one | two)
        case _:
            return None


# --- [EMISSION]


def _systems(source: Source, /) -> tuple[tuple[str, str], ...]:
    """Native language registrations from encoded scripts and every explicit source association."""
    tags = sorted({*ot_tags_from_script(held) for held in source.scripts})
    return tuple(dict.fromkeys((("DFLT", "dflt"), *((tag, "dflt") for tag in tags), *sorted({*held for lookup in source.lookups for held in lookup.features.values()}))))


def _statements(entries: Iterable[tuple[int, Lookups]], systems: tuple[tuple[str, str], ...], inherited: Mapping[str, tuple[ast.LookupReferenceStatement, ...]], /) -> list[ast.Statement]:
    """Native lookup definitions and exact feature/system references, including generated positioning dependencies."""
    blocks = tuple((lookup, _blocks(f"lookup{index}", lookup)) for index, lookup in sorted(entries, key=lambda entry: entry[1].order))
    statements: list[ast.Statement] = [*held for _, held in blocks]
    for feature in dict.fromkeys(tuple(*lookup.features for lookup, _ in blocks)):
        block = ast.FeatureBlock(feature)
        for system in systems:
            if active := tuple((lookup, held) for lookup, held in blocks if feature in lookup.features and (not lookup.features[feature] or system in lookup.features[feature])):
                block.statements.extend((
                    ast.ScriptStatement(system[0]),
                    ast.LanguageStatement(system[1], include_default=False, required=any(lookup.features[feature].get(system, False) for lookup, _ in active)),
                    *inherited.get(feature, ()),
                    *(ast.LookupReferenceStatement(held[-1]) for _, held in active),
                ))
        statements.append(block)
    return statements


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FeatureWriter"]

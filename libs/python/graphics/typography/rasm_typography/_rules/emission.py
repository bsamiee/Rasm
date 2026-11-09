"""Feature text from ordered source lookups through native feature statements and compiler compression."""

# mypy: disable-error-code="no-untyped-call"

from itertools import starmap
from typing import ClassVar
lazy from collections.abc import Iterable, Mapping

import msgspec
lazy from fontTools.feaLib import ast
lazy from fontTools.unicodedata import ot_tags_from_script
lazy from ufo2ft.featureCompiler import FeatureCompiler
lazy from ufo2ft.featureWriters import CursFeatureWriter, GdefFeatureWriter, MarkFeatureWriter
lazy from ufoLib2 import Font

lazy from rasm_typography._faces.glyphs import _Source
lazy from rasm_typography._faces.layout import _Adjustment, _Chain, Contextual, Lifts, Ligatures, Lookups, Pairs, Positions, Single

# --- [SERVICES] -------------------------------------------------------------------------


class _FeatureWriter(msgspec.Struct, frozen=True, kw_only=True):
    """Native feature-writer conformance composing generated attachment handles with scoped source positioning."""

    source: _Source
    tableTag: ClassVar[str] = "GPOS"  # ruff: ignore[mixed-case-variable-in-class-scope]
    mode: ClassVar[str] = "append"
    insertFeatureMarker: ClassVar[None] = None  # ruff: ignore[mixed-case-variable-in-class-scope]

    @property
    def substitutions(self) -> str:
        """Source substitutions available to native glyph classification before positioning is generated."""
        return _rendered(self.source, ((index, lookup) for index, lookup in enumerate(self.source.lookups) if isinstance(lookup, Single | Ligatures | Contextual)))

    def write(self, font: Font, feaFile: ast.FeatureFile, compiler: FeatureCompiler | None = None) -> bool:  # ruff: ignore[invalid-argument-name]
        """Constructs native attachments before custom positioning, then stores complete executable feature text on its UFO."""
        before = frozenset(feaFile.statements)
        written = [writer(mode="append").write(font, feaFile, compiler=compiler) for writer in (CursFeatureWriter, MarkFeatureWriter)]
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


def _class(glyphs: tuple[str, ...], /) -> ast.GlyphClass:
    return ast.GlyphClass(list(glyphs))


def _value(adjustment: _Adjustment, /) -> ast.ValueRecord:
    return ast.ValueRecord(xPlacement=adjustment.dx, yPlacement=adjustment.dy, xAdvance=adjustment.advance, yAdvance=0)


def _block(name: str, statements: list[ast.Statement], flags: ast.LookupFlagStatement, /) -> ast.LookupBlock:
    block = ast.LookupBlock(name)
    block.statements.extend((flags, *statements))
    return block


def _chained(
    name: str,
    rules: tuple[_Chain[str], ...] | tuple[_Chain[_Adjustment | None], ...],
    statement: type[ast.ChainContextSubstStatement | ast.ChainContextPosStatement],
    flags: ast.LookupFlagStatement,
    /,
) -> tuple[ast.LookupBlock, ...]:
    """Returns the single lookups each rule position applies, then the chaining lookup naming them."""
    inner = {
        (index, position): _block(
            f"{name}_{index}_{position}",
            [
                ast.SingleSubstStatement([_class(glyphs)], [ast.GlyphName(change)], [], [], forceChain=False)
                if isinstance(change, str)
                else ast.SinglePosStatement([(_class(glyphs), _value(change))], [], [], forceChain=False)
            ],
            flags,
        )
        for index, rule in enumerate(rules)
        for position, (glyphs, change) in enumerate(rule.input)
        if change is not None and not (isinstance(change, str) and glyphs == (change,))
    }
    chain = _block(
        name,
        [
            statement(
                list(map(_class, rule.backtrack)),
                [_class(glyphs) for glyphs, _ in rule.input],
                list(map(_class, rule.lookahead)),
                [inner.get((index, position)) for position in range(len(rule.input))],
            )
            for index, rule in enumerate(rules)
        ],
        flags,
    )
    return (*inner.values(), chain)


def _lookups(lookup: Lookups, name: str, /) -> tuple[ast.LookupBlock, ...]:
    """Returns the lookup blocks of one record, the last the one its feature references."""
    flags = ast.LookupFlagStatement(
        value=lookup.flags,
        markAttachment=_class(tuple(sorted(lookup.mark_attachment))) if lookup.mark_attachment is not None else None,
        markFilteringSet=_class(tuple(sorted(lookup.mark_filtering))) if lookup.mark_filtering is not None else None,
    )
    match lookup:
        case Single():
            return (_block(name, [ast.SingleSubstStatement([ast.GlyphName(glyph)], [ast.GlyphName(target)], [], [], forceChain=False) for glyph, target in lookup.mapping.items()], flags),)
        case Ligatures(rules=rules):
            mapping: dict[tuple[str, ...], str] = {}
            for parts, target in rules:
                mapping.setdefault(parts, target)
            return (
                _block(
                    name,
                    [
                        *(*((ast.SubtableStatement(),) if index else ()), ast.LigatureSubstStatement([], [ast.GlyphName(part) for part in parts], [], ast.GlyphName(target), forceChain=False))
                        for index, (parts, target) in enumerate(mapping.items())
                    ],
                    flags,
                ),
            )
        case Contextual(rules=rules):
            return _chained(name, rules, ast.ChainContextSubstStatement, flags)
        case Lifts(rules=rules):
            return _chained(name, rules, ast.ChainContextPosStatement, flags)
        case Positions(values=values):
            return (_block(name, [ast.SinglePosStatement([(ast.GlyphName(glyph), _value(adjustment))], [], [], forceChain=False) for glyph, adjustment in values.items()], flags),)
        case Pairs(values=values):
            return (_block(name, [ast.PairPosStatement(ast.GlyphName(first), _value(adjustment), ast.GlyphName(second), None) for (first, second), adjustment in values.items()], flags),)


# --- [EMISSION]


def _systems(source: _Source, /) -> tuple[tuple[str, str], ...]:
    """Native language registrations from encoded scripts and every explicit source association."""
    tags = sorted({*ot_tags_from_script(held) for held in source.scripts})
    return tuple(dict.fromkeys((("DFLT", "dflt"), *((tag, "dflt") for tag in tags), *sorted({*held for lookup in source.lookups for held in lookup.features.values()}))))


def _statements(entries: Iterable[tuple[int, Lookups]], systems: tuple[tuple[str, str], ...], inherited: Mapping[str, tuple[ast.LookupReferenceStatement, ...]], /) -> list[ast.Statement]:
    """Native lookup definitions and exact feature/system references, including generated positioning dependencies."""
    blocks = tuple((lookup, _lookups(lookup, f"lookup{index}")) for index, lookup in sorted(entries, key=lambda entry: entry[1].order))
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


def _rendered(source: _Source, entries: Iterable[tuple[int, Lookups]], /) -> str:
    fea, systems = ast.FeatureFile(), _systems(source)
    fea.statements.extend((*starmap(ast.LanguageSystemStatement, systems), *_statements(entries, systems, {})))
    return fea.asFea()  # type: ignore[no-any-return] # ty: ignore[unsound-return-statement]


def _emitted(source: _Source, /) -> str:
    """Renders source lookup definitions and their exact native feature and language associations."""
    return _rendered(source, enumerate(source.lookups))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []

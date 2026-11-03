"""Feature text, kerning groups, and kerning values written once from a source's lookup records."""

from itertools import count, groupby

import msgspec
lazy from fontTools.feaLib import ast
lazy from fontTools.unicodedata import ot_tags_from_script, script

lazy from typography.faces.glyphs import Source
lazy from typography.faces.layout import Adjustment, Context, Contextual, Lift, Lifts, Ligatures, Lookups, Pairs, Positions, Single

# --- [MODELS] ---------------------------------------------------------------------------


class Emitted(msgspec.Struct, frozen=True, kw_only=True):
    """Feature text with the writers' insertion marker, and the `public.kern1`/`public.kern2` groups and kerning `KernFeatureWriter` reads."""

    features: str
    groups: frozendict[str, tuple[str, ...]]
    kerning: frozendict[tuple[str, str], int]


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [STATEMENTS]


def _class(glyphs: tuple[str, ...], /) -> ast.GlyphClass | ast.GlyphName:
    return ast.GlyphName(glyphs[0]) if len(glyphs) == 1 else ast.GlyphClass([ast.GlyphName(glyph) for glyph in glyphs])


def _value(adjustment: Adjustment, /) -> ast.ValueRecord:
    return ast.ValueRecord(xPlacement=adjustment.dx, yPlacement=adjustment.dy, xAdvance=adjustment.advance, yAdvance=0)


def _block(name: str, statements: list[ast.Statement], /) -> ast.LookupBlock:
    block = ast.LookupBlock(name)
    block.statements.extend(statements)
    return block


def _chained(name: str, rules: tuple[Context | Lift, ...], /) -> tuple[ast.LookupBlock, ...]:
    """Returns the single lookups each rule position applies, then the chaining lookup naming them."""
    inner = {
        (index, position): _block(
            f"{name}_{index}_{position}",
            [
                ast.SingleSubstStatement([_class(glyphs)], [ast.GlyphName(change)], [], [], forceChain=False)
                if isinstance(change, str)
                else ast.SinglePosStatement([(_class(glyphs), _value(change))], [], [], forceChain=False)
            ],
        )
        for index, rule in enumerate(rules)
        for position, (glyphs, change) in enumerate(zip(rule.input, rule.replacement if isinstance(rule, Context) else rule.adjustments, strict=True))
        if change is not None and not (isinstance(change, str) and glyphs == (change,))
    }
    chain = _block(
        name,
        [
            (ast.ChainContextSubstStatement if isinstance(rule, Context) else ast.ChainContextPosStatement)(
                list(map(_class, rule.backtrack)), list(map(_class, rule.input)), list(map(_class, rule.lookahead)), [inner.get((index, position)) for position in range(len(rule.input))]
            )
            for index, rule in enumerate(rules)
        ],
    )
    return (*inner.values(), chain)


def _lookups(lookup: Lookups, name: str, /) -> tuple[ast.LookupBlock, ...]:
    """Returns the lookup blocks of one record, the last the one its feature references."""
    match lookup:
        case Single(mapping=mapping):
            return (_block(name, [ast.SingleSubstStatement([ast.GlyphName(glyph)], [ast.GlyphName(target)], [], [], forceChain=False) for glyph, target in mapping.items()]),)
        case Ligatures(rules=rules):
            return (_block(name, [ast.LigatureSubstStatement([], [ast.GlyphName(part) for part in parts], [], ast.GlyphName(target), forceChain=False) for parts, target in rules]),)
        case Contextual(rules=rules) | Lifts(rules=rules):
            return _chained(name, rules)
        case Positions(values=values):
            return (_block(name, [ast.SinglePosStatement([(ast.GlyphName(glyph), _value(adjustment))], [], [], forceChain=False) for glyph, adjustment in values.items()]),)
        case Pairs(values=values):
            return (_block(name, [ast.PairPosStatement(ast.GlyphName(first), _value(adjustment), ast.GlyphName(second), None) for (first, second), adjustment in values.items()]),)


# --- [KERNING]


def _kerned(source: Source, /) -> tuple[frozendict[str, tuple[str, ...]], frozendict[tuple[str, str], int]]:
    """Splits `kern` pairs into first-glyph classes sharing one value row and second-glyph classes sharing one column."""
    values = {pair: adjustment.advance for lookup in source.lookups if isinstance(lookup, Pairs) and lookup.feature == "kern" for pair, adjustment in lookup.values.items()}
    rows = {first: tuple(sorted((second, value) for (head, second), value in values.items() if head == first)) for first, _ in values}
    firsts = {members[0]: members for members in (tuple(first for first, _ in group) for _, group in groupby(sorted(rows.items(), key=lambda item: item[1]), key=lambda item: item[1]))}
    owner = {member: f"public.kern1.{head}" for head, members in firsts.items() for member in members}
    columns = {second: tuple(sorted({(owner[first], value) for (first, held), value in values.items() if held == second})) for _, second in values}
    seconds = {members[0]: members for members in (tuple(second for second, _ in group) for _, group in groupby(sorted(columns.items(), key=lambda item: item[1]), key=lambda item: item[1]))}
    return (
        frozendict({**{f"public.kern1.{head}": members for head, members in firsts.items()}, **{f"public.kern2.{head}": members for head, members in seconds.items()}}),
        frozendict({(group, f"public.kern2.{head}"): value for head, members in seconds.items() for group, value in columns[members[0]]}),
    )


# --- [EMISSION]


def emitted(source: Source, /) -> Emitted:
    """Writes every lookup record once as feature syntax: language systems of the glyphs' scripts and the lookups' languages, lookups in `order`, lifts in `mark` ahead of the writers' marker, `kern` pairs as groups."""
    fea, names = ast.FeatureFile(), count()
    tags = sorted({tag for glyph in source.glyphs for code in glyph.name.codes if (held := script(chr(code))) not in {"Zyyy", "Zinh", "Zzzz"} for tag in ot_tags_from_script(held)})
    systems = dict.fromkeys(("DFLT/dflt", *(f"{tag}/dflt" for tag in tags), *sorted({*lookup.languages for lookup in source.lookups})))
    fea.statements.extend(ast.LanguageSystemStatement(*system.split("/")) for system in systems)
    ordered = sorted((lookup for lookup in source.lookups if not (isinstance(lookup, Pairs) and lookup.feature == "kern")), key=lambda lookup: lookup.order)
    blocks = tuple((lookup, _lookups(lookup, f"{lookup.feature}{next(names)}")) for lookup in ordered)
    fea.statements.extend(*held for _, held in blocks)
    for feature in dict.fromkeys(lookup.feature for lookup, _ in blocks):
        block = ast.FeatureBlock(feature)
        block.statements.extend(
            statement
            for lookup, held in sorted((pair for pair in blocks if pair[0].feature == feature), key=lambda pair: pair[0].languages)
            for system in lookup.languages or ("",)
            for statement in (*((ast.ScriptStatement(system[:4]), ast.LanguageStatement(system[5:])) if system else ()), ast.LookupReferenceStatement(held[-1]))
        )
        block.statements.extend((ast.Comment("# Automatic Code"),) if feature == "mark" else ())
        fea.statements.append(block)
    groups, kerning = _kerned(source)
    return Emitted(features=fea.asFea(), groups=groups, kerning=kerning)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Emitted", "emitted"]

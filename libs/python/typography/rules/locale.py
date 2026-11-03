"""Locale and cmap: every required and kept-block code point composed from the face's own glyphs, blank format controls, derived signs, language `locl` forms, and required ligatures under `rlig`."""

lazy import math
lazy import unicodedata

import msgspec
lazy from fontTools.misc.transform import Offset, Transform
lazy from fontTools.unicodedata import Blocks
lazy from youseedee import ucd_data

from typography.faces.coverage import Coverage
from typography.rules.scripts import Derived, Rule, Script
lazy from typography.faces.glyphs import Component, Glyph, Joining, Mark, Outline, role, Source
lazy from typography.faces.layout import Ligatures, Lookups, Single
lazy from typography.faces.names import Form, GlyphName
lazy from typography.outlines.paths import area, bounds, Box, pieces, transformed
lazy from typography.rules.measures import dotted, key, nuqta

# --- [MODELS] ---------------------------------------------------------------------------


class Locale(Rule, frozen=True, kw_only=True):
    """Code points the coverage requires or keeps by block composed from the script's own glyphs, its derived signs, language forms, and required ligatures."""

    script: Script
    coverage: Coverage


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [GLYPHS]


def _new(name: GlyphName, advance: float, /, *, contours: Outline = (), components: tuple[Component, ...] = ()) -> Glyph:
    return Glyph(name=name, role=role(name), advance=advance, contours=contours, components=components)


def _forms(code: int, /) -> tuple[Form, ...]:
    """Forms a letter's joining type takes."""
    match Joining.of(code):
        case Joining.DUAL:
            return tuple(Form)
        case Joining.RIGHT:
            return (Form.ISOL, Form.FINA)
        case _:
            return (Form.ISOL,)


def _resolved(source: Source, code: int, form: Form, /) -> tuple[str, ...]:
    """Keys of the glyphs drawing a code point in a form: its own glyph, else the parts its canonical, positional, or no-break decomposition names resolved alike, empty where a part has none."""
    tag, *rest = unicodedata.decomposition(chr(code)).split() or ("",)
    same = not tag.startswith("<") or tag[1:5] in Form or tag == "<noBreak>"
    parts = tuple(int(part, 16) for part in (rest if tag.startswith("<") else (tag, *rest)) if part and same)
    shape = Form(tag[1:5]) if tag[1:5] in Form else form
    held = tuple(_resolved(source, part, shape if not index else Form.ISOL) for index, part in enumerate(parts))
    own = key(code, form if form in _forms(code) else Form.ISOL)
    return (own,) if own in source.named else tuple(*keys for keys in held) if parts and all(held) else ()


def _composed(source: Source, code: int, form: Form, /) -> tuple[Glyph, ...]:
    """A code point in a form as its one resolved glyph or a base and mark, the mark seated later at the base's anchor."""
    name = GlyphName(codes=(code,), form=form)
    match tuple(source.named[part] for part in _resolved(source, code, form)) if name.text not in source.named else ():
        case (Glyph() as base,) | (Glyph() as base, Glyph(role=Mark())):
            return (_new(name, base.advance, components=tuple(Component(name=part) for part in _resolved(source, code, form))),)
        case _:
            return ()


def _bodies(source: Source, codes: frozenset[int], area_: float, /) -> tuple[Glyph, ...]:
    """Forms of letters the face lacks, each the dotless body of the same form of a letter in its joining group."""
    groups = {code: group for code in source.encoded if (group := ucd_data(code).get("Joining_Group", "No_Joining_Group")) != "No_Joining_Group"}
    return tuple(
        _new(GlyphName(codes=(code,), form=form), model.advance, contours=dotted(model, area_)[0])
        for code in sorted(codes)
        if not unicodedata.decomposition(chr(code))
        for form in _forms(code)
        if key(code, form) not in source.named
        and (
            model := next(
                (held for other, group in groups.items() if other != code and group == ucd_data(code).get("Joining_Group") and (held := source.named.get(key(other, form))) and held.contours), None
            )
        )
    )


def _lowest(contours: Outline, /) -> Outline:
    """Returns the lower of the two smallest pieces of a drawing of three or more, empty for fewer."""
    held = sorted(pieces(contours), key=area)[:2] if len(pieces(contours)) > 2 else []
    return min(held, key=lambda piece: extent.ymin if (extent := bounds(piece)) else 0.0, default=())


def _derived(source: Source, rule: Derived, /) -> Glyph | None:
    """Draws a glyph from its model: its lowest small piece around a circle a share of the reference height across, or scaled to that share, turned with its foot at its own, the baseline, or the reference middle, and that piece repeated after it."""
    model, reference = source.named.get(rule.model), bounds(source.named[key(rule.reference)].contours) if rule.reference is not None and key(rule.reference) in source.named else None
    name = GlyphName(codes=(rule.code,), variant=rule.variant)
    match model, bounds(model.contours) if model else None:
        case (Glyph(contours=contours), Box()) if rule.around and reference and rule.height is not None and (dot := bounds(piece := _lowest(contours))):
            radius, step = rule.height * reference.height / 2, 1.25 * dot.width
            turns = (2 * math.pi * index / rule.around for index in range(rule.around))
            return _new(
                name,
                2 * (radius + step),
                contours=tuple(*transformed(piece, Offset(radius + step + radius * math.cos(turn) - dot.middle[0], reference.middle[1] + radius * math.sin(turn) - dot.middle[1])) for turn in turns),
            )
        case (Glyph(advance=advance, contours=contours), Box() as box) if not rule.around and (reference or (rule.height is None and rule.foot != "middle")):
            factor = rule.height * reference.height / box.height if rule.height is not None and reference else 1.0
            scaled = transformed(contours, (factor, 0.0, 0.0, factor, 0.0, 0.0))
            ring = _lowest(scaled) if rule.repeats else ()
            step = 1.25 * extent.width if (extent := bounds(ring)) else 0.0
            foot, (x, y) = {"own": factor * box.ymin, "base": 0.0, "middle": reference.middle[1] if reference else 0.0}[rule.foot], (extent := bounds(scaled) or box).middle
            drawn = transformed(scaled, Transform().translate(0, foot - extent.ymin).translate(x, y).rotate(math.pi).translate(-x, -y)) if rule.turned else scaled
            return _new(
                name,
                advance * factor + rule.repeats * step,
                contours=(*transformed(drawn, Offset(rule.repeats * step, 0)), *(*transformed(ring, Offset(index * step, 0)) for index in range(rule.repeats))),
            )
        case _:
            return None


# --- [LOOKUPS]


def _relaid(source: Source, added: tuple[Glyph, ...], script: Script, /) -> tuple[Lookups, ...]:
    """Adds the joined forms of new glyphs to the form lookups, moves the required ligatures to `rlig`, and writes the language's `locl` forms and variants in place of any held."""
    forms = {form.value: {key(glyph.name.codes[0]): glyph.name.text for glyph in added if len(glyph.name.codes) == 1 and glyph.name.form is form} for form in (Form.INIT, Form.MEDI, Form.FINA)}
    named, tag = {*source.named, *(glyph.name.text for glyph in added)}, script.language.split("/")[1].strip().lower()
    ligatures = tuple(lookup for lookup in source.lookups if isinstance(lookup, Ligatures))
    moved = frozenset(rule for lookup in ligatures for rule in lookup.rules if (codes := GlyphName.parse(rule[1]).codes)[:1] and codes[0] in script.rlig and set(codes[1:2]) <= script.rlig[codes[0]])
    local = {name: target for default, other in script.locl.items() for form in Form if (name := key(default, form)) in named and (target := key(other, form)) in named}
    local |= {glyph.name.text.removesuffix(f".{tag}"): glyph.name.text for glyph in added if glyph.name.variant == tag}
    return (
        *((Single(feature="locl", languages=(script.language,), order=-1, mapping=frozendict(local)),) if local else ()),
        *(
            msgspec.structs.replace(lookup, mapping=lookup.mapping | forms.get(lookup.feature, {}))
            if isinstance(lookup, Single)
            else msgspec.structs.replace(lookup, rules=tuple(rule for rule in lookup.rules if rule not in moved))
            if isinstance(lookup, Ligatures)
            else lookup
            for lookup in source.lookups
            if lookup.feature != "locl" and not (isinstance(lookup, Ligatures) and set(lookup.rules) <= moved)
        ),
        *((Ligatures(feature="rlig", order=min(lookup.order for lookup in ligatures if set(lookup.rules) & moved), rules=tuple(sorted(moved))),) if moved else ()),
    )


def localized(source: Source, rule: Locale, /) -> Source:
    """Draws the derived signs, composes every required and kept-block code point the face lacks from its own glyphs, blanks the format controls at zero advance, and lays out their lookups."""
    script, coverage = rule.script, rule.coverage
    derived = tuple(glyph for drawn in script.derived if (glyph := _derived(source, drawn)) and not (drawn.kept and (held := source.named.get(glyph.name.text)) and held.contours))
    drawn = source.replaced(*derived)
    kept = (code for start, end, block in zip(Blocks.RANGES, (*Blocks.RANGES[1:], 0x110000), Blocks.VALUES, strict=True) if block in coverage.blocks for code in range(start, end))
    wanted = tuple(code for code in sorted({*coverage.required, *kept} - drawn.encoded.keys()) if (code in script.spaces or unicodedata.category(chr(code)) not in {"Cn", "Zs"}) and coverage.exclusion(code).is_none())
    bases = {int(parts[0], 16) for code in wanted if (parts := unicodedata.decomposition(chr(code)).split()) and not parts[0].startswith("<")}
    based = drawn.replaced(*_bodies(drawn, frozenset({*drawn.encoded, *bases}), area(nuqta(drawn, script.dotted))))
    composed = tuple(glyph for code in wanted for form in _forms(code) for glyph in _composed(based, code, form))
    blanks = frozenset(glyph.name.codes for glyph in composed)
    controls = tuple(_new(GlyphName(codes=(code,)), 0.0) for code in wanted if (code,) not in blanks and unicodedata.category(chr(code)) == "Cf" and unicodedata.bidirectional(chr(code)) != "AN")
    added = (*(glyph for glyph in based.glyphs if glyph.name.text not in source.named), *composed, *controls)
    return msgspec.structs.replace(based.replaced(*composed, *controls), lookups=_relaid(source, added, script))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Locale", "localized"]

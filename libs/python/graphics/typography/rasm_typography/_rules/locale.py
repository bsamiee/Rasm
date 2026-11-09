"""Locale and cmap: every required and kept-block code point composed from the face's own glyphs, blank format controls, derived signs, language `locl` forms, and required ligatures under `rlig`."""

# mypy: disable-error-code="no-untyped-call"

lazy from collections import defaultdict
lazy import unicodedata

import msgspec
lazy from fontTools.misc.transform import Offset, Transform
lazy from fontTools.unicodedata import Blocks
lazy from scipy.special import cosdg, sindg

from rasm_typography._faces.coverage import Coverage
from rasm_typography._faces.declaration import Derived, Foot, Script
from rasm_typography._rules.measures import _dotted, _made, _nuqta, Rule
lazy from rasm_typography._faces.glyphs import _Component, _Glyph, _joined, _Joining, _Outline, Mark, Source
lazy from rasm_typography._faces.layout import Ligatures, Lookups, Single
lazy from rasm_typography._faces.names import _Form, _GlyphName
lazy from rasm_typography._outlines.paths import _area, _bounds, _Box, _pieces, _transformed

# --- [MODELS] ---------------------------------------------------------------------------


class Locale(Rule, frozen=True, kw_only=True):
    """Code points the coverage requires or keeps by block composed from the script's own glyphs, its derived signs, language forms, and required ligatures."""

    script: Script
    coverage: Coverage


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GLYPHS]


def _forms(code: int, /) -> tuple[_Form, ...]:
    """Forms a letter's joining type takes, each side a form joins one the type joins on."""
    kind = _Joining.of(code)
    return tuple(form for form in _Form if (not form.right or kind in {_Joining.DUAL, _Joining.RIGHT}) and (not form.left or kind in {_Joining.DUAL, _Joining.LEFT}))


def _resolved(source: Source, code: int, form: _Form, /) -> tuple[str, ...]:
    """Keys of the glyphs drawing a code point in a form: its own glyph, else the parts its canonical, positional, or no-break decomposition names resolved alike, empty where a part has none."""
    if (own := source.key(code, form if form in _forms(code) and form is not _Form.ISOL else None)) in source.named:
        return (own,)
    tag, *rest = unicodedata.decomposition(chr(code)).split() or ("",)
    same = not tag.startswith("<") or tag[1:5] in _Form or tag == "<noBreak>"
    parts = tuple(int(part, 16) for part in (rest if tag.startswith("<") else (tag, *rest)) if part and same)
    shape = _Form(tag[1:5]) if tag[1:5] in _Form else form
    held = tuple(_resolved(source, part, shape if not index else _Form.ISOL) for index, part in enumerate(parts))
    return tuple(*keys for keys in held) if parts and all(held) else ()


def _composed(source: Source, code: int, form: _Form, /) -> tuple[_Glyph, ...]:
    """Encodes an existing isolated glyph or composes missing geometry from resolved parts, retaining native glyph identity."""
    name = _GlyphName(stem=(code,), form=form)
    if (glyph := source.named.get(name.text)) is not None:
        return (msgspec.structs.replace(glyph, codes=glyph.codes | {code}),) if form is _Form.ISOL else ()
    parts = _resolved(source, code, form)
    match tuple(source.named[part] for part in parts):
        case (_Glyph() as base,) | (_Glyph() as base, _Glyph(role=Mark())):
            return (_made(name, base.advance, components=tuple(_Component(name=part) for part in parts)),)
        case _:
            return ()


def _bodies(source: Source, codes: frozenset[int], area_: float, /) -> tuple[_Glyph, ...]:
    """Forms of letters the face lacks, each the dotless body of the same form of a letter in its joining group."""
    groups = {code: group for code in source.encoded if (group := _joined().get(code, (_Joining.NONE, "No_Joining_Group"))[1]) != "No_Joining_Group"}
    return tuple(
        _made(_GlyphName(stem=(code,), form=form), model.advance, contours=_dotted(model, area_)[0])
        for code in sorted(codes)
        if not unicodedata.decomposition(chr(code))
        for form in _forms(code)
        if source.key(code, form) not in source.named
        and (
            model := next(
                (
                    held
                    for other, group in groups.items()
                    if other != code and group == _joined().get(code, (_Joining.NONE, "No_Joining_Group"))[1] and (held := source.named.get(source.key(other, form))) and held.contours
                ),
                None,
            )
        )
    )


def _lowest(contours: _Outline, /) -> _Outline:
    """Returns the lower of the two smallest pieces of a drawing of three or more, empty for fewer."""
    held = _pieces(contours)
    return min(sorted(held, key=_area)[:2], key=lambda piece: extent.ymin if (extent := _bounds(piece)) else 0.0) if len(held) > 2 else ()


def _derived(source: Source, rule: Derived, /) -> _Glyph | None:
    """Draws a glyph from its model: its lowest small piece around a circle a share of the reference height across, or scaled to that share, turned with its foot at its own, the baseline, or the reference middle, and that piece repeated after it."""
    model = source.glyph(source.key(rule.model) if isinstance(rule.model, int) else rule.model).map(lambda glyph: source.drawn(glyph.name.text)).default_value(None)
    reference = _Box.of(rect) if rule.reference is not None and (rect := source.bounds(source.key(rule.reference))) is not None else None
    name = _GlyphName(stem=(rule.code,), suffixes=() if rule.variant is None else (rule.variant,))
    match model, _bounds(model.contours) if model else None:
        case (_Glyph(contours=contours), _Box()) if rule.around and reference and rule.height is not None and (dot := _bounds(piece := _lowest(contours))):
            radius, step = rule.height * reference.height / 2, 1.25 * dot.width
            turns = (360 * index / rule.around for index in range(rule.around))
            return _made(
                name,
                2 * (radius + step),
                contours=tuple(*_transformed(piece, Offset(radius + step + radius * cosdg(turn) - dot.middle[0], reference.middle[1] + radius * sindg(turn) - dot.middle[1])) for turn in turns),  # ty: ignore[no-matching-overload]
            )
        case (_Glyph(advance=advance, contours=contours), _Box() as box) if not rule.around and (reference or (rule.height is None and rule.foot is not Foot.MIDDLE)):
            factor = rule.height * reference.height / box.height if rule.height is not None and reference else 1.0
            scaled = _transformed(contours, (factor, 0.0, 0.0, factor, 0.0, 0.0))
            ring = _lowest(scaled) if rule.repeats else ()
            step = 1.25 * extent.width if (extent := _bounds(ring)) else 0.0
            foot, (x, y) = {Foot.OWN: factor * box.ymin, Foot.BASE: 0.0, Foot.MIDDLE: reference.middle[1] if reference else 0.0}[rule.foot], (extent := _bounds(scaled) or box).middle
            drawn = _transformed(scaled, Transform().translate(0, foot - extent.ymin).translate(x, y).scale(-1, -1).translate(-x, -y)) if rule.turned else scaled
            return _made(
                name,
                advance * factor + rule.repeats * step,
                contours=(*_transformed(drawn, Offset(rule.repeats * step, 0)), *(*_transformed(ring, Offset(index * step, 0)) for index in range(rule.repeats))),
            )
        case _:
            return None


# --- [LOOKUPS]


def _relaid(source: Source, added: tuple[_Glyph, ...], script: Script, /) -> tuple[Lookups, ...]:
    """Adds the joined forms of new glyphs to the form lookups, moves the required ligatures to `rlig`, and writes the language's `locl` forms and variants in place of any held."""
    forms = {
        form.value: {source.key(glyph.name.codes[0]): glyph.name.text for glyph in added if len(glyph.name.codes) == 1 and glyph.name.form is form} for form in (_Form.INIT, _Form.MEDI, _Form.FINA)
    }
    named, tag = {*source.named, *(glyph.name.text for glyph in added)}, script.language[1].strip().lower()
    local = {name: target for default, other in script.locl.items() for form in _Form if (name := source.key(default, form)) in named and (target := source.key(other, form)) in named}
    local |= {glyph.name.text.removesuffix(f".{tag}"): glyph.name.text for glyph in added if glyph.name.suffixes == (tag,)}
    relaid: list[Lookups] = []
    for lookup in source.lookups:
        if not (features := frozendict({tag: systems for tag, systems in lookup.features.items() if tag != "locl"})):
            continue
        match lookup:
            case Single():
                mappings: defaultdict[frozendict[str, str], dict[str, frozendict[tuple[str, str], bool]]] = defaultdict(dict)
                for feature, systems in features.items():
                    mappings[lookup.mapping | forms.get(feature, {})][feature] = systems
                relaid.extend(msgspec.structs.replace(lookup, features=frozendict(features), mapping=mapping) for mapping, features in mappings.items())
            case Ligatures():
                systems = (
                    frozendict({system: any(scope.get(system, False) for scope in features.values()) for system in {*scope for scope in features.values()}}) if all(features.values()) else frozendict()
                )
                ligatures: defaultdict[bool, list[tuple[tuple[str, ...], str]]] = defaultdict(list)
                for parts, target in lookup.rules:
                    codes = source.named[target].name.codes
                    required = "rlig" not in features and bool(codes) and codes[0] in script.rlig and set(codes[1:2]) <= script.rlig[codes[0]]
                    ligatures[required].append((parts, target))
                relaid.extend(msgspec.structs.replace(lookup, features=frozendict({"rlig": systems}) if required else features, rules=tuple(rules)) for required, rules in ligatures.items())
            case _:
                relaid.append(msgspec.structs.replace(lookup, features=features))
    return (*((Single(features=frozendict({"locl": frozendict({script.language: False})}), order=-1, mapping=frozendict(local)),) if local else ()), *relaid)


def _localized(source: Source, rule: Locale, /) -> Source:
    """Draws the derived signs, composes every required and kept-block code point the face lacks from its own glyphs, blanks the format controls at zero advance, and lays out their lookups."""
    script, coverage = rule.script, rule.coverage
    derived = tuple(glyph for drawn in script.derived if (glyph := _derived(source, drawn)) and not (drawn.kept and (held := source.named.get(glyph.name.text)) and held.contours))
    drawn = source.replaced(*derived)
    kept = (code for start, end, block in zip(Blocks.RANGES, (*Blocks.RANGES[1:], 0x110000), Blocks.VALUES, strict=True) if block in coverage.blocks for code in range(start, end))
    wanted = tuple(
        code
        for code in sorted({*coverage.required, *kept} - drawn.encoded.keys())
        if (code in script.spaces or unicodedata.category(chr(code)) not in {"Cn", "Zs"}) and coverage.exclusion(code).is_none()
    )
    bases = {int(parts[0], 16) for code in wanted if (parts := unicodedata.decomposition(chr(code)).split()) and not parts[0].startswith("<")}
    based = drawn.replaced(*_bodies(drawn, frozenset({*drawn.encoded, *bases}), _area(_nuqta(drawn, script.dotted))))
    composed = tuple(glyph for code in wanted for form in _forms(code) for glyph in _composed(based, code, form))
    blanks = frozenset(glyph.name.codes for glyph in composed)
    controls = tuple(_made(_GlyphName(stem=(code,)), 0.0) for code in wanted if (code,) not in blanks and unicodedata.category(chr(code)) == "Cf" and unicodedata.bidirectional(chr(code)) != "AN")
    added = (*(glyph for glyph in based.glyphs if glyph.name.text not in source.named), *composed, *controls)
    return msgspec.structs.replace(based.replaced(*composed, *controls), lookups=_relaid(source, added, script))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Locale"]

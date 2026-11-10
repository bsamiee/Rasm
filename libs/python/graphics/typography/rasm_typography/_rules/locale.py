"""Locale and cmap: every required and kept-block code point composed from the face's own glyphs, blank format controls, derived signs, language `locl` forms, and required ligatures under `rlig`."""

lazy from collections import defaultdict
lazy import unicodedata

import msgspec
lazy from fontTools.misc.transform import Offset, Transform
lazy from fontTools.unicodedata import Blocks
lazy import numpy as np
lazy from scipy.special import cosdg, sindg

from rasm_typography._faces.coverage import Coverage
from rasm_typography._faces.declaration import Foot, Script
from rasm_typography._rules import measures
lazy from rasm_records import Ok, Outcome
lazy from rasm_typography._faces.glyphs import Component, Glyph, joined, Joining, Mark, Point, Source
lazy from rasm_typography._faces.layout import Ligatures, Lookups, Single
lazy from rasm_typography._faces.names import Form, GlyphName
lazy from rasm_typography._outlines import paths

# --- [MODELS] ---------------------------------------------------------------------------


class Locale(measures.Rule, frozen=True, kw_only=True):
    """Code points the coverage requires or keeps by block composed from the script's own glyphs, its derived signs, language forms, and required ligatures."""

    script: Script
    coverage: Coverage

    def applied(self, source: Source, /, **_: object) -> Outcome[Source]:
        """Draws the derived signs, composes every required and kept-block code point the face lacks from its own glyphs, blanks the format controls at zero advance, and lays out their lookups."""
        script, coverage = self.script, self.coverage
        derived: list[Glyph] = []
        for sign in script.derived:
            model = source.glyph(source.key(sign.model) if isinstance(sign.model, int) else sign.model).map(lambda glyph: source.drawn(glyph.name.text)).default_value(None)
            reference = paths.Box.of(rect) if sign.reference is not None and (rect := source.bounds(source.key(sign.reference))) is not None else None
            name = GlyphName(stem=(sign.code,), suffixes=() if sign.variant is None else (sign.variant,))
            match model, paths.bounds(model.contours) if model else None:
                case (Glyph(contours=contours), paths.Box()) if sign.around and reference and sign.height is not None and (dot := paths.bounds(piece := _lowest(contours))):
                    radius, step = sign.height * reference.height / 2, 1.25 * dot.width
                    turns = (np.float64(360 * index / sign.around) for index in range(sign.around))
                    glyph = measures.made(
                        name,
                        2 * (radius + step),
                        contours=tuple(
                            *paths.transformed(piece, Offset(radius + step + radius * cosdg(turn) - dot.middle[0], reference.middle[1] + radius * sindg(turn) - dot.middle[1])) for turn in turns
                        ),
                    )
                case (Glyph(advance=advance, contours=contours), paths.Box() as box) if not sign.around and (reference or (sign.height is None and sign.foot is not Foot.MIDDLE)):
                    factor = sign.height * reference.height / box.height if sign.height is not None and reference else 1.0
                    scaled = paths.transformed(contours, (factor, 0.0, 0.0, factor, 0.0, 0.0))
                    ring = _lowest(scaled) if sign.repeats else ()
                    step = 1.25 * extent.width if (extent := paths.bounds(ring)) else 0.0
                    foot, (x, y) = {Foot.OWN: factor * box.ymin, Foot.BASE: 0.0, Foot.MIDDLE: reference.middle[1] if reference else 0.0}[sign.foot], (extent := paths.bounds(scaled) or box).middle
                    turned = paths.transformed(scaled, Transform().translate(0, foot - extent.ymin).translate(x, y).scale(-1, -1).translate(-x, -y)) if sign.turned else scaled
                    glyph = measures.made(
                        name,
                        advance * factor + sign.repeats * step,
                        contours=(*paths.transformed(turned, Offset(sign.repeats * step, 0)), *(*paths.transformed(ring, Offset(index * step, 0)) for index in range(sign.repeats))),
                    )
                case _:
                    continue
            if not (sign.kept and (held := source.named.get(glyph.name.text)) and held.contours):
                derived.append(glyph)
        drawn = source.replaced(*derived)
        kept = (code for start, end, block in zip(Blocks.RANGES, (*Blocks.RANGES[1:], 0x110000), Blocks.VALUES, strict=True) if block in coverage.blocks for code in range(start, end))
        wanted = tuple(
            code
            for code in sorted({*coverage.required, *kept} - drawn.encoded.keys())
            if (code in script.spaces or unicodedata.category(chr(code)) not in {"Cn", "Zs"}) and coverage.exclusion(code).is_none()
        )
        bases = {int(parts[0], 16) for code in wanted if (parts := unicodedata.decomposition(chr(code)).split()) and not parts[0].startswith("<")}
        groups = {code: group for code in drawn.encoded if (group := joined().get(code, (Joining.NONE, "No_Joining_Group"))[1]) != "No_Joining_Group"}
        nuqta = paths.area(measures.nuqta(drawn, script.dotted))
        based = drawn.replaced(
            *(
                measures.made(GlyphName(stem=(code,), form=form), model.advance, contours=measures.dotted(model, nuqta)[0])
                for code in sorted({*drawn.encoded, *bases})
                if not unicodedata.decomposition(chr(code))
                for form in _forms(code)
                if drawn.key(code, form) not in drawn.named
                and (
                    model := next(
                        (
                            held
                            for other, group in groups.items()
                            if other != code and group == joined().get(code, (Joining.NONE, "No_Joining_Group"))[1] and (held := drawn.named.get(drawn.key(other, form))) and held.contours
                        ),
                        None,
                    )
                )
            )
        )
        composed = tuple(
            glyph
            for code in wanted
            for form in _forms(code)
            for name in (GlyphName(stem=(code,), form=form),)
            for held in (based.named.get(name.text),)
            for keys in (() if held is not None else _resolved(based, code, form),)
            for parts in (tuple(based.named[key] for key in keys),)
            for glyph in (
                ((msgspec.structs.replace(held, codes=held.codes | {code}),) if form is Form.ISOL else ())
                if held is not None
                else (measures.made(name, parts[0].advance, components=tuple(Component(name=key) for key in keys)),)
                if len(parts) == 1 or (len(parts) == 2 and isinstance(parts[1].role, Mark))
                else ()
            )
        )
        blanks = frozenset(glyph.name.codes for glyph in composed)
        controls = tuple(
            measures.made(GlyphName(stem=(code,)), 0.0) for code in wanted if (code,) not in blanks and unicodedata.category(chr(code)) == "Cf" and unicodedata.bidirectional(chr(code)) != "AN"
        )
        added = (*(glyph for glyph in based.glyphs if glyph.name.text not in source.named), *composed, *controls)
        forms = {
            form.value: {source.key(glyph.name.codes[0]): glyph.name.text for glyph in added if len(glyph.name.codes) == 1 and glyph.name.form is form} for form in (Form.INIT, Form.MEDI, Form.FINA)
        }
        named, tag = {*source.named, *(glyph.name.text for glyph in added)}, script.language[1].strip().lower()
        local = {key: target for default, other in script.locl.items() for form in Form if (key := source.key(default, form)) in named and (target := source.key(other, form)) in named}
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
                        frozendict({system: any(scope.get(system, False) for scope in features.values()) for system in {*scope for scope in features.values()}})
                        if all(features.values())
                        else frozendict()
                    )
                    ligatures: defaultdict[bool, list[tuple[tuple[str, ...], str]]] = defaultdict(list)
                    for parts, target in lookup.rules:
                        codes = source.named[target].name.codes
                        required = "rlig" not in features and bool(codes) and codes[0] in script.rlig and set(codes[1:2]) <= script.rlig[codes[0]]
                        ligatures[required].append((parts, target))
                    relaid.extend(msgspec.structs.replace(lookup, features=frozendict({"rlig": systems}) if required else features, rules=tuple(rules)) for required, rules in ligatures.items())
                case _:
                    relaid.append(msgspec.structs.replace(lookup, features=features))
        return Ok(
            msgspec.structs.replace(
                based.replaced(*composed, *controls),
                lookups=(*((Single(features=frozendict({"locl": frozendict({script.language: False})}), order=-1, mapping=frozendict(local)),) if local else ()), *relaid),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _forms(code: int, /) -> tuple[Form, ...]:
    """Forms a letter's joining type takes, each side a form joins one the type joins on."""
    kind = Joining.of(code)
    return tuple(form for form in Form if (not form.right or kind in {Joining.DUAL, Joining.RIGHT}) and (not form.left or kind in {Joining.DUAL, Joining.LEFT}))


def _resolved(source: Source, code: int, form: Form, /) -> tuple[str, ...]:
    """Keys of the glyphs drawing a code point in a form: its own glyph, else the parts its canonical, positional, or no-break decomposition names resolved alike, empty where a part has none."""
    if (own := source.key(code, form if form in _forms(code) and form is not Form.ISOL else None)) in source.named:
        return (own,)
    tag, *rest = unicodedata.decomposition(chr(code)).split() or ("",)
    same = not tag.startswith("<") or tag[1:5] in Form or tag == "<noBreak>"
    parts = tuple(int(part, 16) for part in (rest if tag.startswith("<") else (tag, *rest)) if part and same)
    shape = Form(tag[1:5]) if tag[1:5] in Form else form
    held = tuple(_resolved(source, part, shape if not index else Form.ISOL) for index, part in enumerate(parts))
    return tuple(*keys for keys in held) if parts and all(held) else ()


def _lowest(contours: tuple[tuple[Point, ...], ...], /) -> tuple[tuple[Point, ...], ...]:
    """Returns the lower of the two smallest pieces of a drawing of three or more, empty for fewer."""
    held = paths.pieces(contours)
    return min(sorted(held, key=paths.area)[:2], key=lambda piece: extent.ymin if (extent := paths.bounds(piece)) else 0.0) if len(held) > 2 else ()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Locale"]

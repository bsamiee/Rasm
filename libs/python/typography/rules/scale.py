"""Apparent size: the glyphs of the script scaled to the stem height within the marked span cap, every glyph moved onto the new units per em."""

import msgspec
lazy from fontTools.unicodedata import script_extension

from typography.faces.declaration import Em
from typography.rules.scripts import Rule, Script
lazy from typography.faces.faults import Outcome
lazy from typography.faces.glyphs import Source
lazy from typography.outlines.paths import bounds
lazy from typography.rules.measures import inked, mapped, placement, text_ink
lazy from typography.shaping.corpus import Corpus
lazy from typography.shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Scale(Rule, frozen=True, kw_only=True):
    """Apparent size: the script's glyphs held to the stem height, the factor capped so the marked corpus spans at most the cap, every glyph onto the new units per em."""

    script: Script
    height: Em | None
    cap: Em
    upm: int = 2048


# --- [OPERATIONS] -----------------------------------------------------------------------


def scaled(source: Source, rule: Scale, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
    """Scales the script's glyphs to the stem target, capped so the marked corpus spans at most the cap, and every glyph onto a new units per em.

    Returns:
        Ok(scaled source), `ABSENT` for a face without an inked stem.
    """
    span = max((box.height for line in corpus.marked if (box := bounds(text_ink(original, line)))), default=0.0)
    return inked(source, rule.script.stem).bind(
        lambda held: placement(source, original, rule.script.stem).map(
            lambda move: _onto(source, min(rule.height * source.upm / held[1].ymax if rule.height is not None else 1.0, rule.cap * source.upm / (move[0] * span) if span else 1.0), rule)
        )
    )


def _onto(source: Source, factor: float, rule: Scale, /) -> Source:
    """Moves every glyph onto the new units per em, the script's glyphs also by the factor."""
    metrics, unit = source.metrics, rule.upm / source.upm
    return msgspec.structs.replace(
        source,
        upm=rule.upm,
        glyphs=tuple(
            mapped(glyph, (scale, 0.0, 0.0, scale, 0.0, 0.0))
            for glyph in source.glyphs
            for scale in ((factor * unit if glyph.name.codes and all(rule.script.scripts & set(script_extension(chr(code))) for code in glyph.name.codes) else unit),)
        ),
        metrics=msgspec.structs.replace(
            metrics, ascender=round(metrics.ascender * unit), descender=round(metrics.descender * unit), win_ascent=round(metrics.win_ascent * unit), win_descent=round(metrics.win_descent * unit)
        )
        if metrics
        else None,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Scale", "scaled"]

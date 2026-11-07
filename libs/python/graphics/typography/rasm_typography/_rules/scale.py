"""Apparent size: script glyphs scaled to the stem height within the marked span cap, every glyph moved onto the new units per em."""

import msgspec
lazy from fontTools.misc import transform
lazy from fontTools.misc.roundTools import otRound
lazy from fontTools.unicodedata import script_extension

from rasm_typography._faces.declaration import Em, Script
from rasm_typography._rules.measures import _mapped, _placement, _text_outline, Rule
lazy from rasm_records import Outcome
lazy from rasm_typography._faces.glyphs import _Source
lazy from rasm_typography._outlines.paths import _bounds
lazy from rasm_typography._shaping.corpus import _Corpus
lazy from rasm_typography._shaping.runs import _Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Scale(Rule, frozen=True, kw_only=True):
    """Apparent size: script glyphs held to the stem height with the factor capped at a marked corpus span of `cap`, every glyph moved onto `upm`."""

    script: Script
    height: Em | None
    cap: Em
    upm: int = 2048


# --- [OPERATIONS] -----------------------------------------------------------------------


def _scaled(source: _Source, rule: Scale, /, *, original: _Binary, corpus: _Corpus) -> Outcome[_Source]:
    """Scales script glyphs to the stem height, the marked corpus spanning at most the cap, and moves every glyph onto the new units per em.

    Returns:
        Ok(scaled source), `ABSENT` for a face without a drawn stem.
    """
    span = max((box.height for line in corpus.marked if (box := _bounds(_text_outline(original, line)))), default=0.0)
    return _placement(source, original, rule.script.stem).map(
        lambda held: _onto(source, min(rule.height * source.upm / held[1].ymax if rule.height is not None else 1.0, rule.cap * source.upm / (held[2].xx * span) if span else 1.0), rule)
    )


def _onto(source: _Source, factor: float, rule: Scale, /) -> _Source:
    """Moves every glyph onto the new units per em and scales script glyphs by the factor."""
    metrics, unit = source.metrics, rule.upm / source.upm
    return msgspec.structs.replace(
        source,
        upm=rule.upm,
        glyphs=_mapped(
            source,
            {
                glyph.name.text: transform.Scale(factor * unit if glyph.name.codes and all(rule.script.scripts & set(script_extension(chr(code))) for code in glyph.name.codes) else unit)  # type: ignore[no-untyped-call]
                for glyph in source.glyphs
            },
        ).glyphs,
        metrics=msgspec.structs.replace(
            metrics,
            ascender=otRound(metrics.ascender * unit),
            descender=otRound(metrics.descender * unit),
            win_ascent=otRound(metrics.win_ascent * unit),
            win_descent=otRound(metrics.win_descent * unit),
            x_height=otRound(metrics.x_height * unit),
            cap_height=otRound(metrics.cap_height * unit),
            caret_offset=otRound(metrics.caret_offset * unit),
        ),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Scale"]

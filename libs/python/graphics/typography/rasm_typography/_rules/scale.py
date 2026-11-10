"""Apparent size: script glyphs scaled to the stem height within the marked span cap, every glyph moved onto the new units per em."""

import msgspec
lazy from fontTools.misc import transform
lazy from fontTools.misc.roundTools import otRound
lazy from fontTools.unicodedata import script_extension

from rasm_typography._faces.declaration import Em, Script
from rasm_typography._rules import measures
lazy from rasm_records import Outcome
lazy from rasm_typography._faces.glyphs import Source
lazy from rasm_typography._outlines import paths
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class Scale(measures.Rule, frozen=True, kw_only=True):
    """Apparent size: script glyphs held to the stem height with the factor capped at a marked corpus span of `cap`, every glyph moved onto `upm`."""

    script: Script
    height: Em | None
    cap: Em
    upm: int = 2048

    def applied(self, source: Source, /, *, original: Binary, corpus: Corpus) -> Outcome[Source]:
        """Scales script glyphs to the stem height, the marked corpus spanning at most the cap, and moves every glyph onto the new units per em.

        Returns:
            Ok(scaled source), `ABSENT` for a face without a drawn stem.
        """
        span = max((box.height for line in corpus.marked if (box := paths.bounds(measures.text_outline(original, line)))), default=0.0)
        metrics, unit = source.metrics, self.upm / source.upm
        return measures.placement(source, original, self.script.stem).map(
            lambda held: msgspec.structs.replace(
                source,
                upm=self.upm,
                glyphs=measures.mapped(
                    source,
                    {
                        glyph.name.text: transform.Scale(factor * unit if glyph.name.codes and all(self.script.scripts & set(script_extension(chr(code))) for code in glyph.name.codes) else unit)
                        for factor in (min(self.height * source.upm / held[1].ymax if self.height is not None else 1.0, self.cap * source.upm / (held[2].xx * span) if span else 1.0),)
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
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Scale"]

"""Approved shaping expectations: the JSON record fontspector and fontbakery shaping tests read, the expectations a font's corpus runs yield, and the runs a build no longer shapes alike."""

from functools import partial
from typing import Self

import msgspec
lazy from expression.extra.result import catch

lazy from rasm_records import Fault, Outcome, Reason
lazy from rasm_typography._shaping.corpus import Corpus
lazy from rasm_typography._shaping.runs import Binary

# --- [MODELS] ---------------------------------------------------------------------------


class _Test(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Input text, the language it shapes under, and the glyph list its shaping must equal."""

    input: str
    expectation: str
    language: str | None = None


class _Configuration(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Glyphs no shaped test may hold."""

    forbidden_glyphs: tuple[str, ...] = ()


class Expected(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Approved shaping of a font: one test per corpus line and language, and the glyphs no test may shape to."""

    configuration: _Configuration
    tests: tuple[_Test, ...]

    @classmethod
    def of(cls, binary: Binary, corpus: Corpus, /) -> Self:
        """Records the serialization of every corpus line under each corpus language, script and direction guessed per line.

        Args:
            binary: Font whose shaping the expectations approve.
            corpus: Lines and languages to shape, a `None` language shaping under the font's default system.

        Returns:
            Expectations forbidding the font's glyph 0, one test per line and language.
        """
        return cls(
            configuration=_Configuration(forbidden_glyphs=binary.order[:1]),
            tests=tuple(_Test(input=line, expectation=binary.shaped(line, language=language).serialized, language=language) for line in corpus.lines for language in corpus.languages),
        )

    @classmethod
    def decoded(cls, raw: bytes, /) -> Outcome[Self]:
        """Decodes shaping JSON.

        Returns:
            Ok(expectations), `INVALID` fault for malformed JSON or a field outside its record.
        """
        return catch(lambda: msgspec.json.decode(raw, type=cls), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, "shaping expectations"))

    @property
    def encoded(self) -> bytes:
        """Indented JSON with sorted mapping keys and a final newline, equal records giving equal bytes."""
        return msgspec.json.format(msgspec.json.encode(self, order="deterministic")) + b"\n"

    def regressed(self, binary: Binary, /) -> tuple[tuple[str, str, str], ...]:
        """Each test a font shapes apart from its expectation, and each forbidden glyph a test shapes to, as input, measured glyphs, and bound."""
        runs = tuple((test, binary.shaped(test.input, language=test.language)) for test in self.tests)
        return (
            *((test.input, run.serialized, test.expectation) for test, run in runs if run.serialized != test.expectation),
            *((test.input, glyph, "no forbidden glyph") for test, run in runs for glyph in run.names if glyph in self.configuration.forbidden_glyphs),
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Expected"]

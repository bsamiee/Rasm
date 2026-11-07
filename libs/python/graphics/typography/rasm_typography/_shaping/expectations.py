"""Fontspector shaping expectations: the JSON records, their codec, and the expectations a font's corpus runs yield."""

from functools import partial

import msgspec
lazy from expression.extra.result import catch

from rasm_typography._shaping.runs import _Binary, _Direction
lazy from rasm_records import Fault, Outcome, Reason
lazy from rasm_typography._shaping.corpus import _Corpus

# --- [MODELS] ---------------------------------------------------------------------------


class _Defaults(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Shaping options every test shares, an unset option left to the shaper's guess or default."""

    script: str = ""
    language: str = ""
    direction: _Direction | None = None
    features: frozendict[str, bool] = frozendict()
    shaper: str | None = None
    variations: frozendict[str, float] = frozendict()


class _Expectation(_Defaults, frozen=True, kw_only=True):
    """Input text, the glyph list its shaping must equal, and the options of this test beside the shared defaults, flattened as fontspector reads them."""

    input: str
    expectation: str
    note: str = ""
    only: tuple[str, ...] = ()
    exclude: tuple[str, ...] = ()


class _Configuration(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Options every test shares and glyphs no shaped run may hold."""

    defaults: _Defaults = _Defaults()
    forbidden_glyphs: tuple[str, ...] = ()


class _Expectations(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Shaping file fontspector's shaping checks read from its test directory."""

    configuration: _Configuration
    tests: tuple[_Expectation, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _encoded(expectations: _Expectations, /) -> bytes:
    """Encodes expectations as indented JSON with sorted mapping keys and a final newline, equal records giving equal bytes."""
    return msgspec.json.format(msgspec.json.encode(expectations, order="deterministic")) + b"\n"


def decoded(raw: bytes, /) -> Outcome[_Expectations]:
    """Decodes shaping JSON.

    Returns:
        Ok(expectations), `INVALID` fault for malformed JSON or a field outside its record.
    """
    return catch(lambda: msgspec.json.decode(raw, type=_Expectations), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, "shaping expectations"))


def _expected(binary: _Binary, corpus: _Corpus, file: str, /) -> _Expectations:
    """Records the serialization of every corpus line under each corpus language, script and direction guessed per line.

    Args:
        binary: Font whose shaping the expectations approve.
        corpus: Lines and languages to shape, an empty language shaping under the font's default system.
        file: Font file name every test selects through fontspector's `only`, beside other faces' tests in one test directory.

    Returns:
        Expectations forbidding the font's glyph 0, one test per line and language.
    """
    return _Expectations(
        configuration=_Configuration(forbidden_glyphs=binary.order[:1]),
        tests=tuple(_Expectation(input=line, expectation=binary.shaped(line, language=language).serialized, language=language, only=(file,)) for line in corpus.lines for language in corpus.languages),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["decoded"]

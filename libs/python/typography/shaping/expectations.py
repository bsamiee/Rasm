"""Fontspector shaping expectations: the JSON records, their codec, and the expectations a font's corpus runs yield."""

from functools import partial

import msgspec
lazy from expression.extra.result import catch

from typography.shaping.runs import Binary, Direction
lazy from typography.faces.faults import Fault, Outcome, Reason
lazy from typography.shaping.corpus import Corpus

# --- [MODELS] ---------------------------------------------------------------------------


class Defaults(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Shaping options every test shares, an unset option left to the shaper's guess or default."""

    script: str = ""
    language: str = ""
    direction: Direction | None = None
    features: frozendict[str, bool] = frozendict()
    shaper: str | None = None
    variations: frozendict[str, float] = frozendict()


class Expectation(Defaults, frozen=True, kw_only=True):
    """Input text, the glyph list its shaping must equal, and the options of this test beside the shared defaults, flattened as fontspector reads them."""

    input: str
    expectation: str
    note: str = ""
    only: tuple[str, ...] = ()
    exclude: tuple[str, ...] = ()


class Configuration(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Options every test shares and glyphs no shaped run may hold."""

    defaults: Defaults = Defaults()
    forbidden_glyphs: tuple[str, ...] = ()


class Expectations(msgspec.Struct, frozen=True, kw_only=True, omit_defaults=True):
    """Shaping file fontspector's shaping checks read from its test directory."""

    configuration: Configuration
    tests: tuple[Expectation, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def encoded(expectations: Expectations, /) -> bytes:
    """Encodes expectations as indented JSON with sorted mapping keys and a final newline, equal records giving equal bytes."""
    return msgspec.json.format(msgspec.json.encode(expectations, order="deterministic")) + b"\n"


def decoded(raw: bytes, /) -> Outcome[Expectations]:
    """Decodes shaping JSON.

    Returns:
        Ok(expectations), `INVALID` fault for malformed JSON or a field outside its record.
    """
    return catch(lambda: msgspec.json.decode(raw, type=Expectations), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.INVALID, "shaping expectations"))


def expected(binary: Binary, corpus: Corpus, file: str, /) -> Expectations:
    """Records the serialization of every corpus line under each corpus language, script and direction guessed per line.

    Args:
        binary: Font whose shaping the expectations approve.
        corpus: Lines and languages to shape, an empty language shaping under the font's default system.
        file: Font file name every test selects through fontspector's `only`, beside other faces' tests in one test directory.

    Returns:
        Expectations forbidding the font's glyph 0, one test per line and language.
    """
    return Expectations(
        configuration=Configuration(forbidden_glyphs=binary.order[:1]),
        tests=tuple(Expectation(input=line, expectation=binary.shaped(line, language=language).serialized, language=language, only=(file,)) for line in corpus.lines for language in corpus.languages),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Configuration", "Defaults", "Expectation", "Expectations", "decoded", "encoded", "expected"]

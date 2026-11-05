"""Native fontspector wire records and corpus shaping approvals."""

from expression import Result
import msgspec
import pytest

from typography.faces.faults import Fault, Reason
from typography.shaping.corpus import Corpus
from typography.shaping.expectations import Configuration, decoded, encoded, Expectation, Expectations, expected
from typography.shaping.runs import Binary

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_approvals_cover_each_language_and_distinct_input(binary: Binary, sample: Corpus) -> None:
    """Every corpus line and language receives its native serialized shaping result exactly once."""
    approval = expected(binary, sample, "Test-Regular.ttf")
    assert {(test.input, test.language) for test in approval.tests} == {(line, language) for line in sample.lines for language in sample.languages}
    assert len(approval.tests) == len(sample.lines) * len(sample.languages)
    assert {test.only for test in approval.tests} == {("Test-Regular.ttf",)}
    assert approval.configuration.forbidden_glyphs == binary.order[:1]
    assert approval.tests[0].expectation == "uni0628.fina=2+500|uni0628.init=0+500"
    assert all(test.expectation == binary.shaped(test.input, language=test.language).serialized for test in approval.tests)


def test_native_wire_keeps_required_empty_configuration_and_selectors() -> None:
    """Empty configuration remains present and fontspector's only/exclude selectors keep their wire names."""
    approval = Expectations(configuration=Configuration(), tests=(Expectation(input="text", expectation="glyph=0+100", only=("Test.ttf",), exclude=("Other.ttf",)),))
    raw = encoded(approval)
    assert msgspec.json.decode(raw) == {"configuration": {}, "tests": [{"input": "text", "expectation": "glyph=0+100", "only": ["Test.ttf"], "exclude": ["Other.ttf"]}]}
    assert raw.endswith(b"}\n")
    assert decoded(raw).default_with(lambda faults: pytest.fail(str(faults))) == approval


@pytest.mark.parametrize("raw", [b'{"tests": []}', b'{"configuration": {}, "tests": [{"input": "x"}]}', b'{"configuration": {}, "tests": [{"input": "x", "expectation": "", "only": [1]}]}', b"{"])
def test_native_wire_refusals_keep_the_decode_cause(raw: bytes) -> None:
    """Required native fields, selector types, and JSON syntax fail at the input boundary."""
    match decoded(raw):
        case Result(error=(Fault(reason=Reason.INVALID, cause=msgspec.DecodeError()),)):
            pass
        case outcome:
            pytest.fail(str(outcome))

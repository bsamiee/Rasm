"""Fontspector shaping expectations recorded from the test font and their JSON codec."""

from expression import Ok, Result
import msgspec
import pytest

from typography.faces.faults import Fault, Reason
from typography.shaping.corpus import Corpus
from typography.shaping.expectations import decoded, Defaults, encoded, Expectation, expected
from typography.shaping.runs import Binary

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_expected_records_each_line_under_each_language(binary: Binary, sample: Corpus) -> None:
    """One test per distinct line and language, `.notdef` forbidden, direction and the default system left to the shaper."""
    approved = expected(binary, sample)
    assert (approved.configuration.defaults, approved.configuration.forbidden_glyphs, len(approved.tests)) == (Defaults(), (".notdef",), 4 * 3)
    assert approved.tests[:3] == (
        Expectation(input="بب", expectation="uni0628.fina=2+500|uni0628.init=0+500", language="fa"),
        Expectation(input="بب", expectation="uni0628.fina=2+500|uni0628.init=0+500", language="ar"),
        Expectation(input="بب", expectation="uni0628.fina=2+500|uni0628.init=0+500"),
    )


def test_encoded_writes_fontspector_keys_and_decodes_back(binary: Binary, sample: Corpus) -> None:
    """JSON holding only set options, decoded to the same record."""
    approved = expected(binary, sample)
    raw = encoded(approved)
    match msgspec.json.decode(raw):
        case {"configuration": {"forbidden_glyphs": [".notdef"]} as configuration, "tests": [{"input": "بب", "language": "fa"}, _, {"input": "بب", "expectation": str()} as bare, *_]}:
            assert "language" not in bare
            assert "defaults" not in configuration
        case written:
            pytest.fail(f"{written}")
    assert decoded(raw) == Ok(approved)


def test_decoded_refuses_a_test_without_expectation() -> None:
    """Test entry missing its expectation, refused as invalid."""
    match decoded(b'{"tests": [{"input": "x"}]}'):
        case Result(error=(Fault(reason=Reason.INVALID),)):
            pass
        case refused:
            pytest.fail(f"{refused}")

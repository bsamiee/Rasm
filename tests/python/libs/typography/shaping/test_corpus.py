"""Corpus decode from TOML and the forms, marked forms, and pairs it derives."""

from pathlib import Path

from expression import Ok, Result
import msgspec
import pytest

from typography.faces.faults import Fault, Reason
from typography.shaping.corpus import Corpus, corpus

# --- [CONSTANTS] ------------------------------------------------------------------------

TOML = """
prose = { fa = ["بب"], ar = ["ببب"] }
marked = ["بَ"]
annotated = []
deep = []
words = ["ب ب"]
digits = []
punctuation = []
joins = ["ببب"]
seam = ["ببب"]
zwnj = []
ligatures = []
letters = [0x0628]
vowel_stacks = ["\\u064E"]
stacks = []
"""

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_corpus_decodes_toml_under_field_names(tmp_path: Path, sample: Corpus) -> None:
    """TOML with hex letters and escaped marks decoded to the record, its languages the prose languages and the default system, prose without a language the default system alone."""
    path = tmp_path / "corpus.toml"
    path.write_text(TOML, encoding="utf-8")
    assert (corpus(path), sample.languages) == (Ok(sample), ("fa", "ar", ""))
    assert msgspec.structs.replace(sample, prose=frozendict()).languages == ("",)


@pytest.mark.parametrize("text,reason", [(None, Reason.UNREADABLE), (f"{TOML}extra = []\n", Reason.INVALID), (TOML.replace("0x0628", "-1"), Reason.INVALID)])
def test_corpus_refuses_missing_file_unknown_key_and_negative_code(tmp_path: Path, text: str | None, reason: Reason) -> None:
    """Missing file unreadable, unknown key and code point under zero invalid."""
    path = tmp_path / "corpus.toml"
    if text is not None:
        path.write_text(text, encoding="utf-8")
    match corpus(path):
        case Result(error=(Fault(reason=refused),)) if refused is reason:
            pass
        case outcome:
            pytest.fail(str(outcome))


def test_lines_join_every_kind_once(sample: Corpus) -> None:
    """Plain prose first, a line repeated across kinds kept once."""
    assert (sample.plain, sample.lines) == (("بب", "ببب"), ("بب", "ببب", "بَ", "ب ب"))


def test_word_runs_keep_a_mark_word_between_its_neighbours(sample: Corpus) -> None:
    """Each word once, a lone small high three dots run with the words beside it."""
    assert msgspec.structs.replace(sample, words=("ب ۛ ب",)).word_runs == ("بَ", "ب", "ب ۛ ب")


def test_forms_hold_each_joining_side_open_through_joiner(sample: Corpus) -> None:
    """Four forms of beh, marks typed after the letter inside the joiners, the matrix crossing every letter with every stack."""
    joiner = "\N{ZERO WIDTH JOINER}"
    assert sample.forms(0x0628) == ("ب", f"ب{joiner}", f"{joiner}ب{joiner}", f"{joiner}ب")
    assert sample.forms(0x0628, "\N{ARABIC FATHA}") == sample.matrix == ("بَ", f"بَ{joiner}", f"{joiner}بَ{joiner}", f"{joiner}بَ")


def test_pairs_cross_letters_bare_and_between_two_behs(sample: Corpus) -> None:
    """Every ordered letter pair, unwrapped, wrapped, and with each mark on each letter."""
    assert (sample.pairs(), sample.pairs("ب")) == (("بب",), ("بببب",))
    assert sample.pairs("", ("", "َ")) == ("بب", "ببَ", "بَب", "بَبَ")


def test_set_corpus_decodes() -> None:
    """Corpus file the set declaration names, decoded with Persian and Arabic prose."""
    assert corpus(Path(__file__).parents[5] / "assets" / "fonts" / "corpus.toml").map(lambda read: read.languages) == Ok(("fa", "ar", ""))

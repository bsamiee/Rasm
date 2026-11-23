"""Findings of the Rasm rules a test module names over its case table, from one refurb build per module and Python version, against the `# expect:` markers each case source carries."""

from contextlib import chdir
from dataclasses import replace
from io import StringIO
import re
import sys
from tokenize import COMMENT, generate_tokens

import pytest
lazy from refurb.error import Error, ErrorCode
lazy from refurb.main import run_refurb
lazy from refurb.settings import load_settings

# --- [CONSTANTS] ------------------------------------------------------------------------

_MARKER = re.compile(r"#\s*expect:\s*(?P<items>.+)$")
_LINE = re.compile(r"(?:{0})(?:\s*;\s*(?:{0}))*".format(r'(?:\d+\.\d+\s+)?[A-Z]+\d+(?:\s+(?:`[^`]+`|"[^"]+"))*'))
_ITEM = re.compile(r'(?:(?P<version>\d+\.\d+)\s+)?(?P<code>[A-Z]+\d+)(?P<parts>(?:\s+(?:`[^`]+`|"[^"]+"))*)')
_SPAN = re.compile(r"`([^`]+)`")
_PHRASE = re.compile(r'"([^"]+)"')

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="module")
def version() -> str:
    """Python version refurb types the cases against, the running interpreter's unless a test parametrizes it."""
    return ".".join(map(str, sys.version_info[:2]))


@pytest.fixture(scope="module")
def findings(request: pytest.FixtureRequest, version: str, tmp_path_factory: pytest.TempPathFactory) -> frozendict[str, tuple[Error, ...]]:
    """Builds every source of the module's `CASES` as a module named by its case inside one folder, runs the module's `RULES` alone from that folder as the member's own entry runs from its root, and keys each finding by its case.

    A compiler failure fails the test with its messages.

    Returns:
        Findings of each case, in source order.
    """
    folder = tmp_path_factory.mktemp("refurb")
    for case, source in request.module.CASES.items():
        (folder / f"{case}.py").write_text(source, encoding="utf-8")
    with chdir(folder):
        settings = load_settings(["--python-version", version])
        found = run_refurb(
            replace(
                settings, files=sorted(str(folder / f"{case}.py") for case in request.module.CASES), enable_all=False, disable_all=True, enable=set(map(ErrorCode.from_error, request.module.RULES))
            )
        )
    if messages := tuple(error for error in found if isinstance(error, str)):
        pytest.fail("\n".join(messages), pytrace=False)
    return frozendict({case: tuple(error for error in found if isinstance(error, Error) and error.filename == str(folder / f"{case}.py")) for case in request.module.CASES})


@pytest.fixture(scope="module")
def unmatched(request: pytest.FixtureRequest, version: str, findings: frozendict[str, tuple[Error, ...]]) -> frozendict[str, tuple[tuple[str, ...], tuple[str, ...]]]:
    """Pairs each `# expect: [<version>] <code> <`span`|"phrase">...` item a comment token of a case states, for every version or the one it names, `;` separating items of one line, with an unpaired finding on its line and code.

    A finding meets an item when its message names the item's backticked spans in the item's order, a subject before the name it moves to, and holds each quoted phrase in its prose, the phrase telling one kind of message of a code from another.
    A marker off that form stays as its own text, which no finding meets.

    Returns:
        Items no finding met, then findings no item states, of each case, `((), ())` where the case states every finding.
    """

    def said(message: str, spans: tuple[str, ...], phrases: tuple[str, ...], /) -> bool:
        named = iter(_SPAN.findall(message))
        return all(span in named for span in spans) and all(phrase in _SPAN.sub("", message) for phrase in phrases)

    def paired(source: str, found: tuple[Error, ...], /) -> tuple[tuple[str, ...], tuple[str, ...]]:
        pending = list(found)
        missing: list[str] = []
        for line, code, spans, phrases in sorted(
            (
                entry
                for token in generate_tokens(StringIO(source).readline)
                if token.type == COMMENT and (marker := _MARKER.match(token.string))
                for entry in (
                    ((token.start[0], marker["items"], (), ()),)
                    if _LINE.fullmatch(marker["items"].strip()) is None
                    else (
                        (token.start[0], item["code"], tuple(_SPAN.findall(item["parts"])), tuple(_PHRASE.findall(_SPAN.sub("", item["parts"]))))
                        for item in _ITEM.finditer(marker["items"])
                        if item["version"] in {None, version}
                    )
                )
            ),
            key=lambda entry: (entry[0], entry[1], -len(entry[2]) - len(entry[3])),
        ):
            met = next((error for error in pending if (error.line, str(ErrorCode.from_error(type(error)))) == (line, code) and said(error.msg, spans, phrases)), None)
            if met is None:
                missing.append("".join((f"{line} {code}", *(f" `{span}`" for span in spans), *(f' "{phrase}"' for phrase in phrases))))
            else:
                pending.remove(met)
        return tuple(missing), tuple(f"{error.line} {ErrorCode.from_error(type(error))} {error.msg}" for error in pending)

    return frozendict({case: paired(source, findings[case]) for case, source in request.module.CASES.items()})

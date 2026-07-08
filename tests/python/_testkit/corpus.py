"""Cross-language contracts-corpus reader: manifest decode, disk audit, and round-trip folds."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # noqa: TC003  # msgspec resolves fold annotations at runtime
from pathlib import Path
import re

import msgspec
lazy import pytest


# --- [CONSTANTS] ------------------------------------------------------------------------

_MANIFEST_NAME = "MANIFEST.md"
_PAYLOAD_KINDS: frozenset[str] = frozenset({"wire-bytes", "canonical-json", "digest", "descriptor-set"})
_PIN_VOCABULARY: frozenset[str] = frozenset({"REAL", "DESIGN-PIN"})

_ANCHOR = re.compile(r"csharp:([A-Za-z0-9_.]+)/([A-Za-z0-9_./-]+?)(?:#[A-Z0-9_]+)?`")
_ENTRY_HEAD = re.compile(r"^### \[[0-9.]+\]-\[([A-Z0-9_]+)\]\s*$")
_FIELD = re.compile(r"^- (Seam|Producer|Consumers|Payload|Pin|Blocker|Shape|Expectation|Regenerate when): (.+)$")
_CODE_SPAN = re.compile(r"`([^`]+)`")

# --- [MODELS] ---------------------------------------------------------------------------


class CorpusEntry(msgspec.Struct, frozen=True):
    """One manifest fixture record in the README `[03]-[MANIFEST]` field grammar."""

    fixture: str
    seam: str
    producer: str
    consumers: str
    payload: frozenset[str]
    pin: str
    shape: str
    blocker: str = ""
    expectation: str = ""
    regenerate: str = ""


class CorpusManifest(msgspec.Struct, frozen=True):
    """Decoded corpus registry bound to its on-disk root, with the audit and asset folds."""

    root: Path
    entries: tuple[CorpusEntry, ...]
    ledger: tuple[tuple[str, str, str], ...]

    def entry(self, fixture: str) -> CorpusEntry:
        """Resolve one fixture record by name.

        Returns:
            The matching entry.

        Raises:
            KeyError: When no entry carries ``fixture``.
        """
        match [e for e in self.entries if e.fixture == fixture]:
            case [found]:
                return found
            case _:
                raise KeyError(f"no corpus entry named {fixture!r}; known: {[e.fixture for e in self.entries]}")

    def assets(self, seam: str, *, suffix: str = "") -> tuple[Path, ...]:
        """Enumerate a seam directory's committed asset files.

        Returns:
            Sorted asset paths under ``root/seam`` matching ``suffix``; empty when un-emitted.
        """
        seam_dir = self.root / seam
        return tuple(sorted(p for p in seam_dir.iterdir() if p.is_file() and p.name.endswith(suffix))) if seam_dir.is_dir() else ()

    def audit(self, libs_root: Path) -> tuple[str, ...]:
        """Fold the corpus-honesty defects the contracts law names.

        Pin-state honesty, payload vocabulary, ledger-entry coherence, seam-directory
        registration, DESIGN-PIN emptiness, and producer-anchor page resolution fold into one
        defect stream; an empty return is the clean verdict.

        Returns:
            One human-readable defect string per violation.
        """
        seams = {e.seam for e in self.entries}
        real_seams = {e.seam for e in self.entries if e.pin == "REAL"}
        by_fixture = {e.fixture: e for e in self.entries}
        on_disk = tuple(sorted(p for p in self.root.iterdir() if p.is_dir())) if self.root.is_dir() else ()

        entry_defects = (
            defect
            for e in self.entries
            for defect in (
                f"{e.fixture}: pin {e.pin!r} outside {sorted(_PIN_VOCABULARY)}" if e.pin not in _PIN_VOCABULARY else "",
                f"{e.fixture}: REAL entry missing Expectation" if e.pin == "REAL" and not e.expectation else "",
                f"{e.fixture}: REAL entry carries Blocker" if e.pin == "REAL" and e.blocker else "",
                f"{e.fixture}: DESIGN-PIN entry missing Blocker" if e.pin == "DESIGN-PIN" and not e.blocker else "",
                f"{e.fixture}: DESIGN-PIN entry carries Expectation" if e.pin == "DESIGN-PIN" and e.expectation else "",
                f"{e.fixture}: payload {sorted(e.payload - _PAYLOAD_KINDS)} outside the closed vocabulary" if e.payload - _PAYLOAD_KINDS else "",
                f"{e.fixture}: entry declares no payload kind" if not e.payload else "",
            )
            if defect
        )
        ledger_defects = (
            defect
            for fixture, seam, pin in self.ledger
            for entry in (by_fixture.get(fixture),)
            for defect in (
                f"{fixture}: ledger row has no H3 entry" if entry is None else "",
                f"{fixture}: ledger seam {seam!r} != entry seam {entry.seam!r}" if entry is not None and entry.seam != seam else "",
                f"{fixture}: ledger pin {pin!r} != entry pin {entry.pin!r}" if entry is not None and entry.pin != pin else "",
            )
            if defect
        )
        missing_rows = (f"{fixture}: entry has no ledger row" for fixture in sorted(set(by_fixture) - {row[0] for row in self.ledger}))
        disk_defects = (
            defect
            for directory in on_disk
            for defect in (
                f"{directory.name}: seam directory has no manifest entry" if directory.name not in seams else "",
                f"{directory.name}: DESIGN-PIN seam carries assets"
                if directory.name in seams - real_seams and any(p.is_file() for p in directory.iterdir())
                else "",
            )
            if defect
        )
        anchor_defects = (
            f"{e.fixture}: producer anchor csharp:{pkg}/{page} resolves to no planning page"
            for e in self.entries
            for pkg, page in _ANCHOR.findall(e.producer)
            if not (libs_root / "csharp" / pkg / ".planning" / f"{page}.md").is_file()
        )
        return (*entry_defects, *ledger_defects, *missing_rows, *disk_defects, *anchor_defects)


# --- [OPERATIONS] -----------------------------------------------------------------------


def load_manifest(root: Path) -> CorpusManifest:
    """Decode ``root/MANIFEST.md`` into the typed corpus registry.

    Returns:
        Manifest carrying every ledger row and H3 fixture entry.
    """
    lines = (root / _MANIFEST_NAME).read_text(encoding="utf-8").splitlines()
    ledger = tuple(
        (cells[1], _CODE_SPAN.sub(r"\1", cells[2]).strip(), cells[-1])
        for line in lines
        if line.startswith("|") and "---" not in line and (cells := [c.strip() for c in line.strip("|").split("|")])[0].startswith("[")
        if cells[0] != "[INDEX]"
    )
    entries: list[CorpusEntry] = []
    fields: dict[str, str] = {}
    fixture = ""

    def _seal() -> None:
        payload = frozenset(_CODE_SPAN.findall(fields.get("Payload", "")))
        seam = _CODE_SPAN.sub(r"\1", fields.get("Seam", "")).strip()
        entries.append(
            CorpusEntry(
                fixture=fixture,
                seam=seam,
                producer=fields.get("Producer", ""),
                consumers=fields.get("Consumers", ""),
                payload=payload,
                pin=fields.get("Pin", "").strip(),
                shape=fields.get("Shape", ""),
                blocker=fields.get("Blocker", ""),
                expectation=fields.get("Expectation", ""),
                regenerate=fields.get("Regenerate when", ""),
            )
        ) if fixture else None

    for line in lines:
        head = _ENTRY_HEAD.match(line)
        if head is not None:
            _seal()
            fixture, fields = head.group(1), {}
            continue
        field = _FIELD.match(line)
        if field is not None and fixture:
            fields[field.group(1)] = field.group(2)
    _seal()
    return CorpusManifest(root=root, entries=tuple(entries), ledger=ledger)


def assert_corpus_roundtrip[T](
    manifest: CorpusManifest, fixture: str, decode: Callable[[bytes], T], encode: Callable[[T], bytes], *, suffix: str = ".bin"
) -> int:
    """Round-trip every committed asset of a REAL fixture to byte identity.

    A DESIGN-PIN fixture skips by its named blocker — the consumer obligation starts at pin
    graduation, and a fabricated stand-in is the rejected form.

    Returns:
        The number of assets proven; zero when the producer has not emitted yet.
    """
    entry = manifest.entry(fixture)
    if entry.pin != "REAL":
        pytest.skip(f"{fixture} is {entry.pin}: {entry.blocker or 'producer has not pinned the byte-deriving input'}")
    proven = 0
    for path in manifest.assets(entry.seam, suffix=suffix):
        raw = path.read_bytes()
        again = encode(decode(raw))
        assert again == raw, f"{fixture}: {path.name} re-encode is not byte-identical ({len(again)} bytes vs {len(raw)})"
        proven += 1
    return proven


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CorpusEntry", "CorpusManifest", "assert_corpus_roundtrip", "load_manifest"]

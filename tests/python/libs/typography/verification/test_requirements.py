"""Requirement selection, native process failures, and isolated shaping approvals."""

from pathlib import Path

import msgspec
import pytest

from typography.faces.declaration import Face, Kufi, OrnamentFace, Ringed, Sheared
from typography.faces.glyphs import Source
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary
from typography.verification.requirements import approved, Coverage, Distinct, Expectations, Finding, Identity, MarkClearance, Metrics, Profile, Reachable, requirements, Sanitized, Seam, verified

from .font import built

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_requirements_follow_the_face_geometry() -> None:
    """Genre parameters become measures while transformed and ornamental faces omit inapplicable letter comparisons."""
    genre = Kufi()
    plain = requirements(Face(style="Regular", original="a.ttf", genre=genre))
    assert MarkClearance(gap=genre.clearance) in plain
    assert Metrics(pitch=genre.pitch) in plain
    assert {Seam, Identity, Distinct} <= {type(requirement) for requirement in plain}
    assert {Seam, Identity}.isdisjoint(type(requirement) for requirement in requirements(Face(style="Regular", original="a.ttf", treatments=(Ringed(ring=0.01),))))
    assert not any(isinstance(requirement, Identity) for requirement in requirements(Face(style="Italic", original="a.ttf", treatments=(Sheared(angle=12.0),))))
    assert requirements(OrnamentFace(style="Regular", original="a.ttf")) == (Sanitized(), Profile(excluded=("opentype/monospace",)), Coverage(), Metrics())


def test_independent_native_and_geometry_failures_accumulate(tmp_path: Path, binary: Binary, source: Source, sample: Corpus) -> None:
    """An absent approval and a duplicated digit are both reported from the same font snapshot."""
    duplicated = built(two=((100, 0, 160, 500),))
    path = tmp_path / "Duplicated.ttf"
    path.write_bytes(duplicated.data)
    findings = verified(duplicated, (Expectations(), Distinct()), source=source, original=binary, corpus=sample, path=path, approved=tmp_path / "absent.json")
    assert {finding.requirement for finding in findings} == {"expectations", "distinct"}
    assert findings[-1] == Finding(requirement="distinct", subject="uni06F1 uni06F2", measured=1, bound=0.9)
    assert all(finding.measured for finding in findings if finding.requirement == "expectations")


def test_ots_accepts_the_complete_font_and_rejects_truncated_external_bytes(tmp_path: Path, binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """Native OTS reports the real process rejection when a valid font file is cut off."""
    cut = tmp_path / "Cut-Regular.ttf"
    cut.write_bytes(binary.data[:1024])
    assert not verified(binary, (Sanitized(),), source=source, original=binary, corpus=sample, path=path, approved=path)
    match verified(binary, (Sanitized(),), source=source, original=binary, corpus=sample, path=cut, approved=path):
        case (Finding(requirement="sanitized", subject="Cut-Regular.ttf", measured=str(report), bound="accepted"),) if report:
            pass
        case found:
            pytest.fail(str(found))


def test_native_profile_reports_failures_and_exclusion_removes_the_selected_check(binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """A real universal report contains failing checks and native exclusion removes the selected check's findings."""
    findings = verified(binary, (Profile(),), source=source, original=binary, corpus=sample, path=path, approved=path)
    assert findings
    assert all(finding.measured in {"FAIL", "ERROR", "FATAL"} for finding in findings)
    selected = findings[0].subject.split()[0]
    excluded = verified(binary, (Profile(excluded=(selected,)),), source=source, original=binary, corpus=sample, path=path, approved=path)
    assert not any(finding.subject.split()[0] == selected for finding in excluded)
    assert set(excluded) == {finding for finding in findings if finding.subject.split()[0] != selected}


def test_shaping_approvals_are_isolated_from_unrelated_files(tmp_path: Path, binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """Only the chosen approval controls regression, even beside another valid approval and an absent requested file."""
    approval = approved(binary, sample, tmp_path / "approved" / "Verification-Regular.json").default_with(lambda faults: pytest.fail(str(faults)))
    changed = built(fatha=40)
    approved(changed, sample, approval.with_name("Unrelated-Regular.json")).default_with(lambda faults: pytest.fail(str(faults)))
    applicable = tuple(
        msgspec.structs.replace(requirement, shapers=("ot",)) if isinstance(requirement, Reachable) else requirement
        for requirement in requirements(source.face)
        if not isinstance(requirement, Profile | Coverage)
    )
    assert not verified(binary, applicable, source=source, original=binary, corpus=sample, path=path, approved=approval)
    changed_path = tmp_path / "Changed-Regular.ttf"
    changed_path.write_bytes(changed.data)
    findings = verified(changed, (Expectations(),), source=source, original=binary, corpus=sample, path=changed_path, approved=approval)
    assert {finding.subject for finding in findings} == {"بَب", "بَبُب", "بَ بُ"}
    absent = verified(binary, (Expectations(),), source=source, original=binary, corpus=sample, path=path, approved=approval.with_name("Absent.json"))
    assert absent
    assert all(finding.requirement == "expectations" for finding in absent)

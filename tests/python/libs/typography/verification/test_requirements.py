"""Requirements per genre, treatment, and ornament face, findings accumulated over every requirement, OTS and fontspector runs, and approvals."""

from pathlib import Path

from expression import Some
import msgspec
import pytest

from typography.faces.declaration import Face, Kufi, OrnamentFace, Ringed, Sheared
from typography.faces.glyphs import Source
from typography.shaping.corpus import Corpus
from typography.shaping.expectations import decoded
from typography.shaping.runs import Binary
from typography.verification.requirements import (
    approved,
    Coverage,
    Distinct,
    Expectations,
    Finding,
    Identity,
    MarkClearance,
    Metrics,
    Profile,
    Reproducible,
    Requirements,
    requirements,
    Sanitized,
    Seam,
    verified,
)

from .conftest import built

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [REQUIREMENTS]


def test_requirements_follow_genre_and_treatment() -> None:
    """Kufi values in the text set, a ringed face without seam and identity, a sheared one without identity, a recorded hash added."""
    plain = requirements(Face(style="Regular", original="a.ttf", genre=Kufi()), recorded=Some("abc"))
    assert (MarkClearance(gap=Kufi().clearance), Metrics(pitch=Kufi().pitch), Reproducible(hash="abc")) == (plain[5], plain[11], plain[-1])
    assert {type(requirement) for requirement in plain} >= {Seam, Identity, Distinct}
    assert not {Seam, Identity} & {type(requirement) for requirement in requirements(Face(style="Regular", original="a.ttf", treatments=(Ringed(ring=0.01),)))}
    assert not any(isinstance(requirement, Identity) for requirement in requirements(Face(style="Italic", original="a.ttf", treatments=(Sheared(angle=12.0),))))


def test_requirements_of_an_ornament_face_skip_letters() -> None:
    """Sanitized, the universal profile without the monospace check, coverage, and the original's pitch."""
    assert requirements(OrnamentFace(style="Regular", original="a.ttf")) == (Sanitized(), Profile(excluded=("opentype/monospace",)), Coverage(), Metrics())


# --- [VERIFICATION]


def _verified(binary: Binary, checked: tuple[Requirements, ...], source: Source, sample: Corpus, path: Path, approval: Path) -> tuple[Finding, ...]:
    return verified(binary, checked, source=source, original=binary, corpus=sample, path=path, approved=approval)


def test_verified_accumulates_every_requirement(tmp_path: Path, binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """A missing approval and a duplicated digit reported together, the other requirements held, every finding encoding as JSON."""
    duplicated = built(two=((100, 0, 160, 500),))
    findings = verified(duplicated, (Sanitized(), Expectations(), Distinct(), Reproducible(hash="0")), source=source, original=binary, corpus=sample, path=path, approved=tmp_path / "absent.json")
    assert [(finding.requirement, finding.subject) for finding in findings] == [("expectations", str(tmp_path / "absent.json")), ("distinct", "uni06F1 uni06F2"), ("reproducible", "sha256")]
    assert msgspec.json.decode(msgspec.json.encode(findings), type=tuple[Finding, ...]) == findings


def test_verified_passes_the_test_font_on_its_own_approval(tmp_path: Path, binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """Every text requirement holds once its expectations are approved from the same build, the approval holding one test per line and language."""
    approval = approved(binary, sample, tmp_path / "Verification-Regular.json").ok
    held = tuple(requirement for requirement in requirements(source.face) if not isinstance(requirement, Profile | Coverage))
    assert _verified(binary, held, source, sample, path, approval) == ()
    assert decoded(approval.read_bytes()).map(lambda written: len(written.tests)) == decoded(approval.read_bytes()).map(lambda _: len(sample.lines) * len(sample.languages))


# --- [PROCESSES]


def test_sanitized_accepts_the_test_font_and_refuses_a_cut_one(tmp_path: Path, binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """Built font accepted, its first kilobyte alone refused with the OTS report."""
    (cut := tmp_path / "Cut-Regular.ttf").write_bytes(path.read_bytes()[:1024])
    assert _verified(binary, (Sanitized(),), source, sample, path, path) == ()
    match _verified(binary, (Sanitized(),), source, sample, cut, path):
        case (Finding(subject="Cut-Regular.ttf", measured=str(report), bound="accepted"),) if report:
            pass
        case found:
            pytest.fail(f"{found}")


def test_profile_reports_failing_results_alone(binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """Universal profile over the test font, every finding a failing status of one check."""
    assert all(finding.measured in {"FAIL", "ERROR", "FATAL"} for finding in _verified(binary, (Profile(),), source, sample, path, path))


def test_expectations_pass_approved_shaping_and_report_each_changed_input(tmp_path: Path, binary: Binary, source: Source, sample: Corpus, path: Path) -> None:
    """Expectations approved from the test font pass it, and a build with a sunk fatha changes the marked inputs."""
    approval = approved(binary, sample, tmp_path / "approved" / "Verification-Regular.json").ok
    assert _verified(binary, (Expectations(),), source, sample, path, approval) == ()
    (changed := tmp_path / "Verification-Regular.ttf").write_bytes(built(fatha=40).data)
    assert {finding.subject for finding in _verified(binary, (Expectations(),), source, sample, changed, approval)} == {"بَب", "بَبُب", "بَ بُ"}

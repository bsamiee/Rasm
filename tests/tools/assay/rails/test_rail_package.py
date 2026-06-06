"""Package rail laws."""

from tools.assay.core.status import RailStatus
from tools.assay.rails.package import _lone_match, _slug_from_bytes  # noqa: PLC2701


def test_slug_from_bytes_handles_namespaced_project_xml() -> None:
    """Package slug discovery strips XML namespaces."""
    raw = b'<Project xmlns="urn:test"><PropertyGroup><YakPackageSlug>rasm-bridge</YakPackageSlug></PropertyGroup></Project>'

    assert _slug_from_bytes(raw) == "rasm-bridge"


def test_lone_match_rejects_missing_and_duplicate_slugs() -> None:
    """Package project resolution requires exactly one matching slug."""
    missing = _lone_match("pkg", (("a.csproj", "other"),))
    duplicate = _lone_match("pkg", (("a.csproj", "pkg"), ("b.csproj", "pkg")))
    present = _lone_match("pkg", (("a.csproj", "pkg"),))

    assert missing.is_error()
    assert missing.error.status is RailStatus.FAULTED
    assert duplicate.is_error()
    assert "duplicates" in duplicate.error.message
    assert present.is_ok()
    assert present.ok == "a.csproj"

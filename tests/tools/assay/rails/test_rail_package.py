"""Package rail laws."""

from typing import TYPE_CHECKING

from expression import Ok
import msgspec

from tools.assay.core.model import Claim, fold
from tools.assay.core.status import RailStatus
from tools.assay.rails import package as package_rail
from tools.assay.rails.package import _lifecycle, _lone_match, _slug_from_bytes, PackageParams, YakMeta  # noqa: PLC2701


if TYPE_CHECKING:
    from collections.abc import Callable

    import pytest

    from tests.tools.assay.conftest import AssayHarness


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


def test_yak_meta_validation_contains_workspace_paths_and_patterns(assay_root: AssayHarness) -> None:
    """Package metadata validation rejects escaped outputs and package patterns before staging."""
    project = assay_root.root / "apps/Pkg/Pkg.csproj"
    project.parent.mkdir(parents=True)
    project.write_text("<Project/>", encoding="utf-8")
    yak = assay_root.root / "tools/yak"
    yak.parent.mkdir(parents=True)
    yak.write_text("#!/bin/sh\n", encoding="utf-8")
    yak.chmod(0o755)
    target = project.parent / "bin/Release/net9.0"
    manifest = project.parent / "yak"
    package_dir = assay_root.root / ".artifacts/yak/pkg"

    meta = YakMeta(
        project="apps/Pkg/Pkg.csproj",
        manifest_dir=manifest,
        target_dir=target,
        assembly_name="Pkg",
        target_ext=".rhp",
        yak_path=yak,
        yak_platform="mac",
        yak_push_source="",
        package_dir=package_dir,
        package_pattern="pkg-rh9_1.0-mac.yak",
        target_framework="net9.0",
        project_dir=project.parent,
    )

    assert meta.validate(assay_root.settings, "pkg", "pkg").is_ok()
    escaped = msgspec.structs.replace(meta, package_dir=assay_root.root.parent / "outside", package_pattern="../bad.yak")
    outcome = escaped.validate(assay_root.settings, "pkg", "pkg")
    assert outcome.is_error()
    assert "escaped workspace" in outcome.error.message or "unsafe package pattern" in outcome.error.message


def test_package_lifecycle_acquires_slug_level_lease(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Stage/deploy/publish lifecycle work is wrapped by the package slug lease, not only the staging swap."""
    calls: list[str] = []
    meta = YakMeta(
        project="apps/Pkg/Pkg.csproj",
        manifest_dir=assay_root.root,
        target_dir=assay_root.root,
        assembly_name="Pkg",
        target_ext=".rhp",
        yak_path=assay_root.root / "yak",
        yak_platform="mac",
        yak_push_source="",
        package_dir=assay_root.root / ".artifacts/yak/pkg",
        package_pattern="pkg-rh9_1.0-mac.yak",
    )

    def fake_lease(resource: str, action: Callable[[object], object], **_kwargs: object) -> object:
        calls.append(resource)
        return action(object())

    monkeypatch.setattr(package_rail, "leased", fake_lease)
    monkeypatch.setattr(package_rail, "_resolve_project", lambda *_args, **_kwargs: Ok("apps/Pkg/Pkg.csproj"))
    monkeypatch.setattr(package_rail, "evaluate_meta", lambda *_args, **_kwargs: Ok(meta))
    monkeypatch.setattr(package_rail, "_stage_meta", lambda *_args, **_kwargs: Ok(fold(Claim.PACKAGE, "stage", ())))
    monkeypatch.setattr(package_rail, "_finish", lambda *_args, **_kwargs: Ok(fold(Claim.PACKAGE, "stage", ())))

    outcome = _lifecycle(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug="pkg", version="1.0.0"), "stage")

    assert outcome.is_ok()
    assert calls == ["package-pkg"]

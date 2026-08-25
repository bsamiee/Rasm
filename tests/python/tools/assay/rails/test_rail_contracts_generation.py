"""Generation-image, freshness, and atomic contracts commit laws."""


# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import os
from pathlib import Path
import shutil

from expression import Error, Ok
import pytest

from assay.core.model import Fault, RailStatus, receipt
from assay.rails import contracts as contracts_rail
from assay.rails.contracts_generation import (
    changes,
    compose_image,
    freshness_rows,
    GenerationImage,
    render,
    tree,
)
from tests.python._testkit.spec import assert_error_status, assert_ok, refutes
from tests.python.tools.assay.kit import AssayHarness
from tests.python.tools.assay.rails import test_rail_contracts as corpus_kit

# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (GenerationImage, changes, compose_image, freshness_rows, render, tree)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _file(path: Path, payload: bytes) -> Path:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(payload)
    return path


def _fifo(path: Path) -> None:
    create = getattr(os, "mkfifo", None)
    assert callable(create)
    create(path)


def _committed_bytes(root: Path) -> tuple[tuple[str, bytes], ...]:
    template = assert_ok(contracts_rail.read_template(root))
    manifest = assert_ok(contracts_rail.load_manifest(root / contracts_rail.CORPUS))
    paths = tuple(root / rel for rel in (*contracts_rail.out_dirs(template), *contracts_rail._owned_files(template, manifest)))
    return tuple(
        (path.relative_to(root).as_posix(), path.read_bytes())
        for target in paths
        for path in ((target,) if target.is_file() else tuple(sorted(row for row in target.rglob("*") if row.is_file())))
    )


# --- [ATOMIC_COMMIT]


def test_late_staged_emission_failure_leaves_every_committed_target_byte_unchanged(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    root = corpus_kit._corpus(assay_root.root)
    before = _committed_bytes(root)
    monkeypatch.setattr(contracts_rail, "render", lambda _base, _rows: Error(Fault(("contracts", "render"), message="staged write refused")))

    report = assert_ok(corpus_kit._run(assay_root, corpus_kit._fan(root), "generate"))

    lanes = dict(corpus_kit._detail(report).lanes)
    assert report.status is RailStatus.FAILED
    assert lanes["buf-generate"] == "ok" and lanes["corpus-emit"] == "failed"
    assert _committed_bytes(root) == before


def test_generator_failure_leaves_every_committed_target_byte_unchanged(assay_root: AssayHarness) -> None:
    root = corpus_kit._corpus(assay_root.root)
    before = _committed_bytes(root)
    failed = Ok(receipt(("buf", "generate"), 1, stderr=b"plugin failed", status=RailStatus.FAILED))

    report = assert_ok(corpus_kit._run(assay_root, corpus_kit._fan(root, outcomes={"buf-generate": failed}), "generate"))

    assert report.status is RailStatus.FAILED
    assert dict(corpus_kit._detail(report).lanes)["corpus-emit"] == "skip"
    assert _committed_bytes(root) == before


def test_successful_generation_commits_scratch_output_and_preserves_authored_package_shells(assay_root: AssayHarness) -> None:
    """The commit lands the staged out roots and leaves every authored package shell beside them byte-identical."""
    root = corpus_kit._corpus(assay_root.root)
    authored = tuple(root / contracts_rail.CORPUS / shell for shell in corpus_kit._SHELLS)
    for path in authored:
        path.write_bytes(b"authored")

    def regenerate(scratch: Path) -> None:
        for out in corpus_kit._OUTS:
            shutil.rmtree(scratch / out, ignore_errors=True)
            shutil.copytree(root / out, scratch / out)
        (scratch / corpus_kit._OUTS[0] / "transaction_probe.ts").write_text("export const TransactionProbe = true;\n", encoding="utf-8")

    report = assert_ok(corpus_kit._run(assay_root, corpus_kit._fan(root, regenerate=regenerate), "generate"))

    assert report.status is RailStatus.OK
    assert (root / corpus_kit._OUTS[0] / "transaction_probe.ts").is_file()
    assert all(path.read_bytes() == b"authored" for path in authored)
    assert not (root / contracts_rail._COMMIT_MARKER).exists()


def test_generation_commits_past_package_manager_and_build_artifacts_beside_its_out_roots(assay_root: AssayHarness) -> None:
    """A pnpm link farm and a hard-linked build output beside the out roots ride the git-ignore carve and reach the write untouched."""
    root = corpus_kit._corpus(assay_root.root)
    store = _file(root / "node_modules/.pnpm/dep@1/node_modules/dep/index.js", b"dep")
    farm = root / contracts_rail.CORPUS / "node_modules/@scope"
    farm.mkdir(parents=True)
    (farm / "dep").symlink_to("../../../../node_modules/.pnpm/dep@1/node_modules/dep")
    built = _file(root / contracts_rail.CORPUS / "obj/Debug/built.dll", b"built")
    os.link(store, root / contracts_rail.CORPUS / "obj/Debug/hardlinked.dll")
    corpus_kit._git_ignored(root, "node_modules/", "obj/")

    def regenerate(scratch: Path) -> None:
        for out in corpus_kit._OUTS:
            shutil.rmtree(scratch / out, ignore_errors=True)
            shutil.copytree(root / out, scratch / out)
        (scratch / corpus_kit._OUTS[0] / "transaction_probe.ts").write_text("export const TransactionProbe = true;\n", encoding="utf-8")

    report = assert_ok(corpus_kit._run(assay_root, corpus_kit._fan(root, regenerate=regenerate), "generate"))

    assert report.status is RailStatus.OK
    assert (root / corpus_kit._OUTS[0] / "transaction_probe.ts").is_file()
    assert (farm / "dep").readlink() == Path("../../../../node_modules/.pnpm/dep@1/node_modules/dep")
    assert built.read_bytes() == b"built" and not (root / contracts_rail._COMMIT_MARKER).exists()


# --- [EMISSION]


def test_render_rejects_aliases_without_touching_their_external_targets(tmp_path: Path) -> None:
    stage = tmp_path / "stage"
    stage.mkdir()
    outside_file = _file(tmp_path / "outside.txt", b"outside")
    (stage / "leaf").symlink_to(outside_file)
    outside_dir = tmp_path / "outside"
    outside_dir.mkdir()
    (stage / "parent").symlink_to(outside_dir, target_is_directory=True)

    leaf = assert_error_status(render(stage, (("leaf", b"changed"),)), RailStatus.FAULTED)
    parent = assert_error_status(render(stage, (("parent/value", b"changed"),)), RailStatus.FAULTED)

    assert "regular file" in leaf.message or "symbolic link" in leaf.message.lower()
    assert "alias-free directory" in parent.message
    assert outside_file.read_bytes() == b"outside" and not (outside_dir / "value").exists()


def test_render_rejects_repeated_overlapping_and_hardlinked_emissions_before_writing(tmp_path: Path) -> None:
    stage = tmp_path / "stage"
    stage.mkdir()
    original = _file(stage / "first", b"old")

    overlap = assert_error_status(render(stage, (("first", b"new"), ("first/child", b"child"))), RailStatus.FAULTED)
    assert "repeat or overlap" in overlap.message and original.read_bytes() == b"old"

    hardlink = _file(stage / "hardlink", b"shared")
    os.link(hardlink, tmp_path / "alias")
    alias = assert_error_status(render(stage, (("hardlink", b"new"),)), RailStatus.FAULTED)
    assert "unaliased regular file" in alias.message and hardlink.read_bytes() == b"shared"


# --- [IMAGE_ADMISSION]


def test_compose_refuses_overlapping_owned_paths_and_aliased_or_escaping_inputs_without_copying(tmp_path: Path) -> None:
    repo = tmp_path / "repo"
    generated = tmp_path / "generated"
    stage = tmp_path / "stage"
    _file(repo / "package/catalog.md", b"catalog")
    _file(generated / "package/out/generated", b"generated")

    overlap = assert_error_status(compose_image(repo, generated, stage, ("package/out",), ("package/out/catalog.md",)), RailStatus.FAULTED)
    assert "repeat or overlap" in overlap.message and not stage.exists()

    external = tmp_path / "external"
    _file(external / "generated", b"outside")
    (generated / "aliased").symlink_to(external, target_is_directory=True)
    aliased = assert_error_status(compose_image(repo, generated, stage, ("aliased",), ()), RailStatus.FAULTED)
    assert "absent or unsafe" in aliased.message and not stage.exists()

    (generated / "package/out/escape").symlink_to("../../../external")
    escaped = assert_error_status(compose_image(repo, generated, stage, ("package/out",), ()), RailStatus.FAULTED)
    assert "symlink escapes" in escaped.message and not stage.exists()


def test_compose_images_only_owned_paths_while_the_guard_still_refuses_an_escape_inside_one(tmp_path: Path) -> None:
    """A pnpm link farm beside an out root images nowhere; the same link inside one still refuses."""
    repo = tmp_path / "repo"
    generated = tmp_path / "generated"
    stage = tmp_path / "stage"
    _file(repo / "node_modules/.pnpm/dep/index.js", b"dep")
    _file(repo / "package/.api/catalog.md", b"catalog")
    _file(generated / "package/gen/wire.ts", b"generated")
    (repo / "package/node_modules").mkdir(parents=True)
    (repo / "package/node_modules/dep").symlink_to("../../../node_modules/.pnpm/dep")
    _file(repo / "package/dist/built.js", b"built")
    os.link(repo / "node_modules/.pnpm/dep/index.js", repo / "package/dist/alias.js")

    staged = assert_ok(compose_image(repo, generated, stage, ("package/gen",), ("package/.api/catalog.md",)))

    assert staged.roots == ("package/gen",)
    assert (stage / "package/gen/wire.ts").read_bytes() == b"generated"
    assert (stage / "package/.api/catalog.md").read_bytes() == b"catalog"
    assert not (stage / "package/node_modules").exists() and not (stage / "package/dist").exists()

    shutil.rmtree(stage)
    (generated / "package/gen/dep").symlink_to("../../../node_modules/.pnpm/dep")
    inside = assert_error_status(compose_image(repo, generated, stage, ("package/gen",), ("package/.api/catalog.md",)), RailStatus.FAULTED)
    assert "symlink escapes the transaction image" in inside.message and not stage.exists()


def test_the_image_walk_still_refuses_an_escape_that_lands_inside_an_owned_root(tmp_path: Path) -> None:
    """Narrowing what the transaction owns never softened the walk over what it owns."""
    repo, stage = tmp_path / "repo", tmp_path / "stage"
    _file(repo / "external/reachable", b"outside")

    def composes(witness: Path) -> None:
        shutil.rmtree(stage, ignore_errors=True)
        assert compose_image(repo, witness, stage, ("package/gen",), ()).is_ok()

    clean = tmp_path / "clean"
    _file(clean / "package/gen/wire.ts", b"generated")
    composes(clean)

    poisoned = tmp_path / "poisoned"
    _file(poisoned / "package/gen/wire.ts", b"generated")
    (poisoned / "package/gen/escape").symlink_to("../../../repo/external")

    refutes(poisoned, composes)


def test_generation_sources_refuse_missing_types_and_overlapping_transaction_scope(tmp_path: Path) -> None:
    stage = tmp_path / "stage"
    repo = tmp_path / "repo"
    _file(stage / "package", b"not-a-directory")
    image = GenerationImage(root=stage, roots=("package",))

    missing = assert_error_status(image.sources(repo, ("schema.json",)), RailStatus.FAULTED)
    overlap = assert_error_status(image.sources(repo, ("package/schema.json",)), RailStatus.FAULTED)

    assert "inputs are absent" in missing.message
    assert "scope is unsafe" in overlap.message


# --- [FRESHNESS]


def test_freshness_reports_symlinks_and_special_files_as_unsafe_evidence(tmp_path: Path) -> None:
    repo = tmp_path / "repo"
    scratch = tmp_path / "scratch"
    _file(repo / "generated/value", b"same")
    _file(scratch / "generated/value", b"same")
    (repo / "generated/alias").symlink_to("value")
    _fifo(scratch / "generated/pipe")

    rows, diff = freshness_rows(repo, scratch, ("generated",), 20)

    assert rows == (("special", "generated/alias"), ("special", "generated/pipe"))
    assert not diff

"""Typed fixture surface for quality-rail laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections import Counter
from dataclasses import dataclass, field
from pathlib import Path
from typing import TYPE_CHECKING

from expression import Error, Ok, Result
import msgspec
import pytest

from tools.quality.process import ProcessFault, RailStatus, Workspace
from tools.quality.rails import api as api_rail, bridge as bridge_rail, package as package_rail, static as static_rail, test as test_rail
from tools.quality.settings import ArtifactScope, QualitySettings


if TYPE_CHECKING:
    from collections.abc import Callable, Mapping


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class QualityHarness:
    root: Path
    settings: QualitySettings

    def scope(self, rail: str = "test", path: Path | None = None) -> ArtifactScope:
        scope_path = path or self.settings.artifact_root / "quality" / rail / self.settings.run_id
        (scope_path / "dotnet-cli").mkdir(parents=True, exist_ok=True)
        return ArtifactScope(root=self.root, rail=rail, scope_path=scope_path, dotnet_env=self.settings.dotnet_env(scope_path))

    def write(self, rel: str | Path, text: str = "") -> Path:
        path = self.root / Path(rel)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        return path

    def mkdir(self, rel: str | Path) -> Path:
        path = self.root / Path(rel)
        path.mkdir(parents=True, exist_ok=True)
        return path

    def project(self, rel: str | Path, xml: str = "<Project />") -> Path:
        return self.write(rel, xml)

    @staticmethod
    def json_object(value: object) -> dict[str, object]:
        match value:
            case dict() as decoded:
                return {str(key): item for key, item in decoded.items()}
            case _:
                raise AssertionError(f"expected JSON object payload, got {value!r}")

    @staticmethod
    def json_sequence(value: object) -> tuple[object, ...]:
        match value:
            case list() | tuple() as decoded:
                return tuple(decoded)
            case _:
                raise AssertionError(f"expected JSON array payload, got {value!r}")

    @staticmethod
    def expect[T](result: Result[bytes, ProcessFault], model: type[T]) -> T:
        assert result.is_ok(), result.swap().default_value(ProcessFault.fail("quality", "expect", detail=b"unknown")).message
        return msgspec.json.decode(result.default_value(b""), type=model)

    @staticmethod
    def expect_json(result: Result[bytes, ProcessFault]) -> dict[str, object]:
        assert result.is_ok(), result.swap().default_value(ProcessFault.fail("quality", "expect", detail=b"unknown")).message
        return QualityHarness.json_object(msgspec.json.decode(result.default_value(b"")))

    @staticmethod
    def envelope(capsys: pytest.CaptureFixture[str]) -> dict[str, object]:
        captured = capsys.readouterr()
        rows = captured.out.splitlines()
        assert len(rows) == 1
        assert captured.err.strip()
        return QualityHarness.json_object(msgspec.json.decode(rows[0]))


@dataclass(frozen=True, slots=True)
class RailProbe:
    calls: list[tuple[str, tuple[object, ...], dict[str, object]]] = field(default_factory=list)

    def install(self, monkeypatch: pytest.MonkeyPatch, owner: object, member: str, payload: object) -> None:
        def replacement(*args: object, **kwargs: object) -> Result[object, ProcessFault]:
            self.calls.append((member, args, kwargs))
            return Ok(payload)

        monkeypatch.setattr(owner, member, replacement)


@dataclass(frozen=True, slots=True)
class CliLaw:
    argv: tuple[str, ...]
    rail: str
    verb: str
    command_path: tuple[str, ...]
    owner: object
    member: str
    payload: object

    def install(self, monkeypatch: pytest.MonkeyPatch, probe: RailProbe) -> None:
        probe.install(monkeypatch, self.owner, self.member, self.payload)


@dataclass(frozen=True, slots=True)
class TargetLaw:
    mode: test_rail.TestMode
    mutation: test_rail.MutationMode
    all_targets: bool
    test_modules: str
    explicit: bool
    needle: bytes


@dataclass(frozen=True, slots=True)
class QualityWorkspaceDouble:
    root: Path
    changed_rows: tuple[str, ...] = ()
    projects_rows: tuple[str, ...] = ()
    owners: Mapping[str, str] = field(default_factory=dict)
    fail_graph: bool = False
    calls: Counter[str] = field(default_factory=Counter)

    def changed(self) -> Result[tuple[str, ...], ProcessFault]:
        self.calls["changed"] += 1
        return Ok(self.changed_rows)

    def index(self) -> Result[dict[Path, Path], ProcessFault]:
        self.calls["index"] += 1
        return self._graph_or(lambda: {self.root / Path(row).parent: self.root / row for row in self.projects_rows})

    def projects(self, _directory: str = ".") -> Result[tuple[str, ...], ProcessFault]:
        self.calls["projects"] += 1
        return self._graph_or(lambda: self.projects_rows)

    def paths(self, _args: tuple[str, ...], *, cwd: Path | None = None, suffix: str = "") -> Result[tuple[Path, ...], ProcessFault]:
        _ = cwd
        self.calls["paths"] += 1
        return Ok(tuple(Path(row) for row in self.changed_rows if not suffix or row.endswith(suffix)))

    def owner_rel(self, _index: dict[Path, Path], file: Path) -> str | None:
        self.calls["owner_rel"] += 1
        resolved = file.resolve()
        rel = resolved.relative_to(self.root.resolve()).as_posix() if resolved.is_relative_to(self.root.resolve()) else file.as_posix()
        return self.owners.get(rel)

    @staticmethod
    def csproj(project: Path, tag: str = "") -> object:
        return Workspace.csproj(project, tag)

    def _graph_or[T](self, action: Callable[[], T]) -> Result[T, ProcessFault]:
        match self.fail_graph:
            case True:
                return Error(ProcessFault.fail("workspace", "graph", detail=b"project graph must stay lazy"))
            case False:
                return Ok(action())


@dataclass(frozen=True, slots=True)
class ProjectGraph:
    projects: tuple[str, ...] = ("libs/A/A.csproj", "libs/B/B.csproj")
    sources: tuple[str, ...] = ("libs/A/A.cs",)
    references: tuple[tuple[str, tuple[str, ...]], ...] = (("libs/B/B.csproj", ("../A/A.csproj",)),)

    def materialize(self, quality: QualityHarness) -> None:
        refs = dict(self.references)
        for project in self.projects:
            body = "".join(f'<ProjectReference Include="{include}" />' for include in refs.get(project, ()))
            quality.project(project, f"<Project><ItemGroup>{body}</ItemGroup></Project>" if body else "<Project />")
        for source in self.sources:
            quality.write(source)

    def workspace(
        self, quality: QualityHarness, changed: tuple[str, ...] | None = None, owners: Mapping[str, str] | None = None, *, fail_graph: bool = False
    ) -> QualityWorkspaceDouble:
        owner_map = owners or dict.fromkeys(self.sources, self.projects[0])
        return QualityWorkspaceDouble(quality.root, self.sources if changed is None else changed, self.projects, owner_map, fail_graph)


@dataclass(frozen=True, slots=True)
class PackageShape:
    slug: str = "rasm-bridge"
    project: Path = Path("apps/bridge/plugin.csproj")
    assembly_name: str = "Rasm"
    target_ext: str = ".rhp"
    target_framework: str = "net10.0"
    package_pattern: str = "rasm-rh9_1-mac.yak"

    def props(self, meta: package_rail.PackageMeta) -> dict[str, str]:
        return {
            "AssemblyName": meta.assembly_name,
            "MSBuildProjectDirectory": str(meta.project_dir),
            "TargetDir": str(meta.target_dir),
            "TargetExt": meta.target_ext,
            "TargetFramework": meta.target_framework,
            "YakManifestDirectory": str(meta.manifest_dir),
            "YakPackageDirectory": str(meta.package_dir),
            "YakPackagePattern": meta.package_pattern,
            "YakPackageSlug": self.slug,
            "YakPath": str(meta.yak_path),
            "YakPlatform": meta.yak_platform,
            "YakPushSource": meta.yak_push_source,
        }

    @staticmethod
    def rebuild(meta: package_rail.PackageMeta) -> None:
        meta.target_dir.mkdir(parents=True, exist_ok=True)
        (meta.target_dir / f"{meta.assembly_name}{meta.target_ext}").write_text("", encoding="utf-8")
        (meta.target_dir / f"{meta.assembly_name}.dll").write_text("", encoding="utf-8")

    def materialize(self, quality: QualityHarness) -> package_rail.PackageMeta:
        yak = quality.write("yak", "#!/bin/sh\n")
        yak.chmod(0o755)
        project = quality.project(self.project, f"<Project><PropertyGroup><YakPackageSlug>{self.slug}</YakPackageSlug></PropertyGroup></Project>")
        target = project.parent / "bin" / quality.settings.configuration / self.target_framework
        target.mkdir(parents=True, exist_ok=True)
        quality.write(project.parent.relative_to(quality.root) / "manifest.yml", f"name: {self.slug}\n")
        quality.write(target.relative_to(quality.root) / f"{self.assembly_name}{self.target_ext}")
        quality.write(target.relative_to(quality.root) / f"{self.assembly_name}.dll")
        return package_rail.PackageMeta(
            project=project,
            manifest_dir=project.parent,
            target_dir=target,
            assembly_name=self.assembly_name,
            target_ext=self.target_ext,
            yak_path=yak,
            yak_platform="mac",
            yak_push_source="feed",
            package_dir=project.parent / "yak",
            package_pattern=self.package_pattern,
            target_framework=self.target_framework,
            project_dir=project.parent,
        )


# --- [LAW_TABLES] -----------------------------------------------------------------------


API_DOCTOR = api_rail.ApiDoctorReport(query={"op": "doctor"}, status="ok", rhino={}, ilspy={}, rhinocode={}, counts={}, artifact_paths={}, sources=())
CLI_LAWS: tuple[CliLaw, ...] = (
    CliLaw(
        ("static", "plan"),
        "static",
        "plan",
        ("static", "plan"),
        static_rail,
        "plan_payload",
        msgspec.json.encode(
            static_rail.StaticPlanReport(
                scope="changed",
                inputs=(),
                owners=(),
                projects=(),
                full_triggers=(),
                format_groups=(),
                commands={"fix": (), "report": (), "build": ()},
            )
        ),
    ),
    CliLaw(
        ("test", "list", "--format", "json"),
        "test",
        "list",
        ("test", "list"),
        test_rail,
        "list_tests_payload",
        msgspec.json.encode(test_rail.TestListReport(query={"op": "list"}, status="ok", count=1, returned=1, tests=("Law",), artifact_paths={})),
    ),
    CliLaw(("api", "doctor"), "api", "doctor", (), api_rail, "api", msgspec.json.encode(API_DOCTOR)),
    CliLaw(
        ("api", "show", "source.preview.cs", "--full"),
        "api",
        "show",
        (),
        api_rail,
        "api",
        msgspec.json.encode(
            api_rail.ApiShowReport(
                query={"op": "show"},
                status="ok",
                artifact=api_rail.ApiArtifact(id="a", kind="source.preview.cs", path="source.preview.cs"),
                preview="public sealed class X {}",
                counts={"selected_lines": 1, "preview_lines": 1},
                content="public sealed class X {}",
            )
        ),
    ),
    CliLaw(
        ("bridge", "verify", "scenario.verify.csx"),
        "bridge",
        "verify",
        ("bridge", "verify"),
        bridge_rail,
        "run_verify",
        bridge_rail.VerifyReport(
            summary=bridge_rail.VerifySummaryCounts(ok=1, failed=0, total=1),
            scenarios=(bridge_rail.VerifyScenario(name="scenario", status=RailStatus.OK),),
        ),
    ),
    CliLaw(
        ("bridge", "package", "plan", "rasm-bridge"),
        "package",
        "package-plan",
        ("bridge", "package", "plan"),
        package_rail,
        "package_plan_payload",
        msgspec.json.encode(
            package_rail.PackagePlanReport(
                query={"op": "package-plan"},
                status="ok",
                project="tools/rhino-bridge/plugin/Rasm.RhinoBridge.Plugin.csproj",
                meta={},
                artifact_paths={},
            )
        ),
    ),
)
TARGET_LAWS: tuple[TargetLaw, ...] = (
    TargetLaw("list", "changed", all_targets=False, test_modules="", explicit=False, needle=b"Mutation is only valid for test run."),
    TargetLaw("run", "changed", all_targets=True, test_modules="", explicit=False, needle=b"Mutation requires the default managed test target."),
    TargetLaw("run", "off", all_targets=True, test_modules="*.dll", explicit=False, needle=b"Choose exactly one"),
)


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def quality(tmp_path: Path) -> QualityHarness:
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    (tmp_path / ".config").mkdir()
    (tmp_path / ".config/dotnet-tools.json").write_text("{}", encoding="utf-8")
    target = tmp_path / "tests/csharp/libs/Rasm/Rasm.Tests.csproj"
    target.parent.mkdir(parents=True)
    target.write_text("<Project />", encoding="utf-8")
    return QualityHarness(tmp_path, QualitySettings(root=tmp_path, rhino_app=None, run_id="test-run"))

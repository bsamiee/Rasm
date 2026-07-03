"""Laws for the local provisioning rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

from expression import Ok
import msgspec
import pytest

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error_status, assert_ok
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.core.model import Claim, Fault, ProvisionRun, RailStatus, receipt
from tools.assay.rails import provision as provision_rail
from tools.assay.rails.provision import ProvisionParams


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.composition.settings import ArtifactScope, AssaySettings
    from tools.assay.core.model import Check, Completed, Report

# --- [OPERATIONS] -----------------------------------------------------------------------

_PROJECT = {"rootKey": "abc123", "projectKey": "forge-test-abc123", "instance": "default", "composeProject": "forge-forge-test-abc123-default"}


def _warning_rows(value: object) -> tuple[object, ...]:
    return value if isinstance(value, tuple) else tuple(value) if isinstance(value, list) else ()


def _string_keyed(value: object) -> dict[str, object]:
    return {key: row for key, row in value.items() if isinstance(key, str)} if isinstance(value, dict) else {}


def _resources(extra: dict[str, object]) -> tuple[dict[str, object], dict[str, object]]:
    legacy_resources = extra.pop("resources", {})
    runtime_resources: dict[str, object] = {}
    return (
        {
            "counts": legacy_resources if isinstance(legacy_resources, dict) else {},
            "owned": {"containers": extra.pop("containers", ()), "volumes": (), "networks": ()},
            "images": extra.pop("images", ()),
            "dockerDisk": extra.pop("dockerDisk", ()),
            "runtime": runtime_resources,
        },
        runtime_resources,
    )


def _json(command: str, **extra: object) -> bytes:
    warnings = tuple({"message": row} if isinstance(row, str) else row for row in _warning_rows(extra.pop("warnings", ())))
    resources, runtime_resources = _resources(extra)
    runtime_docker_present = None
    if isinstance(runtime := extra.pop("runtime", None), dict):
        runtime_docker = runtime.get("docker")
        if isinstance(runtime_docker, dict):
            runtime_docker_present = runtime_docker.get("present")
        runtime_resources.update(_string_keyed(runtime))
    if "docker" in extra:
        docker = extra.pop("docker")
        if isinstance(docker, dict) and runtime_docker_present is not None and "present" not in docker:
            docker = {**docker, "present": runtime_docker_present}
        runtime_resources["docker"] = docker
    for key in ("lock", "colima"):
        if key in extra:
            runtime_resources[key] = extra.pop(key)
    extensions = extra.pop("extensions", ())
    extension_carrier = {
        "catalog": extensions if command == "extensions" else (),
        "results": extensions if command in {"up", "check", "apply"} else (),
        "summary": extra.pop("summary", {}),
    }
    tools = extra.pop("tools", {"surfaces": {}, "summary": {}})
    artifacts = extra.pop("artifacts", {"generated": extra.pop("generated", ()), "plan": extra.pop("plan", None)})
    return msgspec.json.encode({
        "schemaVersion": 3,
        "command": command,
        "ok": True,
        "warnings": warnings,
        "error": None,
        "project": _PROJECT,
        "auth": extra.pop("auth", {}),
        "portPolicy": extra.pop("portPolicy", {}),
        "services": extra.pop("services", {}),
        "ports": extra.pop("ports", ()),
        "resources": resources,
        "artifacts": artifacts,
        "extensions": extension_carrier,
        "tools": tools,
        **extra,
    })


def _stdout(command: tuple[str, ...]) -> bytes:
    match command:
        case ("forge-provision", "--json", verb):
            return _json(verb)
        case ("forge-scientific-env", "python3", *_):
            return b"cpython-315 0\n"
        case ("forge-scientific-env", "pkg-config", "--modversion", "openblas"):
            return b"0.3.30\n"
        case ("forge-scientific-env", "sh", "-lc", *_):
            return b"present:libonnxruntime.dylib\n"
        case _:
            return b""


def _recording_fan(calls: list[tuple[tuple[str, ...], ...]]) -> Callable[..., tuple[Result[Completed, Fault], ...]]:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        commands = tuple(check.tool.command for check in checks)
        calls.append(commands)
        return tuple(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_stdout(command))) for command in commands)

    return fan


def _fan_payload(command: tuple[str, ...], stdout: bytes, *, rc: int = 0) -> Callable[..., tuple[Result[Completed, Fault], ...]]:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        assert tuple(check.tool.command for check in checks) == (command,)
        return (Ok(receipt(command, rc, stdout=stdout)),)

    return fan


def _call(
    handler: Callable[[AssaySettings, ArtifactScope, ProvisionParams, SeamExecutor], Result[Report, Fault]],
    assay_root: AssayHarness,
    executor: SeamExecutor,
) -> Result[Report, Fault]:
    return handler(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor)


# --- [PROVISION_PARAMS]


def test_provisionparams_arity_is_zero() -> None:
    """Provision commands do not accept positional tokens."""
    assert ProvisionParams()._arity("apply") == 0
    surplus = ProvisionParams(paths=("extra",)).bound("apply")
    assert isinstance(surplus, Fault)
    assert surplus.status is RailStatus.FAULTED


register_law(ProvisionParams, "arity_is_zero")


_STACK_VERBS = (
    ("up", provision_rail.up, (("forge-provision", "--json", "up"),)),
    ("down", provision_rail.down, (("forge-provision", "--json", "down"),)),
    ("status", provision_rail.status, (("forge-provision", "--json", "status"),)),
    ("doctor", provision_rail.doctor, (("forge-provision", "--json", "doctor"),)),
    ("ports", provision_rail.ports, (("forge-provision", "--json", "ports"),)),
    ("inventory", provision_rail.inventory, (("forge-provision", "--json", "inventory"),)),
    ("extensions", provision_rail.extensions, (("forge-provision", "--json", "extensions"),)),
    ("plan", provision_rail.plan, (("forge-provision", "--json", "plan"),)),
    ("env", provision_rail.env, (("forge-provision", "--json", "env"),)),
    ("apply", provision_rail.apply, (("forge-provision", "--json", "apply"),)),
)


@pytest.mark.parametrize("verb, handler, expected", _STACK_VERBS, ids=[row[0] for row in _STACK_VERBS])
def test_provision_stack_verb_delegates(
    assay_root: AssayHarness,
    verb: str,
    handler: Callable[[AssaySettings, ArtifactScope, ProvisionParams, SeamExecutor], Result[Report, Fault]],
    expected: tuple[tuple[str, ...], ...],
) -> None:
    """Stack verbs delegate to the Forge-owned provisioning command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    report = assert_ok(_call(handler, assay_root, SeamExecutor(fan_fn=_recording_fan(calls))))
    assert (report.claim, report.verb) == (Claim.PROVISION, verb)
    assert calls == [expected]


for _verb, _handler, _expected in _STACK_VERBS:
    register_law(_handler, f"delegates_to_stack_{_verb}")


def test_provision_status_projects_json_detail(assay_root: AssayHarness) -> None:
    """Status JSON is admitted into structured provisioning evidence."""
    payload = _json(
        "status",
        state="present",
        warnings=["port range narrowed"],
        auth={"mode": "auto-root", "risk": "generated-root-secret", "user": "postgres", "credential": "managed-hidden"},
        portPolicy={"mode": "auto", "source": "auto", "range": "15364-15554", "seedFingerprint": "seed-hash"},
        dockerAvailable=True,
        summary={"ok": 2},
        resources={"containers": 2},
        probes={"gdal": "present"},
        services={
            "timescale": {
                "enabled": True,
                "connectable": True,
                "role": "time",
                "profile": "timescale",
                "host": "127.0.0.1",
                "port": 15432,
                "portEnv": "FORGE_PROVISION_TIMESCALE_PORT",
                "containerPort": 5432,
                "dsnEnv": "FORGE_PROVISION_TIMESCALE_DSN",
                "dsnRedacted": "postgres://postgres:***@127.0.0.1:15432/forge",
                "composeService": "timescale",
                "image": "timescale/timescaledb-ha:pg18",
            }
        },
        ports=[
            {
                "service": "search",
                "env": "FORGE_PROVISION_SEARCH_PORT",
                "value": 15433,
                "state": "free",
                "occupied": False,
                "ownerClass": "none",
                "portSource": "auto",
            },
            {
                "service": "timescale",
                "env": "FORGE_PROVISION_TIMESCALE_PORT",
                "value": 15432,
                "state": "busy",
                "occupied": True,
                "ownerClass": "provision:this-project",
                "portSource": "auto",
            },
        ],
    )
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload))
    report = assert_ok(provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.facts == (("schemaVersion", "3"), ("ok", "true"), ("state", "present"))
    assert report.detail.schema_version == 3
    assert report.detail.ok is True
    assert report.detail.warnings == ("port range narrowed",)
    assert report.detail.auth_mode == "auto-root"
    assert report.detail.auth_risk == "generated-root-secret"
    assert report.detail.port_policy == (("mode", "auto"), ("source", "auto"), ("range", "15364-15554"), ("seedFingerprint", "seed-hash"))
    assert report.detail.provision_scope == (
        ("rootKey", "abc123"),
        ("projectKey", "forge-test-abc123"),
        ("instance", "default"),
        ("composeProject", "forge-forge-test-abc123-default"),
        ("authMode", "auto-root"),
        ("authRisk", "generated-root-secret"),
    )
    assert report.detail.resource_counts == (("containers", 2),)
    assert report.detail.summary == (("ok", 2),)
    assert report.detail.services == (("timescale", "true", "timescale", "15432", "timescale/timescaledb-ha:pg18"),)
    assert report.detail.service_connections == (
        (
            "timescale",
            "true",
            "127.0.0.1",
            "15432",
            "FORGE_PROVISION_TIMESCALE_PORT",
            "FORGE_PROVISION_TIMESCALE_DSN",
            "postgres://postgres:***@127.0.0.1:15432/forge",
            "5432",
            "timescale",
        ),
    )
    assert report.detail.service_roles == (("timescale", "time"),)
    assert report.detail.local_service_topology == (("timescale", "true", "timescale", "timescale/timescaledb-ha:pg18", "15432", "", ""),)
    assert report.detail.ports == (
        ("search", "FORGE_PROVISION_SEARCH_PORT", "15433", "free", "false", "none", "auto"),
        ("timescale", "FORGE_PROVISION_TIMESCALE_PORT", "15432", "busy", "true", "provision:this-project", "auto"),
    )
    assert report.detail.local_probe_values == ()


register_law(provision_rail.status, "projects_json_detail")


def test_provision_port_policy_allows_null_seed_fingerprint(assay_root: AssayHarness) -> None:
    """Lifecycle payloads may omit auto-allocation seed evidence."""
    payload = _json("down", portPolicy={"mode": "auto", "source": "current-manifest", "range": "15364-15554", "seedFingerprint": None})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "down"), payload))
    report = assert_ok(provision_rail.down(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.port_policy == (("mode", "auto"), ("source", "current-manifest"), ("range", "15364-15554"))


register_law(provision_rail.down, "allows_null_port_seed_fingerprint")


def test_provision_extensions_projects_catalog_rows(assay_root: AssayHarness) -> None:
    """Extension catalog rows stay separate from verification state rows."""
    payload = _json(
        "extensions",
        extensions=[
            {
                "service": "timescale",
                "extension": "postgis",
                "category": "geospatial",
                "required": True,
                "createOnApply": True,
                "riskClass": "local-extension",
                "sourcePackage": "image:timescale/timescaledb-ha",
                "preloadRequired": False,
                "requiresSuperuser": True,
                "requiresSharedPreload": True,
                "fileAccess": True,
                "networkAccess": True,
                "backgroundWorker": True,
                "createPolicy": "apply-create",
                "sourceRoute": "image:timescale/timescaledb-ha",
                "sourceKind": "image",
                "nixStatus": "runtime-probed",
                "probeKind": "create-extension",
                "capabilityRank": "required",
                "externalAccess": "none",
                "restartClass": "none",
                "serviceProfile": "timescale",
                "imageTag": "timescale/timescaledb-ha:pg18.4-ts2.27.2-all",
                "loadPolicy": "apply-create",
            }
        ],
    )
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "extensions"), payload))
    report = assert_ok(provision_rail.extensions(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.extensions == ()
    assert report.detail.extension_catalog == (
        ("timescale", "postgis", "geospatial", "required", "apply-create", "local-extension", "image:timescale/timescaledb-ha", "false"),
    )
    assert report.detail.extension_metadata == (
        (
            "timescale",
            "postgis",
            "image:timescale/timescaledb-ha",
            "image",
            "runtime-probed",
            "create-extension",
            "required",
            "none",
            "none",
            "timescale",
            "timescale/timescaledb-ha:pg18.4-ts2.27.2-all",
            "apply-create",
        ),
    )
    assert report.detail.extension_requirements == (("timescale", "postgis", "true", "true", "true", "true", "true"),)


register_law(provision_rail.extensions, "projects_catalog_rows")


def test_provision_extensions_admits_runtime_probed_null_fields(assay_root: AssayHarness) -> None:
    """Runtime-probed catalog rows carry null availability/admission/loadName/probeFunction/imageTag; the decode admits them as empty wire cells."""
    payload = _json(
        "extensions",
        extensions=[
            {
                "service": "*",
                "extension": "hll",
                "category": "analytics",
                "kind": "extension",
                "sourceRoute": "postgresql-contrib-or-image-probed",
                "sourceKind": "runtime-probed",
                "nixStatus": "runtime-probed",
                "probeKind": "create-extension",
                "capabilityRank": "optional",
                "createPolicy": "probe-only",
                "loadPolicy": "probe-only",
                "imageTag": None,
                "availability": None,
                "admission": None,
                "loadName": None,
                "probeFunction": None,
            }
        ],
    )
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "extensions"), payload))
    report = assert_ok(provision_rail.extensions(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    probed_row = ("*", "hll", "postgresql-contrib-or-image-probed", "runtime-probed", "runtime-probed", "create-extension", "optional")
    assert report.detail.extension_metadata == ((*probed_row, "", "", "", "", "probe-only"),)
    assert report.detail.tool_surface_extensions == ()


register_law(provision_rail.extensions, "admits_runtime_probed_null_fields")


def test_provision_extensions_preserves_tool_surface_rows(assay_root: AssayHarness) -> None:
    """DuckDB and SQLite catalog metadata survives projection into ProvisionRun."""
    payload = _json(
        "extensions",
        extensions=[
            {
                "service": "duckdb",
                "extension": "postgres_scanner",
                "aliases": ["postgres"],
                "kind": "tool-extension",
                "surface": "duckdb",
                "database": "duckdb",
                "availability": "core-loadable",
                "admission": "catalog-only",
                "profile": "duckdb-tooling",
                "loadPolicy": "catalog-only",
                "createPolicy": "catalog-only",
                "sourceRoute": "duckdb:postgres_scanner",
                "loadName": "postgres_scanner",
            },
            {
                "service": "sqlite",
                "extension": "sqlite-vec",
                "kind": "tool-extension",
                "surface": "sqlite-forge",
                "database": "sqlite",
                "availability": "active-local",
                "admission": "loaded-by-sqlite-forge",
                "profile": "safe",
                "loadPolicy": "loaded-by-sqlite-forge",
                "createPolicy": "loaded-by-sqlite-forge",
                "sourceRoute": "sqlite-forge:sqlite-vec",
                "loadName": "vec0",
                "probeFunction": "vec_version",
            },
        ],
    )
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "extensions"), payload))
    report = assert_ok(provision_rail.extensions(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.tool_surface_extensions == (
        (
            "duckdb",
            "postgres_scanner",
            "duckdb",
            "duckdb",
            "core-loadable",
            "catalog-only",
            "duckdb-tooling",
            "catalog-only",
            "catalog-only",
            "duckdb:postgres_scanner",
            "postgres_scanner",
            "",
            "postgres",
        ),
        (
            "sqlite",
            "sqlite-vec",
            "sqlite-forge",
            "sqlite",
            "active-local",
            "loaded-by-sqlite-forge",
            "safe",
            "loaded-by-sqlite-forge",
            "loaded-by-sqlite-forge",
            "sqlite-forge:sqlite-vec",
            "vec0",
            "vec_version",
            "",
        ),
    )


register_law(provision_rail.extensions, "preserves_tool_surface_rows")


def test_provision_doctor_projects_sanitized_runtime(assay_root: AssayHarness) -> None:
    """Doctor JSON projects runtime facts without paths, sockets, or helper names."""
    payload = _json(
        "doctor",
        docker={
            "executablePresent": True,
            "executableKind": "nix-store",
            "policy": {"status": "ok", "reason": None},
            "endpointKind": "unix",
            "compose": "Docker Compose version v2.39.0",
            "server": "29.0.0",
            "hostConfig": {"credentialHelperPresent": True, "warning": "credential-helper-present-for-host-config"},
            "anonymousPullConfig": {"exists": True},
        },
        runtime={
            "forgeProvision": {"present": True, "schemaVersion": 3},
            "docker": {"present": True},
            "compose": {"present": True, "version": "Docker Compose version v2.39.0"},
            "jq": {"present": True},
            "listenerProbeMethod": "lsof",
            "anonymousDockerConfig": True,
            "hostCredentialHelperPresent": True,
        },
        lock={"present": True, "active": False, "state": "ownerless", "pidAlive": False, "heartbeatStale": False},
        colima={"available": True, "status": {"running": True, "runtime": "docker", "arch": "aarch64"}},
    )
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "doctor"), payload))
    report = assert_ok(provision_rail.doctor(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert dict(report.detail.doctor) == {
        "dockerPolicyStatus": "ok",
        "dockerEndpointKind": "unix",
        "dockerExecutablePresent": "true",
        "dockerExecutableKind": "nix-store",
        "dockerComposeVersion": "Docker Compose version v2.39.0",
        "dockerServerVersion": "29.0.0",
        "anonymousPullConfig": "true",
        "credentialHelperPresent": "true",
        "runtimeForgeProvisionPresent": "true",
        "runtimeForgeProvisionSchemaVersion": "3",
        "runtimeDockerPresent": "true",
        "runtimeComposePresent": "true",
        "runtimeComposeVersion": "Docker Compose version v2.39.0",
        "runtimeJqPresent": "true",
        "runtimeListenerProbeMethod": "lsof",
        "runtimeAnonymousDockerConfig": "true",
        "runtimeHostCredentialHelperPresent": "true",
        "lockState": "ownerless",
        "lockPresent": "true",
        "lockActive": "false",
        "lockPidAlive": "false",
        "lockHeartbeatStale": "false",
        "colimaAvailable": "true",
        "colimaRunning": "true",
        "colimaRuntime": "docker",
        "colimaArch": "aarch64",
    }


register_law(provision_rail.doctor, "projects_sanitized_runtime")


def test_provision_json_verbs_fault_on_malformed_success_json(assay_root: AssayHarness) -> None:
    """Successful JSON-backed commands must emit valid Forge JSON."""
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), b"{not-json"))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "invalid forge-provision JSON" in fault.message


register_law(provision_rail.status, "faults_on_malformed_success_json")


def test_provision_json_verbs_fault_on_log_framed_stdout(assay_root: AssayHarness) -> None:
    """Forge JSON mode is one stdout object, not log lines plus a final object."""
    payload = b"log line\n" + _json("status")
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "expected exactly one forge-provision JSON object" in fault.message


register_law(provision_rail.status, "faults_on_log_framed_stdout")


def test_provision_rejects_schema_v1_payload(assay_root: AssayHarness) -> None:
    """Schema v1 Forge payloads are outside the schema-v3 contract."""
    payload = msgspec.json.encode({"schemaVersion": 1, "command": "status", "ok": True})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "unsupported forge-provision schema" in fault.message


register_law(provision_rail.status, "rejects_schema_v1_payload")


def test_provision_rejects_schema_v2_payload(assay_root: AssayHarness) -> None:
    """Schema v2 Forge payloads are intentionally retired."""
    payload = msgspec.json.encode({"schemaVersion": 2, "command": "status", "ok": True})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "unsupported forge-provision schema" in fault.message


register_law(provision_rail.status, "rejects_schema_v2_payload")


def test_provision_rejects_stale_schema_v3_package_shape(assay_root: AssayHarness) -> None:
    """Older schema-v3 Forge payloads get a redeploy-focused adapter fault."""
    payload = msgspec.json.encode({"schemaVersion": 3, "command": "status", "ok": True, "warnings": []})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "installed forge-provision is older than the Rasm provision adapter" in fault.message
    assert "uv run python -m tools.assay provision status" in fault.message


register_law(provision_rail.status, "rejects_stale_schema_v3_package_shape")


def test_provision_rejects_ok_false_without_structured_error(assay_root: AssayHarness) -> None:
    """Schema v3 ok:false payloads must carry structured error data."""
    payload = msgspec.json.encode({"schemaVersion": 3, "command": "status", "ok": False})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload, rc=1))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "ok:false requires error.code" in fault.message


register_law(provision_rail.status, "rejects_ok_false_without_structured_error")


def test_provision_rejects_schema_v3_payload_without_project(assay_root: AssayHarness) -> None:
    """Forge schema-v3 payloads must preserve project identity at the Assay boundary."""
    payload = msgspec.json.decode(_json("status"))
    assert isinstance(payload, dict)
    payload.pop("project")
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), msgspec.json.encode(payload)))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "missing project object" in fault.message


register_law(provision_rail.status, "rejects_schema_v3_payload_without_project")


def test_provision_ok_false_projects_project_identity(assay_root: AssayHarness) -> None:
    """Structured Forge failures still project the owning root/project/instance tuple."""
    payload = _json("status", ok=False, error={"code": "port-conflict", "message": "fixed port is busy", "exitCode": 1})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload, rc=1))
    report = assert_ok(provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.provision_scope[:4] == (
        ("rootKey", "abc123"),
        ("projectKey", "forge-test-abc123"),
        ("instance", "default"),
        ("composeProject", "forge-forge-test-abc123-default"),
    )


register_law(provision_rail.status, "ok_false_projects_project_identity")


def test_provision_rejects_sensitive_payload_keys(assay_root: AssayHarness) -> None:
    """Assay refuses Forge JSON that contains secret-shaped keys even when the command otherwise succeeds."""
    sensitive = {"token": "redacted"}
    payload = _json("status", services={"timescale": {"enabled": True}}, **sensitive)
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "sensitive key" in fault.message


register_law(provision_rail.status, "rejects_sensitive_payload_keys")


def test_provision_rejects_sensitive_payload_values(assay_root: AssayHarness) -> None:
    """Assay refuses Forge JSON that contains credential-shaped values under otherwise-safe keys."""
    payload = _json("status", error={"code": "x", "message": "postgres://postgres:pw@127.0.0.1/forge", "exitCode": 1}, ok=False)
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "sensitive value" in fault.message


register_law(provision_rail.status, "rejects_sensitive_payload_values")


@pytest.mark.parametrize(
    "value",
    [
        "/Users/example/.docker/config.json",
        "/" + "tmp/forge/socket",
        "/run/secrets/postgres",
        "/etc/passwd",
        "/var/run/docker.sock",
        "unix:///tmp/forge.sock",
    ],
    ids=("users", "tmp", "run-secrets", "etc", "var-run", "unix"),
)
def test_provision_rejects_local_path_payload_values(assay_root: AssayHarness, value: str) -> None:
    """Assay refuses safe-key Forge values that carry absolute local paths."""
    payload = _json("doctor", docker={"executableKind": value})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "doctor"), payload))
    fault = assert_error_status(
        provision_rail.doctor(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "sensitive value" in fault.message


register_law(provision_rail.doctor, "rejects_local_path_payload_values")


def test_provision_allows_redacted_dsn_values(assay_root: AssayHarness) -> None:
    """Redacted local DSNs remain safe connection evidence."""
    payload = _json(
        "env",
        services={
            "timescale": {
                "enabled": True,
                "connectable": True,
                "host": "127.0.0.1",
                "port": 15432,
                "portEnv": "FORGE_PROVISION_TIMESCALE_PORT",
                "dsnEnv": "FORGE_PROVISION_TIMESCALE_DSN",
                "dsnRedacted": "postgres://postgres:***@127.0.0.1:15432/forge",
                "containerPort": 5432,
                "composeService": "timescale",
            }
        },
    )
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "env"), payload))
    report = assert_ok(provision_rail.env(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.service_connections[0][6] == "postgres://postgres:***@127.0.0.1:15432/forge"


register_law(provision_rail.env, "allows_redacted_dsn_values")


@pytest.mark.parametrize(
    "payload, message",
    [
        ({"schemaVersion": 3, "ok": True}, "missing command"),
        ({"schemaVersion": 3, "command": "", "ok": True}, "missing command"),
        ({"schemaVersion": 3, "command": "doctor", "ok": True}, "forge-provision JSON command"),
    ],
    ids=("missing", "empty", "wrong"),
)
def test_provision_rejects_command_mismatch(assay_root: AssayHarness, payload: dict[str, object], message: str) -> None:
    """JSON-backed provision verbs require the Forge command field to match the requested verb."""
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), msgspec.json.encode(payload)))
    fault = assert_error_status(
        provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert message in fault.message


register_law(provision_rail.status, "rejects_command_mismatch")


def test_provision_ok_false_projects_failed_run(assay_root: AssayHarness) -> None:
    """Forge ok:false JSON is a completed provisioning result, not an adapter parse fault."""
    payload = _json("status", ok=False, error={"code": "port-conflict", "message": "fixed port is busy", "exitCode": 1})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload, rc=1))
    report = assert_ok(provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert report.status is RailStatus.FAILED
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.ok is False
    assert report.detail.error == (("code", "port-conflict"), ("message", "fixed port is busy"), ("exitCode", "1"))
    assert not report.artifacts


register_law(provision_rail.status, "ok_false_projects_failed_run")


def test_provision_ok_false_exit_zero_projects_failed_run(assay_root: AssayHarness) -> None:
    """Forge ok:false controls report failure even when the process exits zero."""
    payload = _json("status", ok=False, error={"code": "port-conflict", "message": "fixed port is busy", "exitCode": 0})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload, rc=0))
    report = assert_ok(provision_rail.status(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert report.status is RailStatus.FAILED
    assert report.counts.failed == 1
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.ok is False


register_law(provision_rail.status, "ok_false_exit_zero_projects_failed_run")


def test_provision_plan_projects_safe_plan_summary(assay_root: AssayHarness) -> None:
    """Plan keeps bounded scalar metadata and never projects raw Compose YAML."""
    payload = _json("plan", artifacts={"generated": (), "plan": {"composeYaml": "redacted", "authMode": "auto-root", "serviceCount": 2}})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "plan"), payload))
    report = assert_ok(provision_rail.plan(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.plan_summary == (("authMode", "auto-root"), ("serviceCount", "2"))
    assert "composeYaml" not in dict(report.detail.plan_summary)


register_law(provision_rail.plan, "projects_safe_plan_summary")


def test_provision_check_folds_stack_and_local_probes(assay_root: AssayHarness) -> None:
    """The check verb runs stack evidence plus local runtime probes."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    executor = SeamExecutor(fan_fn=_recording_fan(calls))
    report = assert_ok(provision_rail.check(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    commands = calls[0]
    assert report.claim is Claim.PROVISION
    assert report.verb == "check"
    assert ("forge-provision", "--json", "check") in commands
    assert ("forge-provision", "--json", "tools") in commands
    assert ("duckdb", "--version") not in commands
    assert any(command[:2] == ("forge-scientific-env", "python3") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "pkg-config", "--modversion") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "sh", "-lc") for command in commands)
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.local_probes == (("forge-python-abi", "ok"), ("forge-openblas", "ok"), ("forge-onnxruntime-lib", "ok"))
    assert report.detail.local_probe_values[-1] == ("forge-onnxruntime-lib", "ok", "present:libonnxruntime.dylib")


register_law(provision_rail.check, "folds_stack_and_local_probes")


def test_provision_check_keeps_tools_success_when_stack_check_fails(assay_root: AssayHarness) -> None:
    """The merged check detail may fail while the independent tools subprocess remains successful."""

    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        outcomes = []
        for check in checks:
            command = check.tool.command
            if command == ("forge-provision", "--json", "check"):
                outcomes.append(
                    Ok(
                        receipt(
                            command,
                            1,
                            status=RailStatus.FAILED,
                            stdout=_json("check", ok=False, error={"code": "error", "message": "owned service is not running", "exitCode": 1}),
                        )
                    )
                )
            elif command == ("forge-provision", "--json", "tools"):
                outcomes.append(
                    Ok(
                        receipt(
                            command,
                            0,
                            status=RailStatus.OK,
                            stdout=_json(
                                "tools",
                                tools={
                                    "surfaces": {"duckdb": {"ok": True, "executable": "duckdb", "probe": {"extensionRows": 31}}},
                                    "summary": {"selected": "all", "ok": True},
                                },
                            ),
                        )
                    )
                )
            else:
                outcomes.append(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_stdout(command))))
        return tuple(outcomes)

    executor = SeamExecutor(fan_fn=fan)
    report = assert_ok(provision_rail.check(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    assert report.status is RailStatus.FAILED
    assert isinstance(report.detail, ProvisionRun)
    assert report.detail.ok is False
    assert report.detail.tool_surfaces == (("duckdb", "ok", "31"),)
    assert report.detail.tool_summary == (("ok", "true"), ("selected", "all"))
    result_statuses = {row.id: row.severity for row in report.results}
    assert result_statuses["forge-provision --json check"] == RailStatus.FAILED.value
    assert result_statuses.get("forge-provision --json tools", RailStatus.OK.value) == RailStatus.OK.value


register_law(provision_rail.check, "keeps_tools_success_when_stack_check_fails")


def test_provision_check_scrubs_failed_local_probe_streams(assay_root: AssayHarness) -> None:
    """Failed local probes keep a bounded failure row without durable raw stdout/stderr."""

    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        outcomes = []
        for check in checks:
            command = check.tool.command
            if command == ("forge-provision", "--json", "check"):
                outcomes.append(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_json("check"))))
            elif command == ("forge-scientific-env", "pkg-config", "--modversion", "openblas"):
                outcomes.append(
                    Ok(receipt(command, 1, status=RailStatus.FAILED, stderr=b"POSTGRES_PASSWORD=leak postgres://postgres:pw@127.0.0.1/forge"))
                )
            else:
                outcomes.append(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_stdout(command))))
        return tuple(outcomes)

    executor = SeamExecutor(fan_fn=fan)
    report = assert_ok(provision_rail.check(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor))
    encoded = msgspec.json.encode(report)
    assert report.status is RailStatus.FAILED
    assert b"POSTGRES_PASSWORD" not in encoded
    assert b"postgres://postgres:pw" not in encoded
    assert b"provision probe failed: forge-openblas" in encoded


register_law(provision_rail.check, "scrubs_failed_local_probe_streams")


def test_provision_check_rejects_sensitive_success_local_probe_values(assay_root: AssayHarness) -> None:
    """Successful local probe output must not bypass Forge payload sanitizers."""

    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        outcomes = []
        for check in checks:
            command = check.tool.command
            if command == ("forge-provision", "--json", "check"):
                outcomes.append(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_json("check"))))
            elif command == ("forge-scientific-env", "pkg-config", "--modversion", "openblas"):
                outcomes.append(Ok(receipt(command, 0, status=RailStatus.OK, stdout=b"/nix/store/leak/libopenblas.dylib\n")))
            else:
                outcomes.append(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_stdout(command))))
        return tuple(outcomes)

    executor = SeamExecutor(fan_fn=fan)
    fault = assert_error_status(
        provision_rail.check(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "sensitive local probe value" in fault.message


register_law(provision_rail.check, "rejects_sensitive_success_local_probe_values")


def test_provision_doctor_rejects_diagnostic_json_paths(assay_root: AssayHarness) -> None:
    """Forge diagnostic JSON is not admitted into the Assay-safe provision channel."""
    payload = _json("doctor", diagnostic={"resolvedEndpoint": "unix:///Users/example/.colima/default/docker.sock"})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "doctor"), payload, rc=0))
    fault = assert_error_status(
        provision_rail.doctor(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor), RailStatus.FAULTED
    )
    assert "sensitive value" in fault.message


register_law(provision_rail.doctor, "rejects_diagnostic_json_paths")

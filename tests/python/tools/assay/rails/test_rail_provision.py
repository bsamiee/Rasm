"""Laws for the local provisioning rail."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

from expression import Ok
import msgspec
import pytest

from tests.python._testkit.spec import assert_error_status, assert_ok
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.core.model import Claim, Fault, ProvisionRun, RailStatus, receipt
from tools.assay.rails import provision as provision_rail
from tools.assay.rails.provision import ProvisionParams


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.composition.settings import AssaySettings
    from tools.assay.composition.store import ArtifactScope
    from tools.assay.core.model import Check, Completed, Report


# --- [CONSTANTS] ------------------------------------------------------------------------

_STACK_VERB_NAMES = ("up", "down", "status", "doctor", "ports", "inventory", "extensions", "plan", "env", "apply")

COVERS: tuple[object, ...] = (ProvisionParams, provision_rail.check, *(getattr(provision_rail, verb) for verb in _STACK_VERB_NAMES))

_PROJECT = {"rootKey": "abc123", "projectKey": "forge-test-abc123", "instance": "default", "composeProject": "forge-forge-test-abc123-default"}

# --- [OPERATIONS] -----------------------------------------------------------------------


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
        commands = tuple(check.args.fill(check.tool.command) for check in checks)
        calls.append(commands)
        return tuple(Ok(receipt(command, 0, status=RailStatus.OK, stdout=_stdout(command))) for command in commands)

    return fan


def _fan_payload(command: tuple[str, ...], stdout: bytes, *, rc: int = 0) -> Callable[..., tuple[Result[Completed, Fault], ...]]:
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        assert tuple(check.args.fill(check.tool.command) for check in checks) == (command,)
        return (Ok(receipt(command, rc, stdout=stdout)),)

    return fan


def _override_fan(overrides: dict[tuple[str, ...], Result[Completed, Fault]]) -> Callable[..., tuple[Result[Completed, Fault], ...]]:
    # Canned check-verb fan: overridden commands play their outcome; every other command succeeds with its default stdout.
    def fan(checks: tuple[Check, ...], **_kw: object) -> tuple[Result[Completed, Fault], ...]:
        commands = tuple(check.args.fill(check.tool.command) for check in checks)
        return tuple(overrides.get(command, Ok(receipt(command, 0, status=RailStatus.OK, stdout=_stdout(command)))) for command in commands)

    return fan


def _run(
    handler: Callable[[AssaySettings, ArtifactScope, ProvisionParams, SeamExecutor], Result[Report, Fault]],
    assay_root: AssayHarness,
    executor: SeamExecutor,
) -> Result[Report, Fault]:
    return handler(assay_root.settings, assay_root.scope(Claim.PROVISION), ProvisionParams(), executor)


def _detail(report: Report) -> ProvisionRun:
    assert isinstance(report.detail, ProvisionRun)
    return report.detail


# --- [PROVISION_PARAMS]


def test_provisionparams_arity_is_zero() -> None:
    """Provision commands accept no positional tokens; any path is surplus."""
    surplus = ProvisionParams(paths=("extra",)).bound("apply")
    assert isinstance(surplus, Fault)
    assert surplus.status is RailStatus.FAULTED


# --- [STACK_DELEGATION]

_STACK_VERBS = tuple((verb, getattr(provision_rail, verb)) for verb in _STACK_VERB_NAMES)


@pytest.mark.parametrize("verb, handler", _STACK_VERBS, ids=_STACK_VERB_NAMES)
def test_provision_stack_verb_delegates(
    assay_root: AssayHarness, verb: str, handler: Callable[[AssaySettings, ArtifactScope, ProvisionParams, SeamExecutor], Result[Report, Fault]]
) -> None:
    """Stack verbs delegate to the Forge-owned provisioning command."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    report = assert_ok(_run(handler, assay_root, SeamExecutor(fan_fn=_recording_fan(calls))))
    assert (report.claim, report.verb) == (Claim.PROVISION, verb)
    assert calls == [(("forge-provision", "--json", verb),)]


# --- [FAULT_MATRIX]


def _frow(
    ident: str, payload: bytes | dict[str, object], *fragments: str, verb: str = "status", rc: int = 0
) -> tuple[str, str, bytes, int, tuple[str, ...]]:
    body = payload if isinstance(payload, bytes) else msgspec.json.encode(payload)
    return (ident, verb, body, rc, fragments)


_STALE_V3 = "installed forge-provision is older than the Rasm provision adapter"

# One adapter-boundary fault table: wire framing, schema vintage, command identity, and egress sanitizing.
_FAULT_ROWS = (
    _frow("malformed-json", b"{not-json", "invalid forge-provision JSON"),
    _frow("log-framed-stdout", b"log line\n" + _json("status"), "expected exactly one forge-provision JSON object"),
    _frow("schema-v1", {"schemaVersion": 1, "command": "status", "ok": True}, "unsupported forge-provision schema"),
    _frow("schema-v2", {"schemaVersion": 2, "command": "status", "ok": True}, "unsupported forge-provision schema"),
    _frow(
        "stale-schema-v3",
        {"schemaVersion": 3, "command": "status", "ok": True, "warnings": []},
        _STALE_V3,
        "uv run python -m tools.assay provision status",
    ),
    _frow("ok-false-without-error", {"schemaVersion": 3, "command": "status", "ok": False}, "ok:false requires error.code", rc=1),
    _frow("missing-project", {k: v for k, v in msgspec.json.decode(_json("status")).items() if k != "project"}, "missing project object"),
    _frow("command-missing", {"schemaVersion": 3, "ok": True}, "missing command"),
    _frow("command-empty", {"schemaVersion": 3, "command": "", "ok": True}, "missing command"),
    _frow("command-mismatch", {"schemaVersion": 3, "command": "doctor", "ok": True}, "forge-provision JSON command"),
    _frow("sensitive-key", _json("status", services={"timescale": {"enabled": True}}, token="redacted"), "sensitive key"),  # ruff:ignore[hardcoded-password-func-arg]  # canned secret-shaped probe
    _frow(
        "sensitive-value",
        _json("status", error={"code": "x", "message": "postgres://postgres:pw@127.0.0.1/forge", "exitCode": 1}, ok=False),
        "sensitive value",
    ),
    _frow(
        "diagnostic-json-path",
        _json("doctor", diagnostic={"resolvedEndpoint": "unix:///Users/example/.colima/default/docker.sock"}),
        "sensitive value",
        verb="doctor",
    ),
    *(
        _frow(f"local-path-{label}", _json("doctor", docker={"executableKind": value}), "sensitive value", verb="doctor")
        for label, value in (
            ("users", "/Users/example/.docker/config.json"),
            ("tmp", "/" + "tmp/forge/socket"),
            ("run-secrets", "/run/secrets/postgres"),
            ("etc", "/etc/passwd"),
            ("var-run", "/var/run/docker.sock"),
            ("unix", "unix:///tmp/forge.sock"),
        )
    ),
)


@pytest.mark.parametrize("verb, payload, rc, fragments", [row[1:] for row in _FAULT_ROWS], ids=[row[0] for row in _FAULT_ROWS])
def test_provision_fault_matrix(assay_root: AssayHarness, verb: str, payload: bytes, rc: int, fragments: tuple[str, ...]) -> None:
    """Every adapter-boundary violation faults FAULTED with its exact diagnostic message."""
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", verb), payload, rc=rc))
    fault = assert_error_status(_run(getattr(provision_rail, verb), assay_root, executor), RailStatus.FAULTED)
    for fragment in fragments:
        assert fragment in fault.message


# --- [PROJECTION_LAWS]


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
    detail = _detail(assert_ok(_run(provision_rail.status, assay_root, executor)))
    assert detail.facts == (("schemaVersion", "3"), ("ok", "true"), ("state", "present"))
    assert detail.schema_version == 3
    assert detail.ok is True
    assert detail.warnings == ("port range narrowed",)
    assert detail.auth_mode == "auto-root"
    assert detail.auth_risk == "generated-root-secret"
    assert detail.port_policy == (("mode", "auto"), ("source", "auto"), ("range", "15364-15554"), ("seedFingerprint", "seed-hash"))
    assert detail.provision_scope == (
        ("rootKey", "abc123"),
        ("projectKey", "forge-test-abc123"),
        ("instance", "default"),
        ("composeProject", "forge-forge-test-abc123-default"),
        ("authMode", "auto-root"),
        ("authRisk", "generated-root-secret"),
    )
    assert detail.resource_counts == (("containers", 2),)
    assert detail.summary == (("ok", 2),)
    assert detail.services == (("timescale", "true", "timescale", "15432", "timescale/timescaledb-ha:pg18"),)
    assert detail.service_connections == (
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
    assert detail.service_roles == (("timescale", "time"),)
    assert detail.local_service_topology == (("timescale", "true", "timescale", "timescale/timescaledb-ha:pg18", "15432", "", ""),)
    assert detail.ports == (
        ("search", "FORGE_PROVISION_SEARCH_PORT", "15433", "free", "false", "none", "auto"),
        ("timescale", "FORGE_PROVISION_TIMESCALE_PORT", "15432", "busy", "true", "provision:this-project", "auto"),
    )
    assert detail.local_probe_values == ()


def test_provision_port_policy_allows_null_seed_fingerprint(assay_root: AssayHarness) -> None:
    """Lifecycle payloads may omit auto-allocation seed evidence."""
    payload = _json("down", portPolicy={"mode": "auto", "source": "current-manifest", "range": "15364-15554", "seedFingerprint": None})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "down"), payload))
    detail = _detail(assert_ok(_run(provision_rail.down, assay_root, executor)))
    assert detail.port_policy == (("mode", "auto"), ("source", "current-manifest"), ("range", "15364-15554"))


def test_provision_ok_projection_variants(assay_root: AssayHarness) -> None:
    """Verb-specific Ok payloads project exact safe evidence: redacted DSN admission and scalar plan summary without Compose YAML."""
    dsn = "postgres://postgres:***@127.0.0.1:15432/forge"
    service = {
        "enabled": True,
        "connectable": True,
        "host": "127.0.0.1",
        "port": 15432,
        "portEnv": "FORGE_PROVISION_TIMESCALE_PORT",
        "dsnEnv": "FORGE_PROVISION_TIMESCALE_DSN",
        "dsnRedacted": dsn,
        "containerPort": 5432,
        "composeService": "timescale",
    }
    env_executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "env"), _json("env", services={"timescale": service})))
    env_detail = _detail(assert_ok(_run(provision_rail.env, assay_root, env_executor)))
    assert env_detail.service_connections[0][6] == dsn
    plan_payload = _json("plan", artifacts={"generated": (), "plan": {"composeYaml": "redacted", "authMode": "auto-root", "serviceCount": 2}})
    plan_executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "plan"), plan_payload))
    plan_detail = _detail(assert_ok(_run(provision_rail.plan, assay_root, plan_executor)))
    assert plan_detail.plan_summary == (("authMode", "auto-root"), ("serviceCount", "2"))
    assert "composeYaml" not in dict(plan_detail.plan_summary)


# Extension projection matrix: catalog rows, runtime-probed null admission, and tool-surface rows are attribute expectations per payload.
_EXTENSION_ROWS: tuple[tuple[str, tuple[dict[str, object], ...], dict[str, tuple[tuple[str, ...], ...]]], ...] = (
    (
        "catalog-rows-stay-separate-from-results",
        (
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
            },
        ),
        {
            "extensions": (),
            "extension_catalog": (
                ("timescale", "postgis", "geospatial", "required", "apply-create", "local-extension", "image:timescale/timescaledb-ha", "false"),
            ),
            "extension_metadata": (
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
            ),
            "extension_requirements": (("timescale", "postgis", "true", "true", "true", "true", "true"),),
        },
    ),
    (
        "runtime-probed-null-fields-admitted-as-empty-cells",
        (
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
            },
        ),
        {
            "extension_metadata": (
                (
                    "*",
                    "hll",
                    "postgresql-contrib-or-image-probed",
                    "runtime-probed",
                    "runtime-probed",
                    "create-extension",
                    "optional",
                    "",
                    "",
                    "",
                    "",
                    "probe-only",
                ),
            ),
            "tool_surface_extensions": (),
        },
    ),
    (
        "tool-surface-rows-preserved",
        (
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
        ),
        {
            "tool_surface_extensions": (
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
        },
    ),
)


@pytest.mark.parametrize("rows, expected", [row[1:] for row in _EXTENSION_ROWS], ids=[row[0] for row in _EXTENSION_ROWS])
def test_provision_extensions_projection_matrix(
    assay_root: AssayHarness, rows: tuple[dict[str, object], ...], expected: dict[str, tuple[tuple[str, ...], ...]]
) -> None:
    """Extension payload families project into their exact ProvisionRun row shapes."""
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "extensions"), _json("extensions", extensions=list(rows))))
    detail = _detail(assert_ok(_run(provision_rail.extensions, assay_root, executor)))
    for attr, value in expected.items():
        assert getattr(detail, attr) == value, f"{attr} drifted"


def test_provision_doctor_projects_sanitized_runtime(assay_root: AssayHarness) -> None:
    """Doctor JSON projects the schema-v3 runtime facts without paths, sockets, or helper names."""
    payload = _json(
        "doctor",
        docker={
            "executableKind": "nix-store",
            "policy": {"status": "ok", "reason": None},
            "endpointKind": "unix",
            "endpointPathExists": True,
            "compose": "Docker Compose version v2.39.0",
            "server": "29.0.0",
            "hostConfig": {"credentialHelperPresent": True, "credHelpers": 0, "warning": "credential-helper-present-for-host-config"},
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
            "portsInspectable": True,
            "portsUsable": True,
            "blockedPorts": [],
            "appleContainer": {"present": True, "system": "running", "eligible": {"gate": True}},
        },
        lock={"present": True, "active": False, "state": "ownerless", "pidAlive": False, "heartbeatStale": False, "command": "doctor"},
        colima={
            "available": True,
            "status": {"runtime": "docker", "arch": "aarch64", "driver": "macOS Virtualization.Framework", "mountType": "virtiofs"},
            "raw": None,
        },
    )
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "doctor"), payload))
    detail = _detail(assert_ok(_run(provision_rail.doctor, assay_root, executor)))
    assert dict(detail.doctor) == {
        "dockerPolicyStatus": "ok",
        "dockerEndpointKind": "unix",
        "dockerEndpointPathExists": "true",
        "dockerPresent": "true",
        "dockerExecutableKind": "nix-store",
        "dockerComposeVersion": "Docker Compose version v2.39.0",
        "dockerServerVersion": "29.0.0",
        "anonymousPullConfig": "true",
        "credentialHelperPresent": "true",
        "credentialHelperWarning": "credential-helper-present-for-host-config",
        "runtimeForgeProvisionPresent": "true",
        "runtimeForgeProvisionSchemaVersion": "3",
        "runtimeComposePresent": "true",
        "runtimeComposeVersion": "Docker Compose version v2.39.0",
        "runtimeJqPresent": "true",
        "runtimeListenerProbeMethod": "lsof",
        "runtimeAnonymousDockerConfig": "true",
        "runtimeHostCredentialHelperPresent": "true",
        "portsInspectable": "true",
        "portsUsable": "true",
        "blockedPorts": "0",
        "lockState": "ownerless",
        "lockPresent": "true",
        "lockActive": "false",
        "lockPidAlive": "false",
        "lockHeartbeatStale": "false",
        "lockCommand": "doctor",
        "colimaAvailable": "true",
        "colimaRuntime": "docker",
        "colimaArch": "aarch64",
        "colimaDriver": "macOS Virtualization.Framework",
        "colimaMountType": "virtiofs",
        "appleContainerPresent": "true",
        "appleContainerSystem": "running",
        "appleContainerEligible": "true",
    }


@pytest.mark.parametrize("rc", [0, 1], ids=["exit-zero", "exit-one"])
def test_provision_ok_false_projects_failed_run(assay_root: AssayHarness, rc: int) -> None:
    """Forge ok:false JSON is a completed provisioning FAILURE with project identity — never an adapter parse fault, even on exit zero."""
    payload = _json("status", ok=False, error={"code": "port-conflict", "message": "fixed port is busy", "exitCode": rc})
    executor = SeamExecutor(fan_fn=_fan_payload(("forge-provision", "--json", "status"), payload, rc=rc))
    report = assert_ok(_run(provision_rail.status, assay_root, executor))
    detail = _detail(report)
    assert report.status is RailStatus.FAILED
    assert report.counts.failed == 1
    assert not report.artifacts
    assert detail.ok is False
    assert detail.error == (("code", "port-conflict"), ("message", "fixed port is busy"), ("exitCode", str(rc)))
    assert detail.provision_scope[:4] == (
        ("rootKey", "abc123"),
        ("projectKey", "forge-test-abc123"),
        ("instance", "default"),
        ("composeProject", "forge-forge-test-abc123-default"),
    )


# --- [CHECK_VERB_LAWS]


def test_provision_check_folds_stack_and_local_probes(assay_root: AssayHarness) -> None:
    """The check verb runs stack evidence plus local runtime probes."""
    calls: list[tuple[tuple[str, ...], ...]] = []
    report = assert_ok(_run(provision_rail.check, assay_root, SeamExecutor(fan_fn=_recording_fan(calls))))
    commands = calls[0]
    assert (report.claim, report.verb) == (Claim.PROVISION, "check")
    assert ("forge-provision", "--json", "check") in commands
    assert ("forge-provision", "--json", "tools") in commands
    assert ("duckdb", "--version") not in commands
    assert any(command[:2] == ("forge-scientific-env", "python3") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "pkg-config", "--modversion") for command in commands)
    assert any(command[:3] == ("forge-scientific-env", "sh", "-lc") for command in commands)
    detail = _detail(report)
    assert detail.local_probes == (("forge-python-abi", "ok"), ("forge-openblas", "ok"), ("forge-onnxruntime-lib", "ok"))
    assert detail.local_probe_values[-1] == ("forge-onnxruntime-lib", "ok", "present:libonnxruntime.dylib")


def test_provision_check_keeps_tools_success_when_stack_check_fails(assay_root: AssayHarness) -> None:
    """The merged check detail may fail while the independent tools subprocess remains successful."""
    check_cmd = ("forge-provision", "--json", "check")
    tools_cmd = ("forge-provision", "--json", "tools")
    tools_payload = _json(
        "tools",
        tools={
            "surfaces": {"duckdb": {"ok": True, "executable": "duckdb", "probe": {"extensionRows": 31}}},
            "summary": {"selected": "all", "ok": True},
        },
    )
    executor = SeamExecutor(
        fan_fn=_override_fan({
            check_cmd: Ok(
                receipt(
                    check_cmd,
                    1,
                    status=RailStatus.FAILED,
                    stdout=_json("check", ok=False, error={"code": "error", "message": "owned service is not running", "exitCode": 1}),
                )
            ),
            tools_cmd: Ok(receipt(tools_cmd, 0, status=RailStatus.OK, stdout=tools_payload)),
        })
    )
    report = assert_ok(_run(provision_rail.check, assay_root, executor))
    detail = _detail(report)
    assert report.status is RailStatus.FAILED
    assert detail.ok is False
    assert detail.tool_surfaces == (("duckdb", "ok", "31"),)
    assert detail.tool_summary == (("ok", "true"), ("selected", "all"))
    result_statuses = {row.id: row.severity for row in report.results}
    assert result_statuses["forge-provision --json check"] == RailStatus.FAILED.value
    assert result_statuses.get("forge-provision --json tools", RailStatus.OK.value) == RailStatus.OK.value


def test_provision_check_scrubs_failed_local_probe_streams(assay_root: AssayHarness) -> None:
    """Failed local probes keep a bounded failure row without durable raw stdout/stderr."""
    probe_cmd = ("forge-scientific-env", "pkg-config", "--modversion", "openblas")
    executor = SeamExecutor(
        fan_fn=_override_fan({
            probe_cmd: Ok(receipt(probe_cmd, 1, status=RailStatus.FAILED, stderr=b"POSTGRES_PASSWORD=leak postgres://postgres:pw@127.0.0.1/forge"))
        })
    )
    report = assert_ok(_run(provision_rail.check, assay_root, executor))
    encoded = msgspec.json.encode(report)
    assert report.status is RailStatus.FAILED
    assert b"POSTGRES_PASSWORD" not in encoded
    assert b"postgres://postgres:pw" not in encoded
    assert b"provision probe failed: forge-openblas" in encoded


def test_provision_check_rejects_sensitive_success_local_probe_values(assay_root: AssayHarness) -> None:
    """Successful local probe output must not bypass Forge payload sanitizers."""
    probe_cmd = ("forge-scientific-env", "pkg-config", "--modversion", "openblas")
    executor = SeamExecutor(
        fan_fn=_override_fan({probe_cmd: Ok(receipt(probe_cmd, 0, status=RailStatus.OK, stdout=b"/nix/store/leak/libopenblas.dylib\n"))})
    )
    fault = assert_error_status(_run(provision_rail.check, assay_root, executor), RailStatus.FAULTED)
    assert "sensitive local probe value" in fault.message

"""Run local provisioning infrastructure through the Forge-owned CLI."""

from dataclasses import dataclass
from itertools import starmap
import re
from typing import Final, override

from expression import Error, Ok, Result  # noqa: TC002  # beartype resolves provision handler annotations at registry runtime
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # registry runtime resolves handler annotations
from tools.assay.core.engine import fan_out
from tools.assay.core.model import (  # noqa: TC001
    BaseParams,
    Check,
    Claim,
    Completed,
    Fault,
    fold,
    Input,
    Language,
    Mode,
    ProvisionRun,
    Report,
    Runner,
    Tool,
)
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus, Step


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class ProvisionParams(BaseParams):
    """Parameters for Forge-owned Rasm provisioning commands."""

    @override
    def _arity(self, verb: str) -> int:
        _ = verb
        return 0


class _ProvisionService(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    key: str = ""
    role: str = ""
    enabled: bool = False
    connectable: bool = False
    profile: str = ""
    image: str = ""
    image_env: str = ""
    host: str = ""
    port: int = 0
    port_env: str = ""
    port_source: str = ""
    container_port: int = 0
    dsn_redacted: str | None = None
    dsn_env: str = ""
    compose_service: str = ""
    state: str = ""
    health: str = ""


class _ProvisionPort(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    service: str = ""
    env: str = ""
    value: int = 0
    state: str = ""
    occupied: bool = False
    owner: str = ""
    owner_class: str = ""
    port_source: str = ""


class _ProvisionExtension(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    service: str = ""
    extension: str = ""
    state: str = ""
    version: str | None = None
    category: str | None = None
    required: bool = False
    create_on_verify: bool = False
    kind: str = ""
    source_package: str | None = None
    aliases: tuple[str, ...] = ()
    load_name: str = ""
    probe_function: str | None = None
    expected_service: str = ""
    risk_class: str = ""
    preload_required: bool = False
    self_provisioned: bool = False
    dev_gated: bool = False
    requires_superuser: bool = False
    requires_shared_preload: bool = False
    file_access: bool = False
    network_access: bool = False
    background_worker: bool = False
    create_policy: str = ""
    source_route: str = ""
    source_kind: str = ""
    nix_status: str = ""
    probe_kind: str = ""
    capability_rank: str = ""
    external_access: str = ""
    restart_class: str = ""
    service_profile: str = ""
    image_tag: str = ""
    load_policy: str = ""
    surface: str = ""
    database: str = ""
    availability: str = ""
    admission: str = ""
    profile: str = ""


class _ProvisionAuth(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    mode: str = ""
    risk: str = ""
    user: str = ""
    credential: str = ""
    agent_prompt_required: bool = False


class _ProvisionPortPolicy(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    mode: str = ""
    source: str = ""
    range_: str = msgspec.field(default="", name="range")
    exclude: str = ""
    seed: str | None = None
    seed_fingerprint: str | None = None


class _ProvisionError(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    code: str = ""
    message: str = ""
    exit_code: int = 0


class _ProvisionDockerPolicy(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    status: str = ""
    reason: str | None = None


class _ProvisionDockerHostConfig(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    credential_helper_present: bool | None = None
    warning: str | None = None


class _ProvisionDockerAnonymousPullConfig(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    exists: bool | None = None


class _ProvisionDocker(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    executable_present: bool | None = None
    executable_kind: str | None = None
    policy: _ProvisionDockerPolicy | None = None
    endpoint_kind: str = ""
    compose: str = ""
    server: str = ""
    host_config: _ProvisionDockerHostConfig | None = None
    anonymous_pull_config: _ProvisionDockerAnonymousPullConfig | None = None


class _ProvisionRuntimeProgram(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    present: bool | None = None
    schema_version: int = 0


class _ProvisionRuntimeCompose(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    present: bool | None = None
    version: str | None = None


class _ProvisionRuntime(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    rasm_provision: _ProvisionRuntimeProgram | None = None
    docker: _ProvisionRuntimeProgram | None = None
    compose: _ProvisionRuntimeCompose | None = None
    jq: _ProvisionRuntimeProgram | None = None
    listener_probe_method: str = ""
    anonymous_docker_config: bool | None = None
    host_credential_helper_present: bool | None = None


class _ProvisionLock(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    present: bool | None = None
    active: bool | None = None
    state: str = ""
    pid_alive: bool | None = None
    heartbeat_stale: bool | None = None
    command: str | None = None


class _ProvisionColimaStatus(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    running: bool | None = None
    runtime: str | None = None
    arch: str | None = None


class _ProvisionColima(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    available: bool | None = None
    status: _ProvisionColimaStatus | None = None


class _ProvisionPayload(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    schema_version: int = 0
    command: str = ""
    ok: bool = True
    error: _ProvisionError | None = None
    state: str = ""
    project: str = ""
    root_fingerprint: str = ""
    auth: _ProvisionAuth | None = None
    port_policy: _ProvisionPortPolicy | None = None
    docker_available: bool | None = None
    ports_inspectable: bool | None = None
    ports_usable: bool | None = None
    non_owned_cleanup_policy: str = ""
    summary: dict[str, int] = msgspec.field(default_factory=dict)
    resources: dict[str, int] = msgspec.field(default_factory=dict)
    probes: dict[str, str | int | bool] = msgspec.field(default_factory=dict)
    artifacts: dict[str, object] = msgspec.field(default_factory=dict)
    docker: _ProvisionDocker | None = None
    runtime: _ProvisionRuntime | None = None
    lock: _ProvisionLock | None = None
    colima: _ProvisionColima | None = None
    services: dict[str, _ProvisionService] = msgspec.field(default_factory=dict)
    ports: tuple[_ProvisionPort, ...] = ()
    extensions: tuple[_ProvisionExtension, ...] = ()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
_JSON_VERBS: Final[frozenset[str]] = frozenset({"up", "down", "status", "doctor", "ports", "inventory", "extensions", "plan", "env", "verify"})
_VERB_MODE: Final[dict[str, Mode]] = {"up": Mode.WRITE, "down": Mode.WRITE, "verify": Mode.VERIFY}
_VERB_TIMEOUT: Final[dict[str, float]] = {"up": 300.0, "verify": 180.0}
_FACT_FIELDS: Final[tuple[tuple[str, str], ...]] = (
    ("schemaVersion", "schema_version"),
    ("ok", "ok"),
    ("state", "state"),
    ("project", "project"),
    ("rootFingerprint", "root_fingerprint"),
    ("dockerAvailable", "docker_available"),
    ("portsInspectable", "ports_inspectable"),
    ("portsUsable", "ports_usable"),
    ("nonOwnedCleanupPolicy", "non_owned_cleanup_policy"),
)
_SENSITIVE_KEY_FRAGMENTS: Final[frozenset[str]] = frozenset({"password", "token", "secret", "pgpass"})
_SENSITIVE_KEYS: Final[frozenset[str]] = frozenset(
    {
        "composefile",
        "dockerconfig",
        "dockerconfigpath",
        "dockerhost",
        "envfile",
        "hostlistenercommand",
        "logs",
        "mountpoint",
        "rawcompose",
        "rawlogs",
        "socket",
    }
)
_CREDENTIAL_URI: Final[re.Pattern[str]] = re.compile(r"(?i)[a-z][a-z0-9+.-]*://[^/\s:@]+:([^@\s]+)@")
_RAW_URI: Final[re.Pattern[str]] = re.compile(r"(?i)\b(?:postgres(?:ql)?|mysql|mariadb|mongodb|redis|amqp|http|https|ssh|tcp|unix)://")
_ABSOLUTE_POSIX_PATH: Final[re.Pattern[str]] = re.compile(r"(^|[\s\"'])/(?!/)[^\s\"']+")
_SENSITIVE_VALUE: Final[re.Pattern[str]] = re.compile(
    r"(?i)("
    r"PGPASSFILE="
    r"|POSTGRES_PASSWORD="
    r"|DOCKER_CONFIG="
    r"|DOCKER_HOST="
    r"|\\.artifacts/provisioning"
    r"|(?:^|[\s\"'])\\.env(?:[\s\"']|$)"
    r"|(?:^|[\s\"'])manifest\\.json(?:[\s\"']|$)"
    r"|(?:^|[\s\"'])compose\\.ya?ml(?:[\s\"']|$)"
    r"|docker\\.sock"
    r"|BEGIN [A-Z ]*PRIVATE KEY"
    r"|gh[pousr]_[A-Za-z0-9_]+"
    r"|xox[a-z]-[A-Za-z0-9-]+"
    r")"
)
_SAFE_WIRE_CAP: Final[int] = 512
_PYTHON_ABI_PROBE: Final[str] = "import sys, sysconfig; print(sys.implementation.cache_tag, sysconfig.get_config_var('Py_GIL_DISABLED') or 0)"
_ONNXRUNTIME_LIB_PROBE: Final[str] = 'test -n "${ONNXRUNTIME_LIB:-}" && test -e "$ONNXRUNTIME_LIB" && printf "present:%s\\n" "${ONNXRUNTIME_LIB##*/}"'
_VERIFY_PROBES: Final[tuple[tuple[str, tuple[str, ...]], ...]] = (
    ("duckdb-version", ("duckdb", "--version")),
    ("forge-python-abi", ("forge-scientific-env", "python3", "-c", _PYTHON_ABI_PROBE)),
    ("forge-openblas", ("forge-scientific-env", "pkg-config", "--modversion", "openblas")),
    ("forge-onnxruntime-lib", ("forge-scientific-env", "sh", "-lc", _ONNXRUNTIME_LIB_PROBE)),
)
_PAYLOAD_DECODER: Final[msgspec.json.Decoder[_ProvisionPayload]] = msgspec.json.Decoder(_ProvisionPayload)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _tool(name: str, argv: tuple[str, ...], *, mode: Mode = Mode.RUN, timeout: float = 120.0) -> Tool:
    return Tool(name, Runner.DIRECT, argv, Input.NONE, Language.PYTHON, Claim.PROVISION, mode=mode, timeout=timeout)


def _stack(verb: str) -> Tool:
    return _tool(
        f"rasm-provision-{verb}",
        ("rasm-provision", *(("--json", verb) if verb in _JSON_VERBS else (verb,))),
        mode=_VERB_MODE.get(verb, Mode.RUN),
        timeout=_VERB_TIMEOUT.get(verb, 120.0),
    )


def _unsafe_key(key: str) -> bool:
    normalized = key.replace("_", "").replace("-", "").lower()
    return normalized in _SENSITIVE_KEYS or any(fragment in normalized for fragment in _SENSITIVE_KEY_FRAGMENTS)


def _object_children(value: object) -> tuple[tuple[str, object], ...]:
    match value:
        case dict() as mapping:
            return tuple((str(key), child) for key, child in mapping.items())
        case list() | tuple() as items:
            return tuple((str(index), child) for index, child in enumerate(items))
        case _:
            return ()


def _sensitive_key_path(value: object, path: tuple[str, ...] = ()) -> tuple[str, ...]:
    for key, child in _object_children(value):
        candidate = (*path, key)
        if _unsafe_key(key):
            return candidate
        if found := _sensitive_key_path(child, candidate):
            return found
    return ()


def _sensitive_value_path(value: object, path: tuple[str, ...] = ()) -> tuple[str, ...]:
    if isinstance(value, str):
        credential = _CREDENTIAL_URI.search(value)
        if credential and credential.group(1) != "***":
            return path
        scrubbed = _CREDENTIAL_URI.sub("", value)
        raw_uri = _RAW_URI.search(scrubbed)
        return path if raw_uri or _SENSITIVE_VALUE.search(scrubbed) or _ABSOLUTE_POSIX_PATH.search(scrubbed) else ()
    for key, child in _object_children(value):
        if found := _sensitive_value_path(child, (*path, key)):
            return found
    return ()


def _payload_fault(argv: tuple[str, ...], verb: str, message: str) -> Result[_ProvisionPayload | None, Fault]:
    return Error(Fault(argv, RailStatus.FAULTED, f"{Step.PARSE}: {verb}: {message}"))


def _decode_raw_payload(verb: str, outcome: Completed, body: bytes, *, required: bool) -> Result[object | None, Fault]:
    if not body.startswith(b"{"):
        return _payload_fault(outcome.argv, verb, "expected rasm-provision JSON") if required else Ok(None)
    try:
        raw: object = msgspec.json.decode(body)
    except msgspec.DecodeError as exc:
        return _payload_fault(outcome.argv, verb, f"invalid rasm-provision JSON: {exc}") if required else Ok(None)
    return Ok(raw)


def _validate_payload_wire(verb: str, outcome: Completed, raw: object) -> Result[None, Fault]:
    if found := _sensitive_key_path(raw):
        return _payload_fault(outcome.argv, verb, f"sensitive key in rasm-provision JSON: {'.'.join(found)}").map(lambda _: None)
    if found := _sensitive_value_path(raw):
        return _payload_fault(outcome.argv, verb, f"sensitive value in rasm-provision JSON: {'.'.join(found)}").map(lambda _: None)
    if isinstance(raw, dict) and raw.get("schemaVersion") != 2:
        code = "rasm-provision-update-required" if raw.get("schemaVersion") == 1 else "unsupported rasm-provision schema"
        return _payload_fault(outcome.argv, verb, f"{code}: schemaVersion={raw.get('schemaVersion')!r}").map(lambda _: None)
    return Ok(None)


def _decode_payload(verb: str, outcome: Completed | None) -> Result[_ProvisionPayload | None, Fault]:
    if outcome is None:
        return Error(Fault(("rasm-provision", verb), RailStatus.FAULTED, f"{Step.PARSE}: missing rasm-provision outcome"))
    body = outcome.stdout.strip()
    required = verb in _JSON_VERBS
    return _decode_raw_payload(verb, outcome, body, required=required).bind(
        lambda raw: Ok(None) if raw is None else _decode_validated_payload(verb, outcome, body, raw, required=required)
    )


def _decode_validated_payload(
    verb: str,
    outcome: Completed,
    body: bytes,
    raw: object,
    *,
    required: bool,
) -> Result[_ProvisionPayload | None, Fault]:
    validated = _validate_payload_wire(verb, outcome, raw)
    if validated.is_error():
        return Error(validated.error)
    try:
        payload = _PAYLOAD_DECODER.decode(body)
    except msgspec.DecodeError as exc:
        return _payload_fault(outcome.argv, verb, f"invalid rasm-provision JSON: {exc}") if required else Ok(None)
    if required and payload.command != verb:
        return _payload_fault(outcome.argv, verb, f"rasm-provision JSON command={payload.command}")
    return Ok(payload)


def _safe_wire(value: object, cap: int = _SAFE_WIRE_CAP) -> str:
    text = str(value)
    return text if len(text) <= cap else f"{text[:cap]}..."


def _wire(value: object) -> str:
    match value:
        case None | "":
            return ""
        case bool() as bit:
            return str(bit).lower()
        case _:
            return _safe_wire(value)


def _facts(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    return tuple((wire, _wire(getattr(payload, attr))) for wire, attr in _FACT_FIELDS if _wire(getattr(payload, attr)))


def _error(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    err = payload.error
    return (
        ()
        if err is None
        else tuple(
            (key, value)
            for key, value in (("code", err.code), ("message", err.message), ("exitCode", _wire(err.exit_code)))
            if value
        )
    )


def _auth(payload: _ProvisionPayload) -> tuple[str, str]:
    auth = payload.auth
    return ("", "") if auth is None else (auth.mode, auth.risk)


def _port_policy(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    policy = payload.port_policy
    if policy is None:
        return ()
    return tuple(
        (key, value)
        for key, value in (
            ("mode", policy.mode),
            ("source", policy.source),
            ("range", policy.range_),
            ("exclude", policy.exclude),
            ("seedFingerprint", policy.seed_fingerprint or policy.seed),
        )
        if value
    )


def _provision_scope(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    auth_mode, auth_risk = _auth(payload)
    return tuple(
        (key, value)
        for key, value in (
            ("project", payload.project),
            ("rootFingerprint", payload.root_fingerprint),
            ("authMode", auth_mode),
            ("authRisk", auth_risk),
            ("portsInspectable", _wire(payload.ports_inspectable)),
            ("portsUsable", _wire(payload.ports_usable)),
            ("nonOwnedCleanupPolicy", payload.non_owned_cleanup_policy),
        )
        if value
    )


def _services(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str], ...]:
    return tuple(
        (key, _wire(row.enabled), row.profile, str(row.port), row.image)
        for key, row in sorted(payload.services.items())
    )


def _service_connections(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str, str, str], ...]:
    return tuple(
        (
            key,
            _wire(row.connectable),
            row.host,
            _wire(row.port),
            row.port_env,
            row.dsn_env,
            _wire(row.dsn_redacted),
            _wire(row.container_port),
            row.compose_service,
        )
        for key, row in sorted(payload.services.items())
        if row.host or row.port_env or row.dsn_env or row.dsn_redacted or row.container_port or row.compose_service
    )


def _service_roles(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    return tuple((key, row.role) for key, row in sorted(payload.services.items()) if row.role)


def _local_service_topology(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str], ...]:
    return tuple(
        (key, _wire(row.enabled), row.profile, row.image, str(row.port), row.state, row.health)
        for key, row in sorted(payload.services.items())
    )


def _ports(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str], ...]:
    return tuple(
        (row.service, row.env, _wire(row.value), row.state, _wire(row.occupied), row.owner_class or row.owner, row.port_source)
        for row in sorted(payload.ports, key=lambda row: (row.service, row.env, row.value, row.state))
    )


def _verify_extensions(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str], ...]:
    return tuple(
        (row.service, row.extension, row.state, _wire(row.version), _wire(row.category), "required" if row.required else "optional")
        for row in sorted(payload.extensions, key=lambda row: (row.service, row.extension, row.state, row.version or ""))
        if row.state
    )


def _catalog_extensions(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str, str], ...]:
    return tuple(
        (
            row.service,
            row.extension,
            _wire(row.category),
            "required" if row.required else "optional",
            row.create_policy or ("create-on-verify" if row.create_on_verify else "probe-only"),
            row.risk_class,
            _wire(row.source_package),
            _wire(row.preload_required),
        )
        for row in sorted(payload.extensions, key=lambda row: (row.service, row.extension, row.category or "", not row.required, row.create_policy))
        if not row.state
    )


def _extension_metadata(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str, str, str, str, str, str], ...]:
    return tuple(
        (
            row.service,
            row.extension,
            _wire(row.source_route),
            _wire(row.source_kind),
            _wire(row.nix_status),
            _wire(row.probe_kind),
            _wire(row.capability_rank),
            _wire(row.external_access),
            _wire(row.restart_class),
            _wire(row.service_profile),
            _wire(row.image_tag),
            _wire(row.load_policy),
        )
        for row in sorted(payload.extensions, key=lambda row: (row.service, row.extension, row.category or "", not row.required, row.create_policy))
        if not row.state
        and any(
            (
                row.source_route,
                row.source_kind,
                row.nix_status,
                row.probe_kind,
                row.capability_rank,
                row.external_access,
                row.restart_class,
                row.service_profile,
                row.image_tag,
                row.load_policy,
            )
        )
    )


def _extension_requirements(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str], ...]:
    return tuple(
        (
            row.service,
            row.extension,
            _wire(row.requires_superuser),
            _wire(row.requires_shared_preload),
            _wire(row.file_access),
            _wire(row.network_access),
            _wire(row.background_worker),
        )
        for row in sorted(payload.extensions, key=lambda row: (row.service, row.extension, row.category or "", not row.required, row.create_policy))
        if not row.state
        and (
            row.requires_superuser
            or row.requires_shared_preload
            or row.file_access
            or row.network_access
            or row.background_worker
        )
    )


def _tool_surface_extensions(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str, str, str, str, str, str, str], ...]:
    return tuple(
        (
            row.service,
            row.extension,
            row.surface,
            row.database,
            _wire(row.availability),
            _wire(row.admission),
            _wire(row.profile),
            _wire(row.load_policy),
            _wire(row.create_policy),
            _wire(row.source_route),
            _wire(row.load_name),
            _wire(row.probe_function),
            ",".join(row.aliases),
        )
        for row in sorted(payload.extensions, key=lambda row: (row.surface, row.database, row.extension))
        if not row.state and (row.kind == "tool-extension" or row.surface or row.database)
    )


def _doctor(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    docker = payload.docker
    runtime = payload.runtime
    lock = payload.lock
    colima = payload.colima
    docker_policy = docker.policy if docker else None
    host_config = docker.host_config if docker else None
    anonymous_pull = docker.anonymous_pull_config if docker else None
    colima_status = colima.status if colima else None
    return tuple(
        (key, value)
        for key, value in (
            ("dockerPolicyStatus", docker_policy.status if docker_policy else ""),
            ("dockerPolicyReason", _wire(docker_policy.reason) if docker_policy else ""),
            ("dockerEndpointKind", docker.endpoint_kind if docker else ""),
            ("dockerExecutablePresent", _wire(docker.executable_present) if docker else ""),
            ("dockerExecutableKind", _wire(docker.executable_kind) if docker else ""),
            ("dockerComposeVersion", docker.compose if docker else ""),
            ("dockerServerVersion", docker.server if docker else ""),
            ("anonymousPullConfig", _wire(anonymous_pull.exists) if anonymous_pull else ""),
            ("credentialHelperPresent", _wire(host_config.credential_helper_present) if host_config else ""),
            ("runtimeDockerPresent", _wire(runtime.docker.present) if runtime and runtime.docker else ""),
            ("runtimeComposePresent", _wire(runtime.compose.present) if runtime and runtime.compose else ""),
            ("runtimeComposeVersion", _wire(runtime.compose.version) if runtime and runtime.compose else ""),
            ("runtimeJqPresent", _wire(runtime.jq.present) if runtime and runtime.jq else ""),
            ("runtimeListenerProbeMethod", runtime.listener_probe_method if runtime else ""),
            ("runtimeAnonymousDockerConfig", _wire(runtime.anonymous_docker_config) if runtime else ""),
            ("runtimeHostCredentialHelperPresent", _wire(runtime.host_credential_helper_present) if runtime else ""),
            ("lockState", lock.state if lock else ""),
            ("lockPresent", _wire(lock.present) if lock else ""),
            ("lockActive", _wire(lock.active) if lock else ""),
            ("lockPidAlive", _wire(lock.pid_alive) if lock else ""),
            ("lockHeartbeatStale", _wire(lock.heartbeat_stale) if lock else ""),
            ("colimaAvailable", _wire(colima.available) if colima else ""),
            ("colimaRunning", _wire(colima_status.running) if colima_status else ""),
            ("colimaRuntime", _wire(colima_status.runtime) if colima_status else ""),
            ("colimaArch", _wire(colima_status.arch) if colima_status else ""),
        )
        if value
    )


def _probe_name(argv: tuple[str, ...]) -> str:
    return next((name for name, prefix in _VERIFY_PROBES if argv[: len(prefix)] == prefix), " ".join(argv[:2]))


def _probe_text(outcome: Completed) -> str:
    text = (outcome.stdout or outcome.stderr).decode(errors="replace").strip().splitlines()
    return _safe_wire(text[0]) if text else ""


def _local_probes(done: tuple[Completed, ...]) -> tuple[tuple[str, str], ...]:
    return tuple(
        (_probe_name(outcome.argv), "ok" if outcome.returncode == 0 else "failed")
        for outcome in done
        if outcome.argv[:1] != ("rasm-provision",)
    )


def _local_probe_values(done: tuple[Completed, ...]) -> tuple[tuple[str, str, str], ...]:
    return tuple(
        (_probe_name(outcome.argv), "ok" if outcome.returncode == 0 else "failed", value)
        for outcome in done
        if outcome.argv[:1] != ("rasm-provision",) and outcome.returncode == 0 and (value := _probe_text(outcome))
    )


def _validate_local_probe_values(done: tuple[Completed, ...]) -> Result[None, Fault]:
    for outcome in done:
        if outcome.argv[:1] == ("rasm-provision",) or outcome.returncode != 0:
            continue
        value = _probe_text(outcome)
        if value and _sensitive_value_path({"value": value}):
            return Error(Fault(outcome.argv, RailStatus.FAULTED, f"{Step.PARSE}: {_probe_name(outcome.argv)}: sensitive local probe value"))
    return Ok(None)


def _payload_probe_values(payload: _ProvisionPayload) -> tuple[tuple[str, str, str], ...]:
    return tuple((key, "ok", _wire(value)) for key, value in sorted(payload.probes.items()))


def _resource_counts(payload: _ProvisionPayload) -> tuple[tuple[str, int], ...]:
    return tuple(sorted((payload.resources or payload.summary).items()))


def _project(verb: str, done: tuple[Completed, ...], payload: _ProvisionPayload | None) -> ProvisionRun:
    local = _local_probes(done)
    values = _local_probe_values(done)
    auth_mode, auth_risk = ("", "") if payload is None else _auth(payload)
    return (
        ProvisionRun(verb=verb, local_probes=local, local_probe_values=values)
        if payload is None
        else ProvisionRun(
            verb=verb,
            json=True,
            schema_version=payload.schema_version,
            ok=payload.ok,
            error=_error(payload),
            auth_mode=auth_mode,
            auth_risk=auth_risk,
            port_policy=_port_policy(payload),
            provision_scope=_provision_scope(payload),
            local_service_topology=_local_service_topology(payload),
            service_roles=_service_roles(payload),
            resource_counts=_resource_counts(payload),
            facts=_facts(payload),
            summary=tuple(sorted(payload.summary.items())),
            services=_services(payload),
            service_connections=_service_connections(payload),
            ports=_ports(payload),
            extensions=_verify_extensions(payload),
            extension_catalog=_catalog_extensions(payload),
            extension_metadata=_extension_metadata(payload),
            extension_requirements=_extension_requirements(payload),
            tool_surface_extensions=_tool_surface_extensions(payload),
            doctor=_doctor(payload),
            local_probes=local,
            local_probe_values=(*values, *_payload_probe_values(payload)),
        )
    )


def _provision_failed_exit(detail: ProvisionRun) -> int:
    values = dict(detail.error)
    try:
        code = int(values.get("exitCode", "1") or "1")
    except ValueError:
        code = 1
    return code or 1


def _sanitize_provision_outcome(detail: ProvisionRun, outcome: Completed) -> Completed:
    if outcome.argv[:1] == ("rasm-provision",):
        if detail.json and not detail.ok:
            return msgspec.structs.replace(
                outcome,
                returncode=_provision_failed_exit(detail),
                status=RailStatus.FAILED,
                stdout=b"",
                stderr=b"provision result failed",
                artifacts=(),
            )
        return msgspec.structs.replace(outcome, stdout=b"", stderr=b"", artifacts=())
    if outcome.returncode == 0:
        return msgspec.structs.replace(outcome, stderr=b"", artifacts=())
    return msgspec.structs.replace(outcome, stdout=b"", stderr=f"provision probe failed: {_probe_name(outcome.argv)}".encode(), artifacts=())


def _detail(verb: str, done: tuple[Completed, ...]) -> Result[ProvisionRun, Fault]:
    stack = next((outcome for outcome in done if outcome.argv[:1] == ("rasm-provision",)), None)
    return _validate_local_probe_values(done).bind(lambda _: _decode_payload(verb, stack).map(lambda payload: _project(verb, done, payload)))


def _run(settings: AssaySettings, scope: ArtifactScope, verb: str, tools: tuple[Tool, ...]) -> Result[Report, Fault]:
    outcomes = sequence(block.of_seq(fan_out(tuple(Check(tool=tool) for tool in tools), settings=settings, scope=scope, routed=_ROUTED)))
    return outcomes.bind(
        lambda done: _detail(verb, tuple(done)).map(
            lambda detail: fold(
                Claim.PROVISION,
                verb,
                tuple(_sanitize_provision_outcome(detail, outcome) for outcome in done),
                detail=detail,
                promote_empty=True,
            )
        )
    )


def _invoke(settings: AssaySettings, scope: ArtifactScope, verb: str) -> Result[Report, Fault]:
    return _run(settings, scope, verb, (_stack(verb),))


def up(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Start Forge-owned provisioning services.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "up")


def down(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Stop Forge-owned provisioning services.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "down")


def status(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Report provisioning status.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "status")


def doctor(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Diagnose provisioning runtime readiness.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "doctor")


def ports(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Report configured provisioning ports.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "ports")


def inventory(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Report owned provisioning resources.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "inventory")


def extensions(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Report provisioning extension targets.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "extensions")


def plan(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Render the provisioning compose plan.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "plan")


def env(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Report redacted provisioning environment evidence.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "env")


def verify(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Verify provisioning extensions and local runtime probes.

    Returns:
        Provision report, or provisioning fault.
    """
    return _run(settings, scope, "verify", (_stack("verify"), *tuple(starmap(_tool, _VERIFY_PROBES))))


__all__ = ["ProvisionParams", "doctor", "down", "env", "extensions", "inventory", "plan", "ports", "status", "up", "verify"]

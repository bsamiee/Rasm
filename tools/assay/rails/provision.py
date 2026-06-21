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
    """Parameters for Forge-owned provisioning commands."""

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
    create_on_apply: bool = False
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
    image_tag: str | None = None
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
    seed_fingerprint: str | None = None


class _ProvisionError(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    code: str = ""
    message: str = ""
    exit_code: int = 0


class _ProvisionNotice(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    code: str = ""
    message: str = ""


class _ProvisionOwnedResources(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    containers: tuple[dict[str, object], ...] = ()
    volumes: tuple[dict[str, object], ...] = ()
    networks: tuple[dict[str, object], ...] = ()


class _ProvisionResources(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    counts: dict[str, int] = msgspec.field(default_factory=dict)
    owned: _ProvisionOwnedResources = msgspec.field(default_factory=_ProvisionOwnedResources)
    images: tuple[dict[str, object], ...] = ()
    docker_disk: tuple[dict[str, object], ...] = ()
    runtime: dict[str, object] = msgspec.field(default_factory=dict)


class _ProvisionArtifacts(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    generated: tuple[dict[str, object], ...] = ()
    plan: dict[str, object] | None = None


class _ProvisionExtensions(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    catalog: tuple[_ProvisionExtension, ...] = ()
    results: tuple[_ProvisionExtension, ...] = ()
    summary: dict[str, int] = msgspec.field(default_factory=dict)


class _ProvisionTools(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    surfaces: dict[str, object] = msgspec.field(default_factory=dict)
    summary: dict[str, object] = msgspec.field(default_factory=dict)


class _ProvisionProject(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    root_key: str = ""
    project_key: str = ""
    instance: str = ""
    compose_project: str = ""


class _ProvisionPayload(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    schema_version: int = 0
    command: str = ""
    ok: bool = True
    warnings: tuple[_ProvisionNotice, ...] = ()
    error: _ProvisionError | None = None
    state: str = ""
    project: _ProvisionProject = msgspec.field(default_factory=_ProvisionProject)
    auth: _ProvisionAuth | None = None
    port_policy: _ProvisionPortPolicy | None = None
    resources: _ProvisionResources = msgspec.field(default_factory=_ProvisionResources)
    artifacts: _ProvisionArtifacts = msgspec.field(default_factory=_ProvisionArtifacts)
    services: dict[str, _ProvisionService] = msgspec.field(default_factory=dict)
    ports: tuple[_ProvisionPort, ...] = ()
    extensions: _ProvisionExtensions = msgspec.field(default_factory=_ProvisionExtensions)
    tools: _ProvisionTools = msgspec.field(default_factory=_ProvisionTools)


# --- [CONSTANTS] ------------------------------------------------------------------------

_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
_JSON_VERBS: Final[frozenset[str]] = frozenset({
    "up",
    "down",
    "status",
    "doctor",
    "ports",
    "inventory",
    "extensions",
    "plan",
    "env",
    "check",
    "apply",
    "tools",
})
_VERB_MODE: Final[dict[str, Mode]] = {"up": Mode.WRITE, "down": Mode.WRITE, "apply": Mode.WRITE}
_VERB_TIMEOUT: Final[dict[str, float]] = {"up": 300.0, "check": 180.0, "apply": 180.0}
_FACT_FIELDS: Final[tuple[tuple[str, str], ...]] = (("schemaVersion", "schema_version"), ("ok", "ok"), ("state", "state"))
_SENSITIVE_KEY_FRAGMENTS: Final[frozenset[str]] = frozenset({"password", "token", "secret", "pgpass"})
_SENSITIVE_KEYS: Final[frozenset[str]] = frozenset({
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
})
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
_CHECK_PROBES: Final[tuple[tuple[str, tuple[str, ...]], ...]] = (
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
        f"forge-provision-{verb}",
        ("forge-provision", *(("--json", verb) if verb in _JSON_VERBS else (verb,))),
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
        return _payload_fault(outcome.argv, verb, "expected exactly one forge-provision JSON object on stdout") if required else Ok(None)
    try:
        raw: object = msgspec.json.decode(body)
    except msgspec.DecodeError as exc:
        return _payload_fault(outcome.argv, verb, f"invalid forge-provision JSON: {exc}") if required else Ok(None)
    return Ok(raw)


def _valid_error_shape(error: object) -> bool:
    match error:
        case {"code": str(), "message": str(), "exitCode": int()}:
            return True
        case _:
            return False


def _object_dict(value: object) -> dict[object, object]:
    if not isinstance(value, dict):
        return {}
    return dict(value.items())


def _canonical_carrier_skew(raw: dict[object, object]) -> str | None:
    missing = tuple(key for key in ("resources", "artifacts", "services", "ports", "extensions", "tools") if key not in raw)
    malformed_failure = raw.get("ok") is False and not _valid_error_shape(raw.get("error"))
    valid_command = isinstance(raw.get("command"), str) and bool(raw.get("command"))
    if raw.get("schemaVersion") != 3 or not valid_command:
        return None
    if not missing or malformed_failure:
        return None
    return (
        "installed forge-provision is older than the Rasm provision adapter; "
        "expected schema-v3 carriers resources, artifacts, services, ports, extensions, and tools; "
        f"missing {missing[0]}; use the packaged Parametric_Forge forge-provision after redeploy, then rerun: "
        f"uv run python -m tools.assay provision {_wire(raw.get('command')) or '<verb>'}"
    )


def _payload_sensitive_violation(raw: object) -> str | None:
    if found := _sensitive_key_path(raw):
        return f"sensitive key in forge-provision JSON: {'.'.join(found)}"
    if found := _sensitive_value_path(raw):
        return f"sensitive value in forge-provision JSON: {'.'.join(found)}"
    return None


def _payload_legacy_violation(raw: dict[object, object]) -> str | None:
    legacy = tuple(key for key in ("generated", "owned", "containers") if key in raw)
    if legacy:
        return f"legacy forge-provision top-level carrier: {legacy[0]}"
    return "legacy forge-provision top-level export key" if any(str(key).startswith("FORGE_PROVISION_") for key in raw) else None


def _payload_shape_violation(raw: dict[object, object]) -> str | None:
    warnings = raw.get("warnings", [])
    warning_rows = warnings if isinstance(warnings, list) else ()
    project = raw.get("project")
    checks = (
        (raw.get("schemaVersion") == 3, f"unsupported forge-provision schema: schemaVersion={raw.get('schemaVersion')!r}"),
        (isinstance(raw.get("command"), str) and bool(raw.get("command")), "forge-provision JSON missing command"),
        (isinstance(raw.get("ok"), bool), "forge-provision JSON missing boolean ok"),
        (
            raw.get("ok") is not False or _valid_error_shape(raw.get("error")),
            "forge-provision ok:false requires error.code, error.message, and error.exitCode",
        ),
        (isinstance(raw.get("warnings"), list), "forge-provision JSON missing warnings array"),
        (
            all(isinstance(row, dict) and isinstance(row.get("message"), str) for row in warning_rows),
            "forge-provision warnings must be objects with message",
        ),
        (isinstance(project, dict), "forge-provision JSON missing project object"),
        (
            isinstance(project, dict)
            and all(isinstance(project.get(key), str) and project.get(key) for key in ("rootKey", "projectKey", "instance", "composeProject")),
            "forge-provision project requires rootKey, projectKey, instance, and composeProject",
        ),
        (isinstance(raw.get("resources"), dict), "forge-provision JSON missing resources object"),
        (isinstance(raw.get("artifacts"), dict), "forge-provision JSON missing artifacts object"),
        (isinstance(raw.get("services"), dict), "forge-provision JSON missing services object"),
        (isinstance(raw.get("ports"), list), "forge-provision JSON missing ports array"),
        (isinstance(raw.get("extensions"), dict), "forge-provision JSON missing extensions object"),
        (isinstance(raw.get("tools"), dict), "forge-provision JSON missing tools object"),
    )
    return next((message for valid, message in checks if not valid), None)


def _payload_wire_violation(raw: object) -> str | None:
    if sensitive := _payload_sensitive_violation(raw):
        return sensitive
    if not isinstance(raw, dict):
        return "forge-provision JSON must be an object"
    payload = _object_dict(raw)
    return _payload_legacy_violation(payload) or _canonical_carrier_skew(payload) or _payload_shape_violation(payload)


def _validate_payload_wire(verb: str, outcome: Completed, raw: object) -> Result[None, Fault]:
    violation = _payload_wire_violation(raw)
    return Ok(None) if violation is None else _payload_fault(outcome.argv, verb, violation).map(lambda _: None)


def _decode_payload(verb: str, outcome: Completed | None) -> Result[_ProvisionPayload | None, Fault]:
    if outcome is None:
        return Error(Fault(("forge-provision", verb), RailStatus.FAULTED, f"{Step.PARSE}: missing forge-provision outcome"))
    body = outcome.stdout.strip()
    required = verb in _JSON_VERBS
    return _decode_raw_payload(verb, outcome, body, required=required).bind(
        lambda raw: Ok(None) if raw is None else _decode_validated_payload(verb, outcome, body, raw, required=required)
    )


def _raw_command_mismatch(raw: object, verb: str) -> bool:
    return isinstance(raw, dict) and isinstance(raw.get("command"), str) and bool(raw.get("command")) and raw.get("command") != verb


def _raw_command(raw: object) -> str:
    return str(raw.get("command")) if isinstance(raw, dict) and isinstance(raw.get("command"), str) else ""


def _decode_validated_payload(verb: str, outcome: Completed, body: bytes, raw: object, *, required: bool) -> Result[_ProvisionPayload | None, Fault]:
    if required and _raw_command_mismatch(raw, verb):
        return _payload_fault(outcome.argv, verb, f"forge-provision JSON command={_raw_command(raw)}")
    validated = _validate_payload_wire(verb, outcome, raw)
    if validated.is_error():
        return Error(validated.error)
    try:
        payload = _PAYLOAD_DECODER.decode(body)
    except msgspec.DecodeError as exc:
        return _payload_fault(outcome.argv, verb, f"invalid forge-provision JSON: {exc}") if required else Ok(None)
    if required and payload.command != verb:
        return _payload_fault(outcome.argv, verb, f"forge-provision JSON command={payload.command}")
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
        else tuple((key, value) for key, value in (("code", err.code), ("message", err.message), ("exitCode", _wire(err.exit_code))) if value)
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
            ("seedFingerprint", policy.seed_fingerprint),
        )
        if value
    )


def _provision_scope(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    auth_mode, auth_risk = _auth(payload)
    project = payload.project
    return tuple(
        (key, value)
        for key, value in (
            ("rootKey", project.root_key),
            ("projectKey", project.project_key),
            ("instance", project.instance),
            ("composeProject", project.compose_project),
            ("authMode", auth_mode),
            ("authRisk", auth_risk),
        )
        if value
    )


def _services(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str], ...]:
    return tuple((key, _wire(row.enabled), row.profile, str(row.port), row.image) for key, row in sorted(payload.services.items()))


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
        (key, _wire(row.enabled), row.profile, row.image, str(row.port), row.state, row.health) for key, row in sorted(payload.services.items())
    )


def _ports(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str], ...]:
    return tuple(
        (row.service, row.env, _wire(row.value), row.state, _wire(row.occupied), row.owner_class or row.owner, row.port_source)
        for row in sorted(payload.ports, key=lambda row: (row.service, row.env, row.value, row.state))
    )


def _verify_extensions(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str], ...]:
    return tuple(
        (row.service, row.extension, row.state, _wire(row.version), _wire(row.category), "required" if row.required else "optional")
        for row in sorted(payload.extensions.results, key=lambda row: (row.service, row.extension, row.state, row.version or ""))
        if row.state
    )


def _extension_sort_key(row: _ProvisionExtension) -> tuple[str, str, str, bool, str]:
    return (row.service, row.extension, row.category or "", not row.required, row.create_policy)


def _catalog_extensions(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str, str, str], ...]:
    return tuple(
        (
            row.service,
            row.extension,
            _wire(row.category),
            "required" if row.required else "optional",
            row.create_policy or ("apply-create" if row.create_on_apply else "probe-only"),
            row.risk_class,
            _wire(row.source_package),
            _wire(row.preload_required),
        )
        for row in sorted(payload.extensions.catalog, key=_extension_sort_key)
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
        for row in sorted(payload.extensions.catalog, key=_extension_sort_key)
        if any((
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
        ))
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
        for row in sorted(payload.extensions.catalog, key=_extension_sort_key)
        if (row.requires_superuser or row.requires_shared_preload or row.file_access or row.network_access or row.background_worker)
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
        for row in sorted(payload.extensions.catalog, key=lambda row: (row.surface, row.database, row.extension))
        if row.kind == "tool-extension" or row.surface or row.database
    )


def _dict_child(source: object, key: str) -> dict[str, object]:
    value = source.get(key) if isinstance(source, dict) else None
    return {row_key: row_value for row_key, row_value in value.items() if isinstance(row_key, str)} if isinstance(value, dict) else {}


def _doctor(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    runtime = payload.resources.runtime
    docker = _dict_child(runtime, "docker")
    colima = _dict_child(runtime, "colima")
    runtime_facts = {
        "dockerPolicyStatus": _wire(_dict_child(docker, "policy").get("status")),
        "dockerPolicyReason": _wire(_dict_child(docker, "policy").get("reason")),
        "dockerEndpointKind": _wire(docker.get("endpointKind")),
        "dockerExecutablePresent": _wire(docker.get("executablePresent", docker.get("present"))),
        "dockerExecutableKind": _wire(docker.get("executableKind")),
        "dockerComposeVersion": _wire(docker.get("compose")),
        "dockerServerVersion": _wire(docker.get("server")),
        "anonymousPullConfig": _wire(_dict_child(docker, "anonymousPullConfig").get("exists")),
        "credentialHelperPresent": _wire(_dict_child(docker, "hostConfig").get("credentialHelperPresent")),
        "runtimeForgeProvisionPresent": _wire(_dict_child(runtime, "forgeProvision").get("present")),
        "runtimeForgeProvisionSchemaVersion": _wire(_dict_child(runtime, "forgeProvision").get("schemaVersion")),
        "runtimeDockerPresent": _wire(docker.get("present")),
        "runtimeComposePresent": _wire(_dict_child(runtime, "compose").get("present")),
        "runtimeComposeVersion": _wire(_dict_child(runtime, "compose").get("version")),
        "runtimeJqPresent": _wire(_dict_child(runtime, "jq").get("present")),
        "runtimeListenerProbeMethod": _wire(runtime.get("listenerProbeMethod")),
        "runtimeAnonymousDockerConfig": _wire(runtime.get("anonymousDockerConfig")),
        "runtimeHostCredentialHelperPresent": _wire(runtime.get("hostCredentialHelperPresent")),
        "lockState": _wire(_dict_child(runtime, "lock").get("state")),
        "lockPresent": _wire(_dict_child(runtime, "lock").get("present")),
        "lockActive": _wire(_dict_child(runtime, "lock").get("active")),
        "lockPidAlive": _wire(_dict_child(runtime, "lock").get("pidAlive")),
        "lockHeartbeatStale": _wire(_dict_child(runtime, "lock").get("heartbeatStale")),
        "colimaAvailable": _wire(colima.get("available")),
        "colimaRunning": _wire(_dict_child(colima, "status").get("running")),
        "colimaRuntime": _wire(_dict_child(colima, "status").get("runtime")),
        "colimaArch": _wire(_dict_child(colima, "status").get("arch")),
    }
    return tuple((key, value) for key, value in runtime_facts.items() if value)


def _tool_surface_status(value: object) -> str:
    return "ok" if isinstance(value, dict) and value.get("ok") is True else "failed"


def _tool_surface_executable(value: object) -> str:
    return _wire(value.get("executable") if isinstance(value, dict) else "")


def _tool_surface_probe_value(value: object) -> str:
    if not isinstance(value, dict):
        return ""
    probe = value.get("probe")
    if isinstance(probe, dict):
        return _wire(probe.get("extensionRows"))
    return _wire(value.get("executable"))


def _decode_and_project(verb: str, done: tuple[Completed, ...], stack: Completed | None) -> Result[ProvisionRun, Fault]:
    return _decode_payload(verb, stack).map(lambda payload: _project(verb, done, payload))


def _decode_check_detail(done: tuple[Completed, ...], stack: Completed | None, tools_outcome: Completed | None) -> Result[ProvisionRun, Fault]:
    return _decode_payload("check", stack).bind(
        lambda payload: _decode_payload("tools", tools_outcome).map(
            lambda tool_payload: _project("check", done, _merge_tool_payload(payload, tool_payload))
        )
    )


def _probe_name(argv: tuple[str, ...]) -> str:
    return next((name for name, prefix in _CHECK_PROBES if argv[: len(prefix)] == prefix), " ".join(argv[:2]))


def _probe_text(outcome: Completed) -> str:
    text = (outcome.stdout or outcome.stderr).decode(errors="replace").strip().splitlines()
    return _safe_wire(text[0]) if text else ""


def _local_probes(done: tuple[Completed, ...]) -> tuple[tuple[str, str], ...]:
    return tuple(
        (_probe_name(outcome.argv), "ok" if outcome.returncode == 0 else "failed") for outcome in done if outcome.argv[:1] != ("forge-provision",)
    )


def _local_probe_values(done: tuple[Completed, ...]) -> tuple[tuple[str, str, str], ...]:
    return tuple(
        (_probe_name(outcome.argv), "ok" if outcome.returncode == 0 else "failed", value)
        for outcome in done
        if outcome.argv[:1] != ("forge-provision",) and outcome.returncode == 0 and (value := _probe_text(outcome))
    )


def _validate_local_probe_values(done: tuple[Completed, ...]) -> Result[None, Fault]:
    for outcome in done:
        if outcome.argv[:1] == ("forge-provision",) or outcome.returncode != 0:
            continue
        value = _probe_text(outcome)
        if value and _sensitive_value_path({"value": value}):
            return Error(Fault(outcome.argv, RailStatus.FAULTED, f"{Step.PARSE}: {_probe_name(outcome.argv)}: sensitive local probe value"))
    return Ok(None)


def _payload_probe_values(payload: _ProvisionPayload) -> tuple[tuple[str, str, str], ...]:
    return tuple(
        (f"forge-tools-{key}", _tool_surface_status(value), _tool_surface_executable(value)) for key, value in sorted(payload.tools.surfaces.items())
    )


def _resource_counts(payload: _ProvisionPayload) -> tuple[tuple[str, int], ...]:
    return tuple(sorted(payload.resources.counts.items()))


def _resource_inventory(payload: _ProvisionPayload) -> tuple[tuple[str, int], ...]:
    owned = payload.resources.owned
    return (
        ("containers", len(owned.containers)),
        ("volumes", len(owned.volumes)),
        ("networks", len(owned.networks)),
        ("images", len(payload.resources.images)),
        ("dockerDisk", len(payload.resources.docker_disk)),
    )


def _generated_artifacts(payload: _ProvisionPayload) -> tuple[tuple[str, str, str], ...]:
    return tuple((_wire(row.get("kind")), _wire(row.get("type")), _wire(row.get("exists"))) for row in payload.artifacts.generated)


def _extension_summary(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    return tuple((key, _wire(value)) for key, value in sorted(payload.extensions.summary.items()))


def _tool_summary(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    return tuple((key, _wire(value)) for key, value in sorted(payload.tools.summary.items()))


def _plan_summary(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    plan = payload.artifacts.plan
    if not isinstance(plan, dict):
        return ()
    return tuple(
        (key, _wire(value))
        for key, value in sorted(plan.items())
        if key != "composeYaml" and isinstance(key, str) and isinstance(value, str | int | float | bool | type(None))
    )


def _tool_surfaces(payload: _ProvisionPayload) -> tuple[tuple[str, str, str], ...]:
    return tuple((key, _tool_surface_status(value), _tool_surface_probe_value(value)) for key, value in sorted(payload.tools.surfaces.items()))


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
            warnings=tuple(row.message for row in payload.warnings),
            error=_error(payload),
            auth_mode=auth_mode,
            auth_risk=auth_risk,
            port_policy=_port_policy(payload),
            provision_scope=_provision_scope(payload),
            local_service_topology=_local_service_topology(payload),
            service_roles=_service_roles(payload),
            resource_counts=_resource_counts(payload),
            resource_inventory=_resource_inventory(payload),
            generated_artifacts=_generated_artifacts(payload),
            facts=_facts(payload),
            summary=tuple(sorted(payload.extensions.summary.items())),
            extension_summary=_extension_summary(payload),
            tool_summary=_tool_summary(payload),
            plan_summary=_plan_summary(payload),
            tool_surfaces=_tool_surfaces(payload),
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


def _forge_outcome_failure(outcome: Completed) -> tuple[int, bool]:
    try:
        raw = msgspec.json.decode(outcome.stdout.strip())
    except msgspec.DecodeError:
        return (outcome.returncode or 1, outcome.returncode != 0)
    if not isinstance(raw, dict):
        return (outcome.returncode or 1, outcome.returncode != 0)
    if raw.get("ok") is not False:
        return (0, False)
    error = raw.get("error")
    exit_code = error.get("exitCode") if isinstance(error, dict) else None
    return (exit_code if isinstance(exit_code, int) and exit_code else 1, True)


def _sanitize_provision_outcome(_detail: ProvisionRun, outcome: Completed) -> Completed:
    if outcome.argv[:1] == ("forge-provision",):
        failed_exit, failed = _forge_outcome_failure(outcome)
        if failed:
            return msgspec.structs.replace(
                outcome, returncode=failed_exit, status=RailStatus.FAILED, stdout=b"", stderr=b"provision result failed", artifacts=()
            )
        return msgspec.structs.replace(outcome, stdout=b"", stderr=b"", artifacts=())
    if outcome.returncode == 0:
        return msgspec.structs.replace(outcome, stderr=b"", artifacts=())
    return msgspec.structs.replace(outcome, stdout=b"", stderr=f"provision probe failed: {_probe_name(outcome.argv)}".encode(), artifacts=())


def _stack_outcome(done: tuple[Completed, ...], verb: str) -> Completed | None:
    return next((outcome for outcome in done if outcome.argv[:1] == ("forge-provision",) and outcome.argv[-1:] == (verb,)), None)


def _merge_tool_payload(payload: _ProvisionPayload | None, tool_payload: _ProvisionPayload | None) -> _ProvisionPayload | None:
    if payload is None or tool_payload is None:
        return payload
    return msgspec.structs.replace(payload, tools=tool_payload.tools)


def _detail(verb: str, done: tuple[Completed, ...]) -> Result[ProvisionRun, Fault]:
    stack = _stack_outcome(done, verb)
    if verb != "check":
        return _validate_local_probe_values(done).bind(lambda _: _decode_and_project(verb, done, stack))
    tools_outcome = _stack_outcome(done, "tools")
    return _validate_local_probe_values(done).bind(lambda _: _decode_check_detail(done, stack, tools_outcome))


def _run(settings: AssaySettings, scope: ArtifactScope, verb: str, tools: tuple[Tool, ...]) -> Result[Report, Fault]:
    outcomes = sequence(block.of_seq(fan_out(tuple(Check(tool=tool) for tool in tools), settings=settings, scope=scope, routed=_ROUTED)))
    return outcomes.bind(
        lambda done: _detail(verb, tuple(done)).map(
            lambda detail: fold(
                Claim.PROVISION, verb, tuple(_sanitize_provision_outcome(detail, outcome) for outcome in done), detail=detail, promote_empty=True
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


def check(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Check provisioning extensions and local runtime probes.

    Returns:
        Provision report, or provisioning fault.
    """
    return _run(settings, scope, "check", (_stack("check"), _stack("tools"), *tuple(starmap(_tool, _CHECK_PROBES))))


def apply(settings: AssaySettings, scope: ArtifactScope, _params: ProvisionParams) -> Result[Report, Fault]:
    """Apply Forge-owned provisioning extension changes.

    Returns:
        Provision report, or provisioning fault.
    """
    return _invoke(settings, scope, "apply")


__all__ = ["ProvisionParams", "apply", "check", "doctor", "down", "env", "extensions", "inventory", "plan", "ports", "status", "up"]

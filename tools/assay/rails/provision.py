"""Run local provisioning infrastructure through the Forge-owned CLI."""

from dataclasses import dataclass
from itertools import starmap
from typing import Final, override

from expression import Error, Ok, Result  # noqa: TC002  # beartype resolves handler return annotations at registry runtime
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
    enabled: bool = False
    profile: str = ""
    image: str = ""
    port: int = 0


class _ProvisionPort(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    service: str = ""
    state: str = ""
    owner: str = ""


class _ProvisionExtension(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    service: str = ""
    extension: str = ""
    state: str = ""
    version: str | None = None
    category: str | None = None
    required: bool = False
    create_on_verify: bool = False


class _ProvisionPayload(msgspec.Struct, frozen=True, gc=False, rename="camel"):
    command: str = ""
    state: str = ""
    project: str = ""
    root_fingerprint: str = ""
    docker_available: bool | None = None
    ports_inspectable: bool | None = None
    ports_usable: bool | None = None
    non_owned_cleanup_policy: str = ""
    summary: dict[str, int] = msgspec.field(default_factory=dict)
    services: dict[str, _ProvisionService] = msgspec.field(default_factory=dict)
    ports: tuple[_ProvisionPort, ...] = ()
    extensions: tuple[_ProvisionExtension, ...] = ()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ROUTED: Final[Routed] = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
_JSON_VERBS: Final[frozenset[str]] = frozenset({"status", "doctor", "ports", "inventory", "extensions", "env", "verify"})
_VERB_MODE: Final[dict[str, Mode]] = {"up": Mode.WRITE, "down": Mode.WRITE, "verify": Mode.VERIFY}
_VERB_TIMEOUT: Final[dict[str, float]] = {"up": 300.0, "verify": 180.0}
_FACT_FIELDS: Final[tuple[tuple[str, str], ...]] = (
    ("state", "state"),
    ("project", "project"),
    ("rootFingerprint", "root_fingerprint"),
    ("dockerAvailable", "docker_available"),
    ("portsInspectable", "ports_inspectable"),
    ("portsUsable", "ports_usable"),
    ("nonOwnedCleanupPolicy", "non_owned_cleanup_policy"),
)
_PYTHON_ABI_PROBE: Final[str] = "import sys, sysconfig; print(sys.implementation.cache_tag, sysconfig.get_config_var('Py_GIL_DISABLED') or 0)"
_ONNXRUNTIME_LIB_PROBE: Final[str] = 'test -n "${ONNXRUNTIME_LIB:-}" && test -e "$ONNXRUNTIME_LIB" && printf "%s\\n" "$ONNXRUNTIME_LIB"'
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
        ("rasm-provision", verb, *(("--json",) if verb in _JSON_VERBS else ())),
        mode=_VERB_MODE.get(verb, Mode.RUN),
        timeout=_VERB_TIMEOUT.get(verb, 120.0),
    )


def _decode_payload(verb: str, outcome: Completed | None) -> Result[_ProvisionPayload | None, Fault]:
    if outcome is None:
        return Error(Fault(("rasm-provision", verb), RailStatus.FAULTED, f"{Step.PARSE}: missing rasm-provision outcome"))
    body = outcome.stdout.strip()
    required = verb in _JSON_VERBS and outcome.returncode == 0
    if not body.startswith(b"{"):
        return Error(Fault(outcome.argv, RailStatus.FAULTED, f"{Step.PARSE}: {verb}: expected rasm-provision JSON")) if required else Ok(None)
    try:
        payload = _PAYLOAD_DECODER.decode(body)
    except msgspec.DecodeError as exc:
        return Error(Fault(outcome.argv, RailStatus.FAULTED, f"{Step.PARSE}: {verb}: invalid rasm-provision JSON: {exc}")) if required else Ok(None)
    if required and payload.command not in {"", verb}:
        return Error(Fault(outcome.argv, RailStatus.FAULTED, f"{Step.PARSE}: {verb}: rasm-provision JSON command={payload.command}"))
    return Ok(payload)


def _wire(value: object) -> str:
    match value:
        case None | "":
            return ""
        case bool() as bit:
            return str(bit).lower()
        case _:
            return str(value)


def _facts(payload: _ProvisionPayload) -> tuple[tuple[str, str], ...]:
    return tuple((wire, _wire(getattr(payload, attr))) for wire, attr in _FACT_FIELDS if _wire(getattr(payload, attr)))


def _services(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str], ...]:
    return tuple(
        (key, _wire(row.enabled), row.profile, str(row.port), row.image)
        for key, row in sorted(payload.services.items())
    )


def _ports(payload: _ProvisionPayload) -> tuple[tuple[str, str, str], ...]:
    return tuple((row.service, row.state, row.owner) for row in payload.ports)


def _verify_extensions(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str, str], ...]:
    return tuple(
        (row.service, row.extension, row.state, _wire(row.version), _wire(row.category), "required" if row.required else "optional")
        for row in payload.extensions
        if row.state
    )


def _catalog_extensions(payload: _ProvisionPayload) -> tuple[tuple[str, str, str, str, str], ...]:
    return tuple(
        (
            row.service,
            row.extension,
            _wire(row.category),
            "required" if row.required else "optional",
            "create-on-verify" if row.create_on_verify else "probe-only",
        )
        for row in payload.extensions
        if not row.state
    )


def _probe_name(argv: tuple[str, ...]) -> str:
    return next((name for name, prefix in _VERIFY_PROBES if argv[: len(prefix)] == prefix), " ".join(argv[:2]))


def _probe_text(outcome: Completed) -> str:
    text = (outcome.stdout or outcome.stderr).decode(errors="replace").strip().splitlines()
    return text[0][:512] if text else ""


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
        if outcome.argv[:1] != ("rasm-provision",) and (value := _probe_text(outcome))
    )


def _project(verb: str, done: tuple[Completed, ...], payload: _ProvisionPayload | None) -> ProvisionRun:
    local = _local_probes(done)
    values = _local_probe_values(done)
    return (
        ProvisionRun(verb=verb, local_probes=local, local_probe_values=values)
        if payload is None
        else ProvisionRun(
            verb=verb,
            json=True,
            facts=_facts(payload),
            summary=tuple(sorted(payload.summary.items())),
            services=_services(payload),
            ports=_ports(payload),
            extensions=_verify_extensions(payload),
            extension_catalog=_catalog_extensions(payload),
            local_probes=local,
            local_probe_values=values,
        )
    )


def _detail(verb: str, done: tuple[Completed, ...]) -> Result[ProvisionRun, Fault]:
    stack = next((outcome for outcome in done if outcome.argv[:1] == ("rasm-provision",)), None)
    return _decode_payload(verb, stack).map(lambda payload: _project(verb, done, payload))


def _run(settings: AssaySettings, scope: ArtifactScope, verb: str, tools: tuple[Tool, ...]) -> Result[Report, Fault]:
    outcomes = sequence(block.of_seq(fan_out(tuple(Check(tool=tool) for tool in tools), settings=settings, scope=scope, routed=_ROUTED)))
    return outcomes.bind(
        lambda done: _detail(verb, tuple(done)).map(
            lambda detail: fold(Claim.PROVISION, verb, tuple(done), detail=detail, promote_empty=True)
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
    """Report provisioning paths and DSNs.

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

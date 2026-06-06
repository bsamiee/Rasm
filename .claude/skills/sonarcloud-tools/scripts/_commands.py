"""Command implementations for SonarCloud CLI."""

from collections import Counter
from collections.abc import Callable, Mapping
from typing import Final


type JsonMap = dict[str, object]
type QueryParams = dict[str, str | int]
type GetFn = Callable[[str, QueryParams], tuple[bool, JsonMap]]


BASE_URL: Final = "https://sonarcloud.io/api"
ORG: Final = "bsamiee"
PROJECT: Final = "bsamiee_Parametric_Portal"
DEFAULT_METRICS: Final = (
    "ncloc,coverage,bugs,vulnerabilities,code_smells,duplicated_lines_density,security_hotspots,reliability_rating,security_rating,sqale_rating"
)
DEFAULT_STATUSES: Final = "OPEN,CONFIRMED,REOPENED"


# --- [FUNCTIONS] --------------------------------------------------------------
def _json_object(value: object) -> JsonMap:
    """Normalize an arbitrary JSON value to an object.

    Returns:
        JSON object, or an empty object for non-object values.
    """
    match value:
        case Mapping() as data:
            return {str(key): item for key, item in data.items()}
        case _:
            return {}


def _json_objects(value: object) -> tuple[JsonMap, ...]:
    """Normalize an arbitrary JSON value to object rows.

    Returns:
        Tuple of JSON objects for list values.
    """
    match value:
        case list() as rows:
            return tuple(_json_object(row) for row in rows if isinstance(row, Mapping))
        case _:
            return ()


def _text(value: object) -> str:
    """Project a JSON scalar to text.

    Returns:
        String representation, or an empty string for `None`.
    """
    return "" if value is None else str(value)


def _parse_conditions(conditions: object) -> tuple[JsonMap, ...]:
    """Transform quality gate conditions into normalized format.

    Returns:
        Normalized condition rows.
    """
    return tuple(
        {
            "metric": condition.get("metricKey", ""),
            "status": condition.get("status", ""),
            "actual": condition.get("actualValue", ""),
            "threshold": condition.get("errorThreshold", condition.get("warningThreshold", "")),
        }
        for condition in _json_objects(conditions)
    )


def _summarize_issues(issues: object) -> JsonMap:
    """Group issues by severity and type via Counter (no mutable accumulators).

    Returns:
        Issue counts grouped by severity and type.
    """
    rows = _json_objects(issues)
    by_severity = Counter(_text(issue.get("severity")) or "UNKNOWN" for issue in rows)
    by_type = Counter(_text(issue.get("type")) or "UNKNOWN" for issue in rows)
    return {"by_severity": by_severity, "by_type": by_type}


# --- [COMMANDS] ---------------------------------------------------------------
def quality_gate(arg1: str, arg2: str, get_fn: GetFn) -> JsonMap:
    """Quality gate status.

    Args:
        arg1: Branch name or 'pr' for pull request mode.
        arg2: Pull request number when arg1 is 'pr'.
        get_fn: HTTP GET function with signature (path, params) -> (ok, data).

    Returns:
        Quality gate result object.
    """
    params: QueryParams = {"projectKey": PROJECT, "organization": ORG}
    match (arg1, arg2):
        case ("pr", number) if number:
            params["pullRequest"] = number
        case (branch, _) if branch:
            params["branch"] = branch
        case _:
            pass
    ok, data = get_fn("/qualitygates/project_status", params)
    if not ok:
        return {"status": "error", "message": data.get("error", str(data))}
    project_status = _json_object(data.get("projectStatus"))
    return {
        "status": "success",
        "project": PROJECT,
        "gate_status": project_status.get("status", ""),
        "passed": project_status.get("status") == "OK",
        "conditions": _parse_conditions(project_status.get("conditions")),
    }


def issues(severities: str, types: str, get_fn: GetFn) -> JsonMap:
    """Search issues by severity and type.

    Args:
        severities: Comma-separated severity filter.
        types: Comma-separated type filter.
        get_fn: HTTP GET function.

    Returns:
        Issues result object with summary.
    """
    params: QueryParams = {
        "componentKeys": PROJECT,
        "organization": ORG,
        "statuses": DEFAULT_STATUSES,
        "ps": 100,
        **({"severities": severities} if severities else {}),
        **({"types": types} if types else {}),
    }
    ok, data = get_fn("/issues/search", params)
    if not ok:
        return {"status": "error", "message": data.get("error", str(data))}
    rows = _json_objects(data.get("issues"))
    paging = _json_object(data.get("paging"))
    issues_list = tuple(
        {
            "key": issue.get("key", ""),
            "rule": issue.get("rule", ""),
            "severity": issue.get("severity", ""),
            "type": issue.get("type", ""),
            "message": issue.get("message", ""),
            "component": _text(issue.get("component")).split(":")[-1],
            "line": issue.get("line"),
        }
        for issue in rows
    )
    return {"status": "success", "project": PROJECT, "total": paging.get("total", 0), "issues": issues_list, "summary": _summarize_issues(rows)}


def measures(metrics: str, get_fn: GetFn) -> JsonMap:
    """Project metrics.

    Args:
        metrics: Comma-separated metric keys (default: all).
        get_fn: HTTP GET function.

    Returns:
        Metrics result object.
    """
    params: QueryParams = {"component": PROJECT, "organization": ORG, "metricKeys": metrics or DEFAULT_METRICS}
    ok, data = get_fn("/measures/component", params)
    if not ok:
        return {"status": "error", "message": data.get("error", str(data))}
    component = _json_object(data.get("component"))
    return {
        "status": "success",
        "project": component.get("key", PROJECT),
        "name": component.get("name", ""),
        "metrics": {
            _text(measure.get("metric")): measure.get("value", _json_object(measure.get("period")).get("value", "N/A"))
            for measure in _json_objects(component.get("measures"))
        },
    }


def analyses(page_size: str, get_fn: GetFn) -> JsonMap:
    """Analysis history.

    Args:
        page_size: Number of results per page (default: 10).
        get_fn: HTTP GET function.

    Returns:
        Analysis history object.
    """
    size = int(page_size) if page_size else 10
    params: QueryParams = {"project": PROJECT, "organization": ORG, "ps": min(size, 100)}
    ok, data = get_fn("/project_analyses/search", params)
    if not ok:
        return {"status": "error", "message": data.get("error", str(data))}
    paging = _json_object(data.get("paging"))
    return {
        "status": "success",
        "project": PROJECT,
        "total": paging.get("total", 0),
        "analyses": tuple(
            {
                "key": analysis.get("key", ""),
                "date": analysis.get("date", ""),
                "events": tuple(
                    {"category": event.get("category", ""), "name": event.get("name", "")} for event in _json_objects(analysis.get("events"))
                ),
            }
            for analysis in _json_objects(data.get("analyses"))
        ),
    }


def projects(page_size: str, get_fn: GetFn) -> JsonMap:
    """List organization projects.

    Args:
        page_size: Number of results per page (default: 100).
        get_fn: HTTP GET function.

    Returns:
        Projects result object.
    """
    size = int(page_size) if page_size else 100
    params: QueryParams = {"organization": ORG, "ps": min(size, 500)}
    ok, data = get_fn("/projects/search", params)
    if not ok:
        return {"status": "error", "message": data.get("error", str(data))}
    paging = _json_object(data.get("paging"))
    return {
        "status": "success",
        "organization": ORG,
        "total": paging.get("total", 0),
        "projects": tuple({"key": project.get("key", ""), "name": project.get("name", "")} for project in _json_objects(data.get("components"))),
    }


def hotspots(status: str, get_fn: GetFn) -> JsonMap:
    """Security hotspots.

    Args:
        status: Filter by status (TO_REVIEW|ACKNOWLEDGED|FIXED|SAFE).
        get_fn: HTTP GET function.

    Returns:
        Hotspots result object.
    """
    params: QueryParams = {"projectKey": PROJECT, "organization": ORG, "ps": 100, **({"status": status} if status else {})}
    ok, data = get_fn("/hotspots/search", params)
    if not ok:
        return {"status": "error", "message": data.get("error", str(data))}
    paging = _json_object(data.get("paging"))
    return {
        "status": "success",
        "project": PROJECT,
        "total": paging.get("total", 0),
        "hotspots": tuple(
            {
                "key": hotspot.get("key", ""),
                "message": hotspot.get("message", ""),
                "status": hotspot.get("status", ""),
                "probability": hotspot.get("vulnerabilityProbability", ""),
                "component": _text(hotspot.get("component")).split(":")[-1],
                "line": hotspot.get("line"),
            }
            for hotspot in _json_objects(data.get("hotspots"))
        ),
    }

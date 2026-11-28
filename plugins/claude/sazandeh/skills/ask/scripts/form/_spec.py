"""Spec models, their validators, and the faults of a refused spec."""

from collections import Counter
from enum import StrEnum
from itertools import chain
import os
from pathlib import Path
from typing import Annotated, Self
lazy from collections.abc import Iterable

from expression import Error, Ok, Result
from markdown_it import MarkdownIt
from markdown_it.tree import SyntaxTreeNode
from mdit_py_plugins.front_matter import front_matter_plugin
import msgspec
from pydantic import AfterValidator, BaseModel, BeforeValidator, DirectoryPath, Discriminator, Field, FilePath, model_validator, StringConstraints, Tag, TypeAdapter, ValidationError
from pydantic_core import ErrorDetails, PydanticCustomError
import regex
from ruamel.yaml import YAML
from ruamel.yaml.constructor import SafeConstructor
from ruamel.yaml.error import MarkedYAMLError, YAMLError
import wcwidth

# --- [TYPES] ----------------------------------------------------------------------------


class Reason(StrEnum):
    SCHEMA = "schema"
    SPEC_FORM = "spec_form"
    ASK_FORM = "ask_form"
    SHOW_FORM = "show_form"
    KEY_UNKNOWN = "key_unknown"
    KEY_REPEATED = "key_repeated"
    EMOJI = "emoji"
    HEADER_UNITS = "header_units"
    LABEL_COMMA = "label_comma"
    LABEL_RESERVED = "label_reserved"
    LABEL_CELLS = "label_cells"
    RECOMMENDED_UNKNOWN = "recommended_unknown"
    FACTS_KEYS = "facts_keys"
    RANGE_OPTION = "range_option"
    RANGE_FORM = "range_form"
    RANGE_UNKNOWN = "range_unknown"
    MARKDOWN_SOURCE = "markdown_source"
    PREVIEW_UNITS = "preview_units"
    PREVIEW_WITHHELD = "preview_withheld"
    DIFF_EMPTY = "diff_empty"
    WHEN_UNKNOWN = "when_unknown"
    WHEN_LABEL = "when_label"
    WHEN_MULTIPLE = "when_multiple"
    HEADER_REPEATED = "header_repeated"
    QUESTION_REPEATED = "question_repeated"
    YAML = "yaml"
    UNREADABLE = "unreadable"
    GIT = "git"

    @property
    def status(self) -> int:
        match self:
            case Reason.YAML | Reason.UNREADABLE:
                return os.EX_NOINPUT
            case Reason.GIT:
                return os.EX_UNAVAILABLE
            case _:
                return os.EX_DATAERR


class Change(StrEnum):
    ADDED = "A"
    MODIFIED = "MT"
    DELETED = "D"


type Loc = tuple[str | int, ...]
type Row = tuple[str, str]
type Span = tuple[str, str | None]

# --- [CONSTANTS] ------------------------------------------------------------------------

HEADER_UNITS = 12
LABEL_CELLS = 30 - len("\N{HEAVY RIGHT-POINTING ANGLE QUOTATION MARK ORNAMENT} 1. ")
RECOMMENDED = " (Recommended)"
NEXT = "More options"
PICK = "Let me pick"
_RESERVED = frozenset(label.casefold() for label in (NEXT, PICK, "Other"))
_HIDDEN = regex.compile(r"[[\p{Cc}\p{Cf}\p{Default_Ignorable_Code_Point}\p{Zl}\p{Zp}\u2800]--[\n\t\u200c-\u200f\u061c\ufe00-\ufe0e\u180b-\u180f]]", regex.V1)
EMOJI = regex.compile(r"\p{Emoji_Presentation}|\ufe0f")
_PLAIN_SEGMENT = regex.compile(r"[\w-]+")
_PYDANTIC = frozendict({"extra_forbidden": Reason.KEY_UNKNOWN})
_CONTEXT_DROPPED = frozenset({"field_type", "class", "expected_tags", "discriminator"})


# --- [MODELS] ---------------------------------------------------------------------------

# --- [TEXT]


def clean(text: str, /) -> str:
    return _HIDDEN.sub("", wcwidth.strip_sequences(text.replace("\r\n", "\n")))


def visible(text: str, /) -> str:
    return clean(text).expandtabs()


def units(text: str, /) -> int:
    return len(text.encode("utf-16-le")) // 2


MARKDOWN = MarkdownIt("commonmark").enable(["table", "strikethrough"])
FRONTED = MarkdownIt("commonmark").enable(["table", "strikethrough"]).use(front_matter_plugin)


def flat(node: SyntaxTreeNode, /) -> str:
    inner = "".join(map(flat, node.children))
    match node.type:
        case "link" if inner == node.attrs.get("href"):
            return inner
        case "link":
            return f"{inner} ({node.attrs.get('href')})"
        case "softbreak" | "hardbreak":
            return " "
        case "text" | "code_inline" | "html_inline" | "text_special":
            return node.content
        case _:
            return inner


def plain(text: str, /) -> str:
    return " ".join(flat(SyntaxTreeNode(MARKDOWN.parseInline(visible(text)))).split())


def _refused(reason: Reason, facts: dict[str, object], /) -> PydanticCustomError:
    return PydanticCustomError(reason.value, "refused", facts)


def _line(text: str, /) -> str:
    if EMOJI.search(text):
        raise _refused(Reason.EMOJI, {"line": text})
    return plain(text)


def _header(text: str, /) -> str:
    if (count := units(text)) > HEADER_UNITS:
        raise _refused(Reason.HEADER_UNITS, {"units": count, "limit": HEADER_UNITS})
    return text


def _label(text: str, /) -> str:
    match text:
        case _ if ", " in text:
            raise _refused(Reason.LABEL_COMMA, {"label": text})
        case _ if text.casefold() in _RESERVED:
            raise _refused(Reason.LABEL_RESERVED, {"label": text})
        case _ if (cells := wcwidth.width(text)) > LABEL_CELLS:
            raise _refused(Reason.LABEL_CELLS, {"cells": cells, "limit": LABEL_CELLS})
        case _:
            return text


def _revision(text: str, /) -> str:
    if text.startswith("-"):
        raise _refused(Reason.RANGE_OPTION, {"range": text})
    return text


def _distinct(value: object, /) -> object:
    normal = {key: plain(str(key)) for key in value} if isinstance(value, dict) else {}
    counts = Counter(normal.values())
    if repeated := tuple(str(key) for key, normalized in normal.items() if counts[normalized] > 1):
        raise _refused(Reason.KEY_REPEATED, {"keys": repeated})
    return value


def _detailed(value: object, /) -> object:
    return {"does": value} if isinstance(value, str) else value


def _named(labels: tuple[str, ...], recommended: str | None, /) -> None:
    match recommended:
        case str() if recommended not in labels:
            raise _refused(Reason.RECOMMENDED_UNKNOWN, {"label": recommended, "labels": labels})
        case str() if (cells := wcwidth.width(recommended + RECOMMENDED)) > LABEL_CELLS:
            raise _refused(Reason.LABEL_CELLS, {"label": recommended + RECOMMENDED, "cells": cells, "limit": LABEL_CELLS})
        case _:
            return


type _Line = Annotated[str, AfterValidator(_line), StringConstraints(min_length=1)]
type _Header = Annotated[_Line, AfterValidator(_header)]
type _Name = Annotated[_Line, AfterValidator(_label)]
type _Key = Annotated[_Line, StringConstraints(pattern=r"^[^=]+$")]
type _Revision = Annotated[str, StringConstraints(min_length=1), AfterValidator(_revision)]
type _Facts = Annotated[frozendict[_Key, _Line], BeforeValidator(_distinct)]


class _Model(BaseModel, frozen=True, extra="forbid"):
    @classmethod
    def key(cls) -> str:
        return next(iter(cls.model_fields))

    @classmethod
    def required(cls) -> frozenset[str]:
        return frozenset(name for name, field in cls.model_fields.items() if field.is_required())


def _keyed(reason: Reason, *cases: type[_Model]) -> Discriminator:
    def keyed(value: object, /) -> str | None:
        match value:
            case dict() if len(found := {case.key() for case in cases} & value.keys()) == 1:
                fitting = tuple(case for case in cases if case.key() in found)
                return max(fitting, key=lambda case: len(case.required() & value.keys())).__name__
            case dict():
                return None
            case _:
                return next((case.__name__ for case in cases if isinstance(value, case)), None)

    return Discriminator(keyed, custom_error_type=reason, custom_error_message="refused", custom_error_context={"expected": " ".join(dict.fromkeys(case.key() for case in cases))})


# --- [CONTENT]


class Text(_Model):
    text: Annotated[str, StringConstraints(min_length=1)]


class Fields(_Model):
    fields: Annotated[_Facts, Field(min_length=1)]


class Records(_Model):
    records: Annotated[tuple[Annotated[_Facts, Field(min_length=1)], ...], Field(min_length=1)]


class Pair(_Model):
    path: Annotated[str, StringConstraints(strip_whitespace=True, min_length=1)]
    before: str
    after: str


class Diff(_Model):
    diff: Pair


class Range(_Model):
    repo: DirectoryPath = Path()
    range: _Revision
    paths: tuple[str, ...] = ()


class Git(_Model):
    git: Range


class File(_Model):
    file: FilePath


class Steps(_Model):
    steps: Annotated[tuple[_Line, ...], Field(min_length=2)]


_SHOWN = (Text, Fields, Records, Diff, Git, File, Steps)

type Content = Annotated[
    Annotated[Text, Tag("Text")]
    | Annotated[Fields, Tag("Fields")]
    | Annotated[Records, Tag("Records")]
    | Annotated[Diff, Tag("Diff")]
    | Annotated[Git, Tag("Git")]
    | Annotated[File, Tag("File")]
    | Annotated[Steps, Tag("Steps")],
    _keyed(Reason.SHOW_FORM, *_SHOWN),
]


# --- [ASKS]


class _Detail(_Model):
    does: _Line
    facts: _Facts = frozendict()
    show: Content | None = None


type _Choices = Annotated[frozendict[_Name, Annotated[_Detail, BeforeValidator(_detailed)]], BeforeValidator(_distinct), Field(min_length=2)]
type _Lines = Annotated[frozendict[_Name, _Line], BeforeValidator(_distinct), Field(min_length=2)]
type _Single = Annotated[frozendict[_Name, _Line], Field(min_length=1, max_length=1)]
type _When = Annotated[frozendict[_Header, _Name], Field(min_length=1, max_length=1)]


class Decide(_Model):
    decide: _Line
    header: _Header
    recommended: _Name | None
    show: Content | None = None
    options: _Choices
    when: _When | None = None

    @model_validator(mode="after")
    def named(self) -> Self:
        _named(tuple(self.options), self.recommended)
        keys = tuple(dict.fromkeys(chain.from_iterable(detail.facts for detail in self.options.values())))
        if missing := tuple(name for name, detail in self.options.items() if detail.facts.keys() != frozenset(keys)):
            raise _refused(Reason.FACTS_KEYS, {"keys": keys, "options": missing})
        return self


class Select(_Model):
    select: _Line
    header: _Header
    options: _Lines
    when: _When | None = None


class Consent(_Model):
    consent: _Line
    header: _Header
    recommended: _Name
    action: _Single
    safe: _Single = frozendict({"Not yet": "Leaves everything as it is"})
    facts: _Facts = frozendict()
    show: Content | None = None
    when: _When | None = None

    @model_validator(mode="after")
    def named(self) -> Self:
        _named((*self.action, *self.safe), self.recommended)
        return self


class Review(_Model):
    review: _Line
    header: _Header
    show: Content
    actions: Annotated[frozendict[_Name, _Line], BeforeValidator(_distinct), Field(min_length=2)] | None = None
    recommended: _Name | None = None
    when: _When | None = None

    @property
    def choices(self) -> tuple[Row, ...]:
        match self.actions, self.show:
            case frozendict() as actions, _:
                return tuple(actions.items())
            case None, Diff():
                return (("Apply edit", "Writes the change"), ("Leave as is", "Changes nothing"))
            case None, Git():
                return (("Keep", "Leaves the commits as they are"), ("Revert", "Stages the reverse of the range for a commit"))
            case None, File():
                return (("Save", "Writes the file"), ("Skip", "Writes nothing"))
            case _:
                return (("Accept", "Uses the content as shown"), ("Reject", "Drops the content"))

    @property
    def pick(self) -> str:
        return self.recommended or self.choices[0][0]

    @model_validator(mode="after")
    def named(self) -> Self:
        _named(tuple(label for label, _ in self.choices), self.pick)
        return self


class _Files(_Model):
    changes: DirectoryPath
    paths: tuple[str, ...] = ()
    noun: _Line | None = None
    keys: tuple[Annotated[str, StringConstraints(min_length=1)], ...] = ()
    group: bool = False


class Changes(_Files):
    into: DirectoryPath | None = None


class History(_Files):
    range: _Revision


_ASKED = (Decide, Select, Consent, Review, Changes, History)
_FORMS: frozendict[str, str] = frozendict({case.__name__: case.key() for case in (*_ASKED, *_SHOWN)})

type Ask = Annotated[
    Annotated[Decide, Tag("Decide")]
    | Annotated[Select, Tag("Select")]
    | Annotated[Consent, Tag("Consent")]
    | Annotated[Review, Tag("Review")]
    | Annotated[Changes, Tag("Changes")]
    | Annotated[History, Tag("History")],
    _keyed(Reason.ASK_FORM, *_ASKED),
]
type Files = Changes | History
type Asking = Decide | Select | Consent | Review

_SPEC: TypeAdapter[tuple[Ask, ...]] = TypeAdapter(Annotated[tuple[Ask, ...], Field(min_length=1)])


class _Source(SafeConstructor):
    pass


for _tag in ("int", "float", "timestamp"):
    _Source.add_constructor(f"tag:yaml.org,2002:{_tag}", SafeConstructor.construct_yaml_str)


# --- [ERRORS] ---------------------------------------------------------------------------


def quoted(value: object, /) -> str:
    match value:
        case str() | os.PathLike():
            return msgspec.json.encode(os.fspath(value)).decode()
        case tuple() | list():
            return " ".join(map(quoted, value))
        case _:
            return str(value)


def _segment(part: str | int, /) -> str:
    return str(part) if isinstance(part, int) or _PLAIN_SEGMENT.fullmatch(part) else msgspec.json.encode(part).decode()


class Fault(msgspec.Struct, frozen=True, kw_only=True):
    at: Loc
    kind: Reason | str
    facts: frozendict[str, object] = frozendict()

    @property
    def reason(self) -> Reason:
        match self.kind:
            case Reason():
                return self.kind
            case _:
                return Reason.SCHEMA

    @property
    def row(self) -> str:
        facts = "".join(f", {key} {quoted(value)}" for key, value in self.facts.items())
        return f"  {'.'.join(map(_segment, self.at)) or 'spec'}: {self.kind}{facts}\n"

    @classmethod
    def of(cls, at: Loc, kind: Reason, /, **facts: object) -> Faults:
        return (cls(at=at, kind=kind, facts=frozendict(facts)),)


type Faults = tuple[Fault, *tuple[Fault, ...]]
type Outcome[T] = Result[T, Faults]


# --- [OPERATIONS] -----------------------------------------------------------------------


def gathered[T](outcomes: Iterable[Outcome[T]], /) -> Outcome[tuple[T, ...]]:
    held = tuple(outcomes)
    match tuple(chain.from_iterable(outcome.swap().default_value(()) for outcome in held)):
        case (head, *rest):
            return Error((head, *rest))
        case _:
            return Ok(tuple(*outcome.to_option().to_list() for outcome in held))


def _located(loc: Loc, /) -> Loc:
    named = tuple(_FORMS.get(part, part) if isinstance(part, str) else part for part in loc if part != "[key]")
    return tuple(part for index, part in enumerate(named) if not index or part != named[index - 1])


def _fault(detail: ErrorDetails, /) -> Fault:
    kind = detail["type"]
    reason = Reason(kind) if kind in Reason else _PYDANTIC.get(kind, kind)
    match reason, detail["loc"]:
        case "tuple_type", ():
            return Fault(at=(), kind=Reason.SPEC_FORM, facts=frozendict(expected="sequence of asks"))
        case _, loc:
            return Fault(at=_located(loc), kind=reason, facts=frozendict((key, value) for key, value in detail.get("ctx", {}).items() if key not in _CONTEXT_DROPPED))


def _faulted(error: ValidationError, /) -> Faults:
    faults = tuple(map(_fault, error.errors(include_url=False, include_input=False)))
    held = frozenset(fault.at for fault in faults)
    head, *rest = (fault for fault in faults if not any(other != fault.at and other[: len(fault.at)] == fault.at for other in held))
    return (head, *rest)


def loaded(spec: Path, /) -> Outcome[tuple[Ask, ...]]:
    reader = YAML(typ="safe", pure=True)
    reader.Constructor = _Source
    try:
        data = reader.load(spec.read_text(encoding="utf-8"))
    except MarkedYAMLError as error:
        mark = error.problem_mark
        return Error(Fault.of((), Reason.YAML, problem=str(error.problem), line=mark.line + 1 if mark else 0))
    except (OSError, UnicodeDecodeError, YAMLError) as error:
        return Error(Fault.of((), Reason.UNREADABLE, cause=type(error).__name__))
    try:
        return Ok(_SPEC.validate_python(data))
    except ValidationError as error:
        return Error(_faulted(error))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "EMOJI",
    "FRONTED",
    "HEADER_UNITS",
    "LABEL_CELLS",
    "MARKDOWN",
    "NEXT",
    "PICK",
    "RECOMMENDED",
    "Ask",
    "Asking",
    "Change",
    "Changes",
    "Consent",
    "Content",
    "Decide",
    "Diff",
    "Fault",
    "Faults",
    "Fields",
    "File",
    "Files",
    "Git",
    "History",
    "Loc",
    "Outcome",
    "Pair",
    "Range",
    "Reason",
    "Records",
    "Review",
    "Row",
    "Select",
    "Span",
    "Steps",
    "Text",
    "clean",
    "flat",
    "gathered",
    "loaded",
    "plain",
    "quoted",
    "units",
    "visible",
]

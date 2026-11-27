"""Wheel build hook generating the PyOpenColorIO stubs with pybind11-stubgen from the OpenColorIO the build environment holds."""

import inspect
from itertools import accumulate, pairwise
import logging
from pathlib import Path
import re
from tempfile import TemporaryDirectory
import types
from typing import override

from hatchling.builders.config import BuilderConfig
from hatchling.builders.hooks.plugin.interface import BuildHookInterface
from pybind11_stubgen import run
from pybind11_stubgen.parser.interface import IParser
from pybind11_stubgen.parser.mixins.error_handlers import LogErrors, logger, LoggerData, SuggestCxxSignatureFix
from pybind11_stubgen.parser.mixins.filter import (
    FilterClassMembers,
    FilterInvalidIdentifiers,
    FilterPybind11NativeEnumMembers,
    FilterPybind11ViewClasses,
    FilterPybindInternals,
    FilterTypingModuleAttributes,
)
from pybind11_stubgen.parser.mixins.fix import (
    FixBuiltinTypes,
    FixCurrentModulePrefixInTypeNames,
    FixMissing__all__Attribute,
    FixMissing__future__AnnotationsImport,
    FixMissingEnumMembersAnnotation,
    FixMissingFieldDocString,
    FixMissingFixedSizeImport,
    FixMissingImports,
    FixMissingNoneHashFieldAnnotation,
    FixNumpyArrayFlags,
    FixNumpyDtype,
    FixPEP585CollectionNames,
    FixPybind11EnumStrDoc,
    FixRedundantBuiltinsAnnotation,
    FixRedundantMethodsFromBuiltinObject,
    FixScipyTypeArguments,
    FixTypingTypeNames,
    FixValueReprRandomAddress,
    OverridePrintSafeValues,
    RemoveSelfAnnotation,
    ReplaceReadWritePropertyWithField,
    RewritePybind11EnumValueRepr,
)
from pybind11_stubgen.parser.mixins.parse import BaseParser, ExtractSignaturesFromPybind11Docstrings, ParserDispatchMixin
from pybind11_stubgen.printer import Printer
from pybind11_stubgen.structs import Argument, InvalidExpression, Module, QualifiedName, ResolvedType, Value
from pybind11_stubgen.writer import Writer

# --- [CONSTANTS] ------------------------------------------------------------------------

_MODULE = "PyOpenColorIO"
_SUFFIX = "-stubs"
_CXX_NAME = re.compile(r"\w+(?:::\w+)+")
_OBJECT_DEFAULT = re.compile(r"=\s*<")
_RECORD_FORMAT = "%(name)s - [%(levelname)7s] %(message)s"

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ordered(line: str, /) -> str:
    """Drops the default of each parameter a required one follows, the order pybind11 binds and Python refuses."""
    head, opened, rest = line.partition("(")
    depths = tuple(accumulate(1 if char in "([{" else -1 if char in ")]}" else 0 for char in rest))
    if not (head.lstrip().startswith("def ") and opened and -1 in depths):
        return line
    close = depths.index(-1)
    cuts = (-1, *(index for index, (char, depth) in enumerate(zip(rest[:close], depths, strict=False)) if char == "," and depth == 0), close)
    parameters = tuple(rest[start + 1 : end].strip() for start, end in pairwise(cuts))
    last = max((index for index, parameter in enumerate(parameters) if "=" not in parameter and not parameter.startswith(("*", "/"))), default=-1)
    required = tuple(parameter.partition("=")[0].rstrip() if index < last else parameter for index, parameter in enumerate(parameters))
    return line if required == parameters else f"{head}({', '.join(required)}{rest[close:]}"


def _opaque(enum: re.Pattern[str], args: str, /) -> str:
    """Replaces each default pybind11 printed as an object repr with `...`, keeping the enum member reprs pybind11-stubgen resolves."""
    opened = _OBJECT_DEFAULT.search(args)
    if opened is None:
        return args
    start = opened.end() - 1
    depths = tuple(accumulate(1 if char == "<" else -1 if char == ">" else 0 for char in args[start:]))
    if 0 not in depths:
        return args
    end = start + depths.index(0) + 1
    return f"{args[:start]}{args[start:end] if enum.fullmatch(args[start:end]) else '...'}{_opaque(enum, args[end:])}"


# --- [PARSERS] --------------------------------------------------------------------------


class _FixObjectReprDefaults(RewritePybind11EnumValueRepr):
    """Prints `...` for each default pybind11 rendered as an object repr, the form no Python expression states, before the arguments split on its commas."""

    @override
    def parse_args_str(self, args_str: str) -> list[Argument]:
        return super().parse_args_str(_opaque(self._pybind11_enum_pattern, args_str))


class _FixModuleNames(IParser):
    """Resolves the C++ names pybind11 printed before their type registered, and qualifies each builtin a name of the module shadows."""

    _scopes: tuple[types.ModuleType, ...] = ()

    # --- [SCOPE]

    @override
    def handle_module(self, path: QualifiedName, module: types.ModuleType) -> Module | None:
        outer = self._scopes
        self._scopes = (*outer, module)
        try:
            return super().handle_module(path, module)
        finally:
            self._scopes = outer

    # --- [NAMES]

    @override
    def handle_type(self, type_: type) -> QualifiedName:
        name = super().handle_type(type_)
        shadowed = type_.__module__ == "builtins" and len(name) == 1 and any(getattr(scope, name[0], type_) is not type_ for scope in self._scopes[-1:])
        match self.parse_annotation_str(f"builtins.{name}") if shadowed else None:
            case ResolvedType(name=qualified):
                return qualified
            case _:
                return name

    @override
    def parse_annotation_str(self, annotation_str: str) -> ResolvedType | InvalidExpression | Value:
        named = annotation_str.strip()
        registered = (
            tuple(
                member
                for scope in self._scopes[-1:]
                for member in vars(scope).values()
                if inspect.isclass(member) and member.__module__ == scope.__name__ and member.__name__.endswith(named.rpartition("::")[2])
            )
            if _CXX_NAME.fullmatch(named)
            else ()
        )
        match registered:
            case (found,):
                return ResolvedType(name=self.handle_type(found))
            case _:
                return super().parse_annotation_str(annotation_str)


class _Parser(
    _FixObjectReprDefaults,
    _FixModuleNames,
    LoggerData,
    FixMissing__future__AnnotationsImport,
    FixMissing__all__Attribute,
    FixMissingNoneHashFieldAnnotation,
    FixMissingImports,
    FilterTypingModuleAttributes,
    FixPEP585CollectionNames,
    FixTypingTypeNames,
    FixScipyTypeArguments,
    FixMissingFieldDocString,
    FixMissingFixedSizeImport,
    FixMissingEnumMembersAnnotation,
    OverridePrintSafeValues,
    FixNumpyDtype,
    FixNumpyArrayFlags,
    FixCurrentModulePrefixInTypeNames,
    FixBuiltinTypes,
    RewritePybind11EnumValueRepr,
    FilterClassMembers,
    ReplaceReadWritePropertyWithField,
    FilterInvalidIdentifiers,
    FixValueReprRandomAddress,
    FixRedundantBuiltinsAnnotation,
    FilterPybindInternals,
    FilterPybind11NativeEnumMembers,
    FilterPybind11ViewClasses,
    FixRedundantMethodsFromBuiltinObject,
    RemoveSelfAnnotation,
    FixPybind11EnumStrDoc,
    ExtractSignaturesFromPybind11Docstrings,
    ParserDispatchMixin,
    BaseParser,
    LogErrors,
    SuggestCxxSignatureFix,
):
    """The mixins `stub_parser_from_args` composes with no option set, led by the two fixes this hook adds."""


# --- [COMPOSITION] ----------------------------------------------------------------------


class StubsHook(BuildHookInterface[BuilderConfig]):
    """Generates the stubs into a folder the build holds, then places it as the wheel's `PyOpenColorIO-stubs` package, editable installs included."""

    @override
    def initialize(self, version: str, build_data: dict[str, object]) -> None:
        self.generated = TemporaryDirectory()
        parser = _Parser()
        parser.set_pybind11_enum_locations({re.compile(r".*"): f"{_MODULE}.{_MODULE}"})
        records = logging.StreamHandler()
        records.setFormatter(logging.Formatter(_RECORD_FORMAT))
        logger.addHandler(records)
        try:
            run(parser, Printer(invalid_expr_as_ellipses=True, print_value_comments=False), [_MODULE], self.generated.name, root_suffix=_SUFFIX, dry_run=False, writer=Writer())
        finally:
            logger.removeHandler(records)
        root = Path(self.generated.name, f"{_MODULE}{_SUFFIX}")
        for stub in root.rglob("*.pyi"):
            stub.write_text("".join(_ordered(line) for line in stub.read_text(encoding="utf-8").splitlines(keepends=True) if line.strip() != "from __future__ import annotations"), encoding="utf-8")
        placed = {str(root): root.name}
        build_data.update(force_include=placed, force_include_editable=placed)

    @override
    def finalize(self, version: str, build_data: dict[str, object], artifact_path: str) -> None:
        self.generated.cleanup()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["StubsHook"]

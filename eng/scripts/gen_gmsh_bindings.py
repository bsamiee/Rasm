"""Emit the complete C# bindings from the gmsh api definition in the pinned archive.

Upstream api/gen.py registers every module and function through GenApi argument factories.
Running it with those factories tagged captures the whole definition, each function then emits a DllImport extern and a marshaling wrapper, and gmshc.h from the same archive cross-checks that every exported C function was bound.
"""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Iterable
import importlib.util
from itertools import starmap
from pathlib import Path
import re
import runpy
import sys
from textwrap import dedent, indent
from typing import Protocol

from expression import Error, Ok, Result
import msgspec

from eng.scripts.provision import PinMismatch

# --- [TYPES] ----------------------------------------------------------------------------


class _Factory(Protocol):
    """Callable shape of a GenApi argument factory."""

    def __call__(self, *args: object, **kwargs: object) -> object: ...


class _Kind(msgspec.Struct, frozen=True, gc=False):
    """C# emission template for a GenApi argument kind."""

    extern: str
    param: str
    call: str = "{p}"
    pre: str | None = None
    free: str | None = None
    post: str | None = None
    out: bool = False
    default: str | None = None


class _Arg(msgspec.Struct, frozen=True, gc=False):
    kind: str
    name: str
    value: str | None


class _Fn(msgspec.Struct, frozen=True, gc=False):
    name: str
    doc: str
    rtype: str | None
    args: tuple[_Arg, ...]


class _Mod(msgspec.Struct, frozen=True, gc=False):
    name: str
    doc: str
    fns: tuple[_Fn, ...]
    subs: tuple["_Mod", ...]


# --- [CONSTANTS] ------------------------------------------------------------------------

_FACTORIES = (
    "ibool", "iint", "isize", "idouble", "istring", "ivoidstar",
    "ivectorint", "ivectorsize", "ivectordouble", "ivectorstring", "ivectorpair",
    "ivectorvectorint", "ivectorvectorsize", "ivectorvectordouble",
    "ostring", "ovectorint", "ovectorsize", "ovectordouble", "ovectorstring", "ovectorpair",
    "ovectorvectorint", "ovectorvectorsize", "ovectorvectordouble", "ovectorvectorpair",
    "iargcargv", "isizefun",
)  # fmt: skip
_WRITERS = ("write_cpp", "write_c", "write_python", "write_julia", "write_fortran", "write_texi")
_CSHARP_KEYWORDS = frozenset({"base", "checked", "default", "event", "fixed", "object", "operator", "out", "params", "ref", "string", "value"})
# GenApi element suffix to the C# element type and the GmshMarshal helper suffix, the scalars also form the return and out kinds
_SCALARS = {"int": "int", "size": "long", "double": "double"}
_ELEMENTS = {
    **{element: (cs, cs.capitalize() + "s") for element, cs in _SCALARS.items()},
    "string": ("string", "Strings"),
    "pair": ("(int, int)", "Pairs"),
}
_RETURNS = {f"o{element}": cs for element, cs in _SCALARS.items()}


def _vector_kinds() -> dict[str, _Kind]:
    """Build the vector argument kinds from the element table, each direction and depth shares one template."""
    kinds: dict[str, _Kind] = {}
    for element, cs in _SCALARS.items():
        pre = f"{cs}[] {{n}}_ = {{p}} ?? Array.Empty<{cs}>();"
        kinds[f"ivector{element}"] = _Kind(f"{cs}[] {{p}}, long {{p}}_n", f"{cs}[] {{p}}", "{n}_, (long){n}_.Length", pre=pre, default="vector")
        pre = f"IntPtr[] {{n}}_ = GmshMarshal.InJagged({{p}} ?? Array.Empty<{cs}[]>(), out long[] {{n}}_n_);"
        call = "{n}_, {n}_n_, (long){n}_.Length"
        kinds[f"ivectorvector{element}"] = _Kind(
            "IntPtr[] {p}, long[] {p}_n, long {p}_nn", f"{cs}[][] {{p}}", call, pre=pre, free="GmshMarshal.FreeAll({n}_);", default="vector"
        )
    for element, (cs, marshal) in _ELEMENTS.items():
        post = f"{{p}} = GmshMarshal.Out{marshal}({{n}}_, {{n}}_n_);"
        kinds[f"ovector{element}"] = _Kind(
            "out IntPtr {p}, out long {p}_n", f"out {cs}[] {{p}}", "out IntPtr {n}_, out long {n}_n_", post=post, out=True
        )
        if element != "string":
            post = f"{{p}} = GmshMarshal.OutJagged{marshal}({{n}}_, {{n}}_n_, {{n}}_nn_);"
            call = "out IntPtr {n}_, out IntPtr {n}_n_, out long {n}_nn_"
            kinds[f"ovectorvector{element}"] = _Kind(
                "out IntPtr {p}, out IntPtr {p}_n, out long {p}_nn", f"out {cs}[][] {{p}}", call, post=post, out=True
            )
    return kinds


_KINDS: dict[str, _Kind] = {
    "ibool": _Kind("int {p}", "bool {p}", "{p} ? 1 : 0", default="bool"),
    "iint": _Kind("int {p}", "int {p}", default="int"),
    "isize": _Kind("long {p}", "long {p}", default="int"),
    "idouble": _Kind("double {p}", "double {p}", default="double"),
    "istring": _Kind("[MarshalAs((UnmanagedType)48)] string {p}", "string {p}", default="string"),
    "ivoidstar": _Kind("IntPtr {p}", "IntPtr {p}"),
    "ivectorstring": _Kind(
        "IntPtr[] {p}, long {p}_n",
        "string[] {p}",
        "{n}_, (long){n}_.Length",
        pre="IntPtr[] {n}_ = GmshMarshal.InStrings({p});",
        free="GmshMarshal.FreeAll({n}_);",
        default="vector",
    ),
    "ivectorpair": _Kind(
        "int[] {p}, long {p}_n", "(int, int)[] {p}", "{n}_, (long){n}_.Length", pre="int[] {n}_ = GmshMarshal.Flatten({p});", default="vector"
    ),
    "iargcargv": _Kind(
        "int argc, IntPtr[] argv",
        "string[] {p}",
        "{n}_.Length, {n}_",
        pre="IntPtr[] {n}_ = GmshMarshal.InStrings({p});",
        free="GmshMarshal.FreeAll({n}_);",
        default="argv",
    ),
    "isizefun": _Kind("GmshSizeFunc {p}, IntPtr {p}_data", "GmshSizeFunc {p}", "{p}, IntPtr.Zero", pre="GmshMarshal.KeepAlive({p});"),
    "ostring": _Kind("out IntPtr {p}", "out string {p}", "out IntPtr {n}_", post="{p} = GmshMarshal.OutString({n}_);", out=True),
    **{f"o{element}": _Kind(f"out {cs} {{p}}", f"out {cs} {{p}}", "out {p}", out=True) for element, cs in _SCALARS.items()},
    **_vector_kinds(),
}

# --- [DEFINITION] -----------------------------------------------------------------------


def _tagged(factory: _Factory, kind: str) -> _Factory:
    def _make(*args: object, **kwargs: object) -> object:
        made = factory(*args, **kwargs)
        vars(made)["kind"] = kind
        return made

    return _make


def _norm_arg(argument: object) -> _Arg:
    data = vars(argument)
    value = data["value"]
    return _Arg(kind=str(data.get("kind", type(argument).__name__)), name=str(data["name"]), value=value if isinstance(value, str) else None)


def _norm_fn(rtype: object, name: str, args: Iterable[object], doc: str, _special: object) -> _Fn:
    rtype_name = None if rtype is None else str(getattr(rtype, "__name__", ""))
    return _Fn(name=str(name), doc=" ".join(str(doc).split()), rtype=rtype_name, args=tuple(_norm_arg(argument) for argument in args))


def _norm_mod(module: object) -> _Mod:
    data = vars(module)
    subs = tuple(_norm_mod(sub) for sub in data["submodules"])
    return _Mod(name=str(data["name"]), doc=" ".join(str(data["doc"]).split()), fns=tuple(starmap(_norm_fn, data["fs"])), subs=subs)


def _load_definition(api_dir: Path) -> Result[tuple[_Mod, str], PinMismatch]:
    """Run the pinned gen.py with tagged factories and return the normalized module tree and the declared api version."""
    spec = importlib.util.spec_from_file_location("GenApi", api_dir / "GenApi.py")
    if spec is None or spec.loader is None:
        return Error(PinMismatch("Gmsh api definition", f"cannot load GenApi.py under {api_dir}"))
    genapi = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(genapi)
    for name in _FACTORIES:
        setattr(genapi, name, _tagged(getattr(genapi, name), name))
    for name in _WRITERS:
        setattr(genapi.API, name, lambda _self: None)
    sys.modules["GenApi"] = genapi
    try:
        namespace = runpy.run_path(str(api_dir / "gen.py"))
    finally:
        del sys.modules["GenApi"]
    api = namespace["api"]
    (root,) = api.modules
    return Ok((_norm_mod(root), f"{api.version_major}.{api.version_minor}.{api.version_patch}"))


# --- [EMISSION] -------------------------------------------------------------------------


def _pascal(name: str) -> str:
    return name[0].upper() + name[1:]


def _escape(name: str) -> str:
    return f"@{name}" if name in _CSHARP_KEYWORDS else name


def _xml(doc: str) -> str:
    return doc.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def _default(arg: _Arg) -> str | None:
    """Translate the default value of an argument into C#, absent when the kind or the value has no translation."""
    match _KINDS[arg.kind].default, arg.value:
        case ("argv", _):
            return "null"
        case (_, None) | (None, _):
            return None
        case ("vector", _):
            return "null"
        case ("bool", "true" | "false"):
            return arg.value
        case ("int", value) if re.fullmatch(r"-?\d+", str(value)):
            return value
        case ("double", value) if re.fullmatch(r"-?[\d.]+([eE][-+]?\d+)?", str(value)):
            return repr(float(str(value)))
        case ("double", value) if re.fullmatch(r"[-\d*/.]*M_PI[-\d*/.]*", str(value)):
            return str(value).replace("M_PI", "Math.PI")
        case ("string", value) if re.fullmatch(r'"[^"\\]*"', str(value)):
            return value
        case _:
            return None


def _defaults(fn: _Fn) -> dict[str, str]:
    """Return the trailing run of arguments with a translatable default, C# accepts optional parameters at the end alone."""
    assigned: dict[str, str] = {}
    for arg in reversed(fn.args):
        value = None if _KINDS[arg.kind].out else _default(arg)
        if value is None:
            break
        assigned[arg.name] = value
    return assigned


def _extern(fn: _Fn, cname: str) -> list[str]:
    ret = _RETURNS[fn.rtype] if fn.rtype else "void"
    pieces = [_KINDS[arg.kind].extern.format(p=_escape(arg.name), n=arg.name) for arg in fn.args]
    signature = ", ".join([*pieces, "out int ierr"])
    return [f'        [DllImport(Library, EntryPoint = "{cname}")]', f"        public static extern {ret} {cname}({signature});", ""]


def _wrapper(fn: _Fn, cname: str, indent: str) -> list[str]:
    ret = _RETURNS[fn.rtype] if fn.rtype else "void"
    defaults = _defaults(fn)
    kinds = [(_KINDS[arg.kind], {"p": _escape(arg.name), "n": arg.name}) for arg in fn.args]
    parameters = [kind.param.format(**names) + (f" = {defaults[names['n']]}" if names["n"] in defaults else "") for kind, names in kinds]
    pre = [kind.pre.format(**names) for kind, names in kinds if kind.pre is not None]
    free = [kind.free.format(**names) for kind, names in kinds if kind.free is not None]
    post = [kind.post.format(**names) for kind, names in kinds if kind.post is not None]
    call = f"GmshNative.{cname}({', '.join([*(kind.call.format(**names) for kind, names in kinds), 'out int ierr_'])});"
    body = [
        *pre,
        f"{ret} result_ = {call}" if fn.rtype else call,
        *free,
        "GmshMarshal.Check(ierr_);",
        *post,
        *(["return result_;"] if fn.rtype else []),
    ]
    lines = [
        f"{indent}/// <summary>{_xml(fn.doc)}</summary>",
        f"{indent}public static {ret} {_pascal(fn.name)}({', '.join(parameters)})",
        f"{indent}{{",
    ]
    return [*lines, *(f"{indent}    {line}" for line in body), f"{indent}}}", ""]


def _emit_module(mod: _Mod, cprefix: str, indent: str, *, top: bool, bound: list[tuple[str, _Fn]]) -> Result[list[str], PinMismatch]:
    """Emit the class of a module with its wrappers and nested classes, recording every bound C name."""
    prefix = mod.name if top else cprefix + _pascal(mod.name)
    children = {_pascal(sub.name) for sub in mod.subs}
    lines = [f"{indent}/// <summary>{_xml(mod.doc)}</summary>", f"{indent}public static class {'Gmsh' if top else _pascal(mod.name)}", f"{indent}{{"]
    for fn in mod.fns:
        if _pascal(fn.name) in children:
            return Error(PinMismatch("Gmsh api definition", f"names the function {fn.name} after a nested module class in {mod.name}"))
        cname = prefix + _pascal(fn.name)
        bound.append((cname, fn))
        lines += _wrapper(fn, cname, indent + "    ")
    for sub in mod.subs:
        match _emit_module(sub, prefix, indent + "    ", top=False, bound=bound):
            case Result(tag="error", error=failure):
                return Error(failure)
            case Result(ok=nested):
                lines += nested
    if not lines[-1]:
        lines.pop()
    return Ok([*lines, f"{indent}}}", ""])


def _native_file(version: str, externs: list[str]) -> str:
    """Return the GmshNative.g.cs text, the fixed header followed by the extern of every bound function."""
    header = dedent(f"""\
        // Generated from api/gen.py of gmsh {version}, do not edit
        using System;
        using System.Runtime.InteropServices;

        namespace Rasm.Gmsh
        {{
            /// <summary>Mesh size callback passed to gmshModelMeshSetSizeCallback.</summary>
            public delegate double GmshSizeFunc(int dim, int tag, double x, double y, double z, double lc, IntPtr data);

            /// <summary>Raw P/Invoke declarations for every exported gmsh C function.</summary>
            public static class GmshNative
            {{
                /// <summary>Library name dotnet resolves to the staged gmsh binary per runtime identifier.</summary>
                public const string Library = "gmsh";

                [DllImport(Library, EntryPoint = "gmshFree")]
                public static extern void gmshFree(IntPtr p);

                [DllImport(Library, EntryPoint = "gmshMalloc")]
                public static extern IntPtr gmshMalloc(long n);
        """)
    return "\n".join([header, *externs[:-1], "    }", "}", ""])


def _api_file(version: str, body: list[str]) -> str:
    """Return the Gmsh.g.cs text, the fixed header followed by the emitted module classes."""
    header = dedent(f"""\
        // Generated from api/gen.py of gmsh {version}, do not edit
        #pragma warning disable 0465 // static Finalize on a static class cannot collide with a destructor
        using System;

        namespace Rasm.Gmsh
        {{""")
    return "\n".join([header, *body[:-1], "}", ""])


def _marshal_file(version: str) -> str:
    """Return the marshaling members one scalar element table derives, one copy-in, copy-out, and jagged trio per element."""
    members = [
        indent(
            dedent(f"""\
                internal static {cs}[] Out{marshal}(IntPtr values, long count)
                {{
                    {cs}[] result = new {cs}[count];
                    if (values != IntPtr.Zero)
                    {{
                        Marshal.Copy(values, result, 0, (int)count);
                        GmshNative.gmshFree(values);
                    }}
                    return result;
                }}

                internal static IntPtr[] InJagged({cs}[][] values, out long[] lengths)
                {{
                    IntPtr[] rows = new IntPtr[values.Length];
                    lengths = new long[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {{
                        {cs}[] row = values[i] ?? Array.Empty<{cs}>();
                        rows[i] = Marshal.AllocHGlobal(row.Length * sizeof({cs}));
                        Marshal.Copy(row, 0, rows[i], row.Length);
                        lengths[i] = row.Length;
                    }}
                    return rows;
                }}

                internal static {cs}[][] OutJagged{marshal}(IntPtr values, IntPtr counts, long count)
                {{
                    long[] lengths = OutLongs(counts, count);
                    {cs}[][] result = new {cs}[count][];
                    for (int i = 0; i < (int)count; i++)
                    {{
                        result[i] = Out{marshal}(Marshal.ReadIntPtr(values, i * IntPtr.Size), lengths[i]);
                    }}
                    if (values != IntPtr.Zero)
                    {{
                        GmshNative.gmshFree(values);
                    }}
                    return result;
                }}
            """),
            "        ",
        )
        for cs, marshal in ((cs, cs.capitalize() + "s") for cs in _SCALARS.values())
    ]
    header = dedent(f"""\
        // Generated from api/gen.py of gmsh {version}, do not edit
        using System;
        using System.Runtime.InteropServices;

        namespace Rasm.Gmsh
        {{
            internal static partial class GmshMarshal
            {{
        """)
    return header + "\n".join(members) + "    }\n}\n"


def generate(api_dir: Path, out_dir: Path, version: str) -> Result[int, PinMismatch]:
    """Emit GmshNative.g.cs, Gmsh.g.cs, and GmshMarshal.g.cs into out_dir and return the bound function count."""
    match _load_definition(api_dir):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=(root, declared)) if declared != version:
            return Error(PinMismatch("Gmsh api definition", f"declares version {declared} against the pin {version}"))
        case Result(ok=(root, _)):
            pass
    bound: list[tuple[str, _Fn]] = []
    match _emit_module(root, "", "    ", top=True, bound=bound):
        case Result(tag="error", error=failure):
            return Error(failure)
        case Result(ok=body):
            pass
    names = {cname for cname, _fn in bound}
    exported = set(re.findall(r"^GMSH_API\s+[a-z_]+\s+(gmsh\w+)\(", (api_dir / "gmshc.h").read_text(), re.MULTILINE)) - {"gmshFree", "gmshMalloc"}
    if names != exported:
        missing, extra = ", ".join(sorted(exported - names)), ", ".join(sorted(names - exported))
        return Error(PinMismatch("Gmsh api definition", f"binds functions differing from the gmshc.h exports, missing {missing}, extra {extra}"))
    out_dir.mkdir(parents=True, exist_ok=True)
    _ = (out_dir / "GmshNative.g.cs").write_text(_native_file(version, [line for cname, fn in bound for line in _extern(fn, cname)]))
    _ = (out_dir / "Gmsh.g.cs").write_text(_api_file(version, body))
    _ = (out_dir / "GmshMarshal.g.cs").write_text(_marshal_file(version))
    return Ok(len(bound))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["generate"]

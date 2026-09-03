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
from typing import Protocol

import msgspec

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
    """Normalized argument from the definition."""

    kind: str
    name: str
    value: str | None


class _Fn(msgspec.Struct, frozen=True, gc=False):
    """Normalized api function."""

    name: str
    doc: str
    rtype: str | None
    args: tuple[_Arg, ...]


class _Mod(msgspec.Struct, frozen=True, gc=False):
    """Normalized api module with its nested modules."""

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
_RETURNS = {"oint": "int", "osize": "long", "odouble": "double"}
_CSHARP_KEYWORDS = frozenset({"base", "checked", "default", "event", "fixed", "object", "operator", "out", "params", "ref", "string", "value"})

_KINDS: dict[str, _Kind] = {
    "ibool": _Kind(extern="int {p}", param="bool {p}", call="{p} ? 1 : 0", default="bool"),
    "iint": _Kind(extern="int {p}", param="int {p}", default="int"),
    "isize": _Kind(extern="long {p}", param="long {p}", default="int"),
    "idouble": _Kind(extern="double {p}", param="double {p}", default="double"),
    "istring": _Kind(extern="[MarshalAs((UnmanagedType)48)] string {p}", param="string {p}", default="string"),
    "ivoidstar": _Kind(extern="IntPtr {p}", param="IntPtr {p}"),
    "ivectorint": _Kind(
        extern="int[] {p}, long {p}_n",
        param="int[] {p}",
        pre="int[] {n}_ = {p} ?? Array.Empty<int>();",
        call="{n}_, (long){n}_.Length",
        default="vector",
    ),
    "ivectorsize": _Kind(
        extern="long[] {p}, long {p}_n",
        param="long[] {p}",
        pre="long[] {n}_ = {p} ?? Array.Empty<long>();",
        call="{n}_, (long){n}_.Length",
        default="vector",
    ),
    "ivectordouble": _Kind(
        extern="double[] {p}, long {p}_n",
        param="double[] {p}",
        pre="double[] {n}_ = {p} ?? Array.Empty<double>();",
        call="{n}_, (long){n}_.Length",
        default="vector",
    ),
    "ivectorstring": _Kind(
        extern="IntPtr[] {p}, long {p}_n",
        param="string[] {p}",
        pre="IntPtr[] {n}_ = GmshMarshal.InStrings({p});",
        call="{n}_, (long){n}_.Length",
        free="GmshMarshal.FreeAll({n}_);",
        default="vector",
    ),
    "ivectorpair": _Kind(
        extern="int[] {p}, long {p}_n",
        param="(int, int)[] {p}",
        pre="int[] {n}_ = GmshMarshal.Flatten({p});",
        call="{n}_, (long){n}_.Length",
        default="vector",
    ),
    "ivectorvectorint": _Kind(
        extern="IntPtr[] {p}, long[] {p}_n, long {p}_nn",
        param="int[][] {p}",
        pre="IntPtr[] {n}_ = GmshMarshal.InJagged({p} ?? Array.Empty<int[]>(), out long[] {n}_n_);",
        call="{n}_, {n}_n_, (long){n}_.Length",
        free="GmshMarshal.FreeAll({n}_);",
        default="vector",
    ),
    "ivectorvectorsize": _Kind(
        extern="IntPtr[] {p}, long[] {p}_n, long {p}_nn",
        param="long[][] {p}",
        pre="IntPtr[] {n}_ = GmshMarshal.InJagged({p} ?? Array.Empty<long[]>(), out long[] {n}_n_);",
        call="{n}_, {n}_n_, (long){n}_.Length",
        free="GmshMarshal.FreeAll({n}_);",
        default="vector",
    ),
    "ivectorvectordouble": _Kind(
        extern="IntPtr[] {p}, long[] {p}_n, long {p}_nn",
        param="double[][] {p}",
        pre="IntPtr[] {n}_ = GmshMarshal.InJagged({p} ?? Array.Empty<double[]>(), out long[] {n}_n_);",
        call="{n}_, {n}_n_, (long){n}_.Length",
        free="GmshMarshal.FreeAll({n}_);",
        default="vector",
    ),
    "iargcargv": _Kind(
        extern="int argc, IntPtr[] argv",
        param="string[] {p}",
        pre="IntPtr[] {n}_ = GmshMarshal.InStrings({p});",
        call="{n}_.Length, {n}_",
        free="GmshMarshal.FreeAll({n}_);",
        default="argv",
    ),
    "isizefun": _Kind(
        extern="GmshSizeFunc {p}, IntPtr {p}_data", param="GmshSizeFunc {p}", pre="GmshMarshal.KeepAlive({p});", call="{p}, IntPtr.Zero"
    ),
    "oint": _Kind(extern="out int {p}", param="out int {p}", call="out {p}", out=True),
    "osize": _Kind(extern="out long {p}", param="out long {p}", call="out {p}", out=True),
    "odouble": _Kind(extern="out double {p}", param="out double {p}", call="out {p}", out=True),
    "ostring": _Kind(extern="out IntPtr {p}", param="out string {p}", call="out IntPtr {n}_", post="{p} = GmshMarshal.OutString({n}_);", out=True),
    "ovectorint": _Kind(
        extern="out IntPtr {p}, out long {p}_n",
        param="out int[] {p}",
        call="out IntPtr {n}_, out long {n}_n_",
        post="{p} = GmshMarshal.OutInts({n}_, {n}_n_);",
        out=True,
    ),
    "ovectorsize": _Kind(
        extern="out IntPtr {p}, out long {p}_n",
        param="out long[] {p}",
        call="out IntPtr {n}_, out long {n}_n_",
        post="{p} = GmshMarshal.OutLongs({n}_, {n}_n_);",
        out=True,
    ),
    "ovectordouble": _Kind(
        extern="out IntPtr {p}, out long {p}_n",
        param="out double[] {p}",
        call="out IntPtr {n}_, out long {n}_n_",
        post="{p} = GmshMarshal.OutDoubles({n}_, {n}_n_);",
        out=True,
    ),
    "ovectorstring": _Kind(
        extern="out IntPtr {p}, out long {p}_n",
        param="out string[] {p}",
        call="out IntPtr {n}_, out long {n}_n_",
        post="{p} = GmshMarshal.OutStrings({n}_, {n}_n_);",
        out=True,
    ),
    "ovectorpair": _Kind(
        extern="out IntPtr {p}, out long {p}_n",
        param="out (int, int)[] {p}",
        call="out IntPtr {n}_, out long {n}_n_",
        post="{p} = GmshMarshal.OutPairs({n}_, {n}_n_);",
        out=True,
    ),
    "ovectorvectorint": _Kind(
        extern="out IntPtr {p}, out IntPtr {p}_n, out long {p}_nn",
        param="out int[][] {p}",
        call="out IntPtr {n}_, out IntPtr {n}_n_, out long {n}_nn_",
        post="{p} = GmshMarshal.OutJaggedInts({n}_, {n}_n_, {n}_nn_);",
        out=True,
    ),
    "ovectorvectorsize": _Kind(
        extern="out IntPtr {p}, out IntPtr {p}_n, out long {p}_nn",
        param="out long[][] {p}",
        call="out IntPtr {n}_, out IntPtr {n}_n_, out long {n}_nn_",
        post="{p} = GmshMarshal.OutJaggedLongs({n}_, {n}_n_, {n}_nn_);",
        out=True,
    ),
    "ovectorvectordouble": _Kind(
        extern="out IntPtr {p}, out IntPtr {p}_n, out long {p}_nn",
        param="out double[][] {p}",
        call="out IntPtr {n}_, out IntPtr {n}_n_, out long {n}_nn_",
        post="{p} = GmshMarshal.OutJaggedDoubles({n}_, {n}_n_, {n}_nn_);",
        out=True,
    ),
    "ovectorvectorpair": _Kind(
        extern="out IntPtr {p}, out IntPtr {p}_n, out long {p}_nn",
        param="out (int, int)[][] {p}",
        call="out IntPtr {n}_, out IntPtr {n}_n_, out long {n}_nn_",
        post="{p} = GmshMarshal.OutJaggedPairs({n}_, {n}_n_, {n}_nn_);",
        out=True,
    ),
}

_SUPPORT = """
    /// <summary>Failure one gmsh API call reported.</summary>
    public sealed class GmshException : Exception
    {
        /// <summary>Nonzero status the C API returned through ierr.</summary>
        public int Code { get; }

        /// <summary>Create the exception for one status and message.</summary>
        public GmshException(int code, string message) : base(message)
        {
            Code = code;
        }
    }

    internal static class GmshMarshal
    {
        private static readonly System.Collections.Generic.List<object> _callbacks = new System.Collections.Generic.List<object>();

        internal static void KeepAlive(object callback)
        {
            lock (_callbacks)
            {
                _callbacks.Add(callback);
            }
        }

        internal static void Check(int ierr)
        {
            if (ierr == 0)
            {
                return;
            }
            GmshNative.gmshLoggerGetLastError(out IntPtr message, out int status);
            throw new GmshException(ierr, status == 0 ? OutString(message) : "gmsh call failed with status " + ierr.ToString());
        }

        internal static string OutString(IntPtr value)
        {
            if (value == IntPtr.Zero)
            {
                return string.Empty;
            }
            int length = 0;
            while (Marshal.ReadByte(value, length) != 0)
            {
                length++;
            }
            byte[] buffer = new byte[length];
            Marshal.Copy(value, buffer, 0, length);
            GmshNative.gmshFree(value);
            return Encoding.UTF8.GetString(buffer);
        }

        internal static IntPtr[] InStrings(string[] values)
        {
            string[] source = values ?? Array.Empty<string>();
            IntPtr[] buffers = new IntPtr[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(source[i] ?? string.Empty);
                buffers[i] = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, buffers[i], bytes.Length);
                Marshal.WriteByte(buffers[i], bytes.Length, 0);
            }
            return buffers;
        }

        internal static void FreeAll(IntPtr[] buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                Marshal.FreeHGlobal(buffers[i]);
            }
        }

        internal static int[] Flatten((int, int)[] pairs)
        {
            (int, int)[] source = pairs ?? Array.Empty<(int, int)>();
            int[] flat = new int[source.Length * 2];
            for (int i = 0; i < source.Length; i++)
            {
                flat[2 * i] = source[i].Item1;
                flat[2 * i + 1] = source[i].Item2;
            }
            return flat;
        }

        internal static IntPtr[] InJagged(int[][] values, out long[] lengths)
        {
            IntPtr[] rows = new IntPtr[values.Length];
            lengths = new long[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                int[] row = values[i] ?? Array.Empty<int>();
                rows[i] = Marshal.AllocHGlobal(row.Length * sizeof(int));
                Marshal.Copy(row, 0, rows[i], row.Length);
                lengths[i] = row.Length;
            }
            return rows;
        }

        internal static IntPtr[] InJagged(long[][] values, out long[] lengths)
        {
            IntPtr[] rows = new IntPtr[values.Length];
            lengths = new long[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                long[] row = values[i] ?? Array.Empty<long>();
                rows[i] = Marshal.AllocHGlobal(row.Length * sizeof(long));
                Marshal.Copy(row, 0, rows[i], row.Length);
                lengths[i] = row.Length;
            }
            return rows;
        }

        internal static IntPtr[] InJagged(double[][] values, out long[] lengths)
        {
            IntPtr[] rows = new IntPtr[values.Length];
            lengths = new long[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                double[] row = values[i] ?? Array.Empty<double>();
                rows[i] = Marshal.AllocHGlobal(row.Length * sizeof(double));
                Marshal.Copy(row, 0, rows[i], row.Length);
                lengths[i] = row.Length;
            }
            return rows;
        }

        internal static int[] OutInts(IntPtr values, long count)
        {
            int[] result = new int[count];
            if (values != IntPtr.Zero)
            {
                Marshal.Copy(values, result, 0, (int)count);
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static long[] OutLongs(IntPtr values, long count)
        {
            long[] result = new long[count];
            if (values != IntPtr.Zero)
            {
                Marshal.Copy(values, result, 0, (int)count);
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static double[] OutDoubles(IntPtr values, long count)
        {
            double[] result = new double[count];
            if (values != IntPtr.Zero)
            {
                Marshal.Copy(values, result, 0, (int)count);
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static (int, int)[] OutPairs(IntPtr values, long count)
        {
            int[] flat = OutInts(values, count);
            (int, int)[] pairs = new (int, int)[flat.Length / 2];
            for (int i = 0; i < pairs.Length; i++)
            {
                pairs[i] = (flat[2 * i], flat[2 * i + 1]);
            }
            return pairs;
        }

        internal static string[] OutStrings(IntPtr values, long count)
        {
            string[] result = new string[count];
            for (int i = 0; i < (int)count; i++)
            {
                result[i] = OutString(Marshal.ReadIntPtr(values, i * IntPtr.Size));
            }
            if (values != IntPtr.Zero)
            {
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static int[][] OutJaggedInts(IntPtr values, IntPtr counts, long count)
        {
            long[] lengths = OutLongs(counts, count);
            int[][] result = new int[count][];
            for (int i = 0; i < (int)count; i++)
            {
                result[i] = OutInts(Marshal.ReadIntPtr(values, i * IntPtr.Size), lengths[i]);
            }
            if (values != IntPtr.Zero)
            {
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static long[][] OutJaggedLongs(IntPtr values, IntPtr counts, long count)
        {
            long[] lengths = OutLongs(counts, count);
            long[][] result = new long[count][];
            for (int i = 0; i < (int)count; i++)
            {
                result[i] = OutLongs(Marshal.ReadIntPtr(values, i * IntPtr.Size), lengths[i]);
            }
            if (values != IntPtr.Zero)
            {
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static double[][] OutJaggedDoubles(IntPtr values, IntPtr counts, long count)
        {
            long[] lengths = OutLongs(counts, count);
            double[][] result = new double[count][];
            for (int i = 0; i < (int)count; i++)
            {
                result[i] = OutDoubles(Marshal.ReadIntPtr(values, i * IntPtr.Size), lengths[i]);
            }
            if (values != IntPtr.Zero)
            {
                GmshNative.gmshFree(values);
            }
            return result;
        }

        internal static (int, int)[][] OutJaggedPairs(IntPtr values, IntPtr counts, long count)
        {
            long[] lengths = OutLongs(counts, count);
            (int, int)[][] result = new (int, int)[count][];
            for (int i = 0; i < (int)count; i++)
            {
                result[i] = OutPairs(Marshal.ReadIntPtr(values, i * IntPtr.Size), lengths[i]);
            }
            if (values != IntPtr.Zero)
            {
                GmshNative.gmshFree(values);
            }
            return result;
        }
    }"""

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
    normalized = tuple(_norm_arg(argument) for argument in args)
    return _Fn(name=str(name), doc=" ".join(str(doc).split()), rtype=None if rtype is None else str(getattr(rtype, "__name__", "")), args=normalized)


def _norm_mod(module: object) -> _Mod:
    data = vars(module)
    return _Mod(
        name=str(data["name"]),
        doc=" ".join(str(data["doc"]).split()),
        fns=tuple(starmap(_norm_fn, data["fs"])),
        subs=tuple(_norm_mod(sub) for sub in data["submodules"]),
    )


def _load_definition(api_dir: Path) -> tuple[_Mod, str]:
    """Run the pinned gen.py with tagged factories and return the normalized module tree and the declared api version."""
    spec = importlib.util.spec_from_file_location("GenApi", api_dir / "GenApi.py")
    if spec is None or spec.loader is None:
        raise SystemExit(f"cannot load GenApi.py under {api_dir}")
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
    return _norm_mod(root), f"{api.version_major}.{api.version_minor}.{api.version_patch}"


def _declared(api_dir: Path) -> set[str]:
    """Return the function names gmshc.h from the same archive exports, without gmshFree and gmshMalloc."""
    text = (api_dir / "gmshc.h").read_text()
    return set(re.findall(r"^GMSH_API\s+[a-z_]+\s+(gmsh\w+)\(", text, re.MULTILINE)) - {"gmshFree", "gmshMalloc"}


# --- [EMISSION] -------------------------------------------------------------------------


def _escape(name: str) -> str:
    return f"@{name}" if name in _CSHARP_KEYWORDS else name


def _xml(doc: str) -> str:
    return doc.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def _default(arg: _Arg) -> str | None:
    kind = _KINDS[arg.kind]
    match kind.default, arg.value:
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
    parameters: list[str] = []
    pre: list[str] = []
    calls: list[str] = []
    free: list[str] = []
    post: list[str] = []
    for arg in fn.args:
        kind = _KINDS[arg.kind]
        suffix = f" = {defaults[arg.name]}" if arg.name in defaults else ""
        parameters.append(kind.param.format(p=_escape(arg.name), n=arg.name) + suffix)
        calls.append(kind.call.format(p=_escape(arg.name), n=arg.name))
        for source, target in ((kind.pre, pre), (kind.free, free), (kind.post, post)):
            if source is not None:
                target.append(source.format(p=_escape(arg.name), n=arg.name))
    call = f"GmshNative.{cname}({', '.join([*calls, 'out int ierr_'])});"
    body = [
        *pre,
        f"{ret} result_ = {call}" if fn.rtype else call,
        *free,
        "GmshMarshal.Check(ierr_);",
        *post,
        *(["return result_;"] if fn.rtype else []),
    ]
    name = fn.name[0].upper() + fn.name[1:]
    lines = [f"{indent}/// <summary>{_xml(fn.doc)}</summary>", f"{indent}public static {ret} {name}({', '.join(parameters)})", f"{indent}{{"]
    lines += [f"{indent}    {line}" for line in body]
    return [*lines, f"{indent}}}", ""]


def _emit_module(mod: _Mod, cprefix: str, indent: str, *, top: bool, bound: list[tuple[str, _Fn]]) -> list[str]:
    cls = mod.name[0].upper() + mod.name[1:]
    prefix = mod.name if top else cprefix + cls
    children = {sub.name[0].upper() + sub.name[1:] for sub in mod.subs}
    lines = [f"{indent}/// <summary>{_xml(mod.doc)}</summary>", f"{indent}public static class {'Gmsh' if top else cls}", f"{indent}{{"]
    for fn in mod.fns:
        if fn.name[0].upper() + fn.name[1:] in children:
            raise SystemExit(f"function {fn.name} collides with a nested module in {mod.name}")
        cname = prefix + fn.name[0].upper() + fn.name[1:]
        bound.append((cname, fn))
        lines += _wrapper(fn, cname, indent + "    ")
    for sub in mod.subs:
        lines += _emit_module(sub, prefix, indent + "    ", top=False, bound=bound)
    if not lines[-1]:
        lines.pop()
    return [*lines, f"{indent}}}", ""]


def _native_file(version: str, externs: list[str]) -> str:
    header = [
        f"// Generated from api/gen.py of gmsh {version}, do not edit",
        "using System;",
        "using System.Runtime.InteropServices;",
        "",
        "namespace Rasm.Gmsh",
        "{",
        "    /// <summary>Mesh size callback passed to gmshModelMeshSetSizeCallback.</summary>",
        "    public delegate double GmshSizeFunc(int dim, int tag, double x, double y, double z, double lc, IntPtr data);",
        "",
        "    /// <summary>Raw P/Invoke declarations for every exported gmsh C function.</summary>",
        "    public static class GmshNative",
        "    {",
        "        /// <summary>Library name dotnet resolves to the staged gmsh binary per runtime identifier.</summary>",
        '        public const string Library = "gmsh";',
        "",
        '        [DllImport(Library, EntryPoint = "gmshFree")]',
        "        public static extern void gmshFree(IntPtr p);",
        "",
        '        [DllImport(Library, EntryPoint = "gmshMalloc")]',
        "        public static extern IntPtr gmshMalloc(long n);",
        "",
    ]
    return "\n".join([*header, *externs[:-1], "    }", "}", ""])


def _api_file(version: str, body: list[str]) -> str:
    header = [
        f"// Generated from api/gen.py of gmsh {version}, do not edit",
        "#pragma warning disable 0465 // static Finalize on a static class cannot collide with a destructor",
        "using System;",
        "using System.Runtime.InteropServices;",
        "using System.Text;",
        "",
        "namespace Rasm.Gmsh",
        "{",
    ]
    return "\n".join([*header, *body[:-1], _SUPPORT, "}", ""])


def generate(api_dir: Path, out_dir: Path, version: str) -> int:
    """Emit GmshNative.g.cs and Gmsh.g.cs into out_dir and return the bound function count."""
    root, declared_version = _load_definition(api_dir)
    if declared_version != version:
        raise SystemExit(f"api definition version {declared_version} does not match the pin {version}")
    bound: list[tuple[str, _Fn]] = []
    body = _emit_module(root, "", "    ", top=True, bound=bound)
    externs: list[str] = []
    for cname, fn in bound:
        externs += _extern(fn, cname)
    declared = _declared(api_dir)
    names = {cname for cname, _fn in bound}
    if names != declared:
        raise SystemExit(f"bound functions differ from the gmshc.h exports, missing {sorted(declared - names)}, extra {sorted(names - declared)}")
    out_dir.mkdir(parents=True, exist_ok=True)
    _ = (out_dir / "GmshNative.g.cs").write_text(_native_file(version, externs))
    _ = (out_dir / "Gmsh.g.cs").write_text(_api_file(version, body))
    return len(bound)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["generate"]

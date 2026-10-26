"""Recursive tolerance predicate and assertion, and deterministic codec round trips."""

lazy import cmath
lazy from collections.abc import Iterable, Mapping, Sequence
lazy from decimal import Decimal
lazy from fractions import Fraction

lazy from expression import Option, Result
lazy from expression.collections import Block
lazy import msgspec
lazy import msgspec.json
lazy import msgspec.msgpack
lazy import numpy as np
lazy import pint

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [TOLERANCE]


def _diverge(a: object, b: object, path: str, *, rel_tol: float, abs_tol: float) -> str | None:
    """Return first tolerance divergence at its structural path, or ``None``."""
    match (a, b):
        case (bool() | str() | bytes(), _) | (_, bool() | str() | bytes()):
            return None if isinstance(a, bool) == isinstance(b, bool) and a == b else f"{path}: {a!r} != {b!r}"
        case (np.ndarray() | np.generic(), _) | (_, np.ndarray() | np.generic()) if (left := np.asarray(a)).shape != (right := np.asarray(b)).shape:
            return f"{path}: shape {left.shape} != {right.shape}"
        case (np.ndarray() | np.generic(), _) | (_, np.ndarray() | np.generic()):
            left, right = np.atleast_1d(np.asarray(a), np.asarray(b))
            far = np.argwhere(~np.isclose(left, right, rtol=rel_tol, atol=abs_tol, equal_nan=True))
            return None if far.size == 0 else f"{path}{far[0].tolist()}: {left[tuple(far[0])]!r} !~ {right[tuple(far[0])]!r}"
        case ((int() | float() | complex() | Decimal() | Fraction()) as left, (int() | float() | complex() | Decimal() | Fraction()) as right):
            fa, fb = complex(left), complex(right)
            return None if cmath.isclose(fa, fb, rel_tol=rel_tol, abs_tol=abs_tol) or (cmath.isnan(fa) and cmath.isnan(fb)) else f"{path}: {a!r} !~ {b!r} at rel_tol={rel_tol}, abs_tol={abs_tol}"
        case (Result(tag=tag), Result(tag=other)) | (Option(tag=tag), Option(tag=other)) if tag != other:
            return f"{path}: tags differ, {a!r} != {b!r}"
        case (Result(tag="ok" as tag, ok=x), Result(ok=y)) | (Result(tag="error" as tag, error=x), Result(error=y)) | (Option(tag="some" as tag, some=x), Option(some=y)):
            children: Iterable[tuple[str, object, object]] = ((f".{tag}", x, y),)
        case (Option(), Option()):
            return None
        case (msgspec.Struct(), msgspec.Struct()) if type(a) is type(b):
            children = ((f".{name}", x, y) for (name, x), y in zip(msgspec.structs.asdict(a).items(), msgspec.structs.astuple(b), strict=True))
        case (pint.Quantity() as left, pint.Quantity() as right) if left.units != right.units:
            return f"{path}: units {left.units!r} != {right.units!r}"
        case (pint.Quantity() as left, pint.Quantity() as right):
            children = ((".magnitude", left.magnitude, right.magnitude),)
        case (Mapping(), Mapping()) if set(a) != set(b):
            return f"{path}: unshared keys {sorted(map(repr, set(a) ^ set(b)))}"
        case (Mapping(), Mapping()):
            children = ((f"[{key!r}]", value, b[key]) for key, value in a.items())
        case (Block() as left, Block() as right) | (Sequence() as left, Sequence() as right) if len(left) != len(right):
            return f"{path}: length {len(left)} != {len(right)}"
        case (Block() as left, Block() as right) | (Sequence() as left, Sequence() as right):
            children = ((f"[{index}]", x, y) for index, (x, y) in enumerate(zip(left, right, strict=True)))
        case _:
            return None if a == b else f"{path}: {a!r} != {b!r}"
    return next((divergence for suffix, x, y in children if (divergence := _diverge(x, y, f"{path}{suffix}", rel_tol=rel_tol, abs_tol=abs_tol)) is not None), None)


def close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> bool:
    """Return whether ``actual`` and ``expected`` agree within tolerance at every structural path."""
    return _diverge(actual, expected, "$", rel_tol=rel_tol, abs_tol=abs_tol) is None


def assert_close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> None:
    """Assert recursive tolerance equality, naming first diverging structural path."""
    assert (divergence := _diverge(actual, expected, "$", rel_tol=rel_tol, abs_tol=abs_tol)) is None, f"tolerance violation at {divergence}"


# --- [CODECS]


def assert_roundtrip[T](value: T, encoder: msgspec.json.Encoder | msgspec.msgpack.Encoder, decoder: msgspec.json.Decoder[T] | msgspec.msgpack.Decoder[T]) -> T:
    """Return decoded value after asserting equality and byte-identical re-encode."""
    raw = encoder.encode(value)
    decoded = decoder.decode(raw)
    assert decoded == value, f"{decoder.type!r} decode: {decoded!r} != {value!r}"
    assert (again := encoder.encode(decoded)) == raw, f"{decoder.type!r} re-encode: {again!r} != {raw!r}"
    return decoded


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["assert_close", "assert_roundtrip", "close"]

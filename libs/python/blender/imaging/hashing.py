"""Content hashes over canonical record and array bytes."""

from datetime import datetime
import functools
import hashlib
from types import ModuleType
from typing import Annotated, Final

import cbor2
from expression import Nothing, Option
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

# --- [TYPES] ----------------------------------------------------------------------------

type ContentHash = Annotated[bytes, msgspec.Meta(min_length=_DIGEST_SIZE, max_length=_DIGEST_SIZE)]

# --- [CONSTANTS] ------------------------------------------------------------------------

_DIGEST_SIZE: Final = 16
_MIXERS: Final = (np.uint64(0xFF51AFD7ED558CCD), np.uint64(0xC4CEB9FE1A85EC53), np.uint64(0x9E3779B97F4A7C15))

# --- [OPERATIONS] -----------------------------------------------------------------------


def content_hash(
    scope: type[object],
    *records: msgspec.Struct | bytes,
    arrays: tuple[npt.NDArray[np.bool | np.int8 | np.int16 | np.int32 | np.int64 | np.uint8 | np.uint16 | np.uint32 | np.uint64 | np.float16 | np.float32 | np.float64], ...] = (),
    config: Option[PyOpenColorIO.Config] = Nothing,
    providers: tuple[ModuleType, ...] = (),
) -> ContentHash:
    """Hashes records, arrays, configuration, and providers.

    Args:
        scope: Type naming content domain.
        *records: Records with naive or aware datetimes, no MessagePack Raw or Ext.
        arrays: Fixed-width real arrays hashed with shape and canonical bytes.
        config: Configuration hashed by its cache ID.
        providers: Modules hashed by name and version.

    Returns:
        Framed BLAKE2b digest.
    """
    wide = tuple(np.asarray(array, dtype=np.promote_types(array.dtype, np.int64 if np.can_cast(array.dtype, np.int64) else np.uint64).newbyteorder("<"), order="C") for array in arrays)
    configs = config.to_list()
    chunks = (
        memoryview(cbor2.dumps((f"{scope.__module__}.{scope.__qualname__}", len(records), len(wide), len(configs), len(providers)))),
        *(
            memoryview(
                cbor2.dumps(
                    msgspec.to_builtins(record, builtin_types=(bytes, bytearray, memoryview, datetime), order="deterministic"),
                    canonical=True,
                    encoders={
                        memoryview: lambda encoder, value: encoder.encode_bytes(bytes(value)),
                        datetime: lambda encoder, value: encoder.encode(msgspec.to_builtins(value)) if value.tzinfo is None else encoder.encode_datetime(value),
                    },
                )
            )
            for record in records
        ),
        *(view for array in wide for view in (memoryview(cbor2.dumps((array.dtype.str, array.shape))), array.data)),
        *(memoryview(current.getCacheID().encode()) for current in configs),
        *(memoryview(cbor2.dumps((module.__name__, module.__version__))) for module in providers),
    )
    hasher = hashlib.blake2b(len(chunks).to_bytes(4, "big"), digest_size=_DIGEST_SIZE)
    for chunk in chunks:
        hasher.update(chunk.nbytes.to_bytes(8, "big"))
        hasher.update(chunk)
    return hasher.digest()


def rng(content: ContentHash, *key: int) -> np.random.Generator:
    """Seeds independent generator from content hash and key."""
    return np.random.default_rng(np.random.SeedSequence(int.from_bytes(content, "little"), spawn_key=key))


def hashed(cells: npt.NDArray[np.int64], *, seed: int, stream: int, count: int) -> npt.NDArray[np.float64]:
    """Draws stable uniforms per integer lattice cell in one pass over every cell.

    Args:
        cells: Signed integer coordinates along the last axis.
        seed: Number selecting the draws.
        stream: Signed independent draw family under one seed.
        count: Uniforms per cell.

    Returns:
        `(*cells.shape[:-1], count)` uniforms in [0, 1).
    """

    def mixed(values: npt.NDArray[np.uint64]) -> npt.NDArray[np.uint64]:
        spread = functools.reduce(lambda held, multiplier: (held ^ (held >> np.uint64(33))) * multiplier, _MIXERS[:2], values)
        return spread ^ (spread >> np.uint64(33))

    rows = cells.reshape(-1, cells.shape[-1]).astype(np.uint64)
    start = np.full(len(rows), np.random.SeedSequence(seed).generate_state(1, np.uint64)[0], dtype=np.uint64)
    state = functools.reduce(lambda held, axis: mixed(held ^ axis), (np.full(len(rows), stream).astype(np.uint64), *rows.T), start)
    return np.ldexp((mixed(state[:, None] ^ (np.arange(1, count + 1, dtype=np.uint64) * _MIXERS[2])) >> np.uint64(11)).astype(np.float64), -53).reshape(*cells.shape[:-1], count)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ContentHash", "content_hash", "hashed", "rng"]

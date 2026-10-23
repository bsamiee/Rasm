"""Content hashes over canonical record and array bytes."""

from datetime import datetime
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ContentHash", "content_hash", "rng"]

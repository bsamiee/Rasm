# [PY_COMPUTE_API_H5PY]

`h5py` maps the HDF5 container to Python. Compute composes it as the exchange-container surface alone — sparse-operator archives, graduation drift envelopes, and waveform corpora — never as a field store: the gridded field domain and its full h5py surface belong to `libs/python/data/.api/h5py.md`, and this folder's admission covers only the group/dataset/attribute members its exchange fences spell.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container tree objects this folder touches

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------- | :------------ | :------------------------------------ |
|  [01]   | `File`             | class         | on-disk container and root group      |
|  [02]   | `Group`            | class         | hierarchical name-to-object namespace |
|  [03]   | `Dataset`          | class         | typed n-dimensional array storage     |
|  [04]   | `AttributeManager` | class         | per-object `attrs` metadata map       |

[PUBLIC_TYPE_SCOPE]: raise surface the boundary `catch=` sets name

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `OSError`           | builtin       | non-HDF5, truncated, or locked file at `File(...)` open |
|  [02]   | `FileNotFoundError` | `OSError`     | absent path at read-mode open                           |
|  [03]   | `KeyError`          | builtin       | absent group, dataset, or attribute on subscript        |

`h5py` exports NO exception symbol of its own — the HDF5 error stack surfaces entirely as builtins, so a `catch` tuple over this lane is `(KeyError, OSError)` and nothing narrower exists to name.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: exchange-container IO

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `File(name, mode)`                                | ctor     | open or create the container    |
|  [02]   | `Group.create_group(name) -> Group`               | instance | create subgroup                 |
|  [03]   | `Group.create_dataset(name, data, **) -> Dataset` | instance | create dataset with fixed codec |
|  [04]   | `Dataset[selection]`                              | operator | read selection as `ndarray`     |
|  [05]   | `AttributeManager[name]` / `[name] = value`       | operator | read and write typed attributes |
|  [06]   | `string_dtype(encoding, length)`                  | factory  | vlen or fixed string dtype      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `File` is a context manager; a `with` block flushes and closes the handle, and a leaked handle locks or corrupts the file.
- Mode `x` is the create-only open every writer fence pins, matching the peer's create-only archive session.
- Attribute values keep their numpy dtypes both ways, so the wire pins (`int32` indices, `int64` shape, `float64` values) spell as explicit `astype`/`np.<dtype>` at every write.

[STACKING]:
- `numpy`(`../../.api/numpy.md`): `Dataset[sel]` returns an `ndarray` and `create_dataset(data=)` writes one — read once into the array kernel, never scalar-iterate.
- `scipy`(`.api/scipy.md`): the sparse archive datasets feed the `csr_array`/`csc_array` constructors directly off the read arrays.

[LOCAL_ADMISSION]:
- Consumers are the exchange fences alone: `solvers/linear#EXCHANGE` (scipy-convention sparse archives), `experiments/model#ENVELOPE` (drift envelopes), and `analysis/signal#WAVEFORM_EXCHANGE` (two-axis waveform publication); a compute field, ensemble, or CF store routes to the data branch owners instead.

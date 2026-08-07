# [PY_COMPUTE_API_H5PY]

`h5py` maps the HDF5 container to Python. Compute composes it as the exchange-container surface alone — the sparse-operator archive pair and the graduation drift-envelope writer — never as a field store: the gridded field rail and its full h5py surface belong to `libs/python/data/.api/h5py.md`, and this folder's admission covers only the group/dataset/attribute members its two exchange fences spell.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h5py`
- package: `h5py` (BSD-3-Clause)
- module: `import h5py`
- namespaces: `h5py`
- owner: `compute`
- rail: exchange — the HDF5 container carrying the C#-peer sparse-operator and drift-envelope crossings
- capability: file IO, group namespace, typed dataset storage, attribute metadata, vlen string dtype

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container tree objects this folder touches

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------- | :------------ | :------------------------------------ |
|  [01]   | `File`             | class         | on-disk container and root group      |
|  [02]   | `Group`            | class         | hierarchical name-to-object namespace |
|  [03]   | `Dataset`          | class         | typed n-dimensional array storage     |
|  [04]   | `AttributeManager` | class         | per-object `attrs` metadata map       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: exchange-container IO

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `File(name, mode)`                               | ctor     | open or create the container         |
|  [02]   | `Group.create_group(name) -> Group`              | instance | create subgroup                      |
|  [03]   | `Group.create_dataset(name, data, **) -> Dataset`| instance | create dataset with fixed codec      |
|  [04]   | `Dataset[selection]`                             | operator | read selection as `ndarray`          |
|  [05]   | `AttributeManager[name]` / `[name] = value`      | operator | read and write typed attributes      |
|  [06]   | `string_dtype(encoding, length)`                 | factory  | vlen or fixed string dtype           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `File` is a context manager; a `with` block flushes and closes the handle, and a leaked handle locks or corrupts the file.
- Mode `x` is the create-only open both exchange fences pin, matching the peer's create-only archive session.
- Attribute values keep their numpy dtypes both ways, so the wire pins (`int32` indices, `int64` shape, `float64` values) spell as explicit `astype`/`np.<dtype>` at every write.

[STACKING]:
- `numpy`(`../../.api/numpy.md`): `Dataset[sel]` returns an `ndarray` and `create_dataset(data=)` writes one — read once into the array kernel, never scalar-iterate.
- `scipy`(`.api/scipy.md`): the sparse archive datasets feed the `csr_array`/`csc_array` constructors directly off the read arrays.

[LOCAL_ADMISSION]:
- Consumers are the two exchange fences alone: `solvers/linear#EXCHANGE` (the scipy-convention sparse archive pair) and `experiments/model#ENVELOPE` (the drift-envelope writer); a compute field, ensemble, or CF store over h5py routes to the data branch owners instead.

[RAIL_LAW]:
- Package: `h5py`
- Owns: the HDF5 exchange-container IO this folder's two C#-peer crossings spell
- Accept: a context-managed `File`, create-only `x` mode, explicit dtype pins at every dataset and attribute write, `string_dtype` for vlen categories
- Reject: a file handle leaked outside `with`, an implicit int64 index dataset where the exchange convention pins int32, and any field-store or CF use the data branch owns

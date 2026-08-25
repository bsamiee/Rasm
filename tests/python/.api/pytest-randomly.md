# [PY_TESTS_API_PYTEST_RANDOMLY]

`pytest-randomly` shuffles collection order and reseeds the global random state before every test context, so order-coupling and hidden RNG dependence surface as failures instead of latent flake. It is active in the default run and reproducible from a printed seed.

## [01]-[PUBLIC_TYPES]

Seed helpers and the reseed extension point third-party RNGs register against.

| [INDEX] | [SYMBOL]                           | [KIND]            | [CAPABILITY]                                                                |
| :-----: | :--------------------------------- | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `pytest_randomly.make_seed()`      | seed source       | draws a fresh 32-bit seed via `random.Random().getrandbits(32)`             |
|  [02]   | `pytest_randomly.seed_type(value)` | option parser     | coerces `--randomly-seed` input, accepting the literal `last` and `default` |
|  [03]   | `pytest_randomly.XdistHooks`       | xdist bridge      | broadcasts the controller seed to every worker for one shared parallel seed |
|  [04]   | `pytest_randomly.random_seeder`    | entry-point group | each registered callable receives the per-test seed to reseed a custom RNG  |

```python
def make_seed() -> int: ...
def seed_type(value: str) -> str | int: ...
```

## [02]-[ENTRYPOINTS]

CLI surface fixing the seed and toggling the two behaviors independently.

| [INDEX] | [SURFACE]                                        | [KIND]        | [CAPABILITY]                                                         |
| :-----: | :----------------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `--randomly-seed <int>` · `--randomly-seed last` | seed control  | fixes the seed for replay; `last` = prior run's, default draws fresh |
|  [02]   | `--randomly-dont-reorganize`                     | order toggle  | keeps collection order, still reseeds RNGs per test                  |
|  [03]   | `--randomly-dont-reset-seed`                     | reseed toggle | stops the per-test `random.seed()` reset while still shuffling order |
|  [04]   | `-p no:randomly`                                 | disable       | unloads the plugin entirely                                          |

```python
```

## [03]-[IMPLEMENTATION_LAW]

[PYTEST_RANDOMLY_TOPOLOGY]:
- Before each test context `_reseed` calls `random.seed(seed)` then propagates the derived state to `factory_boy`, `faker`, and `model_bakery`, calls `numpy.random.seed(seed % 2**32)`, and invokes every `pytest_randomly.random_seeder` entry-point reseeder — one seed drives every registered RNG.
- Hypothesis generation sits outside the reseed: the engine seeds from its own internal `Random`, so property-law replay rides the profile lane and the example database (`.api/hypothesis.md`), never `--randomly-seed`.
- Seed prints in the session header, so any observed failure replays with `--randomly-seed <that value>`.

[STACKING]:
- `pytest`(`.api/pytest.md`): active by default; `required_plugins` lists `pytest-randomly`, so the guard fails the session if the plugin is absent.
- `pytest-xdist`(`.api/pytest-xdist.md`): `XdistHooks` broadcasts one controller seed to every worker, keeping a parallel run reproducible.

[LOCAL_ADMISSION]:
- Admitted on the dev plane in `[dependency-groups] dev`; no runtime graph imports `pytest_randomly`.
- Custom RNGs join the reseed only through the `pytest_randomly.random_seeder` entry point, never a per-suite ad-hoc `random.seed` call.

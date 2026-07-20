# [PY_TESTS_API_PYTEST_RANDOMLY]

`pytest-randomly` shuffles collection order and reseeds the global random state before every test context, so order-coupling and hidden RNG dependence surface as failures instead of latent flake. It is active in the default run and reproducible from a printed seed; the mutation lane deliberately disables it because mutmut needs a fixed, derandomized order to attribute kills.

## [01]-[PACKAGE_SURFACE]

- package: `pytest-randomly` · version `4.1.0` · license `MIT`
- namespace: `import pytest_randomly` — the plugin module is the barrel.
- asset: dist `pytest-randomly`; `pytest11` entry point `randomly = pytest_randomly` (disable with `-p no:randomly`).
- rail: test-order shuffling and per-test RNG reseeding — the default anti-flake lane.

## [02]-[PUBLIC_TYPES]

Seed helpers and the reseed extension point third-party RNGs register against.

| [INDEX] | [SYMBOL]                           | [KIND]            | [CAPABILITY]                                                                |
| :-----: | :--------------------------------- | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `pytest_randomly.make_seed()`      | seed source       | draws a fresh 32-bit seed via `random.Random().getrandbits(32)`             |
|  [02]   | `pytest_randomly.seed_type(value)` | option parser     | coerces `--randomly-seed` input, accepting the literal `last` and `default` |
|  [03]   | `pytest_randomly.XdistHooks`       | xdist bridge      | broadcasts the controller seed to every worker for one shared parallel seed |
|  [04]   | `pytest_randomly.random_seeder`    | entry-point group | each registered callable receives the per-test seed to reseed a custom RNG  |

```python signature
def make_seed() -> int: ...                              # random.Random().getrandbits(32)
def seed_type(value: str) -> str | int: ...              # 'default' | 'last' | int
# A third-party RNG registers a reseeder via the entry-point group:
#   [project.entry-points."pytest_randomly.random_seeder"]
#   my_rng = "my_pkg:reseed"      # reseed(seed: int) -> None
```

## [03]-[ENTRYPOINTS]

CLI surface fixing the seed and toggling the two behaviors independently.

| [INDEX] | [SURFACE]                                        | [KIND]        | [CAPABILITY]                                                         |
| :-----: | :----------------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `--randomly-seed <int>` · `--randomly-seed last` | seed control  | fixes the seed for replay; `last` = prior run's, default draws fresh |
|  [02]   | `--randomly-dont-reorganize`                     | order toggle  | keeps collection order, still reseeds RNGs per test                  |
|  [03]   | `--randomly-dont-reset-seed`                     | reseed toggle | stops the per-test `random.seed()` reset while still shuffling order |
|  [04]   | `-p no:randomly`                                 | disable       | unloads the plugin entirely — the mutmut invocation path             |

```python signature
# addoption bindings (dest / default):
#   --randomly-seed            dest=randomly_seed      default="default"   type=seed_type
#   --randomly-dont-reset-seed dest=randomly_reset_seed  default=True  (store_false)
#   --randomly-dont-reorganize dest=randomly_reorganize  default=True  (store_false)
```

## [04]-[IMPLEMENTATION_LAW]

[PYTEST_RANDOMLY_TOPOLOGY]:
- Before each test context `_reseed` calls `random.seed(seed)` then propagates the derived state to `factory_boy`, `faker`, and `model_bakery`, calls `numpy.random.seed(seed % 2**32)`, and invokes every `pytest_randomly.random_seeder` entry-point reseeder — one seed drives every registered RNG.
- Hypothesis generation sits outside the reseed: the engine seeds from its own internal `Random`, so property-law replay rides the profile lane and the example database (`.api/hypothesis.md`), never `--randomly-seed`.
- Seed prints in the session header, so any observed failure replays with `--randomly-seed <that value>`.

[STACKING]:
- `pytest`(`.api/pytest.md`): active by default; `required_plugins` lists `pytest-randomly`, so the guard fails the session if the plugin is absent.
- `pytest-xdist`(`.api/pytest-xdist.md`): `XdistHooks` broadcasts one controller seed to every worker, keeping a parallel run reproducible.
- `mutmut`(`.api/mutmut.md`): mutmut internally passes `-p no:randomly` to disable shuffling; the config's `-o required_plugins=` row waives the required-plugins guard for that disabled-plugin invocation, and `--hypothesis-profile=rasm-mutation` derandomizes generation.

[LOCAL_ADMISSION]:
- Admitted on the dev plane in `[dependency-groups] dev`; no runtime graph imports `pytest_randomly`.
- A custom RNG joins the reseed only through the `pytest_randomly.random_seeder` entry point, never a per-suite ad-hoc `random.seed` call.

[RAIL_LAW]:
- Package: `pytest-randomly`
- Owns: collection-order shuffling, per-test global-RNG reseeding, seed printing and replay, and the third-party reseed entry point.
- Accept: `--randomly-seed <int>` for replay; the two `--randomly-dont-*` toggles to isolate order from reseed; `XdistHooks` for a shared parallel seed; entry-point registration for a custom RNG.
- Reject: an ad-hoc `random.seed` in a suite that bypasses the reseed hook; the plugin left loaded under mutmut (mutation needs fixed order — `-p no:randomly` plus the `required_plugins=` waiver); reliance on a stable order where `--randomly-dont-reorganize` is not set.

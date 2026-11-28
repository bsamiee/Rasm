# [GENERATED]

`proptest` draws inputs from strategies, runs a property over each, and shrinks a failing input to a minimal one it prints.

## [01]-[PROPERTIES]

`#[property_test]` (feature `attr-macro`) turns a function over drawn arguments into one test:

```rust
fn factor(unit: Unit) -> u64 {
    match unit {
        Unit::B => 1,
        Unit::KiB => 1 << 10,
        Unit::MiB => 1 << 20,
        Unit::GiB => 1 << 30,
    }
}

fn bytes() -> impl Strategy<Value = u64> {
    let aligned = (0_u64..1 << 20, select(Unit::VARIANTS)).prop_map(|(count, unit)| count * factor(unit));
    prop_oneof![Just(0), Just((1 << 10) - 1), Just(1 << 10), aligned, Just(u64::MAX), any::<u64>()]
}

#[property_test]
fn size_in_its_largest_unit_reads_back_as_its_bytes(#[strategy = bytes()] bytes: u64) {
    let size = Size::largest(bytes);
    prop_assert_eq!(parsed(&size.to_string()), Ok(bytes), "`{}`", size);
    prop_assert_eq!(size.count * factor(size.unit), bytes, "`{}` states its bytes", size);
    prop_assert!(size.unit == Unit::GiB || size.count % 1024 != 0 || size.count == 0, "`{}` has a larger unit dividing it", size);
}
```

`Size::largest` states bytes as a count in the largest unit dividing them, `Unit::VARIANTS` comes from `strum::VariantArray`, and `factor` writes each unit's definition apart from the code under test:
- Round trips pass for a rendering in bytes every time, and the second relation fails on a count and unit that disagree, the third on a smaller unit
- `prop_oneof!` puts the limit and the values beside it (`1023`, `1024`, `u64::MAX`) in every run and shrinks toward earlier arms, so arms run from simplest to widest
- `#[strategy = <expr>]` is the one argument form, an argument without it draws `any::<T>()`, and a failure prints `<Name>Args { bytes: .. }`
- `#[property_test]` adds `#[test]` itself, and a second `#[test]` runs the test twice under `cargo test`
- `prop_assert!`, `prop_assert_eq!`, and `prop_assert_ne!` return a failed case in place of a panic, and `?` turns any `std::error::Error` into one
- Bodies are synchronous, and async code under a property runs through `block_on` of a current-thread runtime held in a `LazyLock` static, built once per nextest process
- `proptest!` blocks in a suite being edited convert to `#[property_test]`, one form for every property, and a converted block's stated `cases` then holds against `PROPTEST_CASES`

## [02]-[STRATEGIES]

Strategies build valid inputs, as a filter discards draws and starves shrinking:

| [INDEX] | [NEED]                        | [FORM]                                                             |
| :-----: | :---------------------------- | :----------------------------------------------------------------- |
|  [01]   | Values at and beside a limit  | `prop_oneof![Just(limit), Just(limit + 1), 0..limit]`              |
|  [02]   | Weighted kinds                | `prop_oneof![3 => made(), 1 => kept()]`                            |
|  [03]   | Draw sized by an earlier draw | `(1_usize..8).prop_flat_map(\|n\| (Just(n), vec(any::<u8>(), n)))` |
|  [04]   | Collisions between draws      | `select(&["a", "ab", "Ab"][..])`, a regex over a small alphabet    |
|  [05]   | Ordered subset of a list      | `subsequence(list, 0..=4)`                                         |
|  [06]   | Index into a drawn collection | `any::<Index>()` with `.index(len)` or `.get(&slice)`              |
|  [07]   | Text of a grammar             | `"[a-z](-?[a-z0-9]){0,6}"` as a `&str` strategy                    |
|  [08]   | Recursive value               | `leaf.prop_recursive(depth, size, branch, \|inner\| ..)`           |
|  [09]   | Named reusable strategy       | Function returning `impl Strategy`, or `prop_compose!`             |

- `prop_filter("<reason>", ..)` rejects locally and `prop_assume!` globally, each a reject the run counts, and a filter stays where no generating form exists
- Reach of a target class reads from a test drawing a sample through `TestRunner::deterministic()` and `Strategy::new_tree(..)` (`ValueTree` in scope, its `Reason` mapped into a `Failure` variant), asserting each class appears
- Set and map collections reject draws until their minimum size holds when elements collide

## [03]-[CONFIGURATION]

Runs take 256 cases and shrink up to four times that count:
- `config = ProptestConfig { cases: 512, ..ProptestConfig::default() }` sets a field on `#[property_test]`, where a stated field beats `PROPTEST_CASES`, and `proptest!` and `prop_state_machine!` let the variable win
- `PROPTEST_CASES`, `PROPTEST_RNG_SEED`, and `PROPTEST_VERBOSE` set a run from outside, and a fixed seed repeats every draw of a run
- Failures print their minimal input and no seed while persistence is off, and that input is the case a `#[case]` row takes
- `.cargo/config.toml` `[env]` holds `PROPTEST_DISABLE_FAILURE_PERSISTENCE = "1"`, any value of it disabling persistence, as each printed minimal input becomes a named `#[case]` row and its class a raised share of draws
- Persistence left on writes `proptest-regressions/<module>.txt` in the parent of the nearest folder holding `lib.rs` or `main.rs`, replaying every saved seed into that module
- `[[test]]` targets of one package holding a module of one name share that file

## [04]-[MODELS]

`proptest-state-machine` runs a drawn sequence of steps on the system and on a reference model, then shrinks a failing sequence:

```rust
#[derive(Debug, Clone)]
enum Step {
    Reserve(u64),
    Release(u64),
}

struct Held;

impl ReferenceStateMachine for Held {
    type State = u64;
    type Transition = Step;

    fn init_state() -> BoxedStrategy<u64> { Just(0).boxed() }

    fn transitions(held: &u64) -> BoxedStrategy<Step> {
        prop_oneof![(0..=CAPACITY - held + 1).prop_map(Step::Reserve), (0..=held + 1).prop_map(Step::Release)].boxed()
    }

    fn apply(held: u64, step: &Step) -> u64 {
        match *step {
            Step::Reserve(asked) if held + asked <= CAPACITY => held + asked,
            Step::Reserve(_) => held,
            Step::Release(freed) => held.saturating_sub(freed),
        }
    }
}

struct Ledger;

impl StateMachineTest for Ledger {
    type SystemUnderTest = Budget;
    type Reference = Held;

    fn init_test(_: &u64) -> Budget { Budget { capacity: CAPACITY, held: 0 } }

    fn apply(budget: Budget, held: &u64, step: Step) -> Budget {
        match step {
            Step::Reserve(asked) => budget.reserved(asked).unwrap_or_else(|refusal| {
                assert_eq!(refusal, Refusal { asked, left: CAPACITY - held }, "{asked} asked with {held} held after the step");
                budget
            }),
            Step::Release(freed) => budget.released(freed),
        }
    }

    fn check_invariants(budget: &Budget, held: &u64) {
        assert_eq!(budget.held, *held, "held bytes equal the model's");
    }
}

prop_state_machine! {
    #[test]
    fn budget_holds_what_its_model_holds_after_every_step(sequential 1..40 => Ledger);
}
```

`Budget` holds a `capacity` and the bytes `held`, `reserved` returns the next budget or a `Refusal` naming the bytes asked and left, and `CAPACITY` is 100:
- `transitions` draws from the model's state, each amount reaching one past what the state allows, so refusals and saturation both occur
- `StateMachineTest::apply` receives the model's state after the step, and `check_invariants` runs after every step
- `StateMachineTest` takes a unit type local to the test crate, as the orphan rule refuses an impl for the production type there
- Hooks report a broken postcondition by panicking, and `preconditions` gates each drawn step, counting a refused one as a local reject
- Sequence ranges start at 1, as a drawn length of 0 overflows in a debug build

## [05]-[DERIVED]

`proptest-derive` writes `Arbitrary` for a type the test crate declares (a step enum, an input record):
- `#[proptest(strategy = "<expr>")]` takes an expression as a quoted string, and proptest-derive refuses an unquoted one with its error E0026
- `#[proptest(weight = <weight>)]` weights a variant, `#[proptest(skip)]` drops it, and `#[proptest(filter = "<closure>")]` filters a field
- `#[proptest(params(<T>))]` replaces the parameters, read as `params` inside each expression and filled through `any_with`
- Derived impls name `proptest` by that crate name, so the test crate depends on `proptest` directly

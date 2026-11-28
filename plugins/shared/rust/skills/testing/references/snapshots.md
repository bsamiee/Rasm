# [SNAPSHOTS]

`insta` compares rendered output with a `.snap` file or an inline literal a person accepted, `cargo insta` runs and reviews them, and `insta-cmd` records a process run whole.

## [01]-[ASSERTIONS]

Named file snapshots run under settings that fix what the test cannot:

```rust
const SIZE: &str = env!("CARGO_BIN_FILE_SIZE_size");

#[test]
fn unreadable_size_file_exits_66_naming_its_path() -> io::Result<()> {
    let folder = tempfile::TempDir::new_in(env!("CARGO_TARGET_TMPDIR"))?;
    insta::with_settings!({ filters => vec![(r"(cannot read \S+): .+", "$1: [os error]")], prepend_module_to_snapshot => false }, {
        assert_cmd_snapshot!("unreadable-size-file", Command::new(SIZE).env_clear().current_dir(folder.path()).args(["--file", "absent.size"]));
    });
    Ok(())
}
```

`SIZE` is the binary an artifact dependency builds, and the body of the accepted `snapshots/unreadable-size-file.snap` reads:

```text
success: false
exit_code: 66
----- stdout -----

----- stderr -----
error: cannot read absent.size: [os error]
```

- `assert_snapshot!` takes `Display` text and `assert_json_snapshot!` a `Serialize` value, each with its name before the value
- Inline snapshots take no name, `assert_snapshot!(<value>, @"<literal>")`, and a leading name there is snapshotted as the value
- Names come from the row (an `rstest` column or `context.description`), and an unnamed assertion takes its function's name, a numeric counter joining from its second assertion on
- `prepend_module_to_snapshot => false` writes `snapshots/<snapshot>.snap` beside the asserting file, `<snapshot>` the assertion's name, and the module path prefixes it otherwise
- Inputs a test controls stay fixed at their source: a working folder and relative paths, an instant passed as a parameter, a seed
- `filters` (feature `filters`) rewrite text a test cannot fix (an OS error's wording, a version) through regex replacements over every snapshot kind
- Redactions (feature `redactions`) replace serialized fields by selector, `assert_json_snapshot!("quota-body", body, { ".id" => "[id]" })`, and apply to serde snapshots alone
- `insta-cmd` records `success`, `exit_code`, stdout, and stderr, and writes the program, arguments, and each `Command::env` pair into the header's `info`
- `info` takes its values anew on each accept, and `--require-full-match` alone compares it
- Settings bind to the current thread, and an async body takes them through `Settings::bind_async`

## [02]-[CHECK]

Checks before merge run every test and every snapshot through nextest:

```bash
cargo insta test --workspace --test-runner nextest --check --unreferenced reject --disable-nextest-doctest
```

- `cargo insta test` runs `cargo test` unless `--test-runner nextest`, `INSTA_TEST_RUNNER`, or `test.runner` in `.config/insta.yaml` names nextest
- `--check` holds `INSTA_UPDATE=no` and fails a changed or missing snapshot, where a run without it passes each mismatch and exits 1 with snapshots to review
- `--unreferenced reject` fails on a `.snap` no assertion named, and a filtered run rejects every snapshot its filter skipped, which keeps the reject to whole-suite runs
- Snapshots asserted only in a package `default-filter` drops fail that reject, and tests against real services hold none
- Arguments after `--` reach nextest's test-binary slot, where a substring selects tests and an `-E` filterset fails
- `cargo-mutants` runs with `INSTA_UPDATE=no` and `INSTA_FORCE_PASS=0`, where a snapshot catches mutants
- Snapshots every source edit changes (a metrics ratchet) stay out of mutation runs through their filterset

## [03]-[REVIEW]

New and changed snapshots are written as `.snap.new` files and `.pending-snap` entries, accepted one at a time:
1. Run `cargo insta test --test-runner nextest`, which writes each pending snapshot
2. Run `cargo insta pending-snapshots`, which lists each by `.snap` path, or by `file:line` for an inline snapshot
3. Read each `.snap.new` against its input, then run `cargo insta accept --snapshot <path>` or `cargo insta reject --snapshot <path>`, `<path>` as step 2 prints it

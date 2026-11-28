# [FIXTURES]

Shared test infrastructure sits once, in the narrowest place every reader reaches: a test module, a test package, or a support crate of the workspace.

## [01]-[RSTEST_FIXTURES]

`#[fixture]` functions return a value a test names by argument:
- `#[with(<expr>)]` binds the fixture's leading arguments in order, and `#[from(<function>)]` binds an argument of another name to that fixture
- `#[once]` builds into a static that is never dropped, and nextest runs each test in a process of its own, which builds a `#[once]` value once per test
- State built once for many tests comes from a nextest setup script, which has no teardown
- Each test removes leftovers of its own prefix before creating its objects
- `#[timeout]` runs a sync test on a thread it never stops, and nextest's `slow-timeout` bounds every hang in its place

## [02]-[FOLDERS]

`scoped` gives each test a folder no other test names, deleting it on a pass and keeping it, named in the failure, on a failure or panic:

```rust
#[derive(Debug, thiserror::Error)]
pub enum Scope {
    #[error(transparent)]
    Folder(#[from] std::io::Error),
    #[error("test failed, its folder kept at {}", kept.display())]
    Kept { kept: PathBuf },
}

pub async fn scoped<T: Termination>(within: &Path, test: impl AsyncFnOnce(&Path) -> T) -> Result<(), Scope> {
    let folder = tempfile::TempDir::new_in(within)?;
    let passed = AssertUnwindSafe(test(folder.path())).catch_unwind().await.is_ok_and(|outcome| outcome.report() == ExitCode::SUCCESS);
    match passed {
        true => Ok(folder.close()?),
        false => Err(Scope::Kept { kept: folder.keep() }),
    }
}
```

`scoped` sits in the workspace's support crate, `catch_unwind` comes from `futures::FutureExt`, and a test calls `scoped(Path::new(env!("CARGO_TARGET_TMPDIR")), async |folder: &Path| { .. })`:
- Callers pass `env!("CARGO_TARGET_TMPDIR")`, and the support crate takes the folder as a parameter
- Folders sit in the target folder, never inside the source tree a glob or a build reads
- Unix sockets need short paths, and on Unix their folder comes from `tempfile::Builder::new().prefix("<row>").tempdir_in("/tmp")`
- `assert_fs` keys `into_persistent_if` on a flag and `test-context` passes teardown no outcome, and neither decides keep or delete from the verdict

## [03]-[FIXTURE_FILES]

Fixture folders hold each input beside its expected output (`<row>/<input>` beside `<row>/<expected>`), and tests read them through `include_str!` or a `#[files]` table, never restating a fixture's content as a literal.

## [04]-[PROCESSES]

Processes a test starts run in its folder, isolated from the host:
- `Command::env_clear()` followed by `PATH` and each variable the case states, with `current_dir`, `HOME`, and `TMPDIR` inside the folder, so host configuration decides no verdict
- `tokio::process::Command::kill_on_drop(true)` ends an async child when its handle drops, and a `std::process::Child` takes `kill` and `wait` before the verdict
- Harnesses signal and reap every process their test started before the verdict, as a child ignoring `slow-timeout`'s SIGTERM outlives the run
- `leak-timeout` with `result = "fail"` sees a child still holding the test's stdout or stderr, and a child with redirected output escapes it

## [05]-[SUPPORT_CRATES]

Helpers that modules of one test package share sit in one module of that package, and harnesses more than one package reads form one workspace crate:
- `publish = false`, `[lib] test = false`, and a `[[test]]` target holding the harness's own tests
- `[workspace.dependencies]` holds a `<support> = { path = "<folder>" }` row, `<support>` the crate's name and `<folder>` its path outside every test package's folder, and each test package names it under `[dev-dependencies]`
- `.cargo/mutants.toml` `test_package` lists the support crate beside the test packages, so its own tests grade its code
- Types a fixture binary and its tests share (arguments, answers) sit in the test package's `[lib]` with `test = false`, and both read one definition

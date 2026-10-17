// --- [OPERATIONS] ----------------------------------------------------------------------

fn interior_mutable_static(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `static TOTAL` holds `Mutex<u64>`, which mutates through a shared reference
      --> tools/dylint/examples/async_lock_without_await.rs:19:8
       |
    19 | static TOTAL: Mutex<u64> = Mutex::const_new(0);
       |        ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default

    warning: `static HITS` holds `Atomic<u32>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:19:8
       |
    19 | static HITS: AtomicU32 = AtomicU32::new(0);
       |        ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default

    warning: `static ROOT` holds `OnceLock<String>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:21:8
       |
    21 | static ROOT: OnceLock<String> = OnceLock::new();
       |        ^^^^
       |
       = help: declare `static ROOT: LazyLock<String> = LazyLock::new(|| GREETING.to_owned())`, its only use initializes it from no input

    warning: `static PREFIX` holds `OnceLock<String>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:23:8
       |
    23 | static PREFIX: OnceLock<String> = OnceLock::new();
       |        ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static LABEL` holds `OnceLock<String>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:25:8
       |
    25 | static LABEL: OnceLock<String> = OnceLock::new();
       |        ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static ORIGIN` holds `OnceLock<String>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:27:12
       |
    27 | pub static ORIGIN: OnceLock<String> = OnceLock::new();
       |            ^^^^^^
       |
       = help: declare `static ORIGIN: LazyLock<String> = LazyLock::new(|| GREETING.to_owned())`, its only use initializes it from no input

    warning: `static COUNTER` holds `Atomic<u32>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:31:8
       |
    31 | static COUNTER: LazyLock<AtomicU32> = LazyLock::new(|| AtomicU32::new(1));
       |        ^^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SHARED` holds `Arc<Mutex<Vec<u8>>>`, and its `Mutex<Vec<u8>>` mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:33:8
       |
    33 | static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));
       |        ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static VISITS` holds `Cell<u32>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:40:12
       |
    40 |     static VISITS: Cell<u32> = const { Cell::new(0) };
       |            ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static CALLS` holds `Atomic<u32>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:44:10
       |
    44 | counter!(CALLS);
       |          ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SEEN` holds `Atomic<u32>`, which mutates through a shared reference
      --> tools/dylint/examples/interior_mutable_static.rs:58:12
       |
    58 |     static SEEN: AtomicU32 = AtomicU32::new(0);
       |            ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
    ");
}

fn needless_cell(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `total` wraps its value in `Cell`, but no closure captures it and no call shares it
     --> tools/dylint/examples/needless_cell.rs:6:5
      |
    6 |     let total = Cell::new(0);
      |     ^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: bind the value with `let mut` and read and write it in place
      = note: `#[warn(needless_cell)]` on by default

    warning: `listed` wraps its value in `RefCell`, but no closure captures it and no call shares it
      --> tools/dylint/examples/needless_cell.rs:14:5
       |
    14 |     let listed = RefCell::new(Vec::new());
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: bind the value with `let mut` and read and write it in place
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(interior_mutable_static, needless_cell);

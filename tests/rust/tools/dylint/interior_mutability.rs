// --- [OPERATIONS] ----------------------------------------------------------------------

fn interior_mutable_static(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `static TOTAL` holds interior mutable `Mutex<u64>`
      --> tools/dylint/examples/async_lock_without_await.rs:19:8
       |
    19 | static TOTAL: Mutex<u64> = Mutex::const_new(0);
       |        ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default

    warning: `static HITS` holds interior mutable `Atomic<u32>`
      --> tools/dylint/examples/interior_mutable_static.rs:19:8
       |
    19 | static HITS: AtomicU32 = AtomicU32::new(0);
       |        ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default

    warning: `static ROOT` holds interior mutable `OnceLock<String>`
      --> tools/dylint/examples/interior_mutable_static.rs:21:8
       |
    21 | static ROOT: OnceLock<String> = OnceLock::new();
       |        ^^^^
       |
       = help: declare `static ROOT: LazyLock<String> = LazyLock::new(|| GREETING.to_owned())`

    warning: `static PREFIX` holds interior mutable `OnceLock<String>`
      --> tools/dylint/examples/interior_mutable_static.rs:23:8
       |
    23 | static PREFIX: OnceLock<String> = OnceLock::new();
       |        ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static LABEL` holds interior mutable `OnceLock<String>`
      --> tools/dylint/examples/interior_mutable_static.rs:25:8
       |
    25 | static LABEL: OnceLock<String> = OnceLock::new();
       |        ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static ORIGIN` holds interior mutable `OnceLock<String>`
      --> tools/dylint/examples/interior_mutable_static.rs:27:12
       |
    27 | pub static ORIGIN: OnceLock<String> = OnceLock::new();
       |            ^^^^^^
       |
       = help: declare `static ORIGIN: LazyLock<String> = LazyLock::new(|| GREETING.to_owned())`

    warning: `static COUNTER` holds interior mutable `Atomic<u32>`
      --> tools/dylint/examples/interior_mutable_static.rs:31:8
       |
    31 | static COUNTER: LazyLock<AtomicU32> = LazyLock::new(|| AtomicU32::new(1));
       |        ^^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SHARED` holds interior mutable `Mutex<Vec<u8>>` in `Arc<Mutex<Vec<u8>>>`
      --> tools/dylint/examples/interior_mutable_static.rs:33:8
       |
    33 | static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));
       |        ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static VISITS` holds interior mutable `Cell<u32>`
      --> tools/dylint/examples/interior_mutable_static.rs:40:12
       |
    40 |     static VISITS: Cell<u32> = const { Cell::new(0) };
       |            ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static CALLS` holds interior mutable `Atomic<u32>`
      --> tools/dylint/examples/interior_mutable_static.rs:44:10
       |
    44 | counter!(CALLS);
       |          ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SEEN` holds interior mutable `Atomic<u32>`
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
    warning: `total` does not need `Cell`
     --> tools/dylint/examples/needless_cell.rs:6:5
      |
    6 |     let total = Cell::new(0);
      |     ^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = note: no closure captures `total` and every use is a method receiver
      = help: bind the value with `let mut total` and replace each `Cell` method call with a direct read or write
      = note: `#[warn(needless_cell)]` on by default

    warning: `listed` does not need `RefCell`
      --> tools/dylint/examples/needless_cell.rs:14:5
       |
    14 |     let listed = RefCell::new(Vec::new());
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = note: no closure captures `listed` and every use is a method receiver
       = help: bind the value with `let mut listed` and replace each `RefCell` method call with a direct read or write
    ");
}

// --- [COMPOSITION] ---------------------------------------------------------------------

snapshots!(interior_mutable_static, needless_cell);

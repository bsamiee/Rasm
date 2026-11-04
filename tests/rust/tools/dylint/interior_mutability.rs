// --- [OPERATIONS] ----------------------------------------------------------------------

snapshots! {
fn interior_mutable_static(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `static HITS` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:19:8
       |
    19 | static HITS: AtomicU32 = AtomicU32::new(0);
       |        ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default

    warning: `static ROOT` holds interior mutable `OnceLock<String>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:21:8
       |
    21 | static ROOT: OnceLock<String> = OnceLock::new();
       |        ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static ORIGIN` holds interior mutable `OnceLock<String>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:23:12
       |
    23 | pub static ORIGIN: OnceLock<String> = OnceLock::new();
       |            ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static COUNTER` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:27:8
       |
    27 | static COUNTER: LazyLock<AtomicU32> = LazyLock::new(|| AtomicU32::new(1));
       |        ^^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SHARED` holds interior mutable `Mutex<Vec<u8>>` in `Arc<Mutex<Vec<u8>>>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:29:8
       |
    29 | static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));
       |        ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static VISITS` holds interior mutable `Cell<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:36:12
       |
    36 |     static VISITS: Cell<u32> = const { Cell::new(0) };
       |            ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static CALLS` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:40:10
       |
    40 | counter!(CALLS);
       |          ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SEEN` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:54:12
       |
    54 |     static SEEN: AtomicU32 = AtomicU32::new(0);
       |            ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static HITS` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:19:8
       |
    19 | static HITS: AtomicU32 = AtomicU32::new(0);
       |        ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default

    warning: `static ROOT` holds interior mutable `OnceLock<String>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:21:8
       |
    21 | static ROOT: OnceLock<String> = OnceLock::new();
       |        ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static ORIGIN` holds interior mutable `OnceLock<String>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:23:12
       |
    23 | pub static ORIGIN: OnceLock<String> = OnceLock::new();
       |            ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static COUNTER` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:27:8
       |
    27 | static COUNTER: LazyLock<AtomicU32> = LazyLock::new(|| AtomicU32::new(1));
       |        ^^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SHARED` holds interior mutable `Mutex<Vec<u8>>` in `Arc<Mutex<Vec<u8>>>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:29:8
       |
    29 | static SHARED: LazyLock<Arc<Mutex<Vec<u8>>>> = LazyLock::new(|| Arc::new(Mutex::new(Vec::new())));
       |        ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static VISITS` holds interior mutable `Cell<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:36:12
       |
    36 |     static VISITS: Cell<u32> = const { Cell::new(0) };
       |            ^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static CALLS` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:40:10
       |
    40 | counter!(CALLS);
       |          ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SEEN` holds interior mutable `Atomic<u32>`
      --> tools/dylint/../../tests/rust/tools/dylint/interior_mutable_static.rs:54:12
       |
    54 |     static SEEN: AtomicU32 = AtomicU32::new(0);
       |            ^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it

    warning: `static SEQUENCE` holds interior mutable `Atomic<u8>`
      --> tools/dylint/examples/manual_default_field_values.rs:17:8
       |
    17 | static SEQUENCE: AtomicU8 = AtomicU8::new(0);
       |        ^^^^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default

    warning: `static CALLS` holds interior mutable `Atomic<usize>`
      --> tools/dylint/examples/manual_partition.rs:44:8
       |
    44 | static CALLS: AtomicUsize = AtomicUsize::new(0);
       |        ^^^^^
       |
       = help: build the value in `main` and pass it to each operation that reads or writes it
       = note: `#[warn(interior_mutable_static)]` on by default
    ");
}

fn needless_cell(found: &str) {
    insta::assert_snapshot!(found, @"
    warning: `total` uses `Cell` only to access and replace a copyable value
     --> tools/dylint/examples/needless_cell.rs:9:5
      |
    9 |     let total = Cell::new(0);
      |     ^^^^^^^^^^^^^^^^^^^^^^^^^
      |
      = help: bind the value with `let mut total`, use reads and assignment, and use `core::mem::replace` or `core::mem::take` for returned old values
      = note: `#[warn(needless_cell)]` on by default

    warning: `value` uses `Cell` only to access and replace a copyable value
      --> tools/dylint/examples/needless_cell.rs:51:5
       |
    51 |     let value = Cell::new(start);
       |     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
       |
       = help: bind the value with `let mut value`, use reads and assignment, and use `core::mem::replace` or `core::mem::take` for returned old values
    ");
}

}

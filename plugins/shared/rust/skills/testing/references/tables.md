# [TABLES]

`rstest` expands one function into a test per row, value, or file, each with a name nextest lists, filters, and reports alone.

## [01]-[ROWS]

`#[case::<row>]` rows feed the function's `#[case]` arguments in order:

```rust
#[rstest]
#[case::zero_bytes("0B", 0)]
#[case::one_kibibyte("1KiB", 1_024)]
#[case::bytes_past_a_kibibyte("1025B", 1_025)]
#[case::last_gibibyte_count("17179869183GiB", 18_446_744_072_635_809_792)]
fn size_text_reads_as_its_bytes(#[case] text: &str, #[case] want: u64) -> Result<(), SizeError> {
    assert_eq!(parsed(text)?, want, "`{text}`");
    Ok(())
}

#[rstest]
#[case::empty("", SizeError::Empty)]
#[case::count_alone("12", SizeError::Unit { found: String::new() })]
#[case::decimal_unit("12KB", SizeError::Unit { found: "KB".to_owned() })]
#[case::unit_alone("KiB", SizeError::Count { text: "KiB".to_owned() })]
#[case::one_gibibyte_count_past("17179869184GiB", SizeError::Overflow { text: "17179869184GiB".to_owned() })]
fn size_text_outside_the_grammar_is_refused_by_its_reason(#[case] text: &str, #[case] want: SizeError) {
    assert_eq!(parsed(text), Err(want), "`{text}`");
}
```

`parsed` reads a count and a binary unit into bytes, and `SizeError` names each refusal with the text it refused:
- Rows sit on each side of the overflow limit, and each refused row holds one sub-condition
- Tests read `<module>::<function>::case_<n>_<row>`, `<n>` counting from 1 zero-padded to the row count's width
- `test(/^<module>::<function>::/)` selects a table, and `test(=<module>::<function>::case_2_one_kibibyte)` one row
- Attributes written before a `#[case]` apply to that row alone, and attributes after the last row to every row
- `#[case(default)]` and `#[case(default(<expr>))]` give a column a fallback, which a row takes by omitting a trailing value or writing `_`, and `#[case(opt)]` takes an `Option` column
- `#[context] context: Context` (`use rstest::Context;`) hands the body its row's name as `context.description`, `Some` inside a `#[case]` row, the name a snapshot or an object prefix takes
- String literals convert through `FromStr` into a column's type (`#[case] at: Timestamp` from `"2026-11-24T10:00:00Z"`), so a row states a value in its source's notation
- Columns hold functions or closures (`fn(Record) -> Record` edits of one base row, `impl AsyncFn(&Path) -> io::Result<()>` arrangements), so a row states the one change defining its partition
- Async functions under `#[rstest]` take `#[tokio::test]` or `#[test_attr(..)]`, and one with neither fails to compile

## [02]-[MATRICES]

`#[values]` crosses rows with every value, nested one module per argument in declaration order:

```rust
#[rstest]
#[case::one(1)]
#[case::three(3)]
#[case::last_gibibyte_count(17_179_869_183)]
fn odd_count_in_any_unit_stays_in_that_unit(#[case] count: u64, #[values(Unit::B, Unit::KiB, Unit::MiB, Unit::GiB)] unit: Unit) -> Result<(), SizeError> {
    assert_eq!(Size::largest(parsed(&format!("{count}{unit}"))?), Size { count, unit }, "`{count}{unit}`");
    Ok(())
}
```

`Size::largest` states bytes as a count in the largest unit dividing them, and each test reads like `odd_count_in_any_unit_stays_in_that_unit::case_2_three::unit_3_Unit__MiB`:
- Value segments read `<argument>_<n>_<expression>`, the expression sanitized and cut to 64 characters, and a doc comment before a value replaces it as the name
- Products cross inputs whose outcome follows one rule across every value, and a row whose outcome changes with the value joins a table of its own

## [03]-[FILES]

`#[files("<glob>")]` runs one test per matching file, relative to `CARGO_MANIFEST_DIR` or to a `#[base_dir = "<dir>"]` written on the same argument:

```rust
#[rstest]
fn fixture_text_reads_as_the_bytes_beside_it(#[files("fixtures/sizes/*.size")] input: PathBuf) -> Result<(), Failure> {
    let (text, want) = (fs_err::read_to_string(&input)?, fs_err::read_to_string(input.with_extension("bytes"))?);
    let want = want.trim_end().parse::<u64>().map_err(|source| Failure::Count { file: input.with_extension("bytes"), source })?;
    assert_eq!(parsed(text.trim_end())?, want, "{}", input.display());
    Ok(())
}
```

`Failure` holds `io::Error` and `SizeError` through `#[from]` and a `Count` variant naming the file a `ParseIntError` came from, and each test reads `input_<n>_fixtures_sizes_<stem>`, `<stem>` the file's name:
- Globs expand at compile time and an empty glob fails the build, so a new file joins once the test file recompiles
- `#[mode = str]` and `#[mode = bytes]` pass `include_str!` and `include_bytes!` contents, rebuilding the test when a matched file changes
- `#[exclude("<regex>")]` drops matching paths, dot-prefixed paths stay out unless `#[include_dot_files]`, and directories stay out unless `#[dirs]`
- One `#[test]` looping over a folder reports every file as one verdict, and a `#[files]` table reports each file alone

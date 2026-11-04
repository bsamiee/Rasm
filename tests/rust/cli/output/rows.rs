#![feature(default_field_values, error_reporter, iterator_try_collect)]

use core::{
    iter,
    sync::atomic::{AtomicUsize, Ordering},
};
use std::{
    borrow::Cow,
    error::Report,
    ffi::OsString,
    io,
    path::{Path, PathBuf},
    process,
};

use clap::{Parser, ValueEnum, builder::PossibleValue};
use futures::stream;
use libtest_mimic::{Arguments, Trial};
use nonempty_collections::NEVec;
use rasm_command::CommandError;
use rasm_output::OutputError;
use rasm_telemetry::Defaults;
use serde::{
    Serialize,
    ser::{self, Error as _, SerializeMap as _},
};
use serde_content::{Data, Enum, Struct, Value};
use sysexits::ExitCode;
use tracing_subscriber::layer::Identity;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, ValueEnum)]
enum Case {
    Paths,
    Positions,
    Keys,
    Wrappers,
    Once,
    Refusal,
}

// --- [CONSTANTS] ------------------------------------------------------------------------

const CASE_ENV: &str = "RASM_OUTPUT_TEST_CASE";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Parser)]
struct Args {
    #[arg(long, env = CASE_ENV, value_enum)]
    case: Case,
}

#[derive(Parser)]
#[command(no_binary_name = true, disable_help_flag = true)]
struct Invocation {
    executable: PathBuf,
    #[arg(trailing_var_arg = true, allow_hyphen_values = true)]
    arguments: Vec<OsString>,
    #[arg(long, env = CASE_ENV, value_enum)]
    case: Option<Case>,
}

enum Key {
    Once(AtomicUsize),
    Refused,
}

impl Serialize for Key {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where S: ser::Serializer {
        match self {
            Self::Once(calls) => match calls.fetch_add(1, Ordering::Relaxed) {
                0 => serializer.serialize_str("once"),
                1.. => Err(S::Error::custom("key serialized more than once")),
            },
            Self::Refused => Err(S::Error::custom("refused key")),
        }
    }
}

enum Row {
    Value(Value<'static>),
    Key(Key),
    Refused,
}

impl Serialize for Row {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where S: ser::Serializer {
        match self {
            Self::Value(value) => value.serialize(serializer),
            Self::Key(key) => {
                let mut map = serializer.serialize_map(Some(1))?;
                map.serialize_entry(key, "kept")?;
                map.end()
            }
            Self::Refused => {
                let mut map = serializer.serialize_map(Some(1))?;
                map.serialize_entry("refused", "hidden")?;
                map.serialize_entry(&Key::Refused, "hidden")?;
                map.end()
            }
        }
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
enum Error {
    #[error(transparent)]
    Command(#[from] CommandError),
    #[error(transparent)]
    Output(#[from] OutputError),
    #[error(transparent)]
    Io(#[from] io::Error),
    #[error(transparent)]
    Csv(#[from] csv::Error),
}

impl From<Error> for ExitCode {
    fn from(value: Error) -> Self {
        match value {
            Error::Command(source) => source.into(),
            Error::Output(source) => source.into(),
            Error::Io(source) => source.into(),
            Error::Csv(_) => Self::DataErr,
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn rows(case: Case) -> Vec<Row> {
    let named = |name, value| Row::Value(Value::Map(vec![(Value::from(name), Value::from(value))]));
    match case {
        Case::Paths => vec![Row::Value(Value::Map(vec![
            (Value::from("a.b"), Value::from("direct")),
            (Value::from("a"), Value::Map(vec![(Value::from("b"), Value::from("nested"))])),
            (Value::from("list"), Value::Seq(vec![Value::Seq(vec![Value::from(1), Value::from(2)]), Value::Map(vec![(Value::from("b"), Value::from(true))]), Value::Unit])),
        ]))],
        Case::Positions => vec![Row::Value(Value::Option(Some(Box::new(Value::Struct(Box::new(Struct {
            name: Cow::Borrowed("Root"),
            data: Data::NewType {
                value: Value::Seq(vec![
                    Value::from(5),
                    Value::Map(vec![(Value::from(""), Value::Seq(vec![Value::from(1), Value::from(2)]))]),
                    Value::Enum(Box::new(Enum { name: Cow::Borrowed("State"), variant_index: 0, variant: Cow::Borrowed("Tagged"), data: Data::NewType { value: Value::from("kept") } })),
                    Value::Enum(Box::new(Enum {
                        name: Cow::Borrowed("State"),
                        variant_index: 1,
                        variant: Cow::Borrowed("Tuple"),
                        data: Data::Tuple { values: vec![Value::from(true), Value::from(false)] },
                    })),
                    Value::Enum(Box::new(Enum {
                        name: Cow::Borrowed("State"),
                        variant_index: 2,
                        variant: Cow::Borrowed("Record"),
                        data: Data::Struct { fields: vec![(Cow::Borrowed("field"), Value::from("done"))] },
                    })),
                ]),
            },
        }))))))],
        Case::Keys => vec![Row::Value(Value::Map(vec![
            (Value::from(-0.0), Value::from("negative")),
            (Value::from(0.0), Value::from("positive")),
            (Value::from("0.0"), Value::from("text")),
            (Value::from(i128::MIN), Value::from("signed")),
            (Value::from(u128::MAX), Value::from("unsigned")),
            (Value::Map(vec![(Value::from("b"), Value::from(1)), (Value::from("a"), Value::from(2))]), Value::from("first")),
            (Value::Map(vec![(Value::from("a"), Value::from(2)), (Value::from("b"), Value::from(1))]), Value::from("second")),
            (Value::Map(vec![(Value::from("a"), Value::from(1)), (Value::from("a"), Value::from(2))]), Value::from("duplicate")),
        ]))],
        Case::Wrappers => vec![Row::Value(Value::Map(vec![
            (Value::Unit, Value::from("unit")),
            (Value::Option(None), Value::from("none")),
            (Value::Struct(Box::new(Struct { name: Cow::Borrowed("Wrapped"), data: Data::NewType { value: Value::from("wrapped") } })), Value::from("newtype")),
            (Value::Option(Some(Box::new(Value::Seq(vec![Value::from(1), Value::from(2)])))), Value::from("optional")),
            (Value::Bytes(Cow::Borrowed(&[u8::MAX])), Value::from("bytes")),
            (Value::Seq(vec![Value::from(u8::MAX)]), Value::from("sequence")),
            (Value::Enum(Box::new(Enum { name: Cow::Borrowed("State"), variant_index: 0, variant: Cow::Borrowed("Ready"), data: Data::Unit })), Value::from("unit variant")),
            (
                Value::Enum(Box::new(Enum { name: Cow::Borrowed("State"), variant_index: 1, variant: Cow::Borrowed("Tagged"), data: Data::Tuple { values: vec![Value::from(1), Value::from(2)] } })),
                Value::from("tuple variant"),
            ),
            (
                Value::Enum(Box::new(Enum {
                    name: Cow::Borrowed("State"),
                    variant_index: 2,
                    variant: Cow::Borrowed("Record"),
                    data: Data::Struct { fields: vec![(Cow::Borrowed("b"), Value::from(1)), (Cow::Borrowed("a"), Value::from(2))] },
                })),
                Value::from("struct variant"),
            ),
        ]))],
        Case::Once => vec![Row::Key(Key::Once(AtomicUsize::new(0)))],
        Case::Refusal => vec![named("a", 1), Row::Refused, named("b", 2), named("a", 3)],
    }
}

#[cfg(test)]
fn exercised(executable: &Path, case: Case, value: &PossibleValue) -> Result<(), Error> {
    let (minimum, maximum) = (i128::MIN.to_string(), u128::MAX.to_string());
    let observed = process::Command::new(executable).env(CASE_ENV, value.get_name()).args(["--format", "csv", "--color", "never", "--sdk-disabled", "true"]).output()?;
    let expected = match case {
        Case::Paths => vec![vec!["a.b", "a.b", "list"], vec!["direct", "nested", "[[1,2],{\"b\":true},null]"]],
        Case::Positions => vec![vec!["0", "1.", "2.Tagged", "3.Tuple", "4.Record.field"], vec!["5", "[1,2]", "kept", "[true,false]", "done"]],
        Case::Keys => vec![
            vec!["-0.0", "0.0", "0.0", &minimum, &maximum, "{\"b\":1,\"a\":2}", "{\"a\":2,\"b\":1}", "{\"a\":1,\"a\":2}"],
            vec!["negative", "positive", "text", "signed", "unsigned", "first", "second", "duplicate"],
        ],
        Case::Wrappers => vec![
            vec!["", "", "wrapped", "[1,2]", "�", "[255]", "Ready", "{\"Tagged\":[1,2]}", "{\"Record\":{\"b\":1,\"a\":2}}"],
            vec!["unit", "none", "newtype", "optional", "bytes", "sequence", "unit variant", "tuple variant", "struct variant"],
        ],
        Case::Once => vec![vec!["once"], vec!["kept"]],
        Case::Refusal => vec![vec!["a"], vec!["1"], vec!["3"]],
    };
    let actual = csv::ReaderBuilder::new()
        .has_headers(false)
        .from_reader(observed.stdout.as_slice())
        .records()
        .map(|record| record.map(|record| record.iter().map(str::to_owned).collect::<Vec<_>>()))
        .try_collect::<Vec<_>>()?;
    assert_eq!(actual, expected, "CSV must preserve columns, labels and accepted rows for {}", value.get_name());
    let status = match case {
        Case::Refusal => ExitCode::DataErr,
        Case::Paths | Case::Positions | Case::Keys | Case::Wrappers | Case::Once => ExitCode::Ok,
    };
    assert_eq!(observed.status.code(), Some(i32::from(u8::from(status))), "CSV must retain outcome for {}", value.get_name());
    if matches!(case, Case::Refusal) {
        let errors = String::from_utf8_lossy(&observed.stderr);
        assert!(
            errors.contains("refused key") && errors.contains("row 1") && errors.contains("column `b`") && errors.contains("row 2"),
            "Row refusals must preserve each source and position: {errors}"
        );
    }
    Ok(())
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> process::ExitCode {
    let invocation = Invocation::parse();
    if invocation.case.is_some() {
        rasm_output::run(Defaults { .. }, Identity::new(), async |args: Args, _filter| Ok::<_, NEVec<Error>>(stream::iter(rows(args.case).into_iter().map(Ok::<_, Error>))))
    } else {
        let Invocation { executable, arguments, case: _ } = invocation;
        let args = Arguments::from_iter(iter::once(executable.as_os_str()).chain(arguments.iter().map(OsString::as_os_str)));
        let tests = Case::value_variants()
            .iter()
            .filter_map(|case| case.to_possible_value().map(|value| (*case, value)))
            .map(|(case, value)| {
                let executable = executable.clone();
                Trial::test(value.get_name().to_owned(), move || exercised(&executable, case, &value).map_err(|error| Report::new(error).into()))
            })
            .collect();
        libtest_mimic::run(&args, tests).exit_code()
    }
}

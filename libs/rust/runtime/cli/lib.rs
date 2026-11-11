#![feature(error_reporter, trait_alias, try_blocks_heterogeneous)]

use core::error::Error;
use core::fmt;
use core::str::FromStr as _;
#[cfg(windows)]
use core::task::{Context, Poll};
use std::error::Report;
use std::ffi::{OsStr, OsString};
use std::path::PathBuf;
use std::process::{ExitCode, Termination};
use std::{env, io};

use camino::Utf8PathBuf;
use clap::builder::{Resettable, Styles};
use clap::error::ErrorKind;
use clap::parser::ValueSource;
use clap::{Arg, ArgMatches, Args, Command, CommandFactory, FromArgMatches, Parser, ValueHint};
use clap_complete::CompleteEnv;
use enum_map::EnumMap;
use futures::stream;
use heck::ToShoutySnakeCase as _;
use itertools::{Either, Itertools as _};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use opentelemetry::global;
use rasm_telemetry::process::{self, Filter, Layer, Lenient, MakeWriter, Program, Stated};
use tokio::runtime::Builder;
#[cfg(unix)]
use tokio::signal::unix::{SignalKind, signal};
#[cfg(windows)]
use tokio::signal::windows;
#[cfg(unix)]
use tokio_stream::wrappers::SignalStream;
use toml::de;

// --- [TYPES] ---------------------------------------------------------------------------

pub trait Failure = Error + Into<sysexits::ExitCode> + 'static;

pub type Signals = stream::SelectAll<cfg_select! { unix => { SignalStream } windows => { stream::PollFn<Box<dyn FnMut(&mut Context<'_>) -> Poll<Option<()>> + Send + Sync>> } }>;

#[derive(Clone, Copy, Debug, PartialEq, Eq, derive_more::Display, enum_map::Enum)]
#[display(rename_all = "lowercase")]
pub enum Folder {
    Log,
    Cache,
    State,
    Runtime,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

pub const DERIVED: &str = "";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::Display)]
pub enum CommandOperation {
    #[display("start async runtime")]
    Runtime,
    #[display("read this executable")]
    Executable,
    #[display("listen for shutdown signals")]
    Signal,
    #[display("write man pages into {_0}")]
    Manual(Utf8PathBuf),
}

#[derive(Args)]
#[group(skip)]
struct Manual {
    #[arg(long, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, help = "Write man pages into DIRECTORY and exit")]
    manual: Option<Utf8PathBuf>,
}

#[derive(Args)]
#[group(skip)]
#[command(next_help_heading = "Process")]
struct Named {
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of the log file, default the per-user log folder")]
    log_dir: Option<Stated<Utf8PathBuf>>,
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of cached files, default the per-user cache folder")]
    cache_dir: Option<Stated<Utf8PathBuf>>,
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of kept state, default the per-user state folder")]
    state_dir: Option<Stated<Utf8PathBuf>>,
    #[arg(long, global = true, value_name = "DIRECTORY", value_hint = ValueHint::DirPath, value_parser = Lenient(Utf8PathBuf::from_str))]
    #[arg(help = "Folder of sockets and locks, default the per-user runtime folder")]
    runtime_dir: Option<Stated<Utf8PathBuf>>,
    #[command(flatten)]
    telemetry: process::Options,
}

#[derive(Args)]
#[group(skip)]
struct Options {
    #[command(flatten)]
    manual: Manual,
    #[command(flatten)]
    color: colorchoice_clap::Color,
    #[command(flatten)]
    named: Named,
}

#[derive(Debug)]
pub struct Augmented<O, P> {
    pub options: O,
    pub args: P,
}

impl<O, P> CommandFactory for Augmented<O, P>
where
    O: Args,
    P: CommandFactory,
{
    fn command() -> Command { augmented(P::command(), O::augment_args) }

    fn command_for_update() -> Command { augmented(P::command_for_update(), O::augment_args_for_update) }
}

impl<O, P> FromArgMatches for Augmented<O, P>
where
    O: FromArgMatches,
    P: FromArgMatches,
{
    fn from_arg_matches(matches: &ArgMatches) -> Result<Self, clap::Error> { Self::from_arg_matches_mut(&mut matches.clone()) }

    fn from_arg_matches_mut(matches: &mut ArgMatches) -> Result<Self, clap::Error> { Ok(Self { options: O::from_arg_matches_mut(matches)?, args: P::from_arg_matches_mut(matches)? }) }

    fn update_from_arg_matches(&mut self, matches: &ArgMatches) -> Result<(), clap::Error> { self.update_from_arg_matches_mut(&mut matches.clone()) }

    fn update_from_arg_matches_mut(&mut self, matches: &mut ArgMatches) -> Result<(), clap::Error> {
        self.options.update_from_arg_matches_mut(matches)?;
        self.args.update_from_arg_matches_mut(matches)
    }
}

impl<O, P> Parser for Augmented<O, P>
where
    O: Args,
    P: CommandFactory + FromArgMatches,
{
}

#[derive(Debug)]
pub struct Invocation<A> {
    pub args: A,
    pub executable: PathBuf,
    pub styles: Styles,
    pub folders: EnumMap<Folder, Option<Utf8PathBuf>>,
    pub filter: Filter,
    pub parent: opentelemetry::Context,
    pub settings: Vec<(OsString, OsString)>,
    pub export: Vec<(OsString, OsString)>,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum CommandError {
    #[error("cannot {operation}")]
    Io { operation: CommandOperation, source: io::Error },
    #[error(transparent)]
    File(#[from] io::Error),
    #[error("platform names no per-user {folder} folder, set --{folder}-dir")]
    Folder { folder: Folder },
    #[error("{}", fmt::from_fn(|formatter| match phase {
        settings::Phase::Parse(origin) | settings::Phase::Decode { path: _, origin: Some(origin) } => formatter.write_str(&origin.excerpt(&phase.to_string())),
        settings::Phase::Decode { path: _, origin: None } => write!(formatter, "{phase}"),
    }))]
    Settings { phase: Phase, source: Box<de::Error> },
}

impl From<CommandError> for sysexits::ExitCode {
    fn from(value: CommandError) -> Self {
        match value {
            CommandError::Io { operation: _, source } | CommandError::File(source) => source.into(),
            CommandError::Folder { folder: _ } | CommandError::Settings { phase: _, source: _ } => Self::Config,
        }
    }
}

impl From<CommandError> for NEVec<CommandError> {
    fn from(value: CommandError) -> Self { Self::new(value) }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn signals() -> Result<Signals, CommandError> {
    (try bikeshed io::Result<Signals> {
        cfg_select! {
            unix => [SignalKind::interrupt(), SignalKind::terminate(), SignalKind::hangup()].map(|kind| signal(kind).map(SignalStream::new)).into_iter().collect::<io::Result<Signals>>()?,
            windows => {{
                let (mut interrupt, mut interrupt_break, mut close, mut logoff, mut shutdown) =
                    (windows::ctrl_c()?, windows::ctrl_break()?, windows::ctrl_close()?, windows::ctrl_logoff()?, windows::ctrl_shutdown()?);
                let receivers: [Box<dyn FnMut(&mut Context<'_>) -> Poll<Option<()>> + Send + Sync>; 5] = [
                    Box::new(move |context| interrupt.poll_recv(context)),
                    Box::new(move |context| interrupt_break.poll_recv(context)),
                    Box::new(move |context| close.poll_recv(context)),
                    Box::new(move |context| logoff.poll_recv(context)),
                    Box::new(move |context| shutdown.poll_recv(context)),
                ];
                receivers.into_iter().map(stream::poll_fn).collect::<Signals>()
            }}
        }
    })
    .map_err(|source| CommandError::Io { operation: CommandOperation::Signal, source })
}

fn augmented(program: Command, augment: impl FnOnce(Command) -> Command) -> Command {
    let (about, long_about) = (program.get_about().cloned(), program.get_long_about().cloned());
    augment(program).about(Resettable::from(about)).long_about(Resettable::from(long_about))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub fn run<A, W, L, T, E>(program: Program, stderr: W, layer: L, runtime: impl FnOnce(&A) -> Builder, work: impl AsyncFnOnce(Invocation<A>) -> Result<T, NEVec<E>>) -> ExitCode
where
    A: CommandFactory + FromArgMatches,
    W: MakeWriter,
    L: Layer,
    T: Termination,
    E: Failure,
{
    fn status<F>(failures: NEVec<F>) -> ExitCode
    where F: Into<sysexits::ExitCode> {
        sysexits::ExitCode::report(failures.into_nonempty_iter().next().0.into())
    }
    let factory = || {
        let command = Augmented::<Options, A>::command();
        let prefix = command.get_name().to_shouty_snake_case();
        let named = Named::augment_args(Command::new(command.get_name().to_owned()));
        let unnamed = named.get_arguments().filter(|argument| argument.get_env().is_none());
        let derived = command.get_arguments().filter(|argument| argument.get_env() == Some(OsStr::new(DERIVED)));
        let own = Vec::from_iter(unnamed.chain(derived).map(|argument| argument.get_id().clone()));
        command.mut_args(|argument| own.contains(argument.get_id()).then(|| format!("{prefix}_{}", argument.get_id().as_str().to_shouty_snake_case())).into_iter().fold(argument, Arg::env))
    };
    CompleteEnv::with_factory(factory).complete();
    let command = factory();
    let parsed = command.clone().try_get_matches().and_then(|mut matches| {
        let declared = process::Options::augment_args(Command::new(command.get_name().to_owned()));
        let exported = Vec::from_iter(declared.get_arguments().filter(|argument| argument.get_env().is_some()).map(|argument| argument.get_id().clone()));
        let rows = command.get_arguments().filter_map(|argument| {
            let (id, variable) = (argument.get_id(), argument.get_env()?);
            let mut values = matches.get_raw(id.as_str()).filter(|_| matches!(matches.value_source(id.as_str()), Some(ValueSource::CommandLine | ValueSource::EnvVariable)))?;
            let value = match argument.get_value_delimiter() {
                Some(delimiter) => Vec::from_iter(values).join(OsStr::new(delimiter.encode_utf8(&mut [0; char::MAX_LEN_UTF8]))),
                None => values.next_back()?.to_owned(),
            };
            Some((exported.contains(id), (variable.to_owned(), value)))
        });
        let stated = Vec::from_iter(rows);
        Augmented::<Options, A>::from_arg_matches_mut(&mut matches).map(|parsed| (parsed, stated))
    });
    let printed = |error: &clap::Error, status: sysexits::ExitCode| error.print().map_or_else(|source| sysexits::ExitCode::from(source).into(), |()| status.into());
    let exited = |error: &clap::Error| printed(error, if error.use_stderr() { sysexits::ExitCode::Usage } else { sysexits::ExitCode::Ok });
    let reported = |error: CommandError| printed(&clap::Error::raw(ErrorKind::Io, format_args!("{}\n", Report::new(&error))).with_cmd(&command), error.into());
    let written = |directory: Utf8PathBuf| {
        try bikeshed Result<(), CommandError> {
            fs_err::create_dir_all(&directory).map_err(CommandError::File)?;
            clap_mangen::generate_to(command.clone(), &directory).map_err(|source| CommandError::Io { operation: CommandOperation::Manual(directory), source })?
        }
        .map_or_else(reported, |()| ExitCode::SUCCESS)
    };
    let (Augmented { options: Options { manual: Manual { manual: _ }, color, named: Named { log_dir, cache_dir, state_dir, runtime_dir, telemetry } }, args }, rows) = match parsed {
        Ok((Augmented { options: Options { manual: Manual { manual: Some(directory) }, color: _, named: _ }, args: _ }, _)) => return written(directory),
        Ok(parsed) => parsed,
        Err(error) => {
            return match factory().ignore_errors(true).try_get_matches().and_then(|mut matches| Manual::from_arg_matches_mut(&mut matches)) {
                Ok(Manual { manual: Some(directory) }) => written(directory),
                Ok(Manual { manual: None }) => exited(&error),
                Err(displayed) => exited(&displayed),
            };
        }
    };
    color.write_global();
    #[expect(clippy::disallowed_methods, reason = "the composition root reads the running executable once and hands it to the work, so no program reads it itself")]
    let executable = match env::current_exe() {
        Ok(executable) => executable,
        Err(source) => return reported(CommandError::Io { operation: CommandOperation::Executable, source }),
    };
    let runtime = match runtime(&args).enable_all().build() {
        Ok(runtime) => runtime,
        Err(source) => return reported(CommandError::Io { operation: CommandOperation::Runtime, source }),
    };
    let chosen = EnumMap::<Folder, _>::from_array([log_dir, cache_dir, state_dir, runtime_dir].map(|row| row.transpose().map(Option::flatten)));
    let folders = EnumMap::from_fn(|folder| {
        let shared = || match folder {
            Folder::Log => cfg_select! {
                target_vendor = "apple" => dirs::home_dir().map(|home| home.join("Library").join("Logs")),
                windows => dirs::data_local_dir().map(|data| data.join("logs")),
                _ => dirs::state_dir(),
            },
            Folder::Cache => dirs::cache_dir(),
            Folder::State => dirs::state_dir().or_else(dirs::data_local_dir),
            Folder::Runtime => dirs::runtime_dir().or_else(dirs::cache_dir),
        };
        chosen[folder].as_ref().ok().cloned().flatten().or_else(|| Utf8PathBuf::from_path_buf(shared()?.join(command.get_name())).ok())
    });
    let (log, styles) = (folders[Folder::Log].clone(), command.get_styles().clone());
    let program = Program { refused: program.refused.into_iter().chain(chosen.into_values().filter_map(Result::err)).collect(), ..program };
    let instrumented = telemetry.instrumented(&command, program, log, stderr, layer, async |filter, parent| {
        let carried = global::get_text_map_propagator(|propagator| Vec::from_iter(propagator.fields().map(str::to_owned)));
        let stated = rows.into_iter().filter(|(_, (variable, _))| !carried.iter().any(|field| variable.eq_ignore_ascii_case(field)));
        let (export, settings) = stated.partition_map(|(exported, row)| if exported { Either::Left(row) } else { Either::Right(row) });
        work(Invocation { args, executable, styles, folders, filter, parent, settings, export }).await
    });
    runtime.block_on(instrumented).map_or_else(|failures| failures.either(status, status), Termination::report)
}

mod settings;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use settings::{Layers, Origin, Phase, Provenance, decoded};

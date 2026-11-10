#![feature(default_field_values, error_reporter, nonpoison_mutex, sync_nonpoison, trait_alias, try_blocks_heterogeneous)]
#![cfg_attr(unix, feature(iterator_try_collect))]

use core::fmt;
#[cfg(windows)]
use core::task::{Context, Poll};
use std::error::Report;
use std::ffi::OsString;
use std::path::PathBuf;
use std::process::{ExitCode, Termination};
use std::{env, io};

use camino::Utf8PathBuf;
use clap::builder::{Resettable, Styles};
use clap::error::ErrorKind;
use clap::{ArgMatches, Args, Command, CommandFactory, FromArgMatches, Parser, ValueHint};
use clap_complete::CompleteEnv;
use enum_map::EnumMap;
use futures::stream;
use heck::ToShoutySnakeCase as _;
use nonempty_collections::NEVec;
use rasm_observability::ObservabilityError;
use tokio::runtime::Builder;
#[cfg(unix)]
use tokio::signal::unix::{SignalKind, signal};
#[cfg(windows)]
use tokio::signal::windows;
#[cfg(unix)]
use tokio_stream::wrappers::SignalStream;
use toml::de;
use tracing_subscriber::filter::ParseError;
use tracing_subscriber::reload;

use self::telemetry::{Failure, Filter, Layer, MakeWriter, Program};

// --- [TYPES] ---------------------------------------------------------------------------

pub type Signals = stream::SelectAll<cfg_select! { unix => { SignalStream } windows => { stream::PollFn<Box<dyn FnMut(&mut Context<'_>) -> Poll<Option<()>> + Send + Sync>> } }>;

#[derive(Clone, Copy, Debug, PartialEq, Eq, derive_more::Display, enum_map::Enum)]
#[display(rename_all = "lowercase")]
pub enum Folder {
    Log,
    Cache,
    State,
    Runtime,
}

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
struct Options {
    #[command(flatten)]
    manual: Manual,
    #[command(flatten)]
    color: colorchoice_clap::Color,
    #[command(flatten)]
    telemetry: telemetry::Options,
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
    #[error("cannot filter scope {scope}")]
    Scope { scope: &'static str, source: ParseError },
    #[error(transparent)]
    Reload(reload::Error),
    #[error("cannot install tracing subscriber, one is installed")]
    Dispatch,
    #[error("cannot forward log records, a logger is installed")]
    Forward,
    #[error(transparent)]
    Telemetry(#[from] ObservabilityError),
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
            CommandError::Scope { scope: _, source: _ } | CommandError::Reload(_) | CommandError::Dispatch | CommandError::Forward => Self::Software,
            CommandError::Telemetry(error) => error.into(),
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
            unix => [SignalKind::interrupt(), SignalKind::terminate(), SignalKind::hangup()].map(|kind| signal(kind).map(SignalStream::new)).into_iter().try_collect::<Signals>()?,
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
    E: Failure + From<CommandError>,
{
    let factory = || {
        let command = Augmented::<Options, A>::command();
        let prefix = command.get_name().to_shouty_snake_case();
        let named = telemetry::Options::augment_args(Command::new(command.get_name().to_owned()));
        let own = Vec::from_iter(named.get_arguments().filter(|argument| argument.get_env().is_none()).map(|argument| argument.get_id().clone()));
        command.mut_args(|argument| {
            if own.contains(argument.get_id()) {
                let variable = format!("{prefix}_{}", argument.get_id().as_str().to_shouty_snake_case());
                argument.env(variable)
            } else {
                argument
            }
        })
    };
    CompleteEnv::with_factory(factory).complete();
    let command = factory();
    let parsed = command.clone().try_get_matches().and_then(|mut matches| {
        let stated = telemetry::Options::stated(&command, &matches);
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
    let (Augmented { options: Options { manual: Manual { manual: _ }, color, telemetry }, args }, [settings, export]) = match parsed {
        Ok((Augmented { options: Options { manual: Manual { manual: Some(directory) }, color: _, telemetry: _ }, args: _ }, _)) => return written(directory),
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
    let entered = runtime.enter();
    let started = match telemetry.start(&command, program, stderr, layer, &runtime) {
        Ok(started) => started,
        Err(failures) => return telemetry::status(failures),
    };
    let (styles, folders, filter, parent) = (command.get_styles().clone(), started.folders.clone(), started.filter.clone(), started.parent.clone());
    let outcome = runtime.block_on(work(Invocation { args, executable, styles, folders, filter, parent, settings, export }));
    let status = started.finish(&runtime, outcome);
    drop(entered);
    status
}

mod settings;
pub mod telemetry;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use settings::{Layers, Origin, Phase, Provenance, decoded};

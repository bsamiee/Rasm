use core::time::Duration;
use std::io::{self, Write as _};
use std::process::ExitCode;

use clap::Parser;
use jiff::fmt::friendly::SpanParser;
use rmcp::handler::server::wrapper::Parameters;
use rmcp::model::{ElicitRequestParams, ElicitationAction, ElicitationSchema, ProgressNotificationParam, ServerCapabilities, ServerConfig};
use rmcp::service::RequestContext;
use rmcp::{ErrorData, RoleServer, ServerHandler, ServiceExt as _, tool, tool_handler, tool_router};
use schemars::JsonSchema;
use serde::Deserialize;
use tokio::time;
use tokio_util::sync::CancellationToken;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Parser)]
#[command(about = "Stdio MCP server with settable start delay, start failure, crash, progress, elicitation, and result size")]
struct Arguments {
    #[arg(long, value_name = "DURATION", default_value = "0s", value_parser = |text: &str| SpanParser::new().parse_unsigned_duration(text), help = "Time before the server reads its first message")]
    start_delay: Duration,
    #[arg(long, help = "Exit with status 7 before serving, after one line on standard error")]
    fail_start: bool,
}

#[derive(Debug, Deserialize, JsonSchema)]
struct Echo {
    text: String,
}

#[derive(Debug, Deserialize, JsonSchema)]
struct Sized {
    bytes: usize,
}

#[derive(Debug, Deserialize, JsonSchema)]
struct Slow {
    millis: u64,
    steps: u32,
}

#[derive(Debug, Deserialize, JsonSchema)]
struct Ask {
    question: String,
}

#[derive(Debug, Deserialize, JsonSchema)]
struct Variable {
    name: String,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
struct Fixture {
    crashed: CancellationToken,
}

#[tool_router]
impl Fixture {
    #[tool(description = "Return the text")]
    fn echo(&self, Parameters(Echo { text }): Parameters<Echo>) -> String { text }

    #[tool(description = "Return a string of the given length")]
    fn sized(&self, Parameters(Sized { bytes }): Parameters<Sized>) -> String { "x".repeat(bytes) }

    #[tool(description = "Sleep in steps, reporting progress after each")]
    async fn slow(&self, Parameters(Slow { millis, steps }): Parameters<Slow>, context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        let token = context.meta.get_progress_token();
        for step in 1..=steps {
            tokio::select! {
                () = time::sleep(Duration::from_millis(millis) / steps.max(1)) => {}
                () = context.ct.cancelled() => return Err(ErrorData::internal_error("cancelled", None)),
            }
            if let Some(token) = token.clone() {
                context.peer.notify_progress(ProgressNotificationParam::new(token, f64::from(step)).with_total(f64::from(steps))).await.map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
            }
        }
        Ok("done".to_owned())
    }

    #[tool(description = "Ask the client a question through elicitation and return its answer")]
    async fn ask(&self, Parameters(Ask { question }): Parameters<Ask>, context: RequestContext<RoleServer>) -> Result<String, ErrorData> {
        let requested_schema = ElicitationSchema::builder().required_string("answer").build().map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
        let answered = context.peer.create_elicitation(ElicitRequestParams::FormElicitationParams { meta: None, message: question, requested_schema }).await;
        let result = answered.map_err(|error| ErrorData::internal_error(error.to_string(), None))?;
        Ok(match result.action {
            ElicitationAction::Accept => result.content.map_or_else(String::new, |content| content.to_string()),
            _ => "unanswered".to_owned(),
        })
    }

    #[tool(description = "Return the server process id, working directory, and one environment variable")]
    fn whoami(&self, Parameters(Variable { name }): Parameters<Variable>) -> String {
        #[expect(clippy::disallowed_methods, reason = "the fixture reports the environment and directory its parent gave it")]
        let (value, directory) = (std::env::var(&name).ok(), std::env::current_dir().ok());
        serde_json::json!({ "pid": std::process::id(), "directory": directory, "value": value }).to_string()
    }

    #[tool(description = "End the server process with status 3")]
    fn crash(&self) -> String {
        self.crashed.cancel();
        "crashing".to_owned()
    }
}

#[tool_handler]
impl ServerHandler for Fixture {
    fn get_info(&self) -> ServerConfig { ServerConfig::new(ServerCapabilities::builder().enable_tools().enable_tool_list_changed().build()) }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main(flavor = "current_thread")]
async fn main() -> ExitCode {
    let Arguments { start_delay, fail_start } = Arguments::parse();
    if fail_start {
        return match writeln!(io::stderr().lock(), "fixture refuses to start") {
            Ok(()) | Err(_) => ExitCode::from(7),
        };
    }
    time::sleep(start_delay).await;
    let fixture = Fixture { crashed: CancellationToken::new() };
    let crashed = fixture.crashed.clone();
    let Ok(running) = fixture.serve(rmcp::transport::stdio()).await else { return ExitCode::from(4) };
    tokio::select! {
        _ = running.waiting() => ExitCode::SUCCESS,
        () = crashed.cancelled() => ExitCode::from(3),
    }
}

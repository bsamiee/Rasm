#![feature(deref_patterns)]

use core::hint::black_box;
use std::io::{self, BufRead};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Job {
    kind: String,
    name: String,
}

#[derive(Debug)]
struct Step {
    state: &'static str,
    label: &'static str,
}

#[derive(Debug, Clone, Copy)]
enum Color {
    Red,
    Blue,
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const DONE: &str = "done";
const PAUSE: &str = "pause";

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn build(job: &Job) -> usize { job.name.len() }

const fn test(job: &Job) -> usize { job.name.len().strict_add(1) }

fn run(job: &Job) -> usize {
    match &job.kind {
        "build" => build(job),
        "test" => test(job),
        _ => 0,
    }
}

const fn steps() -> [Step; 2] { [Step { state: "pending", label: "fetch" }, Step { state: DONE, label: "unpack" }] }

fn finished(steps: &[Step]) -> usize { steps.iter().filter(|step| step.state == DONE).count() }

fn labels(steps: &[Step]) -> Vec<&'static str> { steps.iter().map(|step| step.label).collect() }

fn logged(level: &str, text: &str) -> String { if level == "warn" { text.to_uppercase() } else { text.to_owned() } }

fn command(input: &str) -> String { input.trim().to_lowercase() }

fn dispatched(input: &str) -> usize {
    match &command(input) {
        "start" => input.len(),
        "stop" | "halt" => input.len().strict_mul(2),
        _ => 0,
    }
}

fn mode(input: &str) -> usize {
    let mode = "fast";
    match mode {
        "fast" => input.len(),
        "slow" => input.len().strict_mul(2),
        PAUSE => input.len().strict_add(1),
        _ => 0,
    }
}

fn listed(input: &str) -> bool { ["yes", "on"].contains(&command(input).as_str()) }

fn parsed(text: &str) -> Option<Color> {
    match text {
        "red" => Some(Color::Red),
        "blue" => Some(Color::Blue),
        _ => None,
    }
}

fn chained(text: &str) -> Color {
    if text == "red" {
        Color::Red
    } else if text == "blue" {
        Color::Blue
    } else {
        Color::Red
    }
}

fn named(job: &Job) -> bool { job.name == "lints" }

fn answered(input: &mut dyn BufRead) -> bool {
    let mut answer = String::from("n");
    input.read_line(&mut answer).unwrap_or_default();
    answer.trim() == "y"
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let input = black_box("build").to_owned();
    let job = Job { kind: input.clone(), name: input.clone() };
    black_box((
        run(&job),
        finished(&steps()),
        labels(&steps()),
        logged("warn", &input),
        logged("info", &input),
        dispatched(&input),
        mode(&input),
        listed(&input),
        parsed(&input),
        chained(&input),
        named(&job),
        answered(&mut io::stdin().lock()),
        (PAUSE, build(&job), test(&job)),
    ));
}

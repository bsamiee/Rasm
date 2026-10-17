#![feature(deref_patterns)]

use core::hint::black_box;
use std::{
    collections::BTreeMap,
    io::{self, BufRead},
};

// --- [TYPES] ---------------------------------------------------------------------------

trait Severity {
    fn base(&self) -> usize;

    fn weight(&self, level: &str) -> usize {
        if level == "warn" {
            self.base().strict_mul(2)
        } else if level == "info" {
            self.base()
        } else {
            0
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Job {
    kind: String,
    name: String,
}

impl Severity for Job {
    fn base(&self) -> usize { self.name.len() }
}

#[derive(Debug)]
struct Step {
    state: &'static str,
    label: &'static str,
}

impl Severity for Step {
    fn base(&self) -> usize { self.label.len() }
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

fn returned(text: &str) -> Option<Color> {
    match text {
        "red" => return Some(Color::Red),
        "blue" => return Some(Color::Blue),
        _ => {}
    }
    None
}

fn leveled(level: &str, text: &str) -> usize {
    if level == "warn" {
        text.len()
    } else if level == "info" {
        text.len().strict_mul(2)
    } else {
        0
    }
}

fn answered(input: &mut dyn BufRead) -> bool {
    let mut answer = String::from("n");
    input.read_line(&mut answer).unwrap_or_default();
    answer.trim() == "y"
}

fn tallied(words: &[&str]) -> usize {
    let mut tally = BTreeMap::new();
    tally.insert("ok", words.len());
    tally.insert("err", 0);
    tally.get("ok").copied().unwrap_or_default().strict_add(tally["err"])
}

fn speed(mode: &str) -> u32 {
    match mode {
        "fast" => 1,
        "slow" => 2,
        _ => 3,
    }
}

fn flag(text: &str) -> bool {
    match text {
        "yes" => true,
        "no" => false,
        _ => false,
    }
}

#[derive(Debug)]
pub struct Request {
    pub kind: String,
}

fn format_of(request: &Request) -> u32 {
    if request.kind == "json" {
        1
    } else if request.kind == "text" {
        2
    } else {
        3
    }
}

fn counted<'word>(words: &[&'word str]) -> BTreeMap<&'word str, usize> {
    words.iter().fold(BTreeMap::new(), |mut counts, &word| {
        counts.entry(word).and_modify(|count| *count = count.strict_add(1)).or_insert(1);
        counts
    })
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let input = black_box("build").to_owned();
    let job = Job { kind: input.clone(), name: input.clone() };
    black_box(&(
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
        returned(&input),
        (leveled("warn", &input), (leveled as fn(&str, &str) -> usize)("info", &input)),
        (job.weight("warn"), job.weight("info"), steps()[0].weight("warn")),
        answered(&mut io::stdin().lock()),
        (PAUSE, build(&job), test(&job)),
        (tallied(&[&input]), counted(&[&input]).len()),
        (speed(&input), flag(&input), format_of(&Request { kind: input.clone() })),
    ));
}

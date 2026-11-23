#![feature(error_reporter)]

use core::{fmt, iter};
use std::collections::{BTreeMap, BTreeSet};
use std::error::Report;
use std::io;
use std::path::{Path, PathBuf};
use std::process::{Command, ExitStatus, Stdio};

use cargo_metadata::diagnostic::{Diagnostic, DiagnosticLevel};
use cargo_metadata::{Message, MetadataCommand};
use itertools::Itertools as _;
use thiserror::Error;
use walkdir::WalkDir;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, derive_more::Display)]
#[display(rename_all = "lowercase")]
enum Stage {
    Start,
    #[display("read the output of")]
    Output,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::Display)]
#[display("{lint}{}{}", fmt::from_fn(|out| target.iter().try_for_each(|held| write!(out, " from {held}"))), fmt::from_fn(|out| text.iter().try_for_each(|held| write!(out, " naming `{held}`"))))]
struct Annotation {
    line: usize,
    target: Option<String>,
    lint: String,
    text: Option<String>,
}

#[derive(Debug, derive_more::Display)]
#[display("{lint} at {level:?} from {target} through {primaries} primary spans\n{rendered}")]
struct Finding {
    line: usize,
    lint: String,
    level: DiagnosticLevel,
    target: String,
    primaries: usize,
    wording: String,
    rendered: String,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, derive_more::Display)]
enum Problem {
    #[display("{}:{} expects {annotation} and nothing reports it", path.display(), annotation.line)]
    Missing { path: PathBuf, annotation: Annotation },
    #[display("{}:{} reports with no annotation {finding}", path.display(), finding.line)]
    Unexpected { path: PathBuf, finding: Finding },
    #[display("{} allows {lint}, the lint it exists to exercise", path.display())]
    Silenced { path: PathBuf, lint: String },
    #[display("{lint} fires in no fixture")]
    Unfired { lint: String },
}

#[derive(Debug, Error)]
enum Failure {
    #[error("cannot {stage} cargo dylint")]
    Io { stage: Stage, source: io::Error },
    #[error("cannot locate the workspace root")]
    Metadata { source: cargo_metadata::Error },
    #[error("cannot read fixture annotations")]
    Fixture { source: walkdir::Error },
    #[error("cannot read {}", path.display())]
    Read { path: PathBuf, source: io::Error },
    #[error("{}:{line} annotation `{text}` names no lint or no line", path.display())]
    Annotation { path: PathBuf, line: usize, text: String },
    #[error("cargo dylint exited with {status}\n{errors}")]
    Compile { errors: String, status: ExitStatus },
    #[error("fixtures and lint diagnostics differ\n{}", problems.iter().format("\n"))]
    Mismatch { problems: Vec<Problem> },
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[test]
fn every_annotation_meets_one_diagnostic_and_every_diagnostic_one_annotation() -> Result<(), Report<Failure>> {
    let root = MetadataCommand::new().no_deps().exec().map_err(|source| Failure::Metadata { source })?.workspace_root.into_std_path_buf();
    let manifest = Path::new(env!("CARGO_MANIFEST_DIR"));
    let listed = Command::new(env!("CARGO")).args(["dylint", "list", "--all"]).stderr(Stdio::null()).output().map_err(|source| Failure::Io { stage: Stage::Start, source })?;
    let declared = String::from_utf8_lossy(&listed.stdout)
        .lines()
        .filter_map(|line| match line.split_whitespace().collect::<Vec<_>>()[..] {
            [lint, "deny" | "forbid" | "warn" | "allow", ..] => Some(lint.to_owned()),
            _ => None,
        })
        .collect::<BTreeSet<_>>();
    let fixtures = ["examples", "expansions"]
        .into_iter()
        .flat_map(|folder| WalkDir::new(manifest.join(folder)).sort_by_file_name())
        .filter_ok(|entry| entry.path().extension().is_some_and(|extension| extension == "rs"))
        .map(|entry| {
            let path = entry.map_err(|source| Failure::Fixture { source })?.into_path();
            let text = fs_err::read_to_string(&path).map_err(|source| Failure::Read { path: path.clone(), source })?;
            Ok::<_, Failure>((path, text))
        })
        .collect::<Result<BTreeMap<_, _>, _>>()?;
    let annotations = |path: &Path, source: &str| {
        let count = source.lines().count();
        source.lines().enumerate().try_fold(Vec::<(usize, Annotation)>::new(), |mut held, (index, written)| {
            let row = index + 1;
            let Some((target, marked)) = written.match_indices("//").find_map(|(at, _)| {
                let comment = written.get(at + 2..)?;
                comment.strip_prefix('[').and_then(|bracketed| bracketed.split_once("]~")).map(|(target, after)| (Some(target), after)).or_else(|| comment.strip_prefix('~').map(|after| (None, after)))
            }) else {
                return Ok(held);
            };
            let (direction, named) = marked.split_at(marked.find(char::is_whitespace).unwrap_or(marked.len()));
            let named = named.trim();
            let (lint, wording) = named.split_at(named.find(char::is_whitespace).unwrap_or(named.len()));
            let aimed = match direction.as_bytes() {
                [] => Some(row),
                [b'|'] => held.last().filter(|(previous, _)| *previous + 1 == row).map(|(_, annotation)| annotation.line),
                [b'^', ..] if direction.bytes().all(|step| step == b'^') => row.checked_sub(direction.len()).filter(|aimed| *aimed > 0),
                [b'v', ..] if direction.bytes().all(|step| step == b'v') => Some(row + direction.len()).filter(|aimed| *aimed <= count),
                _ => None,
            };
            let aimed = aimed.filter(|_| !lint.is_empty()).ok_or_else(|| Failure::Annotation { path: path.to_path_buf(), line: row, text: written.trim().to_owned() })?;
            held.push((row, Annotation { line: aimed, target: target.map(str::to_owned), lint: lint.to_owned(), text: Some(wording.trim()).filter(|wording| !wording.is_empty()).map(str::to_owned) }));
            Ok::<_, Failure>(held)
        })
    };
    let mut expected = fixtures
        .iter()
        .map(|(path, source)| Ok::<_, Failure>((path.clone(), annotations(path, source)?.into_iter().map(|(_, annotation)| annotation).collect::<Vec<_>>())))
        .collect::<Result<BTreeMap<_, _>, _>>()?;
    let output = Command::new(env!("CARGO"))
        .args(["dylint", "--all", "-p", env!("CARGO_PKG_NAME"), "--fail-on-no-libraries", "--", "--locked", "--keep-going", "--examples", "--test", "expansions", "--message-format=json"])
        .stderr(Stdio::inherit())
        .output()
        .map_err(|source| Failure::Io { stage: Stage::Start, source })?;
    let worded = |diagnostic: &Diagnostic| {
        let helps = diagnostic.children.iter().flat_map(|part| {
            part.spans.iter().filter_map(|span| span.suggested_replacement.as_deref()).map(|replacement| format!("{}: `{replacement}`", part.message)).chain(iter::once(part.message.clone()))
        });
        iter::once(diagnostic.message.clone()).chain(helps).join("\n")
    };
    let (errors, mut found, checked) =
        Message::parse_stream(output.stdout.as_slice()).try_fold((String::new(), BTreeMap::<PathBuf, Vec<Finding>>::new(), false), |(mut errors, mut found, checked), message| {
            let message = message.map_err(|source| Failure::Io { stage: Stage::Output, source })?;
            if let Message::CompilerMessage(message) = &message
                && let Some(rendered) = &message.message.rendered
            {
                if matches!(message.message.level, DiagnosticLevel::Error | DiagnosticLevel::Ice) && message.message.code.as_ref().is_none_or(|code| code.explanation.is_some()) {
                    errors.extend([rendered.trim_end(), "\n\n"]);
                }
                if let Some(code) = &message.message.code {
                    let primaries = message.message.spans.iter().filter(|span| span.is_primary).collect::<Vec<_>>();
                    let (path, line) = primaries.first().map_or((PathBuf::new(), 0), |span| (root.join(&span.file_name), span.line_start));
                    found.entry(path).or_default().push(Finding {
                        line,
                        lint: code.code.clone(),
                        level: message.message.level,
                        target: message.target.name.clone(),
                        primaries: primaries.len(),
                        wording: worded(&message.message),
                        rendered: rendered.trim_end().to_owned(),
                    });
                }
            }
            Ok::<_, Failure>((errors, found, checked || matches!(message, Message::BuildFinished(_))))
        })?;
    (checked && errors.is_empty()).ok_or(Failure::Compile { errors, status: output.status })?;
    let paths = expected.keys().chain(found.keys()).cloned().collect::<BTreeSet<_>>();
    let meets = |annotation: &Annotation, finding: &Finding| {
        finding.line == annotation.line
            && finding.lint == annotation.lint
            && finding.primaries == 1
            && finding.level == DiagnosticLevel::Error
            && annotation.target.as_ref().is_none_or(|target| *target == finding.target)
            && annotation.text.as_ref().is_none_or(|text| finding.wording.contains(text.as_str()))
    };
    let (mut problems, met) = paths.into_iter().fold((Vec::new(), BTreeSet::new()), |(mut problems, mut met), path| {
        let mut written = expected.remove(&path).unwrap_or_default();
        written.sort_by_key(|annotation| usize::from(annotation.target.is_none()) + usize::from(annotation.text.is_none()));
        let mut open = found.remove(&path).unwrap_or_default().into_iter().map(Some).collect::<Vec<_>>();
        for annotation in written {
            match open.iter_mut().find(|slot| slot.as_ref().is_some_and(|finding| meets(&annotation, finding))).and_then(Option::take) {
                Some(finding) => _ = met.insert(finding.lint),
                None => problems.push(Problem::Missing { path: path.clone(), annotation }),
            }
        }
        problems.extend(open.into_iter().flatten().map(|finding| Problem::Unexpected { path: path.clone(), finding }));
        (problems, met)
    });
    problems.extend(declared.iter().filter(|lint| !met.contains(*lint)).map(|lint| Problem::Unfired { lint: lint.clone() }));
    problems.extend(fixtures.iter().flat_map(|(path, text)| {
        let subject = path.strip_prefix(manifest).ok().and_then(|relative| relative.iter().nth(1)).and_then(|name| Path::new(name).file_stem().unwrap_or(name).to_str().map(str::to_owned));
        text.lines()
            .filter_map(|line| line.split_once("dylint_lib = \"rasm_lints\", allow(").map(|(_, allowed)| allowed.split([',', ')']).map(str::trim).map(str::to_owned).collect::<Vec<_>>()))
            .flatten()
            .filter(|lint| Some(lint) == subject.as_ref() && declared.contains(lint))
            .map(|lint| Problem::Silenced { path: path.clone(), lint })
            .collect::<Vec<_>>()
    }));
    Ok(problems.is_empty().ok_or(Failure::Mismatch { problems })?)
}

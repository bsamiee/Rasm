use core::hint::black_box;
use std::sync::mpsc;

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERSIONS: &[Version] = &[Version("2024-11-05"), Version("2025-06-18"), Version("2026-07-28")];

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Debug, PartialEq)]
struct Version(&'static str);

#[derive(Debug)]
struct Lifecycle {
    preferred: Vec<Version>,
    legacy: Option<Version>,
}

#[derive(Debug)]
struct Catalog {
    words: Vec<&'static str>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Version {
    fn legacy(&self) -> bool { self.0 < "2026" }
}

impl Lifecycle {
    fn unplaced(&self) -> usize { VERSIONS.len().saturating_sub(self.preferred.len()).saturating_sub(usize::from(self.legacy.as_ref().is_some_and(Version::legacy))) }
}

impl Catalog {
    fn split(&self) -> (usize, Option<&'static str>) { (self.words.iter().filter(|word| word.is_empty()).count(), self.words.iter().find(|word| !word.is_empty()).copied()) }
}

fn short(word: &&&str) -> bool { word.len() < 4 }

fn lifecycle() -> Lifecycle {
    Lifecycle { preferred: VERSIONS.iter().rev().filter(|version| !version.legacy()).cloned().collect(), legacy: VERSIONS.iter().rev().find(|version| version.legacy()).cloned() }
}

fn halves(words: &[&'static str]) -> (Vec<&'static str>, Vec<&'static str>) {
    let blank = words.iter().filter(|word| word.trim().is_empty()).copied().collect::<Vec<_>>();
    let filled = words.iter().filter(|text| !text.trim().is_empty()).copied().collect::<Vec<_>>();
    (blank, filled)
}

fn named(words: &[&'static str]) -> (usize, Option<&'static str>) {
    let counted = words.iter().filter(short).count();
    (counted, words.iter().find(|word| !short(word)).copied())
}

fn other(words: &[&'static str], names: &[&'static str]) -> (usize, Option<&'static str>) { (words.iter().filter(|word| word.is_empty()).count(), names.iter().find(|word| !word.is_empty()).copied()) }

fn same(words: &[&'static str]) -> (usize, Option<&'static str>) { (words.iter().filter(|word| word.is_empty()).count(), words.iter().find(|word| word.is_empty()).copied()) }

fn branched(words: &[&'static str], counting: bool) -> usize {
    if counting { words.iter().filter(|word| word.is_empty()).count() } else { usize::from(words.iter().find(|word| !word.is_empty()).is_some()) }
}

fn probed(words: &[&'static str]) -> (Option<&'static str>, Option<&'static str>) {
    (words.iter().filter(|word| word.is_empty()).next_back().copied(), words.iter().find(|word| !word.is_empty()).copied())
}

fn drained(received: &mpsc::Receiver<u32>) -> (usize, Option<u32>) { (received.try_iter().filter(|value| *value > 3).count(), received.try_iter().find(|value| *value <= 3)) }

fn computed(numbers: fn() -> Vec<u32>) -> (usize, Option<u32>) { (numbers().iter().filter(|value| value.is_power_of_two()).count(), numbers().into_iter().find(|value| !value.is_power_of_two())) }

fn bumped(words: &mut [String]) -> (usize, usize) {
    let grown = words.iter_mut().filter(|word| word.is_empty()).map(|word| word.push('x')).count();
    (grown, words.iter_mut().filter(|word| !word.is_empty()).count())
}

fn partitioned(words: &[&'static str]) -> (Vec<&'static str>, Vec<&'static str>) { words.iter().partition(|word| word.is_empty()) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let words = ["", "port", "tls"];
    let (sender, received) = mpsc::channel();
    sender.send(4).unwrap_or_default();
    black_box((
        lifecycle().unplaced(),
        short(&&"tls"),
        Catalog { words: words.to_vec() }.split(),
        halves(&words),
        named(&words),
        other(&words, &words),
        same(&words),
        branched(&words, true),
        probed(&words),
        drained(&received),
        computed(|| vec![1, 2]),
        bumped(&mut [String::new()]),
        partitioned(&words),
    ));
}

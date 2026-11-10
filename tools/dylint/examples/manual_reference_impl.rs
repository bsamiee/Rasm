use core::any::Any;
use core::convert::Infallible;
use core::fmt;
use core::hint::black_box;
use core::ops::Deref;
use core::str::FromStr;
use std::process::ExitCode;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Default)]
struct Names(Vec<String>);

impl Names {
    const fn len(&self) -> usize { self.0.len() }

    const fn is_empty(&self) -> bool { self.0.is_empty() }
}

impl AsRef<[String]> for Names {
    fn as_ref(&self) -> &[String] { &self.0 }
}

impl From<Vec<String>> for Names {
    fn from(value: Vec<String>) -> Self { Self(value) }
}

#[derive(Debug)]
struct Label(String);

impl fmt::Display for Label {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { fmt::Display::fmt(&self.0, f) }
}

impl FromStr for Label {
    type Err = Infallible;

    fn from_str(s: &str) -> Result<Self, Self::Err> { s.parse().map(Self) }
}

#[derive(Debug)]
#[repr(transparent)]
struct Count(u64);

impl Count {
    const fn get(&self) -> u64 { self.0 }

    const fn checked_add(&self, other: u64) -> Option<u64> { self.0.checked_add(other) }
}

#[derive(Debug)]
struct Scores(Vec<u32>);

impl Scores {
    fn get(&self, index: usize) -> Option<&u32> { self.0.get(index.checked_add(1)?) }
}

#[derive(Debug, Default)]
struct Queue(Vec<u32>);

impl Queue {
    fn push(&mut self, value: u32) { self.0.push(value); }

    const fn len(&self) -> usize { self.0.len() }
}

#[derive(Debug, Default)]
pub struct Public(Vec<u32>);

impl Public {
    #[must_use]
    pub const fn len(&self) -> usize { self.0.len() }
}

#[derive(Debug)]
struct Lines(Vec<String>);

impl Lines {
    const fn count(&self) -> usize { self.0.len() }
}

struct Tagged(String);

impl fmt::Debug for Tagged {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { fmt::Display::fmt(&self.0, f) }
}

#[derive(Debug)]
struct Title(String);

impl AsRef<String> for Title {
    fn as_ref(&self) -> &String { &self.0 }
}

impl AsRef<str> for Title {
    fn as_ref(&self) -> &str { self.0.as_ref() }
}

impl AsMut<String> for Title {
    fn as_mut(&mut self) -> &mut String { &mut self.0 }
}

#[derive(Debug, derive_more::AsRef)]
struct Derived(String);

#[derive(Debug)]
struct Trimmed(String);

impl AsRef<str> for Trimmed {
    fn as_ref(&self) -> &str { self.0.trim() }
}

#[derive(Debug)]
struct Hex(Vec<u8>);

impl fmt::Display for Hex {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { self.0.iter().try_for_each(|byte| write!(f, "{byte:02x}")) }
}

#[derive(Debug)]
struct Reordered(Vec<usize>);

impl Reordered {
    fn insert(&mut self, index: usize, value: usize) { self.0.insert(value, index); }
}

#[derive(Debug)]
struct Repeated(Vec<usize>);

impl Repeated {
    fn insert(&mut self, index: usize, _value: usize) { self.0.insert(index, index); }
}

#[derive(Debug)]
struct Ignored(Vec<usize>);

impl Ignored {
    fn truncate(&mut self, length: usize, _extra: usize) { self.0.truncate(length); }
}

#[derive(Debug)]
struct Content {
    raw: String,
    offset: usize,
}

impl Content {
    fn as_ref(&self) -> &str { &self.raw[self.offset..] }
}

impl AsRef<str> for Content {
    fn as_ref(&self) -> &str { &self.raw }
}

impl Deref for Content {
    type Target = str;

    fn deref(&self) -> &Self::Target { &self.raw[self.offset..] }
}

#[derive(Debug)]
struct Viewed(Content);

impl AsRef<str> for Viewed {
    fn as_ref(&self) -> &str { self.0.as_ref() }
}

#[derive(Debug)]
struct Coerced(Content);

impl AsRef<str> for Coerced {
    fn as_ref(&self) -> &str { &self.0 }
}

#[derive(Debug)]
struct DerefOnly(String);

impl Deref for DerefOnly {
    type Target = str;

    fn deref(&self) -> &Self::Target { self.0.as_str() }
}

#[derive(Debug)]
struct Indirect(DerefOnly);

impl AsRef<str> for Indirect {
    fn as_ref(&self) -> &str { self.0.as_ref() }
}

#[derive(Debug)]
struct Callsite;

impl AsRef<str> for Callsite {
    #[track_caller]
    fn as_ref(&self) -> &str { core::panic::Location::caller().file() }
}

#[derive(Debug)]
struct Tracked(Callsite);

impl AsRef<str> for Tracked {
    #[track_caller]
    fn as_ref(&self) -> &str { self.0.as_ref() }
}

#[derive(Debug)]
struct Conditional(String);

#[cfg(unix)]
impl AsRef<str> for Conditional {
    fn as_ref(&self) -> &str { self.0.as_ref() }
}

#[derive(Debug)]
struct Constrained<T>(T);

impl<T: Copy> AsRef<T> for Constrained<T> {
    fn as_ref(&self) -> &T { &self.0 }
}

#[derive(Debug)]
struct CheckedMap(String);

impl CheckedMap {
    fn try_new(value: String) -> Option<Self> { Some(value).filter(|text| !text.is_empty()).map(Self) }

    const fn len(&self) -> usize { self.0.len() }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    #[cfg(unix)]
    black_box(Conditional(String::from("conditional")).as_ref().len());
    let names = Names::from(vec![Label::from_str("name").map_or_else(|never| match never {}, |label| label.to_string())]);
    let count = Count(3);
    let mut reordered = Reordered(vec![1]);
    let mut repeated = Repeated(vec![2]);
    let mut ignored = Ignored(vec![3]);
    reordered.insert(0, 1);
    repeated.insert(0, 1);
    ignored.truncate(0, 1);
    let shown = format!("{:?}{}{}", Tagged(String::from("tag")), Hex(vec![1]), Lines(names.as_ref().to_vec()).count());
    black_box(&(
        Scores(vec![1, 2]).get(0).copied(),
        count.checked_add(count.get()),
        black_box(&count as &dyn Any).downcast_ref::<Count>().map(Count::get),
        shown,
        CheckedMap::try_new(String::from("checked")).map(|value| value.len()),
        (reordered, repeated, ignored, Viewed(Content { raw: String::from("abc"), offset: 1 }).as_ref().len()),
        (Coerced(Content { raw: String::from("abc"), offset: 1 }).as_ref().len(), AsRef::<u32>::as_ref(&Constrained(7))),
        Indirect(DerefOnly(String::from("indirect"))).as_ref().len(),
        Tracked(Callsite).as_ref().len(),
        names.len() == 1,
        {
            let mut queue = Queue::default();
            queue.push(1);
            [queue.len(), Public::default().len()]
        },
        (format!("{:?}", Tagged(String::new())), Hex(vec![2]).to_string()),
        {
            let mut title = Title(String::from("title"));
            AsMut::<String>::as_mut(&mut title).push('s');
            (AsRef::<String>::as_ref(&title).len(), AsRef::<str>::as_ref(&title).len(), AsRef::<String>::as_ref(&Derived(String::new())).len(), Trimmed(String::new()).as_ref().len())
        },
    ));
    ExitCode::from(u8::from(names.is_empty()))
}

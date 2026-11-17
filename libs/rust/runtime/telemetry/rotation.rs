use core::cmp::Reverse;
use core::num::NonZero;
use std::fs::Metadata;
use std::io::{self, Write};
#[cfg(unix)]
use std::os::unix::fs::MetadataExt as _;
#[cfg(windows)]
use std::os::windows::fs::MetadataExt as _;
use std::process;

use camino::{Utf8Path, Utf8PathBuf};
use fs_err::{File, OpenOptions};
use itertools::Itertools as _;
use jiff::{SignedDuration, Timestamp};

// --- [TYPES] ---------------------------------------------------------------------------

type Identity = cfg_select! {
    unix => { (u64, u64) }
    windows => { (Option<u32>, Option<u64>) }
};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(const_fn, validate(predicate = SignedDuration::is_positive), derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Deref))]
pub struct Age(SignedDuration);

#[derive(Clone, Copy, Debug, Default, PartialEq, Eq)]
pub struct Rotation {
    pub bytes: NonZero<u64> = NonZero::new(8 << 20).unwrap(),
    pub age: Age = Age::try_new(SignedDuration::from_hours(24)).ok().unwrap(),
    pub files: NonZero<usize> = NonZero::new(7).unwrap(),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug)]
pub struct RotatingFile {
    directory: Utf8PathBuf,
    program: String,
    path: Utf8PathBuf,
    rotation: Rotation,
    identity: Identity,
    file: File,
    born: Timestamp,
}

impl RotatingFile {
    pub fn open(directory: Utf8PathBuf, program: String, rotation: Rotation) -> io::Result<Self> {
        fs_err::create_dir_all(&directory)?;
        let path = directory.join(format!("{program}.{}.log", process::id()));
        let (identity, file) = Self::opened(&path)?;
        Ok(Self { directory, program, path, rotation, identity, file, born: Timestamp::now() })
    }

    fn opened(path: &Utf8Path) -> io::Result<(Identity, File)> {
        let file = OpenOptions::new().create(true).append(true).open(path)?;
        Ok((identity(&file.metadata()?), file))
    }

    fn rotated(&mut self, now: Timestamp) -> io::Result<()> {
        if self.file.metadata()?.len() > 0 {
            fs_err::rename(&self.path, self.path.with_extension(format!("{}.log", now.as_microsecond())))?;
        }
        (self.identity, self.file, self.born) = Self::opened(&self.path).map(|(identity, file)| (identity, file, now))?;
        let digits = |part: &str| !part.is_empty() && part.bytes().all(|digit| digit.is_ascii_digit());
        let owned = |name: &str| {
            let numbered = name.strip_prefix(self.program.as_str()).and_then(|rest| rest.strip_prefix('.')?.strip_suffix(".log"));
            numbered.is_some_and(|numbers| numbers.split('.').count() <= 2 && numbers.split('.').all(digits))
        };
        let absent = |error: io::Error| (error.kind() == io::ErrorKind::NotFound).then_some(None).ok_or(error);
        fs_err::read_dir(&self.directory)?
            .map(|entry| {
                let entry = entry?;
                if entry.file_name().to_str().is_some_and(owned) { entry.metadata().and_then(|held| held.modified()).map(|modified| Some((modified, entry.path()))) } else { Ok(None) }
            })
            .filter_map(|held| held.or_else(absent).transpose())
            .collect::<io::Result<Vec<_>>>()?
            .into_iter()
            .sorted_unstable_by_key(|(modified, _)| Reverse(*modified))
            .skip(self.rotation.files.get())
            .try_for_each(|(_, path)| fs_err::remove_file(path).or_else(|error| (error.kind() == io::ErrorKind::NotFound).ok_or(error)))
    }
}

impl Write for RotatingFile {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        let now = Timestamp::now();
        let held = fs_err::metadata(&self.path).map(Some).or_else(|error| (error.kind() == io::ErrorKind::NotFound).then_some(None).ok_or(error))?;
        let length = match held {
            Some(held) if identity(&held) == self.identity => held.len(),
            Some(_) | None => {
                (self.identity, self.file, self.born) = Self::opened(&self.path).map(|(identity, file)| (identity, file, now))?;
                self.file.metadata()?.len()
            }
        };
        if length.saturating_add(buf.len().saturating_cast()) > self.rotation.bytes.get() || now.duration_since(self.born) >= *self.rotation.age {
            self.rotated(now)?;
        }
        self.file.write_all(buf).map(|()| buf.len())
    }

    fn flush(&mut self) -> io::Result<()> { self.file.flush() }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn identity(file: &Metadata) -> Identity {
    cfg_select! {
        unix => (file.dev(), file.ino()),
        windows => (file.volume_serial_number(), file.file_index()),
    }
}

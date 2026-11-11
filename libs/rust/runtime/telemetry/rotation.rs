use core::cmp::Reverse;
use core::num::NonZero;
use std::fs::Metadata;
use std::io::{self, Write};
#[cfg(unix)]
use std::os::unix::fs::MetadataExt as _;
#[cfg(windows)]
use std::os::windows::fs::MetadataExt as _;
use std::time::SystemTime;

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
    path: Utf8PathBuf,
    rotation: Rotation,
    identity: Identity,
    file: File,
    born: Timestamp,
}

impl RotatingFile {
    pub fn open(path: Utf8PathBuf, rotation: Rotation) -> io::Result<Self> {
        path.parent().map_or(Ok(()), fs_err::create_dir_all)?;
        let (identity, file, born) = Self::opened(&path)?;
        Ok(Self { path, rotation, identity, file, born })
    }

    fn lock(path: &Utf8Path) -> io::Result<File> { OpenOptions::new().create(true).append(true).open(format!("{path}.lock")) }

    fn opened(path: &Utf8Path) -> io::Result<(Identity, File, Timestamp)> {
        let file = OpenOptions::new().create(true).append(true).open(path)?;
        let (held, stamped) = (file.metadata()?, Self::lock(path)?.metadata()?.modified()?);
        Ok((identity(&held), file, Timestamp::try_from(stamped).map_err(io::Error::other)?))
    }

    fn rotated(&mut self, now: Timestamp) -> io::Result<()> {
        let (Some(directory), Some(stem), extension) = (self.path.parent(), self.path.file_stem(), self.path.extension()) else { return Err(io::ErrorKind::InvalidInput.into()) };
        let archived = |stamp: &str| self.path.with_extension(extension.map_or_else(|| stamp.to_owned(), |extension| format!("{stamp}.{extension}")));
        let stamped = |name: &str| {
            let rotated = match extension {
                Some(extension) => name.strip_suffix(extension)?.strip_suffix('.')?,
                None => name,
            };
            rotated.strip_prefix(stem)?.strip_prefix('.').filter(|stamp| !stamp.is_empty() && stamp.bytes().all(|digit| digit.is_ascii_digit())).map(str::to_owned)
        };
        let lock = Self::lock(&self.path)?;
        lock.lock()?;
        if Timestamp::try_from(lock.metadata()?.modified()?).map_err(io::Error::other)? == self.born {
            if self.file.metadata()?.len() > 0 {
                fs_err::rename(&self.path, archived(&now.as_microsecond().to_string()))?;
            }
            lock.set_modified(SystemTime::from(now))?;
            fs_err::read_dir(directory)?
                .map(|entry| Ok(entry?.file_name().to_str().and_then(stamped)))
                .filter_map(Result::transpose)
                .collect::<io::Result<Vec<_>>>()?
                .into_iter()
                .sorted_unstable_by_key(|stamp| Reverse((stamp.len(), stamp.clone())))
                .skip(self.rotation.files.get())
                .try_for_each(|stamp| fs_err::remove_file(archived(&stamp)).or_else(|error| (error.kind() == io::ErrorKind::NotFound).ok_or(error)))?;
        }
        (self.identity, self.file, self.born) = Self::opened(&self.path)?;
        Ok(())
    }
}

impl Write for RotatingFile {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        let now = Timestamp::now();
        let held = fs_err::metadata(&self.path).map(Some).or_else(|error| (error.kind() == io::ErrorKind::NotFound).then_some(None).ok_or(error))?;
        let length = match held {
            Some(held) if identity(&held) == self.identity => held.len(),
            Some(_) | None => {
                (self.identity, self.file, self.born) = Self::opened(&self.path)?;
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

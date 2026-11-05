use core::convert::identity;
use std::{ffi::OsStr, io::ErrorKind};

use clippy_utils::{diagnostics::span_lint, tokenize_with_text};
use itertools::Itertools as _;
use rustc_data_structures::fx::FxHashSet;
use rustc_lexer::TokenKind;
use rustc_lint::{LateContext, LateLintPass};
use rustc_span::{FileName, Span, kw};

// --- [SERVICES] ------------------------------------------------------------------------

lint!(ORPHAN_MODULE: "source files and `mod.rs` folders in a module's folder that no module declares");

// --- [COMPOSITION] ---------------------------------------------------------------------

impl LateLintPass<'_> for Pass {
    fn check_crate_post(&mut self, cx: &LateContext<'_>) {
        const EXTENSION: &str = "rs";
        const NESTED: &str = "mod.rs";
        const ROOTS: [&str; 4] = ["lib", "main", "mod", "build"];
        if cx.tcx.sess.is_test_crate() {
            return;
        }
        let rooted = |stem: Option<&str>| stem.is_some_and(|stem| ROOTS.contains(&stem));
        let files = cx.tcx.sess.source_map().files();
        let sources = files
            .iter()
            .filter(|file| !file.is_imported())
            .filter_map(|file| match &file.name {
                FileName::Real(name) => name.local_path().filter(|path| path.extension().is_some_and(|extension| extension == EXTENSION)).map(|path| (file, path)),
                _ => None,
            })
            .collect_vec();
        let loaded = sources.iter().map(|&(_, path)| path).collect::<FxHashSet<_>>();
        for &(file, path) in &sources {
            let declared = file
                .src
                .iter()
                .flat_map(|source| tokenize_with_text(source))
                .filter(|(kind, ..)| !matches!(kind, TokenKind::Whitespace | TokenKind::LineComment { .. } | TokenKind::BlockComment { .. }))
                .tuple_windows()
                .filter_map(|((_, word, _), (_, name, _))| (word == kw::Mod.as_str()).then_some(name))
                .collect::<FxHashSet<_>>();
            let folder = if rooted(path.file_stem().and_then(OsStr::to_str)) { path.with_file_name("") } else { path.with_extension("") };
            let report = |message: String| span_lint(cx, ORPHAN_MODULE, Span::with_root_ctxt(file.start_pos, file.start_pos), message);
            match fs_err::read_dir(folder).and_then(|entries| entries.map(|entry| entry.map(|entry| entry.path())).collect::<Result<Vec<_>, _>>()) {
                Ok(entries) => entries
                    .iter()
                    .sorted()
                    .filter_map(|entry| {
                        let stem = entry.file_stem().and_then(OsStr::to_str);
                        let module = if entry.extension().is_some_and(|extension| extension == EXTENSION) {
                            Some(entry.clone())
                        } else {
                            Some(entry.join(NESTED)).filter(|nested| fs_err::exists(nested).is_ok_and(identity))
                        }?;
                        (!rooted(stem) && !stem.is_some_and(|stem| declared.contains(stem)) && !loaded.contains(module.as_path())).then_some(module)
                    })
                    .for_each(|module| report(format!("`{}` sits in this module's folder with no module declaring it, declare it here or delete it", module.display()))),
                Err(error) if error.kind() == ErrorKind::NotFound => {}
                Err(error) => report(error.to_string()),
            }
        }
    }
}

use core::iter;
use std::path::PathBuf;

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> Result<(), protox::Error> {
    let path = build_rs::input::cargo_manifest_path();
    let manifest = fs_err::read_to_string(&path)?.parse::<toml::Table>().map_err(protox::Error::new)?;
    let rows = |key: &str| manifest["package"]["metadata"]["protobuf"][key].clone().try_into::<Vec<PathBuf>>().map_err(protox::Error::new);
    let (files, includes) = (rows("files")?, rows("includes")?);
    iter::once(&path).chain(&includes).for_each(build_rs::output::rerun_if_changed);
    let mut compiler = protox::Compiler::new(includes)?;
    compiler.include_source_info(true).include_imports(true).open_files(files)?;
    fs_err::write(build_rs::input::out_dir().join(build_rs::input::cargo_pkg_name()).with_extension("bin"), compiler.encode_file_descriptor_set())?;
    Ok(tonic_prost_build::compile_fds(compiler.file_descriptor_set())?)
}

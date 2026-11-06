# [PYTHON]

uv resolves projects, workspace members, and dependency groups through native metadata.

## [01]-[RUNTIMES]

- Root Python checkers target interpreter versions the toolchain manager resolves
- Embedded interpreter requirements sit in each owning project's `requires-python`
- Embedded runtime compatibility covers executed modules and transitive imports
- Workspace resolution intersects every member's `requires-python`, incompatible runtimes use independent projects
- `--package` selects workspace execution without isolating resolution or interpreter requirements
- `requires-python` covers package syntax, APIs, and imported dependency requirements

## [02]-[DEPENDENCIES]

- `[project] dependencies` holds runtime requirements, dependency groups hold development requirements outside published metadata
- `[tool.uv.sources]` selects workspace members or independent path projects
- Referenced virtual projects declare `tool.uv.package=false`, installed path projects declare `[build-system]`
- Dependency groups can narrow interpreter ranges separately from project requirements
- Published packages declare every directly imported dependency, type-only imports included
- Version bounds express package metadata gaps or native library compatibility
- Environment markers select platform and interpreter requirements
- `no-build-package` refuses dependencies requiring a source build
- Rust extension builds derive compiler selection from rustup and source toolchain metadata

## [03]-[LOCK]

- Workspace members share `uv.lock`, independent path sources participate in each requesting project's lock
- PEP 723 scripts use independent resolution
- Backend settings, extra build requirements, and package build variables select cached wheel variants
- Inherited process variables and linked library contents alone do not invalidate cached wheels
- `uv cache clean <package>` then `uv sync --reinstall-package <package>` rebuilds an unkeyed native ABI change
- Native environment constraints and consuming build settings change together when a library's soname series changes
- `pixi run --locked uv sync` builds against one native prefix without installing a second Python interpreter
- Native libraries stay in Pixi when a consumer has no wheel for a declared platform or opens a library by name
- Keep versioned soname series in Pixi requirements; use `*` for libraries opened by name or linked without a version
- `LDFLAGS` supplies C and C++ link paths, `LIBRARY_PATH` also reaches Rust links that record no `LC_RPATH`
- Keep uv's cache outside the checkout, where Cargo can build Rust distributions outside the root workspace
- CI syncs the `dev` group without Pixi; dependency markers keep Blender-only packages on its interpreter
- `exclude-dependencies` selects one provider when distributions compete for one installed module

## [04]-[ENVIRONMENT]

- Interpreter discovery uses resolver output under the same environment as execution
- `uv run` and `uv sync` use the project environment, `--active` selects `VIRTUAL_ENV`
- Workspace tool settings come from root configuration, independent projects use native configuration discovery
- Workspace members share one environment and installed dependencies
- Editable path sources expose Python edits, native extension changes require rebuilding
- Python import visibility follows installed files, `sys.path`, and import hooks
- Native builds can reuse objects and build-system caches from earlier source builds
- Modules named in `sys.stdlib_module_names` shadow standard libraries when earlier on the import path
- The Pixi `python` task supplies library lookup variables to its child; macOS strips inherited `DYLD_*` during activation

## [05]-[CHECKERS]

- Tools supporting per-file runtimes run once, tools accepting one version per process split by project `requires-python`
- Checker cache inputs include implementation, imported source, settings, resolved dependencies, and the resolver-selected interpreter
- Ruff, mypy, and pytest read cache paths from `pyproject.toml`, commands bypassing it use their own defaults
- `mypy` skips dot-prefixed children during directory discovery, hidden source trees take explicit inputs
- Ruff discovers nested interpreter requirements before an extended `target-version`
- Compiler option snapshots decide interface cache compatibility
- Parser threads and type-checker processes multiply resource use under concurrent invocations
- Refurb reads source arguments and default files through mypy's option parser
- `[tool.refurb] enable_all` enables checks added by installed releases
- `[tool.refurb] load` imports configured modules and package descendants, entry modules guard execution
- Check modules expose one `Error...` subclass and one `check`, support modules hold shared analysis
- Dependency checks use importing projects' manifests and resolved environments to classify declared, development, and transitive requirements
- Host-provided modules and deliberately unimported distributions take narrowly scoped dependency-checker exceptions
- Build graphs join native project ownership with static runtime and type-only imports

## [06]-[HOST_PACKAGING]

- Wheel contents and editable import roots derive from build-backend declarations
- Exported dependency closures supply host bundles under embedded interpreter markers and wheel tags
- Bundled first-party modules use relative imports within the relocatable host namespace
- Host-provided distributions stay outside the bundled closure
- Manifest versions derive from product release metadata during staging
- Installation uses native package managers on the intended host executable and user profile

## [07]-[TYPING]

- Published host typings supply runtime declarations before local exceptions
- Incomplete providers leave native runtime values unchanged
- Provider defects take diagnostic-specific ignores at affected lines, repeated defects can use one file directive
- Root configuration holds no local exceptions
- Resolution, build, and import leave each dependency's published interpreter support unchanged

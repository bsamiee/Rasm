export const meta = {
  name: 'languages',
  description: 'Language rows deepen and tune read: skills, sources, release, companion, catalog, and checkers in ownership order',
  whenToUse: "deepen and tune read the rows through workflow('languages'), which runs no agent",
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const owned = (row) => ({
  ...row,
  pathspec: [...row.pathspec, ...row.outlines.flatMap((key) => [`':(glob)tools/ast-grep/*/${key}/**'`, `'tools/ast-grep/outline/${key}.yml'`])],
  owners: [...row.owners, {
    name: 'ast-grep',
    paths: row.outlines.flatMap((key) => [`tools/ast-grep/rules/${key}`, `tools/ast-grep/utils/${key}`, `tools/ast-grep/outline/${key}.yml`]),
    decides: 'a form syntax decides alone, as a rule, a util its rules call or an outline row',
    skills: ['use-ast-grep'],
  }],
})

// --- [COMPOSITION] ---------------------------------------------------------------------

return {
  hosts: [
    ['creative-cloud', ['uxp-coding'], ['typescript']],
    ['blender', ['blender-coding', 'blender-coding-extensions', 'blender-coding-interface', 'blender-coding-nodes', 'blender-coding-render', 'use-blender'], ['python']],
    ['rhino', ['use-rhino'], ['dotnet', 'typescript']],
  ],
  languages: {
    typescript: owned({
      libs: 'libs/typescript',
      skills: ['typescript-coding', 'typescript-coding-effect'],
      families: [],
      companion: '.claude/workflows/TYPESCRIPT.md',
      catalog: 'pnpm-workspace.yaml',
      sources: 'node_modules/.pnpm',
      release: 'tsconfig.base.json `lib`, and each package at the release pnpm-workspace.yaml catalogs',
      outlines: ['typescript', 'sql', 'pgsql', 'plpgsql'],
      metadata: [],
      pathspec: ["'*.ts'", "'*.tsx'", "'*.mts'", "'*.cts'", "'*.js'", "'*.jsx'", "'*.mjs'", "'*.cjs'", "'*.jxa'", "'*.jsxinc'", "'*package.json'", "'*tsconfig*.json'", "'biome.json'", "'pnpm-workspace.yaml'"],
      unit: 'folder holding the nearest package.json',
      owners: [
        { name: 'tsc', paths: ['tsconfig.base.json'], decides: 'a `compilerOptions` row or an @effect/language-service diagnostic', skills: [] },
        { name: 'biome', paths: ['biome.json'], decides: 'a Biome rule or option row', skills: [] },
      ],
    }),
    python: owned({
      libs: 'libs/python',
      skills: ['python-coding', 'python-document'],
      families: ['python-coding-cyclopts', 'python-coding-expression', 'python-coding-msgspec', 'python-coding-pydantic', 'python-coding-telemetry'],
      companion: null,
      catalog: 'pyproject.toml',
      sources: '.venv/lib/python*/site-packages',
      release: '`requires-python` of the member pyproject.toml holding the file, a Ruff `per-file-target-version` row, else the first mise Python row',
      outlines: ['python', 'toml', 'sql'],
      metadata: [],
      pathspec: ["'*.py'", "'*.py3'", "'*.pyi'", "'*.bzl'", "'*.bazel'", "'*pyproject.toml'"],
      unit: 'folder holding the nearest pyproject.toml a uv root lists (`uv workspace list --paths`)',
      owners: [
        { name: 'ruff', paths: ['pyproject.toml'], decides: 'a Ruff rule or option row of a [tool.ruff*] table in the root or a member pyproject.toml', skills: [] },
        { name: 'ty', paths: ['pyproject.toml'], decides: 'a ty rule row of a [tool.ty*] table in the root or a member pyproject.toml', skills: [] },
        { name: 'mypy', paths: ['pyproject.toml'], decides: 'a mypy option or error code row of a [tool.mypy] table', skills: [] },
        { name: 'refurb', paths: ['tools/refurb'], decides: 'a form that needs types mypy resolves, as a check module or a [tool.refurb] row', skills: ['python-coding'] },
      ],
      blender: {
        roots: ['libs/python/blender', 'apps/blender'],
        sources: 'libs/python/blender/.venv/lib/python*/site-packages',
        release: "Blender's bundled Python 3.13",
      },
    }),
    rust: owned({
      libs: 'libs/rust',
      skills: ['rust-coding'],
      families: [],
      companion: '.claude/workflows/RUST.md',
      catalog: 'Cargo.toml',
      sources: '~/.cargo/registry/src/index.crates.io-*',
      release: 'Cargo.toml `edition` and `rust-version`, rust-toolchain.toml `channel`, and each crate at the release Cargo.lock pins',
      outlines: ['rust', 'toml', 'sql', 'pgsql'],
      metadata: [],
      pathspec: ["'*.rs'", "'*Cargo.toml'", "'.config/clippy.toml'", "'.config/rustfmt.toml'", "'rust-toolchain.toml'"],
      unit: 'folder holding the nearest Cargo.toml',
      owners: [
        { name: 'clippy', paths: ['Cargo.toml', '.config/clippy.toml'], decides: 'a rustc or Clippy lint row of `[workspace.lints.*]` or a clippy.toml option', skills: [] },
        { name: 'dylint', paths: ['tools/dylint'], decides: 'a form that needs types and resolved paths rustc holds, as a lint', skills: ['rust-coding'] },
      ],
    }),
    swift: owned({
      libs: 'libs/swift',
      skills: ['swift-coding'],
      families: [],
      companion: null,
      catalog: 'Package.swift',
      sources: '.build/checkouts',
      release: 'Package.swift `swift-tools-version` and `platforms`, and each package at the release Package.resolved pins',
      outlines: ['swift', 'sql'],
      metadata: [],
      pathspec: ["'*.swift'", "'Package.swift'", "'.config/swift-format.json'", "'.config/swiftlint.yml'", "'Xcode.xcconfig'"],
      unit: 'Package.swift target folder holding the file',
      owners: [
        { name: 'swift-format', paths: ['.config/swift-format.json'], decides: 'a swift-format rule row', skills: [] },
        { name: 'swiftlint', paths: ['.config/swiftlint.yml'], decides: 'a SwiftLint rule or option row', skills: [] },
      ],
    }),
    dotnet: owned({
      libs: 'libs/dotnet',
      skills: ['dotnet-coding', 'dotnet-roslyn-codelens'],
      families: ['dotnet-coding-languageext', 'dotnet-coding-mapperly', 'dotnet-coding-memory', 'dotnet-coding-thinktecture', 'dotnet-coding-ui', 'dotnet-msbuild-antipatterns', 'dotnet-msbuild-diagnostics', 'dotnet-msbuild-evaluation', 'dotnet-msbuild-execution', 'dotnet-msbuild-packaging'],
      companion: null,
      catalog: 'Directory.Packages.props',
      sources: '~/.nuget/packages',
      release: 'global.json SDK, Directory.Build.props `TargetFramework` and `LangVersion`, and each package at the release Directory.Packages.props pins',
      outlines: ['csharp', 'xml', 'sql', 'pgsql'],
      metadata: [],
      pathspec: ["'*.cs'", "'*.csproj'", "'*.props'", "'*.targets'", "'*.slnx'", "'NuGet.config'", "'.editorconfig'", "'global.json'", "'apps/roslyn/*'"],
      unit: 'folder holding the nearest .csproj',
      owners: [
        { name: 'analyzers', paths: ['.editorconfig', '.config/.globalconfig'], decides: 'an analyzer severity or option row', skills: [] },
        { name: 'Rasm.Analyzers', paths: ['apps/roslyn/Rasm.Analyzers', 'apps/roslyn/Rasm.Analyzers.CodeFixes', 'apps/roslyn/Rasm.Rhino.Analyzers'], decides: 'a form that needs symbols Roslyn binds, as an analyzer with its code fix', skills: ['dotnet-coding', 'dotnet-roslyn-codelens'] },
      ],
    }),
  },
}

# [TYPESCRIPT]

## [01]-[WORKSPACE]

- Workspace manifest owns the package globs and the catalog
- Every version sits in the catalog
- Every package names dependencies as `catalog:`
- Versions in a package manifest are pins, move them to the catalog
- Peer ranges the catalog crosses resolve through an `overrides` row `<consumer>><peer>: catalog:` scoped to the one consumer
- `peerDependencyRules.allowedVersions` rows restating a version copy the catalog, replace them with the override row
- Dependencies with no import in the tree leave the catalog and every manifest
- Upgrade mover runs once over the catalog at the newest-publish target
- Per-package tag lists on the upgrade command are filters, delete them

## [02]-[ROOT_MANIFEST]

- Root manifest holds development dependencies and root targets
- Root targets are one tool call
- Targets with a command that copies, links, or resolves a path into a package are wrappers, delete them on sight
- Tools read the files a package publishes from the package
- Copies of package files in the tree are tracked once and moved by hand
- `node -p` or `require.resolve` inside a command computes a path the tool resolves itself, delete the target holding it
- Fields a tool reads (`browserslist`, `engines`) exist when a tool in the tree reads them
- Workspace packages are a manifest beside a compiler project file, both at the package root
- Packages needing a patch take `pnpm patch` and a `patchedDependencies` row
- `postinstall` scripts patching a package are script hosts, replace them with `pnpm patch`

## [03]-[COMPILER]

- One base configuration holds the compiler options every project shares
- One project file per package extends the base and adds the package output directory and type roots
- Root project file covers configuration, tooling, and infrastructure sources at the root
- Third configuration kinds at the root (`tsconfig.<variant>.json`) signal options placed outside the base or a project, move them there

## [04]-[CONFIGURATION]

- Each project's runner configuration imports the root factory
- Root factory holds the policy
- Lint configuration holds the rule set and path overrides
- Each override names the file kind it relaxes

# [DOTNET]

## [01]-[BUILD_FILES]

- `Directory.Build.props` classifies projects by tree position
- `Directory.Build.props` holds the defaults every project imports
- `Directory.Build.targets` holds items, host references, and policy targets that read the classification
- Properties a project reads before its body sit in `.props`
- Values derived from an SDK property sit in `.targets`
- SDK imports the nearest file of each name walking up
- Subtree files import the root through `GetPathOfFileAbove` or stop the chain
- Project files hold what differs from the root: references, a description, a host token
- `Exec` tasks in a directory file signal a tool call the graph owns, make the command a graph target

## [02]-[PACKAGES]

- Central package versions hold every row, one row per package id
- Catalog project references every central row, upgrade mover moves rows no other project references yet
- Versions read from another file by regex at evaluation signal a version owned outside the manifest, move the version to the manifest
- Package metadata blocks, versioning packages, and release group properties join with the first library release

## [03]-[SDK]

- `global.json` pins the SDK version
- Tool manager reads `global.json` as the idiomatic version file
- One SDK owner per machine
- Second SDK on the path ahead of the owner signals a machine fact, correct it outside the tree

## [04]-[FEEDS]

- Restore reads the registry, one source with one package folder
- Local feeds mapped into the restore path make every build depend on a packaging chain, keep the chain on its own workflow
- Bindings needing a packaged native binary arrive with the packaging workflow
- Consumers reference the published package

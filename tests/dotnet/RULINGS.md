# [DOTNET_TESTS_RULINGS]

`tests/dotnet` rulings settle .NET-tree decisions.

## [01]-[PACKAGES]

- (none)

## [02]-[SHAPE]

- (none)

## [03]-[COLLAPSE]

- (none)

## [04]-[STRUCTURE]

- Results pin to module-adjacent `TestResults` via generated per-assembly `testconfig.json` — launch cwd never picks where TRX, dumps, or logs land.
- MTP relocates a live diagnostic log once the result dir differs, disposing its writer mid-hold — the pin spells the bootstrap folder verbatim.

## [05]-[PROCESS]

- Diagnostics open before any config loads, at the cwd `dotnet test` exports — pass an artifacts-root output dir or run from the output folder.

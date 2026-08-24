# [TS_CONTRACTS_RULINGS]

`typescript/contracts` rulings settle the generated TypeScript SDK package.

## [01]-[PACKAGES]

- `@bufbuild/protoc-gen-es` moves with the direct `@bufbuild/protobuf` dependency because emitted modules bind its generated-code runtime.
- `CloudEventsAvro` adds no contract dependency or export row because it is a readonly JSON value resolved by the existing wildcard.
- `@rasm/contracts` publishes independently because contract consumers must not install the private `@rasm/*` application estate.
- `@rasm/core` depends on `@rasm/contracts` through `workspace:*`; no compatibility subpath or second export owner remains.

## [02]-[SHAPE]

- One `./*` export owns every module specifier — the workspace resolves `gen/*.ts` and `publishConfig` swaps the tarball to `dist`.
- Workspace targets stay bare specifiers — a condition serving `gen` and `dist` at once needs a custom condition every consumer tsconfig declares.
- `valid_types=protovalidate_required` emits `<Name>Valid` and binds it to each `GenMessage` descriptor.
- Publisher descriptors remain direct module inputs, so `CloudEventSchema` stays outside estate registries.
- `CloudEventsAvro` is generated from the exact frozen publisher bytes; consumers compile that value instead of transcribing the schema.

## [03]-[COLLAPSE]

(none)

## [04]-[STRUCTURE]

- Clean TypeScript project build emits ESM JavaScript and declarations; raw `gen/**` never enters the tarball.
- `gen/**` stays under the clean generation sweep and outside source mutation and coverage roots.
- `dist/**` stays under the package build sweep and root build-output exclusion; TypeScript build state routes through `.cache/`.
- Assay restores manifest-distributed publisher assets after Buf's clean sweep, preserving one generation entrypoint.
- Catalogue roster markers contain gate-emitted descriptor data; generator grammar remains the hand-maintained correspondence.
- `tsconfig.build.json` restates `allowImportingTsExtensions` and `rootDir` because the emitting build inverts both root settings.
- `composite` buys the `--build` up-to-date check alone; no project reference reaches this package, since a reference forbids the root's `noEmit`.
- `[02]-[STRATA]` carries keyless code-span rows and no graph — an admitted import root holds no rank, so a drawn stratum restates proto imports.
- Router cards index the catalogue and the emission root because generation authors every module; no design page exists for a card to reach.
- Seam kinds stay the corpus-closed vocabulary, so the publisher-asset crossing registers as `[EVENT]` rather than minting an asset kind beside it.

## [05]-[PROCESS]

- `assay contracts generate` authors every descriptor module, publisher-asset module, and roster row.
- `pnpm --filter @rasm/contracts pack` proves the publishable artifact after canonical generation.
- Distribution metadata answers that pack proof alone — publication runs operator-side, and no configuration re-derives `files` or `publishConfig`.
- `files` ships `dist` and `README.md` alone; the gate proves every non-built row on disk, so a manifest row cannot outlive its file.

import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { describe, expect, it, layer } from '@effect/vitest';
import { Array, Effect, Option, Record, Schema } from 'effect';

// --- [CONSTANTS] -------------------------------------------------------------------------

const _HOME = new URL('../..', import.meta.url).pathname;

// --- [MODELS] ----------------------------------------------------------------------------

const _Manifest = Schema.Struct({
    name: Schema.NonEmptyString,
    dependencies: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.String }), { default: () => ({}) }),
});

// --- [OPERATIONS] ------------------------------------------------------------------------

const _decode = Schema.decodeUnknown(Schema.parseJson(_Manifest));

// Version facts live only in pnpm-workspace.yaml: a spec-estate dependency resolves through the catalog or the workspace graph.
const _admitted = (pin: string): boolean => pin === 'catalog:' || pin.startsWith('workspace:');

layer(NodeContext.layer)('workspace admission', (it) => {
    it.effect('every spec-estate package pins through catalog: or workspace: only', () =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const entries = yield* fs.readDirectory(_HOME);
            const manifests = yield* Effect.forEach(
                Array.filter(entries, (entry) => !entry.startsWith('.')),
                (entry) =>
                    Effect.option(Effect.flatMap(fs.readFileString(path.join(_HOME, entry, 'package.json')), (raw) => Effect.orDie(_decode(raw)))),
            );
            const packages = Array.getSomes(manifests);
            expect(packages.length).toBeGreaterThan(0);
            const stray = Array.flatMap(packages, (manifest) =>
                Array.filterMap(Record.toEntries(manifest.dependencies), ([name, pin]) =>
                    _admitted(pin) ? Option.none() : Option.some(`${manifest.name}: ${name}@${pin}`),
                ),
            );
            expect(stray).toEqual([]);
        }),
    );
});

describe('admission predicate', () => {
    it('refuses the inline version literal the gauge exists to catch', () => {
        expect(_admitted('catalog:')).toBe(true);
        expect(_admitted('workspace:*')).toBe(true);
        expect(_admitted('^4.1.9')).toBe(false);
        expect(_admitted('latest')).toBe(false);
    });
});

import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { expect, layer } from '@effect/vitest';
import { Array, Effect, HashMap, Option } from 'effect';
import { Asset, ContentDigest, Corpus, CorpusFault, CorpusRoot } from './corpus.ts';

// --- [CONSTANTS] -------------------------------------------------------------------------

// A two-fixture scratch registry: one REAL entry with emitted pairs, one DESIGN-PIN entry whose seam holds only a stray file.
const _SCRATCH_MANIFEST = [
    '| [INDEX] | [FIXTURE] | [SEAM] | [PRODUCER] | [PAYLOAD] | [PIN] |',
    '| :-----: | :-------- | :----- | :--------- | :-------- | :---- |',
    '| [01] | EMITTED_ONE | `alpha` | `csharp:Owner/emit` | `wire-bytes` + `canonical-json` | REAL |',
    '| [02] | PINNED_ONE | `beta` | `csharp:Owner/pin` | `wire-bytes` | DESIGN-PIN |',
    '| [03] | SEAMFILE_ONE | `gamma` | `csharp:Owner/broken` | `wire-bytes` | REAL |',
].join('\n');

const _ECHO_BYTES = Uint8Array.from([0xca, 0xfe, 0x00, 0x42]);
const _ECHO_JSON = '{"echo":true}';

// --- [OPERATIONS] ------------------------------------------------------------------------

const _scratchCorpus = Effect.gen(function* () {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const scratch = yield* fs.makeTempDirectoryScoped();
    yield* fs.writeFileString(path.join(scratch, 'MANIFEST.md'), _SCRATCH_MANIFEST);
    yield* fs.makeDirectory(path.join(scratch, 'alpha'));
    yield* fs.makeDirectory(path.join(scratch, 'beta'));
    yield* fs.writeFile(path.join(scratch, 'alpha', 'echo.bin'), _ECHO_BYTES);
    yield* fs.writeFileString(path.join(scratch, 'alpha', 'echo.json'), _ECHO_JSON);
    yield* fs.writeFileString(path.join(scratch, 'beta', 'NOTES.md'), 'stray non-asset file');
    yield* fs.writeFileString(path.join(scratch, 'gamma'), 'a seam path occupied by a file, not a directory');
    return scratch;
});

layer(NodeContext.layer)('corpus', (it) => {
    it.effect('manifest ledger decodes with honest pin states', () =>
        Effect.gen(function* () {
            const registry = yield* Corpus.manifest;
            const identity = yield* HashMap.get(registry, 'CANONICAL_BYTE_IDENTITY');
            const drift = yield* HashMap.get(registry, 'DESCRIPTOR_DRIFT');
            expect(identity.pin).toBe('REAL');
            expect(identity.seam).toBe('content-identity');
            expect(identity.payloads).toContain('digest');
            expect(drift.pin).toBe('DESIGN-PIN');
        }),
    );

    it.effect('a REAL fixture never resolves Blocked; an unemitted DESIGN-PIN resolves Blocked', () =>
        Effect.gen(function* () {
            const real = yield* Corpus.resolve('CANONICAL_BYTE_IDENTITY');
            const pinned = yield* Corpus.resolve('HLC_TWO_HALF');
            expect(Asset.$is('Blocked')(real)).toBe(false);
            expect(Asset.$is('Blocked')(pinned)).toBe(true);
        }),
    );

    it.effect('the bare resolve sweeps the whole registry and every pin lands on its honest arm', () =>
        Effect.gen(function* () {
            const registry = yield* Corpus.manifest;
            const census = yield* Corpus.resolve();
            expect(census).toHaveLength(HashMap.size(registry));
            for (const asset of census) {
                expect(asset.fixture.pin === 'REAL' ? Asset.$is('Blocked')(asset) : Asset.$is('Awaiting')(asset)).toBe(false);
            }
        }),
    );

    it.effect('an unregistered fixture is a typed fault, never a vacuous pass', () =>
        Effect.gen(function* () {
            const fault = yield* Effect.flip(Corpus.resolve('PHANTOM_FIXTURE'));
            expect(fault).toBeInstanceOf(CorpusFault);
            expect(fault.reason).toBe('unregistered');
        }),
    );

    it.effect('a malformed manifest is a typed decode fault at the seam', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const scratch = yield* fs.makeTempDirectoryScoped();
                yield* fs.writeFileString(path.join(scratch, 'MANIFEST.md'), '| broken | ledger |');
                const fault = yield* Effect.flip(Effect.provideService(Corpus.manifest, CorpusRoot, scratch));
                expect(fault.reason).toBe('malformed');
            }),
        ),
    );

    it.effect('an emitted pair loads its frozen bytes and canonical JSON through the reader', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const scratch = yield* _scratchCorpus;
                const asset = yield* Effect.provideService(Corpus.resolve('EMITTED_ONE'), CorpusRoot, scratch);
                const pairs = Asset.$match(asset, { Emitted: ({ pairs: held }) => held, Awaiting: () => [], Blocked: () => [] });
                expect(Array.map(pairs, (pair) => pair.message)).toEqual(['echo']);
                const head = yield* Array.head(pairs);
                const loaded = yield* Effect.provideService(Corpus.load(asset.fixture, head), CorpusRoot, scratch);
                expect(loaded.bytes).toEqual(Option.some(_ECHO_BYTES));
                expect(loaded.json).toEqual(Option.some(_ECHO_JSON));
            }),
        ),
    );

    it.effect('a stray non-asset file never mints a vacuous Emitted: the pin still decides', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const scratch = yield* _scratchCorpus;
                const pinned = yield* Effect.provideService(Corpus.resolve('PINNED_ONE'), CorpusRoot, scratch);
                expect(Asset.$is('Blocked')(pinned)).toBe(true);
            }),
        ),
    );

    it.effect('an unreadable seam is a typed fault, never a vacuous pin verdict: only NotFound is honest absence', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const scratch = yield* _scratchCorpus;
                const fault = yield* Effect.flip(Effect.provideService(Corpus.resolve('SEAMFILE_ONE'), CorpusRoot, scratch));
                expect(fault).toBeInstanceOf(CorpusFault);
                expect(fault.reason).toBe('unreadable');
            }),
        ),
    );
});

// The named TS consumer law: seed-zero XxHash128 over the frozen 52-byte stream reproduces the canonical digest.
layer(ContentDigest.Default)('content digest', (it) => {
    it.effect('reproduces the frozen CANONICAL_BYTE_IDENTITY digest', () =>
        Effect.gen(function* () {
            const digest = yield* ContentDigest.x128(Corpus.CANONICAL_BYTE_IDENTITY.bytes);
            expect(digest).toBe(Corpus.CANONICAL_BYTE_IDENTITY.digest);
        }),
    );

    it.effect('a single flipped byte refutes the digest', () =>
        Effect.gen(function* () {
            const corrupted = Uint8Array.from(Corpus.CANONICAL_BYTE_IDENTITY.bytes);
            corrupted[0] = 0xff;
            const digest = yield* ContentDigest.x128(corrupted);
            expect(digest).not.toBe(Corpus.CANONICAL_BYTE_IDENTITY.digest);
        }),
    );

    it.effect('the canonical digest is the byte-reversed little-endian twin', () =>
        Effect.sync(() => {
            const reversed = Option.map(Option.fromNullable(Corpus.CANONICAL_BYTE_IDENTITY.memoryLe.match(/../g)), (pairs) =>
                [...pairs].reverse().join(''),
            );
            expect(reversed).toEqual(Option.some(Corpus.CANONICAL_BYTE_IDENTITY.digest));
        }),
    );
});

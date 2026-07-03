import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { expect, layer } from '@effect/vitest';
import { Effect, HashMap, Option } from 'effect';
import { Asset, ContentDigest, Corpus, CorpusFault, CorpusRoot } from './corpus.ts';

// --- [OPERATIONS] ------------------------------------------------------------------------

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
});

// The named TS consumer law: seed-zero XxHash128 over the frozen 52-byte stream reproduces the canonical digest.
layer(ContentDigest.Default)('content digest', (it) => {
    it.effect('reproduces the frozen CANONICAL_BYTE_IDENTITY digest', () =>
        Effect.gen(function* () {
            const digest = yield* ContentDigest.x32(Corpus.CANONICAL_BYTE_IDENTITY.bytes);
            expect(digest).toBe(Corpus.CANONICAL_BYTE_IDENTITY.digest);
        }),
    );

    it.effect('a single flipped byte refutes the digest', () =>
        Effect.gen(function* () {
            const corrupted = Uint8Array.from(Corpus.CANONICAL_BYTE_IDENTITY.bytes);
            corrupted[0] = 0xff;
            const digest = yield* ContentDigest.x32(corrupted);
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

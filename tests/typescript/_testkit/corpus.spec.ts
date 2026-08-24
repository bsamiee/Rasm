import { FileSystem, Path } from '@effect/platform';
import { NodeContext } from '@effect/platform-node';
import { expect, layer } from '@effect/vitest';
import { Array, Effect, Option } from 'effect';
import { Corpus, CorpusRoot } from './corpus.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

layer(NodeContext.layer)('corpus', (it) => {
    it.effect('strictly decodes V2 and proves every verified vector at the strongest TypeScript authority', () =>
        Effect.gen(function* () {
            const manifest = yield* Corpus.manifest;
            const proof = yield* Corpus.prove(manifest);
            const cases = Array.flatMap(manifest.entries, (entry) => entry.cases);
            const vectors = Array.reduce(
                cases,
                0,
                (total, contract) => total + (contract.readiness.kind === 'verified' ? contract.readiness.vectors.length : 0),
            );
            const assets = Array.reduce(
                cases,
                0,
                (total, contract) =>
                    total +
                    (contract.readiness.kind === 'verified'
                        ? Array.reduce(
                              contract.readiness.vectors,
                              0,
                              (held, vector) => held + vector.specimens.length + Option.match(vector.expected, { onNone: () => 0, onSome: () => 1 }),
                          )
                        : 0),
            );
            expect(proof.verified).toBe(vectors);
            expect(proof.assets).toBe(assets);
            expect(proof.blocked).toBe(Array.filter(cases, (contract) => contract.readiness.kind === 'blocked').length);
        }),
    );

    it.effect('preserves both HLC uint64 axes through the exact generated type', () =>
        Effect.gen(function* () {
            const manifest = yield* Corpus.manifest;
            const contract = yield* Corpus.resolve(manifest, { entry: 'hlc-stamp', case: 'message' });
            const proof = yield* Corpus.prove(contract);
            const vectors = yield* contract.readiness.kind === 'verified'
                ? Effect.succeed(contract.readiness.vectors.length)
                : Effect.dieMessage('hlc-stamp/message must carry verified vectors');
            expect(proof.valueParity).toBe(vectors);
            expect(proof.verified).toBe(vectors);
        }),
    );

    it.effect('refuses drift in a live registered specimen', () =>
        Effect.scoped(
            Effect.gen(function* () {
                const fs = yield* FileSystem.FileSystem;
                const path = yield* Path.Path;
                const manifest = yield* Corpus.manifest;
                const contract = yield* Corpus.resolve(manifest, { entry: 'fault-wire', case: 'detail' });
                const specimen = yield* contract.readiness.kind === 'verified'
                    ? Option.flatMap(Array.head(contract.readiness.vectors), (vector) => Array.head(vector.specimens))
                    : Option.none();
                const raw = yield* Corpus.load(specimen);
                const root = yield* fs.makeTempDirectoryScoped();
                const target = path.join(root, specimen.path);
                yield* fs.makeDirectory(path.dirname(target), { recursive: true });
                yield* fs.writeFile(
                    target,
                    Uint8Array.from(raw, (byte, index) => (index === 0 ? byte ^ 1 : byte)),
                );
                const fault = yield* Effect.flip(Effect.provideService(Corpus.prove(contract), CorpusRoot, root));
                expect(fault.reason).toBe('drift');
                expect(fault.detail).toContain(specimen.path);
            }),
        ),
    );

    it.effect('refuses a corpus path traversal before filesystem access', () =>
        Effect.gen(function* () {
            const manifest = yield* Corpus.manifest;
            const contract = yield* Corpus.resolve(manifest, { entry: 'fault-wire', case: 'detail' });
            const specimen = yield* contract.readiness.kind === 'verified'
                ? Option.flatMap(Array.head(contract.readiness.vectors), (vector) => Array.head(vector.specimens))
                : Option.none();
            const fault = yield* Effect.flip(Corpus.load({ ...specimen, path: '../outside.bin' }));
            expect(fault.reason).toBe('unregistered');
            expect(fault.detail).toContain('not normalized corpus-relative');
        }),
    );
});

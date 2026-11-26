import { assert, it } from '@effect/vitest';
import { HostError } from '@rasm/object-model/model';
import { Render } from '@rasm/object-model/rpcs';
import { execute, Premiere, render } from '@rasm/premierepro/uxp';
import { Unmatched } from '@rasm/search/unmatched';
import { Effect, Layer, Option, Path, Schema } from 'effect';
import { vi } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FRAME = { width: 1920, height: 1080 } as const;
const WRITTEN = { name: 'rendered.png', nativePath: '/private/var/folders/plugin-temp/rendered.png' } as const;

// --- [MODELS] --------------------------------------------------------------------------

class Guid {
    readonly #text: string;

    constructor(text: string) {
        this.#text = text;
    }

    toString(): string {
        return this.#text;
    }
}
class Sequence {
    readonly name: string;
    readonly guid: Guid;
    readonly position: { readonly seconds: number };
    readonly frame = FRAME;

    constructor(name: string, guid: string, seconds: number) {
        this.name = name;
        this.guid = new Guid(guid);
        this.position = { seconds };
    }

    getPlayerPosition() {
        return Promise.resolve(this.position);
    }

    getFrameSize() {
        return Promise.resolve(this.frame);
    }
}
class Project {
    readonly name = 'Edit.prproj';
    readonly sequences: readonly Sequence[];

    constructor(sequences: readonly Sequence[]) {
        this.sequences = sequences;
    }

    getSequences() {
        return Promise.resolve(this.sequences);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const hosted = (answer: boolean) => {
    const sequences = [new Sequence('Assembly', 'a1', 2), new Sequence('Final', 'b2', 5)] as const;
    const exportSequenceFrame = vi.fn<(...args: readonly unknown[]) => Promise<boolean>>(() => Promise.resolve(answer));
    const module = { Project: { getActiveProject: () => Promise.resolve(new Project(sequences)) }, Exporter: { exportSequenceFrame }, EventManager: {}, Constants: {} };
    const required = (id: string): unknown => (id === 'premierepro' ? module : undefined);
    return {
        sequences,
        exportSequenceFrame,
        run: <A, E, R>(effect: Effect.Effect<A, E, R>) =>
            Effect.acquireRelease(
                Effect.sync(() => vi.stubGlobal('require', required)),
                () => Effect.sync(() => vi.unstubAllGlobals()),
            ).pipe(Effect.andThen(Effect.provide(effect, Layer.merge(Premiere.layer, Path.layer))), Effect.scoped),
    };
};
const requested = (target: string) => Schema.decodeEffect(Render)({ target });

// --- [COMPOSITION] ---------------------------------------------------------------------

vi.mock(import('adobe:uxp'), () => ({}));
vi.mock(import('adobe:fs'), () => ({}));

it.effect('answers a sequence a body returns by its guid, the key a rename or reorder of the sequence list keeps', () =>
    hosted(true).run(
        Effect.map(execute("(function body() {\n    return resolve('/project/sequence[1]');\n})"), (result) => {
            assert.deepStrictEqual(result, Option.some({ type: 'Sequence', specifier: '/project/sequence[@guid="b2"]', valid: true, name: 'Final' }));
        }),
    ),
);

it.effect('exports the frame at the player position into the folder of the written file by its name, at the sequence frame size', () => {
    const host = hosted(true);
    return host.run(
        Effect.gen(function* () {
            yield* render(yield* requested('/project/sequence[@guid="a1"]'), WRITTEN);
            assert.deepStrictEqual(host.exportSequenceFrame.mock.calls, [[host.sequences[0], { seconds: 2 }, 'rendered.png', '/private/var/folders/plugin-temp/', FRAME.width, FRAME.height]]);
        }),
    );
});

it.effect('refuses a project target, as Premiere exports frames from a sequence alone', () =>
    hosted(true).run(
        Effect.gen(function* () {
            const failed = yield* Effect.flip(render(yield* requested('/project'), WRITTEN));
            assert.instanceOf(failed, Unmatched);
            assert.strictEqual(failed.subject, 'sequence');
        }),
    ),
);

it.effect('fails a render whose export answers false, as Premiere returns a refused export as a value', () =>
    hosted(false).run(
        Effect.gen(function* () {
            const failed = yield* Effect.flip(render(yield* requested('/project/sequence[0]'), WRITTEN));
            assert.instanceOf(failed, HostError);
            assert.strictEqual(failed.reason._tag, 'Refused');
        }),
    ),
);

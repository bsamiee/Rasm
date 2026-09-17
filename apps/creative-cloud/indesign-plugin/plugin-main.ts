// --- [IMPORTS] -------------------------------------------------------------------------

import './runtime.ts';
import { entrypoints } from 'adobe:uxp';
import { Effect, Fiber, Option, Ref, Stream, SubscriptionRef } from 'effect';
import { run } from './client.ts';
import { manifest, panel } from './uxp.config.ts';

// --- [STATE] ---------------------------------------------------------------------------

const _status = Effect.runSync(SubscriptionRef.make(manifest.version));
const _link = Ref.makeUnsafe(Option.none<Fiber.Fiber<void>>());
const _panel = Ref.makeUnsafe(Option.none<Fiber.Fiber<void>>());

// --- [LIFECYCLE] -----------------------------------------------------------------------

const _start = (slot: Ref.Ref<Option.Option<Fiber.Fiber<void>>>, work: Effect.Effect<void>): Promise<void> => Effect.runPromise(Ref.set(slot, Option.some(Effect.runFork(work))));

const _stop = (slot: Ref.Ref<Option.Option<Fiber.Fiber<void>>>): Promise<void> =>
    Effect.runPromise(Effect.flatMap(Ref.getAndSet(slot, Option.none()), Option.match({ onNone: () => Effect.void, onSome: Fiber.interrupt })));

const _render = (root: HTMLElement): Effect.Effect<void> =>
    Effect.scoped(
        Effect.gen(function* () {
            const line = yield* Effect.acquireRelease(
                Effect.sync(() => root.appendChild(document.createElement('p'))),
                (appended) => Effect.sync(() => appended.remove()),
            );
            yield* Stream.runForEach(SubscriptionRef.changes(_status), (text) =>
                Effect.sync(() => {
                    line.textContent = text;
                }),
            );
        }),
    );

const _setup: Pick<typeof entrypoints, 'plugin'> & { readonly panels: Record<string, Record<'create' | 'destroy', (root: HTMLElement) => Promise<void>>> } = {
    plugin: { create: () => _start(_link, run(_status)), destroy: () => _stop(_link) },
    panels: { [panel.id]: { create: (root) => _start(_panel, _render(root)), destroy: () => _stop(_panel) } },
};

entrypoints.setup(_setup as typeof entrypoints);

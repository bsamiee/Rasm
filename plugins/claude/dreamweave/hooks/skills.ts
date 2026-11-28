import type { Part } from '@rasm/claude-code/host';
import { Array, Effect, Layer, Option, type PlatformError, Predicate } from 'effect';
import { KeyValueStore } from 'effect/persistence';

// --- [TYPES] ---------------------------------------------------------------------------

interface Cut {
    readonly name: string;
    readonly body: number;
    readonly prefix: string;
    readonly at: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const MARKER = '\n\n[... skill content truncated for compaction; use Read on the skill path if you need the full text]';
const HEADING = '### Skill: ';

// --- [OPERATIONS] ----------------------------------------------------------------------

const cuts = (text: string): readonly Cut[] =>
    Array.unfold(text.indexOf(MARKER), (at) => Option.liftPredicate([at, text.indexOf(MARKER, at + MARKER.length)] as const, () => at !== -1)).map((at) => {
        const heading = text.lastIndexOf(HEADING, at) + HEADING.length;
        const body = text.indexOf('\n\n', heading) + 2;
        return { name: text.slice(heading, text.indexOf('\n', heading)), body, prefix: text.slice(body, at), at };
    });

const whole = (store: KeyValueStore.KeyValueStore, cut: Cut) =>
    Effect.map(
        Effect.catch(store.get(`skill:${cut.name}`), (error) => Effect.as(Effect.logWarning(`store.get skill:${cut.name} failed, so its compaction keeps the engine's cut text`, error), undefined)),
        (saved) => (Predicate.isString(saved) && saved.length > cut.prefix.length && saved.startsWith(cut.prefix) ? saved : `${cut.prefix}${MARKER}`),
    );
const restored = (text: string) =>
    KeyValueStore.KeyValueStore.use((store) => {
        const found = cuts(text);
        return Effect.map(
            Effect.forEach(found, (cut) => whole(store, cut)),
            (bodies) => found.reduceRight((joined, cut, at) => `${joined.slice(0, cut.body)}${bodies[at] ?? cut.prefix}${joined.slice(cut.at + MARKER.length)}`, text),
        );
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const skills: Part<never, never, PlatformError.PlatformError> = {
    layer: Layer.empty,
    hooks: {
        'skill.prompt': (e, next) => Effect.tap(next(e), (result) => KeyValueStore.KeyValueStore.use((store) => store.set(`skill:${e.skill}`, result.text)).pipe(Effect.catch((error) => Effect.logWarning(`store.set skill:${e.skill} failed, so its compaction keeps the engine's cut text`, error)))),
        'prompt.attachment': (e, next) => (e.type === 'invoked_skills' ? Effect.flatMap(next(e), ({ text }) => Effect.map(text === null ? Effect.succeed(null) : restored(text), (shown) => ({ text: shown }))) : next(e)),
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { skills };

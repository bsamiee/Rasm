import type { EngineInterface, On } from 'claude-code';
import { Array, Cause, Effect, Option, Predicate } from 'effect';

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

function whole($: EngineInterface, cut: Cut) {
    return Effect.map(
        Effect.promise(() => $.store.get(`skill:${cut.name}`)),
        (saved) => (Predicate.isString(saved) && saved.length > cut.prefix.length && saved.startsWith(cut.prefix) ? saved : `${cut.prefix}${MARKER}`),
    );
}

function restored($: EngineInterface, text: string) {
    const found = cuts(text);
    return Effect.map(
        Effect.forEach(found, (cut) => whole($, cut)),
        (bodies) => found.reduceRight((joined, cut, at) => `${joined.slice(0, cut.body)}${bodies[at] ?? cut.prefix}${joined.slice(cut.at + MARKER.length)}`, text),
    );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

function skills(on: On) {
    on('skill.prompt', ($, e, next) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const result = yield* Effect.promise(() => next(e));
                yield* Effect.tryPromise({ try: () => $.store.set(`skill:${e.skill}`, result.text), catch: (cause) => new Cause.UnknownError(cause, `store.set skill:${e.skill} failed, so its compaction keeps the engine's cut text`) }).pipe(
                    Effect.catch((error) => Effect.sync(() => $.ui.log(error.message, { to: 'debug' }))),
                );
                return result;
            }),
        ),
    );
    on('prompt.attachment', { type: 'invoked_skills' }, ($, e, next) =>
        Effect.runPromise(
            Effect.gen(function* () {
                const { text } = yield* Effect.promise(() => next(e));
                return { text: text === null ? null : yield* restored($, text) };
            }),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { skills };

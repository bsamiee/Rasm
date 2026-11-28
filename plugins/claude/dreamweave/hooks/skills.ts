import type { Part } from '@rasm/claude-code/host';
import { Skills } from '@rasm/claude-code/skills';
import { Array, Effect, Option, type PlatformError } from 'effect';

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
const whole = (cut: Cut) =>
    Skills.use((skills) => skills.text(cut.name)).pipe(
        Effect.catch((error) => Effect.as(Effect.logWarning('skill text unread, cut text kept', error).pipe(Effect.annotateLogs({ skill: cut.name })), Option.none<string>())),
        Effect.map((saved) =>
            saved.pipe(
                Option.filter((full) => full.length > cut.prefix.length && full.startsWith(cut.prefix)),
                Option.getOrElse(() => `${cut.prefix}${MARKER}`),
            ),
        ),
    );
const restore = (text: string) => {
    const found = cuts(text);
    return Effect.map(Effect.forEach(found, whole), (bodies) => found.reduceRight((joined, cut, at) => `${joined.slice(0, cut.body)}${bodies[at] ?? cut.prefix}${joined.slice(cut.at + MARKER.length)}`, text));
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const skills: Part<Skills, never, PlatformError.PlatformError> = {
    layer: Skills.layer,
    hooks: {
        ...Skills.hooks,
        'prompt.attachment': (e, next) => (e.type === 'invoked_skills' ? Effect.flatMap(next(e), ({ text }) => Effect.map(text === null ? Effect.succeed(null) : restore(text), (shown) => ({ text: shown }))) : next(e)),
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { skills };

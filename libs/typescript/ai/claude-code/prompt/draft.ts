import type { EngineInterface, PromptBox, PromptFillArgs, PromptFilled, PromptPaintArgs, PromptPainted, PromptSubmitArgs, PromptSubmitResult, PromptSuggestArgs, PromptSuggestResult } from 'claude-code';
import { Context, Effect, Layer, type PlatformError } from 'effect';
import { call } from '#/engine/services';

// --- [SERVICES] ------------------------------------------------------------------------

class Draft extends Context.Service<
    Draft,
    {
        // --- [PERSON]

        readonly read: Effect.Effect<PromptBox, PlatformError.PlatformError>;
        readonly fill: (args: PromptFillArgs) => Effect.Effect<PromptFilled, PlatformError.PlatformError>;
        readonly paint: (runs: (box: PromptBox) => PromptPaintArgs['decorations']) => Effect.Effect<PromptPainted, PlatformError.PlatformError>;
        readonly suggest: (args: PromptSuggestArgs) => Effect.Effect<PromptSuggestResult, PlatformError.PlatformError>;

        // --- [MODEL]

        readonly submit: (args: PromptSubmitArgs) => Effect.Effect<PromptSubmitResult, PlatformError.PlatformError>;
    }
>()('@rasm/claude-code/prompt/draft') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

function draft($: EngineInterface) {
    const read = call('prompt.read', 'main', () => $.prompt.read());
    const paint = (box: PromptBox, runs: (box: PromptBox) => PromptPaintArgs['decorations']): Effect.Effect<PromptPainted, PlatformError.PlatformError> =>
        call('prompt.paint', 'main', () => $.prompt.paint({ text: box.text, decorations: runs(box) })).pipe(Effect.filterOrElse((painted) => painted.isPainted || painted.text === box.text, (painted) => paint(painted, runs)));
    return Layer.succeed(Draft)({
        read,
        fill: (args) => call('prompt.fill', 'main', () => $.prompt.fill(args)),
        paint: (runs) => Effect.flatMap(read, (box) => paint(box, runs)),
        suggest: (args) => call('prompt.suggest', 'main', () => $.prompt.suggest(args)),
        submit: (args) => call('prompt.submit', 'main', () => $.prompt.submit(args)),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Draft, draft };

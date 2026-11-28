import type { EngineInterface, PromptBox, PromptDecoration, PromptFillArgs, PromptPainted, PromptSuggestArgs } from 'claude-code';
import { Context, Effect, type PlatformError } from 'effect';
import { call } from '#/engine/services';

// --- [SERVICES] ------------------------------------------------------------------------

class Draft extends Context.Service<Draft, ReturnType<typeof makeDraft>>()('@rasm/claude-code/ui/draft') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

function makeDraft($: EngineInterface) {
    const read = call('prompt.read', 'main', () => $.prompt.read());
    const paint = (box: PromptBox, runs: (box: PromptBox) => PromptDecoration[]): Effect.Effect<PromptPainted, PlatformError.PlatformError> =>
        call('prompt.paint', 'main', () => $.prompt.paint({ text: box.text, decorations: runs(box) })).pipe(
            Effect.filterOrElse(
                (painted) => painted.isPainted || painted.text === box.text,
                (painted) => paint(painted, runs),
            ),
        );
    return {
        read,
        fill: (args: PromptFillArgs) => call('prompt.fill', 'main', () => $.prompt.fill(args)),
        paint: (runs: (box: PromptBox) => PromptDecoration[]) => Effect.flatMap(read, (box) => paint(box, runs)),
        suggest: (args: PromptSuggestArgs) => call('prompt.suggest', 'main', () => $.prompt.suggest(args)),
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Draft, makeDraft };

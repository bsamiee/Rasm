import type { Args, EngineInterface, Frozen, GlobNext, ThemeResult, ThemeSetArgs } from 'claude-code';
import { Context, Data, Effect, Layer, Option, PlatformError, Ref, type Scope, Semaphore } from 'effect';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

interface Shown {
    readonly plugin: string;
    readonly attributes: Frozen<ThemeSetArgs>;
}
type Standing = Data.TaggedEnum<{
    Set: Shown;
    Preview: Shown & { readonly restores: Option.Option<Shown> };
}>;

// --- [MODELS] --------------------------------------------------------------------------

const Standing = Data.taggedEnum<Standing>();

// --- [SERVICES] ------------------------------------------------------------------------

class Theme extends Context.Service<
    Theme,
    {
        readonly set: (attributes: ThemeSetArgs) => Effect.Effect<ThemeResult, PlatformError.PlatformError>;
        readonly clear: Effect.Effect<ThemeResult, PlatformError.PlatformError>;
        readonly preview: (attributes: ThemeSetArgs) => Effect.Effect<ThemeResult, PlatformError.PlatformError, Scope.Scope>;
    }
>()('@rasm/claude-code/ui/theme') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

function heard($: EngineInterface, standing: Ref.Ref<Option.Option<Standing>>, e: Frozen<Args<'theme.set' | 'theme.clear'>>, next: GlobNext<'theme.*'>) {
    const { plugin } = next.origin;
    return plugin === $.plugin.name
        ? next(e)
        : next(e).then((answer) => {
              Effect.runSync(Effect.when(Ref.set(standing, next.is('theme.set', e) ? Option.some(Standing.Set({ plugin, attributes: e })) : Option.none()), Effect.succeed(answer.deny === undefined)));
              return answer;
          });
}

function theme($: EngineInterface, standing: Ref.Ref<Option.Option<Standing>>) {
    const permits = Semaphore.makeUnsafe(1);
    const plugin = $.plugin.name;
    const show = (attributes: ThemeSetArgs, held: Standing) => call('theme.set', plugin, () => $.theme.set(attributes)).pipe(Effect.tap(Ref.set(standing, Option.some(held))));
    const set = (attributes: ThemeSetArgs) => show(attributes, Standing.Set({ plugin, attributes }));
    const clear = call('theme.clear', plugin, () => $.theme.clear()).pipe(Effect.tap(Ref.set(standing, Option.none())));
    const open = Effect.fnUntraced(function* (attributes: ThemeSetArgs) {
        const restores = Option.flatMap(yield* Ref.get(standing), Standing.$match({ Set: Option.some<Shown>, Preview: ({ restores: held }) => held }));
        const foreign = Option.filter(restores, (held) => held.plugin !== plugin && held.attributes.backdrop?.scene !== undefined);
        yield* Option.match(foreign, {
            onNone: () => Effect.void,
            onSome: (held) => PlatformError.badArgument({ module: 'Theme', method: 'preview', description: `scene ${held.attributes.backdrop?.scene} of ${held.plugin}, which this plugin cannot set again` }),
        });
        const held = Standing.Preview({ plugin, attributes, restores });
        return [yield* show(attributes, held), held] as const;
    });
    return Layer.succeed(Theme)({
        set: (attributes) => Semaphore.withPermit(permits)(set(attributes)),
        clear: Semaphore.withPermit(permits)(clear),
        preview: (attributes) =>
            Effect.acquireRelease(Semaphore.withPermit(permits)(open(attributes)), ([, held]) =>
                Ref.get(standing).pipe(
                    Effect.flatMap((current) => (Option.exists(current, (shown) => shown === held) ? Option.match(held.restores, { onNone: () => clear, onSome: (restored) => set(restored.attributes) }) : Effect.void)),
                    Semaphore.withPermit(permits),
                    Effect.catchCause((cause) => Effect.logWarning('theme preview did not restore', cause)),
                ),
            ).pipe(Effect.map(([shown]) => shown)),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { heard, Theme, theme };

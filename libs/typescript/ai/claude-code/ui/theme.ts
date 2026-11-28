import type { Args, EngineInterface, Frozen, GlobNext, ThemeResult, ThemeSetArgs } from 'claude-code';
import { Array, Context, Data, Effect, Layer, Option, PlatformError, Ref, type Scope, Semaphore, Struct, Tuple } from 'effect';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

interface Shown {
    readonly plugin: string;
    readonly attributes: Frozen<ThemeSetArgs>;
}
type Standing = Data.TaggedEnum<{
    Set: Shown;
    Preview: { readonly previews: Array.NonEmptyReadonlyArray<Shown>; readonly over: Option.Option<Shown> };
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

function observe($: EngineInterface, standing: Ref.Ref<Option.Option<Standing>>, e: Frozen<Args<'theme.set' | 'theme.clear'>>, next: GlobNext<'theme.*'>) {
    const { plugin } = next.origin;
    return plugin === $.plugin.name ? next(e) : next(e).then((answer) => (answer.deny === undefined ? Effect.runSync(Effect.as(Ref.set(standing, next.is('theme.set', e) ? Option.some(Standing.Set({ plugin, attributes: e })) : Option.none()), answer)) : answer));
}

function theme($: EngineInterface, standing: Ref.Ref<Option.Option<Standing>>) {
    const plugin = $.plugin.name;
    const locked = Semaphore.withPermit(Semaphore.makeUnsafe(1));
    const show = (held: Option.Option<Standing>) =>
        Option.match(held, {
            onNone: () => call('theme.clear', plugin, () => $.theme.clear()),
            onSome: (stands) => call('theme.set', plugin, () => $.theme.set(Standing.$match(stands, { Set: Struct.get('attributes'), Preview: ({ previews }) => Array.lastNonEmpty(previews).attributes }))),
        }).pipe(Effect.tap(Ref.set(standing, held)));
    return Layer.succeed(Theme)({
        set: (attributes) => Option.some(Standing.Set({ plugin, attributes })).pipe(show, locked),
        clear: Option.none().pipe(show, locked),
        preview: (attributes) =>
            Effect.acquireRelease(
                locked(
                    Effect.gen(function* () {
                        const current = yield* Ref.get(standing);
                        const over = Option.flatMap(current, Standing.$match({ Set: Option.some<Shown>, Preview: Struct.get('over') }));
                        yield* Option.match(
                            Option.filter(over, (held) => held.plugin !== plugin && held.attributes.backdrop?.scene !== undefined),
                            { onNone: () => Effect.void, onSome: (held) => PlatformError.badArgument({ module: 'Theme', method: 'preview', description: `scene held, scene ${held.attributes.backdrop?.scene}, plugin ${held.plugin}` }) },
                        );
                        const held: Shown = { plugin, attributes };
                        const previews = Option.match(Option.filter(current, Standing.$is('Preview')), { onNone: () => Array.of(held), onSome: (open) => Array.append(open.previews, held) });
                        return [yield* show(Option.some(Standing.Preview({ previews, over }))), held] as const;
                    }),
                ),
                ([, held]) =>
                    Ref.get(standing).pipe(
                        Effect.map(Option.filter(Standing.$is('Preview'))),
                        Effect.map(Option.filter(({ previews }) => previews.includes(held))),
                        Effect.flatMap(
                            Option.match({
                                onNone: () => Effect.void,
                                onSome: ({ previews, over }) => {
                                    const left = previews.filter((open) => open !== held);
                                    const next = Array.isReadonlyArrayNonEmpty(left) ? Option.some(Standing.Preview({ previews: left, over })) : Option.map(over, Standing.Set);
                                    return Array.lastNonEmpty(previews) === held ? show(next) : Ref.set(standing, next);
                                },
                            }),
                        ),
                        locked,
                        Effect.catchCause((cause) => Effect.logWarning('preview restore failed', cause)),
                    ),
            ).pipe(Effect.map(Tuple.get(0))),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { observe, Theme, theme };

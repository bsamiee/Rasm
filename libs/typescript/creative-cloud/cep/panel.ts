import { fillColor } from '@rasm/mark';
import { wcagContrast } from 'culori';
import { Effect, FiberSet, Function, Layer, Logger, Predicate, Queue, Record, Schema, type Scope, Stream, Struct } from 'effect';
import { ExtensionManifest } from '#/index';

// --- [TYPES] ---------------------------------------------------------------------------

type Face<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X>) => void) => Effect.Effect<void, F, Scope.Scope>;

interface Cep {
    readonly evalScript: (script: string, callback: (reply: string) => void) => void;
    readonly getSystemPath: (type: string) => string;
    readonly getExtensionId: () => string;
    readonly getHostEnvironment: () => string;
    readonly addEventListener: (type: string, listener: () => void) => void;
    readonly removeEventListener: (type: string, listener: () => void) => void;
}

// --- [MODELS] --------------------------------------------------------------------------

const Cep = Schema.Struct(Struct.Record(['evalScript', 'getSystemPath', 'getExtensionId', 'getHostEnvironment', 'addEventListener', 'removeEventListener'], Schema.declare(Predicate.isFunction)));
const Panel = Schema.Struct({ __adobe_cep__: Schema.declare((host): host is Cep => Schema.is(Cep)(host)) });
const Rgb = Schema.Struct(Struct.Record(['red', 'green', 'blue'], Schema.Finite));
const Environment = Schema.fromJsonString(
    Schema.Struct({
        appSkinInfo: Schema.Struct({
            baseFontFamily: Schema.String,
            baseFontSize: Schema.Finite,
            panelBackgroundColorSRGB: Schema.Struct({ color: Rgb }),
            appBarBackgroundColorSRGB: Schema.Struct({ color: Rgb }),
            systemHighlightColor: Rgb,
        }),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const cepHost = Effect.suspend(() => Schema.decodeUnknownEffect(Panel)(globalThis)).pipe(Effect.map(Struct.get('__adobe_cep__')));
const css = ({ red, green, blue }: typeof Rgb.Type) => `rgb(${red} ${green} ${blue})`;
const skinned = (root: HTMLElement) => (environment: typeof Environment.Type) => {
    const { baseFontFamily, baseFontSize, panelBackgroundColorSRGB, appBarBackgroundColorSRGB, systemHighlightColor } = environment.appSkinInfo;
    const background = css(panelBackgroundColorSRGB.color);
    const scheme = wcagContrast(background, ExtensionManifest.fills.light.color) > wcagContrast(background, ExtensionManifest.fills.dark.color) ? 'light' : 'dark';
    return Effect.sync(() => {
        Record.toEntries({
            '--cep-host-background-color': background,
            '--cep-host-bar-background-color': css(appBarBackgroundColorSRGB.color),
            '--cep-host-highlight-color': css(systemHighlightColor),
            '--cep-host-text-color': fillColor(ExtensionManifest.fills[scheme]),
            '--cep-host-font-family': baseFontFamily,
            '--cep-host-font-size': `${baseFontSize}px`,
        }).forEach(([property, value]) => root.style.setProperty(property, value));
        Object.assign(root.style, { colorScheme: scheme, background: 'var(--cep-host-background-color)', color: 'var(--cep-host-text-color)', fontFamily: 'var(--cep-host-font-family)', fontSize: 'var(--cep-host-font-size)' });
    });
};
const followed = <E>(cep: Cep, queue: Queue.Queue<string, E>) => {
    const themeChanged = 'com.adobe.csxs.events.ThemeColorChanged';
    const listener = (): void => {
        Queue.offerUnsafe(queue, cep.getHostEnvironment());
    };
    return Effect.acquireRelease(
        Effect.sync(() => {
            cep.addEventListener(themeChanged, listener);
            listener();
        }),
        Function.constant(Effect.sync(() => cep.removeEventListener(themeChanged, listener))),
    );
};
const skin = Stream.unwrap(Effect.map(cepHost, (cep) => Stream.callback<string>((queue) => followed(cep, queue)))).pipe(Stream.mapEffect((environment) => Schema.decodeEffect(Environment)(environment)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const servePanel = <E = never, F = never>({ link, face }: { readonly link?: (cep: Cep) => Layer.Layer<never, E>; readonly face?: Face<F> }): void => {
    Effect.gen(function* () {
        const cep = yield* cepHost;
        yield* link === undefined ? Effect.void : Effect.forkScoped(Layer.launch(link(cep)));
        if (face !== undefined) {
            yield* Effect.forkScoped(Stream.runForEach(skin, skinned(document.documentElement)));
            const forked = yield* FiberSet.makeRuntime<never, void, never>();
            yield* face(document.body, (work) => forked(Effect.ignore(Effect.tapCause(Effect.asVoid(work), Effect.logError))));
        }
    }).pipe(Layer.effectDiscard, Layer.launch, Effect.tapCause(Effect.logError), Effect.provideService(Logger.LogToStderr, true), Effect.runFork);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Cep, servePanel, skin };

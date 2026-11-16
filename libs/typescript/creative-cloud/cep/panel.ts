import 'core-js/modules/web.url.can-parse.js';
import { Completion, Envelope } from '@rasm/extendscript';
import { HostError } from '@rasm/object-model';
import type { Unmatched } from '@rasm/search';
import { layer } from '@rasm/telemetry';
import { wcagContrast } from 'culori';
import { type Cause, Effect, FiberSet, Function, flow, Layer, Logger, Predicate, Record, Schema, type Scope, Stream, Struct } from 'effect';
import { FetchHttpClient } from 'effect/http';
import type { Rpc } from 'effect/rpc';
import { ExtensionManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X>) => void) => Effect.Effect<void, F, Scope.Scope>;

interface Cep extends Stream.EventListener<unknown> {
    readonly evalScript: (script: string, callback: (reply: string) => void) => void;
    readonly getSystemPath: (type: string) => string;
    readonly getExtensionId: () => string;
    readonly getHostEnvironment: () => string;
}

// --- [MODELS] --------------------------------------------------------------------------

const Cep = Schema.Struct(Struct.Record(['evalScript', 'getSystemPath', 'getExtensionId', 'getHostEnvironment', 'addEventListener', 'removeEventListener'], Schema.declare(Predicate.isFunction)));
const Hosted = Schema.Struct({ __adobe_cep__: Schema.declare((host): host is Cep => Schema.is(Cep)(host)) });
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
const Reply = Schema.fromJsonString(Completion);
const Sent = Function.memoize((rpc: { readonly _tag: string; readonly payloadSchema: Schema.Codec<unknown, unknown> }) => Schema.fromJsonString(Schema.toCodecJson(Envelope(rpc))));

// --- [OPERATIONS] ----------------------------------------------------------------------

const cepHost = Effect.suspend(() => Schema.decodeUnknownEffect(Hosted)(globalThis)).pipe(Effect.map(Struct.get('__adobe_cep__')));
const css = ({ red, green, blue }: typeof Rgb.Type) => `rgb(${red} ${green} ${blue})`;
const skinned = (root: HTMLElement) => (environment: typeof Environment.Type) => {
    const { baseFontFamily, baseFontSize, panelBackgroundColorSRGB, appBarBackgroundColorSRGB, systemHighlightColor } = environment.appSkinInfo;
    const background = css(panelBackgroundColorSRGB.color);
    return Effect.sync(() => {
        Record.toEntries({
            '--cep-host-bar-background-color': css(appBarBackgroundColorSRGB.color),
            '--cep-host-highlight-color': css(systemHighlightColor),
        }).forEach(([property, value]) => root.style.setProperty(property, value));
        Object.assign(root.style, { colorScheme: wcagContrast(background, 'black') > wcagContrast(background, 'white') ? 'light' : 'dark', background, color: 'CanvasText', fontFamily: baseFontFamily, fontSize: `${baseFontSize}px` });
    });
};
const skin = Stream.unwrap(
    Effect.gen(function* () {
        const cep = yield* cepHost;
        const changed = Stream.fromEventListener(cep, 'com.adobe.csxs.events.ThemeColorChanged');
        return changed.pipe(
            Stream.prepend([undefined]),
            Stream.map(() => cep.getHostEnvironment()),
            Stream.mapEffect((environment) => Schema.decodeEffect(Environment)(environment)),
        );
    }),
);
const bridge = (cep: Cep) =>
    Effect.fnUntraced(function* <Tag extends string, Payload extends Schema.Codec<{ readonly label: string }, unknown>, Success extends Schema.Codec<unknown, unknown>, Failure extends Schema.Top>(
        payload: Payload['Type'],
        { rpc }: { readonly rpc: Rpc.Rpc<Tag, Payload, Success, Failure> },
    ): Effect.fn.Return<Success['Type'], HostError | Unmatched | Cause.IllegalArgumentError | Schema.SchemaError> {
        const sent = yield* Effect.orDie(Schema.encodeEffect(Sent(rpc))({ tag: rpc._tag, label: payload.label, payload }));
        const literal = sent.replaceAll(/[\u2028\u2029]/gu, (separator) => `\\u${Number(separator.codePointAt(0)).toString(16)}`);
        return yield* Effect.callback<string>((resume) => cep.evalScript(`${ExtensionManifest.global}(${literal})`, flow(Effect.succeed, resume))).pipe(
            Effect.filterOrFail(
                (reply) => reply !== 'EvalScript error.',
                () => HostError.make({ reason: { _tag: 'Unavailable' }, subject: `${payload.label}: ${ExtensionManifest.global}` }),
            ),
            Effect.flatMap(Schema.decodeEffect(Reply)),
            Effect.flatMap(Completion.answered(Schema.toCodecJson(rpc.successSchema), payload.label)),
        );
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const servePanel = <E = never, F = never>({ link, panel }: { readonly link?: (cep: Cep) => Layer.Layer<never, E>; readonly panel?: Panel<F> }): void => {
    Effect.gen(function* () {
        const cep = yield* cepHost;
        yield* link === undefined ? Effect.void : Effect.forkScoped(Layer.launch(link(cep)));
        if (panel !== undefined) {
            yield* Effect.forkScoped(Stream.runForEach(skin, skinned(document.documentElement)));
            const forked = yield* FiberSet.makeRuntime<never, unknown, never>();
            yield* panel(document.body, (work) => {
                forked(Effect.ignore(Effect.tapCause(work, Effect.logError)));
            });
        }
    }).pipe(Layer.effectDiscard, Layer.launch, Effect.provide(Layer.unwrap(Effect.map(cepHost, (cep) => layer({ serviceName: cep.getExtensionId() }))).pipe(Layer.provide(FetchHttpClient.layer))), Effect.tapCause(Effect.logError), Effect.provideService(Logger.LogToStderr, true), Effect.runFork);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { bridge, type Cep, type Panel, servePanel, skin };

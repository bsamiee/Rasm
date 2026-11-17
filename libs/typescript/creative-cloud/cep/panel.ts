import 'core-js/modules/web.url.can-parse.js';
import { Completion, Envelope } from '@rasm/extendscript';
import { Link } from '@rasm/mcp';
import { HostError } from '@rasm/object-model';
import type { Unmatched } from '@rasm/search';
import { layer } from '@rasm/telemetry';
import { wcagContrast } from 'culori';
import { type Cause, Effect, FiberSet, Function, flow, Layer, Logger, Option, Predicate, Record, Schema, type Scope, Stream, Struct } from 'effect';
import { FetchHttpClient } from 'effect/http';
import { ExtensionManifest } from '#/manifest';

// --- [TYPES] ---------------------------------------------------------------------------

type Panel<F> = (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X>) => void) => Effect.Effect<void, F, Scope.Scope>;

interface Cep extends Stream.EventListener<unknown> {
    readonly evalScript: (script: string, callback: (reply: string) => void) => void;
    readonly getSystemPath: (type: string) => string;
    readonly getExtensionId: () => string;
    readonly getHostEnvironment: () => string;
}
interface Engine {
    readonly fs: { readonly readFile: (path: string, encoding: string) => unknown; readonly deleteFile: (path: string) => unknown };
    readonly encoding: { readonly Base64: string };
}

// --- [MODELS] --------------------------------------------------------------------------

const Cep = Schema.Struct(Struct.Record(['evalScript', 'getSystemPath', 'getExtensionId', 'getHostEnvironment', 'addEventListener', 'removeEventListener'], Schema.declare(Predicate.isFunction)));
const Hosted = Schema.Struct({ __adobe_cep__: Schema.declare((host): host is Cep => Schema.is(Cep)(host)) });
const Engine = Schema.Struct({ fs: Schema.Struct(Struct.Record(['readFile', 'deleteFile'], Schema.declare(Predicate.isFunction))), encoding: Schema.Struct({ Base64: Schema.String }) });
const Engined = Schema.Struct({ cep: Schema.declare((engine): engine is Engine => Schema.is(Engine)(engine)) }).annotate({ description: 'File system the CEP engine sets on window' });
const Read = Schema.Struct({ err: Schema.Literal(0), data: Schema.Uint8ArrayFromBase64 }).annotate({ description: 'window.cep.fs.readFile result without error' });
const Deleted = Schema.Struct({ err: Schema.Literal(0) }).annotate({ description: 'window.cep.fs.deleteFile result without error' });
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
const Sent = Function.memoize((request: { readonly name: string; readonly parametersSchema: Schema.Codec<unknown, unknown> }) => Schema.fromJsonString(Schema.toCodecJson(Envelope(request))));

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
const bridge =
    (cep: Cep) =>
    <Payload extends Schema.Codec<{ readonly label: string }, unknown>, Reply extends Schema.Codec<unknown, unknown>>(request: { readonly name: string; readonly parametersSchema: Payload; readonly successSchema: Reply }) =>
    (payload: Payload['Type']): Effect.Effect<Reply['Type'], HostError | Unmatched | Cause.IllegalArgumentError | Schema.SchemaError> =>
        Effect.orDie(Schema.encodeEffect(Sent(request))({ tag: request.name, label: payload.label, payload })).pipe(
            Effect.map((sent) => sent.replaceAll(/[\u2028\u2029]/gu, (separator) => `\\u${Number(separator.codePointAt(0)).toString(16)}`)),
            Effect.flatMap((literal) => Effect.callback<string>((resume) => cep.evalScript(`${ExtensionManifest.global}(${literal})`, flow(Effect.succeed, resume)))),
            Effect.filterOrFail(
                (reply) => reply !== 'EvalScript error.',
                () => HostError.of({ _tag: 'Unavailable' }, `${payload.label}: ${ExtensionManifest.global}`)(),
            ),
            Effect.flatMap(Schema.decodeEffect(Reply)),
            Effect.flatMap(Completion.answered(Schema.toCodecJson(request.successSchema), payload.label)),
        );
const takeImage = Effect.fnUntraced(function* (path: string) {
    const { cep: engine } = yield* Effect.orDie(Effect.suspend(() => Schema.decodeUnknownEffect(Engined)(globalThis)));
    const { data } = yield* Effect.suspend(() => Schema.decodeUnknownEffect(Read)(engine.fs.readFile(path, engine.encoding.Base64))).pipe(
        Effect.mapError(HostError.refused(`Read of ${path}`)),
        Effect.ensuring(
            Effect.ignore(
                Effect.suspend(() => Schema.decodeUnknownEffect(Deleted)(engine.fs.deleteFile(path))),
                { log: 'Warn' },
            ),
        ),
    );
    return yield* Effect.acquireUseRelease(
        Effect.tryPromise({ try: () => createImageBitmap(new Blob([data.slice()])), catch: HostError.refused(`Image decode of ${path}`) }),
        ({ width, height }) => Effect.succeed({ image: data, width, height }),
        (bitmap) => Effect.sync(() => bitmap.close()),
    );
});
const peer = (cep: Cep): Effect.Effect<Link> =>
    Effect.orDie(Effect.all([Schema.decodeEffect(Schema.URLFromString)(cep.getSystemPath('hostApplication')), Schema.decodeEffect(Schema.URLFromString)(cep.getSystemPath('extension'))])).pipe(
        Effect.map(([application, root]) => Link.make({ application, extension: cep.getExtensionId(), root: Option.some(root), runtime: Option.none() })),
    );

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

export { bridge, type Cep, type Panel, peer, servePanel, skin, takeImage };

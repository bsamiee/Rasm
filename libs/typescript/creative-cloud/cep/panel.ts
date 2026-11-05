import { HostError, Requests } from '@rasm/automation';
import { Peer } from '@rasm/automation/peer';
import { inkColor } from '@rasm/mark';
import { wcagContrast } from 'culori';
import { Effect, FiberSet, Function, flow, Layer, Logger, Predicate, Queue, Record, Schema, type Scope, Stream, Struct } from 'effect';
import type { Rpc } from 'effect/rpc';
import { Completion, Envelope } from '#/contract';
import { ExtensionManifest } from '#/extension-manifest';

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

// --- [CONSTANTS] -----------------------------------------------------------------------

const THEME_CHANGED = 'com.adobe.csxs.events.ThemeColorChanged';

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
const Sent = Function.memoize((rpc: Parameters<typeof Envelope>[0]) => Schema.fromJsonString(Schema.toCodecJson(Envelope(rpc))));
const Reply = Schema.fromJsonString(Completion);

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = (cep: Cep) =>
    Effect.fnUntraced(function* <Tag extends Requests['_tag'], Payload extends Requests['payloadSchema'], Success extends Requests['successSchema']>(payload: Payload['Type'], { rpc }: { readonly rpc: Rpc.Rpc<Tag, Payload, Success, Requests['errorSchema']> }) {
        const sent = yield* Effect.orDie(Schema.encodeEffect(Sent(rpc))({ tag: rpc._tag, label: payload.label, payload }));
        return yield* Effect.callback<string>((resume) => cep.evalScript(`${ExtensionManifest.global}(${sent.replaceAll('\u2028', '\\u2028').replaceAll('\u2029', '\\u2029')})`, flow(Effect.succeed, resume))).pipe(
            Effect.filterOrFail(
                (reply) => reply !== 'EvalScript error.',
                () => HostError.make({ reason: { _tag: 'Unavailable' }, subject: `${payload.label}: ${ExtensionManifest.script} ${ExtensionManifest.global}` }),
            ),
            Effect.flatMap(Schema.decodeEffect(Reply)),
            Effect.flatMap(Completion.answered(Schema.toCodecJson(rpc.successSchema), payload.label)),
        );
    });
const cepHost = Effect.suspend(() => Schema.decodeUnknownEffect(Panel)(globalThis)).pipe(Effect.map(Struct.get('__adobe_cep__')));
const css = ({ red, green, blue }: typeof Rgb.Type) => `rgb(${red} ${green} ${blue})`;
const skinned = (root: HTMLElement) => (environment: typeof Environment.Type) => {
    const { baseFontFamily, baseFontSize, panelBackgroundColorSRGB, appBarBackgroundColorSRGB, systemHighlightColor } = environment.appSkinInfo;
    const background = css(panelBackgroundColorSRGB.color);
    const scheme = wcagContrast(background, ExtensionManifest.inks.light.color) > wcagContrast(background, ExtensionManifest.inks.dark.color) ? 'light' : 'dark';
    return Effect.sync(() => {
        Record.toEntries({
            '--cep-host-background-color': background,
            '--cep-host-bar-background-color': css(appBarBackgroundColorSRGB.color),
            '--cep-host-highlight-color': css(systemHighlightColor),
            '--cep-host-text-color': inkColor(ExtensionManifest.inks[scheme]),
            '--cep-host-font-family': baseFontFamily,
            '--cep-host-font-size': `${baseFontSize}px`,
        }).forEach(([property, value]) => root.style.setProperty(property, value));
        Object.assign(root.style, { colorScheme: scheme, background: 'var(--cep-host-background-color)', color: 'var(--cep-host-text-color)', fontFamily: 'var(--cep-host-font-family)', fontSize: 'var(--cep-host-font-size)' });
    });
};
const followed = <E>(cep: Cep, queue: Queue.Queue<string, E>) => {
    const listener = (): void => {
        Queue.offerUnsafe(queue, cep.getHostEnvironment());
    };
    return Effect.acquireRelease(
        Effect.sync(() => {
            cep.addEventListener(THEME_CHANGED, listener);
            listener();
        }),
        Function.constant(Effect.sync(() => cep.removeEventListener(THEME_CHANGED, listener))),
    );
};
const linked = (cep: Cep) => Peer.dial(Requests, Requests.toLayer(Struct.Record(Array.from(Requests.requests.values(), Struct.get('_tag')), served(cep))), Schema.decodeEffect(Schema.URLFromString)(cep.getSystemPath('hostApplication')))(cep.getExtensionId());

// --- [SERVICES] ------------------------------------------------------------------------

const skin = Stream.unwrap(Effect.map(cepHost, (cep) => Stream.callback<string>((queue) => followed(cep, queue)))).pipe(Stream.mapEffect((environment) => Schema.decodeEffect(Environment)(environment)));
const CepPanel = {
    skin,
    serve: <F = never>({ link = false, face }: { readonly link?: boolean; readonly face?: Face<F> }): void => {
        Effect.gen(function* () {
            const cep = yield* cepHost;
            yield* link ? Effect.forkScoped(Layer.launch(linked(cep))) : Effect.void;
            if (face !== undefined) {
                yield* Effect.forkScoped(Stream.runForEach(skin, skinned(document.documentElement)));
                const forked = yield* FiberSet.makeRuntime<never, void, never>();
                yield* face(document.body, (work) => forked(Effect.ignore(Effect.tapCause(Effect.asVoid(work), Effect.logError))));
            }
            return yield* Effect.never;
        }).pipe(Effect.scoped, Effect.tapCause(Effect.logError), Effect.provideService(Logger.LogToStderr, true), Effect.runFork);
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CepPanel };

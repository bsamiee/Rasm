import { dirname, join } from 'node:path';
import process from 'node:process';
import { isSea } from 'node:sea';
import type { Application } from '@rasm/creative-cloud-desktop';
import { readCoreOmv } from '@rasm/extendscript/node';
import { registerResourceTemplate, Server } from '@rasm/mcp';
import { Output } from '@rasm/mcp/cli';
import { Declaration } from '@rasm/object-model';
import { readDocuments, readManifest, readOmv, readTypeScript } from '@rasm/object-model/node';
import { Unmatched } from '@rasm/search';
import { HOSTS } from '@rasm/uxp';
import base from '@rasm/uxp/tsconfig.base.json' with { type: 'json' };
import { Array, Cause, Context, Duration, Effect, Exit, FileSystem, Layer, Option, Order, Path, Record, Schema, Scope, Stream, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpSchema } from 'effect/ai';
import { Installed } from '#/applications';

// --- [TYPES] ---------------------------------------------------------------------------

type Finding = Effect.Success<ReturnType<Effect.Success<ReturnType<Effect.Success<ReturnType<typeof readTypeScript>>>>['check']>>[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const UXP = { types: ['@adobe/cc-ext-uxp-types/uxp'], lib: base.compilerOptions.lib.filter((name) => name !== 'dom') };
const CHECKED = {
    photoshop: { product: HOSTS.PS.product, root: '@types/photoshop', ...UXP, preamble: "declare function require(id: 'photoshop'): typeof import('photoshop');\ndeclare function resolve(specifier: string): unknown;", enumeration: Option.none<string>() },
    premierepro: { product: HOSTS.premierepro.product, root: '@adobe/premierepro', ...UXP, preamble: "declare function require(id: 'premierepro'): import('@adobe/premierepro').premierepro;", enumeration: Option.none<string>() },
    indesign: { product: HOSTS.ID.product, root: 'indesign-uxp-types/flat', ...UXP, types: [...UXP.types, 'indesign-uxp-types'], preamble: "declare function require(id: 'indesign'): typeof import('indesign');", enumeration: Option.some('Enumeration') },
} as const;
const PRODUCTS = Record.fromIterableWith(Record.toEntries(CHECKED), ([host, { product }]) => [product, host] as const);

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.Literals([...Record.keys(CHECKED), 'illustrator', 'aftereffects', 'mediaencoder']).annotate({ description: 'Host module a UXP script requires' });

// --- [SERVICES] ------------------------------------------------------------------------

class Declarations extends Context.Service<Declarations>()('@rasm/atelier/declarations', {
    make: Effect.gen(function* () {
        const [rows, server, { folder: stored }, files, posix, scope] = yield* Effect.all([Installed, Server, Output, FileSystem.FileSystem, Path.Path, Effect.scope]);
        const shared = Effect.provideContext(readCoreOmv(stored('ExtendScript', 'declarations')), yield* Effect.context<Effect.Services<ReturnType<typeof readCoreOmv>>>());
        const opened = yield* Effect.cached(Scope.provide(readTypeScript(Option.liftPredicate({ executable: join(dirname(process.execPath), 'tsc'), library: join(import.meta.dirname, 'typescript') }, isSea), join(import.meta.dirname, 'node_modules')), scope));
        const compiled = yield* Effect.all(Record.map(CHECKED, (row) => Effect.cached(Effect.flatMap(opened, (open) => open(Struct.omit(row, ['product']))))));
        const installed = Effect.gen(function* () {
            const dictionaries = '/Library/Application Support/Adobe/Scripting Dictionaries CC';
            const newest = Array.match(Array.filter(yield* files.readDirectory(dictionaries), String.startsWith('Illustrator')), { onEmpty: Option.none, onNonEmpty: (named) => Option.some(Array.max(named, Order.String)) });
            const folder = yield* Effect.fromOption(newest, () => new Cause.NoSuchElementError(`No Illustrator folder under ${dictionaries}, which the release installer writes`));
            return yield* files.readFileString(join(dictionaries, folder, 'omv.xml'));
        });
        const [whole, partial] = yield* Effect.all([
            Effect.cachedWithTTL(
                Effect.flatMap(Effect.all([installed, shared]), ([text, { javascript, scriptui }]) => readOmv([javascript, scriptui, text])),
                (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero),
            ),
            Effect.cached(Effect.flatMap(installed, (text) => readOmv([text]))),
        ]);
        const illustrator = whole.pipe(
            Effect.tapCause((cause) => Effect.logWarning('Illustrator pages lack the core ExtendScript classes', cause)),
            Effect.catchCause(() => partial),
        );
        yield* Effect.forkScoped(Effect.ignore(illustrator));
        const mediaencoder = Effect.gen(function* () {
            const { url } = yield* Effect.fromOption(
                Array.findFirst(yield* SubscriptionRef.get(rows), (row) => row.product === HOSTS.ame.product),
                () => new Cause.NoSuchElementError(`No installed application with product code ${HOSTS.ame.product}`),
            );
            return yield* readManifest(yield* files.readFileString(yield* posix.fromFileUrl(new URL('Contents/Resources/json/ScriptingAPIManifest.json', url))));
        });
        const paged = { illustrator, ...(yield* Effect.all({ mediaencoder: Effect.cached(mediaencoder), aftereffects: Effect.cached(readDocuments(files.stream(join(import.meta.dirname, 'aftereffects.jsonl')))) })) };
        const resolved: Record.ReadonlyRecord<typeof Host.Type, Effect.Effect<Effect.Success<typeof paged.illustrator>, Effect.Error<(typeof paged)[keyof typeof paged]>>> = { ...Record.map(compiled, Effect.flatMap(Struct.get('catalog'))), ...paged };
        const refused = Effect.fnUntraced(function* (row: Application, line: (at: number) => Option.Option<number>, { host, listed }: { readonly host: keyof typeof CHECKED; readonly listed: Array.NonEmptyReadonlyArray<Finding> }) {
            const { index } = yield* resolved[host];
            const named = Array.intersection(Array.dedupe(Array.getSomes(Array.map(listed, Struct.get('declared')))), [...index.classes, ...index.enumerations]);
            const linked = [...Array.map(named, (name) => ({ uri: server.uri(`object-model/${host}/${name}`), name: `${host} ${name}`, description: `Declaration of ${name}` })), { uri: server.uri(`object-model/${host}`), name: `${host} object model`, description: 'Every class and enumeration the declarations state' }];
            const { add } = yield* Server.ToolCall;
            yield* Effect.forEach(linked, (link) => add(McpSchema.ResourceLink.make({ ...link, mimeType: 'application/json' })), { discard: true });
            return yield* new Cause.IllegalArgumentError(
                [`${row.name} declarations refuse the body before it runs:`, ...Array.map(listed, (each) => Option.match(Option.flatMap(each.line, line), { onNone: () => each.text, onSome: (at) => `line ${at}: ${each.text}` })), `Read ${Array.map(linked, Struct.get('uri')).join(', ')}`].join('\n'),
            );
        });
        const check = Effect.fnUntraced(function* (row: Application, program: string, line: (at: number) => Option.Option<number>) {
            const host = Record.get(PRODUCTS, row.product);
            const found = yield* Effect.transposeOption(Option.map(host, (key) => compiled[key]));
            const findings = yield* Effect.transposeOption(Option.map(found, (source) => source.check(program)));
            return yield* Option.match(Option.all({ host, listed: Option.flatMap(findings, Option.liftPredicate(Array.isReadonlyArrayNonEmpty)) }), { onNone: () => Effect.void, onSome: (refusal) => refused(row, line, refusal) });
        });
        return { index: (host: typeof Host.Type) => Effect.map(resolved[host], Struct.get('index')), page: (host: typeof Host.Type, name: string) => Effect.flatMap(resolved[host], (held) => held.page(name)), check };
    }),
}) {
    static readonly layer = Layer.effect(Declarations, Declarations.make);
    static readonly instruction = `Read a host object model before scripting it: atelier://object-model/{host} lists its classes and enumerations, atelier://object-model/{host}/{name} holds one with its members, for host ${Host.literals.join(', ')}. A refused script links the declaration it broke.`;
    static readonly checked = Record.keys(CHECKED);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const declarations = Effect.gen(function* () {
    const { index, page } = yield* Declarations;
    const names = (input: string, host: typeof Host.Type | undefined) =>
        UndefinedOr.match(host, {
            onUndefined: () => Effect.succeed(Array.empty<string>()),
            onDefined: (given) => Effect.map(index(given), ({ classes, enumerations }) => [...Unmatched.rank(input, [...classes, ...enumerations])]),
        });
    yield* registerResourceTemplate`object-model/${McpSchema.param('host', Host)}`({
        name: 'object-model',
        description: 'Class and enumeration names one host declares, read from its vendor declarations, installed omv.xml with the core classes of the ExtendScript Toolkit, scripting API manifest (Media Encoder lists internal classes too, as only its binary names the public set), or UXP reference pages',
        schema: Declaration,
        changes: Stream.empty,
        completion: { host: () => Effect.succeed([...Host.literals]) },
        content: (_, host) => index(host),
    });
    yield* registerResourceTemplate`object-model/${McpSchema.param('host', Host)}/${McpSchema.param('name', Schema.String)}`({
        name: 'object-model-declaration',
        description: 'One class with its fields and methods or one enumeration with its members, as its declared source states them, name in any case',
        schema: Declaration,
        changes: Stream.empty,
        completion: { host: () => Effect.succeed([...Host.literals]), name: (input, { host }) => names(input, host) },
        content: (_, host, name) => page(host, name),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { declarations, Declarations };

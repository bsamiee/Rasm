import { SourceMap } from 'node:module';
import { HostError } from '@rasm/object-model/model';
import { run } from '@rasm/process';
import { type EnvConfig, transformSync } from '@swc/wasm';
import { parse, type Token, tokTypes } from 'acorn';
import { simple } from 'acorn-walk';
import { Array, Cause, Effect, FileSystem, Function, Option, Order, Path, Record, Result, Schema, Stream, Struct } from 'effect';
import { Digest } from 'effect/crypto';
import { Zip } from 'effect/encoding';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CORE = {
    image: 'https://raw.githubusercontent.com/Adobe-CEP/CEP-Resources/b748e7cc41f9e682da4cb4eb8b37b2eca2281486/ExtendScript-Toolkit/AdobeExtendScriptToolkit_4_LS22.dmg',
    payload: 'payloads/AdobeExtendScriptToolkit_4_LS22/AdobeExtendScriptToolkit_4_LS22.zip',
    members: {
        javascript: { path: 'Assets2/xplat/CommonOMV/javascript.xml', sha256: 'fe1dbf4641b8caafe332915cdebf454e165c030d7529692629138a807596cab7' },
        scriptui: { path: 'Assets2/xplat/CommonOMV/scriptui.xml', sha256: '468e4dbbf9a5ef84ea0695d607d8771ea3706a64f1d9d42b46f25102cd0f85ec' },
    },
} as const;
const ASSUMED = { assumptions: { iterableIsArray: true } };
const RESERVED =
    /^(?:abstract|boolean|break|byte|case|catch|char|class|const|continue|debugger|default|delete|do|double|else|enum|export|extends|false|final|finally|float|for|function|goto|if|implements|import|in|instanceof|int|interface|long|native|new|null|package|private|protected|public|return|short|static|super|switch|synchronized|this|throw|throws|transient|true|try|typeof|var|void|volatile|while|with)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Diagnostic = Schema.TemplateLiteralParser(['  x ', Schema.String, '\n   ,-[', Schema.Int, ':', Schema.Int, ']', Schema.String]).annotate({ description: 'String @swc/wasm throws on a transform failure' });
const Mapping = Schema.fromJsonString(
    Schema.Struct({
        version: Schema.Int,
        sources: Schema.String.pipe(Schema.Array, Schema.mutable),
        sourcesContent: Schema.String.pipe(Schema.Array, Schema.mutable, Schema.withDecodingDefaultKey(Effect.succeed([]))),
        names: Schema.String.pipe(Schema.Array, Schema.mutable),
        mappings: Schema.String,
        file: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
        sourceRoot: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    }),
);
const Entry = Schema.Struct({ generatedLine: Schema.Int, originalLine: Schema.Int });

// --- [OPERATIONS] ----------------------------------------------------------------------

const toExtendScript = (code: string): string => {
    const tokens: Token[] = [];
    const tree = parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true, onToken: tokens });
    const wraps: (readonly [number, number, string, number, string])[] = Array.filterMap(Array.window(tokens, 2), ([dot, member]) => (dot.type === tokTypes.dot && RESERVED.test(code.slice(member.start, member.end)) ? Result.succeed([dot.start, member.start, '["', member.end, '"]'] as const) : Result.failVoid));
    simple(tree, {
        LogicalExpression: ({ operator, right }) => {
            if (operator === '||' && right.type === 'LogicalExpression' && right.operator === '&&') {
                wraps.push([right.start, right.start, '(', right.end, ')']);
            }
        },
        ConditionalExpression: ({ consequent, alternate }) => {
            wraps.push(...Array.filterMap([consequent, alternate], ({ type, start, end }) => (type === 'ConditionalExpression' ? Result.succeed([start, start, '(', end, ')'] as const) : Result.failVoid)));
        },
        Property: ({ key, computed }) => {
            if (!computed && key.type === 'Identifier' && RESERVED.test(key.name)) {
                wraps.push([key.start, key.start, '"', key.end, '"']);
            }
        },
    });
    const edits = Array.flatMap(wraps, ([from, start, open, end, close]) => [
        { at: from, end: start, text: open, key: [from, true, from - end] as const },
        { at: end, end, text: close, key: [end, false, end - from] as const },
    ]);
    const [rest, pieces] = Array.mapAccum(Array.sortWith(edits, Struct.get('key'), Order.Tuple([Order.Number, Order.Boolean, Order.Number])), 0, (from, { at, end, text }) => [end, `${code.slice(from, at)}${text}`]);
    return `${pieces.join('')}${code.slice(rest)}`;
};
const lower = Object.assign(
    (code: string, { rewrite = Function.identity, ...env }: EnvConfig & { readonly rewrite?: (code: string) => string }, syntax: 'typescript' | 'ecmascript' = 'ecmascript') =>
        Effect.try({
            try: () => Struct.evolve(transformSync(code, { isModule: false, env, jsc: { parser: { syntax }, ...ASSUMED }, sourceMaps: true }), { code: rewrite }),
            catch: (cause) => Option.match(Schema.decodeUnknownOption(Diagnostic)(cause), { onNone: () => HostError.refused(`${syntax} transform`), onSome: ([, text, , line]) => HostError.thrown(text, Option.some(line)) })(cause),
        }),
    { extendscript: { targets: { ie: '6' }, exclude: ['transform-typeof-symbol'], rewrite: toExtendScript } },
);
const originalLine = (map: string | undefined) =>
    Effect.map(Effect.orDie(Schema.decodeUnknownEffect(Mapping)(map)), (mapping) => {
        const source = new SourceMap(mapping);
        return (line: number) =>
            Schema.decodeUnknownOption(Entry)(source.findEntry(line - 1, Number.MAX_SAFE_INTEGER)).pipe(
                Option.filter((entry) => entry.generatedLine === line - 1),
                Option.map((entry) => entry.originalLine + 1),
            );
    });
const readCoreOmv = Effect.fn('readCoreOmv')(function* (folder: string) {
    const [fs, path, digest] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Digest.Digest]);
    const files = Record.map(CORE.members, (member) => path.join(folder, path.basename(member.path)));
    const verify = Effect.fnUntraced(function* <E, R>(member: (typeof CORE.members)[keyof typeof CORE.members], read: Effect.Effect<Uint8Array, E, R>) {
        const bytes = yield* read;
        const received = yield* digest.digestHex(Digest.sha256, bytes);
        return received === member.sha256 ? bytes : yield* new Cause.UnknownError(received, `${member.path} hashes to sha256 ${received} in place of ${member.sha256}`);
    });
    const fetched = Effect.gen(function* () {
        const mount = path.join(yield* fs.makeTempDirectoryScoped(), path.basename(CORE.image));
        yield* Effect.acquireRelease(run(['diskutil', 'image', 'attach', '--nobrowse', '--mountPoint', mount, CORE.image]), () => Effect.orDie(run(['diskutil', 'eject', mount])));
        const paths = new Set<string>(Record.collect(CORE.members, (_, member) => member.path));
        const held = yield* fs.stream(path.join(mount, CORE.payload)).pipe(
            Stream.pipeThroughChannel(Zip.decode()),
            Stream.filter(({ header }) => paths.has(header.path)),
            Stream.mapEffect(
                Effect.fnUntraced(function* ({ header, body }) {
                    return [header.path, yield* Stream.mkUint8Array(body)] as const;
                }),
            ),
            Stream.take(paths.size),
            Stream.runCollect,
            Effect.map(Record.fromEntries),
        );
        const bytes = yield* Effect.all(Record.map(CORE.members, (member) => verify(member, Effect.fromOption(Record.get(held, member.path)))));
        yield* fs.makeDirectory(folder, { recursive: true });
        yield* Effect.forEach(Record.toEntries(bytes), ([key, each]) => fs.writeFile(files[key], each), { discard: true });
        return bytes;
    }).pipe(Effect.scoped);
    const bytes = yield* Effect.catch(Effect.all(Record.map(CORE.members, (member, key) => verify(member, fs.readFile(files[key])))), () => fetched);
    return Record.map(bytes, (each) => new TextDecoder().decode(each));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { lower, originalLine, readCoreOmv };

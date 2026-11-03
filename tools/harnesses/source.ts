import { Array, Effect, FileSystem, Option, Path, type PlatformError, String } from 'effect';
import type { Call } from '#/cases';
import { HarnessError } from '#/error';

// --- [TYPES] ---------------------------------------------------------------------------

interface Mapped {
    readonly file: string;
    readonly line: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const JSXBIN = /(?<quote>["'])(?<literal>@JSXBIN@(?:\\.|(?!\k<quote>)[^\\])*)\k<quote>/su;
const BYTE_ORDER_MARK = /^\u{FEFF}/u;
const CONTINUATION = /\\\r?\n/gu;
const ESCAPE = /\\(?<character>.)/gu;
const INCLUDE = /^[ \t]*(?:\/\/[ \t]*@|#)include[ \t]+["'](?<name>[^"']+)["'][ \t;]*$/u;
const INCLUDE_PATH = /^[ \t]*(?:\/\/[ \t]*@|#)includepath[ \t]+["'](?<paths>[^"']*)["']/mu;
const DIRECTIVE = /^[ \t]*#(?:target|targetengine|strict|script)\b/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const prepared = Effect.fn('prepared')(function* (script: string, lines: Option.Option<NonNullable<typeof Call.Type.lines>>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = (yield* fs.readFileString(script)).replace(BYTE_ORDER_MARK, '');
    const binary = Option.map(Option.fromNullishOr(JSXBIN.exec(text)?.groups?.literal), (literal) => literal.replace(CONTINUATION, '').replace(ESCAPE, '$<character>'));
    const located = Effect.fnUntraced(function* (file: string, searched: readonly string[], name: string, chain: readonly string[]) {
        const found = yield* Effect.findFirst(
            searched.map((entry): string => path.resolve(entry, name)),
            (candidate: string) => fs.exists(candidate),
        );
        const included = yield* Effect.fromOption(found, () => HarnessError.make({ reason: { _tag: 'Unavailable', because: `include ${name} found in none of ${searched.join(', ')}` }, subject: file }));
        return chain.includes(included) ? yield* HarnessError.make({ reason: { _tag: 'Unavailable', because: `include cycle ${[...chain, included].join(' > ')}` }, subject: file }) : included;
    });
    const expanded: (file: string, body: string, chain: readonly string[]) => Effect.Effect<readonly (readonly [string, Mapped])[], HarnessError | PlatformError.PlatformError> = Effect.fnUntraced(function* (file: string, body: string, chain: readonly string[]) {
        const folder = path.dirname(file);
        const searched = [
            folder,
            ...(INCLUDE_PATH.exec(body)?.groups?.paths ?? '')
                .split(';')
                .filter((entry) => entry !== '')
                .map((entry) => path.resolve(folder, entry)),
        ];
        const nested = (included: string) => Effect.flatMap(fs.readFileString(included), (source) => expanded(included, source.replace(BYTE_ORDER_MARK, ''), [...chain, included]));
        const lined = (source: string, index: number) =>
            Option.match(Option.fromNullishOr(INCLUDE.exec(source)?.groups?.name), {
                onNone: () => Effect.succeed([[DIRECTIVE.test(source) ? '' : source, { file, line: index + 1 }] as const]),
                onSome: (name) => Effect.flatMap(located(file, searched, name, chain), nested),
            });
        return (yield* Effect.forEach(Array.fromIterable(String.linesIterator(body)), lined)).flat();
    });
    const rows = Option.isSome(binary) ? [[binary.value, { file: script, line: 1 }] as const] : yield* expanded(script, text, [script]);
    const kept = Option.match(lines, { onNone: () => rows, onSome: (ranges) => ranges.flatMap(([from, to]) => rows.slice(from - 1, to)) });
    return { source: { script, code: kept.map(([line]) => line).join('\n') }, map: kept.map(([, at]) => at), binary: Option.isSome(binary) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Mapped, prepared };

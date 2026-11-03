import { Array, Effect, FileSystem, Match, Option, Path, type PlatformError, Record, Result, String } from 'effect';
import type { Call } from '#/cases';
import { HarnessError } from '#/error';

// --- [TYPES] ---------------------------------------------------------------------------

interface Mapped {
    readonly file: string;
    readonly line: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENDSCRIPT = ['jsx', 'jsxbin', 'js'] as const;
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
    const located = (file: string, searched: readonly string[], name: string, chain: readonly string[]) =>
        Effect.findFirst(
            searched.map((entry) => path.resolve(entry, name)),
            fs.exists,
        ).pipe(
            Effect.flatMap(Option.match({ onNone: () => HarnessError.unavailable(file)(`include ${name} found in none of ${searched.join(', ')}`), onSome: Effect.succeed })),
            Effect.filterOrElse(
                (included) => !chain.includes(included),
                (included) => HarnessError.unavailable(file)(`include cycle ${[...chain, included].join(' > ')}`),
            ),
        );
    const expanded = (file: string, body: string, chain: readonly string[]): Effect.Effect<readonly (readonly [string, Mapped])[], HarnessError | PlatformError.PlatformError> => {
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
        return Effect.map(Effect.forEach(String.linesIterator(body), lined), Array.flatten);
    };
    const rows = yield* Option.match(binary, { onNone: () => expanded(script, text, [script]), onSome: (literal) => Effect.succeed([[literal, { file: script, line: 1 }] as const]) });
    const kept = Option.match(lines, { onNone: () => rows, onSome: (ranges) => ranges.flatMap(([from, to]) => rows.slice(from - 1, to)) });
    return { source: { script, code: kept.map(([line]) => line).join('\n') }, map: kept.map(([, at]) => at), binary: Option.isSome(binary) };
});
const engineOf =
    <E extends string>(engines: { readonly [K in E]: { readonly extensions: readonly string[] } }, fallback: NoInfer<E>) =>
    (scripts: readonly string[]): Result.Result<E, HarnessError> => {
        const held = (script: string) =>
            Option.map(
                Record.findFirst(engines, ({ extensions }) => extensions.some((extension) => script.endsWith(`.${extension}`))),
                ([engine]) => engine,
            );
        const unavailable = HarnessError.unavailable(scripts.join(', '));
        return Match.value({ unknown: scripts.filter((script) => Option.isNone(held(script))), found: Array.dedupe(Array.getSomes(scripts.map(held))) }).pipe(
            Match.when({ unknown: (listed) => listed.length > 0 }, ({ unknown }) => Result.fail(unavailable(`${unknown.join(', ')} holds no extension of ${Record.collect(engines, (engine, { extensions }) => `${engine} ${extensions.join(' ')}`).join(', ')}`))),
            Match.when({ found: (listed) => listed.length > 1 }, ({ found }) => Result.fail(unavailable(`cases mix engines ${found.join(', ')}`))),
            Match.orElse(({ found }) => Result.succeed(Option.getOrElse(Array.head(found), () => fallback))),
        );
    };
const assembled = Effect.fn('assembled')(function* (engine: URL) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const paths = yield* Effect.forEach([new URL('extendscript.jsxinc', import.meta.url), engine], (url) => path.fromFileUrl(url));
    const texts = yield* Effect.forEach(paths, (file) => fs.readFileString(file));
    const numbered = (file: string, text: string) => text.split('\n').map((_, index): Mapped => ({ file, line: index + 1 }));
    return { code: texts.join('\n'), map: Array.zipWith(paths, texts, numbered).flat() };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { assembled, EXTENDSCRIPT, engineOf, type Mapped, prepared };

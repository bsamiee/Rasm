import { SourceMap } from 'node:module';
import { HostError } from '@rasm/automation';
import { type JscTarget, transformSync } from '@swc/wasm';
import { parse } from 'acorn';
import { simple } from 'acorn-walk';
import { Array, Effect, Option, Order, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASSUMED = { assumptions: { iterableIsArray: true } };

// --- [MODELS] --------------------------------------------------------------------------

const Diagnostic = Schema.TemplateLiteralParser(['  x ', Schema.String, '\n   ,-[', Schema.Int, ':', Schema.Int, ']', Schema.String]).annotate({ description: 'String @swc/wasm throws on a transform failure' });
const Mapping = Schema.fromJsonString(
    Schema.Struct({
        version: Schema.Int,
        sources: Schema.mutable(Schema.Array(Schema.String)),
        sourcesContent: Schema.mutable(Schema.Array(Schema.String)).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        names: Schema.mutable(Schema.Array(Schema.String)),
        mappings: Schema.String,
        file: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
        sourceRoot: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    }),
);
const Entry = Schema.Struct({ originalSource: Schema.String, originalLine: Schema.Int });

// --- [OPERATIONS] ----------------------------------------------------------------------

const grouped = (code: string): string => {
    const marks: (readonly [number, string])[] = [];
    simple(parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true }), {
        LogicalExpression: ({ operator, right }) => {
            if (operator === '||' && right.type === 'LogicalExpression' && right.operator === '&&') {
                marks.push([right.start, '('], [right.end, ')']);
            }
        },
    });
    return Array.sort(
        marks,
        Order.mapInput(Order.flip(Order.Number), ([at]: readonly [number, string]) => at),
    ).reduce((text, [at, mark]) => `${text.slice(0, at)}${mark}${text.slice(at)}`, code);
};
const lower = (code: string, target: JscTarget, syntax: 'typescript' | 'ecmascript' = 'ecmascript') =>
    Effect.try({
        try: () => {
            const lowered = transformSync(code, { isModule: false, jsc: { target, parser: { syntax }, ...ASSUMED }, sourceMaps: true });
            return target === 'es3' ? { ...lowered, code: grouped(lowered.code) } : lowered;
        },
        catch: (cause) =>
            Option.match(Schema.decodeUnknownOption(Diagnostic)(cause), {
                onNone: () => HostError.refused(`${syntax} transform to ${target}`)(cause),
                onSome: ([, text, , line]) => HostError.thrown(text, Option.some(line))(cause),
            }),
    });
const traced = (map: string | undefined) =>
    Effect.map(Effect.orDie(Schema.decodeUnknownEffect(Mapping)(map)), (mapping) => {
        const source = new SourceMap(mapping);
        return (line: number) => Option.map(Schema.decodeUnknownOption(Entry)(source.findEntry(line - 1, Number.MAX_SAFE_INTEGER)), ({ originalSource, originalLine }) => ({ file: originalSource, line: originalLine + 1 }));
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { lower, traced };

import { SourceMap } from 'node:module';
import { HostError } from '@rasm/automation';
import { type JscTarget, transformSync } from '@swc/wasm';
import { parse } from 'acorn';
import { simple } from 'acorn-walk';
import { Array, Effect, Option, Order, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASSUMED = { assumptions: { iterableIsArray: true } };
const RESERVED =
    /^(?:abstract|boolean|break|byte|case|catch|char|class|const|continue|debugger|default|delete|do|double|else|enum|export|extends|false|final|finally|float|for|function|goto|if|implements|import|in|instanceof|int|interface|long|native|new|null|package|private|protected|public|return|short|static|super|switch|synchronized|this|throw|throws|transient|true|try|typeof|var|void|volatile|while|with)$/u;

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
    const marks: (readonly [number, number, string])[] = [];
    simple(parse(code, { ecmaVersion: 'latest', allowReturnOutsideFunction: true }), {
        LogicalExpression: ({ operator, right }) => {
            if (operator === '||' && right.type === 'LogicalExpression' && right.operator === '&&') {
                marks.push([right.start, right.start, '('], [right.end, right.end, ')']);
            }
        },
        ConditionalExpression: ({ consequent, alternate }) => {
            marks.push(
                ...[consequent, alternate]
                    .filter(({ type }) => type === 'ConditionalExpression')
                    .flatMap(({ start, end }): (readonly [number, number, string])[] => [
                        [start, start, '('],
                        [end, end, ')'],
                    ]),
            );
        },
        Property: ({ key, computed }) => {
            if (!computed && key.type === 'Identifier' && RESERVED.test(key.name)) {
                marks.push([key.start, key.start, '"'], [key.end, key.end, '"']);
            }
        },
        MemberExpression: ({ property, computed }) => {
            if (!computed && property.type === 'Identifier' && RESERVED.test(property.name)) {
                marks.push([code.lastIndexOf('.', property.start), property.start, '["'], [property.end, property.end, '"]']);
            }
        },
    });
    return Array.sort(
        marks,
        Order.mapInput(Order.flip(Order.Number), ([at]: readonly [number, number, string]) => at),
    ).reduce((text, [start, end, mark]) => `${text.slice(0, start)}${mark}${text.slice(end)}`, code);
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

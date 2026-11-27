import type { Act } from '@rasm/applications/server/act';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Context, Effect, Formatter, Option, Predicate, Result, Schema, String, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { current, ends } from '#/handle/target';

// --- [MODELS] --------------------------------------------------------------------------

const Reply = Schema.toCodecJson(Schema.Json);
const tool = Tool.make('execute_script', {
    description: 'Runs code in the target and returns the value of the script. A dialog, hang, or exit of the target returns the call at once, its outcome following as a finished notice',
    parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Source the script runtime of the target runs' }) }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotateMerge(Context.make(Tool.OpenWorld, false).pipe(Context.add(Tool.Strict, true)));

// --- [SERVICES] ------------------------------------------------------------------------

const Script = Context.Reference<{
    readonly run: (aim: Effect.Success<typeof current>, code: string) => Effect.Effect<{ readonly result: Option.Option<Schema.Json>; readonly written?: typeof Act.Written.Type }, Error, ApplicationServer>;
}>('@rasm/application-tools/execute-script/Script', {
    defaultValue: () => ({
        run: Effect.fnUntraced(function* ({ row, live }, code) {
            const server = yield* ApplicationServer;
            const { script, command } = yield* server.dictionary({ url: row.url });
            yield* Effect.fromOption(script, () => Unmatched.of({ subject: `script command in the dictionary of ${row.name}`, candidates: command.map(Struct.get('name')) }));
            const { reply, written } = yield* server.execute({ instance: live.instance, arguments: { direct: code } });
            return { result: Option.some(yield* Effect.orDie(Schema.encodeEffect(Reply)(reply))), written };
        }),
    }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const ExecuteScript = {
    tool,
    clause: 'the code runs through the script command the scripting dictionary of the target declares, such as do JavaScript, and a target declaring none refuses it with the commands it declares',
    handle: Effect.fnUntraced(function* ({ code }: Tool.Parameters<typeof tool>) {
        const aim = yield* current;
        const [{ run }, { returnEarly }] = yield* Effect.all([Script, ToolCall]);
        yield* returnEarly(yield* ends);
        const { result, written } = yield* run(aim, code);
        const text = result.pipe(
            Option.filter(Predicate.isNotNull),
            Option.map((value) => (Predicate.isString(value) ? value : Formatter.format(value))),
            Option.filter(String.isNonEmpty),
            Option.map(String.split('\n')),
        );
        return new Report({
            lines: [
                ...Option.toArray(text).map((lines) => (Predicate.isTupleOf(lines, 1) ? new Line({ head: ['result'], value: Format.quoted(lines[0]) }) : new Line({ head: ['result:'], rows: lines.map((line) => new Line({ head: [line] })) }))),
                ...Array.fromNullishOr(written).flatMap(
                    Result.match({
                        onFailure: ({ message }) => [new Line({ head: ['written'], value: 'unread', facts: [message] })],
                        onSuccess: Array.match({ onEmpty: Array.empty, onNonEmpty: (urls) => [new Line({ head: ['written'], value: Format.list(urls.map((url) => Format.quoted(Format.path(url)))) })] }),
                    }),
                ),
            ],
        });
    }, Effect.scoped),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExecuteScript, Reply, Script };

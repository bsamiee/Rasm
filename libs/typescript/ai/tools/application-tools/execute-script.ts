import type { Act } from '@rasm/applications/server/act';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Handle } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Format, Report } from '@rasm/report';
import { Unmatched } from '@rasm/search/unmatched';
import { Context, Effect, Formatter, Option, Predicate, Result, Schema, Struct, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Target } from '#/handle/target';

// --- [MODELS] --------------------------------------------------------------------------

const tool = Tool.make('execute_script', {
    description: 'Runs code in the target and returns the value of the script. A dialog, hang, or exit of the target returns the call at once, its outcome following as a finished notice',
    parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Source the script runtime of the target runs' }) }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotate(Tool.OpenWorld, false);

// --- [SERVICES] ------------------------------------------------------------------------

const Script = Context.Reference<{
    readonly description: string;
    readonly run: (aim: Effect.Success<typeof Target.current>, code: string) => Effect.Effect<{ readonly result: Option.Option<Schema.Json>; readonly written?: typeof Act.Written.Type }, Error, ApplicationServer>;
}>('@rasm/application-tools/execute-script/Script', {
    defaultValue: () => ({
        description: 'the code runs through the script command the scripting dictionary of the target declares, such as do JavaScript, and a target declaring none refuses it with the commands it declares',
        run: Effect.fnUntraced(function* ({ row, live }, code) {
            const server = yield* ApplicationServer;
            const { script, command } = yield* server.dictionary({ url: row.url });
            yield* Effect.fromOption(script, () => Unmatched.of({ subject: `script command in the dictionary of ${row.name}`, candidates: command.map(Struct.get('name')) }));
            const { reply, written } = yield* server.execute({ instance: live.instance, arguments: { direct: code } });
            return { result: Option.some(reply), written };
        }),
    }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const ExecuteScript = {
    tool,
    Script,
    run: Effect.fnUntraced(function* ({ code }: Tool.Parameters<typeof tool>) {
        const aim = yield* Target.current;
        const [script, { returnEarly }] = yield* Effect.all([Script, ToolCall]);
        yield* returnEarly(yield* Target.ends);
        const { result, written } = yield* script.run(aim, code);
        return Report.of({
            result: result.pipe(
                Option.filter(Predicate.isNotNull),
                Option.map((value) => (Predicate.isString(value) ? value : Formatter.format(value))),
                Option.getOrUndefined,
            ),
            written: UndefinedOr.map(written, (read) => Result.match(read, { onFailure: ({ message }) => message, onSuccess: (urls) => urls.map(Format.path) })),
        });
    }, Effect.scoped),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExecuteScript };

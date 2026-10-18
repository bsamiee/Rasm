import { Attachment, Instructions, unit, WRITING } from '@rasm/mcp/link';
import { register } from '@rasm/mcp/server';
import { Effect, FileSystem, Option, Path, Predicate, Record, Schema } from 'effect';
import { Tool } from 'effect/ai';
import type { applications, installed } from './applications.ts';
import { Scripting, ScriptingError } from './scripting.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const execute = Effect.fn('execute')(function* (application: Effect.Success<typeof installed>['application'], target: Effect.Success<ReturnType<typeof applications>>) {
    const [sender, files, path] = yield* Effect.all([Scripting, FileSystem.FileSystem, Path.Path]);
    const steps: Record.ReadonlyRecord<string, { readonly command: string; readonly arguments: Schema.JsonObject }> = {
        'com.adobe.InDesign': {
            command: 'do script',
            arguments: {
                language: 'javascript',
                direct: `(function (given) {
        var source = given[0], name = given[1], preview = given[2], rest = [], opened = {}, out, done = false, at;
        for (at = 3; at < given.length; at++) rest.push(given[at]);
        for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
        try {
            app.doScript(function () { out = (function () { return eval(source); }).apply(null, rest); done = true; if (preview) throw new Error(name); }, ScriptLanguage.JAVASCRIPT, [], UndoModes.ENTIRE_SCRIPT, name);
        } catch (error) {
            if (!(done && preview)) throw error;
        }
        for (at = app.documents.length - 1; preview && at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.NO);
        return out;
    })(arguments)`,
            },
        },
        'com.adobe.Photoshop': {
            command: 'do javascript',
            arguments: {
                direct: `(function (given) {
        var source = given[0], name = given[1], preview = given[2], rest = [], opened = {}, document = app.documents.length ? app.activeDocument : null, state = document ? document.activeHistoryState : null, at;
        if (preview && !document) throw new Error('Photoshop records history per document and none is open, open one to preview');
        for (at = 3; at < given.length; at++) rest.push(given[at]);
        for (at = 0; at < app.documents.length; at++) opened[app.documents[at].id] = true;
        var undone = function () {
            for (at = app.documents.length - 1; at >= 0; at--) if (!opened[app.documents[at].id]) app.documents[at].close(SaveOptions.DONOTSAVECHANGES);
            if (document) { app.activeDocument = document; document.activeHistoryState = state; }
        };
        $.global.atelier = function () { $.global.atelier.out = (function () { return eval(source); }).apply(null, rest); };
        try {
            if (document) document.suspendHistory(name, 'atelier()'); else $.global.atelier();
        } catch (error) {
            undone();
            throw error;
        }
        if (preview) undone();
        return $.global.atelier.out;
    })(arguments)`,
            },
        },
    };
    const attached = Effect.fnUntraced(function* (url: URL) {
        const [mimeType, data] = yield* Effect.all([sender.contentType({ url }), Effect.flatMap(path.fromFileUrl(url), files.readFile)], { concurrency: 'unbounded' });
        return new Attachment({ uri: url.href, mimeType: Option.getOrElse(mimeType, () => 'application/octet-stream'), data });
    });
    const Command = Schema.TaggedStruct('Command', {
        command: Schema.String.annotate({ description: 'Command name from the application dictionary' }),
        arguments: Schema.JsonObject.annotate({ description: 'Dictionary parameters by name in the JSON form replies use, direct for the direct parameter. Reply values go into later requests unchanged' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        artifacts: Schema.Array(Schema.URLFromString)
            .annotate({ description: 'File URLs the command writes, returned with the reply' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
    }).annotate({ description: 'Scripting dictionary command sent as an Apple Event' });
    yield* register(
        Tool.make('execute', {
            description: "Runs a scripting dictionary command, or a form an application's extension adds, in one application",
            parameters: Schema.Struct({ application, ...unit(Schema.Never).fields, run: Command }),
            success: Schema.Struct({ reply: Schema.Json, artifacts: Schema.Array(Attachment.schema), unread: Schema.Array(Schema.String).annotate({ description: 'Why each artifact the reply does not carry could not be read' }) }),
        })
            .annotateMerge(WRITING)
            .annotate(Instructions, [
                "- execute: runs a scripting dictionary command, or a form an application's extension adds, in one application. preview undoes it where the application records an undo step. A call failing because the Apple Events sender stopped may have run, read the application's state before repeating it.",
            ]),
        Effect.fn('execute')(function* ({ application: given, description, preview, run: { command, arguments: parameters, artifacts } }) {
            const chosen = yield* target(given);
            const step = Option.filter(Record.get(steps, chosen.identifier), (row) => row.command === command && Predicate.isString(parameters.direct) && URL.parse(parameters.direct)?.protocol !== 'file:' && (parameters.language ?? row.arguments.language) === row.arguments.language);
            const sent = yield* Option.match(step, {
                onSome: (row) => Effect.succeed({ ...parameters, ...row.arguments, 'with arguments': [parameters.direct ?? null, description, Option.isSome(preview), ...(globalThis.Array.isArray(parameters['with arguments']) ? parameters['with arguments'] : [])] }),
                onNone: () => (Option.isSome(preview) ? Effect.fail(ScriptingError.make({ reason: { _tag: 'Unpreviewable', application: chosen.name } })) : Effect.succeed(parameters)),
            });
            const reply = yield* sender.execute({ application: chosen.url, command, arguments: sent });
            const [read, failures] = yield* Effect.partition(artifacts, attached, { concurrency: 'unbounded' });
            return { reply, artifacts: read, unread: failures.map(({ message }) => message) };
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };

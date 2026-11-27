import { basename } from 'node:path';
import { pathToFileURL } from 'node:url';
import { Entry } from '@rasm/applications/catalog';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Output } from '@rasm/mcp/cli';
import { Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { Array, Context, DateTime, Effect, Formatter, Option, Order, Predicate, Record, Result, Schema, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Outline } from '#/handle/outline';
import { Target } from '#/handle/target';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT = 50;
const BY_VALUE = Order.mapInput(Order.Number, Number.parseFloat);

// --- [MODELS] --------------------------------------------------------------------------

const LogRecord = Schema.fromJsonString(Schema.Struct({ timestamp: Schema.DateTimeUtcFromString, severity_text: Schema.String, scope: Schema.optionalKey(Schema.String), body: Schema.String, attributes: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json)) }));
const tool = Tool.make('diagnostics', {
    description: 'Reads diagnostics newest first: without application, the log lines of the server; with one, its crash, hang, and diagnostic reports, the samples taken while it did not respond, and the errors, faults, and lines of its own in its unified log',
    parameters: Schema.Struct({
        application: Schema.optionalKey(Schema.String.annotate({ description: 'Application as applications lists it, the server itself when absent' })),
        since: ApplicationServer.Diagnostics.fields.since.annotate({ description: 'Seconds back from now, else since the instance launched' }),
        limit: ApplicationServer.Diagnostics.fields.limit.annotate({ description: 'Most reports, log lines, and unified log events returned', default: LIMIT }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(LIMIT))),
        find: ApplicationServer.Diagnostics.fields.find.annotate({ description: 'Text a log line or unified log event holds, ignoring case, searched before the limit applies' }),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotate(Tool.Readonly, true);

// --- [SERVICES] ------------------------------------------------------------------------

const Logs = Context.Reference<(row: Entry) => Effect.Effect<(typeof ApplicationServer.Diagnostics.Type)['folders'], Error>>('@rasm/application-tools/diagnostics/Logs', { defaultValue: () => () => Effect.succeed([]) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const coalesce = (entries: readonly { readonly at: Option.Option<DateTime.Utc>; readonly text: string }[]): readonly Line[] => {
    const split = entries.map(({ at, text }) => ({ at, pieces: String.split(text, /(?<numeral>\p{Nd}+(?:\.\p{Nd}+)?)/u) }));
    const alike = Array.groupBy(split, ({ pieces }) => pieces.filter((_, index) => index % 2 === 0).join('\0'));
    return Record.collect(alike, (_, run) => {
        const text = Array.headNonEmpty(run).pieces.map((piece, index) => {
            const spelled = Array.map(run, ({ pieces }) => pieces[index] ?? piece);
            const [low, high] = [Array.min(spelled, BY_VALUE), Array.max(spelled, BY_VALUE)];
            return BY_VALUE(low, high) === 0 ? piece : `${low} to ${high}`;
        });
        const span = Option.map(Option.all([Array.lastNonEmpty(run).at, Array.headNonEmpty(run).at]), (ends) => Array.dedupe(ends.map(Format.instant)).join(' to '));
        return new Line({ head: [...Option.toArray(span), ...(run.length > 1 ? [Format.count(run.length, { other: 'times' })] : [])], value: text.join('') });
    });
};
const Diagnostics = {
    tool,
    Logs,
    run: Effect.fnUntraced(function* ({ application, ...query }: Tool.Parameters<typeof tool>) {
        const [server, output] = yield* Effect.all([ApplicationServer, Output]);
        const read = yield* UndefinedOr.match(application, {
            onUndefined: () => Effect.succeed({ folders: [{ url: pathToFileURL(`${output.logs}/`), glob: '*.log' }] }),
            onDefined: Effect.fnUntraced(function* (given: string) {
                const [row, handle, logs] = yield* Effect.all([Target.resolve(given), Handle, Logs]);
                const [rows, { followed }, folders] = yield* Effect.all([SubscriptionRef.get(server.running), handle.get(Target), logs(row)]);
                return { application: { url: row.url, ...Option.getOrUndefined(Option.map(Array.head(Entry.instancesOf(rows, followed)(row).meant), Struct.pick(['instance']))) }, folders };
            }),
        });
        const { reports, samples, logs, unified, unreadable } = yield* server.diagnostics({ ...query, ...read });
        const recorded = logs.map(({ file, text }) =>
            Option.match(Schema.decodeOption(LogRecord)(text), {
                onNone: () => ({ file, at: Option.none<DateTime.Utc>(), text }),
                onSome: ({ timestamp, severity_text, scope, body, attributes = {} }) => ({
                    file,
                    at: Option.some(timestamp),
                    text: [[severity_text, ...Array.fromNullishOr(scope), body].join(' '), ...Record.collect(attributes, (key, value) => `${key} ${Predicate.isString(value) ? Format.quoted(value) : Formatter.format(value)}`)].join(', '),
                }),
            }),
        );
        const logged = Result.match(unified, {
            onFailure: ({ message }) => [{ value: message }],
            onSuccess: (events) => {
                const entries = events.map(({ timestamp, messageType, subsystem, category, processImagePath, senderImagePath, eventMessage }) => ({
                    process: basename(processImagePath),
                    at: DateTime.make(timestamp),
                    text: [messageType, subsystem, category, ...(senderImagePath === processImagePath ? [] : [`from ${basename(senderImagePath)}`]), eventMessage].filter(String.isNonEmpty).join(' '),
                }));
                return Record.collect(
                    Array.groupBy(entries, ({ process }) => process),
                    (value, run) => ({ value, rows: coalesce(run) }),
                );
            },
        });
        const lines = [
            ...reports.map((report) => new Line({ head: [report.kind, report.date], value: Format.path(report.url), rows: Report.of(Struct.pick(report, ['event', 'exception', 'termination'])).lines })),
            ...Report.of({ samples: samples.map(Format.path), unreadable: unreadable.map(({ message }) => message) }).lines,
            ...Record.collect(
                Array.groupBy(recorded, ({ file }) => file),
                (file, run) => new Line({ head: ['log'], value: Format.path(file), rows: coalesce(run) }),
            ),
            ...logged.map((stated) => new Line({ head: ['unified log'], ...stated })),
        ];
        return new Report({ lines: Array.isReadonlyArrayEmpty(lines) ? [new Line({ head: [Outline.nouns.match.other], value: Format.count(lines.length) })] : lines });
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Diagnostics };

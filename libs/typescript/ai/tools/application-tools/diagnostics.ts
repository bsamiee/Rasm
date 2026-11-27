import { basename } from 'node:path';
import { pathToFileURL } from 'node:url';
import { Entry } from '@rasm/applications/catalog';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Output } from '@rasm/mcp/cli';
import { Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { Array, Context, Effect, Equivalence, Formatter, Function, Option, Order, Predicate, Record, Result, Schema, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { Outline } from '#/handle/outline';
import { resolve, Target } from '#/handle/target';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT = 50;
const NUMBER = /(?<number>\p{Nd}+(?:\.\p{Nd}+)?)/u;
const TIMES = { other: 'times' } as const;
const BY_VALUE = Order.mapInput(Order.Number, Number.parseFloat);
const ALIKE = Array.makeEquivalence(Equivalence.String);

// --- [MODELS] --------------------------------------------------------------------------

const LogRecord = Schema.fromJsonString(Schema.Struct({ timestamp: Schema.String, severity_text: Schema.String, scope: Schema.optionalKey(Schema.String), body: Schema.String, attributes: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json)) }));
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
}).annotateMerge(Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Strict, true)));

// --- [SERVICES] ------------------------------------------------------------------------

const Logs = Context.Reference<(row: Entry) => Effect.Effect<(typeof ApplicationServer.Diagnostics.Type)['folders'], Error>>('@rasm/application-tools/diagnostics/Logs', { defaultValue: () => () => Effect.succeed([]) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const coalesce = (entries: readonly { readonly at: Option.Option<string>; readonly text: string }[]): readonly Line[] => {
    const split = entries.map(({ at, text }) => {
        const pieces = String.split(text, NUMBER);
        return { at, pieces, words: pieces.filter((_, index) => index % 2 === 0) };
    });
    return (Array.isReadonlyArrayNonEmpty(split) ? Array.groupWith(split, (left, right) => ALIKE(left.words, right.words)) : []).map((run) => {
        const [newest, oldest] = [Array.headNonEmpty(run), Array.lastNonEmpty(run)];
        const text = newest.pieces
            .map((piece, index) => {
                const spelled = Array.map(run, ({ pieces }) => pieces[index] ?? piece);
                const [low, high] = [Array.min(spelled, BY_VALUE), Array.max(spelled, BY_VALUE)];
                return index % 2 === 0 || Number(low) === Number(high) ? piece : `${low} to ${high}`;
            })
            .join('');
        const span = Predicate.isTupleOf(run, 1) ? newest.at : Option.map(Option.all([oldest.at, newest.at]), ([from, to]) => `${from} to ${to}`);
        return new Line({ head: [...Option.toArray(span), ...(run.length > 1 ? [Format.count(run.length, TIMES)] : [])], value: text });
    });
};
const Diagnostics = {
    tool,
    handle: Effect.fnUntraced(function* ({ application, ...query }: Tool.Parameters<typeof tool>) {
        const [server, output] = yield* Effect.all([ApplicationServer, Output]);
        const read = yield* UndefinedOr.match(application, {
            onUndefined: () => Effect.succeed({ folders: [{ url: pathToFileURL(`${output.logs}/`), glob: '*.log' }] }),
            onDefined: Effect.fnUntraced(function* (given: string) {
                const [row, handle, logs] = yield* Effect.all([resolve(given), Handle, Logs]);
                const [rows, { followed }, folders] = yield* Effect.all([SubscriptionRef.get(server.running), handle.get(Target), logs(row)]);
                return { application: { url: row.url, ...Option.getOrUndefined(Option.map(Array.head(Entry.instancesOf(rows, followed)(row).meant), Struct.pick(['instance']))) }, folders };
            }),
        });
        const { reports, samples, logs, unified, unreadable } = yield* server.diagnostics({ ...query, ...read });
        const entries = logs.map(({ file, text }) => ({
            file,
            entry: Option.match(Schema.decodeOption(LogRecord)(text), {
                onNone: () => ({ at: Option.none<string>(), text }),
                onSome: ({ timestamp, severity_text, scope, body, attributes = {} }) => ({
                    at: Option.some(timestamp),
                    text: [[severity_text, ...Array.fromNullishOr(scope), body].join(' '), ...Record.collect(attributes, (key, value) => `${key} ${Predicate.isString(value) ? Format.quoted(value) : Formatter.format(value)}`)].join(', '),
                }),
            }),
        }));
        const filed = Array.isReadonlyArrayNonEmpty(entries) ? Array.groupWith(entries, (left, right) => left.file === right.file) : [];
        const lines = [
            ...reports.map((report) => new Line({ head: [report.kind, report.date], value: Format.path(report.url), rows: Report.of(Struct.pick(report, ['event', 'exception', 'termination'])).lines })),
            ...Array.match(samples, { onEmpty: Array.empty, onNonEmpty: (urls) => [new Line({ head: ['samples'], value: Format.list(urls.map((url) => Format.quoted(Format.path(url)))) })] }),
            ...filed.map((group) => new Line({ head: ['log'], value: Format.path(Array.headNonEmpty(group).file), rows: coalesce(group.map(Struct.get('entry'))) })),
            ...Result.match(unified, {
                onFailure: ({ message }) => [new Line({ head: ['unified log'], value: 'unread', facts: [message] })],
                onSuccess: Array.match({
                    onEmpty: Array.empty,
                    onNonEmpty: (events) => [
                        new Line({
                            head: ['unified log'],
                            value: Format.list(Array.dedupe(events.map(({ processImagePath }) => basename(processImagePath)))),
                            rows: coalesce(
                                events.map(({ timestamp, messageType, subsystem, category, processImagePath, senderImagePath, eventMessage }) => ({
                                    at: Option.some(timestamp),
                                    text: [messageType, subsystem, category, ...(senderImagePath === processImagePath ? [] : [`from ${basename(senderImagePath)}`]), eventMessage].filter(String.isNonEmpty).join(' '),
                                })),
                            ),
                        }),
                    ],
                }),
            }),
            ...Array.match(unreadable, { onEmpty: Array.empty, onNonEmpty: (causes) => [new Line({ head: ['unreadable'], value: Format.list(causes.map(Format.quoted)) })] }),
        ];
        return new Report({ lines: Array.match(lines, { onEmpty: () => [new Line({ head: [Outline.nouns.match.other], value: Format.count(lines.length) })], onNonEmpty: Function.identity }) });
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Diagnostics, Logs };

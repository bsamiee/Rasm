// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Data, Match, Option, Result, Schema, String } from 'effect';
import { HostId, JobId } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type HostRejection = Data.TaggedEnum<{
    readonly scriptThrew: {
        readonly name: string;
        readonly message: string;
        readonly line: Option.Option<number>;
        readonly fileName: Option.Option<string>;
        readonly number: Option.Option<number>;
    };
    readonly descriptorFailed: { readonly index: number; readonly result: number; readonly message: string };
    readonly userCancelled: Record<never, never>;
    readonly preferenceLocked: { readonly section: string; readonly key: string };
    readonly modalDenied: { readonly holder: Option.Option<string> };
    readonly notAllowed: { readonly method: string };
    readonly menuItemNotListed: { readonly name: string };
    readonly noActiveDocument: Record<never, never>;
    readonly unknownMethod: { readonly method: string };
    readonly malformedParams: { readonly reason: string };
}>;

type BridgeError = Data.TaggedEnum<{
    readonly hostNotRunning: { readonly host: HostId };
    readonly automationDenied: { readonly host: HostId };
    readonly hostUnresponsive: { readonly host: HostId; readonly code: number };
    readonly scriptNotCompiled: { readonly host: HostId; readonly code: number; readonly line: string };
    readonly hostRejected: { readonly host: HostId; readonly code: number; readonly reason: string };
    readonly deadlineExceeded: { readonly host: HostId; readonly jobId: JobId };
    readonly hostNotAttached: { readonly host: HostId };
    readonly hostBusy: { readonly host: HostId; readonly jobId: JobId; readonly startedAt: number };
    readonly transportClosed: { readonly host: HostId; readonly code: number; readonly reason: string };
    readonly resultNotDecodable: { readonly host: HostId; readonly text: string; readonly reason: string };
    readonly hostThrew: { readonly host: HostId; readonly rejection: HostRejection };
}>;

type Probe = Result.Result<Schema.Json, BridgeError>;

// --- [MODELS] --------------------------------------------------------------------------

const _optionalNumber = Schema.OptionFromOptionalKey(Schema.Number);

const HostRejection: Schema.Codec<HostRejection, unknown> = Schema.Union([
    Schema.TaggedStruct('scriptThrew', { name: Schema.String, message: Schema.String, line: _optionalNumber, fileName: Schema.OptionFromOptionalKey(Schema.String), number: _optionalNumber }),
    Schema.TaggedStruct('descriptorFailed', { index: Schema.Number, result: Schema.Number, message: Schema.String }),
    Schema.TaggedStruct('userCancelled', {}),
    Schema.TaggedStruct('preferenceLocked', { section: Schema.String, key: Schema.String }),
    Schema.TaggedStruct('modalDenied', { holder: Schema.OptionFromNullOr(Schema.String) }),
    Schema.TaggedStruct('notAllowed', { method: Schema.String }),
    Schema.TaggedStruct('menuItemNotListed', { name: Schema.String }),
    Schema.TaggedStruct('noActiveDocument', {}),
    Schema.TaggedStruct('unknownMethod', { method: Schema.String }),
    Schema.TaggedStruct('malformedParams', { reason: Schema.String }),
]);

const BridgeError: Schema.Codec<BridgeError, unknown> = Schema.Union([
    Schema.TaggedStruct('hostNotRunning', { host: HostId }),
    Schema.TaggedStruct('automationDenied', { host: HostId }),
    Schema.TaggedStruct('hostUnresponsive', { host: HostId, code: Schema.Number }),
    Schema.TaggedStruct('scriptNotCompiled', { host: HostId, code: Schema.Number, line: Schema.String }),
    Schema.TaggedStruct('hostRejected', { host: HostId, code: Schema.Number, reason: Schema.String }),
    Schema.TaggedStruct('deadlineExceeded', { host: HostId, jobId: JobId }),
    Schema.TaggedStruct('hostNotAttached', { host: HostId }),
    Schema.TaggedStruct('hostBusy', { host: HostId, jobId: JobId, startedAt: Schema.Number }),
    Schema.TaggedStruct('transportClosed', { host: HostId, code: Schema.Number, reason: Schema.String }),
    Schema.TaggedStruct('resultNotDecodable', { host: HostId, text: Schema.String, reason: Schema.String }),
    Schema.TaggedStruct('hostThrew', { host: HostId, rejection: HostRejection }),
]);

const bridgeError: Data.TaggedEnum.Constructor<BridgeError> = Data.taggedEnum<BridgeError>();

const _Report = Schema.TemplateLiteralParser([Schema.String, ': ', Schema.Literals(['syntax', 'execution']), ' error: ', Schema.String, ' (', Schema.NumberFromString, ')']);
const _report: (text: string) => Option.Option<typeof _Report.Type> = Schema.decodeUnknownOption(_Report);

// --- [CLASSIFICATION] ------------------------------------------------------------------

const classify = (host: HostId, reply: { readonly exitCode: number; readonly stdout: string; readonly stderr: string }): Result.Result<string, BridgeError> =>
    reply.exitCode === 0
        ? Result.succeed(String.trim(reply.stdout))
        : Result.fail(
              Option.match(Array.findFirst(String.linesIterator(reply.stderr), _report), {
                  onNone: () => bridgeError.hostRejected({ host, code: reply.exitCode, reason: String.trim(reply.stderr) }),
                  onSome: ([line, , phase, , message, , code]) =>
                      Match.value({ phase, code }).pipe(
                          Match.withReturnType<BridgeError>(),
                          Match.when({ phase: 'syntax' }, () => bridgeError.scriptNotCompiled({ host, code, line })),
                          Match.when({ code: -600 }, () => bridgeError.hostNotRunning({ host })),
                          Match.whenOr({ code: -1743 }, { code: -1744 }, () => bridgeError.automationDenied({ host })),
                          Match.when({ code: -1712 }, () => bridgeError.hostUnresponsive({ host, code })),
                          Match.orElse(() => bridgeError.hostRejected({ host, code, reason: message })),
                      ),
              }),
          );

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Probe };
export { BridgeError, bridgeError, classify, HostRejection };

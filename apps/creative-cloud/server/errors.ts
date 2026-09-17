// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Match, Option, type PlatformError, Predicate, Schema, String } from 'effect';
import { AbsolutePath, Autocorrections, HostId, JobId } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type HostRejection = (typeof HostRejection)['Type'];
type BridgeError = (typeof BridgeError)['Type'];
type NonZeroExit = (typeof NonZeroExit)['Type'];

// --- [MODELS] --------------------------------------------------------------------------

const _optionalNumber = Schema.OptionFromOptionalKey(Schema.Number);
const _optionalString = Schema.OptionFromOptionalKey(Schema.String);

const HostRejection: Schema.TaggedUnion<{
    readonly scriptThrew: Schema.TaggedStruct<
        'scriptThrew',
        {
            readonly name: Schema.String;
            readonly message: Schema.String;
            readonly stack: Schema.OptionFromOptionalKey<Schema.String>;
            readonly line: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly fileName: Schema.OptionFromOptionalKey<Schema.String>;
            readonly number: Schema.OptionFromOptionalKey<Schema.Number>;
        }
    >;
    readonly descriptorFailed: Schema.TaggedStruct<'descriptorFailed', { readonly index: Schema.Number; readonly result: Schema.Number; readonly message: Schema.String }>;
    readonly userCancelled: Schema.TaggedStruct<'userCancelled', Record<never, never>>;
    readonly preferenceLocked: Schema.TaggedStruct<'preferenceLocked', { readonly section: Schema.String; readonly key: Schema.String }>;
    readonly modalDenied: Schema.TaggedStruct<'modalDenied', { readonly holder: Schema.OptionFromNullOr<Schema.String> }>;
    readonly menuItemNotListed: Schema.TaggedStruct<'menuItemNotListed', { readonly name: Schema.String }>;
    readonly documentNotOpen: Schema.TaggedStruct<'documentNotOpen', { readonly path: typeof AbsolutePath }>;
    readonly profileAbsent: Schema.TaggedStruct<'profileAbsent', { readonly profile: Schema.String }>;
    readonly noActiveDocument: Schema.TaggedStruct<'noActiveDocument', Record<never, never>>;
    readonly pageOutOfRange: Schema.TaggedStruct<'pageOutOfRange', { readonly index: Schema.Int; readonly count: Schema.Int }>;
    readonly itemNotFound: Schema.TaggedStruct<'itemNotFound', { readonly itemId: Schema.Int }>;
    readonly documentNotFound: Schema.TaggedStruct<'documentNotFound', { readonly documentId: Schema.Int }>;
    readonly actionNotFound: Schema.TaggedStruct<'actionNotFound', { readonly set: Schema.String; readonly action: Schema.String }>;
    readonly unknownMethod: Schema.TaggedStruct<'unknownMethod', { readonly method: Schema.String }>;
    readonly malformedParams: Schema.TaggedStruct<'malformedParams', { readonly cause: Schema.Defect }>;
    readonly resultNotJson: Schema.TaggedStruct<'resultNotJson', { readonly cause: Schema.Defect }>;
}> = Schema.TaggedUnion({
    scriptThrew: { name: Schema.String, message: Schema.String, stack: _optionalString, line: _optionalNumber, fileName: _optionalString, number: _optionalNumber },
    descriptorFailed: { index: Schema.Number, result: Schema.Number, message: Schema.String },
    userCancelled: {},
    preferenceLocked: { section: Schema.String, key: Schema.String },
    modalDenied: { holder: Schema.OptionFromNullOr(Schema.String) },
    menuItemNotListed: { name: Schema.String },
    documentNotOpen: { path: AbsolutePath },
    profileAbsent: { profile: Schema.String },
    noActiveDocument: {},
    pageOutOfRange: { index: Schema.Int, count: Schema.Int },
    itemNotFound: { itemId: Schema.Int },
    documentNotFound: { documentId: Schema.Int },
    actionNotFound: { set: Schema.String, action: Schema.String },
    unknownMethod: { method: Schema.String },
    malformedParams: { cause: Schema.Defect() },
    resultNotJson: { cause: Schema.Defect() },
});

const BridgeError: Schema.TaggedUnion<{
    readonly hostNotRunning: Schema.TaggedStruct<'hostNotRunning', { readonly host: typeof HostId }>;
    readonly automationDenied: Schema.TaggedStruct<'automationDenied', { readonly host: typeof HostId }>;
    readonly hostUnresponsive: Schema.TaggedStruct<'hostUnresponsive', { readonly host: typeof HostId; readonly code: Schema.Number }>;
    readonly scriptNotCompiled: Schema.TaggedStruct<'scriptNotCompiled', { readonly host: typeof HostId; readonly code: Schema.Number; readonly line: Schema.String }>;
    readonly hostRejected: Schema.TaggedStruct<'hostRejected', { readonly host: typeof HostId; readonly code: Schema.Number; readonly reason: Schema.String }>;
    readonly deadlineExceeded: Schema.TaggedStruct<'deadlineExceeded', { readonly host: typeof HostId; readonly jobId: typeof JobId }>;
    readonly hostNotAttached: Schema.TaggedStruct<'hostNotAttached', { readonly host: typeof HostId }>;
    readonly hostBusy: Schema.TaggedStruct<'hostBusy', { readonly host: typeof HostId; readonly jobId: typeof JobId; readonly startedAt: Schema.Number }>;
    readonly hostSaturated: Schema.TaggedStruct<'hostSaturated', { readonly host: typeof HostId; readonly pid: Schema.Int; readonly cpu: Schema.Number }>;
    readonly pluginDetached: Schema.TaggedStruct<'pluginDetached', { readonly host: typeof HostId; readonly jobId: typeof JobId }>;
    readonly resultNotDecodable: Schema.TaggedStruct<'resultNotDecodable', { readonly host: typeof HostId; readonly value: Schema.Codec<Schema.Json>; readonly cause: Schema.Defect }>;
    readonly fileNotAccessible: Schema.TaggedStruct<'fileNotAccessible', { readonly host: typeof HostId; readonly path: Schema.OptionFromNullOr<Schema.String>; readonly reason: Schema.String }>;
    readonly hostThrew: Schema.TaggedStruct<'hostThrew', { readonly host: typeof HostId; readonly rejection: typeof HostRejection; readonly autocorrections: typeof Autocorrections }>;
}> = Schema.TaggedUnion({
    hostNotRunning: { host: HostId },
    automationDenied: { host: HostId },
    hostUnresponsive: { host: HostId, code: Schema.Number },
    scriptNotCompiled: { host: HostId, code: Schema.Number, line: Schema.String },
    hostRejected: { host: HostId, code: Schema.Number, reason: Schema.String },
    deadlineExceeded: { host: HostId, jobId: JobId },
    hostNotAttached: { host: HostId },
    hostBusy: { host: HostId, jobId: JobId, startedAt: Schema.Number },
    hostSaturated: { host: HostId, pid: Schema.Int, cpu: Schema.Number },
    pluginDetached: { host: HostId, jobId: JobId },
    resultNotDecodable: { host: HostId, value: Schema.Json, cause: Schema.Defect() },
    fileNotAccessible: { host: HostId, path: Schema.OptionFromNullOr(Schema.String), reason: Schema.String },
    hostThrew: { host: HostId, rejection: HostRejection, autocorrections: Autocorrections },
});

const NonZeroExit: Schema.TaggedStruct<'nonZeroExit', { readonly exitCode: Schema.Number; readonly stderr: Schema.String }> = Schema.TaggedStruct('nonZeroExit', {
    exitCode: Schema.Number,
    stderr: Schema.String,
});

const _Report = Schema.TemplateLiteralParser([Schema.String, ': ', Schema.Literals(['syntax', 'execution']), ' error: ', Schema.String, ' (', Schema.NumberFromString, ')']);
const _report: (text: string) => Option.Option<(typeof _Report)['Type']> = Schema.decodeUnknownOption(_Report);

// --- [CLASSIFICATION] ------------------------------------------------------------------

const classify = (host: HostId, exit: NonZeroExit): BridgeError =>
    Option.match(Array.findFirst(String.linesIterator(exit.stderr), _report), {
        onNone: () => BridgeError.cases.hostRejected.make({ host, code: exit.exitCode, reason: String.trim(exit.stderr) }),
        onSome: ([line, , phase, , message, , code]) =>
            Match.value({ phase, code }).pipe(
                Match.withReturnType<BridgeError>(),
                Match.when({ phase: 'syntax' }, () => BridgeError.cases.scriptNotCompiled.make({ host, code, line })),
                Match.when({ code: -600 }, () => BridgeError.cases.hostNotRunning.make({ host })),
                Match.whenOr({ code: -1743 }, { code: -1744 }, () => BridgeError.cases.automationDenied.make({ host })),
                Match.when({ code: -1712 }, () => BridgeError.cases.hostUnresponsive.make({ host, code })),
                Match.orElse(() => BridgeError.cases.hostRejected.make({ host, code, reason: message })),
            ),
    });

const notDecodable =
    (host: HostId, value: Schema.Json) =>
    (cause: unknown): BridgeError =>
        BridgeError.cases.resultNotDecodable.make({ host, value, cause });

const inaccessible =
    (host: HostId) =>
    (error: PlatformError.PlatformError): BridgeError =>
        BridgeError.cases.fileNotAccessible.make({
            host,
            path: Option.flatMap(Option.liftPredicate(error.reason, Predicate.hasProperty('pathOrDescriptor')), (reason) => Option.liftPredicate(reason.pathOrDescriptor, Predicate.isString)),
            reason: error.reason._tag,
        });

// --- [EXPORTS] -------------------------------------------------------------------------

export { BridgeError, classify, HostRejection, inaccessible, NonZeroExit, notDecodable };

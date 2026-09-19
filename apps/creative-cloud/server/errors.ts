// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Cause, Effect, Match, Option, type PlatformError, Predicate, Schema, String } from 'effect';
import { AbsolutePath, Autocorrections, type Closed, HostId, JobId, OptionalNumber, OptionalString } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type HostRejection = (typeof HostRejection)['Type'];
type BridgeError = (typeof BridgeError)['Type'];
type NonZeroExit = (typeof NonZeroExit)['Type'];
type WriteRejection = (typeof WriteRejection)['Type'];
type Inaccessible = PlatformError.PlatformError['reason']['_tag'];

type AppliedReply<Keyed extends Schema.Struct.Fields> = Schema.Struct<{
    readonly kind: Schema.Literal<'applied'>;
    readonly applied: Schema.$Array<Schema.Struct<Keyed & { readonly from: Schema.Codec<Schema.Json>; readonly to: Schema.Codec<Schema.Json> }>>;
    readonly rejected: Schema.$Array<Schema.Struct<Keyed & { readonly reason: typeof WriteRejection }>>;
}>;

type PreferencesReply<Keyed extends Schema.Struct.Fields> = Schema.Struct<{
    readonly kind: Schema.Literal<'preferences'>;
    readonly values: Schema.$Record<Schema.String, Schema.$Record<Schema.String, Schema.Codec<Schema.Json>>>;
    readonly unreadable: Schema.$Array<Schema.Struct<Keyed & { readonly cause: Schema.Defect }>>;
}>;

// --- [TABLES] --------------------------------------------------------------------------

const _INACCESSIBLE = [
    'AlreadyExists',
    'BadResource',
    'Busy',
    'InvalidData',
    'NotFound',
    'PermissionDenied',
    'TimedOut',
    'UnexpectedEof',
    'Unknown',
    'WouldBlock',
    'WriteZero',
    'BadArgument',
] as const;

const _MAC_ERRORS = { procNotFound: -600, errAETimeout: -1712, errAEEventNotPermitted: -1743, errAEEventWouldRequireUserConsent: -1744 } as const;

// --- [MODELS] --------------------------------------------------------------------------

const WriteRejection: Schema.TaggedUnion<{
    readonly unknownKey: Schema.TaggedStruct<'unknownKey', Record<never, never>>;
    readonly readOnly: Schema.TaggedStruct<'readOnly', Record<never, never>>;
    readonly threw: Schema.TaggedStruct<'threw', { readonly cause: Schema.Defect }>;
    readonly unchanged: Schema.TaggedStruct<'unchanged', Record<never, never>>;
}> = Schema.TaggedUnion({ unknownKey: {}, readOnly: {}, threw: { cause: Schema.Defect() }, unchanged: {} });

const applied = <const Keyed extends Schema.Struct.Fields>(keyed: Keyed): AppliedReply<Keyed> =>
    Schema.Struct({
        kind: Schema.Literal('applied'),
        applied: Schema.Array(Schema.Struct({ ...keyed, from: Schema.Json, to: Schema.Json })),
        rejected: Schema.Array(Schema.Struct({ ...keyed, reason: WriteRejection })),
    });

const preferences = <const Keyed extends Schema.Struct.Fields>(keyed: Keyed): PreferencesReply<Keyed> =>
    Schema.Struct({
        kind: Schema.Literal('preferences'),
        values: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Json)),
        unreadable: Schema.Array(Schema.Struct({ ...keyed, cause: Schema.Defect() })),
    });

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
            readonly code: Schema.OptionFromOptionalKey<Schema.Number>;
            readonly tag: Schema.OptionFromOptionalKey<Schema.String>;
        }
    >;
    readonly descriptorFailed: Schema.TaggedStruct<'descriptorFailed', { readonly index: Schema.Number; readonly result: Schema.Number; readonly message: Schema.String }>;
    readonly userCancelled: Schema.TaggedStruct<'userCancelled', Record<never, never>>;
    readonly preferenceLocked: Schema.TaggedStruct<'preferenceLocked', { readonly section: Schema.String; readonly key: Schema.String }>;
    readonly presetGroupAbsent: Schema.TaggedStruct<'presetGroupAbsent', { readonly kind: Schema.String; readonly groups: Schema.$Array<Schema.String> }>;
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
    scriptThrew: {
        name: Schema.String,
        message: Schema.String,
        stack: OptionalString,
        line: OptionalNumber,
        fileName: OptionalString,
        number: OptionalNumber,
        code: OptionalNumber,
        tag: OptionalString,
    },
    descriptorFailed: { index: Schema.Number, result: Schema.Number, message: Schema.String },
    userCancelled: {},
    preferenceLocked: { section: Schema.String, key: Schema.String },
    presetGroupAbsent: { kind: Schema.String, groups: Schema.Array(Schema.String) },
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
    readonly hostNotInstalled: Schema.TaggedStruct<'hostNotInstalled', { readonly host: typeof HostId; readonly unresolved: Schema.$Array<Schema.String> }>;
    readonly hostNotRunning: Schema.TaggedStruct<'hostNotRunning', { readonly host: typeof HostId }>;
    readonly automationDenied: Schema.TaggedStruct<'automationDenied', { readonly host: typeof HostId }>;
    readonly hostUnresponsive: Schema.TaggedStruct<'hostUnresponsive', { readonly host: typeof HostId; readonly code: Schema.Number }>;
    readonly scriptTimedOut: Schema.TaggedStruct<'scriptTimedOut', { readonly host: typeof HostId; readonly timeoutMs: Schema.Number }>;
    readonly scriptNotCompiled: Schema.TaggedStruct<'scriptNotCompiled', { readonly host: typeof HostId; readonly code: Schema.Number; readonly line: Schema.String }>;
    readonly hostRejected: Schema.TaggedStruct<'hostRejected', { readonly host: typeof HostId; readonly code: Schema.Number; readonly reason: Schema.String }>;
    readonly deadlineExceeded: Schema.TaggedStruct<'deadlineExceeded', { readonly host: typeof HostId; readonly jobId: typeof JobId }>;
    readonly hostNotAttached: Schema.TaggedStruct<'hostNotAttached', { readonly host: typeof HostId }>;
    readonly portNotBound: Schema.TaggedStruct<'portNotBound', { readonly host: typeof HostId; readonly port: Schema.Int; readonly cause: Schema.Defect }>;
    readonly hostBusy: Schema.TaggedStruct<'hostBusy', { readonly host: typeof HostId; readonly jobId: typeof JobId; readonly startedAt: Schema.Number }>;
    readonly pluginDetached: Schema.TaggedStruct<'pluginDetached', { readonly host: typeof HostId; readonly jobId: typeof JobId }>;
    readonly resultNotDecodable: Schema.TaggedStruct<'resultNotDecodable', { readonly host: typeof HostId; readonly value: Schema.Codec<Schema.Json>; readonly cause: Schema.Defect }>;
    readonly fileNotAccessible: Schema.TaggedStruct<
        'fileNotAccessible',
        { readonly host: typeof HostId; readonly path: Schema.OptionFromNullOr<Schema.String>; readonly reason: Schema.Literals<Closed<Inaccessible, typeof _INACCESSIBLE>> }
    >;
    readonly commandFailed: Schema.TaggedStruct<'commandFailed', { readonly host: typeof HostId; readonly program: Schema.String; readonly code: Schema.Number; readonly stderr: Schema.String }>;
    readonly hostThrew: Schema.TaggedStruct<'hostThrew', { readonly host: typeof HostId; readonly rejection: typeof HostRejection; readonly autocorrections: typeof Autocorrections }>;
}> = Schema.TaggedUnion({
    hostNotInstalled: { host: HostId, unresolved: Schema.Array(Schema.String) },
    hostNotRunning: { host: HostId },
    automationDenied: { host: HostId },
    hostUnresponsive: { host: HostId, code: Schema.Number },
    scriptTimedOut: { host: HostId, timeoutMs: Schema.Number },
    scriptNotCompiled: { host: HostId, code: Schema.Number, line: Schema.String },
    hostRejected: { host: HostId, code: Schema.Number, reason: Schema.String },
    deadlineExceeded: { host: HostId, jobId: JobId },
    hostNotAttached: { host: HostId },
    portNotBound: { host: HostId, port: Schema.Int, cause: Schema.Defect() },
    hostBusy: { host: HostId, jobId: JobId, startedAt: Schema.Number },
    pluginDetached: { host: HostId, jobId: JobId },
    resultNotDecodable: { host: HostId, value: Schema.Json, cause: Schema.Defect() },
    fileNotAccessible: { host: HostId, path: Schema.OptionFromNullOr(Schema.String), reason: Schema.Literals(_INACCESSIBLE) },
    commandFailed: { host: HostId, program: Schema.String, code: Schema.Number, stderr: Schema.String },
    hostThrew: { host: HostId, rejection: HostRejection, autocorrections: Autocorrections },
});

const NonZeroExit: Schema.TaggedStruct<'nonZeroExit', { readonly program: Schema.String; readonly exitCode: Schema.Number; readonly stderr: Schema.String }> = Schema.TaggedStruct('nonZeroExit', {
    program: Schema.String,
    exitCode: Schema.Number,
    stderr: Schema.String,
});

const _Report = Schema.TemplateLiteralParser([Schema.String, ': ', Schema.Literals(['syntax', 'execution']), ' error: ', Schema.String, ' (', Schema.NumberFromString, ')']);
const _report: (line: string) => Option.Option<(typeof _Report)['Type']> = Schema.decodeUnknownOption(_Report);

// --- [CLASSIFICATION] ------------------------------------------------------------------

const classify = (host: HostId, exit: NonZeroExit): BridgeError =>
    Option.match(Array.findFirst(String.linesIterator(exit.stderr), _report), {
        onNone: () => BridgeError.cases.hostRejected.make({ host, code: exit.exitCode, reason: String.trim(exit.stderr) }),
        onSome: ([line, , phase, , message, , code]) =>
            Match.value({ phase, code }).pipe(
                Match.withReturnType<BridgeError>(),
                Match.when({ phase: 'syntax' }, () => BridgeError.cases.scriptNotCompiled.make({ host, code, line })),
                Match.when({ code: _MAC_ERRORS.procNotFound }, () => BridgeError.cases.hostNotRunning.make({ host })),
                Match.whenOr({ code: _MAC_ERRORS.errAEEventNotPermitted }, { code: _MAC_ERRORS.errAEEventWouldRequireUserConsent }, () => BridgeError.cases.automationDenied.make({ host })),
                Match.when({ code: _MAC_ERRORS.errAETimeout }, () => BridgeError.cases.hostUnresponsive.make({ host, code })),
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

const exited =
    (host: HostId) =>
    (exit: NonZeroExit): BridgeError =>
        BridgeError.cases.commandFailed.make({ host, program: exit.program, code: exit.exitCode, stderr: exit.stderr });

const faulted = (cause: Cause.Cause<unknown>): Effect.Effect<void> => (Cause.hasInterruptsOnly(cause) ? Effect.void : Effect.logError(cause));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { AppliedReply, PreferencesReply };
export { applied, BridgeError, classify, exited, faulted, HostRejection, inaccessible, NonZeroExit, notDecodable, preferences, WriteRejection };

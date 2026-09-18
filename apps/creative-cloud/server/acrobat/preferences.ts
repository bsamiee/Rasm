// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Filter, identity, Option, Predicate, Record, Schema, Stream, String, Struct } from 'effect';
import { ChildProcess } from 'effect/unstable/process';
import { build, type PlistValue, parse } from 'plist';

// --- [TYPES] ---------------------------------------------------------------------------

type Scope = (typeof Scope)['Type'];
type Verb = 'export' | 'import';

interface Plan {
    readonly tree: PlistValue;
    readonly applied: readonly { readonly path: string; readonly from: Option.Option<boolean>; readonly to: boolean }[];
    readonly unchanged: readonly string[];
}

// --- [TABLE] ---------------------------------------------------------------------------

const _USER_ROWS = {
    'DC/Privileged/bProtectedMode': false,
    'DC/UnitsAndGuides/RulersVisible': true,
    'DC/AVGeneral/AcrobatRHPBottomBannerIPMEnabled': false,
    'DC/IPM/DoNotCheckForMessage': true,
    'DC/ToolRecommenderSection/OnDocNextToolRecommendation': false,
    'DC/AVGeneral/ShowWhatsNewExpContentAgain': false,
    'DC/HomeWelcome/LastShowStatus': false,
    'DC/AVGeneral/DisableStudioHome': true,
    'DC/HelpAndLearn/IsNewUserForContextualHelp': false,
    'DC/HelpAndLearn/HelpAndLearnV2NewUsers': false,
    'DC/ReadAloud/ReadAloudV2Enabled': false,
    'DC/ReadAloud/PaidVoicesEnabled': false,
} as const;

const _MACHINE_ROWS = {
    'DC/FeatureLockdown/bWhatsNewExp': true,
    'DC/FeatureLockdown/bToggleFTE': true,
    'DC/FeatureLockdown/cServices/bToggleNotifications': true,
    'DC/FeatureLockdown/cServices/bToggleNotificationToasts': true,
    'DC/FeatureLockdown/cServices/bEnableBellButton': true,
} as const;

const _machinePlist = (bundleId: string): string => `/Library/Preferences/${bundleId}.plist`;

const _SCOPES = {
    user: {
        rows: _USER_ROWS,
        argv: (bundleId: string, verb: Verb): Array.NonEmptyReadonlyArray<string> => ['defaults', verb, bundleId, '-'],
        settle: (): readonly Array.NonEmptyReadonlyArray<string>[] => [],
        leaf: (value: boolean): PlistValue => [0, value],
        read: (node: unknown): Option.Option<boolean> => Option.map(Schema.decodeUnknownOption(Schema.Tuple([Schema.Number, Schema.Boolean]))(node), ([, flag]) => flag),
    },
    machine: {
        rows: _MACHINE_ROWS,
        argv: (bundleId: string, verb: Verb): Array.NonEmptyReadonlyArray<string> =>
            (
                ({ export: ['defaults', 'export', _machinePlist(bundleId), '-'], import: ['sudo', 'defaults', 'import', _machinePlist(bundleId), '-'] }) satisfies Record<
                    Verb,
                    Array.NonEmptyReadonlyArray<string>
                >
            )[verb],
        settle: (bundleId: string): readonly Array.NonEmptyReadonlyArray<string>[] => [
            ['sudo', 'chown', 'root:wheel', _machinePlist(bundleId)],
            ['sudo', 'chmod', '755', _machinePlist(bundleId)],
        ],
        leaf: identity,
        read: (node: unknown): Option.Option<boolean> => Schema.decodeUnknownOption(Schema.Boolean)(node),
    },
};

// --- [MODELS] --------------------------------------------------------------------------

const Scope: Schema.toTaggedUnion<
    'scope',
    readonly [
        Schema.Struct<{ readonly scope: Schema.Literal<'user'>; readonly rows: Schema.OptionFromOptionalKey<Schema.NonEmptyArray<Schema.Literals<Array<keyof typeof _USER_ROWS>>>> }>,
        Schema.Struct<{ readonly scope: Schema.Literal<'machine'>; readonly rows: Schema.OptionFromOptionalKey<Schema.NonEmptyArray<Schema.Literals<Array<keyof typeof _MACHINE_ROWS>>>> }>,
    ]
> = Schema.Union([
    Schema.Struct({ scope: Schema.Literal('user'), rows: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Literals(Struct.keys(_USER_ROWS)))) }),
    Schema.Struct({ scope: Schema.Literal('machine'), rows: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Literals(Struct.keys(_MACHINE_ROWS)))) }),
]).pipe(Schema.toTaggedUnion('scope'));

// --- [TREE] ----------------------------------------------------------------------------

const _dict: (node: PlistValue) => Option.Option<Record.ReadonlyRecord<string, PlistValue>> = Option.liftPredicate(
    (candidate: PlistValue): candidate is Record.ReadonlyRecord<string, PlistValue> =>
        Predicate.isReadonlyObject(candidate) && !Array.isArray(candidate) && !Predicate.isDate(candidate) && !Predicate.isUint8Array(candidate),
);

const _at = (root: PlistValue, segments: readonly string[]): Option.Option<PlistValue> =>
    Array.reduce(segments, Option.some(root), (node, key) => Option.flatMap(node, (value) => Option.flatMap(_dict(value), Record.get(key))));

const _set = (node: PlistValue, segments: readonly string[], leaf: PlistValue): PlistValue =>
    Array.matchLeft(segments, {
        onEmpty: () => leaf,
        onNonEmpty: (key, rest) => {
            const dict = Option.getOrElse(_dict(node), () => ({}));
            return {
                ...dict,
                [key]: _set(
                    Option.getOrElse(Record.get(dict, key), () => ({})),
                    rest,
                    leaf,
                ),
            };
        },
    });

// --- [DEFAULTS] ------------------------------------------------------------------------

const defaults = (request: Scope, bundleId: string, tree: Option.Option<PlistValue>): ChildProcess.StandardCommand => {
    const [program, ...args] = _SCOPES[request.scope].argv(bundleId, Option.match(tree, { onNone: (): Verb => 'export', onSome: (): Verb => 'import' }));
    return ChildProcess.make(program, args, { stdin: Option.getOrUndefined(Option.map(tree, (value) => Stream.encodeText(Stream.make(build(value))))) });
};

const settle = (request: Scope, bundleId: string): readonly ChildProcess.StandardCommand[] =>
    Array.map(_SCOPES[request.scope].settle(bundleId), ([program, ...args]) => ChildProcess.make(program, args));

// --- [PLAN] ----------------------------------------------------------------------------

const plan = (request: Scope, xml: string): Plan => {
    const root = parse(xml);
    const { leaf, read, rows } = _SCOPES[request.scope];
    const entries = Record.toEntries<string, boolean>(rows);
    const chosen = Option.match(request.rows, {
        onNone: () => entries,
        onSome: (names: readonly string[]) => Array.filter(entries, ([path]) => Array.contains(names, path)),
    });
    const writes = Array.map(chosen, ([path, to]) => {
        const segments = String.split(path, '/');
        return { path, segments, to, from: Option.flatMap(_at(root, segments), read) };
    });
    const [unchanged, applied] = Array.partition(
        writes,
        Filter.fromPredicate((write) => !Option.contains(write.from, write.to)),
    );
    return {
        tree: Array.reduce(applied, root, (tree, write) => _set(tree, write.segments, leaf(write.to))),
        applied: Array.map(applied, Struct.pick(['path', 'from', 'to'])),
        unchanged: Array.map(unchanged, Struct.get('path')),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { defaults, plan, Scope, settle };

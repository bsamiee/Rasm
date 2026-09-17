// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Option, Predicate, Record, Result, Schema, String, Struct } from 'effect';
import { type PlistValue, parse } from 'plist';

// --- [TYPES] ---------------------------------------------------------------------------

type Scope = (typeof Scope)['Type'];

interface Write {
    readonly path: string;
    readonly from: Option.Option<string>;
    readonly to: string;
    readonly commands: readonly string[];
}

// --- [TABLE] ---------------------------------------------------------------------------

const _ROWS = {
    user: {
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
    },
    machine: {
        'DC/FeatureLockdown/bWhatsNewExp': true,
        'DC/FeatureLockdown/bToggleFTE': true,
        'DC/FeatureLockdown/cServices/bToggleNotifications': true,
        'DC/FeatureLockdown/cServices/bToggleNotificationToasts': true,
        'DC/FeatureLockdown/cServices/bEnableBellButton': true,
        'DC/FeatureLockdown/cServices/bToggleDocumentCloud': true,
        'DC/FeatureLockdown/cServices/bToggleWebConnectors': true,
    },
} as const;

const _FORMS = {
    user: { leaf: Option.some(1), add: (value: boolean): readonly string[] => [' array', ':0 integer 0', `:1 bool ${value}`] },
    machine: { leaf: Option.none<number>(), add: (value: boolean): readonly string[] => [` bool ${value}`] },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Scope: Schema.toTaggedUnion<
    'scope',
    readonly [
        Schema.Struct<{ readonly scope: Schema.Literal<'user'>; readonly rows: Schema.OptionFromOptionalKey<Schema.NonEmptyArray<Schema.Literals<Array<keyof (typeof _ROWS)['user']>>>> }>,
        Schema.Struct<{ readonly scope: Schema.Literal<'machine'>; readonly rows: Schema.OptionFromOptionalKey<Schema.NonEmptyArray<Schema.Literals<Array<keyof (typeof _ROWS)['machine']>>>> }>,
    ]
> = Schema.Union([
    Schema.Struct({ scope: Schema.Literal('user'), rows: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Literals(Struct.keys(_ROWS.user)))) }),
    Schema.Struct({ scope: Schema.Literal('machine'), rows: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Literals(Struct.keys(_ROWS.machine)))) }),
]).pipe(Schema.toTaggedUnion('scope'));

// --- [PLAN] ----------------------------------------------------------------------------

const _at = (root: PlistValue, segments: readonly string[]): Option.Option<PlistValue> =>
    Array.reduce(segments, Option.some(root), (node, key) => Option.flatMap(node, (value) => (Predicate.isReadonlyObject(value) ? Record.get(value, key) : Option.none())));

const _write = (root: PlistValue, scope: Scope['scope'], [path, value]: readonly [string, boolean]): Write => {
    const { leaf, add } = _FORMS[scope];
    const segments = String.split('/')(path);
    const key = `:${Array.join(segments, ':')}`;
    const found = _at(root, segments);
    const from = Option.map(
        Option.flatMap(found, (node) => Option.match(leaf, { onNone: () => Option.some(node), onSome: (index) => (Array.isArray(node) ? Array.get(node, index) : Option.none()) })),
        (current) => `${current}`,
    );
    const to = `${value}`;
    const commands = Option.match(found, {
        onSome: () => (Option.exists(from, (current) => current === to) ? [] : [`Set ${key}${Option.match(leaf, { onNone: () => '', onSome: (index) => `:${index}` })} ${to}`]),
        onNone: () => [
            ...Array.filterMap(segments, (_segment, index) => {
                const prefix = Array.take(segments, index + 1);
                return index + 1 < segments.length && Option.isNone(_at(root, prefix)) ? Result.succeed(`Add :${Array.join(prefix, ':')} dict`) : Result.failVoid;
            }),
            ...Array.map(add(value), (form) => `Add ${key}${form}`),
        ],
    });
    return { path, from, to, commands };
};

const plan = (request: Scope, xml: Option.Option<string>): readonly Write[] => {
    const root = Option.match(xml, { onNone: (): PlistValue => ({}), onSome: parse });
    const rows: Readonly<Record<string, boolean>> = Scope.match(request, {
        user: ({ rows: names }) => Option.match(names, { onNone: () => _ROWS.user, onSome: (picked) => Struct.pick(_ROWS.user, picked) }),
        machine: ({ rows: names }) => Option.match(names, { onNone: () => _ROWS.machine, onSome: (picked) => Struct.pick(_ROWS.machine, picked) }),
    });
    return Array.map(Record.toEntries(rows), (row) => _write(root, request.scope, row));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { plan, Scope };

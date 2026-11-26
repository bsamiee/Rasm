import type { Value } from '@rasm/applications';
import { Description } from '@rasm/applications/source';
import { HostError, Specifier } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import type { Application, Document, MenuAction, ToolBox } from 'adobe:indesign';
import { Array, Cause, Effect, Match, Option, Predicate, pipe, Result, Schema, Struct } from 'effect';
import { Dom } from '#/uxp/dom';

// --- [TYPES] ---------------------------------------------------------------------------

type Path = typeof Description.Locator.Type;

// --- [CONSTANTS] -----------------------------------------------------------------------

const HELD = { tools: 'toolBoxTools', panels: 'panels', menus: 'menuActions' } as const satisfies Record<string, keyof Application>;
const TOOL = 'currentTool' satisfies keyof ToolBox;
const AREA = 'area' satisfies keyof MenuAction;
const LEVEL = 'menu' satisfies (typeof Description.Container.Type)['kind'];
const TOOLBOX = '$ID/KBSCE Tools';

// --- [OPERATIONS] ----------------------------------------------------------------------

const same = Schema.toEquivalence(Description.Locator);
const at = (app: Application, field: keyof Application, ...steps: readonly (readonly [string, string])[]): Path => [[app.constructorName, field], ...steps];
const actionAt = (app: Application, action: MenuAction): Path => at(app, HELD.menus, [AREA, action.area], [action.constructorName, action.toSpecifier()]);
const listed = (app: Application, area: string): readonly MenuAction[] => {
    const { area: areas, id: ids } = app.menuActions.everyItem();
    return Array.filterMap(Array.zip(areas, ids), ([named, id]) => (named === area ? Result.succeed(app.menuActions.itemByID(id)) : Result.failVoid));
};
const toolbox = (app: Application): readonly MenuAction[] =>
    Option.match(
        Array.findFirst(Array.dedupe(app.menuActions.everyItem().area), (area) => area !== '' && app.findKeyStrings(area).includes(TOOLBOX)),
        { onNone: Array.empty, onSome: (area) => listed(app, area) },
    );
const actionItem = (app: Application, action: MenuAction) =>
    Description.Control.make({
        locator: actionAt(app, action),
        kind: 'item',
        role: action.constructorName,
        label: { text: action.name, attribute: 'name' },
        ...(Option.isSome(Dom.unavailable(action, 'enabled', false)) ? {} : { enabled: action.enabled, ...(action.checked ? { mark: '✓' } : {}) }),
    });
const facets = ({ app }: Dom['Service'], document: Option.Option<Document>) => {
    const { toolBoxTools: toolBox, panels, menuActions } = app;
    const choice = at(app, HELD.tools, [toolBox.constructorName, TOOL]);
    return Description.make({
        containers: [
            {
                locator: at(app, HELD.tools),
                kind: 'toolbar',
                controls: [
                    Description.Control.make({
                        locator: choice,
                        kind: 'choice',
                        role: toolBox.constructorName,
                        label: { text: TOOL, attribute: TOOL },
                        value: { text: { value: toolBox.currentToolName } },
                        selected: 'attribute',
                        options: Array.map(toolbox(app), (action) => ({ label: action.name, locator: actionAt(app, action) })),
                    }),
                ],
            },
            {
                locator: at(app, HELD.panels),
                kind: 'panel',
                controls: pipe(
                    Array.makeBy(panels.length, (index) => Option.fromUndefinedOr(panels.item(index))),
                    Array.getSomes,
                    Array.filter(({ name }) => name.length > 0),
                    Array.map(({ constructorName, name, visible }) => Description.Control.make({ locator: at(app, HELD.panels, [constructorName, name]), kind: 'toggle', role: constructorName, label: { text: name, attribute: 'name' }, value: { bool: { value: visible } } })),
                ),
            },
            {
                locator: at(app, HELD.menus),
                kind: 'menuBar',
                controls: Array.map(Array.dedupe(menuActions.everyItem().area), (area) => {
                    const item = at(app, HELD.menus, [AREA, area]);
                    return Description.Control.make({ locator: item, kind: 'item', role: menuActions.constructor.name, label: { text: area, attribute: AREA }, opens: Array.append(item, [LEVEL, area] as const) });
                }),
            },
        ],
        pointer: {
            tool: toolBox.currentToolName,
            holding: Option.match(Option.filter(Option.map(document, Struct.get('placeGuns')), Struct.get('loaded')), { onNone: Array.empty<string>, onSome: ({ pageItems }) => Array.map(pageItems.everyItem().getElements(), (item) => item.toSpecifier()) }),
        },
    });
};
const describe = Effect.fn('describe')(function* ({ within }: { readonly within?: Path }) {
    const dom = yield* Dom;
    if (within === undefined) {
        const document = yield* Effect.option(Dom.current);
        return yield* Effect.try({ try: () => facets(dom, document), catch: HostError.refused('facets') });
    }
    const { app } = dom;
    const [, area] = Array.lastNonEmpty(within);
    const item = at(app, HELD.menus, [AREA, area]);
    yield* same(within, item) ? Effect.void : Unmatched.of({ subject: 'menu area', query: Option.some(area), candidates: Array.dedupe(app.menuActions.everyItem().area) });
    return yield* Effect.try({
        try: () =>
            Description.make({
                containers: [
                    {
                        locator: Array.append(item, [LEVEL, area] as const),
                        kind: LEVEL,
                        title: area,
                        parent: at(app, HELD.menus),
                        controls: Array.map(listed(app, area), (action) => actionItem(app, action)),
                    },
                ],
            }),
        catch: HostError.refused(area),
    });
});
const operate = Effect.fn('operate')(function* ({ locator, to }: { readonly locator: Path; readonly to?: typeof Value.Type }) {
    const { app, owning } = yield* Dom;
    const text = locator.map((step) => step.join(' ')).join(' > ');
    const refused = HostError.refused(text);
    yield* Match.value(Array.map(locator, ([, identifier]) => identifier)).pipe(
        Match.when([HELD.tools, TOOL], () =>
            Predicate.isString(to)
                ? Effect.try({ try: () => toolbox(app), catch: refused }).pipe(
                      Effect.flatMap(Unmatched.single('tool', Option.some(to), (action: MenuAction) => [action.name])),
                      Effect.flatMap((action) => Effect.try({ try: action.invoke.bind(action), catch: refused })),
                  )
                : new Cause.IllegalArgumentError(`${text} takes the name of a tool, received ${typeof to}`),
        ),
        Match.when([HELD.panels, Match.string], ([, name]) =>
            Effect.try({ try: () => app.panels.itemByName(name), catch: refused }).pipe(
                Effect.filterOrFail(
                    ({ isValid }) => isValid,
                    () => Unmatched.of({ subject: 'panel', query: Option.some(name) }),
                ),
                Effect.flatMap((panel) => Dom.write(panel, { visible: Predicate.isBoolean(to) ? to : !panel.visible })),
                Effect.flatten,
            ),
        ),
        Match.when([HELD.menus, Match.string, Match.string], (identifiers) => {
            const specifier = Array.lastNonEmpty(identifiers);
            return Schema.decodeEffect(Specifier.FromString)(specifier).pipe(
                Effect.flatMap(Dom.locate),
                Effect.filterOrFail(owning('invoke'), () => Unmatched.of({ subject: 'menu action', query: Option.some(specifier) })),
                Effect.filterOrFail(
                    (action) => to === undefined || (Predicate.isBoolean(to) && Option.isNone(Dom.unavailable(action, 'checked', false))),
                    () => new Cause.IllegalArgumentError(`${text} invokes with no value, or takes the boolean its checked state turns to`),
                ),
                Effect.flatMap((action) => (to === undefined || to !== action.checked ? Effect.try({ try: action.invoke.bind(action), catch: refused }) : Effect.void)),
            );
        }),
        Match.orElse(() => Unmatched.of({ subject: 'facet control', query: Option.some(text) })),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { describe, operate };

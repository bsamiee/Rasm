import { Description, type Source } from '@rasm/applications/description';
import { HostError, Specifier } from '@rasm/object-model/model';
import { Unmatched } from '@rasm/search/unmatched';
import type { Plugin } from '@rasm/uxp/plugin';
import type { Event, Panel } from 'adobe:indesign';
import { Array, Cause, Effect, Formatter, Match, Option, Predicate, pipe, Result, Schema, Stream, String, Struct } from 'effect';
import { type DefaultTreeAdapterMap, defaultTreeAdapter, html, parseFragment } from 'parse5';
import { Dom } from '#/uxp/dom';

// --- [OPERATIONS] ----------------------------------------------------------------------

const same = Schema.toEquivalence(Description.Locator);
const roots = Dom.useSync(({ app }): readonly [typeof Description.Locator.Type, typeof Description.Locator.Type] => [[[app.menuActions.constructorName, app.constructorName]], [[app.panels.constructorName, app.constructorName]]]);
const describe = Effect.fn('describe')(function* ({ within }: Parameters<Source['describe']>[0]) {
    const [{ app }, [menuRoot, panelRoot]] = yield* Effect.all([Dom, roots]);
    const { panels, menuActions } = app;
    const areaAt = (named: string) => Array.append(menuRoot, [menuActions.constructorName, named] as const);
    const levelAt = (named: string) => Array.append(areaAt(named), ['menu' satisfies Description['containers'][number]['kind'], named] as const);
    if (within === undefined) {
        return yield* Effect.try({
            try: () =>
                Description.make({
                    containers: [
                        {
                            locator: panelRoot,
                            kind: 'panel',
                            controls: pipe(
                                Array.makeBy(panels.length, (index) => Option.fromUndefinedOr(panels.item(index))),
                                Array.getSomes,
                                Array.filter(({ name }) => name.length > 0),
                                Array.map(({ constructorName, name, visible }): Description['containers'][number]['controls'][number] => ({ locator: Array.append(panelRoot, [constructorName, name] as const), kind: 'toggle', role: constructorName, label: { text: name }, value: { _tag: 'bool', value: visible } })),
                            ),
                        },
                        {
                            locator: menuRoot,
                            kind: 'menuBar',
                            controls: Array.map(Array.dedupe(menuActions.everyItem().area), (area): Description['containers'][number]['controls'][number] => ({ locator: areaAt(area), kind: 'item', role: menuActions.constructorName, label: { text: area }, opens: levelAt(area) })),
                        },
                    ],
                }),
            catch: HostError.refused('panels and menu areas'),
        });
    }
    const [, area] = Array.lastNonEmpty(within);
    yield* same(within, areaAt(area)) ? Effect.void : Unmatched.of({ subject: 'menu area', query: Option.some(area), candidates: Array.dedupe(menuActions.everyItem().area) });
    return yield* Effect.try({
        try: () => {
            const { area: areas, id: ids } = menuActions.everyItem();
            return Description.make({
                containers: [
                    {
                        locator: levelAt(area),
                        kind: 'menu',
                        title: area,
                        parent: menuRoot,
                        controls: Array.map(
                            Array.filterMap(Array.zip(areas, ids), ([named, id]) => (named === area ? Result.succeed(menuActions.itemByID(id)) : Result.failVoid)),
                            (action): Description['containers'][number]['controls'][number] => ({
                                locator: Array.append(areaAt(area), [action.constructorName, action.toSpecifier()] as const),
                                kind: 'item',
                                role: action.constructorName,
                                label: { text: action.name },
                                ...(Option.isSome(Dom.unavailable(action, 'enabled', false)) ? {} : { enabled: action.enabled, ...(action.checked ? { mark: '✓' } : {}) }),
                            }),
                        ),
                    },
                ],
            });
        },
        catch: HostError.refused(area),
    });
});
const operate = Effect.fn('operate')(function* (payload: Parameters<Source['operate']>[0]) {
    const { app, owning } = yield* Dom;
    const { panels, menuActions } = app;
    const text = Formatter.format(payload.locator);
    const refused = HostError.refused(text);
    const { locator, to } = yield* Match.valueTags(payload, {
        press: (press) => Effect.succeed(press),
        pointer: () => Unmatched.of({ subject: 'canvas of the workspace', query: Option.some(text) }),
    });
    yield* Match.value([Array.headNonEmpty(locator)[0], ...Array.map(Array.tailNonEmpty(locator), ([, identifier]) => identifier)]).pipe(
        Match.when([panels.constructorName, Match.string], ([, name]) =>
            Effect.try({ try: () => panels.itemByName(name), catch: refused }).pipe(
                Effect.filterOrFail(
                    ({ isValid }) => isValid,
                    () => Unmatched.of({ subject: 'panel', query: Option.some(name) }),
                ),
                Effect.flatMap((panel) => Effect.flatten(Dom.write(panel, { visible: Predicate.isBoolean(to) ? to : !panel.visible } satisfies Partial<Panel>))),
            ),
        ),
        Match.when([menuActions.constructorName, Match.string, Match.string], (identifiers) =>
            Schema.decodeEffect(Specifier.FromString)(Array.lastNonEmpty(identifiers)).pipe(
                Effect.flatMap(Dom.locate),
                Effect.filterOrFail(owning('invoke'), () => Unmatched.of({ subject: 'menu action', query: Option.some(Array.lastNonEmpty(identifiers)) })),
                Effect.filterOrFail(
                    (action) => to === undefined || (Predicate.isBoolean(to) && Option.isNone(Dom.unavailable(action, 'checked', false))),
                    () => new Cause.IllegalArgumentError(`${text} invokes with no value, or takes the boolean its checked state turns to`),
                ),
                Effect.flatMap((action) => (to === undefined || to !== action.checked ? Effect.try({ try: action.invoke.bind(action), catch: refused }) : Effect.void)),
            ),
        ),
        Match.orElse(() => Unmatched.of({ subject: 'control of the workspace', query: Option.some(text) })),
    );
});
const renderText = (markup: string) => {
    const rendered = (node: DefaultTreeAdapterMap['childNode']): string =>
        Option.match(Option.liftPredicate(node, defaultTreeAdapter.isElementNode), {
            onNone: () => (defaultTreeAdapter.isTextNode(node) ? node.value : ''),
            onSome: ({ tagName, childNodes }) => (html.getTagID(tagName) === html.TAG_ID.BR ? '\n' : childNodes.map(rendered).join('')),
        });
    const lines = parseFragment(markup).childNodes.map(rendered).join('').split('\n');
    return Option.liftPredicate(
        lines
            .map((line) => line.replaceAll(/\s+/gu, ' ').trim())
            .join('\n')
            .trim(),
        String.isNonEmpty,
    );
};
const toolbar = {
    current: Dom.use(({ app: { toolBoxTools } }) => Effect.try({ try: () => ({ id: toolBoxTools.currentTool.toString(), label: toolBoxTools.currentToolName, ...Option.getOrUndefined(Option.map(renderText(toolBoxTools.currentToolHint), (help) => ({ help }))) }), catch: HostError.refused('current tool') })),
    listed: Dom.useSync(({ UITools, members }) => Array.filterMap(members(UITools), (tool) => (UITools.NONE.equals(tool) ? Result.failVoid : Result.succeed({ id: tool.toString(), label: tool.toString() })))),
    select: (id: string) => Dom.use(({ app }) => Effect.flatten(Dom.write(app.toolBoxTools, { currentTool: id }))),
    changes: Stream.merge<Event, HostError, Dom | Plugin, Event, HostError, Dom | Plugin>(
        Dom.events(({ app, ToolBox }) => [app.toolBoxTools, [ToolBox.AFTER_ATTRIBUTE_CHANGED]]),
        Dom.events(({ app, Application, Document, PageItem }) => [app, [Application.AFTER_CONTEXT_CHANGED, Document.AFTER_IMPORT, PageItem.AFTER_PLACE]]),
    ),
    pointer: Effect.flatMap(Effect.option(Dom.current), (document) =>
        Effect.try({
            try: () => ({ holding: Option.match(Option.filter(Option.map(document, Struct.get('placeGuns')), Struct.get('loaded')), { onNone: Array.empty<string>, onSome: ({ pageItems }) => Array.map(pageItems.everyItem().getElements(), (item) => item.toSpecifier()) }) }),
            catch: HostError.refused('place gun'),
        }),
    ),
};
const workspace = { roots, describe, operate };

// --- [EXPORTS] -------------------------------------------------------------------------

export { toolbar, workspace };

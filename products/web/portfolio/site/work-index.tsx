import { Array, Function, Option } from 'effect';
import type { ReactNode, Ref } from 'react';
import { entryTitle, format } from '#/media/labels.ts';
import type { Portfolio } from '#/model/portfolio.ts';
import { UrlFragment, type useNavigation } from '#/site/navigation.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const reveal = (link: HTMLAnchorElement | null) => link?.scrollIntoView({ block: 'nearest', behavior: 'instant' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const WorkIndex = ({ portfolio, navigation }: { portfolio: typeof Portfolio.Type; navigation: ReturnType<typeof useNavigation> }) => (
    <nav aria-label="Work index" className="work-index">
        <a className="min-h-11 self-start pb-5.75 eyebrow text-muted hover:text-accent-text" href={UrlFragment.work}>
            Index
        </a>
        <div className="index-list">
            {Array.match(portfolio.entries, {
                onEmpty: () => (
                    <a className="group index-link" href={UrlFragment.work}>
                        <span className="col-span-full">Selected work</span>
                    </a>
                ),
                onNonEmpty: (entries) =>
                    entries.map((entry) => (
                        <a
                            aria-current={navigation.active === entry.id ? 'location' : undefined}
                            className="group index-link ordinal before:font-mono before:text-accent-text before:tabular-nums"
                            href={UrlFragment(entry.id)}
                            key={entry.id}
                            onClick={(event) => {
                                if (!(event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) && event.button === 0) {
                                    event.preventDefault();
                                    navigation.jump(entry);
                                }
                            }}
                            ref={navigation.active === entry.id ? reveal : undefined}
                        >
                            <span className="wrap-anywhere underline-offset-4 group-hover:underline group-aria-current:underline">{entryTitle(entry)}</span>
                        </a>
                    )),
            })}
        </div>
    </nav>
);
const StickyBar = ({ portfolio, navigation, children, ref }: { portfolio: typeof Portfolio.Type; navigation: ReturnType<typeof useNavigation>; children: ReactNode; ref: Ref<HTMLDivElement> }) => (
    <div className="sticky top-0 z-30" ref={ref}>
        {children}
        {portfolio.entries.length > 0 && (
            <nav aria-label="Work index" className="hidden max-md:block">
                <label className="flex items-center justify-between gap-4 border-b border-line bg-background px-(--page-gutter) py-2">
                    <span className="eyebrow">Index</span>
                    <select
                        className="control min-w-0 flex-1 truncate border-0 shadow-none"
                        onChange={(event) =>
                            Option.match(
                                Array.findFirst(portfolio.entries, ({ id }) => id === event.target.value),
                                { onNone: Function.constVoid, onSome: navigation.jump },
                            )
                        }
                        value={navigation.active ?? ''}
                    >
                        <option disabled value="">
                            Select work
                        </option>
                        {portfolio.entries.map((entry, index) => (
                            <option key={entry.id} value={entry.id}>
                                [{format.numeral(index + 1, portfolio.entries.length)}] {entryTitle(entry)}
                            </option>
                        ))}
                    </select>
                </label>
            </nav>
        )}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { StickyBar, WorkIndex };

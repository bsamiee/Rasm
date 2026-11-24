import { useAtomSet, useAtomValue } from '@effect/atom-react';
import { Array, Function, Option } from 'effect';
import type { ReactNode } from 'react';
import { entryTitle, format } from '#/media/labels';
import { activeEntry, entryHref, jumpToEntry, stickyBar, workAnchor } from '#/site/navigation';
import { shown } from '#/site/state';

// --- [OPERATIONS] ----------------------------------------------------------------------

const reveal = (link: HTMLAnchorElement | null) => link?.scrollIntoView({ block: 'nearest', behavior: 'instant' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const WorkIndex = () => {
    const { entries } = useAtomValue(shown);
    const active = Option.getOrUndefined(useAtomValue(activeEntry));
    const jump = useAtomSet(jumpToEntry);
    return (
        <nav aria-label="Work index" className="work-index">
            <a className="eyebrow" href={workAnchor.href}>
                Index
            </a>
            <div className="index-list">
                {Array.match(entries, {
                    onEmpty: () => (
                        <a className="index-link" href={workAnchor.href}>
                            <span>Selected work</span>
                        </a>
                    ),
                    onNonEmpty: (listed) =>
                        listed.map((entry) => (
                            <a
                                aria-current={active === entry.id ? 'location' : undefined}
                                className="index-link ordinal"
                                href={entryHref(entry)}
                                key={entry.id}
                                onClick={(event) => {
                                    if (!(event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) && event.button === 0) {
                                        event.preventDefault();
                                        jump(entry);
                                    }
                                }}
                                ref={active === entry.id ? reveal : undefined}
                            >
                                <span>{entryTitle(entry)}</span>
                            </a>
                        )),
                })}
            </div>
        </nav>
    );
};
const StickyBar = ({ children }: { readonly children: ReactNode }) => {
    const { entries } = useAtomValue(shown);
    const active = useAtomValue(activeEntry);
    const jump = useAtomSet(jumpToEntry);
    return (
        <div className="sticky-bar" ref={useAtomSet(stickyBar)}>
            {children}
            {entries.length > 0 && (
                <nav aria-label="Work index">
                    <label>
                        <span className="eyebrow">Index</span>
                        <select className="control" onChange={(event) => Array.findFirst(entries, ({ id }) => id === event.target.value).pipe(Option.match({ onNone: Function.constVoid, onSome: jump }))} value={Option.getOrElse(active, () => '')}>
                            <option disabled value="">
                                Select work
                            </option>
                            {entries.map((entry, index) => (
                                <option key={entry.id} value={entry.id}>
                                    [{format.numeral(index + 1, entries.length)}] {entryTitle(entry)}
                                </option>
                            ))}
                        </select>
                    </label>
                </nav>
            )}
        </div>
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { StickyBar, WorkIndex };

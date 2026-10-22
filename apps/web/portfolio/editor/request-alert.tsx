import { Match, SchemaIssue } from 'effect';
import type { Atom } from 'effect/reactivity';
import type { deleteRequest, draft, saveRequest } from './state.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type RequestFailure = Atom.Failure<typeof saveRequest>['error'] | Atom.Failure<typeof draft> | Atom.Failure<typeof deleteRequest>;

// --- [COMPOSITION] ---------------------------------------------------------------------

const RequestAlert = <E extends RequestFailure>({ error, conflict, onField }: { error: E; conflict?: string; onField?: (path: readonly PropertyKey[]) => void } & ([Extract<E, { readonly _tag: 'Conflict' }>] extends [never] ? unknown : { conflict: string })) => (
    <div className="border-accent-text bg-surface border-l-3 p-4 text-sm" role="alert">
        {Match.type<RequestFailure>().pipe(
            Match.tag('SchemaError', ({ issue }) => (
                <>
                    <p>Review these fields:</p>
                    <ul>
                        {SchemaIssue.makeFormatterStandardSchemaV1()(issue).issues.map(({ path: segments, message }) => {
                            const path = segments?.map((segment) => (typeof segment === 'object' ? segment.key : segment)) ?? [];
                            const text = `${path.map(String).join(' › ')}: ${message}`;
                            return (
                                <li key={text}>
                                    {onField === undefined ? (
                                        message
                                    ) : (
                                        <button className="text-accent-text block text-left underline" onClick={() => onField(path)} type="button">
                                            {text}
                                        </button>
                                    )}
                                </li>
                            );
                        })}
                    </ul>
                </>
            )),
            Match.tag('PreconditionFailed', () => <p>A newer draft was saved in another tab. Your changes remain here. Reloading replaces these unsaved edits with that draft.</p>),
            Match.tag('Forbidden', () => <p>Sign in with the owner account to edit.</p>),
            Match.tag('Conflict', () => <p>{conflict}</p>),
            Match.tag('ServiceUnavailable', () => <p>The service is unavailable. Your changes remain here.</p>),
            Match.tag('HttpClientError', () => <p>The request could not finish. Your changes remain here. Try again.</p>),
            Match.exhaustive,
        )(error)}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { RequestAlert };

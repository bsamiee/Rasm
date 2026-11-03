import { Issues } from '@rasm/ui/form/issues';
import { Match } from 'effect';
import type { Atom } from 'effect/reactivity';
import { Button } from 'react-aria-components';
import type { deleteRequest, discardRequest, draft, saveRequest } from '#/editor/state.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type RequestFailure = Atom.Failure<typeof saveRequest>['error'] | Atom.Failure<typeof draft> | Atom.Failure<typeof deleteRequest> | Atom.Failure<typeof discardRequest>['error'];

// --- [COMPOSITION] ---------------------------------------------------------------------

const RequestAlert = <E extends RequestFailure>({ error, conflict, onField }: { error: E; conflict?: string; onField?: (path: readonly PropertyKey[]) => void } & ([Extract<E, { readonly _tag: 'Conflict' }>] extends [never] ? unknown : { conflict: string })) => (
    <div className="border-accent-text bg-surface border-l-3 p-4 text-sm" role="alert">
        {Match.typeTags<RequestFailure>()({
            SchemaError: ({ issue }) => (
                <>
                    <p>Invalid fields</p>
                    <Issues.List issue={issue} onField={onField} />
                </>
            ),
            PlacementConflict: ({ placements }) => (
                <>
                    <p>Draft places files missing from library or of another type. Remove each placement or upload its file again</p>
                    <ul>
                        {placements.map(({ path, assetId }) => (
                            <li key={path.join('.')}>{onField === undefined ? assetId : <Button onPress={() => onField(path)}>{path.join(' › ')}</Button>}</li>
                        ))}
                    </ul>
                </>
            ),
            PreconditionFailed: ({ revision }) => <p>Draft revision {revision} saved in another tab. Edits stay here until reload replaces them or replacing keeps them</p>,
            Forbidden: () => <p>Sign in as owner to edit</p>,
            NotFound: () => <p>Upload data already removed. Refresh list</p>,
            Conflict: () => <p>{conflict}</p>,
            ServiceUnavailable: () => <p>Service unavailable. Edits stay here</p>,
            HttpClientError: () => <p>Request failed. Edits stay here. Try again</p>,
        })(error)}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { RequestAlert };

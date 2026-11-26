import { Issues } from '@rasm/ui/issues';
import { Match, type Schema } from 'effect';
import type { HttpClientError } from 'effect/http';
import type { HttpApi, HttpApiEndpoint, HttpApiGroup } from 'effect/http-api';
import { Button } from 'react-aria-components';
import type { Api } from '#/model/api';

// --- [TYPES] ---------------------------------------------------------------------------

type RequestFailure = (typeof Api extends HttpApi.HttpApi<string, infer Groups> ? HttpApiEndpoint.Errors<HttpApiGroup.Endpoints<Groups>> : never) | HttpClientError.HttpClientError | Schema.SchemaError;

// --- [COMPOSITION] ---------------------------------------------------------------------

const RequestAlert = <E extends RequestFailure>({ error, conflict, onField }: { readonly error: E; readonly conflict?: string; readonly onField?: (path: readonly PropertyKey[]) => void } & ([Extract<E, { readonly _tag: 'Conflict' }>] extends [never] ? unknown : { readonly conflict: string })) => (
    <div className="request-alert" role="alert">
        {Match.value<RequestFailure>(error).pipe(
            Match.tag('SchemaError', ({ issue }) => (
                <>
                    <p>Invalid fields</p>
                    <Issues.List issue={issue} onField={onField} />
                </>
            )),
            Match.tag('PlacementConflict', ({ placements }) => (
                <>
                    <p>Draft places files missing from library or of another type. Remove each placement or upload its file again</p>
                    <ul>
                        {placements.map(({ path, assetId }) => (
                            <li key={path.join('.')}>{onField === undefined ? assetId : <Button onPress={() => onField(path)}>{path.join(' › ')}</Button>}</li>
                        ))}
                    </ul>
                </>
            )),
            Match.tag('PreconditionFailed', ({ revision }) => <p>Draft revision {revision} saved in another tab. Edits stay here until reload replaces them or replacing keeps them</p>),
            Match.tag('UploadRefused', ({ message }) => <p>{message}</p>),
            Match.tag('Forbidden', () => <p>Sign in as owner to edit</p>),
            Match.tag('NotFound', () => <p>Upload data already removed. Refresh list</p>),
            Match.tag('Conflict', () => <p>{conflict}</p>),
            Match.tag('ServiceUnavailable', () => <p>Service unavailable. Edits stay here</p>),
            Match.tag('HttpClientError', () => <p>Request failed. Edits stay here. Try again</p>),
            Match.exhaustive,
        )}
    </div>
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { RequestAlert };

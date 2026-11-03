import { Unmatched } from '@rasm/search/unmatched';
import { Schema } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Execute, Terms } from '#/apple-events';
import { Installation, Perform, Pid, RunningInstance, Screenshot, State } from '#/applications';
import { AvailableFace } from '#/available-face';

// --- [ERRORS] --------------------------------------------------------------------------

const error = Schema.Union([Unmatched, Schema.ErrorInstance()]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Contract = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'On subscribe and each Spotlight update, or discovery error' }), error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('instances', { success: Schema.Array(RunningInstance).annotate({ description: 'On subscribe and each change, exited instances kept until next change' }), stream: true }),
    Rpc.make('applications', { payload: { toOpen: Schema.URL }, success: Schema.Array(Schema.URL).annotate({ description: 'Launch Services applications for file, default first' }) }),
    Rpc.make('status', { payload: { pid: Pid }, success: State, error }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Schema.Struct({ pid: Pid.annotate({ description: 'Process ID once launch finishes' }) }), error: Schema.ErrorInstance() }),
    Rpc.make('perform', { payload: { pid: Pid, ...Perform.fields }, error }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: Terms.Kind, name: Schema.String }, success: Schema.String, error }),
    Rpc.make('execute', { payload: { pid: Pid, ...Execute.fields }, success: Execute.Result, error }),
    Rpc.make('screenshot', { payload: { pid: Pid, ...Screenshot.fields, file: Schema.URL.annotate({ description: 'PNG file written' }) }, success: Screenshot.Result, error }),
    Rpc.make('faces', { success: Schema.Array(AvailableFace) }),
    Rpc.make('face', { payload: { name: AvailableFace.Name }, success: AvailableFace, error: Unmatched }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract };

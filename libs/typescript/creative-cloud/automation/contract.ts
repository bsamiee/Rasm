import { Unmatched } from '@rasm/search';
import { Schema } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { AppleEventValue, Execute, Kind, Terms } from '#/apple-events';
import { Installation, Perform, Pid, RunningInstance, Screenshot, State } from '#/applications';
import { AvailableFace, FaceName } from '#/fonts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Contract = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'On subscribe and each Spotlight update, or discovery error' }), error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('instances', { success: Schema.Array(RunningInstance).annotate({ description: 'On subscribe and each change, exited instances kept until next change' }), stream: true }),
    Rpc.make('applications', { payload: { toOpen: Schema.URL }, success: Schema.Array(Schema.URL).annotate({ description: 'Launch Services applications for file, default first' }) }),
    Rpc.make('status', { payload: { pid: Pid }, success: State, error: Unmatched.Refusal }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Schema.Struct({ pid: Pid.annotate({ description: 'Process ID once launch finishes' }) }), error: Schema.ErrorInstance() }),
    Rpc.make('perform', { payload: { pid: Pid, ...Perform.fields }, error: Unmatched.Refusal }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: Kind, name: Schema.String }, success: Schema.String, error: Unmatched.Refusal }),
    Rpc.make('execute', { payload: { pid: Pid, ...Execute.fields }, success: Execute.Result, error: Unmatched.Refusal }),
    Rpc.make('script', {
        payload: { pid: Pid, source: Schema.String.annotate({ description: 'Source for dictionary script command' }) },
        success: AppleEventValue,
        error: Unmatched.Refusal,
    }),
    Rpc.make('screenshot', { payload: { pid: Pid, ...Screenshot.fields }, success: Screenshot.Result, error: Unmatched.Refusal }),
    Rpc.make('faces', { success: Schema.Array(AvailableFace) }),
    Rpc.make('face', { payload: { name: FaceName }, success: AvailableFace, error: Unmatched }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract };

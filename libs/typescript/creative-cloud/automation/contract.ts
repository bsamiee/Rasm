import { Unmatched } from '@rasm/search/unmatched.ts';
import { Schema } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { AppleEventValue, Execute, Kind, Terms } from './apple-events.ts';
import { Installation, Perform, Pid, RunningInstance, Screenshot, State } from './applications.ts';
import { AvailableFace, FaceName } from './fonts.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Contract = RpcGroup.make(
    Rpc.make('installations', { success: Schema.Result(Schema.Array(Installation), Schema.ErrorInstance()).annotate({ description: 'Installed applications at subscription and on each Spotlight update, or why discovery failed' }), error: Schema.ErrorInstance(), stream: true }),
    Rpc.make('instances', {
        success: Schema.Array(RunningInstance).annotate({ description: 'Running instances with states at subscription and after each change, an exited instance listed until the next change' }),
        stream: true,
    }),
    Rpc.make('applications', { payload: { toOpen: Schema.URL }, success: Schema.Array(Schema.URL).annotate({ description: 'Applications Launch Services opens the file with, default first' }) }),
    Rpc.make('status', { payload: { pid: Pid }, success: State, error: Unmatched.Refusal }),
    Rpc.make('launch', { payload: { url: Schema.URL }, success: Schema.Struct({ pid: Pid.annotate({ description: 'Process of the instance once it finished launching' }) }), error: Schema.ErrorInstance() }),
    Rpc.make('perform', { payload: { pid: Pid, ...Perform.fields }, error: Unmatched.Refusal }),
    Rpc.make('terms', { payload: { url: Schema.URL }, success: Terms, error: Schema.ErrorInstance() }),
    Rpc.make('definition', { payload: { url: Schema.URL, kind: Kind, name: Schema.String }, success: Schema.String, error: Unmatched.Refusal }),
    Rpc.make('execute', { payload: { pid: Pid, ...Execute.fields }, success: Execute.Result, error: Unmatched.Refusal }),
    Rpc.make('script', {
        payload: { pid: Pid, source: Schema.String.annotate({ description: 'Source the dictionary script command runs' }) },
        success: AppleEventValue,
        error: Unmatched.Refusal,
    }),
    Rpc.make('screenshot', { payload: { pid: Pid, ...Screenshot.fields }, success: Screenshot.Result, error: Unmatched.Refusal }),
    Rpc.make('faces', { success: Schema.Array(AvailableFace) }),
    Rpc.make('face', { payload: { name: FaceName }, success: AvailableFace, error: Unmatched }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract };

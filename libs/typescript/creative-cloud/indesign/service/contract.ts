import { Unmatched } from '@rasm/search';
import { Schema } from 'effect';
import { Rpc, RpcGroup, RpcMiddleware } from 'effect/rpc';
import { FindChange, Queries } from '../find.ts';
import { Font } from '../fonts.ts';
import { Inspect, Kind } from '../inspection.ts';
import { Change, Create, Remove } from '../mutations.ts';
import { Preflight } from '../preflight.ts';
import { Reference } from '../references.ts';
import { Render } from '../render.ts';
import { Script } from '../scripts.ts';
import { Invoke, Menu } from '../session.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Hold extends RpcMiddleware.Service<Hold>()('@rasm/indesign/service/contract/Hold', { error: Schema.ErrorInstance() }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const InDesign = RpcGroup.make(
    Rpc.make('inspect', { payload: Inspect, success: Inspect.Result, error: Unmatched.Refusal }),
    Rpc.make('change', { payload: Change, success: Change.Result, error: Unmatched.Refusal }),
    Rpc.make('create', { payload: Create, success: Reference, error: Unmatched.Refusal }),
    Rpc.make('remove', { payload: Remove, success: Reference, error: Unmatched.Refusal }),
    Rpc.make('execute_script', { payload: Script, success: Script.Result, error: Unmatched.Refusal }),
    Rpc.make('find_change', { payload: FindChange, success: FindChange.Result, error: Unmatched.Refusal }),
    Rpc.make('font', { payload: Font, success: Font.Result, error: Unmatched.Refusal }),
    Rpc.make('kind', { payload: Kind, success: Kind.Result, error: Unmatched.Refusal }),
    Rpc.make('render', { payload: Render, success: Render.Result, error: Unmatched.Refusal }),
    Rpc.make('menu', { payload: Menu, success: Menu.Result, error: Unmatched.Refusal }),
    Rpc.make('invoke', { payload: Invoke, success: Menu.Action, error: Unmatched.Refusal }),
    Rpc.make('preflight', { payload: Preflight, success: Preflight.Result, error: Unmatched.Refusal }),
    Rpc.make('queries', { payload: Queries, success: Queries.Result, error: Unmatched.Refusal }),
).middleware(Hold);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Hold, InDesign };

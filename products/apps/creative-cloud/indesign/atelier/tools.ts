import { Adjust } from '@rasm/indesign/typesetting';
import { Call, Change, FindChange, Inspect, Inspection, Preflight } from '@rasm/indesign/views';
import { Failure, WriteResult } from '@rasm/object-model/rpcs';
import { Tool, Toolkit } from 'effect/ai';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Tools = Toolkit.make(
    Tool.make('read', { description: 'Reads object fields and typed view, or collection members, / for application. File fields read as paths', parameters: Inspect, success: Inspection, failure: Failure }).annotate(Tool.Readonly, true),
    Tool.make('change', { description: 'Writes object fields in one undo step, failures name every refused field by path', parameters: Change, success: WriteResult, failure: Failure }),
    Tool.make('call', { description: 'Calls object or collection method in one undo step, atelier://object-model/indesign/{name} lists its parameters', parameters: Call, success: Call.Result, failure: Failure }),
    Tool.make('find_change', { description: 'Finds or changes matches in one undo step and restores search preferences, values take change forms', parameters: FindChange, success: FindChange.Result, failure: Failure }),
    Tool.make('adjust', { description: 'Adds kerning, tracking, baseline shift or mark offsets to target text in one undo step, / for text defaults', parameters: Adjust, success: Adjust.Result, failure: Failure }),
    Tool.make('preflight', { description: 'Reads preflight rows per document, the finished live results when no profile is given, each read waiting for the preflight process every live check restarts', parameters: Preflight, success: Preflight.Result, failure: Failure }).annotate(Tool.Readonly, true),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tools };

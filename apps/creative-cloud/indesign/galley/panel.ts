import { panel } from '@rasm/indesign/service/panel';
import { plugin } from '@rasm/uxp/entry';
import { Schedule } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

plugin(panel(Schedule.spaced('2 seconds')), { commands: {}, panels: {} });

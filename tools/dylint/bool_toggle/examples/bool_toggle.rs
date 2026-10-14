use std::process::ExitCode;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug)]
struct Switch {
    on: bool,
    level: u8,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn deref_place(start: bool) -> bool {
    let mut on = start;
    let flag = &mut on;
    *flag = !*flag;
    on
}

const fn local_place(start: bool) -> bool {
    let mut on = start;
    on = !on;
    on
}

const fn field_place(switch: &mut Switch) { switch.on = !switch.on; }

const fn indexed_place(flags: &mut [bool; 2]) { flags[1] = !flags[1]; }

const fn xor_place(start: bool) -> bool {
    let mut on = start;
    let flag = &mut on;
    *flag ^= true;
    on
}

const fn integer_place(switch: &mut Switch) { switch.level = !switch.level; }

const fn other_place(switch: &mut Switch, source: bool) { switch.on = !source; }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() -> ExitCode {
    let flag = xor_place(deref_place(local_place(false)));
    let mut flags = [false, true];
    let mut switch = Switch { on: false, level: 0 };
    indexed_place(&mut flags);
    field_place(&mut switch);
    integer_place(&mut switch);
    other_place(&mut switch, flag);
    ExitCode::from(switch.level.wrapping_add(u8::from(switch.on && flags[1])))
}

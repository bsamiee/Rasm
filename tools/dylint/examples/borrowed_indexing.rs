use core::hint::black_box;
use std::collections::{BTreeMap, HashMap, VecDeque};

use indexmap::IndexMap;

// --- [OPERATIONS] ----------------------------------------------------------------------

fn ordered(ports: &BTreeMap<u32, u16>) -> u16 { ports[&1] }

fn hashed(ports: &HashMap<&str, u16>) -> u16 { ports["http"] }

fn inserted(ports: &IndexMap<u32, u16>) -> u16 { ports[&2] }

fn owned(ports: BTreeMap<u32, u16>) -> u16 { ports[&1] }

struct Registry {
    ports: HashMap<u32, u16>,
}

fn field(registry: &Registry) -> u16 { registry.ports[&4] }

fn looked_up(ports: &BTreeMap<u32, u16>) -> Option<u16> { ports.get(&1).copied() }

fn listed(ports: &[u16]) -> Option<u16> { ports.first().copied() }

macro_rules! port {
    ($ports:expr) => {
        $ports[&3]
    };
}

fn expanded(ports: &BTreeMap<u32, u16>) -> u16 { port!(ports) }

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let ordered_ports = BTreeMap::from([(1, 80), (3, 443)]);
    black_box(&(
        ordered(&ordered_ports),
        hashed(&HashMap::from([("http", 80)])),
        inserted(&IndexMap::from([(2, 22)])),
        field(&Registry { ports: HashMap::from([(4, 8)]) }),
        looked_up(&ordered_ports),
        listed(&[1]),
        expanded(&ordered_ports),
        owned(ordered_ports),
        (queued(&VecDeque::from([1, 2, 3, 4])), listed_vec(&vec![1])),
    ));
}

fn queued(queue: &VecDeque<u16>) -> u16 { queue[3] }

fn listed_vec(ports: &Vec<u16>) -> u16 { ports[0] }

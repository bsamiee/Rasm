use core::hint::black_box;

use tokio::sync::{broadcast, mpsc, watch};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Debug)]
struct Progress(Option<watch::Sender<u32>>);

#[derive(Debug)]
struct Events {
    sent: Option<broadcast::Sender<u32>>,
    closing: Option<watch::Sender<bool>>,
    toggled: Option<watch::Sender<u32>>,
    branched: Option<watch::Sender<u32>>,
    queued: Option<mpsc::UnboundedSender<u32>>,
}

#[derive(Debug)]
pub struct Exported {
    pub sent: Option<watch::Sender<u32>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

impl Progress {
    fn new(tracked: bool) -> Self { Self(tracked.then(|| watch::Sender::new(0))) }

    fn stage(&self) {
        if let Some(sent) = &self.0 {
            sent.send_modify(|step| *step = step.saturating_add(1));
        }
    }

    fn report(&self, step: u32) {
        if let Some(sent) = self.0.as_ref() {
            sent.send_replace(step);
        }
    }
}

impl Events {
    fn publish(&self, event: u32) {
        if let Some(sent) = &self.sent {
            black_box(sent.send(event).ok());
        }
    }

    fn close(&mut self) { drop(self.closing.take()); }

    fn reset(&mut self) { self.toggled = None; }

    fn touch(&self) {
        if let Some(toggled) = &self.toggled {
            toggled.send_replace(0);
        }
    }

    fn either(&self) -> u32 { if let Some(branched) = &self.branched { *branched.borrow() } else { 0 } }

    fn queue(&self, event: u32) {
        if let Some(queued) = &self.queued {
            black_box(queued.send(event).ok());
        }
    }

    fn present(&self) -> bool { self.closing.is_some() }
}

fn local(tracked: bool) {
    let sent = tracked.then(|| watch::Sender::new(0_u32));
    if let Some(sent) = &sent {
        sent.send_replace(1);
    }
}

fn counted(tracked: bool) -> usize {
    let sent = tracked.then(|| watch::Sender::new(0_u32));
    sent.as_ref().map_or(0, watch::Sender::receiver_count)
}

pub fn exported(exported: &Exported) {
    if let Some(sent) = &exported.sent {
        sent.send_replace(1);
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let progress = Progress::new(true);
    progress.stage();
    progress.report(2);
    let mut events = Events { sent: None, closing: None, toggled: None, branched: None, queued: None };
    events.publish(1);
    events.touch();
    events.reset();
    events.queue(1);
    black_box((events.either(), events.present()));
    events.close();
    local(true);
    black_box((counted(true), progress.clone()));
    exported(&Exported { sent: None });
}

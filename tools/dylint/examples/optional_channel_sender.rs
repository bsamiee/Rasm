use core::hint::black_box;

use tokio::sync::watch;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Debug)]
struct Progress(Option<watch::Sender<u32>>);

#[derive(Debug)]
struct Events {
    closing: Option<watch::Sender<bool>>,
    toggled: Option<watch::Sender<u32>>,
    branched: Option<watch::Sender<u32>>,
}

#[derive(Debug)]
struct Unsent {
    sent: Option<watch::Sender<u32>>,
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
    fn close(&mut self) {
        if let Some(closing) = &self.closing {
            closing.send_replace(true);
        }
        drop(self.closing.take());
    }

    fn reset(&mut self) {
        if let Some(toggled) = &self.toggled {
            toggled.send_replace(0);
        }
        self.toggled = None;
    }

    fn either(&self) -> u32 { if let Some(branched) = &self.branched { *branched.borrow() } else { 0 } }
}

fn local(tracked: bool) {
    let sent = tracked.then(|| watch::Sender::new(0_u32));
    if let Some(sent) = &sent {
        sent.send_replace(1);
    }
}

fn forwarded(sent: Option<watch::Sender<u32>>) {
    if let Some(sent) = &sent {
        sent.send_replace(2);
    }
}

fn split(unsent: Unsent) -> Option<watch::Sender<u32>> {
    if let Some(sent) = &unsent.sent {
        sent.send_replace(3);
    }
    let Unsent { sent } = unsent;
    sent
}

fn counted(tracked: bool) -> usize {
    let sent = tracked.then(|| watch::Sender::new(0_u32));
    sent.as_ref().map_or(0, watch::Sender::receiver_count)
}

// --- [COMPOSITION] ---------------------------------------------------------------------

fn main() {
    let progress = Progress::new(true);
    progress.stage();
    progress.report(2);
    let mut events = Events { closing: None, toggled: None, branched: None };
    events.close();
    events.reset();
    local(true);
    forwarded(None);
    black_box(&(events.either(), counted(true), progress.clone(), split(Unsent { sent: None })));
}

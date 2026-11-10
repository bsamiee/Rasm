use core::num::{NonZero, TryFromIntError};
use core::{iter, mem};
use std::collections::{BTreeMap, BTreeSet};

use enum_map::EnumMap;
use itertools::Itertools as _;
use tokio::time::Instant;

use crate::{Claim, Digest, Kind, Label, ProvisioningError, SessionName, StoreId};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub(crate) enum Tenure {
    Called { until: Option<Instant> },
    Held { holds: NonZero<usize> },
    Released { until: Option<Instant> },
}

#[derive(Debug, Clone, PartialEq, Eq, derive_more::IsVariant)]
pub(crate) enum Phase {
    Pending { claim: Option<Digest> },
    Held,
    Kept { until: Option<Instant> },
    Build { digest: Digest },
    Ready { digest: Digest },
    Leaving,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub(crate) struct Entry {
    pub(crate) session: Option<SessionName>,
    pub(crate) phase: Phase,
}

#[derive(Debug)]
struct Session {
    tenure: Tenure,
    stores: BTreeSet<(Kind, StoreId)>,
}

#[derive(Debug)]
enum Stage {
    Open,
    Sealed { template: StoreId, used: Instant, ready: Vec<StoreId> },
}

#[derive(Debug, Clone, PartialEq, Eq, PartialOrd, Ord)]
enum Due {
    Session(SessionName),
    Store(Kind, StoreId),
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Default)]
pub(crate) struct Ledger {
    sessions: BTreeMap<SessionName, Session>,
    stores: EnumMap<Kind, BTreeMap<StoreId, Entry>>,
    limits: EnumMap<Kind, Option<u32>>,
    claims: BTreeMap<(Kind, Digest), Stage>,
    deadlines: BTreeSet<(Instant, Due)>,
}

impl Ledger {
    // --- [SESSIONS]

    fn retenure(&mut self, session: &SessionName, tenure: Tenure) {
        let held = self.sessions.entry(session.clone()).or_insert_with(|| Session { tenure, stores: BTreeSet::new() });
        if let Some(before) = ending(mem::replace(&mut held.tenure, tenure)) {
            self.deadlines.remove(&(before, Due::Session(session.clone())));
        }
        self.deadlines.extend(ending(tenure).map(|until| (until, Due::Session(session.clone()))));
    }

    pub(crate) fn touch(&mut self, session: &SessionName, until: Option<Instant>) -> Tenure {
        match self.sessions.get(session).map(|held| held.tenure) {
            None | Some(Tenure::Called { until: _ }) => {
                self.retenure(session, Tenure::Called { until });
                Tenure::Called { until }
            }
            Some(tenure @ (Tenure::Held { holds: _ } | Tenure::Released { until: _ })) => tenure,
        }
    }

    pub(crate) fn hold(&mut self, session: &SessionName) {
        let holds = match self.sessions.get(session).map(|held| held.tenure) {
            Some(Tenure::Held { holds }) => holds.saturating_add(1),
            None | Some(Tenure::Called { until: _ } | Tenure::Released { until: _ }) => NonZero::<usize>::MIN,
        };
        self.retenure(session, Tenure::Held { holds });
    }

    pub(crate) fn release(&mut self, session: &SessionName, until: Option<Instant>) {
        if let Some(Tenure::Held { holds }) = self.sessions.get(session).map(|held| held.tenure) {
            self.retenure(session, holds.get().checked_sub(1).and_then(NonZero::new).map_or(Tenure::Released { until }, |holds| Tenure::Held { holds }));
        }
    }

    fn own(&mut self, session: &SessionName, kind: Kind, id: StoreId) {
        if let Some(owner) = self.sessions.get_mut(session) {
            owner.stores.insert((kind, id));
        }
    }

    pub(crate) fn end(&mut self, session: &SessionName) -> Vec<(Kind, StoreId)> {
        let ended = self.sessions.remove(session);
        if let Some(until) = ended.as_ref().and_then(|ended| ending(ended.tenure)) {
            self.deadlines.remove(&(until, Due::Session(session.clone())));
        }
        ended.into_iter().flat_map(|ended| ended.stores).filter(|&(kind, id)| self.orphan(kind, id)).collect()
    }

    fn orphan(&mut self, kind: Kind, id: StoreId) -> bool {
        self.stores[kind].get_mut(&id).is_some_and(|entry| match mem::replace(&mut entry.phase, Phase::Leaving) {
            Phase::Held => true,
            Phase::Build { digest } => {
                self.claims.remove(&(kind, digest));
                true
            }
            Phase::Pending { claim } => {
                if let Some(digest) = claim {
                    self.claims.remove(&(kind, digest));
                }
                false
            }
            kept @ Phase::Kept { until: _ } => {
                entry.phase = kept;
                false
            }
            Phase::Ready { digest: _ } | Phase::Leaving => false,
        })
    }

    pub(crate) fn next(&self) -> Option<Instant> { self.deadlines.first().map(|(at, _)| *at) }

    pub(crate) fn expire(&mut self, now: Instant) -> Vec<(Kind, StoreId)> {
        let due = Vec::from_iter(iter::from_fn(|| self.deadlines.first().is_some_and(|(at, _)| *at <= now).then(|| self.deadlines.pop_first()).flatten()));
        due.into_iter()
            .flat_map(|(_, due)| match due {
                Due::Session(session) => self.end(&session),
                Due::Store(kind, id) => Vec::from_iter(self.stores[kind].get_mut(&id).filter(|entry| entry.phase.is_kept()).map(|entry| {
                    entry.phase = Phase::Leaving;
                    (kind, id)
                })),
            })
            .collect()
    }

    // --- [STORES]

    pub(crate) fn bound(&mut self, kind: Kind, limit: Option<u32>) { self.limits[kind] = limit; }

    pub(crate) fn vacant(&self, kind: Kind) -> bool { !self.limits[kind].is_some_and(|limit| usize::try_from(limit).is_ok_and(|limit| self.stores[kind].len() >= limit)) }

    pub(crate) fn admit(&mut self, kind: Kind, id: StoreId, session: Option<(&SessionName, Option<Instant>)>, claim: Option<&Digest>) -> Result<(), ProvisioningError> {
        let (claimed, live) = (claim.filter(|digest| self.claims.contains_key(&(kind, (*digest).clone()))), self.stores[kind].len());
        match (claimed, self.limits[kind].map(usize::try_from)) {
            (Some(digest), _) => Err(ProvisioningError::Template { digest: digest.clone(), claim: Claim::Claimed }),
            (None, Some(Ok(limit))) if live >= limit => Err(ProvisioningError::Exhausted { kind }),
            (None, Some(Ok(_) | Err(TryFromIntError { .. })) | None) => {
                if let Some((session, until)) = session {
                    self.touch(session, until);
                    self.own(session, kind, id);
                }
                self.claims.extend(claim.map(|digest| ((kind, digest.clone()), Stage::Open)));
                self.stores[kind].insert(id, Entry { session: session.map(|(session, _)| session.clone()), phase: Phase::Pending { claim: claim.cloned() } });
                Ok(())
            }
        }
    }

    pub(crate) fn commit(&mut self, kind: Kind, id: StoreId, phase: Phase) -> bool {
        let stocked = |claims: &mut BTreeMap<(Kind, Digest), Stage>| match &phase {
            Phase::Ready { digest } => match claims.get_mut(&(kind, digest.clone())) {
                Some(Stage::Sealed { template: _, used: _, ready }) => {
                    ready.push(id);
                    true
                }
                Some(Stage::Open) | None => false,
            },
            Phase::Pending { claim: _ } | Phase::Held | Phase::Kept { until: _ } | Phase::Build { digest: _ } | Phase::Leaving => true,
        };
        let pending = self.stores[kind].get_mut(&id).filter(|entry| entry.phase.is_pending());
        pending.filter(|_| stocked(&mut self.claims)).map(|entry| entry.phase = phase).is_some()
    }

    pub(crate) fn forget(&mut self, kind: Kind, id: StoreId) {
        let Some(Entry { session, phase }) = self.stores[kind].remove(&id) else { return };
        if let Some(owner) = session.and_then(|session| self.sessions.get_mut(&session)) {
            owner.stores.remove(&(kind, id));
        }
        match phase {
            Phase::Build { digest } | Phase::Pending { claim: Some(digest) } => drop(self.claims.remove(&(kind, digest))),
            Phase::Ready { digest } =>
                if let Some(Stage::Sealed { template: _, used: _, ready }) = self.claims.get_mut(&(kind, digest)) {
                    ready.retain(|held| *held != id);
                },
            Phase::Kept { until: Some(until) } => drop(self.deadlines.remove(&(until, Due::Store(kind, id)))),
            Phase::Pending { claim: None } | Phase::Held | Phase::Kept { until: None } | Phase::Leaving => {}
        }
    }

    pub(crate) fn locate(&self, id: StoreId) -> Option<(Kind, &Entry)> { self.stores.iter().find_map(|(kind, stores)| stores.get(&id).map(|entry| (kind, entry))) }

    pub(crate) fn keep(&mut self, kind: Kind, id: StoreId, until: Option<Instant>) -> bool {
        let kept = self.stores[kind].get_mut(&id).filter(|entry| entry.phase.is_held() || entry.phase.is_kept()).map(|entry| mem::replace(&mut entry.phase, Phase::Kept { until }));
        if let Some(Phase::Kept { until: Some(before) }) = kept {
            self.deadlines.remove(&(before, Due::Store(kind, id)));
        }
        self.deadlines.extend(kept.as_ref().and(until).map(|until| (until, Due::Store(kind, id))));
        kept.is_some()
    }

    pub(crate) fn recover(&mut self, kind: Kind, id: StoreId, label: Label, until: Option<Instant>, now: Instant) -> bool {
        let recovered = match label {
            Label::Held { session } => {
                self.touch(&session, until);
                self.own(&session, kind, id);
                Some(Some(Entry { session: Some(session), phase: Phase::Held }))
            }
            Label::Kept { session } => {
                self.deadlines.extend(until.map(|until| (until, Due::Store(kind, id))));
                Some(Some(Entry { session: Some(session), phase: Phase::Kept { until } }))
            }
            Label::Template { digest } => {
                self.claims.insert((kind, digest), Stage::Sealed { template: id, used: now, ready: Vec::new() });
                Some(None)
            }
            Label::Ready { digest } => match self.claims.get_mut(&(kind, digest.clone())) {
                Some(Stage::Sealed { template: _, used: _, ready }) => {
                    ready.push(id);
                    Some(Some(Entry { session: None, phase: Phase::Ready { digest } }))
                }
                Some(Stage::Open) | None => None,
            },
            Label::Build { session: _, digest: _ } => None,
        };
        recovered.map(|entry| self.stores[kind].extend(entry.map(|entry| (id, entry)))).is_some()
    }

    // --- [TEMPLATES]

    pub(crate) fn building(&self, kind: Kind, digest: &Digest) -> bool {
        match self.claims.get(&(kind, digest.clone())) {
            Some(Stage::Open) => true,
            Some(Stage::Sealed { template: _, used: _, ready: _ }) | None => false,
        }
    }

    pub(crate) fn seal(&mut self, kind: Kind, id: StoreId, now: Instant, bound: NonZero<usize>) -> Option<Vec<StoreId>> {
        let digest = self.stores[kind].get(&id).and_then(|entry| match &entry.phase {
            Phase::Build { digest } => Some(digest.clone()),
            Phase::Pending { claim: _ } | Phase::Held | Phase::Kept { until: _ } | Phase::Ready { digest: _ } | Phase::Leaving => None,
        })?;
        self.forget(kind, id);
        self.claims.insert((kind, digest), Stage::Sealed { template: id, used: now, ready: Vec::new() });
        let sealed = self.claims.iter().filter_map(|((held, digest), claim)| match claim {
            Stage::Sealed { template: _, used, ready: _ } if *held == kind => Some((*used, digest.clone())),
            Stage::Sealed { template: _, used: _, ready: _ } | Stage::Open => None,
        });
        let evicted = sealed.sorted().rev().skip(bound.get()).flat_map(|(_, digest)| match self.claims.remove(&(kind, digest)) {
            Some(Stage::Sealed { template, used: _, ready }) => {
                for id in &ready {
                    self.stores[kind].entry(*id).and_modify(|entry| entry.phase = Phase::Leaving);
                }
                iter::once(template).chain(ready).collect()
            }
            Some(Stage::Open) | None => Vec::new(),
        });
        Some(evicted.collect())
    }

    pub(crate) fn take(&mut self, kind: Kind, digest: &Digest, session: &SessionName, until: Option<Instant>, now: Instant) -> Option<(StoreId, Option<StoreId>)> {
        let (template, taken) = match self.claims.get_mut(&(kind, digest.clone()))? {
            Stage::Sealed { template, used, ready } => {
                *used = now;
                Some((*template, ready.pop()))
            }
            Stage::Open => None,
        }?;
        if let Some(entry) = taken.and_then(|id| self.stores[kind].get_mut(&id)) {
            *entry = Entry { session: Some(session.clone()), phase: Phase::Pending { claim: None } };
            self.touch(session, until);
            if let Some(id) = taken {
                self.own(session, kind, id);
            }
        }
        Some((template, taken))
    }

    pub(crate) fn stock(&mut self, kind: Kind, digest: &Digest, id: StoreId, bound: usize) -> Result<Option<(StoreId, Option<StoreId>)>, ProvisioningError> {
        let pools = self.claims.iter().filter_map(|((held, other), claim)| match claim {
            Stage::Sealed { template, used, ready } if *held == kind => Some((*used, other, *template, ready.len())),
            Stage::Sealed { template: _, used: _, ready: _ } | Stage::Open => None,
        });
        let (stocked, own, colder) = pools.fold((0_usize, None, None), |(stocked, own, colder), (used, other, template, ready)| {
            let colder = iter::chain(colder, (other != digest && ready > 0).then_some((used, other.clone()))).min();
            (stocked.saturating_add(ready), own.or_else(|| (other == digest).then_some((used, template))), colder)
        });
        let Some((used, template)) = own else { return Ok(None) };
        let displaced = match colder {
            _ if stocked < bound => None,
            Some((cold, other)) if cold < used => match self.claims.get_mut(&(kind, other)) {
                Some(Stage::Sealed { template: _, used: _, ready }) => ready.pop(),
                Some(Stage::Open) | None => None,
            },
            Some(_) | None => return Ok(None),
        };
        if let Some(entry) = displaced.and_then(|gone| self.stores[kind].get_mut(&gone)) {
            entry.phase = Phase::Leaving;
        }
        self.admit(kind, id, None, None).map(|()| Some((template, displaced)))
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const fn ending(tenure: Tenure) -> Option<Instant> {
    match tenure {
        Tenure::Called { until } | Tenure::Released { until } => until,
        Tenure::Held { holds: _ } => None,
    }
}

use core::iter;
use core::time::Duration;
#[cfg(unix)]
use std::error::Report;
#[cfg(unix)]
use std::process;
use std::sync::Arc;
use std::sync::nonpoison::Mutex;

#[cfg(target_os = "macos")]
use libproc::proc_pid::pidinfo;
#[cfg(target_os = "macos")]
use libproc::task_info::TaskInfo;
#[cfg(unix)]
use nix::sys::resource::{UsageWho, getrusage};
#[cfg(unix)]
use nix::sys::time::TimeValLike as _;
use opentelemetry::KeyValue;
use opentelemetry::metrics::MeterProvider;
#[cfg(unix)]
use opentelemetry_semantic_conventions::attribute::EXCEPTION_MESSAGE;
use opentelemetry_semantic_conventions::attribute::{CPU_MODE, DISK_IO_DIRECTION, PROCESS_CONTEXT_SWITCH_TYPE, PROCESS_PID, SYSTEM_PAGING_FAULT_TYPE};
use opentelemetry_semantic_conventions::metric::{
    PROCESS_CONTEXT_SWITCHES, PROCESS_CPU_TIME, PROCESS_DISK_IO, PROCESS_MEMORY_USAGE, PROCESS_MEMORY_VIRTUAL, PROCESS_PAGING_FAULTS, PROCESS_THREAD_COUNT, PROCESS_UNIX_FILE_DESCRIPTOR_COUNT,
    PROCESS_UPTIME,
};
use sysinfo::{MINIMUM_CPU_UPDATE_INTERVAL, Pid, Process, ProcessRefreshKind, ProcessesToUpdate, System};
use tokio::time::Instant;

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Clone, Copy)]
struct Usage {
    cpu: Option<[Duration; 2]> = None,
    switches: Option<[u64; 2]> = None,
    faults: Option<[u64; 2]> = None,
    ticks: Option<[u64; 2]> = None,
    threads: Option<i64> = None,
}

impl Usage {
    fn read(pid: Pid) -> Self {
        #[cfg(unix)]
        let own = Some(pid.as_u32()).filter(|pid| *pid == process::id()).and_then(|_| {
            let usage = getrusage(UsageWho::RUSAGE_SELF);
            usage.inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = %Report::new(error), "resource usage unreadable")).ok()
        });
        #[cfg(target_os = "macos")]
        let task = i32::try_from(pid.as_u32()).ok().and_then(|pid| {
            let task = pidinfo::<TaskInfo>(pid, 0);
            task.inspect_err(|error| tracing::debug!({ EXCEPTION_MESSAGE } = error.as_str(), { PROCESS_PID } = pid, "task information unreadable")).ok()
        });
        cfg_select! {
            unix => Self {
                cpu: own.and_then(|usage| [usage.user_time(), usage.system_time()].try_map(|spent| u64::try_from(spent.num_microseconds()).ok().map(Duration::from_micros))),
                switches: own.and_then(|usage| [usage.voluntary_context_switches(), usage.involuntary_context_switches()].try_map(|count| u64::try_from(count).ok())),
                faults: own.and_then(|usage| [usage.major_page_faults(), usage.minor_page_faults()].try_map(|count| u64::try_from(count).ok())),
                #[cfg(target_os = "macos")]
                ticks: task.map(|task| [task.pti_total_user, task.pti_total_system]),
                #[cfg(target_os = "macos")]
                threads: task.map(|task| i64::from(task.pti_threadnum)),
                ..
            },
            _ => {{
                let _: Pid = pid;
                Self { .. }
            }}
        }
    }

    fn cpu(self, spent: Duration) -> Result<[Duration; 2], Duration> {
        let split = || {
            let ticks = self.ticks?;
            let whole = u128::from(ticks[0]).checked_add(u128::from(ticks[1]))?;
            ticks.try_map(|part| u64::try_from(spent.as_nanos().checked_mul(u128::from(part))?.checked_div(whole)?).ok().map(Duration::from_nanos))
        };
        self.cpu.or_else(split).ok_or(spent)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn process<I>(provider: &dyn MeterProvider, pids: impl Fn() -> I + Send + Sync + 'static) -> Result<(), sysinfo::Error>
where I: IntoIterator<Item = u32> {
    fn measured<I, T>(read: &(impl Fn() -> I, Mutex<(System, Vec<(Pid, Usage)>, Option<Instant>)>), measure: impl Fn(&Process, Usage) -> T) -> Vec<(KeyValue, T)>
    where I: IntoIterator<Item = u32> {
        let (listed, held) = read;
        let mut guard = held.lock();
        let (system, pids, refreshed) = &mut *guard;
        if refreshed.is_none_or(|at| at.elapsed() >= MINIMUM_CPU_UPDATE_INTERVAL) {
            let named = Vec::from_iter(listed().into_iter().map(Pid::from_u32));
            system.refresh_processes_specifics(ProcessesToUpdate::Some(&named), true, ProcessRefreshKind::nothing().with_memory().with_cpu().with_disk_usage());
            *pids = named.into_iter().map(|pid| (pid, Usage::read(pid))).collect();
            *refreshed = Some(Instant::now());
        }
        pids.iter().filter_map(|&(pid, usage)| Some((KeyValue::new(PROCESS_PID, i64::from(pid.as_u32())), measure(system.process(pid)?, usage)))).collect()
    }
    const BYTES: &str = "By";
    const SECONDS: &str = "s";
    let pids = Arc::new((pids, Mutex::new((System::new()?, Vec::new(), None))));
    let meter = provider.meter_with_scope(crate::scope(env!("CARGO_PKG_NAME")));
    let counts: &[(&str, &str, fn(&Process, Usage) -> Option<i64>)] = &[
        (PROCESS_MEMORY_USAGE, BYTES, |process, _| Some(process.memory().saturating_cast())),
        (PROCESS_MEMORY_VIRTUAL, BYTES, |process, _| Some(process.virtual_memory().saturating_cast())),
        (PROCESS_THREAD_COUNT, "{thread}", |process, usage| usage.threads.or_else(|| process.tasks().map(|tasks| tasks.len().saturating_cast()))),
        #[cfg(unix)]
        (PROCESS_UNIX_FILE_DESCRIPTOR_COUNT, "{file_descriptor}", |process, _| process.open_files().map(usize::saturating_cast)),
    ];
    for &(name, unit, measure) in counts {
        let pids = Arc::clone(&pids);
        let observed = move || measured(&pids, measure).into_iter().filter_map(|(pid, count)| Some((pid, count?)));
        meter.i64_observable_up_down_counter(name).with_unit(unit).with_callback(move |observer| observed().for_each(|(pid, count)| observer.observe(count, &[pid]))).build();
    }
    let kinds: [(&str, &str, &str, [&str; 2], fn(&Process, Usage) -> Option<[u64; 2]>); 3] = [
        (PROCESS_CONTEXT_SWITCHES, "{context_switch}", PROCESS_CONTEXT_SWITCH_TYPE, ["voluntary", "involuntary"], |_, usage| usage.switches),
        (PROCESS_PAGING_FAULTS, "{fault}", SYSTEM_PAGING_FAULT_TYPE, ["major", "minor"], |_, usage| usage.faults),
        (PROCESS_DISK_IO, BYTES, DISK_IO_DIRECTION, ["read", "write"], |process, _| {
            let moved = process.disk_usage();
            Some([moved.total_read_bytes, moved.total_written_bytes])
        }),
    ];
    for (name, unit, key, kinds, measure) in kinds {
        let pids = Arc::clone(&pids);
        let observed = move || measured(&pids, measure).into_iter().filter_map(|(pid, counts)| Some((pid, counts?)));
        meter
            .u64_observable_counter(name)
            .with_unit(unit)
            .with_callback(move |observer| {
                for (pid, counts) in observed() {
                    for (kind, count) in iter::zip(kinds, counts) {
                        observer.observe(count, &[pid.clone(), KeyValue::new(key, kind)]);
                    }
                }
            })
            .build();
    }
    let (spent, lived) = (Arc::clone(&pids), pids);
    meter
        .f64_observable_counter(PROCESS_CPU_TIME)
        .with_unit(SECONDS)
        .with_callback(move |observer| {
            for (pid, spent) in measured(&spent, |process, usage| usage.cpu(Duration::from_millis(process.accumulated_cpu_time()))) {
                match spent {
                    Ok(modes) => iter::zip(["user", "system"], modes).for_each(|(mode, spent)| observer.observe(spent.as_secs_f64(), &[pid.clone(), KeyValue::new(CPU_MODE, mode)])),
                    Err(total) => observer.observe(total.as_secs_f64(), &[pid]),
                }
            }
        })
        .build();
    meter
        .f64_observable_gauge(PROCESS_UPTIME)
        .with_unit(SECONDS)
        .with_callback(move |observer| measured(&lived, |process, _| Duration::from_secs(process.run_time()).as_secs_f64()).into_iter().for_each(|(pid, seconds)| observer.observe(seconds, &[pid])))
        .build();
    Ok(())
}

import { Effect, Record, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Pid = Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Process ID' });
const ProcessInstance = Schema.String.check(Schema.isPattern(/^\d+@\d+$/u))
    .pipe(Schema.brand('ProcessInstance'))
    .annotate({ description: 'Process ID and kernel start time as <pid>@<start>, unequal for a later process reusing the ID' });
const ProcessStatus = Schema.Literals(['running', 'runnable', 'throttled', 'uninterruptible', 'sleeping', 'stopped', 'traced']).annotate({
    description:
        'Main thread state: running after time on a processor, runnable while waiting for one, throttled or suspended by the kernel under resource starvation, uninterruptible while waiting in the kernel on a page-in or disk read, sleeping while waiting on an event, stopped by a signal or suspension, traced while a debugger holds it stopped',
});
const Usage = Schema.Struct({
    cpu: Schema.Finite.annotate({ description: 'Processor seconds' }),
    runnable: Schema.Finite.annotate({ description: 'Seconds ready to run while waiting for a processor' }),
    pageWait: Schema.Finite.annotate({ description: 'Seconds waiting on page faults' }),
    diskBytesRead: Schema.Finite,
    diskBytesWritten: Schema.Finite,
    footprint: Schema.Finite.annotate({ description: 'Physical footprint bytes' }),
    threads: Schema.Int,
    runningThreads: Schema.Int.annotate({ description: 'Threads running or waiting for a processor at the reading, none while every thread waits on an event or lock' }),
    assertions: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Name of each power assertion the process holds, the long work it declares' })),
    assertionsFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Power assertion read failure' })),
}).annotate({ description: 'proc_pid_rusage counters since launch, with the footprint, threads, and power assertions at the reading' });
const UsageInterval = Schema.Struct({
    cause: Schema.Literals(['running', 'runnable', 'paging', 'reading', 'writing', 'idle']).annotate({ description: 'The processor, runnable, or page wait time covering half of the interval, else disk reads or writes, else idle' }),
    duration: Schema.Finite.annotate({ description: 'Seconds between the two readings' }),
    ...Struct.pick(Usage.fields, ['cpu', 'runnable', 'pageWait', 'diskBytesRead', 'diskBytesWritten']),
    end: Usage.annotate({ description: 'Reading at the end of the interval' }),
}).annotate({ description: 'Growth of proc_pid_rusage counters between two readings, with its cause' });
const Load = Schema.Struct({
    idle: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })).annotate({ description: 'Share of host_statistics64 HOST_CPU_LOAD_INFO processor ticks idle across every CPU, absent where both readings hold the same counters' })),
    ...Record.map({ pageins: 'Pages read in from disk', swapins: 'Pages swapped in from compressor segments', swapouts: 'Pages swapped out to compressor segments', decompressions: 'Pages decompressed' }, (counted) =>
        Schema.optionalKey(Schema.Finite.annotate({ description: `${counted}, absent where host_statistics64 served the earlier reading again, as it rate-limits callers outside the system` })),
    ),
    deviceUtilization: Schema.optionalKey(Schema.Int.annotate({ description: 'GPU Device Utilization %, the busiest accelerator, absent where the driver states none' })),
    diskBytesRead: Schema.Finite,
    diskBytesWritten: Schema.Finite,
    diskTime: Schema.Finite.annotate({ description: 'Seconds block storage drivers spent reading and writing' }),
    availableCapacity: Schema.Finite.annotate({ description: 'Bytes free on the startup volume at the end of the steps, NSURLVolumeAvailableCapacityKey, which counts no purgeable space' }),
    availableMemory: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 100 })).annotate({ description: 'kern.memorystatus_level at the end of the steps, percent of memory available' }),
}).annotate({ description: 'host_statistics64 processor time and paging counters, block storage statistics, and GPU utilization' });
class Pressure extends Schema.Opaque<Pressure>()(
    Schema.Struct({
        memory: Schema.Union([Schema.Literals(['normal', 'warning', 'critical']), Schema.TemplateLiteral([Schema.Int])]).annotate({ description: 'kern.memorystatus_vm_pressure_level' }),
        thermal: Schema.Union([Schema.Literals(['nominal', 'fair', 'serious', 'critical']), Schema.TemplateLiteral([Schema.Int])]).annotate({ description: 'ProcessInfo thermalState' }),
        lowPower: Schema.Boolean.annotate({ description: 'Low Power Mode' }),
        advisory: Schema.optionalKey(
            Schema.Record(Schema.String, Schema.Union([Schema.Literals(['bad', 'ok', 'great']), Schema.TemplateLiteral([Schema.Int])])).annotate({
                description: 'IOCopySystemLoadAdvisoryDetailed levels by key: CombinedLevel, the system verdict on running optional work now, and the UserLevel, BatteryLevel, and ThermalLevel constraints; absent where the call answered NULL',
            }),
        ),
    }).annotate({ description: 'System pressure on subscribe and each memory, thermal, power, or load advisory change, the raw value for a level this build names no case for' }),
) {}
const DiagnosticReport = Schema.Struct({
    url: Schema.URLFromString.annotate({ description: 'Report file in ~/Library/Logs/DiagnosticReports or /Library/Logs/DiagnosticReports' }),
    kind: Schema.Literals(['crash', 'hang', 'diag', 'spin']).annotate({ description: 'crash for an .ips report of bug_type 309, else the spindump text report extension' }),
    date: Schema.String.annotate({ description: 'ISO 8601 capture time: captureTime of a crash report, Date/Time of a text report' }),
    pid: Pid,
    launched: Schema.optionalKey(Schema.String.annotate({ description: 'ISO 8601 launch time to the millisecond, procLaunch of a crash report' })),
    name: Schema.optionalKey(Schema.String.annotate({ description: 'Process name' })),
    executable: Schema.optionalKey(Schema.String.annotate({ description: 'Executable path, user components redacted to * under /Users, /Volumes, and /private/tmp' })),
    coalition: Schema.optionalKey(Schema.String.annotate({ description: 'Resource coalition name of a crash report, the responsible app or job grouping the process' })),
    bundleIdentifier: Schema.optionalKey(Schema.String),
    event: Schema.optionalKey(Schema.String.annotate({ description: 'Text report Event line: hang, cpu usage, disk writes, wakeups' })),
    exception: Schema.optionalKey(
        Schema.Struct({
            type: Schema.String,
            signal: Schema.optionalKey(Schema.String),
            subtype: Schema.optionalKey(Schema.String),
            message: Schema.optionalKey(Schema.String),
            rawCodes: Schema.optionalKey(Schema.Array(Schema.Finite).annotate({ description: 'Mach exception codes as signed 64-bit numbers (mach_exception_data_type_t): the kern_return_t, then the code the kernel adds, negative where its top bit is set, the faulting address spelled in subtype' })),
        }).annotate({ description: 'Mach exception of a crash report: type (EXC_BAD_ACCESS), signal, subtype with the faulting address, message, codes' }),
    ),
    termination: Schema.optionalKey(
        Schema.Struct({
            namespace: Schema.String,
            code: Schema.optionalKey(Schema.Finite),
            indicator: Schema.optionalKey(Schema.String),
            byProc: Schema.optionalKey(Schema.String.annotate({ description: 'Process that terminated it' })),
            reasons: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'dyld or code signing lines: library not loaded, symbol not found, launch constraint violation' })),
            details: Schema.String.pipe(Schema.Array, Schema.optionalKey),
        }).annotate({ description: 'Kernel termination reason, CODESIGNING with Launch Constraint Violation for a refused launch, DYLD for a failed load' }),
    ),
    applicationSpecificInformation: Schema.optionalKey(Schema.Record(Schema.String, Schema.Array(Schema.String)).annotate({ description: 'Application Specific Information of a crash report (asi) by image: abort() called, a malloc corruption, a fork-safety violation' })),
    virtualMemoryRegionInfo: Schema.optionalKey(Schema.String.annotate({ description: 'Memory regions around the faulting address of a crash report (vmRegionInfo)' })),
    faultingThread: Schema.optionalKey(
        Schema.Array(Schema.Struct({ image: Schema.optionalKey(Schema.String), imageOffset: Schema.Finite, symbol: Schema.optionalKey(Schema.String), symbolLocation: Schema.optionalKey(Schema.Finite) })).annotate({
            description: 'Frames of the thread that triggered the crash, innermost first, each in its image with its symbol where the report resolved one',
        }),
    ),
});
const LogEntry = Schema.Struct({
    timestamp: Schema.String.annotate({ description: 'ISO 8601 time the event was logged' }),
    messageType: Schema.Literals(['Debug', 'Info', 'Default', 'Error', 'Fault']).annotate({ description: 'Log level as log names it, Default for notice' }),
    processID: Pid,
    processImagePath: Schema.String.annotate({ description: 'Executable of the logging process' }),
    senderImagePath: Schema.String.annotate({ description: 'Library or executable whose code logged the event' }),
    subsystem: Schema.String,
    category: Schema.String,
    eventMessage: Schema.String.annotate({ description: 'Message with its arguments composed' }),
}).annotate({ description: 'Unified log event as log show and log stream write it' });
const ended = {
    detail: Schema.optionalKey(Schema.Array(Schema.Literals(['memory', 'codeSigning', 'decryption'])).annotate({ description: 'Kernel exit detail: memory limit, invalid code signature, failed decryption' })),
    reporting: Schema.optionalKey(Schema.Boolean.annotate({ description: 'A crash report is on its way' })),
    report: Schema.optionalKey(DiagnosticReport),
    reportFailure: Schema.optionalKey(Schema.String.annotate({ description: 'Crash report read failure case: timedOut after 60 seconds, cancelled, corrupt for a report outside its documented format, or foundation or system for a failed read' })),
};
const Exited = Schema.TaggedUnion({
    status: { status: Schema.Int.annotate({ description: 'Exit status' }), ...ended },
    signal: { signal: Schema.Int.annotate({ description: 'Terminating signal number' }), ...ended },
    statusFailure: {
        statusFailure: Schema.String.annotate({ description: 'Why the kernel gave no exit status: Permission denied for a process this one may not signal, No such process for one gone before it was watched' }),
        ...ended,
    },
}).annotate({ description: 'Exit status, terminating signal, or why the kernel gave neither, with the kernel exit detail and the crash report of the exit' });
class Diagnostics extends Schema.Opaque<Diagnostics>()(
    Schema.Struct({
        application: Schema.optionalKey(Schema.Struct({ url: Schema.URL.annotate({ description: 'Bundle of the installation' }), instance: Schema.optionalKey(ProcessInstance) }).annotate({ description: 'Reports and unified log of the instance, else of the installation executable; absent reads the server itself' })),
        folders: Schema.Array(Schema.Struct({ url: Schema.URL.annotate({ description: 'Folder of log files' }), glob: Schema.optionalKey(Schema.String.annotate({ description: 'fnmatch pattern of file names, every file when absent' })) })).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
        since: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds back from now, else since the instance launched' })),
        limit: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Most reports, log lines, and unified log events returned' }),
        find: Schema.optionalKey(Schema.String.annotate({ description: 'Text a log line or unified log event holds, ignoring case, searched before the limit applies' })),
    }),
) {
    static readonly Result = Schema.Struct({
        reports: Schema.Array(DiagnosticReport).annotate({ description: 'Newest first' }),
        samples: Schema.Array(Schema.URLFromString).annotate({ description: 'sample reports of the instance taken as it turned unresponsive, newest first' }),
        logs: Schema.Array(Schema.Struct({ file: Schema.String.annotate({ description: 'File URL of the log file' }), text: Schema.String })).annotate({ description: 'Newest first across folders, by the JSON timestamp of a line, else file modification time' }),
        unified: Schema.Result(Schema.Array(LogEntry).annotate({ description: 'log show events, newest first' }), Schema.ErrorInstance()),
        unreadable: Schema.Array(Schema.String).annotate({ description: 'Report files, log folders, and log lines that failed to read, with cause' }),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Diagnostics, DiagnosticReport, Exited, Load, LogEntry, Pid, Pressure, ProcessInstance, ProcessStatus, Usage, UsageInterval };

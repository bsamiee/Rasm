#include <ApplicationServices/ApplicationServices.h>
#include <spawn.h>
#include <stdbool.h>

CF_ASSUME_NONNULL_BEGIN

// --- [TYPES] ---------------------------------------------------------------------------

typedef CF_OPTIONS(int32_t, CGSSpaceMask) {
    kCGSSpaceIncludesCurrent = 1 << 0,
    kCGSSpaceIncludesOthers = 1 << 1,
    kCGSSpaceIncludesUser = 1 << 2,
} CF_SWIFT_NAME(SpaceMask);

typedef CF_OPTIONS(uint32_t, CGSWindowCaptureOptions) {
    kCGSWindowCaptureBestResolution = 1 << 8,
    kCGSWindowCaptureIgnoreGlobalClipShape = 1 << 11,
} CF_SWIFT_NAME(WindowCaptureOptions);

// --- [CONSTANTS] -----------------------------------------------------------------------

extern const CFStringRef _kLSPermittedFrontASNsArrayKey CF_SWIFT_NAME(permittedFrontKey);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RESPONSIBILITY]

pid_t responsibility_get_pid_responsible_for_pid(pid_t process) CF_SWIFT_NAME(responsiblePID(_:));
int responsibility_spawnattrs_setdisclaim(posix_spawnattr_t _Nullable* _Nonnull attributes, int disclaim) CF_SWIFT_NAME(disclaim(_:_:));

// --- [PROCESSES]

bool CPSFindProcessByPID(pid_t process, ProcessSerialNumber* serial) CF_SWIFT_NAME(findProcess(_:_:));
CGError CPSGetFrontProcess(ProcessSerialNumber* serial) CF_SWIFT_NAME(frontSerial(_:));
CGError CPSGetKeyFocusProcess(ProcessSerialNumber* serial, uint8_t* flag) CF_SWIFT_NAME(keyFocusSerial(_:_:));
CGError SLPSAddToPermittedFrontPSNList(const ProcessSerialNumber* serial) CF_SWIFT_NAME(addToPermittedFrontList(_:));
CGError SLPSRemoveFromPermittedFrontPSNList(const ProcessSerialNumber* serial) CF_SWIFT_NAME(removeFromPermittedFrontList(_:));
CFTypeRef _Nullable _LSCopyMetaApplicationInformationItem(int session, CFStringRef key) CF_RETURNS_RETAINED CF_SWIFT_NAME(metaInformation(_:_:));

// --- [CONNECTIONS]

int CGSMainConnectionID(void) CF_SWIFT_NAME(mainConnection());
CGError CGSGetConnectionIDForPSN(int connection, const ProcessSerialNumber* serial, int* owner) CF_SWIFT_NAME(connectionOfSerial(_:_:_:));
CGError CGSConnectionGetPID(int connection, pid_t* process) CF_SWIFT_NAME(processOfConnection(_:_:));
CGError SLSNewConnection(int zero, int* connection) CF_SWIFT_NAME(newConnection(_:_:));
CGError SLSReleaseConnection(int connection) CF_SWIFT_NAME(releaseConnection(_:));

// --- [EVENTS]

CGError SLSGetEventPort(int connection, mach_port_t* port) CF_SWIFT_NAME(eventPort(_:_:));
CGEventRef _Nullable SLEventCreateNextEvent(int connection) CF_RETURNS_RETAINED CF_SWIFT_NAME(nextEvent(_:));
CGError
SLSRegisterConnectionNotifyProc(int connection, void (*callback)(uint32_t notification, void* _Nullable data, size_t length, void* _Nullable context), uint32_t notification, void* _Nullable context)
    CF_SWIFT_NAME(registerNotifyProc(_:_:_:_:));
CGError SLPSPostEventRecordTo(const ProcessSerialNumber* serial, const uint8_t* record) CF_SWIFT_NAME(postEventRecord(_:_:));
const void* _Nullable SLEventRecordPointer(CGEventRef event) CF_SWIFT_NAME(recordPointer(_:));
CGError SLSEventAppUnresponsiveStatus(int connection, const ProcessSerialNumber* serial, double* _Nullable seconds, uint32_t* _Nullable flags) CF_SWIFT_NAME(unresponsiveStatus(_:_:_:_:));
CGError SLSEventSetAppIsUnresponsiveNotificationTimeout(int connection, double seconds) CF_SWIFT_NAME(setUnresponsiveNotificationTimeout(_:_:));
void SLEventSetAuthenticationMessage(CGEventRef event, CFTypeRef message) CF_SWIFT_NAME(setAuthenticationMessage(_:_:));
void CGEventSetWindowLocation(CGEventRef event, CGPoint location) CF_SWIFT_NAME(setWindowLocation(_:_:));

// --- [WINDOWS]

AXError _AXUIElementGetWindow(AXUIElementRef element, CGWindowID* window) CF_SWIFT_NAME(windowNumber(_:_:));
CGError SLSRequestNotificationsForWindows(int connection, const CGWindowID* windows, int count) CF_SWIFT_NAME(requestNotifications(_:_:_:));
CGError SLSGetWindowOwner(int connection, CGWindowID window, int* owner) CF_SWIFT_NAME(windowOwner(_:_:_:));
CFTypeRef _Nullable SLSWindowQueryWindows(int connection, CFArrayRef windows, int count) CF_RETURNS_RETAINED CF_SWIFT_NAME(queryWindows(_:_:_:));
CFTypeRef _Nullable SLSWindowQueryResultCopyWindows(CFTypeRef query) CF_RETURNS_RETAINED CF_SWIFT_NAME(queriedWindows(_:));
bool SLSWindowIteratorAdvance(CFTypeRef iterator) CF_SWIFT_NAME(advance(_:));
CGWindowID SLSWindowIteratorGetWindowID(CFTypeRef iterator) CF_SWIFT_NAME(iteratedWindow(_:));
CGWindowID SLSWindowIteratorGetParentID(CFTypeRef iterator) CF_SWIFT_NAME(iteratedParent(_:));
CGError SLSFindWindowAndOwner(int connection, int zero, int one, int other, CGPoint* screen, CGPoint* local, CGWindowID* window, int* owner) CF_SWIFT_NAME(findWindowAndOwner(_:_:_:_:_:_:_:_:));
CFArrayRef _Nullable CGSHWCaptureWindowList(int connection, const uint32_t* windows, int count, CGSWindowCaptureOptions options) CF_RETURNS_RETAINED CF_SWIFT_NAME(captureWindowList(_:_:_:_:));

// --- [SPACES]

CFArrayRef _Nullable SLSCopySpaces(int connection, CGSSpaceMask mask) CF_RETURNS_RETAINED CF_SWIFT_NAME(copySpaces(_:_:));
CFArrayRef _Nullable SLSCopySpacesForWindows(int connection, CGSSpaceMask mask, CFArrayRef windows) CF_RETURNS_RETAINED CF_SWIFT_NAME(copySpacesForWindows(_:_:_:));
CFArrayRef _Nullable SLSCopyWindowsWithOptionsAndTags(int connection, uint32_t owner, CFArrayRef spaces, uint32_t options, uint64_t* setTags, uint64_t* clearTags) CF_RETURNS_RETAINED
    CF_SWIFT_NAME(copyWindowsWithOptionsAndTags(_:_:_:_:_:_:));

// --- [CURSOR]

int CGSCurrentCursorSeed(void) CF_SWIFT_NAME(currentCursorSeed());
CGError CGSGetCurrentCursorLocation(int connection, CGPoint* location) CF_SWIFT_NAME(currentCursorLocation(_:_:));
CGError CGSGetCursorScale(int connection, float* scale) CF_SWIFT_NAME(cursorScale(_:_:));
const char* _Nullable CGSCursorNameForSystemCursor(int cursor) CF_SWIFT_NAME(systemCursorName(_:));
CGError CGSGetGlobalCursorDataSize(int connection, size_t* length) CF_SWIFT_NAME(globalCursorDataSize(_:_:));
CGError CGSGetGlobalCursorData(int connection, void* _Nullable data, size_t* length, size_t* rowBytes, CGRect* bounds, CGPoint* hotSpot, size_t* depth, size_t* components, size_t* bits)
    CF_SWIFT_NAME(globalCursorData(_:_:_:_:_:_:_:_:_:));
CGError CGSGetSystemDefinedCursorDataSize(int connection, int cursor, size_t* length) CF_SWIFT_NAME(systemDefinedCursorDataSize(_:_:_:));
CGError
CGSGetSystemDefinedCursorData(int connection, int cursor, void* _Nullable data, size_t* length, size_t* rowBytes, CGRect* bounds, CGPoint* hotSpot, size_t* depth, size_t* components, size_t* bits)
    CF_SWIFT_NAME(systemDefinedCursorData(_:_:_:_:_:_:_:_:_:_:));
CGError CoreCursorGetDataSize(int connection, int cursor, size_t* length) CF_SWIFT_NAME(coreCursorDataSize(_:_:_:));
CGError CoreCursorGetData(int connection,
                          int cursor,
                          void* _Nullable data,
                          size_t* length,
                          size_t* rowBytes,
                          CGSize* size,
                          CGPoint* hotSpot,
                          size_t* depth,
                          size_t* components,
                          size_t* bits,
                          size_t* frames,
                          size_t* duration) CF_SWIFT_NAME(coreCursorData(_:_:_:_:_:_:_:_:_:_:_:_:));

CF_ASSUME_NONNULL_END

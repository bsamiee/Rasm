#include <AppKit/NSAccessibilityConstants.h>
#include <AppKit/NSCursor.h>
#include <ApplicationServices/ApplicationServices.h>
#include <lifetimebound.h>
#include <ptrcheck.h>
#include <spawn.h>
#include <stdbool.h>
#include <stddef.h>

CF_ASSUME_NONNULL_BEGIN

// --- [TYPES] ---------------------------------------------------------------------------

typedef CF_OPTIONS(uint32_t, CGSSpaceMask) {
    kCGSSpaceIncludesCurrent = 1U << 0,
    kCGSSpaceIncludesOthers = 1U << 1,
    kCGSSpaceIncludesUser = 1U << 2,
} CF_SWIFT_NAME(SpaceMask);

typedef CF_OPTIONS(uint32_t, SLSWindowCaptureOptions) {
    kSLSWindowCaptureBestResolution = 1U << 8,
    kSLSWindowCaptureIgnoreGlobalClipShape = 1U << 11,
} CF_SWIFT_NAME(WindowCaptureOptions);

typedef CF_OPTIONS(uint64_t, SLSWindowQueryOptions) {
    kSLSWindowQueryTitles = 1ULL << 0,
    kSLSWindowQueryAttachedWindows = 1ULL << 1,
    kSLSWindowQuerySpaces = 1ULL << 2,
    kSLSWindowQueryDisplays = 1ULL << 3,
} CF_SWIFT_NAME(WindowQueryOptions);

typedef CF_OPTIONS(uint64_t, SLSSpaceAttributes) {
    kSLSSpaceOrderedIn = 1ULL << 0,
    kSLSSpaceOrderedOut = 1ULL << 1,
    kSLSSpaceTiled = 1ULL << 2,
    kSLSSpaceTopLevel = 1ULL << 3,
} CF_SWIFT_NAME(SpaceAttributes);

typedef const struct CF_BRIDGED_TYPE(id) SLSWindowQuery* SLSWindowQueryRef CF_SWIFT_NAME(WindowQuery);
typedef const struct CF_BRIDGED_TYPE(id) SLSWindowIterator* SLSWindowIteratorRef CF_SWIFT_NAME(WindowIterator);
typedef const struct CF_BRIDGED_TYPE(id) SLSSpaceIterator* SLSSpaceIteratorRef CF_SWIFT_NAME(SpaceIterator);
typedef struct SLSEventRecord SLSEventRecord;

// --- [CONSTANTS] -----------------------------------------------------------------------

#define kCGSessionScreenIsLockedKey CFSTR("CGSSessionScreenIsLocked")
#define kScreenIsLockedNotification CFSTR("com.apple.screenIsLocked")
#define kScreenIsUnlockedNotification CFSTR("com.apple.screenIsUnlocked")
#define kAXSelectTextWithCriteriaParameterizedAttribute CFSTR("AXSelectTextWithCriteria")
#define kAXSelectTextActivity CFSTR("AXSelectTextActivity")
#define kAXSelectTextActivityFindAndReplace CFSTR("AXSelectTextActivityFindAndReplace")
#define kAXSelectTextReplacementString CFSTR("AXSelectTextReplacementString")
#define kAXSelectTextSearchStrings CFSTR("AXSelectTextSearchStrings")

extern const CFStringRef _kLSPermittedFrontASNsArrayKey CF_SWIFT_NAME(permittedFrontKey);
APPKIT_EXTERN const NSAccessibilityAttributeName NSAccessibilityContentSizeAttribute NS_SWIFT_NAME(contentSize);
APPKIT_EXTERN const NSAccessibilityAttributeName NSAccessibilityFullScreenAttribute NS_SWIFT_NAME(fullScreen);
APPKIT_EXTERN const NSAccessibilityAttributeName NSAccessibilityPreferredLanguageAttribute NS_SWIFT_NAME(preferredLanguage);
APPKIT_EXTERN const NSAccessibilityAttributeName NSAccessibilitySectionsAttribute NS_SWIFT_NAME(sections);
APPKIT_EXTERN const NSAccessibilityAttributeName NSAccessibilityUserInputLabelsAttribute NS_SWIFT_NAME(userInputLabels);
APPKIT_EXTERN const NSAccessibilitySubrole NSAccessibilitySegmentSubrole NS_SWIFT_NAME(segment);
APPKIT_EXTERN const NSAccessibilityNotificationName NSAccessibilityRemoteApplicationElementsAddedNotification NS_SWIFT_NAME(remoteApplicationElementsAdded);
APPKIT_EXTERN const NSAccessibilityNotificationName NSAccessibilityRemoteApplicationElementsRemovedNotification NS_SWIFT_NAME(remoteApplicationElementsRemoved);
APPKIT_EXTERN NSString* const NSAccessibilityRemoteApplicationElementsKey NS_SWIFT_NAME(remoteApplicationElementsKey);
APPKIT_EXTERN NSString* const NSAccessibilitySectionDescriptionKey NS_SWIFT_NAME(sectionDescriptionKey);
APPKIT_EXTERN NSString* const NSAccessibilitySectionObjectKey NS_SWIFT_NAME(sectionObjectKey);
static const CGEventType kCGSEventAppKitDefined CF_SWIFT_NAME(appKitDefinedEventType) = (CGEventType)NX_KITDEFINED;
static const CGEventField kCGSEventWindowNumber CF_SWIFT_NAME(windowNumberField) = (CGEventField)51;
static const CGEventField kCGSEventCompoundSubtype CF_SWIFT_NAME(compoundSubtypeField) = (CGEventField)83;
static const AXMenuItemModifiers kAXMenuItemModifierFunction CF_SWIFT_NAME(functionModifier) = (AXMenuItemModifiers)(1U << 4);

// --- [MODELS] --------------------------------------------------------------------------

__attribute__((objc_runtime_visible)) CF_SWIFT_NAME(EventAuthenticationMessage) @interface SLSEventAuthenticationMessage : NSObject
+ (nullable instancetype)messageWithEventRecord:(const SLSEventRecord*)record pid:(pid_t)pid version:(uint32_t)version;
@end

@interface NSCursor (SPI)
- (NSInteger)_coreCursorType CF_SWIFT_NAME(coreCursorType());
@end

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RESPONSIBILITY]

pid_t responsibility_get_pid_responsible_for_pid(pid_t process) CF_SWIFT_NAME(responsiblePID(_:));
int responsibility_spawnattrs_setdisclaim(posix_spawnattr_t _Nullable* _Nonnull attributes, bool disclaim) CF_SWIFT_NAME(disclaim(_:_:));

// --- [PROCESSES]

bool CPSFindProcessByPID(pid_t process, ProcessSerialNumber* serial) CF_SWIFT_NAME(findProcess(_:_:));
CGError CPSGetFrontProcess(ProcessSerialNumber* serial) CF_SWIFT_NAME(frontSerial(_:));
CGError CPSGetKeyFocusProcess(ProcessSerialNumber* serial, uint8_t* flag) CF_SWIFT_NAME(keyFocusSerial(_:_:));
CGError SLPSAddToPermittedFrontPSNList(const ProcessSerialNumber* serial) CF_SWIFT_NAME(addToPermittedFrontList(_:));
CGError SLPSRemoveFromPermittedFrontPSNList(const ProcessSerialNumber* serial) CF_SWIFT_NAME(removeFromPermittedFrontList(_:));
CFTypeRef _Nullable _LSCopyMetaApplicationInformationItem(int session, CFStringRef key) CF_RETURNS_RETAINED CF_SWIFT_NAME(metaInformation(_:_:));
AXError _AXUIElementGetActualPid(AXUIElementRef element, pid_t* process) CF_SWIFT_NAME(actualProcess(_:_:));

// --- [CONNECTIONS]

int CGSMainConnectionID(void) CF_SWIFT_NAME(mainConnection());
CGError CGSGetConnectionIDForPSN(int connection, const ProcessSerialNumber* serial, int* owner) CF_SWIFT_NAME(connectionOfSerial(_:_:_:));
CGError CGSConnectionGetPID(int connection, pid_t* process) CF_SWIFT_NAME(processOfConnection(_:_:));
CGError SLSConnectionGetPIDAndVersion(int connection, pid_t* process, uint32_t* version) CF_SWIFT_NAME(processAndVersionOfConnection(_:_:_:));
CGError SLSNewConnection(int zero, int* connection) CF_SWIFT_NAME(newConnection(_:_:));
CGError SLSReleaseConnection(int connection) CF_SWIFT_NAME(releaseConnection(_:));

// --- [EVENTS]

CGError SLSGetEventPort(int connection, mach_port_t* port) CF_SWIFT_NAME(eventPort(_:_:));
CGEventRef _Nullable SLEventCreateNextEvent(int connection) CF_RETURNS_RETAINED CF_SWIFT_NAME(nextEvent(_:));
CGError
SLSRegisterConnectionNotifyProc(int connection, void (*callback)(uint32_t notification, void* _Nullable data, size_t length, void* _Nullable context), uint32_t notification, void* _Nullable context)
    CF_SWIFT_NAME(registerNotifyProc(_:_:_:_:));
CGError SLPSPostEventRecordTo(const ProcessSerialNumber* serial, const SLSEventRecord* record) CF_SWIFT_NAME(postEventRecord(_:_:));
const SLSEventRecord* SLEventRecordPointer(CGEventRef event) CF_SWIFT_NAME(recordPointer(_:));
CGError SLSEventAppUnresponsiveStatus(int connection, const ProcessSerialNumber* serial, double* _Nullable seconds, uint32_t* _Nullable flags) CF_SWIFT_NAME(unresponsiveStatus(_:_:_:_:));
void SLEventSetAuthenticationMessage(CGEventRef event, SLSEventAuthenticationMessage* message) CF_SWIFT_NAME(setAuthenticationMessage(_:_:));
void CGEventSetWindowLocation(CGEventRef event, CGPoint location) CF_SWIFT_NAME(setWindowLocation(_:_:));

// --- [WINDOWS]

AXError _AXUIElementGetWindow(AXUIElementRef element, CGWindowID* window) CF_SWIFT_NAME(windowNumber(_:_:));
CGError SLSRequestNotificationsForWindows(int connection, const CGWindowID* __counted_by(count) __noescape windows, int count) CF_SWIFT_NAME(requestNotifications(_:_:_:));
SLSWindowQueryRef _Nullable SLSWindowQueryWindows(int connection, CFArrayRef windows, SLSWindowQueryOptions options) CF_RETURNS_RETAINED CF_SWIFT_NAME(queryWindows(_:_:_:));
SLSWindowIteratorRef _Nullable SLSWindowQueryResultCopyWindows(SLSWindowQueryRef query) CF_RETURNS_RETAINED CF_SWIFT_NAME(WindowIterator.init(_:));
bool SLSWindowIteratorAdvance(SLSWindowIteratorRef iterator) CF_SWIFT_NAME(WindowIterator.advance(self:));
CGWindowID SLSWindowIteratorGetWindowID(SLSWindowIteratorRef iterator) CF_SWIFT_NAME(getter:WindowIterator.window(self:));
CGWindowID SLSWindowIteratorGetParentID(SLSWindowIteratorRef iterator) CF_SWIFT_NAME(getter:WindowIterator.parent(self:));
SLSSpaceAttributes SLSWindowIteratorGetSpaceAttributes(SLSWindowIteratorRef iterator) CF_SWIFT_NAME(getter:WindowIterator.spaceAttributes(self:));
SLSSpaceIteratorRef _Nullable SLSWindowIteratorCopySpaces(SLSWindowIteratorRef iterator) CF_RETURNS_RETAINED CF_SWIFT_NAME(getter:WindowIterator.spaces(self:));
bool SLSSpaceIteratorAdvance(SLSSpaceIteratorRef iterator) CF_SWIFT_NAME(SpaceIterator.advance(self:));
uint64_t SLSSpaceIteratorGetSpaceID(SLSSpaceIteratorRef iterator) CF_SWIFT_NAME(getter:SpaceIterator.space(self:));
CGError SLSFindWindowAndOwner(int connection, int zero, int one, int other, CGPoint* screen, CGPoint* local, CGWindowID* window, int* owner) CF_SWIFT_NAME(findWindowAndOwner(_:_:_:_:_:_:_:_:));
CFArrayRef _Nullable SLSHWCaptureWindowList(int connection, const CGWindowID* __counted_by(count) __noescape windows, int count, SLSWindowCaptureOptions options) CF_RETURNS_RETAINED
    CF_SWIFT_NAME(captureWindowList(_:_:_:_:));

// --- [SPACES]

CFArrayRef _Nullable SLSCopySpaces(int connection, CGSSpaceMask mask) CF_RETURNS_RETAINED CF_SWIFT_NAME(copySpaces(_:_:));
CFArrayRef _Nullable SLSCopySpacesForWindows(int connection, CGSSpaceMask mask, CFArrayRef windows) CF_RETURNS_RETAINED CF_SWIFT_NAME(copySpacesForWindows(_:_:_:));

// --- [CURSOR]

int CGSCurrentCursorSeed(void) CF_SWIFT_NAME(currentCursorSeed());
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

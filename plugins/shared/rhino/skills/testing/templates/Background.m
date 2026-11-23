#import <AppKit/AppKit.h>
#import <objc/runtime.h>

// --- [CONSTANTS] -----------------------------------------------------------------------

static IMP OrderWindow;
static IMP SetActivationPolicy;
static IMP DocumentClassForType;
static IMP SetIsVisible;

// --- [OPERATIONS] ----------------------------------------------------------------------

static CFStringRef Domain(void) {
    static CFStringRef domain;
    const char* path = getenv("RHINO_TEST_PREFERENCES");
    if (domain == NULL && path != NULL) {
        domain = CFStringCreateWithCString(kCFAllocatorDefault, path, kCFStringEncodingUTF8);
    }
    return domain;
}

static CFStringRef Redirected(CFStringRef app) {
    return Domain() != NULL
                   && (app == kCFPreferencesCurrentApplication
                       || (app != NULL && (CFEqual(app, (__bridge CFStringRef)NSProcessInfo.processInfo.processName) || CFStringHasPrefix(app, CFSTR("com.mcneel.rhinoceros")))))
               ? Domain()
               : app;
}

static CFPropertyListRef CopyAppValue(CFStringRef key, CFStringRef app) { return CFPreferencesCopyAppValue(key, Redirected(app)); }
static void SetAppValue(CFStringRef key, CFPropertyListRef value, CFStringRef app) { CFPreferencesSetAppValue(key, value, Redirected(app)); }
static Boolean AppSynchronize(CFStringRef app) { return CFPreferencesAppSynchronize(Redirected(app)); }
static Boolean GetAppBooleanValue(CFStringRef key, CFStringRef app, Boolean* valid) { return CFPreferencesGetAppBooleanValue(key, Redirected(app), valid); }
static CFIndex GetAppIntegerValue(CFStringRef key, CFStringRef app, Boolean* valid) { return CFPreferencesGetAppIntegerValue(key, Redirected(app), valid); }
static CFPropertyListRef CopyValue(CFStringRef key, CFStringRef app, CFStringRef user, CFStringRef host) { return CFPreferencesCopyValue(key, Redirected(app), user, host); }
static void SetValue(CFStringRef key, CFPropertyListRef value, CFStringRef app, CFStringRef user, CFStringRef host) { CFPreferencesSetValue(key, value, Redirected(app), user, host); }
static Boolean Synchronize(CFStringRef app, CFStringRef user, CFStringRef host) { return CFPreferencesSynchronize(Redirected(app), user, host); }
static CFArrayRef CopyKeyList(CFStringRef app, CFStringRef user, CFStringRef host) { return CFPreferencesCopyKeyList(Redirected(app), user, host); }

static OSStatus Untransformed(const ProcessSerialNumber* process, ProcessApplicationTransformState state) {
    if (state == kProcessTransformToBackgroundApplication) {
        return TransformProcessType(process, state);
    }
    fprintf(stderr, "[background] refused TransformProcessType(%u)\n", (unsigned)state);
    return noErr;
}

__attribute__((used, section("__DATA,__interpose"))) static struct {
    const void* replacement;
    const void* original;
} Interposed[] = {
    {(const void*)CopyAppValue, (const void*)CFPreferencesCopyAppValue},
    {(const void*)SetAppValue, (const void*)CFPreferencesSetAppValue},
    {(const void*)AppSynchronize, (const void*)CFPreferencesAppSynchronize},
    {(const void*)GetAppBooleanValue, (const void*)CFPreferencesGetAppBooleanValue},
    {(const void*)GetAppIntegerValue, (const void*)CFPreferencesGetAppIntegerValue},
    {(const void*)CopyValue, (const void*)CFPreferencesCopyValue},
    {(const void*)SetValue, (const void*)CFPreferencesSetValue},
    {(const void*)Synchronize, (const void*)CFPreferencesSynchronize},
    {(const void*)CopyKeyList, (const void*)CFPreferencesCopyKeyList},
    {(const void*)Untransformed, (const void*)TransformProcessType},
};

static NSUserDefaults* Standard(id self, SEL _cmd) {
    static NSUserDefaults* defaults;
    static dispatch_once_t once;
    dispatch_once(&once, ^{
        defaults = [[NSUserDefaults alloc] initWithSuiteName:(__bridge NSString*)Domain()];
    });
    return defaults;
}

static void Alert(NSString* line) {
    fprintf(stderr, "[background] %s\n", line.UTF8String);
    const char* path = getenv("RHINO_TEST_ALERTS");
    FILE* file = path == NULL ? NULL : fopen(path, "a");
    if (file == NULL) {
        return;
    }
    fprintf(file, "%s\n", [line stringByReplacingOccurrencesOfString:@"\n" withString:@" "].UTF8String);
    fclose(file);
}

static NSString* Texts(NSView* view) {
    NSMutableArray<NSString*>* texts = [NSMutableArray array];
    if ([view isKindOfClass:[NSTextField class]] && ((NSTextField*)view).stringValue.length > 0) {
        [texts addObject:((NSTextField*)view).stringValue];
    }
    for (NSView* child in view.subviews) {
        NSString* inner = Texts(child);
        if (inner.length > 0) {
            [texts addObject:inner];
        }
    }
    return [texts componentsJoinedByString:@" | "];
}

static void Unordered(NSWindow* self, SEL _cmd, NSWindowOrderingMode place, NSInteger relativeTo) {
    if (place == NSWindowOut) {
        ((void (*)(id, SEL, NSWindowOrderingMode, NSInteger))OrderWindow)(self, _cmd, place, relativeTo);
    }
}

static void Unshown(NSWindow* self, SEL _cmd, ...) {}

static void Invisible(NSWindow* self, SEL _cmd, BOOL visible) {
    if (!visible) {
        ((void (*)(id, SEL, BOOL))SetIsVisible)(self, _cmd, visible);
    }
}

static BOOL Prohibited(NSApplication* self, SEL _cmd, NSApplicationActivationPolicy policy) {
    return ((BOOL (*)(id, SEL, NSApplicationActivationPolicy))SetActivationPolicy)(self, _cmd, NSApplicationActivationPolicyProhibited);
}

static void Inactive(id self, SEL _cmd, ...) {}

static BOOL Unactivated(NSRunningApplication* self, SEL _cmd, NSApplicationActivationOptions options) { return NO; }

static NSModalResponse RefusedAlert(NSAlert* self, SEL _cmd) {
    Alert([NSString stringWithFormat:@"alert refused: %@ | %@", self.messageText, self.informativeText]);
    return NSModalResponseAbort;
}

static NSModalResponse RefusedModal(id self, SEL _cmd, NSWindow* window) {
    Alert([NSString stringWithFormat:@"modal window refused: %s '%@' %@", object_getClassName(window), window.title, Texts(window.contentView)]);
    return NSModalResponseAbort;
}

static void RefusedSession(NSApplication* self, SEL _cmd, NSWindow* window, NSWindow* relative) {
    fprintf(stderr, "[background] modal session kept unordered: %s '%s' %s\n", object_getClassName(window), window.title.UTF8String, Texts(window.contentView).UTF8String);
}

static Class DocumentClass(NSDocumentController* self, SEL _cmd, NSString* type) {
    static NSDictionary<NSString*, NSString*>* classes;
    static dispatch_once_t once;
    dispatch_once(&once, ^{
        NSURL* app = [[NSBundle bundleForClass:NSClassFromString(@"MRDocument")].bundleURL URLByDeletingLastPathComponent].URLByDeletingLastPathComponent.URLByDeletingLastPathComponent;
        NSMutableDictionary* table = [NSMutableDictionary dictionary];
        for (NSDictionary* row in [NSBundle bundleWithURL:app].infoDictionary[@"CFBundleDocumentTypes"]) {
            if (row[@"NSDocumentClass"] != nil) {
                table[row[@"CFBundleTypeName"]] = row[@"NSDocumentClass"];
            }
        }
        classes = table;
    });
    return NSClassFromString(classes[type]) ?: ((Class (*)(id, SEL, NSString*))DocumentClassForType)(self, _cmd, type);
}

// --- [COMPOSITION] ---------------------------------------------------------------------

__attribute__((constructor)) static void Background(void) {
    method_setImplementation(class_getClassMethod([NSUserDefaults class], @selector(standardUserDefaults)), (IMP)Standard);
    OrderWindow = method_setImplementation(class_getInstanceMethod([NSWindow class], @selector(orderWindow:relativeTo:)), (IMP)Unordered);
    method_setImplementation(class_getInstanceMethod([NSWindow class], @selector(orderFrontRegardless)), (IMP)Unshown);
    method_setImplementation(class_getInstanceMethod([NSWindow class], @selector(makeKeyAndOrderFront:)), (IMP)Unshown);
    SetIsVisible = method_setImplementation(class_getInstanceMethod([NSWindow class], @selector(setIsVisible:)), (IMP)Invisible);
    SetActivationPolicy = method_setImplementation(class_getInstanceMethod([NSApplication class], @selector(setActivationPolicy:)), (IMP)Prohibited);
    for (NSString* name in @[ @"activateIgnoringOtherApps:", @"activate", @"unhide:", @"unhideWithoutActivation" ]) {
        method_setImplementation(class_getInstanceMethod([NSApplication class], NSSelectorFromString(name)), (IMP)Inactive);
    }
    method_setImplementation(class_getInstanceMethod([NSRunningApplication class], @selector(activateWithOptions:)), (IMP)Unactivated);
    method_setImplementation(class_getInstanceMethod([NSAlert class], @selector(runModal)), (IMP)RefusedAlert);
    method_setImplementation(class_getInstanceMethod([NSApplication class], @selector(runModalForWindow:)), (IMP)RefusedModal);
    method_setImplementation(class_getInstanceMethod([NSApplication class], NSSelectorFromString(@"_orderFrontModalWindow:relativeToWindow:")), (IMP)RefusedSession);
    DocumentClassForType = method_setImplementation(class_getInstanceMethod([NSDocumentController class], @selector(documentClassForType:)), (IMP)DocumentClass);
}

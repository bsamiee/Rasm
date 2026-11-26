import ApplicationServices
package import Darwin

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("CGSMainConnectionID")
package func mainConnection() -> Int32

package func keyFocusProcess() -> pid_t? {
    var (serial, flag, connection, process): (ProcessSerialNumber, Bool, Int32, pid_t) = (ProcessSerialNumber(), false, 0, 0)
    guard unsafe keyFocusSerial(&serial, &flag) == 0, unsafe connectionOfSerial(mainConnection(), &serial, &connection) == 0, unsafe processOfConnection(connection, &process) == 0 else {
        return nil
    }
    return process
}

@_silgen_name("CPSStealKeyFocusReturningID")
func stealKeyFocus(_: Int32, _: UInt32, _: UnsafeMutablePointer<Int32>) -> Int32

@_silgen_name("CPSReleaseKeyFocusWithID")
func releaseKeyFocus(_: Int32) -> Int32

@_silgen_name("CPSGetKeyFocusProcess")
private func keyFocusSerial(_: UnsafeMutablePointer<ProcessSerialNumber>, _: UnsafeMutablePointer<Bool>) -> Int32

@_silgen_name("CGSGetConnectionIDForPSN")
private func connectionOfSerial(_: Int32, _: UnsafePointer<ProcessSerialNumber>, _: UnsafeMutablePointer<Int32>) -> Int32

@_silgen_name("CGSConnectionGetPID")
private func processOfConnection(_: Int32, _: UnsafeMutablePointer<pid_t>) -> Int32

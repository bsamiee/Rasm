import ApplicationServices
package import Darwin

// --- [OPERATIONS] ----------------------------------------------------------------------

@_silgen_name("CGSMainConnectionID")
package func mainConnection() -> Int32

package func keyFocusProcess() -> pid_t? {
    var (serial, flag, connection, process): (ProcessSerialNumber, UInt8, Int32, pid_t) = (ProcessSerialNumber(), 0, 0, 0)
    guard unsafe keyFocusSerial(&serial, &flag) == .success, unsafe connectionOfSerial(mainConnection(), &serial, &connection) == .success,
        unsafe processOfConnection(connection, &process) == .success
    else { return nil }
    return process
}

@_silgen_name("CPSGetKeyFocusProcess")
private func keyFocusSerial(_: UnsafeMutablePointer<ProcessSerialNumber>, _: UnsafeMutablePointer<UInt8>) -> CGError

@_silgen_name("CGSGetConnectionIDForPSN")
private func connectionOfSerial(_: Int32, _: UnsafePointer<ProcessSerialNumber>, _: UnsafeMutablePointer<Int32>) -> CGError

@_silgen_name("CGSConnectionGetPID")
private func processOfConnection(_: Int32, _: UnsafeMutablePointer<pid_t>) -> CGError

# --- [TARGET] ---------------------------------------------------------------------------

if(CMAKE_HOST_SYSTEM_PROCESSOR MATCHES "^(arm64|aarch64)$")
    set(VCPKG_TARGET_ARCHITECTURE arm64)
else()
    set(VCPKG_TARGET_ARCHITECTURE x64)
endif()
set(VCPKG_CMAKE_SYSTEM_NAME "${CMAKE_HOST_SYSTEM_NAME}")
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)
set(VCPKG_BUILD_TYPE release)

# --- [TOOLCHAIN] ------------------------------------------------------------------------

set(VCPKG_CHAINLOAD_TOOLCHAIN_FILE "${CMAKE_CURRENT_LIST_DIR}/../toolchains/llvm.cmake")

# --- [TARGET] ---------------------------------------------------------------------------

cmake_host_system_information(RESULT processor QUERY OS_PLATFORM)
if(processor MATCHES "^(aarch64|arm64|ARM64)$")
    set(VCPKG_TARGET_ARCHITECTURE arm64)
else()
    set(VCPKG_TARGET_ARCHITECTURE x64)
endif()
if(NOT CMAKE_HOST_WIN32)
    set(VCPKG_CMAKE_SYSTEM_NAME "${CMAKE_HOST_SYSTEM_NAME}")
endif()
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)
set(VCPKG_BUILD_TYPE release)

# --- [TOOLCHAIN] ------------------------------------------------------------------------

set(VCPKG_ENV_PASSTHROUGH_UNTRACKED PATH)
set(VCPKG_CHAINLOAD_TOOLCHAIN_FILE "${CMAKE_CURRENT_LIST_DIR}/../toolchains/llvm.cmake")

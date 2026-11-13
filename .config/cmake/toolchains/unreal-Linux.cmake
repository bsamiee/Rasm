# --- [ENGINE] ---------------------------------------------------------------------------

set(engine "$ENV{UNREAL_ENGINE_ROOT}")
if(NOT engine)
    set(newest 0)
    file(STRINGS "$ENV{HOME}/.config/Epic/UnrealEngine/Install.ini" installed REGEX "^UE_[0-9.]+=")
    foreach(row IN LISTS installed)
        if(row MATCHES "^UE_([0-9.]+)=(.+)$" AND CMAKE_MATCH_1 VERSION_GREATER "${newest}")
            set(newest "${CMAKE_MATCH_1}")
            set(engine "${CMAKE_MATCH_2}")
        endif()
    endforeach()
endif()
file(READ "${engine}/Engine/Config/Linux/Linux_SDK.json" sdk)
string(JSON release GET "${sdk}" MainVersion)
set(CMAKE_SYSROOT "${engine}/Engine/Extras/ThirdPartyNotUE/SDKs/HostLinux/Linux_x64/${release}/x86_64-unknown-linux-gnu")

# --- [COMPILERS] ------------------------------------------------------------------------

set(CMAKE_C_COMPILER "${CMAKE_SYSROOT}/bin/clang")
set(CMAKE_CXX_COMPILER "${CMAKE_SYSROOT}/bin/clang++")
set(CMAKE_C_COMPILER_TARGET x86_64-unknown-linux-gnu)
set(CMAKE_CXX_COMPILER_TARGET x86_64-unknown-linux-gnu)
set(CMAKE_AR "${CMAKE_SYSROOT}/bin/llvm-ar")
set(CMAKE_RANLIB "${CMAKE_SYSROOT}/bin/llvm-ranlib")
set(CMAKE_LINKER_TYPE LLD)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_C_FLAGS_INIT "-fvisibility-ms-compat")
set(CMAKE_CXX_FLAGS_INIT "-fvisibility-ms-compat -fvisibility-inlines-hidden -fno-rtti -nostdinc++ -isystem ${CMAKE_SYSROOT}/include -isystem ${CMAKE_SYSROOT}/include/c++/v1")

# --- [STANDARD_LIBRARY] -----------------------------------------------------------------

foreach(kind IN ITEMS EXE SHARED MODULE)
    set(CMAKE_${kind}_LINKER_FLAGS_INIT "-nodefaultlibs -B${CMAKE_SYSROOT}/usr/lib -B${CMAKE_SYSROOT}/usr/lib64 -L${CMAKE_SYSROOT}/usr/lib -L${CMAKE_SYSROOT}/usr/lib64")
endforeach()
set(CMAKE_C_STANDARD_LIBRARIES_INIT "-lm -lc -lpthread -lgcc_s -lgcc")
set(CMAKE_CXX_STANDARD_LIBRARIES_INIT "${CMAKE_SYSROOT}/lib64/libc++.a ${CMAKE_SYSROOT}/lib64/libc++abi.a ${CMAKE_C_STANDARD_LIBRARIES_INIT}")

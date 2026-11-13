# --- [ENGINE] ---------------------------------------------------------------------------

set(engine "$ENV{UNREAL_ENGINE_ROOT}")
if(NOT engine)
    set(newest 0)
    file(READ "$ENV{HOME}/Library/Application Support/Epic/UnrealEngineLauncher/LauncherInstalled.dat" installed)
    string(JSON count LENGTH "${installed}" InstallationList)
    math(EXPR last "${count} - 1")
    foreach(index RANGE ${last})
        string(JSON name GET "${installed}" InstallationList ${index} AppName)
        if(name MATCHES "^UE_([0-9.]+)$" AND CMAKE_MATCH_1 VERSION_GREATER "${newest}")
            set(newest "${CMAKE_MATCH_1}")
            string(JSON engine GET "${installed}" InstallationList ${index} InstallLocation)
        endif()
    endforeach()
endif()
file(READ "${engine}/Engine/Config/Mac/Mac_SDK.json" sdk)
string(JSON CMAKE_OSX_DEPLOYMENT_TARGET GET "${sdk}" DeploymentTarget)
set(CMAKE_OSX_ARCHITECTURES "arm64;x86_64")

# --- [COMPILERS] ------------------------------------------------------------------------

set(CMAKE_C_COMPILER /usr/bin/clang)
set(CMAKE_CXX_COMPILER /usr/bin/clang++)
set(CMAKE_OBJCXX_COMPILER /usr/bin/clang++)
set(CMAKE_C_FLAGS_INIT "-fvisibility-ms-compat")
set(CMAKE_CXX_FLAGS_INIT "-fvisibility-ms-compat -fvisibility-inlines-hidden -fno-rtti")
set(CMAKE_OBJCXX_FLAGS_INIT "-fvisibility-ms-compat -fvisibility-inlines-hidden -fno-rtti")

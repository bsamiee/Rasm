// Copyright 2026 Bardia Samiee. All Rights Reserved.

#include "RasmProbe.h"

void FRasmProbeModule::StartupModule()
{
}

void FRasmProbeModule::ShutdownModule()
{
}

int32 FRasmProbeModule::Doubled(int32 Value)
{
	return Value * 2;
}

IMPLEMENT_MODULE(FRasmProbeModule, RasmProbe)

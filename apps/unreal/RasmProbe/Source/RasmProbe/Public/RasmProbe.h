// Copyright 2026 Bardia Samiee. All Rights Reserved.

#pragma once

#include "Modules/ModuleManager.h"

class FRasmProbeModule : public IModuleInterface
{
public:
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;

	RASMPROBE_API static int32 Doubled(int32 Value);
};

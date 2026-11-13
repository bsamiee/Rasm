// Copyright 2026 Bardia Samiee. All Rights Reserved.

#include "ToleranceLibrary.h"

#include "Modules/ModuleManager.h"

#include <rasm/tolerance.hpp>

bool UToleranceLibrary::Within(double Left, double Right, int32 Ulps)
{
	return Ulps >= 0 && rasm::within(Left, Right, static_cast<unsigned>(Ulps));
}

IMPLEMENT_MODULE(FDefaultModuleImpl, Tolerance)

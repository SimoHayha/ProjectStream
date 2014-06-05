#pragma once

#include "StreamAPIRoot.h"

namespace
{
	StreamAPIRoot*	Root;
}

extern "C"
{
	__declspec(dllexport) bool	Initialize();
}
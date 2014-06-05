#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#include "StreamAPI.h"

__declspec(dllexport) bool	Initialize()
{
	Root = new StreamAPIRoot();

	return true;
}


int CALLBACK WinMain(
	_In_  HINSTANCE hInstance,
	_In_  HINSTANCE hPrevInstance,
	_In_  LPSTR lpCmdLine,
	_In_  int nCmdShow
	)
{
	Root = new StreamAPIRoot();

	return 0;
}

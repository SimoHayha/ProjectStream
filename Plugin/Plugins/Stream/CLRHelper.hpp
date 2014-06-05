#pragma once

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

class CLRHelper
{
public:
	static array<unsigned char>^ GetString(char const* str)
	{
		size_t len = strnlen_s(str, 512);
		array<unsigned char>^ result = gcnew array<unsigned char>(len);
		for (size_t i = 0u; i < len; ++i)
			result[i] = str[i];
		return result;
	}

	static void Write(System::IO::FileStream^ stream, char const* str)
	{
		array<unsigned char>^ arr = GetString(str);
		stream->Write(arr, 0, arr->Length);
	}
};
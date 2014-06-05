#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <winhttp.h>
#include <string>

#include "CLRHelper.hpp"

#include "StreamAPIRoot.h"

StreamAPIRoot::StreamAPIRoot()
{
	HINTERNET hSession = WinHttpOpen(L"WinHTTP Example/1.0", WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);
	HINTERNET hConnect;
	HINTERNET hRequest;
	BOOL bResult;
	DWORD dwSize;
	DWORD dwDownloaded = 0;
	LPSTR pszOutBuffer;

	if (hSession)
		hConnect = WinHttpConnect(hSession, L"www.microsoft.com", INTERNET_DEFAULT_HTTPS_PORT, 0);

	if (hConnect)
		hRequest = WinHttpOpenRequest(hConnect, L"GET", NULL, NULL, WINHTTP_NO_REFERER, WINHTTP_DEFAULT_ACCEPT_TYPES, WINHTTP_FLAG_SECURE);

	if (hRequest)
		bResult = WinHttpSendRequest(hRequest, WINHTTP_NO_ADDITIONAL_HEADERS, 0, WINHTTP_NO_REQUEST_DATA, 0, 0, 0);

	OutputDebugStringA((LPCSTR)GetLastError());

	System::IO::FileStream^ stream = gcnew System::IO::FileStream("WinHttpLog.txt", System::IO::FileMode::Append);
	CLRHelper::Write(stream, "Error ");
	char tmp[512];
	sprintf_s(tmp, "%s", GetLastError());
	CLRHelper::Write(stream, tmp);
	CLRHelper::Write(stream, " in WinHttpSendRequest.\n");

	if (bResult)
	{
		do
		{
			dwSize = 0;
			if (!WinHttpQueryDataAvailable(hRequest, &dwSize))
			{
				CLRHelper::Write(stream, "Error ");
				char tmp[512];
				sprintf_s(tmp, "%s", GetLastError());
				CLRHelper::Write(stream, tmp);
				CLRHelper::Write(stream, " in WinHttpQueryDataAvailable.\n");
			}

			pszOutBuffer = new char[dwSize + 1];
			if (!WinHttpReadData(hRequest, (LPVOID)pszOutBuffer, dwSize, &dwDownloaded))
			{
				CLRHelper::Write(stream, "Error ");
				char tmp[512];
				sprintf_s(tmp, "%s", GetLastError());
				CLRHelper::Write(stream, tmp);
				CLRHelper::Write(stream, " in WinHttpReadData.\n");
			}
			delete pszOutBuffer;

		} while (dwSize > 0);
		stream->Close();
	}
}

StreamAPIRoot::~StreamAPIRoot()
{
}
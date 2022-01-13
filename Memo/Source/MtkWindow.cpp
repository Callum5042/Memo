#include "../Include/MtkWindow.h"
#include <exception>
#include <strsafe.h>
#include <windowsx.h>

#define APPWM_ICONNOTIFY (WM_APP + 1)

MTK::Window::~Window() 
{

}

void MTK::Window::Create(WindowCreateSettings settings)
{
	// Get the instance handle
	HINSTANCE hInstance = GetModuleHandle(NULL);

	// Register the window class
	WNDCLASS wndClass = {};
	wndClass.lpfnWndProc = WindowProc;
	wndClass.hInstance = hInstance;
	wndClass.lpszClassName = settings.windowClassName.c_str();
	wndClass.hCursor = LoadCursor(NULL, IDC_ARROW);

	if (!RegisterClass(&wndClass))
	{
		throw std::exception("RegisterClass failed");
	}

	// Create the window
	m_Hwnd = CreateWindowEx(0, settings.windowClassName.c_str(), settings.title.c_str(), WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, settings.width, settings.height, NULL, NULL, hInstance, this);

	if (m_Hwnd == NULL)
	{
		throw std::exception("CreateWindowEx failed");
	}

	// Show the created window
	ShowWindow(m_Hwnd, SW_SHOWNORMAL);
}

void MTK::Window::Destroy()
{
	DestroyWindow(m_Hwnd);
}

LRESULT MTK::Window::HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_COMMAND:
	{
		if (HIWORD(wParam) == BN_CLICKED)
		{
			/*auto button_id = LOWORD(wParam);
			auto button = m_PushButtons[button_id];
			if (button->OnClick)
			{
				button->OnClick();
			}*/
		}

		return 0;
	}

	case WM_CREATE:
	{
		OnCreate();

		auto icon_image = LoadImage(NULL, L"C:\\Users\\Callum\\Desktop\\memo.ico", IMAGE_ICON, 0, 0, LR_LOADFROMFILE | LR_DEFAULTSIZE | LR_SHARED);

		m_Icon.cbSize = sizeof(NOTIFYICONDATA);
		m_Icon.hWnd = m_Hwnd;
		m_Icon.uFlags = NIF_ICON | NIF_TIP | NIF_GUID | NIF_MESSAGE;
		m_Icon.hIcon = static_cast<HICON>(icon_image);
		StringCchCopy(m_Icon.szTip, ARRAYSIZE(m_Icon.szTip), L"Test application");
		m_Icon.uCallbackMessage = APPWM_ICONNOTIFY;

		Shell_NotifyIcon(NIM_ADD, &m_Icon);

		return 0;
	}

	case APPWM_ICONNOTIFY:
	{
		if (LOWORD(lParam) == WM_CONTEXTMENU)
		{
			MessageBox(NULL, L"Context menu", L"Info", MB_OK);
		}

		if (lParam == WM_RBUTTONUP)
		{
			// MessageBox(NULL, L"System Tray", L"Info", MB_OK);

			POINT cursor;
			GetCursorPos(&cursor);

			HMENU hPopupMenu = CreatePopupMenu();
			InsertMenu(hPopupMenu, 0, MF_BYPOSITION | MF_STRING, 1000, L"Exit");
			InsertMenu(hPopupMenu, 0, MF_BYPOSITION | MF_STRING, 1001, L"Play");
			SetForegroundWindow(m_Hwnd);
			TrackPopupMenu(hPopupMenu, TPM_BOTTOMALIGN | TPM_LEFTALIGN, cursor.x, cursor.y, 0, m_Hwnd, NULL);
		}

		return 0;
	}

	case WM_QUIT:
		if (OnQuit())
		{
			return 0;
		}
		else
		{
			return DefWindowProc(m_Hwnd, uMsg, wParam, lParam);
		}

	case WM_DESTROY:
		Shell_NotifyIcon(NIM_DELETE, &m_Icon);
		PostQuitMessage(0);
		return 0;

	default:
		return DefWindowProc(m_Hwnd, uMsg, wParam, lParam);
	}
}

int MTK::Window::FindEmptyControlId()
{
	int id = 1;
	auto it = m_PushButtons.find(id);
	while (it != m_PushButtons.cend())
	{
		id++;
		it = m_PushButtons.find(id);
	}

	return id;
}

void MTK::Window::RegisterButton(PushButtonV2* button)
{
	m_PushButtons[button->GetControlId()] = button;
}
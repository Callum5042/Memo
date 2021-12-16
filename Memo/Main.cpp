#include <iostream>

#if defined(WIN32) || defined(_WIN32)
#include <Windows.h>
#pragma comment(linker,"\"/manifestdependency:type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif

template <class TWindow>
class BaseWindow
{
public:
	BaseWindow() = default;
	virtual ~BaseWindow() = default;

	static LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
	{
		// Get window pointer
		TWindow* window = nullptr;
		if (uMsg == WM_NCCREATE)
		{
			CREATESTRUCT* pCreate = (CREATESTRUCT*)lParam;
			window = static_cast<TWindow*>(pCreate->lpCreateParams);
			SetWindowLongPtr(hwnd, GWLP_USERDATA, (LONG_PTR)window);

			window->m_Hwnd = hwnd;
		}
		else
		{
			window = reinterpret_cast<TWindow*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));
		}

		// Loop
		if (window != nullptr)
		{
			return window->HandleMessage(uMsg, wParam, lParam);
		}
		else
		{
			return DefWindowProc(hwnd, uMsg, wParam, lParam);
		}
	}

	virtual bool Create()
	{
		HINSTANCE hInstance = GetModuleHandle(NULL);

		// Register class
		WNDCLASS wndClass = {};
		wndClass.lpfnWndProc = WindowProc;
		wndClass.hInstance = hInstance;
		wndClass.lpszClassName = L"MemoWnd";

		if (!RegisterClass(&wndClass))
		{
			MessageBox(nullptr, L"RegisterClass failed!", L"Error", MB_OK | MB_ICONERROR);
			return false;
		}

		// Create window
		HWND hwnd = CreateWindowEx(
			0,                              // Optional window styles.
			L"MemoWnd",                     // Window class
			L"Learn to Program Windows",    // Window text
			WS_OVERLAPPEDWINDOW,            // Window style

			// Size and position
			CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,

			NULL,       // Parent window    
			NULL,       // Menu
			hInstance,  // Instance handle
			this        // Additional application data
		);

		if (hwnd == nullptr)
		{
			MessageBox(nullptr, L"CreateWindowEx failed!", L"Error", MB_OK | MB_ICONERROR);
			return false;
		}

		NONCLIENTMETRICS ncm;
		ncm.cbSize = sizeof(NONCLIENTMETRICS);
		::SystemParametersInfo(SPI_GETNONCLIENTMETRICS, sizeof(NONCLIENTMETRICS), &ncm, 0);
		HFONT hFont = ::CreateFontIndirect(&ncm.lfMessageFont);
		::SendMessage(hwnd, WM_SETFONT, (WPARAM)hFont, MAKELPARAM(TRUE, 0));

		ShowWindow(hwnd, SW_SHOWNORMAL);

		// Add button
		HWND hwndButton = CreateWindow(
			L"BUTTON",  // Predefined class; Unicode assumed 
			L"OK",      // Button text 
			WS_TABSTOP | WS_VISIBLE | WS_CHILD,  // Styles 
			10,         // x position 
			10,         // y position 
			100,        // Button width
			30,        // Button height
			hwnd,     // Parent window
			NULL,       // No menu.
			(HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE),
			NULL);      // Pointer not needed.

		return true;
	}

protected:
	HWND m_Hwnd = NULL;
	virtual LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam) = 0;
};


class Window : public BaseWindow<Window>
{
public:
	Window() = default;
	virtual ~Window() = default;

public:
	LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam) override
	{
		switch (uMsg)
		{
		case WM_DESTROY:
			PostQuitMessage(0);
			return 0;

		case WM_PAINT:
		{
			PAINTSTRUCT ps;
			HDC hdc = BeginPaint(m_Hwnd, &ps);

			FillRect(hdc, &ps.rcPaint, (HBRUSH)(COLOR_WINDOW + 1));

			EndPaint(m_Hwnd, &ps);
			return 0;
		}

		case WM_CLOSE:
			if (MessageBox(m_Hwnd, L"Are you sure you want to quit?", L"Oh no", MB_OKCANCEL) == IDOK)
			{
				DestroyWindow(m_Hwnd);
			}
			return 0;

		default:
			return DefWindowProc(m_Hwnd, uMsg, wParam, lParam);
		}
	}

private:

};

int main(int argc, char** argv)
{
	Window* window = new Window();
	if (!window->Create())
		return -1;

	// Run the message loop.
	MSG msg = { };
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return 0;
}
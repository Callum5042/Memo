#include "../Include/MtkWindow.h"
#include <exception>

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
	case WM_DESTROY:
		PostQuitMessage(0);
		return 0;

	default:
		return DefWindowProc(m_Hwnd, uMsg, wParam, lParam);
	}
}
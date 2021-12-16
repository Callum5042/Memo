#pragma once

#include <string>

#if defined(WIN32) || defined(_WIN32)
#include <Windows.h>
// #pragma comment(linker,"\"/manifestdependency:type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif

namespace MTK
{
	constexpr int WINDOW_DEFAULT_WIDTH = 0;
	constexpr int WINDOW_DEFAULT_HEIGHT = 0;

	// Window creation settings
	struct WindowCreateSettings
	{
		std::wstring title;
		std::wstring windowClassName = L"MemoWindow";
		int width = WINDOW_DEFAULT_WIDTH;
		int height = WINDOW_DEFAULT_HEIGHT;
	};

	// Window interface
	class IWindow
	{
	public:
		IWindow() = default;
		virtual ~IWindow() = default;

		// Create the window
		virtual void Create(WindowCreateSettings settings) = 0;

		// Destroy the window
		virtual void Destroy() = 0;
	};

	// Base Win32 message loop
	template <typename TWindow>
	class BaseWindow
	{
	public:
		BaseWindow() = default;
		virtual ~BaseWindow() = default;

		// Win32 message loop
		static inline LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
		{
			// Get window pointer
			TWindow* window = nullptr;
			if (uMsg == WM_NCCREATE)
			{
				CREATESTRUCT* pCreate = reinterpret_cast<CREATESTRUCT*>(lParam);
				window = static_cast<TWindow*>(pCreate->lpCreateParams);
				SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(window));
				window->m_Hwnd = hwnd;
			}
			else
			{
				window = reinterpret_cast<TWindow*>(GetWindowLongPtr(hwnd, GWLP_USERDATA));
			}

			// Handle window messages
			if (window != nullptr)
			{
				return window->HandleMessage(uMsg, wParam, lParam);
			}
			else
			{
				return DefWindowProc(hwnd, uMsg, wParam, lParam);
			}
		}

	protected:
		HWND m_Hwnd = NULL;
		virtual LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam) = 0;
	};

	// Win32 Window
	class Window : public BaseWindow<Window>, public IWindow
	{
	public:
		Window() = default;
		virtual ~Window() override;

		// Handle window messages
		LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam) override;

	protected:
		// Create the window
		void Create(WindowCreateSettings settings) override;

		// Destroy the window
		void Destroy() override;
	};
}
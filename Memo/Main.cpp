#include <iostream>

#if defined(WIN32) || defined(_WIN32)
#include <Windows.h>
#endif

#include <d2d1.h>
#pragma comment(lib, "d2d1")

#include <dwrite.h>
#pragma comment(lib, "dwrite")

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

		ShowWindow(hwnd, SW_SHOWNORMAL);
		return true;
	}

protected:
	HWND m_Hwnd = NULL;
	virtual LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam) = 0;
};


class Window : public BaseWindow<Window>
{
	ID2D1Factory* pFactory;
	ID2D1HwndRenderTarget* pRenderTarget;
	ID2D1SolidColorBrush* pBrush;
	D2D1_ELLIPSE            ellipse;


	IDWriteFactory* m_pDWriteFactory = nullptr;
	IDWriteTextFormat* m_pTextFormat = nullptr;

public:
	Window() = default;
	virtual ~Window() = default;

	void CalculateLayout()
	{
		if (pRenderTarget != NULL)
		{
			D2D1_SIZE_F size = pRenderTarget->GetSize();
			const float x = size.width / 2;
			const float y = size.height / 2;
			const float radius = min(x, y);
			ellipse = D2D1::Ellipse(D2D1::Point2F(x, y), radius, radius);
		}
	}

	HRESULT CreateGraphicsResources()
	{
		HRESULT hr = S_OK;
		if (pRenderTarget == NULL)
		{
			RECT rc;
			GetClientRect(m_Hwnd, &rc);

			D2D1_SIZE_U size = D2D1::SizeU(rc.right, rc.bottom);

			hr = pFactory->CreateHwndRenderTarget(
				D2D1::RenderTargetProperties(),
				D2D1::HwndRenderTargetProperties(m_Hwnd, size),
				&pRenderTarget);

			if (SUCCEEDED(hr))
			{
				const D2D1_COLOR_F color = D2D1::ColorF(1.0f, 1.0f, 0);
				hr = pRenderTarget->CreateSolidColorBrush(color, &pBrush);

				if (SUCCEEDED(hr))
				{
					CalculateLayout();
				}
			}
		}

		// DirectWrite
		hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(m_pDWriteFactory), reinterpret_cast<IUnknown**>(&m_pDWriteFactory));

		static const WCHAR msc_fontName[] = L"Verdana";
		static const FLOAT msc_fontSize = 50;

		hr = m_pDWriteFactory->CreateTextFormat(msc_fontName, NULL, DWRITE_FONT_WEIGHT_NORMAL, DWRITE_FONT_STYLE_NORMAL, DWRITE_FONT_STRETCH_NORMAL, msc_fontSize, L"", &m_pTextFormat);

		m_pTextFormat->SetTextAlignment(DWRITE_TEXT_ALIGNMENT_CENTER);
		m_pTextFormat->SetParagraphAlignment(DWRITE_PARAGRAPH_ALIGNMENT_CENTER);

		return hr;
	}

	void DiscardGraphicsResources()
	{
		//SafeRelease(&pRenderTarget);
		//SafeRelease(&pBrush);
	}

	void OnPaint()
	{
		HRESULT hr = CreateGraphicsResources();
		if (SUCCEEDED(hr))
		{
			PAINTSTRUCT ps;
			BeginPaint(m_Hwnd, &ps);

			/*pRenderTarget->BeginDraw();

			pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::SkyBlue));
			pRenderTarget->FillEllipse(ellipse, pBrush);

			hr = pRenderTarget->EndDraw();
			if (FAILED(hr) || hr == D2DERR_RECREATE_TARGET)
			{
				DiscardGraphicsResources();
			}*/

			static const WCHAR sc_helloWorld[] = L"Hello, World!";

			// Retrieve the size of the render target.
			D2D1_SIZE_F renderTargetSize = pRenderTarget->GetSize();

			pRenderTarget->BeginDraw();

			pRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());

			pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::White));

			pRenderTarget->DrawText(
				sc_helloWorld,
				ARRAYSIZE(sc_helloWorld) - 1,
				m_pTextFormat,
				D2D1::RectF(0, 0, renderTargetSize.width, renderTargetSize.height),
				pBrush
			);

			hr = pRenderTarget->EndDraw();


			EndPaint(m_Hwnd, &ps);
		}
	}

	void Resize()
	{
		if (pRenderTarget != NULL)
		{
			RECT rc;
			GetClientRect(m_Hwnd, &rc);

			D2D1_SIZE_U size = D2D1::SizeU(rc.right, rc.bottom);

			pRenderTarget->Resize(size);
			CalculateLayout();
			InvalidateRect(m_Hwnd, NULL, FALSE);
		}
	}

public:
	LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam) override
	{
		switch (uMsg)
		{
		case WM_CREATE:
			if (FAILED(D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &pFactory)))
			{
				return -1;  // Fail CreateWindowEx.
			}
			return 0;

		case WM_DESTROY:
			PostQuitMessage(0);
			return 0;

		case WM_PAINT:
		{
			OnPaint();
			return 0;
		}

		case WM_SIZE:
			Resize();
			return 0;

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
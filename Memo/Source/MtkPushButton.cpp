#include "../Include/MtkPushButton.h"
#include "../Include/MtkWindow.h"

MTK::PushButton::PushButton(MTK::Window* window) : m_Window(window)
{
}

MTK::PushButton::~PushButton()
{
	DestroyWindow(m_Hwnd);
}

void MTK::PushButton::Create(ButtonCreateSettings settings)
{
	m_ControlId = m_Window->FindEmptyControlId();

	HWND hwnd = m_Window->GetHwnd();

	HINSTANCE hInstance = reinterpret_cast<HINSTANCE>(GetWindowLongPtr(hwnd, GWLP_HINSTANCE));
	m_Hwnd = CreateWindow(L"BUTTON", settings.text.c_str(), WS_VISIBLE | WS_CHILD,
		settings.x, settings.y, settings.width, settings.height,
		hwnd, (HMENU)GetControlId(), hInstance, NULL);

	if (m_Hwnd == NULL)
	{
		throw std::exception("CreateWindow failed!");
	}

	m_Window->AddButton(this);
}
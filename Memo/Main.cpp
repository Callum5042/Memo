#include "Include\MtkWindow.h"
#include <memory>

class MainWindow : public MTK::Window
{
public:
	MainWindow() = default;
	virtual ~MainWindow() = default;

	// Show window
	void Show()
	{
		// Create the window
		MTK::WindowCreateSettings settings;
		settings.title = L"Memo";
		settings.width = 800;
		settings.height = 600;

		MTK::Window::Create(settings);
	}

	// On creation
	void OnCreate() override
	{
		// Add UI Elements
		/*AddButton(MTK::ButtonCreateSettings{ L"Click me", 10, 10, 100, 20 });
		AddButton(MTK::ButtonCreateSettings{ L"Click me tooo", 10, 50, 100, 20 });*/

		// Add menu
		HMENU menu = CreateMenu();
		HMENU fileMenu = CreateMenu();

		AppendMenu(menu, MF_POPUP, reinterpret_cast<UINT_PTR>(fileMenu), L"File");
		AppendMenu(fileMenu, MF_STRING, 1, L"Open");


		SetMenu(m_Hwnd, menu);
	}
};

int main(int argc, char** argv)
{
	// Create and show window
	auto window = std::make_unique<MainWindow>();
	window->Show();

	// Run the message loop.
	MSG msg = { };
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return 0;
}
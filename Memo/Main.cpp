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

		// Add UI Elements


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
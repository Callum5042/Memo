#include "Include\MtkWindow.h"
#include <memory>

int main(int argc, char** argv)
{
	std::unique_ptr<MTK::IWindow> window = std::make_unique<MTK::Window>();

	MTK::WindowCreateSettings settings;
	settings.width = 800;
	settings.height = 600;
	window->Create(settings);


	// Run the message loop.
	MSG msg = { };
	while (GetMessage(&msg, NULL, 0, 0))
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return 0;
}
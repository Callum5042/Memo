#include "Include\MtkWindow.h"
#include "Include\MtkPushButton.h"

class MainWindow : public MTK::Window
{
	std::unique_ptr<MTK::PushButtonV2> m_DefaultButton = nullptr;

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
		{
			MTK::ButtonCreateSettings settings;
			settings.text = L"Click me";
			settings.x = 10;
			settings.y = 10;
			settings.width = 100;
			settings.height = 20;

			m_DefaultButton = std::make_unique<MTK::PushButtonV2>(this);
			m_DefaultButton->Create(settings);
			m_DefaultButton->OnClick = []()
			{

			};
		}
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
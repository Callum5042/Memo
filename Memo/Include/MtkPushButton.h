#pragma once

namespace MTK
{
	class Window;

	// Button creation settings
	struct ButtonCreateSettings
	{
		std::wstring text;
		int x = 0;
		int y = 0;
		int width = 0;
		int height = 0;
	};

	class PushButton
	{
	public:
		PushButton(MTK::Window* window);
		virtual ~PushButton();

		// Create window
		void Create(ButtonCreateSettings settings);

		// On click event
		virtual void OnClick() = 0;

		// Get control ID
		constexpr int GetControlId() { return m_ControlId; }

	protected:
		MTK::Window* m_Window = nullptr;

	private:
		HWND m_Hwnd = NULL;

		// Used to determine by the window which object raises the event
		int m_ControlId = 0;
	};
}
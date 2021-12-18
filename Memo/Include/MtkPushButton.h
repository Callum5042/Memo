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

	class PushButtonV2
	{
	public:
		PushButtonV2(MTK::Window* window);
		virtual ~PushButtonV2() = default;

		// Create window
		void Create(ButtonCreateSettings settings);

		// Get control ID
		constexpr __int64 GetControlId() { return m_ControlId; }

		// On click event
		std::function<void()> OnClick;
		 
	private:
		MTK::Window* m_Window = nullptr;
		HWND m_Hwnd = NULL;

		// Used to determine by the window which object raises the event
		__int64 m_ControlId = 0;
	};
}
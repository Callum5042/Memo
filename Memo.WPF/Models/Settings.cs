using System.Collections.Generic;

namespace Memo.WPF.Models
{
    public class Settings
    {
        public IList<TabSettings> TabSettings { get; set; } = new List<TabSettings>();
    }

    public class TabSettings
    {
        public string? Name { get; set; }

        public string? Path { get; set; }

        public string? Text { get; set; }
    }
}

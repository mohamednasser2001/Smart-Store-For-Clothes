using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class SuggestSizeVM
    {
        public int Height { get; set; }
        public int Weight { get; set; }
        public int Age { get; set; }

        public string? RecommendedSize { get; set; }
    }
}

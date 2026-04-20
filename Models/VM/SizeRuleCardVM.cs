using System;
using System.Collections.Generic;
using System.Text;

namespace Models.VM
{
    public class SizeRuleCardVM
    {
        public int Id { get; set; }

        public string SizeName { get; set; } = null!;

        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public int MinWeight { get; set; }
        public int MaxWeight { get; set; }

        public int MinAge { get; set; }
        public int MaxAge { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Models.VM
{
    public class SizeRuleVM
    {
        public int Id { get; set; }

        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public int MinWeight { get; set; }
        public int MaxWeight { get; set; }

        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        public int SizeId { get; set; }

        public IEnumerable<SelectListItem> SizesList { get; set; } = new List<SelectListItem>();
    }
}

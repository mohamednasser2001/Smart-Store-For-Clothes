using System;
using System.Collections.Generic;
using System.Text;


    namespace Models.VM
    {
        public class CheckoutVM
        {
            public int CartId { get; set; }

            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;

            public string City { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;

            public decimal SubTotal { get; set; }
            public decimal ShippingCost { get; set; }
            public decimal Total { get; set; }
        }
    }


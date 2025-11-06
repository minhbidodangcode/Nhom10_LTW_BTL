using Microsoft.AspNetCore.Mvc;

namespace Quanlinhahang.Models
{
    public class DatBanViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime BookingDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public string? TableType { get; set; }
        public string? Note { get; set; }
        public List<CartItem>? Items { get; set; }
    }

}


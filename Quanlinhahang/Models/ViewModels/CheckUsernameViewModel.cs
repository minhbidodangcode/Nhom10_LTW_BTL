using System.ComponentModel.DataAnnotations;

namespace Quanlinhahang.Models.ViewModels
{
    public class CheckUsernameViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}

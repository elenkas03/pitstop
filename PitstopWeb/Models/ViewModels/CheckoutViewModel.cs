using System.ComponentModel.DataAnnotations;
using PitstopWeb.Models;

namespace PitstopWeb.Models.ViewModels;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Името е задължително")]
    [Display(Name = "Име и фамилия")]
    public string ShippingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Адресът е задължителен")]
    [Display(Name = "Адрес за доставка")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Градът е задължителен")]
    [Display(Name = "Град")]
    public string ShippingCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефонът е задължителен")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    public List<CartItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.LineTotal);
}

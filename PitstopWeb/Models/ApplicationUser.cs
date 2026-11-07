using Microsoft.AspNetCore.Identity;

namespace PitstopWeb.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}

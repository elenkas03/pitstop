using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PitstopWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public abstract class AdminControllerBase : Controller
{
}

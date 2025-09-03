using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using testEFvsID.Data;

namespace testEFvsID.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =RoleName.Administrator)]
    public class Admin : Controller
    {
        [Route("/admin-oc/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}

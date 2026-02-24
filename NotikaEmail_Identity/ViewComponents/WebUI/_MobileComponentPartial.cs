using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.ViewComponents.WebUI
{
    public class _MobileComponentPartial:ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            return View();
        }


    }
}

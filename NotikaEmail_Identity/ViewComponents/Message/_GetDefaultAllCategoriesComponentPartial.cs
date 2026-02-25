using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Services.CategoryServices;

namespace NotikaEmail_Identity.ViewComponents.Message
{
    public class _GetDefaultAllCategoriesComponentPartial(ICategoryService _categoryService):ViewComponent
    {

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var catgeories=await _categoryService.GetAllAsync();
            return View(catgeories);
        }




    }
}

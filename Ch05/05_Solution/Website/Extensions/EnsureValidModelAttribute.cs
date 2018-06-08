using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HPlusSports
{
    public class EnsureValidModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller;
            var modelState = controller.ViewData.ModelState;

            if (modelState.IsValid)
            {
                return;
            }

            filterContext.Result = new ViewResult
            {
                ViewName = filterContext.ActionDescriptor.ActionName,
                TempData = controller.TempData,
                ViewData = controller.ViewData,
            };
        }
    }
}
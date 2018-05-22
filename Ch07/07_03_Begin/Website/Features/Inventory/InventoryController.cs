﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using HPlusSports.Models;
using HPlusSports.Requests;
using HPlusSports.Services;
using MediatR;

namespace HPlusSports.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class InventoryController : Controller
    {
        private HPlusSportsDbContext _context;
        private readonly IMediator _mediator;

        public InventoryController(
            HPlusSportsDbContext context,
            MediatR.IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public ActionResult Index()
        {
            var products =
              _context.Products
                .OrderBy(x => x.CategoryId)
                .ThenBy(x => x.Name);

            return View(products);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var product = new Product
            {
                CategoryId = request.CategoryId,
                Description = request.Description,
                MSRP = request.MSRP,
                Name = request.Name,
                Price = request.Price,
                SKU = request.SKU,
                Summary = request.Summary,
                LastUpdated = DateTime.UtcNow,
                LastUpdatedUserId = GetUserId(this),
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData.SuccessMessage($"Successfully created \"{product.Name}\"");

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public ActionResult Update(long id)
        {
            var existing = _context.Products.Find(id);

            if (existing == null)
            {
                TempData.ErrorMessage($"Couldn't update product #\"{id}\": product not found!");
            }

            return View(existing);
        }

        [HttpPost]
        public async Task<ActionResult> Update(UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            request.LastUpdatedUserId = GetUserId(this);

            var response = await _mediator.Send(request);

            if (!response.Success)
            {
                TempData.ErrorMessage(response.Message);
                return View();
            }

            TempData.SuccessMessage(response.Message);

            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(long id)
        {
            var product = _context.Products.Find(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();

                TempData.SuccessMessage($"Successfully deleted \"{product.Name}\"");
            }
            else
            {
                TempData.ErrorMessage($"Couldn't delete \"{product.Name}\": product not found!");
            }

            return RedirectToAction(nameof(Index));
        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            ViewData["CategoryId"] =
              _context.Categories
                  .Select(x => new SelectListItem
                  {
                      Text = x.Name,
                      Value = x.Id.ToString(),
                  })
                  .ToArray();
        }

        // Overwriteable function for unit testing
        internal Func<Controller, string> GetUserId =
            (controller) => controller.User.Identity.Name;
    }
}

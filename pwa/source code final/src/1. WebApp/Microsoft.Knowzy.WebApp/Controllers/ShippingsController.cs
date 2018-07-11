// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Knowzy.Domain;
using Microsoft.Knowzy.Models.ViewModels;
using Micrososft.Knowzy.Repositories.Contracts;

namespace Microsoft.Knowzy.WebApp.Controllers
{
    public class ShippingsController : Controller
    {
        #region Fields
        
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor

        public ShippingsController(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        #endregion

        #region Public Methods

        public async Task<IActionResult> Index()
        {
            return View(await _orderRepository.GetShippings());
        }

        public async Task<IActionResult> Details(string orderId)
        {
            return View(await _orderRepository.GetShipping(orderId));
        }

        public async Task<IActionResult> Edit(string orderId)
        {
            var getShippingTask = _orderRepository.GetShipping(orderId);
            var getNumberOfAvailableProducts = _orderRepository.GetProductCount();
            await Task.WhenAll(GenerateDropdowns(), getShippingTask, getNumberOfAvailableProducts);
            var order = getShippingTask.Result;
            order.MaxAvailableItems = getNumberOfAvailableProducts.Result;
            return View(order);
        }

        public async Task<IActionResult> Create()
        {
            await GenerateDropdowns();
            return View(new ShippingViewModel
            {
                PostalCarrierId = 1,
                OrderLines = new List<OrderLineViewModel>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingViewModel shipping)
        {
            if (ModelState.IsValid)
            {
                var shippingModel = _mapper.Map<ShippingViewModel, Shipping>(shipping);
                await _orderRepository.AddShipping(shippingModel);
                return RedirectToAction("Index", "Shippings");
            }
            await GenerateDropdowns();
            return View(shipping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShippingViewModel shipping)
        {
            if (ModelState.IsValid)
            {
                var shippingModel = _mapper.Map<ShippingViewModel, Shipping>(shipping);
                await _orderRepository.UpdateShipping(shippingModel);
                return RedirectToAction("Details", "Shippings", new { orderId = shipping.Id });
            }
            await GenerateDropdowns();
            return View(shipping);
        }

        public async Task<IActionResult> AddOrderItem(IEnumerable<string> productIds)
        {
            var itemToAdd =  (await _orderRepository.GetProducts()).FirstOrDefault(product => productIds.All(id => id != product.Id));            
            var orderLineViewmodel = new OrderLineViewModel{ ProductImage = itemToAdd.Image, ProductId = itemToAdd.Id, ProductPrice = itemToAdd.Price, Quantity = 1 };
            return PartialView("EditorTemplates/OrderLineViewModel", orderLineViewmodel);
        }

        public IActionResult Error()
        {
            return View();
        }

        #endregion

        #region Private Methods

        private async Task GenerateDropdowns()
        {
            ViewBag.PostalCarrier = await _orderRepository.GetPostalCarriers();
        }

        #endregion
    }
}

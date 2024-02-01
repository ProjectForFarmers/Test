﻿using Microsoft.AspNetCore.Mvc;
using ProjectForFarmers.Application.DataTransferObjects.Order;
using ProjectForFarmers.Application.Services.Business;
using ProjectForFarmers.Application.ViewModels.Order;
using ProjectForFarmers.Domain;

namespace ProjectForFarmers.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService OrderService;
        private readonly IConfiguration Configuration;

        public OrderController(IOrderService orderService, IConfiguration configuration)
        {
            OrderService = orderService;
            Configuration = configuration;
        }

        [HttpGet("{producerId}/{producer}")]
        [ProducesResponseType(typeof(LoadDashboardVm), 200)]
        public async Task<IActionResult> LoadDashboard([FromRoute] Guid producerId, [FromRoute] Producer producer)
        {
            var vm = await OrderService.LoadDashboard(producerId, producer);
            return Ok(vm);
        }

        [HttpGet("{producerId}/{producer}")]
        [ProducesResponseType(typeof(OrderListVm), 200)]
        public async Task<IActionResult> GetAll([FromRoute] Guid producerId, [FromRoute] Producer producer)
        {
            var vm = await OrderService.GetAll(producerId, producer);
            return Ok(vm);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DashboardVm), 200)]
        public async Task<IActionResult> GetDashboard([FromRoute] Guid id)
        {
            var vm = await OrderService.GetDashboard(id);
            return Ok(vm);
        }

        [HttpGet("{producerId}/{producer}")]
        [ProducesResponseType(typeof(DashboardVm), 200)]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> ExportToExcel([FromRoute] Guid producerId, [FromRoute] Producer producer)
        {
            string fileName = await OrderService.ExportToExcel(producerId, producer);
            string filePath = Configuration["Files"] + "\\fileName";
            string contentType = "application/octet-stream";

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            System.IO.File.Delete(filePath);

            return File(fileBytes, contentType, fileName);
        }

        [HttpGet("{producerId}/{producer}")]
        [ProducesResponseType(typeof(DashboardVm), 200)]
        public async Task<IActionResult> GetCurrentMonthDashboard([FromRoute] Guid producerId, [FromRoute] Producer producer) 
        {
            var vm = await OrderService.GetCurrentMonthDashboard(producerId, producer);

            return Ok(vm);
        }

        [HttpPut]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Duplicate(OrderListDto orderListDto)
        {
            await OrderService.Duplicate(orderListDto);

            return NoContent();
        }

        [HttpDelete]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete(OrderListDto orderListDto)
        {
            await OrderService.Delete(orderListDto);

            return NoContent();
        }
    }
}
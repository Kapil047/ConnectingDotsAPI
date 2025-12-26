using ConnectingDotsAPI.Models;
using ConnectingDotsAPI.Services.CacheService;
using ConnectingDotsAPI.Services;
using ConnectingDotsAPI.Services.OrderService;
using ConnectingDotsAPI.Services.ProductService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace ConnectingDotsAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService, IAuthService authService, ICacheService cacheService) : ControllerBase
    {
        private readonly IOrderService orderService = orderService;
        private readonly IAuthService authService = authService;
        private readonly ICacheService cacheService = cacheService;

        [HttpPost]
        [Route("records")]
        [Authorize]
        public async Task<IActionResult> GetAll([FromBody] DataTablesParameters param)
        {
            try
            {
                var authorization = Request.Headers.Authorization.ToString();
                var userId = authService.GetUserId(authorization);

                string cacheKey = $"product_records";
                var value = cacheService.GetValue(cacheKey);
                var _result = new List<OrderModel.OrderWithDetails>();
                //if (!string.IsNullOrEmpty(value))
                //{
                //    _result = JsonConvert.DeserializeObject<List<OrderModel.OrderWithDetails>>(value);
                //}
                //else
                {
                    _result = await orderService.GetOrders(null);
                    cacheService.SetValue(cacheKey, JsonConvert.SerializeObject(_result), 5);
                }

                var result = _result.AsQueryable();

                param.Columns.ToList().ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(p.Search.Value))
                    {
                        var searchValue = p.Search.Value.ToLower();

                        switch (p.Data)
                        {
                            case "customOrderNumber":
                                result = result.Where(x => !string.IsNullOrEmpty(x.CustomOrderNumber) &&
                                                            x.CustomOrderNumber.ToLower().Contains(searchValue));
                                break;

                            case "customer":
                                result = result.Where(x => !string.IsNullOrEmpty(x.Customer) &&
                                                            x.Customer.ToLower().Contains(searchValue));
                                break;

                            case "orderStatus":
                                result = result.Where(x => !string.IsNullOrEmpty(x.OrderStatus) &&
                                                            x.OrderStatus.ToLower().Contains(searchValue));
                                break;

                            case "poDate":
                                if (DateTime.TryParseExact(searchValue, "dd-MM-yyyy", null,
                                                           System.Globalization.DateTimeStyles.None, out DateTime poDate))
                                {
                                    result = result.Where(x => x.Podate.HasValue && x.Podate.Value.Date == poDate.Date);
                                }
                                break;

                            case "deliveryDate":
                                if (DateTime.TryParseExact(searchValue, "dd-MM-yyyy", null,
                                                           System.Globalization.DateTimeStyles.None, out DateTime deliveryDate))
                                {
                                    result = result.Where(x => x.DeliveryDate.HasValue && x.DeliveryDate.Value.Date == deliveryDate.Date);
                                }
                                break;

                            case "deadline":
                                if (DateTime.TryParseExact(searchValue, "dd-MM-yyyy", null,
                                                           System.Globalization.DateTimeStyles.None, out DateTime deadlineDate))
                                {
                                    result = result.Where(x => x.Deadline.HasValue && x.Deadline.Value.Date == deadlineDate.Date);
                                }
                                break;

                            default:
                                // Optional: Handle unexpected column names gracefully if needed
                                break;
                        }
                    }
                });


                // Apply sorting
                var sortOrder = param.Order.FirstOrDefault();
                if (sortOrder != null)
                {
                    var column = param.Columns[sortOrder.Column].Data;
                    var direction = sortOrder.Dir.ToLower();

                    switch (column)
                    {
                        case "customOrderNumber":
                            result = direction == "asc"
                                ? result.OrderBy(x => x.CustomOrderNumber)
                                : result.OrderByDescending(x => x.CustomOrderNumber);
                            break;

                        case "customer":
                            result = direction == "asc"
                                ? result.OrderBy(x => x.Customer)
                                : result.OrderByDescending(x => x.Customer);
                            break;

                        case "orderStatus":
                            result = direction == "asc"
                                ? result.OrderBy(x => x.OrderStatus)
                                : result.OrderByDescending(x => x.OrderStatus);
                            break;

                        case "poDate":
                            result = direction == "asc"
                                ? result.OrderBy(x => x.Podate)
                                : result.OrderByDescending(x => x.Podate);
                            break;

                        case "deliveryDate":
                            result = direction == "asc"
                                ? result.OrderBy(x => x.DeliveryDate)
                                : result.OrderByDescending(x => x.DeliveryDate);
                            break;

                        case "deadline":
                            result = direction == "asc"
                                ? result.OrderBy(x => x.Deadline)
                                : result.OrderByDescending(x => x.Deadline);
                            break;

                        case "customerId":
                            result = direction == "asc"
                                ? result.OrderBy(x => x.CustomerId)
                                : result.OrderByDescending(x => x.CustomerId);
                            break;

                        default:
                            // Optional: Handle unexpected column names
                            break;
                    }
                }


                if (param.Length == -1) param.Length = _result.Count;
                return new JsonResult(new
                {
                    param.Draw,
                    Data = result.Skip((param.Start / param.Length) * param.Length).Take(param.Length).ToList(),
                    RecordsFiltered = result.Count(),
                    RecordsTotal = _result.Count
                });

            }
            catch (Exception ex)
            {
                if (ex.Message == "TOKEN_NOT_FOUND")
                    return Unauthorized("INVALID_TOKEN");
                return BadRequest(ex.Message);
            }
        }

        // Create Order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderModelRequest.Order orderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await orderService.CreateOrderAsync(orderDto);
            return Ok(order);
        }
        [HttpGet, Route("{id}")]
        public async Task<IActionResult> GetOrders(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();
            var guid = Guid.TryParse(id, out var orderId);
            if (!guid) return BadRequest("The id supplied is incorrect");
            var orders = await orderService.GetOrders(orderId);
            if (orders.Count == 0) return NotFound();
            return Ok(orders.FirstOrDefault());
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await orderService.GetOrders(null);
            return Ok(orders);
        }


    }
}

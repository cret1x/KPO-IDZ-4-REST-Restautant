using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RestRestaurant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : Controller
    {
        /// <summary>
        /// Creates new order
        /// </summary>
        /// <param name="uid">User id</param>
        /// <param name="special_requests">Special requests</param>
        /// <param name="dishes">List of dishes</param>
        /// <returns>Result</returns>
        [HttpPut(Name = "CreateOrder")]
        public IResult CreateOrder(int uid, string special_requests, ICollection<Dish> dishes)
        {
            DatabaseManager.GetInstance().CreateOrder(uid, special_requests, dishes);
            return Results.Ok();
        }

        /// <summary>
        /// Gets order info by order id
        /// </summary>
        /// <param name="id">Order id</param>
        /// <returns>Order</returns>
        [HttpGet(Name = "GetOrder")]
        public IResult GetOrder(int id)
        {
            var order = DatabaseManager.GetInstance().GetOrder(id);
            if (order == null)
            {
                return Results.NotFound();
            } else
            {
                return Results.Json(order);
            }
        }
    }
}

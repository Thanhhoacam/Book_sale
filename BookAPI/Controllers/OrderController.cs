using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/<OrderController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<OrderController>/5
        [HttpGet("{id}")]
        public OrderVM Get(int id)
        {
            OrderVM o = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "Product")
            };

            return o;
        }

        // POST api/<OrderController>
        [HttpPost]
        public IActionResult Post(OrderHeader orderHeader)
        {
            _unitOfWork.OrderHeader.Add(orderHeader);
            _unitOfWork.Save();

            return NoContent();

        }

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, OrderHeader orderHeader)
        {
           if (id != orderHeader.Id)
            {
                return BadRequest();
            }
           

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            return NoContent();

        }
        [HttpPut("StartProcessing/{id}")]
        public IActionResult StartProcessing(int id)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == id);
            if (orderHeaderFromDb == null)
            {
                return NotFound();
            }


            _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            return NoContent();

        }
        [HttpPut("ShipOrder/{id}")]
        public IActionResult ShipOrder(int id, OrderHeader orderHeader)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == id);
            if (orderHeaderFromDb == null)
            {
                return NotFound();
            }
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            return NoContent();

        }
        [HttpPut("CancelOrder/{id}")]
        public IActionResult CancelOrder(int id, OrderHeader orderHeader)
        {
            if (id != orderHeader.Id)
            {
                return BadRequest();
            }
          
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == id);
            if (orderHeaderFromDb == null)
            {
                return NotFound();
            }

            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
               _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            
            _unitOfWork.Save();
            return NoContent();

        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id);
            if (orderHeader == null)
            {
                return NotFound();
            }
            _unitOfWork.OrderHeader.Remove(orderHeader);
            _unitOfWork.Save();
            return NoContent();
        }
    }
}

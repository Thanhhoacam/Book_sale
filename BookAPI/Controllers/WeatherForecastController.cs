using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
       

        private readonly IUnitOfWork _unitOfWork;

        public WeatherForecastController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<ProductController> Get()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return (IEnumerable<ProductController>)objProductList;
        }
    }
}

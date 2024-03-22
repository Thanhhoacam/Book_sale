using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

        }
        // GET: api/<ProductController>
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return objProductList;
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public Product Get(int id)
        {
            Product product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
            return product;
        }

        // POST api/<ProductController>
        [HttpPost]
        public IActionResult Post(Product product)
        {
            product.Id = 0;
            _unitOfWork.Product.Add(product);
            _unitOfWork.Save();

            return NoContent();
        }

        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, Product product)
        {
            Product newProduct = _unitOfWork.Product.Get(p => p.Id == id);
            if (newProduct == null)
            {
                return NotFound();
            }

            // Assuming _unitOfWork is your DbContext instance


            _unitOfWork.Product.Update(product);

            _unitOfWork.Save();




            return NoContent();


        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Product product = _unitOfWork.Product.Get(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            return NoContent();
        }

        // DELETE api/<ProductController>/deleteImg/5
        [HttpDelete("deleteImg/{id}")]
        public IActionResult DeleteImage(int id)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == id);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();
            }
            return NoContent();
        }
    }
}

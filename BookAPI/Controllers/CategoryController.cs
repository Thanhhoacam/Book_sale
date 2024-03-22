using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;


        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        // GET: api/<ProductController>
        [HttpGet]
        public IEnumerable<Category> Get()
        {
            List<Category> objProductList = _unitOfWork.Category.GetAll().ToList();
            return objProductList;
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public Category Get(int id)
        {
           Category category = _unitOfWork.Category.Get(u => u.Id == id);
            return category;
        }

        // POST api/<ProductController>
        [HttpPost]
        public IActionResult Post(Category category)
        {

            category.Id = 0;
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
               
            return NoContent();
        }

        // PUT api/<ProductController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, Category category)
        {
            Category newCategory = _unitOfWork.Category.Get(p => p.Id == id);
            if (newCategory == null)
            {
                return NotFound();
            }

            // Assuming _unitOfWork is your DbContext instance
          

            _unitOfWork.Category.Update(category);

            _unitOfWork.Save();

           
            
           
            return NoContent();

         
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Category product = _unitOfWork.Category.Get(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(product);
            _unitOfWork.Save();
            return NoContent();
        }
    }
}

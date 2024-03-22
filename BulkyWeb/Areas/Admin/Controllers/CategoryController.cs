using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient client = null;
        private string ProductAPIBaseURL = "";
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            ProductAPIBaseURL = "http://localhost:5236/api/Category";
        }
        public async Task<IActionResult> Index()
        {
            //List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            HttpResponseMessage response = await client.GetAsync(ProductAPIBaseURL);
            string stringData = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            List<Category> objCategoryList = System.Text.Json.JsonSerializer.Deserialize<List<Category>>(stringData, options);
            return View(objCategoryList);
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
            }

            if (ModelState.IsValid)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                // Cấu hình quy tắc ánh xạ mặc định cho tất cả các đối tượng
                var contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy() // Áp dụng các quy tắc đặt tên (ví dụ: CamelCase)
                };

                // Cấu hình JsonSerializer để sử dụng DefaultContractResolver đã cấu hình
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                };

                string stringData = JsonConvert.SerializeObject(obj, settings);
                var contentData = new StringContent(stringData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(ProductAPIBaseURL, contentData);
                //_unitOfWork.Category.Add(obj);
                //_unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);
            //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
            //Category? categoryFromDb2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();

            HttpResponseMessage response = await client.GetAsync("http://localhost:5236/api/Category/"+id);
            string stringData = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Cấu hình quy tắc ánh xạ mặc định cho tất cả các đối tượng
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy() // Áp dụng các quy tắc đặt tên (ví dụ: CamelCase)
            };

            // Cấu hình JsonSerializer để sử dụng DefaultContractResolver đã cấu hình
            var settings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            };

            Category categoryFromDb = JsonConvert.DeserializeObject<Category>(stringData, settings);

            return View(categoryFromDb);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                // Cấu hình quy tắc ánh xạ mặc định cho tất cả các đối tượng
                var contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy() // Áp dụng các quy tắc đặt tên (ví dụ: CamelCase)
                };

                // Cấu hình JsonSerializer để sử dụng DefaultContractResolver đã cấu hình
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                };

                string stringData = JsonConvert.SerializeObject(obj, settings);
                var contentData = new StringContent(stringData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(ProductAPIBaseURL + "/" + obj.Id, contentData);
                //_unitOfWork.Category.Update(obj);
                //_unitOfWork.Save();
                if (response.IsSuccessStatusCode)
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();

        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            //Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
            HttpResponseMessage response = await client.DeleteAsync(ProductAPIBaseURL + "/" + id);
            if (response.IsSuccessStatusCode)
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}

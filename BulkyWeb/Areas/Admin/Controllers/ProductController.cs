using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAcess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly HttpClient client = null;
        private string ProductAPIBaseURL = "";
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;

           
         

            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            ProductAPIBaseURL = "http://localhost:5236/api/Product";
        }
        public async Task<IActionResult> Index() 
        {
            HttpResponseMessage response = await client.GetAsync(ProductAPIBaseURL);
            string stringData = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            List<Product> objProductList = System.Text.Json.JsonSerializer.Deserialize<List<Product>>(stringData,options);
           
            return View(objProductList);
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            HttpResponseMessage response = await client.GetAsync("http://localhost:5236/api/Category");
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
            List<Category> objCategoryList = System.Text.Json.JsonSerializer.Deserialize<List<Category>>(stringData, options);
            ProductVM productVM = new()
            {
                CategoryList = objCategoryList.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };


            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                HttpResponseMessage response2 = await client.GetAsync("http://localhost:5236/api/Product/"+id);
                 string stringData2 = await response2.Content.ReadAsStringAsync();
                //update
                

                // Sử dụng JsonSerializerSettings trong quá trình Serialize/Deserialize
                var pro = JsonConvert.DeserializeObject<Product>(stringData2, settings);
                productVM.Product = pro;

                return View(productVM);
            }
            
        }
        [HttpPost]
        public async Task<IActionResult> Upsert(ProductVM productVM, List<IFormFile> files)
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

            string stringData = JsonConvert.SerializeObject(productVM.Product,settings);
            var contentData = new StringContent(stringData, Encoding.UTF8, "application/json");
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0) {
                    //_unitOfWork.Product.Add(productVM.Product);
                    HttpResponseMessage response = await client.PostAsync(ProductAPIBaseURL, contentData);
                }
                else {
                    //_unitOfWork.Product.Update(productVM.Product);
                    HttpResponseMessage response = await client.PutAsync(ProductAPIBaseURL+"/"+ productVM.Product.Id, contentData);
                }

                //_unitOfWork.Save();


                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {

                    foreach(IFormFile file in files) 
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create)) {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new() {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId=productVM.Product.Id,
                        };

                        if (productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();

                        productVM.Product.ProductImages.Add(productImage);

                    }

                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();




                }

                
                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                HttpResponseMessage response = await client.GetAsync("http://localhost:5236/api/Category");
                 stringData = await response.Content.ReadAsStringAsync();
                 options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
               
                List<Category> objCategoryList = System.Text.Json.JsonSerializer.Deserialize<List<Category>>(stringData, options);
               
                productVM.CategoryList = objCategoryList.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }


        public async Task<IActionResult> DeleteImage(int imageId) {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null) {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl)) {
                    var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath)) {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }

        #region API CALLS

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath)) {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths) {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}

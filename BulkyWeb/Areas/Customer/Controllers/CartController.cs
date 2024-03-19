using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers {

    [Area("customer")]
    [Authorize]
    public class CartController : Controller {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender) {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }


        public IActionResult Index() {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new() {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader= new()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var cart in ShoppingCartVM.ShoppingCartList) {
                cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.Product.Id).ToList();
                cart.Price = cart.Product.Price;
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary() {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new() {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
           


            foreach (var cart in ShoppingCartVM.ShoppingCartList) {
                cart.Price = cart.Product.Price;
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST() {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

			ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


			foreach (var cart in ShoppingCartVM.ShoppingCartList) {
				cart.Price = cart.Product.Price;
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            
				//it is a regular customer 
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			
				
			
			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();
            foreach(var cart in ShoppingCartVM.ShoppingCartList) {
                OrderDetail orderDetail = new() {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
               
            }
            HttpContext.Session.Clear();
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ShoppingCartList);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
		}


       


		public IActionResult Plus(int cartId) {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId) {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1) {
                //remove that from cart
                
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId) {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
           
            _unitOfWork.ShoppingCart.Remove(cartFromDb);

            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
              .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }



        
    }
}

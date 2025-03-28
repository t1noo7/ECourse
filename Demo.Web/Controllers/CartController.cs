using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Demo.Core.Models;
using Microsoft.AspNetCore.Identity;
using Demo.Core.Services;
using Demo.Core.Repositories;
using Demo.Application.Repositories;

namespace Demo.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IMongoClient _client;
        private readonly UserManager<User> _userManager;
        private readonly IUserGroupManager _userGroupManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        public CartController(IMongoClient client,
            UserManager<User> userManager,
            IUserGroupManager userGroupManager,
            SignInManager<User> signInManager,
            IUserRepository userRepository,
            ICourseRepository courseRepository
            )
        {
            _client = client;
            _userManager = userManager;
            _userGroupManager = userGroupManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
        }

        [Route("/cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            var user = _userRepository.GetByUsername(User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (user.CartItem == null || !user.CartItem.Any())
            {
                return View(new List<Course>()); // Trả về view với danh sách trống
            }
            ViewBag.CartCount = user.CartItem.Count;
            return View(user.CartItem);
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(Guid courseID)
        {
            var user = _userRepository.GetByUsername(User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }
            if (user.CartItem.Any(c => c.Id == courseID))
            {
                return Json(new { success = false, message = "Course already in cart" });
            }
            var course = _courseRepository.Find(c => c.Id == courseID).SingleOrDefault();
            if (course == null)
            {
                return Json(new { success = false, message = "Course not found" });
            }
            user.CartItem.Add(course);
            _userRepository.UpdateAsync(user);
            return Json(new { success = true, message = "Course added to cart successfully" });
        }

        // [HttpPost]
        // [Route("api/cart/update")]
        // public IActionResult UpdateCart(Guid courseID, int? quantity)
        // {
        //     // lấy giỏ hàng ra để xử lý
        //     var user = _userRepository.GetByUsername(User.Identity.Name);
        //     try
        //     {
        //         if (user.CartItem != null)
        //         {
        //             CartItem item = cart.SingleOrDefault(p => p.courses.Id == courseID);
        //             if (item != null && quantity.HasValue)//đã có --) cập nhật số lượng
        //             {
        //                 item.quantity = quantity.Value;
        //             }
        //             //lưu lại session
        //             HttpContext.Session.Set<List<CartItem>>("CartItems", cart);
        //         }
        //         return Json(new { success = true });
        //     }
        //     catch
        //     {
        //         return Json(new { success = false });
        //     }
        // }

        [HttpPost]
        [Route("api/cart/remove")]
        public async Task<IActionResult> Remove(Guid courseID)
        {
            var user = _userRepository.GetByUsername(User.Identity.Name);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            var courseToRemove = user.CartItem.FirstOrDefault(c => c.Id == courseID);
            if (courseToRemove == null)
            {
                return Json(new { success = false, message = "Course not found in cart" });
            }

            user.CartItem.Remove(courseToRemove);
            var result = await _userRepository.UpdateAsync(user);

            return result != null
                ? Json(new { success = true, message = "Course removed successfully" })
                : Json(new { success = false, message = "Error removing course" });
        }
    }
}
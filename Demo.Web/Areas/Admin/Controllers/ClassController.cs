using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Helpers;
using Demo.Database.Repositories;
using Demo.Web.Areas.Admin.Models;

namespace Demo.Web.Areas.Admin.Controllers
{
    //[WebAuthorize(RoleList.Content, RoleList.Product, RoleList.Admin)]
    [Area("Admin")]
    public class ClassController : Controller
    {
        private readonly ILogger<ClassController> _logger;
        private readonly IClassRepository _classRepository;
        private readonly ICourseRepository _courseRepository;

        public ClassController(ILogger<ClassController> logger,
            IClassRepository classRepository, ICourseRepository courseRepository)
        {
            _logger = logger;
            _classRepository = classRepository;
            _courseRepository = courseRepository;
        }

        public IActionResult Index(Guid courseId)
        {
            if (courseId != Guid.Empty)
            {
                var classes = _classRepository.Find(x => x.Deleted == false && x.Course.Id == courseId).ToList();
                return View(classes);
            }
            else
            {
                var classes = _classRepository.Find(x => x.Deleted == false).ToList();
                return View(classes);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var classroom = await _classRepository.GetByIdAsync(id);
            if (classroom == null) return NotFound();

            return View(classroom);
        }

        public IActionResult Edit(Guid? id)
        {
            Class? model = null;
            var lscourse = _courseRepository.Find(x => x.Deleted == false).ToList();

            if (id.HasValue)
            {
                model = _classRepository.Get(id.Value);
            }

            if (model == null)
            {
                model = new Class
                {
                    Id = Guid.NewGuid()
                };
            }
            ViewBag.Courses = lscourse;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Class model, Guid courseId, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid && (!ModelState.ContainsKey("returnUrl") && !ModelState.ContainsKey("fileInput")))
                {
                    return View(model);
                }

                model.ModifiedBy = User?.Identity?.Name;
                model.Modified = DateTimeExtensions.UTCNowVN;
                var course = _courseRepository.Find(x => x.Id == courseId).FirstOrDefault();

                if (model.Id != Guid.Empty && model.Id != null)
                {
                    model.CreatedBy = model.ModifiedBy;
                    model.Created = DateTimeExtensions.UTCNowVN;
                    model.Course = course;
                }

                if (string.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.ClassName);
                    if (_classRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_classRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }

                await _classRepository.UpsertAsync(model);
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _classRepository.DeleteAsync(id);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> ViewStudentsInClass(Guid classId)
        {
            var students = await _classRepository.GetStudentsInClassAsync(classId);
            return PartialView("_ViewStudentsInClassPopup", students);
        }

        [HttpGet]
        public async Task<IActionResult> AssignStudentsPopup()
        {
            var students = await _classRepository.GetApprovedStudentsWithCourseAsync();
            var courses = _courseRepository.Find(x => !x.Deleted).ToList();

            var vm = new AssignStudentsViewModel
            {
                ApprovedStudents = students,
                AvailableCourses = courses
            };

            return PartialView("_AssignStudentsPopup", vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableClassesByCourse(Guid courseId)
        {
            var classes = await _classRepository.GetClassesByCourseIdAsync(courseId);
            var result = classes.Select(c => new
            {
                id = c.Id,
                className = c.ClassName,
                studentCount = c.StudentIds.Count
            });
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> AssignStudentsToClass(Guid classId, List<Guid> studentIds)
        {
            await _classRepository.AddStudentsToClassAsync(classId, studentIds);
            return Ok();
        }
    }
}

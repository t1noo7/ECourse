using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Demo.Web.Models;
using Demo.Application.Repositories;

namespace Demo.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBannerRepository _bannerRepository;

    public HomeController(ILogger<HomeController> logger,
        IBannerRepository bannerRepository)
    {
        _logger = logger;
        _bannerRepository = bannerRepository;
    }

    public IActionResult Index()
    {
        var banners = _bannerRepository.Find(x => x.Status && x.Deleted == false).OrderBy(x => x.Order).ToList();
        
        ViewBag.Banners = banners;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AboutUs()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}

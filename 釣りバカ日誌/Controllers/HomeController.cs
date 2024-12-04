using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Diagnostics;
using System.Net;
using System.Text;
using 釣りバカ日誌.Models;
using 釣りバカ日誌.Service;
using 釣りバカ日誌.Service.IService;

namespace 釣りバカ日誌.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _conf;
        private readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, IConfiguration conf, IHomeService homeService)
        {
            _logger = logger;
            _conf = conf;
            _homeService = homeService;
        }

        public IActionResult Index(HomeViewModel model)
        {
            return View();
        }

        public IActionResult GetHomeData(HomeViewModel model)
        {
            model = _homeService.GetHomeData(model);

            return Json(model);
        }

        public IActionResult Edit(EditViewModel model)
        {
            return View(model);
        }

        public IActionResult GetEditData(EditViewModel model)
        {
            model = _homeService.GetEditData(model);

            return Json(model);
        }

        public IActionResult GetMunicipalities(EditViewModel model)
        {
            model = _homeService.GetMunicipalities(model);

            return Json(model);
        }

        public IActionResult GetTideData(EditViewModel model)
        {
            model = _homeService.GetTideData(model);

            return Json(model);
        }

        public IActionResult Update(EditViewModel model)
        {
            model = _homeService.Update(model);

            return Json(model);
        }

        public IActionResult Delete(HomeViewModel model)
        {
            model = _homeService.Delete(model);

            return Json(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
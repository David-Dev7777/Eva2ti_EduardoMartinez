using System.Diagnostics;
using estructura_pagina.Models;
using Microsoft.AspNetCore.Mvc;


namespace Eva2ti_EduardoMartinez.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Lista de imágenes dinámicas
        public ActionResult IndexImage()
        {
            var images = new List<imageMolde>
            {
                new imageMolde { Id = 1, Title = "Tatuador Profesional", ImagePath = "/images/tatuador_alejandro.jpg" },
                new imageMolde { Id = 2, Title = "Máquina de Tatuaje", ImagePath = "/images/maquina-tattoo.jpg" },
                new imageMolde { Id = 3, Title = "Agujas para Tatuaje", ImagePath = "/images/ajugas_tatto.jpg" },
            };

            return View(images);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // borra la cache para el cierre de sesion
        public IActionResult Ventas()
        {
            ViewBag.MensajeLoguin = "";
            // Verificar si la sesión está activa
            if (HttpContext.Session.GetString("RUT") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View("/Views/formularios/vista_ventas.cshtml");


        }
        public IActionResult Error_usu()
        {
             return View("/Views/formularios/Error_Usu.cshtml");
        }


        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login()
        {
            // Verificar si la sesión está activa
            if (HttpContext.Session.GetString("RUT") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View("/Views/formularios/vista_login.cshtml");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Admin_usu()
        {
            // Verificar si la sesión está activa
            if (HttpContext.Session.GetString("RUT") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar el rol del usuario
            if (HttpContext.Session.GetString("ROL")?.Trim() == "ADMINISTRADOR")
            {
                return View("/Views/formularios/Admin_usu.cshtml");
            }
            else
            {
                ViewBag.MensajeLoguin = "Ud no es usuario ADMINISTRADOR";
                return View("/Views/formularios/Error_Usu.cshtml");
            }
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Editar_usu()
        {
            // Verificar si la sesión está activa
            if (HttpContext.Session.GetString("RUT") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            else if (HttpContext.Session.GetString("ROL")?.Trim() == "ADMINISTRADOR")
            {
                return View("/Views/formularios/Edit_usu.cshtml");
            }
            else
            {
                ViewBag.MensajeLoguin = "Ud no es usuario ADMINISTRADOR";
                return View("/Views/formularios/Error_Usu.cshtml");
            }
            
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Agg_usu()
        {
            // Verificar si la sesión está activa
            if (HttpContext.Session.GetString("RUT") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            else if (HttpContext.Session.GetString("ROL")?.Trim() == "ADMINISTRADOR")
            {
                return View("/Views/formularios/Agregar_usuario.cshtml");
            }
            else
            {
                ViewBag.MensajeLoguin = "Ud no es usuario ADMINISTRADOR";
                return View("/Views/formularios/Error_Usu.cshtml");
            }

            
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Config()
        {
            // Verificar si la sesión está activa
            if (HttpContext.Session.GetString("RUT") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            else if (HttpContext.Session.GetString("ROL")?.Trim() == "ADMINISTRADOR")
            {
                return View("/Views/formularios/vista_config.cshtml");
            }
            else
            {
                ViewBag.MensajeLoguin = "Ud no es usuario ADMINISTRADOR";
                return View("/Views/formularios/Error_Usu.cshtml");
            }

            
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

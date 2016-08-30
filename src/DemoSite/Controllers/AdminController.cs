using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ChessBucket.Models.GameViewModels;
using System.Text;
using ChessBucket.Data;
using ChessBucket.Models;
using ChessBucket.Models.AdminViewModels;
using Microsoft.EntityFrameworkCore;

namespace ChessBucket.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Queue");
        }
        public IActionResult Queue()
        {
            return View();
        }
        [HttpGet]
        public string SearchQueue(int page)
        {
            var batchItems = _context.BatchQueue.Take(40);
            QueueViewModel vm = new QueueViewModel();
            vm.QueueItems = batchItems.Select(i => QueueItemViewModel.CreateFrom(i)).ToList();
            return JsonConvert.SerializeObject(vm);
        }

        [HttpGet]
        public void TriggerQProcessor()
        {
            var t = new Task(() =>
            {
                QueueProcessor p = new QueueProcessor();
                p.Run();
            });
            t.Start();
        }
    }
}

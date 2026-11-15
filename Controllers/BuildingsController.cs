using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusRooms.Data;
using CampusRooms.Models;

namespace CampusRooms.Controllers
{
    public class BuildingsController : Controller
    {
        private readonly AppDbContext _db;

        public BuildingsController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("buildings/{id:int}")]
        public IActionResult Details(int id)
        {
            Building? building = _db.Buildings.Include(each_building => each_building.Rooms).FirstOrDefault(each_building => each_building.Id == id);
            if (building == null)
            {
                return NotFound();
            }
            ViewData["Title"] = building.Name;
            return View(building);
        }
    }
}

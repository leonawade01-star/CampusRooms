using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using CampusRooms.Data;
using CampusRooms.Models;


namespace CampusRooms.Controllers
{
    // The rooms pages: list, details, add, edit, delete.
    public class RoomsController : Controller
    {
        private readonly AppDbContext _db;

        public RoomsController(AppDbContext db)
        {
            _db = db;
        }

        private void LoadBuildings()
        {
            List<Building> buildings = _db.Buildings.OrderBy(building => building.Name).ToList();
            ViewBag.Buildings = new SelectList(buildings, "Id", "Name");
        }

        // GET /rooms: every room, sorted by code
        [HttpGet("rooms")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Rooms";
            List<Room> rooms = _db.Rooms.Include(each_room => each_room.Building).OrderBy(each_room => each_room.Code).ToList();
            ViewBag.Count = rooms.Count;
            return View(rooms);
        }

        // GET /rooms/2: one room. /rooms/99 is a 404.
        [HttpGet("rooms/{id:int}")]
        public IActionResult Details(int id)
        {
            Room? room = _db.Rooms.Include(each_room => each_room.Building).FirstOrDefault(each_room => each_room.Id == id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["Title"] = room.Code;
            return View(room);
        }

        [HttpGet("rooms/seats/{min_seats:int}")]
        public IActionResult BigRooms(int min_seats)
        {
            List<Room> rooms = _db.Rooms.FromSql($"SELECT * FROM Rooms WHERE Seats >= {min_seats}").Include(each_room => each_room.Building).OrderByDescending(each_room => each_room.Seats).ToList();
            ViewData["Title"] = $"Rooms with at least" + min_seats + "seats";
            ViewBag.MinSeats = min_seats;
            return View("BigRooms", rooms);
        }

        // GET /rooms/add: an empty form
        [Authorize(Policy = "CanManageRooms")]
        [HttpGet("rooms/add")]
        public IActionResult Add()
        {
            ViewData["Title"] = "Add a room";
            LoadBuildings();
            return View(new Room());
        }

        // POST /rooms/add: save the new room, or show the form again with the messages
        [Authorize(Policy = "CanManageRooms")]
        [HttpPost("rooms/add")]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Room new_room)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Add a room";
                LoadBuildings();
                return View(new_room);
            }

            _db.Rooms.Add(new_room);
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // The unique index on Code said no
                ModelState.AddModelError("Code", "Another room already has that code");
                ViewData["Title"] = "Add a room";
                return View(new_room);
            }

            TempData["Message"] = $"{new_room.Code} {new_room.Name} was added";
            return RedirectToAction("Index");
        }

        // GET /rooms/edit/2: the form, filled in. /rooms/edit/99 is a 404.
        [Authorize(Policy = "CanManageRooms")]
        [HttpGet("rooms/edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Edit a room";
            LoadBuildings();
            return View(room);
        }

        // POST /rooms/edit/2: save the changes to the same row
        [Authorize(Policy = "CanManageRooms")]
        [HttpPost("rooms/edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Room changed_room)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit a room";
                LoadBuildings();
                return View(changed_room);
            }

            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                TempData["Message"] = "That room was deleted before you saved";
                return RedirectToAction("Index");
            }
            room.Code = changed_room.Code;
            room.Name = changed_room.Name;
            room.Seats = changed_room.Seats;
            room.HasProjector = changed_room.HasProjector;

            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("Code", "Another room already has that code");
                ViewData["Title"] = "Edit a room";
                return View(changed_room);
            }

            TempData["Message"] = $"Saved {room.Code}";
            return RedirectToAction("Index");
        }

        // GET /rooms/delete/2: ask first. Nothing is deleted yet.
        [Authorize(Roles = "Admin")]
        [HttpGet("rooms/delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            ViewData["Title"] = "Delete a room";
            return View(room);
        }

        // POST /rooms/delete/2: the Yes button. Now the row goes.
        [Authorize(Roles = "Admin")]
        [HttpPost("rooms/delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                TempData["Message"] = "That room was already deleted";
                return RedirectToAction("Index");
            }
            _db.Rooms.Remove(room);
            _db.SaveChanges();
            TempData["Message"] = $"{room.Code} {room.Name} was deleted";
            return RedirectToAction("Index");
        }
    }
}

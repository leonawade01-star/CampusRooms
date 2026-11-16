using CampusRooms.Data;
using CampusRooms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomsApiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RoomsApiController(AppDbContext db)
        {
            _db = db;
        }

        private static RoomDto ToDto(Room room)
        {
            RoomDto dto = new RoomDto();
            dto.Id = room.Id;
            dto.Code = room.Code;
            dto.Name = room.Name;
            dto.Seats = room.Seats;
            dto.HasProjector = room.HasProjector;
            if (room.Building != null)
            {
                dto.BuildingName = room.Building.Name;
            }
            return dto;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Room> rooms = _db.Rooms
                .Include(each_room => each_room.Building)
                .OrderBy(each_room => each_room.Code)
                .ToList();

            List<RoomDto> result = new List<RoomDto>();
            foreach (Room room in rooms)
            {
                result.Add(ToDto(room));
            }
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetOne(int id)
        {
            Room? room = _db.Rooms
                .Include(each_room => each_room.Building)
                .FirstOrDefault(each_room => each_room.Id == id);
            if (room == null)
            {
                return NotFound();
            }
            return Ok(ToDto(room));
        }

        private bool BuildingIsValid(int? building_id)
        {
            Building? building = _db.Buildings.Find(building_id);
            if (building == null)
            {
                ModelState.AddModelError("BuildingId", "There is no building with that Id");
                return false;
            }
            return true;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Create(RoomInput input)
        {
            if (!BuildingIsValid(input.BuildingId))
            {
                return ValidationProblem(ModelState);     
            }

            Room room = new Room();
            room.Code = input.Code;
            room.Name = input.Name;
            room.Seats = input.Seats;
            room.HasProjector = input.HasProjector;
            room.BuildingId = input.BuildingId;

            _db.Rooms.Add(room);
            try
            {
                _db.SaveChanges();      
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Another room already has that code" });
            }
            _db.Entry(room).Reference(saved_room => saved_room.Building).Load();
            return CreatedAtAction(nameof(GetOne), new { id = room.Id }, ToDto(room));
        }

        [AllowAnonymous]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, RoomInput input)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            if (!BuildingIsValid(input.BuildingId))
            {
                return ValidationProblem(ModelState);
            }
            room.Code = input.Code;
            room.Name = input.Name;
            room.Seats = input.Seats;
            room.HasProjector = input.HasProjector;
            room.BuildingId = input.BuildingId;

            try
            {
                _db.SaveChanges();      
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Another room already has that code" });
            }
            return NoContent();
        }

        [AllowAnonymous]
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            Room? room = _db.Rooms.Find(id);
            if (room == null)
            {
                return NotFound();
            }
            _db.Rooms.Remove(room);
            _db.SaveChanges();          
            return NoContent();
        }
    }
}
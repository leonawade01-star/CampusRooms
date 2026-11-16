using System.ComponentModel.DataAnnotations;

namespace CampusRooms.Models
{
    public class RoomInput
    {
        [Required(ErrorMessage = "Give the room code, e.g. MB-101")]
        [StringLength(10, ErrorMessage = "The room code is at most 8 characters")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Give the room a name")]
        [StringLength(40, ErrorMessage = "The name is at most 40 characters")]
        public string Name { get; set; } = "";

        [Range(1, 300, ErrorMessage = "Seats must be from 1 to 300")]
        public int Seats { get; set; }

        public bool HasProjector { get; set; }

        [Required(ErrorMessage = "Pick a building")]
        public int? BuildingId { get; set; }
    }
}
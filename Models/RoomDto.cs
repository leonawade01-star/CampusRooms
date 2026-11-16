namespace CampusRooms.Models
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Seats { get; set; }
        public bool HasProjector { get; set; }
        public string? BuildingName { get; set; }
    }
}

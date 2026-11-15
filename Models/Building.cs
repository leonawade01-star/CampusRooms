using System.Runtime.CompilerServices;

namespace CampusRooms.Models
{
    public class Building
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Room> Rooms { get; set; } = new List<Room>();
    }
}

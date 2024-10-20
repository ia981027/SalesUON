using MongoDB.Bson; // Import the MongoDB.Bson namespace
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace SalesUON.Models
{
    public class Sale
    {
        [BsonId] // Mark this property as the MongoDB _id field
        [BsonRepresentation(BsonType.ObjectId)] // Specify the data type for _id
        public string Id { get; set; } // You can use 'string' for ObjectId

        public string Make { get; set; }
        public string Model { get; set; }
        [Key]
        public string VIN { get; set; }
        public DateTime SaleDate { get; set; }
    }
}

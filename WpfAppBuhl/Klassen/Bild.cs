using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppBuhl.Klassen
{
    public class Bild
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.Binary)]
        public byte[] BildBytes { get; set; }
        public string ContentType { get; set; }
        public ObjectId PersonId { get; set; }

        public Bild(byte[] BildBytes, string ContentType, ObjectId PersonId) 
        {
            this.BildBytes = BildBytes;
            this.ContentType = ContentType;
            this.PersonId = PersonId;
        }
    }
}

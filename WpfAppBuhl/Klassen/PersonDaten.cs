using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppBuhl.Klassen
{
    public class PersonDaten
    {
        [BsonId]
        public ObjectId Id { get; set; }

        //falls daten nicht eingegeben, wird Unbekannt eingetragen
        public string Name { get; set; }
        public string Nachname { get; set; } = "Unbekannt";
        public string Strasse { get; set; } = "Unbekannt";
        public string Plz { get; set; } = "Unbekannt";
        public string Ort { get; set; } = "Unbekannt";
        public List<string> Telefon { get; set; }

        public PersonDaten(string name, string nachname, string strasse, string plz, string ort, List<string> telefon) 
        {
            Name = name;
            Nachname = nachname;
            Strasse = strasse;
            Plz = plz;
            Ort = ort;
            Telefon = telefon;
        }
    }
}

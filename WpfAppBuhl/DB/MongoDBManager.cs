using WpfAppBuhl.Klassen;
using WpfAppBuhl;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Cryptography;

namespace WpfAppBuhl.DB
{
    public static class MongoDBManager
    {
        private static MongoDBConnection connection;
        static MongoDBManager()
        {
            connection = new MongoDBConnection();
        }

        public static void SpeicherePerson(PersonDaten personDaten, byte[] bildBytes = null)
        {
            //Dokument zum speichern in der DB erstellen
            var document = new BsonDocument
            {
                { "Name", personDaten.Name },
                { "Nachname", personDaten.Nachname },
                { "Strasse", personDaten.Strasse },
                { "Plz", personDaten.Plz },
                { "Ort", personDaten.Ort },
                { "Telefon", new BsonArray(personDaten.Telefon) }
            };
            

            try
            {
                //Verbinde zur buhl DB und der Collection person
                var collection = connection.GetCollection<BsonDocument>("person");
                //var collection = database.GetCollection<BsonDocument>("person");
                collection.InsertOne(document);

                //Holt sich die gerade, durch Speicherung erstellte, ObjectId
                var personId = document["_id"].AsObjectId;

                //Falls bild ausgewählt wurde
                if (bildBytes != null) 
                {
                    var bildDaten = new Bild(
                    bildBytes,
                    "image/jpeg",
                    personId
                    );

                    SpeichereBild(bildDaten);
                }
                MessageBox.Show("Deine Daten wurden erfolgreich angelegt");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //MessageBox.Show($"{collection}");
        }

        public static void AktualisierePerson(PersonDaten personDaten)
        {
            var collection = connection.GetCollection<PersonDaten>("person");

            //Finde DB Eintrag nach Id
            var filter = Builders<PersonDaten>.Filter.Eq(person => person.Id, personDaten.Id);
            //Aktualisiere die Einträge für die DB im richtigen MongoDB Format
            var update = Builders<PersonDaten>.Update
            .Set(person => person.Name, personDaten.Name)
            .Set(person => person.Nachname, personDaten.Nachname)
            .Set(person => person.Strasse, personDaten.Strasse)
            .Set(person => person.Plz, personDaten.Plz)
            .Set(person => person.Ort, personDaten.Ort)
            .Set(person => person.Telefon, personDaten.Telefon);

            var result = collection.UpdateOne(filter, update);

            var aktualisierungen = result.ModifiedCount;

            //Überprüfe, ob etwas aktualisiert wurde
            if (aktualisierungen > 0)
            {
                MessageBox.Show($"es wurden {aktualisierungen} Textfelder aktualisiert!");
            }
            else
            {
                MessageBox.Show("Es wurden keine Einträge aktualisiert");
            }
        }

        public static List<PersonDaten> LadeAllePersonen()
        {
            var collection = connection.GetCollection<PersonDaten>("person");

            //Alle Personen laden und nach Namen sortieren
            return collection.Find(person => true)
                            .SortBy(p => p.Name)
                            .ToList();
        }

        public static void LöschePerson(ObjectId personObjectId)
        {
            var personCollection = connection.GetCollection<PersonDaten>("person");
            var bildCollection = connection.GetCollection<Bild>("bilder");


            try
            {
                //Bild der Person löschen
                var bildFilter = Builders<Bild>.Filter.Eq(bild => bild.PersonId, personObjectId);
                var bildResult = bildCollection.DeleteOne(bildFilter);

                //Person selbst löschen
                var filter = Builders<PersonDaten>.Filter.Eq(person => person.Id, personObjectId);
                var result = personCollection.DeleteOne(filter);

                if (result.DeletedCount > 0)
                {
                    MessageBox.Show("Die Person wurde gelöscht");
                }
                else
                {
                    MessageBox.Show("Keine Person gefunden");
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        public static void SpeichereBild(Bild bildDaten)
        {
            var collection = connection.GetCollection<Bild>("bilder");

            try
            {
                //Bild speichern
                collection.InsertOne(bildDaten);
                MessageBox.Show("Bild wurde erfolgreich gespeichert");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern des Bildes: {ex.Message}");
            }
        }

        public static Bild LadeBild(ObjectId personId)
        {
            var collection = connection.GetCollection<Bild>("bilder");

            var filter = Builders<Bild>.Filter.Eq(b => b.PersonId, personId);
            return collection.Find(filter).FirstOrDefault();
        }

        public static void AktualisiereBild(ObjectId personId, Bild neuesBild)
        {
            var collection = connection.GetCollection<Bild>("bilder");

            //Richtiges Bild finden nach der dazugehörigen Person
            var filter = Builders<Bild>.Filter.Eq(bild => bild.PersonId, personId);
            //BildBytes und ContentType aktualisieren
            var update = Builders<Bild>.Update
                .Set(b => b.BildBytes, neuesBild.BildBytes)
                .Set(b => b.ContentType, neuesBild.ContentType);
            try
            {
                var result = collection.UpdateOne(filter, update);

                if (result.ModifiedCount <= 0)
                {
                    //Wenn es ein für die DB unbekanntes Bild ist, mache einen neuen Eintrag dafür
                    SpeichereBild(neuesBild);
                    MessageBox.Show("Bild wurde neu gespeichert");
                }
                else
                {
                    MessageBox.Show("Bild wurde aktualisiert");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void LoescheBild(ObjectId personId)
        {
            var collection = connection.GetCollection<Bild>("bilder");

            try
            {
                //lösche das Bild mit der dazugehörigen personenId
                var filter = Builders<Bild>.Filter.Eq(bild => bild.PersonId, personId);
                var result = collection.DeleteOne(filter);

                //Falls ein Bild gelöscht wurde
                if (result.DeletedCount > 0)
                {
                    MessageBox.Show("Das Bild wurde gelöscht!");
                }
                else
                {
                    MessageBox.Show("Es konnte kein Bild gelöscht werden");
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
            }
        }

    }
}

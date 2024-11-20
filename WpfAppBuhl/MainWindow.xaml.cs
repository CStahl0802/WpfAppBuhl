using WpfAppBuhl.Klassen;
using WpfAppBuhl.DB;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MongoDB.Bson;

namespace WpfAppBuhl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FuellePersonenComboBox();

            AktualisierePersonenListe();

            ladeDaten.Click += ladeDaten_Click;

            telefonAuflistung.SelectionChanged += TelefonAuflistung_SelectionChanged;

            //Event Handler für Änderungen in der Telefon TextBox
            telefon.LostFocus += Telefon_LostFocus;
        }

        //Liste für alle angelegten Telefonnummern
        List<string> weitereTelefonnummern = new List<string>();

        private void Telefon_LostFocus(object sender, RoutedEventArgs e)
        {
            //Nur ausführen wenn eine Nummer in der ComboBox ausgewählt ist
            if (telefonAuflistung.SelectedIndex >= 0 && !string.IsNullOrWhiteSpace(telefon.Text))
            {
                int selectedIndex = telefonAuflistung.SelectedIndex;
                string newNumber = telefon.Text.Trim();
                string currentNumber = weitereTelefonnummern[selectedIndex];

                //Wenn die Nummer unverändert ist, die Methode frühzeitig beenden 
                if (newNumber == currentNumber)
                {
                    return;
                }

                //Prüfe, ob die neue Nummer bereits an einer anderen Position existiert
                int existingIndex = weitereTelefonnummern.IndexOf(newNumber);
                if (existingIndex != -1 && existingIndex != selectedIndex)
                {
                    MessageBox.Show("Diese Nummer existiert bereits an einer anderen Position.");
                    //Stelle die ursprüngliche Nummer wieder her
                    telefon.Text = currentNumber;
                    return;
                }

                //Aktualisiere die Nummer in der Liste und ComboBox
                weitereTelefonnummern[selectedIndex] = newNumber;

                //ComboBox aktualisieren und Auswahl beibehalten
                FuelleTelefonComboBox(weitereTelefonnummern);
                telefonAuflistung.SelectedIndex = selectedIndex;
            }
        }

        //Event-Handler für die Auswahl einer Telefonnummer
        private void TelefonAuflistung_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (telefonAuflistung.SelectedItem != null)
            {
                //Ausgewählte Nummer in die Telefon-TextBox übertragen
                telefon.Text = telefonAuflistung.SelectedItem.ToString();
            }
            
        }

        private void FuelleTelefonComboBox(List<string> telefonnummern)
        {
            //Alle einträge aus telefonauflistunglöschen
            telefonAuflistung.Items.Clear();

            //Combobox neu aufbauen
            foreach (var nummer in telefonnummern)
            {
                telefonAuflistung.Items.Add(nummer);
            }
        }

        private void FuellePersonenComboBox()
        {
            //Alle Personen aus der Datenbank laden
            var allePersonen = MongoDBManager.LadeAllePersonen();

            //ComboBox leeren und neu befüllen
            personen.Items.Clear();

            //Für jede Person einen ComboBox-Eintrag erstellen
            foreach (var person in allePersonen)
            {
                //Erstelle einen aussagekräftigen String für die Anzeige
                string displayText = $"{person.Name} {person.Nachname}";

                //Füge ein ComboBoxItem hinzu, das sowohl den Anzeigenamen als auch die kompletten Personendaten enthält
                personen.Items.Add(new ComboBoxItem
                {
                    Content = displayText,
                    Tag = person
                });
            }
        }

        //Aktion beim klick des Buttons
        private void submitButton_Click(object sender, RoutedEventArgs e)
        {

            byte[] bildBytes = null;

            //Wenn ein Bild in die bildAnzeige geladen wurde, encode das Bild
            if (bildAnzeige.Source is BitmapImage bitmapImage)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    bildBytes = ms.ToArray();
                }
            }

            //Klasse mit vorhandenen Daten instanzieren
            var personDaten = new PersonDaten
            (            
                name.Text,
                nachname.Text,
                strasse.Text,
                plz.Text,
                ort.Text,
                weitereTelefonnummern
            );

            //Überprüfe ob ein Name eigegeben wurde
            if (personDaten.Name.Length >= 1)
            {

                //Überprüfe, ob in der ComboBox eine Person ausgewählt wurde
                if (personen.SelectedItem != null)
                {
                    var selectedItem = (ComboBoxItem)personen.SelectedItem;
                    var existingPerson = (PersonDaten)selectedItem.Tag;

                    //Id der ausgewählten Person auf dem Objekt speichern
                    personDaten.Id = existingPerson.Id;

                    //Datensatz aktualisieren
                    MongoDBManager.AktualisierePerson(personDaten);

                    //Falls bild vorhand wird es aktualisiert oder gespeichert
                    if (bildBytes != null)
                    {
                        var bildDaten = new Bild(
                            bildBytes,
                            "image/jpeg",
                            personDaten.Id
                        );
                        MongoDBManager.AktualisiereBild(personDaten.Id, bildDaten);
                    }
                }
                else
                {
                    //Neuen Eintrag speichern
                    MongoDBManager.SpeicherePerson(personDaten, bildBytes);
                }

                //Liste aktualisieren
                AktualisierePersonenListe();

                //Aktualisiere die ComboBox mit dem neusten Eintrag
                FuellePersonenComboBox();

                //Text in TextBoxen löschen
                name.Text = null;
                nachname.Text = null;
                strasse.Text = null;
                plz.Text = null;
                ort.Text = null;
                telefon.Text = null;
                bildAnzeige.Source = null;

                //Telefon ComboBox leeren
                telefonAuflistung.Items.Clear();

                //Telefonnummern Liste leeren
                weitereTelefonnummern.Clear();

                //Aktualisiere die ComboBox mit dem neusten Eintrag
                FuellePersonenComboBox();
            }
            else
            {
                MessageBox.Show($"Bitte gebe deinen Namen ein!");
            }
        }

        private void telefonErweitern_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(telefon.Text))
            {
                string newNumber = telefon.Text.Trim();

                //Prüfe, ob die Nummer bereits in der Liste existiert
                if (!weitereTelefonnummern.Contains(newNumber))
                {
                    //Füge neue Nummer der Liste Hinzu
                    weitereTelefonnummern.Add(newNumber);

                    //ComboBox neu befüllen
                    FuelleTelefonComboBox(weitereTelefonnummern);

                    MessageBox.Show($"Deine Nummer wurde hinzugefügt: {newNumber}");

                    //Textfeld leeren für die nächste Eingabe
                    telefon.Text = string.Empty;

                    //Deselektiere alle Einträge in der ComboBox
                    telefonAuflistung.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Diese Nummer existiert bereits.");
                }
            }
            else
            {
                MessageBox.Show("Bitte geben Sie eine Telefonnummer ein.");
            }
        }

        private void ladeDaten_Click(object sender, RoutedEventArgs e)
        {
            //Prüfe ob ein Item ausgewählt wurde
            if (personen.SelectedItem != null)
            {
                //Daten aus der Combobox personen holen
                var selectedItem = (ComboBoxItem)personen.SelectedItem;
                var personDaten = (PersonDaten)selectedItem.Tag;

                //Befülle die TextBoxen mit den Daten
                name.Text = personDaten.Name;
                nachname.Text = personDaten.Nachname;
                strasse.Text = personDaten.Strasse;
                plz.Text = personDaten.Plz;
                ort.Text = personDaten.Ort;

                //Liste der Telefonnummern aktualisieren
                weitereTelefonnummern = personDaten.Telefon.ToList();

                //ComboBox mit Telefonnummern aktualisieren
                FuelleTelefonComboBox(weitereTelefonnummern);

                //Das Bild von der Person laden
                LadeBildVonPerson(personDaten.Id);

                //Erste Telefonnummer in die Telefon-TextBox laden, falls vorhanden
                telefon.Text = weitereTelefonnummern.FirstOrDefault() ?? string.Empty;
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Person aus!");
            }
        }

        private void loeschen_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)personen.SelectedItem;
            var existingPerson = (PersonDaten)selectedItem.Tag;

            //Id der ausgewählten Person auf dem Objekt speichern
            var personDatenId = existingPerson.Id;

            //Es muss eine person ausgewählt sein in der ComboBox personen
            if(personen.SelectedItem != null){
                MongoDBManager.LöschePerson(personDatenId);

                //Aktualisiere personenListe
                AktualisierePersonenListe();

                //Aktualisiere die ComboBox mit dem neusten Eintrag
                FuellePersonenComboBox();

                //Telefon ComboBox leeren
                telefonAuflistung.Items.Clear();

                //Telefonnummern Liste leeren
                weitereTelefonnummern.Clear();

                //Text in TextBoxen löschen
                name.Text = null;
                nachname.Text = null;
                strasse.Text = null;
                plz.Text = null;
                ort.Text = null;
                telefon.Text = null;
                bildAnzeige.Source = null;
            }
        }

        private void suchKriterien_TextChanged(object sender, TextChangedEventArgs e)
        {
            //sobald etwas in der suchKriterien Leiste eingegeben wird personenListe aktualisiert
            AktualisierePersonenListe();
        }

        private void AktualisierePersonenListe()
        {
            //StackPanel personenListe leeren
            personenListe.Children.Clear();

            //Alle Personen von der DB laden
            var allePersonen = MongoDBManager.LadeAllePersonen();

            //Suchkriterium auf die Vornamen anwenden
            var suchbegriff = suchKriterien.Text.ToLower();
            var gefiltertePersonen = allePersonen
                .Where(p => string.IsNullOrEmpty(suchbegriff) ||
                            p.Name.ToLower().Contains(suchbegriff))
                .OrderBy(p => p.Name);

            //Für jede gefilterte Person einen Eintrag erstellen
            foreach (var person in gefiltertePersonen)
            {
                var personPanel = new StackPanel
                {
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush(Colors.LightGray)
                };

                //Hauptinformationen Name Nachname
                personPanel.Children.Add(new TextBlock
                {
                    Text = $"Name: {person.Name} {person.Nachname}",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5)
                });

                //Adresse
                personPanel.Children.Add(new TextBlock
                {
                    Text = $"Adresse: {person.Strasse}, {person.Plz} {person.Ort}",
                    Margin = new Thickness(5)
                });

                //Telefonnummern Layout mit Grid
                var telefonGrid = new Grid();
                telefonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                telefonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var telefonLabel = new TextBlock
                {
                    Text = "Telefon: ",
                    Margin = new Thickness(5, 5, 0, 0),
                    VerticalAlignment = VerticalAlignment.Top
                };
                Grid.SetColumn(telefonLabel, 0);

                var telefonPanel = new WrapPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                //Array aus Telefonnummern durch iterieren und in Zeile schreiben, beim letzten Index das Komma nicht setzen
                int lastIndex = person.Telefon.Count - 1;
                for (int i = 0; i < person.Telefon.Count; i++)
                {
                    telefonPanel.Children.Add(new TextBlock
                    {
                        Text = i == lastIndex ? person.Telefon[i] : person.Telefon[i] + ", ",
                        Margin = new Thickness(2, 0, 2, 0)
                    });
                }

                //Scrollbar einfügen um horizontal durch die Telefonnummern scrollen zu können
                var telefonScrollViewer = new ScrollViewer
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Height = 50,
                    MaxWidth = 300,
                    Padding = new Thickness(0, 0, 0, 5)
                };

                telefonScrollViewer.Content = telefonPanel;
                Grid.SetColumn(telefonScrollViewer, 1);

                telefonGrid.Children.Add(telefonLabel);
                telefonGrid.Children.Add(telefonScrollViewer);

                personPanel.Children.Add(telefonGrid);

                //Trennlinie
                personPanel.Children.Add(new Separator
                {
                    Margin = new Thickness(5)
                });

                //Panel zur Liste hinzufügen
                personenListe.Children.Add(personPanel);
            }

            //Anzahl der gefundenen Einträge anzeigen
            var anzahlText = new TextBlock
            {
                Text = $"Gefundene Einträge: {gefiltertePersonen.Count()}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
            personenListe.Children.Insert(0, anzahlText);
        }

        private void bildEinfuegen_Click(object sender, RoutedEventArgs e)
        {

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Bilder|*.jpg;*.jpeg;*.png;*.bmp|Alle Dateien|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    //Bild laden und anzeigen
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    bildAnzeige.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Verarbeiten des Bildes: {ex.Message}");
                }
            }
        }

        //Klick auf das Bildfeld öffnet auch den FileDialog
        private void bildAnzeige_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bildEinfuegen_Click(sender, e);
        }

        private void LadeBildVonPerson(ObjectId personId)
        {
            var bildDaten = MongoDBManager.LadeBild(personId);
            if (bildDaten != null)
            {
                try
                {
                    //Bytes in BitmapImage umwandeln
                    var bitmapImage = new BitmapImage();
                    using (var ms = new MemoryStream(bildDaten.BildBytes))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                    }
                    bildAnzeige.Source = bitmapImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Laden des Bildes: {ex.Message}");
                }
            }
            else
            {
                //Wenn kein bild vorhanden ist
                bildAnzeige.Source = null;
            }
        }

        private void bildLoeschen_Click(object sender, RoutedEventArgs e)
        {

            //Wenn noch keine Person ausgewählt wurde, dann wurde das Bild auch noch nicht gespeichert und wird wieder auf null gesetzt
            if(personen.SelectedItem == null)
            {
                bildAnzeige.Source = null;
                return;
            }

            //Überprüfe ob ein Bild vorhanden ist
            if (personen.SelectedItem != null)
            {
                // Hole die Personendaten aus dem Tag des ausgewählten Items
                var selectedItem = (ComboBoxItem)personen.SelectedItem;
                var personDaten = (PersonDaten)selectedItem.Tag;

                MongoDBManager.LoescheBild(personDaten.Id);

                //wenn das Bild gelöscht wurde, leere die Bildanzeige
                bildAnzeige.Source = null;
            }
        }
    }
}
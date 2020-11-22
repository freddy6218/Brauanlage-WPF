using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Brauanlage_WPF
{
    internal class Recipes
    {
        #region Felder
        public string Name { get; set; }
        public string Datum { get; set; }
        public string Sorte { get; set; }
        public string Autor { get; set; }
        public string Ausschlagswuerze { get; set; }
        public string Sudhausausbeute { get; set; }
        public string Stammwuerze { get; set; }
        public string Bittere { get; set; }
        public string Farbe { get; set; }
        public string Alkohol { get; set; }
        public string Kurzbeschreibung { get; set; }
        public string Infusion_Hauptguss { get; set; }
        public string Malz1 { get; set; }
        public string Malz1_Menge { get; set; }
        public string Malz1_Einheit { get; set; }
        public string Malz2 { get; set; }
        public string Malz2_Menge { get; set; }
        public string Malz2_Einheit { get; set; }
        public string Malz3 { get; set; }
        public string Malz3_Menge { get; set; }
        public string Malz3_Einheit { get; set; }
        public string Malz4 { get; set; }
        public string Malz4_Menge { get; set; }
        public string Malz4_Einheit { get; set; }
        public string Malz5 { get; set; }
        public string Malz5_Menge { get; set; }
        public string Malz5_Einheit { get; set; }
        public string Malz6 { get; set; }
        public string Malz6_Menge { get; set; }
        public string Malz6_Einheit { get; set; }
        public string Malz7 { get; set; }
        public string Malz7_Menge { get; set; }
        public string Malz7_Einheit { get; set; }
        public string Malz8 { get; set; }
        public string Malz8_Menge { get; set; }
        public string Malz8_Einheit { get; set; }
        public string Infusion_Einmaischtemperatur { get; set; }
        public string Infusion_Rasttemperatur1 { get; set; }
        public string Infusion_Rastzeit1 { get; set; }
        public string Infusion_Rasttemperatur2 { get; set; }
        public string Infusion_Rastzeit2 { get; set; }
        public string Infusion_Rasttemperatur3 { get; set; }
        public string Infusion_Rastzeit3 { get; set; }
        public string Infusion_Rasttemperatur4 { get; set; }
        public string Infusion_Rastzeit4 { get; set; }
        public string Infusion_Rasttemperatur5 { get; set; }
        public string Infusion_Rastzeit5 { get; set; }
        public string Infusion_Rasttemperatur6 { get; set; }
        public string Infusion_Rastzeit6 { get; set; }
        public string Infusion_Rasttemperatur7 { get; set; }
        public string Infusion_Rastzeit7 { get; set; }
        public string Infusion_Rasttemperatur8 { get; set; }
        public string Infusion_Rastzeit8 { get; set; }
        public string Abmaischtemperatur { get; set; }
        public string Nachguss { get; set; }
        public string Kochzeit_Wuerze { get; set; }
        public string Hopfen_VWH_1_Sorte { get; set; }
        public string Hopfen_VWH_1_Menge { get; set; }
        public string Hopfen_VWH_1_alpha { get; set; }
        public string Hopfen_VWH_2_Sorte { get; set; }
        public string Hopfen_VWH_2_Menge { get; set; }
        public string Hopfen_VWH_2_alpha { get; set; }
        public string Hopfen_VWH_3_Sorte { get; set; }
        public string Hopfen_VWH_3_Menge { get; set; }
        public string Hopfen_VWH_3_alpha { get; set; }
        public string Hopfen_VWH_4_Sorte { get; set; }
        public string Hopfen_VWH_4_Menge { get; set; }
        public string Hopfen_VWH_4_alpha { get; set; }
        public string Hopfen_VWH_5_Sorte { get; set; }
        public string Hopfen_VWH_5_Menge { get; set; }
        public string Hopfen_VWH_5_alpha { get; set; }
        public string Hopfen_1_Sorte { get; set; }
        public string Hopfen_1_Menge { get; set; }
        public string Hopfen_1_alpha { get; set; }
        public string Hopfen_1_Kochzeit { get; set; }
        public string Hopfen_2_Sorte { get; set; }
        public string Hopfen_2_Menge { get; set; }
        public string Hopfen_2_alpha { get; set; }
        public string Hopfen_2_Kochzeit { get; set; }
        public string Hopfen_3_Sorte { get; set; }
        public string Hopfen_3_Menge { get; set; }
        public string Hopfen_3_alpha { get; set; }
        public string Hopfen_3_Kochzeit { get; set; }
        public string Hopfen_4_Sorte { get; set; }
        public string Hopfen_4_Menge { get; set; }
        public string Hopfen_4_alpha { get; set; }
        public string Hopfen_4_Kochzeit { get; set; }
        public string Hopfen_5_Sorte { get; set; }
        public string Hopfen_5_Menge { get; set; }
        public string Hopfen_5_alpha { get; set; }
        public string Hopfen_5_Kochzeit { get; set; }
        public string Hefe { get; set; }
        public string Gaertemperatur { get; set; }
        public string Endvergaerungsgrad { get; set; }
        public string Karbonisierung { get; set; }
        public string Anmerkung_Autor { get; set; }
        #endregion

        public static List<Recipes> RezepteListe = new List<Recipes>();

        public static int LeseRezepte(string targetDirectory)
        {
            try
            {
                if (!Directory.Exists(targetDirectory))
                    throw new System.Exception("Das Verzeichnis \"\\Rezepte\" existiert nicht!");

                //Dateien aus Verzeichnis auflisten
                string[] fileEntries = Directory.GetFiles(targetDirectory);

                RezepteListe.Clear();

                foreach (string fileName in fileEntries)
                {
                    //Lese Dateierweiterung
                    string FileInfos = new FileInfo(fileName).Extension;

                    //Liste nur *.json Dateien auf
                    if (FileInfos == ".json")
                    {
                        RezepteListe.Add(ProcessFile(fileName));
                    }
                }

                //Unterordner auslesen
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);

                foreach (string subdirectory in subdirectoryEntries)
                {
                    LeseRezepte(subdirectory);
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        //Rezepte ausgeben und in Klasse speichern
        private static Recipes ProcessFile(string path)
        {
            try
            {   //Datei mit dem Stream-Reader öffnen
                StreamReader sr = new StreamReader(path);

                //Datei in String schreiben
                String line = sr.ReadToEnd();
                Recipes rezept = JsonConvert.DeserializeObject<Recipes>(line);

                sr.Close();
                sr.Dispose();

                return rezept;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace Brauanlage_WPF
{
    internal class Recipes
    {
        public string Name { get; set; }
        public string Datum { get; set; }
        public string Sorte { get; set; }


        public static int LeseRezepte(string targetDirectory)
        {
            try
            {
                if (!Directory.Exists(targetDirectory))
                    throw new System.Exception("Das Verzeichnis \"\\Rezepte\" existiert nicht!");

                //Dateien aus Verzeichnis auflisten
                string[] fileEntries = Directory.GetFiles(targetDirectory);

                foreach (string fileName in fileEntries)
                {
                    //Lese Dateierweiterung
                    string FileInfos = new FileInfo(fileName).Extension;

                    //Liste nur *.json Dateien auf
                    if (FileInfos == ".json")
                    {
                        ProcessFile(fileName);
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
        private static void ProcessFile(string path)
        {
            Console.WriteLine("Gefundenes Rezept: '{0}'.", path);

            try
            {   //Datei mit dem Stream-Reader öffnen
                StreamReader sr = new StreamReader(path);

                //Datei in String schreiben
                String line = sr.ReadToEnd();
                Recipes rezept = JsonConvert.DeserializeObject<Recipes>(line);

                //Daten ausgeben
                Console.Write(rezept.Name);
                Console.Write(" - ");
                Console.WriteLine(rezept.Sorte);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
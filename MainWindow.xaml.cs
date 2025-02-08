using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Xml.Linq;

namespace WPF_ProdejAut
{
    public partial class MainWindow : Window
    {
        private string xmlPath = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Otevře dialog pro výběr XML souboru a načte jeho obsah.
        /// </summary>
        private void BtnNactiXML_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                xmlPath = openFileDialog.FileName;
                NactiXML();
            }
        }

        /// <summary>
        /// Načte XML soubor a zobrazí seznam všech prodaných aut.
        /// </summary>
        private void NactiXML()
        {
            try
            {
                if (string.IsNullOrEmpty(xmlPath))
                {
                    MessageBox.Show("Vyberte XML soubor.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                XDocument xmlDoc = XDocument.Load(xmlPath);

                var auta = xmlDoc.Descendants("Auto")
                    .Select(a => new Auto
                    {
                        Model = (string)a.Element("Model"),
                        DatumProdeje = DateTime.Parse((string)a.Element("DatumProdeje")),
                        Cena = double.Parse((string)a.Element("Cena")),
                        DPH = double.Parse((string)a.Element("DPH"))
                    }).ToList();

                // Naplnění tabulky seznamem všech prodaných aut
                dataGridAuta.ItemsSource = auta;

                // Výpočet součtu prodejů o víkendu
                VypocetProdejeOVikendu(auta);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při načítání XML souboru: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Sečte celkovou cenu vozů prodaných o víkendu a vypočítá cenu s DPH.
        /// </summary>
        private void VypocetProdejeOVikendu(List<Auto> auta)
        {
            var vikendoveProdeje = auta
                .Where(a => a.DatumProdeje.DayOfWeek == DayOfWeek.Saturday || a.DatumProdeje.DayOfWeek == DayOfWeek.Sunday)
                .GroupBy(a => a.Model)
                .SelectMany(grp => new List<VikendovyProdej>
                {
                    new VikendovyProdej { Model = grp.Key, CenaSDPH = null }, // První řádek = Název modelu
                    new VikendovyProdej { Model = grp.Sum(a => a.Cena).ToString("N2"), CenaSDPH = grp.Sum(a => a.Cena * (1 + a.DPH / 100)) } // Druhý řádek = Cena
                })
                .ToList();

            dataGridVikend.ItemsSource = vikendoveProdeje;
        }
    }

    /// <summary>
    /// Reprezentuje auto načtené z XML.
    /// </summary>
    public class Auto
    {
        public string Model { get; set; }
        public DateTime DatumProdeje { get; set; }
        public double Cena { get; set; }
        public double DPH { get; set; }
    }

    /// <summary>
    /// Reprezentuje souhrnné výsledky prodeje o víkendu.
    /// </summary>
    public class VikendovyProdej
    {
        public string Model { get; set; }
        public double? CenaSDPH { get; set; }
    }
}

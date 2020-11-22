using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Brauanlage_WPF.Pages
{
    /// <summary>
    /// Interaktionslogik für RecipePage.xaml
    /// </summary>
    public partial class RecipePage : Page
    {

        public RecipePage()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (Recipes.LeseRezepte("Rezepte") != 0)            
                MainWindow.ChangeStatus("Ein Fehler beim auslesen der Rezepte ist aufgetreten!", MainWindow.IconType.Error);
            else
                MainWindow.ChangeStatus("Die Rezepte wurden ausgelesen.", MainWindow.IconType.Information);


        }
    }
}

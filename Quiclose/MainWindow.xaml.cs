using System.Diagnostics;
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

namespace Quiclose {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow () {
            InitializeComponent();

            var ps = Process.GetProcessesByName("Notepad");
            string o = ps.Length != 0 ? ("Found: " + string.Join(", ", ps.Select(p => p.Id))) : "NONE FOUND!";

            Debug.WriteLine($"\n*****\n\n{o}\n\n*****\n");

            if (ps.Length == 1) ps[0].Kill();
        }

    }
}
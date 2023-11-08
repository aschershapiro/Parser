using Microsoft.Win32;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Parser;
namespace AviaFDR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        BinaryParser parser;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void bt_load_Click(object sender, RoutedEventArgs e)
        {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Open File";
                openFileDialog.Filter = "Data Files (*.dat, *.bin)|*.dat;*.bin";
                openFileDialog.Multiselect = false;

                bool? result = openFileDialog.ShowDialog(Application.Current.MainWindow);

                if (result == true)
                {
                    tb_path.Text = openFileDialog.FileName;
                }
                else
            tb_path.Text = string.Empty;
            }
        private void bt_conv_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_path.Text))
            {
                AcrylicMessageBox.Show(this,"File path is invalid!","Warning",MessageBoxButton.OK);
            }
            else
            {
                parser = new BinaryParser();
                parser.ParseFile(tb_path.Text);
                AcrylicMessageBox.Show(this, "Output .mat file is now placed in the same folder as input file!", "Done!", MessageBoxButton.OK);
            }
        }
    }


}


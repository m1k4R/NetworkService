using NetworkService.Classes;
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
using System.Windows.Shapes;

namespace NetworkService
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
            comboBoxType.ItemsSource = Vrste.VrsteMjeraca;
            comboBoxType.DisplayMemberPath = "Name";
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool validate()
        {
            bool result = true;

            if (textBoxName.Text.Trim().Equals(""))
            {
                result = false;
                labelNameGreska.Content = "Popunite polje!";
                textBoxName.BorderBrush = Brushes.Red;
                textBoxName.BorderThickness = new Thickness(2);
            }
            else
            {
                labelNameGreska.Content = "";
                textBoxName.BorderBrush = Brushes.Transparent;
            }

            if (comboBoxType.SelectedItem == null)
            {
                result = false;
                labelTypeGreska.Content = "Izaberite vrstu!";
                comboBoxType.BorderBrush = Brushes.Red;
                comboBoxType.BorderThickness = new Thickness(2);
            }
            else
            {
                labelTypeGreska.Content = "";
                comboBoxType.BorderBrush = Brushes.Transparent;
            }
            
            if (textBoxID.Text.Trim().Equals(""))
            {
                result = false;
                labelIdGreska.Content = "Popunite polje!";
                textBoxID.BorderBrush = Brushes.Red;
                textBoxID.BorderThickness = new Thickness(2);
            }
            else
            {
                try
                {
                    Int32.Parse(textBoxID.Text.Trim());

                    if (Int32.Parse(textBoxID.Text.Trim()) < 0)
                    {
                        result = false;
                        labelIdGreska.Content = "Id nije validan!";
                        textBoxID.BorderBrush = Brushes.Red;
                        textBoxID.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        

                        foreach (Mjerac m in MainWindow.Lista)
                        {
                            if (Int32.Parse(textBoxID.Text.Trim()) == m.Id)
                            {
                                result = false;
                                labelIdGreska.Content = "Id vec postoji u listi!";
                                textBoxID.BorderBrush = Brushes.Red;
                                textBoxID.BorderThickness = new Thickness(2);
                            }

                          /*  if (Int32.Parse(textBoxID.Text.Trim()) != m.Id)
                            {
                                labelIdGreska.Content = "";
                                textBoxID.BorderBrush = Brushes.Transparent;
                            }  */
                        }
                    }
                }
                catch (Exception)
                {
                    result = false;
                    labelIdGreska.Content = "Id nije validan!";
                    textBoxID.BorderBrush = Brushes.Red;
                    textBoxID.BorderThickness = new Thickness(2);
                }
            }

            return result;

        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (validate())
            {
                MainWindow.Lista.Add(new Mjerac(Int32.Parse(textBoxID.Text), textBoxName.Text, (Vrsta)comboBoxType.SelectedItem));
                //MainWindow.ListaCanvas.Add(new Mjerac(Int32.Parse(textBoxID.Text), textBoxName.Text, (Vrsta)comboBoxType.SelectedItem));

                MainWindow.ListaCanvas.Clear();

                foreach (Mjerac m in MainWindow.Lista)
                {
                    MainWindow.ListaCanvas.Add(m);
                }

                this.Close();
            }
        }

        private void comboBoxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Vrsta mjerac = Vrste.VrsteMjeraca[comboBoxType.SelectedIndex];

            Uri adresaSlike = new Uri(mjerac.Img);
            BitmapImage imageBitmap = new BitmapImage(adresaSlike);

            image.Source = imageBitmap;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}

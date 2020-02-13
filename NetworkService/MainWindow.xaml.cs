using NetworkService.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace NetworkService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int count = 3; // Inicijalna vrednost broja objekata u sistemu
                               // ######### ZAMENITI stvarnim brojem elemenata


        public static ObservableCollection<Mjerac> Lista { get; set; }
        public static ObservableCollection<Mjerac> ListaCanvas { get; set; }
        public static ObservableCollection<Mjerac> ListaPomocna { get; set; }

        List<List<string>> ListaLista = new List<List<string>>();

        private Dictionary<int, Mjerac> Canvasi { get; set; }

        private bool search = false;

        private Classes.Slika draggedItem = null;
        private Mjerac odabrani = null;
        private bool dragging = false;

        public MainWindow()
        {
            InitializeComponent();

            Lista = new ObservableCollection<Mjerac>();
            ListaCanvas = new ObservableCollection<Mjerac>();
            ListaPomocna = new ObservableCollection<Mjerac>();
            Canvasi = new Dictionary<int, Mjerac>();

            Mjerac m1 = new Mjerac(1, "Visemlazni vodomjer", Vrste.VrsteMjeraca[0]);
            Mjerac m2 = new Mjerac(2, "Woltman vodomjer", Vrste.VrsteMjeraca[1]);
            Mjerac m3 = new Mjerac(3, "Elektromehanicki dozator", Vrste.VrsteMjeraca[2]);

            Lista.Add(m1);
            Lista.Add(m2);
            Lista.Add(m3);

            foreach (Mjerac m in Lista)
            {
                ListaCanvas.Add(m);
            }


            DataContext = this;

            createListener(); //Povezivanje sa serverskom aplikacijom
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                    //Prijem poruke
                    NetworkStream stream = tcpClient.GetStream();
                    string incomming;
                    byte[] bytes = new byte[1024];
                    int i = stream.Read(bytes, 0, bytes.Length);
                    //Primljena poruka je sacuvana u incomming stringu
                    incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Objekat_1:272"

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji

                            int id = Int32.Parse((incomming.Split('_', ':'))[1]);
                            double valueMjeraca = double.Parse((incomming.Split('_', ':'))[2]);


                            try
                            {
                                Lista[id].Mvalue = valueMjeraca;


                                using (System.IO.StreamWriter file = new System.IO.StreamWriter("Log.txt", true))
                                {
                                    file.WriteLine(incomming + "   " + DateTime.Now);
                                }
                            }
                            catch (Exception)
                            { }

                            try
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    if (valueMjeraca < 670 || valueMjeraca > 735)
                                    {
                                        if (Canvasi.Count > 0)
                                        {
                                            int idCanvasa = 0;

                                            Mjerac m = Lista[id];



                                            foreach (KeyValuePair<int, Mjerac> pair in Canvasi)
                                            {
                                                if (pair.Value == m)
                                                {
                                                    idCanvasa = pair.Key;
                                                }
                                            }

                                                ((TextBlock)((Canvas)(canvas.Children[idCanvasa - 1])).Children[0]).Background = Brushes.Red;

                                        }



                                    }

                                    else
                                    {
                                        if (Canvasi.Count > 0)
                                        {
                                            int idCanvasa = 0;

                                            Mjerac m = Lista[id];

                                            try
                                            {
                                                foreach (KeyValuePair<int, Mjerac> pair in Canvasi)
                                                {
                                                    if (pair.Value == m)
                                                    {
                                                        idCanvasa = pair.Key;
                                                    }
                                                }

                                            ((TextBlock)((Canvas)(canvas.Children[idCanvasa - 1])).Children[0]).Background = Brushes.Transparent;

                                            }
                                            catch (Exception)
                                            { }
                                        }
                                    }
                                });

                            }
                            catch (Exception)
                            { }
                        }
                        }, null);
                }
            
            
            });
        

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            AddWindow dodaj = new AddWindow();
            dodaj.ShowDialog();
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Lista.Count > 0)
            {
                try
                {
                    Mjerac m = Lista.ElementAt(dataGridTable.SelectedIndex);
                    //Mjerac m = Lista[dataGridTable.SelectedIndex];
                    ListaCanvas.Remove(m);

                    if (Canvasi.Count > 0)
                    {
                        int idCanvasa = 0;

                        foreach (KeyValuePair<int, Mjerac> pair in Canvasi)
                        {
                            if (pair.Value == m)
                            {
                                idCanvasa = pair.Key;
                            }
                        }

                        ((Canvas)(canvas.Children[idCanvasa - 1])).Background = Brushes.Transparent;
                        ((TextBlock)((Canvas)(canvas.Children[idCanvasa - 1])).Children[0]).Background = Brushes.Transparent;
                        ((TextBlock)((Canvas)(canvas.Children[idCanvasa - 1])).Children[0]).Text = "";
                        ((Canvas)(canvas.Children[idCanvasa - 1])).Resources.Remove("taken");
                        ((Button)((Canvas)(canvas.Children[idCanvasa - 1])).Children[1]).Visibility = Visibility.Hidden;

                        try
                        {
                            Canvasi.Remove(idCanvasa);  // ili idCanvasa - 1
                        }
                        catch (Exception)
                        { }

                        #region Delete switch-case
                        /*   switch (idCanvasa)
                           {
                               case 1:
                                   canvas1.Background = Brushes.Transparent;
                                   ((TextBlock)canvas1.Children[0]).Text = "";
                                   ((TextBlock)canvas1.Children[0]).Background = Brushes.Transparent;
                                   canvas1.Resources.Remove("taken");

                                   ((Button)canvas1.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(1);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 2:
                                   canvas2.Background = Brushes.Transparent;
                                   ((TextBlock)canvas2.Children[0]).Text = "";
                                   ((TextBlock)canvas2.Children[0]).Background = Brushes.Transparent;
                                   canvas2.Resources.Remove("taken");

                                   ((Button)canvas2.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(2);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 3:
                                   canvas3.Background = Brushes.Transparent;
                                   ((TextBlock)canvas3.Children[0]).Text = "";
                                   ((TextBlock)canvas3.Children[0]).Background = Brushes.Transparent;
                                   canvas3.Resources.Remove("taken");

                                   ((Button)canvas3.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(3);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 4:
                                   canvas4.Background = Brushes.Transparent;
                                   ((TextBlock)canvas4.Children[0]).Text = "";
                                   ((TextBlock)canvas4.Children[0]).Background = Brushes.Transparent;
                                   canvas4.Resources.Remove("taken");

                                   ((Button)canvas4.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(4);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 5:
                                   canvas5.Background = Brushes.Transparent;
                                   ((TextBlock)canvas5.Children[0]).Text = "";
                                   ((TextBlock)canvas5.Children[0]).Background = Brushes.Transparent;
                                   canvas5.Resources.Remove("taken");

                                   ((Button)canvas5.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(5);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 6:
                                   canvas6.Background = Brushes.Transparent;
                                   ((TextBlock)canvas6.Children[0]).Text = "";
                                   ((TextBlock)canvas6.Children[0]).Background = Brushes.Transparent;
                                   canvas6.Resources.Remove("taken");

                                   ((Button)canvas6.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(6);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 7:
                                   canvas7.Background = Brushes.Transparent;
                                   ((TextBlock)canvas7.Children[0]).Text = "";
                                   ((TextBlock)canvas7.Children[0]).Background = Brushes.Transparent;
                                   canvas7.Resources.Remove("taken");

                                   ((Button)canvas7.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(7);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 8:
                                   canvas8.Background = Brushes.Transparent;
                                   ((TextBlock)canvas8.Children[0]).Text = "";
                                   ((TextBlock)canvas8.Children[0]).Background = Brushes.Transparent;
                                   canvas8.Resources.Remove("taken");

                                   ((Button)canvas8.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(8);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 9:
                                   canvas9.Background = Brushes.Transparent;
                                   ((TextBlock)canvas9.Children[0]).Text = "";
                                   ((TextBlock)canvas9.Children[0]).Background = Brushes.Transparent;
                                   canvas9.Resources.Remove("taken");

                                   ((Button)canvas9.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(9);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 10:
                                   canvas10.Background = Brushes.Transparent;
                                   ((TextBlock)canvas10.Children[0]).Text = "";
                                   ((TextBlock)canvas10.Children[0]).Background = Brushes.Transparent;
                                   canvas10.Resources.Remove("taken");

                                   ((Button)canvas10.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(10);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 11:
                                   canvas11.Background = Brushes.Transparent;
                                   ((TextBlock)canvas11.Children[0]).Text = "";
                                   ((TextBlock)canvas11.Children[0]).Background = Brushes.Transparent;
                                   canvas11.Resources.Remove("taken");

                                   ((Button)canvas11.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(11);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 12:
                                   canvas12.Background = Brushes.Transparent;
                                   ((TextBlock)canvas12.Children[0]).Text = "";
                                   ((TextBlock)canvas12.Children[0]).Background = Brushes.Transparent;
                                   canvas12.Resources.Remove("taken");

                                   ((Button)canvas12.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(12);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 13:
                                   canvas13.Background = Brushes.Transparent;
                                   ((TextBlock)canvas13.Children[0]).Text = "";
                                   ((TextBlock)canvas13.Children[0]).Background = Brushes.Transparent;
                                   canvas13.Resources.Remove("taken");

                                   ((Button)canvas13.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(13);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 14:
                                   canvas14.Background = Brushes.Transparent;
                                   ((TextBlock)canvas14.Children[0]).Text = "";
                                   ((TextBlock)canvas14.Children[0]).Background = Brushes.Transparent;
                                   canvas14.Resources.Remove("taken");

                                   ((Button)canvas14.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(14);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 15:
                                   canvas15.Background = Brushes.Transparent;
                                   ((TextBlock)canvas15.Children[0]).Text = "";
                                   ((TextBlock)canvas15.Children[0]).Background = Brushes.Transparent;
                                   canvas15.Resources.Remove("taken");

                                   ((Button)canvas15.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(15);
                                   }
                                   catch (Exception)
                                   { }

                                   break;
                               case 16:
                                   canvas16.Background = Brushes.Transparent;
                                   ((TextBlock)canvas16.Children[0]).Text = "";
                                   ((TextBlock)canvas16.Children[0]).Background = Brushes.Transparent;
                                   canvas16.Resources.Remove("taken");

                                   ((Button)canvas16.Children[1]).Visibility = Visibility.Hidden;

                                   try
                                   {
                                       Canvasi.Remove(16);
                                   }
                                   catch (Exception)
                                   { }

                                   break;

                           } */
                        #endregion

                    }
                }
                catch (Exception)
                { }

                if (dataGridTable.SelectedIndex != -1)
                {
                    Lista.RemoveAt(dataGridTable.SelectedIndex);

                   
                }
                
            }
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            // ObservableCollection<Mjerac> pom = new ObservableCollection<Mjerac>();

            search = true;

            foreach (Mjerac m in Lista)
            {
                ListaPomocna.Add(m);
            }

            Lista.Clear();

            foreach (Mjerac m in ListaPomocna)
            {
                if (rbtName.IsChecked == true)
                {
                    if (m.Name.ToUpper().Contains(textBoxSearch.Text.ToUpper()))
                    {
                        Lista.Add(m);
                    }
                }

                if (rbtType.IsChecked == true)
                {
                    if (m.Type.Name.ToUpper().Contains(textBoxSearch.Text.ToUpper()))
                    {
                        Lista.Add(m);
                    }
                }

               // dataGridTable.ItemsSource = pom;
                buttonDelete.IsEnabled = false;
                buttonAdd.IsEnabled = false;
                buttonSearch.IsEnabled = false;
            }
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            if (search == false)
            {
                var result = MessageBox.Show("       Are you sure?\n  You will lose all data!", "", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    Lista.Clear();
                    ListaCanvas.Clear();
                    buttonDelete.IsEnabled = true;
                    buttonAdd.IsEnabled = true;
                    buttonSearch.IsEnabled = true;

                    int idCanvasa = 0;

                    while (Canvasi.Count > 0)
                    {
                        
                            ((Canvas)(canvas.Children[idCanvasa])).Background = Brushes.Transparent;
                            ((TextBlock)((Canvas)(canvas.Children[idCanvasa])).Children[0]).Background = Brushes.Transparent;
                            ((TextBlock)((Canvas)(canvas.Children[idCanvasa])).Children[0]).Text = "";
                            ((Canvas)(canvas.Children[idCanvasa])).Resources.Remove("taken");
                            ((Button)((Canvas)(canvas.Children[idCanvasa])).Children[1]).Visibility = Visibility.Hidden;

                            try
                            {
                                Canvasi.Remove(idCanvasa);
                            }
                            catch (Exception)
                            { }

                            idCanvasa++;
                        
                    }
                }
                else
                {

                }
            }
            else
            {
                Lista.Clear();

                foreach (Mjerac m in ListaPomocna)
                {
                    Lista.Add(m);
                }

                ListaPomocna.Clear();

                //dataGridTable.ItemsSource = Lista;
                buttonDelete.IsEnabled = true;
                buttonAdd.IsEnabled = true;
                buttonSearch.IsEnabled = true;
                search = false;
            }

        }

        

        //drag and drop

        private void ListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggedItem = null;
            listBoxMjeraci.SelectedItem = null;
            dragging = false;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!dragging)
            {
                dragging = true;
                Mjerac o = (Mjerac)listBoxMjeraci.SelectedItem;
                odabrani = o;
                draggedItem = new Classes.Slika(o.Type.Img);
              //  draggedItem = new Slika(((ImageBrush)o.Background).ImageSource.ToString());

                DragDrop.DoDragDrop(this, draggedItem, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }  

        private void dragOver(object sender, DragEventArgs e)
        {
            base.OnDragOver(e);
            if (((Canvas)sender).Resources["taken"] != null)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private void drop(object sender, DragEventArgs e)
        {
            int id;

            base.OnDrop(e);
            if (draggedItem != null)
            {
                if (((Canvas)sender).Resources["taken"] == null)
                {
                    BitmapImage logo = new BitmapImage();
                    string idName = (((Canvas)sender).Name.Replace("canvas", "")).Trim();
                    id = Int32.Parse(idName);
                    //Int32.TryParse((((Canvas)sender).Name.Replace("canvas", "")).Trim(), out id);
                    Canvasi.Add(id, odabrani);
                    ListaCanvas.Remove(odabrani);
                    logo.BeginInit();
                    logo.UriSource = new Uri(draggedItem.imageUri);
                    logo.EndInit();
                    ((Canvas)sender).Background = new ImageBrush(logo);
                    ((TextBlock)((Canvas)sender).Children[0]).Foreground = Brushes.White;
                    ((TextBlock)((Canvas)sender).Children[0]).FontWeight = FontWeights.Bold;
                    ((TextBlock)((Canvas)sender).Children[0]).Text = odabrani.Name;
                    ((TextBlock)((Canvas)sender).Children[0]).Background = Brushes.Transparent;
                    ((Canvas)sender).Resources.Add("taken", true);

                    ((Button)((Canvas)sender).Children[1]).Visibility = Visibility.Visible;

                }
                listBoxMjeraci.SelectedItem = null;
                dragging = false;
            }
            e.Handled = true;
        }

        private void x1_Click(object sender, RoutedEventArgs e)
        {
            if (canvas1.Resources["taken"] != null)
            {
                canvas1.Background = Brushes.Transparent;
               // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas1.Children[0]).Text = "";
                ((TextBlock)canvas1.Children[0]).Background = Brushes.Transparent;
                canvas1.Resources.Remove("taken");

                ((Button)canvas1.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[1]);
                    Canvasi.Remove(1);
                }
                catch (Exception)
                { }

            }
        }

        private void x2_Click(object sender, RoutedEventArgs e)
        {
            if (canvas2.Resources["taken"] != null)
            {
                canvas2.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas2.Children[0]).Text = "";
                ((TextBlock)canvas2.Children[0]).Background = Brushes.Transparent;
                canvas2.Resources.Remove("taken");

                ((Button)canvas2.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[2]);
                    Canvasi.Remove(2);
                }
                catch (Exception)
                { }
            }
        }

        private void x3_Click(object sender, RoutedEventArgs e)
        {
            if (canvas3.Resources["taken"] != null)
            {
                canvas3.Background = Brushes.Transparent;
                // ((TextBlock)canvas3.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas3.Children[0]).Text = "";
                ((TextBlock)canvas3.Children[0]).Background = Brushes.Transparent;
                canvas3.Resources.Remove("taken");

                ((Button)canvas3.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[3]);
                    Canvasi.Remove(3);
                }
                catch (Exception)
                { }
            }
        }

        private void x4_Click(object sender, RoutedEventArgs e)
        {
            if (canvas4.Resources["taken"] != null)
            {
                canvas4.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas4.Children[0]).Text = "";
                ((TextBlock)canvas4.Children[0]).Background = Brushes.Transparent;
                canvas4.Resources.Remove("taken");

                ((Button)canvas4.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[4]);
                    Canvasi.Remove(4);
                }
                catch (Exception)
                { }
            }
        }

        private void x5_Click(object sender, RoutedEventArgs e)
        {
            if (canvas5.Resources["taken"] != null)
            {
                canvas5.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas5.Children[0]).Text = "";
                ((TextBlock)canvas5.Children[0]).Background = Brushes.Transparent;
                canvas5.Resources.Remove("taken");

                ((Button)canvas5.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[5]);
                    Canvasi.Remove(5);
                }
                catch (Exception)
                { }
            }
        }

        private void x6_Click(object sender, RoutedEventArgs e)
        {
            if (canvas6.Resources["taken"] != null)
            {
                canvas6.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas6.Children[0]).Text = "";
                ((TextBlock)canvas6.Children[0]).Background = Brushes.Transparent;
                canvas6.Resources.Remove("taken");

                ((Button)canvas6.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[6]);
                    Canvasi.Remove(6);
                }
                catch (Exception)
                { }
            }
        }

        private void x7_Click(object sender, RoutedEventArgs e)
        {
            if (canvas7.Resources["taken"] != null)
            {
                canvas7.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas7.Children[0]).Text = "";
                ((TextBlock)canvas7.Children[0]).Background = Brushes.Transparent;
                canvas7.Resources.Remove("taken");

                ((Button)canvas7.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[7]);
                    Canvasi.Remove(7);
                }
                catch (Exception)
                { }
            }
        }

        private void x8_Click(object sender, RoutedEventArgs e)
        {
            if (canvas8.Resources["taken"] != null)
            {
                canvas8.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas8.Children[0]).Text = "";
                ((TextBlock)canvas8.Children[0]).Background = Brushes.Transparent;
                canvas8.Resources.Remove("taken");

                ((Button)canvas8.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[8]);
                    Canvasi.Remove(8);
                }
                catch (Exception)
                { }
            }
        }

        private void x9_Click(object sender, RoutedEventArgs e)
        {
            if (canvas9.Resources["taken"] != null)
            {
                canvas9.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas9.Children[0]).Text = "";
                ((TextBlock)canvas9.Children[0]).Background = Brushes.Transparent;
                canvas9.Resources.Remove("taken");

                ((Button)canvas9.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[9]);
                    Canvasi.Remove(9);
                }
                catch (Exception)
                { }
            }
        }

        private void x10_Click(object sender, RoutedEventArgs e)
        {
            if (canvas10.Resources["taken"] != null)
            {
                canvas10.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas10.Children[0]).Text = "";
                ((TextBlock)canvas10.Children[0]).Background = Brushes.Transparent;
                canvas10.Resources.Remove("taken");

                ((Button)canvas10.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[10]);
                    Canvasi.Remove(10);
                }
                catch (Exception)
                { }
            }
        }

        private void x11_Click(object sender, RoutedEventArgs e)
        {
            if (canvas11.Resources["taken"] != null)
            {
                canvas11.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas11.Children[0]).Text = "";
                ((TextBlock)canvas11.Children[0]).Background = Brushes.Transparent;
                canvas11.Resources.Remove("taken");

                ((Button)canvas11.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[11]);
                    Canvasi.Remove(11);
                }
                catch (Exception)
                { }
            }
        }

        private void x12_Click(object sender, RoutedEventArgs e)
        {
            if (canvas12.Resources["taken"] != null)
            {
                canvas12.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas12.Children[0]).Text = "";
                ((TextBlock)canvas12.Children[0]).Background = Brushes.Transparent;
                canvas12.Resources.Remove("taken");

                ((Button)canvas12.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[12]);
                    Canvasi.Remove(12);
                }
                catch (Exception)
                { }
            }
        }

        private void x13_Click(object sender, RoutedEventArgs e)
        {
            if (canvas13.Resources["taken"] != null)
            {
                canvas13.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas13.Children[0]).Text = "";
                ((TextBlock)canvas13.Children[0]).Background = Brushes.Transparent;
                canvas13.Resources.Remove("taken");

                ((Button)canvas13.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[13]);
                    Canvasi.Remove(13);
                }
                catch (Exception)
                { }
            }
        }

        private void x14_Click(object sender, RoutedEventArgs e)
        {
            if (canvas14.Resources["taken"] != null)
            {
                canvas14.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas14.Children[0]).Text = "";
                ((TextBlock)canvas14.Children[0]).Background = Brushes.Transparent;
                canvas14.Resources.Remove("taken");

                ((Button)canvas14.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[14]);
                    Canvasi.Remove(14);
                }
                catch (Exception)
                { }
            }
        }

        private void x15_Click(object sender, RoutedEventArgs e)
        {
            if (canvas15.Resources["taken"] != null)
            {
                canvas15.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas15.Children[0]).Text = "";
                ((TextBlock)canvas15.Children[0]).Background = Brushes.Transparent;
                canvas15.Resources.Remove("taken");

                ((Button)canvas15.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[15]);
                    Canvasi.Remove(15);
                }
                catch (Exception)
                { }
            }
        }

        private void x16_Click(object sender, RoutedEventArgs e)
        {
            if (canvas16.Resources["taken"] != null)
            {
                canvas16.Background = Brushes.Transparent;
                // ((TextBlock)canvas1.Children[0]).Foreground = Brushes.Black;
                ((TextBlock)canvas16.Children[0]).Text = "";
                ((TextBlock)canvas16.Children[0]).Background = Brushes.Transparent;
                canvas16.Resources.Remove("taken");

                ((Button)canvas16.Children[1]).Visibility = Visibility.Hidden;

                try
                {
                    ListaCanvas.Add(Canvasi[16]);
                    Canvasi.Remove(16);
                }
                catch (Exception)
                { }
            }
        }

        private void buttonReport_Click(object sender, RoutedEventArgs e)
        {
            //string text = System.IO.File.ReadAllText(@"d:\Milijana\Desktop\PredmetniZadatak3\NetworkService\NetworkService\bin\Debug\Log.txt");
            //textBoxReport.Text = text;

            textBoxReport.Text = "";

            System.IO.StreamReader text = new System.IO.StreamReader(@"D:\PR\hci\PredmetniZadatak3\NetworkService\NetworkService\bin\Debug\Log.txt");
            string line = text.ReadLine();

            int id = 1;

            List<List<string>> ListaRp = new List<List<string>>();

            for (int i = 0; i < MainWindow.Lista.Count; i++)
            {
                ListaRp.Add(new List<string>());
            }
   
            foreach (List<string> malaLista in ListaRp)
            {
                malaLista.Add("Objekat_" + id + ":\n");
                id++;
            }  
            

            try
            {
                while ((line = text.ReadLine()) != null)
                {
                    id = Int32.Parse((line.Split('_', ':'))[1]);
                    //string ostatak = line.Split('_', ':')[2];
                    string vrijednost = line.Substring(10, 3);
                    string datum = line.Substring(16, 9);
                    string vrijeme = line.Substring(26, 7);

                    foreach (List<string> malaLista in ListaRp)
                    {
                        if (id == ListaRp.IndexOf(malaLista))
                        {
                            malaLista.Add("          " + vrijednost + "      " + datum + "    " + vrijeme + "\n");
                        }
                    }
                }

                foreach (List<string> malaLista in ListaRp)
                {
                    ListaLista.Add(malaLista);
                }
            }
            catch (Exception)
            { } 

           

            foreach (List<string> malaLista in ListaRp)
            {
                foreach (string s in malaLista)
                {
                    textBoxReport.Text += s;
                }
            }

         /*   foreach (List<string> malaLista in ListaRp)
            {
                malaLista.Clear();
            }  */



        }

        private void buttonShow_Click(object sender, RoutedEventArgs e)
        {
            labelI.Visibility = Visibility.Visible;
            labelN.Visibility = Visibility.Visible;
            labelT.Visibility = Visibility.Visible;
            labelV.Visibility = Visibility.Visible;


            int index;
            textBoxValues.Text = "";

            //Mjerac izabrani = (Mjerac)comboBoxObject.SelectedItem;

            foreach (Mjerac m in Lista)
            {
                if (comboBoxObject.SelectedItem == m)
                {
                    index = m.Id;

                    labelId.Content = m.Id;
                    labelName.Content = m.Name;
                    labelType.Content = m.Type.Name;

                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.UriSource = new Uri(m.Type.Img);
                    logo.EndInit();
                    imageObj.Source = logo;

                    int rBr = comboBoxObject.SelectedIndex;

                    if (rBr <= ListaLista.Count)
                    {

                        foreach (List<string> malaLista in ListaLista)
                        {
                            if (ListaLista.IndexOf(malaLista) == comboBoxObject.SelectedIndex)
                            {
                                if (malaLista.Count > 10)
                                {
                                    foreach (string s in malaLista)
                                    {
                                        textBoxValues.Text += s.Replace("Objekat_" + index + ":", "");
                                    }
                                }
                            }

                            
                        }
                    }
                }
            } 


            // CharData

            CanvasBar.Children.Clear();

            List<double> plotValues = new List<double>();
            string text = System.IO.File.ReadAllText(@"D:\PR\hci\PredmetniZadatak3\NetworkService\NetworkService\bin\Debug\Log.txt");

            int idx = 0;
            char[] arr = new char[3];       // ovo nam sluzi za vrednost koju treba da napravimo
            int objId = comboBoxObject.SelectedIndex; // ovde uzimamo index elementa koji si odabrala da plotujemo

            try
            {
                int korak = 0;      // korak za pozicioniranje na vrednost

                foreach (char c in text)
                {
                    if (c == '_')       //kada naidjemo na donju crtu znamo da je sl index od objekta
                    {
                        korak++;
                    }

                    if ((korak == 2) && (c.ToString() == objId.ToString()))  //ako je to taj id povecavamo korak jer znamo da sl vrednost
                    {
                        korak++;
                    }
                    else if (korak == 2)            // ako nije to taj id onda anuliramo korak i cekamo sl donju crtu
                    {
                        korak = 0;
                    }

                    if (korak == 4) //ako je korak stigao do 4 to znaci da je sada vrednost
                    {
                        if (c != ' ')   // ako nije razmak znaci da je jos uvek broj koji nam treba
                        {
                            arr[idx++] = c;     // dodajemo char po char od broja u niz
                        }
                        else            //dosli smo do razmaka znaci da je broj ucitan u niz
                        {
                            string str = new string(arr);   // pravimo string od niza
                            double value = double.Parse(str);  // pravimo broj od stringa
                            plotValues.Add(value);          // dodajemo vrednost u listu
                            arr[0] = '\0';
                            arr[1] = '\0';                  // resetujemo niz
                            arr[2] = '\0';
                            korak = 0;                      // resetujemo korak
                            idx = 0;                        // resetujemo index od niza
                        }
                    }

                    if (korak == 1)                         // ako je prepoznata donja crta ovo sluzi za zastitu u slucaju da nismo naisli
                    {                                           // na indeks objekta koji nam treba
                        korak++;
                    }

                    if ((korak == 3) && (c == ':'))             // ako smo naisli na : onda ide vrednost pa povecamo korak
                    {
                        korak++;
                    }
                }

                const double margin = 10;       //margina udaljenosti od ivice kanvasa
                double xmin = margin;
                double xmax = CanvasBar.Width - margin;     // maximalna vrednost na x osi
                double ymax = CanvasBar.Height - margin;       // maximalna vrednost na y osi
                const double stepX = 37.7;                  // korak za podeok na x osi to racunamo kao maximalna vrednost na x osi podeljeno na broj barova tj kod tebe 377/10
                const double stepY = 63.4;                  // korak za podeok na y osi to racunamo kao visina podeljeno na br vrednosti koje primas 
                                                            // posto ti primas od 580 do 820 ja sam zaokruzio to na 500 i 900 i oduzeo 500 od 900 dobio 4
                                                            // podelio 400 sa 100 i dobio 4 pa dodao 1 za lepse crtanje pa maximalna vrednost na y osi sa 5

                int brPod = plotValues.Count;

                if (brPod > 9)     // ovde ogranicavamo prikaz na poslednjih 10
                {
                    while (plotValues.Count > 9)
                    {
                        plotValues.RemoveAt(0);
                    }
                }

                GeometryGroup xaxis_geom = new GeometryGroup();
                xaxis_geom.Children.Add(new LineGeometry(new Point(0, ymax), new Point(CanvasBar.Width, ymax))); // ovo pravi liniju x ose

                for (double x = xmin + stepX; x <= CanvasBar.Width - stepX; x += stepX) // ovo pravi podeoke na x osi, tu koristimo onaj stepX
                {
                    xaxis_geom.Children.Add(new LineGeometry(new Point(x, ymax - margin / 2), new Point(x, ymax + margin / 2)));
                }

                System.Windows.Shapes.Path xaxis_path = new System.Windows.Shapes.Path();
                xaxis_path.StrokeThickness = 1;
                xaxis_path.Stroke = Brushes.Black;
                xaxis_path.Data = xaxis_geom;

                CanvasBar.Children.Add(xaxis_path);  // ovo iscrta x osu

                GeometryGroup yaxis_geom = new GeometryGroup();
                yaxis_geom.Children.Add(new LineGeometry(new Point(xmin, 0), new Point(xmin, CanvasBar.Height))); // ovo pravi liniju od y ose

                for (double y = stepY; y <= CanvasBar.Height - stepY; y += stepY) // ovo pravi podeoke na y osi, tu koristimo ona stepY
                {
                    yaxis_geom.Children.Add(new LineGeometry(new Point(xmin - margin / 2, y), new Point(xmin + margin / 2, y)));
                }

                System.Windows.Shapes.Path yaxis_path = new System.Windows.Shapes.Path();
                yaxis_path.StrokeThickness = 1;
                yaxis_path.Stroke = Brushes.Black;
                yaxis_path.Data = yaxis_geom;

                CanvasBar.Children.Add(yaxis_path);  // ovde se crta y osa sa podeocima

                int br = 0; // indeks za kretanje po 
                double y_new; // promenljiva u kojoj cemo cuvati vrednosti

                for (double x = xmin; x <= ((plotValues.Count) * stepX); x += stepX)    // ovo pravi barove po tvojim vrednostima
                {
                    y_new = plotValues[br] / 100;

                    y_new = y_new - 5; // namestamo da nam krece vrednost od 500
                    if (br != plotValues.Count) // proverava da li ima jos elementa u listi da ne bi pukao program
                    {
                        br++;
                    }

                    System.Windows.Shapes.Rectangle rect;

                    var converter = new System.Windows.Media.BrushConverter();  // pravim boju barova
                    var brush = (Brush)converter.ConvertFromString("#FF264864");

                    rect = new System.Windows.Shapes.Rectangle();   // pravi pravougaonik
                    rect.Stroke = new SolidColorBrush(Colors.White);    // boja spoljasnje linije pravougaonika
                    rect.Fill = brush;       // boja unutrasnjosti pravougaonika
                    rect.Width = stepX;             // sirina pravougaonika po fiksnoj vrednosti
                    rect.Height = y_new * stepY;    // visina pravougaonik po vrednosti koju si dobila
                    Canvas.SetLeft(rect, x - 2);
                    Canvas.SetBottom(rect, margin); //smestanje pravougaonika na kanvas

                    CanvasBar.Children.Add(rect);
                }
                br = 0;
            }
            catch (Exception) { }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                AddWindow dodaj = new AddWindow();
                dodaj.ShowDialog();
            }
        }

        
    }
}

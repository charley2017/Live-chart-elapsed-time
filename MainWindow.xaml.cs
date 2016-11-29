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
using LiveCharts;
using LiveCharts.Wpf;
using System.IO.Ports;
using System.Diagnostics;
using LiveCharts.Configurations;
using System.Windows.Threading;
using System.ComponentModel;
using LiveCharts.Wpf.Charts.Base;

namespace graph_test_6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        private double _axisMax;
        private double _axisMin;
        Stopwatch stopWatch = new Stopwatch();



        public MainWindow()
        {
            InitializeComponent();
            //To handle live data easily, in this case we built a specialized type
            //the MeasureModel class, it only contains 2 properties
            //DateTime and Value
            //We need to configure LiveCharts to handle MeasureModel class
            //The next code configures MEasureModel  globally, this means
            //that livecharts learns to plot MeasureModel and will use this config every time
            //a ChartValues instance uses this type.
            //this code ideally should only run once, when application starts is reccomended.
            //you can configure series in many ways, learn more at http://lvcharts.net/App/examples/v1/wpf/Types%20and%20Configuration


            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Elapsed.Seconds)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            var mapper2 = Mappers.Xy<Graph1SecondVal>()
                .X(model => model.DateTime.Elapsed.Seconds)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);
            Charting.For<Graph1SecondVal>(mapper2);


            //the values property will store our values array


            ChartValues = new ChartValues<MeasureModel>();
            ChartValuesTwo = new ChartValues<Graph1SecondVal>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => stopWatch.Elapsed.Seconds.ToString();

            AxisStep = TimeSpan.FromSeconds(10).Ticks;
            SetAxisLimits(stopWatch);

            //The next code simulates data changes every 300 ms
            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            Timer.Tick += TimerOnTick;
            IsDataInjectionRunning = false;
            R = new Random();
            DataContext = this;
        }
        public ChartValues<Graph1SecondVal> ChartValuesTwo { get; set; }

        public ChartValues<MeasureModel> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }

        public double AxisStep { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        public DispatcherTimer Timer { get; set; }
        public bool IsDataInjectionRunning { get; set; }
        public Random R { get; set; }

        private void RunDataOnClick(object sender, RoutedEventArgs e)
        {
            if (IsDataInjectionRunning)
            {
                Timer.Stop();
               // stopWatch.Stop();
                IsDataInjectionRunning = false;
            }
            else
            {

                Timer.Start();
                stopWatch.Start();
                IsDataInjectionRunning = true;
            }
        }

        private void TimerOnTick(object sender, EventArgs eventArgs) // Class is referencing from here!
        {
            var now = DateTime.Now;
            

            ChartValuesTwo.Add(new Graph1SecondVal
            {
                DateTime = stopWatch,
                Value = R.Next(0, 10)
            });
        

            ChartValues.Add(new MeasureModel
            {
                DateTime = stopWatch,
                Value = R.Next(0, 20)
            });

            SetAxisLimits(stopWatch);

            //lets only use the last 30 values
            if (ChartValues.Count > 30) ChartValues.RemoveAt(0);
        }

        private void SetAxisLimits(Stopwatch now)
        {
            AxisMax = now.Elapsed.Seconds + 300000; // lets force the axis to be 100ms ahead
            AxisMin = now.Elapsed.Seconds - 300000; //we only care about the last 8 seconds
            textBox1.Text = AxisMax.ToString() + "," + now.Elapsed.Seconds.ToString();

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) // if subrscribed to event
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

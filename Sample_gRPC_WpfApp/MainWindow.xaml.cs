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
using Grpc.Net.Client;
using Sample_gRPC_ClassLibrary;

namespace Sample_gRPC_WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitTimeZone();

            InitializeComponent();

            this.DataContext = this;

            this.grpcClient = new SampleClient();
        }

        private SampleClient grpcClient = null;

        private void ReqestButton_Click(object sender, RoutedEventArgs e)
        {
            string requestText = this.ReqestTextBox.Text;

            var reply = this.grpcClient.GreeterClient.SayHello(new HelloRequest { Name = requestText });

            this.ReplayTextBox.Text = reply.Message;
        }

        private void MyFunctionButton_Click(object sender, RoutedEventArgs e)
        {
            string requestText = this.MyFunctionTextBox.Text;
            string requestInt = this.MyFunctionIntBox.Text;
            Int32 intValue;
            Int32.TryParse(requestInt, out intValue);

            MyRequest myRequest = new MyRequest();
            myRequest.Parameter1 = requestText;
            myRequest.ParameterIntValue = intValue;

            var reply = this.grpcClient.GreeterClient.MyFunction(myRequest);

            this.ReplayMyFunctionTextBox.Text = reply.Message;
        }

        private void CalcButton_Click(object sender, RoutedEventArgs e)
        {
            string value1 = this.Int1TextBox.Text;
            string value2 = this.Int2TextBox.Text;
            Int32 intValue1;
            Int32 intValue2;

            Int32.TryParse(value1, out intValue1);
            Int32.TryParse(value2, out intValue2);

            var reply = this.grpcClient.GreeterClient.Calc(
                new CalcParameter { Parameter1 = intValue1, Parameter2 = intValue2 }
                );

            this.AdditionTextBox.Text = reply.Addition.ToString();
            this.SubtractionTextBox.Text = reply.Subtraction.ToString();
            this.MultiplicationTextBox.Text = reply.Multiplication.ToString();
            this.DivisionTextBox.Text = reply.Division.ToString();

        }

        private Dictionary<string, TimeSpan> _timeZone = null;
        public Dictionary<string, TimeSpan> TimeZone { get { return this._timeZone; } }

        private void InitTimeZone()
        {
            TimeSpan JapanTimeSpan = new TimeSpan(9, 0, 0);
            TimeSpan BritainTimeSpan = new TimeSpan(0, 0, 0);
            TimeSpan AmericaEasternTimeSpan = new TimeSpan(-4, 0, 0);

            this._timeZone = new Dictionary<string, TimeSpan>();

            this._timeZone.Add("Japan", JapanTimeSpan);
            this._timeZone.Add("Britain", BritainTimeSpan);
            this._timeZone.Add("AmericaEastern", AmericaEasternTimeSpan);
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.xDatePicker.SelectedDate == null)
                return;

            if (this.xTimeSpanComboBox.SelectedValue == null)
                return;

            int hour, minute, span;
            if(int.TryParse(this.xHourTextBox.Text, out hour) == false) return;
            if (int.TryParse(this.xMinuteTextBox.Text, out minute) == false) return;
            if (int.TryParse(this.xSpanTextBox.Text, out span) == false) return;

            DateTime dateTime = this.xDatePicker.SelectedDate.Value;
            string timeZoneText = ((KeyValuePair<string, TimeSpan>)this.xTimeSpanComboBox.SelectedValue).Key;
            TimeSpan timeZone = ((KeyValuePair<string, TimeSpan>)this.xTimeSpanComboBox.SelectedValue).Value;
            TimeSpan duration = new TimeSpan(0, span, 0);

            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0, 0, timeZone);


            //ReservationTime 作成
            ReservationTime reservationTime = new ReservationTime();
            reservationTime.Subject = "診察予約";
            reservationTime.Time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(dateTimeOffset);
            reservationTime.Duration = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(duration);
            reservationTime.TimeZone = timeZoneText;

            var reply = this.grpcClient.GreeterClient.Reserve(reservationTime);

            this.xReserveTextBox.Text = reply.Time.ToDateTime().ToString() + " / " + reply.TimeZone.ToString();

        }
    }
}


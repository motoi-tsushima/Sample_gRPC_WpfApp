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
using Google.Protobuf.WellKnownTypes;
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
            TimeSpan TokyoTimeSpan = new TimeSpan(9, 0, 0);
            TimeSpan LondonTimeSpan = new TimeSpan(0, 0, 0);
            TimeSpan New_YorkTimeSpan = new TimeSpan(-5, 0, 0);
            TimeSpan Los_AngelesTimeSpan = new TimeSpan(-8, 0, 0);
            TimeSpan AnchorageTimeSpan = new TimeSpan(-9, 0, 0);
            TimeSpan SingaporeTimeSpan = new TimeSpan(8, 0, 0);
            TimeSpan TaipeiTimeSpan = new TimeSpan(8, 0, 0);
            TimeSpan ShanghaiTimeSpan = new TimeSpan(8, 0, 0);
            TimeSpan SydneyTimeSpan = new TimeSpan(10, 0, 0);
            TimeSpan YangonTimeSpan = new TimeSpan(6, 30, 0);
            TimeSpan MoscowTimeSpan = new TimeSpan(4, 0, 0);
            TimeSpan ParisTimeSpan = new TimeSpan(1, 0, 0);
            TimeSpan BerlinTimeSpan = new TimeSpan(1, 0, 0);
            TimeSpan JerusalemTimeSpan = new TimeSpan(2, 0, 0);

            this._timeZone = new Dictionary<string, TimeSpan>();

            this._timeZone.Add("Tokyo", TokyoTimeSpan);
            this._timeZone.Add("London", LondonTimeSpan);
            this._timeZone.Add("New_York", New_YorkTimeSpan);
            this._timeZone.Add("Los_Angeles", Los_AngelesTimeSpan);
            this._timeZone.Add("Anchorage", AnchorageTimeSpan);
            this._timeZone.Add("Singapore", SingaporeTimeSpan);
            this._timeZone.Add("Taipei", TaipeiTimeSpan);
            this._timeZone.Add("Shanghai", ShanghaiTimeSpan);
            this._timeZone.Add("Sydney", SydneyTimeSpan);
            this._timeZone.Add("Yangon", YangonTimeSpan);
            this._timeZone.Add("Moscow", MoscowTimeSpan);
            this._timeZone.Add("Paris", ParisTimeSpan);
            this._timeZone.Add("Berlin", BerlinTimeSpan);
            this._timeZone.Add("Jerusalem", JerusalemTimeSpan);
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            //選択していない場合は無視する。
            if (this.xDatePicker.SelectedDate == null)
                return;

            if (this.xTimeSpanComboBox.SelectedValue == null)
                return;

            //時間を文字列から数値に変換する。
            int hour, minute, span;
            if(int.TryParse(this.xHourTextBox.Text, out hour) == false) return;
            if (hour > 24) return;
            if (int.TryParse(this.xMinuteTextBox.Text, out minute) == false) return;
            if (minute > 60) return;
            if (int.TryParse(this.xSpanTextBox.Text, out span) == false) return;

            //クライアント側のタイムゾーンを設定
            DateTime dateTime = this.xDatePicker.SelectedDate.Value;
            string timeZoneText = ((KeyValuePair<string, TimeSpan>)this.xTimeSpanComboBox.SelectedValue).Key;
            TimeSpan timeZone = ((KeyValuePair<string, TimeSpan>)this.xTimeSpanComboBox.SelectedValue).Value;

            //予約先施設側のタイムゾーンを設定s
            string countryTimeZoneText = ((KeyValuePair<string, TimeSpan>)this.xCountryTimeSpanComboBox.SelectedValue).Key;
            TimeSpan countryTimeZone = ((KeyValuePair<string, TimeSpan>)this.xCountryTimeSpanComboBox.SelectedValue).Value;

            //予約時間
            TimeSpan duration = new TimeSpan(0, span, 0);

            //クライアント側のタイムゾーンを反映した日付型を作成する。
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, 0, 0, timeZone);

            //ReservationTime 作成
            ReservationTime reservationTime = new ReservationTime();
            reservationTime.Subject = "診察予約";
            reservationTime.Time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(dateTimeOffset);
            reservationTime.Duration = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(duration);
            reservationTime.TimeZone = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(timeZone);
            reservationTime.CountryTimeZone = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(countryTimeZone);

            // gRPC サービスを呼び出す。
            var reply = this.grpcClient.GreeterClient.Reserve(reservationTime);

            // リプライ日付をクライアント側のタイムゾーンに変換する。
            DateTimeOffset replyDateTime = reply.Time.ToDateTimeOffset().ToOffset(timeZone);
            DateTimeOffset replyDateTimeOffset = new DateTimeOffset(replyDateTime.DateTime, timeZone);

            DateTimeOffset replyCountryDateTime = reply.Time.ToDateTimeOffset().ToOffset(countryTimeZone);


            // 予約日を表示する。
            this.xReserveTextBox.Text = replyDateTimeOffset.ToString("yyyy年MM月dd日 H時m分") + " / 時間 = " + reply.Duration.ToTimeSpan().ToString() + " / TimeZone = " + reply.TimeZone.ToTimeSpan().ToString();
            this.xCountryTextBox.Text = replyCountryDateTime.DateTime.ToString("yyyy年MM月dd日 H時m分") + " / TimeZone = " + countryTimeZone.ToString();
            this.xReserveUtcTextBox.Text = reply.Time.ToDateTimeOffset().ToString("yyyy年MM月dd日 H時m分");

        }
    }
}


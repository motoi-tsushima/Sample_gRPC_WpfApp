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
using Microsoft.Win32;
using System.IO;
using Grpc.Core;

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
            this._timeZone = new Dictionary<string, TimeSpan>();

            var sysTimeZones = TimeZoneInfo.GetSystemTimeZones();

            foreach (TimeZoneInfo timeZone in sysTimeZones)
            {
                string timeZoneId = timeZone.Id;
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                TimeSpan offset = timeZoneInfo.BaseUtcOffset;
                this._timeZone.Add(timeZoneId, offset);
            }
        }

        private void ChangeTZButtonButton_Click(object sender, RoutedEventArgs e)
        {
            //選択していない場合は無視する。
            if (this.xSampleDatePicker.SelectedDate == null)
                return;

            if (this.xSampleTimeSpanComboBox.SelectedValue == null)
                return;

            //時間を文字列から数値に変換する。
            int hour, minute;
            if (int.TryParse(this.xSampleHourTextBox.Text, out hour) == false) return;

            if (hour >= 24)
            {
                this.xSampleHourTextBox.Text = "×24超";
                return;
            }

            if (int.TryParse(this.xSampleMinuteTextBox.Text, out minute) == false) return;

            if (minute >= 60)
            {
                this.xSampleMinuteTextBox.Text = "×60超";
                return;
            }

            //タイムゾーンを設定
            DateTime date = this.xSampleDatePicker.SelectedDate.Value;
            TimeSpan timeZone = ((KeyValuePair<string, TimeSpan>)this.xSampleTimeSpanComboBox.SelectedValue).Value;

            DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0, DateTimeKind.Utc);

            //ReservationTime 作成
            ReservationTime reservationTime = new ReservationTime();
            reservationTime.Time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime);
            reservationTime.TimeZone = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(timeZone);

            // gRPC サービスを呼び出す。
            var reply = this.grpcClient.GreeterClient.ChangeTimeZone(reservationTime);

            // 時差日を表示する。
            DateTime replyDateTime = reply.Time.ToDateTime();
            TimeSpan timeSpan = reply.TimeZone.ToTimeSpan();
            this.xChangeTextBox.Text = replyDateTime.ToString("yyyy年MM月dd日 H時m分") + " / TimeZone = " + timeSpan.ToString();
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            //選択していない場合は無視する。
            if (this.xDatePicker.SelectedDate == null)
                return;

            if (this.xTimeSpanComboBox.SelectedValue == null)
                return;

            if (this.xCountryTimeSpanComboBox.SelectedValue == null)
                return;

            //時間を文字列から数値に変換する。
            int hour, minute, span;
            if(int.TryParse(this.xHourTextBox.Text, out hour) == false) return;

            if (hour >= 24) 
            {
                this.xHourTextBox.Text = "×24超";
                return; 
            }

            if (int.TryParse(this.xMinuteTextBox.Text, out minute) == false) return;

            if (minute >= 60)
            {
                this.xMinuteTextBox.Text = "×60超";
                return;
            }

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
            reservationTime.Subject = "打合せ予約";
            reservationTime.Time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(dateTimeOffset);
            reservationTime.Duration = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(duration);
            reservationTime.TimeZone = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(timeZone);
            reservationTime.CountryTimeZone = Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(countryTimeZone);

            // gRPC サービスを呼び出す。
            var reply = this.grpcClient.GreeterClient.Reserve(reservationTime);

            // リプライ日付をクライアント側のタイムゾーンに変換する。
            DateTimeOffset replyDateTime = reply.Time.ToDateTimeOffset().ToOffset(timeZone);

            DateTimeOffset replyCountryDateTime = reply.Time.ToDateTimeOffset().ToOffset(countryTimeZone);


            // 予約日を表示する。
            this.xReserveTextBox.Text = replyDateTime.ToString("yyyy年MM月dd日 H時m分") + " / 時間 = " + reply.Duration.ToTimeSpan().ToString() + " / TimeZone = " + reply.TimeZone.ToTimeSpan().ToString();
            this.xCountryTextBox.Text = replyCountryDateTime.DateTime.ToString("yyyy年MM月dd日 H時m分") + " / TimeZone = " + countryTimeZone.ToString();
            this.xReserveUtcTextBox.Text = reply.Time.ToDateTimeOffset().ToString("yyyy年MM月dd日 H時m分");

        }

        /// <summary>
        /// ファイルダウンロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FileDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt files (*.txt)|*.txt";

            if (this.xDownloadFileNameTextBox.Text.Length == 0)
            {
                if (saveFileDialog.ShowDialog() == false)
                    return;

                this.xDownloadFileNameTextBox.Text = saveFileDialog.FileName;
            }

            FileDownloadRequest fileDownloadRequest = new FileDownloadRequest();
            fileDownloadRequest.FileName = this.xDownloadFileNameTextBox.Text;

            // gRPC サービスを呼び出す。
            var response = this.grpcClient.GreeterClient.FileDownload(fileDownloadRequest);

            using (var fs = new FileStream(fileDownloadRequest.FileName, FileMode.Create, FileAccess.Write))
            {
                int offset = 0;

                while (await response.ResponseStream.MoveNext())
                {
                    var reply = response.ResponseStream.Current;
                    byte[] bin = reply.Binary.ToByteArray();
                    fs.Write(bin, offset, (int)reply.FileSize);
                }
            }
        }
    }
}


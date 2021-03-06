﻿using System;
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
        /// ファイルダウンロードのgRPCサービスメソッドのインスタンス
        /// </summary>
        private AsyncServerStreamingCall<FileDownloadStream> _responseFileDownload;

        /// <summary>
        /// ファイルダウンロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FileDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = System.IO.Path.GetFileName(this.xDownloadFileNameTextBox.Text);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog.FileName = fileName;

            if (this.xDownloadFileNameTextBox.Text.Length == 0)
            {
                return;
            }

            if (saveFileDialog.ShowDialog() == false)
                return;

            this.FileDownloadButton.IsEnabled = false;
            this.CancelButton.IsEnabled = true;
            this.xDownloadMessage.Text = fileName + " ダウンロード中";

            FileDownloadRequest fileDownloadRequest = new FileDownloadRequest();
            fileDownloadRequest.FileName = this.xDownloadFileNameTextBox.Text;

            // gRPC サービスを呼び出す。
            //var response = this.grpcClient.GreeterClient.FileDownload(fileDownloadRequest);
            this._responseFileDownload = this.grpcClient.GreeterClient.FileDownload(fileDownloadRequest);

            try
            {
                using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                {
                    int offset = 0;

                    while (await this._responseFileDownload?.ResponseStream.MoveNext())
                    {
                        var reply = this._responseFileDownload.ResponseStream.Current;
                        byte[] bin = reply.Binary.ToByteArray();
                        fs.Write(bin, offset, (int)reply.FileSize);
                    }
                }

                this.xDownloadMessage.Text = "";
            }
            catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Cancelled)
            {
                this.xDownloadMessage.Text = "ダウンロードをキャンセルしました。";
            }

            this.FileDownloadButton.IsEnabled = true;
            this.CancelButton.IsEnabled = false;
        }

        /// <summary>
        /// ファイルダウンロードのキャンセル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._responseFileDownload == null)
                return;

            this._responseFileDownload.Dispose();
        }

        /// <summary>
        /// ファイルアップロードのgRPCサービスメソッドのインスタンス
        /// </summary>
        //private AsyncClientStreamingCall<FileUploadStream, FileUploadResponse> _callFileUpload;

        /// <summary>
        /// キャンセルした
        /// </summary>
        private bool _uploadCanceled = false;

        /// <summary>
        /// ファイルアップロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FileUploadButton_Click(object sender, RoutedEventArgs e)
        {
            AsyncClientStreamingCall<FileUploadStream, FileUploadResponse> _callFileUpload;
            string filePath;
            string fileName;
            const int BufferSize = 10240;
            byte[] bin = new byte[BufferSize];

            this._uploadCanceled = false;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            var dlg = openFileDialog.ShowDialog();
            if(dlg == false)
            {
                return;
            }

            filePath = openFileDialog.FileName;
            fileName = System.IO.Path.GetFileName(filePath);

            this.FileUploadButton.IsEnabled = false;
            this.CancelUploadButton.IsEnabled = true;
            this.xUploadMessage.Text = fileName + " アップロード中";

            // gRPC メッセージ 宣言
            FileUploadStream fileUploadStream = new FileUploadStream();
            fileUploadStream.FileName = fileName;
            fileUploadStream.FileSize = BufferSize;

            // gRPC サービスを呼び出す。
            _callFileUpload = this.grpcClient.GreeterClient.FileUpload();

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                int sendSize = 0;
                int readSize = 0;

                while((readSize = fs.Read(bin, 0, BufferSize)) > 0)
                {
                    if (this._uploadCanceled)
                    {
                        break;
                    }

                    fileUploadStream.Binary = Google.Protobuf.ByteString.CopyFrom(bin);

                    await _callFileUpload.RequestStream.WriteAsync(fileUploadStream);
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    this.xUploadMessage.Text = fileName + " アップロード中 / Send Byte=" + (sendSize += readSize);
                }

                await _callFileUpload.RequestStream.CompleteAsync();

            }

            this.FileUploadButton.IsEnabled = true;
            this.CancelUploadButton.IsEnabled = false;

            if (this._uploadCanceled)
            {
                this.xUploadMessage.Text = "キャンセルしました";
            }
            else
            {
                this.xUploadMessage.Text = "Result = " + _callFileUpload.ResponseAsync.Result.Result.ToString();
            }
        }

        /// <summary>
        /// ファイルアップロードのキャンセル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelUploadButton_Click(object sender, RoutedEventArgs e)
        {
            this._uploadCanceled = true;
        }

        /// <summary>
        /// 双方向ストリーミング実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BidiUploadButton_Click(object sender, RoutedEventArgs e)
        {
            AsyncDuplexStreamingCall<BidirectionalStreamRequest, BidirectionalStreamResponse> _callBidiStream;
            string filePath;
            string fileName;
            const int BufferSize = 10240;
            byte[] bin = new byte[BufferSize];

            this._bidiCanceled = false;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            var dlg = openFileDialog.ShowDialog();
            if (dlg == false)
            {
                return;
            }

            filePath = openFileDialog.FileName;
            fileName = System.IO.Path.GetFileName(filePath);

            this.BidiUploadButton.IsEnabled = false;
            this.CancelBidiButton.IsEnabled = true;
            this.BidiDownloadButton.IsEnabled = false;
            this.BidiMessage.Text = fileName + " アップロード中";

            // gRPC メッセージ 宣言
            BidirectionalStreamRequest bidiRequest = new BidirectionalStreamRequest();
            bidiRequest.Request = "normal";
            bidiRequest.BinarySize = BufferSize;

            // gRPC サービスを呼び出す。
            _callBidiStream = this.grpcClient.GreeterClient.BidirectionalStream();

            // 非同期レスポンス受信とファイル出力
            string writeFileName = this.BidiDownloadTextBox.Text;
            string responseMessage = "";

            var readTask = Task.Run(async () =>
            {
                FileStream wfs = null;

                try
                {
                    await foreach (var message in _callBidiStream.ResponseStream.ReadAllAsync())
                    {
                        if (this._bidiCanceled)
                        {
                            break;
                        }

                        if (wfs == null)
                        {
                            wfs = new FileStream(writeFileName, FileMode.Create, FileAccess.Write);
                        }

                        byte[] wbin = message.Binary.ToByteArray();

                        wfs.Write(wbin, 0, (int)message.BinarySize);
                    }
                }
                catch(Exception ex)
                {
                    responseMessage = ex.Message;
                }
                finally
                {
                    if (wfs != null)
                    {
                        wfs.Close();
                        wfs.Dispose();
                    }
                }
            });

            //ファイルアップロードと非同期リクエストストリーム
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                int sendSize = 0;
                int readSize = 0;

                while ((readSize = fs.Read(bin, 0, BufferSize)) > 0)
                {
                    if (this._bidiCanceled || readTask.IsCompleted || readTask.IsCanceled)
                    {
                        break;
                    }

                    bidiRequest.Binary = Google.Protobuf.ByteString.CopyFrom(bin);

                    await _callBidiStream.RequestStream.WriteAsync(bidiRequest);
                    await Task.Delay(TimeSpan.FromMilliseconds(10));

                    this.BidiMessage.Text = fileName + " 双方向ストリーミング中 / Send Byte=" + (sendSize += readSize);
                }

                await _callBidiStream.RequestStream.CompleteAsync();
            }

            //後始末
            _callBidiStream.Dispose();

            this.BidiUploadButton.IsEnabled = true;
            this.CancelBidiButton.IsEnabled = false;
            this.BidiDownloadButton.IsEnabled = true;

            if (this._bidiCanceled)
            {
                this.BidiMessage.Text = "キャンセルしました";
                this.BidiDownloadMessage.Text = "";
            }
            else
            {
                this.BidiMessage.Text = "ストリーミング完了";
                this.BidiDownloadMessage.Text = responseMessage;
            }
        }

        /// <summary>
        /// 双方向ストリーミングのダウンロードファイル名設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BidiDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = this.BidiDownloadTextBox.Text;

            if (this.BidiDownloadTextBox.Text.Length == 0)
            {
                return;
            }

            if (saveFileDialog.ShowDialog() == false)
                return;

            this.BidiDownloadTextBox.Text = saveFileDialog.FileName;

        }

        /// <summary>
        /// キャンセルした
        /// </summary>
        private bool _bidiCanceled = false;

        /// <summary>
        /// 双方向ストリーミング中断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBidiButton_Click(object sender, RoutedEventArgs e)
        {
            this._bidiCanceled = true;
        }
    }
}


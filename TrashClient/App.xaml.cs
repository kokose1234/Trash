//  Copyright 2021 Jonguk Kim
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Net;
using System.Windows;

namespace TrashClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            if (!CheckForInternetConnection())
            {
                MessageBox.Show("인터넷 연결을 확인해 주세요", "알림");
                Environment.Exit(0);
            }
        }

        private static bool CheckForInternetConnection()
        {
            try
            {
                var request = (HttpWebRequest) WebRequest.Create("https://www.google.com/");
                request.KeepAlive = false;
                request.Timeout = 3000;
                using var response = (HttpWebResponse) request.GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) =>
            MessageBox.Show(e.Exception.ToString(), "오류");
    }
}
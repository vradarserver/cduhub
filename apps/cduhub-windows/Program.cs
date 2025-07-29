// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cduhub.CommandLine;

namespace Cduhub.WindowsGui
{
    public static class Program
    {
        private const string _SingleInstanceMutexName = @"Global\CduHub-SGEZ8Z2CM8UA";

        public static Hub Hub { get; private set; }

        public static GithubUpdateChecker UpdateChecker { get; private set; }

        public static InformationalVersion Version { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var singleInstanceMutex = CreateSingleInstanceMutex(out var mutexAcquired);
            if(!mutexAcquired) {
                MessageBox.Show("Only one instance of CDU Hub can run at a time", "Already Running");
            } else {
                try {
                    Version = InformationalVersion.FromAssembly(
                        Assembly.GetExecutingAssembly()
                    );

                    var mainForm = new MainForm();
                    using(Hub = new Hub()) {
                        using(UpdateChecker = new GithubUpdateChecker()) {
                            Task.Run(() => UpdateChecker.StartCheckingAsync(Hub.HttpClient));
                            Hub.Connect();
                            Application.Run(mainForm);
                        }
                    }
                } finally {
                    singleInstanceMutex.ReleaseMutex();
                    singleInstanceMutex.Dispose();
                }
            }
        }

        private static Mutex CreateSingleInstanceMutex(out bool acquired)
        {
            var allowEveryoneRule = new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                MutexRights.FullControl,
                AccessControlType.Allow
            );
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            var result = new Mutex(false, _SingleInstanceMutexName, out var _, securitySettings);
            try {
                acquired = result.WaitOne(1000, false);
            } catch(AbandonedMutexException) {
                acquired = true;
            }

            return result;
        }

        /// <summary>
        /// Called when an unhandled exception was caught for the GUI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ShowException(e.Exception);
        }

        /// <summary>
        /// Called when an unhandled exception was caught for any thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Don't translate, I don't want to hide errors if the translation throws exceptions
            Exception ex = e.ExceptionObject as Exception;
            if(ex != null) {
                ShowException(ex);
            } else {
                MessageBox.Show(String.Format("An exception that was not of type Exception was caught.\r\n{0}", e.ExceptionObject), "Unknown Exception Caught");
            }
        }

        /// <summary>
        /// Shows the details of an exception to the user and logs it.
        /// </summary>
        /// <param name="ex"></param>
        public static void ShowException(Exception ex)
        {
            // Don't translate, I don't want to confuse things if the translation throws exceptions

            var isThreadAbort = Flatten(ex, r => r.InnerException).Any(r => r is ThreadAbortException);
            if(!isThreadAbort) {
                var message = ExceptionMultiLine(ex, "\r\n");

                try {
                    MessageBox.Show(message, "Unhandled Exception Caught");
                } catch {
                    ;
                }
            }
        }

        /// <summary>
        /// Returns a multi-line description of an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="newLine"></param>
        /// <returns></returns>
        public static string ExceptionMultiLine(Exception exception, string newLine = null)
        {
            if(newLine == null) newLine = Environment.NewLine;
            var result = "";

            if(exception != null) {
                var buffer = new StringBuilder();

                for(var ex = exception;ex != null;ex = ex.InnerException) {
                    if(buffer.Length > 0) buffer.AppendFormat("-- INNER EXCEPTION --{0}", newLine);
                    buffer.AppendFormat("{0}: {1}{2}", ex.GetType().FullName, ex.Message, newLine);

                    var socketException = ex as SocketException;
                    if(socketException != null) {
                        buffer.AppendFormat("Socket I/O error, error = {0}, native = {1}, code = {2}", socketException.ErrorCode, socketException.NativeErrorCode, socketException.SocketErrorCode);
                    }

                    buffer.AppendFormat("{0}{1}", ex.StackTrace == null ? "No stack trace" : ex.StackTrace.ToString(), newLine);
                }

                result = buffer.ToString();
            }

            return result;
        }

        /// <summary>
        /// Takes the root object in a parent-child object and returns the collection of objects in the hierarchy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="getChild"></param>
        /// <returns></returns>
        public static IEnumerable<T> Flatten<T>(T root, Func<T, T> getChild)
        {
            var result = new List<T>();
            for(var i = root;i != null;i = getChild(i)) {
                result.Add(i);
            }

            return result;
        }
    }
}

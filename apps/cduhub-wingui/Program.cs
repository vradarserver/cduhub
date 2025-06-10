// Copyright © 2025 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Net.Sockets;
using System.Text;

namespace Cduhub.WindowsGui
{
    static class Program
    {
        public static Hub Hub;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var mainForm = new MainForm();
            using(Hub = new()) {
                Hub.CloseApplication += (_,_) => mainForm.Close();
                Hub.Connect();

                Application.Run(mainForm);
            }
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

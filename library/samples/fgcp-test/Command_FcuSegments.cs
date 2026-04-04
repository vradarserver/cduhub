// Copyright © 2026 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;
using Cduhub.CommandLine;
using McduDotNet;

namespace FgcpTest
{
    class Command_FcuSegments : CommonCommand
    {
        public bool SuppressCleanup { get; set; }

        public bool Run()
        {
            var result = true;

            using(var fcu = DeviceFactory.ConnectLocalFgcp<IFgcpFcu>()) {
                if(fcu == null) {
                    Console.WriteLine("No FCU device connected");
                    result = false;
                } else {
                    Console.WriteLine($"Connected to {fcu}");

                    const int updateDelayMS = 1;
                    var nextTickUtc = DateTime.MinValue;

                    var leftValue = -1;
                    var leftDecimal = 0;
                    var leftWords = 0;

                    var rightValue = 10000;
                    var rightDecimal = 4;
                    var rightWords = 3;

                    var headingValue = 1000;
                    var headingDecimal = 3;
                    var headingWords = 0x0f;

                    var speedValue = -1;
                    var speedDecimal = 0;
                    var speedWords = 0;

                    var annunciatorWords = 0x100;

                    var altitudeValue = 0;
                    var vsValue = 9999;
                    var vsPlus = 0;
                    var vsDecimal = false;
                    var altitudeWords = 0;

                    Console.WriteLine($"Press Q to quit");
                    while(!Console.KeyAvailable || Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                        if(DateTime.UtcNow >= nextTickUtc) {
                            if(++leftValue == 10000) {
                                leftValue = 0;
                                if(++leftDecimal == 5) {
                                    leftDecimal = 0;
                                }
                                if(++leftWords == 4) {
                                    leftWords = 0;
                                }
                            }

                            if(--rightValue == -1) {
                                rightValue = 9999;
                                if(--rightDecimal == -1) {
                                    rightDecimal = 4;
                                }
                                if(--rightWords == -1) {
                                    rightWords = 3;
                                }
                            }

                            if(++speedValue == 1000) {
                                speedValue = 0;
                                if(++speedDecimal == 4) {
                                    speedDecimal = 0;
                                }
                                if(++speedWords == 8) {
                                    speedWords = 0;
                                }
                            }

                            if(--headingValue == -1) {
                                headingValue = 999;
                                if(--headingDecimal == -1) {
                                    headingDecimal = 3;
                                }
                                if(--headingWords == -1) {
                                    headingWords = 0x0f;
                                }
                            }

                            if(++annunciatorWords == 0x1000) {
                                annunciatorWords = 0;
                            }

                            if(++altitudeValue > 99999) {
                                altitudeValue = 0;
                            }
                            if(--vsValue == -1) {
                                vsValue = 9999;
                            }
                            if(altitudeValue % 100 == 0) {
                                if(++vsPlus == 3) {
                                    vsPlus = 0;
                                }
                                vsDecimal = !vsDecimal;
                                if(++altitudeWords == 0x80) {
                                    altitudeWords = 0;
                                }
                            }

                            SetupBaro(fcu.Displays.LeftBarometer, leftValue, leftDecimal, leftWords);
                            SetupBaro(fcu.Displays.RightBarometer, rightValue, rightDecimal, rightWords);
                            SetupSpeed(fcu.Displays.Speed, speedValue, speedDecimal, speedWords);
                            SetupHeading(fcu.Displays.Heading, headingValue, headingDecimal, headingWords);
                            SetupAnnunciator(fcu.Displays.Annunciator, annunciatorWords);
                            SetupAltitude(fcu.Displays.Altitude, altitudeValue, vsPlus, vsValue, vsDecimal, altitudeWords);

                            fcu.RefreshSegmentedDisplays();

                            nextTickUtc = DateTime.UtcNow.AddMilliseconds(updateDelayMS);
                        }
                    }

                    if(!SuppressCleanup) {
                        fcu.Cleanup();
                    }
                }
            }

            return result;
        }

        private void SetupBaro(FcuDisplayBarometer baro, int number, int decimalIndex, int wordFlags)
        {
            var text = FormatRightDecimalNumber("{0:0000}", number, decimalIndex);
            baro.BaroDigits.SetFrom(text);
            baro.Qfe = (wordFlags & 0x01) != 0;
            baro.Qnh = (wordFlags & 0x02) != 0;
        }

        private void SetupSpeed(FcuDisplaySpeed speed, int number, int decimalIndex, int wordFlags)
        {
            var text = FormatLeftDecimalNumber("{0:000}", number, decimalIndex);
            speed.SpeedDigits.SetFrom(text);
            speed.Dot = (wordFlags & 0x01) != 0;
            speed.Spd = (wordFlags & 0x02) != 0;
            speed.Mach = (wordFlags & 0x04) != 0;
        }

        private void SetupHeading(FcuDisplayHeading heading, int number, int decimalIndex, int wordFlags)
        {
            var text = FormatLeftDecimalNumber("{0:000}", number, decimalIndex);
            heading.HeadingDigits.SetFrom(text);
            heading.Dot = (wordFlags & 0x01) != 0;
            heading.Hdg = (wordFlags & 0x02) != 0;
            heading.Trk = (wordFlags & 0x04) != 0;
            heading.Lat = (wordFlags & 0x08) != 0;
        }

        private void SetupAnnunciator(FcuDisplayAnnunciator annunciator, int annunciatorWords)
        {
            var flags = (annunciatorWords & 0xf00) >> 8;
            annunciator.Hdg = (flags & 0x01) != 0;
            annunciator.VS = (flags & 0x02) != 0;
            annunciator.Trk = (flags & 0x04) != 0;
            annunciator.Fpa = (flags & 0x08) != 0;
        }

        private void SetupAltitude(FcuDisplayAltitude altitude, int altitudeValue, int vsPlus, int vsValue, bool vsDecimal, int flags)
        {
            altitude.AltitudeDigits.SetFrom(altitudeValue.ToString("00000"));
            char vsPrefix;
            switch(vsPlus) {
                case 0:     vsPrefix = ' '; break;
                case 1:     vsPrefix = '-'; break;
                case 2:     vsPrefix = '|'; break;
                case 3:     vsPrefix = '+'; break;
                default:    throw new NotImplementedException();
            }
            var vsText = $"{vsPrefix}{FormatLeftDecimalNumber("{0:0000}", vsValue, vsDecimal ? 2 : 0)}";
            altitude.VerticalSpeedDigits.SetFrom(vsText);

            altitude.Alt =             (flags & 0x01) != 0;
            altitude.LvlChGroupLeft =  (flags & 0x02) != 0;
            altitude.LvlCh =           (flags & 0x04) != 0;
            altitude.LvlChGroupRight = (flags & 0x08) != 0;
            altitude.VS =              (flags & 0x10) != 0;
            altitude.Fpa =             (flags & 0x20) != 0;
            altitude.AltDot =          (flags & 0x40) != 0;
        }

        private static string FormatRightDecimalNumber(string format, int number, int decimalIndex)
        {
            var buffer = new StringBuilder();
            buffer.AppendFormat(format, number);

            if(decimalIndex == buffer.Length) {
                buffer.Append('.');
            } else if(decimalIndex > 0) {
                buffer.Insert(decimalIndex, '.');
            }

            return buffer.ToString();
        }

        private static string FormatLeftDecimalNumber(string format, int number, int decimalIndex)
        {
            var buffer = new StringBuilder();
            buffer.AppendFormat(format, number);

            if(decimalIndex > 0) {
                buffer.Insert(decimalIndex - 1, '.');
            }

            return buffer.ToString();
        }
    }
}

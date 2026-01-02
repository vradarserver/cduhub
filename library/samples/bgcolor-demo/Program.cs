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
using WwDevicesDotNet;

namespace BackgroundColorDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Background Color Demo");
            Console.WriteLine("====================");
            Console.WriteLine();
            
            using(var cdu = CduFactory.ConnectLocal()) {
                Console.WriteLine($"Connected to: {cdu.DeviceId}");
                
                // Example 1: Simple inverted text (white background, black text)
                Console.WriteLine("\nExample 1: Inverted text (C-130 style)");
                ShowInvertedText(cdu);
                WaitForKey("Press any key for Example 2...");
                
                // Example 2: Colored backgrounds with different foreground colors
                Console.WriteLine("\nExample 2: Colored backgrounds");
                ShowColoredBackgrounds(cdu);
                WaitForKey("Press any key for Example 3...");
                
                // Example 3: Alert/Warning style display
                Console.WriteLine("\nExample 3: Alert/Warning display");
                ShowAlertDisplay(cdu);
                WaitForKey("Press any key for Example 4...");
                
                // Example 4: Using Cell.Set() directly
                Console.WriteLine("\nExample 4: Direct cell manipulation");
                ShowDirectCellManipulation(cdu);
                WaitForKey("Press any key for Example 5...");
                
                // Example 5: Compositor API fluent style
                Console.WriteLine("\nExample 5: Compositor API (your requested style)");
                ShowCompositorApiStyle(cdu);
                WaitForKey("Press any key for Example 6...");
                
                // Example 6: Palette customization
                Console.WriteLine("\nExample 6: Palette customization");
                ShowPaletteCustomization(cdu);
                WaitForKey("Press any key to cleanup...");
                
                cdu.Cleanup();
                Console.WriteLine("\nDemo complete!");
            }
        }
        
        static void ShowInvertedText(ICdu cdu)
        {
            // Example 1: Classic inverted text style (like C-130 displays)
            cdu.Screen.Clear();
            
            // Title line with inverted text using Compositor
            cdu.Output
                .Line(0)
                .BGWhite()
                .Colour(Colour.Black)
                .Write("  WARNING  ")
                .BGBlack()  // Reset background
                .Colour(Colour.White);

            // Normal text below
            cdu.Output
                .Line(2)
                .Write("This is white")
                .Line(3)
                .Write("on black background");
            
            cdu.RefreshDisplay();
        }
        
        static void ShowColoredBackgrounds(ICdu cdu)
        {
            // Example 2: Show various background/foreground combinations
            cdu.Screen.Clear();
            
            var line = 0;
            
            // Title
            SetInvertedLine(cdu, line++, "COLOR COMBINATIONS", Colour.Amber, Colour.Black);
            line++;
            
            // Various combinations from the color table
            SetColoredLine(cdu, line++, "BLACK ON WHITE", Colour.Black, Colour.White);
            SetColoredLine(cdu, line++, "BLACK ON AMBER", Colour.Black, Colour.Amber);
            SetColoredLine(cdu, line++, "BLACK ON CYAN", Colour.Black, Colour.Cyan);
            SetColoredLine(cdu, line++, "WHITE ON RED", Colour.White, Colour.Red);
            SetColoredLine(cdu, line++, "WHITE ON GREEN", Colour.White, Colour.Green);
            SetColoredLine(cdu, line++, "BLACK ON YELLOW", Colour.Black, Colour.Yellow);
            SetColoredLine(cdu, line++, "WHITE ON MAGENTA", Colour.White, Colour.Magenta);
            
            cdu.RefreshDisplay();
        }
        
        static void ShowAlertDisplay(ICdu cdu)
        {
            // Example 3: Alert/Warning style display using Compositor API
            cdu.Screen.Clear();
            
            // Red background alert header
            cdu.Output
                .Line(0)
                .BGRed()
                .Colour(Colour.White)
                .Centred("!! SYSTEM ALERT !!")
                .BGBlack();  // Reset background
            
            // Amber background warning
            cdu.Output
                .Line(2)
                .BGAmber()
                .Colour(Colour.Black)
                .Centred("  FUEL LOW  ")
                .BGBlack();  // Reset background
            
            // Details in normal colors
            cdu.Output
                .Line(4)
                .Colour(Colour.Yellow)
                .Write("REMAINING: 120 LBS")
                .Line(6)
                .Colour(Colour.White)
                .Write("NEAREST AIRPORT:")
                .Line(7)
                .Colour(Colour.Green)
                .Write("KJFK 15NM");
            
            // Green background "OK" status at bottom
            cdu.Output
                .Line(12)
                .BGGreen()
                .Black()
                .Centred("  PUMPS OK  ")
                .BGBlack();  // Reset background
            
            cdu.RefreshDisplay();
        }
        
        static void ShowDirectCellManipulation(ICdu cdu)
        {
            // Example 4: Direct manipulation of cells
            cdu.Screen.Clear();
            
            // Create a progress bar effect using background colors
            var line = 5;
            var progressPercent = 65; // 65% progress
            var progressCells = (Metrics.Columns * progressPercent) / 100;
            
            // Label
            cdu.Output
                .Line(3)
                .Centred("DOWNLOAD PROGRESS");
            
            // Progress bar
            for(int col = 0; col < Metrics.Columns; col++) {
                var cell = cdu.Screen.Rows[line].Cells[col];
                cell.Character = '█';
                cell.Small = false;
                
                if(col < progressCells) {
                    // Completed portion - green background
                    cell.Colour = Colour.Green;
                    cell.BackgroundColour = Colour.Green;
                } else {
                    // Remaining portion - dark gray
                    cell.Colour = Colour.Grey;
                    cell.BackgroundColour = Colour.Black;
                }
            }
            
            // Percentage text
            cdu.Output
                .Line(7)
                .Colour(Colour.Cyan)
                .Centred($"{progressPercent}% COMPLETE");
            
            cdu.RefreshDisplay();
        }
        
        static void ShowCompositorApiStyle(ICdu cdu)
        {
            // Example 5: Your requested Compositor API style
            cdu.Screen.Clear();
            
            // Title with inverted colors
            cdu.Output
                .Line(0)
                .BGAmber()
                .Colour(Colour.Black)
                .Centred("FLIGHT STATUS")
                .BGBlack();  // Reset background
            
            // Multi-color status display exactly as you requested
            cdu.Output
                .Line(4)
                .Colour(Colour.Yellow)
                .BGColour(Colour.Black)
                .Write("REMAINING: ")
                .BGGreen()
                .Colour(Colour.Black)
                .Write(" 120 LBS ")
                .BGBlack()
                .Line(6)
                .Colour(Colour.White)
                .Write("NEAREST AIRPORT:")
                .Line(7)
                .Colour(Colour.Green)
                .Write("KJFK 15NM");
            
            // Additional examples of fluent API
            cdu.Output
                .Line(9)
                .Colour(Colour.Cyan)
                .Write("STATUS: ")
                .BGGreen()
                .Colour(Colour.Black)
                .Write(" NORMAL ")
                .BGBlack()
                .Colour(Colour.White)
                .Write(" TEMP: ")
                .BGAmber()
                .Colour(Colour.Black)
                .Write(" WARN ")
                .BGBlack();
            
            // Inverted line at bottom
            cdu.Output
                .Line(12)
                .BGWhite()
                .Colour(Colour.Black)
                .Centred("PRESS ANY KEY TO CONTINUE")
                .BGBlack();
            
            cdu.RefreshDisplay();
        }
        
        static void ShowPaletteCustomization(ICdu cdu)
        {
            // Example 6: Palette customization
            cdu.Screen.Clear();
            
            // Show with default palette
            cdu.Output
                .Line(0)
                .Centred("DEFAULT PALETTE")
                .Line(2)
                .Colour(Colour.White)
                .Write("This is 'White': ")
                .Write($"#{cdu.Palette.White}")
                .Line(3)
                .Colour(Colour.Amber)
                .Write("This is 'Amber': ")
                .Write($"#{cdu.Palette.Amber}");
            
            cdu.RefreshDisplay();
            System.Threading.Thread.Sleep(2000);
            
            // Customize the palette
            var originalWhite = new PaletteColour();
            originalWhite.CopyFrom(cdu.Palette.White);
            var originalAmber = new PaletteColour();
            originalAmber.CopyFrom(cdu.Palette.Amber);
            
            // Make "White" actually red, "Amber" actually blue
            cdu.Palette.White.Set(0xFF, 0x00, 0x00);  // Red
            cdu.Palette.Amber.Set(0x00, 0x00, 0xFF);  // Blue
            cdu.RefreshPalette();
            
            cdu.Output
                .Line(5)
                .Centred("CUSTOMIZED PALETTE")
                .Line(7)
                .Colour(Colour.White)  // Index "White" → but displays RED
                .Write("This is 'White': ")
                .Write($"#{cdu.Palette.White}")
                .Line(8)
                .Colour(Colour.Amber)  // Index "Amber" → but displays BLUE
                .Write("This is 'Amber': ")
                .Write($"#{cdu.Palette.Amber}");
            
            cdu.RefreshDisplay();
            System.Threading.Thread.Sleep(2000);
            
            // Restore original palette
            cdu.Palette.White.CopyFrom(originalWhite);
            cdu.Palette.Amber.CopyFrom(originalAmber);
            cdu.RefreshPalette();
        }
        
        // Helper method to set an entire line with foreground and background colors
        static void SetColoredLine(ICdu cdu, int line, string text, Colour foreground, Colour background)
        {
            for(int col = 0; col < Metrics.Columns && col < text.Length; col++) {
                var cell = cdu.Screen.Rows[line].Cells[col];
                cell.Character = text[col];
                cell.Colour = foreground;
                cell.BackgroundColour = background;
                cell.Small = false;
            }
            
            // Fill remaining cells with spaces
            for(int col = text.Length; col < Metrics.Columns; col++) {
                var cell = cdu.Screen.Rows[line].Cells[col];
                cell.Character = ' ';
                cell.Colour = Colour.White;
                cell.BackgroundColour = Colour.Black;
                cell.Small = false;
            }
        }
        
        // Helper method to center text with inverted colors
        static void SetInvertedLine(ICdu cdu, int line, string text, Colour foreground, Colour background)
        {
            var startCol = Math.Max(0, (Metrics.Columns - text.Length) / 2);
            
            for(int col = 0; col < Metrics.Columns; col++) {
                var cell = cdu.Screen.Rows[line].Cells[col];
                
                if(col >= startCol && col < startCol + text.Length) {
                    cell.Character = text[col - startCol];
                } else {
                    cell.Character = ' ';
                }
                
                cell.Colour = foreground;
                cell.BackgroundColour = background;
                cell.Small = false;
            }
        }
        
        static void WaitForKey(string message)
        {
            Console.WriteLine();
            Console.WriteLine(message);
            Console.ReadKey(intercept: true);
        }
    }
}

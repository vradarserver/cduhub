using McduDotNet;

namespace Leds
{
    class Program
    {
        static void Main(string[] _)
        {
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");

                mcdu.Output
                    .Small()
                    .Grey()
                    .LeftLabel(1, ">FAIL")
                    .LeftLabel(2, ">FM")
                    .LeftLabel(3, ">FM1")
                    .LeftLabel(4, ">FM2")
                    .LeftLabel(5, ">IND")
                    .LeftLabel(6, ">LINE")
                    .RightLabel(1, "MCDU<")
                    .RightLabel(2, "MENU<")
                    .RightLabel(3, "RDY<")
                    .RightLabel(5, "BRIGHT -5%<")
                    .RightLabel(6, "BRIGHT +5%<");
                mcdu.RefreshDisplay();

                mcdu.KeyDown += (_, args) => {
                    var refreshLeds = true;
                    switch(args.Key) {
                        case Key.LineSelectLeft1:   mcdu.Leds.Fail = !mcdu.Leds.Fail; break;
                        case Key.LineSelectLeft2:   mcdu.Leds.Fm = !mcdu.Leds.Fm; break;
                        case Key.LineSelectLeft3:   mcdu.Leds.Fm1 = !mcdu.Leds.Fm1; break;
                        case Key.LineSelectLeft4:   mcdu.Leds.Fm2 = !mcdu.Leds.Fm2; break;
                        case Key.LineSelectLeft5:   mcdu.Leds.Ind = !mcdu.Leds.Ind; break;
                        case Key.LineSelectLeft6:   mcdu.Leds.Line = !mcdu.Leds.Line; break;
                        case Key.LineSelectRight1:  mcdu.Leds.Mcdu = !mcdu.Leds.Mcdu; break;
                        case Key.LineSelectRight2:  mcdu.Leds.Menu = !mcdu.Leds.Menu; break;
                        case Key.LineSelectRight3:  mcdu.Leds.Rdy = !mcdu.Leds.Rdy; break;
                        case Key.LineSelectRight5:  mcdu.Leds.Brightness = Math.Max(0, mcdu.Leds.Brightness - 0.05); break;
                        case Key.LineSelectRight6:  mcdu.Leds.Brightness = Math.Min(100, mcdu.Leds.Brightness + 0.05); break;
                        default:                    refreshLeds = false; break;
                    }

                    if(refreshLeds) {
                        mcdu.RefreshLeds();
                    }
                };

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                mcdu.Cleanup();
            }
        }
    }
}

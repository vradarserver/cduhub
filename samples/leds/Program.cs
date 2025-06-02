using McduDotNet;

namespace Leds
{
    class Program
    {
        static void Main(string[] _)
        {
            using(var mcdu = McduFactory.ConnectLocal()) {
                Console.WriteLine($"Using {mcdu.ProductId} MCDU");

                var screen = mcdu.Screen;
                screen.Small = true;

                screen.LeftLineSelect(1, ">FAIL");
                screen.LeftLineSelect(2, ">FM");
                screen.LeftLineSelect(3, ">FM1");
                screen.LeftLineSelect(4, ">FM2");
                screen.LeftLineSelect(5, ">IND");
                screen.LeftLineSelect(6, ">LINE");

                screen.RightLineSelect(1, "MCDU<");
                screen.RightLineSelect(2, "MENU<");
                screen.RightLineSelect(3, "RDY<");
                screen.RightLineSelect(5, "BRIGHT -5%<");
                screen.RightLineSelect(6, "BRIGHT +5%<");
                mcdu.RefreshDisplay();

                mcdu.KeyDown += (_, args) => {
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
                    }
                };

                Console.WriteLine($"Press Q to quit");
                while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q);

                mcdu.Screen.Clear();
                mcdu.RefreshDisplay();
                mcdu.Leds.TurnAllOn(false);
            }
        }
    }
}

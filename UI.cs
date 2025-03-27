using Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Poker
{
    static public class UI
    {
        static UI()
        {
            Console.CursorVisible = false;
        }
        
        static public class Header
        {
            static Header()
            {
                Console.SetCursorPosition(2, 1);
                Console.Out.WriteLine("Player 00 plays now with balance of 0$");
            }

            static public void update(int playerId = -1, int playerBalance = -1)
            {
                if (playerId != -1) {
                    Console.SetCursorPosition(7, 1);
                    Console.Out.WriteLine( (playerId > 8) ? "0"+playerId : playerId );
                }

                if (playerBalance != -1)
                {
                    Console.SetCursorPosition(37, 1);
                    Console.Out.WriteLine( (playerBalance + "$").PadRight(10, ' ') );
                }
                
            }
        }


        static public class Table
        {
            static Table()
            {
                Console.SetCursorPosition(0, 5);
                Console.Out.Write('+' + new string('-', 79) + '+');

                for (int i = 6; i < 15; ++i)
                {
                    Console.SetCursorPosition(0, i); Console.Out.Write('|');
                    Console.SetCursorPosition(80, i); Console.Out.Write('|');

                }

                Console.SetCursorPosition(0, 15);
                Console.Out.Write('+' + new string('-', 79) + '+');
            }

            static public void update(String[]? hand = null, string[]? communityCards = default)
            {
                if ( (communityCards ?? Array.Empty<string>() ).Length != 0)
                {
                    int temp = 4 * communityCards.Length - 2;

                    Console.SetCursorPosition(temp/2, 8);
                    Array.ForEach( communityCards, card => Console.Out.Write(card + "  ") );

                }

                if ((hand ?? Array.Empty<string>()).Length != 0)
                {
                    int temp = 4 * hand.Length - 2;

                    Console.SetCursorPosition(temp / 2, 13);
                    Array.ForEach(hand, card => Console.Out.Write(card + "  "));

                }

            }
        }


        static public class Option
        {
            static Option()
            {
                Console.SetCursorPosition(0, 20);
                Console.Out.WriteLine(new string('-', 68));
                Console.Out.WriteLine("| 'Space' for  check | 'Z' for call | 'X' for raise | 'W' for fold |");
                Console.Out.WriteLine(new string('-', 68));

            }

            static public void update()
            {

            }
        }








    }
}

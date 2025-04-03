using Max;

namespace Poker
{
    static public class UI
    {


        static public class Header
        {
            static Header()
            {
                Console.SetCursorPosition(2, 1);
                Console.Out.WriteLine("Player 00 plays now   |   Balance/Betted: 0$/0$");
            }

            static public void update(int playerId = -1, int playerBalance = -1, uint playerBet = 0)
            {
                if (playerId != -1)
                {
                    Console.SetCursorPosition(9, 1);
                    Console.Out.WriteLine((playerId < 10) ? "0" + playerId : playerId);
                }

                if (playerBalance != -1)
                {
                    Console.SetCursorPosition(44, 1);
                    Console.Out.WriteLine((playerBalance + "$ / " + playerBet + '$').PadRight(25, ' '));
                }

            }
        }


        static public class Table
        {
            static Table()
            {
                Console.SetCursorPosition(0, 5);
                Console.Out.Write('+' + new string('-', 79) + '+');

                for (int i = 6; i < 18; ++i)
                {
                    Console.SetCursorPosition(0, i); Console.Out.Write('|');
                    Console.SetCursorPosition(80, i); Console.Out.Write('|');

                }

                Console.SetCursorPosition(0, 18);
                Console.Out.Write('+' + new string('-', 79) + '+');
            }

            static public void update(Card[]? hand = null, Card[]? communityCards = default)
            {
                if ((communityCards ?? Array.Empty<Card>()).Length != 0)
                {
                    int temp = 4 * communityCards.Length - 2;

                    Console.SetCursorPosition(38 - temp / 2, 8);
                    Array.ForEach(communityCards, card => Console.Out.Write(card.getCard() + "  "));

                }

                if ((hand ?? Array.Empty<Card>()).Length != 0)
                {
                    int temp = 4 * hand.Length - 2;

                    Console.SetCursorPosition(38 - temp / 2, 17);
                    Array.ForEach(hand, card => Console.Out.Write(card.getCard() + "  "));

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

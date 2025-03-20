using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Max
{
    public class Program
    {

        static List<Card> communityCards = new List<Card>(); // the five cards in the middle of the poker table
        static Deck deck = new Deck(1);

        static int Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Player[] players;

            {
                int temp = UI.inputI("How many players are joining?: ") ?? 0;
                players = new Player[(temp < 2) ? 2 : temp];


                int balance = UI.inputI("What should be the balance of the players?: ") ?? 1000;


                ////////////////////////////////////////////////////////////////////////Dealing cards
                deck.shuffle(5);

                for (int i = 0; i < players.Length; ++i)
                {
                    players[i] = new Player(balance);
                    players[i].updateHand(deck.first());
                }
            }
            UI.print("Done with dealing first cards");

            for (int i = 0; i < players.Length; ++i)
            {

                players[i].updateHand(deck.first());
            }

            UI.print("Done with dealing second cards");
            ////////////////////////////////////////////////////////////////////////



            UI.Reset();
            UI.print($"{deck.cards.Count / 52} decks of cards in the deck - {deck.cards.Count} cards in the game - {players.Length} players online", 0, 0, true);
            UI.print(" ------------------------------------------------------------------", 0, 15, true);
            UI.print("| 'Space' for  check | 'Z' for call | 'X' for raise | 'W' for fold |", 0, 16, true);
            UI.print(" ------------------------------------------------------------------", 0, 17, true);
            UI.print("What will you do?", 0, 18, true);

            


            ////////////////////////////////////////////////////////////////////////Game loop
            ConsoleKey key;

            for (int turn = 0; ; ++turn) //(int turn = 0; turn < 3; ++turn) //The game loop
            {
                for (int playerIndex = 0; playerIndex < players.Length; ++playerIndex)
                {
                    int i = 0;
                    communityCards.ForEach(
                        x => {
                            UI.print(x.getCard(), 5 + i * 12, 8, true);
                            ++i;
                        });

                    UI.print($"{players[playerIndex].balance}", 80, 5, true);
                    UI.print($"{players[playerIndex].bet}", 80, 6, true);


                    UI.print($"{players[playerIndex].hand[0].getCard()}    {players[playerIndex].hand[1].getCard()}", 15, 20, true); 



                    if (players[playerIndex].folded) continue;

                    key = Console.ReadKey(intercept: true).Key;
                    UI.Reset();



                    // check, call, raise, or fold
                    switch (key)
                    {

                        case ConsoleKey.Spacebar: //check

                            UI.print("Check             ", 2, 4, true);
                            break;

                        case ConsoleKey.Z: //call


                            if (!players[playerIndex].newBet(players[(playerIndex - 1 < 0) ? (players.Length + playerIndex - 1) : (playerIndex - 1)].bet))
                            {
                                players[playerIndex].allIn();
                            }

                            UI.print($"Called for {players[playerIndex].bet}$", 2, 4, true);

                            break;

                        case ConsoleKey.X: //raise

                            if (!players[playerIndex].newBet(UI.inputI("What will be your new bet?: ", 0, 22) ?? players[playerIndex].bet))
                            {
                                players[playerIndex].allIn();
                            }

                            UI.print($"Raised to {players[playerIndex].bet}$ ", 2, 4, true);


                            break;

                        case ConsoleKey.W: //fold

                            UI.print($"{"Max"} folded   ", 2, 4, true);

                            players[playerIndex].fold();

                            break;


                        case ConsoleKey.Escape:

                            return 0;


                    }


                }
                communityCardsAdd(turn);

                if (turn > 2)
                {
                    UI.print( players[0].sequenceFinder(players[0].hand.Concat(communityCards).OrderByDescending(cards => cards.value).ToArray(), "@@"));
                }
            }

            

            //checking for the highest pair
            for (int playerIndex = 0; playerIndex < players.Length; ++playerIndex)
            {


            }


                return 0;
        }

        public static void communityCardsAdd(int turn)
        {
            if (turn > 2) return;// 5 -1

            deck.first(); //burn

            if (turn == 0) { communityCards.Add(deck.first()); communityCards.Add(deck.first()); }
            communityCards.Add(deck.first());
        }




    }




    public class Player
    {
        public Card[] hand { get; private set; } = new Card[2];
        public int balance { get; private set; }
        public int bet { get; private set; } = 0;

        public bool folded { get; private set; } = false;

        public Player(int balance)
        {
            this.balance = balance;

        }

        public void fold(bool activate = false)
        {

            folded = true;
            if (activate) folded = false;
        }

        public void allIn()
        {
            bet = balance;
        }
        public bool newBet(int x)
        {
            if (x > balance) return false;

            bet = x;
            return true;
        }

        public void updateHand(Card card)
        {
            if (hand[0].Equals(new Card())) hand[0] = card;
            else if (hand[1].Equals(new Card())) hand[1] = card;
        }



        public (int rank, Card[] cards) rank(Card [] hand, List<Card> communityCards)
        {
            Card[] cards = hand.Concat(communityCards).ToArray();

            Card[] temp;



            /// <para> '-' means don't care </para>
            /// <para> '#' means match for suit </para>
            /// <para> '@' means match for value. Fx. "@@@@" is four of a kind and "@@-@@" is 2 pairs </para>
            /// <para> 's' means match for next value to be one less (straight), if starting with uppercase the output will start with highest card </para>




            //Royal flush
            {
                temp = matchByStraight(cards);
                temp = matchBySuit(temp);

                if (temp.Length > 5) return (9, temp);
            }

            //Straight flush
            {
                temp = matchByStraight(cards, 13);
                temp = matchBySuit(temp);

                if (temp.Length > 5) return (8, temp);
            }

            //Four of a kind
            {
                temp = matchByValue(cards, 4);

                if (temp.Length > 4) return (7, temp);
            }

            //Full house
            {
                temp = matchByValue(cards, 3);
                if (temp.Length > 2) temp = temp.Concat( matchByValue(cards, 2, temp[0].value-1) ).ToArray();

                if (temp.Length > 5) return (6, temp);
            }

            //Flush
            {
                temp = matchBySuit(cards);

                if (temp.Length > 5) return (5, temp);
            }

            //Straight
            {
                temp = matchByStraight(cards);

                if (temp.Length > 5) return (4, temp);
            }

            //Three of a kind
            {
                temp = matchByValue(cards, 3);

                if (temp.Length > 3) return (3, temp);
            }

            //Two pairs
            {
                temp = matchByValue(cards, 2);
                temp = temp.Concat( matchByValue(cards, 2, temp[0].value-1) ).ToArray();

                if (temp.Length > 4) return (2, temp);
            }

            //Pair
            {
                temp = matchByValue(cards, 2);

                if (temp.Length > 2) return (1, temp);
            }

            //High card
            {
                return (0, matchByValue(cards, 1));
            }



        }

        Card[] matchBySuit(Card[] cards)
        {
            string suits = string.Join("", cards.Select((c) => c.suit.ToString() ?? "").ToArray());

            if (suits.Length - 5 >= suits.Replace("0", "").Length) return cards.Where((c) => c.suit == 0).ToArray();
            if (suits.Length - 5 >= suits.Replace("1", "").Length) return cards.Where((c) => c.suit == 1).ToArray();
            if (suits.Length - 5 >= suits.Replace("2", "").Length) return cards.Where((c) => c.suit == 2).ToArray();
            if (suits.Length - 5 >= suits.Replace("3", "").Length) return cards.Where((c) => c.suit == 3).ToArray();

            return new Card[0];
        }

        Card[] matchByValue(Card[] cards, int lenght, int start = 14)
        {
            string values = string.Join("", cards.Select((c) => c.value.ToString("X")).ToArray());

            for (int i = start; i > 0; --i)
            {
                if (values.Length - lenght >= values.Replace(i.ToString("X"), "").Length) return cards.Where((c) => c.value == i).ToArray();
            }


            return new Card[0];
        }

        Card[] matchByStraight(Card[] cards, int start = 14)
        {
            char[] values = new string(' ', 14).ToCharArray();
            foreach (Card c in cards)
            {
                values[c.value] = '#';
            }
            int index = string.Join("", values.Reverse()).IndexOf("#####"); if (index < 1) return new Card[0];

            return cards.Where( (c) => c.value >= index && c.value <= index+5).ToArray();
            
        }

        /// <summary>
        /// Findes a match in yours and community cards using the sequence
        /// </summary>
        /// <param name="sequence">
        /// <para> Each charcter has a meaning in the sequence. </para>
        /// <para> '-' means don't care </para>
        /// <para> '#' means match for suit </para>
        /// <para> '@' means match for value. Fx. "@@@@" is four of a kind and "@@-@@" is 2 pairs </para>
        /// <para> 's' means match for next value to be one less (straight), if starting with uppercase the output will start with highest card </para>
        /// <para> Fx. "S####" royal flush, "s####" straight flush and "sssss" flush</para>
        /// </param>
        /// <returns></returns>




       

    }
        


    public class Deck
    {
        public List<Card> cards { get; private set; } = new List<Card>();


        public Deck(int decks)
        {
            for (int i = 0; i < decks; ++i)
            {
                for (int j = 2; j <= 14; ++j)
                {
                    for (int k = 0; k < 4; ++k)
                    {

                        cards.Add(new Card(j, k));
                    }
                }
            }
        }

        public void shuffle(int times)
        {
            for (int i = 0; i < times; ++i)
            {
                for (int j = 0; j < cards.Count; ++j)
                {
                    int index = Random.Shared.Next(cards.Count);

                    Card temp = cards[index];
                    cards[index] = cards[j];
                    cards[j] = temp;

                }
            }
        }

        public Card first()
        {
            Card c = cards[0];
            cards.RemoveAt(0);
            return c;
        }
    }



    public struct Card
    {
        public int value { get; private set; }
        public int suit { get; private set; }

        public Card(int value, int suit)
        {
            this.value = value;
            this.suit = suit;
        }

        public string getSuit() => new[] { "♥️", "♠️", "♦️", "♣️" }[suit]; //To get get suit of the card

        public string getValue()
        {
            //Return for value or number of the card
            if (value <= 10) return value.ToString();
            return "JDKA"[value - 11] + "";
        }

        public string getCard() => $"{getValue()} {getSuit()}";

    }






    public static class UI
    {

        static List<(string, int, int)> buffer = new List<(string, int, int)>();

        static UI()
        {
            Console.CursorVisible = false;
        }

        public static void print(object text)
        {
            Console.Write(text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void print(object text, int x = 0, int y = 0, bool sticky = false ) // the name sticky comes from CSS, where it is used to make an element fixed or sticked to one position on the screen and not an the body
        {

            Console.SetCursorPosition(x, y);
            Console.Write(text);
           
            if (sticky && buffer.Contains(((string)text, x, y))) buffer.Remove( ((string)text, x, y) );
            if (sticky) buffer.Add((text.ToString() ?? "", x, y));
        }

        public static string inputS(string text)
        {
            print(text);
            return Console.ReadLine() ?? "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>String form of the input using consol stream</returns>
        public static string inputS(string text, int x, int y, int space = 1)
        {
            print(text, x, y);
            return Console.ReadLine() ?? "";
        }

        public static int? inputI(string text)
        {
            print(text);

            if (!int.TryParse(Console.ReadLine(), out int value)) return null;
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>String form of the input using consol stream</returns>
        public static int? inputI(string text, int x, int y)
        {
            print(text, x, y);

            if (!int.TryParse(Console.ReadLine(), out int value)) return null;
            return value;
        }


        public static void cursorLineReset()
        {
            Console.WriteLine();
        }


        public static void Reset(bool cleanBuffer = false)
        {
            Console.Clear();
            
            if (cleanBuffer)
            {
                buffer.Clear();
            }
            else
            {
                foreach ((string, int, int) e in buffer.ToArray())
                {
                    print(e.Item1, e.Item2, e.Item3);
                }
            }
        }
    }
}
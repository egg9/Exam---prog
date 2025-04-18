﻿using Spectre.Console;
using System.Text;

namespace Max
{
    static public class Program
    {

        static List<Card> communityCards = new List<Card>(5); // the five cards in the middle of the poker table
        static List<Player> players = new List<Player>();
        static Deck deck = new Deck();


        static int Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            deck.shuffle(1000);

            do
            {

                Console.Clear();
                AnsiConsole.Clear();

                for (
                    int i = AnsiConsole.Prompt(
                    new TextPrompt<int>("How many players will be joining? ").DefaultValue(2)
                    .Validate((n) => n switch
                    {
                        < 1 => ValidationResult.Error("At least one person must be playing"),
                        < 2000 => ValidationResult.Success(),
                        _ => ValidationResult.Error("Damn you got way too many friend or just have a good imagination")
                    })
                    ), balance = AnsiConsole.Prompt(
                    new TextPrompt<int>("What should be the balance of each player? ").DefaultValue(1000)
                    .Validate((n) =>
                    {
                        if (n <= 0) return ValidationResult.Error("Come on, don't gamble if you're broke");
                        else if (n <= 2147483647 / i) return ValidationResult.Success();
                        else return ValidationResult.Error("Chill cowboy, there is a limit");
                    })
                    ); i > 0; --i)
                {
                    players.Add(new Player((uint)balance));
                    players[players.Count - 1].updateHand(new[] { deck.first(), deck.first() });
                }


            } while (!AnsiConsole.Prompt(
                new ConfirmationPrompt("Are you people sure?")
                ));

            Console.CursorVisible = false;


            do
            {

                Console.Clear();
                AnsiConsole.Clear();
                //start rendering
                Poker.UI.Header.update();
                Poker.UI.Table.update();
                Poker.UI.Option.update();
                // end



                ////////////////////////////////////////////////////////////////////////Game loop
                ConsoleKey key;
                uint biggestBet = 0;

                for (uint turn = 1; turn < 5; turn = communityCardsAdd(turn))
                {


                    for (int j = 0; j < players.Count; ++j) //The game loop
                    {
                        if (players[j].folded) continue;
                        players[j].Hand = players[j].rank(communityCards);


                        Poker.UI.Header.update((int)players[j].Id, (int)players[j].balance, players[j].bet);
                        Poker.UI.Table.update(players[j].hand, communityCards.ToArray());


                        do
                        {
                            key = Console.ReadKey(intercept: true).Key;

                            switch (key)
                            {

                                case ConsoleKey.Spacebar: //check

                                    if (players[j].bet < biggestBet) key = ConsoleKey.None;

                                    break;

                                case ConsoleKey.Z: //call


                                    if (!players[j].newBet(biggestBet))
                                    {
                                        players[j].allIn();
                                    }

                                    UI.print($"Called for {players[j].bet}$", 2, 4, true);

                                    break;

                                case ConsoleKey.X: //raise

                                    if (!players[j].newBet((uint?)UI.inputI("What will be your new bet?: ", 0, 23) ?? biggestBet))
                                    {
                                        players[j].allIn();
                                    }

                                    UI.print($"Raised to {players[j].bet}$ ", 2, 4, true);


                                    break;

                                case ConsoleKey.W: //fold

                                    UI.print($"{"Max"} folded   ", 2, 4, true);

                                    players[j].fold();

                                    break;


                                case ConsoleKey.Escape:

                                    if (AnsiConsole.Prompt(
                                        new ConfirmationPrompt("Are you people sure?")
                                        )) return 0;

                                    break;
                            }
                        } while (!new[] { ConsoleKey.Spacebar, ConsoleKey.Z, ConsoleKey.X, ConsoleKey.W }.Contains(key));
                    }

                }


                Player[] winners = players.OrderByDescending(p => p.Hand.rank).ToArray();

                Poker.UI.Header.update((int)winners[0].Id, (int)winners[0].balance, winners[0].bet);
                Poker.UI.Table.update(winners[0].hand, communityCards.ToArray());

                Console.SetCursorPosition(2, 4);
                UI.print(winners[0].Hand.rank + " : ");
                UI.printCards(winners[0].Hand.cards);

                Console.Read();

            } while (players.);

            return 0;
        }

        public static uint communityCardsAdd(uint turn)
        {
            deck.first();

            if (turn == 2 || turn == 3) communityCards.Add(deck.first());
            else if (turn == 1)
            {
                communityCards.Add(deck.first());
                communityCards.Add(deck.first());
                communityCards.Add(deck.first());
            }

            return ++turn;
        }
    }




    public class Player
    {
        public Card[] hand { get; private set; } = new Card[2];
        public uint balance { get; private set; }
        public uint bet { get; private set; } = 0;

        static uint id = 1;
        public uint Id { get; private set; }
        public bool folded { get; private set; } = false;
        public (int rank, Card[] cards) Hand;

        public Player(uint balance)
        {
            this.balance = balance;
            Id = id++;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns>Returns false if the player doens't have enough money, otherwise returns true</returns>
        public bool newBet(uint x)
        {
            if (x > balance) return false;

            bet = x;
            return true;
        }

        public void updateHand(Card[] card)
        {
            hand = card;
        }



        public (int rank, Card[] cards) rank(List<Card> communityCards)
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
                if (temp.MaxBy(c => c.value).value == 14)
                {
                    temp = temp.Concat(matchBySuit(temp)).ToArray();

                    if (temp.Length >= 5) return (9, temp);
                }
            }

            //Straight flush
            {
                temp = matchByStraight(cards, 13);
                temp = temp.Concat(matchBySuit(temp)).ToArray();

                if (temp.Length >= 5) return (8, temp);
            }

            //Four of a kind
            {
                temp = matchByValue(cards, 4);

                if (temp.Length >= 4) return (7, temp);
            }

            //Full house
            {
                temp = matchByValue(cards, 3);
                if (temp.Length >= 3) temp = temp.Concat(matchByValue(cards, 2, new[] { temp[0].value })).ToArray();

                if (temp.Length >= 5) return (6, temp);
            }

            //Flush
            {
                temp = matchBySuit(cards);

                if (temp.Length >= 5) return (5, temp);
            }

            //Straight
            {
                temp = matchByStraight(cards);

                if (temp.Length >= 5) return (4, temp);
            }

            //Three of a kind
            {
                temp = matchByValue(cards, 3);

                if (temp.Length >= 3) return (3, temp);
            }

            //Two pairs
            {
                temp = matchByValue(cards, 2);
                if (temp.Length >= 2) temp = temp.Concat(matchByValue(cards, 2, new[] { temp[0].value })).ToArray();

                if (temp.Length >= 4) return (2, temp);
            }

            //Pair
            {
                temp = matchByValue(cards, 2);

                if (temp.Length >= 2) return (1, temp);
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

        Card[] matchByValue(Card[] cards, int lenght, int[]? skip = default)
        {
            string values = string.Join("", cards.Select((c) => c.value.ToString("X")).ToArray());

            for (int i = 14; i > 0; --i)
            {
                if ((skip ?? new[] { 0 }).Contains(i)) continue;
                if (values.Length - lenght >= values.Replace(i.ToString("X"), "").Length) return cards.Where((c) => c.value == i).ToArray()[0..lenght];
            }


            return new Card[0];
        }

        Card[] matchByStraight(Card[] cards, int start = 14)
        {
            char[] values = new string(' ', 15).ToCharArray();
            foreach (Card c in cards)
            {
                values[c.value] = '#';
            }
            int index = string.Join("", values.Reverse()).IndexOf("#####"); if (index < 1) return new Card[1];

            return cards.Where((c) => c.value >= index && c.value <= index + 5).ToArray();

        }

        ~Player()
        {
            --id;
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


        public Deck(int decks = 1)
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

        public static void print(object[] arr)
        {
            foreach (var e in arr)
            {
                Console.Write(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void print(object text, int x = 0, int y = 0, bool sticky = false) // the name sticky comes from CSS, where it is used to make an element fixed or sticked to one position on the screen and not an the body
        {

            Console.SetCursorPosition(x, y);
            Console.Write(text);

            if (sticky && buffer.Contains(((string)text, x, y))) buffer.Remove(((string)text, x, y));
            if (sticky) buffer.Add((text.ToString() ?? "", x, y));
        }

        public static void print(object[] arr, int x = 0, int y = 0, bool sticky = false) // the name sticky comes from CSS, where it is used to make an element fixed or sticked to one position on the screen and not an the body
        {
            foreach (var e in arr)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(e);

                if (sticky && buffer.Contains(((string)e, x, y))) buffer.Remove(((string)e, x, y));
                if (sticky) buffer.Add((e.ToString() ?? "", x, y));

                x += 2;
            }
        }

        public static void printCards(Card[] arr) // the name sticky comes from CSS, where it is used to make an element fixed or sticked to one position on the screen and not an the body
        {
            foreach (var e in arr)
            {

                Console.Write(e.getCard() + ' ');


            }
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




    /*public class MyListNode<T>
    {
        public T data;
        MyListNode<T>? next;
        MyListNode<T>? prev;

        public MyListNode(T data, MyListNode<T>? prev = null, MyListNode<T>? next = null)
        {
            this.data = data;

            this.prev = prev;
            this.next = next;
        }
    }

    public class MyList<T> : IEnumerable<T>
    {
        MyListNode<T> headNode;
        MyListNode<T> endNode;

        public MyList(T[] arr)
        {
            headNode = new MyListNode<T>(arr[0]);

            foreach (T e in arr[1..])
            {

            }
        }

        public void addAt(T data, int index)
        {

        }

        /*public T this[int index]
        {
            get
            {

            }
        }


    }*/


}
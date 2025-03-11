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



        public (int, Card[]) rank(Card [] hand, List<Card> communityCards)
        {

            Card [] cards = new[] { hand[0], hand[1] }.Concat(communityCards).OrderByDescending(cards => cards.value).ToArray();
            (int, Card[]) output;



            /// <para> '-' means don't care </para>
            /// <para> '#' means match for suit </para>
            /// <para> '@' means match for value. Fx. "@@@@" is four of a kind and "@@-@@" is 2 pairs </para>
            /// <para> 's' means match for next value to be one less (straight), if starting with uppercase the output will start with highest card </para>


            //Royal flush
            {
                Card[] temp = cards.Select(cards => { if (new[] { 14, 13, 12, 11, 10 }.Contains(cards.value)) return cards; else return new Card(); }).ToArray();

               // if (    )
                {

                }
            }






            //two pair
            {
                int[] temp = new int[15];
                output = (2, new[] { 0, 0 });

                foreach (Card item in cards)
                {
                    ++temp[item.value];
                }

                for (int i = 14; i >= 1; --i)
                {
                    if (i < 2) break;
                    if (temp[i] > 1) output = (2, new[] { i, i });
                }



                for (int i = 14; i >= 1; --i)
                {
                    if (i < 2) break;
                    if (temp[i] > 1 && i != output.Item2[0]) return (2, output.Item2.Concat( new[] { i, i }).ToArray());
                }
                
            }
            


            //pair
            {
                cards = cards.OrderByDescending(e => e.value).ToArray();

                for (int i = 0; i < 6; ++i)
                {
                    if ( subSequenceMatch("@@", cards) ) return (2, new[] { cards[0 + i], cards[1 + i] });
                }

                
            }
            

            //High card
            {
                cards = cards.OrderByDescending(e => e.value).ToArray();
                return (1, new[] { cards[0] } );
            }

           // sequenceFinder("-----");
        }

        public bool subSequenceMatch(string subSequence, Card[] inputCards, int index = 0)
        {
            if (subSequence[index] != subSequence[index + 1]) return true;

            switch (subSequence[index])
            {
                case '#':
                    if (inputCards[index].suit != inputCards[index + 1].suit) return false;
                    else return subSequenceMatch(subSequence, inputCards, index + 1);

                case '@':
                    if (inputCards[index].value != inputCards[index + 1].value) return false;
                    else return subSequenceMatch(subSequence, inputCards, index + 1);

                case 's':
                    if (inputCards[index].value != inputCards[index + 1].value - 1) return false;

                    return subSequenceMatch(subSequence, inputCards, index + 1);

                default:
                    return false;
                
            }
        }

        public bool sequenceMatcher(string sequence, Card[] inputCards, out List<Card> cards)
        {
            Card[] inputCardsBySuit = inputCards.OrderByDescending(e => e.suit).ToArray();
            Card[] inputCardsByValue = inputCards.OrderByDescending(e => e.value).ToArray();


            int count1 = sequence.Count(e => e == '#');
            bool boolean = true;
            for (int i = 0; i < inputCardsBySuit.Length; ++i)
            {
                foreach (Card card in inputCardsBySuit)
                {
                    
                }
            }
            


            foreach (char ch in sequence)
            {
                if (ch == '#')
                {
                    cards.Add()

                }

                if (ch == '@') { }

                if (ch == 's') { }




                if (ch == '#') { }


            }


            return false;
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
        public bool? sequenceFinder(string sequence, Card[] inputCards )
        {
            if (sequence.Length > 5 || sequence.Length < 1 || inputCards.Length != 7) return null;


            List<Card> outputCards = new List<Card>(5);


            inputCards = inputCards.OrderBy(c => c.value).ToArray();
            Card[,] table = new Card[15,4];


            foreach (Card card in inputCards)
            {
                table[card.value, card.suit] = card;
                if (card.value == 14) table[1, card.suit] = card;
            }




            inputCards = inputCards.OrderByDescending(c => c.value).ToArray();

            List<Card>[] cardObj = new List<Card>[inputCards.Distinct().Count()]; cardObj.Initialize();
            for (int i = 0, j = 0; i < inputCards.Length; ++i)
            {
                cardObj[j].Add(inputCards[i]);

                if (inputCards[i].value != inputCards[i].value) ++j;
            }


            switch(sequence)
            {
                //royal flush
                case "S####":
                case "s####":
                    if (sequence[0] == 'S' && cardObj[0][0].value != 14) return false;

                    if (nextInSequence(cardObj, "sssss")) return nextInSequence(cardObj, "#####");
                    else return false;

                case "@@-@@":
                case "@@@@":
                case "@@@":
                case "@@":
                    if (sequence[2] == '-') 

                    return (sequence[2] == '-') ? nextInSequence(cardObj, sequence) && nextInSequence(cardObj, sequence, 2) : nextInSequence(cardObj, sequence);


                    break;
                
            }





        }



        bool nextInSequence(Card[,] inputCards, string sequence, out List< Card > cards)
        {
            cards = new List<Card>();
            string[] subSequences = sequence.Split('-');
            string temp = "";


            Card[][] inputCardsTransposed = new Card[4][];
            for (int i = 0; i < 15; ++i)
            {
                inputCardsTransposed[4] = new Card[15];
                for (int j = 0; j < 4; ++j)
                {
                    inputCardsTransposed[j][i] = inputCards[i, j];
                }
            }


            foreach (string subSequence in subSequences)
            {

                for (int i = 3; i >= 0; i--)
                {

                    int index = ((inputCardsTransposed[i]
                        .Select((select) => (select.Equals(new Card())) ? '-' : '+').ToString() ?? "")
                        .Reverse().ToString() ?? "")
                        .IndexOf( new string('+', subSequence.Count(ch => ch == '#')) );

                    if (index != 0) break;

                    cards.AddRange(inputCardsTransposed[i][(index..subSequence.Count(ch => ch == '#'))] );

                }

            }



            foreach (char c in "#@s")
            {
                if (sequence.Count(e => e == c) == 1)
                {
                    cards.Add((c == '#') ? inputCardsBySuit[0] : inputCardsByValue[0]);
                    continue;
                }

                for (int j = 1; j < sequence.Count(e => e == sequence[i]); ++j)
                {
                    if (sequence[i] == '#' && inputCardsBySuit[j - 1].suit == inputCardsBySuit[j].suit) cards.Add(inputCardsBySuit[j - 1]);
                    if (sequence[i] == '@' && inputCardsByValue[j - 1].value == inputCardsByValue[j].value) cards.Add(inputCardsByValue[j - 1]);
                    if (sequence[i] == 's' && inputCardsByValue[j - 1].value == inputCardsByValue[j].value - 1) cards.Add(inputCardsByValue[j - 1]);
                }
            }


            return false;
        }



            Card[]? suits(Card[] inputCards, string sequence = "")
        {
            List<Card>[] suit = new List<Card>[4];
            for (int i = 0; i < 4; ++i)
            {
                suit[i] = new List<Card>();
            }

            foreach (Card item in inputCards)
            {
                suit[item.suit].Add(item);
            }

            for (int i = 0; i < 4; ++i)
            {
                if (suit[i].Count == sequence.Count(c => c == '#'))
                {
                    for (int j = 0; j < sequence.Count(c => c == '#'); ++j)
                    {
                        return suit[i].Slice(0, sequence.Count(c => c == '#')).ToArray();
                    }

                }
            }
            return null;
        }

        /*
        Card[]? value(Card[] inputCards, string sequence = "")
        {
            List<Card> outputCards = new List<Card>(5);

            inputCards = inputCards.OrderBy(c => c.value).ToArray();

            




            /*
            List<Card>[] value = new List<Card>[15];
            for (int i = 0; i < 15; ++i)
            {
                value[i] = new List<Card>();
            }

            foreach (Card item in inputCards)
            {
                value[item.value].Add(item);
            }



            
            for (int i = 14; i >= 2; --i)
            {

                if (value[i].Count == sequence.Count(c => c == '@'))
                {
                    for (int j = 0; j < sequence.Count(c => c == '@'); ++j)
                    {
                        outputCards.Add(value[i][j]);
                    }

                    continue;
                }
                else if (value[i].Count == sequence.Count(c => c == '£'))
                {
                    for (int j = 0; j < sequence.Count(c => c == '£'); ++j)
                    {
                        outputCards.Add(value[i][j]);
                    }

                    return outputCards.ToArray();
                }
            
        }


            Card[]? straight(Card[] inputCards, string sequence = "")
            {
                inputCards = inputCards.OrderBy(c => c.value).ToArray();
                List<Card> outputCards = new List<Card>(5);

                for (int i = 0; i < 3; ++i)
                {

                    

                    if (!nextInSequence(inputCards, i)) continue;
                    else return inputCards[0..5];
                    //else return inputCards.Skip(i).Take(5).ToArray();
                    //else return inputCards.Take(new Range(i, i+5)).ToArray();


                }

                return null;
            }

            bool nextInSequence(Card[] c, int index)
            {

                if (c[index].value == c[index + 1].value - 1) return nextInSequence(c, index - 1);
                else return false;
            }
            */

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
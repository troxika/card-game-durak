using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsReceive
{
    public class DeckResponse
    {
        public bool success { get; set; }
        public string deck_id { get; set; }
        public int remaining { get; set; }
    }

    public class DrawResponse
    {
        public bool success { get; set; }
        public string deck_id { get; set; }
        public int remaining { get; set; }
        public Card[] cards { get; set; }
    }

    public enum Suit { Clubs, Diamonds, Hearts, Spades }

    public class Card
    {
        public string value { get; set; }
        public string suit { get; set; }
        public string image { get; set; }
        public string code { get; set; }

        public Suit SuitEnum
        {
            get
            {
                return suit switch
                {
                    "CLUBS" => Suit.Clubs,
                    "DIAMONDS" => Suit.Diamonds,
                    "HEARTS" => Suit.Hearts,
                    "SPADES" => Suit.Spades,
                    _ => throw new Exception("Неизвестная масть"),
                };
            }
        }
    }

    public class Player
    {
        public string Id { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
    }
    public class GameState
    {
        public bool IsPlayerTurn { get; set; }
        public List<Card> TableCards { get; set; } = new List<Card>();
        public List<Card> OpponentCards { get; set; } = new List<Card>();
    }
}

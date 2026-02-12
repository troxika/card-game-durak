using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CardsReceive
{
    internal class DeckService
    {
        private readonly HttpClient _client;

        public DeckService(HttpClient client)
        {
            _client = client;
        }

        public async Task<DeckResponse> CreateDeckAsync()
        {
            var json = await _client.GetStringAsync(
                "https://deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1"
            );

            return JsonSerializer.Deserialize<DeckResponse>(json);
        }

        public async Task<DrawResponse> DrawCardsAsync(string deckId, int count)
        {
            var json = await _client.GetStringAsync(
                $"https://deckofcardsapi.com/api/deck/{deckId}/draw/?count={count}"
            );

            return JsonSerializer.Deserialize<DrawResponse>(json);
        }
    }
}
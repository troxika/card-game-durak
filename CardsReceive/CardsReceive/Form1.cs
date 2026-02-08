using System.Text.Json;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace CardsReceive
{
    public partial class Form1 : Form
    {
        private string currentDeckId = "";
        private static readonly HttpClient client = new HttpClient();
        private int remainingCards = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Создаем новую колоду и перемешиваем
                var json = await client.GetStringAsync(
                    "https://deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1"
                );

                var data = JsonSerializer.Deserialize<DeckResponse>(json);

                currentDeckId = data.deck_id;
                remainingCards = data.remaining;

                // Показываем количество карт сразу
                lblRemainingCards.Text = $"Осталось карт в колоде: {remainingCards}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании колоды: " + ex.Message);
            }
        }

        private async void btnLoadCards_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentDeckId))
            {
                MessageBox.Show("Колода ещё не загружена!");
                return;
            }

            flowLayoutPanelPlayer.Controls.Clear();
            flowLayoutPanelOpponent.Controls.Clear();

            try
            {
                int drawCount = Math.Min(12, remainingCards);

                var json = await client.GetStringAsync(
                    $"https://deckofcardsapi.com/api/deck/{currentDeckId}/draw/?count={drawCount}"
                );

                var data = JsonSerializer.Deserialize<DrawResponse>(json);

                int playerCardsCount = Math.Min(6, data.cards.Length);

                // Карты игрока
                for (int i = 0; i < playerCardsCount; i++)
                {
                    var pb = await CreateCardPictureBoxAsync(data.cards[i].image);
                    flowLayoutPanelPlayer.Controls.Add(pb);
                }

                // Карты противника (рубашкой)
                for (int i = playerCardsCount; i < data.cards.Length; i++)
                {
                    var pb = await CreateCardPictureBoxAsync("https://deckofcardsapi.com/static/img/back.png");
                    flowLayoutPanelOpponent.Controls.Add(pb);
                }

                remainingCards = data.remaining;
                lblRemainingCards.Text = $"Осталось карт в колоде: {remainingCards}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при раздаче карт: " + ex.Message);
            }
        }

        private async Task<PictureBox> CreateCardPictureBoxAsync(string imageUrl)
        {
            PictureBox pb = new PictureBox
            {
                Width = 80,
                Height = 120,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            try
            {
                var data = await client.GetByteArrayAsync(imageUrl);
                using (var ms = new MemoryStream(data))
                {
                    pb.Image = Image.FromStream(ms);
                }
            }
            catch
            {
                // Если картинка не загрузилась, оставляем пустой PictureBox
            }

            return pb;
        }
    }

    // Ответ от shuffle/new
    public class DeckResponse
    {
        public bool success { get; set; }
        public string deck_id { get; set; }
        public int remaining { get; set; }
    }

    // Ответ от draw
    public class DrawResponse
    {
        public bool success { get; set; }
        public string deck_id { get; set; }
        public int remaining { get; set; }
        public Card[] cards { get; set; }
    }

    public class Card
    {
        public string value { get; set; }
        public string suit { get; set; }
        public string image { get; set; }
        public string code { get; set; }
    }
}
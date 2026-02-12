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
        private Image backImage;
        private DeckService deckService;
        public Form1()
        {
            InitializeComponent();
            deckService = new DeckService(client);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var data = await deckService.CreateDeckAsync();

                currentDeckId = data.deck_id;
                remainingCards = data.remaining;

                lblRemainingCards.Text = $"Осталось карт в колоде: {remainingCards}";

                var backBytes = await client.GetByteArrayAsync(
                    "https://deckofcardsapi.com/static/img/back.png"
                );

                using (var ms = new MemoryStream(backBytes))
                {
                    backImage = Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании колоды: " + ex.Message);
            }
        }


        private async void btnLoadCards_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentDeckId) || remainingCards < 12)
            {
                MessageBox.Show("Недостаточно карт!");
                return;
            }

            btnLoadCards.Enabled = false;

            ClearPanel(flowLayoutPanelPlayer);
            ClearPanel(flowLayoutPanelOpponent);

            try
            {
                var data = await deckService.DrawCardsAsync(currentDeckId, 12);

                // Игрок (параллельно)
                var playerTasks = data.cards
                    .Take(6)
                    .Select(c => CreateCardPictureBoxAsync(c.image))
                    .ToList();

                var playerCards = await Task.WhenAll(playerTasks);

                foreach (var pb in playerCards)
                    flowLayoutPanelPlayer.Controls.Add(pb);

                // Оппонент (рубашка)
                for (int i = 0; i < 6; i++)
                {
                    PictureBox pb = new PictureBox
                    {
                        Width = 80,
                        Height = 120,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = backImage,
                        Margin = new Padding(0)
                    };

                    flowLayoutPanelOpponent.Controls.Add(pb);
                }

                remainingCards = data.remaining;
                lblRemainingCards.Text = $"Осталось карт в колоде: {remainingCards}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            finally
            {
                btnLoadCards.Enabled = true;
            }
        }



        private async Task<PictureBox> CreateCardPictureBoxAsync(string imageUrl)
        {
            PictureBox pb = new PictureBox
            {
                Width = 80,
                Height = 120,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Margin = new Padding(0)
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

            }

            return pb;
        }
        private void ClearPanel(FlowLayoutPanel panel)
        {
            panel.Controls.Clear();
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
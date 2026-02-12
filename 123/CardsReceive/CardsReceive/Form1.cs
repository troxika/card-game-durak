using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace CardsReceive
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        private StreamWriter writer;
        private StreamReader reader;
        private string playerId;
        private bool isMyTurn = true; // Для теста можно начать с true
        private PictureBox selectedPictureBox = null;
        private Card selectedCard = null;

        private static readonly HttpClient httpClient = new HttpClient();
        private Image backImage;

        private Player player = new Player();
        private Player opponent = new Player();
        private List<PictureBox> tableCards = new List<PictureBox>();
        private Dictionary<Card, PictureBox> opponentCardMap = new Dictionary<Card, PictureBox>();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Загружаем рубашку карты
                var backBytes = await httpClient.GetByteArrayAsync("https://deckofcardsapi.com/static/img/back.png");
                using (var ms = new MemoryStream(backBytes))
                {
                    backImage = Image.FromStream(ms);
                }

                // Подключаемся к TCP серверу
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 27015); // IP и порт сервера
                NetworkStream ns = tcpClient.GetStream();
                reader = new StreamReader(ns, Encoding.UTF8);
                writer = new StreamWriter(ns, Encoding.UTF8) { AutoFlush = true };

                // Отправляем имя игрока
                playerId = Guid.NewGuid().ToString();
                await writer.WriteLineAsync(playerId);

                // Запускаем получение сообщений от сервера
                _ = Task.Run(ReceiveMessagesFromServerAsync);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к серверу: " + ex.Message);
            }
        }

        private async void btnLoadCards_Click(object sender, EventArgs e)
        {
            if (writer != null)
            {
                await writer.WriteLineAsync("getcard()");
            }
        }

        private async Task ReceiveMessagesFromServerAsync()
        {
            try
            {
                while (true)
                {
                    string line = await reader.ReadLineAsync();
                    if (line == null) break;

                    // Пример строки: CARDS:["AS","2H","JD"]
                    if (line.StartsWith("CARDS:"))
                    {
                        string json = line.Substring(6);
                        string[] codes = JsonSerializer.Deserialize<string[]>(json);

                        foreach (var code in codes)
                        {
                            // Получаем карту с сервера (можно запросить образ карты)
                            // Для примера создаём Card с заглушкой
                            Card c = new Card
                            {
                                code = code,
                                image = $"https://deckofcardsapi.com/static/img/{code}.png"
                            };
                            player.Cards.Add(c);
                        }

                        // Обновляем UI
                        Invoke(() => UpdatePlayerCards());
                    }
                    else
                    {
                        // любые другие сообщения
                        Console.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении сообщений: " + ex.Message);
            }
        }

        private void UpdatePlayerCards()
        {
            flowLayoutPanelPlayer.Controls.Clear();
            foreach (var card in player.Cards)
            {
                var pb = new PictureBox
                {
                    Width = 80,
                    Height = 120,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Margin = new Padding(5),
                    Tag = card
                };
                pb.Click += PlayerCard_Click;

                // Загружаем изображение асинхронно
                _ = LoadCardImageAsync(pb, card.image);

                flowLayoutPanelPlayer.Controls.Add(pb);
            }
        }

        private async Task LoadCardImageAsync(PictureBox pb, string url)
        {
            try
            {
                var data = await httpClient.GetByteArrayAsync(url);
                using var ms = new MemoryStream(data);
                pb.Image = Image.FromStream(ms);
            }
            catch
            {
                pb.Image = backImage;
            }
        }

        private void PlayerCard_Click(object sender, EventArgs e)
        {
            if (!isMyTurn)
            {
                MessageBox.Show("Сейчас не ваш ход!");
                return;
            }

            foreach (PictureBox pb in flowLayoutPanelPlayer.Controls)
                pb.BorderStyle = BorderStyle.None;

            selectedPictureBox = sender as PictureBox;
            selectedCard = selectedPictureBox?.Tag as Card;

            if (selectedPictureBox != null)
                selectedPictureBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private async void btnPlayCard_Click(object sender, EventArgs e)
        {
            if (!isMyTurn)
            {
                MessageBox.Show("Сейчас не ваш ход!");
                return;
            }

            if (selectedCard == null)
            {
                MessageBox.Show("Выберите карту!");
                return;
            }

            try
            {
                await writer.WriteLineAsync($"PLAY:{selectedCard.code}");
                player.Cards.Remove(selectedCard);
                selectedCard = null;
                selectedPictureBox = null;
                UpdatePlayerCards();
                isMyTurn = false;
                btnPlayCard.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке хода: " + ex.Message);
            }
        }
    }
}

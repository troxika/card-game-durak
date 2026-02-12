using System.Text.Json;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Numerics;
using static CardsReceive.Form1;
using System.Net.Http.Json;

namespace CardsReceive
{

    public partial class Form1 : Form
    {
        private bool isMyTurn = false;
        private PictureBox selectedPictureBox = null;
        private string currentDeckId = "";
        private static readonly HttpClient client = new HttpClient();
        private int remainingCards = 0;
        private Image backImage;
        private DeckService deckService;
        private List<PictureBox> tableCards = new List<PictureBox>();
        private Card selectedCard = null;
        private Player player = new Player();
        private Player opponent = new Player();
        private Suit trumpSuit;
        private Dictionary<Card, PictureBox> opponentCardMap = new Dictionary<Card, PictureBox>();

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
            if (string.IsNullOrEmpty(currentDeckId)) return;
            if (remainingCards == 0) { MessageBox.Show("Карт нет"); return; }

            btnLoadCards.Enabled = false;

            // Очистка панелей
            ClearPanel(flowLayoutPanelPlayer);
            ClearPanel(flowLayoutPanelOpponent);
            ClearPanel(flowLayoutPanelTable);
            tableCards.Clear();
            player.Cards.Clear();
            opponent.Cards.Clear();
            opponentCardMap.Clear();

            try
            {
                int drawCount = Math.Min(12, remainingCards);
                var data = await deckService.DrawCardsAsync(currentDeckId, drawCount);

                // Козырь — последняя карта в колоде
                trumpSuit = data.cards.Last().SuitEnum;

                int playerCount = drawCount / 2;

                // Раздаём игроку
                for (int i = 0; i < playerCount; i++)
                {
                    var card = data.cards[i];
                    player.Cards.Add(card);

                    var pb = await CreateCardPictureBoxAsync(card.image);
                    pb.Tag = card;
                    pb.Click += PlayerCard_Click;

                    flowLayoutPanelPlayer.Controls.Add(pb);
                }

                // Раздаём оппоненту (рубашка)
                for (int i = playerCount; i < drawCount; i++)
                {
                    var card = data.cards[i];
                    opponent.Cards.Add(card);

                    PictureBox pb = new PictureBox
                    {
                        Width = 80,
                        Height = 120,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = backImage,
                        Margin = new Padding(5)
                    };

                    flowLayoutPanelOpponent.Controls.Add(pb);
                    opponentCardMap[card] = pb; // важная строка!
                }

                remainingCards = data.remaining;
                lblRemainingCards.Text = $"Осталось карт: {remainingCards}";
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
        private void PlayerCard_Click(object sender, EventArgs e)
        {
            if (!isMyTurn)
            {
                MessageBox.Show("Сейчас не ваш ход!");
                return;
            }

            // Снимаем выделение с предыдущей карты
            foreach (PictureBox pb in flowLayoutPanelPlayer.Controls)
                pb.BorderStyle = BorderStyle.None;

            selectedPictureBox = sender as PictureBox;
            selectedCard = selectedPictureBox?.Tag as Card;

            // Подсвечиваем выбранную карту
            if (selectedPictureBox != null)
                selectedPictureBox.BorderStyle = BorderStyle.FixedSingle;
        }
        private async Task OpponentTurn(Card attackCard)
        {
            Card defendCard = opponent.Cards.FirstOrDefault(c => CanBeat(attackCard, c));

            if (defendCard != null)
            {
                opponent.Cards.Remove(defendCard);

                // Получаем PictureBox карты оппонента
                var pb = opponentCardMap[defendCard];
                flowLayoutPanelOpponent.Controls.Remove(pb);
                opponentCardMap.Remove(defendCard);

                // Загружаем лицевую карту
                PictureBox newPb = new PictureBox
                {
                    Width = 80,
                    Height = 120,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Margin = new Padding(5),
                    BorderStyle = BorderStyle.FixedSingle
                };

                try
                {
                    var data = await client.GetByteArrayAsync(defendCard.image);
                    using (var ms = new MemoryStream(data))
                    {
                        newPb.Image = Image.FromStream(ms);
                    }
                }
                catch
                {
                    newPb.Image = backImage;
                }

                flowLayoutPanelTable.Controls.Add(newPb);
                tableCards.Add(newPb);
            }
            else
            {
                // Оппонент не может отбить → забирает все карты со стола
                foreach (var pb in tableCards)
                {
                    flowLayoutPanelOpponent.Controls.Add(pb);
                }
                tableCards.Clear();
            }
        }
        private bool CanBeat(Card attack, Card defend)
        {
            if (attack.SuitEnum == defend.SuitEnum)
                return ValueRank(defend.value) > ValueRank(attack.value);
            else
                return defend.SuitEnum == trumpSuit;
        }

        private int ValueRank(string value)
        {
            string[] order = { "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            int index = Array.IndexOf(order, value);
            return index >= 0 ? index + 1 : 0;
        }
        private void ClearPanel(FlowLayoutPanel panel)
        {
            panel.Controls.Clear();
        }

        private async void btnPlayCard_Click(object sender, EventArgs e)
        {
            // Проверка хода
            if (!isMyTurn)
            {
                MessageBox.Show("Сейчас не ваш ход!");
                return;
            }

            // Проверка выбора карты
            if (selectedCard == null || selectedPictureBox == null)
            {
                MessageBox.Show("Выберите карту для хода!");
                return;
            }

            // Подготовка данных для сервера
            var moveData = new
            {
                playerId = player.Id,      // теперь player.Id есть
                cardCode = selectedCard.code
            };

            try
            {
                // Отправка хода на сервер
                var response = await client.PostAsJsonAsync("https://server.example.com/api/play", moveData);
                response.EnsureSuccessStatusCode();

                // После успешного ответа сервера — выкладываем карту на стол
                flowLayoutPanelPlayer.Controls.Remove(selectedPictureBox);
                player.Cards.Remove(selectedCard);

                flowLayoutPanelTable.Controls.Add(selectedPictureBox);
                selectedPictureBox.Margin = new Padding(5);
                tableCards.Add(selectedPictureBox);

                // Снимаем выделение
                selectedPictureBox.BorderStyle = BorderStyle.None;
                selectedCard = null;
                selectedPictureBox = null;

                // Блокируем кнопку до следующего хода
                isMyTurn = false;
                btnPlayCard.Enabled = false;

                // Опционально: можно запросить обновление состояния игры с сервера
                await UpdateGameStateFromServerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке хода: " + ex.Message);
            }
        }

        // Пример метода для обновления состояния игры с сервера
        private async Task UpdateGameStateFromServerAsync()
        {
            try
            {
                var response = await client.GetStringAsync("https://server.example.com/api/state?playerId=" + player.Id);
                var gameState = JsonSerializer.Deserialize<GameState>(response);

                // Обновляем стол
                flowLayoutPanelTable.Controls.Clear();
                tableCards.Clear();
                foreach (var card in gameState.TableCards)
                {
                    PictureBox pb = await CreateCardPictureBoxAsync(card.image);
                    pb.Tag = card;
                    tableCards.Add(pb);
                    flowLayoutPanelTable.Controls.Add(pb);
                }

                // Обновляем карты противника
                flowLayoutPanelOpponent.Controls.Clear();
                opponentCardMap.Clear();
                foreach (var card in gameState.OpponentCards)
                {
                    PictureBox pb = new PictureBox
                    {
                        Width = 80,
                        Height = 120,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Image = backImage,
                        Margin = new Padding(5)
                    };
                    opponentCardMap[card] = pb;
                    flowLayoutPanelOpponent.Controls.Add(pb);
                }

                // Проверяем чей ход
                isMyTurn = gameState.IsPlayerTurn;
                btnPlayCard.Enabled = isMyTurn;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении состояния игры: " + ex.Message);
            }
        }
        private async Task SendMoveToServerAsync(Card card)
        {
            try
            {
                // Пример JSON, который отправляем серверу
                var moveData = new
                {
                    playerId = player.Id,
                    cardCode = card.code
                };

                var json = JsonSerializer.Serialize(moveData);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://server.example.com/move", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();

                // Сервер вернул обновления
                await ProcessServerResponse(responseJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отправке хода: " + ex.Message);
            }
        }
        private async Task ProcessServerResponse(string json)
        {
            var gameState = JsonSerializer.Deserialize<GameState>(json);

            // Обновляем стол
            flowLayoutPanelTable.Controls.Clear();
            tableCards.Clear();
            foreach (var card in gameState.TableCards)
            {
                PictureBox pb = await CreateCardPictureBoxAsync(card.image);
                pb.Tag = card;
                tableCards.Add(pb);
                flowLayoutPanelTable.Controls.Add(pb);
            }

            // Обновляем карты противника
            flowLayoutPanelOpponent.Controls.Clear();
            opponentCardMap.Clear();
            foreach (var card in gameState.OpponentCards)
            {
                PictureBox pb = new PictureBox
                {
                    Width = 80,
                    Height = 120,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = backImage,
                    Margin = new Padding(5)
                };
                opponentCardMap[card] = pb;
                flowLayoutPanelOpponent.Controls.Add(pb);
            }

            // Ход игрока
            isMyTurn = gameState.IsPlayerTurn;
            btnPlayCard.Enabled = isMyTurn;
        }
        private void UpdateTurn(string currentPlayerId)
        {
            isMyTurn = player.Id == currentPlayerId;
            btnPlayCard.Enabled = isMyTurn; // включаем/выключаем кнопку
        }
    }
}
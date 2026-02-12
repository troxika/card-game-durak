using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Reflection.PortableExecutable;
// Создаем и запускаем сервер

ServerObject server = new ServerObject();
await server.ListenAsync();
//класс который содержит колоду
public class deck
{
    public bool success { get; set; } // создалась ли колода
    public string deck_id { get; set; } // id колоды
    public card[] cards { get; set; } // массив обьектов карт, описан ниже
    public int remaining {  get; set; } // сколько карт осталось
}
// массив карт
public class card
{
    public string code { get; set; } // код карты
    public string image { get; set; } // ссылка на изображение карт
    public string value { get; set; } // чифровое значение карты ( цифра на карте)
    public string suit {  get; set; } // масть 
}

class ServerObject
{
    TcpListener tcpListener = new TcpListener(IPAddress.Any, 27015);
    List<ClientObject> clients = new List<ClientObject>();
    private readonly object clientsLock = new object();

    // 👇 Текущий игрок (храним его Id)
    public string CurrentPlayerId { get; set; }

    protected internal void RemoveConnection(string id)
    {
        lock (clientsLock)
        {
            ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                // Если уходит текущий игрок – переключаем ход
                if (CurrentPlayerId == id)
                    SwitchTurn();

                clients.Remove(client);
                client?.Close();
            }
        }
    }

    protected internal async Task ListenAsync()
    {
        try
        {
            Console.WriteLine("Введите количество игроков");
            int playercount = 2; // здесь можно заменить на int.Parse(Console.ReadLine())

            tcpListener.Start();
            Console.WriteLine("Комната создана. Ожидание подключений...");

            for (int i = 0; i < playercount; i++)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                ClientObject clientObject = new ClientObject(tcpClient, this);

                lock (clientsLock)
                {
                    clients.Add(clientObject);
                }
                Console.WriteLine($"Новое подключение: {clientObject.Id}");
            }

            // Все игроки подключены
            Console.WriteLine("Все игроки подключены. Запуск обработки...");

            // Назначаем первого игрока текущим
            if (clients.Count > 0)
                CurrentPlayerId = clients[0].Id;
            Console.WriteLine($"Текущий игрок{CurrentPlayerId}");

            // Запускаем обработку каждого клиента
            foreach (var client in clients)
            {
                _ = Task.Run(client.ProcessAsync);
            }

            // Оповещаем о начале игры и текущем игроке
            Console.WriteLine("начало игры");
            // Бесконечное ожидание, чтобы сервер не завершался
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в ListenAsync: {ex}");
        }
    }

    // Переключение хода на следующего игрока (круговой порядок)
    protected internal void SwitchTurn()
    {
        lock (clientsLock)
        {
            if (clients.Count == 0)
            {
                CurrentPlayerId = null;
                return;
            }

            int currentIndex = clients.FindIndex(c => c.Id == CurrentPlayerId);
            int nextIndex = (currentIndex + 1) % clients.Count;
            CurrentPlayerId = clients[nextIndex].Id;
        } 
    }

    // Оповещение всех о том, кто сейчас ходит
    

    // Рассылка сообщения всем 
    
    protected internal async Task BroadcastMessageAsync(string message, string? excludeId)
    {
        List<ClientObject> clientsCopy;
        lock (clientsLock)
        {
            clientsCopy = new List<ClientObject>(clients);
        }

        foreach (var client in clientsCopy)
        {
            if (client.Id != excludeId)
            {
                try
                {
                    await client.Writer.WriteLineAsync(message);
                    await client.Writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки клиенту {client.Id}: {ex.Message}");
                }
            }
        }
    }

    // Трансляция сообщения подключенным всем клиентам подключенным к серверу ( убрать или переделать )
    

    // Отключение всех клиентов
    protected internal void Disconnect()
    {
        Console.WriteLine("Отключение всех клиентов...");

        List<ClientObject> clientsCopy;
        lock (clientsLock)
        {
            clientsCopy = new List<ClientObject>(clients);
            clients.Clear();
        }

        foreach (var client in clientsCopy)
        {
            try
            {
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении клиента: {ex.Message}");
            }
        }

        tcpListener.Stop();
        Console.WriteLine("Сервер остановлен");
    }

}
// обьект клиента со всеми данными о клиенте
class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get; }
    protected internal StreamReader Reader { get; }
    private TcpClient client;
    private ServerObject server;

    // 👇 Имя пользователя (теперь доступно для чтения извне)
    public string UserName { get; private set; }

    HttpClient htclient = new HttpClient();
    deck de = null;
    public List<card> Hand { get; set; } = new List<card>();
    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;

        var stream = client.GetStream();
        Reader = new StreamReader(stream, Encoding.UTF8);
        Writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
    }
    public async Task GiveCardToClient()
    {
        // Проверяем, создана ли колода на сервере
        if (de == null)
        {
            Console.WriteLine($"[{Id}] Ошибка: колода не инициализирована.");
            return;
        }

        // Сколько карт не хватает до 6
        int need = 6 - Hand.Count;
        if (need > 0 && de.remaining > 0)
        {
            int take = Math.Min(need, de.remaining);

            using (HttpClient http = new HttpClient())
            {
                string url = $"https://deckofcardsapi.com/api/deck/{de.deck_id}/draw/?count={take}";
                string response = await http.GetStringAsync(url);
                var draw = JsonSerializer.Deserialize<deck>(response);

                // Добавляем карты в руку текущего клиента
                Hand.AddRange(draw.cards);

                // Обновляем остаток колоды на сервере
                de.remaining = draw.remaining;

                // Отправляем клиенту коды полученных карт (для отображения)
                var codes = draw.cards.Select(c => c.code).ToList();
                string json = JsonSerializer.Serialize(codes);
                await Writer.WriteLineAsync($"CARDS:{json}");
                await Writer.FlushAsync();

                Console.WriteLine($"Игроку {UserName} выдано {take} карт. Осталось в колоде: {de.remaining}");
            }
        }
    }
    public async Task ProcessAsync()
    {
        try
        {
            createnewdeck();

            // 1. Получаем имя пользователя (первое сообщение от клиента)
            UserName = await Reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(UserName))
                UserName = $"Игрок_{Id.Substring(0, 5)}";

            // 2. Оповещаем всех о входе
            string joinMessage = $"{UserName} вошёл в игру";
            cardWork();
            //await server.BroadcastMessageAsync(joinMessage, Id);
            Console.WriteLine($"[{Id}] {joinMessage}");

            // 3. Основной цикл обработки сообщений
            while (true)
            {
                try
                {
                    string? clientMessage = await Reader.ReadLineAsync();
                    if (clientMessage == null)
                        break; // клиент отключился

                    // только текущий игрок может действовать
                    if (server.CurrentPlayerId != Id)
                    {
                        await Writer.WriteLineAsync("❌ Сейчас не ваш ход. Ожидайте.");
                        continue;
                    }
                    if (clientMessage == "getcard()")
                    {
                        GiveCardToClient();
                    }
                    else
                    {

                    }
                }
                catch (IOException)
                {
                    break; // разрыв соединения
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки сообщения от {Id}: {ex.Message}");
                    // По желанию: отправляем клиенту сообщение об ошибке
                    await Writer.WriteLineAsync($"Ошибка сервера: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка в ProcessAsync для клиента {Id}: {e.Message}");
        }
        finally
        {
            // Отключение клиента (обработка выхода уже есть в RemoveConnection)
            if (!string.IsNullOrEmpty(UserName))
            {
                string leaveMessage = $"{UserName} покинул игру";
                Console.WriteLine(leaveMessage);
                await server.BroadcastMessageAsync(leaveMessage, Id);
            }
            server.RemoveConnection(Id);
        }
    }

    async Task cardWork()
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {

            int count = 6;

            //using HttpResponseMessage response = await client.GetAsync("https://deckofcardsapi.com/api/deck//draw/?count=2");
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below

            string responseBody = await htclient.GetStringAsync($"https://deckofcardsapi.com/api/deck/{de.deck_id}/draw/?count={count}"); // цифра после count отвечает за количество карт, которые возьмет и запишет сервер
            Console.WriteLine("Подключено успешно");
            //Console.WriteLine(responseBody); // вывод всего запроса json
            de = JsonSerializer.Deserialize<deck>(responseBody); // запись в массивы данных из запроса

            for (int i = 0; i < count; i++)
            {
                Writer.WriteLine(de.cards[i].code, de.cards[i].value, de.cards[i].suit, de.cards[i].image);
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
    async Task createnewdeck()
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            //using HttpResponseMessage response = await client.GetAsync("https://deckofcardsapi.com/api/deck/new/draw/?count=2");
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            string responseBody = await htclient.GetStringAsync("https://deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1"); // цифра после count отвечает за количество карт, которые возьмет и запишет сервер
            //Console.WriteLine(responseBody); // вывод всего запроса json
            de = JsonSerializer.Deserialize<deck>(responseBody); // запись в массивы данных из запроса
            Console.WriteLine(de.deck_id);
            //Console.WriteLine($"{de.cards[1].value}"); // пример вывода значения карты в консоль
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
    protected internal void Close()
    {
        try
        {
            Writer?.Close();
        }
        catch { }

        try
        {
            Reader?.Close();
        }
        catch { }

        try
        {
            client?.Close();
        }
        catch { }
    }
}

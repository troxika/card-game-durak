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
class deck
{
    public bool success { get; set; } // создалась ли колода
    public string deck_id { get; set; } // id колоды
    public card[] cards { get; set; } // массив обьектов карт, описан ниже
    public int remaining {  get; set; } // сколько карт осталось
}
// массив карт
class card
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
    HttpClient client = new HttpClient();
    // Для потокобезопасной работы со списком клиентов
    private readonly object clientsLock = new object();
    // подключение к api и занесение данныз из api в массивы deck и card
    async Task createnewdeck(deck de)
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            //using HttpResponseMessage response = await client.GetAsync("https://deckofcardsapi.com/api/deck/new/draw/?count=2");
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            string responseBody = await client.GetStringAsync("https://deckofcardsapi.com/api/deck/new/draw/?count=0"); // цифра после count отвечает за количество карт, которые возьмет и запишет сервер
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
    async Task cardWork(deck de)
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            int count = 6;
            //using HttpResponseMessage response = await client.GetAsync("https://deckofcardsapi.com/api/deck//draw/?count=2");
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            Console.WriteLine(de.deck_id);
            string responseBody = await client.GetStringAsync($"https://deckofcardsapi.com/api/deck/{de.deck_id}/draw/?count={count}"); // цифра после count отвечает за количество карт, которые возьмет и запишет сервер
            Console.WriteLine("Подключено успешно");
            //Console.WriteLine(responseBody); // вывод всего запроса json
            de = JsonSerializer.Deserialize<deck>(responseBody); // запись в массивы данных из запроса

            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(de.cards[i].code);
                Console.WriteLine(de.cards[i].value);
                Console.WriteLine(de.cards[i].suit);
                Console.WriteLine(de.cards[i].image);
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }
    protected internal void RemoveConnection(string id)
    {
        lock (clientsLock)
        {
            ClientObject? client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                clients.Remove(client);
                client?.Close();
            }
        }
    }
    // запуск сервера и создание колоды карт
    protected internal async Task ListenAsync()
    {
        try
        {
            deck? de = null;
            await createnewdeck(de); // создаем колоду
            tcpListener.Start(); // запускаем слушание у сервера
            Console.WriteLine("комната создана. Ожидание подключений...");

            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                ClientObject clientObject = new ClientObject(tcpClient, this); // запись клиента в массив с клиентами в будующем для хранения имен
                lock (clientsLock)
                {
                    clients.Add(clientObject);
                }

                Console.WriteLine($"Новое подключение: {clientObject.Id}");
                cardWork(de);
                Console.WriteLine(de.deck_id);
                _ = Task.Run(() => clientObject.ProcessAsync());
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в ListenAsync: {ex.Message}");
        }
        finally
        {
            Disconnect();
        }
    }

    // Трансляция сообщения подключенным всем клиентам подключенным к серверу ( убрать или переделать )
    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        List<ClientObject> clientsCopy;
        lock (clientsLock)
        {
            clientsCopy = new List<ClientObject>(clients);
        }

        foreach (var client in clientsCopy)
        {
            if (client.Id != id) // если id клиента не равно id отправителя
            {
                try
                {
                    await client.Writer.WriteLineAsync(message);  // отправка сообщения всем клиентам
                    await client.Writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки клиенту {client.Id}: {ex.Message}");
                }
            }
        }
    }

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
    protected internal StreamWriter Writer { get; } // отправка сообщения клиенту
    protected internal StreamReader Reader { get; } // чтение соо от клиента

    private TcpClient client;
    private ServerObject server;
    private string? userName;
    private static readonly object fileLock = new object();

    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;

        var stream = client.GetStream();
        Reader = new StreamReader(stream, Encoding.UTF8);
        Writer = new StreamWriter(stream, Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    public async Task ProcessAsync()
    {
        try
        {
            // Получаем имя пользователя
            userName = await Reader.ReadLineAsync();
            string message = $"{userName} вошел в комнату";

            // Посылаем сообщение о входе в чат всем подключенным пользователям
            await server.BroadcastMessageAsync(message, Id);
            Console.WriteLine(message);

            // В бесконечном цикле получаем сообщения от клиента
            while (true)
            {
                try
                {
                    string? clientMessage = await Reader.ReadLineAsync();

                    if (clientMessage == null)
                    {
                        // Клиент отключился
                        break;
                    }
                }
                catch (IOException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки сообщения: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка в ProcessAsync для клиента {Id}: {e.Message}");
        }
        finally
        {
            // При выходе из цикла отправляем сообщение о выходе
            if (!string.IsNullOrEmpty(userName))
            {
                string leaveMessage = $"{userName} покинул чат";
                Console.WriteLine(leaveMessage);

                try
                {
                    await server.BroadcastMessageAsync(leaveMessage, Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось отправить сообщение о выходе: {ex.Message}");
                }
            }
            server.RemoveConnection(Id);
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

using Scada.Comm.Config;
using Scada.Comm.Devices;
using Scada.Comm.Drivers.DrvRodosBu.Config;
using Scada.Data.Models;
using Scada.Lang;
using ScadaCommFunc;
using System.Buffers.Text;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace Scada.Comm.Drivers.DrvRodosBu.Logic
{
    internal class DevRodosBuLogic : DeviceLogic
    {
        #region Values
        private readonly Stopwatch stopwatch;      // measures the time of operations
        private bool fileyes = false;   // При отсутствии файла не выполнять опрос
        private string fileName = "";
        private string filePath = "";
        private RodosOptions options;          // the tester options
        private readonly NotifDeviceConfig config; // the device configuration
        private HttpClient httpClient;             // sends HTTP requests
        private const int ResponseDisplayLenght = 50;
        private string content = "1";
        private string contentResponse;
        private Header auth = new Header();
        private string Response;

        private DevTemplate devTemplate = new DevTemplate();
        private response devString = new response();
        private Dictionary<int, string> ActiveValue = new Dictionary<int, string>(); // Словарь переменных - Index, TagName (имя тега должно быть уникально, индекс соответствует индексу переменной и идет последовательно от 0)
        private string password;
        private string login;

        #endregion Values

        #region Option Line
        private class OptionLine
        {
            public bool log = false;
            public string ResponseStatus;

            public override string ToString()
            {
                return $"Response Status: {ResponseStatus}";
            }
        }

        private OptionLine opLine = new OptionLine();
        private OptionLine GetOptionLine()
        {
            if (!LineContext.SharedData.ContainsKey(LineContext.CommLineNum.ToString()))
            {
                LineContext.SharedData.Add(LineContext.CommLineNum.ToString(), opLine);
            }
            else
            {
                opLine = LineContext.SharedData[LineContext.CommLineNum.ToString()] as OptionLine;
            }
            return opLine;
        }
        #endregion Option Line

        static DevRodosBuLogic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DevRodosBuLogic(ICommContext commContext, ILineContext lineContext, DeviceConfig deviceConfig)
            : base(commContext, lineContext, deviceConfig)
        {
            CanSendCommands = true;
            ConnectionRequired = false;

            stopwatch = new Stopwatch();
            config = new NotifDeviceConfig();
            httpClient = null;
        }

        #region InitDeviceTag
        public override void InitDeviceTags()
        {
            if (devTemplate != null)
            {
                TagGroup tagGroup = new TagGroup($"Состояние реле {DeviceConfig.StrAddress}"); // Имя группы строковой адрес

                foreach (var channels in ActiveValue)
                {
                    DeviceTag deviceTag = tagGroup.AddTag(channels.Value, devTemplate.Value[channels.Key].name);
                    deviceTag.SetFormat(TagFormat.OffOn);
                }
                DeviceTags.AddGroup(tagGroup);
            }
        }
        #endregion InitDeviceTag

        #region OnCommLineStart
        /// <summary>
        /// Выполнить действия при запуске линии связи
        /// </summary>
        public override void OnCommLineStart()
        {
            devTemplate = null;

            fileName = DeviceConfig.PollingOptions.CmdLine == null ? "" : DeviceConfig.PollingOptions.CmdLine.Trim();
            filePath = AppDirs.ConfigDir + fileName;
            options = new RodosOptions(DeviceConfig.PollingOptions.CustomOptions);
            try
            {
                password = ScadaUtils.Decrypt(options.Password);
                login = options.Login;
            }
            catch { }

            GetOptionLine(); // Если используем опции Линии

            if (fileName == "") // Чтение файла шаблона
            {
                Log.WriteLine(string.Format(Locale.IsRussian ?
                    "Ошибка: Не задан шаблон устройства для {0}" :
                    "Error: Template is undefined for the {0}", Title));
            } // Чтение файла шаблона
            else
            {
                try
                {
                    devTemplate = FileFunc.LoadXml(typeof(DevTemplate), filePath) as DevTemplate;
                    fileyes = true;
                }
                catch (Exception err)
                {
                    Log.WriteLine(string.Format(Locale.IsRussian ?
                    "Ошибка: " + err.Message :
                    "Error: " + err.Message, Title));
                }
            }

            if (fileyes)
            {
                Header header = new Header();
                header.Name = "Accept";

                if (devTemplate.HttpRequest.Count > 0)
                {
                    header.Value = string.IsNullOrEmpty(devTemplate.HttpRequest[0].Header) ? "text/html,application/xhtml+xml,application/xml" : devTemplate.HttpRequest[0].Header;
                    config.ContentType = string.IsNullOrEmpty(devTemplate.HttpRequest[0].ContentType) ? "application/xml" : devTemplate.HttpRequest[0].ContentType;
                }
                else
                {
                    header.Value = "text/html, application/xml"; // */* text/html,application/xhtml+xml,application/xml
                    config.ContentType = "application/xml";
                }
                config.Headers.Add(header);

                // TEST TEST
                //header = new Header();
                //header.Name = "Accept-Encoding";
                //header.Value = "gzip, deflate";
                //config.Headers.Add(header);

                header = new Header();
                header.Name = "Authorization";

                byte[] bs64 = Encoding.UTF8.GetBytes($"{login}:{password}");
                string pass = Convert.ToBase64String(bs64);

                header.Value = $"Basic {pass}";
                config.Headers.Add(header);

                // Добавление словарей активных устройств и тегов из прочитанного шаблона
                GetDictParameter();
            }
        }
        #endregion OnCommLineStart

        #region Session
        /// <summary>
        /// Performs a communication session.
        /// </summary>
        public override void Session()
        {
            base.Session();
            //Stopwatch stopwatch = Stopwatch.StartNew();
            LastRequestOK = false;

            //Проверка наличия файла конфигурации
            if (!fileyes)        // Если конфигурация не была загружена, выставляем все теги в невалидное состояние и выходим
            {
                DeviceData.Invalidate();
                return;
            }

            // Проверка наличия файла конфигурации
            config.Headers.Remove(auth);
            config.Method = RequestMethod.Post;

            if (devTemplate.HttpRequest.Count > 0)
            {
                config.Uri = string.IsNullOrEmpty(devTemplate.HttpRequest[0].Uri) ? $"http://{DeviceConfig.StrAddress}/pstat.xml" : devTemplate.HttpRequest[0].Uri;
                config.Content = string.IsNullOrEmpty(devTemplate.HttpRequest[0].Content) ? content : devTemplate.HttpRequest[0].Content;
            }
            else
            {
                config.Uri = $"http://{DeviceConfig.StrAddress}/pstat.xml"; // $"http://admin:{password}@{DeviceConfig.StrAddress}/pstat.xml"
                config.Content = content;
            }

            Request();

            if (LastRequestOK)
            {
                if (Open(contentResponse, out string logText))
                {
                    // Выборка из полученного xml значений в массив
                    int[] myArray = _rlProperties
                        .Select(p => (int)p.GetValue(devString))
                        .ToArray();

                    for (int i = 0; i < myArray.Length; i++ )
                    {
                        DeviceData.Set(ActiveValue[i], myArray[i]);
                    }
                }
                else
                {
                    Log.WriteLine(logText);
                }
            }

            //stopwatch.Stop();
            //Log.WriteLine(Locale.IsRussian ?
            //    "Получено за {0} мс" :
            //    "Received in {0} ms", stopwatch.ElapsedMilliseconds);

            FinishSession();
        }
        #endregion Session

        #region SendCommand
        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public override void SendCommand(TeleCommand cmd)
        {
            base.SendCommand(cmd);
            LastRequestOK = false;

            if (!double.IsNaN(cmd.CmdVal) && cmd.CmdCode != "")
            {
                try
                {
                    config.Headers.Remove(auth);
                    config.Method = RequestMethod.Post;

                    int idkey = ActiveValue.Where(x => x.Value == cmd.CmdCode).FirstOrDefault().Key; // Это индекс кода тега - подставляем в rb{}n, rb{}f, rb{}s где 1=n - включить, 0=f - выключить, 2=s - пульс - получить через case
                    config.ContentType = null;

                    if (devTemplate.HttpRequest.Count > 1) // Второй HttpRequest для команды
                    {
                        config.Uri = string.IsNullOrEmpty(devTemplate.HttpRequest[1].Uri) ? $"http://{DeviceConfig.StrAddress}/protect/rb{idkey.ToString()}{GetFunc((int)cmd.CmdVal)}.cgi" : devTemplate.HttpRequest[1].Uri;
                        config.Content = string.IsNullOrEmpty(devTemplate.HttpRequest[0].Content) ? content : devTemplate.HttpRequest[1].Content;
                    }
                    else
                    {
                        config.Uri = $"http://{DeviceConfig.StrAddress}/protect/rb{idkey.ToString()}{GetFunc((int)cmd.CmdVal)}.cgi";
                        config.Content = content;
                    }

                    Request();


                    if (LastRequestOK)
                    {



                    }

                }
                catch (Exception ex)
                {
                    Log.WriteLine(ex.Message);
                }
            }

            FinishCommand();
        }


        #endregion SendCommand

        #region Get Func
        private static string GetFunc(int val)
        {
            string fu = "f";
            switch (val)
            {
                case 0: fu = "f"; break;
                case 1: fu = "n"; break;
                case 2: fu = "s"; break;
            }
            return fu;
        }
        #endregion Get Func

        #region CommLineTerninate
        /// <summary>
        /// Performs actions when terminating a communication line.
        /// </summary>
        public override void OnCommLineTerminate()
        {
            httpClient?.Dispose();
        }
        #endregion CommLineTerninate

        #region Request
        private void Request()
        {
            if (CreateRequest(out HttpRequestMessage request))
            {
                int tryNum = 0;

                while (RequestNeeded(ref tryNum))
                {
                    LastRequestOK = SendNotification(request, out contentResponse);

                    FinishRequest();
                    tryNum++;
                }
                request.Dispose();
            }
        }

        #region Send Notification
        /// <summary>
        /// Sends a notification using the specified request.
        /// </summary>
        private bool SendNotification(HttpRequestMessage request, out string contentResponse) // , string tagCode
        {
            try
            {
                bool requestResult = true;
                // send request and receive response
                Log.WriteLine(Locale.IsRussian ?
                    "Отправка запроса:" :
                    "Send request:");
                Log.WriteLine(request.RequestUri.ToString()); // Тут можно пробоват прятать пароль ???
                HttpStatusCode responseStatus;
                string responseContent;
                stopwatch.Restart();
                using (HttpResponseMessage response = httpClient.SendAsync(request).Result)
                {
                    responseStatus = response.StatusCode;
                    responseContent = response.Content.ReadAsStringAsync().Result;
                }

                contentResponse = responseContent;

                stopwatch.Stop();

                // Проверка статуса ответа для запросов и команд
                if ((int)responseStatus != 200)
                {
                    Log.WriteLine(Locale.IsRussian ?
                        "Ошибка: {0}"  :
                        "Error: {0}", responseContent);
                    requestResult = false;
                }
                else if (responseContent.Length > 0)
                {
                    Log.WriteLine(Locale.IsRussian ?
                        "Содержимое ответа:" :
                        "Response content:");

                    bool full_log = false;
                    bool.TryParse(options.Full_Log, out full_log);

                    if (responseContent.Length <= ResponseDisplayLenght || full_log)
                    {
                        Log.WriteLine(responseContent);
                    }
                    else
                    {
                        Log.WriteLine(responseContent.Substring(0, ResponseDisplayLenght));
                        Log.WriteLine("...");
                    }
                }

                // output response to log
                Log.WriteLine(Locale.IsRussian ?
                    "Ответ получен за {0} мс. Статус: {1} ({2})" :
                    "Response received in {0} ms. Status: {1} ({2})",
                    stopwatch.ElapsedMilliseconds, (int)responseStatus, responseStatus);

                opLine.ResponseStatus = $"{(int)responseStatus} ({responseStatus})"; // TEST Вывод статуса в лог линии "Общие данные"
                LineContext.SharedData[LineContext.CommLineNum.ToString()] = opLine;

                return requestResult;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Locale.IsRussian ?
                    "Ошибка при отправке запроса: {0}" :
                    "Error sending request: {0}", ex.Message);
                contentResponse = null;
                return false;
            }
        }
        #endregion Send Notification

        /// <summary>
        /// Creates a request for sending a notification.
        /// </summary>
        private bool CreateRequest(out HttpRequestMessage request)
        {
            request = null;

            try
            {
                // initialize HTTP client
                if (httpClient == null)
                {
                    httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(PollingOptions.Timeout) };

                    foreach (Header header in config.Headers)
                    {
                        if (!string.IsNullOrEmpty(header.Name))
                            httpClient.DefaultRequestHeaders.Add(header.Name, header.Value);
                    }
                }

                request = new HttpRequestMessage(
                    config.Method == RequestMethod.Post ? HttpMethod.Post : HttpMethod.Get,
                    config.Uri);

                if (config.Method == RequestMethod.Post)
                {
                    request.Content = string.IsNullOrEmpty(config.ContentType) ?
                        new StringContent(config.Content, Encoding.UTF8) :
                        new StringContent(config.Content, Encoding.UTF8, config.ContentType);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Locale.IsRussian ?
                    "Ошибка при создании запроса: {0}" :
                    "Error creating request: {0}", ex.Message);
                httpClient?.Dispose();
                httpClient = null;
                request?.Dispose();
                request = null;

                return false;
            }
        }
        #endregion Request

        #region Open Request
        private bool Open(string Response, out string message)
        {
            message = null;
            try
            {
                //response devString = null;
                XDocument xdoc = XDocument.Parse(Response);
                XDeclaration declaration = new XDeclaration("1.0", "UTF-8", "yes");
                xdoc.Declaration = declaration;

                using (MemoryStream w = new MemoryStream())
                {
                    xdoc.Save(w);
                    w.Position = 0;
                    XmlSerializer serializer = new XmlSerializer(typeof(response));
                    devString = serializer.Deserialize(w) as response;
                    w.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                message = ex.InnerException.ToString();
                return false;
            }
        }
        #endregion Open Request

        private void GetDictParameter()
        {
            int idx = 0;
            foreach (var tag in devTemplate.Value)
            {
                if (!ActiveValue.ContainsKey(idx))
                {
                    ActiveValue.Add(idx, tag.code); // построении вытащить имя Переменной
                    idx++;
                }
            }
        }

        // Статическое поле — инициализируется один раз при загрузке класса
        private static readonly PropertyInfo[] _rlProperties = typeof(response)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(int) && p.Name.StartsWith("rl"))
            .OrderBy(p => int.Parse(p.Name.Substring(2).Replace("string", "")))
            .ToArray();

    }
}

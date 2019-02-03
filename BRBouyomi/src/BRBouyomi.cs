// プラグインのファイル名は、「Plugin_*.dll」という形式にして下さい。
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using FNF.BouyomiChanApp;
using FNF.XmlSerializerSetting;
using Bouyomi;

namespace BRBouyomi
{
    /// <summary>
    /// BRBouyomiメインクラス
    /// </summary>
    public class BRBouyomi : IPlugin
    {
        public string Name => "BRBouyomi";
        public string Version => "2019/01/31版";
        public string Caption => "再生指定単語の改行を行います。";

        // プラグインの設定画面情報（設定画面が必要なければnullを返してください）
        public ISettingFormData SettingFormData
            => null;

        private const string recvIP = "127.0.0.1";
        private const int recvPORT = 50005;

        public const string replaceTagFile = @"ReplaceTag.dic";
#if DEBUG
        public const string debugFile = @"DebugReplaceTag.dic";
#endif

        private Accept scAccept;

        public static List<ReplaceTag> replaceTagList = new List<ReplaceTag>();

        /// <summary>
        /// プラグイン開始時処理
        /// </summary>
        public void Begin()
        {
            try
            {
                replaceTagList = Utility.ReadReplaceTag();
                scAccept = new Accept(recvIP, recvPORT);
                scAccept.Start();
            }
            catch (Exception e)
            {
                End();
                throw e;
            }
        }

        /// <summary>
        /// プラグイン終了時処理
        /// </summary>
        public void End()
        {
            scAccept.Stop();
        }

        /// <summary>
        /// 受付クラス
        /// </summary>
        private class Accept
        {
            private bool active = true;
            private readonly string mHost;
            private readonly int mPort;
            private TcpClient client;
            private TcpListener listener;
            private Thread thread;

            public Accept(string host, int port)
            {
                mHost = host;
                mPort = port;
            }

            public void Start()
            {
                thread = new Thread(Run);
                thread.Start();
            }

            public void Stop()
            {
                active = false;
                client?.Close();
                listener?.Stop();
                thread?.Abort();
            }

            private void Run()
            {
                var ip = IPAddress.Parse(mHost);
                var ipEndPoint = new IPEndPoint(ip, mPort);

                listener = new TcpListener(ipEndPoint);
                listener.Start();

                // 要求待ち
                while (active)
                {
                    Response response;
                    try
                    {
                        client = listener.AcceptTcpClient();
                        response = new Response(client);
                        response.Start();
                    }
                    catch (Exception)
                    {
                        Stop();
                    }
                }
            }
        }

        /// <summary>
        /// 応答クラス
        /// </summary>
        private class Response
        {
            private readonly TcpClient mClient;

            public Response(TcpClient client)
            {
                mClient = client;
            }

            public void Start()
            {
                var thread = new Thread(Run);
                thread.Start();
            }

            private void Run()
            {
                BRBouyomiTalks bouyomiTalks;
                using (var networkStream = mClient.GetStream())
                {
                    // デフォルトはInfiniteで、タイムアウトしない
                    networkStream.ReadTimeout = 10000;
                    networkStream.WriteTimeout = 10000;

                    using (var binaryReader = new BinaryReader(networkStream))
                    {
                        // 受信
                        bouyomiTalks = new BRBouyomiTalks(binaryReader);
                    }
                }

                var bouyomiTalksList = bouyomiTalks.SplitForReplaceTag();

                foreach (var bt in bouyomiTalksList)
                {
                    bt.AddTalkTask();
                }
            }
        }
    }

    /// <summary>
    /// BRBouyomiタスククラス
    /// </summary>
    [Serializable]
    public class BRBouyomiTalks : BouyomiTalks
    {
        public BRBouyomiTalks(BinaryReader binaryReader) : base(binaryReader) { }

        public List<BouyomiTalks> SplitForReplaceTag()
        {
            List<BouyomiTalks> bouyomiTalksList = new List<BouyomiTalks>();

            var messages = new List<string>
            {
                MessageProp.Message
            };

            var searchedMessage = messages.Last();
            // メッセージ切り分け処理
            // 切り分けられなくなるまで
            do
            {
                ReplaceTag replaceTag = Utility.SearchReplaceTag(BRBouyomi.replaceTagList, searchedMessage);
                // 検索にヒットしなかった場合切り分けしない
                if (replaceTag == null)
                {
                    break;
                }
#if DEBUG
                using (var streamWriter = new StreamWriter(BRBouyomi.debugFile, true, Encoding.UTF8))
                {
                    streamWriter.WriteLine(replaceTag.ToStringProperty(ReplaceTag.SEPARATOR));
                }
#endif
                messages = Utility.SplitMessage(searchedMessage, replaceTag.BeforeReplace);

                // 次の検索文字を設定
                searchedMessage = messages.Last();
                messages.RemoveAt(messages.Count - 1);

                // 切り分け後格納
                foreach (var message in messages)
                {
                    bouyomiTalksList.Add(ReplacedMessage(message, MessageProperty.MessageEncode.UTF8));
                }
            } while (messages.Count > 0);

            // 最後の検索文字を格納
            bouyomiTalksList.Add(ReplacedMessage(searchedMessage, MessageProperty.MessageEncode.UTF8));

            return bouyomiTalksList;
        }
    }

    /// <summary>
    /// 便利クラス
    /// </summary>
    public static class Utility
    {
        public static List<ReplaceTag> ReadReplaceTag()
        {
            var readList = new List<ReplaceTag>();
            using (var streamReader = new StreamReader(BRBouyomi.replaceTagFile, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    readList.Add(new ReplaceTag(line));
                }
            }

            // 第一ソートキー：優先度
            // 第ニソートキー：文字数
            // 並べ替えられてないっぽい
            var replaceTagList = readList.OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.BeforeReplace.Length)
                .ToList();

            return replaceTagList;
        }

#if DEBUG
        public static void WriteReplaceTag(List<ReplaceTag> writeList)
        {
            using (var streamWriter = new StreamWriter(BRBouyomi.debugFile, false, Encoding.UTF8))
            {
                foreach (var list in writeList)
                {
                    streamWriter.WriteLine(list.ToStringProperty(ReplaceTag.SEPARATOR));
                }
            }
        }
#endif

        public static ReplaceTag SearchReplaceTag(List<ReplaceTag> replaceTagList, string message)
        {
            return replaceTagList.Find(
                replaceTag => message.IndexOf(replaceTag.BeforeReplace, StringComparison.Ordinal)
                >= 0);
        }

        public static List<string> SplitMessage(string splitMessage, string splitter)
        {
            List<string> messages = new List<string>();
            var numMessage = splitMessage.IndexOf(splitter);

            //先頭～切り分け文字先頭-1まで切り取り
            var startMessageLen = numMessage;
            if (startMessageLen != 0)
            {
                messages.Add(splitMessage.Substring(0, startMessageLen));
            }

            messages.Add(splitter);

            //切り分け文字後尾～最後尾まで切り取り
            var endMessageStart = numMessage + splitter.Length;
            var endmessageLen = splitMessage.Length - endMessageStart;

            if (endmessageLen > 0)
            {
                messages.Add(splitMessage.Substring(endMessageStart, endmessageLen));
            }

            return messages;
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FNF.BouyomiChanApp;
using FNF.Utility;
using System.Reflection;

namespace Bouyomi
{
    /// <summary>
    /// 棒読みちゃんタスククラス
    /// </summary>
    [Serializable]
    public class BouyomiTalks
    {
        /// <summary>
        /// コマンド（ 0:メッセージ読み上げ）
        /// </summary>
        public Int16 Command { get; set; }
        /// <summary>
        /// 速度（-1:棒読みちゃん画面上の設定）
        /// </summary>
        public Int16 Speed { get; set; }
        /// <summary>
        /// 音程（-1:棒読みちゃん画面上の設定）
        /// </summary>
        public Int16 Tone { get; set; }
        /// <summary>
        /// 音量（-1:棒読みちゃん画面上の設定）
        /// </summary>
        public Int16 Volume { get; set; }
        /// <summary>
        /// 声質 0:棒読みちゃん画面上の設定、1:女性1、2:女性2、3:男性1、4:男性2、5:中性、6:ロボット、7:機械1、8:機械2、10001～:SAPI5）
        /// </summary>
        public Int16 Voice { get; set; }

        public MessageProperty MessageProp { get; private set; }

        [Serializable]
        public class MessageProperty
        {
            /// <summary>
            /// 読み上げ文字列
            /// </summary>
            public string Message { get; private set; }
            /// <summary>
            /// 読み上げ文字列のbyte配列の文字コード(0:UTF-8, 1:Unicode, 2:Shift-JIS)
            /// </summary>
            public byte Code { get; private set; }
            /// <summary>
            /// 読み上げ文字列のbyte配列の長さ
            /// </summary>
            public Int32 Length { get; private set; }
            /// <summary>
            /// 読み上げ文字列のbyte配列
            /// </summary>
            public byte[] ByteMessage { get; private set; }

            public enum MessageEncode : byte
            {
                UTF8 = 0,
                Unicode = 1,
                ShiftJIS = 2
            }

            public MessageProperty(byte bCode, Int32 iLength, byte[] bMessage)
            {
                Code = bCode;
                Length = iLength;
                ByteMessage = bMessage;

                Message = GetEncoding((MessageEncode)bCode).GetString(bMessage);
            }

            public MessageProperty(string message, MessageEncode messageEncode)
            {
                Message = message;

                Code = (byte)messageEncode;
                ByteMessage = GetEncoding(messageEncode).GetBytes(message);
                Length = ByteMessage.Length;
            }

            private Encoding GetEncoding(MessageEncode bCode)
            {
                switch (bCode)
                {
                    case MessageEncode.UTF8:
                        return Encoding.UTF8;
                    case MessageEncode.Unicode:
                        return Encoding.Unicode;
                    case MessageEncode.ShiftJIS:
                        return Encoding.GetEncoding("Shift_JIS");
                    default:
                        return Encoding.UTF8;
                }
            }
        }

        public BouyomiTalks(BinaryReader binaryReader)
        {
            Command = binaryReader.ReadInt16();
            Speed = binaryReader.ReadInt16();
            Tone = binaryReader.ReadInt16();
            Volume = binaryReader.ReadInt16();
            Voice = binaryReader.ReadInt16();
            var bCode = binaryReader.ReadByte();
            var iLength = binaryReader.ReadInt32();
            var bMessage = binaryReader.ReadBytes(iLength);
            MessageProp = new MessageProperty(bCode, iLength, bMessage);
        }

        public void AddTalkTask()
        {
            Pub.AddTalkTask(MessageProp.Message, Speed, Tone, Volume, (VoiceType)Voice);
        }
        
        public BouyomiTalks ReplacedMessage(string message, MessageProperty.MessageEncode encode)
        {
            var clone = this.DeepCopy();
            clone.SetMessage(message, encode);
            return clone;
        }

        public void SetMessage(string message, MessageProperty.MessageEncode messageEncode)
        {
            MessageProp = new MessageProperty(message, messageEncode);
        }
    }

    /// <summary>
    /// ReplaceTagクラス
    /// </summary>
    public class ReplaceTag
    {
        public byte Priority { get; private set; }
        public string SearchTrigger { get; private set; }
        public string BeforeReplace { get; private set; }
        public string AfterReplace { get; private set; }

        public const string SEPARATOR = "\t";

        public ReplaceTag(string line)
        {
            var fields = line.Split('\t');
            Priority = byte.Parse(fields[0]);
            SearchTrigger = fields[1];
            BeforeReplace = fields[2];
            AfterReplace = fields[3];
        }

    }

    /// <summary>
    /// 拡張メソッドクラス
    /// </summary>
    public static class Extensions
    {
        public static T DeepCopy<T>(this T src)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, src);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// すべての公開プロパティの情報を文字列にして返します
        /// </summary>
        public static string ToStringProperty<T>(this T obj, string separator)
        {
            return string.Join(separator, (string[])obj
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(c => c.CanRead)
                .Select(c => c.GetValue(obj, null).ToString()).ToArray());
        }
    }
}

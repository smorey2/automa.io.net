using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Automa.IO
{
    internal static class StreamReaderExtensions
    {
        static MethodInfo CheckAsyncTaskInProgressMethod = typeof(StreamReader).GetMethod("CheckAsyncTaskInProgress", BindingFlags.Instance | BindingFlags.NonPublic);
        static MethodInfo ReadBufferMethod = typeof(StreamReader).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(x => x.Name == "ReadBuffer" && !x.IsPrivate);
        static FieldInfo charBufferField = typeof(StreamReader).GetField("_charBuffer", BindingFlags.Instance | BindingFlags.NonPublic) ?? typeof(StreamReader).GetField("charBuffer", BindingFlags.Instance | BindingFlags.NonPublic);
        static FieldInfo charLenField = typeof(StreamReader).GetField("_charLen", BindingFlags.Instance | BindingFlags.NonPublic) ?? typeof(StreamReader).GetField("charLen", BindingFlags.Instance | BindingFlags.NonPublic);
        static FieldInfo charPosField = typeof(StreamReader).GetField("_charPos", BindingFlags.Instance | BindingFlags.NonPublic) ?? typeof(StreamReader).GetField("charPos", BindingFlags.Instance | BindingFlags.NonPublic);

        static void CheckAsyncTaskInProgress(this StreamReader source) => CheckAsyncTaskInProgressMethod.Invoke(source, null);
        static void ReadBuffer(this StreamReader source) => ReadBufferMethod.Invoke(source, null);
        static char[] charBuffer(this StreamReader source) => (char[])charBufferField.GetValue(source);
        static int charLen(this StreamReader source) => (int)charLenField.GetValue(source);
        static int charPos(this StreamReader source) => (int)charPosField.GetValue(source);
        static void charPos(this StreamReader source, int value) => charPosField.SetValue(source, value);

        public static string SafeReadToEnd(this StreamReader source)
        {
            if (source.BaseStream == null)
                throw new InvalidOperationException("ReaderClosed");
            source.CheckAsyncTaskInProgress();
            var charBuffer = source.charBuffer();
            var charLen = source.charLen(); var charPos = source.charPos();
            var b = new StringBuilder(charLen - charPos);
            try
            {
                while (true)
                {
                    b.Append(charBuffer, charPos, charLen - charPos);
                    source.charPos(charLen);
                    source.ReadBuffer();
                    charLen = source.charLen(); charPos = source.charPos();
                    if (charLen <= 0)
                        return b.ToString();
                }
            }
            catch (Exception e)
            {
                if (e.InnerException != null && !(e.InnerException is IOException))
                    throw e.InnerException;
                charLen = source.charLen(); charPos = source.charPos();
                if (charLen > 0)
                    b.Append(charBuffer, charPos, charLen - charPos);
                return b.ToString();
            }
        }
    }
}
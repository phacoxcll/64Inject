using System;
using System.Diagnostics;
using System.IO;

namespace Cll
{
    public class Log : IDisposable
    {
        public enum TabMode
        {
            None,
            OnlyFirst,
            ExceptFirst,
            All
        }

        private bool disposed = false;
        private StreamWriter _log;

        public Log()
        {
            _log = null;
        }

        public Log(string filename)
        {
            if (filename != null && filename.Length > 0)
            {
                try
                {
                    _log = File.CreateText(filename);
                }
                catch
                {
                    _log = null;
                }
            }
            else
                _log = null;
        }

        ~Log()
        {
            Dispose(false);
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (_log != null)
                        _log.Dispose();
                }
                disposed = true;
            }
        }

        public void Write(string value)
        {
            Debug.Write(value);
            Console.Write(value);
            if (_log != null)
                _log.Write(value);
        }

        public void WriteLine(string value)
        {
            Debug.WriteLine(value);
            Console.WriteLine(value);
            if (_log != null)
                _log.WriteLine(value);
        }

        public void WriteLine(string value, int width, int tab, TabMode mode)
        {
            int tabLength = GetTabLength(value);
            string t = tab > 0 ? new string(' ', tab) : value.Substring(0, tabLength);
            string s = value.Substring(tabLength, value.Length - tabLength);
            string[] words = s.Split(new char[] { ' ' });

            if (words.Length == 1)
                WriteLine((mode == TabMode.All || mode == TabMode.OnlyFirst ? t : "") + s);

            else if (words.Length > 1)
            {
                int charCount = words[0].Length;
                int start = 0;
                int i = 1;
                for (; i < words.Length; i++)
                {
                    if ((mode == TabMode.All || mode == TabMode.OnlyFirst ? t.Length : 0) +
                        charCount + words[i].Length + 1 < width)
                        charCount += words[i].Length + 1;
                    else
                    {
                        WriteLine((mode == TabMode.All || mode == TabMode.OnlyFirst ? t : "") +
                            s.Substring(start, charCount));
                        start += charCount + 1;
                        charCount = words[i].Length;
                        break;
                    }

                    if (i + 1 == words.Length)
                        WriteLine((mode == TabMode.All || mode == TabMode.OnlyFirst ? t : "") +
                            s.Substring(start, charCount));
                }
                i++;
                for (; i < words.Length; i++)
                {
                    if ((mode == TabMode.All || mode == TabMode.ExceptFirst ? t.Length : 0) +
                        charCount + words[i].Length + 1 < width)
                        charCount += words[i].Length + 1;
                    else
                    {
                        WriteLine((mode == TabMode.All || mode == TabMode.ExceptFirst ? t : "") +
                            s.Substring(start, charCount));
                        start += charCount + 1;
                        charCount = words[i].Length;
                    }

                    if (i + 1 == words.Length)
                        WriteLine((mode == TabMode.All || mode == TabMode.ExceptFirst ? t : "") +
                                s.Substring(start, charCount));
                }
            }
            else
                WriteLine(mode == TabMode.All || mode == TabMode.OnlyFirst ? t : "");
        }

        public void WriteText(string text, int width, int tab, TabMode mode)
        {
            text = text.Replace("\r", "");
            string[] lines = text.Split(new char[] { '\n' });

            foreach (string line in lines)
                WriteLine(line, width, tab, mode);
        }

        private int GetTabLength(string line)
        {
            int i;

            for (i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                    break;
            }

            return i;
        }
    }
}

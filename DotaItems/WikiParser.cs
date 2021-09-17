using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaItems
{
	class WikiParser
	{
		int pos = 0;
		string text;
		int lineNumber = 0;
		int column = 0;

		public WikiParser(string text)
		{
			this.text = text;
		}

		private char Read()
		{
			while (pos < text.Length)
			{
				char c = text[pos++];
				switch (c)
				{
					case '\n':
						lineNumber++;
						column = 0;
						break;
					case '\r':
						break;
					default:
						column++;
						return c;
				}
			}
			return '\0';
		}

		private char Peek()
		{
			var pos = this.pos;
			var lineNum = lineNumber;
			var column = this.column;
			var ret = Read();
			this.pos = pos;
			lineNumber = lineNum;
			this.column = column;
			return ret;
		}

		public Dictionary<string, object> ReadObject()
		{
			Dictionary<string, object> output = new();
			Read();
			Read();
			var (key, value) = ReadPair();
			output[key] = value;
			while (Peek() != '}')
			{
				Read();
				(key, value) = ReadPair();
				output[key] = value;
			}
			Read();
			Read();
			return output;
		}

		public (string key, object value) ReadPair()
		{
			var key = ReadKey();
			if (Peek() == '=')
			{
				Read();
				return (key, ReadValue());
			}
			else
			{
				return (key, true);
			}
		}

		private string ReadKey()
		{
			StringBuilder sb = new StringBuilder();
			while (!"}|=".Contains(Peek()))
			{
				sb.Append(Read());
			}
			return sb.ToString();
		}

		private object ReadValue()
		{
			switch (Peek())
			{
				case '{':
					return ReadObject();
				default:
					return ReadKey();
			}
		}
	}
}

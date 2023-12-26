using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBASInterpreter
{
    internal class NextItemFetcher
    {
        private readonly HashSet<string> Z;

        public NextItemFetcher(HashSet<string> Z) {
            this.Z = Z;
        }    
        public string GetNextItemWithoutSpace(string programText, ref int i)
        {
            var item = GetNextItem(programText, i);
            i += item.Length;

            if (string.IsNullOrEmpty(item))
            {
                return item;
            }

            while (item.IsSpace())
            {
                item = GetNextItem(programText, i);
                i += item.Length;
            }

            if (item.Length == 1 && char.IsDigit(item[0]))
            {
                while (char.IsDigit(programText[i]))
                {
                    item += programText[i];
                    i++;
                }
            }

            if (item.Length == 1 && (char.IsLetter(item[0]) || item[0] == '_'))
            {
                while (char.IsLetter(programText[i]) || item[0] == '_')
                {
                    item += programText[i];
                    i++;
                }
            }

            item = item.Trim();
            return item;
        }

        public string GetNextItem(string inputStr, int i)
        {
            var startIndex = i;
            int j = 1;
            string item = "";
            while (startIndex + j <= inputStr.Length)
            {
                item = inputStr[startIndex..(startIndex + j)];
                var items = Z.Where(x => x == item);

                if (items.Count() == 1)
                {
                    item = items.First();
                    break;
                }

                if (!items.Any() || item == "$")
                {
                    item = inputStr[startIndex..(startIndex + j)];
                    j--;
                    break;
                }

                j++;
            }

            return item;
        }
    }
}

using System.IO;
using NHunspell;

namespace CodeCleaner
{
    public enum SuggestionKind { MeaninglessWord, UpperCase, LowerCase, ParameterCount };
    public class Cleaner
    {
        readonly Hunspell hunspell;
        readonly string affixPath;
        readonly string dictionaryPath;
        public TextWriter suggestionStream;   // suggestion messages go to this stream

        public Cleaner(string dictionariesPath, TextWriter suggestionStream)
        {
            affixPath = Path.Combine(dictionariesPath, "en_US.aff");
            dictionaryPath = Path.Combine(dictionariesPath, "en_US.dic");
            this.suggestionStream = suggestionStream;
            hunspell = new Hunspell(affixPath, dictionaryPath);
        }

        public void Suggest(int line, int coloumn, SuggestionKind kind)
        {
            string suggestion = "";
            switch (kind)
            {
                case SuggestionKind.MeaninglessWord:
                    suggestion = "Meaningless word!";
                    break;
                case SuggestionKind.UpperCase:
                    suggestion = "Name sould starts with upper case!";
                    break;
                case SuggestionKind.LowerCase:
                    suggestion = "Name sould starts with lower case!";
                    break;
                case SuggestionKind.ParameterCount:
                    suggestion = "More than 4 parameter!";
                    break;
                default:
                    break;
            }

            suggestionStream.WriteLine("Clean code suggestion in line {0} coloumn {1}: {2}"
                , line, coloumn, suggestion);
        }

        void CheckNamesMeaning(string name, int line, int colomn)
        {
            string[] words = name.SplitCamelCase();
            int currentColoumn = colomn;
            for (int i = 0; i < words.Length; i++)
            {
                if (!hunspell.Spell(words[i]))
                    Suggest(line, currentColoumn, SuggestionKind.MeaninglessWord);
                currentColoumn += words[i].Length;
            }
        }

        void CheckUpperCase(string name, int line, int coloumn)
        {
            if (!name.StartsWithUpperCase())
                Suggest(line, coloumn, SuggestionKind.UpperCase);
        }

        void CheckLowerCase(string name, int line, int coloumn)
        {
            if (name.StartsWithUpperCase())
                Suggest(line, coloumn, SuggestionKind.LowerCase);
        }

        public void CheckClassName(string name, int line, int coloumn)
        {
            CheckUpperCase(name, line, coloumn);
            CheckNamesMeaning(name, line, coloumn);
        }
      
        public void CheckNamespaceName(string name, int line, int coloumn)
        {
            CheckUpperCase(name, line, coloumn);
            CheckNamesMeaning(name, line, coloumn);
        }

        public void CheckFunctionName(string name, int line, int coloumn)
        {
            CheckUpperCase(name, line, coloumn);
            CheckNamesMeaning(name, line, coloumn);
        }

        public void CheckParameterName(string name, int line, int coloumn)
        {
            CheckLowerCase(name, line, coloumn);
            CheckNamesMeaning(name, line, coloumn);
        }
      
        public void CheckParameterCount(int paramCount, int line, int column)
        {
            if (paramCount >= 5)
                Suggest(line, column, SuggestionKind.ParameterCount);
        }
    }
}

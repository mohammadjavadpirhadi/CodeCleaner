using System.Collections.Generic;
using System.IO;
using NHunspell;

namespace CodeCleaner
{
    public enum SuggestionKind { MeaninglessWord, UpperCase, LowerCase, ParameterCount, LineCount,
                                 IndentBlockCount };
    public class Cleaner
    {
        readonly Hunspell hunspell;
        readonly string affixPath;
        readonly string dictionaryPath;
        readonly List<string> variables;
        readonly List<int> variablesBlockNumber;
        readonly Errors errors;
        public int blockNumber;
        public TextWriter suggestionStream;   // suggestion messages go to this stream
        public bool isInFunction;
        public int currentFunctionBlockNumber;
        public int startLine;
        public int endLine;
        public int startColumn;

        public Cleaner(string dictionariesPath, TextWriter suggestionStream, Errors errors)
        {
            affixPath = Path.Combine(dictionariesPath, "en_US.aff");
            dictionaryPath = Path.Combine(dictionariesPath, "en_US.dic");
            this.suggestionStream = suggestionStream;
            this.errors = errors;
            hunspell = new Hunspell(affixPath, dictionaryPath);
            hunspell.Add("args");
            variables = new List<string>();
            variablesBlockNumber = new List<int>();
            blockNumber = 0;
            isInFunction = false;
            currentFunctionBlockNumber = 0;
            startLine = 0;
            endLine = 0;
            startColumn = 0;
        }

        void Suggest(int line, int coloumn, SuggestionKind kind)
        {
            string suggestion = "";
            switch (kind)
            {
                case SuggestionKind.MeaninglessWord:
                    suggestion = "meaningless word!";
                    break;
                case SuggestionKind.UpperCase:
                    suggestion = "illegal lower case start!";
                    break;
                case SuggestionKind.LowerCase:
                    suggestion = "illegal upper case start!";
                    break;
                case SuggestionKind.ParameterCount:
                    suggestion = "more than 4 parameter!";
                    break;
                case SuggestionKind.LineCount:
                    suggestion = "more than 24 line!";
                    break;
                case SuggestionKind.IndentBlockCount:
                    suggestion = "more than 2 indent block!";
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
                if (!hunspell.Spell(words[i]) || (words[i].Length == 1 && words[i] != "I"))
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
            if (!variables.Contains(name))
            {
                variables.Add(name);
                variablesBlockNumber.Add(blockNumber + 1);
            }
        }

        public void CheckVariableDefinition(string name, int line, int coloumn)
        {
            if (!variables.Contains(name))
                errors.SynErr(line, coloumn, 214);
        }

        public void CheckNewVariableName(string name, int line, int coloumn, bool isForVariable)
        {
            CheckLowerCase(name, line, coloumn);
            if (!isForVariable)
                CheckNamesMeaning(name, line, coloumn);
            if (variables.Contains(name))
                errors.SynErr(line, coloumn, 215);
            else
            {
                variables.Add(name);
                if (isForVariable)
                    variablesBlockNumber.Add(blockNumber + 1);
                else
                    variablesBlockNumber.Add(blockNumber);
            }
        }

        public void CheckIndentBlockCount()
        {
            if (isInFunction && currentFunctionBlockNumber + 2 < blockNumber)
                Suggest(startLine, startColumn, SuggestionKind.IndentBlockCount);
        }
        public void RemoveBlockVariables()
        {
            if (variables.Count <= 0)
                return;
            while (variables.Count > 0 &&
                variablesBlockNumber[variablesBlockNumber.Count - 1] == blockNumber)
            {
                variables.RemoveAt(variables.Count - 1);
                variablesBlockNumber.RemoveAt(variablesBlockNumber.Count - 1);
            }
            blockNumber--;
        }
      
        public void CheckParameterCount(int paramCount, int line, int column)
        {
            if (paramCount >= 5)
                Suggest(line, column, SuggestionKind.ParameterCount);
        }

        public void CheckLineCount()
        {
            if (endLine - startLine + 1 > 24)
                Suggest(startLine, startColumn, SuggestionKind.LineCount);
        }
    }
}

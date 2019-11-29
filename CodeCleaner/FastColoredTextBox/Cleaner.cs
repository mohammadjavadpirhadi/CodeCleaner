using System.Collections.Generic;
using System.IO;
using NHunspell;

namespace CodeCleaner
{
    public enum SuggestionKind
    {
        MeaninglessWord, UpperCase, LowerCase, ParameterCount, LineCount,
        IndentBlockCount
    };

    public class Suggestion
    {
        public string word;
        public bool hasWord;
        public int line;
        public int column;
        public SuggestionKind kind;
        public string message;
        public string suggestion;
        public bool hasSuggestion;

        public Suggestion(string word, int line, int column,
            SuggestionKind kind, string message, string suggestion,
            bool hasWord = true, bool hasSuggestion = true)
        {
            this.word = word;
            this.hasWord = hasWord;
            this.line = line;
            this.column = column;
            this.kind = kind;
            this.message = message;
            this.suggestion = suggestion;
            this.hasSuggestion = hasSuggestion;
        }
    }

    public class Cleaner
    {
        readonly Hunspell hunspell;
        readonly string affixPath;
        readonly string dictionaryPath;
        readonly List<string> variables;
        readonly List<int> variablesBlockNumber;
        readonly Errors errors;
        public bool isForVariable;
        public int blockNumber;
        public TextWriter suggestionStream;   // suggestion messages go to this stream
        public bool isInFunction;
        public int currentFunctionBlockNumber;
        public int startLine;
        public int endLine;
        public int startColumn;
        public List<Suggestion> suggestions;

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
            isForVariable = false;
            blockNumber = 0;
            isInFunction = false;
            currentFunctionBlockNumber = 0;
            startLine = 0;
            endLine = 0;
            startColumn = 0;
            this.suggestions = new List<Suggestion>();
        }

        void Suggest(int line, int column, SuggestionKind kind, string word = "")
        {
            string message = "";
            string suggestion = "";
            switch (kind)
            {
                case SuggestionKind.MeaninglessWord:
                    message = "Meaningless word!";
                    break;
                case SuggestionKind.UpperCase:
                    suggestion = word.ToUpper()[0] + word.Substring(1);
                    message = string.Format("Illegal lower case start -> Click to rename to \"{0}\"",
                        suggestion);
                    break;
                case SuggestionKind.LowerCase:
                    suggestion = word.ToLower()[0] + word.Substring(1);
                    message = string.Format("Illegal upper case start -> Click to rename to \"{0}\"",
                        suggestion);
                    break;
                case SuggestionKind.ParameterCount:
                    message = "More than 4 parameter!";
                    break;
                case SuggestionKind.LineCount:
                    message = "More than 24 line!";
                    break;
                case SuggestionKind.IndentBlockCount:
                    message = "More than 2 indent block!";
                    break;
                default:
                    break;
            }

            if (word != "")
                if (suggestion != "")
                    suggestions.Add(new Suggestion(word, line, column, kind, message, suggestion));
                else
                    suggestions.Add(new Suggestion(word, line, column, kind,
                        message, suggestion, hasSuggestion: false));
            else
            {
                if (suggestion != "")
                    suggestions.Add(new Suggestion(word, line, column, kind, message, suggestion, hasWord: false));
                else
                    suggestions.Add(new Suggestion(word, line, column, kind, message, suggestion, false, false));
            }

            suggestionStream.WriteLine("Clean code suggestion in line {0} column {1}: {2}"
                , line, column, message);
        }

        void CheckNamesMeaning(string name, int line, int column)
        {
            string[] words = StringExtensions.SplitCamelCase(name);
            int currentColumn = column;
            for (int i = 0; i < words.Length; i++)
            {
                if (!hunspell.Spell(words[i]) || (words[i].Length == 1 && words[i] != "I"))
                    Suggest(line, currentColumn, SuggestionKind.MeaninglessWord, name);
                currentColumn += words[i].Length;
            }
        }

        void CheckUpperCase(string name, int line, int column)
        {
            if (!StringExtensions.StartsWithUpperCase(name))
                Suggest(line, column, SuggestionKind.UpperCase, name);
        }

        void CheckLowerCase(string name, int line, int column)
        {
            if (StringExtensions.StartsWithUpperCase(name))
                Suggest(line, column, SuggestionKind.LowerCase, name);
        }

        public void CheckClassName(string name, int line, int column)
        {
            CheckUpperCase(name, line, column);
            CheckNamesMeaning(name, line, column);
        }

        public void CheckNamespaceName(string name, int line, int column)
        {
            CheckUpperCase(name, line, column);
            CheckNamesMeaning(name, line, column);
        }

        public void CheckFunctionName(string name, int line, int column)
        {
            CheckUpperCase(name, line, column);
            CheckNamesMeaning(name, line, column);
        }

        public void CheckParameterName(string name, int line, int column)
        {
            CheckLowerCase(name, line, column);
            CheckNamesMeaning(name, line, column);
            if (!variables.Contains(name))
            {
                variables.Add(name);
                variablesBlockNumber.Add(blockNumber + 1);
            }
        }

        public void CheckVariableDefinition(string name, int line, int column)
        {
            if (!variables.Contains(name))
                errors.SynErr(line, column, 214);
        }

        public void CheckNewVariableName(string name, int line, int column)
        {
            CheckLowerCase(name, line, column);
            if (!isForVariable)
                CheckNamesMeaning(name, line, column);
            if (variables.Contains(name))
                errors.SynErr(line, column, 215);
            else
            {
                variables.Add(name);
                if (isForVariable)
                    variablesBlockNumber.Add(blockNumber + 1);
                else
                    variablesBlockNumber.Add(blockNumber);
            }
        }

        public void CheckIndentBlockCount(int line, int column)
        {
            if (isInFunction && currentFunctionBlockNumber + 2 < blockNumber)
                Suggest(line, column, SuggestionKind.IndentBlockCount);
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

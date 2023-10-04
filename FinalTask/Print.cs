using System;
using System.Collections.Generic;
using System.Text;
using FinalTask.Task1.ThirdParty;

namespace FinalTask.Task1
{
    public class Print : ICommand
    {
        private readonly IDatabaseManager manager;
        private readonly IView view;
        private string tableName;

        private const char HorizontalLine = '═';
        private const char CellSeparator = '║';
        private const char TopLeftCorner = '╔';
        private const char TopRightCorner = '╗';
        private const char BottomLeftCorner = '╚';
        private const char BottomRightCorner = '╝';
        private const char BottomCellSeparator = '╩';
        private const char InnerLeft = '╠';
        private const char InnerRight = '╣';
        private const char InnerCellSeparator = '╬';
        private const char MiddleTop = '╦';
        private const char SingleSpace = ' ';

        private const string NewLine = "\n";


        public Print(IView view, IDatabaseManager manager)
        {
            this.view = view;
            this.manager = manager;
        }

        public bool CanProcess(string command)
        {
            return command.StartsWith("print ");
        }

        public void Process(string command)
        {
            var commandPart = command.Split(' ');
            if (commandPart.Length != 2)
                throw new ArgumentException(
                    "incorrect number of parameters. Expected 1, but is " + (commandPart.Length - 1));
            tableName = commandPart[1];
            var data = manager.GetTableData(tableName);
            view.Write(GetTableString(data));
        }

        private string GetTableString(IList<IDataSet> data)
        {
            return (data.Count == 0) ? GetEmptyTable(tableName) : GetHeaderOfTheTable(data) + GetStringTableData(data);
        }

        private string GetEmptyTable(string tableName)
        {
            var textEmptyTable = $"{CellSeparator} Table '{tableName}' is empty or does not exist {CellSeparator}";
            var horizontalLine = new string(HorizontalLine, textEmptyTable.Length - 2);

            return $"{TopLeftCorner}{horizontalLine}{TopRightCorner}{NewLine}{textEmptyTable}{NewLine}{BottomLeftCorner}{horizontalLine}{BottomRightCorner}{NewLine}";
        }

        private int GetMaxColumnSizeWithPadding(IList<IDataSet> dataSets)
        {
            const int EvenColumnLengthAdjestment = 2;
            const int OddColumnLengthAdjestment = 3;
            int maxColumnSize = dataSets.Count == 0 ? 0 : dataSets
                .SelectMany(dataSet => dataSet.GetValues())
                .Max(value => value.ToString().Length);
            return maxColumnSize += (maxColumnSize % 2 == 0) ? EvenColumnLengthAdjestment : OddColumnLengthAdjestment;
        }

        private string GetStringTableData(IList<IDataSet> dataSets)
        {
            var rowsCount = dataSets.Count;
            var maxColumnSize = rowsCount == 0 ? 0 : GetMaxColumnSizeWithPadding(dataSets);
            var result = string.Empty;
            var columnCount = GetColumnCount(dataSets);
            for (var row = 0; row < rowsCount; row++)
            {
                var values = dataSets[row].GetValues();
                result += CellSeparator;
                for (var column = 0; column < columnCount; column++)
                {
                    var valuesLength = values[column].ToString().Length;
                    if (valuesLength % 2 == 0)
                    {
                        for (var j = 0; j < (maxColumnSize - valuesLength) / 2; j++)
                            result += SingleSpace;
                        result += values[column].ToString();
                        for (var j = 0; j < (maxColumnSize - valuesLength) / 2; j++)
                            result += SingleSpace;
                        result += CellSeparator;
                    }
                    else
                    {
                        for (var j = 0; j < (maxColumnSize - valuesLength) / 2; j++)
                            result += SingleSpace;
                        result += values[column].ToString();
                        for (var j = 0; j <= (maxColumnSize - valuesLength) / 2; j++)
                            result += SingleSpace;
                        result += CellSeparator;
                    }
                }

                result += NewLine;
                if (row < rowsCount - 1)
                {
                    result += InnerLeft;
                    for (var j = 1; j < columnCount; j++)
                    {
                        for (var i = 0; i < maxColumnSize; i++)
                            result += HorizontalLine;
                        result += InnerCellSeparator;
                    }

                    for (var i = 0; i < maxColumnSize; i++)
                        result += HorizontalLine;
                    result += $"{InnerRight}{NewLine}";
                }
            }

            result += BottomLeftCorner;
            for (var j = 1; j < columnCount; j++)
            {
                for (var i = 0; i < maxColumnSize; i++)
                    result += HorizontalLine;
                result += BottomCellSeparator;
            }

            for (var i = 0; i < maxColumnSize; i++)
                result += HorizontalLine;
            result += $"{BottomRightCorner}{NewLine}";
            return result;
        }

        private int GetColumnCount(IList<IDataSet> dataSets)
        {
            return dataSets.Count > 0 ? dataSets[0].GetColumnNames().Count : 0;
        }

        private string GetFormattedColumnName(string columnName, int maxColumnSize)
        {
            var paddingLength = maxColumnSize - columnName.Length;
            var leftPadding = new string(' ', paddingLength / 2);
            var rightPadding = new string(' ', (paddingLength + 1) / 2);

            return $"{leftPadding}{columnName}{rightPadding}";
        }

        private StringBuilder GetTableTopBorder(int columnCount, int maxColumnSize)
        {
            var result = new StringBuilder();
            result.Append(TopLeftCorner);
            for (var j = 1; j <= columnCount; j++)
            {
                result.Append(new string(HorizontalLine, maxColumnSize));
                result.Append((j < columnCount) ? MiddleTop : $"{TopRightCorner}{NewLine}");
            }
            return result;
        }

        private StringBuilder GetColumnNames(IList<string> columnNames, int columnCount, int maxColumnSize)
        {
            var result = new StringBuilder();
            for (var column = 0; column < columnCount; column++)
            {
                result.Append(CellSeparator);
                result.Append(GetFormattedColumnName(columnNames[column], maxColumnSize));
            }
            result.Append($"{CellSeparator}{NewLine}");
            return result;
        }

        private StringBuilder GetInnerBaseForRow(int columnCount, int maxColumnSize)
        {
            var result = new StringBuilder();
            result.Append(InnerLeft);
            for (var j = 1; j <= columnCount; j++)
            {
                result.Append(new string(HorizontalLine, maxColumnSize));
                result.Append((j < columnCount) ? InnerCellSeparator : $"{InnerRight}{NewLine}");
            }
            return result;
        }

        private string GetHeaderOfTheTable(IList<IDataSet> dataSets)
        {
            var maxColumnSize = GetMaxColumnSizeWithPadding(dataSets);
            var columnCount = GetColumnCount(dataSets);
            var columnNames = dataSets[0].GetColumnNames();

            return $"{GetTableTopBorder(columnCount, maxColumnSize)}{GetColumnNames(columnNames, columnCount, maxColumnSize)}{GetInnerBaseForRow(columnCount, maxColumnSize)}";
        }
    }
}

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
        private const char InnerBottomCellSeparator = '╩';
        private const char InnerLeft = '╠';
        private const char InnerRight = '╣';
        private const char InnerCellSeparator = '╬';
        private const char InnerTopCellSeparator = '╦';
        private const char SingleSpace = ' ';

        private const string NewLine = "\n";

        public Print(IView view, IDatabaseManager manager)
        {
            this.view = view;
            this.manager = manager;
        }

        public bool CanProcess(string command)
        {
            return command.StartsWith("print ", StringComparison.OrdinalIgnoreCase);
        }

        public void Process(string command)
        {
            var commandPart = command.Split(SingleSpace);
            if (commandPart.Length != 2)
            {
                throw new ArgumentException($"Incorrect number of parameters. Expected 1, but is {commandPart.Length - 1}");
            }
            tableName = commandPart[1];
            var data = manager.GetTableData(tableName);
            view.Write(GetTableString(data));
        }

        private string GetTableString(IList<IDataSet> data)
        {
            return data.Count == 0 ? GetEmptyTable(tableName) : GetHeaderOfTheTable(data) + GetStringTableData(data);
        }

        private string GetEmptyTable(string tableName)
        {
            var textEmptyTable = $"{SingleSpace}Table '{tableName}' is empty or does not exist{SingleSpace}";

            return $"{GetTableTopBorder(1, textEmptyTable.Length)}{CellSeparator}{textEmptyTable}{CellSeparator}{NewLine}{GetTableBottomBorder(1, textEmptyTable.Length)}";
        }

        private int GetMaxColumnSizeWithPadding(IList<IDataSet> dataSets)
        {
            const int EvenColumnLengthAdjustment = 2;
            const int OddColumnLengthAdjustment = 3;
            int maxColumnSize = dataSets.Count == 0 ? 0 : dataSets
                .SelectMany(dataSet => dataSet.GetValues())
                .Max(value => value.ToString().Length);
            return maxColumnSize += (maxColumnSize % 2 == 0) ? EvenColumnLengthAdjustment : OddColumnLengthAdjustment;
        }

        private string GetStringTableData(IList<IDataSet> dataSets)
        {
            var rowsCount = dataSets.Count;
            var maxColumnSize = GetMaxColumnSizeWithPadding(dataSets);
            var result = new StringBuilder();
            var columnCount = GetColumnCount(dataSets);
            for (var row = 0; row < rowsCount; row++)
            {
                var values = dataSets[row].GetValues().Select(s => s.ToString()).ToList();

                result.Append(GetRowValues(values, columnCount, maxColumnSize));

                result.Append((row < rowsCount - 1)
                    ? GetInnerBaseForRow(columnCount, maxColumnSize)
                    : GetTableBottomBorder(columnCount, maxColumnSize));
            }

            return result.ToString();
        }

        private int GetColumnCount(IList<IDataSet> dataSets)
        {
            return dataSets.Count > 0 ? dataSets[0].GetColumnNames().Count : 0;
        }

        private string GetFormattedColumnName(string columnName, int maxColumnSize)
        {
            var paddingLength = maxColumnSize - columnName.Length;
            var leftPadding = new string(SingleSpace, paddingLength / 2);
            var rightPadding = new string(SingleSpace, (paddingLength + 1) / 2);

            return $"{leftPadding}{columnName}{rightPadding}";
        }

        private StringBuilder GetTableTopBorder(int columnCount, int maxColumnSize)
        {
            var result = new StringBuilder();
            result.Append(TopLeftCorner);
            for (var j = 1; j <= columnCount; j++)
            {
                result.Append(new string(HorizontalLine, maxColumnSize));
                result.Append((j < columnCount) ? InnerTopCellSeparator : $"{TopRightCorner}{NewLine}");
            }
            return result;
        }

        private StringBuilder GetTableBottomBorder(int columnCount, int maxColumnSize)
        {
            var result = new StringBuilder();
            result.Append(BottomLeftCorner);
            for (var j = 1; j <= columnCount; j++)
            {
                result.Append(new string(HorizontalLine, maxColumnSize));
                result.Append((j < columnCount) ? InnerBottomCellSeparator : $"{BottomRightCorner}{NewLine}");
            }
            return result;
        }

        private StringBuilder GetRowValues(IList<string> values, int columnCount, int maxColumnSize)
        {
            var result = new StringBuilder();
            for (var column = 0; column < columnCount; column++)
            {
                result.Append(CellSeparator);
                result.Append(GetFormattedColumnName(values[column], maxColumnSize));
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

            return $"{GetTableTopBorder(columnCount, maxColumnSize)}{GetRowValues(columnNames, columnCount, maxColumnSize)}{GetInnerBaseForRow(columnCount, maxColumnSize)}";
        }
    }
}

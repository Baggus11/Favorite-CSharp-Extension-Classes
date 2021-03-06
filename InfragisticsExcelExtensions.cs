  /// <summary>
    /// This Extension class holds extension methods for Ingragistics.Documents.Excel
    /// that were not implemented well.
    /// </summary>
    public static class IGExcelExtensions
    {
        private static readonly Logger NLogger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Has Sheet
        /// Check to see if an Excel Workbook contains a sheet (by name)
        /// </summary>
        /// <param name="book"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static bool HasSheet(this Workbook book, string sheetName)
        {
            try
            {
                return (book.Worksheets.Count > 0) ?
                    book.Worksheets.Where(s => s.Name == sheetName).FirstOrDefault() != null : false;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Debug.WriteLine(errMsg);
                //MessageBox.Show(errMsg);
                NLogger.Error(errMsg);
                return false;
            }
        }

        /// <summary>
        /// Get Worksheet by name
        /// </summary>
        /// <param name="book"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static Worksheet GetWorksheet(this Workbook book, string sheetName)
        {
            try
            {
                if (book.Worksheets.Count == 0)
                {
                    Debug.WriteLine("Workbook contains no sheets!");
                    return null;
                }
                return book.Worksheets.Where(s => s.Name == sheetName).First();
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Debug.WriteLine(errMsg);
                //MessageBox.Show(errMsg);
                NLogger.Error(errMsg);
                return null;
            }
        }

        /// <summary>
        /// Update Workbook Extension
        /// Change the extension based on the format of the workbook instance
        /// </summary>
        /// <param name="book"></param>
        /// <param name="updatedBookPath"></param>
        /// <returns></returns>
        public static string UpdateWorkbookFileExtension(this Workbook book, string bookPath)
        {
            if (string.IsNullOrWhiteSpace(bookPath))
                return bookPath;
            string parentPath = Path.GetDirectoryName(bookPath);
            string updatedBookPath = "";
            string bookFileNameWoExt = Path.GetFileNameWithoutExtension(bookPath);
            switch (book.CurrentFormat)
            {
                case WorkbookFormat.Excel97To2003:
                    updatedBookPath = Path.Combine(parentPath, $$"{bookFileNameWoExt}.xls");
                    break;
                case WorkbookFormat.Excel97To2003Template:
                    updatedBookPath = Path.Combine(parentPath, $$"{bookFileNameWoExt}.xlt");
                    break;
                case WorkbookFormat.Excel2007:
                    updatedBookPath = Path.Combine(parentPath, $$"{bookFileNameWoExt}.xlsx");
                    break;
                case WorkbookFormat.Excel2007MacroEnabled:
                    updatedBookPath = Path.Combine(parentPath, $$"{bookFileNameWoExt}.xlm");
                    break;
                case WorkbookFormat.Excel2007MacroEnabledTemplate:
                    updatedBookPath = Path.Combine(parentPath, $$"{bookFileNameWoExt}.xltm");
                    break;
                case WorkbookFormat.Excel2007Template:
                    updatedBookPath = Path.Combine(parentPath, $$"{bookFileNameWoExt}.xltx");
                    break;
                case WorkbookFormat.StrictOpenXml:
                    updatedBookPath = Path.Combine(parentPath, $$"{bookFileNameWoExt}.xml"); //this is a complete guess right here!
                    break;
                default:
                    break;
            }
            return updatedBookPath;
        }

        /// <summary>
        /// Replace All
        /// For all Cells, Replace target character with its replacement character
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="target"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static bool ReplaceAll(this Worksheet sheet, char target, char replacement)
        {
            try
            {
                foreach (var row in sheet.Rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        cell.Value = cell.Value.ToString().Replace(target, replacement).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Debug.WriteLine(errMsg);
                NLogger.Error(errMsg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Append Rows
        /// Appends a list of line items to a given worksheet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sheet"></param>
        /// <param name="rows"></param>
        /// <param name="startingRow"></param>
        /// <returns></returns>
        public static bool AppendRows<T>(this Worksheet sheet, List<T> rows, int startingRow = 0)
        {
            if (rows == null || rows.Count == 0)
            {
                string msg = $$"Could not add to WorkSheet '{sheet.Name}'! Rows list is null or empty!";
                Debug.WriteLine(msg);
                NLogger.Info(msg);
                return false;
            }
            bool success = true;
            int rowIndex = startingRow;
            int colIndex = 0;
            foreach (var rowLine in rows)
            {
                colIndex = 0;
                foreach (var field in typeof(T)
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    try
                    {
                        sheet.Rows[rowIndex].Cells[colIndex].Value = field.GetValue(rowLine);
                        colIndex++;
                    }
                    catch (Exception ex)
                    {
                        string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                        Debug.WriteLine(errMsg);
                        NLogger.Error(errMsg);
                        success = false;
                        break;
                    }
                }
                if (!success) break;
                rowIndex++;
            }
            return success;
        }

        /// <summary>
        /// Append Column Headers by referencing the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sheet"></param>
        /// <param name="t"></param>
        /// <param name="insertPositions"></param>
        /// <returns></returns>
        public static bool AppendColumnHeaders<T>(this Worksheet sheet, params int[] insertPositions)
        {
            bool success = true;

            //
            /// Append column headers at the specified row positions
            /// Collisions are not detected
            ////

            List<string> headerNames = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(prop => prop.Name).ToList();

            int colIndex = 0;
            foreach (int rowPos in insertPositions)
            {
                colIndex = 0;
                foreach (var item in headerNames)
                {
                    try
                    {
                        sheet.Rows[rowPos].Cells[colIndex].Value = headerNames[colIndex];
                        colIndex++;
                    }
                    catch (Exception ex)
                    {
                        string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                        Debug.WriteLine(errMsg);
                        NLogger.Error(errMsg);
                        success = false;
                        break;
                    }
                    if (!success) break;
                }
            }
            return success;

        }

        /// <summary>
        /// Append Column Headers from a list of header names
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="headerNames"></param>
        /// <param name="insertPositions"></param>
        /// <returns></returns>
        public static bool AppendColumnHeaders(this Worksheet sheet, List<string> headerNames, params int[] insertPositions)
        {
            bool success = true;
            if (headerNames == null || headerNames.Count == 0) return false;
            foreach (int rowPos in insertPositions)
            {
                for (int colIndex = 0; colIndex < headerNames.Count; colIndex++)
                {
                    try
                    {
                        sheet.Rows[rowPos].Cells[colIndex].Value = headerNames[colIndex];
                    }
                    catch (Exception ex)
                    {
                        string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                        Debug.WriteLine(errMsg);
                        NLogger.Error(errMsg);
                        success = false;
                        break;
                    }
                    if (!success) break;
                }
            }
            return success;
        }

        /// <summary>
        /// Clear Worksheet Data
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="startingRow"></param>
        /// <returns></returns>
        public static bool ClearAllRows(this Worksheet sheet)
        {
            try
            {
                bool success = true;
                var rows = sheet.Rows.ToList();
                foreach (var row in rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        try { cell.Value = ""; }
                        catch (Exception ex)
                        {
                            string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                            Debug.WriteLine(errMsg);
                            NLogger.Error(errMsg);
                            success = false;
                            break;
                        }
                    }
                }
                return success;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Debug.WriteLine(errMsg);
                NLogger.Error(errMsg);
                return false;
            }
        }

        /// <summary>
        /// Clear Cells
        /// Clears all cell data for a given WorksheetRow
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static bool ClearCells(this WorksheetRow row)
        {
            bool success = true;
            foreach (var cell in row.Cells)
            {
                try { cell.Value = ""; }
                catch (Exception ex)
                {
                    string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                    Debug.WriteLine(errMsg);
                    NLogger.Error(errMsg);
                    success = false;
                    break;
                }
            }
            return success;
        }

        /// <summary>
        /// Clear Matching Rows
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static bool ClearMatchingRows(this Worksheet sheet, string filter)
        {
            try
            {
                bool success = true;
                //List<string> unMatchedLines = new List<string>();
                //List<string> matchedLines = new List<string>();
                foreach (var row in sheet.Rows)
                {
                    StringBuilder sb = new StringBuilder();
                    //Derive the full row content as a string:
                    foreach (var cell in row.Cells)
                    {
                        try
                        {
                            sb.Append($$"{cell.Value.ToString()} ");//space to ensure columns values, and append.
                        }
                        catch (Exception ex)
                        {
                            string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                            Debug.WriteLine(errMsg);
                            NLogger.Error(errMsg);
                            return false;
                        }
                    }
                    string line = sb.ToString();
                    //If the new line is a match, clear its cells!
                    if (Regex.IsMatch(line, filter))
                    {
                        try
                        {
                            //matchedLines.Add(line);
                            success = row.ClearCells();
                        }
                        catch (Exception ex)
                        {
                            string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                            Debug.WriteLine(errMsg);
                            //MessageBox.Show(errMsg);
                            NLogger.Error(errMsg);
                            return false;
                        }
                    }
                    else
                    {
                        success = false;
                        //unMatchedLines.Add(line.Trim());
                    }
                }
                //unMatchedLines.Dump("Unmatched");
                //matchedLines.Dump("Matched");
                return success;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Debug.WriteLine(errMsg);
                NLogger.Error(errMsg);
                return false;
            }
        }

        /// <summary>
        /// Autosize the columns using their text as a measuring guide
        /// From: http://www.infragistics.com/community/forums/t/1134.aspx
        /// </summary>
        /// <param name="book">Excel book</param>
        /// <param name="sheet">Excel sheet</param>
        public static void AutoSizeColumns(this Workbook book, Worksheet sheet)
        {
            Dictionary<int, int> colWidths = new Dictionary<int, int>();

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                // loops through all worksheets in the workbook
                foreach (var wksheet in book.Worksheets)
                {
                    // 
                    if (sheet == null || (book.Worksheets.Contains(sheet) && wksheet == sheet))
                    {
                        // couldn't guarantee that the worksheet had columns every time
                        // so had to loop through rows and cells instead to find the correct width
                        foreach (var row in wksheet.Rows)
                        {
                            foreach (var cell in row.Cells)
                            {
                                // get width of cell value
                                string value = cell.Value.ToString();

                                // width w/o spaces is initially retrieved
                                int width = value.Replace(" ", "").Length;

                                // reduces width to compensate for small size of spaces
                                width += (value.Length - width) / 2;

                                // add width to collection for later reference
                                if (!colWidths.ContainsKey(cell.ColumnIndex))
                                    colWidths.Add(cell.ColumnIndex, width);
                                else if (value.ToString().Length > colWidths[cell.ColumnIndex])
                                    colWidths[cell.ColumnIndex] = width;
                            }
                        }
                    }
                    // loops through the new column widths and set each column to the new widths
                    foreach (var colIndex in colWidths.Keys)
                    {
                        Debug.WriteLine($$"Column {colIndex} Width: {colWidths[colIndex] + 2}");
                        wksheet.Columns[colIndex].SetWidth(colWidths[colIndex] + 2, WorksheetColumnWidthUnit.Character);
                    }
                }
            }
        }

        /// <summary>
        /// Format Columns As
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="formatStr"></param>
        /// <param name="columns"></param>
        public static void FormatColumnsAs(this Worksheet sheet, string formatStr, params int[] columns)
        {
            try
            {
                foreach (int colIndex in columns)
                    sheet.Columns[colIndex].CellFormat.FormatString = formatStr;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("{0}: {1}", MethodBase.GetCurrentMethod().Name, ex.ToString());
                Debug.WriteLine(errMsg);
                NLogger.Error(errMsg);
            }
        }

        /// <summary>
        /// Append a dataset to given workbook
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="dataSet"></param>
        public static void AppendDataSet(this Workbook workbook, DataSet dataSet)
        {
            try
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    // Create the worksheet to represent this data table
                    Worksheet worksheet = workbook.Worksheets.Add(table.TableName);

                    // Create column headers for each column
                    for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                    {
                        worksheet.Rows[0].Cells[columnIndex].Value = table.Columns[columnIndex].ColumnName;
                    }

                    // Starting at row index 1, copy all data rows in
                    // the data table to the worksheet
                    int rowIndex = 1;
                    foreach (DataRow dataRow in table.Rows)
                    {
                        WorksheetRow row = worksheet.Rows[rowIndex++];

                        for (int columnIndex = 0; columnIndex < dataRow.ItemArray.Length; columnIndex++)
                        {
                            row.Cells[columnIndex].Value = dataRow.ItemArray[columnIndex];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errMsg = string.Format($$"{MethodBase.GetCurrentMethod().Name}: {ex.ToString()}");
                Debug.WriteLine(errMsg);
                NLogger.Error(errMsg);
            }
        }

        /// <summary>
        /// Set Background of given columns
        /// (defaults to Solid fill style)
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="color"></param>
        /// <param name="columns"></param>
        public static void SetColumnBackgroundColor(this Worksheet sheet, Color color, params int[] columns)
        {
            try
            {
                foreach (int colIndex in columns)
                    sheet.Columns[colIndex].CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), null, FillPatternStyle.Solid);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format($$"{MethodBase.GetCurrentMethod().Name}: {ex.ToString()}");
                Debug.WriteLine(errMsg);
                NLogger.Error(errMsg);
            }
        }


        /// <summary>
        /// Set Background of given rows    
        /// (defaults to Solid fill style)
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="color"></param>
        /// <param name="rows"></param>
        public static void SetColumnHeaderBackgroundColor(this Worksheet sheet, Color color, params int[] columns)
        {
            try
            {
                foreach (int colIndex in columns)
                    sheet.Rows[0].Cells[colIndex].CellFormat.Fill = new CellFillPattern(new WorkbookColorInfo(color), null, FillPatternStyle.Solid);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format($$"{MethodBase.GetCurrentMethod().Name}: {ex.ToString()}");
                Debug.WriteLine(errMsg);
                NLogger.Error(errMsg);
            }
        }

        /// <summary>
        /// Set Worksheet Text Alignment
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="alignment"></param>
        public static void SetWorkSheetCellAlignment(this Worksheet sheet, HorizontalCellAlignment alignment)
        {
            foreach (var column in sheet.Columns)
            {
                column.CellFormat.Alignment = alignment;
            }
        }
    }
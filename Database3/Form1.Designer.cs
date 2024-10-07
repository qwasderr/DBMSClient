using DatabaseServiceReference;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
namespace Database3
{
    partial class Form1 : Form
    {
        private DatabaseServiceClient _serviceClient;
        private DatabaseServiceReference.Database _database;
        private DatabaseServiceReference.Table _selectedTable;
        private DataGridView _tableDataGridView;
        private ListBox _tableListBox;
        private TextBox _tableNameTextBox;
        private ComboBox _schemaComboBox;
        private List<DataGridViewCell> _modifiedCells = new List<DataGridViewCell>();
        private List<TextBox> _inputTextBoxes;
        private List<ComboBox> _inputComboBoxes;
        private List<DateTimePicker> _inputDatePickers;
        private List<Label> _inputLabels = new List<Label>();
        private System.ComponentModel.IContainer components = null;
        private Button _differenceButton;
        private ComboBox _tableComboBox1;
        private ComboBox _tableComboBox2;

        private void InitializeDifference()
        {
            _differenceButton = new Button
            {
                Text = "Calculate Difference",
                Location = new System.Drawing.Point(700, 100),
                Width = 200,
                Height = 30
            };
            _differenceButton.Click += DifferenceButton_Click;
            this.Controls.Add(_differenceButton);

            _tableComboBox1 = new ComboBox
            {
                Location = new System.Drawing.Point(700, 150),
                Width = 200
            };
            this.Controls.Add(_tableComboBox1);

            _tableComboBox2 = new ComboBox
            {
                Location = new System.Drawing.Point(700, 200),
                Width = 200
            };
            this.Controls.Add(_tableComboBox2);

            UpdateTableComboBoxes();
        }
        private void UpdateTableComboBoxes()
        {
            if (_database != null)
            {
                _tableComboBox1.Items.Clear();
                _tableComboBox2.Items.Clear();

                foreach (var table in _database.Tables)
                {
                    _tableComboBox1.Items.Add(table.Name);
                    _tableComboBox2.Items.Add(table.Name);
                }
            }
        }
        private async void DifferenceButton_Click(object sender, EventArgs e)
        {
            var tableName1 = _tableComboBox1.SelectedItem?.ToString();
            var tableName2 = _tableComboBox2.SelectedItem?.ToString();

            if (tableName1 == null || tableName2 == null)
            {
                MessageBox.Show("Select two tables.");
                return;
            }

            try
            {
                
                string db = JsonSerializer.Serialize(_database);
                //var differenceTable = await _serviceClient.GetTableDifferenceAsync(_database, tableName1, tableName2);
                string dt = await _serviceClient.GetTableDifference2Async(db, tableName1, tableName2);
                var differenceTable = JsonSerializer.Deserialize<DatabaseServiceReference.Table>(dt);
                if (differenceTable != null)
                {
                    MessageBox.Show($"Difference table created: {differenceTable.Name}");

                   
                    //_database = await _serviceClient.AddTableAsync(_database, differenceTable.Name, differenceTable);
                    string diftable = JsonSerializer.Serialize(differenceTable);
                    string resDB = await _serviceClient.AddTable2Async(db, differenceTable.Name, diftable);
                    _database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(resDB);
                    
                    UpdateTableComboBoxes();
                    UpdateTableList();
                }
                else
                {
                    MessageBox.Show("Failed to calculate difference. Table not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate difference: {ex.Message}");
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        /// 
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // Form1
            // 
            ClientSize = new Size(565, 376);
            Name = "Form1";
            Text = "Form1";
           
            //Load += Form1_Load;
            ResumeLayout(false);
        }

        private void TableDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var changedCell = _tableDataGridView[e.ColumnIndex, e.RowIndex];
            if (!_modifiedCells.Contains(changedCell))
            {
                _modifiedCells.Add(changedCell);
            }
        }

       private bool ValidateCell(object value, DatabaseServiceReference.Field field)
        {
            string fieldType = field.Type.ToLower();

            switch (fieldType)
            {
                case "int":
                    if (!int.TryParse(value?.ToString(), out _))
                    {
                        return false;
                    }
                    break;

                case "real":
                    if (!float.TryParse(value?.ToString(), out _))
                    {
                        return false;
                    }
                    break;

                case "char":
                    if (value?.ToString().Length != 1)
                    {
                        return false;
                    }
                    break;

                case "string":
                    if (string.IsNullOrWhiteSpace(value?.ToString()))
                    {
                        return false;
                    }
                    break;

                case "time":

                    if (!TimeSpan.TryParseExact(value?.ToString(), @"hh\:mm\:ss",
                        System.Globalization.CultureInfo.InvariantCulture, out _))
                    {
                        return false;
                    }
                    break;

                case "timeint":
                    if (TimeSpan.TryParseExact(value?.ToString(), @"hh\:mm\:ss",
                        System.Globalization.CultureInfo.InvariantCulture, out TimeSpan timeValue))
                    {
                        if (field.LowerBound.HasValue && field.UpperBound.HasValue)
                        {
                            if (timeValue < field.LowerBound.Value || timeValue > field.UpperBound.Value)
                            {
                                MessageBox.Show($"Time value for field '{field.Name}' must be between {field.LowerBound.Value} and {field.UpperBound.Value}.");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Invalid time format in field '{field.Name}'. Please enter time in HH:MM format.");
                        return false;
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }

        private void ResetModifiedCells()
        {
            _modifiedCells.Clear();
        }
        #endregion

        private async void DeleteTableButton_Click(object sender, EventArgs e)
        {
            if (_selectedTable != null)
            {
                var result = MessageBox.Show("Are you sure?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        
                        string jsonContent = JsonSerializer.Serialize(_database);
                        //_database = await _serviceClient.DeleteTableAsync(_database, _selectedTable.Name);
                        string db = await _serviceClient.DeleteTable2Async(jsonContent, _selectedTable.Name);
                        _database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(db);
                        _selectedTable = null;

                        
                        UpdateTableList();
                        _tableDataGridView.Rows.Clear();
                        _tableDataGridView.Columns.Clear();
                        MessageBox.Show("Success.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete table: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Choose the Table.");
            }
        }
        private async void CreateSchemaButton_Click(object sender, EventArgs e)
        {
            
            using (var createSchemaForm = new SchemaForm(_database, _serviceClient))
            {
                if (createSchemaForm.ShowDialog() == DialogResult.OK)
                {
                    var newSchema = createSchemaForm.NewSchema;
                    //_database.AddSchema(newSchema);
                    //_database = await _serviceClient.AddSchemaAsync(_database, newSchema);
                    string jsonContent = JsonSerializer.Serialize(_database);
                    string db = await _serviceClient.AddSchema2Async(jsonContent, newSchema);
                    _database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(db);
                    PopulateSchemaComboBox();
                    
                }
            }
        }
        private void PopulateSchemaComboBox()
        {
            _schemaComboBox.Items.Clear();

            foreach (var schema in _database.Schemas)
            {
                if (schema != null)
                {
                    _schemaComboBox.Items.Add(schema.Name);
                }
            }

            if (_schemaComboBox.Items.Count > 0)
            {
                _schemaComboBox.SelectedIndex = 0;
            }
        }
        private void InitializeElements()
        {
            _inputTextBoxes = new List<TextBox>();
            _inputComboBoxes = new List<ComboBox>();
            _inputDatePickers = new List<DateTimePicker>();
            _inputLabels = new List<Label>();


            Button createSchemaButton = new Button
            {
                Text = "Create Schema",
                Location = new Point(540, 20),
                Width = 200,
                Height = 30
            };
            createSchemaButton.Click += CreateSchemaButton_Click;
            this.Controls.Add(createSchemaButton);

            Button deleteTableButton = new Button
            {
                Text = "Delete the table",
                Location = new Point(400, 270),
                Width = 200,
                Height = 30
            };
            deleteTableButton.Click += DeleteTableButton_Click;
            this.Controls.Add(deleteTableButton);

            Button saveDatabaseButton = new Button()
            {
                Text = "Save Database",
                Height = 30,
                Location = new System.Drawing.Point(300, 270)
            };
            saveDatabaseButton.Click += SaveDatabaseButton_Click;
            this.Controls.Add(saveDatabaseButton);

            _schemaComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(20, 20),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateSchemaComboBox();
            _schemaComboBox.SelectedIndexChanged += SchemaComboBox_SelectedIndexChanged;
            this.Controls.Add(_schemaComboBox);



            _tableNameTextBox = new TextBox
            {
                Location = new System.Drawing.Point(230, 20),
                Width = 200
            };
            this.Controls.Add(_tableNameTextBox);

            Button createTableButton = new Button
            {
                Text = "Create Table",
                Location = new System.Drawing.Point(440, 20),
                Width = 100,
                Height = 30
            };
            createTableButton.Click += async (sender, args) =>
            {
                try
                {
                    string tableName = _tableNameTextBox.Text;
                    string selectedSchema = _schemaComboBox.SelectedItem?.ToString();

                    if (!string.IsNullOrWhiteSpace(tableName) && !string.IsNullOrEmpty(selectedSchema))
                    {
                        string jsonContent = JsonSerializer.Serialize(_database);
                        //DatabaseServiceReference.Schema schema = GetSchemaByName(selectedSchema);
                        //DatabaseServiceReference.Schema schema = await _serviceClient.GetSchemaByNameAsync(_database,selectedSchema);
                        string schema2 = await _serviceClient.GetSchemaByName2Async(jsonContent, selectedSchema);
                        DatabaseServiceReference.Schema schema = JsonSerializer.Deserialize<DatabaseServiceReference.Schema>(schema2);
                        if (schema != null)
                        {
                            
                            //object db = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(jsonContent);
                            //_database = await _serviceClient.AddRowAsync(_database, name, newRow);
                            string db = await _serviceClient.CreateTable2Async(jsonContent, tableName, schema);
                            _database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(db);

                            //_database = await _serviceClient.CreateTableAsync(_database, tableName, schema);

                            UpdateTableList();
                            UpdateTableComboBoxes();
                            MessageBox.Show("Table created successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Invalid schema selected.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Enter a table name and select a schema.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating table: {ex.Message}");
                }
            };
            this.Controls.Add(createTableButton);


            _tableListBox = new ListBox
            {
                Location = new System.Drawing.Point(20, 60),
                Width = 200,
                Height = 200
            };
            _tableListBox.SelectedIndexChanged += async (sender, args) =>
            {
                if (_tableListBox.SelectedItem != null)
                {
                    //_selectedTable = _database.GetTable(_tableListBox.SelectedItem.ToString());
                    string jsonContent = JsonSerializer.Serialize(_database);
                    var i = 0;
                    //_selectedTable = await _serviceClient.GetTableAsync(_database, _tableListBox.SelectedItem.ToString());
                    string table = await _serviceClient.GetTable2Async(jsonContent, _tableListBox.SelectedItem.ToString());
                    _selectedTable = JsonSerializer.Deserialize<DatabaseServiceReference.Table>(table);
                    UpdateTableRows();
                }
            };
            this.Controls.Add(_tableListBox);
            UpdateTableList();


            _tableDataGridView = new DataGridView
            {
                Location = new System.Drawing.Point(230, 60),
                Width = 400,
                Height = 200,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(_tableDataGridView);
            _tableDataGridView.CellValueChanged += TableDataGridView_CellValueChanged;

            Button addRowButton = new Button
            {
                Text = "Add Row",
                Location = new System.Drawing.Point(20, 270),
                Width = 100,
                Height = 30
            };
            addRowButton.Click += async (sender, args) =>
            {
                if (_selectedTable != null)
                {
                    try
                    {
                        List<DatabaseServiceReference.Value> values = new List<DatabaseServiceReference.Value>();
                        int textBoxIndex = 0;
                        bool isValid = true;

                        foreach (var field in _selectedTable.Schema.Fields)
                        {
                            if (field.Name == "ID")
                            {
                                continue; 
                            }

                            object inputValue = null;

                            if (textBoxIndex < _inputTextBoxes.Count)
                            {
                                var textBox = _inputTextBoxes[textBoxIndex];
                                string inputText = textBox.Text;

                                switch (field.Type.ToLower())
                                {
                                    case "time":
                                        
                                        if (TimeSpan.TryParseExact(inputText, @"hh\:mm\:ss",
                                            System.Globalization.CultureInfo.InvariantCulture, out TimeSpan parsedTime))
                                        {
                                            inputValue = parsedTime;
                                            DatabaseServiceReference.Value temp6 = new DatabaseServiceReference.Value();
                                            temp6.FieldValue = parsedTime;
                                            //values.Add(new Value(parsedTime));
                                            values.Add(temp6);
                                        }
                                        else
                                        {
                                            isValid = false;
                                            MessageBox.Show($"Invalid time format in field '{field.Name}'. Enter time in format HH:MM:SS.");
                                            return;
                                        }
                                        break;

                                    case "timeint":
                                       
                                        if (TimeSpan.TryParseExact(inputText, @"hh\:mm\:ss",
                                            System.Globalization.CultureInfo.InvariantCulture, out TimeSpan parsedTimeInt))
                                        {
                                            TimeSpan? startTime = field.LowerBound;  
                                            TimeSpan? endTime = field.UpperBound;   

                                            if (parsedTimeInt < startTime || parsedTimeInt > endTime)
                                            {
                                                isValid = false;
                                                MessageBox.Show($"Invalid timeint value in field '{field.Name}'. Enter a time between {field.LowerBound} and {field.UpperBound}.");
                                                return;
                                            }
                                            inputValue = parsedTimeInt;
                                            DatabaseServiceReference.Value temp1 = new DatabaseServiceReference.Value();
                                            temp1.FieldValue = parsedTimeInt;
                                            //values.Add(new Value(parsedTimeInt));
                                            values.Add(temp1);
                                        }
                                        else
                                        {
                                            isValid = false;
                                            MessageBox.Show($"Invalid time format in field '{field.Name}'. Enter time in format HH:MM:SS.");
                                            return;
                                        }
                                        break;

                                    case "string":
                                        
                                        inputValue = inputText;
                                        DatabaseServiceReference.Value temp2 = new DatabaseServiceReference.Value();
                                        temp2.FieldValue = inputText;
                                        //values.Add(new Value(inputText));
                                        values.Add(temp2);
                                        break;

                                    case "char":
                                        
                                        if (inputText.Length == 1)
                                        {
                                            inputValue = inputText[0];
                                            DatabaseServiceReference.Value temp3 = new DatabaseServiceReference.Value();
                                            temp3.FieldValue = inputText[0];
                                            //values.Add(new Value(inputText[0]));
                                            values.Add(temp3);
                                        }
                                        else
                                        {
                                            isValid = false;
                                            MessageBox.Show($"Invalid char value in field '{field.Name}'. Enter a single character.");
                                            return;
                                        }
                                        break;

                                    case "int":
                                        
                                        if (int.TryParse(inputText, out int intValue))
                                        {
                                            inputValue = intValue;
                                            DatabaseServiceReference.Value temp4 = new DatabaseServiceReference.Value();
                                            temp4.FieldValue = intValue;
                                            //values.Add(new Value(intValue));
                                            values.Add(temp4);
                                        }
                                        else
                                        {
                                            isValid = false;
                                            MessageBox.Show($"Invalid integer value in field '{field.Name}'.");
                                            return;
                                        }
                                        break;

                                    case "real":
                                        
                                        if (double.TryParse(inputText, out double doubleValue))
                                        {
                                            inputValue = doubleValue;
                                            DatabaseServiceReference.Value temp5 = new DatabaseServiceReference.Value();
                                            temp5.FieldValue = doubleValue;
                                            //values.Add(new Value(doubleValue));
                                            values.Add(temp5);
                                        }
                                        else
                                        {
                                            isValid = false;
                                            MessageBox.Show($"Invalid real number value in field '{field.Name}'.");
                                            return;
                                        }
                                        break;

                                    default:
                                        isValid = false;
                                        MessageBox.Show($"Unknown field type '{field.Type}' in field '{field.Name}'.");
                                        return;
                                }

                               
                                if (!ValidateCell(inputValue, field))
                                {
                                    isValid = false;
                                    MessageBox.Show($"Invalid value in field '{field.Name}'.");
                                    return;
                                }

                                textBoxIndex++;
                            }
                        }

                        
                        if (isValid)
                        {
                            var newRow = new DatabaseServiceReference.Row();
                            newRow.Values = values.ToArray();
                            var name=_selectedTable.Name;
                            //_selectedTable.AddRow(newRow);
                            string jsonContent = JsonSerializer.Serialize(_database);
                            //object db = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(jsonContent);
                            //_database = await _serviceClient.AddRowAsync(_database, name, newRow);
                            string db = await _serviceClient.AddRow2Async(jsonContent, name, newRow);
                            _database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(db);
                            string jsonContent2 = JsonSerializer.Serialize(_database);
                            var i = 0;
                            string table = await _serviceClient.GetTable2Async(jsonContent2, name);
                            _selectedTable = JsonSerializer.Deserialize<DatabaseServiceReference.Table>(table);
                            //_selectedTable = await _serviceClient.GetTableAsync(_database, name);
                            UpdateTableRows(); 
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add row: {ex.Message}");
                    }
                }
            };

            this.Controls.Add(addRowButton);


            Button deleteRowButton = new Button
            {
                Text = "Delete Row",
                Location = new System.Drawing.Point(130, 270),
                Width = 100,
                Height = 30
            };
            deleteRowButton.Click += async (sender, args) =>
            {
                if (_selectedTable != null && _tableDataGridView.SelectedRows.Count > 0)
                {
                    int selectedIndex = _tableDataGridView.SelectedRows[0].Index;
                    var name= _selectedTable.Name;
                    string jsonContent = JsonSerializer.Serialize(_database);
                    //_database = await _serviceClient.DeleteRowAsync(_database, name, selectedIndex);
                    string db = await _serviceClient.DeleteRow2Async(jsonContent, name, selectedIndex);
                    _database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(db);
                    string jsonContent2 = JsonSerializer.Serialize(_database);
                    string table = await _serviceClient.GetTable2Async(jsonContent2, name);
                    _selectedTable = JsonSerializer.Deserialize<DatabaseServiceReference.Table>(table);
                    // _selectedTable = await _serviceClient.GetTableAsync(_database, name);
                    //_selectedTable.DeleteRow(selectedIndex);
                    UpdateTableRows();
                }
            };
            this.Controls.Add(deleteRowButton);
        }

        public Database Convert2(string db)
        {
            return JsonSerializer.Deserialize<Database>(db);
        }
        private void SchemaComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTableRows();
        }

        private void UpdateTableList()
        {

            _tableListBox.Items.Clear();
            foreach (var table in _database.Tables)
            {
                _tableListBox.Items.Add(table.Name);
            }
        }


        private async Task<int> GetRowValueCountAsync2(DatabaseServiceReference.Value[] values)
        {
            return await _serviceClient.CountVAsync(values);
        }
        private async Task<int> GetRowValueCountAsync(DatabaseServiceReference.Value[] values)
        {
            string jsonContent = JsonSerializer.Serialize(values);
            return await _serviceClient.CountV2Async(jsonContent);
        }

        private async void UpdateTableRows()
        {
            if (_selectedTable != null)
            {
               
                _tableDataGridView.Columns.Clear();
                foreach (var control in _inputTextBoxes)
                {
                    this.Controls.Remove(control);
                }
                foreach (var control in _inputComboBoxes)
                {
                    this.Controls.Remove(control);
                }
                foreach (var control in _inputLabels)
                {
                    this.Controls.Remove(control);
                }

                _inputTextBoxes.Clear();
                _inputComboBoxes.Clear();
                _inputLabels.Clear();

                int yOffset = 300;

                
                var idColumn = new DataGridViewTextBoxColumn
                {
                    HeaderText = "ID",
                    Name = "ID",
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };
                _tableDataGridView.Columns.Add(idColumn);

               
                foreach (var field in _selectedTable.Schema.Fields)
                {
                    if (field.Name != "ID")
                    {
                        _tableDataGridView.Columns.Add(field.Name, field.Name);

                       
                        var label = new Label
                        {
                            Text = field.Name,
                            Location = new System.Drawing.Point(20, yOffset),
                            Width = 100
                        };
                        _inputLabels.Add(label);
                        this.Controls.Add(label);

                        
                        Control inputControl;
                        switch (field.Type.ToLower())
                        {
                            case "time":
                                
                                var timeTextBox = new TextBox
                                {
                                    Location = new System.Drawing.Point(130, yOffset),
                                    Width = 200,
                                    PlaceholderText = "HH:MM:SS"
                                };
                                _inputTextBoxes.Add(timeTextBox);
                                inputControl = timeTextBox;
                                break;

                            case "timeint":
                                
                                var timeIntTextBox = new TextBox
                                {
                                    Location = new System.Drawing.Point(130, yOffset),
                                    Width = 200,
                                    PlaceholderText = "HH:MM:SS"
                                };
                                _inputTextBoxes.Add(timeIntTextBox);
                                inputControl = timeIntTextBox;
                                break;

                            case "string":
                            case "int":
                            case "real":
                            case "char":
                                var textBox = new TextBox
                                {
                                    Location = new System.Drawing.Point(130, yOffset),
                                    Width = 200
                                };
                                _inputTextBoxes.Add(textBox);
                                inputControl = textBox;
                                break;

                            default:
                                continue;
                        }

                        this.Controls.Add(inputControl);
                        yOffset += 30;
                    }
                }

                
                _tableDataGridView.Rows.Clear();
                foreach (var row in _selectedTable.Rows)
                {
                    int count = await GetRowValueCountAsync(row.Values);
                    //var rowValues = new object[row.Values.Count];
                    var rowValues = new object[count];
                    for (int i = 0; i < count; i++)
                    {
                        //var fieldType = _selectedTable.Schema.Fields[i].Type.ToLower();
                        var fieldValue = row.Values[i].FieldValue;



                        if (fieldValue is TimeSpan timeSpanValue)
                        {
                            rowValues[i] = timeSpanValue.ToString(@"hh\:mm\:ss");
                        }
                        else
                        {
                            rowValues[i] = fieldValue;
                        }


                    }

                    _tableDataGridView.Rows.Add(rowValues);
                }
            }
        }

        private async Task<int> ToInt(DatabaseServiceReference.Value value)
        {
            string jsonContent = JsonSerializer.Serialize(value);
            //return await _serviceClient.ToIntAsync(value);
            return await _serviceClient.ToInt2Async(jsonContent);

        }

        private async Task<DatabaseServiceReference.Row> FindExistingRowAsync(int rowId)
        {
            foreach (var row in _selectedTable.Rows)
            {
                int rowValue = await ToInt(row.Values[0]);
                if (rowValue == rowId)
                {
                    return row;
                }
            }
            return null;
        }
        private int RowIndex(DatabaseServiceReference.Row row, DatabaseServiceReference.Table table) {
            for (int i = 0; i < table.Rows.Length; i++)
            {
                if (table.Rows[i] == row) return i;
            }
            return -1;
        }
        private int TableIndex(string tableName, DatabaseServiceReference.Database db) { 
            for (int i=0; i<db.Tables.Length; i++)
            {
                if (db.Tables[i].Schema.Name == tableName) return i;
            }
            return -1;
        }
        private async void SaveDatabaseButton_Click(object sender, EventArgs e)
        {
            bool isValid = true;

            if (_selectedTable != null)
            {
                string tableName = _selectedTable.Schema.Name;

                foreach (DataGridViewRow row in _tableDataGridView.Rows)
                {
                    if (row.IsNewRow) continue;

                    var idCell = row.Cells["ID"].Value;
                    int rowId;

                    if (idCell is System.Text.Json.JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == JsonValueKind.Number)
                        {
                            rowId = jsonElement.GetInt32();
                        }
                        else
                        {
                            MessageBox.Show("Invalid ID value.");
                            continue;
                        }
                    }
                    else
                    {
                        rowId = Convert.ToInt32(idCell);
                    }

                    //var existingRow = _selectedTable.Rows.FirstOrDefault(r => r.Values[0].ToInt() == rowId);
                    var existingRow = await FindExistingRowAsync(rowId);
                    var Rowidx = RowIndex(existingRow,_selectedTable);
                    if (existingRow != null)
                    {
                        List<DatabaseServiceReference.Value> updatedValues = new List<DatabaseServiceReference.Value>();
                        //int countV = 0;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            var a = _modifiedCells;
                            string columnName = _tableDataGridView.Columns[cell.ColumnIndex].Name;
                            object cellValue = cell.Value;
                           
                                var field = _selectedTable.Schema.Fields.FirstOrDefault(f => f.Name == columnName);

                                if (field == null || !ValidateCell(cellValue, field))
                                {
                                    isValid = false;
                                    cell.Style.BackColor = Color.Red;
                                }
                                else
                                {
                                    cell.Style.BackColor = Color.White;
                                DatabaseServiceReference.Value v = new DatabaseServiceReference.Value();
                                v.FieldValue = cellValue;
                                updatedValues.Add(v);
                                
                                    //updatedValues[countV]=v;
                                    //++countV;
                                }
                            
                        }

                        if (isValid)
                        {
                            existingRow.Values = updatedValues.ToArray();
                            int TableIdx = TableIndex(tableName, _database);
                            _selectedTable.Rows[Rowidx]=existingRow;
                            _database.Tables[TableIdx]=_selectedTable;
                        }
                    }
                }
            }

            if (isValid)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Database files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //_database.SaveToDisk(saveFileDialog.FileName);
                    string jsonContent = JsonSerializer.Serialize(_database);
                    //await _serviceClient.SaveDatabaseAsync(_database, saveFileDialog.FileName);
                    await _serviceClient.SaveDatabase2Async(jsonContent, saveFileDialog.FileName);
                    //_database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(db);
                    MessageBox.Show("Database saved.");
                }
            }
            else
            {
                MessageBox.Show("Invalid data.");
            }
        }
    }

   
}

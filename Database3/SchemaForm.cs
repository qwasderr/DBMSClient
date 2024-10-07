using DatabaseServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Database3
{
    public partial class SchemaForm : Form
    {

        private DatabaseServiceClient _serviceClient;
        public DatabaseServiceReference.Schema NewSchema { get; private set; }
        private DatabaseServiceReference.Database _database;
        public SchemaForm(DatabaseServiceReference.Database database, DatabaseServiceClient sc)
        {
            InitializeComponent();
            InitializeTimeintControls();
            NewSchema = null;
            _database = database;
            _serviceClient = sc;
        }
        private void btnAddField_Click(object sender, EventArgs e)
        {
            string fieldName = txtFieldName.Text;
            string fieldType = cmbFieldType.SelectedItem.ToString().ToLower();

            Field newField = new Field(fieldName, fieldType);

            if (fieldType == "timeint")
            {
                if (TimeSpan.TryParseExact(lowerBoundTextBox.Text, @"hh\:mm\:ss",
                    System.Globalization.CultureInfo.InvariantCulture, out TimeSpan lowerBound) &&
                    TimeSpan.TryParseExact(upperBoundTextBox.Text, @"hh\:mm\:ss",
                    System.Globalization.CultureInfo.InvariantCulture, out TimeSpan upperBound))
                {
                    newField.LowerBound = lowerBound;
                    newField.UpperBound = upperBound;
                }
                else
                {
                    MessageBox.Show("Invalid time bounds. Please enter valid time intervals in HH:MM format.");
                    return;
                }
            }
            //lstFields.Items.Add($"{fieldName} ({fieldType})");
            lstFields.Items.Add($"{fieldName} ({fieldType}) {newField.LowerBound} {newField.UpperBound}");
            txtFieldName.Clear();
            //cmbFieldType.SelectedIndex = -1;
            // _schema.Fields.Add(newField);
        }
        private void btnAddField_Click2(object sender, EventArgs e)
        {
            
            string fieldName = txtFieldName.Text;
            string fieldType = cmbFieldType.SelectedItem.ToString();

            if (string.IsNullOrWhiteSpace(fieldName) || fieldType == null)
            {
                MessageBox.Show("Please provide both field name and type.");
                return;
            }

            lstFields.Items.Add($"{fieldName} ({fieldType})");
            txtFieldName.Clear();
            cmbFieldType.SelectedIndex = -1;
        }
        public TimeSpan? ConvertStringToTimeSpan(string timeString)
        {
            if (TimeSpan.TryParseExact(timeString, @"hh\:mm\:ss",
                System.Globalization.CultureInfo.InvariantCulture, out TimeSpan timeSpanResult))
            {
                return timeSpanResult;
            }
            else
            {
                return null;
            }
        }

        private async void btnSaveSchema_Click(object sender, EventArgs e)
        {
            string tableName = txtTableName.Text;
            if (string.IsNullOrWhiteSpace(tableName) || lstFields.Items.Count == 0)
            {
                MessageBox.Show("Please provide a schema name and at least one field.");
                return;
            }

           
            var fields = new List<DatabaseServiceReference.Field>();
            DatabaseServiceReference.Field fd = new DatabaseServiceReference.Field();
            fd.Name = "ID";
            fd.Type = "int";
            fields.Add(fd);
            foreach (var item in lstFields.Items)
            {
                var parts = item.ToString().Split(' ');
                if (parts[2] == "")
                {
                    DatabaseServiceReference.Field fd2= new DatabaseServiceReference.Field();
                    fd2.Name = parts[0];
                    fd2.Type = parts[1].Trim('(', ')');
                    //fields.Add(new DatabaseServiceReference.Field(parts[0], parts[1].Trim('(', ')')));
                    fields.Add(fd2);
                }
                else if (parts.Length == 4)
                {
                    DatabaseServiceReference.Field fd3 = new DatabaseServiceReference.Field();
                    fd3.Name = parts[0];
                    fd3.Type = parts[1].Trim('(', ')');
                    fd3.LowerBound = ConvertStringToTimeSpan(parts[2]);
                    fd3.UpperBound = ConvertStringToTimeSpan(parts[3]);
                    //fields.Add(new DatabaseServiceReference.Field(parts[0], parts[1].Trim('(', ')'), parts[2], parts[3]));
                    fields.Add(fd3);
                }
            }

            //var newSchema = new DatabaseServiceReference.Schema(fields, tableName);
            var newSchema = new DatabaseServiceReference.Schema();
            newSchema.Fields = fields.ToArray();
            newSchema.Name=tableName;
            NewSchema=newSchema;
            //_database.AddSchema(newSchema);
            string jsonContent = JsonSerializer.Serialize(_database);
            string db = await _serviceClient.AddSchema2Async(jsonContent, newSchema);
            //_database = await _serviceClient.AddSchemaAsync(_database, newSchema);
            _database = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(db);
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Database files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Database File";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //_database.SaveToDisk(saveFileDialog.FileName);
                    //_database.SaveToDisk(saveFileDialog.FileName);
                    string jsonContent2 = JsonSerializer.Serialize(_database);
                    //await _serviceClient.SaveDatabaseAsync(_database, saveFileDialog.FileName);
                    await _serviceClient.SaveDatabase2Async(jsonContent2, saveFileDialog.FileName);
                    //await _serviceClient.SaveDatabaseAsync(_database, saveFileDialog.FileName);
                    MessageBox.Show("Database saved.");

                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }
    }
}

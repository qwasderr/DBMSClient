using DatabaseServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Database3
{
    public partial class StartForm : Form
    {
        private DatabaseServiceClient _serviceClient;
        public StartForm()
        {
            InitializeComponent();
            InitializeUI();
            var binding = new BasicHttpBinding();
            binding.SendTimeout = TimeSpan.FromMinutes(2);
            _serviceClient = new DatabaseServiceClient(binding, new EndpointAddress("http://localhost:53391/DatabaseService.svc"));

        }

        private void InitializeUI()
        {
            this.Text = "Select Action";
            this.Size = new Size(300, 200);

            Button createDatabaseButton = new Button
            {
                Text = "Create Database",
                Location = new Point(50, 50),
                Width = 200,
                Height = 40
            };
            createDatabaseButton.Click += CreateDatabaseButton_Click;
            this.Controls.Add(createDatabaseButton);

            Button loadDatabaseButton = new Button
            {
                Text = "Upload Database",
                Location = new Point(50, 100),
                Width = 200,
                Height = 40
            };
            loadDatabaseButton.Click += LoadDatabaseButton_Click;
            this.Controls.Add(loadDatabaseButton);
        }

        private async void CreateDatabaseButton_Click(object sender, EventArgs e)
        {
            using (var inputDialog = new InputDialog("Enter Database Name"))
            {
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    string databaseName = inputDialog.InputText;

                    if (!string.IsNullOrEmpty(databaseName))
                    {
                        try
                        {
                            
                            DatabaseServiceReference.Database db = await _serviceClient.CreateDatabaseAsync(databaseName);
                            //db.Name= databaseName;
                            //var mainForm = new Form1(new DatabaseServiceReference.Database(databaseName));
                            var mainForm = new Form1(db);
                            mainForm.Show();
                            this.Hide();
                        }
                        
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error: {ex.Message}");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Database name can't be empty.");
                    }
                }
            }
        }
        private async Task<bool> DatabaseExistsAsync(string databaseName)
        {
            return await _serviceClient.DatabaseExistsAsync(databaseName);
        }
        private async void LoadDatabaseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Database files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    
                    //DatabaseServiceReference.Database db = await _serviceClient.LoadDatabaseAsync(openFileDialog.FileName);
                    string obj = await _serviceClient.LoadDatabase2Async(openFileDialog.FileName);
                    var db = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(obj);

                    if (db != null)
                    {
                        MessageBox.Show("Database loaded successfully.");
                        var mainForm = new Form1(db);
                        mainForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Failed to load the database.");
                    }
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show($"File not found: {ex.Message}");
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show($"The operation timed out: {ex.Message}");
                }
                catch (CommunicationException ex)
                {
                    

                        string json = File.ReadAllText(openFileDialog.FileName);
                        
                        var loadedDatabase = JsonSerializer.Deserialize<DatabaseServiceReference.Database>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                         var mainForm = new Form1(loadedDatabase);
                        mainForm.Show();
                        this.Hide();

                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show($"An invalid operation occurred: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace);
                    //MessageBox.Show($"An unexpected error occurred: {ex.Message}");
                    
                }
            }
        }

    }

    public class InputDialog : Form
    {
        private TextBox textBox;
        private Button okButton;
        private Button cancelButton;

        public string InputText { get { return textBox.Text; } }

        public InputDialog(string prompt)
        {
            this.Text = prompt;

            textBox = new TextBox { Left = 20, Top = 20, Width = 200 };
            okButton = new Button { Text = "OK", Left = 130, Width = 90, Top = 50, DialogResult = DialogResult.OK };
            cancelButton = new Button { Text = "Cancel", Left = 30, Width = 90, Top = 50, DialogResult = DialogResult.Cancel };

            okButton.Click += (sender, e) => { this.Close(); };
            cancelButton.Click += (sender, e) => { this.Close(); };

            this.Controls.Add(textBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(250, 100);
        }
    }


}

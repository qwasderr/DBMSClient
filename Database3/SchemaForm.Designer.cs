namespace Database3
{
    partial class SchemaForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private TextBox txtTableName;
        private TextBox txtFieldName;
        private ComboBox cmbFieldType;
        private ListBox lstFields;
        private Button btnAddField;
        private Button btnSaveSchema;
        private Label lblTableName;
        private Label lblFieldName;
        private Label lblFieldType;
        private Label lblFieldsList;
        private Label upperBoundLabel;
        private Label lowerBoundLabel;
        private TextBox lowerBoundTextBox;
        private TextBox upperBoundTextBox;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        /// 

        private void InitializeTimeintControls()
        {
            // Створюємо поля для нижньої та верхньої меж
            lowerBoundLabel = new Label
            {
                Text = "Lower Bound (HH:MM)",
                Location = new System.Drawing.Point(20, 150),
                Visible = false
            };
            this.Controls.Add(lowerBoundLabel);

            lowerBoundTextBox = new TextBox
            {
                Location = new System.Drawing.Point(130, 150),
                Width = 100,
                Visible = false
            };
            this.Controls.Add(lowerBoundTextBox);

            upperBoundLabel = new Label
            {
                Text = "Upper Bound (HH:MM)",
                Location = new System.Drawing.Point(20, 180),
                Visible = false
            };
            this.Controls.Add(upperBoundLabel);

            upperBoundTextBox = new TextBox
            {
                Location = new System.Drawing.Point(130, 180),
                Width = 100,
                Visible = false
            };
            this.Controls.Add(upperBoundTextBox);

            // Зміна видимості при виборі типу timeint
            cmbFieldType.SelectedIndexChanged += (sender, e) =>
            {
                string selectedType = cmbFieldType.SelectedItem.ToString().ToLower();
                if (selectedType == "timeint")
                {
                    lowerBoundLabel.Visible = true;
                    lowerBoundTextBox.Visible = true;
                    upperBoundLabel.Visible = true;
                    upperBoundTextBox.Visible = true;
                }
                else
                {
                    lowerBoundLabel.Visible = false;
                    lowerBoundTextBox.Visible = false;
                    upperBoundLabel.Visible = false;
                    upperBoundTextBox.Visible = false;
                }
            };
        }
        private void InitializeComponent()
        {
            txtTableName = new TextBox();
            txtFieldName = new TextBox();
            cmbFieldType = new ComboBox();
            lstFields = new ListBox();
            btnAddField = new Button();
            btnSaveSchema = new Button();
            lblTableName = new Label();
            lblFieldName = new Label();
            lblFieldType = new Label();
            lblFieldsList = new Label();
            SuspendLayout();
            // 
            // txtTableName
            // 
            txtTableName.Location = new Point(120, 20);
            txtTableName.Name = "txtTableName";
            txtTableName.Size = new Size(150, 27);
            txtTableName.TabIndex = 0;
            // 
            // txtFieldName
            // 
            txtFieldName.Location = new Point(120, 60);
            txtFieldName.Name = "txtFieldName";
            txtFieldName.Size = new Size(150, 27);
            txtFieldName.TabIndex = 1;
            // 
            // cmbFieldType
            // 
            cmbFieldType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFieldType.Items.AddRange(new object[] { "string", "int", "real", "time", "timeint", "char" });
            cmbFieldType.Location = new Point(120, 100);
            cmbFieldType.Name = "cmbFieldType";
            cmbFieldType.Size = new Size(150, 28);
            cmbFieldType.TabIndex = 2;
            // 
            // lstFields
            // 
            lstFields.Location = new Point(20, 252);
            lstFields.Name = "lstFields";
            lstFields.Size = new Size(250, 144);
            lstFields.TabIndex = 3;
            // 
            // btnAddField
            // 
            btnAddField.Location = new Point(20, 402);
            btnAddField.Name = "btnAddField";
            btnAddField.Size = new Size(75, 23);
            btnAddField.TabIndex = 4;
            btnAddField.Text = "Add Field";
            btnAddField.Click += btnAddField_Click;
            // 
            // btnSaveSchema
            // 
            btnSaveSchema.Location = new Point(195, 402);
            btnSaveSchema.Name = "btnSaveSchema";
            btnSaveSchema.Size = new Size(75, 23);
            btnSaveSchema.TabIndex = 5;
            btnSaveSchema.Text = "Save Schema";
            btnSaveSchema.Click += btnSaveSchema_Click;
            // 
            // lblTableName
            // 
            lblTableName.AutoSize = true;
            lblTableName.Location = new Point(20, 23);
            lblTableName.Name = "lblTableName";
            lblTableName.Size = new Size(108, 20);
            lblTableName.TabIndex = 6;
            lblTableName.Text = "Schema Name:";
            // 
            // lblFieldName
            // 
            lblFieldName.AutoSize = true;
            lblFieldName.Location = new Point(20, 63);
            lblFieldName.Name = "lblFieldName";
            lblFieldName.Size = new Size(88, 20);
            lblFieldName.TabIndex = 7;
            lblFieldName.Text = "Field Name:";
            // 
            // lblFieldType
            // 
            lblFieldType.AutoSize = true;
            lblFieldType.Location = new Point(20, 103);
            lblFieldType.Name = "lblFieldType";
            lblFieldType.Size = new Size(79, 20);
            lblFieldType.TabIndex = 8;
            lblFieldType.Text = "Field Type:";
            // 
            // lblFieldsList
            // 
            lblFieldsList.AutoSize = true;
            lblFieldsList.Location = new Point(19, 229);
            lblFieldsList.Name = "lblFieldsList";
            lblFieldsList.Size = new Size(76, 20);
            lblFieldsList.TabIndex = 9;
            lblFieldsList.Text = "Fields List:";
            // 
            // SchemaForm
            // 
            ClientSize = new Size(548, 466);
            Controls.Add(txtTableName);
            Controls.Add(txtFieldName);
            Controls.Add(cmbFieldType);
            Controls.Add(lstFields);
            Controls.Add(btnAddField);
            Controls.Add(btnSaveSchema);
            Controls.Add(lblTableName);
            Controls.Add(lblFieldName);
            Controls.Add(lblFieldType);
            Controls.Add(lblFieldsList);
            Name = "SchemaForm";
            Text = "Create Schema";
            ResumeLayout(false);
            PerformLayout();
        }


        #endregion
    }
}
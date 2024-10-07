using DatabaseServiceReference;

namespace Database3
{
    public partial class Form1 : Form
    {
        public Form1(DatabaseServiceReference.Database database)
        {
            _database = database;
            InitializeComponent();
            InitializeDifference();
            InitializeElements();
            _serviceClient = new DatabaseServiceClient();
        }
        public Form1()
        {
            InitializeComponent();
            InitializeDifference();
            InitializeElements();
            _serviceClient = new DatabaseServiceClient();
            //_database = new Database();
        }

    }
}

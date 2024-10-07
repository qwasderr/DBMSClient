using Database3;
using NUnit.Framework;
using NUnit.Framework.Legacy;
namespace TestProject1
{
    [TestFixture]
    
    public class Tests
    {
        private Schema schema;
        [SetUp]
        public void Setup()
        {
            schema = new Schema(new List<Field>
        {
            new Field("ID", "int"),
            new Field("Model", "string"),
            new Field("Year", "int"),
            new Field("Price", "real")
        }, "Cars");
        }

        [Test]
        public void TestCreateCarsTable()
        {
        
            var carsTable = new Table("Cars", schema);

            ClassicAssert.AreEqual("Cars", carsTable.Name);

            ClassicAssert.AreEqual(4, carsTable.Schema.Fields.Count);
            ClassicAssert.AreEqual("ID", carsTable.Schema.Fields[0].Name);
            ClassicAssert.AreEqual("Model", carsTable.Schema.Fields[1].Name);
            ClassicAssert.AreEqual("Year", carsTable.Schema.Fields[2].Name);
            ClassicAssert.AreEqual("Price", carsTable.Schema.Fields[3].Name);
        }




        [Test]
        public void TestCreateDatabase()
        {
            var database = new Database("TestDatabase");

            ClassicAssert.AreEqual("TestDatabase", database.Name);

            ClassicAssert.AreEqual(0, database.Tables.Count);
           
            var carsTable = new Table("Cars", schema);
            database.AddTable(carsTable);

            ClassicAssert.AreEqual(1, database.Tables.Count);
            ClassicAssert.AreEqual("Cars", database.Tables[0].Name);
        }


        [Test]
        public void TestDifferenceBetweenTwoTables()
        {

            var table1 = new Table("Table1", schema);
            var table2 = new Table("Table2", schema);

            table1.AddRow(new Row(new List<Value> { new Value("Mazda CX-7"), new Value(2020), new Value(1000.0) }));
            table1.AddRow(new Row(new List<Value> { new Value("BMW X3"), new Value(2021), new Value(1500.0) }));

            table2.AddRow(new Row(new List<Value> { new Value("BMW X3"), new Value(2021), new Value(1500.0) }));
            table2.AddRow(new Row(new List<Value> { new Value("Dodge Challenger"), new Value(2022), new Value(2000.0) }));

            var resultTable = table1.Difference(table2);

            ClassicAssert.AreEqual(1, resultTable.Rows.Count);

            ClassicAssert.AreEqual(1, resultTable.Rows[0].Values[0].FieldValue);
            ClassicAssert.AreEqual("Mazda CX-7", resultTable.Rows[0].Values[1].FieldValue);
            ClassicAssert.AreEqual(2020, resultTable.Rows[0].Values[2].FieldValue); 
            ClassicAssert.AreEqual(1000.0, resultTable.Rows[0].Values[3].FieldValue); 
        }
    }
}
using System;
using System.Text.Json;
namespace Database3
{
    public class Database
    {
        public string Name { get; set; }
        public List<Table> Tables { get; set; }
        public List<Schema> Schemas;

        public void AddSchema(Schema schema)
        {
            Schemas.Add(schema);
        }
        public Database(string name)
        {
            Name = name;
            Tables = new List<Table>();
            Schemas = new List<Schema>();
        }
        public void AddTable(Table table)
        {


            Tables.Add(table);
        }
        public void CreateTable(string name, Schema schema)
        {
            if (Tables.Any(t => t.Name == name))
            {
                throw new Exception($"Table with the name '{name}' already exists.");
            }

            Tables.Add(new Table(name, schema));
        }

        public void DeleteTable(string name)
        {
            Tables.RemoveAll(t => t.Name == name);
        }

        public Table GetTable(string name)
        {
            return Tables.Find(t => t.Name == name);
        }

        public void SaveToDisk(string filePath)
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true
                });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving database: {ex.Message}");
            }
        }

        public void LoadFromDisk(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                //var loadedDatabase = JsonSerializer.Deserialize<Database>(json);
                var loadedDatabase = JsonSerializer.Deserialize<Database>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, 
                    IncludeFields = true
                });

                this.Name = loadedDatabase.Name;
                this.Tables = loadedDatabase.Tables;
                this.Schemas = loadedDatabase.Schemas;
            }
            


            catch (Exception ex)
            {
                throw new Exception($"Error loading database: {ex.Message}");
            }
        }
    }


    public class Table
    {
        public string Name { get; set; }
        public Schema Schema { get; set; }
        public List<Row> Rows { get; set; }
        private int _nextId;

        public Table(string name, Schema schema)
        {
            Name = name;
            Schema = schema;
            Rows = new List<Row>();
            _nextId = 1;
        }

        public void AddRow(Row row)
        {
            if (Schema.ValidateRow(row))
            {
                row.Values.Insert(0, new Value(_nextId));
                Rows.Add(row);
                _nextId++;
            }
            else
            {
                throw new Exception("Row validation failed.");
            }
        }

        public void DeleteRow(int index)
        {
            if (index >= 0 && index < Rows.Count)
            {
                Rows.RemoveAt(index);
            }
        }

        public Row GetRow(int index)
        {
            return Rows[index];
        }

        public void EditRow(int index, Row row)
        {
            if (index >= 0 && index < Rows.Count && Schema.ValidateRow(row))
            {
                Rows[index] = row;
            }
        }

        public Table Difference(Table anotherTable)
        {
            if (!Schema.Equals(anotherTable.Schema))
            {
                throw new Exception("Schemas are not compatible.");
            }

            var differenceRows = new List<Row>();

            foreach (var row in Rows)
            {
                if (!anotherTable.Rows.Any(r => r.Equals(row)))
                {
                    differenceRows.Add(row);
                }
            }
            Table differenceTable = new Table(Name + "_diff", Schema)
            {
                Rows = differenceRows
            };
            foreach (var row in differenceTable.Rows)
            {
                row.Values.RemoveAt(0);
            }
            for (var i = 0; i < differenceTable.Rows.Count(); ++i)
                differenceTable.Rows[i].Values.Insert(0, new Value(i + 1));
            return differenceTable;
        }
    }

    public class Schema
    {
        public List<Field> Fields { get; set; }
        public string Name { get; set; }

        public Schema(List<Field> fields, string Name)
        {
            Fields = fields;
            this.Name = Name;
        }

        public bool ValidateRow(Row row)
        {
            return true;
        }
        public override bool Equals(object obj)
        {
            if (obj is Schema otherSchema)
            {
                if (Fields.Count != otherSchema.Fields.Count)
                    return false;

                for (int i = 0; i < Fields.Count; i++)
                {
                    if (!Fields[i].Equals(otherSchema.Fields[i]))
                        return false;
                }

                return true;
            }
            return false;
        }
    }

    public class Field
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public TimeSpan? LowerBound { get; set; }
        public TimeSpan? UpperBound { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is Field otherField)
            {
                return Name == otherField.Name && Type == otherField.Type;
            }
            return false;
        }
        public Field(string name, string type)
        {
            Name = name;
            Type = type;
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
        public Field()
        {
        }
        public Field(string name, string type, string ts1, string ts2)
        {
            Name = name;
            Type = type;
            LowerBound = ConvertStringToTimeSpan(ts1);
            UpperBound = ConvertStringToTimeSpan(ts2);
        }
    }

    public class Row
    {
        public List<Value> Values { get; set; }

        public Row(List<Value> values)
        {
            Values = values;
        }
        public override bool Equals(object obj)
        {

            if (obj is Row otherRow)
            {
                if (Values.Count != otherRow.Values.Count)
                    return false;

                for (int i = 1; i < Values.Count; i++)
                {
                    if (!Values[i].Equals(otherRow.Values[i]))
                        return false;
                }

                return true;
            }
            return false;
        }
    }

    public class Value
    {
        public object FieldValue { get; set; }

        public Value(object fieldValue)
        {
            FieldValue = fieldValue;
        }
        public override bool Equals(object obj)
        {
            if (obj is Value otherValue)
            {
                if (FieldValue is JsonElement jsonElement1 && otherValue.FieldValue is JsonElement jsonElement2)
                {
                    if (jsonElement1.ValueKind == JsonValueKind.String && jsonElement2.ValueKind == JsonValueKind.String)
                    {
                        string str1 = jsonElement1.GetString();
                        string str2 = jsonElement2.GetString();
                        return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
                    }

                    if (jsonElement1.ValueKind == JsonValueKind.Number && jsonElement2.ValueKind == JsonValueKind.Number)
                    {
                        if (jsonElement1.TryGetInt32(out int int1) && jsonElement2.TryGetInt32(out int int2))
                        {
                            return int1 == int2;
                        }
                    }

                    return Equals(FieldValue, otherValue.FieldValue);
                }
            }

            return false;
        }


        public int ToInt()
        {
            if (FieldValue is int intValue)
                return intValue;
            return int.Parse(FieldValue.ToString());
        }

        public double ToDouble()
        {
            if (FieldValue is double doubleValue)
                return doubleValue;
            return double.Parse(FieldValue.ToString());
        }

        public string ToString()
        {
            return FieldValue.ToString();
        }
    }
}





using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Database3
{

    [DataContract]
    public class Database
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<Table> Tables { get; set; }
        [DataMember]
        public List<Schema> Schemas { get; set; }

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
        public List<Schema> GetSchemas()
        {
            return this.Schemas;
        }
        public void DeleteSchema(int id)
        {

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
                // Ensure that the Schemas property is also serialized with the rest of the database
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    // Ensure that all object references, including schemas, are properly handled
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
                    PropertyNameCaseInsensitive = true, // Makes property names case-insensitive
                    IncludeFields = true                 // Include private fields if necessary
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

    [DataContract]
    public class Table
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Schema Schema { get; set; }
        [DataMember]
        public List<Row> Rows { get; set; }
        [DataMember]
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
    [DataContract]
    public class Schema
    {
        [DataMember]
        public List<Field> Fields { get; set; }
        [DataMember]
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
    [DataContract]
    public class Field
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public TimeSpan? LowerBound { get; set; }
        [DataMember]
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
    [DataContract]
    public class Row
    {
        [DataMember]
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
    [DataContract]
    public class Value
    {
        [DataMember]
        public object FieldValue { get; set; }

        public Value(object fieldValue)
        {
            FieldValue = fieldValue;
        }
        public override bool Equals(object obj)
        {
            if (obj is Value otherValue)
            {
                var thisValue = ConvertFieldValue(FieldValue);
                var otherConvertedValue = ConvertFieldValue(otherValue.FieldValue);

                if (thisValue is string strValue1 && otherConvertedValue is string strValue2)
                {
                    return string.Equals(strValue1, strValue2, StringComparison.OrdinalIgnoreCase);
                }
                if (thisValue is int intValue1 && otherConvertedValue is int intValue2)
                {
                    return intValue1 == intValue2;
                }

                if (thisValue is double doubleValue1 && otherConvertedValue is double doubleValue2)
                {
                    return Math.Abs(doubleValue1 - doubleValue2) < 0.000001;
                }

                if (thisValue is float floatValue1 && otherConvertedValue is float floatValue2)
                {
                    return Math.Abs(floatValue1 - floatValue2) < 0.000001f;
                }

                if (thisValue is char charValue1 && otherConvertedValue is char charValue2)
                {
                    return charValue1 == charValue2;
                }

                if (thisValue is bool boolValue1 && otherConvertedValue is bool boolValue2)
                {
                    return boolValue1 == boolValue2;
                }

                if (thisValue is IConvertible && otherConvertedValue is IConvertible)
                {
                    try
                    {
                        double val1 = Convert.ToDouble(thisValue);
                        double val2 = Convert.ToDouble(otherConvertedValue);
                        return Math.Abs(val1 - val2) < 0.000001;
                    }
                    catch
                    {
                        return Equals(thisValue, otherConvertedValue);
                    }
                }

                return Equals(thisValue, otherConvertedValue);
            }

            return false;
        }
        private object ConvertFieldValue(object fieldValue)
        {
            if (fieldValue is JsonElement jsonElement)
            {
                switch (jsonElement.ValueKind)
                {
                    case JsonValueKind.String:
                        return jsonElement.GetString();
                    case JsonValueKind.Number:
                        if (jsonElement.TryGetInt32(out int intValue))
                        {
                            return intValue;
                        }
                        if (jsonElement.TryGetDouble(out double doubleValue))
                        {
                            return doubleValue;
                        }
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return jsonElement.GetBoolean();
                    case JsonValueKind.Null:
                        return null;
                }
            }

            return fieldValue;
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

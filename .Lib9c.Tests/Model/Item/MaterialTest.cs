namespace Lib9c.Tests.Model.Item
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Nekoyume.Model.Item;
    using Nekoyume.TableData;
    using Xunit;

    public class MaterialTest
    {
        private readonly MaterialItemSheet.Row _materialRow;

        public MaterialTest()
        {
            var tableSheets = new TableSheets(TableSheetsImporter.ImportSheets());
            _materialRow = tableSheets.MaterialItemSheet.First;
        }

        [Fact]
        public void Serialize()
        {
            Assert.NotNull(_materialRow);

            var costume = new Material(_materialRow);
            var serialized = costume.Serialize();
            var deserialized = new Material((Bencodex.Types.Dictionary)serialized);

            Assert.Equal(costume, deserialized);
        }

        [Fact]
        public void SerializeWithDotNetAPI()
        {
            Assert.NotNull(_materialRow);

            var costume = new Material(_materialRow);
            var formatter = new BinaryFormatter();
            using var ms = new MemoryStream();
            formatter.Serialize(ms, costume);
            ms.Seek(0, SeekOrigin.Begin);

            var deserialized = (Material)formatter.Deserialize(ms);

            Assert.Equal(costume, deserialized);
        }
    }
}

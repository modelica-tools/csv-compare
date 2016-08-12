using System.IO;
using NUnit.Framework;
using CsvCompare;

namespace tests
{
    /// <summary>
    /// Test CsvFile class
    /// </summary>
    [TestFixture]
    public class TestCsvFile
    {
        const string FileName = "some.file";

        /// <summary>
        /// Test creating CsvFile object from non-exsting file.
        /// </summary>
        [Test]
        public void ConstructorFileNotFound()
        {
            // make sure the file does not exists
            Assert.IsFalse(File.Exists(FileName));

            // check that we get appropriate exception
            Assert.That(() => new CsvFile(FileName, new Options(), new Log()),
                        Throws.TypeOf<FileNotFoundException>());
        }
    }
}

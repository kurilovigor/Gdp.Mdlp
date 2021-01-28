using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Gdp.Mdlp.Tests
{
    [TestClass]
    public class HelpersTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void Serialization_DocFilter()
        {
            /*
            var config = new ConnectionConfig
            {
                Api = new Uri("https://api.mdlp.crpt.ru/api/v1/"),
                Auth = new AuthConfig
                {
                    ClientId = "a6682ac3-f86e-4e4c-969a-5f50719bbc9a",
                    ClientSecret = "755cc62e-5729-41df-ac4e-19b214362abc",
                    Tumbprint = "0C2D9F3FDA17E0374984B3FE853FB1FA605266B8"
                }
            };
            using (var s = new Connection(config))
            {

            }
            */
            TestContext.WriteLine(JsonConvert.SerializeObject(new Model.DocFilter
            {
                document_id = "4c615547-7ba4-4bd6-87a7-cc7eb4ec41d8"
            }));
        }
        [TestMethod]
        public void Barcode_ParseSgtin()
        {
            TestContext.WriteLine(Barcode.ToSgtin("010383895708076221184397364519591EE0692MabG2rYp5cQKd3lGrmaklvMNj"));
            TestContext.WriteLine(Barcode.ToSgtin("0103838989543945215GML1GLACFHCA91EE0692aCM554QzQPLCSkVcH3+dGIXwj"));
            TestContext.WriteLine(Barcode.ToSgtin("0103838989543945215GN5AIXCCSJM591EE06925V4O1lCT+BLc1pIc4GXEiHuQd"));
            TestContext.WriteLine(Barcode.ToSgtin("0103838989543945215GN5AIXCCSJM591EE06925V4O1lCT+BLc1pIc4GXEiHuQd").Length.ToString());

        }
        [TestMethod]
        public void Barcode_ParseGtin()
        {
            TestContext.WriteLine(Barcode.ToGtin("010383895708076221184397364519591EE0692MabG2rYp5cQKd3lGrmaklvMNj"));
            TestContext.WriteLine(Barcode.ToGtin("0103838989543945215GML1GLACFHCA91EE0692aCM554QzQPLCSkVcH3+dGIXwj"));
            TestContext.WriteLine(Barcode.ToGtin("0103838989543945215GN5AIXCCSJM591EE06925V4O1lCT+BLc1pIc4GXEiHuQd"));

        }
        [TestMethod]
        public void Document_FormatMskDateTime()
        {
            TestContext.WriteLine(Format.MskTimeStamp());
            TestContext.WriteLine(Format.ToMskDateTime(DateTime.Now));
        }
        [TestMethod]
        public void DocumentsOutcomeRequest()
        {
            TestContext.WriteLine(JsonConvert.SerializeObject(
                new Model.DocumentsOutcomeRequest()
                {
                    start_from = 0,
                    count = 10
                }
                ));
        }
        [TestMethod]
        public void DocumentsIncomeRequest()
        {
            TestContext.WriteLine(JsonConvert.SerializeObject(
                new Model.DocumentsOutcomeRequest
                {
                    start_from = 0,
                    count = 10
                }));
        }
        [TestMethod]
        public void DocFilter()
        {
            TestContext.WriteLine(JsonConvert.SerializeObject(
                Model.DocFilter
                    .Create()
                    .StartDate(DateTime.Now.AddDays(-2))
                    .EndDate(DateTime.Now)
                    .DocStatus(Model.DocStatus.PROCESSED_DOCUMENT)
                    .DocType(602)
                ));
        }
        [TestMethod]
        public void ParseDateTime()
        {
            TestContext.WriteLine($"Format.ParseDateTime: {Format.ParseDateTime("2020-10-14T10:15:00.082+03:00")}");
            TestContext.WriteLine($"Format.ParseDateTime: {Format.ParseDateTime("2020-10-09T14:43:02.958+03:00")}");
            TestContext.WriteLine(Format.NowDateTimeOffset());
            TestContext.WriteLine(Format.ToDate(DateTime.Now));
        }
    }
}

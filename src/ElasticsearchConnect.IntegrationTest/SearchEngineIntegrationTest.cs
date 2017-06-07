using ElasticsearchConnect.Api.IoCConfiguration;
using ElasticsearchConnect.IntegrationTest.Models;
using ElasticsearchConnect.Repository;
using DryIoc;
using Elasticsearch.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ElasticsearchConnect.IntegrationTest
{
    [TestClass]
    public class SearchEngineIntegrationTest
    {
        string testIndexName = "test_index_integration";

        [TestMethod]
        [TestCategory("SearchEngine")]
        public void CreateIndex_With_Valid_Name_No_Exception_Expected()
        {
            try
            {
                FluentIntegrationTest.Create()
                               .Start(this.testIndexName)
                               .LoadData(() => null)
                               .Cleanup();
            }
            catch (System.Exception ex)
            {
                Debug.Write(ex.Message);
                Assert.Fail();
            }
        }


        [TestMethod]
        [TestCategory("SearchEngine")]
        public void Insert_Multiple_Records_Success()
        {
            FluentIntegrationTest.Create()
                           .Start(this.testIndexName)
                           .LoadData(this.GetTestData)
                           .ExecuteSearch(new Model.SearchQueryItem()
                           {
                               Fulltext = "*",
                               Pagination = new Model.PagingParameter() { Page = 1, PageSize = 100 },
                               Select = new string[2] { "Firstname","Id" }
                           }).Assert((expected, actual) =>
                           {
                               Assert.AreEqual(expected.Length, actual.Paging.TotalCount);
                           });
        }

        [TestMethod]
        [TestCategory("SearchEngine")]
        public void GetFilter_Fields_Returns_List_Of_Properties()
        {
            FluentIntegrationTest.Create()
                          .Start(this.testIndexName)
                          .LoadData(this.GetTestData)
                          .ExecuteCustom((s) =>
                          {
                              s.SetType("Object");
                              return s.GetFilterFields();
                          }).Assert((actual) =>
                          {
                              var fields = actual as List<KeyValuePair<string, string>>;
                              Assert.AreEqual(6, fields.Count());//check number of fields
                              Assert.IsTrue(fields.Any(d => d.Key == "Id"));
                              Assert.IsTrue(fields.Any(d => d.Key == "Firstname.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "LastName.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "Email.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "Gender.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "IpAddress.keyword"));
                          });
        }

        [TestMethod]
        [TestCategory("SearchEngine")]
        public void GetNestedFilter_Fields_Returns_List_Of_Properties()
        {
            FluentIntegrationTest.Create()
                          .Start(this.testIndexName)
                          .LoadData(this.GetNestedTestData)//nestedData.Json
                          .ExecuteCustom((s) =>
                          {
                              s.SetType("Object");
                              return s.GetFilterFields();
                          }).Assert((actual) =>
                          {
                              var fields = actual as List<KeyValuePair<string, string>>;
                              Assert.AreEqual(15, fields.Count());//check number of fields
                              Assert.IsTrue(fields.Any(d => d.Key == "id"));
                              Assert.IsTrue(fields.Any(d => d.Key == "name.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "username.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "email.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "address.street.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "address.suite.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "address.city.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "address.zipcode.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "address.geo.lat.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "address.geo.lng.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "phone.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "website.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "company.name.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "company.catchPhrase.keyword"));
                              Assert.IsTrue(fields.Any(d => d.Key == "company.bs.keyword"));
                          });
        }

        [TestMethod]
        [TestCategory("SearchEngine")]
        public void SearchData_Where_Id_Equals_1_Returns_One_Document()
        {
            FluentIntegrationTest.Create()
                         .Start(this.testIndexName)
                         .LoadData(this.GetTestData)
                         .ExecuteSearch(new Model.SearchQueryItem()
                         {
                             Filters = new List<Model.CustomFilter>(){ new Model.CustomFilter()
                                         {
                                             Name ="Id",
                                             Value = "1"
                                         }},
                             Pagination = new Model.PagingParameter() { Page = 1, PageSize = 100 }
                         }).Assert((expected, actual) =>
                         {
                             var response = actual.Documents.SingleOrDefault() as dynamic;
                             var expectedObject = expected.FirstOrDefault(d => d.Id == 1);

                             Assert.AreEqual(1, actual.Paging.TotalCount);
                             Assert.AreEqual(expectedObject.Id, response.Id);
                             Assert.AreEqual(expectedObject.Firstname, response.Firstname);
                             Assert.AreEqual(expectedObject.LastName, response.LastName);
                             Assert.AreEqual(expectedObject.Email, response.Email);
                             Assert.AreEqual(expectedObject.Gender, response.Gender);
                             Assert.AreEqual(expectedObject.IpAddress, response.IpAddress);
                         });
        }

        [TestMethod]
        [TestCategory("SearchEngine")]
        public void Search_By_Fulltext_Equals_Famale_And_Aggregate_By_Gender_Returns_SearchResult_And_CountDistict()
        {
            FluentIntegrationTest.Create()
                         .Start(this.testIndexName)
                         .LoadData(this.GetTestData)
                         .ExecuteSearch(new Model.SearchQueryItem()
                         {
                             Fulltext = "Female",
                             Pagination = new Model.PagingParameter() { Page = 1, PageSize = 100 },
                             Aggregations = new List<Model.AggregationItem>()
                             {
                                new Model.AggregationItem()
                                {
                                    Name = "Gender.keyword"
                                }
                             }
                         }).Assert((expected, actual) =>
                         {
                             var expectedObject = expected.Count(d => d.Gender == "Female");

                             var response = actual.Aggregations.FirstOrDefault(d => d.Key == "Gender.keyword")
                                                               .Items
                                                               .FirstOrDefault()
                                                               .Count;
                             Assert.AreNotEqual(0, actual.Documents.Count());
                             Assert.AreEqual(expectedObject, response);
                         });
        }

        #region private methods

        private dynamic[] GetTestData()
        {
            string path = AppContext.BaseDirectory + @"\FakeData\data.json";
            string json = File.ReadAllText(path);

            //Deserialize test Data
            object[] jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<object[]>(json);
            return jsonData;
        }

        private dynamic[] GetNestedTestData()
        {
            string path = AppContext.BaseDirectory + @"\FakeData\nestedData.json";
            string json = File.ReadAllText(path);

            //Deserialize test Data
            object[] jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<object[]>(json);
            return jsonData;
        }
        #endregion
    }
}

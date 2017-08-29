
using ElasticConnector.IntegrationTest.Models;
using ElasticConnector.Model;
using ElasticConnector.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace ElasticConnector.IntegrationTest
{

    public class SearchEngineIntegrationTest
    {
        string testIndexName = "test_index_integration";

        [Fact]
        public void CreateIndex_With_Valid_Name_No_Exception_Expected()
        {

            FluentIntegrationTest.Create()
                           .Start(this.testIndexName)
                           .LoadData(() => null)
                           .Cleanup();

        }

        [Fact]
        public void TestCreateCustomMapping()
        {
            //string[] urls = new string[] { "http://10.243.132.5:9200" };
            //ElasticsearchSearchEngine<Company> elasticsearch = new ElasticsearchSearchEngine<Company>(urls);
            //elasticsearch.CreateIndex("test_company");
        }


        [Fact]
        public void Insert_Multiple_Records_Success()
        {
            FluentIntegrationTest.Create()
                           .Start(this.testIndexName)
                           .LoadData(this.GetTestData)
                           .ExecuteSearch(new SearchQueryItem()
                           {
                               Fulltext = "*",
                               Pagination = new PagingParameter() { Page = 1, PageSize = 100 },
                               Select = new string[2] { "Firstname", "Id" }
                           }).Assert((expected, actual) =>
                           {
                               Assert.Equal(expected.Length, actual.Paging.TotalCount);
                           });
        }

        [Fact]
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
                              Assert.Equal(6, fields.Count());//check number of fields
                              Assert.True(fields.Any(d => d.Key == "Id"));
                              Assert.True(fields.Any(d => d.Key == "Firstname.keyword"));
                              Assert.True(fields.Any(d => d.Key == "LastName.keyword"));
                              Assert.True(fields.Any(d => d.Key == "Email.keyword"));
                              Assert.True(fields.Any(d => d.Key == "Gender.keyword"));
                              Assert.True(fields.Any(d => d.Key == "IpAddress.keyword"));
                          });
        }

        [Fact]
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
                              Assert.Equal(15, fields.Count());//check number of fields
                              Assert.True(fields.Any(d => d.Key == "id"));
                              Assert.True(fields.Any(d => d.Key == "name.keyword"));
                              Assert.True(fields.Any(d => d.Key == "username.keyword"));
                              Assert.True(fields.Any(d => d.Key == "email.keyword"));
                              Assert.True(fields.Any(d => d.Key == "address.street.keyword"));
                              Assert.True(fields.Any(d => d.Key == "address.suite.keyword"));
                              Assert.True(fields.Any(d => d.Key == "address.city.keyword"));
                              Assert.True(fields.Any(d => d.Key == "address.zipcode.keyword"));
                              Assert.True(fields.Any(d => d.Key == "address.geo.lat.keyword"));
                              Assert.True(fields.Any(d => d.Key == "address.geo.lng.keyword"));
                              Assert.True(fields.Any(d => d.Key == "phone.keyword"));
                              Assert.True(fields.Any(d => d.Key == "website.keyword"));
                              Assert.True(fields.Any(d => d.Key == "company.name.keyword"));
                              Assert.True(fields.Any(d => d.Key == "company.catchPhrase.keyword"));
                              Assert.True(fields.Any(d => d.Key == "company.bs.keyword"));
                          });
        }

        [Fact]
        public void SearchData_Where_Id_Equals_1_Returns_One_Document()
        {
            FluentIntegrationTest.Create()
                         .Start(this.testIndexName)
                         .LoadData(this.GetTestData)
                         .ExecuteSearch(new SearchQueryItem()
                         {
                             Filters = new List<CustomFilter>(){ new CustomFilter()
                                         {
                                             Name ="Id",
                                             Value = "1"
                                         }},
                             Pagination = new PagingParameter() { Page = 1, PageSize = 100 }
                         }).Assert((expected, actual) =>
                         {
                             var response = actual.Documents.SingleOrDefault() as dynamic;
                             var expectedObject = expected.FirstOrDefault(d => d.Id == 1);

                             Assert.Equal(1, actual.Paging.TotalCount);
                             Assert.Equal(expectedObject.Id, response.Id);
                             Assert.Equal(expectedObject.Firstname, response.Firstname);
                             Assert.Equal(expectedObject.LastName, response.LastName);
                             Assert.Equal(expectedObject.Email, response.Email);
                             Assert.Equal(expectedObject.Gender, response.Gender);
                             Assert.Equal(expectedObject.IpAddress, response.IpAddress);
                         });
        }

        [Fact]
        public void Search_By_Fulltext_Equals_Famale_And_Aggregate_By_Gender_Returns_SearchResult_And_CountDistict()
        {
            FluentIntegrationTest.Create()
                         .Start(this.testIndexName)
                         .LoadData(this.GetTestData)
                         .ExecuteSearch(new SearchQueryItem()
                         {
                             Fulltext = "Female",
                             Pagination = new PagingParameter() { Page = 1, PageSize = 100 },
                             Aggregations = new List<AggregationItem>()
                             {
                                new AggregationItem()
                                {
                                    Name = "Gender.keyword"
                                }
                             }
                         }).Assert((expected, actual) =>
                         {
                             double expectedObject = expected.Count(d => d.Gender == "Female");

                             var response = actual.Aggregations.FirstOrDefault(d => d.Key == "Gender.keyword")
                                                               .Items
                                                               .FirstOrDefault()
                                                               .Count;
                             Assert.NotEqual(0, actual.Documents.Count());
                             Assert.Equal(expectedObject, response);
                         });
        }

        [Fact]
        public void InserUserProduct()
        {
            FluentIntegrationTest.Create()
                         .Start("user-product-index")
                         .LoadData(this.GetUserProductTestData);
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

        private dynamic[] GetUserProductTestData()
        {
            string path = AppContext.BaseDirectory + @"\FakeData\user-product.json";
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

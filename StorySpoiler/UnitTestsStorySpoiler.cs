using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]
    public class StorySpilerTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("kalinexam", "kalinexam");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Order(1)]
        [Test]

        public void CreateStory_WithRequiredFields_ShouldReturnCreated()
        {
            var story = new StoryDTO
            {
                Title = "New Story",
                Description ="some cool story here",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);
            
            //Assert Status

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString() ?? string.Empty;

            //Assert response
            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty, "StoryId should not be null or empty.");

            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Order(2)]
        [Test]

        public void Edit_TheCreatedStory_ShouldReturnOK()
        {
            var updatedStory = new StoryDTO
            {
                Title = "Updated Story Title",
                Description = "This is new cool story",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }
        [Order(3)]
        [Test]

        public void GetAllStories_ShouldReturnOK_AndNotEmptyArray()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(json.GetArrayLength(), Is.GreaterThan(0), "Stories array should not be empty");
        }

        [Order(4)]
        [Test]

        public void Delete_TheCreatedStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Order(5)]
        [Test]

        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var badStory = new StoryDTO
            {
                Title = null,
                Description = null,
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(badStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]

        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var nonExistingStoryId = "1234210993811-";

            var updatedStory = new StoryDTO
            {
                Title = "Non existing story",
                Description = "Trying to update",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingStory_ShouldReturnBadReqyest()
        {
            var nonExistingStoryId = "1241124124--";

            var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]

        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}
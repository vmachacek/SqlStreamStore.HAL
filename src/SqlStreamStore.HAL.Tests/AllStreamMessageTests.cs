namespace SqlStreamStore.HAL.Tests
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class AllStreamMessageTests : IDisposable
    {
        public AllStreamMessageTests()
        {
            _fixture = new MiddlewareFixture();
        }

        public void Dispose() => _fixture.Dispose();
        private readonly MiddlewareFixture _fixture;
        private const string HeadOfAll = "stream?d=b&m=20&p=-1";

        [Fact]
        public async Task read_single_message_all_stream()
        {
            // position of event in all stream would be helpful here
            var writeResult = await _fixture.WriteNMessages("a-stream", 1);

            using(var response = await _fixture.HttpClient.GetAsync("/stream/0"))
            {
                response.StatusCode.ShouldBe(HttpStatusCode.OK);

                var resource = await response.AsHal();

                resource.Links.Keys.ShouldBe(new[] { "self", "streamStore:feed" });

                resource.ShouldLink("self", "/stream/0");
                resource.ShouldLink("streamStore:feed", HeadOfAll);
            }
        }

        [Fact]
        public async Task read_single_message_does_not_exist_all_stream()
        {
            using(var response = await _fixture.HttpClient.GetAsync("/stream/0"))
            {
                response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

                var resource = await response.AsHal();

                resource.Links.Keys.ShouldBe(new[] { "streamStore:feed" });

                resource.ShouldLink("streamStore:feed", HeadOfAll);
            }
        }
    }
}
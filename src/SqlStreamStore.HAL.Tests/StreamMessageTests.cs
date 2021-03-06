﻿namespace SqlStreamStore.HAL.Tests
{
    using System.Net;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;

    public class StreamMessageTests
    {
        public StreamMessageTests()
        {
            _fixture = new MiddlewareFixture();
        }

        public void Dispose() => _fixture.Dispose();
        private readonly MiddlewareFixture _fixture;
        private const string HeadOfStream = "../a-stream?d=b&m=20&p=-1";

        [Fact]
        public async Task read_single_message_stream()
        {
            // position of event in all stream would be helpful here
            var writeResult = await _fixture.WriteNMessages("a-stream", 1);

            using(var response = await _fixture.HttpClient.GetAsync("/streams/a-stream/0"))
            {
                response.StatusCode.ShouldBe(HttpStatusCode.OK);

                var resource = await response.AsHal();

                resource.Links.Keys.ShouldBe(new[] { "self", "first", "next", "last", "streamStore:feed" });

                resource.ShouldLink("self", "0");
                resource.ShouldLink("first", "0");
                resource.ShouldLink("next", "1");
                resource.ShouldLink("last", "-1");
                resource.ShouldLink("streamStore:feed", HeadOfStream);
            }
        }

        [Fact]
        public async Task read_single_message_does_not_exist_stream()
        {
            using(var response = await _fixture.HttpClient.GetAsync("/streams/a-stream/0"))
            {
                response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

                var resource = await response.AsHal();

                resource.Links.Keys.ShouldBe(new[] { "self", "first", "last", "streamStore:feed" });

                resource.ShouldLink("self", "0");
                resource.ShouldLink("first", "0");
                resource.ShouldLink("last", "-1");
                resource.ShouldLink("streamStore:feed", HeadOfStream);
            }
        }

    }
}
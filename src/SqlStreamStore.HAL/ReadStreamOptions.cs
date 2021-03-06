namespace SqlStreamStore.HAL
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using SqlStreamStore.Streams;

    internal class ReadStreamOptions
    {
        private readonly int _fromVersionInclusive;
        private readonly int _maxCount;

        public ReadStreamOptions(IOwinRequest request)
        {
            StreamId = request.Path.Value.Remove(0, 1);

            EmbedPayload = request.Query.Get("e") != null;

            ReadDirection = request.Query.Get("d") == "f"
                ? 1
                : -1;

            if(!int.TryParse(request.Query.Get("p"), out _fromVersionInclusive))
            {
                _fromVersionInclusive = ReadDirection > 0 ? StreamVersion.Start : StreamVersion.End;
            }

            if(!int.TryParse(request.Query.Get("m"), out _maxCount))
            {
                _maxCount = 20;
            }
        }

        public long FromVersionInclusive => _fromVersionInclusive;
        public int MaxCount => _maxCount;
        public bool EmbedPayload { get; }
        public int ReadDirection { get; }
        public string StreamId { get; }

        public string Self => ReadDirection > 0
            ? LinkFormatter.FormatForwardLink(StreamId, MaxCount, FromVersionInclusive)
            : LinkFormatter.FormatBackwardLink(StreamId, MaxCount, FromVersionInclusive);

        public Func<IReadonlyStreamStore, CancellationToken, Task<ReadStreamPage>> GetReadOperation()
            => (streamStore, ct) => ReadDirection > 0
                ? streamStore.ReadStreamForwards(StreamId, _fromVersionInclusive, _maxCount, EmbedPayload, ct)
                : streamStore.ReadStreamBackwards(StreamId, _fromVersionInclusive, _maxCount, EmbedPayload, ct);
    }
}
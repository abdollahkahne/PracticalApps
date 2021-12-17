using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

#nullable disable
namespace ServerSide.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(MessageInput input)
        {
            //  Use await syntax to wait for the server method to complete and try...catch syntax to handle errors.
            await Clients.All.SendAsync(ChatEvents.Message, new { Message = input.Messsage, Username = "Context.GetHttpContext().User.Identity.Name " });
        }
        // metods for sending streams using ChannelReader<T> or IAsyncEnumerable<T>
        public async IAsyncEnumerable<int> CounterStream(CounterInput counter, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < counter.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return i;
                await Task.Delay(counter.Delay, cancellationToken);
            }
        }

        public ChannelReader<int> CounterChannel(CounterInput counter, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<int>();
            _ = writeToChannel(channel.Writer, counter, cancellationToken);// we should return the channel reader asap
            return channel.Reader;
        }

        private async Task writeToChannel(ChannelWriter<int> channel, CounterInput counter, CancellationToken cancellationToken)
        {
            Exception writeException = null;
            try
            {
                for (int i = 0; i < counter.Count; i++)
                {
                    await channel.WriteAsync(i, cancellationToken);
                    await Task.Delay(counter.Delay, cancellationToken);
                }

            }
            catch (System.Exception ex)
            {

                writeException = ex;
            }
            finally
            {
                channel.Complete(writeException);
            }
        }

        public async Task UploadChannelReader(ChannelReader<string> stream)
        {
            while (await stream.WaitToReadAsync())
            {
                var item = await stream.ReadAsync();
                Console.WriteLine(item);
            }
        }

        public async Task UploadStream(IAsyncEnumerable<string> stream)
        {
            await foreach (var item in stream)
            {
                // cancellationToken.ThrowIfCancellationRequested(); // This make the problem to fail/cancel at calling it
                Console.WriteLine(item);
            }

            // Here the stream should closed by sender which is client
        }

    }

}
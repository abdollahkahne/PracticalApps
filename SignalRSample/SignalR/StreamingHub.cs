using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRSample.SignalR
{
    public class StreamingHub : Hub
    {
        // async iterator method  (Simplest way to return an IAsyncEnumerable<T>)
        public async IAsyncEnumerable<int> Counter(CounterStream counter, [EnumeratorCancellation] CancellationToken cancellationToken) // the cancellation token received as IAsyncEnumerable<out T>.GetAsyncEnumerator(CancellationToken) 
        {
            for (int i = 0; i < counter.Count; i++)
            {
                // Check the cancellation token regularly so that the server will stop
                // producing items if the client disconnects.
                cancellationToken.ThrowIfCancellationRequested();// since this is an async enumerator method throwing does not effect the produced items. Only discontinue method with an error
                yield return i;
                // Console.WriteLine(i + " Is not Cancelled!"); // This cancelled if we dispose the subscription from client side
                await Task.Delay(counter.Delay, cancellationToken);
            }
        }

        // Streaming through SendAsyn method: I think it may be possible but it is not as easy as none-stream case
        // streams only send on demand and consider them to pull directly in case of a message
        // What if we want to push an special case of users (Caller, Groups, Other) an stream? 
        public async Task SendCounter()
        {
            // var counterStream = await Task.FromResult(Counter(new CounterStream { Count = 20, Delay = 1000 }, default(CancellationToken)));
            // await Clients.Caller.SendAsync("myStream", counterStream);
            await Clients.Caller.SendAsync("myStream", ChannelCounter(new CounterStream { Count = 60, Delay = 100 }, default(CancellationToken))); // Here only a message sent to browser to himself see the stream
        }

        // ChannelReader Has some problem compare to IAsyncEnumerable<T>:
        // 1- It is not return the initial Channel Reader early enough
        // 2- do not complete channel writer when exiting
        // but
        // Creates an IAsyncEnumerable<T> that enables reading all of the data from the channel using channelReaderInstance.ReadAllAsync(cancellationToken);
        public ChannelReader<int> ChannelCounter(CounterStream counter, CancellationToken cancellationToken)
        {
            // Create an unbounded channel (so its capacity property is not bounded)
            var channel = Channel.CreateUnbounded<int>(); // use static method for creation of channels. Also we can use new Channel<T>() or new Channel<TWrite,TRead>() to create 
            // here we should not use await
            _ = writeItemsAsync(channel.Writer, counter.Count, counter.Delay, cancellationToken); // the _ is discard variables and has some advantage in readability, out variable when you do not need it, tuples and destructon when you do not need them and switch for other case
            return channel.Reader; // We return the reader of channel to client (to read from it) but we work on writer to add the contents to the queue simulatenously.
            // and sync this method is not async-await, Whenever an object is written to the ChannelWriter<T>, the object is immediately sent to the client through its realting reader.
            // Other hub invocations are blocked until a ChannelReader is returned (consider that this is different than channel closing/disposing).
        }
        private async Task writeItemsAsync(ChannelWriter<int> writer, int count, int delay, CancellationToken cancellationToken) // CancellationToken parameter is triggered when the client unsubscribes from the stream.
        // Use this token to stop the server operation and release any resources if the client disconnects before the end of the stream.
        {
            // better to do this service in a background thread (here it is background thread itself since we do not use async-await in main caller)
            // Wrap logic in a try ... catch statement. Complete the Channel in a finally block.
            Exception localException = null;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    await writer.WriteAsync(i, cancellationToken); // this writer writing to a queue and here we add this writing to queue
                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                localException = ex; // Here only save the exception and do not throw it here
            }
            finally
            {
                writer.Complete(localException); // At the end, the ChannelWriter is completed to tell the client the stream is closed.
            }
        }

        // A hub method automatically becomes a client-to-server streaming hub method when it accepts one or more objects of type ChannelReader<T> or IAsyncEnumerable<T>. 
        public async Task UploadStream(ChannelReader<string> channelReader)
        {
            while (await channelReader.WaitToReadAsync()) // This reads as soon as the new data arrived.
            {
                var item = await channelReader.ReadAsync();// If the method is readable, read it and since it is ValueTask it only can read once!
                Console.WriteLine(item);
            }
        }

        public async Task UploadAsyncEnumerable(IAsyncEnumerable<string> stream)
        {
            // var cts = new CancellationTokenSource();
            // stream.WithCancellation(cts.Token);
            // cts.CancelAfter(5000);// Cancel the stream from server side: is it possible??? Not directly
            await foreach (var item in stream) // consider that we can not use await directly to IAsyncEnumerable (Altough it has async in name) but we can wait in its enumeration
            {
                // cts.Token.ThrowIfCancellationRequested(); // Does this make the client to stop sending stream? No we should send the caller a message to him cancel it if be interested!!
                Console.WriteLine(item);
            }

        }

    }
    public class CounterStream
    {
        public int Count { get; set; }
        public int Delay { get; set; }
    }
}


// Streaming Method
// A Hub is not streaming but the method is streaming,
// supports streaming from client to server and from server to client. 
// When streaming, each fragment is sent to the client or server as soon as it becomes available, rather than waiting for all of the data to become available.
// A streaming method return IAsyncEnumerable<T> or ChannelReader<T> sync or async (Task<IAsyncEnumerable<T>>, or Task<ChannelReader<T>>)
// The simplest way to return IAsyncEnumerable<T> is by making the hub method an async iterator method
// Here instead of invocation we have Subscription term and client subscribe to a method instead of inboke it. 
// In Subscription, we need a cancellation token to support Unsubscribe event too. 
// Two common Problem in ChannelReader<T>:
// Exit the method
// Not return the result early
// With AsyncEnumerable<T> the problem solved.


// Channel Reader has following method and props(Not all method and props is written here)
//  Count: Int=> Gets the current number of items available from this channel reader.
//  Completion:Task =>Gets a Task that completes when no more data will ever be available to be read from this channel.
//  CanCount:bool => Gets a value that indicates whether Count is available for use on this ChannelReader<T> instance.
//  CanPeek: bool => Gets a value that indicates whether TryPeek(T) is available for use on this ChannelReader<T> instance.

// 1- ReadAllAsync(cancellationToken) => Get and IAsyncEnumerable from Channel
// 2- ReadAsync(cancellationToken) => Read item availabe in Channel (FIFO) async and if not cancelled remove the item from channel
// 3- WaitToReadAsync(cancellationToken) => This wait until an item get available for read and return ValueTask<bool>
// 4- virtual bool TryPeek (out T item)
// 5- abstract bool TryRead (out T item);

// Channel Writer: Provide a base class for writing to channel
// 1- WaitToWriteAsync(cancellationToken): Returns a ValueTask<bool> that will complete when space is available to write an item.
// 2- WriteAsync(T,cancellationToken): Asynchronously writes an item to the channel. This return a ValueTask.
// 3-(Abstract) TryWrite(T): Attempts to write the specified item to the channel.Return true if item added to the channel. This is abstract method
// 4- Complete(exception): Mark the channel as being complete, meaning no more items will be written to it (the complete state used in channel and channel reader)
// 5- TryComplete(exception): Attempts to mark the channel as being completed, meaning no more data will be written to it.


// IAsyncEnumerable<T>: Exposes an enumerator that provides asynchronous iteration over values of a specified type. (As you see only enumeration be done async so it is not async itself)
// This class has not other special property or method. Only one method:
// 1- GetAsyncEnumerator(cancellationToken) which used behind the scene when enumeration. After this we can write to channel one item

// An Abstract method is a method without a body. The implementation of an abstract method is done by a derived class.

//Since we have not the ChannelReader and IAsyncEnumerable in JavaScript client, we have the similar concept with Class Subject which create using new signalR.Subject() and can be added item to it. 
// At the sever we can read its items using a ChannelReader or an IAsyncEnumerable
// A Subject is a special type of Observable that allows values to be multicasted to many Observers. Subjects are like EventEmitters.
// we have too similar concept here:
// 1- event emiter/raiser and event Catchers/handlers
// 2- Observable => Observers
// Every Subject is an Observable and an Observer (This not implemented yet apparantly and handle observers using ISubscribeResult and IStreamSubscriber if it is required). You can subscribe to a Subject, and you can call next to feed values as well as error and complete it.
// 
// It can be created using subject=new signalR.Subject() and has two properties Observers and cancelCallback and 4 methods;
// observers:Array<IStreamSubscriber<T>>:Shows the observers which observe this and to add an observer to it we use subject.subscribe(IStreamSubscriber<T> subscription )
// cancellCallback:()=>Promise(void): It can set manually to add a callback which runs on cancellation
// To feed it with new item we should call its next(T) method: subject.next(item);
// To make it completed we should call its complete() method similart (Successful complete);
// To feed it with error we should call its error(any);
// To add new client side Obsever we can use its subscribe method which add an observer to this subject

// Generators in JS
// Another similar concept in JavaScript is Generatos which works similar to IAsyncEnumerable in dotnet.
// Regular functions return only one, single value (or nothing).
// Generators can return (“yield”) multiple values, one after another, on-demand. 
// To create a generator, we need a special syntax construct: function*, so-called “generator function”. (Has a star at the end of function word!). For example:
// function* generateSequence() { // using this syntax we defined to thing a normal function named generatedSequence which return an Generator Object which has a next() method which we describe it later
//   yield 1;
//   yield 2;
//   return 3;
// }
// function* vs function
// When generator function is called, it doesn’t run its code. Instead it returns a special object, called “generator object”, to manage the execution.
// let generator = generateSequence();
// alert(generator); // [object Generator] // It Create an Object Generator but consider that the generator function is not yet called. Only the function responsible for creation of Generator Object which saved at function (whitout star!) are called and return
// The Generator Object has one important method next() which return an object with the shape {value:1,done:false} at every call
// function* f(…) or function *f(…): both syntax or correct but the first preferred
// Generators are iterator (this is like that say IAsyncEnumerable is an enumerable!! but in js most of built in objects like array are iterable) which means we can use for (let item of obj) {console.log(item);}. 
// Since generators are iterator, we can use generator in a loop instead of generatorObj.next() multiple times!
// for (let val in generatorObj) {console.log(val);} // Only log 1,2 as primitive value
// The difference here is that it only return none-completed values (the value which returned with yield) and the completed value which return using return itself does not considered in loop (It is equal to call the generatorObj.return()). 
// with iterable object we can use spread too for example we have :[1,...generatorObj];
// we can convert none-iterable object to iterable using [Symbol.iterator] property of them by setting its value as a generator object
// For example consider the Range Object. in js we can define a range as let rng={from:1,to:10} and convert it to iteratore. So we add the [Symbol.iterator] to this objec as generator object:
/*  rng[Symbol.iterator]=function() {
    return {
        current:this.from,
        last:this.to,
        next(){
            if (this.current<this.last) {
                return {value:this.current++,done:false};
            } else {
                return {done:true}
            }
        }
    }
 } */

// Or we can use the generator syntax directly as below:
/* rng[Symbol.iterator]=function*() {
   for (let value=this.from;value<=this.to;value++) {
       yield value;
   }
}

*/
// Here Generator are a syntatic sugar just to like it easy to create an object with next() method!
// There is another syntatic sugar here to compose multiple generator object into once: yield* (it spread the generators next result and combine into each other in runtime! an not built time)
// function* generateSequence(start, end) {
//   for (let i = start; i <= end; i++) yield i;
// }

// function* generatePasswordCodes() {

//   // 0..9
//   yield* generateSequence(48, 57);

//   // A..Z
//   yield* generateSequence(65, 90);

//   // a..z
//   yield* generateSequence(97, 122);

// }

// Yield is a two-way street in Generators
// Another feature related to yield and next() method in generator is that we can use next function of a generator to feed the generator a value in case of not returned yields. For example consider we have let question=yield "2+2=?":
// Here yield does not return any thing to the outer function. but it open an option to feed inner function from outer function using next() method (Only in Generators! since the next defined with this in mind!)
// so if we call generatorObj.next(4) the question variable set with 4. In fact until we do not consume yield result it is ready to set with next method.
// generatorObj.next(4) is equal to add yield 4 to the current position of generator function
// We can complete the generator every time with return method of it as generatorObj.return();
// we can return an error using throw method of it which needs an error as argument too: generatorObj.throw(new Error("Error"));

// Observables and Observers in Reactive JS (RxJS) (It seems that is different than Microsoft SignalR from some aspects. I personally prefer Microsoft signalR since it is simple to learn and implement. I think the similarity between them is limited to just the Subject which is a multicast (executed once for multiple observers like event emitter)!!)
// Observables vs Promise: They are similar someway. They used for event-handling both but in case of Promise we only have one event which start immidiately when using and it can not be cancelled.
// Observers can handle zero or more events (Work like stream) and Can be start when required and can be cancelled on demand.
// Pull vs Push and single vs multiple:
// What is Push? In Push systems, the Producer determines when to send data to the Consumer. The Consumer is unaware of when it will receive that data (Promise and Observables). 
// Some times we should call a function/object to work done (pull) and sometime a funtion/object push some works to be done (push).
// For example we can call function to execute and return a value or an array (iterator/generator?) to get a sequence of data.
// Or we can use a promise an then wait to push a value to a callback/handler (considered it resolved) and similary use an Observable to push a sequence/stream of values to its observers/subscribers.
// Observables are like functions with zero arguments, but generalize those to allow multiple values (using yield for example).
// Also we can call a function using foo.call() and similary call observable like foo.subscribe(callback);
// Observable usually is not event emitter and we should call them ourselved but Subject is a special type which is an event-emitter 
// As opposed to EventEmitters which share the side effects and have eager execution regardless of the existence of subscribers, Observables have no shared execution (for each subscribe run isolated. what if we have multiple observers??!! we should subscribe to it to add to observer. but the execution I think is same for both!) and are lazy(do not call if not subscriber exist!).
// Observables are able to deliver values either synchronously or asynchronously. But it is wrong to claim Observables are asynchronous
// Important: What is the difference between an Observable and a function? Observables can "return" multiple values over time, something which functions cannot.
// Conclusion:
// func.call() means "give me one value synchronously"
// observable.subscribe() means "give me any amount of values, either synchronously or asynchronously"

// subscribe calls are not shared among multiple Observers of the same Observable. When calling observable.subscribe with an Observer, the function subscribe in new Observable(function subscribe(subscriber) {...}) is run for that given subscriber
// This is drastically different to event handler APIs like addEventListener / removeEventListener. With observable.subscribe, the given Observer is not registered as a listener in the Observable. The Observable does not even maintain a list of attached Observers.

// Eager vs lazy: 
// Subscribing to an Observable is analogous to calling a Function. it has same side effect in 


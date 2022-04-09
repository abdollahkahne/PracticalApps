export async function callArrayFromDotnet() {
    var arr=await DotNet.invokeMethodAsync("BlazorWebAssemblySignalRApp.Client","ReturnArrayAsync");
    console.log(arr);
    // return DotNet.createJSObjectReference(arr);
    // var arrC= new DotNet.DotNetObject(arr);
    // console.log(arrC);
    // return new DotNet.DotNetObject(arr);
    return arr; // This should be converted to .Net Array using Serialization
}

export async function callInstanceMethod(dotNetObj) {
    console.log(dotNetObj);// return DotNetObject for DotNetObjectReference
    var numbers=await dotNetObj.invokeMethodAsync("instanceMethodForArray");
    return ["This is first instance","Second Instance!"].concat(numbers.map(i=>`Instance ${i}`));
}

export function getScores(dotNetJSModules) {
    dotNetJSModules.invokeMethod("GetScore",12,18);
    dotNetJSModules.invokeMethod("GetCredit",2,4);
    dotNetJSModules.invokeMethod("GetAverage");
}

// If we want to work with JS DOM Event instead of Blazor events
// we can create a class which has the 
class Scores {
    dotNetJSModules;
    score;
    credits;
    constructor(instance) {
        this.dotNetJSModules=instance;
    }
    getScores(){
        this.score=this.dotNetJSModules.invokeMethod("GetScore",12,18);
    }
    getCredit() {
        this.credits=this.dotNetJSModules.invokeMethod("GetCredit",2,4);
    }
    getAverage() {
        this.dotNetJSModules.invokeMethod("GetAverage");
    }
}

export function workWithScores(dotNetObjRef) {
    var scores=new Scores(dotNetObjRef);
    scores.getCredit();
    console.log("Credits Created!");
    console.log(scores.credits)
    scores.getScores();
    console.log("Scores Created!")
    console.log(scores.score);
    scores.getAverage();
    console.log("Average Calculated!")
    return true; // We can use the input without returning it since change apply to dotnet object
}

export async function workWithStreams() {
    // Example 1
    var stream=new Blob(["This is a text blob created using its constructor."," We want to create a readable stream"],{type:"text/plain"});//.stream();

    // // Example 2
    // var response=await fetch("https://streams.spec.whatwg.org/");
    // var body=response.body; // this is a readable stream of a binary data (Uint8Array)
    // var stringStream=body.pipeThrough(new TextDecoderStream()); // this is a readable stream of strings

    // // Example 3
    // const compressed=body.pipeThrough(new CompressionStream("gzip"));
    await DotNet.invokeMethodAsync("BlazorWebAssemblySignalRApp.Client","workWithStreams",DotNet.createJSStreamReference(stream));
    return new Blob(["This is another text blob created using its constructor."," Apparantly the sream for IJSStreamReference is a js typed array or BufferArray and it throw error for ReadableStream. So the work in browser/JS Side should be done synchronousely"],{type:"text/plain"});;
}

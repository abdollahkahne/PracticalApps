// class DotNetStreamReference {
//     stream();
//     async arrayBuffer();
//     _streamPromise:Promise<ReadableStream>;
// }

function triggerFileDownload(filename,url) {
    const anchor=document.createElement("a");
    anchor.href=url;
    anchor.download=filename??"";
    anchor.click();
    anchor.remove();
}

export async function downloadFileFromStream(stream,filename) {
    // This function do the following:
    // 1- Create an ArrayBuffer(equal to Byte[]) from stream 
    // 2- Create a Blob to wrap arround ArrayBuffer (Blob can be accessed from different thread and services)
    // 3- provide a url to Blob using Url.createObjectUrl(blob)

    console.log(stream);

    // stream is a java script Object which has 2 special methods and one specia field
    //its methods are stream() and async arrayBuffer() which generate an stream or arrayBuffer asynchronousely
    // its field is internal named _streamPromise which is a promise fullfilled at the end to ReadableStream.
    // this ReadableStream has a getReader method which we can get a reader to read the stream byte by byte!
    const arrayBuffer= await stream.arrayBuffer();
    const blob=new Blob([arrayBuffer]);
    const url=URL.createObjectURL(blob);

    // create an a tag with this url and filename and call its click() method to download it and at the end remove it from DOM.
    triggerFileDownload(filename,url);

    // This is an important step to ensure memory isn't leaked on the client.
    URL.revokeObjectURL(url); // release the object url for later use
}

export function downloadFileFromUrl(url,filename) {
    triggerFileDownload(filename,url);
}
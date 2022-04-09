Work with Images
Images in DOM set using img tag and src property. Its src property should be Url and it is mandatory. So when we work with it from Blazor we only can set its value a URL. But what if the data be an stream which gets from server?
Here we can not use Blazor to set src Url directly. Altough we can set it if we have a url for stream but what if we have not that. In this case we should use JS Interop to create a url from stream. There is a function in js which create url from a File or Blob object:
URL.createObjectUrl(blob);
Each time you call createObjectURL(), a new object URL is created, even if you've already created one for the same object. Each of these must be released by calling URL.revokeObjectURL() when you no longer need them. For example In a form, the object URL is typically should be revoked after the user submits the form for processing, as the object URL is no longer required at that point.

Note: The image preview technique described in ImageSrcComponent in Blazor Server involves round-tripping the image data from the client to the server and back. In a future release, this aspect might be optimized to better facilitate image previews.

In this case Blazor sends stream to js as DotNetStreamReference. This has three methods after serialization:
* arrayBuffer(): this is an async function and should be used with await.
* blob(): This method does not exist but can easily get from array buffer using blob=new Blob([buffer]) where buffer=streamRef.arrayBuffer();
* stream(): this is not async function!
blob() method can use to create a blob and then create a url to that and assign to src of img element. 

Blob
The Blob object represents a blob, which is a file-like object of immutable, raw data; they can be read as text or binary data, or converted into a ReadableStream so its methods can be used for processing the data.
Blobs can represent data that isn't necessarily in a JavaScript-native format. The File interface is based on Blob, inheriting blob functionality and expanding it to support files on the user's system. 
To construct a Blob from other non-blob objects and data, use the Blob() constructor. This constructor get an array of data. Here we can see some example from it:
* Create Blob From a Json string or js object:
const obj = {hello: 'world'};
const blob = new Blob([JSON.stringify(obj, null, 2)], {type : 'application/json'});
* Create a Blob from a TypedArray specifying its mim-type: new Blob([typedArray.buffer], {type: mimeType}))
const bytes = new Uint8Array(59);
for(let i = 0; i < 59; i++) {
  bytes[i] = 32 + i;
}
new Blob([bytes.buffer], {type: "text/plain"}));

It has following API (methods and fields)
* size
* type
* arrayBuffer(): Returns a promise that resolves with an ArrayBuffer containing the entire contents of the Blob as binary data.
* stream(): Returns a ReadableStream that can be used to read the contents of the Blob.
* text(): Returns a promise that resolves with a USVString containing the entire contents of the Blob interpreted as UTF-8 text.
* slice(): this slice a blob to other blobs.

Note: To read a blob as string you can use it text() method to get Unicode Scaler Values (USVString) but you can use methods for reading content of file to get its content as string/url/binary as below:
const reader = new FileReader();
reader.addEventListener('loadend', () => {
   // reader.result contains the contents of blob as a typed array
});
reader.readAsArrayBuffer(blob);
// By using other methods of FileReader, it is possible to read the contents of a Blob as a string or a data URL. (readAsArrayBuffer(), readAsBinaryString(), readAsDataUrl(), readAsText())
https://developer.mozilla.org/en-US/docs/Web/API/FileReader

Note: Another approach for reading content of a Blob is creating a response using it as content and then read the response:
const text = await (new Response(blob)).text();

Note: File is derived from Blob so we can use Blob instead of File in APIs and use Blob methods in Files. 

File
The File interface provides information about files and allows JavaScript in a web page to access their content.
File objects are generally retrieved from a FileList object returned as a result of a user selecting files using the <input> element or from a drag and drop operation's DataTransfer object.
A File object is a specific kind of a Blob, and can be used in any context that a Blob can. In particular, FileReader, URL.createObjectURL(), createImageBitmap(), and XMLHttpRequest.send() accept both Blobs and Files.
You can see following link to learn more about files:
https://developer.mozilla.org/en-US/docs/Web/API/File/Using_files_from_web_applications
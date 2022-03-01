InputFile and File Upload in Blazor
<InputFile> Component does not need EditForm and EditContext to work. It implemented directly ComponentBase and so it has not field related to InputBase like Value and ValueChanged. 
In work with files in Blazor there is two important concept:
1- Stream (both in Server and client some times): Stream is a sequence of bytes which has some fields like Length,CanRead,CanWrite,Position,CanSeek and usually copy to other stream using Stream helper methods. It can get buffer to an array of bytes after reading and saving in memory. It is in memory so we can do some work with it and it is somehow native to language created it (By it is binary and it has some char set and char encoding). There is something which is immutable and saved on physical disk which get from stream using buffer which named blob. Buffer is something temporary for work and also stream. but BLOB is permanent and multiple thread can work with it and is not native to language and runtime created it. 
2- IBrowserFile: IBrowserFile returns metadata exposed by the browser as properties. Use this metadata for preliminary validation:
* Name (filename.ext)
* LastModified (as DateTimeOffset)
* ContentType
* Size
* Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default);
* (Extension Method) RequestImageFileAsync: used to resize image file and use them as an img all using JS.

After selecting a file in InputFile a change event triggered which has following data in JavaScript value (or Files FileList which works using index[] for example this.files[0].name):
1- name: c:\fakepath\filename.ext
2- size
3- type
4- lastmodified in number

then an event InputFileChange event triggered by Blazor framework which has InputFileChangeEventArgs type and it has following file info:
1- FileCount
2- File: return IBrowserFile
3- GetMultipleFiles(int maximumFileCount = 10)

To read data from a user-selected file, call IBrowserFile.OpenReadStream on the file and read from the returned stream. OpenReadStream enforces a maximum size in bytes of its Stream. Reading one file or multiple files larger than 512,000 bytes (500 KB) results in an exception. 
Only in case it is necessary, use IBrowserFile.OpenReadStream. Also Avoid reading the incoming file stream directly into memory all at once. For example, don't copy all of the file's bytes into a MemoryStream or read the entire stream into a byte array all at once. Instead, consider adopting either of the following approaches:
    * Copy the stream directly to a file on disk without reading it into memory.
    * Upload files from the client directly to an external service

Here we see some unrecommended practice and the recommended one:
1-Do not use stream reader to read whole file as string. 
Stream stream = e.File.OpenReadStream();
string content = await new StreamReader(stream).ReadToEndAsync();
2- Do not copy to memory stream before upload to external services or backend file store:
using (var ms = new MemoryStream())
{
    await stream.CopyToAsync(ms);
}

3- A correct example for 2 is :
var stream = e.File.OpenReadStream();
var path = Path.Combine(Environment.CurrentDirectory, "Uploaded", e.File.Name);
Console.WriteLine(path);
using (var fs = new FileStream(path, FileMode.Create))
{
    await stream.CopyToAsync(fs);
}

Multiple File Picker
InputFileChangeEventArgs.GetMultipleFiles allows reading multiple files. Specify the maximum number of files to prevent a malicious user from uploading a larger number of files than the app expects. InputFileChangeEventArgs.File allows reading the first and only file if the file upload doesn't support multiple files. So in case of multiple it is null. for multiple file use e.GetMultipleFiles to get a List<IBrowserFile> and then you can use this list to enumerate through and for example use OpenReadStream to get an stream to read the related file. 

Note: Never trust the values of the following properties, especially the Name property for display in the UI. Don't trust file names supplied by clients for:
    Saving the file to a file system or service.
    Display in UIs that don't encode file names automatically or via developer code.
Note: The examples saves files without scanning their contents. In production scenarios, use an anti-virus/anti-malware scanner API on uploaded files before making them available for download or for use by other systems. 

How to upload file using an API?
1- Implement API: implement a controller which receive an IFormFile or IEnumerable<IFormFile> or IFormFileCollection. Then inside it use FileStream to copy the stream of each file to a path. for creation of path, you can use Path class static methods like GetRandomFileName(), GetExtension() and Combine(). 
IFormFile Represents a file sent with the HttpRequest. It has information about file headers and also OpenReadStream() to get it as a stream or CopyToAsync(stream) to copy it to another stream. 
IFormFileCollection implements IEnumerable<IFormFile> and List<IFormFile> and Represents the collection of files sent with the HttpRequest. to get a file with name you can use its Item[filename] property or its GetFile(string) method. If you use a multiple input, you can use GetFiles(string) to get a list of files with the specified name or directly use IEnumerable<IFormFile> parameter with the name!
2- Upload to API: To upload a file from client, you should select multipart form-data http content class (MultipartFormDataContent) and then add files selected in OnChange event callback to it using an stream content created from file stream in client. 
note: untrusted/unsafe file name is automatically HTML-encoded by Razor for safe display in the UI. But to display it other places like logs you can encode it using HtmlEncoder.Default.Encode(item.FileName) or System.Net.WebUtility.Encode(item.FileName)
note: To get content type of a file use new MediaTypeHeaderValue(file.ContentType)

How to implement Progress Bar when uploading?
You can use stream to stream with a specified buffer. for example consider you have the writeable stream fileSteam from file system and readablestream e.File.OpenReadStream(). To read or write using a buffer you can define a buffer with byte[cap] and then use it in readableStream.ReadAsync(buffer) or writeableStream.WriteAsync(buffer);

In Blazor Server, file data is streamed over the SignalR connection into .NET code on the server as the file is read. 

File Download
In downloading file in Blazor we have 2 situation:
1- Directly from a url which can simply used with an <a href="url" download />. For file downloads over 250 MB, we recommend this. 
2- Download from an stream: In this case we can use JavaScript Interoperability and send the stream to JavaScript and then convert that stream to a url asynchronously (stream=>bufferArray(bytesArray)=>blob=>url) and use the url to download the stream contents. Consider that in this case the stream already downloaded to client or dynamicly created, since streaming done in Blazor WebAssembly Host. 
When downloading files, Cross-Origin Resource Sharing (CORS) considerations apply (even from hosted web).
Native byte[] streaming interop is used to ensure efficient transfer to the client (DotnetStreamReference). 
we have following object in dotnet for this purpose:
0- IJSRuntime: this is a runtime of js which can be used in dotnet to call js functions. It has two main methods: InvokeAsync<T> and InvokeVoidAsync. 
1- IJSObjectReference: this is used to add a module from JS to dotnet and then use it. Actualy we use it as T in InvokeAsync method of object IJSRuntime and function "import" in js. 
3- DotNetStreamReference: this is used to send an stream from dotnet to js and use it in js. The stream natively has reachable from js function then.
4- We also other method which can be use in JS Interoperability. To use an object or stream from js in dotnet you should search for IJSObjectReference or IJSStreamReference and to use object or stream from dotnet in js you should use DotnetObjectReference and DotNetStreamReference respectively. 

Security considerations
Use caution when providing users with the ability to download files from a server. Attackers may execute denial of service (DOS) attacks, API exploitation attacks, or attempt to compromise networks and servers in other ways.
Security steps that reduce the likelihood of a successful attack are:
    1- Download files from a dedicated file download area on the server, preferably from a non-system drive. Using a dedicated location makes it easier to impose security restrictions on downloadable files. 
    1.5- Disable execute permissions on the file download area.
    2- Verify that client-side checks are also performed on the server. Client-side checks are easy to circumvent.



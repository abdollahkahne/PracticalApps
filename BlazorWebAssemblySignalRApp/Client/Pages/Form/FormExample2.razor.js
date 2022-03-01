export function logValue(elem) {
    console.log(elem.name+": "+elem.value);
}
export async function setImageSrc(stream) {
    console.log("I runned ...");
    console.log(stream);
    
    // To use stream we can use stream method which provided by dotnet (return readablestream so we can convert it to blob and then use it) or use its _streamPromise property which resolve to a ReadableStream
    stream.stream().then(stream => new Response(stream))
    .then(response => response.blob())
    .then(blob => URL.createObjectURL(blob))
    .then(url => {
        const img = document.querySelector("img.net");
        img.src = url;
    })
    .catch(err => console.error(err));

    // To use stream we can use ArrayBuffer async method which provided by dotnet too. It creates a buffer chunk which can be used to create the blob
    // var buffer=await stream.arrayBuffer(); // this create a chunk so to use it we should add []
    // var blob=new Blob([buffer]);
    // const url = URL.createObjectURL(blob);
    // const img = document.querySelector("img.net");
    // img.src = url;
        
}

// The other approach is to use JS File Reader to read image and then use its result to show image
function handleFiles(files) {
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
  
      if (!file.type.startsWith('image/')){ continue }
  
      const img = document.createElement("img");
      img.classList.add("obj");
      img.file = file;
      preview.appendChild(img); // Assuming that "preview" is the div output where the content will be displayed.
  
      const reader = new FileReader();
      reader.onload = (function(aImg) { return function(e) { aImg.src = e.target.result; }; })(img); // the function retun a function since we required to use image from outer scope inside it otherwise the outer scope update an it always mention the last image element (matter in multiple file case!)!
      reader.readAsDataURL(file);
    }
  }
  
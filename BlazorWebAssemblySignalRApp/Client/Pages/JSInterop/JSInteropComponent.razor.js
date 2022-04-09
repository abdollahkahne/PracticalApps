class Student {
    constructor(student) {
        this.name = student.name;
        this.age = student.age;
        this.isMarried = student.isMarried;
        // this.created=new Date().toISOString();
        // this.created=new Date(Date.now());
        // this.created=new Date();
        // this.created=student.created;
        this.created="2022-02-28T01:18:06.932+08:30";
        console.log(this.created);
        return this;
    }
}
export function getStudent(student) {    
    var std= new Student(student);
    console.log(std.created);
    return std;
}
export function getPromise(student) {
    return new Promise((resolve,reject)=>{setTimeout(()=> resolve(getStudent({name:"Ali",isMarried:true,age:32})),2000);})
}

// use Browser API to do some task
// text decoder in browsers
// Decode means get a byte array and decode it to string
// Byte Array in browsers (ArrayBuffer and ArrayBufferView (ArrayBufferView is a helper type representing any of the following JavaScript types like Int8Array)) inludes Uint8Array,Int8Array,Uint16Array,Int16Array
export function convertArray(windows1251Arr) {
    // create decoder
    var decoder=new TextDecoder("windows-1251");
    // convert to uint8 array to work in decode function
    var uint8Arr=new Uint8Array(windows1251Arr); // 
    
    var decoded=decoder.decode(uint8Arr);
    console.log(decoded);
    return decoded;
}

export function showPromptLog(str) {
    console.log(str);
    // The following only works on Chrome and other browsers hang!!
    // debugger;
    window.alert(str);
}

export function changeBackgroundColor(elem) {
    var divElem=document.createElement("div");
    divElem.innerHTML=`<h4>Injected By JS</h4><p>since this is injected by JS interop. Blazor may interfere with it. But does it get things changed?</p>`;
    elem.style.backgroundColor="pink";
    elem.style.border="2px solid grey";
    elem.style.padding="10px";
    elem.style.margin="5px";
    // elem.appendChild(divElem);
    elem.innerHTML=divElem.innerHTML;
}
export function workWithFileStream(streamRef) {
    console.log(streamRef);
    console.log(streamRef.stream()); // this is a promise that resolved to a readable stream in js. 
    const reader = streamRef.stream().then(stream =>workWithReadableStream(stream));
    function workWithReadableStream(stream) {
        const reader=stream.getReader();
        let count = 0;
        let receivedText="";
        reader.read().then(result=>processText(result,count,receivedText));

        // We implemented a loop/recursive function here
        function processText({done,value},charsReceived,receivedText) {
            if (done) {
                console.log("Stream Reading Completed");
                console.log("Value should be null:",value);
            } else {
                let updatedCharsReceived=value.length+charsReceived;
                console.log('Received ' + updatedCharsReceived + ' characters so far. Current chunk = ' + value);
                let updatedReceivedText= receivedText+value;
                console.log("Received Result so far:",updatedReceivedText);
                return reader.read().then(result=>processText(result,updatedCharsReceived,updatedReceivedText));
       
            }
        }
    }
    
  
    // // read() returns a promise that resolves
    // // when a value has been received
    // reader.read().then(function processText({ done, value }) {
    //   // Result objects contain two properties:
    //   // done  - true if the stream has already given you all its data.
    //   // value - some data. Always undefined when done is true.
    //   if (done) {
    //     console.log("Stream complete");
    //     // para.textContent = value;
    //     console.log(value);
    //     return;
    //   }
  
    //   // value for fetch streams is a Uint8Array
    //   charsReceived += value.length;
    //   const chunk = value;
    // //   let listItem = document.createElement('li');
    // //   listItem.textContent = 'Received ' + charsReceived + ' characters so far. Current chunk = ' + chunk;
    // //   list2.appendChild(listItem);
    // console.log('Received ' + charsReceived + ' characters so far. Current chunk = ' + chunk);
  
    //   result += chunk;
  
    //   // Read some more, and call this function again
    //   return reader.read().then(processText);
    // });
}
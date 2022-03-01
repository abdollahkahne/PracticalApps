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
    debugger;
    window.alert(str);
}
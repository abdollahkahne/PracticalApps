export async function setImageUrlFromStream(imgId,stream) {
    // console.log(stream);
    const buffer=await stream.arrayBuffer();
    // console.log(buffer);
    const blob=new Blob([buffer]);
    // console.log(blob.text());
    readBlob(blob);
    await readBlobUsingResponse(blob);
    const url=URL.createObjectURL(blob);
    const imgElem=document.getElementById(imgId);
    imgElem.src=url;
}
function readBlob(blob) {
    const reader=new FileReader();
    reader.onloadend=()=>{
        console.log(reader.result);
    };
    reader.readAsDataURL(blob);
}

async function readBlobUsingResponse(blob) {
    var response=new Response(blob);
    console.log(await response.arrayBuffer());
}
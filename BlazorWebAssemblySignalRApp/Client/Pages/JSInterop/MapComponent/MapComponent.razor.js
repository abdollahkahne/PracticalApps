// So we can import js in our js too
import 'https://api.mapbox.com/mapbox-gl-js/v1.12.0/mapbox-gl.js';
mapboxgl.accessToken="pk.eyJ1IjoibWVoZGltb3dsYXZpMTk4NCIsImEiOiJjbDBiZWNic3kwc2F0M2NycXN6cHZyZnc2In0.TbTu7r2QXjFDb_yTjvZN1g";

export function addMapToElement(element) {
    return new mapboxgl.Map({
        container:element,
        style:'mapbox://styles/mapbox/streets-v11',
        center:[-74.5,40],
        zoom:9,
    });
}
export function setMapCenter(map,latitude,longitude) {
    map.setCenter([longitude,latitude]);
}

// If this invokes from Blazor it return Utf-8 string but in case of direct use it gives base64 string
export function decodeByteArray(bytes) {
    var decoder=new TextDecoder();
    var str=decoder.decode(bytes);
    return str;
}
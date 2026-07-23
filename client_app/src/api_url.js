import { Capacitor } from '@capacitor/core';


let _url = "";

if (Capacitor.isNativePlatform()) {
    _url = "";
}

const url = _url;

export default url;
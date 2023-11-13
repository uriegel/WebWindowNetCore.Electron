import { contextBridge, ipcRenderer } from "electron"

contextBridge.exposeInMainWorld(
    "api", {
        invoke: (channel: string, data: any) => {
            //let validChannels = ["openDevTools", "show", "dropFiles", "startDrag"] 
            let validChannels = ["openDevTools", "show", "dropFiles"] 
            if (validChannels.includes(channel)) 
                return ipcRenderer.invoke(channel, data) 
            else
                throw "Channel not valid"
        },
    }
);
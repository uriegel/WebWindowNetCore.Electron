import { BrowserWindow, app } from "electron"

interface StartInfo {
    title: string
}

const startInfo: StartInfo = JSON.parse(process.env['StartInfo']!)

const createWindow = async () => {
    const win = new BrowserWindow({title: startInfo.title})
    win.removeMenu()   

    win.once('ready-to-show', win.show)

    win.loadURL(`https://www.google.de`)

    win.webContents.on('did-finish-load', function() {
        win.webContents.executeJavaScript(`alert('Hello There ${startInfo.title}')`);
    })
}

app.on('ready', createWindow)
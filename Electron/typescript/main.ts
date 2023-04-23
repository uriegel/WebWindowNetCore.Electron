import { BrowserWindow, app } from "electron"

interface StartInfo {
    title: string,
    url: string,
    iconPath?: string
}

const startInfo: StartInfo = JSON.parse(process.env['StartInfo']!)

const createWindow = async () => {
    const win = new BrowserWindow({title: startInfo.title, icon: startInfo.iconPath ?? "" })
    win.removeMenu()   

    win.once('ready-to-show', win.show)

    win.loadURL(startInfo.url)

    win.webContents.on('did-finish-load', function() {
        win.webContents.executeJavaScript(`alert('Hello There ${startInfo.url}')`);
    })
}

app.on('ready', createWindow)
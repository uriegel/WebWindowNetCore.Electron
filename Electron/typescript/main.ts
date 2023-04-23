import { BrowserWindow, app, ipcMain } from "electron"

interface StartInfo {
    title: string,
    url: string,
    iconPath?: string
    showDevTools: boolean
}

const startInfo: StartInfo = JSON.parse(process.env['StartInfo']!)

const createWindow = async () => {
    const win = new BrowserWindow({ title: startInfo.title, icon: startInfo.iconPath ?? "", webPreferences: {
        preload: `${__dirname}/preload.js`
    }})
    win.removeMenu()   

    win.once('ready-to-show', win.show)

    win.loadURL(startInfo.url)

    win.webContents.on('did-finish-load', function () {
        if (startInfo.showDevTools)
            win.webContents.executeJavaScript(`
                async function webViewShowDevTools() {
                    await window.api.invoke('openDevTools')
                }`)
    })

    ipcMain.handle('openDevTools', async (event, arg) => {
        return new Promise(function (resolve) {
            win.webContents.openDevTools()
            resolve("")
        })
    })
}

exports.call = (payload: string) => {
    console.log("Gekohlt", payload)
}

app.on('ready', createWindow)


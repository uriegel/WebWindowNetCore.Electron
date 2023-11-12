import { BrowserWindow, app, ipcMain } from "electron"

interface StartInfo {
    title: string,
    url: string,
    iconPath?: string
    showDevTools: boolean
    saveBounds: boolean
    dropFiles: boolean
}

const startInfo: StartInfo = JSON.parse(process.env['StartInfo']!)

const createWindow = async () => {
    const win = new BrowserWindow({ title: startInfo.title, icon: startInfo.iconPath ?? "", show: !startInfo.saveBounds, webPreferences: {
        preload: `${__dirname}/preload.js`
    }})
    win.removeMenu()   

    win.loadURL(startInfo.url)

    win.webContents.on('did-finish-load', function () {
        if (startInfo.saveBounds)
            win.webContents.executeJavaScript(`
                const bounds = JSON.parse(localStorage.getItem('window-bounds') || '{}')
                const isMaximized = localStorage.getItem('isMaximized')
                if (bounds.width && bounds.height)
                    window.api.invoke('show', JSON.stringify({width: bounds.width, height: bounds.height, isMaximized: isMaximized == 'true', x: bounds.x, y: bounds.y}))
                else
                    window.api.invoke('show')`)
        if (startInfo.showDevTools)
            win.webContents.executeJavaScript(`
                async function webViewShowDevTools() {
                    await window.api.invoke('openDevTools')
                }`)
        if (startInfo.dropFiles)
            win.webContents.executeJavaScript(`
                async function webViewDropFiles(id, move, files) {
                    let paths = Array.from(files).map(f => f.path)
                    await window.api.invoke('dropFiles', JSON.stringify({paths, move, id}))
                }`)
        })

    var timer: NodeJS.Timeout

    const onBounds = () => {
        clearTimeout(timer)
        timer = setTimeout(() => {
            if (!win.isMaximized())
                win.webContents.executeJavaScript(`
                    localStorage.setItem('window-bounds', JSON.stringify({
                        width: ${win.getBounds().width}, 
                        height: ${win.getBounds().height},
                        x: ${win.getBounds().x},
                        y: ${win.getBounds().y}
                    }))
                    localStorage.setItem('isMaximized', false)
                `)
            else
                win.webContents.executeJavaScript(`localStorage.setItem('isMaximized', true)`)
        }, 400)
    }

    win.on("resize", onBounds)
    win.on("move", onBounds)

    ipcMain.handle('openDevTools', async (event, arg) => {
        return new Promise(function (resolve) {
            win.webContents.openDevTools()
            resolve("")
        })
    })
    ipcMain.handle('show', async (event, arg) => {
        return new Promise(function (resolve) {
            if (arg) {
                var action = JSON.parse(arg)
                if (action.width && action.height && action.x && action.y) 
                    win.setBounds({ width: action.width, height: action.height, x: action.x, y: action.y })
                if (action.isMaximized) 
                    win.maximize()
                else
                    win.show()
            } else
                win.show()
            resolve("")
        })
    })
    ipcMain.handle('dropFiles', async (event, arg) => {
        return new Promise(function (resolve) {
            console.log(arg)
            resolve("")
        })
    })
}

app.on('ready', createWindow)


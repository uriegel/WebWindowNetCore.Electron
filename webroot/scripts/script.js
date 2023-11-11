const source = new EventSource("http://localhost:20000/sse/test")
source.onmessage = (event) => console.log("SSE event", event.data)

const btn1 = document.getElementById("button")
const btn2 = document.getElementById("button2")
const btnDevTools = document.getElementById("buttonDevTools")
const dropZone = document.getElementById("dropZone")

dropZone.addEventListener("dragover", e => {
    e.preventDefault()
    e.stopPropagation()
})

dropZone.addEventListener("drop", e => {
    e.preventDefault()
    e.stopPropagation()

    let pathes = Array.from(e.dataTransfer.files).map(f => f.path)
    console.log("pathes", pathes)
})

btnDevTools.onclick = () => webViewShowDevTools()

btn1.onclick = async () => {
    var res = await webViewRequest("cmd1", {
        text: "Text",
        id: 123
    })
    console.log("cmd1", res)
}

btn2.onclick = async () => {
    var res = await webViewRequest("cmd2", {
        name: "Text",
        number: 123
    })
    console.log("cmd2", res)
}



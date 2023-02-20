import 'highlight.js/styles/github.css'
import hljs from 'highlight.js/lib/core'
import { Component, JSXElement } from 'solid-js';

// I need to rearrange this to be headless, use standard copy/download icons


// https://stackoverflow.com/questions/25734072/creating-javascript-blob-with-data-from-html-element-then-downloading-it-as-a

export const DownloadText: Component<{ text: string, children: JSXElement }> = (props) => {
    var blob = new Blob([props.text], { type: 'text/plain' });
    let textFile = window.URL.createObjectURL(blob);
    return <a download >{props.children}</a>
}

export const CodeView: Component<{ downloadAs?: string, code: string, language: string }> = (props) => {
    const x = hljs.highlight(props.code, {
        language: props.language
    }).value;

    const url = URL.createObjectURL(new Blob([props.code], { type: "text/plain" }));
    let a: HTMLAnchorElement
    return <div class='w-full'>
        <div class='flex flex-row'>
            <button onClick={() => { navigator.clipboard.writeText(props.code) }}>Copy</button>
            <button onClick={() => { a.click(); }} >Download</button>
            <a href={url} download={props.downloadAs ?? "file.txt"} ref={a!}></a>
        </div>
        <div class='w-full mt-4 overflow-hidden ' >
            <pre style='word-break: break-all; white-space: pre-wrap' innerHTML={x}>

            </pre>
        </div></div>
}
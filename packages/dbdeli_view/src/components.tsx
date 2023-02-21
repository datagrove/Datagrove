import hljs from "highlight.js"
import { Component, JSXElement } from "solid-js"

export const Button: Component<{ class?: string, onClick: () => void, children: JSXElement }> = (props) => {
    return <button class={`${props.class} mr-2 inline-flex items-center rounded border border-transparent bg-sky-800 px-2.5 py-1.5 text-xs font-medium text-white shadow-sm hover:bg-sky-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2`} onClick={props.onClick}>{props.children}</button>
  }
  export const SmallButton: Component<{ onClick: () => void, children: JSXElement }> = (props) => {
    return <a class="mr-2 cursor-pointer inline-flex items-center  text-sky-800 px-2.5 py-1.5  font-medium rounded-md  hover:underline " onClick={props.onClick}>{props.children}</a>
  }
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
        <Button onClick={() => { navigator.clipboard.writeText(props.code) }}>Copy</Button>
        <Button onClick={() => { a.click(); }} >Download</Button>
        <a href={url} download={props.downloadAs ?? "file.txt"} ref={a!}></a>
      </div>
      <div class='w-full mt-4 overflow-hidden ' >
        <pre style='word-break: break-all; white-space: pre-wrap' innerHTML={x}>
  
        </pre>
      </div></div>
  }
  
// @refresh reload
import { Component, createResource, ErrorBoundary, For, Suspense, createSignal, Show, JSXElement, onMount } from "solid-js"
import { render } from 'solid-js/web'
import {
  useLocation,
  A,
  Routes,
  Route,
  Router,
} from "@solidjs/router"
import "./index.css"
import json from 'highlight.js/lib/languages/json';
import hljs from 'highlight.js/lib/core';
hljs.registerLanguage('json', json);

import { CodeMirror } from '../../codemirror/src'
import { Rpc, RpcInterface } from '../../drpc/src/index'
import { Button, SmallButton } from "./components";

interface Configure {
  sku: {
    [key: string]: ConfigureSku
  }
}
interface ConfigureSku {
  name: string
  limit: number
  db: string
}

export type Reservation = { sku: string, id: number, description: string }

export const [config, setConfig] = createSignal<Configure>({sku:{}})
export const [lease, setLease] = createSignal([] as Reservation[])


//  each database is going to be on a channel, so this has to change.
const rpc = new Rpc({
  onmessage: (s: RpcInterface)=>{
    switch(s.method){
    // update is only one sku at a time, maybe it should be a lease at time?
    // not clear then how to send first update. we could just send all the skus and source them.
    case 'update':
      setLease(s.params)
      break
    case 'config':
      setConfig(s.params)
    }
  }
})

// home is dashboard that shows any running tests
const reserve = async (desc: string) => {
  rpc.ask('reserve', desc)
}
const release = async (id: number) => {
  rpc.ask('release', id)
}
const releaseAll = async () => {
  rpc.ask('releaseAll')
}

const Avail : Component<{}> = (props) =>{ 
  // const availCount = () => config() ? config().nworker - lease().length : 0 : {availCount()}
  return <div>Servers Available </div>
}
// dashboard for one database sku
const Dashboard : Component<{}> = (props) => {

  // available is now a list of numbers, one for each database.


  let descInput: HTMLInputElement
  return (
    <main class="mx-auto text-gray-700 p-4">
      <label for="desc" class="block font-medium text-gray-700">Lock a server</label>

      <div class='mt-1' >
        <div class='flex flex-row'>
          <select class='mr-2'>
            <For each={ ['v10','v100']}>{(e,i)=>{
              return <option value={e}>{e}</option>
            }}</For>
          </select>
          <input ref={descInput!} class="mr-2 border-2 p-1 rounded-md border-sky-800 opacity-100'" autofocus name='desc' type='text' placeholder='Reservation notice'></input>
          <Button onClick={() => reserve(descInput!.value)}>Reserve</Button>
        </div>
      </div>
      <Avail/>
      <table class='mt-4'>
        <For each={lease() ?? []} >{(e) => {
          return <tr><td>{e.description} <span class='text-neutral-400'>(#{e.id})</span></td>
            <td><SmallButton onClick={() => release(e.id)}>Release</SmallButton></td>
          </tr>
        }

        }</For></table>
      <div class='my-4'>
        <Button onClick={() => { }}>Refresh</Button>
        <Button onClick={() => releaseAll()}>Release all</Button></div>
    </main>
  );
}

export function Configure() {
  let cm: CodeMirror
  let div: HTMLDivElement
  onMount(() => {
    cm = new CodeMirror(div, JSON.stringify(config(), null, "    "))
  })
  const savefn = () => {
    rpc.ask('configure', cm.text)
  }
  return (
    <main class="mx-auto text-gray-700 p-4" >
      <Button class='mb-2' onClick={savefn}>Save</Button>
      <div ref={div!} />
    </main >
  );
}



const tabs = [
  { href: '/', name: 'Home' },
  { href: '/setup', name: 'Configure' },
]

export function Root() {
  const location = useLocation();
  const active = (path: string) =>
    path == location.pathname
      ? "border-sky-600"
      : "border-transparent hover:border-sky-600";
  return (

    <Suspense>
      <ErrorBoundary fallback={err => err}>
        <nav class="bg-sky-800">
          <ul class="container flex items-center p-3 text-gray-200">
            <For each={tabs}>{(e, i) => {
              return <li class={`border-b-2 ${active(e.href)} mx-1.5 sm:mx-6`}>
                <A href={e.href}>{e.name}</A>
              </li>
            }
            }</For>

          </ul>
        </nav>
        <Routes>
          <Route path="/" component={()=><Dashboard/>} />
          <Route path="/setup" component={Configure} />

        </Routes>
      </ErrorBoundary>
    </Suspense>
  );
}





render(() => <Router><Root /></Router>, document.getElementById("app")!);
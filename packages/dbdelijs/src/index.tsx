// @refresh reload
import { Component, ErrorBoundary, For, Suspense, createSignal, JSXElement, } from "solid-js"
import { render } from 'solid-js/web'
import {
  useLocation,
  A,
  Routes,
  Route,
  Router,
} from "@solidjs/router"
import "./index.css"

import { Rpc, RpcInterface } from '../../drpc/src/index'

export const buttonCss = "mr-2 inline-flex items-center rounded border border-transparent bg-sky-800 px-2.5 py-1.5 text-xs font-medium text-white shadow-sm hover:bg-sky-600 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:bg-neutral-200"
export const Button: Component<{ class?: string, onClick: () => void, children: JSXElement }> = (props) => {
  return <button class={`${props.class} ${buttonCss}`} onClick={props.onClick}>{props.children}</button>
}
export const SmallButton: Component<{ onClick: () => void, children: JSXElement }> = (props) => {
  return <a class="mr-2 cursor-pointer inline-flex items-center  text-sky-800 px-2.5 py-1.5  font-medium rounded-md  hover:underline " onClick={props.onClick}>{props.children}</a>
}
export const DownloadText: Component<{ text: string, children: JSXElement }> = (props) => {
  var blob = new Blob([props.text], { type: 'text/plain' });
  let textFile = window.URL.createObjectURL(blob);
  return <a download >{props.children}</a>
}


type SharedState = {
  //options: {}
  sku: {
    [key: string]: ConfigureSku
  }
  reservation: {
    [key: string]: Reservation
  }
}
type Reservation = {
  sku: string
  ticket: number
  description: string
}
type ConfigureSku = {
  limit: number
  database: string
  databaseType: string
}

export const [shared, setShared] = createSignal<SharedState>({
  sku: {},
  reservation: {}
})


//  each database is going to be on a channel, so this has to change.
const rpc = new Rpc({
  onmessage: (s: RpcInterface) => {
    console.log("onmessage", s)
    switch (s.method) {
      // update is only one sku at a time, maybe it should be a lease at time?
      // not clear then how to send first update. we could just send all the skus and source them.
      case 'update':
        setShared(s.params)
        break
      // case 'config':
      //   setConfig(s.params)
    }
  }
})




const inputCss = "mr-2 border-2 p-1 rounded-md border-sky-800 opacity-100"

const ServerLock: Component<{}> = (props) => {
  let descInput: HTMLInputElement
  let pickSku: HTMLSelectElement
  const canReserve = () => descInput?.value
  const skuList = (): string[] => Object.keys(shared().sku)

  const rsv = () => {
    console.log("sel", pickSku, pickSku?.value, descInput?.value)
    rpc.ask('reserve', {
      sku: pickSku?.value,
      description: descInput?.value
    })
  }
  return <div class='mt-2'>
    <label for="desc" class="block font-medium text-gray-700">Lock a server</label>

    <div class='mt-1' >
      <div class='flex flex-row'>
        <select ref={pickSku!} class='mr-2'>
          <For each={skuList()}>{(e, i) => {
            return <option value={e}>{e}</option>
          }}</For>
        </select>
        <input ref={descInput!} class="mr-2 border-2 p-1 rounded-md border-sky-800 opacity-100'" autofocus name='desc' type='text' placeholder='Reservation notice'></input>
        <button class={buttonCss} onClick={rsv}>Reserve</button>
      </div>
    </div></div>

}
const AddDatabase: Component<{}> = (props) => {
  let sku: HTMLInputElement
  let limit: HTMLInputElement
  let database: HTMLInputElement
  return <div>      <div class='flex flex-row'>
    <label for="sku" class="block font-medium text-gray-700">Name</label>,
    <label for="limit" class="block font-medium text-gray-700">Limit</label>,
    <label for="file" class="block font-medium text-gray-700">Database file</label>
  </div>
    <div class='flex flex-row'>
      <input class={inputCss} name='sku' placeholder='Name' />
      <input class={inputCss + " w-16"} name='limit' type='number' value='100' />
      <input class={inputCss} name='file' type='file' placeholder='database' />
    </div></div>
}
const DatabaseList: Component<{}> = (props) => {
  const serverList = () => Object.entries(shared().sku)
  return <div class='mt-2'>
    <h2 class='mt-2'>Databases  </h2>
    <table>
      <For each={serverList()} >{(e) => {
        return <tr>
          <td>{e[0]}</td>
          <td>{e[1].limit}</td>
          <td>{e[1].database}</td>
        </tr>
      }}</For>
    </table>
  </div>
}
// dashboard for one database sku
const Dashboard: Component<{}> = (props) => {
  const release = async (sku: string, id: number) => {
    rpc.ask('release', {
      sku: sku,
      ticket: id
    })
  }
  const reservations = () => Object.values(shared().reservation)
  return (
    <main class="mx-auto text-gray-700 p-4">

      <DatabaseList />
      <ServerLock />
      <table class='mt-4'>
        <For each={reservations()} >{(e) => {
          return <tr><td>{e.sku}</td><td>{e.description} <span class='text-neutral-400'>(#{e.ticket})</span></td>
            <td><SmallButton onClick={() => release(e.sku, e.ticket)}>Release</SmallButton></td>
          </tr>
        }

        }</For></table>
      <div class='my-4'>
        <Button onClick={() => { }}>Refresh</Button>
      </div>
    </main>
  );
}



const tabs = [
  { href: '/', name: 'DB Deli' },
  //{ href: '/setup', name: 'Configure' },
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
          <Route path="/" component={() => <Dashboard />} />


        </Routes>
      </ErrorBoundary>
    </Suspense>
  );
}

render(() => <Router><Root /></Router>, document.getElementById("app")!);




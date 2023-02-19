import './index.css'
import { For, JSXElement } from 'solid-js'


export function H2(props: { children: JSXElement }) {
    return <h4 class="mx-2 pt-4 pb-2 text-2xl font-bold dark:text-white">{props.children}</h4>
}
export function H3(props: { children: JSXElement }) {
    return <h4 class="pt-4 text-2xl font-bold dark:text-white">{props.children}</h4>
}
export function TagList(props: { each: string[] }) {
    if (!props.each.length) return <div />
    else {
        const [first, ...rest] = props.each
        return <div>
            <For each={rest}>{(e, i) => `, ${e}`}</For>
        </div>
    }
}

function PictureName(props: { path: string }) {
    return <a id={props.path.split('/')[1]} ><h4 class="pt-4 text-2xl font-bold dark:text-white">{props.path.split("/").slice(1).join(": ")}</h4></a>
}
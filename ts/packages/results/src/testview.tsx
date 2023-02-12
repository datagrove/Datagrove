import { Component } from "solid-js";
import 'highlight.js/styles/github.css'
import hljs from 'highlight.js/lib/core';
import gherkin from 'highlight.js/lib/languages/gherkin';
import { UnitTest, TestData, store, UnitTestLong} from "./data";
hljs.registerLanguage('gherkin', gherkin);

import { createSignal, For, JSXElement, onMount, Show , createResource} from 'solid-js'
import { render } from 'solid-js/web'
import { Route, Routes, Router, A, useParams } from "@solidjs/router";
import { Step } from './data'
import { BackNav, Nav } from "./nav";
import { H3 } from "./widget";

// with line fade https://www.felixmokross.dev/blog/code-samples-line-highlighting



export const CodeView: Component<{ code: string }> = (props) => {
    const x = hljs.highlight(props.code, {
        language: 'gherkin'
    }).value;

    return <div class='mt-4 rounded-md bg-neutral-500yes' >
        <pre class='whitespace-pre' innerHTML={x}>
        </pre>
    </div>
}

//     <div class="block mt-4  p-6 bg-white border border-gray-200 rounded-lg shadow-md dark:bg-gray-800 dark:border-gray-700 ">
export const PreCard: Component<{ children: JSXElement }> = (props) => {
    return <div class='block bg-white dark:bg-gray-800 rounded-md shadow-md p-4 mt-4'>
        <pre  class="w-full overflow-y-auto  font-normal text-gray-700 dark:text-gray-400">{props.children}</pre>
    </div>
}
export const Card: Component<{ children: JSXElement }> = (props) => {
    return <div class='block break-all bg-white dark:bg-gray-800 rounded-md shadow-md p-4 mt-4'>
        {props.children}
    </div>
}
export const StackDump: Component<{ test: UnitTestLong }> = (props) => {
    if (!props.test) {
        return <div>No test found</div>
    }
    return <div>
       <Show when={props.test.error}><PreCard>{props.test.error}</PreCard></Show>
         <Show when={props.test.stack}><PreCard>{props.test.stack}</PreCard></Show>
         <Show when ={props.test.output}><PreCard>{props.test.output}</PreCard></Show>
    </div>
}

// in our og, we are defining a step as an image. the name of the step is the last piece of the path.
export const ImageBrowser: Component<{ test: TestData }> = (props) => {
    const [path, setPath] = createSignal(props.test.step[0])
    const scr = () => `TestResults/${path().screen}`

    return <div></div>
    return <div>
        <div class="relative overflow-x-auto shadow-md sm:rounded-lg pt-4 flex flex-row" >
            <div class='flex-none'>
                <a target='blank' href={scr()} ><img class=' mr-2 h-96' src={scr()} alt={path().name} loading="lazy" /></a>
            </div>
            <div class='flex-1'>
                <For each={props.test.step}>{(e, i) => {
                    return <div><a class='text-blue-700 hover:text-blue-600 cursor-pointer' onClick={
                        () => { setPath(e) }
                    }>{e.name}</a></div>
                }}</For></div>

        </div></div>
}

export const StepList: Component<{ test: TestData }> = (props) => {

    return <div>
        <For each={ props.test.gherkin?.step??[]}>{(e,i)=> {
        return <ul class=' block mt-4  p-6 bg-white border border-gray-200 rounded-lg shadow-md dark:bg-gray-800 dark:border-gray-700'>
            <li>{e.text}</li>
            <li>{e.matches}</li>
            <li>{e.stepDef}</li>
            </ul>
        }}</For>
    </div>
}

export const A2 : Component<{children: JSXElement, href: string}> = (props) =>{
    return <a  class='text-blue-700 hover:underline hover:text-blue-600 cursor-pointer' {...props}>{props.children}</a>
}

const url = new URL(location.href);
const hostRoot = `${url.protocol}//${url.hostname}:${url.port}`

export const TestView: Component<{}> = (props) => {
    const fetchUser = async (id: string)  => {
        var path = '/TestResults/images/' + id + '.json'
        console.log("path=", path)
        var s = await (await fetch(path)).json()
        console.log("test=",s)
        return s as UnitTestLong;
    }

    const params = useParams<{ id: string }>()
    const test = () : TestData|undefined => store()?.test.get(params.id)
    const feature = () =>test()?.gherkin?.file
    const test_name = ()=>test()?.test_name ?? ""
    console.log(test())
    const [testDesc] = createResource(params.id, fetchUser);
    return <>
        <BackNav back={true}>{test_name()}</BackNav>
        
        <div class='p-2 px-4'>
        <Show when={test()} >
        <p><A2 href={'/traceViewer/index.html?trace=' + hostRoot + '/TestResults/images/'+test_name()+'.zip'}>View Trace</A2></p><p> <A2 href={'/TestResults/images/'+test_name()+'.zip'}>Download Trace</A2></p>

            <Show when={testDesc()} fallback={"Loading"}>
            <StackDump test={testDesc()!} />
            </Show>
            <StepList  test={test()!} />
            <ImageBrowser test={test()!} />
            <p class='break-words pt-4 text-2xl font-bold dark:text-white'>{feature()?.path??"No path specified"}</p>
            <H3>{feature()?.qualified_name??""}</H3>
            < CodeView code={feature()?.source??""} />
        </Show></div>
    </>
}
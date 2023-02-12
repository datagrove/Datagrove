import './index.css'
import { Component, createSignal, For, onMount } from 'solid-js'
import { render, Show, Switch, Match } from 'solid-js/web'
import { Route, Routes, Router, A, useNavigate } from "@solidjs/router";
import { TestView } from './testview'
import { H2 } from './widget';
import { TestData, UnitTest, store } from './data';
import { BackNav } from './nav';

// how should I gather the feature files?
// change to SSR (astro?)
// there's no need to actually run any of this code on the client
// maybe the go should be called from the nodejs renderer.


// One  set of test results.
const TestTable: Component<{ test: TestData[] }> = (props) => {
    const navigate = useNavigate()
    return <>
        <div class='pl-2 pr-2 mt-2 overflow-hidden'>
            <div class="relative overflow-x-auto shadow-md sm:rounded-lg">
                <table class="w-full text-sm text-left text-gray-500 dark:text-gray-400">
                    <thead class="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
                        <tr>
                            <th scope="col" class="px-6 py-3">
                                Test
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        <For each={props.test}>{(e, i) => {
                            return <tr onClick={() => navigate('/test/' + e.test_name)} class="cursor-pointer bg-white border-b dark:bg-gray-800 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600">
                                <td class="px-6 py-4 font-medium text-gray-900 whitespace-nowrap dark:text-white">
                                    <A href={"/test/" + e.test_name}>{e.test_name}</A>
                                </td>
                            </tr>
                        }}</For>
                    </tbody>
                </table>
            </div>
        </div></>
}
// One  set of test results.
const Errors: Component<{ test: TestData[] }> = (props) => {
    const navigate = useNavigate()
    return <>
        <div class='pl-2 pr-2 mt-2 overflow-hidden'>
            <div class="relative overflow-x-auto shadow-md sm:rounded-lg">
                <table class="w-full text-sm text-left text-gray-500 dark:text-gray-400">
                    <thead class="text-xs text-gray-700 uppercase bg-gray-50 dark:bg-gray-700 dark:text-gray-400">
                        <tr>
                            <th scope="col" class="px-6 py-3">
                                Test
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        <For each={props.test}>{(e, i) => {
                            return <tr onClick={() => navigate('/test/' + e.test_name)} class="cursor-pointer bg-white border-b dark:bg-gray-800 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600">
                                <td class="px-6 py-4 font-medium text-gray-900  dark:text-white">
                                    <A class='text-red-500 whitespace-nowrap' href={"/test/" + e.test_name}>{e.test_name}</A>

                                    
                                </td>
                            </tr>
                        }}</For>
                    </tbody>
                </table>
            </div>
        </div></>
}
const tabs = [
    { name: 'V10', route: '/' },
    { name: 'V100', route: '/v100' },
    { name: 'Load', route: '/load' }
]
// function Performance() {
//     return <><Nav tabs={tabs} selected={2} />
//         <p class='m-4'> coming soon </p>
//     </>

// }

/*
                                    <Switch>
                                        <Match when={e.test.error.indexOf("= logs =")!=-1}>
                                            <pre class=' w-full break-all'>
                                                {e.test.error}
                                            </pre> 
                                        </Match>
                                        <Match when={true}>
                                            <p class='w-screen pr-96 break-all' >
                                                {e.test.error}
                                            </p>
                                        </Match>
                                    </Switch>
*/

// what's the best way to distingish test batches?
// ideally they run at the same time to minimize length.
// but it's not clear we can distinguish them. Can we add a prefix to the test?
// Can we access the TestCategory? Problem with TestCategory is that some tests run as both. We can add this in the AOT compiler though. Practically we probably need these to run in different batches sequentially.
function TestResults() {
    const passed = () => store()?.pass.length ?? 0
    const failed = () => store()?.fail.length ?? 0
    const date = () => store()?.date ?? 0
    return <><BackNav back={false} >
        V10 <a href='#passed'>{passed()} passed</a>, {failed()} failed,  {date()}
    </BackNav>
        <Show when={store()}>
            <H2>Failed</H2>
            <Errors test={store()!.fail} />
            <a id='passed'><H2>Passed</H2></a>
            <TestTable test={store()!.pass} />
        </Show>
    </>
}

function App() {
    return <>
        <Routes>

            <Route path="/test/:id" component={TestView as any} />

            <Route path="/" component={TestResults} />
        </Routes></>
}
render(
    () => (
        <Router>
            <App />
        </Router>
    ),
    document.getElementById("app")!
);

/*
        <For each={dx} >{(e, i) => {
            return <div><PictureName path={e} /><div class="border-solid border-2 border-sky-500 m-4"><img src={`TestResults/${e}`} alt="..." loading="lazy" /></div></div>
        }}</For>*/

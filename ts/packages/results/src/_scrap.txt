
import './index.css'
import { For, JSXElement } from 'solid-js'
import { H3, TagList } from './widget'
import feature from './feature.json'
import { Feature } from './data'

// this probably goes away
const features = new Map<string, Feature>()

export function Features(props: {}) {
    return <div>
        <For each={feature}>{(e, i) => <div>
            <H3>{e.name}</H3>
            <p>{e.description}</p>
            <div >
                <For each={e.test}>{(ex, i) => <div class='text-neutral-200 py-2' >
                    <table>
                        <tbody>
                            <tr>
                                <td class='text-green-400 pr-2'>Name</td><td>
                                    <a href={'#' + ex.name} class='text-blue-500 hover:text-blue-700'>{ex.name}</a></td></tr>

                            <tr>
                                <td class='text-green-400 pr-2'>Scenario</td><td>{ex.scenario}</td></tr>
                            <tr> <td class='text-green-400 pr-2'>Given</td><td>{ex.given}</td></tr>
                            <tr><td class='text-green-400 pr-2'>When</td><td>{ex.when}</td></tr>
                            <tr><td class='text-green-400 pr-2'>Then</td><td>{ex.then}</td></tr>
                        </tbody></table>


                    <TagList each={ex.tag} />

                </div>}</For>
            </div>
        </div>
        }</For>
    </div>
}
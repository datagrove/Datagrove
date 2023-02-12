import { createSignal } from 'solid-js'


export interface UnitTest {
    test_name: string
    outcome: string

}
export interface UnitTestLong {
    output: string
    error: string
    stack: string
}


// not available yet, from AOT compiler?
// unit tests are single scenarios inside the feature. 
// should we try to highlight the step? we can't extract it because we need the background.
export interface TestData {
    test_name: string
    test: UnitTest
    output: string
    scenario?: string
    given?: string
    when?: string
    then?: string
    step: Step[]
    gherkin?: GherkinScenario
}
export interface Store {
    name: string
    date: string
    test: Map<string, TestData>
    pass: TestData[]
    fail: TestData[]
}
export interface Step {
    name: string
    screen: string
}
export interface Feature {
    name: string
    description: string
    test: TestData[]
}

export interface GherkinFile {
    path: string;
    source: string,
    qualified_name: string,
    // this should be markdown
    scenario: GherkinScenario[]
}
export interface GherkinStep {
    text: string
    matches: string
    stepDef: string
    screen: string
}
export interface GherkinScenario {
    name: string
    step: GherkinStep[]
    file?: GherkinFile
}
export interface Gherkin {
    feature: GherkinFile[]
}

interface TestResults {
    file: string[]
    test: UnitTest[]
    date: string
    name: string
}

async function buildTests(): Promise<Store> {

    var tj : TestResults = await (await fetch('/test_results.json')).json() 
    const ghx : Gherkin = await (await fetch(`/TestResults/v10.json`)).json()

    // we should maybe do this in the go program? make a map of tests.
    async function testLog(): Promise<Map<string, TestData>> {
        const tests = new Map<string, TestData>()
        const trx: UnitTest[] = tj.test;
        for (let e of trx) {
            tests.set(e.test_name, {
                test_name: e.test_name,
                test: e,
                output: '',
                step: []
            })
        }
        return tests
    }

    // json created by the go app.
    // dx is a list of files, but we need the output in trx?
    const tests = await testLog();
    const shot = tj.file.filter((e) => e.split('.').pop() == "jpg")

    const mscenario = new Map<string,GherkinScenario>();

    // this is crap, trx doesn't have qualified names, so we need to unqualify them. will fix with custom logger.
    
    for (let o of ghx.feature) {
        for (let sc of o.scenario) {
            sc.file = o
            const nm = sc.name.split('.')
            const n = nm.pop();
            if (n) {
                // each scenario has a name, but each test has __NN example id
                mscenario.set(n, sc)
            }
        }
    }
    for (let [k, v] of tests) {
        const nm = k.split("__")[0]
        v.gherkin = mscenario.get(nm)
    }
    console.log("tests", tests)
    console.log("scenario", mscenario)
    shot.forEach((e: string) => {
        const [_, test, step] = e.split('/')
        const trx = tests.get(test)
        if (trx) {
            trx.step.push({
                name: step,
                screen: e
            })
        }
    })

    const v = [...tests.values()]
    return {
        name: tj.name,
        test: tests,
        date: tj.date,
        pass: v.filter(e => e.test.outcome=="Passed").sort((a,b)=>a.test_name.localeCompare(b.test_name)),
        fail: v.filter(e => e.test.outcome=="Failed").sort((a,b)=>a.test_name.localeCompare(b.test_name)),
    };
}

export const [store, setStore] = createSignal<Store>();

(async () => {
    const ox = await buildTests()
    setStore(ox);
})()




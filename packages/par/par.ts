

// execute 0..n async,  return a promise to all of them
export async function parfor_async<T>(n: number, fn: (i: number) => Promise<T>) {
    let r: Promise<T>[] = []
    for (let i = 0; i < n; i++)
        r.push(fn(i))
    return Promise.all(r)
}

export const nstring = (n: number, fn: (i: number) => string) =>
    [...Array(n)].map((_, i) => fn(i)).join("\n")

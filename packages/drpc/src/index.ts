
// json rpc. method can get quite log because of package paths.
// probably use a way prepare numeric alternative
export interface RpcInterface {
    method: string | number | undefined;
    id: number
    params?: any;
    result?: any;
    error?: any;
}

export type RpcCallback = (o: RpcInterface) => void;

export class Rpc {
    pending = new Map<number, [any, any]>();
    nextId = 52;
    error: string | undefined = undefined
    s: WebSocket | undefined
    toSend: string[] = []
    api: string
    onmessage: RpcCallback

    constructor(props: { api?: string, onmessage: RpcCallback }) {
        this.api = props?.api ?? "ws://"+ location.hostname + ":" + location.port + "/ws"
        this.onmessage = props.onmessage
        this.connect();
    }

    close() {
        this.failAll("closing")
        this.s?.close()
    }

    failAll(error: any) {
        for (const [k, v] of this.pending) {
            v[1](error)
        }
    }
    async askBin(m: string, params: any, b: ArrayBuffer[]): Promise<any> {
        const id = this.nextId++;
        const s : RpcInterface = {
            method: m,
            id: id,
            params: params
        }
        console.log("ask", s);
        this.sendBin(s, b);
        return new Promise((resolve, reject) => {
            // stash the resolve and reject? or can we store the entire promise?
            this.pending.set(id, [resolve, reject]);
        });
    }
    async ask( m: string, params?: any): Promise<any> {
        const id = this.nextId++;
        const s = {

            method: m,
            id: id,
            params: params
        }
        console.log("ask", s);
        this.send(s);
        return new Promise((resolve, reject) => {
            // stash the resolve and reject? or can we store the entire promise?
            this.pending.set(id, [resolve, reject]);
        });
    }
    notify(channel: number, method: string, params: any | undefined = undefined) {
        const r = {
            channel: channel,
            method: method,
            params: params
        };
        console.log("notify", r);
        this.send(r);
    }

    sendBin(x: any, b: ArrayBuffer[]) {
        if (!this.s) throw "websocket failed";
        const packet = JSON.stringify(x);
        this.s.send(packet);
        for (const e of b)
            this.s.send(e);
    }
    send(x: any) {
        if (!this.s) throw "websocket failed";
        const packet = JSON.stringify(x);
        if (this.s.readyState != 1)
            this.toSend.push(packet);
        else this.s.send(packet);
    }
    connect() {
        this.s = new WebSocket(this.api);
        this.s.onerror = (ev: any) => {
            this.failAll(" is not available");
        }
        this.s.onclose = (ev: any) => {
            this.failAll("retry");
        }
        this.s.onopen = (ev: any) => {
            for (const x of this.toSend)
                this.s?.send(x)
            this.toSend = [];
        }
        this.s.onmessage = (ev: any) => {
            let d: RpcInterface
            try {
                d = JSON.parse(ev.data);
            } catch (e) {
                console.log("json error", ev.data, ev);
                return
            }

            if (d.method) {
                this.onmessage(d)
            }
            else if (d.id) {
                const r = this.pending.get(d.id);
                if (r) {
                    this.pending.delete(d.id);
                    // this should probably throw with an error code?
                    // what about the go approach?
                    if (d.error) {
                        this.error = d.error
                        r[0](undefined)
                    } else {
                        r[0](d.result);
                    }
                }
                else {
                    console.log("no completion", r, d.id, this.pending);
                }
            }
            else {
                console.log("BAD RPC ", d);
            }
        }
    }

}



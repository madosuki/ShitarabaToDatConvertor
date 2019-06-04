open System
open FSharp.Control.Tasks.V2
open libShitarabaToDat.shitarabaToDatTools

[<EntryPoint>]
let main argv =

    let url = "https://jbbs.shitaraba.net/bbs/read_archive.cgi/otaku/14796/1504176296/"
    let c = shitarabaToDatClass(url)
    let r = c.htmlToDat.GetAwaiter().GetResult()
    c.WriteToFile("sjis")
    
    (*
    for i in r do
        printfn "%A" i
    *)

    0 // return an integer exit code

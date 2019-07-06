open System
open FSharp.Control.Tasks.V2
open CommandLine
open libShitarabaToDat.shitarabaToDatTools

type options = {
    [<Option("url", Required = true, HelpText = "Target URL")>] url : string
    [<Option("encoding", HelpText = "DAT File Encoding (e.g. sjis). Default value is utf-8. Used by System.Text.Encoding.GetEncoding()")>] encoding : string
}

let run (url: string) (encode: 'T) =
    let c = shitarabaToDatClass(url)
    let r = c.htmlToDat.GetAwaiter().GetResult()

    if encode = null then
        c.WriteToFile("utf-8")
    else if encode.GetType() = typeof<string> then
        c.WriteToFile(encode)
    
[<EntryPoint>]
let main argv =
    let result = CommandLine.Parser.Default.ParseArguments<options>(argv)
    match result with
    | :? Parsed<options> as parsed -> run parsed.Value.url parsed.Value.encoding
    | :? NotParsed<options> as notParsed -> printfn "%A" notParsed.Errors

    // let url = "https://jbbs.shitaraba.net/bbs/read_archive.cgi/otaku/14796/1504176296/"
    // let c = shitarabaToDatClass(url)
    // let r = c.htmlToDat.GetAwaiter().GetResult()
    // c.WriteToFile("sjis")
    
    (*
    for i in r do
        printfn "%A" i
    *)

    0 // return an integer exit code
